using System;
using UnityEngine;

public class Sonar : MonoBehaviour
{
	#region Variables

	public static Sonar Instance { get; private set; }

	public Action<bool> PingStateChanged = delegate { };

	[SerializeField] private GameObject m_pingEmitterPrefab;

	[SerializeField] private float m_pingSpeed;
	[SerializeField] private float m_pingMaxDistance;
	[SerializeField] private float m_pingHold;
	[SerializeField] private float m_pingSoundDuration;

	private bool m_pingEnabled;

	private float m_lastPingInput = -10000;

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

	private void OnEnable() => InputManager.Instance.PingEvent += Ping;

	private void OnDisable() => InputManager.Instance.PingEvent -= Ping;

	void Update()
	{
		if (m_pingEnabled && m_lastPing == null)
		{
			m_pingEnabled = false;
			PingStateChanged.Invoke(false);
		}
		else if (Time.time <= m_lastPingInput + m_pingHold)
			Ping();
	}

	#endregion

	#region Sonar

	private void Ping()
	{
		if (!m_pingEnabled)
		{
			m_pingEnabled = true;
			PingStateChanged.Invoke(true);

			m_lastPing = Instantiate(m_pingEmitterPrefab, transform.position, Quaternion.identity).GetComponent<Ping>();
			m_lastPing.Setup("Player", m_pingSpeed, m_pingMaxDistance, m_pingSoundDuration);
		}
		else
			m_lastPingInput = Time.time;
	}

	#endregion
}
