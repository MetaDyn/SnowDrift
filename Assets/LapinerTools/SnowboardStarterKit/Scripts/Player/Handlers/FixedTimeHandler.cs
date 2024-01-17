using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Snowboard
{
	public class FixedTimeHandler : MonoBehaviour
	{
		public static FixedTimeHandler s_instance = null;
		public static FixedTimeHandler Instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = FindObjectOfType<FixedTimeHandler>();
				}
				if (s_instance == null)
				{
					s_instance = new GameObject("FixedTimeHandler", typeof(FixedTimeHandler)).GetComponent<FixedTimeHandler>();
				}
				return s_instance;
			}
		}

		private float m_nextDeltaTime = -1f;
#if UNITY_EDITOR
		private float m_updateLoopTime = -1f;
#endif

#if UNITY_EDITOR
		private void Awake()
		{
			m_updateLoopTime = Time.fixedTime;
		}
#endif

		private void Start()
		{
			StartCoroutine(UpdateFixedTime());
		}

		public void SetFixedTime(float p_fixedDeltaTime)
		{
#if UNITY_EDITOR
			if (m_updateLoopTime + 1f < Time.fixedTime)
			{
				Debug.LogError("FixedTimeHandler: SetFixedTime: update loop 'UpdateFixedTime' was not started! Start it in a coroutine!");
			}
#endif
			if (m_nextDeltaTime == -1)
			{
				m_nextDeltaTime = p_fixedDeltaTime;
			}
			else if (m_nextDeltaTime > p_fixedDeltaTime)
			{
				m_nextDeltaTime = p_fixedDeltaTime;
			}
		}

		private System.Collections.IEnumerator UpdateFixedTime()
		{
			WaitForFixedUpdate wait = new WaitForFixedUpdate();
			while (true)
			{
#if UNITY_EDITOR
				m_updateLoopTime = Time.fixedTime;
#endif
				if (m_nextDeltaTime != -1)
				{
					Time.fixedDeltaTime = m_nextDeltaTime;
					m_nextDeltaTime = -1f;
				}
				yield return wait;
			}
		}
	}
}
