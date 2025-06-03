using UnityEngine;
using UnityEngine.EventSystems;

namespace Utilities
{
	public class SelectOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
	{
		#region Variables

		private EventSystem m_eventSystem;

		private bool m_pointerOnElement;

		#endregion

		#region Unity

		private void Awake()
		{
			m_eventSystem = EventSystem.current;
		}

		#endregion

		#region Pointer Events

		public void OnPointerEnter(PointerEventData eventData)
		{
			m_pointerOnElement = true;
			SelectGameObject();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			m_pointerOnElement = false;
		}

		public void OnPointerMove(PointerEventData eventData)
		{
			if (m_pointerOnElement)
				SelectGameObject();
		}

		#endregion

		#region Implementation

		private void SelectGameObject()
		{
			if (m_eventSystem.currentSelectedGameObject != gameObject)
			{
				m_eventSystem.SetSelectedGameObject(gameObject);
			}
		}

		#endregion
	}
}