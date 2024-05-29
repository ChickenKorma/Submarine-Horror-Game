using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	#region Variables

	private bool m_inGame = true;

	[SerializeField] private GameObject m_gameOverScreen;
	[SerializeField] private GameObject m_gameWonScreen;

	// Sonar
	[SerializeField] private Color m_red;
	[SerializeField] private Color m_green;

	[SerializeField] private Image m_pingIndicatorPanel;
	[SerializeField] private TMP_Text m_pingIndicatorText;

	// Beacon
	[SerializeField] private Slider m_beaconInputSlider;

	// Motion indicator
	[SerializeField] private Transform m_creatureTransform;
	[SerializeField] private Transform m_playerTransform;
	[SerializeField] private Transform m_exitTransform;

	[SerializeField] private RectTransform m_creatureIndicatorTransform;
	[SerializeField] private RectTransform m_beaconIndicatorTransform;
	private Animator m_creatureIndicatorAnimator;
	private Animator m_beaconIndicatorAnimator;

	[SerializeField] private float m_maxIndicatedObjectDistance;
	[SerializeField] private float m_maxIndicatorScreenDistance;

	[SerializeField] private float m_wanderingCreatureFlashSpeed;
	[SerializeField] private float m_huntingCreatureFlashSpeed;
	[SerializeField] private float m_beaconFlashSpeed;

	[SerializeField] private RectTransform m_exitIndicatorTransform;

	private bool m_isHunting;

	private static Vector2 s_motionIndicatorPositionOffset = new(-100, 100);

	#endregion

	#region Unity

	private void Awake()
	{
		m_gameOverScreen.SetActive(false);
		m_gameWonScreen.SetActive(false);

		m_creatureIndicatorAnimator = m_creatureIndicatorTransform.GetComponent<Animator>();

		m_beaconIndicatorAnimator = m_beaconIndicatorTransform.GetComponent<Animator>();
		m_beaconIndicatorAnimator.speed = m_beaconFlashSpeed;
	}

	private void Start()
	{
		OnHuntingStateChanged(false);
		UpdateBeaconIndicator(0);

		StartCoroutine(FlashCreatureIndicator());
		StartCoroutine(FlashBeaconIndicator());
	}

	private void Update()
	{
		UpdateExitIndicator();
	}

	private void OnEnable()
	{
		ExitDetection.Instance.GameWon += GameWon;

		CreatureBehaviour.Instance.AttackedPlayer += GameOver;
		CreatureBehaviour.Instance.HuntingStateChanged += OnHuntingStateChanged;

		Sonar.Instance.PingStateChanged += UpdateSonarIndicator;
		Sonar.Instance.BeaconInputHold += UpdateBeaconIndicator;
	}

	private void OnDisable()
	{
		ExitDetection.Instance.GameWon -= GameWon;

		CreatureBehaviour.Instance.AttackedPlayer -= GameOver;
		CreatureBehaviour.Instance.HuntingStateChanged -= OnHuntingStateChanged;

		Sonar.Instance.PingStateChanged -= UpdateSonarIndicator;
		Sonar.Instance.BeaconInputHold -= UpdateBeaconIndicator;
	}

	#endregion

	#region Game States

	private void GameOver()
	{
		m_inGame = false;
		m_gameOverScreen.SetActive(true);
		Time.timeScale = 0;
	}

	private void GameWon()
	{
		m_inGame = false;
		m_gameWonScreen.SetActive(true);
		Time.timeScale = 0;
	}

	#endregion

	#region Sonar

	private void UpdateSonarIndicator(bool pingEnabled)
	{
		if (pingEnabled)
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

	private void UpdateBeaconIndicator(float holdProgress) => m_beaconInputSlider.value = holdProgress;

	#endregion

	#region Motion indicator

	private IEnumerator FlashCreatureIndicator()
	{
		while (m_inGame)
		{
			float creatureIndicatorFlashSpeed = m_isHunting ? m_huntingCreatureFlashSpeed : m_wanderingCreatureFlashSpeed;
			m_creatureIndicatorAnimator.speed = creatureIndicatorFlashSpeed;

			UpdateMotionIndicator(m_creatureIndicatorTransform, m_creatureTransform, m_creatureIndicatorAnimator);

			yield return new WaitForSeconds(1.05f * (1 / creatureIndicatorFlashSpeed));
		}
	}

	private IEnumerator FlashBeaconIndicator()
	{
		while (m_inGame)
		{
			if (Beacon.Instance != null)
				UpdateMotionIndicator(m_beaconIndicatorTransform, Beacon.Instance.transform, m_beaconIndicatorAnimator);

			yield return new WaitForSeconds(1.05f * (1 / m_beaconFlashSpeed));
		}
	}

	private void UpdateMotionIndicator(RectTransform indicatorTransform, Transform targetTransform, Animator animator)
	{
		Vector3 directionToTarget = m_playerTransform.worldToLocalMatrix.MultiplyPoint(targetTransform.position);

		float detectorMagnitude = Mathf.Clamp(directionToTarget.magnitude / m_maxIndicatedObjectDistance, 0, 1);

		directionToTarget.Normalize();
		directionToTarget *= detectorMagnitude * m_maxIndicatorScreenDistance;

		indicatorTransform.anchoredPosition = new Vector2(directionToTarget.x, directionToTarget.z) + s_motionIndicatorPositionOffset;

		animator.SetTrigger("Flash");
	}

	private void OnHuntingStateChanged(bool isHunting) => m_isHunting = isHunting;

	private void UpdateExitIndicator()
	{
		Vector3 directionToExit = m_playerTransform.worldToLocalMatrix.MultiplyPoint(m_exitTransform.position).normalized;

		Vector3 currentEulerAngles = m_exitIndicatorTransform.rotation.eulerAngles;
		m_exitIndicatorTransform.rotation = Quaternion.Euler(currentEulerAngles.x, currentEulerAngles.y, (Mathf.Rad2Deg * Mathf.Atan2(directionToExit.z, directionToExit.x)) - 90f);
	}

	#endregion
}
