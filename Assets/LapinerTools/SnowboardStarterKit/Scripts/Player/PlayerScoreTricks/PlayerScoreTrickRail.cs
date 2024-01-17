using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerScoreTrickRail : PlayerScoreTrickGroundedSpinBase
	{
		// SCORING CONSTANTS
		private const float TAP_SCORE = 50.0f;
		private const float NORMAL_GRIND_MIN_TIME = 0.5f;
		private const float NORMAL_TIME_MULTIPLIER = 200.0f;
		private const float PERFECT_GRIND_MIN_TIME = 1.5f;
		private const float PERFECT_GRIND_TIME_MULTIPLIER = 400.0f;
		private const float SPEED_MULTIPLIER = 0.5f;
		private const float SPEED_MULTIPLIER_SPIN = 0.15f;

		private float m_lockTime = 0.0f;
		private float m_trickScoreGrind = 0.0f;
		private float m_scoreSoFar = 0.0f;
		private string m_trickNameGrind = "";

		private bool m_isShiftySpin = false;
		private bool m_isShiftySpinBuffer = false;

		protected override bool IsValidPlayerState { get{ return m_playerStrl.State == PlayerState.Locked; } }
		protected override string TrickNamePostFix { get{ return " Spin"; } }

		public PlayerScoreTrickRail(PlayerController p_playerController)
			: base(p_playerController)
		{
		}
		
		public override void OnUpdate()
		{
			m_isShiftySpinBuffer = m_isShiftySpin;
			m_trickScoreGrind = 0.0f;
			m_trickNameGrind = "";
			
			switch (m_playerStrl.State)
			{
				case PlayerState.Locked:
				{
					float v = m_playerStrl.Velocity.magnitude;
					// rotation amount
					float deltaRotY = m_playerStrl.FrameRotY;
					if (deltaRotY != 0.0f)
					{
						if (m_isShiftySpin)
						{
							m_rotSlide += Mathf.Abs(deltaRotY) * v * SPEED_MULTIPLIER_SPIN;
						}
						else
						{
							m_rotSlide += deltaRotY * v * SPEED_MULTIPLIER_SPIN;
						}
						if ( deltaRotY * m_rotSlide < 0.0f )
						{
							m_isShiftySpin = true;
							m_isShiftySpinBuffer = true;
							m_rotSlide = Mathf.Abs(m_rotSlide);
						}
					}

					// grind time
					m_lockTime += Time.deltaTime;
				m_scoreSoFar += Time.deltaTime * v * SPEED_MULTIPLIER;
				
					break;
				}
				case PlayerState.Air:
				{
					// just do nothing and wait
					// do not call base.OnUpdate(); so that rotation is not reseted
					break;
				}
				case PlayerState.TakeOff:
				case PlayerState.Ground:
				case PlayerState.Dead:
				{
					countLockTime();
					base.OnUpdate();

					m_isShiftySpin = false;
					m_lockTime = 0.0f;
					m_scoreSoFar = 0.0f;
					break;
				}
			}
		}
		
		public override void EndLevel()
		{
			base.EndLevel();

			countLockTime();
			m_isShiftySpin = false;
			m_lockTime = 0.0f;
			m_scoreSoFar = 0.0f;
		}
		
		public override int Score
		{
			get
			{
				if (m_trickScoreGrind > 0.5f)
				{
					return (int)(m_trickScoreGrind + 0.5f) + base.Score;
				}
				else
				{
					return 0;
				}
			}
		}
		
		public override string Name
		{
			get
			{
				if (string.IsNullOrEmpty(m_trickNameGrind) || string.IsNullOrEmpty(base.m_trickName))
				{
					return m_trickNameGrind;
				}
				else if (m_isShiftySpinBuffer)
				{
					return m_trickNameGrind + " with shifted spin";
				}
				else
				{
					return m_trickNameGrind + " with " + base.Name;
				}
			}
		}
		
		public override string Connective
		{
			get
			{
				return "to";
			}
		}
		
		private void countLockTime()
		{
			if (m_scoreSoFar > 0.0f)
			{
				if (m_lockTime >= PERFECT_GRIND_MIN_TIME)
				{
					m_trickScoreGrind = m_scoreSoFar * PERFECT_GRIND_TIME_MULTIPLIER;
					m_trickNameGrind = "Perfect Grind";
				}
				else if (m_lockTime >= NORMAL_GRIND_MIN_TIME)
				{
					m_trickScoreGrind = m_scoreSoFar * NORMAL_TIME_MULTIPLIER;
					m_trickNameGrind = "Grind";
				}
				else
				{
					m_trickScoreGrind = m_scoreSoFar * TAP_SCORE;
					m_trickNameGrind = "Tap";
				}
			}
		}
	}
}
