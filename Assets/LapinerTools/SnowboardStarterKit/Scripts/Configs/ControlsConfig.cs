using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class ControlsConfig : MonoBehaviour
	{
#region Singleton Instance Logic

		public const string RESOURCE_PATH = "Configs\\ControlsConfig";

		protected static ControlsConfig s_instance;
		/// <summary>
		/// You can use the static Instance property to access the ControlsConfig API from wherever you need it in your code. See also ControlsConfig.IsInstanceSet.
		/// </summary>
		public static ControlsConfig Instance
		{
			get
			{
				// first try to find an existing instance
				if (s_instance == null)
				{
					s_instance = GameObject.FindObjectOfType<ControlsConfig>();
				}
				// if no instance exists yet, then create a new one
				if (s_instance == null)
				{
					if (Resources.Load<ControlsConfig>(RESOURCE_PATH) != null)
					{
						s_instance = Instantiate(Resources.Load<ControlsConfig>(RESOURCE_PATH));
					}
					if (s_instance == null)
					{
						Debug.LogError("ControlsConfig: could not load resource at path '" + RESOURCE_PATH + "'!");
						s_instance = new GameObject("Fallback ControlsConfig").AddComponent<ControlsConfig>();
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

#region Button Controls Values

		[SerializeField]
		private float m_BUTTON_CONTROL_SIZE = 0.3f;
		public float BUTTON_CONTROL_SIZE
		{
			get { return m_BUTTON_CONTROL_SIZE; }
			set { m_BUTTON_CONTROL_SIZE = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_UP_IMG;
		public Texture2D BUTTON_CONTROL_UP_IMG
		{
			get { return m_BUTTON_CONTROL_UP_IMG; }
			set { m_BUTTON_CONTROL_UP_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_UP_DOWN_IMG;
		public Texture2D BUTTON_CONTROL_UP_DOWN_IMG
		{
			get { return m_BUTTON_CONTROL_UP_DOWN_IMG; }
			set { m_BUTTON_CONTROL_UP_DOWN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_DOWN_IMG;
		public Texture2D BUTTON_CONTROL_DOWN_IMG
		{
			get { return m_BUTTON_CONTROL_DOWN_IMG; }
			set { m_BUTTON_CONTROL_DOWN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_DOWN_DOWN_IMG;
		public Texture2D BUTTON_CONTROL_DOWN_DOWN_IMG
		{
			get { return m_BUTTON_CONTROL_DOWN_DOWN_IMG; }
			set { m_BUTTON_CONTROL_DOWN_DOWN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_LEFT_IMG;
		public Texture2D BUTTON_CONTROL_LEFT_IMG
		{
			get { return m_BUTTON_CONTROL_LEFT_IMG; }
			set { m_BUTTON_CONTROL_LEFT_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_LEFT_DOWN_IMG;
		public Texture2D BUTTON_CONTROL_LEFT_DOWN_IMG
		{
			get { return m_BUTTON_CONTROL_LEFT_DOWN_IMG; }
			set { m_BUTTON_CONTROL_LEFT_DOWN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_RIGHT_IMG;
		public Texture2D BUTTON_CONTROL_RIGHT_IMG
		{
			get { return m_BUTTON_CONTROL_RIGHT_IMG; }
			set { m_BUTTON_CONTROL_RIGHT_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_RIGHT_DOWN_IMG;
		public Texture2D BUTTON_CONTROL_RIGHT_DOWN_IMG
		{
			get { return m_BUTTON_CONTROL_RIGHT_DOWN_IMG; }
			set { m_BUTTON_CONTROL_RIGHT_DOWN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_UPLEFT_IMG;
		public Texture2D BUTTON_CONTROL_UPLEFT_IMG
		{
			get { return m_BUTTON_CONTROL_UPLEFT_IMG; }
			set { m_BUTTON_CONTROL_UPLEFT_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_UPLEFT_DOWN_IMG;
		public Texture2D BUTTON_CONTROL_UPLEFT_DOWN_IMG
		{
			get { return m_BUTTON_CONTROL_UPLEFT_DOWN_IMG; }
			set { m_BUTTON_CONTROL_UPLEFT_DOWN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_UPRIGHT_IMG;
		public Texture2D BUTTON_CONTROL_UPRIGHT_IMG
		{
			get { return m_BUTTON_CONTROL_UPRIGHT_IMG; }
			set { m_BUTTON_CONTROL_UPRIGHT_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_UPRIGHT_DOWN_IMG;
		public Texture2D BUTTON_CONTROL_UPRIGHT_DOWN_IMG
		{
			get { return m_BUTTON_CONTROL_UPRIGHT_DOWN_IMG; }
			set { m_BUTTON_CONTROL_UPRIGHT_DOWN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_LEFTSMALL_IMG;
		public Texture2D BUTTON_CONTROL_LEFTSMALL_IMG
		{
			get { return m_BUTTON_CONTROL_LEFTSMALL_IMG; }
			set { m_BUTTON_CONTROL_LEFTSMALL_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_LEFTSMALL_DOWN_IMG;
		public Texture2D BUTTON_CONTROL_LEFTSMALL_DOWN_IMG
		{
			get { return m_BUTTON_CONTROL_LEFTSMALL_DOWN_IMG; }
			set { m_BUTTON_CONTROL_LEFTSMALL_DOWN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_RIGHTSMALL_IMG;
		public Texture2D BUTTON_CONTROL_RIGHTSMALL_IMG
		{
			get { return m_BUTTON_CONTROL_RIGHTSMALL_IMG; }
			set { m_BUTTON_CONTROL_RIGHTSMALL_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_RIGHTSMALL_DOWN_IMG;
		public Texture2D BUTTON_CONTROL_RIGHTSMALL_DOWN_IMG
		{
			get { return m_BUTTON_CONTROL_RIGHTSMALL_DOWN_IMG; }
			set { m_BUTTON_CONTROL_RIGHTSMALL_DOWN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_DOWNLEFT_IMG;
		public Texture2D BUTTON_CONTROL_DOWNLEFT_IMG
		{
			get { return m_BUTTON_CONTROL_DOWNLEFT_IMG; }
			set { m_BUTTON_CONTROL_DOWNLEFT_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_DOWNLEFT_DOWN_IMG;
		public Texture2D BUTTON_CONTROL_DOWNLEFT_DOWN_IMG
		{
			get { return m_BUTTON_CONTROL_DOWNLEFT_DOWN_IMG; }
			set { m_BUTTON_CONTROL_DOWNLEFT_DOWN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_DOWNRIGHT_IMG;
		public Texture2D BUTTON_CONTROL_DOWNRIGHT_IMG
		{
			get { return m_BUTTON_CONTROL_DOWNRIGHT_IMG; }
			set { m_BUTTON_CONTROL_DOWNRIGHT_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_DOWNRIGHT_DOWN_IMG;
		public Texture2D BUTTON_CONTROL_DOWNRIGHT_DOWN_IMG
		{
			get { return m_BUTTON_CONTROL_DOWNRIGHT_DOWN_IMG; }
			set { m_BUTTON_CONTROL_DOWNRIGHT_DOWN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_JUMP_BTN_IMG;
		public Texture2D BUTTON_CONTROL_JUMP_BTN_IMG
		{
			get { return m_BUTTON_CONTROL_JUMP_BTN_IMG; }
			set { m_BUTTON_CONTROL_JUMP_BTN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_JUMP_BTN_DOWN_IMG;
		public Texture2D BUTTON_CONTROL_JUMP_BTN_DOWN_IMG
		{
			get { return m_BUTTON_CONTROL_JUMP_BTN_DOWN_IMG; }
			set { m_BUTTON_CONTROL_JUMP_BTN_DOWN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_JUMP_BTN_GRAY_IMG;
		public Texture2D BUTTON_CONTROL_JUMP_BTN_GRAY_IMG
		{
			get { return m_BUTTON_CONTROL_JUMP_BTN_GRAY_IMG; }
			set { m_BUTTON_CONTROL_JUMP_BTN_GRAY_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_GRAB_BTN_IMG;
		public Texture2D BUTTON_CONTROL_GRAB_BTN_IMG
		{
			get { return m_BUTTON_CONTROL_GRAB_BTN_IMG; }
			set { m_BUTTON_CONTROL_GRAB_BTN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_GRAB_BTN_DOWN_IMG;
		public Texture2D BUTTON_CONTROL_GRAB_BTN_DOWN_IMG
		{
			get { return m_BUTTON_CONTROL_GRAB_BTN_DOWN_IMG; }
			set { m_BUTTON_CONTROL_GRAB_BTN_DOWN_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_BUTTON_CONTROL_GRAB_BTN_GRAY_IMG;
		public Texture2D BUTTON_CONTROL_GRAB_BTN_GRAY_IMG
		{
			get { return m_BUTTON_CONTROL_GRAB_BTN_GRAY_IMG; }
			set { m_BUTTON_CONTROL_GRAB_BTN_GRAY_IMG = value; }
		}

#endregion

#region Joystick Controls Values

		[SerializeField]
		private float m_JOYSTICK_CONTROL_SIZE = 0.3f;
		public float JOYSTICK_CONTROL_SIZE
		{
			get { return m_JOYSTICK_CONTROL_SIZE; }
			set { m_JOYSTICK_CONTROL_SIZE = value; }
		}

		[SerializeField]
		private Texture2D m_JOYSTICK_CONTROL_BACKGROUND_IMG;
		public Texture2D JOYSTICK_CONTROL_BACKGROUND_IMG
		{
			get { return m_JOYSTICK_CONTROL_BACKGROUND_IMG; }
			set { m_JOYSTICK_CONTROL_BACKGROUND_IMG = value; }
		}

		[SerializeField]
		private Texture2D m_JOYSTICK_CONTROL_THUMB_IMG;
		public Texture2D JOYSTICK_CONTROL_THUMB_IMG
		{
			get { return m_JOYSTICK_CONTROL_THUMB_IMG; }
			set { m_JOYSTICK_CONTROL_THUMB_IMG = value; }
		}

#endregion

#region Tilt Controls Values

		[SerializeField]
		private float m_TILT_SENSIVITY = 1f;
		public float TILT_SENSIVITY
		{
			get { return m_TILT_SENSIVITY; }
			set { m_TILT_SENSIVITY = value; }
		}

#endregion
	}
}