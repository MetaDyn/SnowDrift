using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Snowboard
{
	public static class PlayerInput
	{
		private static PlayerInputDevice[][] s_devises = new PlayerInputDevice[5][];
		private static int m_activeIndex = (int)EPlayers.SINGLE;

		public static void Create(EPlayers p_index)
		{
			int index = (int)p_index;
			if (s_devises[index] == null)
			{
				List<PlayerInputDevice> devices = new List<PlayerInputDevice>();
#if UNITY_IOS || UNITY_ANDROID || UNITY_TIZEN
				if (GameDatabaseHandler.Instance.GetBool(GameDatabaseHandler.BoolVars.IS_CONTROL_MODE_BUTTONS))
				{
					devices.Add(new PlayerInputTouchButtons(
						ControlsConfig.Instance.BUTTON_CONTROL_SIZE,
						GameDatabaseHandler.Instance.GetBool(GameDatabaseHandler.BoolVars.IS_CONTROL_ON_RIGHT),
						GameDatabaseHandler.Instance.GetBool(GameDatabaseHandler.BoolVars.IS_BUTTON_CONTROL_SIMPLE)));
				}
				if (GameDatabaseHandler.Instance.GetBool(GameDatabaseHandler.BoolVars.IS_CONTROL_MODE_JOYSTICK))
				{
					devices.Add(new PlayerInputTouchJoystick(
						ControlsConfig.Instance.JOYSTICK_CONTROL_SIZE,
						GameDatabaseHandler.Instance.GetBool(GameDatabaseHandler.BoolVars.IS_CONTROL_ON_RIGHT)));
				}
				if (GameDatabaseHandler.Instance.GetBool(GameDatabaseHandler.BoolVars.IS_CONTROL_MODE_TILT))
				{
					PlayerInputAccelerator accInput = new PlayerInputAccelerator();
					accInput.Sensitivity = ControlsConfig.Instance.TILT_SENSIVITY;
					devices.Add(accInput);
				}
#endif
				devices.Add(new PlayerInputKeyboard());
				s_devises[index] = devices.ToArray ();
			}
		}
		
		public static void DestroyAll()
		{
			s_devises = new PlayerInputDevice[5][];
		}

		public static void Destroy(EPlayers p_index)
		{
			s_devises[(int)p_index] = null;
		}
		
		public static void SetActiveIndex(EPlayers p_activePlayer)
		{
			m_activeIndex = (int)p_activePlayer;
		}

		public static float AxisX
		{
			get
			{
				float result = 0f;
				PlayerInputDevice[] devices = s_devises[m_activeIndex];
				if (devices != null)
				{
					foreach (PlayerInputDevice device in devices)
					{
						result += device.AxisX;
					}
				}
				return Mathf.Clamp(result, -1f, 1f);
			}
		}
		
		public static float AxisY
		{
			get
			{
				float result = 0f;
				PlayerInputDevice[] devices = s_devises[m_activeIndex];
				if (devices != null)
				{
					foreach (PlayerInputDevice device in devices)
					{
						result += device.AxisY;
					}
				}
				return Mathf.Clamp(result, -1f, 1f);
			}
		}
		
		public static bool IsJump
		{
			get
			{
				PlayerInputDevice[] devices = s_devises[m_activeIndex];
				if (devices != null)
				{
					foreach (PlayerInputDevice device in devices)
					{
						if (device.IsJump)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public static bool IsSlide
		{
			get
			{
#if SLIDING_SUPPORTED
				PlayerInputDevice[] devices = s_devises[m_activeIndex];
				if (devices != null)
				{
					foreach (PlayerInputDevice device in devices)
					{
						if (device.IsSlide)
						{
							return true;
						}
					}
				}
#endif
				return false;
			}
		}

		public static bool IsGrab
		{
			get
			{
				PlayerInputDevice[] devices = s_devises[m_activeIndex];
				if (devices != null)
				{
					foreach (PlayerInputDevice device in devices)
					{
						if (device.IsGrab)
						{
							return true;
						}
					}
				}
				return false;
			}
		}
		
		public static void Draw ()
		{
			PlayerInputDevice[] devices = s_devises[m_activeIndex];
			if (devices != null)
			{
				foreach (PlayerInputDevice device in devices)
				{
					device.Draw ();
				}
			}
		}
		
		public static void SaveCurrentAsCalibration()
		{
			PlayerInputDevice[] devices = s_devises[m_activeIndex];
			if (devices != null)
			{
				foreach (PlayerInputDevice device in devices)
				{
					device.SaveCurrentAsCalibration();
				}
			}
		}
	}
}
