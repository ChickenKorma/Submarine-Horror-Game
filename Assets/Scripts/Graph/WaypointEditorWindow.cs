#if (UNITY_EDITOR)
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaypointEditorWindow : EditorWindow
{
	#region Unity

	[MenuItem("Tools/Waypoint Window")]
	public static void Open()
	{
		GetWindow<WaypointEditorWindow>();
	}

	public Transform WaypointRoot;

	private void OnGUI()
	{
		SerializedObject obj = new(this);

		EditorGUILayout.PropertyField(obj.FindProperty("WaypointRoot"));

		if (WaypointRoot == null)
		{
			EditorGUILayout.HelpBox("No root transform selected!", MessageType.Warning);
		}
		else
		{
			EditorWindowTools.DrawDivider();

			EditorWindowTools.DrawHeader("Waypoint Controls");

			EditorGUILayout.BeginVertical();
			DrawWaypointControls();
			EditorGUILayout.EndVertical();

			EditorWindowTools.DrawDivider();

			EditorWindowTools.DrawHeader("Graph Controls");

			EditorGUILayout.BeginVertical();
			DrawGraphControls();
			EditorGUILayout.EndVertical();
		}

		obj.ApplyModifiedProperties();
	}

	#endregion

	#region Waypoint Controls

	private void DrawWaypointControls()
	{
		if (GUILayout.Button("Create Waypoint"))
		{
			GameObject waypointObj = CreateNewWaypoint();
			Selection.activeGameObject = waypointObj;
		}

		if (GUILayout.Button("Delete Waypoints"))
			DeleteWaypoints();

		if (GUILayout.Button("Link Waypoints"))
			LinkWaypoints();

		if (GUILayout.Button("Unlink Waypoints"))
			UnlinkWaypoints();
	}

	private GameObject CreateNewWaypoint()
	{
		Waypoint[] selectedWaypoints = GetSelectedWaypoints();

		if (selectedWaypoints.Length != 0)
			return CreateWaypoint(selectedWaypoints[0].transform.position);
		else
		{
			Waypoint[] waypoints = WaypointRoot.GetComponentsInChildren<Waypoint>();

			if (waypoints.Length != 0)
				return CreateWaypoint(waypoints[^1].transform.position);
		}

		return CreateWaypoint();
	}

	private GameObject CreateWaypoint(Vector3 position = new Vector3())
	{
		GameObject waypointObj = new("Waypoint " + WaypointRoot.childCount, typeof(Waypoint));
		waypointObj.transform.SetParent(WaypointRoot);
		waypointObj.transform.localPosition = position;

		return waypointObj;
	}

	private void DeleteWaypoints()
	{
		foreach (Waypoint waypoint in GetSelectedWaypoints())
		{
			foreach (Waypoint connection in waypoint.Connections)
			{
				connection.RemoveConnection(waypoint);
			}

			DestroyImmediate(waypoint.gameObject);
		}
	}

	private void LinkWaypoints()
	{
		Waypoint[] waypoints = GetSelectedWaypoints();

		for (int i = 0; i < waypoints.Length; i++)
		{
			for (int j = 0; j < waypoints.Length; j++)
			{
				if (i != j)
					waypoints[i].AddConnection(waypoints[j]);
			}
		}
	}

	private void UnlinkWaypoints()
	{
		Waypoint[] waypoints = GetSelectedWaypoints();

		for (int i = 0; i < waypoints.Length; i++)
		{
			for (int j = 0; j < waypoints.Length; j++)
			{
				if (i != j)
					waypoints[i].RemoveConnection(waypoints[j]);
			}
		}
	}

	private Waypoint[] GetSelectedWaypoints()
	{
		List<Waypoint> waypoints = new();

		foreach (GameObject obj in Selection.gameObjects)
		{
			if (obj.TryGetComponent(out Waypoint waypoint))
				waypoints.Add(waypoint);
		}

		return waypoints.ToArray();
	}

	#endregion

	#region Graph Controls

	private void DrawGraphControls()
	{
		if (GUILayout.Button("Save Graph"))
		{
			string savePath = EditorUtility.SaveFilePanelInProject("Save graph", "Graph.txt", "txt", "Select location and name to save graph", "Assets/Resources/");

			if (savePath != null)
				SaveGraph(savePath);
		}

		if (GUILayout.Button("Load Graph"))
		{
			string graphPath = EditorUtility.OpenFilePanel("Select graph text file", "Assets/Resources/", "txt");

			if (graphPath != null)
			{
				GraphModel selectedGraph = GraphData.LoadGraph(graphPath);

				if (selectedGraph != null)
					LoadGraph(selectedGraph);
			}
		}

		if (GUILayout.Button("Delete Graph"))
			DeleteGraph();
	}

	private void SaveGraph(string savePath)
	{
		Waypoint[] waypoints = WaypointRoot.GetComponentsInChildren<Waypoint>();
		Node[] nodes = new Node[waypoints.Length];

		for (int i = 0; i < waypoints.Length; i++)
		{
			nodes[i] = new Node(i, waypoints[i]);
		}

		for (int i = 0; i < waypoints.Length; i++)
		{
			Waypoint waypoint = waypoints[i];

			for (int j = 0; j < waypoint.Connections.Count; j++)
			{
				int connectionIndex = Array.IndexOf(waypoints, waypoint.Connections[j]);
				nodes[i].ConnectionIndexes.Add(connectionIndex);
			}
		}

		GraphData.SaveGraph(nodes, savePath);
	}

	private void LoadGraph(GraphModel graph)
	{
		DeleteGraph();

		Waypoint[] waypoints = new Waypoint[graph.Nodes.Length];

		for (int i = 0; i < graph.Nodes.Length; i++)
		{
			waypoints[i] = CreateWaypoint(graph.Nodes[i].Position).GetComponent<Waypoint>();
		}

		for (int i = 0; i < graph.Nodes.Length; i++)
		{
			Node node = graph.Nodes[i];
			Waypoint nodeWaypoint = waypoints[i];

			for (int j = 0; j < node.ConnectionIndexes.Count; j++)
			{
				nodeWaypoint.AddConnection(waypoints[node.ConnectionIndexes[j]]);
			}
		}
	}

	private void DeleteGraph()
	{
		Waypoint[] waypoints = WaypointRoot.GetComponentsInChildren<Waypoint>();

		for (int i = 0; i < waypoints.Length; i++)
		{
			DestroyImmediate(waypoints[i].gameObject);
		}
	}

	#endregion
}
#endif