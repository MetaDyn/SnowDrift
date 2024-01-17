using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public abstract class PlayerInputDevice
	{
		public abstract float AxisX {get;}
		public abstract float AxisY {get;}
		
		public abstract bool IsJump {get;}
		public abstract bool IsSlide {get;}
		public abstract bool IsGrab {get;}
		
		public abstract void Draw();
		
		public abstract void SaveCurrentAsCalibration();

		public static bool IsTouchInArea(Rect area)
		{
#if UNITY_EDITOR
			if (Input.GetMouseButton(0))
			{
				if (area.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
#else
			for (int i=0; i<Input.touchCount; i++)
			{
				Touch t = Input.GetTouch(i);
				if (area.Contains(new Vector2(t.position.x, Screen.height - t.position.y)))
#endif
				{
					return true;
				}
			}
			return false;
		}
	}

	public abstract class PlayerInputSimpleDrawBase : PlayerInputDevice
	{
#if SLIDING_SUPPORTED
		protected static float BIG_CTRL_BTN_SIZE { get { return Mathf.Min(180.0f, Screen.height * 0.3f); } }
#else
		protected static float BIG_CTRL_BTN_SIZE { get { return SMALL_CTRL_BTN_SIZE; } }
#endif
		protected static float SMALL_CTRL_BTN_SIZE { get { return Mathf.Min(140.0f, Screen.height * 0.27f); } }
		protected static float SMALLEST_CTRL_BTN_SIZE { get { return Mathf.Min(100.0f, Screen.height * 0.16f); } }
		protected static float CTRL_BTN_OFFSET { get { return Screen.height * 0.0115f; } }
		
		private PlayerController m_playerCtrl = null;
		protected bool IsInAir
		{
			get
			{
				if (m_playerCtrl == null)
				{
					m_playerCtrl = (PlayerController)GameObject.FindObjectOfType(typeof(PlayerController));
					if (m_playerCtrl != null)
					{
						return m_playerCtrl.State == PlayerState.Air;
					}
					else
					{
						return false;
					}
				}
				else
				{
					return m_playerCtrl.State == PlayerState.Air;
				}
			}
		}
	}

	public abstract class PlayerInputSimpleDraw : PlayerInputSimpleDrawBase
	{
		protected Rect m_jumpBtnRect;
		protected Rect m_slideBtnRect;
		protected Rect m_grabBtnRect;
		
		public override bool IsJump { get { return IsTouchInArea(m_jumpBtnRect) && !IsInAir; } }
		public override bool IsSlide
		{
			get
			{
#if SLIDING_SUPPORTED
				return UtilityMobile.IsTouchInArea(m_slideBtnRect) && !IsInAir;
#else
				return false;
#endif
			}
		}
		public override bool IsGrab { get { return IsTouchInArea(m_grabBtnRect) && IsInAir; } }
		
		public override void Draw()
		{
			// reset GUI matrix
			Matrix4x4 oldGUImatrix = GUI.matrix;
			GUI.matrix = Matrix4x4.identity;
			
			// Draw jump or grab button
			if (IsInAir)
			{
				if (IsGrab)
				{
					GUI.DrawTexture(m_grabBtnRect, ControlsConfig.Instance.BUTTON_CONTROL_GRAB_BTN_DOWN_IMG);
				}
				else
				{
					GUI.DrawTexture(m_grabBtnRect, ControlsConfig.Instance.BUTTON_CONTROL_GRAB_BTN_IMG);
				}
				GUI.DrawTexture(m_jumpBtnRect, ControlsConfig.Instance.BUTTON_CONTROL_JUMP_BTN_IMG);
#if SLIDING_SUPPORTED
				GUI.DrawTexture(m_slideBtnRect, ...);
#endif
			}
			else
			{
				if (IsJump)
				{
					GUI.DrawTexture(m_jumpBtnRect, ControlsConfig.Instance.BUTTON_CONTROL_JUMP_BTN_DOWN_IMG);
				}
				else
				{
					GUI.DrawTexture(m_jumpBtnRect, ControlsConfig.Instance.BUTTON_CONTROL_JUMP_BTN_IMG);
				}
#if SLIDING_SUPPORTED
				if (IsSlide)
				{
					GUI.DrawTexture(m_slideBtnRect, ...);
				}
				else
				{
					GUI.DrawTexture(m_slideBtnRect, ...);
				}
#endif
				if (PlayerController.GameOverType == EGameOverType.LANDED_WITH_GRAB)
				{
					float intensity = (Time.realtimeSinceStartup * 6000 % 1000) / 1000f;
					if (intensity > 0.5f)
					{
						intensity = 0.5f - (intensity - 0.5f);
					}
					GUI.color = Color.red * intensity*2f;
				}
				GUI.DrawTexture(m_grabBtnRect, ControlsConfig.Instance.BUTTON_CONTROL_GRAB_BTN_IMG);
			}
			
			GUI.matrix = oldGUImatrix;
			GUI.color = Color.white;
		}
	}

	public abstract class PlayerInputSimpleDrawWithStdBtns : PlayerInputSimpleDraw
	{
		public PlayerInputSimpleDrawWithStdBtns(bool p_isOtherCtrlOnRight)
			: base()
		{
			// init standard buttons used by buttons/joystick
			if (p_isOtherCtrlOnRight)
			{
				m_jumpBtnRect = new Rect(0, Screen.height - BIG_CTRL_BTN_SIZE, BIG_CTRL_BTN_SIZE, BIG_CTRL_BTN_SIZE);
				m_grabBtnRect = new Rect(m_jumpBtnRect);
				m_grabBtnRect.x = m_grabBtnRect.xMax + CTRL_BTN_OFFSET;
				m_grabBtnRect.width = SMALL_CTRL_BTN_SIZE;
				m_grabBtnRect.height = SMALL_CTRL_BTN_SIZE;
				m_grabBtnRect.y += BIG_CTRL_BTN_SIZE - SMALL_CTRL_BTN_SIZE;
#if SLIDING_SUPPORTED
				m_slideBtnRect = new Rect(m_jumpBtnRect);
				m_slideBtnRect.width = SMALL_CTRL_BTN_SIZE;
				m_slideBtnRect.height = SMALL_CTRL_BTN_SIZE;
				m_slideBtnRect.y = m_slideBtnRect.yMin - m_slideBtnRect.height - CTRL_BTN_OFFSET;
#endif
			}
			else
			{
				m_jumpBtnRect = new Rect(Screen.width - BIG_CTRL_BTN_SIZE, Screen.height - BIG_CTRL_BTN_SIZE, BIG_CTRL_BTN_SIZE, BIG_CTRL_BTN_SIZE);
				m_grabBtnRect = new Rect(m_jumpBtnRect);
				m_grabBtnRect.x -= SMALL_CTRL_BTN_SIZE + CTRL_BTN_OFFSET;
				m_grabBtnRect.width = SMALL_CTRL_BTN_SIZE;
				m_grabBtnRect.height = SMALL_CTRL_BTN_SIZE;
				m_grabBtnRect.y += BIG_CTRL_BTN_SIZE - SMALL_CTRL_BTN_SIZE;
#if SLIDING_SUPPORTED
				m_slideBtnRect = new Rect(m_jumpBtnRect);
				m_slideBtnRect.width = SMALL_CTRL_BTN_SIZE;
				m_slideBtnRect.height = SMALL_CTRL_BTN_SIZE;
				m_slideBtnRect.x = Screen.width - SMALL_CTRL_BTN_SIZE;
				m_slideBtnRect.y = m_slideBtnRect.yMin - m_slideBtnRect.height - CTRL_BTN_OFFSET;
#endif
			}
		}
	}
}
