using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerModelData : MonoBehaviour
	{
		[SerializeField]
		private string m_deadPrefabPath = null;
		public string DeadPrefabPath { get{ return m_deadPrefabPath; } }

		[SerializeField]
		private Transform m_hipsBone = null;
		public Transform HipsBone { get{ return m_hipsBone; } }
		
		[SerializeField]
		private bool m_isModelRotatedBy90Degrees = true;
		public bool IsModelRotatedBy90Degrees { get{ return m_isModelRotatedBy90Degrees; } }

		[SerializeField]
		private float m_blobShadowScale = 1f;
		public float BlobShadowScale { get{ return m_blobShadowScale; } }
		
		[SerializeField]
		private float m_blobShadowYOffset = 0f;
		public float BlobShadowYOffset { get{ return m_blobShadowYOffset; } }

		[SerializeField]
		private Transform[] m_disabledCollidersOnLandAnim = new Transform[0];
		public Transform[] DisabledCollidersOnLandAnim { get{ return m_disabledCollidersOnLandAnim; } }

		[SerializeField]
		private Transform[] m_fixedJointsOnDeath = new Transform[0];
		public Transform[] FixedJointsOnDeath { get{ return m_fixedJointsOnDeath; } }

		[SerializeField]
		private Collider[] m_ignoreCollisionWithPlayerCollider = new Collider[0];
		public Collider[] IgnoreCollisionWithPlayerCollider { get{ return m_ignoreCollisionWithPlayerCollider; } }

		[SerializeField]
		private Collider[] m_additionalColliderOnDeath = new Collider[0];
		public Collider[] AdditionalColliderOnDeath { get{ return m_additionalColliderOnDeath; } }

#if UNITY_EDITOR
		private void Awake ()
		{
			if (m_hipsBone == null)
			{
				Debug.LogError("PlayerModelData: m_hipsBone is not set! Set it in the inspector!");
			}
		}
#endif
	}
}
