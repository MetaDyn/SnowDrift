using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Snowboard
{
	public class PlayerScoreTrickSpeed : IPlayerScoreTrick
	{
		// SCORING CONSTANTS
		private const float GROUND_MIN_TIME = 1.25f;
		
		private List<float> m_speedLimits = new List<float>();
		private List<string> m_speedLimitNames = new List<string>();
		private List<int> m_speedLimitScores = new List<int>();
		
		private PlayerController m_playerStrl;
		private float m_groundTime = 0.0f;
		private int m_trickScore = 0;
		private string m_trickName = "";
		
		public PlayerScoreTrickSpeed(PlayerController p_playerController)
		{
			m_playerStrl = p_playerController;
			m_speedLimits.Add(25f);
			m_speedLimitNames.Add("Going fast - 50km/h");
			m_speedLimitScores.Add(1500);
			m_speedLimits.Add(32.5f);
			m_speedLimitNames.Add("Are you nuts!? - 100km/h");
			m_speedLimitScores.Add(2500);
			m_speedLimits.Add(40f);
			m_speedLimitNames.Add("!!! o.O AAAAAAAAAAAAH O.o !!! - 150km/h");
			m_speedLimitScores.Add(4250);
		}
		
		public override void OnUpdate()
		{
			m_trickScore = 0;
			m_trickName = "";
			
			if (m_speedLimits.Count == 0)
			{
				return;
			}
			
			if (m_playerStrl.State == PlayerState.Ground)
			{
				m_groundTime += Time.deltaTime;
				
				if (m_groundTime > GROUND_MIN_TIME)
				{
					m_groundTime = 0f;
					
					if (m_speedLimits[0] <= m_playerStrl.Velocity.magnitude)
					{
						m_trickName = m_speedLimitNames[0];
						m_trickScore = m_speedLimitScores[0];
						m_speedLimits.RemoveAt(0);
						m_speedLimitNames.RemoveAt(0);
						m_speedLimitScores.RemoveAt(0);
					}
				}
			}
			else
			{
				m_groundTime = 0f;
			}
		}
		
		public override void EndLevel()
		{
		}
		
		public override int Score
		{
			get
			{
				return m_trickScore;
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
	}
}
