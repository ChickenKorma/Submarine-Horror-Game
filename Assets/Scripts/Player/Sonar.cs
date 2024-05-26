using UnityEngine;

public class Sonar : MonoBehaviour
{
	#region Variables

	public static Sonar Instance { get; private set; }

	[SerializeField] private Material m_material;

	[SerializeField] private Transform m_pingLight;

	[SerializeField] private float m_pingSpeed;
	[SerializeField] private float m_pingMaxDistance;
	[SerializeField] private float m_pingHold;
	[SerializeField] private float m_pingSoundDuration;

	private Vector3 m_pingOrigin;
	private float m_pingDistance;

	private bool m_pingEnabled = false;

	private float m_lastPing = -10000;

	public bool PingEnabled { get => m_pingEnabled; }

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

	private void Start()
	{
		m_material.SetFloat("_PingMaxDistance", m_pingMaxDistance);
		m_material.SetFloat("_PingDistance", m_pingMaxDistance);
	}

	void Update()
	{
		if (m_pingEnabled)
		{
			if (m_pingDistance > m_pingMaxDistance)
				m_pingEnabled = false;
			else
			{
				m_pingDistance += m_pingSpeed * Time.deltaTime;
				m_material.SetFloat("_PingDistance", m_pingDistance);
			}
		}
		else if (Time.time <= m_lastPing + m_pingHold)
			Ping();
	}

	#endregion

	#region Sonar

	private void Ping()
	{
		if (!m_pingEnabled)
		{
			m_pingOrigin = transform.position;
			m_material.SetVector("_PingOrigin", m_pingOrigin);
			m_pingLight.position = m_pingOrigin;

			m_pingDistance = 0;
			m_material.SetFloat("_PingDistance", m_pingDistance);

			m_pingEnabled = true;

			CreatureBehaviour.Instance.AddSound(m_pingOrigin, 1, m_pingSoundDuration);
		}
		else
			m_lastPing = Time.time;
	}

	#endregion
}
