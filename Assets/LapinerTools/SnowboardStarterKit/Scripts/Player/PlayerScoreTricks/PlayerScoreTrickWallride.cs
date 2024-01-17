using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerScoreTrickWallride : IPlayerScoreTrick
	{
		// SCORING CONSTANTS
		private const float TIME_MULTIPLIER = 300.0f;
		private const float SPEED_MULTIPLIER = 0.25f;
		private const float MIN_SPEED = 8.5f;
		
		private PlayerController m_playerCtrl;
		private float m_wallTime = 0.0f;
		private float m_trickScore = 0.0f;
		private float m_scoreSoFar = 0.0f;
		private string m_trickName = "";
		
		public PlayerScoreTrickWallride(PlayerController p_playerController)
		{
			m_playerCtrl = p_playerController;
		}
		
		public override void OnUpdate()
		{
			m_trickScore = 0.0f;
			m_trickName = "";

			float cosUp = Mathf.Abs(Vector3.Dot(m_playerCtrl.BordUpVector, Vector3.up));
			if (m_playerCtrl.State == PlayerState.Ground &&
			    (!m_playerCtrl.IsCollidingWithTerrain || m_playerCtrl.IsOnAWall) &&
			    cosUp < 0.7)
			{
				if (m_playerCtrl.Velocity.magnitude > MIN_SPEED)
				{
					m_wallTime += Time.deltaTime;
					m_scoreSoFar += Time.deltaTime * Mathf.Min(15f, m_playerCtrl.Velocity.magnitude * SPEED_MULTIPLIER);
				}
			}
			else
			{
				if (m_wallTime > 0.25f)
				{
					countWallTime(m_wallTime);
				}
				m_wallTime = 0.0f;
				m_scoreSoFar = 0.0f;
			}
		}
		
		public override void EndLevel()
		{
			if (m_wallTime > 0.25f)
			{
				countWallTime(m_wallTime);
			}
			m_wallTime = 0.0f;
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
		
		private void countWallTime(float pWallTime)
		{
			m_trickScore = m_scoreSoFar * TIME_MULTIPLIER * pWallTime;
			m_trickName = "Wallride";
		}
	}
}
