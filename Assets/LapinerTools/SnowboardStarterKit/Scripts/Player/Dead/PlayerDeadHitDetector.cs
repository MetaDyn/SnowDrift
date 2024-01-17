using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerDeadHitDetector : MonoBehaviour
	{
		protected const float MIN_HIT_V = 5.0f;

		private GameObject m_root = null;

		protected Vector3 m_lastV = Vector3.zero;

		protected virtual void Awake()
		{
			m_root = transform.root.gameObject;
		}

		protected void Update()
		{
			m_lastV = GetComponent<Rigidbody>().velocity;
		}
		
		protected void OnCollisionEnter(Collision collisionInfo)
		{
			CheckCollision(collisionInfo);
		}
		
		protected void OnCollisionStay(Collision collisionInfo)
		{
			CheckCollision(collisionInfo);
		}

		/// <summary>
		/// Returns hit power.
		/// </summary>
		protected virtual float CheckCollision(Collision collisionInfo)
		{
			if (m_root == collisionInfo.transform.root.gameObject)
			{
				return -1; // do somthing only if the collision is not inside the same root object
			}
			
			// handle hit strength
			if ((m_lastV - GetComponent<Rigidbody>().velocity).magnitude > MIN_HIT_V)
			{
				float hitPower = (m_lastV - GetComponent<Rigidbody>().velocity).magnitude * 0.03f;
				if (hitPower >= 0.5f)
				{
					// send a message to the sound manager
					gameObject.SendMessageUpwards("OnPlayerHitHard");
				}
				else if (hitPower >= 0.25f)
				{
					// send a message to the sound manager
					gameObject.SendMessageUpwards("OnPlayerHitMessage", Mathf.Max(hitPower * 2.1f, Mathf.Max(0.25f, GetComponent<Rigidbody>().velocity.magnitude / 10.0f)));
				}
				return hitPower;
			}
			else
			{
				return 0;
			}
		}
	}
}
