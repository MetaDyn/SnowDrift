using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Snowboard
{
	public class CameraFollow : MonoBehaviour
	{
		private const float CINEMATIC_ROT_SPEED = 35.0f;
		private const float CINEMATIC_TARGET_HEIGHT = 1f;
		private const float CINEMATIC_HEIGHT = 3f;
		private const float CINEMATIC_CAM_DISTANCE = 8.5f;
		
		private const float DEAD_STATE_UP_FLOATING = 5.0f;
		private const float AIR_LOOK_SPEED = 0.6f;
		private const float CAM_HEIGHT_SPEED = 0.2f;
		private const float SPEED_TO_FOV_FACTOR = 1.7f;

		private const float CAMERA_SMOOTH_TIME = 0.1f;
		private const float CAMERA_SMOOTH_TIME_FOV = 0.1f;
		
		private static CameraFollow s_instance = null;
		public static CameraFollow Instance
		{
			get
			{
				if (s_instance == null && Camera.main != null)
				{
					s_instance = Camera.main.GetComponent<CameraFollow>();
				}
				return s_instance;
			}
		}
		private static Dictionary<int, CameraFollow> s_instances = new Dictionary<int, CameraFollow>();
		public static CameraFollow GetInstance(PlayerController p_ctrl)
		{
			CameraFollow cam;
			s_instances.TryGetValue(p_ctrl.GetHashCode(), out cam);
			return cam;
		}
		
		public enum State
		{
			FollowPlayerAlive,
			FollowPlayerDead,
			ReplayCinematic
		}
		
		public Transform target = null;
		public float maxDistance = 50.0f;
		public float height = 3.0f;
		public float targetHeight = 1.75f;
		
		public State cameraState = State.FollowPlayerAlive;
		
		private Vector3 m_cameraSmoothV = Vector3.zero;
		private Vector3 m_dampedSmoothPos = Vector3.zero;
		private float m_cameraSmoothFOV_V = 0f;
		
		private float m_cameraHeight = 0f;
		private float m_cameraHeightSmoothDampV = 0f;
		private float m_targetHeight = 1.75f;
		private float m_targetHeightSmoothDampV = 0f;

		private float m_maxDistance = 1f;

		private Vector3 m_followPosition = Vector3.zero;
		
		private bool m_isOcclusion = false;
		
		private float m_origFOV;
		
		private bool m_isHipBoneSet = false;

		public void SetTarget(Transform p_target)
		{
			target = p_target;
			m_isHipBoneSet = false;
		}

		public void SetPosition(Vector3 p_position)
		{
			m_cameraSmoothV = Vector3.zero;
			m_followPosition = p_position - 0.5f*height*Vector3.up;
			if (target != null)
			{
				m_dampedSmoothPos = p_position-target.position;
			}
			transform.position = p_position;
		}
		
		// Use this for initialization
		void Start()
		{
			m_cameraHeight = height;
			m_targetHeight = targetHeight;
			m_followPosition = transform.position;
			if (target != null)
			{
				m_dampedSmoothPos = transform.position-target.position;
			}
			m_origFOV = GetComponent<Camera>().fieldOfView;
			m_maxDistance = maxDistance;
		}

		void OnDestroy()
		{
			if (s_instances.ContainsValue(this))
			{
				Dictionary<int, CameraFollow>.KeyCollection keys = s_instances.Keys;
				for (int i = 0; i < keys.Count; i++)
				{
					foreach (int hash in keys)
					{
						if (s_instances[hash] == this)
						{
							s_instances.Remove(hash);
							return;
						}
					}
				}
			}
		}

		// Update is called once per frame
		void LateUpdate()
		{
			if (target != null) // target is null when player is destroyed
			{
				if (cameraState == State.ReplayCinematic)
				{
					m_targetHeight = CINEMATIC_TARGET_HEIGHT;
					// set camera position
					m_followPosition = Vector3.forward*CINEMATIC_CAM_DISTANCE;
					m_followPosition = Quaternion.Euler(0f, CINEMATIC_ROT_SPEED*Time.time, 0f) * m_followPosition;
					m_followPosition += target.position;
					// set camera fov
					GetComponent<Camera>().fieldOfView = m_origFOV;
					m_maxDistance = maxDistance;
					// apply
					transform.position = m_followPosition + CINEMATIC_HEIGHT * Vector3.up;
				}
				else if (cameraState == State.FollowPlayerDead)
				{
					Vector3 distanceOrig;
					Vector3 distanceChange;
					m_followPosition -= DEAD_STATE_UP_FLOATING * Time.deltaTime * new Vector3(0.0f, 0.99f, 0.1f);
					fixUpFollowPos(out distanceOrig, out distanceChange);
					
					setTransformPositionDamped(target.position + distanceOrig - distanceChange + height * Vector3.up, 0f);
					
					// set camera fov
					GetComponent<Camera>().fieldOfView = Mathf.SmoothDamp(GetComponent<Camera>().fieldOfView, m_origFOV, ref m_cameraSmoothFOV_V, CAMERA_SMOOTH_TIME_FOV);
					m_maxDistance = maxDistance;
				}
				else
				{
					if (!m_isHipBoneSet)
					{
						GameObject playerModel = PlayerAnimationHelper.GetPlayerModel(target.gameObject);
						if (playerModel != null)
						{
							Transform hips = PlayerAnimationHelper.GetHipsBone(playerModel);
							if (hips != null)
							{
								target = hips;
								m_isHipBoneSet = true;

								// change height for big models
								if (playerModel.name.Contains("Werewolf") || playerModel.name.Contains("Golem"))
								{
									height += 0.5f;
								}

								// cache link with player controller
								PlayerController ctrl = playerModel.transform.root.GetComponent<PlayerController>();
								if (ctrl != null)
								{
									CameraFollow cachedCam;
									if (!s_instances.TryGetValue(ctrl.GetHashCode(), out cachedCam))
									{
										s_instances.Add(ctrl.GetHashCode(), this);
									}
									else if (cachedCam != this)
									{
										Debug.LogError("CameraFollow: player controller of a follow cam should not change on runtime!\n" +
										               "player controller: '"+ctrl.name+"' follow cam old: '"+cachedCam.name+"' follow cam new '"+name+"'");
									}
								}
							}
						}
					}
					
					PlayerController playerCTRL = target.root.GetComponent<PlayerController>();
					float playerV = 0f;
					if (playerCTRL != null)
					{
						// set camera fov
						GetComponent<Camera>().fieldOfView = Mathf.Min(118, Mathf.SmoothDamp(GetComponent<Camera>().fieldOfView, m_origFOV + playerCTRL.Velocity.magnitude * SPEED_TO_FOV_FACTOR, ref m_cameraSmoothFOV_V, CAMERA_SMOOTH_TIME_FOV));
						
						// correct camera distance
						float camFOVfactor = m_origFOV / GetComponent<Camera>().fieldOfView;
						m_maxDistance = Mathf.Max(0.4f, maxDistance * camFOVfactor);

						// follow player
						float nextTargetHeigth = targetHeight*camFOVfactor;
						if (playerCTRL.State == PlayerState.Air)
						{
							nextTargetHeigth *= 0.7f;
						}
						m_targetHeight = Mathf.SmoothDamp(m_targetHeight, nextTargetHeigth, ref m_targetHeightSmoothDampV, AIR_LOOK_SPEED);
						
						float nextCamHeight = height*camFOVfactor;
						if (playerCTRL.State != PlayerState.Air)
						{
							nextCamHeight *= playerCTRL.BordUpVector.y;
						}
						m_cameraHeight = Mathf.SmoothDamp(m_cameraHeight, nextCamHeight, ref m_cameraHeightSmoothDampV, CAM_HEIGHT_SPEED);

						Vector3 distanceOrig;
						Vector3 distanceChange;
						fixUpFollowPos(out distanceOrig, out distanceChange);

						Vector3 targetV = target.root.gameObject.GetComponent<Rigidbody>().velocity;
						playerV = targetV.magnitude;
						if (playerV > 0.001 && distanceChange.magnitude > 0.001)
						{
							Vector3 transformPosBK = transform.position;
							Vector3 followPosBK = m_followPosition;
							transform.position = m_followPosition;
							transform.RotateAround(target.position, Vector3.Cross(distanceOrig, targetV), Vector3.Angle(distanceOrig, targetV));
							m_followPosition = transform.position;
							transform.position = transformPosBK;
							
							Vector3 bordUp = playerCTRL.BordUpVector;
							float moveUp = Vector3.Dot(bordUp, followPosBK - m_followPosition);
							m_followPosition += moveUp*bordUp;
							
							fixUpFollowPos();
						}
					}
					else
					{
						m_maxDistance = maxDistance;
					}
					
					setTransformPositionDamped(m_followPosition + m_cameraHeight * Vector3.up, playerV);
				}
				
				transform.LookAt(target.position + m_targetHeight * Vector3.up);
				
				fixUpOcclusion();
			}
		}
		
		private void fixUpFollowPos(out Vector3 o_distanceOrig, out Vector3 o_distanceChange)
		{
			Vector3 targetPos = target.position;
			o_distanceOrig = targetPos - m_followPosition;
			
			float distanceLength = o_distanceOrig.magnitude;
			
			if (distanceLength > 0.000001f && distanceLength != m_maxDistance)
			{
				if (m_isOcclusion && distanceLength < m_maxDistance)
				{
					o_distanceChange = Vector3.zero;
					return;
				}
				else
				{
					m_isOcclusion = false;
				}
					
				float moveDist = distanceLength - m_maxDistance;
				
				o_distanceChange = o_distanceOrig * (moveDist / distanceLength);
			
				m_followPosition += o_distanceChange;
			}
			else
			{
				o_distanceChange = Vector3.zero;
			}
		}
		
		private void fixUpOcclusion()
		{
			Vector3 direction = target.position - transform.position;
			float distance = direction.magnitude;
			direction.Normalize();
			RaycastHit hitInfo;
			float ignoreDist = 2.0f;
			if (ignoreDist < distance)
			{
				if (Physics.Raycast(target.position - direction*ignoreDist, -direction, out hitInfo, distance-ignoreDist))
				{
					m_followPosition = hitInfo.point + direction * 2.0f;
					m_isOcclusion = true;
				}
				else if (Physics.Raycast(transform.position,direction, out hitInfo, distance - ignoreDist))
				{
					m_followPosition = hitInfo.point + direction * 2.0f;
					m_isOcclusion = true;
				}
			}
		}
		
		private void fixUpFollowPos()
		{
			Vector3 dummyA;
			Vector3 dummyB;
			
			fixUpFollowPos(out dummyA, out dummyB);
		}
		
		private void setTransformPositionDamped(Vector3 newPosition, float targetV)
		{
			Vector3 nextDampedSmoothPos = newPosition-target.position;
			m_dampedSmoothPos = Vector3.SmoothDamp(m_dampedSmoothPos, nextDampedSmoothPos, ref m_cameraSmoothV, CAMERA_SMOOTH_TIME, Mathf.Infinity, Time.deltaTime);
			transform.position = target.position + m_dampedSmoothPos;
		}
	}
}
