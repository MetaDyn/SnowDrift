using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Snowboard
{
	public class SleepingRigidbody : MonoBehaviour
	{
		private void Start()
		{
			Rigidbody body = GetComponent<Rigidbody>();
			if (body != null)
			{
				body.Sleep();
			}
		}
	}
}
