using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBehaviour : MonoBehaviour
{
	#region Variables

	public static CreatureBehaviour Instance { get; private set; }

	[SerializeField] private Transform m_playerTransform;

	[Header("Graph")]
	[SerializeField] private string m_graphTextFileName;
	[SerializeField] private int m_startingNodeIndex;

	public WeightedNode[] Nodes { get; private set; }
	private KDTree m_tree;

	public List<WeightedNode> CurrentPath { get; private set; } = new();
	public WeightedNode CurrentNode;
	public WeightedNode TargetNode;
	private Vector3 TargetNodeNormalizedDirection;

	private WeightedNode m_beaconNode;
	private Vector3 m_beaconPosition;

	private bool m_isOnGraph = true;
	private bool m_returnToGraph;
	private bool m_isBeaconActive;

	private float m_totalNodeWeight;

	[Header("Behaviour Tuning")]
	[SerializeField] private float m_nodeWeightNeutralizingSpeed;

	[SerializeField] private float m_detectionThreshold;
	[SerializeField] private float m_newWanderThreshold;

	[SerializeField] private float m_attackDistance;
	private float m_attackDistanceSquared;

	[SerializeField] private float m_beaconAttackDuration;
	private bool m_isAttackingBeacon;

	[SerializeField] private float m_travelSpeed;
	private float TargetNodeDistance = 10000;
	private float m_travelDistance = 0;

	public Action AttackedPlayer = delegate { };

	public float TotalVolumeFactor { get; private set; } = 1;


	// Audio
	private float m_minGrowlDelay;
	private float m_maxGrowlDelay;

	private Coroutine m_roaringCoroutine;
	private Coroutine m_growlingCoroutine;

	private bool m_gameOver;

	#endregion

	#region Unity

	private void Awake()
	{
		if (Instance != null)
			Destroy(this);
		else
			Instance = this;
	}

	private void Start()
	{
		GraphModel graphModel = GraphData.LoadGraph(Resources.Load<TextAsset>(m_graphTextFileName));

		Nodes = new WeightedNode[graphModel.Nodes.Length];

		for (int i = 0; i < graphModel.Nodes.Length; i++)
		{
			Nodes[i] = new(graphModel.Nodes[i], m_nodeWeightNeutralizingSpeed);
		}

		m_tree = new(Nodes);

		m_totalNodeWeight = Nodes.Length;

		CurrentNode = Nodes[m_startingNodeIndex];
		transform.position = CurrentNode.Node.Position;

		m_attackDistanceSquared = m_attackDistance * m_attackDistance;

		UpdateGrowlBehaviour();
		m_growlingCoroutine = StartCoroutine(PlayGrowlingSounds());
	}

	private void Update()
	{
		if (m_gameOver)
			return;

		// Lerp node weights back to neutral
		float time = Time.time;
		float deltaTime = Time.deltaTime;

		foreach (WeightedNode node in Nodes)
			m_totalNodeWeight += node.NeutralizeWeight(time, deltaTime);

		UpdateGrowlBehaviour();

		// If currently attacking the beacon then do nothing else
		if (m_isAttackingBeacon)
			return;

		// Only move if we're not attacking something
		if (!CheckForAttacks())
		{
			if (m_isOnGraph)
			{
				// If we have a target then move towards it
				if (TargetNode != null)
				{
					UpdatePosition();

					if (m_travelDistance >= TargetNodeDistance)
					{
						// If we've reached the target then set the current node and reset progress
						m_travelDistance = 0;

						// Reduce target node weight if neutral
						if (Mathf.Abs(1 - TargetNode.Weight) < 0.01f)
							ChangeNodeWeight(TargetNode, -0.5f);

						CurrentNode = TargetNode;
						transform.position = CurrentNode.Node.Position;
						TargetNode = null;

						// If we've reached the node closest to the beacon or player then go off-graph and head directly to them
						if (CurrentNode == m_beaconNode || CurrentNode == m_tree.GetNearestNode(m_playerTransform.position))
						{
							m_isOnGraph = false;
							m_roaringCoroutine ??= StartCoroutine(PlayRoaringSounds());
							return;
						}
					}
					else
						return;
				}
				if (CurrentPath.Count > 0)
					IterateTarget();
				else
					StartNewWander();
			}
			else
			{
				Vector3 targetPosition;

				if (m_returnToGraph)
					targetPosition = CurrentNode.Node.Position; // Move back towards where we left the node-based movement
				else if (m_isBeaconActive)
					targetPosition = m_beaconPosition; // Move towards beacon
				else
					targetPosition = m_playerTransform.position; // Move towards player

				Vector3 targetDirection = targetPosition - transform.position;

				if (m_returnToGraph && targetDirection.sqrMagnitude < 0.1f)
				{
					// If moving back to graph and we are close enough to the last node then snap back to graph movement

					m_returnToGraph = false;
					m_isOnGraph = true;

					transform.position = targetPosition;

					StartNewWander();
				}
				else
				{
					transform.position += m_travelSpeed * Time.deltaTime * targetDirection.normalized;
				}
			}
		}
	}

	#endregion

	#region Nodes/Graph

	private void ChangeNodeWeight(WeightedNode node, float weightChange)
	{
		node.Weight += weightChange;
		m_totalNodeWeight += weightChange;
	}

	private void SetNodeWeight(WeightedNode node, float newWeight)
	{
		float oldWeight = node.Weight;
		node.Weight = newWeight;
		m_totalNodeWeight += newWeight - oldWeight;
	}

	// Shortest path between source and destination excluding source node in final path
	private List<WeightedNode> GetShortestPath(WeightedNode source, WeightedNode destination)
	{
		WeightedNode[] nodes = Nodes;

		if (source == destination)
			return new();

		int sourceIndex = Array.IndexOf(nodes, source);
		int destinationIndex = Array.IndexOf(nodes, destination);

		if (sourceIndex < 0 || destinationIndex < 0)
			return new();

		List<WeightedNode> nodeQueue = new();

		int nodeCount = nodes.Length;
		bool[] isNodeVisited = new bool[nodeCount];
		int[] distance = new int[nodeCount];
		int[] predecessor = new int[nodeCount];

		for (int i = 0; i < nodeCount; i++)
		{
			isNodeVisited[i] = false;
			distance[i] = int.MaxValue;
			predecessor[i] = -1;
		}

		isNodeVisited[sourceIndex] = true;
		distance[sourceIndex] = 0;
		nodeQueue.Add(source);

		while (nodeQueue.Count != 0)
		{
			WeightedNode node = nodeQueue[0];
			int nodeIndex = node.Node.Index;

			nodeQueue.RemoveAt(0);

			List<int> connectionIndexes = node.Node.ConnectionIndexes;

			if (connectionIndexes != null)
			{
				for (int i = 0; i < connectionIndexes.Count; i++)
				{
					int adjacentIndex = connectionIndexes[i];
					WeightedNode adjacentNode = Nodes[adjacentIndex];

					if (isNodeVisited[adjacentIndex] == false)
					{
						isNodeVisited[adjacentIndex] = true;
						distance[adjacentIndex] = distance[nodeIndex] + 1;
						predecessor[adjacentIndex] = nodeIndex;
						nodeQueue.Add(adjacentNode);

						if (adjacentIndex == destinationIndex)
						{
							List<WeightedNode> shortestPath = new();

							int crawlIndex = destinationIndex;

							while (predecessor[crawlIndex] != -1)
							{
								shortestPath.Insert(0, nodes[crawlIndex]);
								crawlIndex = predecessor[crawlIndex];
							}

							return shortestPath;
						}
					}
				}
			}
		}

		return new();
	}

	#endregion

	#region Moving

	// Update the travel distance between nodes and the physical position of the creature
	private void UpdatePosition()
	{
		Vector3 currentNodeToTargetNode = TargetNode.Node.Position - CurrentNode.Node.Position;
		float moveDistance = m_travelSpeed * Time.deltaTime;

		m_travelDistance += moveDistance;
		transform.position += moveDistance * currentNodeToTargetNode.normalized;
	}

	// Choose a random node biased by weights and set a path towards it
	private void StartNewWander()
	{
		float randomNodeWeightPoint = UnityEngine.Random.Range(0, m_totalNodeWeight);

		float cumulativeNodeWeight = 0;

		for (int i = 0; i < Nodes.Length; i++)
		{
			cumulativeNodeWeight += Nodes[i].Weight;

			if (cumulativeNodeWeight >= randomNodeWeightPoint)
			{
				SetPath(Nodes[i]);
				break;
			}
		}
	}

	// Creates and sets the current path to move to the given node
	private void SetPath(WeightedNode destinationNode)
	{
		// Already at the destination
		if (destinationNode == CurrentNode)
		{
			// If we're moving away from the destination then invert everything to turn back towards it
			if (m_travelDistance > 0)
			{
				CurrentNode = TargetNode;
				TargetNode = destinationNode;

				m_travelDistance = TargetNodeDistance - m_travelDistance;
				TargetNodeNormalizedDirection = -TargetNodeNormalizedDirection;
			}
			else
			{
				TargetNode = null;

				// Covers edge-case where the beacon is dropped near the current node exactly at the same time the creature arrives at that node
				if (m_beaconNode == CurrentNode)
					m_isOnGraph = false;
			}

			CurrentPath.Clear();
		}
		// Already heading towards the destination
		else if (destinationNode == TargetNode)
		{
			CurrentPath.Clear();
		}
		else
		{
			if (CurrentPath.Count > 0)
			{
				int index = CurrentPath.IndexOf(destinationNode);

				// Destination is within our current path
				if (index != -1)
				{
					int removeCount = CurrentPath.Count - index - 1;

					// Remove any nodes in the current path after the destination node
					if (removeCount > 0)
						CurrentPath.RemoveRange(index + 1, removeCount);

					return;
				}
			}

			// Make a new path to the destination node.
			if (TargetNode == null)
			{
				CurrentPath = GetShortestPath(CurrentNode, destinationNode);
				IterateTarget();
			}
			else
			{
				CurrentPath = GetShortestPath(TargetNode, destinationNode);
			}
		}
	}

	// Iterates the target node to the next node in the path
	private void IterateTarget()
	{
		TargetNode = CurrentPath[0];

		CurrentPath.RemoveAt(0);

		Vector3 targetNodeDirection = TargetNode.Node.Position - CurrentNode.Node.Position;
		TargetNodeNormalizedDirection = targetNodeDirection.normalized;
		TargetNodeDistance = Vector3.Magnitude(targetNodeDirection);
	}

	#endregion

	#region Attacking

	// Returns true if creature is close enough to attack something, otherwise false
	private bool CheckForAttacks()
	{
		if (m_isBeaconActive && Vector3.SqrMagnitude(m_beaconPosition - transform.position) <= m_attackDistanceSquared)
		{
			AttackBeacon();
			return true;
		}
		else if (Vector3.SqrMagnitude(m_playerTransform.position - transform.position) <= m_attackDistanceSquared)
		{
			AttackedPlayer.Invoke();
			m_gameOver = true;
			return true;
		}

		return false;
	}

	// Destroys the beacon and heads back to the graph if needed
	private void AttackBeacon()
	{
		Destroy(Beacon.Instance.gameObject);

		m_isBeaconActive = false;

		m_growlingCoroutine = StartCoroutine(PlayGrowlingSounds());

		if (!m_isOnGraph)
			m_returnToGraph = true;

		// Reset the beacon node weight as it could be very high
		SetNodeWeight(m_beaconNode, 1);

		StartCoroutine(AttackingBeacon());
	}

	private IEnumerator AttackingBeacon()
	{
		m_isAttackingBeacon = true;

		yield return new WaitForSeconds(m_beaconAttackDuration);

		m_isAttackingBeacon = false;
	}

	#endregion

	#region Sound Logging

	// Adds weight to the closest node and it's connections
	public void AddSound(Vector3 location, float volume)
	{
		WeightedNode closestNode = m_tree.GetNearestNode(location);

		ChangeNodeWeight(closestNode, volume);

		foreach (int connectionIndex in closestNode.Node.ConnectionIndexes)
		{
			WeightedNode connection = Nodes[connectionIndex];
			ChangeNodeWeight(connection, volume * 0.25f);

			foreach (int secondConnectionIndex in connection.Node.ConnectionIndexes)
				ChangeNodeWeight(Nodes[secondConnectionIndex], volume * 0.05f);
		}

		UpdateGrowlBehaviour();

		if (!m_isBeaconActive)
		{
			// If above detection threshold then head directly to sound location
			if (m_totalNodeWeight > m_detectionThreshold)
			{
				SetPath(closestNode);
			}
			// If above new wander threshold then start a new wander (to prevent new sounds having no effect when creature is already on a long wander)
			else if (m_totalNodeWeight > m_newWanderThreshold)
			{
				StartNewWander();
			}
		}
	}

	// Send the creature to the beacon location
	public void BeaconDropped()
	{
		m_roaringCoroutine ??= StartCoroutine(PlayRoaringSounds());

		m_isBeaconActive = true;

		m_beaconPosition = Beacon.Instance.transform.position;
		m_beaconNode = m_tree.GetNearestNode(m_beaconPosition);

		SetPath(m_beaconNode);
	}

	#endregion

	#region Audio

	// Loop roaring sounds with a small delay until roaring is stopped
	private IEnumerator PlayRoaringSounds()
	{
		m_growlingCoroutine = null;

		while (m_growlingCoroutine is null)
			yield return new WaitForSeconds(AudioManager.Instance.PlayCreatureRoar() + UnityEngine.Random.Range(0.3f, 1f));

		m_roaringCoroutine = null;
	}

	// Loop growling sounds with a large delay until roaring is started
	private IEnumerator PlayGrowlingSounds()
	{
		m_roaringCoroutine = null;

		while (m_roaringCoroutine is null)
			yield return new WaitForSeconds(AudioManager.Instance.PlayCreatureGrowl() + UnityEngine.Random.Range(m_minGrowlDelay, m_maxGrowlDelay));

		m_growlingCoroutine = null;
	}

	// Updates the growl volume and delays when the total node weight has changed
	private void UpdateGrowlBehaviour()
	{
		TotalVolumeFactor = Nodes.Length / m_totalNodeWeight;

		AudioManager.Instance.SetCreatureGrowlVolume(TotalVolumeFactor);

		m_minGrowlDelay = 5 + (10 * TotalVolumeFactor);
		m_maxGrowlDelay = m_minGrowlDelay * 1.5f;
	}

	#endregion

	#region Nested Classes

	public class WeightedNode
	{
		#region Construction

		public WeightedNode(Node node, float nodeWeightNeutralizingSpeed)
		{
			Node = node;
			Weight = 1;

			m_weightNeutralizingSpeed = nodeWeightNeutralizingSpeed;
		}

		#endregion

		#region Variables

		public Node Node;

		public float Weight
		{
			get => m_weight;
			set
			{
				m_changeTime = Time.time;
				m_startingDifference = value - 1;

				m_weight = value;
			}
		}

		private float m_weight;

		private float m_changeTime;
		private float m_startingDifference;

		private readonly float m_weightNeutralizingSpeed;

		#endregion

		#region Implementation

		// Uses cubic ease out curve to reduce the node weight back to 1
		public float NeutralizeWeight(float time, float deltaTime)
		{
			if (Weight == 1)
				return 0;

			float currentDifference = Weight - 1;
			float absCurrentDifference = Mathf.Abs(currentDifference);

			if (absCurrentDifference < 0.01f)
			{
				m_weight -= currentDifference;
				return -currentDifference;
			}

			float timeSinceChange = time - m_changeTime;

			float weightChange = m_startingDifference * m_weightNeutralizingSpeed * Mathf.Pow(timeSinceChange, 3) * deltaTime;
			weightChange = Mathf.Clamp(weightChange, -absCurrentDifference, absCurrentDifference);

			m_weight -= weightChange;
			return -weightChange;
		}

		#endregion
	}

	private class KDTree
	{
		#region Construction

		public KDTree(WeightedNode[] nodes)
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				m_rootNode = InsertNode(m_rootNode, new TreeNode(nodes[i]));
			}
		}

		#endregion

		#region Variables

		private const int s_k = 3;

		private readonly TreeNode m_rootNode;

		#endregion

		#region Implementation

		// Adds the given node into the tree
		private TreeNode InsertNode(TreeNode rootNode, TreeNode insertingNode)
		{
			return InsertRecursive(rootNode, insertingNode, 0);
		}

		// Traverses down the tree to insert the node in the correct place
		private TreeNode InsertRecursive(TreeNode rootNode, TreeNode insertingNode, int depth)
		{
			if (rootNode is null)
				return insertingNode;

			int dimension = depth % s_k;

			if (insertingNode.Node.Node.Position[dimension] < rootNode.Node.Node.Position[dimension])
				rootNode.Left = InsertRecursive(rootNode.Left, insertingNode, depth + 1);
			else
				rootNode.Right = InsertRecursive(rootNode.Right, insertingNode, depth + 1);

			return rootNode;
		}

		// Finds the nearest node to the given position.
		public WeightedNode GetNearestNode(Vector3 position)
		{
			return SearchRecursive(m_rootNode, position, 0, null, float.MaxValue).Node.Node;
		}

		// Recursively moves down the tree by comparing to the given position until it reaches a leaf node, backtracks to find the closest node
		private (TreeNode Node, float Distance) SearchRecursive(TreeNode currentNode, Vector3 position, int depth, TreeNode bestNode, float bestDistance)
		{
			// Reached a leaf node so start backtracking.
			if (currentNode is null)
				return (bestNode, bestDistance);

			float distance = Vector3.SqrMagnitude(position - currentNode.Node.Node.Position);

			// If the current node is closer than our best then change the best to current.
			if (distance < bestDistance)
			{
				bestNode = currentNode;
				bestDistance = distance;
			}

			int axis = depth % s_k;

			// Decides which direction to go by comparing the position in the determined axis
			bool goLeft = position[axis] < currentNode.Node.Node.Position[axis];

			(bestNode, bestDistance) = SearchRecursive(goLeft ? currentNode.Left : currentNode.Right, position, depth + 1, bestNode, bestDistance);

			float dimensionDistance = Mathf.Pow(position[axis] - currentNode.Node.Node.Position[axis], 2);

			// If the distance in the single axis is closer than the current best distance then we should explore the other direction
			if (dimensionDistance < bestDistance)
				(bestNode, bestDistance) = SearchRecursive(goLeft ? currentNode.Right : currentNode.Left, position, depth + 1, bestNode, bestDistance);

			return (bestNode, bestDistance);
		}

		#endregion

		#region Nested Classes

		private class TreeNode
		{
			public readonly WeightedNode Node;
			public TreeNode Left = null;
			public TreeNode Right = null;

			public TreeNode(WeightedNode node)
			{
				Node = node;
			}
		}

		#endregion
	}

	#endregion
}