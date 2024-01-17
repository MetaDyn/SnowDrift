using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerSnowManager : MonoBehaviour
	{
		private const float MIN_SPEED_SNOW_PARTICLES = 5; // straight snow particles start to emit at this speed
		private const float MAX_SPEED_SNOW_PARTICLES = 25; // straight snow particles emit with maximal amount with this speed
		private const float MAX_SNOW_PARTICLES_EMISSION = 15; // straight snow particles emit with maximal amount with this speed

		private const float MIN_SPEED_SNOW_SIDE_PARTICLES = 2; // side snow particles start to emit at this speed
		private const float MAX_SPEED_SNOW_SIDE_PARTICLES = 15; // side snow particles emit with maximal amount with this speed
		private const float MAX_SNOW_SIDE_PARTICLES_START_SPEED = 10; // side snow particles emit with maximal amount with this speed

		[SerializeField]
		private ParticleSystem m_snowPartSys;
		[SerializeField]
		private ParticleSystem m_snowSideBackPartSys;
		[SerializeField]
		private ParticleSystem m_snowSideFrontPartSys;

		private PlayerSnowTrailUpdater m_trail;

		private Vector3 m_lastPosition = Vector3.zero;
		private Vector3 m_v = Vector3.zero;
		private Vector3 m_a = Vector3.zero;
		private float m_straightV = 0;
		private float m_straightA = 0;
		private float m_sideV = 0;
		private float m_sideA = 0;

		private bool m_isSideParticleUpdate = true;

		private void Start()
		{
			if (m_snowPartSys == null || m_snowSideBackPartSys == null || m_snowSideFrontPartSys == null)
			{
				Debug.LogError("PlayerSnowManager: one or more particle systems are not initialized!");
				Destroy(this);
			}
			m_trail = transform.parent.GetComponentInChildren<PlayerSnowTrailUpdater>();
			if (m_trail == null)
			{
				Debug.LogError("PlayerSnowManager: could not find SnowboardMeshTrailUpdater!");
				Destroy(this);
			}
		}

		private void FixedUpdate()
		{
			if (m_lastPosition == Vector3.zero)
			{
				m_lastPosition = transform.position;
			}
			else
			{
				m_v = Vector3.SmoothDamp(m_v, (transform.position-m_lastPosition) / Time.deltaTime, ref m_a, 0.25f);
				m_lastPosition = transform.position;
			}
		}

		private void Update ()
		{
			if (m_trail.IsDrawing)
			{
				m_sideV = Vector3.Dot(m_v, transform.forward);
				m_straightV = m_v.magnitude+m_sideV;
			}
			else
			{
				m_sideV = Mathf.SmoothDamp(m_sideV, 0f, ref m_sideA, 0.25f);
				m_straightV = Mathf.SmoothDamp(m_straightV, 0f, ref m_straightA, 0.25f);
			}
			HandleParticleStraight(m_snowPartSys, m_straightV, MIN_SPEED_SNOW_PARTICLES, MAX_SPEED_SNOW_PARTICLES, MAX_SNOW_PARTICLES_EMISSION);
			if (m_isSideParticleUpdate)
			{
				HandleParticleSide(m_snowSideBackPartSys, m_sideV, MIN_SPEED_SNOW_SIDE_PARTICLES, MAX_SPEED_SNOW_SIDE_PARTICLES, MAX_SNOW_SIDE_PARTICLES_START_SPEED);
				HandleParticleSide(m_snowSideFrontPartSys, -m_sideV, MIN_SPEED_SNOW_SIDE_PARTICLES, MAX_SPEED_SNOW_SIDE_PARTICLES, MAX_SNOW_SIDE_PARTICLES_START_SPEED);
			}
		}

		private static void HandleParticleStraight(ParticleSystem p_partSys, float p_v, float p_vMin, float p_vMax, float p_emissionMax)
		{
			if (p_partSys != null)
			{
				ParticleSystem.EmissionModule emission = p_partSys.emission;
				if (p_v > p_vMin)
				{
					emission.rateOverTime = p_emissionMax * Mathf.Clamp01((p_v-p_vMin) / p_vMax);
					if (!p_partSys.isPlaying)
					{
						p_partSys.Play();
					}
				}
				else if (p_partSys.isPlaying)
				{
					emission.rateOverTime = 0;
					p_partSys.Stop();
				}
			}
		}

		private static void HandleParticleSide(ParticleSystem p_partSys, float p_v, float p_vMin, float p_vMax, float p_startSpeedMax)
		{
			if (p_partSys != null)
			{
				ParticleSystem.MainModule partSysMain = p_partSys.main;
				if (p_v > p_vMin)
				{
					float factor = Mathf.Clamp01((p_v-p_vMin) / p_vMax);
					partSysMain.startSpeed = p_startSpeedMax * factor;
					partSysMain.startColor = new Color(1f,1f,1f,0.05f+0.5f*(factor*factor));
					if (!p_partSys.isPlaying)
					{
						p_partSys.Play();
					}
				}
				else if (p_partSys.isPlaying)
				{
					partSysMain.startSpeed = 0;
					p_partSys.Stop();
				}
			}
		}
	}
}
