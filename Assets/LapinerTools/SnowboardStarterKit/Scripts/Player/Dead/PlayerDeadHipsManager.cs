using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerDeadHipsManager : MonoBehaviour
	{
		public bool m_sendFailLevelMessage = true;
		public EGameOverType m_gameOverType = EGameOverType.NONE;

		private const int MOMENT_OF_DEATH_FRAME_COUNT = 10;
		private const float TIME_TILL_GAME_OVER = 10f;
		private int m_frameCount = 0;

		private PlayerPhysicsHandler m_physicsHandler = null;

		private float m_deathTime;

		private float m_noMoveTime = 1.5f;
		private Vector3 m_lastFramePos = Vector3.zero;

		public void SetPlayerController(PlayerController p_playerCtrl)
		{
			m_physicsHandler = p_playerCtrl.PhysicsHandler;

			m_deathTime = Time.time;
			m_physicsHandler.HandlePhysicsAccuracyMomentOfDeath();
		}

		void Update()
		{
			if (m_sendFailLevelMessage)
			{
				if (m_noMoveTime <= 0 || Time.time - m_deathTime > TIME_TILL_GAME_OVER)
				{
					GameLevelHandler.Instance.GameOver(m_gameOverType);
					m_sendFailLevelMessage = false;
				}
			}
		}
		
		void FixedUpdate ()
		{
			// level end message
			if (m_sendFailLevelMessage)
			{
				if ((transform.position - m_lastFramePos).magnitude < 0.01f)
				{
					m_noMoveTime -= Time.deltaTime;
				}
				m_lastFramePos = transform.position;
			}

			// physics accuracy
			if (m_frameCount <= MOMENT_OF_DEATH_FRAME_COUNT)
			{
				// increase physics detail for a bunch of frames
				m_frameCount++;
				m_physicsHandler.HandlePhysicsAccuracyMomentOfDeath();
			}
			else
			{
				// normal physics detail
				m_physicsHandler.HandlePhysicsAccuracy(GetComponent<Rigidbody>().velocity.magnitude);
			}
		}
	}
}
