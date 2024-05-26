using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBehaviour : MonoBehaviour
{
    #region Variables

    public static CreatureBehaviour Instance;

    [SerializeField]
    private TextAsset graphTextAsset;

    private Node[] navigationGraph;

    private Node currentNode;
    private Node targetNode;
    private List<Node> currentPath = new();

    private float targetNodeDistance = 0;
    private float currentTravelDistance = 0;

    [SerializeField]
    private float travelSpeed;

    private bool hunting;

    [SerializeField] private float volumeMaxThreshold;
    [SerializeField] private float volumeMinThreshold;

    private float nodePositionTolerance;

    private List<SoundLog> soundBuffer;

    [SerializeField] private Transform soundLog;

    [SerializeField]
    private Transform playerTransform;

    private float totalVolume;
    private Node averageSoundNode;

    #endregion

    #region Unity

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        navigationGraph = GraphData.LoadGraph(graphTextAsset).Nodes;

        if (navigationGraph == null)
            throw new NullReferenceException();

        soundBuffer = new List<SoundLog>();

        currentNode = navigationGraph[0];
    }

    private void Update()
    {
        Move();
    }

    #endregion

    #region Pathing

    private void Move()
    {
        if (hunting = totalVolume > volumeMaxThreshold || (hunting && totalVolume > volumeMinThreshold))
        {
            if (currentPath.Count > 0 && currentPath[currentPath.Count - 1].Position.Equals(averageSoundNode.Position))
            {
                if (targetNode == null)
                    IncrementTargetNode();
            }
            else
            {
                if (targetNode == null)
                {
                    currentPath = GetShortestPath(navigationGraph, currentNode, averageSoundNode);
                    IncrementTargetNode();
                }
                else
                    currentPath = GetShortestPath(navigationGraph, targetNode, averageSoundNode);
            }

            transform.localScale = new Vector3(2, 2, 2);
        }
        else
        {
            if (targetNode == null)
            {
                if (currentPath.Count == 0)
                    currentPath = GetShortestPath(navigationGraph, currentNode, navigationGraph[UnityEngine.Random.Range(0, navigationGraph.Length)]);

                IncrementTargetNode();
            }

            transform.localScale = new Vector3(1, 1, 1);
        }

        currentTravelDistance += travelSpeed * Time.deltaTime;

        if (targetNode == null)
            transform.position = currentNode.Position;
        else
            transform.position = currentNode.Position + ((targetNode.Position - currentNode.Position) * (currentTravelDistance / targetNodeDistance));

        if (currentTravelDistance >= targetNodeDistance)
        {
            currentNode = targetNode;
            targetNode = null;
            currentTravelDistance = 0;
        }
    }

    private void IncrementTargetNode()
    {
        if (currentPath.Count > 0)
        {
            targetNode = currentPath[0];
            currentPath.RemoveAt(0);

            targetNodeDistance = Vector3.Distance(currentNode.Position, targetNode.Position);
        }
        else
        {
            currentTravelDistance = 0; // Safety net for when creature is trying to move to its current node
        }
    }

    // Shortest path between source and destination excluding source node in final path
    private static List<Node> GetShortestPath(Node[] nodes, Node source, Node destination)
    {
        if (source == destination)
            return new();

        int sourceIndex = Array.IndexOf(nodes, source);
        int destinationIndex = Array.IndexOf(nodes, destination);

        if (sourceIndex < 0 || destinationIndex < 0)
            return new();

        List<Node> nodeQueue = new List<Node>();

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
                    Node adjacentNode = nodes[connectionIndexes[i]];
                    int adjacentIndex = connectionIndexes[i];

                    if (isNodeVisited[adjacentIndex] == false)
                    {
                        isNodeVisited[adjacentIndex] = true;
                        distance[adjacentIndex] = distance[nodeIndex] + 1;
                        predecessor[adjacentIndex] = nodeIndex;
                        nodeQueue.Add(adjacentNode);

                        if (adjacentIndex == destinationIndex)
                        {
                            List<Node> shortestPath = new List<Node>();

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
        Vector3 playerPosition = playerTransform.position;

        Node nearestNode = navigationGraph[0];
        float nearestDistance = Vector3.SqrMagnitude(position - nearestNode.Position);

        if (nearestDistance > nodePositionTolerance)
        {
            for (int i = 1; i < navigationGraph.Length; i++)
            {
                float distance = Vector3.SqrMagnitude(position - navigationGraph[i].Position);

                if (distance < nearestDistance)
                {
                    nearestNode = navigationGraph[i];

                    if (distance < nodePositionTolerance)
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

    #region Sound

    public void AddSound(Vector3 location, float volume, float duration)
    {
        SoundLog sound = new SoundLog(location, volume);

        soundBuffer.Add(sound);
        StartCoroutine(RemoveSound(sound, duration));

        UpdateSound();
    }

    private IEnumerator RemoveSound(SoundLog sound, float duration)
    {
        yield return new WaitForSeconds(duration);

        soundBuffer.Remove(sound);

        UpdateSound();
    }

    private void UpdateSound()
    {
        float aggregateVolume = 0;
        Vector3 aggregateSoundPosition = Vector3.zero;

        if (soundBuffer.Count > 0)
        {
            foreach (SoundLog soundLog in soundBuffer)
            {
                aggregateVolume += soundLog.Volume;
                aggregateSoundPosition += soundLog.Source * soundLog.Volume;
            }

            totalVolume = aggregateVolume;
            averageSoundNode = GetNearestNode(aggregateSoundPosition / aggregateVolume);

            soundLog.position = averageSoundNode.Position;
        }
        else
        {
            soundLog.position = new(2000, 0, 0);
        }
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