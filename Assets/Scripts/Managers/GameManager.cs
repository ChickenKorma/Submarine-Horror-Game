using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	#region Variables

	public static GameManager Instance { get; private set; }

	public bool IsPaused { get; private set; }

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
		CreatureBehaviour.Instance.AttackedPlayer += PauseTime;

		ExitDetection.Instance.GameWon += PauseTime;

		InputManager.Instance.PauseEvent += TogglePause;
	}

	private void OnDisable()
	{
		CreatureBehaviour.Instance.AttackedPlayer -= PauseTime;

		ExitDetection.Instance.GameWon -= PauseTime;

		InputManager.Instance.PauseEvent -= TogglePause;
	}

	#endregion

	#region Implementation

	public void LoadScene(int sceneBuildIndex)
	{
		Time.timeScale = 1;
		SceneManager.LoadScene(sceneBuildIndex);
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void TogglePause()
	{
		IsPaused = !IsPaused;
		Time.timeScale = IsPaused ? 0 : 1;
	}

	private void PauseTime()
	{
		Time.timeScale = 0;
	}

	#endregion
}
