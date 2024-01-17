using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerMovementLocker : MonoBehaviour
	{
		private const float MIN_DIST = 0.5f;

		[SerializeField]
		private Transform[] m_lockPoints = new Transform[0];
		public Transform[] LockPoints { set{ m_lockPoints=value; } get{ return m_lockPoints; } }

		public bool GetLockPoints(Vector3 p_position, Vector3 p_velocity, out Transform o_closestInMotionDir, out Transform o_closestInOppositeDir)
		{
			o_closestInMotionDir = null;
			o_closestInOppositeDir = null;
			if (LockPoints.Length >= 2)
			{
				// find the closest point in direction of movement and one in the opposite direction
				float distInMotionDir = float.MaxValue;
				float distInOppositeDir = float.MaxValue;
				for (int i = 0; i < m_lockPoints.Length; i++)
				{
					Vector3 distVect = m_lockPoints[i].position - p_position;
					float dist = distVect.sqrMagnitude;
					if (Vector3.Dot(distVect, p_velocity) > MIN_DIST)
					{
						if (distInMotionDir > dist && o_closestInOppositeDir != m_lockPoints[i])
						{
							distInMotionDir = dist;
							o_closestInMotionDir = m_lockPoints[i];
						}
					}
					else
					{
						if (distInOppositeDir > dist && o_closestInMotionDir != m_lockPoints[i])
						{
							distInOppositeDir = dist;
							o_closestInOppositeDir = m_lockPoints[i];
						}
					}
				}
				if (o_closestInMotionDir != null && o_closestInOppositeDir != null)
				{
					return true;
				}
			}
			return false;
		}
	}
}
