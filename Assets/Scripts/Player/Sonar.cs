using Creature;
using Managers;
using System;
using UnityEngine;

namespace Player
{
	public class Sonar : MonoBehaviour
	{
		#region Variables

		public static Sonar Instance { get; private set; }

		public Action<bool> PingStateChanged = delegate { };
		public Action<float> BeaconInputHold = delegate { };
		public Action<int> BeaconsRemainingChanged = delegate { };

		[SerializeField] private GameObject m_pingEmitterPrefab;
		[SerializeField] private GameObject m_beaconPrefab;

		[SerializeField] private float m_pingSpeed;
		[SerializeField] private float m_pingMaxDistance;
		[SerializeField] private float m_pingHold;
		[SerializeField] private float m_pingVolume;

		private bool m_pingEnabled;

		private bool m_beaconInputHeld;
		[SerializeField] private float m_beaconInputHoldSpeed;
		private float m_beaconInputHoldProgress;

		private float m_lastPingInput = -10000;

		private Ping m_lastPing;

		private int m_beaconsRemaining = 5;

		#endregion

		#region Unity

		private void Awake()
		{
			if (Instance != null)
				Destroy(this);
			else
				Instance = this;
		}

		private void OnEnable()
		{
			InputManager.Instance.PingEvent += Ping;
			InputManager.Instance.BeaconHoldEvent += BeaconInputHeld;
		}

		private void OnDisable()
		{
			InputManager.Instance.PingEvent -= Ping;
			InputManager.Instance.BeaconHoldEvent -= BeaconInputHeld;
		}

		void Update()
		{
			if (m_pingEnabled && m_lastPing == null)
			{
				m_pingEnabled = false;
				PingStateChanged.Invoke(false);
			}
			else if (Time.time <= m_lastPingInput + m_pingHold)
				Ping();

			if (m_beaconInputHeld && m_beaconsRemaining > 0 && Beacon.Instance == null)
			{
				m_beaconInputHoldProgress += m_beaconInputHoldSpeed * Time.deltaTime;
				BeaconInputHold.Invoke(m_beaconInputHoldProgress);

				if (m_beaconInputHoldProgress >= 1)
				{
					DropBeacon();

					m_beaconsRemaining--;
					BeaconsRemainingChanged.Invoke(m_beaconsRemaining);

					m_beaconInputHoldProgress = 0;
					BeaconInputHold.Invoke(m_beaconInputHoldProgress);
				}
			}
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
				m_lastPing.Setup("Player", m_pingSpeed, m_pingMaxDistance);
				CreatureBehaviour.Instance.AddSound(transform.position, m_pingVolume);

				AudioManager.Instance.PlaySubmarineSonarPing();
			}
			else
				m_lastPingInput = Time.time;
		}

		private void BeaconInputHeld(bool holding)
		{
			if (m_beaconInputHeld != holding)
			{
				m_beaconInputHeld = holding;
				m_beaconInputHoldProgress = 0;
				BeaconInputHold.Invoke(m_beaconInputHoldProgress);
			}
		}

		private void DropBeacon()
		{
			Instantiate(m_beaconPrefab, transform.position, Quaternion.identity);

			CreatureBehaviour.Instance.BeaconDropped();

			AudioManager.Instance.PlayBeaconRelease();
		}

		#endregion
	}
}
