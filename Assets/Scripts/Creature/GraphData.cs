using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Data is saved as so :
// "index,positionx,positiony,positionz,connectionIndex1,connectionIndex2 ..."

public static class GraphData
{
    public static void SaveGraph(Node[] nodes, string filePath)
    {
        using StreamWriter writer = new StreamWriter(filePath, false);

        for (int i = 0; i < nodes.Length; i++)
        {
            Node node = nodes[i];
            List<string> nodeData = new List<string>
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
        List<Node> nodes = new List<Node>();

        using (StreamReader reader = new StreamReader(filePath))
        {
            while (reader.Peek() >= 0)
            {
                string nodeData = reader.ReadLine();
                nodes.Add(TextToNode(nodeData));
            }
        }

        GraphModel graph = new GraphModel();
        graph.Nodes = nodes.ToArray();

        return graph;
    }

    public static GraphModel LoadGraph(TextAsset asset)
    {
        string[] lines = asset.text.Split(new char[] { '\r', '\n' });

        List<Node> nodes = new List<Node>();

        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                nodes.Add(TextToNode(line));
            }
        }

        GraphModel graph = new GraphModel();
        graph.Nodes = nodes.ToArray();

        return graph;
    }

    private static Node TextToNode(string text)
    {
        string[] nodeData = text.Split(',');

        int index = int.Parse(nodeData[0]);
        float posX = float.Parse(nodeData[1]);
        float posY = float.Parse(nodeData[2]);
        float posZ = float.Parse(nodeData[3]);

        List<int> connections = new List<int>();

        for (int i = 4; i < nodeData.Length; i++)
        {
            connections.Add(int.Parse(nodeData[i]));
        }

        return new Node(index, new Vector3(posX, posY, posZ), connections);
    }
}