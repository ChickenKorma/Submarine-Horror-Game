using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	#region Variables

	private bool m_inGame = true;

	[SerializeField] private GameObject m_gameOverScreen;

	// Sonar
	[SerializeField] private Color m_red;
	[SerializeField] private Color m_green;

	[SerializeField] private Image m_pingIndicatorPanel;

	[SerializeField] private TMP_Text m_pingIndicatorText;

	[SerializeField] private Transform m_creatureTransform;
	[SerializeField] private Transform m_playerTransform;

	// Motion indicator
	[SerializeField] private RectTransform m_motionIndicatorTransform;
	private Animator m_motionIndicatorAnimator;

	[SerializeField] private float m_maxCreatureDistance;
	[SerializeField] private float m_maxIndicatorScreenDistance;

	private bool m_isHunting;

	[SerializeField] private float m_wanderingFlashSpeed;
	[SerializeField] private float m_huntingFlashSpeed;

	private float m_motionIndicatorFlashSpeed;

	private static Vector2 s_motionIndicatorPositionOffset = new(-100, 100);

	#endregion

	#region Unity

	private void Awake()
	{
		m_gameOverScreen.SetActive(false);

		m_motionIndicatorAnimator = m_motionIndicatorTransform.GetComponent<Animator>();
	}

	private void Start()
	{
		OnHuntingStateChanged(false);

		StartCoroutine(FlashMotionIndicator());
	}

	private void Update()
	{
		UpdateSonarIndicator();
	}

	private void OnEnable()
	{
		CreatureBehaviour.Instance.AttackedPlayer += GameOver;
		CreatureBehaviour.Instance.HuntingStateChanged += OnHuntingStateChanged;
	}

	private void OnDisable()
	{
		CreatureBehaviour.Instance.AttackedPlayer -= GameOver;
		CreatureBehaviour.Instance.HuntingStateChanged -= OnHuntingStateChanged;
	}

	#endregion

	#region Game States

	private void GameOver()
	{
		m_inGame = false;
		m_gameOverScreen.SetActive(true);
		Time.timeScale = 0;
	}

	#endregion

	#region Sonar

	private void UpdateSonarIndicator()
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

	#endregion

	#region Motion indicator

	private IEnumerator FlashMotionIndicator()
	{
		while (m_inGame)
		{
			m_motionIndicatorFlashSpeed = m_isHunting ? m_huntingFlashSpeed : m_wanderingFlashSpeed;
			m_motionIndicatorAnimator.speed = m_motionIndicatorFlashSpeed;

			UpdateMotionIndicator();

			yield return new WaitForSeconds(1 / m_motionIndicatorFlashSpeed);
		}
	}

	private void UpdateMotionIndicator()
	{
		Vector3 directionToCreature = m_playerTransform.worldToLocalMatrix.MultiplyPoint(m_creatureTransform.position);

		float detectorMagnitude = Mathf.Clamp(directionToCreature.magnitude / m_maxCreatureDistance, 0, 1);

		directionToCreature.Normalize();
		directionToCreature *= detectorMagnitude * m_maxIndicatorScreenDistance;

		m_motionIndicatorTransform.anchoredPosition = new Vector2(directionToCreature.x, directionToCreature.z) + s_motionIndicatorPositionOffset;

		m_motionIndicatorAnimator.SetTrigger("Flash");
	}

	private void OnHuntingStateChanged(bool isHunting) => m_isHunting = isHunting;

	#endregion
}
