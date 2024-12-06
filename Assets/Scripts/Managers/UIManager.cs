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
	[SerializeField] private GameObject m_pauseScreen;

	[Header("Sonar Use Indicator")]
	[SerializeField] private Color m_red;
	[SerializeField] private Color m_green;

	[SerializeField] private Image m_pingIndicatorPanel;
	[SerializeField] private TMP_Text m_pingIndicatorText;

	[Header("Beacon Use Indicator")]
	[SerializeField] private Slider m_beaconInputSlider;

	[Header("Motion Detector")]
	[SerializeField] private Transform m_creatureTransform;
	[SerializeField] private Transform m_playerTransform;
	[SerializeField] private Transform m_exitTransform;

	private Vector3 m_creaturePosition;

	[SerializeField] private RectTransform m_creatureIndicatorTransform;
	[SerializeField] private RectTransform m_beaconIndicatorTransform;
	private Animator m_creatureIndicatorAnimator;
	private Animator m_beaconIndicatorAnimator;

	[SerializeField] private float m_maxIndicatedObjectDistance;
	[SerializeField] private float m_maxIndicatorScreenDistance;

	[SerializeField] private float m_creatureMinFlashSpeed;
	[SerializeField] private float m_creatureMaxFlashSpeed;
	[SerializeField] private float m_beaconFlashSpeed;

	[SerializeField] private RectTransform m_exitIndicatorTransform;

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
		UpdateBeaconIndicator(0);

		StartCoroutine(FlashCreatureIndicator());
		StartCoroutine(FlashBeaconIndicator());
	}

	private void Update()
	{
		UpdateExitIndicator();

		UpdateMotionIndicator(m_creatureIndicatorTransform, m_creaturePosition);

		if (Beacon.Instance != null)
			UpdateMotionIndicator(m_beaconIndicatorTransform, Beacon.Instance.transform.position);
	}

	private void OnEnable()
	{
		ExitDetection.Instance.GameWon += GameWon;

		CreatureBehaviour.Instance.AttackedPlayer += GameOver;

		Sonar.Instance.PingStateChanged += UpdateSonarIndicator;
		Sonar.Instance.BeaconInputHold += UpdateBeaconIndicator;

		InputManager.Instance.PauseEvent += TogglePauseScreen;
	}

	private void OnDisable()
	{
		ExitDetection.Instance.GameWon -= GameWon;

		CreatureBehaviour.Instance.AttackedPlayer -= GameOver;

		Sonar.Instance.PingStateChanged -= UpdateSonarIndicator;
		Sonar.Instance.BeaconInputHold -= UpdateBeaconIndicator;

		InputManager.Instance.PauseEvent -= TogglePauseScreen;
	}

	#endregion

	#region Game States

	private void GameOver()
	{
		m_gameOverScreen.SetActive(true);
	}

	private void GameWon()
	{
		m_gameWonScreen.SetActive(true);
	}

	public void TogglePauseScreen()
	{
		m_pauseScreen.SetActive(GameManager.Instance.IsPaused);
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
			float creatureIndicatorFlashSpeed = m_creatureMaxFlashSpeed - (CreatureBehaviour.Instance.TotalVolumeFactor * (m_creatureMaxFlashSpeed - m_creatureMinFlashSpeed));
			m_creatureIndicatorAnimator.speed = creatureIndicatorFlashSpeed;

			m_creaturePosition = m_creatureTransform.position;

			m_creatureIndicatorAnimator.SetTrigger("Flash");
			AudioManager.Instance.PlayMotionDetectorBeep();

			yield return new WaitForSeconds(1.05f * (1 / creatureIndicatorFlashSpeed));
		}
	}

	private IEnumerator FlashBeaconIndicator()
	{
		while (m_inGame)
		{
			if (Beacon.Instance != null)
				m_beaconIndicatorAnimator.SetTrigger("Flash");

			yield return new WaitForSeconds(1.05f * (1 / m_beaconFlashSpeed));
		}
	}

	private void UpdateMotionIndicator(RectTransform indicatorTransform, Vector3 targetPosition)
	{
		Vector3 directionToTarget = m_playerTransform.worldToLocalMatrix.MultiplyPoint(targetPosition);

		float detectorMagnitude = Mathf.Clamp(directionToTarget.magnitude / m_maxIndicatedObjectDistance, 0, 1);

		directionToTarget.Normalize();
		directionToTarget *= detectorMagnitude * m_maxIndicatorScreenDistance;

		indicatorTransform.anchoredPosition = new Vector2(directionToTarget.x, directionToTarget.z) + s_motionIndicatorPositionOffset;
	}

	private void UpdateExitIndicator()
	{
		Vector3 directionToExit = m_playerTransform.worldToLocalMatrix.MultiplyPoint(m_exitTransform.position).normalized;

		Vector3 currentEulerAngles = m_exitIndicatorTransform.rotation.eulerAngles;
		m_exitIndicatorTransform.rotation = Quaternion.Euler(currentEulerAngles.x, currentEulerAngles.y, (Mathf.Rad2Deg * Mathf.Atan2(directionToExit.z, directionToExit.x)) - 90f);
	}

	#endregion
}
