using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class LevelEndSurface : MonoBehaviour
	{	
		void OnTriggerEnter (Collider other)
		{
			collision(other.gameObject);
		}
		
		void OnCollisionEnter (Collision collisionInfo)
		{
			collision(collisionInfo.gameObject);
		}
		
		private void collision (GameObject pGameObject)
		{
			// check if it is really the player falling on this surface
			if (pGameObject.transform.root.tag == "Player" &&
			    // check if level is not failed (for example timeout) or finished (already called this method)
				!GameLevelHandler.Instance.IsLevelFinishedOrFailed)
			{
				PlayerController ctrl = pGameObject.transform.root.GetComponent<PlayerController>();
				ctrl.SetState(PlayerState.LevelEnd);
				GameLevelHandler.Instance.EndLevel(ctrl);
				
				if (GetComponent<AudioSource>() != null)
				{
					GetComponent<AudioSource>().Play();
				}
				else
				{
					Debug.LogError("LevelEndSurface: owner game object has no sound source attached!");
				}
			}
		}
	}
}
