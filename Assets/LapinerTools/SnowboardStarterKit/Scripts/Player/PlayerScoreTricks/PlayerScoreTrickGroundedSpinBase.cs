using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public abstract class PlayerScoreTrickGroundedSpinBase : IPlayerScoreTrick
	{
		// SCORING CONSTANTS
		private const float ROT_TOLERANCE = 85.0f;
		private const float ROT_MIN_VALUE = 180.0f;
		private const float ROT_SLIDE_MULTIPLIER = 2.5f;
		
		protected PlayerController m_playerStrl;
		protected float m_rotSlide = 0.0f;
		protected float m_trickScore = 0.0f;
		protected string m_trickName = "";

		protected abstract bool IsValidPlayerState { get; }
		protected abstract string TrickNamePostFix { get; }

		public PlayerScoreTrickGroundedSpinBase(PlayerController p_playerController)
		{
			m_playerStrl = p_playerController;
		}
		
		public override void OnUpdate()
		{
			m_trickScore = 0.0f;
			m_trickName = "";

			if (m_playerStrl.State == PlayerState.Dead)
			{
				m_rotSlide = 0.0f;
			}
			else if (IsValidPlayerState)
			{
				float deltaRotY = m_playerStrl.FrameRotY;
				if (deltaRotY != 0.0f)
				{
					if ( deltaRotY * m_rotSlide >= 0.0f )
					{
						m_rotSlide += deltaRotY;
					}
					else
					{
						countRotSlide();
						m_rotSlide = 0.0f;
					}
				}
				else
				{
					countRotSlide();
					m_rotSlide = 0.0f;
				}
			}
			else
			{
				countRotSlide();
				m_rotSlide = 0.0f;
			}
		}
		
		public override void EndLevel()
		{
			countRotSlide();
			m_rotSlide = 0.0f;
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
				return "to";
			}
		}
		
		protected void countRotSlide()
		{
			float absRot = Mathf.Abs(m_rotSlide);
			if (absRot >= ROT_MIN_VALUE - ROT_TOLERANCE)
			{
				m_trickScore = ROT_SLIDE_MULTIPLIER * absRot;
				m_trickName = (int)((absRot+ROT_TOLERANCE) / 180.0f) * 180 + TrickNamePostFix;
			}
		}
	}
}
