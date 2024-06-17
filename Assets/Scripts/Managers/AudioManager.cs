using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	#region Variables

	public static AudioManager Instance { get; private set; }

	public AudioMixer MainAudioMixer;

	[Header("Creature Sources")]
	[SerializeField] private AudioSource m_creatureMovingAudioSource;
	[SerializeField] private AudioSource m_creatureVocalAudioSource;
	[SerializeField] private AudioSource m_creatureAttackAudioSource;

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

	private bool m_isAttacking;

	[Header("Audio Clips")]
	[SerializeField] private AudioClip[] m_sonarPingClips;
	[SerializeField] private AudioClip[] m_collisionClips;
	[SerializeField] private AudioClip[] m_creakingClips;
	[SerializeField] private AudioClip[] m_creatureGrowlClips;
	[SerializeField] private AudioClip[] m_creatureRoarClips;

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

	#region Creature sounds

	public void SetCreatureMoving(bool moving) => SetSound(m_creatureMovingAudioSource, moving);

	public void PlayCreatureGrowl() => PlaySound(m_creatureVocalAudioSource, forcePlay: true, clips: m_creatureGrowlClips);

	public float PlayCreatureRoar() => PlaySound(m_creatureVocalAudioSource, forcePlay: true, clips: m_creatureRoarClips);

	public void CreatureAttackPlayer()
	{
		SetCreatureMoving(false);
		SetSubmarineMoving(false);
		SetSubmarineRotating(false);
		m_creatureVocalAudioSource.Stop();

		PlaySound(m_creatureAttackAudioSource, forcePlay: true);
		m_isAttacking = true;
	}

	public void CreatureAttackBeacon()
	{
		SetCreatureMoving(false);
		m_creatureVocalAudioSource.Stop();

		PlaySound(m_creatureAttackAudioSource, forcePlay: true);
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

	private float PlaySound(AudioSource audioSource, bool forcePlay = false, float? delay = null, AudioClip[] clips = null)
	{
		if (m_isAttacking)
			return 100f;

		if (forcePlay || (!forcePlay && !audioSource.isPlaying))
		{
			if (clips is not null)
				audioSource.clip = clips[Random.Range(0, clips.Length - 1)];

			if (delay is not null)
				audioSource.PlayDelayed((float)delay);
			else
				audioSource.Play();
		}

		return audioSource.clip.length;
	}

	private void SetSound(AudioSource audioSource, bool playSound)
	{
		if (m_isAttacking)
			return;

		if (playSound)
			PlaySound(audioSource);
		else
			audioSource.Stop();
	}

	#endregion
}
