using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	#region Variables

	public static AudioManager Instance { get; private set; }

	public AudioMixer MainAudioMixer;

	// Submarine
	[SerializeField] private AudioSource m_submarineMovingAudioSource;
	[SerializeField] private AudioSource m_submarineRotatingAudioSource;
	[SerializeField] private AudioSource m_beaconReleaseAudioSource;
	[SerializeField] private AudioSource m_submarineSonarPingAudioSource;
	[SerializeField] private AudioSource m_collisionAudioSource;
	[SerializeField] private AudioSource m_creakingAudioSource;

	// Beacon
	[HideInInspector] public AudioSource BeaconSonarPingAudioSource;

	// UI
	[SerializeField] private AudioSource m_beepAudioSource;

	// Clips
	[SerializeField] private AudioClip[] m_sonarPingClips;
	[SerializeField] private AudioClip[] m_collisionClips;
	[SerializeField] private AudioClip[] m_creakingClips;

	#endregion

	#region Unity

	private void Awake()
	{
		if (Instance != null)
			Destroy(this);
		else
			Instance = this;
	}

	#endregion

	#region Submarine sounds

	public void SetSubmarineMoving(bool moving)
	{
		if (moving)
		{
			if (!m_submarineMovingAudioSource.isPlaying)
				m_submarineMovingAudioSource.Play();
		}
		else
			m_submarineMovingAudioSource.Stop();
	}

	public void SetSubmarineRotating(bool rotating)
	{
		if (rotating)
		{
			if (!m_submarineRotatingAudioSource.isPlaying)
				m_submarineRotatingAudioSource.Play();
		}
		else
			m_submarineRotatingAudioSource.Stop();
	}

	public void PlayBeaconRelease()
	{
		if (!m_beaconReleaseAudioSource.isPlaying)
			m_beaconReleaseAudioSource.Play();
	}

	public void PlaySubmarineSonarPing() => PlaySonarPing(m_submarineSonarPingAudioSource);

	public void PlaySubmarineCrash()
	{
		PlayCollision();
		PlayCreaking(0.5f);
	}

	private void PlayCollision()
	{
		if (!m_collisionAudioSource.isPlaying)
		{
			m_collisionAudioSource.clip = m_sonarPingClips[Random.Range(0, m_sonarPingClips.Length - 1)];
			m_collisionAudioSource.Play();
		}
	}

	private void PlayCreaking(float delay = 0)
	{
		if (!m_creakingAudioSource.isPlaying)
		{
			m_creakingAudioSource.clip = m_sonarPingClips[Random.Range(0, m_sonarPingClips.Length - 1)];
			m_creakingAudioSource.PlayDelayed(delay);
		}
	}

	#endregion

	#region Beacon sounds

	public void PlayBeaconSonarPing() => PlaySonarPing(BeaconSonarPingAudioSource);

	#endregion

	#region UI sounds

	public void PlayMotionDetectorBeep()
	{
		if (!m_beepAudioSource.isPlaying)
			m_beepAudioSource.Play();
	}

	#endregion

	#region Helper methods

	private void PlaySonarPing(AudioSource audioSource)
	{
		if (!audioSource.isPlaying)
		{
			audioSource.clip = m_sonarPingClips[Random.Range(0, m_sonarPingClips.Length - 1)];
			audioSource.Play();
		}
	}

	#endregion
}
