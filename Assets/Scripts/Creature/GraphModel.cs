using System.Collections.Generic;
using UnityEngine;

public class GraphModel
{
	public Node[] Nodes;
	public Node[] WanderableNodes;
}

[System.Serializable]
public class Node
{
	public Node(int index, Waypoint waypoint)
	{
		Index = index;
		Wanderable = waypoint.Wanderable;
		Position = waypoint.transform.localPosition;
		ConnectionIndexes = new List<int>();
	}

	public Node(int index, bool wanderable, Vector3 position, List<int> connectionIndexes)
	{
		Index = index;
		Wanderable = wanderable;
		Position = position;
		ConnectionIndexes = connectionIndexes;
	}

	public int Index { get; }

	public bool Wanderable { get; }

	public Vector3 Position { get; }

	public List<int> ConnectionIndexes { get; }
}