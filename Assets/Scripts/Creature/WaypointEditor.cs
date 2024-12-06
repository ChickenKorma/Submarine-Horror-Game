#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;

[InitializeOnLoad()]
public class WaypointEditor
{
	[DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
	public static void OnDrawSceneGizmo(Waypoint waypoint, GizmoType gizmoType)
	{
		if (waypoint != null)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(waypoint.transform.position, 0.8f);

			Gizmos.color = Color.yellow;

			foreach (Waypoint connection in waypoint.Connections)
			{
				if (connection.transform != null)
				{
					Gizmos.DrawLine(waypoint.transform.position, connection.transform.position);
				}
				else
				{
					waypoint.RemoveConnection(connection);
				}
			}
		}
	}
}
#endif