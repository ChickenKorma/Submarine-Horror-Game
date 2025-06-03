using Creature;
using Exit;
using UnityEngine;

namespace Managers
{
	public class GameManager : MonoBehaviour
	{
		#region Variables

		public static GameManager Instance { get; private set; }

		public bool IsPaused { get; private set; }

		private GameState m_gameState;

		private float m_oldTimeScale = 1;

		#endregion

		#region Unity

		private void Awake()
		{
			if (Instance != null)
				Destroy(this);
			else
				Instance = this;
		}

		private void OnEnable()
		{
			CreatureBehaviour.Instance.AttackedPlayer += EndGame;

			ExitDetection.Instance.GameWon += EndGame;

			InputManager.Instance.PauseEvent += TogglePause;
		}

		private void OnDisable()
		{
			CreatureBehaviour.Instance.AttackedPlayer -= EndGame;

			ExitDetection.Instance.GameWon -= EndGame;

			InputManager.Instance.PauseEvent -= TogglePause;
		}

		#endregion

		#region Implementation

		public void TogglePause()
		{
			switch (m_gameState)
			{
				case GameState.Paused:
					m_gameState = GameState.Playing;
					InputManager.Instance.EnableGameplayControls();

					IsPaused = false;
					UpdateTimeScale();
					break;

				case GameState.Playing:
					m_gameState = GameState.Paused;
					InputManager.Instance.DisableGameplayControls();

					IsPaused = true;
					UpdateTimeScale();
					break;

				case GameState.GameOver:
					IsPaused = false;
					break;
			}
		}

		private void UpdateTimeScale()
		{
			if (IsPaused)
			{
				m_oldTimeScale = Time.timeScale;
				Time.timeScale = 0;
			}
			else
				Time.timeScale = m_oldTimeScale;
		}

		private void EndGame()
		{
			m_gameState = GameState.GameOver;
			InputManager.Instance.DisableGameplayControls();

			Time.timeScale = 0;
		}

		#endregion

		#region Enums

		private enum GameState
		{
			Playing,
			Paused,
			GameOver
		}

		#endregion
	}
}
