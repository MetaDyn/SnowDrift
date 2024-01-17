using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerConfig : MonoBehaviour
	{
#region Singleton Instance Logic

		public const string RESOURCE_PATH = "Configs\\PlayerConfig";

		protected static PlayerConfig s_instance;
		/// <summary>
		/// You can use the static Instance property to access the ControlsConfig API from wherever you need it in your code. See also ControlsConfig.IsInstanceSet.
		/// </summary>
		public static PlayerConfig Instance
		{
			get
			{
				// first try to find an existing instance
				if (s_instance == null)
				{
					s_instance = GameObject.FindObjectOfType<PlayerConfig>();
				}
				// if no instance exists yet, then create a new one
				if (s_instance == null)
				{
					if (Resources.Load<PlayerConfig>(RESOURCE_PATH) != null)
					{
						s_instance = Instantiate(Resources.Load<PlayerConfig>(RESOURCE_PATH));
					}
					if (s_instance == null)
					{
						Debug.LogError("PlayerConfig: could not load resource at path '" + RESOURCE_PATH + "'!");
						s_instance = new GameObject("Fallback PlayerConfig").AddComponent<PlayerConfig>();
					}
					GameObject.DontDestroyOnLoad(s_instance);
				}
				return s_instance;
			}
		}
		/// <summary>
		/// The static IsInstanceSet property can be used before accessing the Instance property to prevent a new instance from being created in the teardown phase.
		/// For example, if you want to unregister from an event, then first check that IsInstanceSet is true. If it is false, then your event registration is not valid anymore.
		/// </summary>
		public static bool IsInstanceSet { get{ return s_instance != null; } }

#endregion

#region Player General Ability Values

		[SerializeField]
		private float m_SPEED = 1f;
		public float SPEED // [0..1]
		{
			get { return m_SPEED; }
			set { m_SPEED = value; }
		}

		[SerializeField]
		private float m_SPIN_AIR = 1f;
		public float SPIN_AIR
		{
			get { return m_SPIN_AIR; }
			set { m_SPIN_AIR = value; }
		}

		[SerializeField]
		private float m_SPIN_GROUND = 1f;
		public float SPIN_GROUND // [0..1]
		{
			get { return m_SPIN_GROUND; }
			set { m_SPIN_GROUND = value; }
		}

		[SerializeField]
		private float m_JUMP = 1f;
		public float JUMP // [0..1]
		{
			get { return m_JUMP; }
			set { m_JUMP = value; }
		}

#endregion

#region Player Control Settings

		[SerializeField]
		private bool m_ALWAYS_ACCELERATE = true;
		public bool ALWAYS_ACCELERATE
		{
			get { return m_ALWAYS_ACCELERATE; }
			set { m_ALWAYS_ACCELERATE = value; }
		}

		[SerializeField]
		private float m_JUMP_INTERVAL = 0.75f;
		public float JUMP_INTERVAL // jump key minimal interval
		{
			get { return m_JUMP_INTERVAL; }
			set { m_JUMP_INTERVAL = value; }
		}

#endregion

#region Player Rotation

		[SerializeField]
		private float m_FLAT_ROT_SPEED = 30f;
		public float FLAT_ROT_SPEED
		{
			get { return m_FLAT_ROT_SPEED; }
			set { m_FLAT_ROT_SPEED = value; }
		}

		[SerializeField]
		private float m_HILL_ROT_SPEED = 280f;
		public float HILL_ROT_SPEED
		{
			get { return m_HILL_ROT_SPEED; }
			set { m_HILL_ROT_SPEED = value; }
		}

		[SerializeField]
		private float m_AIR_ROT_SPEED = 325f;
		public float AIR_ROT_SPEED
		{
			get { return m_AIR_ROT_SPEED; }
			set { m_AIR_ROT_SPEED = value; }
		}

		[SerializeField]
		private float m_BOMB_ROT_FACTOR = 0.5f;
		public float BOMB_ROT_FACTOR
		{
			get { return m_BOMB_ROT_FACTOR; }
			set { m_BOMB_ROT_FACTOR = value; }
		}

		[SerializeField]
		private float m_CARVE_ROT_FACTOR = 0.5f;
		public float CARVE_ROT_FACTOR
		{
			get { return m_CARVE_ROT_FACTOR; }
			set { m_CARVE_ROT_FACTOR = value; }
		}

		[SerializeField]
		private float m_MIN_SPEED_HILL = 5.0f;
		public float MIN_SPEED_HILL
		{
			get { return m_MIN_SPEED_HILL; }
			set { m_MIN_SPEED_HILL = value; }
		}

		[SerializeField]
		private float m_MIN_SPEED_BOMB = 35.0f;
		public float MIN_SPEED_BOMB
		{
			get { return m_MIN_SPEED_BOMB; }
			set { m_MIN_SPEED_BOMB = value; }
		}

#endregion

#region Player Physics

		[SerializeField]
		private float m_ACCELERATION_FORCE = 115f;
		public float ACCELERATION_FORCE
		{
			get { return m_ACCELERATION_FORCE; }
			set { m_ACCELERATION_FORCE = value; }
		}

		[SerializeField]
		private float m_JUMP_UP_SPEED = 5f;
		public float JUMP_UP_SPEED
		{
			get { return m_JUMP_UP_SPEED; }
			set { m_JUMP_UP_SPEED = value; }
		}

		[SerializeField]
		private float m_DRAG_AIR = 0.04f;
		public float DRAG_AIR
		{
			get { return m_DRAG_AIR; }
			set { m_DRAG_AIR = value; }
		}

		[SerializeField]
		private float m_DRAG_GROUND_MAX = 0.0475f;
		public float DRAG_GROUND_MAX
		{
			get { return m_DRAG_GROUND_MAX; }
			set { m_DRAG_GROUND_MAX = value; }
		}

		[SerializeField]
		private float m_DRAG_OFFROAD_FACTOR = 15.0f;
		public float DRAG_OFFROAD_FACTOR
		{
			get { return m_DRAG_OFFROAD_FACTOR; }
			set { m_DRAG_OFFROAD_FACTOR = value; }
		}

		[SerializeField]
		private float m_DRAG_MIN_SPEED = 5.0f;
		public float DRAG_MIN_SPEED
		{
			get { return m_DRAG_MIN_SPEED; }
			set { m_DRAG_MIN_SPEED = value; }
		}

		[SerializeField]
		private float m_ADDITIONAL_GRAVITY_IN_AIR = -5f;
		public float ADDITIONAL_GRAVITY_IN_AIR
		{
			get { return m_ADDITIONAL_GRAVITY_IN_AIR; }
			set { m_ADDITIONAL_GRAVITY_IN_AIR = value; }
		}

		[SerializeField]
		private string m_TERRAIN_LAYER = "Terrain";
		public int TERRAIN_LAYER { get { return LayerMask.NameToLayer(m_TERRAIN_LAYER); } }

		[SerializeField]
		private string m_IGNORE_RAYCAST_LAYER = "Ignore Raycast";
		public int IGNORE_RAYCAST_LAYER { get { return LayerMask.NameToLayer(m_IGNORE_RAYCAST_LAYER); } }

		[SerializeField]
		private bool m_ENABLE_TRICK_SCORES = true;
		public bool ENABLE_TRICK_SCORES
		{
			get { return m_ENABLE_TRICK_SCORES; }
			set { m_ENABLE_TRICK_SCORES = value; }
		}

#endregion
	}
}