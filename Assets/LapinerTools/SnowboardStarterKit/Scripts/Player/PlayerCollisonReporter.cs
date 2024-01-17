using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerCollisonReporter : MonoBehaviour
	{
		private GameObject m_player = null;
		
		void Awake()
		{
			m_player = transform.root.gameObject;
			if (m_player == null)
			{
				Debug.LogWarning("PlayerCollisonReporter: PlayerController is not found!");
			}
		}
		
		void OnTriggerEnter(Collider other)
		{
			if (m_player != null &&
			    other.GetComponent<LevelEndSurface>() == null &&
			    !other.GetComponent<Collider>().isTrigger &&
			    other.transform.root != m_player.transform)
			{
				if (other.tag == "Locker")
				{
					m_player.SendMessage("OnPlayerCollisionLocker");
				}
				else
				{
					m_player.SendMessage("OnPlayerCollision");
					Debug.Log("PlayerCollisonReporter: OnTriggerEnter: '" + name + "' (root '" + transform.root.name + "') with '" + other.name + "' (root '" + other.transform.root.name + "')");
				}
			}
		}
	}
}