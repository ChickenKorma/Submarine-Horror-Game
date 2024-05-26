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
			Gizmos.color = gizmoType == GizmoType.Selected ? Color.green : Color.yellow;
			Gizmos.DrawSphere(waypoint.transform.position, 0.5f);

			Gizmos.color = Color.red;

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