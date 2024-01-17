using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Snowboard
{
	public static class PlayerAnimationHelper
	{	
		public static GameObject GetPlayerModel(GameObject PlayerControllerGO)
		{
			PlayerModelData modelData = PlayerControllerGO.GetComponentInChildren<PlayerModelData>();
			if (modelData != null)
			{
				return modelData.gameObject;
			}
			else
			{
				return null;
			}
		}

		public static string GetDeadResourcePath(GameObject PlayerModel)
		{
			PlayerModelData playerModelData = PlayerModel.GetComponent<PlayerModelData>();
			if (playerModelData == null)
			{
				Debug.LogError("PlayerAnimationHelper: GetDeadResourcePath: could not find a PlayerModelData component in " + PlayerModel.name);
				return null;
			}
			return playerModelData.DeadPrefabPath;
		}

		public static Transform GetHipsBone(GameObject PlayerModel)
		{
			PlayerModelData playerModelData = PlayerModel.GetComponent<PlayerModelData>();
			if (playerModelData == null)
			{
				Debug.LogError("PlayerAnimationHelper: GetHipsBone: could not find a PlayerModelData component in " + PlayerModel.name);
				return null;
			}
			return playerModelData.HipsBone;
		}

		public static Transform[] GetAllBones(GameObject PlayerModel)
		{
			if (PlayerModel == null)
			{
				Debug.LogError("PlayerAnimationHelper: GetAllBones: parameter PlayerModel is null!");
				return new Transform[0];
			}
			PlayerModelData playerModelData = PlayerModel.GetComponent<PlayerModelData>();
			if (playerModelData == null)
			{
				Debug.LogError("PlayerAnimationHelper: GetAllBones: could not find a PlayerModelData component in " + PlayerModel.name);
				return new Transform[0];
			}
			return GetAllBones(playerModelData.HipsBone);
		}

		public static Transform[] GetAllBones(Transform HipsBone)
		{
			return HipsBone.parent.GetComponentsInChildren<Transform>(true);
		}

		public static Transform[] GetFixedJointsOnDeath(GameObject PlayerModel)
		{
			PlayerModelData playerModelData = PlayerModel.GetComponent<PlayerModelData>();
			if (playerModelData == null)
			{
				Debug.LogError("PlayerAnimationHelper: FixedJointsOnDeath: could not find a PlayerModelData component in " + PlayerModel.name);
				return null;
			}
			return playerModelData.FixedJointsOnDeath;
		}

		public static Transform[] GetDisabledCollidersOnLandAnim(GameObject PlayerModel)
		{
			PlayerModelData playerModelData = PlayerModel.GetComponent<PlayerModelData>();
			if (playerModelData == null)
			{
				Debug.LogError("PlayerAnimationHelper: GetDisabledCollidersOnLandAnim: could not find a PlayerModelData component in " + PlayerModel.name);
				return null;
			}
			return playerModelData.DisabledCollidersOnLandAnim;
		}

		public static Collider[] GetIgnoreCollisionWithPlayerCollider(GameObject PlayerModel)
		{
			PlayerModelData playerModelData = PlayerModel.GetComponent<PlayerModelData>();
			if (playerModelData == null)
			{
				Debug.LogError("PlayerAnimationHelper: GetIgnoreCollisionWithPlayerCollider: could not find a PlayerModelData component in " + PlayerModel.name);
				return null;
			}
			return playerModelData.IgnoreCollisionWithPlayerCollider;
		}

		public static Collider[] GetAdditionalColliderOnDeath(GameObject PlayerModel)
		{
			PlayerModelData playerModelData = PlayerModel.GetComponent<PlayerModelData>();
			if (playerModelData == null)
			{
				Debug.LogError("PlayerAnimationHelper: GetAdditionalColliderOnDeath: could not find a PlayerModelData component in " + PlayerModel.name);
				return null;
			}
			return playerModelData.AdditionalColliderOnDeath;
		}

		public static bool GetIsModelRotatedBy90Degrees(GameObject PlayerModel)
		{
			PlayerModelData playerModelData = PlayerModel.GetComponent<PlayerModelData>();
			if (playerModelData == null)
			{
				Debug.LogError("PlayerAnimationHelper: GetIsModelRotatedBy90Degrees: could not find a PlayerModelData component in " + PlayerModel.name);
				return false;
			}
			return playerModelData.IsModelRotatedBy90Degrees;
		}

		public static void CopyAliveBoneSettings(Transform bone, Transform bonePrefab)
		{
			// copy collider
			copyColliderAlive(bone, bonePrefab);
		}
		
		public static void CopyDeadBoneSettings(Transform bone, Transform bonePrefab, Vector3 velocity)
		{
			if (CopyRigidBodyAndJoints (bone, bonePrefab, velocity))
			{
				// set up collider
				setDeadColliderSettings(bone);

				if (bonePrefab.GetComponent<PlayerDeadHitDetector>() != null)
				{
					bone.gameObject.AddComponent<PlayerDeadHitDetector>();
				}
			}
		}
		
		public static void MoveToLastPhysicsFrame(Transform root, Vector3 velocity)
		{
			// move back
			float GO_BACK_DIST = velocity.magnitude * Time.fixedDeltaTime;
			root.position = root.position - velocity.normalized * GO_BACK_DIST;
		}
		
		public static void ConnectDeadBodyJoints(Transform[] bones, Transform fallbackBoneWithRigidbody)
		{
			foreach(Transform bone in bones)
			{
				CharacterJoint joint = bone.GetComponent<CharacterJoint>();
				// connect joint
				if (joint != null)
				{
					Transform parentBody = bone.parent;
					while (parentBody != null && parentBody.GetComponent<Rigidbody>() == null)
					{
						parentBody = parentBody.parent;
					}
					if (parentBody == null)
					{
						if (fallbackBoneWithRigidbody != bone)
						{
							parentBody = fallbackBoneWithRigidbody;
						}
						else
						{
							Debug.LogError("PlayerAnimationHelper: ConnectDeadBodyJoints: could not find parent rigidbody for " +
								"bone '" + bone.name + "'!");
							continue;
						}
					}
					joint.connectedBody = parentBody.GetComponent<Rigidbody>();
					// set collision ignore
					if (bone.GetComponent<Collider>() != null)
					{
						Transform parentBodyWithCollider = parentBody;
						while (parentBodyWithCollider != null &&
							parentBodyWithCollider.GetComponent<Collider>() == null)
						{
							parentBodyWithCollider = parentBodyWithCollider.parent;
						}
						if (parentBodyWithCollider != null)
						{
							Physics.IgnoreCollision(bone.GetComponent<Collider>(), parentBodyWithCollider.GetComponent<Collider>());
						}
					}
				}
			}
		}
		
		public static bool CopyRigidBodyAndJoints(Transform bone, Transform bonePrefab, Vector3 velocity)
		{
			Rigidbody bodyPrefab = bonePrefab.GetComponent<Rigidbody>();
			if (bodyPrefab != null)
			{
				// copy rigidbody
				Rigidbody body = bone.gameObject.GetComponent<Rigidbody>();
				if (body == null)
				{
					body = bone.gameObject.AddComponent<Rigidbody>();
				}
				else if (body.isKinematic)
				{
					body.isKinematic = false;
				}
				body.interpolation = RigidbodyInterpolation.Interpolate;
				body.mass = bodyPrefab.mass;
				body.velocity = velocity;

				// copy joints
				CharacterJoint jointPrefab = bonePrefab.GetComponent<CharacterJoint>();
				if (jointPrefab != null)
				{
					CharacterJoint joint = bone.gameObject.AddComponent<CharacterJoint>();
					joint.anchor = jointPrefab.anchor;
					joint.swing1Limit = jointPrefab.swing1Limit;
					joint.swing2Limit = jointPrefab.swing2Limit;
					joint.axis = jointPrefab.axis;
					joint.swingAxis = jointPrefab.swingAxis;
					joint.lowTwistLimit = jointPrefab.lowTwistLimit;
					joint.highTwistLimit = jointPrefab.highTwistLimit;
				}
				return true;
			}
			return false;
		}
		
		public static bool CopyCollider(Transform bone, Transform bonePrefab)
		{
			// copy collider
			if (bone.GetComponent<Collider>() == null && bonePrefab.GetComponent<Collider>() != null)
			{
				BoxCollider boxColliderPrefab = bonePrefab.GetComponent<BoxCollider>();
				if (boxColliderPrefab == null)
				{
					CapsuleCollider capsuleColliderPrefab = bonePrefab.GetComponent<CapsuleCollider>();
					if (capsuleColliderPrefab == null)
					{
						SphereCollider sphereColliderPrefab = bonePrefab.GetComponent<SphereCollider>();
						SphereCollider sphereCollider = bone.gameObject.AddComponent<SphereCollider>();
						sphereCollider.center = sphereColliderPrefab.center;
						sphereCollider.radius = sphereColliderPrefab.radius;
					}
					else
					{
						CapsuleCollider capsuleCollider = bone.gameObject.AddComponent<CapsuleCollider>();
						capsuleCollider.center = capsuleColliderPrefab.center;
						capsuleCollider.radius = capsuleColliderPrefab.radius;
						capsuleCollider.height = capsuleColliderPrefab.height;
						capsuleCollider.direction = capsuleColliderPrefab.direction;
					}
				}
				else
				{
					BoxCollider boxCollider = bone.gameObject.AddComponent<BoxCollider>();
					boxCollider.center = boxColliderPrefab.center;
					boxCollider.size = boxColliderPrefab.size;
				}
				bone.GetComponent<Collider>().material = bonePrefab.GetComponent<Collider>().sharedMaterial;
				return true;
			}
			return false;
		}
		
		private static void copyColliderAlive(Transform bone, Transform bonePrefab)
		{
			// copy collider
			if (CopyCollider(bone, bonePrefab))
			{
				// attach collision reporter
				bone.GetComponent<Collider>().isTrigger = true;
				if (!bone.name.ToLower().Contains("leg") &&
				    !bone.name.ToLower().Contains("joint_hiplt") &&
				    !bone.name.ToLower().Contains("joint_hiprt") &&
				    !bone.name.ToLower().Contains("shoulder") &&
				    !bone.name.ToLower().Contains("thigh") &&
				    !bone.name.ToLower().Contains("knee") &&
				    !bone.name.ToLower().Contains("calf") &&
				    !bone.name.ToLower().Contains("shin") &&
				    !bone.name.ToLower().Contains("elbow") &&
				    !bone.name.ToLower().Contains("arm"))
				{
					bone.gameObject.AddComponent<PlayerCollisonReporter>();
				}
				else
				{
					bone.GetComponent<Collider>().enabled = false;
				}
			}
		}
		
		private static void setDeadColliderSettings(Transform bone)
		{
			if (bone.GetComponent<Collider>() != null)
			{
				// remove collision reporter
				GameObject.Destroy(bone.gameObject.GetComponent<PlayerCollisonReporter>());

				bone.GetComponent<Collider>().enabled = true;
				bone.GetComponent<Collider>().isTrigger = false;
			}
		}
	}
}
