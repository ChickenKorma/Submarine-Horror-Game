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

	public Transform waypointRoot;

	private void OnGUI()
	{
		SerializedObject obj = new(this);

		EditorGUILayout.PropertyField(obj.FindProperty("waypointRoot"));

		if (waypointRoot == null)
		{
			EditorGUILayout.HelpBox("No root transform selected!", MessageType.Warning);
		}
		else
		{
			DrawDivider();

			DrawHeader("Waypoint Controls");

			EditorGUILayout.BeginVertical();
			DrawWaypointControls();
			EditorGUILayout.EndVertical();

			DrawDivider();

			DrawHeader("Graph Controls");

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
			GameObject waypointObj = CreateWaypoint();
			Selection.activeGameObject = waypointObj;
		}

		if (GUILayout.Button("Delete Waypoints"))
			DeleteWaypoints();

		if (GUILayout.Button("Link Waypoints"))
			LinkWaypoints();

		if (GUILayout.Button("Unlink Waypoints"))
			UnlinkWaypoints();

		if (GUILayout.Button("Toggle Wanderable"))
			ToggleWanderableWaypoints();
	}

	private GameObject CreateWaypoint(Vector3 position = new Vector3())
	{
		GameObject waypointObj = new("Waypoint " + waypointRoot.childCount, typeof(Waypoint));
		waypointObj.transform.SetParent(waypointRoot);
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

	private void ToggleWanderableWaypoints()
	{
		foreach (Waypoint waypoint in GetSelectedWaypoints())
			waypoint.Wanderable = !waypoint.Wanderable;
	}

	private Waypoint[] GetSelectedWaypoints()
	{
		List<Waypoint> waypoints = new();

		foreach (GameObject obj in Selection.gameObjects)
		{
			if (obj.TryGetComponent<Waypoint>(out Waypoint waypoint))
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
		Waypoint[] waypoints = waypointRoot.GetComponentsInChildren<Waypoint>();
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
			waypoints[i].Wanderable = graph.Nodes[i].Wanderable;
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
		Waypoint[] waypoints = waypointRoot.GetComponentsInChildren<Waypoint>();

		for (int i = 0; i < waypoints.Length; i++)
		{
			DestroyImmediate(waypoints[i].gameObject);
		}
	}

	#endregion

	#region Helper methods

	private static void DrawDivider(int thickness = 1, int padding = 30)
	{
		Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
		r.height = thickness;
		r.y += padding / 2;
		r.x -= 2;
		r.width += 6;
		EditorGUI.DrawRect(r, Color.grey);
	}

	private static void DrawHeader(string text)
	{
		GUIStyle style = EditorStyles.largeLabel;
		style.fontStyle = FontStyle.Bold;

		DrawLabel(text, style);
	}

	private static void DrawLabel(string text, GUIStyle style)
	{
		Vector2 size = style.CalcSize(new GUIContent(text));
		Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(size.y * 2));

		EditorGUI.LabelField(r, text, style);
	}

	#endregion
}