using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	[SerializeField] private Color m_red;
	[SerializeField] private Color m_green;

	[SerializeField] private Image m_pingIndicatorPanel;

	[SerializeField] private TMP_Text m_pingIndicatorText;

	void Update()
	{
		if (Sonar.Instance.PingEnabled)
		{
			m_pingIndicatorPanel.color = m_red;
			m_pingIndicatorText.text = "Pinging";
		}
		else
		{
			m_pingIndicatorPanel.color = m_green;
			m_pingIndicatorText.text = "Ping available";
		}
	}
}
