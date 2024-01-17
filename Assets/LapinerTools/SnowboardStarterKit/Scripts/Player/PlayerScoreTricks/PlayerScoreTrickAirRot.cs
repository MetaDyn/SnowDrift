using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerScoreTrickAirRot : IPlayerScoreTrick
	{
		// SCORING CONSTANTS
		private const float GRAB_MULTIPLIER = 2.0f;
		private static string[] FLIP_NAMES = {"", "Double ", "Triple ", "Quad "};
		private const float ROT_TOLERANCE_SPIN = 80.0f;
		private const float ROT_TOLERANCE_FLIP = 155.0f;
		private const float SPIN_MULTIPLIER = 2.0f;
		private const float FLIP_MULTIPLIER = 4.0f;
		
		private PlayerController m_playerStrl;
		private float m_rotSpin = 0.0f;
		private float m_rotFlip = 0.0f;
		private float m_trickScore = 0.0f;
		private string m_trickName = "";
		private bool m_wasGrab = false;
		
		public PlayerScoreTrickAirRot(PlayerController p_playerController)
		{
			m_playerStrl = p_playerController;
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
					countRot(m_rotSpin, m_rotFlip);
					m_rotSpin = 0.0f;
					m_rotFlip = 0.0f;
					m_wasGrab = false;
				
					break;
				}
				case PlayerState.Air:
				{
					if (m_playerStrl.GrabState != PlayerGrabState.NO_GRAB)
					{
						m_wasGrab = true;
					}
					m_rotSpin += m_playerStrl.FrameRotY;
					m_rotFlip += m_playerStrl.FrameRotX;
					break;
				}
				case PlayerState.Dead:
				{
					m_rotSpin = 0.0f;
					m_rotFlip = 0.0f;
					m_wasGrab = false;
					break;
				}
			}
		}
		
		public override void EndLevel()
		{
			countRot(m_rotSpin, m_rotFlip);
			m_rotSpin = 0.0f;
			m_rotFlip = 0.0f;
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
		
		private void countRot(float pRotSpin, float pRotFlip)
		{
			float absRotSpin = Mathf.Abs(pRotSpin);
			if (absRotSpin >= 180.0f - ROT_TOLERANCE_SPIN)
			{
				m_trickScore = SPIN_MULTIPLIER * absRotSpin;
				m_trickName = (int)((absRotSpin+ROT_TOLERANCE_SPIN) / 180.0f) * 180 + " Spin";
			}
			
			float absRotFlip = Mathf.Abs(pRotFlip);
			if (absRotFlip >= 360.0f - ROT_TOLERANCE_FLIP)
			{
				m_trickScore += FLIP_MULTIPLIER * absRotFlip;
				
				if (m_trickName.Length > 0)
				{
					m_trickName += " with a ";
				}
				int count = (int)((absRotFlip+ROT_TOLERANCE_FLIP) / 360.0f);
				
				if (count < 5)
				{
					m_trickName += FLIP_NAMES[count -1];
				}
				else
				{
					m_trickName += "STOP CHEATING ";
				}
				
				if (pRotFlip > 0)
				{
					m_trickName += "Frontflip";
				}
				else
				{
					m_trickName += "Backflip";
				}
			}
			
			if (m_wasGrab && !string.IsNullOrEmpty(m_trickName))
			{
				m_trickScore *= GRAB_MULTIPLIER;
				m_trickName = GRAB_PREFIX + m_trickName;
			}
		}
	}
}
