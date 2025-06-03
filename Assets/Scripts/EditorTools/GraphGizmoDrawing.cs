using Creature;
using System.Linq;
using UnityEngine;

namespace EditorTools
{
	public class GraphGizmoDrawing : MonoBehaviour
	{


#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (DebugToolsWindow.Instance != null && DebugToolsWindow.Instance.GraphEnabled)
			{
				if (DebugToolsWindow.Instance.ShowNodesAndConnections)
					DrawNodesAndConnections();

				if (DebugToolsWindow.Instance.ShowCreaturePath)
					DrawCreaturePath();

				if (DebugToolsWindow.Instance.ShowNodeWeights)
					DrawNodeWeights();
			}
		}
#endif

		#region Draw Graph/Nodes

		private void DrawNodesAndConnections()
		{
			foreach (Waypoint waypoint in transform.GetComponentsInChildren<Waypoint>())
			{
				if (waypoint != null)
				{
					Gizmos.color = Color.yellow;

					Gizmos.DrawSphere(waypoint.transform.position, 0.8f);

					foreach (Waypoint connection in waypoint.Connections)
					{
						if (connection.transform != null)
						{
							Gizmos.DrawLine(waypoint.transform.position, connection.transform.position);
						}
					}
				}
			}
		}

		private void DrawNodeWeights()
		{
			if (CreatureBehaviour.Instance != null)
			{
				foreach (CreatureBehaviour.WeightedNode node in CreatureBehaviour.Instance.Nodes)
				{
					float hue = 0.333f;

					if (node.Weight > 1)
						hue = Mathf.Clamp(0.333f - (0.333f * node.Weight / 20), 0, 0.333f);
					else if (node.Weight < 1)
						hue = Mathf.Clamp(0.333f + (0.666f * (1 - node.Weight)), 0.333f, 0.666f);

					Gizmos.color = Color.HSVToRGB(hue, 1, 1);
					Gizmos.DrawSphere(node.Node.Position, 2);
				}
			}
		}

		private void DrawCreaturePath()
		{
			if (CreatureBehaviour.Instance != null && CreatureBehaviour.Instance.CurrentNode != null && CreatureBehaviour.Instance.TargetNode != null)
			{
				Gizmos.color = Color.green;

				Gizmos.DrawLine(CreatureBehaviour.Instance.CurrentNode.Node.Position, CreatureBehaviour.Instance.TargetNode.Node.Position);

				if (CreatureBehaviour.Instance.CurrentPath.Any())
				{
					Gizmos.DrawLine(CreatureBehaviour.Instance.TargetNode.Node.Position, CreatureBehaviour.Instance.CurrentPath[0].Node.Position);

					for (int i = 1; i < CreatureBehaviour.Instance.CurrentPath.Count; i++)
					{
						Gizmos.DrawLine(CreatureBehaviour.Instance.CurrentPath[i - 1].Node.Position, CreatureBehaviour.Instance.CurrentPath[i].Node.Position);
					}
				}
			}
		}

		#endregion
	}
}
