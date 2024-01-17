using UnityEngine;
using System.Collections;
using MT_MeshTrail;

namespace Snowboard
{
	public class PlayerSnowTrailUpdater : MonoBehaviour
	{
		private enum Direction { FORWARD, BACKWARD, RIGHT, LEFT, UP, DOWN }

		private const float MIN_PLAYER_GROUND_STATE_STABILITY = 0.75f;
		private const float MIN_PLAYER_GROUND_STATE_STABILITY_INV = 1f - MIN_PLAYER_GROUND_STATE_STABILITY;

		[SerializeField]
		private float ADDITIONAL_SCALE = 0.4f;
		[SerializeField]
		private Direction PARENT_UP = Direction.UP;
		[SerializeField]
		private Direction PARENT_FORWARD = Direction.FORWARD;

		private TerrainCollider m_terrainCollider;
		private PlayerController m_player;
		private float m_yScale = 0f;
		private MT_MeshTrailRenderer m_trail;
		private Vector3 m_lastPos;
		private Vector3 m_originalScale;
		private Vector3 m_originalLocalPosition;

		private Ray m_cachedRay = new Ray(Vector3.zero, Vector3.down);

		public bool IsDrawing { get{ return m_trail != null ? m_trail.IsDrawing : false; } }

		void Start ()
		{
			// try find player, but also works without player controller
			m_player = transform.root.GetComponent<PlayerController>();
			// get trail
			m_trail = GetComponent<MT_MeshTrailRenderer>();
			if (m_trail == null)
			{
				Debug.LogError("SnowboardMeshTrailUpdater: could not find a MT_MeshTrailRenderer component!");
				Destroy(this);
				return;
			}
			else
			{
				// save the trail setup
				m_originalScale = m_trail.Scale;
				m_originalLocalPosition = m_trail.transform.localPosition;
			}
			m_lastPos = transform.position;
			if (transform.parent == null)
			{
				Debug.LogError("SnowboardMeshTrailUpdater: needs snowboard as parent!");
				m_trail.IsDrawing = false;
				Destroy(this);
				return;
			}
			// get level's terrain
			TryFindTerrainCollider();
		}

		private void TryFindTerrainCollider()
		{
			if (Terrain.activeTerrain != null)
			{
				m_terrainCollider = Terrain.activeTerrain.GetComponent<TerrainCollider>();
				if (m_terrainCollider == null)
				{
					Debug.LogError("Could not find terrain collider!");
					m_trail.IsDrawing = false;
					Destroy(this);
					return;
				}
			}
		}

		private void Update ()
		{
			bool isDrawing = UpdateIsDrawing();
			m_trail.IsDrawing = isDrawing;
			if (isDrawing)
			{
				UpdateTransformAndScale();
			}
		}

		private bool UpdateIsDrawing()
		{
			if (m_player != null)
			{
				// if player is alive easily get state
				float groundStability = m_player.GroundStateStability;
				m_yScale = Mathf.Clamp01((groundStability-MIN_PLAYER_GROUND_STATE_STABILITY) / MIN_PLAYER_GROUND_STATE_STABILITY_INV);
				m_yScale *= m_yScale;
				return m_player.State == PlayerState.Ground &&
					groundStability >= MIN_PLAYER_GROUND_STATE_STABILITY &&
					m_player.IsCollidingWithTerrain;
			}
			else if (m_terrainCollider != null)
			{
				// if no player script found check for collision of the snowboard by raycasting
				RaycastHit hitInfo;
				transform.localPosition = m_originalLocalPosition;
				m_cachedRay.origin = transform.position+Vector3.up;
				if (m_terrainCollider.Raycast(m_cachedRay, out hitInfo, 1.45f))
				{
					transform.position = hitInfo.point + Vector3.up*0.05f;
					m_yScale = 1f;
					return true;
				}
				m_yScale = 0f;
			}
			else
			{
				// try to find terrain collider
				TryFindTerrainCollider();
			}
			return false;
		}

		private void UpdateTransformAndScale()
		{
			// changes can only be calculated when there is movement (without movement there is no trail anyway)
			Vector3 forward = transform.position - m_lastPos;
			if (forward.sqrMagnitude > 0.1f)
			{
				// change the width of the trail depending on angle between the snowboard and the motion direction
				Vector3 parentUp = GetParentDirection(PARENT_UP);
				Vector3 parentForward = GetParentDirection(PARENT_FORWARD);
				float scaleFactor = 1f-Mathf.Abs(Vector3.Dot(forward.normalized, parentForward));
				scaleFactor = Mathf.Clamp01(scaleFactor*1.2f);
				Vector3 newScale = m_originalScale;
				newScale.x += scaleFactor*ADDITIONAL_SCALE;
				newScale.y *= m_yScale;
				m_trail.Scale = newScale;
				// transform direction
				// in motion direction - when the scale is 0 or 1
				// in board lateral direction (still towards movement) - when the scale is 0.7
				Vector3 boardLatDir = Vector3.Cross(parentForward, parentUp);
				if (Vector3.Dot(forward.normalized, boardLatDir) < 0)
				{
					boardLatDir *= -1f; // riding switch
				}
				float directionFactor;
				if (scaleFactor <= 0.7f)
				{
					directionFactor = scaleFactor / 0.7f;
				}
				else
				{
					directionFactor = 1f - (scaleFactor-0.7f) / 0.3f;
				}
				Vector3 lookAtDir = Vector3.Lerp(forward, boardLatDir, directionFactor);
				transform.LookAt(transform.position + lookAtDir, parentUp);
				// save this position as last position
				m_lastPos = transform.position;
			}
		}

		private Vector3 GetParentDirection(Direction pDirection)
		{
			switch (pDirection)
			{
				case Direction.FORWARD: return transform.parent.forward;
				case Direction.BACKWARD: return -transform.parent.forward;
				case Direction.RIGHT: return transform.parent.right;
				case Direction.LEFT: return -transform.parent.right;
				case Direction.UP: return transform.parent.up;
				case Direction.DOWN: return -transform.parent.up;
				default: return transform.parent.forward; 
			}
		}
	}
}
