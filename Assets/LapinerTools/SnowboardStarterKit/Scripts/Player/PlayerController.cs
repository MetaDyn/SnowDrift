using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Snowboard
{
	public enum PlayerState
	{
		Locked,
		Ground,
		TakeOff,
		Air,
		Dead,
		LevelEnd
	}

	public enum PlayerBalanceState
	{
		Center,
		Normal,
		OnEdge
	}

	public enum PlayerGrabState
	{
		NO_GRAB,
		RANDOM_GRAB
	}

	[RequireComponent (typeof (AudioSource))]
	[RequireComponent (typeof (Collider))]
	public class PlayerController : MonoBehaviour
	{
		public EPlayers m_playerIndex = EPlayers.SINGLE;
		
		public Transform damper = null;
		
		public AudioSource AdditionalAudioSource1 = null;
		public AudioSource AdditionalAudioSource2 = null;
		public AudioClip SoundLanding = null;
		public AudioClip SoundLandingLocker = null;
		public AudioClip SoundSlideSoft = null;
		public AudioClip SoundRoll = null;
		public AudioClip SoundGrind = null;

		private float AdditionalAudioSource1VolumeSmoothDampV = 0f;
		private float AdditionalAudioSource2VolumeSmoothDampV = 0f;
		private float AdditionalAudioSource2PitchSmoothDampV = 0f;

		private const float SPAWN_SAFE_TIME = 3f;

		private const float SLOMOTION_VELOCITY_MIN = 17f;
		private const float BOUNCE_OFF_WALL_MIN_INTERVAL = 0.5f;
		private const float BOUNCE_OFF_WALL_MAX_ANGLE = 0.5f;
		private const float BOUNCE_OFF_WALL_MAX_SPEED = 8.0f;
		private const float BOUNCE_OFF_WALL_MIN_SPEED_AFTER = 3.0f;
		private const float BOUNCE_OFF_WALL_SPEED_FACTOR = 0.5f;
		private const float BOUNCE_OFF_WALL_SAFE_TIME = 0.2f;
		
		private const int COLLISION_ABSENCE_MAX = 20;
		private const int COLLISION_ABSENCE_MAX_DIV_2 = COLLISION_ABSENCE_MAX / 2;
		
		// Handler
		private PlayerInputHandler m_inputHandler;
		private PlayerPhysicsHandler m_physicsHandler;
		private PlayerModelHandler m_modelHandler;
		private PlayerScoreHandler m_scoreHandler;
		private PlayerLandingAssistantHandler m_landingHandler;
		
		// Members
		private bool m_isSwitch = false;
		private Vector3 m_bordDirection;
		private Vector3 m_bordUpVector;
		private Vector3 m_bordUpVectorAccumulator = Vector3.zero;
		private Vector3 m_keepSameUpVectorAccumulator = Vector3.zero;
		private bool m_keepSameUpVector = false;
		private float m_lastWallBounceTime = 0.0f;
		private int m_skippedCollisionFramesNum = 0;

		private bool m_isCollidingWithTerrain = false;
		private bool m_isCollidingWithRigidBody = false;
		private int m_lastCollisionPointAccumulatorNum = 0;
		private Vector3 m_lastCollisionPointAccumulator = Vector3.zero;
		private Vector3 m_lastCollisionPoint = Vector3.zero;
		private Vector3 m_velocityLastFrame = Vector3.zero;
		private float m_lastSlideStartTime = 0.0f;
		private float m_lastRailLandingSoundTime = 0.0f;
		private float m_lastOffRoadTime = -1.0f;
		
		private PlayerState m_stateLastFrame = PlayerState.Air;
		private PlayerState m_state = PlayerState.Air;
		private PlayerGrabState m_grabState = PlayerGrabState.NO_GRAB;
		
		private int m_collisionAbsenceCounter = COLLISION_ABSENCE_MAX;

		private Vector3 m_lockedDirection = Vector3.zero;
		private Vector3 m_lockedTarget = Vector3.zero;
		
		private float m_lastJumpTime = 0.0f;
		
		private float m_airTime = 0.0f;
		
		private Vector3 m_lastFrameVelocity = Vector3.zero;
		
		private int m_takeOffCounter = 0;
		
		private bool m_isDeathMessageSuppressed = false;
		
		private Rigidbody m_cachedRigidBody = null;
		private SpringJoint m_cachedDamperJoint = null;
		private float m_cachedDamperRadius = 1;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// PROPERTIES
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static EGameOverType GameOverType { get; set; }
		
		public Vector3 Velocity
		{
			get{ return m_cachedRigidBody.velocity; }
			set{ m_cachedRigidBody.velocity = value; }
		}

		public PlayerPhysicsHandler PhysicsHandler { get{ return m_physicsHandler; } }
		public PlayerScoreHandler ScoreHandler { get{ return m_scoreHandler; } }
		private PlayerInputHandler InputHandler { get{ return m_inputHandler; } }
		private PlayerModelHandler ModelHandler { get{ return m_modelHandler; } }
		private PlayerLandingAssistantHandler LandingHandler { get{ return m_landingHandler; } }

		public bool IsAccelerating { get{ return m_inputHandler.IsAccelerating; } }
		public bool IsOffRoad { get{ return m_physicsHandler.IsOffroad; } }
		public bool IsOnAWall { get{ return m_physicsHandler.IsOnAWall; } }
		public bool IsSwitch { get{ return m_isSwitch; } }
		public bool IsLanding { get{ return m_landingHandler.IsAssisting; } }
		public bool IsCollidingWithTerrain { get{ return m_isCollidingWithTerrain; } }

		public float AirTime { get{ return m_airTime; } }
		public float TimeToLanding { get{ return m_landingHandler.TimeToLanding; } }
		/// <summary>
		/// 0->will go to air state, 1->solid ground state
		/// valid only if State=Ground
		/// </summary>
		public float GroundStateStability
		{
			get
			{
				if (m_state == PlayerState.Locked)
				{
					return Mathf.Clamp01(((float)m_collisionAbsenceCounter - (float)COLLISION_ABSENCE_MAX_DIV_2) / COLLISION_ABSENCE_MAX_DIV_2);
				}
				else
				{
					return Mathf.Clamp01((float)m_collisionAbsenceCounter / (float)COLLISION_ABSENCE_MAX);
				}
			}
		}

		public float LastLandTime { get{ return m_physicsHandler.LastLandTime; } }
		public float LastLandVChange { get{ return m_physicsHandler.LastLandVChange; } }
		public Vector3 LastFrameVelocity { get{ return m_lastFrameVelocity; } }
		public Vector3 LastSlideVLoss { get{ return m_physicsHandler.LastSlideVLoss; } }
		public Vector3 LastCollisionPoint { get{ return m_lastCollisionPoint; } }
		public float LastJumpTime { get{ return m_lastJumpTime; } }
		
		public Vector3 Position { get{ return transform.position; } }
		public Vector3 BordLateralDirection { get{ return Vector3.Cross(m_bordDirection, m_bordUpVector).normalized; } }
		public Vector3 BordUpVector { get{ return m_bordUpVector; } }
		public Vector3 BordDirection { get{ return m_bordDirection; } }

		public Vector3 LockedDirection { get{ return m_lockedDirection; } }	
		public Vector3 LockedTarget { get{ return m_lockedTarget; } }
		
		public PlayerState State { get{ return m_state; } }
		public PlayerGrabState GrabState { get{ return m_grabState; } }
		public PlayerBalanceState BalanceState { get{ return m_inputHandler.BalanceState; } }

		public SpringJoint DamperJoint { get{ return m_cachedDamperJoint; } }
		public float DamperRadius { get{ return m_cachedDamperRadius; } }

		public float FrameRotX { get{ return m_inputHandler.FrameRotX; } }
		public float FrameRotY { get{ return m_inputHandler.FrameRotY;} }

		public void SetBordDirection(Vector3 pBordDirection) // function istead of property for clearness of what is done
		{
			m_bordDirection = pBordDirection;
		}
		
		public void SetBordUpVector(Vector3 pBordUpVector) // function istead of property for clearness of what is done
		{
			m_bordUpVector = pBordUpVector;
		}
		
		public void SetState(PlayerState pNextState) // function istead of property for clearness of what is done
		{
			if (m_state != pNextState)
			{
				m_state = pNextState;
				if (m_state == PlayerState.LevelEnd)
				{
					m_scoreHandler.LevelEnd();
				}
			}
		}
		
		public void SetGrabState(PlayerGrabState pNextGrabState) // function istead of property for clearness of what is done
		{
			m_grabState = pNextGrabState;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void OnPlayerOffRoad()
		{
			m_lastOffRoadTime = Time.time;
			m_physicsHandler.IsOffroad = true;
		}
		
		public void SuppressDeathMessage()
		{
			m_isDeathMessageSuppressed = true;
		}
		
		public void OnPlayerCollisionDeadlySurface()
		{
			die();
		}
		
		public void OnPlayerCollisionLocker()
		{
			die();
		}
		
		public void OnPlayerCollision()
		{
			StartCoroutine(WaitForEndOfFrameAndDieByCollision());
		}
		
		private IEnumerator WaitForEndOfFrameAndDieByCollision()
		{
			yield return new WaitForEndOfFrame();
			if (Time.time - m_lastWallBounceTime > BOUNCE_OFF_WALL_SAFE_TIME)
			{
				die();
			}
		}
		
		public void ApplyRotationAxis(Vector3 pAxis, float rotFactor)
		{
			if (rotFactor != 0.0f && !System.Single.IsNaN(rotFactor * Time.deltaTime))
			{
				Quaternion rotQuat = Quaternion.AngleAxis(rotFactor * Time.deltaTime, pAxis);
				m_bordDirection = rotQuat * m_bordDirection;
				m_bordUpVector = rotQuat * m_bordUpVector;
			}
		}
		
		public void GoToTakeOff()
		{
			m_state = PlayerState.TakeOff;
			m_lastJumpTime = Time.time;
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// MONO BEHAVIOUR METHODS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		void Awake()
		{
			GameOverType = EGameOverType.NONE;
			m_cachedRigidBody = GetComponent<Rigidbody>();
			m_cachedDamperJoint = GetComponent<SpringJoint>();
			m_cachedDamperRadius = damper.GetComponent<SphereCollider>().radius;
		}
		
		void Start ()
		{
			if (SoundLanding == null || SoundLandingLocker == null ||
				SoundSlideSoft == null || SoundGrind == null ||
			    (SoundRoll == null && AdditionalAudioSource2 != null) ||
			    (SoundRoll != null && AdditionalAudioSource2 == null))
			{
				Debug.LogError("PlayerController: sounds are not set properly!");
			}
			
			// assign looping sounds
			if (AdditionalAudioSource2 != null)
			{
				AdditionalAudioSource2.clip = SoundRoll;
				AdditionalAudioSource2.Play();
			}
			
			m_inputHandler = new PlayerInputHandler(this, m_cachedRigidBody);
			m_physicsHandler = new PlayerPhysicsHandler(this, m_cachedRigidBody);
			m_modelHandler = new PlayerModelHandler(this);
			m_scoreHandler = new PlayerScoreHandler(this);
			m_landingHandler = new PlayerLandingAssistantHandler(this);
			
			m_bordDirection = transform.forward;
			m_bordUpVector = transform.up;
		}
		
		void FixedUpdate ()
		{	
			if (m_state != PlayerState.LevelEnd)
			{
				m_physicsHandler.HandlePhysicsAccuracy(Velocity.magnitude);
				if (Time.time - m_lastOffRoadTime > 0.25f) {
					m_physicsHandler.IsOffroad = false;
				}
				
				// if there is no collision for multiple frames then we are in air
				if (m_state == PlayerState.TakeOff)
				{
					m_collisionAbsenceCounter -= 2;	
				}
				else
				{
					m_collisionAbsenceCounter--;
				}
				if (m_collisionAbsenceCounter < 0 || (m_state == PlayerState.Locked && m_collisionAbsenceCounter < COLLISION_ABSENCE_MAX_DIV_2))
				{
					setIsAir(true);
				}
				
				m_physicsHandler.OnFixedUpdate();
			}
		}
		
		void Update ()
		{
			if (m_state != PlayerState.LevelEnd)
			{	
				handleSounds();
				
				applyCollisionInfo();
				
				UpdateSwitchState();
				
				if (Time.timeScale > 0.01f)
				{
					// ignore controls when popups are opened or game is paused
					m_inputHandler.OnUpdate();
				}
				
				handleStates ();
				
				m_modelHandler.OnUpdate();
				m_scoreHandler.OnUpdate();
				m_landingHandler.OnUpdate();
				
				if (m_state != PlayerState.Air)
				{
					m_airTime = 0.0f;
				}
				else
				{
					m_airTime += Time.deltaTime;
				}
				
				m_stateLastFrame = m_state;
				
				m_physicsHandler.OnUpdate();
			}
			else
			{
				m_modelHandler.OnUpdate();
				m_cachedRigidBody.isKinematic = true; // stop player
				if (AdditionalAudioSource1 != null)
				{
					AdditionalAudioSource1.Stop();
				}
				if (AdditionalAudioSource2 != null)
				{
					AdditionalAudioSource2.Stop();
				}
			}
			
			m_lastFrameVelocity = Velocity;
		}

		void OnDestroy()
		{
			if (m_scoreHandler != null)
			{
				m_scoreHandler.OnUpdate();
			}
		}

		public void OnCollisionExtern(Collision collisionInfo)
		{
			saveCollisionInfo(collisionInfo);
			setIsAir(false);
			checkForLocker(collisionInfo);
		}
		
		public void UpdateSwitchState()
		{
			// update switch state
			if (m_state != PlayerState.Air)
			{
				m_isSwitch = Mathf.Sign(m_bordUpVector.y) * Mathf.Abs(Vector3.Angle(m_bordDirection, m_cachedRigidBody.velocity.normalized)) > 90;
			}
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void die()
		{
			die(false);
		}
			
		public void die(bool pIsByKKey)
		{
			if (m_state != PlayerState.Dead && m_state != PlayerState.LevelEnd && Time.timeSinceLevelLoad > SPAWN_SAFE_TIME)
			{
				GameObject deadPlayer = m_modelHandler.GenerateDeadBody();
				if (pIsByKKey)
				{
					deadPlayer.GetComponent<PlayerDeadSoundManager>().SetDeathByKey();
				}
				if (m_isDeathMessageSuppressed)
				{
					deadPlayer.GetComponentInChildren<PlayerDeadHipsManager>().m_sendFailLevelMessage = false;
				}
				else if (GameOverType != EGameOverType.NONE)
				{
					deadPlayer.GetComponentInChildren<PlayerDeadHipsManager>().m_gameOverType = GameOverType;
				}
				
				Destroy(gameObject);
				
				// make the camera follow the dead body
				CameraFollow cam = CameraFollow.GetInstance(this);
				cam.target = PlayerAnimationHelper.GetHipsBone(deadPlayer);
				cam.maxDistance = 5.0f;
				cam.height = 3.0f;
				cam.cameraState = CameraFollow.State.FollowPlayerDead;
				
				m_state = PlayerState.Dead;
			}
		}
		
		private void checkForLocker(Collision collisionInfo)
		{
			GameObject collObj = collisionInfo.gameObject;
			bool isLocker = collObj.tag == "Locker";
			PlayerMovementLocker locker = null;
			if (isLocker)
			{
				locker = collObj.GetComponent<PlayerMovementLocker>();
				if (locker == null && collObj.transform.parent != null)
				{
					locker = collObj.GetComponentInParent<PlayerMovementLocker>();
				}
				isLocker = locker != null;
			}
			if (m_state == PlayerState.Ground)
			{
				if (isLocker)
				{
					TryLockTarget(collisionInfo, locker);
				}
			}
			else if (m_state == PlayerState.Locked)
			{
				if (!isLocker)
				{
					m_state = PlayerState.Ground;
				}
				else
				{
					TryLockTarget(collisionInfo, locker);
				}
			}
		}
		
		private void TryLockTarget(Collision p_collisionInfo, PlayerMovementLocker p_locker)
		{
			Transform lockFrom;
			Transform lockTo;
			Vector3 collisionPointAverage = Vector3.zero;
			for (int i = 0; i < p_collisionInfo.contacts.Length; i++)
			{
				collisionPointAverage += p_collisionInfo.contacts[i].point;
			}
			collisionPointAverage /= (float)p_collisionInfo.contacts.Length;
			if (p_locker.LockPoints.Length == 0)
			{
				m_lockedDirection = m_cachedRigidBody.velocity;	
				m_lockedTarget = collisionPointAverage;
				m_state = PlayerState.Locked;
			}
			else if (p_locker.GetLockPoints(collisionPointAverage, m_cachedRigidBody.velocity, out lockTo, out lockFrom))
			{
				m_lockedDirection = (lockTo.position - lockFrom.position).normalized;	
				m_lockedTarget = lockTo.position;
				if (m_physicsHandler.LockOnRail())
				{
					m_state = PlayerState.Locked;
				}
			}		
		}
		
		private void setIsAir(bool value)
		{
			m_collisionAbsenceCounter = COLLISION_ABSENCE_MAX;
				
			switch (m_state)
			{
				case PlayerState.Locked:
				case PlayerState.Ground:
				case PlayerState.TakeOff:
					if (value)
					{
						m_state = PlayerState.Air;
					}
					break;
				case PlayerState.Air:
					if (!value)
					{
						if (m_grabState == PlayerGrabState.NO_GRAB)
						{
							m_state = PlayerState.Ground;
						}
						else
						{
							GameOverType = EGameOverType.LANDED_WITH_GRAB;
							die(); // tried to land while grabbed
						}
					}
					break;
			}
		}
		
		private void saveCollisionInfo(Collision collisionInfo)
		{
			if (collisionInfo.contacts.Length > 0)
			{
				m_isCollidingWithRigidBody = collisionInfo.rigidbody != null;
				m_isCollidingWithTerrain = collisionInfo.gameObject.layer == PlayerConfig.Instance.TERRAIN_LAYER;
				// calculate the new up vector
				foreach (ContactPoint contP in collisionInfo.contacts)
				{
					if (!m_keepSameUpVector)
					{
						m_bordUpVectorAccumulator += contP.normal;
					}
					m_lastCollisionPointAccumulator+= contP.point;
					m_lastCollisionPointAccumulatorNum++;
					// dont start riding up walls
					if (m_state == PlayerState.Ground)
					{
						m_keepSameUpVectorAccumulator += contP.normal;
						if ((contP.normal - m_bordUpVector).magnitude < 0.3f)
						{
							m_bordUpVectorAccumulator = contP.normal;
							m_keepSameUpVectorAccumulator -= contP.normal;
							m_keepSameUpVector = true;
						}
					}
				}
			}
		}
		
		private void updateBordDirection()
		{
			// update the bord's direction
			Vector3 lateralDirection = Vector3.Cross(m_bordDirection, m_bordUpVector);
			m_bordDirection = Vector3.Cross(m_bordUpVector, lateralDirection).normalized;
		}
		
		private void applyCollisionInfo()
		{
			if (m_lastCollisionPointAccumulatorNum > 0)
			{
				m_lastCollisionPoint = m_lastCollisionPointAccumulator / (float)m_lastCollisionPointAccumulatorNum;
				m_lastCollisionPointAccumulator = Vector3.zero;
				m_lastCollisionPointAccumulatorNum = 0;
				
				// mirror direction if collision with wall
				if (!m_isCollidingWithRigidBody &&
					Time.time - m_lastWallBounceTime > BOUNCE_OFF_WALL_MIN_INTERVAL &&
					m_keepSameUpVectorAccumulator.magnitude > 0.5f &&
					Vector3.Dot(m_keepSameUpVectorAccumulator, BordUpVector) < 0.5f)
				{
					m_keepSameUpVectorAccumulator.Normalize();
					if (Mathf.Abs(m_keepSameUpVectorAccumulator.y) < 0.5f)
					{
						Vector3 v = Velocity;
						float mirrorFactorNorm = Vector3.Dot(v.normalized, m_keepSameUpVectorAccumulator);
						float mirrorFactor = Vector3.Dot(v, m_keepSameUpVectorAccumulator);
						if (Mathf.Abs(mirrorFactorNorm) < BOUNCE_OFF_WALL_MAX_ANGLE || v.magnitude < BOUNCE_OFF_WALL_MAX_SPEED)
						{
							bool isSwitch = Mathf.Sign(m_bordUpVector.y) * Mathf.Abs(Vector3.Angle(m_bordDirection, v.normalized)) > 90;
							// mirror speed
							v -= 2.0f * mirrorFactor * m_keepSameUpVectorAccumulator;
							v *= BOUNCE_OFF_WALL_SPEED_FACTOR;
							float vLength = v.magnitude;
							if (vLength < BOUNCE_OFF_WALL_MIN_SPEED_AFTER)
							{
								v *= (BOUNCE_OFF_WALL_MIN_SPEED_AFTER / vLength);
							}
							Velocity = v;
							// redirect board
							if (isSwitch)
							{
								m_bordDirection = -v.normalized;
								m_bordDirection -= 0.5f * m_keepSameUpVectorAccumulator;
							
							}
							else
							{
								m_bordDirection = v.normalized;
								m_bordDirection += 0.5f * m_keepSameUpVectorAccumulator;
							}
							m_bordDirection.Normalize();
							m_lastWallBounceTime = Time.time;
						}
					}
				}
				
				m_bordUpVectorAccumulator.Normalize();
				RaycastHit hit;
				Vector3 damperToCollision = m_lastCollisionPoint - damper.position;
				if (Physics.Raycast(damper.position, damperToCollision, out hit, damperToCollision.magnitude * 1.35f))
				{
					if ((m_bordUpVectorAccumulator - hit.normal).magnitude < 0.3f)
					{
						m_bordUpVector = hit.normal;
						updateBordDirection();
						m_skippedCollisionFramesNum = 0;
					}
					else
					{
						if (m_skippedCollisionFramesNum > 3)
						{
							m_bordUpVector = m_bordUpVectorAccumulator;
							updateBordDirection();
							m_skippedCollisionFramesNum = 0;
						}
						
						m_skippedCollisionFramesNum++;
					}
				}
				m_bordUpVectorAccumulator = Vector3.zero;
			}
			m_keepSameUpVectorAccumulator = Vector3.zero;
			m_keepSameUpVector = false;
		}
		
		private void handleSounds()
		{
			// DEFAULT AUDIO SOURCE
			// play landing sound
			if (m_stateLastFrame == PlayerState.Air)
			{
				if (m_state == PlayerState.Ground || m_state == PlayerState.TakeOff)
				{
					if (m_airTime > 0.3f && // make sure the sound is only played on noticeable airs
						m_lastJumpTime + 0.3f < Time.time) // make sure the sound is not repeated to often
					{
						// play ground landing sound
						float landVFactor = Mathf.Abs(Vector3.Dot(m_bordUpVector, m_velocityLastFrame)) * 0.05f;
						if (landVFactor > 0.1f)
						{
							GetComponent<AudioSource>().PlayOneShot(SoundLanding, landVFactor);
						}
					}
				}
				else if (m_state == PlayerState.Locked && m_lastRailLandingSoundTime + 0.3f < Time.time)
				{
					// play locker landing sound
					float landVFactor = Mathf.Abs(Vector3.Dot(m_bordUpVector, m_velocityLastFrame)) * 0.03f;
					GetComponent<AudioSource>().PlayOneShot(SoundLandingLocker, 0.1f + landVFactor);
					m_lastRailLandingSoundTime = Time.time;
				}
			}
			m_velocityLastFrame = m_cachedRigidBody.velocity;
			
			// ADDITIONAL AUDIO SOURCE 1
			float volume1 = 0.0f;
			// slide and grind sounds
			float bodyVLength = m_cachedRigidBody.velocity.magnitude;
			float slideV = Mathf.Abs(m_physicsHandler.LastSlideV);
			if (m_state == PlayerState.Ground)
			{
				// play sliding sound
				if (AdditionalAudioSource1 != null )
				{
					if (AdditionalAudioSource1.clip != SoundSlideSoft)
					{
						AdditionalAudioSource1.clip = SoundSlideSoft;
					}
					AdditionalAudioSource1.pitch =  Mathf.Min(0.85f, 0.35f + Mathf.Pow(bodyVLength * 0.02f, 2.0f));
					float movementVolume = 0.03f * Mathf.Clamp01(bodyVLength*0.25f) + 0.05f * Mathf.Clamp01(bodyVLength*0.05f);
					volume1 = Mathf.SmoothDamp(AdditionalAudioSource1.volume, Mathf.Clamp(movementVolume + Mathf.Clamp(Mathf.Pow(slideV * 0.075f, 2.0f), 0.0f, 1.0f) * 4.0f * m_physicsHandler.LastSlideVLoss.magnitude, 0.0f, 0.6f), ref AdditionalAudioSource1VolumeSmoothDampV, 0.1f);
					if (!AdditionalAudioSource1.isPlaying)
					{
						AdditionalAudioSource1.Play();
						m_lastSlideStartTime = Time.realtimeSinceStartup;
					}
				}
			}
			else if (m_lastSlideStartTime + 0.1f < Time.realtimeSinceStartup)
			{
				if (m_state == PlayerState.Locked)
				{
					// play grinding sound
					if (AdditionalAudioSource1 != null )
					{
						if (AdditionalAudioSource1.clip != SoundGrind)
						{
							AdditionalAudioSource1.clip = SoundGrind;
						}
						float factor =  Mathf.Min(1.0f, bodyVLength * 0.035f);
						AdditionalAudioSource1.pitch = 0.7f;
						volume1 = 0.1f + Mathf.Pow(factor, 2.0f) * 0.6f;
						if (!AdditionalAudioSource1.isPlaying)
						{
							AdditionalAudioSource1.Play();
							m_lastSlideStartTime = Time.realtimeSinceStartup;
						}
					}
				}
				else if (AdditionalAudioSource1 != null && AdditionalAudioSource1.isPlaying)
				{
					volume1 = 0.0f;
					AdditionalAudioSource1.Stop();
				}
			}
			if (AdditionalAudioSource1 != null)
			{
				AdditionalAudioSource1.volume = volume1;
			}
			
			
			// ADDITIONAL AUDIO SOURCE 2
			// play rolling sound
			if (AdditionalAudioSource2 != null)
			{
				if (m_state == PlayerState.Ground || m_state == PlayerState.TakeOff && bodyVLength > 2.5f)
				{
					float factor =  Mathf.Min(1.0f, bodyVLength * 0.035f);
					AdditionalAudioSource2.pitch = 0.5f + Mathf.Pow(factor, 2.0f);
					AdditionalAudioSource2.volume = factor*1.5f - volume1 * 0.5f;
				}
				else
				{
					if ((m_stateLastFrame == PlayerState.Ground || m_stateLastFrame == PlayerState.TakeOff) && bodyVLength > 2.5f)
					{
						AdditionalAudioSource2.pitch = 0.5f + 0.5f * (AdditionalAudioSource2.pitch - 0.5f);
						AdditionalAudioSource2.volume = AdditionalAudioSource2.volume * 0.5f;
					}
					
					AdditionalAudioSource2.pitch =  Mathf.SmoothDamp(AdditionalAudioSource2.pitch, 0.5f, ref AdditionalAudioSource2PitchSmoothDampV, 0.2f);
					AdditionalAudioSource2.volume = Mathf.SmoothDamp(AdditionalAudioSource2.volume, 0.0f, ref AdditionalAudioSource2VolumeSmoothDampV, 0.2f);
				}
			}
		}
		
		private void handleStates ()
		{
			// handle take off
			if (m_state == PlayerState.TakeOff)
			{
				m_takeOffCounter++;
				if (m_takeOffCounter > 20)
				{
					m_state = PlayerState.Ground;
					m_takeOffCounter = 0;
				}
			}
			else
			{
				m_takeOffCounter = 0;
			}
			
			// correct board up vector
			if (m_state == PlayerState.Locked)
			{
				Vector3 lateralDirection = Vector3.Cross(m_lockedDirection, m_bordUpVector);
				m_bordUpVector = Vector3.Cross(lateralDirection, m_lockedDirection).normalized;
			}		
		}
		
	#if UNITY_EDITOR
		private void OnDrawGizmos ()
		{
			// board direction
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, transform.position + m_bordDirection);
			// board up vector
			Gizmos.color = Color.green;
			Gizmos.DrawLine(transform.position, transform.position + m_bordUpVector);
			// locker
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(m_lockedTarget, 0.25f);
			Gizmos.DrawLine(m_lockedTarget, m_lockedTarget+m_lockedDirection);
		}
	#endif
	}
}
