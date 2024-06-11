using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	#region Variables

	public static AudioManager Instance { get; private set; }

	public AudioMixer MainAudioMixer;

	// Submarine
	[Header("Submarine Sources")]
	[SerializeField] private AudioSource m_submarineMovingAudioSource;
	[SerializeField] private AudioSource m_submarineRotatingAudioSource;
	[SerializeField] private AudioSource m_beaconReleaseAudioSource;
	[SerializeField] private AudioSource m_submarineSonarPingAudioSource;
	[SerializeField] private AudioSource m_collisionAudioSource;
	[SerializeField] private AudioSource m_creakingAudioSource;

	[HideInInspector] public AudioSource BeaconSonarPingAudioSource;

	[Header("UI Sources")]
	[SerializeField] private AudioSource m_beepAudioSource;

	[Header("Audio Clips")]
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

	public void SetSubmarineMoving(bool moving) => SetSound(m_submarineMovingAudioSource, moving);

	public void SetSubmarineRotating(bool rotating) => SetSound(m_submarineRotatingAudioSource, rotating);

	public void PlayBeaconRelease() => PlaySound(m_beaconReleaseAudioSource);

	public void PlaySubmarineSonarPing() => PlaySound(m_submarineSonarPingAudioSource, clips: m_sonarPingClips);

	public void PlaySubmarineCrash()
	{
		PlayCollision();
		PlayCreaking(0.8f);
	}

	private void PlayCollision() => PlaySound(m_collisionAudioSource, forcePlay: true, clips: m_collisionClips);

	private void PlayCreaking(float? delay = null) => PlaySound(m_creakingAudioSource, true, delay, m_creakingClips);

	#endregion

	#region Beacon sounds

	public void PlayBeaconSonarPing() => PlaySound(BeaconSonarPingAudioSource, clips: m_sonarPingClips);

	#endregion

	#region UI sounds

	public void PlayMotionDetectorBeep() => PlaySound(m_beepAudioSource);

	#endregion

	#region Helper methods

	private void PlaySound(AudioSource audioSource, bool forcePlay = false, float? delay = null, AudioClip[] clips = null)
	{
		if (forcePlay || (!forcePlay && !audioSource.isPlaying))
		{
			if (clips is not null)
				audioSource.clip = clips[Random.Range(0, clips.Length - 1)];

			if (delay is not null)
				audioSource.PlayDelayed((float)delay);
			else
			audioSource.Play();
		}
	}

	private void SetSound(AudioSource audioSource, bool playSound)
	{
		if (playSound)
			PlaySound(audioSource);
		else
			audioSource.Stop();
	}

	#endregion
}
