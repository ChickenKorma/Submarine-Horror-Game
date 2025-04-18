using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBehaviour : MonoBehaviour
{
	#region Variables

	public static CreatureBehaviour Instance { get; private set; }

	[SerializeField] private string m_graphTextFileName;
	[SerializeField] private int m_startingNodeIndex;

	private WeightedNode[] m_nodes;
	private KDTree m_tree;

	private List<WeightedNode> m_currentPath = new();
	private WeightedNode m_currentNode;
	private WeightedNode m_targetNode;
	private Vector3 m_targetNodeNormalizedDirection;

	private bool m_isOnGraph = true;
	private bool m_returnToGraph;
	private bool m_isBeaconActive;

	private float m_totalNodeWeight;

	[SerializeField] private float m_detectionThreshold;
	[SerializeField] private float m_newWanderThreshold;

	public float TotalVolumeFactor { get; private set; } = 1;

	private float m_minGrowlDelay;
	private float m_maxGrowlDelay;

	[SerializeField] private float m_travelSpeed;
	private float m_targetNodeDistance = 10000;
	private float m_travelDistance = 0;

	[SerializeField] private Transform m_playerTransform;

	private WeightedNode m_beaconNode;
	private Vector3 m_beaconPosition;

	[SerializeField] private float m_attackDistance;
	private float m_attackDistanceSquared;

	[SerializeField] private float m_beaconAttackDuration;
	private bool m_isAttackingBeacon;

	public Action AttackedPlayer = delegate { };

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

		m_nodes = new WeightedNode[graphModel.Nodes.Length];

		for (int i = 0; i < graphModel.Nodes.Length; i++)
		{
			m_nodes[i] = new(graphModel.Nodes[i]);
		}

		m_tree = new(m_nodes);

		m_totalNodeWeight = m_nodes.Length;

		m_currentNode = m_nodes[m_startingNodeIndex];
		transform.position = m_currentNode.Node.Position;

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

		foreach (WeightedNode node in m_nodes)
			ChangeNodeWeight(node, node.GetWeightChangeToNeutralize(time, deltaTime));

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
				if (m_targetNode != null)
				{
					UpdatePosition();

					if (m_travelDistance >= m_targetNodeDistance)
					{
						// If we've reached the target then set the current node and reset progress
						m_travelDistance = 0;

						// Reduce target node weight if neutral
						if (Mathf.Abs(1 - m_targetNode.Weight) < 0.01f)
							ChangeNodeWeight(m_targetNode, -0.5f);

						m_currentNode = m_targetNode;
						transform.position = m_currentNode.Node.Position;
						m_targetNode = null;

						// If we've reached the node closest to the beacon or player then go off-graph and head directly to them
						if (m_currentNode == m_beaconNode || m_currentNode == m_tree.GetNearestNode(m_playerTransform.position))
						{
							m_isOnGraph = false;
							m_roaringCoroutine ??= StartCoroutine(PlayRoaringSounds());
							return;
						}
					}
					else
						return;
				}
				if (m_currentPath.Count > 0)
					IterateTarget();
				else
					StartNewWander();
			}
			else
			{
				Vector3 targetPosition;

				if (m_returnToGraph)
					targetPosition = m_currentNode.Node.Position; // Move back towards where we left the node-based movement
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

	#region Graph

	// Shortest path between source and destination excluding source node in final path
	private List<WeightedNode> GetShortestPath(WeightedNode source, WeightedNode destination)
	{
		WeightedNode[] nodes = m_nodes;

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
					WeightedNode adjacentNode = m_nodes[adjacentIndex];

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
		Vector3 currentNodeToTargetNode = m_targetNode.Node.Position - m_currentNode.Node.Position;
		float moveDistance = m_travelSpeed * Time.deltaTime;

		m_travelDistance += moveDistance;
		transform.position += moveDistance * currentNodeToTargetNode.normalized;
	}

	// Choose a random node biased by weights and set a path towards it
	private void StartNewWander()
	{
		float randomNodeWeightPoint = UnityEngine.Random.Range(0, m_totalNodeWeight);

		float cumulativeNodeWeight = 0;

		for (int i = 0; i < m_nodes.Length; i++)
		{
			cumulativeNodeWeight += m_nodes[i].Weight;

			if (cumulativeNodeWeight >= randomNodeWeightPoint)
			{
				SetPath(m_nodes[i]);
				break;
			}
		}
	}

	// Creates and sets the current path to move to the given node
	private void SetPath(WeightedNode destinationNode)
	{
		// Already at the destination
		if (destinationNode == m_currentNode)
		{
			// If we're moving away from the destination then invert everything to turn back towards it
			if (m_travelDistance > 0)
			{
				m_currentNode = m_targetNode;
				m_targetNode = destinationNode;

				m_travelDistance = m_targetNodeDistance - m_travelDistance;
				m_targetNodeNormalizedDirection = -m_targetNodeNormalizedDirection;
			}
			else
			{
				m_targetNode = null;

				// Covers edge-case where the beacon is dropped near the current node exactly at the same time the creature arrives at that node
				if (m_beaconNode == m_currentNode)
					m_isOnGraph = false;
			}

			m_currentPath.Clear();
		}
		// Already heading towards the destination
		else if (destinationNode == m_targetNode)
		{
			m_currentPath.Clear();
		}
		else
		{
			if (m_currentPath.Count > 0)
			{
				int index = m_currentPath.IndexOf(destinationNode);

				// Destination is within our current path
				if (index != -1)
				{
					int removeCount = m_currentPath.Count - index - 1;

					// Remove any nodes in the current path after the destination node
					if (removeCount > 0)
						m_currentPath.RemoveRange(index + 1, removeCount);

					return;
				}
			}

			// Make a new path to the destination node.
			if (m_targetNode == null)
			{
				m_currentPath = GetShortestPath(m_currentNode, destinationNode);
				IterateTarget();
			}
			else
			{
				m_currentPath = GetShortestPath(m_targetNode, destinationNode);
			}
		}
	}

	// Iterates the target node to the next node in the path
	private void IterateTarget()
	{
		m_targetNode = m_currentPath[0];

		m_currentPath.RemoveAt(0);

		Vector3 targetNodeDirection = m_targetNode.Node.Position - m_currentNode.Node.Position;
		m_targetNodeNormalizedDirection = targetNodeDirection.normalized;
		m_targetNodeDistance = Vector3.Magnitude(targetNodeDirection);

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

	// Adds weight to the closest node
	public void AddSound(Vector3 location, float volume)
	{
		WeightedNode closestNode = m_tree.GetNearestNode(location);

		ChangeNodeWeight(closestNode, volume);

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
		TotalVolumeFactor = m_nodes.Length / m_totalNodeWeight;

		AudioManager.Instance.SetCreatureGrowlVolume(TotalVolumeFactor);

		m_minGrowlDelay = 5 + (10 * TotalVolumeFactor);
		m_maxGrowlDelay = m_minGrowlDelay * 1.5f;
	}

	#endregion

	#region Nested Classes

	private class WeightedNode
	{
		#region Construction

		public WeightedNode(Node node)
		{
			Node = node;
			Weight = 1;
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

		private const float s_changeFactor = 1.9E-6f;

		#endregion

		#region Implementation

		// Uses cubic ease out curve to reduce the node weight back to 1
		public float GetWeightChangeToNeutralize(float time, float deltaTime)
		{
			if (Weight == 1)
				return 0;

			float currentDifference = Weight - 1;
			float absCurrentDifference = Mathf.Abs(currentDifference);

			if (absCurrentDifference < 0.01f)
				return -currentDifference;

			float timeSinceChange = time - m_changeTime;

			float weightChange = m_startingDifference * s_changeFactor * Mathf.Pow(timeSinceChange, 3) * deltaTime;
			return -Mathf.Clamp(weightChange, -absCurrentDifference, absCurrentDifference);
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