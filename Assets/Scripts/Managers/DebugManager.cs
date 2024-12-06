using TMPro;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
	[SerializeField] private TMP_Text m_fpsText;
	[SerializeField] private TMP_Text m_averageFpsText;

	private float m_lastFPSSampleTime;

	void Update()
	{
		if (Time.time > m_lastFPSSampleTime + 1)
		{
			m_fpsText.text = $"FPS: {(int)(1 / Time.unscaledDeltaTime)}";
			m_averageFpsText.text = $"Average FPS: {(int)(Time.frameCount / Time.time)}";

			m_lastFPSSampleTime = Time.time;
		}
	}
}
