using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
	public class LevelManager : MonoBehaviour
	{
		public static void LoadScene(int sceneBuildIndex)
		{
			Time.timeScale = 1;
			SceneManager.LoadScene(sceneBuildIndex);
		}

		public static void QuitGame()
		{
			Application.Quit();
		}
	}
}
