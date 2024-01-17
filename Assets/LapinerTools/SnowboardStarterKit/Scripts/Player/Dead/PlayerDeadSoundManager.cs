using UnityEngine;
using System.Collections;

namespace Snowboard
{
	[RequireComponent (typeof (AudioSource))]
	public class PlayerDeadSoundManager : MonoBehaviour
	{
		public AudioClip[] HitSounds;
		public AudioClip[] PainSounds;
		public AudioClip[] SplatterSounds;
		public AudioClip[] KeyDeathSounds;
		
		private const int MAX_PAIN_SOUNDS = 2; // maximal count of pain sounds
		private const float MIN_SOUND_PAUSE = 0.4f; // minimal time between sounds in seconds
		
		private int m_painSoundCount = 0;
		private float m_lastSoundPlayTime = 0;
		private bool m_isDeathByKey = false;
		
		public void SetDeathByKey()
		{
			m_isDeathByKey = true;
		}
		
		private void Start()
		{
			if (HitSounds == null || HitSounds.Length < 1 ||
				PainSounds == null || PainSounds.Length < 1 ||
				SplatterSounds == null || SplatterSounds.Length < 1 ||
				KeyDeathSounds == null || KeyDeathSounds.Length < 1)
			{
				Debug.LogError("PlayerDeadSoundManager: sounds are not set properly!");
			}
			
			m_lastSoundPlayTime = Time.timeScale - 0.5f * MIN_SOUND_PAUSE;
			if (m_isDeathByKey && KeyDeathSounds.Length > 0)
			{
				GetComponent<AudioSource>().PlayOneShot(KeyDeathSounds[Random.Range(0, KeyDeathSounds.Length)], Random.Range(0.5f, 1.0f)); // player death sound
			}
			else if (PainSounds.Length > 0)
			{
				GetComponent<AudioSource>().PlayOneShot(PainSounds[Random.Range(0, PainSounds.Length)], Random.Range(0.5f, 1.0f)); // player death sound
			}
		}
		
		private void Update()
		{
			GetComponent<AudioSource>().pitch = Time.timeScale;
		}
		
		private void OnPlayerHitHard()
		{
			if (HitSounds.Length > 0 && m_lastSoundPlayTime + MIN_SOUND_PAUSE < Time.time)
			{
				m_lastSoundPlayTime = Time.time;

				GetComponent<AudioSource>().PlayOneShot(HitSounds[Random.Range(0, HitSounds.Length)]);
				GetComponent<AudioSource>().PlayOneShot(SplatterSounds[Random.Range(0, SplatterSounds.Length)]);
				m_painSoundCount = MAX_PAIN_SOUNDS; // no pain sounds any more player is probably dead
			}
		}
		
		private void OnPlayerHitMessage(float pFactor)
		{
			if (m_lastSoundPlayTime + MIN_SOUND_PAUSE < Time.time)
			{
				if (HitSounds.Length > 0)
				{
					GetComponent<AudioSource>().PlayOneShot(HitSounds[Random.Range(0, HitSounds.Length)], pFactor);
				}
				
				if (PainSounds.Length > 0 && (m_painSoundCount == 0 || (pFactor > 0.25f && m_painSoundCount < MAX_PAIN_SOUNDS)))
				{
					GetComponent<AudioSource>().PlayOneShot(PainSounds[Random.Range(0, PainSounds.Length)], pFactor);
					m_painSoundCount++;
				}
				
				m_lastSoundPlayTime = Time.time;
			}
		}
	}
}
