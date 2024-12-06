using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	#region Variables

	public static GameManager Instance { get; private set; }

	#endregion

	#region Unity

	private void Awake()
	{
		if (Instance != null)
			Destroy(this);
		else
			Instance = this;
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

	#endregion
}
