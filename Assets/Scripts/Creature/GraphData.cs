using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Data is saved as so :
// "index,wanderable,positionx,positiony,positionz,connectionIndex1,connectionIndex2 ..."

public static class GraphData
{
	public static void SaveGraph(Node[] nodes, string filePath)
	{
		using StreamWriter writer = new(filePath, false);

		for (int i = 0; i < nodes.Length; i++)
		{
			Node node = nodes[i];
			List<string> nodeData = new()
			{
				node.Index.ToString(),
				node.Position.x.ToString(),
				node.Position.y.ToString(),
				node.Position.z.ToString()
			};

			foreach (int connectionIndex in node.ConnectionIndexes)
			{
				nodeData.Add(connectionIndex.ToString());
			}

			writer.WriteLine(string.Join(',', nodeData));
		}
	}

	public static GraphModel LoadGraph(string filePath)
	{
		List<Node> nodes = new();

		using (StreamReader reader = new(filePath))
		{
			while (reader.Peek() >= 0)
			{
				Node node = TextToNode(reader.ReadLine());
				nodes.Add(node);
			}
		}

		return new() { Nodes = nodes.ToArray() };
	}

	public static GraphModel LoadGraph(TextAsset asset)
	{
		string[] lines = asset.text.Split(new char[] { '\r', '\n' });

		List<Node> nodes = new();
		List<Node> wanderableNodes = new();

		foreach (string line in lines)
		{
			if (!string.IsNullOrEmpty(line))
			{
				Node node = TextToNode(line);
				nodes.Add(node);
			}
		}

		return new() { Nodes = nodes.ToArray() };
	}

	private static Node TextToNode(string text)
	{
		string[] nodeData = text.Split(',');

		int index = int.Parse(nodeData[0]);
		float posX = float.Parse(nodeData[1]);
		float posY = float.Parse(nodeData[2]);
		float posZ = float.Parse(nodeData[3]);

		List<int> connections = new();

		for (int i = 4; i < nodeData.Length; i++)
		{
			connections.Add(int.Parse(nodeData[i]));
		}

		return new Node(index, new Vector3(posX, posY, posZ), connections);
	}
}
