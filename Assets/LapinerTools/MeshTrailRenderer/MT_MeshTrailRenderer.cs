using UnityEngine;
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3_OR_NEWER
using UnityEngine.Rendering;
#endif
using System.Collections;

namespace MT_MeshTrail
{
	/// <summary>
	/// The MT_MeshTrailRenderer is the heart of this package. This script allows you to draw a 3d trail, 
	/// which is independent of the camera position and angle (in contrast to the Unity built-in TrailRenderer).
	/// </summary>
	public class MT_MeshTrailRenderer : MonoBehaviour
	{
		public static readonly Vector3 INIT_LAST_DRAW_LOCATION = new Vector3(-9999.123f,0f,0f);
		
		[SerializeField]
		private Material m_material = null;
		/// <summary>
		/// Material of the generated trail mesh.
		/// </summary>
		public Material Material
		{
			get{ return m_material; }
			set
			{
				m_material = value;
				if (m_renderer != null)
				{
					m_renderer.sharedMaterial = m_material;
				}
			}
		}
		[SerializeField]
		private bool m_castShadows = false;
		/// <summary>
		/// Cast shadows setting of the generated trail mesh.
		/// </summary>
		public bool CastShadows
		{
			get{ return m_castShadows; }
			set
			{
				m_castShadows = value;
				if (m_renderer != null)
				{
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3_OR_NEWER
					m_renderer.shadowCastingMode = m_castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
#else
					m_renderer.castShadows = m_castShadows;
#endif
				}
			}
		}
		[SerializeField]
		private bool m_receiveShadows = false;
		/// <summary>
		/// Receive shadows setting of the generated trail mesh.
		/// </summary>
		public bool ReceiveShadows
		{
			get{ return m_receiveShadows; }
			set
			{
				m_receiveShadows = value;
				if (m_renderer != null)
				{
					m_renderer.receiveShadows = m_receiveShadows;
				}
			}
		}

		[SerializeField]
		// This Vector3 array contains the vertices of the edge between trail mesh steps. Vertices are defined in 
		// local coordinates of the game object that has this script attached. 
		private Vector3[] TRAIL_MESH_VERTICES = new Vector3[4]
		{
			new Vector3(-1.25f, 0.5f, 0f),
			new Vector3(-1.0f, 0.0f, 0f),
			new Vector3( 1.0f, 0.0f, 0f),
			new Vector3( 1.25f, 0.5f, 0f)
		};
		/// <summary>
		/// This Vector3 array contains the vertices of the edge between trail mesh steps. Vertices are defined in 
		/// local coordinates of the game object that has this script attached.
		/// </summary>
		public Vector3[] TrailMeshVertices
		{
			get{ return TRAIL_MESH_VERTICES; }
			set{ TRAIL_MESH_VERTICES = value; }
		}

		[SerializeField]
		private bool m_isUVsNeeded = false;
		/// <summary>
		/// if true, then the TRAIL_MESH_UVS must be provided. If false, then the generated trail mesh will 
		/// have no UV coordinates (use only with custom shaders that need no UV).
		/// </summary>
		public bool IsUVsNeeded
		{
			get{ return m_isUVsNeeded; }
			set
			{
				m_isUVsNeeded = value;
				if (m_renderer != null)
				{
					Debug.LogError("MT_MeshTrailRenderer: IsUVsNeeded cannot be set on runtime!");
				}
			}
		}
		[SerializeField]
		// This Vector2 array contains the UV coordinates for each vertex of the edge between trail mesh steps.
		private Vector2[] TRAIL_MESH_UVS = new Vector2[4]
		{
			new Vector2(    0f,	0f ),
			new Vector2( 0.25f,	0f ),
			new Vector2(  0.5f,	0f ),
			new Vector2(    1f,	0f )
		};
		/// <summary>
		/// This Vector2 array contains the UV coordinates for each vertex in TRAIL_MESH_VERTICES. This array must have the same length 
		/// as the vertex array.
		/// </summary>
		public Vector3[] TrailMeshUVs
		{
			get{ return TRAIL_MESH_VERTICES; }
			set{ TRAIL_MESH_VERTICES = value; }
		}
		[SerializeField]
		private Vector2 m_UVSpread = Vector2.up;
		/// <summary>
		/// This vector is applied to the UV coordinates of each trail edge. The applied vector is multiplied with the trail length at the edge.
		/// </summary>
		public Vector2 UVSpread
		{
			get{ return m_UVSpread; }
			set{ m_UVSpread = value; }
		}


		[SerializeField]
		private bool m_isNormalsNeeded = false;
		/// <summary>
		/// if true, then the TRAIL_MESH_NORMALS must be provided. If false, then the generated trail mesh will 
		/// have no normals (use only with custom shaders that need no normals).
		/// </summary>
		public bool IsNormalsNeeded
		{
			get{ return m_isNormalsNeeded; }
			set
			{
				m_isNormalsNeeded = value;
				if (m_renderer != null)
				{
					Debug.LogError("MT_MeshTrailRenderer: IsNormalsNeeded cannot be set on runtime!");
				}
			}
		}
		[SerializeField]
		private Vector3[] TRAIL_MESH_NORMALS = new Vector3[4]
		{
			new Vector3( 1f, -0.2f, 0f).normalized,
			new Vector3( 0.45f, 1.0f, 0f).normalized,
			new Vector3(-0.45f, 1.0f, 0f).normalized,
			new Vector3(-1f, -0.2f, 0f).normalized
		};
		/// <summary>
		/// This Vector3 array contains the normals for each vertex in TRAIL_MESH_VERTICES. This array must have the same length 
		/// as the vertex array. Normals are defined in local coordinates of the game object that has this script attached.
		/// </summary>
		public Vector3[] TrailMeshNormals
		{
			get{ return TRAIL_MESH_NORMALS; }
			set{ TRAIL_MESH_NORMALS = value; }
		}
		[SerializeField]
		private int[] TRAIL_MESH_TRIANGLES = new int[18]
		{
			// cull front
			//0, -4, -3,
			//1,  0, -3,
			//1, -3, -2,
			//2,  1, -2,
			//2, -2, -1,
			//3,  2, -1
			
			// cull back
			0, -3, -4,
			1, -3,  0,
			1, -2, -3,
			2, -2,  1,
			2, -1, -2,
			3, -1,  2
		};
		/// <summary>
		/// This int array contains the triangle indices built between the current vertex array and the vertex array of the last edge. 
		/// In other words, this array describes how single edges of the trail mesh are connected. The index values can/must be negative. 
		/// For example, if you want to build a triangle between the first two vertices of the new trail edge and the last vertex of the 
		/// preceding edge you would use these indices: [0, 1, -1].
		/// </summary>
		public int[] TrailMeshTriangles
		{
			get{ return TRAIL_MESH_TRIANGLES; }
			set{ TRAIL_MESH_TRIANGLES = value; }
		}

		[SerializeField]
		private bool m_useRandomVertexOffsets = false;
		/// <summary>
		/// if true, then the RANDOM_VERTEX_OFFSETS array must be provided (local space).
		/// If true, then the vertices will get a random offset defined in the RANDOM_VERTEX_OFFSETS array.
		/// </summary>
		public bool UseRandomVertexOffsets
		{
			get{ return m_useRandomVertexOffsets; }
			set{ m_useRandomVertexOffsets = value; }
		}
		[SerializeField]
		private Vector3[] RANDOM_VERTEX_OFFSETS = new Vector3[4]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 0f, 0f)
		};
		/// <summary>
		/// This Vector3 array contains the random offset for each vertex in TRAIL_MESH_VERTICES. This array must have the same length 
		/// as the vertex array. Offsets range is defined as [-RandomVertexOffsets[i],RandomVertexOffsets[i]].
		/// </summary>
		public Vector3[] RandomVertexOffsets
		{
			get{ return RANDOM_VERTEX_OFFSETS; }
			set{ RANDOM_VERTEX_OFFSETS = value; }
		}

		[SerializeField]
		private int RECALC_BOUNDS_PERIOD_ELEMENTS = 8;
		/// <summary>
		/// Number of trail edges added before the mesh bounds are recalculated. Use a low value if your trail disappears randomly (when camera moves). 
		/// Use a high value if you are looking for faster computation. On desktop platforms you can set this value to '1' if you want (the 
		/// performance implications will not be noticeable). However, on mobile platforms you should find a balanced value. Also take into account 
		/// that the performance implications grow with higher MAX_VERTEX_COUNT.
		/// </summary>
		public int RecalcBoundsPeriodElements
		{
			get{ return RECALC_BOUNDS_PERIOD_ELEMENTS; }
			set{ RECALC_BOUNDS_PERIOD_ELEMENTS = value; }
		}
		
		[SerializeField]
		private int RECALC_BOUNDS_PERIOD_FRAMES = 10; // number frames allowed for the bounds to be outdated
		/// <summary>
		/// Maximal number of frames before the mesh bounds are recalculated after a change. Use a low value if your trail disappears randomly 
		/// (when camera moves). Use a high value if you are looking for faster computation. On desktop platforms you can set this value to '1' 
		/// if you want (the performance implications will not be noticeable). However, on mobile platforms you should find a balanced value. 
		/// Also take into account that the performance implications grow with higher MAX_VERTEX_COUNT.
		/// </summary>
		public int RecalcBoundsPeriodFrames
		{
			get{ return RECALC_BOUNDS_PERIOD_FRAMES; }
			set{ RECALC_BOUNDS_PERIOD_FRAMES = value; }
		}
		
		[SerializeField]
		private int MAX_VERTEX_COUNT = 1000;
		/// <summary>
		/// The vertex limit for this trail. The oldest vertices will be overwritten once the vertex limit is reached. 
		/// </summary>
		public int MaxVertexCount
		{
			get{ return MAX_VERTEX_COUNT; }
			set{ MAX_VERTEX_COUNT = value; }
		}
		
		[SerializeField]
		private float MIN_DISTANCE = 0.4f;
		/// <summary>
		/// Minimal distance between two edges of the trail.
		/// </summary>
		public float MinTrailSectionDistance
		{
			get{ return MIN_DISTANCE; }
			set{ MIN_DISTANCE = value; }
		}

		[SerializeField]
		private bool m_isJerkFree = true;
		/// <summary>
		/// When active: an edge will be added to the mesh even if the MIN_DISTANCE between this game object and the last 
		/// trail edge is not reached. This new intermediate edge will be updated every frame and moved with the game object 
		/// until MIN_DISTANCE is exceeded. Then a static edge will be added and a new intermediate edge will be created. 
		/// You can safely use this feature on desktop platforms. If your game runs on mobile with many trails and you need to 
		/// improve performance, then you could disable this feature and hide the jerking edge generation behind a particle system.
		/// </summary>
		public bool IsJerkFree
		{
			get{ return m_isJerkFree; }
			set{ m_isJerkFree = value; }
		}

		[SerializeField]
		private bool m_isRotateTowardsMovementDirection = true;
		/// <summary>
		/// If true, then this game object will be rotated towards move direction in LateUpdate. If this object has a parent, then the up 
		/// vector will be rotated to match the up vector of the parent as good as possible. If there is no parent, then the up vector is 
		/// not defined and can become random over time.
		/// </summary>
		public bool IsRotateTowardsMovementDirection
		{
			get{ return m_isRotateTowardsMovementDirection; }
			set{ m_isRotateTowardsMovementDirection = value; }
		}

		[SerializeField]
		private bool m_isDrawing = true;
		/// <summary>
		/// If true, then the trail will be drawn. If false, then there will be no trail.
		/// </summary>
		public bool IsDrawing
		{
			get{ return m_isDrawing; }
			set{ m_isDrawing = value; }
		}
		public void StartDraw() { m_isDrawing = true; } // call over SendMessage
		public void StopDraw() { m_isDrawing = false; } // call over SendMessage
		
		[SerializeField]
		private Vector3 m_scale = Vector3.one;
		/// <summary>
		/// Vertex scale that will be applied to the TRAIL_MESH_VERTICES array. Often the trails in a game differ only in 
		/// width or height. For example, if you have small and big snowboards drawing a trail into snow. You can use this 
		/// scale property to save time and avoid redefining the vertex array unnecessary.
		/// </summary>
		public Vector3 Scale
		{
			get{ return m_scale; }
			set{ m_scale = value; }
		}
		public void SetScale(Vector3 p_scale) { m_scale = p_scale; } // call over SendMessage

		private Quaternion? m_normalRotationOverride = null;
		/// <summary>
		/// Sometimes the vertex edge orientation and normal orientation need to be different. 
		/// Use this function to pass a Quaternion that defines the normal orientation.
		/// </summary>
		public void OverrideNormalRotation(Quaternion p_customNormalRotation)
		{
			m_normalRotationOverride = p_customNormalRotation;
		}

		private Vector3 m_lastDrawLocation = INIT_LAST_DRAW_LOCATION;
		/// <summary>
		/// Last edge position of the currently drawn connected trail. If there is no trail drawn, then this property has the value 
		/// of INIT_LAST_DRAW_LOCATION. This property is not accessible via the inspector, but could be useful in scripts.
		/// </summary>
		public Vector3 LastDrawLocation { get{ return m_lastDrawLocation; } }

		private GameObject m_meshGO;
		private MeshFilter m_meshFilter;
		private Mesh m_mesh;
		private MeshRenderer m_renderer;
		private bool m_isNewTrail = true;
		private int m_boundsOutdatedElementCount = 0;
		private int m_nextBoundsRecalcFrame = -1;
		private bool m_isBoundsUpToDate = true;
		private int m_overwriteOffsetVerts = 0;
		private int m_overwriteOffsetTris = 0;
		private float m_distance = 0;
		private Vector3[] m_nextOffsets = null;

		
		public void Clear()
		{
			if (m_mesh == null) { return; }
			
			// reset internal vars
			m_lastDrawLocation = INIT_LAST_DRAW_LOCATION;

			m_isNewTrail = true;
			m_boundsOutdatedElementCount = 0;
			m_nextBoundsRecalcFrame = -1;
			m_isBoundsUpToDate = true;
			m_overwriteOffsetVerts = -TRAIL_MESH_VERTICES.Length;
			m_overwriteOffsetTris = -TRAIL_MESH_TRIANGLES.Length;
			m_distance = 0;
			
			// reset mesh
			m_mesh.triangles = new int[0];
			if (m_isUVsNeeded)
			{
				m_mesh.uv = new Vector2[0];
			}
			if (m_isNormalsNeeded)
			{
				m_mesh.normals = new Vector3[0];
			}
			m_mesh.vertices = new Vector3[0];
			m_mesh.RecalculateBounds();
		}
		
		private void Start()
		{
			m_overwriteOffsetVerts = -TRAIL_MESH_VERTICES.Length;
			m_overwriteOffsetTris = -TRAIL_MESH_TRIANGLES.Length;
			CreateMeshGameObject();

			Material.renderQueue = (int)RenderQueue.Geometry - 75;
		}

		private void LateUpdate()
		{
			if (m_isDrawing)
			{
				if (m_lastDrawLocation == INIT_LAST_DRAW_LOCATION)
				{
					m_lastDrawLocation = transform.position;
				}
				else if ((m_lastDrawLocation - transform.position).magnitude > MIN_DISTANCE)
				{
					if (m_isRotateTowardsMovementDirection)
					{
						transform.LookAt(2f*transform.position - m_lastDrawLocation, transform.parent==null?transform.up:transform.parent.up);
					}
					Draw();
					m_isNewTrail = false;
					if (m_isUVsNeeded)
					{
						m_distance += (m_lastDrawLocation - transform.position).magnitude;
					}
					m_lastDrawLocation = transform.position;
				}
				else if (!m_isNewTrail && m_isJerkFree)// only trails with at least two iterations can jerk -> !m_isNewTrail
				{
					if (m_isRotateTowardsMovementDirection)
					{
						transform.LookAt(2f*transform.position - m_lastDrawLocation, transform.parent==null?transform.up:transform.parent.up);
					}
					DrawJerkFree();
				}
			}
			else
			{
				m_isNewTrail = true;
				m_lastDrawLocation = INIT_LAST_DRAW_LOCATION;
			}
			
			if (!m_isBoundsUpToDate && Time.frameCount > m_nextBoundsRecalcFrame)
			{
				m_boundsOutdatedElementCount = 0;
				m_isBoundsUpToDate = true;
				m_mesh.RecalculateBounds();
			}
		}
		
		private void OnDestroy()
		{
			Destroy(m_meshGO);
			Destroy(m_meshFilter);
			Destroy(m_mesh);
			Destroy(m_renderer);
		}
		
		private void Draw()
		{
			m_boundsOutdatedElementCount++;
			// get old mesh data arrays
			Vector3[] oldVertices = m_mesh.vertices;
			Vector3[] oldNormals;
			Vector2[] oldUVs;
			if (m_isNormalsNeeded)
			{
				oldNormals = m_mesh.normals;
			}
			else
			{
				oldNormals = null;
			}
			if (m_isUVsNeeded)
			{
				oldUVs = m_mesh.uv;
			}
			else
			{
				oldUVs = null;
			}
#if UNITY_5_1
			// Unity 5.1 bug: https://issuetracker.unity3d.com/issues/leapmotion-mesh-failed-getting-triangles-submesh-topology-is-lines-or-points
			int[] oldTriangles;
			if (m_mesh.vertexCount > 0)
			{
				oldTriangles = m_mesh.triangles;
			}
			else
			{
				oldTriangles = new int[0];
			}
#else
			int[] oldTriangles = m_mesh.triangles;
#endif

			// allocate new mesh data arrays
			Vector3[] vertices;
			Vector3[] normals;
			Vector2[] uvs;
			int[] triangles;
			int indexOffsetVerts;
			int indexOffsetTris;
			if (oldVertices.Length+TRAIL_MESH_VERTICES.Length > MAX_VERTEX_COUNT)
			{
				// the maximal amount of vertices is reached now vertices need to be overwritten
				m_overwriteOffsetVerts += TRAIL_MESH_VERTICES.Length;
				// also the maximal amount for triangles is reached and needs to be overwritten
				if (m_overwriteOffsetVerts >= oldVertices.Length)
				{
					m_overwriteOffsetVerts = 0;
					m_overwriteOffsetTris = 0;
				}
				else if (!m_isNewTrail)
				{
					m_overwriteOffsetTris += TRAIL_MESH_TRIANGLES.Length;
				}
				// no new arrays need to be generated
				vertices = oldVertices;
				normals = oldNormals;
				uvs = oldUVs;
				// clean up triangles referencing overwritten vertices
				bool isGoingOverEdge = m_overwriteOffsetVerts == oldVertices.Length-TRAIL_MESH_VERTICES.Length;
				int indexToCheckForOverwrites = m_overwriteOffsetTris + TRAIL_MESH_TRIANGLES.Length;
				bool isVertexOverwritten = false;
				if (oldTriangles.Length > indexToCheckForOverwrites + TRAIL_MESH_TRIANGLES.Length)
				{
					for (int i = 0; i < TRAIL_MESH_TRIANGLES.Length; i++)
					{
						if (oldTriangles[indexToCheckForOverwrites+i] <= m_overwriteOffsetVerts)
						{
							isVertexOverwritten = true;
							break;
						}
					}
				}
				if (isGoingOverEdge || isVertexOverwritten)
				{
					System.Collections.Generic.List<int> tempTris = new System.Collections.Generic.List<int>(oldTriangles);
					if (isVertexOverwritten)
					{
						tempTris.RemoveRange(indexToCheckForOverwrites, TRAIL_MESH_TRIANGLES.Length);
					}
					// clean up triangles referencing overwritten vertices over the edges of the vertex and index arrays
					if (isGoingOverEdge)
					{
						// remove the first triangle indicies, because they now reference the overwritten vertex at the end of the array
						tempTris.RemoveRange(0, TRAIL_MESH_TRIANGLES.Length);
					}
					// fill up triangles if the array is too small
					if (m_isNewTrail && tempTris.Count <= m_overwriteOffsetTris + TRAIL_MESH_TRIANGLES.Length)
					{
						tempTris.AddRange(new int[(m_overwriteOffsetTris + TRAIL_MESH_TRIANGLES.Length + TRAIL_MESH_TRIANGLES.Length)-tempTris.Count]);
					}
					else if (!m_isNewTrail && tempTris.Count <= m_overwriteOffsetTris)
					{
						tempTris.AddRange(new int[TRAIL_MESH_TRIANGLES.Length + m_overwriteOffsetTris-tempTris.Count]);
					}
					triangles = tempTris.ToArray();
				}
				else
				{
					// fill up triangles if the array is too small
					int fillUpCount = 0;
					if (m_isNewTrail && oldTriangles.Length <= m_overwriteOffsetTris + TRAIL_MESH_TRIANGLES.Length)
					{
						fillUpCount = m_overwriteOffsetTris + TRAIL_MESH_TRIANGLES.Length + TRAIL_MESH_TRIANGLES.Length - oldTriangles.Length;
					}
					else if (!m_isNewTrail && oldTriangles.Length <= m_overwriteOffsetTris)
					{
						fillUpCount = m_overwriteOffsetTris + TRAIL_MESH_TRIANGLES.Length - oldTriangles.Length;
					}
					if (fillUpCount > 0)
					{
						triangles = new int[oldTriangles.Length+fillUpCount];
						System.Array.Copy(oldTriangles, triangles, oldTriangles.Length);
					}
					else
					{
						triangles = oldTriangles;
					}
				}
				// insert new vertices somewhere in the middle
				indexOffsetVerts = m_overwriteOffsetVerts;
				indexOffsetTris = m_overwriteOffsetTris;
			}
			else
			{
				// append new verts at end of array
				indexOffsetVerts = oldVertices.Length;
				indexOffsetTris = oldTriangles.Length;
				// verts
				vertices = new Vector3[indexOffsetVerts+TRAIL_MESH_VERTICES.Length];
				System.Array.Copy(oldVertices, vertices, indexOffsetVerts);
				// normals
				if (m_isNormalsNeeded)
				{
					normals = new Vector3[indexOffsetVerts+TRAIL_MESH_NORMALS.Length];
					System.Array.Copy(oldNormals, normals, oldNormals.Length);
				}
				else
				{
					normals = null;
				}
				// uvs
				if (m_isUVsNeeded)
				{
					uvs = new Vector2[indexOffsetVerts+TRAIL_MESH_NORMALS.Length];
					System.Array.Copy(oldUVs, uvs, oldUVs.Length);
				}
				else
				{
					uvs = null;
				}
				// triangles
				if (oldVertices.Length != 0 && !m_isNewTrail)
				{
					triangles = new int[indexOffsetTris+TRAIL_MESH_TRIANGLES.Length];
					System.Array.Copy(oldTriangles, triangles, indexOffsetTris);
				}
				else
				{
					triangles = oldTriangles;
				}
			}
			// calculate values for the added mesh data
			for (int i = 0; i < TRAIL_MESH_VERTICES.Length; i++)
			{
				int currIndex = indexOffsetVerts + i;
				Vector3 scaledVert = TRAIL_MESH_VERTICES[i];
				// add random offsets
				if (m_nextOffsets != null)
				{
					scaledVert += m_nextOffsets[i];
				}
				scaledVert.Scale(m_scale);
				vertices[currIndex] = transform.TransformPoint(scaledVert);
				if (m_isNormalsNeeded)
				{
					if (m_normalRotationOverride != null)
					{
						normals[currIndex] = m_normalRotationOverride.Value * TRAIL_MESH_NORMALS[i];
					}
					else
					{
						normals[currIndex] = transform.TransformDirection(TRAIL_MESH_NORMALS[i]);
					}
				}
				if (m_isUVsNeeded)
				{
					uvs[currIndex] = TRAIL_MESH_UVS[i] + m_UVSpread*m_distance;
				}
			}
			// calculate new triangles
			if (oldVertices.Length != 0 && !m_isNewTrail)
			{
				if (indexOffsetTris != 0)
				{
					for (int i = 0; i < TRAIL_MESH_TRIANGLES.Length; i++)
					{
						triangles[indexOffsetTris+i] = indexOffsetVerts + TRAIL_MESH_TRIANGLES[i];
					}
				}
				else
				{
					for (int i = 0; i < TRAIL_MESH_TRIANGLES.Length; i++)
					{
						int index = indexOffsetVerts + TRAIL_MESH_TRIANGLES[i];
						if (index < 0)
						{
							triangles[indexOffsetTris+i] = vertices.Length + index;
						}
						else
						{
							triangles[indexOffsetTris+i] = index;
						}
					}
				}
			}
			// assign new mesh data arrays
			m_mesh.vertices = vertices;
			if (m_isNormalsNeeded)
			{
				m_mesh.normals = normals;
			}
			if (m_isUVsNeeded)
			{
				m_mesh.uv = uvs;
			}
			m_mesh.triangles = triangles;
			// recalculate bounds if needed
			if (m_boundsOutdatedElementCount % RECALC_BOUNDS_PERIOD_ELEMENTS == 0)
			{
				m_boundsOutdatedElementCount = 0;
				m_isBoundsUpToDate = true;
				m_mesh.RecalculateBounds();
			}
			else
			{
				m_isBoundsUpToDate = false;
				m_nextBoundsRecalcFrame = Time.frameCount + RECALC_BOUNDS_PERIOD_FRAMES;
			}
			// generate next random offsets
			m_nextOffsets = null;
			if (m_useRandomVertexOffsets)
			{
				if (TRAIL_MESH_VERTICES.Length == RANDOM_VERTEX_OFFSETS.Length)
				{
					m_nextOffsets = new Vector3[TRAIL_MESH_VERTICES.Length];
					for (int i = 0; i < m_nextOffsets.Length; i++)
					{
						Vector3 rnd = RANDOM_VERTEX_OFFSETS[i];
						m_nextOffsets[i] = new Vector3(
							Random.Range(-rnd.x, rnd.x),
							Random.Range(-rnd.y, rnd.y),
							Random.Range(-rnd.z, rnd.z));
					}
				}
				else
				{
					Debug.LogError("MT_MeshTrailRenderer: the RANDOM_VERTEX_OFFSETS array has a different length than the TRAIL_MESH_VERTICES array!");
				}
			}
		}

		private void DrawJerkFree()
		{
			// get mesh data arrays
			Vector3[] vertices = m_mesh.vertices;
			if (vertices.Length < TRAIL_MESH_VERTICES.Length*2)
			{
				return; // the mesh is to small to be updated
			}
			Vector3[] normals;
			if (m_isNormalsNeeded)
			{
				normals = m_mesh.normals;
			}
			else
			{
				normals = null;
			}
			Vector2[] uvs;
			if (m_isUVsNeeded)
			{
				uvs = m_mesh.uv;
			}
			else
			{
				uvs = null;
			}
			// calculate the index offset of the newest mesh edge
			int indexOffsetVerts;
			if (vertices.Length+TRAIL_MESH_VERTICES.Length > MAX_VERTEX_COUNT)
			{
				// the maximal amount of vertices is reached the newest edge is somewhere in the middle
				indexOffsetVerts = m_overwriteOffsetVerts;
				if (indexOffsetVerts < 0)
				{
					indexOffsetVerts = vertices.Length + indexOffsetVerts;
				}
			}
			else
			{
				// the newest edge is at end of array
				indexOffsetVerts = vertices.Length-TRAIL_MESH_VERTICES.Length;
			}
			// update the newest edge to fit object's position and rotation
			for (int i = 0; i < TRAIL_MESH_VERTICES.Length; i++)
			{
				int currIndex = indexOffsetVerts + i;
				Vector3 scaledVert = TRAIL_MESH_VERTICES[i];
				// add random offsets
				if (m_nextOffsets != null)
				{
					scaledVert += m_nextOffsets[i];
				}
				scaledVert.Scale(m_scale);
				vertices[currIndex] = transform.TransformPoint(scaledVert);
				if (m_isNormalsNeeded)
				{
					if (m_normalRotationOverride != null)
					{
						normals[currIndex] = m_normalRotationOverride.Value * TRAIL_MESH_NORMALS[i];
					}
					else
					{
						normals[currIndex] = transform.TransformDirection(TRAIL_MESH_NORMALS[i]);
					}
				}
				if (m_isUVsNeeded)
				{
					uvs[currIndex] = TRAIL_MESH_UVS[i] + m_UVSpread*m_distance;
				}
			}
			// assign new mesh data arrays
			m_mesh.vertices = vertices;
			if (m_isNormalsNeeded)
			{
				m_mesh.normals = normals;
			}
			if (m_isUVsNeeded)
			{
				m_mesh.uv = uvs;
			}
		}
		
		private void CreateMeshGameObject()
		{
			m_meshGO = new GameObject("MT_MeshTrailRenderer Mesh");
			m_meshFilter = m_meshGO.AddComponent<MeshFilter>();
			m_mesh = new Mesh();
			m_meshFilter.sharedMesh = m_mesh;
			m_renderer = m_meshGO.AddComponent<MeshRenderer>();
			m_renderer.sharedMaterial = m_material;
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3_OR_NEWER
			m_renderer.shadowCastingMode = m_castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
#else
			m_renderer.castShadows = m_castShadows;
#endif
			m_renderer.receiveShadows = m_receiveShadows;
		}
	}
}
