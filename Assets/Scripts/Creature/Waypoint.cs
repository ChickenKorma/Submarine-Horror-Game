using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
	public List<Waypoint> Connections = new();

	public void AddConnection(Waypoint connection)
	{
		if (!Connections.Contains(connection) && connection != this)
		{
			Connections.Add(connection);
		}
	}

	public void RemoveConnection(Waypoint connection)
	{
		Connections.Remove(connection);
	}
}