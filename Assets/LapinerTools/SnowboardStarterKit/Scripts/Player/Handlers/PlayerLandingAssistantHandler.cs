using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerLandingAssistantHandler
	{
		private const float ROT_CORRECTION_SPEED = 1.5f;
		
		private const int PREDICT_STEP_NUM = 4;
		private const float MAX_PREDICT_STEP_TIME = 0.25f;
		
		private PlayerController m_playerStrl;
		
		private Vector3 m_predictNormal = Vector3.zero;
		
		private bool m_isAssisting = false;
		public bool IsAssisting
		{
			get
			{
				return m_isAssisting;
			}
		}
		
		private float m_timeToLanding = 0.0f;
		public float TimeToLanding
		{
			get
			{
				return m_timeToLanding;
			}
		}
		
		public PlayerLandingAssistantHandler(PlayerController p_playerController )
		{
			m_playerStrl = p_playerController;
		}
		
		public void OnUpdate()
		{
			m_isAssisting = false;
			switch (m_playerStrl.State)
			{
				case PlayerState.Ground:
				case PlayerState.TakeOff:
				case PlayerState.Locked:
				case PlayerState.Dead:
					// do nothing
					break;
				case PlayerState.Air:
				{
					// search for a landing normal
					if (predictLandingNormal())
					{
						// correct players rotation
						float rotAngle = Mathf.Rad2Deg*Mathf.Acos(Vector3.Dot(m_playerStrl.BordUpVector, m_predictNormal));
						Vector3 rotAxis = Vector3.Cross(m_playerStrl.BordUpVector, m_predictNormal).normalized;
						m_playerStrl.ApplyRotationAxis(rotAxis, rotAngle*ROT_CORRECTION_SPEED);
						m_isAssisting = true;
					}
					break;
				}
			}	
		}
		
		private bool predictLandingNormal()
		{
			m_timeToLanding = 0.0f;
			Vector3 currPos = m_playerStrl.Position;
			Vector3 currVelocity = m_playerStrl.Velocity;
			for (int i=0; i<PREDICT_STEP_NUM; i++)
			{
				if (rayCastForLanding(currPos, currVelocity))
				{
					m_timeToLanding += i * MAX_PREDICT_STEP_TIME;
					return true;
				}
				else
				{
					// simulate physics for MAX_PREDICT_STEP_TIME and try again
					currPos += currVelocity*MAX_PREDICT_STEP_TIME;
					currVelocity += PlayerPhysicsHandler.GetVChangeDueToGravity(MAX_PREDICT_STEP_TIME);
				}
			}
			
			return false;
		}
		
		private bool rayCastForLanding(Vector3 pPos, Vector3 pVelocity)
		{
			float maxDist = pVelocity.magnitude * MAX_PREDICT_STEP_TIME;
			RaycastHit rayHit;
			if (Physics.Raycast(pPos, pVelocity.normalized, out rayHit, maxDist))
			{
				m_timeToLanding = rayHit.distance / pVelocity.magnitude;
				m_predictNormal = rayHit.normal;
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
