#if (UNITY_EDITOR)
using UnityEditor;

namespace EditorTools
{
	public class DebugToolsWindow : EditorWindow
	{
		#region Unity

		[MenuItem("Tools/Debug Tools")]
		public static void Open()
		{
			GetWindow<DebugToolsWindow>();
		}

		private void OnEnable()
		{
			if (Instance != null)
				Destroy(this);
			else
				Instance = this;
		}

		private void OnGUI()
		{
			EditorWindowTools.DrawHeader("Visualise Objects");
			ObjectsEnabled = EditorGUILayout.Toggle("Enabled", ObjectsEnabled);
			EditorGUILayout.Space();
			ShowPlayer = EditorGUILayout.Toggle("Show Player", ShowPlayer);
			ShowCreature = EditorGUILayout.Toggle("Show Creature", ShowCreature);

			EditorWindowTools.DrawDivider();

			EditorWindowTools.DrawHeader("Visualise Nodes, Graphs and Paths");
			GraphEnabled = EditorGUILayout.Toggle("Enabled", GraphEnabled);
			EditorGUILayout.Space();
			ShowNodesAndConnections = EditorGUILayout.Toggle("Show Nodes and Connections", ShowNodesAndConnections);
			ShowNodeWeights = EditorGUILayout.Toggle("Show Node Weights", ShowNodeWeights);
			ShowCreaturePath = EditorGUILayout.Toggle("Show Creature Path", ShowNodeWeights);
		}

		#endregion

		#region Variables

		public static DebugToolsWindow Instance;

		public bool ObjectsEnabled;

		public bool ShowPlayer;
		public bool ShowCreature;

		public bool GraphEnabled;

		public bool ShowNodesAndConnections;
		public bool ShowNodeWeights;
		public bool ShowCreaturePath;

		#endregion
	}
}
#endif