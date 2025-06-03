using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utilities
{
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public class HoldingInteraction : IInputInteraction
	{
		private bool m_isHolding;

		public void Process(ref InputInteractionContext context)
		{
			if (context.control.IsActuated(InputSystem.settings.defaultButtonPressPoint))
			{
				if (m_isHolding)
					context.Performed();
				else
				{
					context.Started();
					m_isHolding = true;
				}
			}
			else if (m_isHolding)
			{
				context.Canceled();
				m_isHolding = false;
			}
		}

		public void Reset()
		{
		}

		static HoldingInteraction()
		{
			InputSystem.RegisterInteraction<HoldingInteraction>();
		}

		// Executes static constructor on load before scene is loaded.
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
		}
	}
}
