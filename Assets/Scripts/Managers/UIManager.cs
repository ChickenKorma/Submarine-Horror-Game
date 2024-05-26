using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Color red;
    [SerializeField] private Color green;

    [SerializeField] private Image pingIndicatorPanel;

    [SerializeField] private TMP_Text pingIndicatorText;

    void Update()
    {
        if (Sonar.Instance.PingEnabled)
        {
            pingIndicatorPanel.color = red;
            pingIndicatorText.text = "Pinging";
        }
        else
        {
            pingIndicatorPanel.color = green;
            pingIndicatorText.text = "Ping available";
        }
    }
}
