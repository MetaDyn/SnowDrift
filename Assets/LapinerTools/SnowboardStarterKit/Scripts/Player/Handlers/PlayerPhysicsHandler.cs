using UnityEngine;
using System;

namespace Snowboard
{
	public class PlayerPhysicsHandler
	{
		// Physic Constants
		// Ground
		private const float FRICTION_FACTOR_CENTER = 5f; // maximul velocity lost per second
		private const float FRICTION_FACTOR_NORMAL = 13f; // maximul velocity lost per second
		private const float FRICTION_FACTOR_ONEDGE = 24f; // maximul velocity lost per second

		private const float LATERAL_V_SMOTHER = 2.0f; // from that velocity on we break less
		// Skid Marks
		private const float MIN_SKID_V_ANGLE = 0.85f;
		private const float MIN_SKID_LATERAL_V = 2f;//0.5f * (FRICTION_FACTOR_NORMAL + FRICTION_FACTOR_CENTER);
		private const float MAX_SKID_LATERAL_V = 6f;//FRICTION_FACTOR_ONEDGE;
		private const float HALF_WHEEL_BASE = 0.525f;
		private const float HALF_TRUCK_WIDTH = 0.125f;
		// Locked
		private const float MIN_LOCK_V = 4.0f; // if v in direction of lock target is lower then the lock is released
		private const float MIN_ALLOWED_V = 2.0f; // if v after lock precedure is lower then no lock is applied
		public const float MIN_LOCK_TARGET_DIST = 0.75f; // if lock target is nearer then the lock is released
		private const float LOCK_REDIRECT_V = 0.5f; // amount of velocity redirected in lock target direction
		
		public const float MIN_V = 0.5f;
		
		private Rigidbody m_cachedRigidBody;
		private Rigidbody m_cachedRigidBodyDamper;
		private PlayerController m_playerStrl;

		private int m_fixedFrameCounter = 0;
		private bool m_wasUpdate = false;
		private PlayerState m_bufferedPlayerState; 
		
		private bool m_isOffroad = false;
		public bool IsOffroad
		{
			get{ return m_isOffroad; }
			set{ m_isOffroad = value; }
		}
		
		private bool m_isOnAWall = false;
		public bool IsOnAWall { get{ return m_isOnAWall; } }
		
		private Vector3 m_lastSlideVLoss = Vector3.zero;
		public Vector3 LastSlideVLoss { get{ return m_lastSlideVLoss / (float)m_fixedFrameCounter; } }
		
		private float m_lastSlideV = 0f;
		public float LastSlideV { get{ return m_lastSlideV; } }

		private bool m_wasInAir = false;
		private Vector3 m_velocityLastFixedFrame = Vector3.zero;
		private float m_lastLandTime = 0f;
		public float LastLandTime { get{ return m_lastLandTime; } }
		private float m_lastLandVChange = 0f;
		public float LastLandVChange { get{ return m_lastLandVChange; } }

		public PlayerPhysicsHandler(PlayerController p_playerController, Rigidbody p_cachedRigidBody)
		{
			m_playerStrl = p_playerController;
			m_cachedRigidBody = p_cachedRigidBody;
			m_cachedRigidBodyDamper = p_playerController.damper.GetComponent<Rigidbody>();
			m_bufferedPlayerState = p_playerController.State;
		}
		
		public static Vector3 GetVChangeDueToGravity(float pDeltaTime)
		{
			return Physics.gravity * pDeltaTime;
		}
		
		public void OnUpdate()
		{
			if (m_wasUpdate == false)
			{
				m_bufferedPlayerState = m_playerStrl.State;
			}
			switch (m_playerStrl.State)
			{
				case PlayerState.Ground:
				case PlayerState.TakeOff:
					m_bufferedPlayerState = m_playerStrl.State;
					break;
				case PlayerState.Locked:
					m_bufferedPlayerState = m_playerStrl.State;
					break;
			}
			m_wasUpdate = true;
		}
		
		public void OnFixedUpdate()
		{
			if (m_wasUpdate)
			{
				m_fixedFrameCounter = 0;
				m_lastSlideV = 0.0f;
				m_lastSlideVLoss = Vector3.zero;
				m_wasUpdate = false;
			}
			m_fixedFrameCounter++;
			switch (m_bufferedPlayerState)
			{
				case PlayerState.Ground:
				{
					// DRAG
					float dragFadeFactor = 1.0f;
					float vLength = m_playerStrl.Velocity.magnitude;
					if (vLength < PlayerConfig.Instance.DRAG_MIN_SPEED)
					{
						dragFadeFactor = vLength / PlayerConfig.Instance.DRAG_MIN_SPEED;
						dragFadeFactor *= dragFadeFactor;
					}
					if (m_isOffroad)
					{
						dragFadeFactor *= PlayerConfig.Instance.DRAG_OFFROAD_FACTOR;
					}
					float drag = dragFadeFactor * PlayerConfig.Instance.DRAG_GROUND_MAX;
					m_cachedRigidBody.drag = drag;
					m_cachedRigidBodyDamper.drag = drag;

					m_isOnAWall = (m_playerStrl.BordUpVector - Vector3.up).sqrMagnitude >= 1.0f;
					if (m_playerStrl.BordUpVector.y > 0.0f && !m_isOnAWall)
					{
						// ACCELERATION
						if (m_playerStrl.IsAccelerating)
						{
							Vector3 dir = m_playerStrl.BordDirection;
							if (Vector3.Dot(dir, m_playerStrl.Velocity)<0)
							{
								dir *= -1;
							}
							dir *= PlayerConfig.Instance.ACCELERATION_FORCE;
							m_cachedRigidBody.AddForce(dir);
							m_cachedRigidBodyDamper.AddForce(dir);
						}
					
						// (LATERAL) FRICTION
						Vector3 lateralDirection = m_playerStrl.BordLateralDirection;
						float lateralV = Vector3.Dot(lateralDirection, m_playerStrl.Velocity);
						
						float lateralVSign = Mathf.Sign(lateralV);
						float vLossDueToFriction;
						if (m_playerStrl.BalanceState == PlayerBalanceState.Center)
						{
							vLossDueToFriction = FRICTION_FACTOR_CENTER * Time.fixedDeltaTime;
						}
						else if (m_playerStrl.BalanceState == PlayerBalanceState.Normal)
						{
							vLossDueToFriction = FRICTION_FACTOR_NORMAL * Time.fixedDeltaTime;
						}
						else 
						{
							vLossDueToFriction = FRICTION_FACTOR_ONEDGE * Time.fixedDeltaTime;
						}
						
						// make the end of the slide smoother (less breaking on low lateral velocity)
						if (lateralVSign * lateralV < LATERAL_V_SMOTHER)
						{
							vLossDueToFriction *= (lateralVSign * lateralV) / LATERAL_V_SMOTHER;
						}
						
						Vector3 vLoss;
						if (lateralVSign * lateralV < vLossDueToFriction)
						{
							// stop the slide -> carve
							vLoss = lateralDirection * lateralV ;
							m_lastSlideV = 0.0f;
							m_playerStrl.Velocity = m_playerStrl.Velocity - vLoss;
						}
						else
						{
							// apply some (lateral)friction
							vLoss = lateralVSign * lateralDirection * vLossDueToFriction;
							m_lastSlideV = lateralV - vLossDueToFriction;
							m_playerStrl.Velocity = m_playerStrl.Velocity - vLoss;
						}
						m_lastSlideVLoss += vLoss;
					
						// MINIMAL SPEED LIMIT
						float vDiv = MIN_V - vLength;
						if (vDiv > 0)
						{
							if (vLoss.magnitude <= vDiv)
							{
								m_playerStrl.Velocity = m_playerStrl.Velocity + vLoss * 0.99f;
							}
							else
							{
								Vector3 slideVAdd = vLoss;
								slideVAdd *= vDiv / slideVAdd.magnitude;
								m_playerStrl.Velocity = m_playerStrl.Velocity + slideVAdd * 0.99f;
							}
						}
					}
				}
				break;
				
				case PlayerState.Locked:
				{
					// RAIL/BOX LOCK
					Vector3 lockDist = m_playerStrl.LockedTarget - m_playerStrl.LastCollisionPoint;
					if (lockDist.magnitude > MIN_LOCK_TARGET_DIST)
					{
						LockOnRail();
					}
				}
				break;
				
				default:
				if (m_playerStrl.State != PlayerState.TakeOff)
				{
					// drag in air is constant
					m_cachedRigidBody.drag = PlayerConfig.Instance.DRAG_AIR;
					m_cachedRigidBodyDamper.drag = PlayerConfig.Instance.DRAG_AIR;
					// make player come down faster -> shorter jumps
					m_cachedRigidBody.AddForce(0f, PlayerConfig.Instance.ADDITIONAL_GRAVITY_IN_AIR, 0f, ForceMode.Acceleration);
					m_cachedRigidBodyDamper.AddForce(0f, PlayerConfig.Instance.ADDITIONAL_GRAVITY_IN_AIR, 0f, ForceMode.Acceleration);
				}
				break;
			}
			HandleLanding();
			m_velocityLastFixedFrame = m_cachedRigidBody.velocity;
		}
		public bool LockOnRail()
		{
			float lockVLength = Vector3.Dot(m_playerStrl.LockedDirection, m_playerStrl.Velocity);
			if (lockVLength > MIN_LOCK_V) // if we are going away from the lock target we have already passed it
			{
				Vector3 damperOffset = m_playerStrl.BordUpVector*m_playerStrl.damper.GetComponent<SphereCollider>().radius*0.99f;
				Vector3 damperLowestPos = m_playerStrl.damper.transform.position - damperOffset;
				Vector3 moveDist = m_playerStrl.LockedTarget-damperLowestPos;
				if (moveDist.magnitude > MIN_LOCK_TARGET_DIST)
				{
					Vector3 moveDir = moveDist.normalized;
					// correct players velocity, so that the player moves only in the direction allowed by the locker
					float allowedV = Vector3.Dot(moveDir, m_playerStrl.Velocity);
					if (allowedV > MIN_ALLOWED_V)
					{
						Vector3 newV = moveDir*allowedV;
						m_playerStrl.Velocity = newV;
						m_playerStrl.damper.GetComponent<Rigidbody>().velocity = newV;
						// correct players position, so that he smoothly moves over the locker
						Vector3 lockDir = m_playerStrl.LockedDirection;
						Vector3 allowedMove = lockDir * Vector3.Dot(lockDir, moveDist);
						Vector3 oldDamperPos = m_playerStrl.damper.transform.position;
						m_playerStrl.damper.transform.position = m_playerStrl.LockedTarget - allowedMove + damperOffset;
						Vector3 playerPos = m_playerStrl.transform.position;
						playerPos += (m_playerStrl.damper.transform.position - oldDamperPos)*0.5f;
						playerPos.y += (m_playerStrl.damper.transform.position.y - oldDamperPos.y)*0.25f;
						m_playerStrl.transform.position = playerPos;
					}
				}
				return true;
			}
			return false;
		}

		private void HandleLanding()
		{
			if (m_wasInAir && m_playerStrl.State != PlayerState.Air)
			{
				m_lastLandTime = Time.realtimeSinceStartup;
				Vector3 vChange = m_cachedRigidBody.velocity - m_velocityLastFixedFrame;
				m_lastLandVChange = Vector3.Dot(m_playerStrl.BordUpVector, vChange);
			}
			m_wasInAir = m_playerStrl.State == PlayerState.Air;
		}

		private const float MAX_PH_DELTA_TIME = 0.01421984f;
		private const float MIN_PH_DELTA_TIME = 0.005f;
		private const float DEATH_PH_DELTA_TIME = 0.0095f;
		private const float MIN_SOVER_PH_DELTA_INCREASE_SPEED = 20.0f;
		private const float PH_DELTA_SPEED_FACTOR = 0.001f;
		private const float PH_DELTA_MAX = 0.001f;

		public void HandlePhysicsAccuracyMomentOfDeath()
		{
			FixedTimeHandler.Instance.SetFixedTime(DEATH_PH_DELTA_TIME);
		}
		
		public void HandlePhysicsAccuracy(float speed)
		{
			float targetFixedDeltaTime;
			if (speed > MIN_SOVER_PH_DELTA_INCREASE_SPEED)
			{
				targetFixedDeltaTime = Math.Max(MIN_PH_DELTA_TIME, MAX_PH_DELTA_TIME - (speed - MIN_SOVER_PH_DELTA_INCREASE_SPEED) * PH_DELTA_SPEED_FACTOR);
			}
			else
			{
				targetFixedDeltaTime = MAX_PH_DELTA_TIME;
			}
			float diff = targetFixedDeltaTime - Time.fixedDeltaTime;
			if (Math.Abs(diff) > PH_DELTA_MAX)
			{
				FixedTimeHandler.Instance.SetFixedTime(Time.fixedDeltaTime + PH_DELTA_MAX*Math.Sign(diff));
			}
			else
			{
				FixedTimeHandler.Instance.SetFixedTime(Time.fixedDeltaTime);
			}
		}
	}
}
