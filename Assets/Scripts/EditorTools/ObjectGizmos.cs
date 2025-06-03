using UnityEngine;

namespace EditorTools
{
	public class ObjectGizmos : MonoBehaviour
	{
		#region Variables

		[SerializeField] private Transform m_playerTransform;
		[SerializeField] private Transform m_creatureTransform;

		#endregion

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (DebugToolsWindow.Instance != null && DebugToolsWindow.Instance.ObjectsEnabled)
			{
				if (DebugToolsWindow.Instance.ShowCreature)
					DrawCreature();

				if (DebugToolsWindow.Instance.ShowPlayer)
					DrawPlayer();
			}
		}
#endif

		#region Draw Objects

		private void DrawCreature()
		{
			if (m_creatureTransform != null)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(m_creatureTransform.position, 4);
			}
		}

		private void DrawPlayer()
		{
			if (m_playerTransform != null)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawWireSphere(m_playerTransform.position, 3);
			}
		}

		#endregion
	}
}
