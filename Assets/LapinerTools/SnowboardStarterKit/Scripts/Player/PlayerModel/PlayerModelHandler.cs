using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Snowboard
{
	public class PlayerModelHandler
	{
		// Graphic Constants
		private const float MODEL_LEAN_SENSITIVITY = 1.75f;
		private const float MODEL_LEAN_MAX_ANGLE = 38.0f;
		private const float MODEL_Y_OFFSET = -0.45f;
		private const float DAMPER_Y_OFFSET = -0.1f;
		private const float MODEL_EULER_ROT_SPEED = 12.0f;
		private const float MODEL_UP_VECT_SPEED = 12.0f;
		private const float SPINE_ROT_FADE_SPEED = 3f;
		private const float MIN_AIR_TIME_FOR_LAND = 0.75f;
		private const float LAND_SOFT_MIN_V_CHANGE = 0.75f;
		private const float LAND_SOFT_ANIM_DURATION_FACTOR = 0.2f;
		private const float LAND_HARD_MIN_V_CHANGE = 2.0f;
		private const float LAND_HARD_ANIM_DURATION_FACTOR = 0.1f;
		private const float LAND_HARD_ANIM_DURATION_LIMIT = 2f;
		private const float ANIM_REL_SPEED_FACTOR = 0.05f;
		private Vector3 m_modelEulerRotOffset = Vector3.zero;
		
		private EAnim m_nextGrab = EAnim.grab01;
		private EAnim m_currAnim = EAnim.ride;

		private PlayerController m_playerCtrl;
		private GameObject m_playerModel;
		private Animator m_playerModelAnim;

		private GameObject m_deadPlayerPrefab;
		
		private Transform m_playerHipsBone;
		private Collider[] m_disabledCollidersOnLandAnim;
		
		private float m_spineRotationFade = 1.0f; 
		
		private float m_currEuler = 0.0f;
		private Vector3 m_currDirection = Vector3.forward;
		private Vector3 m_currUpVector = Vector3.up;
		private Quaternion m_lastRotAroundGroundPivot = Quaternion.identity;
		private bool m_isLandColliderEnabled = true;
		private float m_landColliderDisabledTill = -1f;

		private float m_landAnimationTill = -1f;

		public PlayerModelHandler(PlayerController p_playerController)
		{
			m_playerCtrl = p_playerController;
		}
		
		public void OnUpdate()
		{
			// LOAD PLAYER MODEL
			if (m_playerModel == null)
			{
				initializeCharacter();
				return;
			}
			
			// ANIMATE LOADED PLAYER MODEL
			if (m_playerModel != null && m_playerCtrl.State != PlayerState.Dead)
			{
				// update bone animation
				updateAnimationPose();
				
				// calculate smoothed up vector
				m_currUpVector = Vector3.Slerp(m_currUpVector, m_playerCtrl.BordUpVector, Mathf.Clamp01(MODEL_UP_VECT_SPEED * Time.deltaTime));
				m_currDirection = Vector3.Slerp(m_currDirection, m_playerCtrl.BordDirection, Mathf.Clamp01(MODEL_UP_VECT_SPEED * Time.deltaTime));
				
				// ROTATION OF THE WHOLE MODEL
				// add default offset (goofy/regular)
				Vector3 euler = m_modelEulerRotOffset;
				float eulerValue = 0.0f;
				// add lean back due to slide velocity loss
				if (m_playerCtrl.State != PlayerState.Air && m_playerCtrl.State != PlayerState.Locked)
				{
					float loss = m_playerCtrl.LastSlideVLoss.magnitude / Time.fixedDeltaTime;
					if (loss > 0.00001f)
					{
						// calculate
						eulerValue = MODEL_LEAN_SENSITIVITY * loss * Mathf.Sign(Vector3.Dot(m_playerCtrl.LastSlideVLoss, m_playerCtrl.BordLateralDirection));
						if (Mathf.Abs(eulerValue) > MODEL_LEAN_MAX_ANGLE)
						{
							eulerValue = Mathf.Sign(eulerValue) * MODEL_LEAN_MAX_ANGLE;
						}
					}
				}
				// smooth
				m_currEuler = Mathf.Lerp(m_currEuler, eulerValue, Mathf.Clamp01(MODEL_EULER_ROT_SPEED * Time.deltaTime));		
				euler.x += m_currEuler;
				// apply rotation
				Quaternion rotationAroundHips = Quaternion.LookRotation(m_currDirection, m_currUpVector);			
				Quaternion rotationAroundGroundPivot = Quaternion.Euler(euler);
				m_playerModel.transform.rotation *= Quaternion.Inverse(m_lastRotAroundGroundPivot);
				m_playerModel.transform.position -= Vector3.up * MODEL_Y_OFFSET;
				Vector3 localDistToHips = m_playerModel.transform.InverseTransformDirection(m_playerHipsBone.transform.position - m_playerModel.transform.position);
				m_playerModel.transform.rotation = rotationAroundHips;
				m_playerModel.transform.localPosition = localDistToHips - m_playerModel.transform.TransformDirection(localDistToHips);
				m_playerModel.transform.position += Vector3.up * MODEL_Y_OFFSET;
				m_playerModel.transform.rotation *= rotationAroundGroundPivot;
				m_lastRotAroundGroundPivot = rotationAroundGroundPivot;
				// apply an offset to the damper collider, so that it always has the size of the board
				m_playerCtrl.DamperJoint.anchor = m_playerCtrl.transform.InverseTransformPoint(m_playerModel.transform.position + m_playerModel.transform.up * (m_playerCtrl.DamperRadius+DAMPER_Y_OFFSET));

				// LOOK&FLIP ROTATIONS FADE
				if (m_playerCtrl.GrabState != PlayerGrabState.NO_GRAB || m_landAnimationTill > Time.realtimeSinceStartup)
				{
					m_spineRotationFade = Mathf.Max(m_spineRotationFade-Time.deltaTime*SPINE_ROT_FADE_SPEED, 0.0f);	
				}
				else
				{
					m_spineRotationFade = Mathf.Min(m_spineRotationFade+Time.deltaTime*SPINE_ROT_FADE_SPEED, 1.0f);
				}
				// LOOK&FLIP ROTATION
				float cosVelDirRad = Vector3.Dot(m_playerCtrl.BordDirection, m_playerCtrl.Velocity.normalized);
				float lookNormTime = Mathf.Lerp(0.5741f, (1.0741f - (cosVelDirRad+1.0f)*0.5f), m_spineRotationFade) / 1.0741f;
				float flipNormTime = Mathf.Max( 0.0f, -m_currUpVector.y*m_spineRotationFade);
				m_playerModelAnim.Play("Look Layer.look", 1, lookNormTime);
				m_playerModelAnim.Play("Flip Layer.flip", 2, flipNormTime);

				// replay recording
				if (PlayerReplay.IsRecordForModelNeeded)
				{
					PlayerReplay.Record(
						m_playerModel.transform.position,
						m_playerModel.transform.rotation.eulerAngles,
						lookNormTime,
						flipNormTime);
				}
			}

			// DISABLE LOWEST COLLIDERS IF PLAYER IS LANDING
			updateLandColliders();
		}
		
		public GameObject GenerateDeadBody()
		{
			// unparent character from player prefab
			m_playerModel.transform.parent = null;
			m_playerCtrl.AdditionalAudioSource1 = null;
			m_playerModel.GetComponent<AudioSource>().Stop();
			m_playerModel.GetComponent<AudioSource>().loop = false;
			m_playerModel.GetComponent<AudioSource>().pitch = 1.0f;
			m_playerModel.GetComponent<AudioSource>().volume = 1.0f;
			
			// change name and tag
			m_playerModel.name = "PlayerDead";
			m_playerModel.tag = "DeadPlayer";
			
			// copy settings of PlayerDeadSoundManager from prefab
			PlayerDeadSoundManager soundMgrPrefab = m_deadPlayerPrefab.GetComponent<PlayerDeadSoundManager>();
			PlayerDeadSoundManager soundMgr = m_playerModel.AddComponent<PlayerDeadSoundManager>();
			soundMgr.HitSounds = soundMgrPrefab.HitSounds;
			soundMgr.PainSounds = soundMgrPrefab.PainSounds;
			soundMgr.SplatterSounds = soundMgrPrefab.SplatterSounds;
			soundMgr.KeyDeathSounds = soundMgrPrefab.KeyDeathSounds;
			
			// copy settings of bone bodies and scripts
			Transform[] bonesPrefab = PlayerAnimationHelper.GetAllBones(m_deadPlayerPrefab);
			Transform[] bones = PlayerAnimationHelper.GetAllBones(m_playerModel);
			foreach(Transform bone in bones)
			{
				for (int i=0; i<bonesPrefab.Length; i++)
				{
					if (bone.name == bonesPrefab[i].name)
					{
						PlayerAnimationHelper.CopyDeadBoneSettings(bone, bonesPrefab[i], m_playerCtrl.LastFrameVelocity);
						break;
					}
				}
			}
			PlayerAnimationHelper.ConnectDeadBodyJoints(bones, m_playerHipsBone);
			
			// fix joint some bones
			Transform[] fixedJoints = PlayerAnimationHelper.GetFixedJointsOnDeath(m_playerModel);
			for (int i = 0; i < fixedJoints.Length; i++)
			{
				if (fixedJoints[i].GetComponent<Rigidbody>() == null)
				{
					Debug.LogError("PlayerModelHandler: GenerateDeadBody: could not add fixed joint to '" + fixedJoints[i].name +
					               "', because it has no Rigidbody attached!");
					continue;
				}
				for (int j = i+1; j < fixedJoints.Length; j++)
				{
					if (fixedJoints[j].GetComponent<Rigidbody>() != null)
					{
						FixedJoint joint = fixedJoints[i].gameObject.AddComponent<FixedJoint>();
						joint.connectedBody = fixedJoints[j].GetComponent<Rigidbody>();
					}
					else
					{
						Debug.LogError("PlayerModelHandler: GenerateDeadBody: could not add fixed joint to '" + fixedJoints[j].name +
						               "', because it has no Rigidbody attached!");
					}
				}
			}

			// activate some additional colliders
			Collider[] additionalColliders = PlayerAnimationHelper.GetAdditionalColliderOnDeath(m_playerModel);
			for (int i = 0; i < additionalColliders.Length; i++)
			{
				additionalColliders[i].enabled = true;
				additionalColliders[i].isTrigger = false;
			}

			// remove animation
			GameObject.Destroy(m_playerModelAnim);
			m_playerModelAnim = null;
			
			// handle collision
			PlayerAnimationHelper.MoveToLastPhysicsFrame(m_playerModel.transform, m_playerCtrl.LastFrameVelocity);
			
			// attach a PlayerDeadHipsManager
			m_playerHipsBone.gameObject.AddComponent<PlayerDeadHipsManager>().SetPlayerController(m_playerCtrl);
			
			return m_playerModel;
		}
		
		private void setAnimationPose(EAnim Pose)
		{
			m_currAnim = Pose;
			// configure animator properties
			m_playerModelAnim.SetInteger("pose", (int)Pose);
			// replay recording
			PlayerReplay.Record(Pose);
		}
		
		private void initializeCharacter()
		{
			// model handler will handle the damper anchor in its update function
			m_playerCtrl.DamperJoint.autoConfigureConnectedAnchor = false;
			m_playerCtrl.DamperJoint.connectedAnchor = Vector3.zero;
			// load character model and set its parent
			string modelPath = GameCharacterHandler.Instance.GetPlayerModelResourcePath();
			if (Resources.Load(modelPath) == null)
			{
				Debug.LogError("PlayerModelHandler: initializeCharacter: could not load model with resource path '" + modelPath + "'!");
			}
			m_playerModel = (GameObject)Object.Instantiate(Resources.Load(modelPath));
			m_playerModel.transform.parent = m_playerCtrl.transform;
			m_playerModel.transform.position = m_playerCtrl.transform.position + Vector3.up*MODEL_Y_OFFSET;
			m_playerModel.transform.localRotation = Quaternion.identity;
			m_playerModel.transform.localScale = Vector3.one;
			// get model rotation offset
			if (PlayerAnimationHelper.GetIsModelRotatedBy90Degrees(m_playerModel))
			{
				m_modelEulerRotOffset = new Vector3(0.0f, 90.0f, 0.0f);
			}
			// get and cache often accessed bones
			m_playerHipsBone = PlayerAnimationHelper.GetHipsBone(m_playerModel);
			// add animations
			m_playerModelAnim = m_playerModel.GetComponent<Animator>();
			
			// load dead player prefab
			string deadModelPath = PlayerAnimationHelper.GetDeadResourcePath(m_playerModel);
			m_deadPlayerPrefab = (GameObject)Resources.Load(deadModelPath);
			// copy settings of bone colliders
			Transform[] bonesPrefab = PlayerAnimationHelper.GetAllBones(m_deadPlayerPrefab);
			Transform[] bones = PlayerAnimationHelper.GetAllBones(m_playerModel);
			foreach(Transform bone in bones)
			{
				for (int i=0; i<bonesPrefab.Length; i++)
				{
					if (bone.name == bonesPrefab[i].name)
					{
						PlayerAnimationHelper.CopyAliveBoneSettings(bone, bonesPrefab[i]);
						break;
					}
				}
			}
			// audio
			AudioSource audioSource = m_playerModel.AddComponent<AudioSource>();
			audioSource.volume = 0;
			audioSource.loop = true;
			audioSource.playOnAwake = false;
			m_playerCtrl.AdditionalAudioSource1 = audioSource;

			// ignore collision between some colliders and the player spheres
			Collider[] ignoreColliders = PlayerAnimationHelper.GetIgnoreCollisionWithPlayerCollider(m_playerModel);
			for (int i = 0; i < ignoreColliders.Length; i++)
			{
				Physics.IgnoreCollision(m_playerCtrl.GetComponent<Collider>(), ignoreColliders[i]);
				Physics.IgnoreCollision(m_playerCtrl.damper.GetComponent<Collider>(), ignoreColliders[i]);
			}

			// cache colliders that will be disabled while in land animation
			Transform[] colliderTransforms = PlayerAnimationHelper.GetDisabledCollidersOnLandAnim(m_playerModel);
			m_disabledCollidersOnLandAnim = new Collider[colliderTransforms.Length];
			for (int i = 0; i < colliderTransforms.Length; i++)
			{
				if (colliderTransforms[i] != null)
				{
					m_disabledCollidersOnLandAnim[i] = colliderTransforms[i].GetComponent<Collider>();
				}
				if (m_disabledCollidersOnLandAnim[i] == null)
				{
					Debug.LogError("PlayerModelHandler: initializeCharacter: could not find collider at index '" + i + "' in DisabledCollidersOnLandAnim");
				}
			}
			
			// move player to ignore raycast layer
			int ignoreRayLayer = PlayerConfig.Instance.IGNORE_RAYCAST_LAYER;
			foreach (Transform t in m_playerModel.transform.root.GetComponentsInChildren<Transform>())
			{
				t.gameObject.layer = ignoreRayLayer;
			}

			// reset follow camera
			CameraFollow followCam = CameraFollow.GetInstance(m_playerCtrl);
			if (followCam != null) // null if was not set yet
			{
				followCam.SetTarget(m_playerCtrl.transform);
				followCam.height = 1.5f;
			}
		}

		private void updateLandColliders()
		{
			bool isLandAnim = m_currAnim == EAnim.land || m_currAnim == EAnim.land_soft || m_currAnim == EAnim.land_hard;
			if (isLandAnim)
			{
				m_landColliderDisabledTill = Time.time + 1f;
				if (m_isLandColliderEnabled)
				{
					m_isLandColliderEnabled = false;
					for (int i = 0; i < m_disabledCollidersOnLandAnim.Length; i++)
					{
						if (m_disabledCollidersOnLandAnim[i] != null)
						{
							m_disabledCollidersOnLandAnim[i].enabled = false;
						}
					}
				}
			}
			else if (!m_isLandColliderEnabled && m_landColliderDisabledTill <= Time.time)
			{
				m_isLandColliderEnabled = true;
				for (int i = 0; i < m_disabledCollidersOnLandAnim.Length; i++)
				{
					if (m_disabledCollidersOnLandAnim[i] != null)
					{
						m_disabledCollidersOnLandAnim[i].enabled = true;
					}
				}
			}
		}

		private void updateAnimationPose()
		{
			// update pose
			if (m_playerCtrl.State == PlayerState.LevelEnd)
			{
				setAnimationPose(EAnim.ride);
			}
			else if (m_playerCtrl.State == PlayerState.TakeOff)
			{
				// PlayerState.TakeOff
				// start jump animation
				if (m_playerCtrl.IsSwitch)
				{
					setAnimationPose(EAnim.jump_goofy);
				}
				else
				{
					setAnimationPose(EAnim.jump_regular);
				}
			}
			else if (m_playerCtrl.State == PlayerState.Air)
			{
				// !PlayerState.TakeOff && PlayerState.Air
				// air or grab animation
				if (m_landAnimationTill <= Time.realtimeSinceStartup)
				{
					if (m_playerCtrl.GrabState == PlayerGrabState.NO_GRAB)
					{
						if (m_playerCtrl.IsLanding && m_playerCtrl.AirTime > MIN_AIR_TIME_FOR_LAND)
						{
							setAnimationPose(EAnim.land);
						}
						else
						{
							setAnimationPose(EAnim.air);
						}
						m_nextGrab = EAnim.grab01 + Random.Range(0,2);
					}
					else
					{
						setAnimationPose(m_nextGrab);
					}
				}
			}
			else if (m_playerCtrl.LastLandVChange > LAND_SOFT_MIN_V_CHANGE  &&  Time.realtimeSinceStartup - m_playerCtrl.LastLandTime < Mathf.Min(LAND_HARD_ANIM_DURATION_LIMIT, m_playerCtrl.LastLandVChange * LAND_SOFT_ANIM_DURATION_FACTOR))
			{
				// ride, bomb, shaky can be replaced by land_hard or land_soft
				if (m_landAnimationTill < Time.realtimeSinceStartup)
				{
					if (m_playerCtrl.LastLandVChange > LAND_HARD_MIN_V_CHANGE && Time.realtimeSinceStartup - m_playerCtrl.LastLandTime <= Mathf.Min(LAND_HARD_ANIM_DURATION_LIMIT, m_playerCtrl.LastLandVChange * LAND_HARD_ANIM_DURATION_FACTOR))
					{
						setAnimationPose(EAnim.land_hard);
						m_landAnimationTill = Mathf.Min(LAND_HARD_ANIM_DURATION_LIMIT, m_playerCtrl.LastLandVChange * LAND_HARD_ANIM_DURATION_FACTOR) + m_playerCtrl.LastLandTime;
					}
					else
					{
						setAnimationPose(EAnim.land_soft);
						m_landAnimationTill = m_playerCtrl.LastLandVChange * LAND_SOFT_ANIM_DURATION_FACTOR + m_playerCtrl.LastLandTime;
					}
				}
			}
			else if (m_playerCtrl.IsOffRoad)
			{
				// !PlayerState.TakeOff && !PlayerState.Air && !IsOffRoad -> off road animation
				setAnimationPose(EAnim.shaky);
			}
			else if (m_playerCtrl.IsAccelerating || m_playerCtrl.BalanceState == PlayerBalanceState.Center)
			{
				// !PlayerState.TakeOff && !PlayerState.Air -> bomb animation if on centers or is accelerating
				setAnimationPose(EAnim.bomb);
			}
			else
			{
				// default ride animation
				setAnimationPose(EAnim.ride);
			}

			// update pose parameters
			m_playerModelAnim.SetFloat("rel_speed", m_playerCtrl.Velocity.magnitude*ANIM_REL_SPEED_FACTOR);
		}
	}
}
