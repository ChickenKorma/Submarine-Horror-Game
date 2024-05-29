using UnityEngine;

public class Beacon : MonoBehaviour
{
	#region Variables

	public static Beacon Instance { get; private set; }

	[SerializeField] private GameObject m_pingEmitterPrefab;

	[SerializeField] private float m_pingSpeed;
	[SerializeField] private float m_pingMaxDistance;
	[SerializeField] private float m_pingSoundDuration;

	private bool m_pingEnabled;

	private Ping m_lastPing;

	#endregion

	#region Unity

	private void Awake()
	{
		if (Instance != null)
			Destroy(this);
		else
			Instance = this;
	}

	void Update()
	{
		if (!m_pingEnabled || (m_pingEnabled && m_lastPing == null))
		{
			m_pingEnabled = true;

			m_lastPing = Instantiate(m_pingEmitterPrefab, transform.position, Quaternion.identity).GetComponent<Ping>();
			m_lastPing.Setup("Beacon", m_pingSpeed, m_pingMaxDistance, m_pingSoundDuration);
		}
	}

	private void OnDestroy() => Destroy(m_lastPing);

	#endregion
}
