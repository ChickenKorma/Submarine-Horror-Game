using System.Collections.Generic;
using UnityEngine;

public class GraphModel
{
	public Node[] Nodes;
}

[System.Serializable]
public class Node
{
	public Node(int index, Waypoint waypoint)
	{
		Index = index;
		Position = waypoint.transform.localPosition;
		ConnectionIndexes = new List<int>();
	}

	public Node(int index, Vector3 position, List<int> connectionIndexes)
	{
		Index = index;
		Position = position;
		ConnectionIndexes = connectionIndexes;
	}

	public int Index { get; }

	public Vector3 Position { get; }

	public List<int> ConnectionIndexes { get; }
}