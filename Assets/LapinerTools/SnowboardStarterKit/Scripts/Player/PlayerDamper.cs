using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Snowboard
{
	public class PlayerDamper : MonoBehaviour
	{
		public PlayerController PlayerCTRL = null;

		private Rigidbody m_cachedRigidBody = null;
		private List<Collision> m_collData = new List<Collision>();
		
		void Awake()
		{
			if (PlayerCTRL == null)
			{
				Debug.LogWarning("PlayerDamper: PlayerController is not assigned!");
			}
			m_cachedRigidBody = GetComponent<Rigidbody>();
			Physics.IgnoreCollision(PlayerCTRL.GetComponent<Collider>(), GetComponent<Collider>());
		}
		
		void FixedUpdate()
		{
			// flush collision at the end of a frame
			foreach(Collision coll in m_collData)
			{
				if (coll != null && coll.collider != null && coll.gameObject != null) // could have been destroyed by now
				{
					PlayerCTRL.OnCollisionExtern(coll);
				}
			}
			m_collData.Clear();

			// additional damping
			m_cachedRigidBody.velocity = PlayerCTRL.Velocity * 0.25f + m_cachedRigidBody.velocity * 0.75f;
		}
		
		void OnCollisionEnter(Collision collisionInfo)
		{
			m_collData.Add(collisionInfo);
		}
		
		void OnCollisionStay(Collision collisionInfo)
		{
			m_collData.Add(collisionInfo);
		}
	}
}