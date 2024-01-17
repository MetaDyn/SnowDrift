using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerInputTouchJoystick : PlayerInputSimpleDrawWithStdBtns
	{
		private float X_AXIS_JOYSTICK_DEADZONE { get{ return 0.001f + 0.0005f*Mathf.Abs(m_axisY); } }
		private float Y_AXIS_JOYSTICK_DEADZONE { get{ return 0.065f + 0.045f*Mathf.Abs(m_axisX); } }
		
		protected Rect m_targetRect;
		protected float m_dotScaling = 0.1f;
		private float m_halfWidth;
		private float m_halfHeight;
		
		private Texture2D m_bgTex;
		private Texture2D m_stickTex;
		
		private int m_lastUpdateFrame = -1;
		private int m_touchID = -1;
		
		private float m_axisX;
		public override float AxisX
		{
			get
			{
				updateTouches();
				return m_axisX;
			}
		}
		
		private float m_axisY;
		public override float AxisY
		{
			get
			{
				updateTouches();
				return -m_axisY;
			}
		}
		
		private Rect m_oversizeRect;
		public Rect TargetRect
		{
			get
			{
				return m_targetRect;
			}
			set
			{
				m_targetRect = value;
				bool isOnRight = m_targetRect.center.x > Screen.width*0.5f;
				float stdBtnsMinX = Mathf.Min(m_jumpBtnRect.xMin,
	#if SLIDING_SUPPORTED
				                              m_slideBtnRect.xMin,
	#endif
				                              m_grabBtnRect.xMin);
				float stdBtnsMaxX = Mathf.Max(m_jumpBtnRect.xMax,
	#if SLIDING_SUPPORTED
				                              m_slideBtnRect.xMax,
	#endif
				                              m_grabBtnRect.xMax);
				m_oversizeRect = new Rect();
				if (isOnRight)
				{
					m_oversizeRect.xMin = Mathf.Max(m_targetRect.xMin - m_targetRect.width * 0.75f, stdBtnsMaxX + Screen.width*0.05f);
					m_oversizeRect.xMax = Screen.width;
				}
				else
				{
					m_oversizeRect.xMin = 0;
					m_oversizeRect.xMax = Mathf.Min(m_targetRect.xMax + m_targetRect.width * 0.75f, stdBtnsMinX - Screen.width*0.05f);
				}
				m_oversizeRect.yMin = 0;
				m_oversizeRect.yMax = Screen.height;
				m_touchID = -1;
				m_halfWidth = m_targetRect.width*0.5f;
				m_halfHeight = m_targetRect.height*0.5f;
			}
		}
		
		public PlayerInputTouchJoystick(float size, bool isOnRight)
			: base(isOnRight)
		{
			float joystickSize = Mathf.Min(256.0f, Screen.width * 0.21f);
			joystickSize += joystickSize * 0.75f*size;
			if (isOnRight)
			{
				TargetRect = new Rect(Screen.width-joystickSize-2f*CTRL_BTN_OFFSET, Screen.height-joystickSize-2f*CTRL_BTN_OFFSET, joystickSize, joystickSize);
			}
			else
			{
				TargetRect = new Rect(2f*CTRL_BTN_OFFSET, Screen.height-joystickSize-2f*CTRL_BTN_OFFSET, joystickSize, joystickSize);
			}
			m_dotScaling = 0.5f;
			m_bgTex = ControlsConfig.Instance.JOYSTICK_CONTROL_BACKGROUND_IMG;
			m_stickTex = ControlsConfig.Instance.JOYSTICK_CONTROL_THUMB_IMG;
		}
		
		public override void Draw ()
		{
			base.Draw();
			
			// reset GUI matrix
			Matrix4x4 oldGUImatrix = GUI.matrix;
			GUI.matrix = Matrix4x4.identity;
			
			// Background
			GUI.DrawTexture(m_targetRect, m_bgTex);
			
			// Stick
			Vector2 dotOffset;
			dotOffset.x = m_halfWidth * AxisX;
			dotOffset.y =-m_halfHeight * AxisY;
			float dotDist = dotOffset.magnitude;
			if (dotOffset.x > m_halfWidth)
			{
				dotOffset.x *= (m_halfWidth) / dotDist;
			}
			if (dotOffset.y > m_halfWidth)
			{
				dotOffset.y *= (m_halfWidth) / dotDist;
			}
			Vector2 dotPos;
			dotPos.x = m_targetRect.xMin + m_halfWidth * (1.0f - m_dotScaling);
			dotPos.y = m_targetRect.yMin + m_halfHeight * (1.0f - m_dotScaling);
			dotPos += dotOffset;
			GUI.DrawTexture(new Rect(dotPos.x, dotPos.y, m_targetRect.width * m_dotScaling, m_targetRect.height * m_dotScaling), m_stickTex);
			
			GUI.matrix = oldGUImatrix;
		}
		
		public override void SaveCurrentAsCalibration () { }
		
		private void updateTouches()
		{
			if (m_lastUpdateFrame != Time.frameCount)
			{
				m_lastUpdateFrame = Time.frameCount;
				
				// update touches
	#if UNITY_EDITOR
				if (Input.GetMouseButton(0))
				{
					int fingerID = 0;
					Vector2 tPos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
	#else
				for (int i=0; i<Input.touchCount; i++)
				{
					Touch t = Input.GetTouch(i);
					int fingerID = t.fingerId;
					Vector2 tPos = new Vector2(t.position.x, Screen.height - t.position.y);
	#endif
					// check for touch inside GUI
					if (m_targetRect.Contains(tPos))
					{
						Vector2 axisValues = getAxisValues(tPos);
						m_axisX = axisValues.x;
						m_axisY = axisValues.y;
						//m_axisX = (((tPos.x - m_targetRect.xMin) / m_targetRect.width) - 0.5f) * 2.0f;
						//m_axisY = (((tPos.y - m_targetRect.yMin) / m_targetRect.height) - 0.5f) * 2.0f;
						m_touchID = fingerID;
						applyDeadZone();
						return;
					}
					// check for touch beeing dragged out of the GUI
					if (fingerID == m_touchID)
					{
						Vector2 axisValues = getAxisValues(tPos);
						m_axisX = axisValues.x;
						m_axisY = axisValues.y;
						//m_axisX = (((tPos.x - m_targetRect.xMin) / m_targetRect.width) - 0.5f) * 2.0f;
						//m_axisY = (((tPos.y - m_targetRect.yMin) / m_targetRect.height) - 0.5f) * 2.0f;
						//float max = Mathf.Max(Mathf.Abs(m_axisX), Mathf.Abs(m_axisY));
						//m_axisX /= max;
						//m_axisY /= max;
						applyDeadZone();
						return;
					}
					// check for touch close to the GUI
					if (m_oversizeRect.Contains(tPos))
					{
						m_touchID = fingerID;
						return;
					}
				}
				m_axisX = 0f;
				m_axisY = 0f;
				m_touchID = -1;
			}
		}
		
		private Vector2 getAxisValues(Vector2 pTouchPos)
		{
			Vector2 tPosNorm = new Vector2(pTouchPos.x, pTouchPos.y);
			tPosNorm -= m_targetRect.center;
			tPosNorm /= m_halfWidth;
			if (Mathf.Abs(tPosNorm.x) > 1.0f || Mathf.Abs(tPosNorm.y) > 1.0f)
			{
				tPosNorm /= Mathf.Max(Mathf.Abs(tPosNorm.x), Mathf.Abs(tPosNorm.y));
			}
			// smoothing example:
			// if y axis is one than the x axis is calculated exponentially and vice versa
			float expX = Mathf.Sign(tPosNorm.x)*tPosNorm.x*tPosNorm.x;
			float expY = Mathf.Sign(tPosNorm.y)*tPosNorm.y*tPosNorm.y;
			float combX = Mathf.Lerp(tPosNorm.x, expX, Mathf.Abs(tPosNorm.y)-Mathf.Abs(tPosNorm.x));
			float combY = Mathf.Lerp(tPosNorm.y, expY, Mathf.Abs(tPosNorm.x)-Mathf.Abs(tPosNorm.y));
			tPosNorm.x = combX;
			tPosNorm.y = combY;
			return tPosNorm;
		}
		
		private void applyDeadZone()
		{
			if (Mathf.Abs(m_axisX)<X_AXIS_JOYSTICK_DEADZONE)
			{
				m_axisX = 0f;
			}
			else
			{
				m_axisX = Mathf.Sign(m_axisX)*(Mathf.Abs(m_axisX) - X_AXIS_JOYSTICK_DEADZONE)/(1f - X_AXIS_JOYSTICK_DEADZONE);
			}
			
			if (Mathf.Abs(m_axisY)<Y_AXIS_JOYSTICK_DEADZONE)
			{
				m_axisY = 0f;
			}
			else
			{
				m_axisY = Mathf.Sign(m_axisY)*(Mathf.Abs(m_axisY) - Y_AXIS_JOYSTICK_DEADZONE)/(1f - Y_AXIS_JOYSTICK_DEADZONE);
			}
		}
	}
}
