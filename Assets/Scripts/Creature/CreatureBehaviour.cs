using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBehaviour : MonoBehaviour
{
	#region Variables

	public static CreatureBehaviour Instance { get; private set; }

	private Node[] m_navigationGraphNodes;
	private Node[] m_wanderableGraphNodes;

	private List<Node> m_currentPath = new();
	private Node m_currentNode;
	private Node m_targetNode;

	[SerializeField] private float m_travelSpeed;
	private float m_targetNodeDistance = 10000;
	private float m_currentTravelDistance = 0;

	[SerializeField] private float m_nodePositionTolerance;

	[SerializeField] private Transform m_playerTransform;

	[SerializeField] private float m_attackDistance;
	private float m_attackDistanceSquared;

	[SerializeField] private float m_beaconAttackDuration;
	private bool m_isAttackingBeacon;

	public Action<bool> HuntingStateChanged = delegate { };
	private bool m_isHunting;

	public Action AttackedPlayer = delegate { };

	public bool IsHunting
	{
		get => m_isHunting;
		private set
		{
			if (m_isHunting != value)
			{
				m_isHunting = value;
				HuntingStateChanged.Invoke(value);
			}
		}
	}

	private List<SoundLog> m_soundBuffer;
	private float m_totalVolume;
	private Node m_averageSoundNode;

	[SerializeField] private float m_volumeMaxThreshold;
	[SerializeField] private float m_volumeMinThreshold;

	[SerializeField] private Transform m_soundLog;

	private Coroutine m_roaringCoroutine;
	private Coroutine m_growlingCoroutine;

	[SerializeField] private string m_graphTextFileName;

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
		m_navigationGraphNodes = graphModel.Nodes;
		m_wanderableGraphNodes = graphModel.WanderableNodes;

		m_currentNode = m_navigationGraphNodes[0];

		m_soundBuffer = new List<SoundLog>();

		m_attackDistanceSquared = m_attackDistance * m_attackDistance;
	}

	private void Update()
	{
		if (!m_isAttackingBeacon)
		{
			Move();
			CheckForAttacks();
		}
	}

	#endregion

	#region Pathing

	private void Move()
	{
		if (IsHunting = m_totalVolume > m_volumeMaxThreshold || (IsHunting && m_totalVolume > m_volumeMinThreshold))
		{
			if (m_currentPath.Count > 0 && m_currentPath[^1].Position.Equals(m_averageSoundNode.Position))
			{
				if (m_targetNode == null)
					IncrementTargetNode();
			}
			else
			{
				if (m_targetNode == null)
				{
					m_currentPath = GetShortestPath(m_currentNode, m_averageSoundNode);
					IncrementTargetNode();
				}
				else
					m_currentPath = GetShortestPath(m_targetNode, m_averageSoundNode);
			}

			transform.localScale = new Vector3(2, 2, 2);

			m_roaringCoroutine ??= StartCoroutine(PlayRoaringSounds());
		}
		else
		{
			if (m_targetNode == null)
			{
				if (m_currentPath.Count == 0)
					m_currentPath = GetShortestPath(m_currentNode, m_wanderableGraphNodes[UnityEngine.Random.Range(0, m_wanderableGraphNodes.Length)], true);

				IncrementTargetNode();
			}

			transform.localScale = new Vector3(1, 1, 1);

			m_growlingCoroutine ??= StartCoroutine(PlayGrowlingSounds());
		}

		m_currentTravelDistance += m_travelSpeed * Time.deltaTime;

		if (m_currentTravelDistance >= m_targetNodeDistance)
		{
			m_currentNode = m_targetNode;
			m_targetNode = null;
			m_currentTravelDistance = 0;
		}

		if (m_targetNode == null)
			transform.position = m_currentNode.Position;
		else
		{
			transform.position = m_currentNode.Position + ((m_targetNode.Position - m_currentNode.Position) * (float)(m_currentTravelDistance / m_targetNodeDistance));
			transform.LookAt(m_targetNode.Position);
		}
	}

	private void IncrementTargetNode()
	{
		if (m_currentPath.Count > 0)
		{
			m_targetNode = m_currentPath[0];
			m_currentPath.RemoveAt(0);

			m_targetNodeDistance = Vector3.Distance(m_currentNode.Position, m_targetNode.Position);
		}
		else
		{
			m_currentTravelDistance = 0; // Safety net for when creature is trying to move to its current node
		}
	}

	// Shortest path between source and destination excluding source node in final path
	private List<Node> GetShortestPath(Node source, Node destination, bool wandering = false)
	{
		Node[] nodes = m_navigationGraphNodes;

		if (source == destination)
			return new();

		int sourceIndex = Array.IndexOf(nodes, source);
		int destinationIndex = Array.IndexOf(nodes, destination);

		if (sourceIndex < 0 || destinationIndex < 0)
			return new();

		List<Node> nodeQueue = new();

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
			Node node = nodeQueue[0];
			int nodeIndex = node.Index;

			nodeQueue.RemoveAt(0);

			List<int> connectionIndexes = node.ConnectionIndexes;

			if (connectionIndexes != null)
			{
				for (int i = 0; i < connectionIndexes.Count; i++)
				{
					int adjacentIndex = connectionIndexes[i];
					Node adjacentNode = m_navigationGraphNodes[adjacentIndex];

					if (wandering && !adjacentNode.Wanderable)
						continue;

					if (isNodeVisited[adjacentIndex] == false)
					{
						isNodeVisited[adjacentIndex] = true;
						distance[adjacentIndex] = distance[nodeIndex] + 1;
						predecessor[adjacentIndex] = nodeIndex;
						nodeQueue.Add(adjacentNode);

						if (adjacentIndex == destinationIndex)
						{
							List<Node> shortestPath = new();

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

	private Node GetNearestNode(Vector3 position)
	{
		Node nearestNode = m_navigationGraphNodes[0];
		float nearestDistance = Vector3.SqrMagnitude(position - nearestNode.Position);

		if (nearestDistance > m_nodePositionTolerance)
		{
			for (int i = 1; i < m_navigationGraphNodes.Length; i++)
			{
				float distance = Vector3.SqrMagnitude(position - m_navigationGraphNodes[i].Position);

				if (distance < nearestDistance)
				{
					nearestNode = m_navigationGraphNodes[i];

					if (distance < m_nodePositionTolerance)
					{
						break;
					}
					else
					{
						nearestDistance = distance;
					}
				}
			}
		}

		return nearestNode;
	}

	#endregion

	#region Attacking

	private void CheckForAttacks()
	{
		if (Beacon.Instance != null)
		{
			Vector3 directionToBeacon = Beacon.Instance.transform.position - transform.position;

			if (Vector3.SqrMagnitude(directionToBeacon) <= m_attackDistanceSquared && Physics.Raycast(new Ray(transform.position, directionToBeacon), out RaycastHit beaconHit) && beaconHit.collider.CompareTag("Beacon"))
			{
				Destroy(Beacon.Instance.gameObject);
				AudioManager.Instance.CreatureAttackBeacon();
				StartCoroutine(AwaitBeaconAttack());
			}
		}

		if (!m_isAttackingBeacon)
		{
			Vector3 directionToPlayer = m_playerTransform.position - transform.position;

			if (Vector3.SqrMagnitude(directionToPlayer) <= m_attackDistanceSquared && Physics.Raycast(new Ray(transform.position, directionToPlayer), out RaycastHit playerHit) && playerHit.collider.CompareTag("Player"))
			{
				AttackedPlayer.Invoke();
				AudioManager.Instance.CreatureAttackPlayer();
			}
		}
	}

	private IEnumerator AwaitBeaconAttack()
	{
		m_isAttackingBeacon = true;

		yield return new WaitForSeconds(m_beaconAttackDuration);

		m_isAttackingBeacon = false;
	}

	#endregion

	#region Sound Logging

	public void AddSound(Vector3 location, float volume, float duration)
	{
		SoundLog sound = new(location, volume);

		m_soundBuffer.Add(sound);
		StartCoroutine(RemoveSound(sound, duration));

		UpdateSound();
	}

	private IEnumerator RemoveSound(SoundLog sound, float duration)
	{
		yield return new WaitForSeconds(duration);

		m_soundBuffer.Remove(sound);

		UpdateSound();
	}

	private void UpdateSound()
	{
		float aggregateVolume = 0;
		Vector3 aggregateSoundPosition = Vector3.zero;

		if (m_soundBuffer.Count > 0)
		{
			foreach (SoundLog soundLog in m_soundBuffer)
			{
				aggregateVolume += soundLog.Volume;
				aggregateSoundPosition += soundLog.Source * soundLog.Volume;
			}

			m_totalVolume = aggregateVolume;
			m_averageSoundNode = GetNearestNode(aggregateSoundPosition / aggregateVolume);

			m_soundLog.position = m_averageSoundNode.Position;
		}
		else
		{
			m_soundLog.position = new(2000, 0, 0);
		}
	}

	#endregion

	#region Audio

	private IEnumerator PlayRoaringSounds()
	{
		while (IsHunting)
			yield return new WaitForSeconds(AudioManager.Instance.PlayCreatureRoar() + UnityEngine.Random.Range(0.3f, 1f));

		m_roaringCoroutine = null;
	}

	private IEnumerator PlayGrowlingSounds()
	{
		while (!IsHunting)
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(10, 15));

			AudioManager.Instance.PlayCreatureGrowl();
		}

		m_growlingCoroutine = null;
	}

	#endregion
}

readonly struct SoundLog
{
	public SoundLog(Vector3 source, float volume)
	{
		Source = source;
		Volume = volume;
	}

	public readonly Vector3 Source;
	public readonly float Volume;
}