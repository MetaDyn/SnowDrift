using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerScoreTrickAir : IPlayerScoreTrick
	{
		// SCORING CONSTANTS
		private const float GRAB_MULTIPLIER = 2.0f;
		private const float BIG_AIR_MIN_TIME = 2.0f;
		private const float BIG_AIR_TIME_MULTIPLIER = 100.0f;
		private const float HUGE_AIR_MIN_TIME = 3.0f;
		private const float HUGE_AIR_TIME_MULTIPLIER = 400.0f;
		private const float REDBULL_AIR_MIN_TIME = 4.0f;
		private const float REDBULL_AIR_TIME_MULTIPLIER = 800.0f;
		
		private PlayerController m_playerStrl;
		private float m_airTime = 0.0f;
		private float m_trickScore = 0.0f;
		private string m_trickName = "";
		private bool m_wasGrab = false;
		
		public PlayerScoreTrickAir(PlayerController p_playerController)
		{
			m_playerStrl = p_playerController;
		}
		
		public float AirTime
		{
			get
			{
				return m_airTime;
			}
		}
		
		public override void OnUpdate()
		{
			m_trickScore = 0.0f;
			m_trickName = "";
			
			switch (m_playerStrl.State)
			{
				case PlayerState.Locked:
				case PlayerState.Ground:
				case PlayerState.TakeOff:
				{
					if (m_airTime > 0.0f)
					{
						countAirTime(m_airTime);
					}
					m_airTime = 0.0f;
					m_wasGrab = false;
					
					break;
				}
				case PlayerState.Air:
				{
					if (m_playerStrl.GrabState != PlayerGrabState.NO_GRAB)
					{
						m_wasGrab = true;
					}
					m_airTime += Time.deltaTime;
					break;
				}
				case PlayerState.Dead:
				{
					m_airTime = 0.0f;
					m_wasGrab = false;
					break;
				}
			}
		}
		
		public override void EndLevel()
		{
			if (m_airTime > 0.0f)
			{
				countAirTime(m_airTime);
			}
			m_airTime = 0.0f;
		}
		
		public override int Score
		{
			get
			{
				return (int)(m_trickScore + 0.5f);
			}
		}
		
		public override string Name
		{
			get
			{
				return m_trickName;
			}
		}
		
		public override string Connective
		{
			get
			{
				return "+";
			}
		}
		
		private void countAirTime(float pAirTime)
		{
			if (pAirTime >= REDBULL_AIR_MIN_TIME)
			{
				m_trickScore = REDBULL_AIR_TIME_MULTIPLIER * pAirTime;
				m_trickName = "RedBull Jump";
			}
			else if (pAirTime >= HUGE_AIR_MIN_TIME)
			{
				m_trickScore = HUGE_AIR_TIME_MULTIPLIER * pAirTime;
				m_trickName = "Huge Jump";
			}
			else if (pAirTime >= BIG_AIR_MIN_TIME)
			{
				m_trickScore = BIG_AIR_TIME_MULTIPLIER * pAirTime;
				m_trickName = "Big Jump";
			}
			if (m_wasGrab && !string.IsNullOrEmpty(m_trickName))
			{
				m_trickScore *= GRAB_MULTIPLIER;
				m_trickName = GRAB_PREFIX + m_trickName;
			}
		}
	}
}
