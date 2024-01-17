using UnityEngine;

namespace Snowboard
{
	public class PlayerInputTouchButtons : PlayerInputSimpleDrawWithStdBtns
	{
		private class ArrowButton
		{
			private Rect m_btnRect;
			private Rect m_touchRect;
			private Texture2D m_btnTex;
			private Texture2D m_btnDownTex;
			private bool m_isDown = false;
			
			public ArrowButton (Rect p_rect, float p_touchExtend, Texture2D p_btn, Texture2D p_btnDown)
			{
				m_btnRect = p_rect;
				m_touchRect = new Rect(m_btnRect);
				m_touchRect.xMin -= p_touchExtend;
				m_touchRect.yMin -= p_touchExtend;
				m_touchRect.xMax += p_touchExtend;
				m_touchRect.yMax += p_touchExtend;
				m_btnTex = p_btn;
				m_btnDownTex = p_btnDown;
			}
			
			public void Draw ()
			{
				m_isDown = IsTouchInArea(m_touchRect);
				if (m_isDown)
				{
					GUI.DrawTexture(m_btnRect, m_btnDownTex);
				}
				else
				{
					GUI.DrawTexture(m_btnRect, m_btnTex);
				}
			}
			
			public bool IsDown { get { return m_isDown; } }
		}
		
		private ArrowButton m_upBtn;
		private ArrowButton m_downBtn;
		private ArrowButton m_leftBtn;
		private ArrowButton m_rightBtn;
		private ArrowButton m_upLeftBtn;
		private ArrowButton m_upRightBtn;
		private ArrowButton m_downLeftGroundBtn;
		private ArrowButton m_downRightGroundBtn;
		private ArrowButton m_downLeftAirBtn;
		private ArrowButton m_downRightAirBtn;
		private bool m_isSimple;
		
		private Vector2 m_axis = Vector2.zero;
		public override float AxisX
		{
			get
			{
				return m_axis.x;
			}
		}
		public override float AxisY
		{
			get
			{
				return m_axis.y;
			}
		}
			
		public PlayerInputTouchButtons(float p_size, bool isOnRight, bool isSimpleMode)
			: base(isOnRight)
		{		
			m_isSimple = isSimpleMode;
			// calc rects
			float realSize = GetRealSize(p_size)*SMALLEST_CTRL_BTN_SIZE;
			Rect downRightBtnRect;
			if (isOnRight)
			{
				downRightBtnRect = new Rect(
					Screen.width - realSize - 2f*CTRL_BTN_OFFSET,
					Screen.height - realSize - CTRL_BTN_OFFSET,
					realSize, realSize);
			}
			else
			{
				downRightBtnRect = new Rect(
					2f*realSize + 4f*CTRL_BTN_OFFSET,
					Screen.height - realSize - CTRL_BTN_OFFSET,
					realSize, realSize);
			}
			Rect downBtnRect = new Rect(downRightBtnRect);
			downBtnRect.x -= downBtnRect.width + CTRL_BTN_OFFSET;
			Rect downLeftBtnRect = new Rect(downBtnRect);
			downLeftBtnRect.x -= downBtnRect.width + CTRL_BTN_OFFSET;
			Rect rigtBtnRect = new Rect(downRightBtnRect);
			rigtBtnRect.y -= downBtnRect.height + CTRL_BTN_OFFSET;
			Rect leftBtnRect = new Rect(rigtBtnRect);
			leftBtnRect.x -= 2*(leftBtnRect.width + CTRL_BTN_OFFSET);
			Rect upRightBtnRect = new Rect(rigtBtnRect);
			upRightBtnRect.y -= upRightBtnRect.height + CTRL_BTN_OFFSET;
			Rect upBtnRect = new Rect(upRightBtnRect);
			upBtnRect.x -= upBtnRect.width + CTRL_BTN_OFFSET;
			Rect upLeftBtnRect = new Rect(upBtnRect);
			upLeftBtnRect.x -= upBtnRect.width + CTRL_BTN_OFFSET;
			rigtBtnRect.xMin -= rigtBtnRect.width*0.5f;
			leftBtnRect.xMax += leftBtnRect.width*0.5f;
			
			// init buttons
			m_upBtn = new ArrowButton(upBtnRect, 0.5f*CTRL_BTN_OFFSET, ControlsConfig.Instance.BUTTON_CONTROL_UP_IMG, ControlsConfig.Instance.BUTTON_CONTROL_UP_DOWN_IMG);
			m_downBtn = new ArrowButton(downBtnRect, 0.5f*CTRL_BTN_OFFSET, ControlsConfig.Instance.BUTTON_CONTROL_DOWN_IMG, ControlsConfig.Instance.BUTTON_CONTROL_DOWN_DOWN_IMG);
			m_leftBtn = new ArrowButton(leftBtnRect, 0.5f*CTRL_BTN_OFFSET, ControlsConfig.Instance.BUTTON_CONTROL_LEFT_IMG, ControlsConfig.Instance.BUTTON_CONTROL_LEFT_DOWN_IMG);
			m_rightBtn = new ArrowButton(rigtBtnRect, 0.5f*CTRL_BTN_OFFSET, ControlsConfig.Instance.BUTTON_CONTROL_RIGHT_IMG, ControlsConfig.Instance.BUTTON_CONTROL_RIGHT_DOWN_IMG);
			if (!m_isSimple)
			{
				m_upLeftBtn = new ArrowButton(upLeftBtnRect, 0.5f*CTRL_BTN_OFFSET, ControlsConfig.Instance.BUTTON_CONTROL_UPLEFT_IMG, ControlsConfig.Instance.BUTTON_CONTROL_UPLEFT_DOWN_IMG);
				m_upRightBtn = new ArrowButton(upRightBtnRect, 0.5f*CTRL_BTN_OFFSET, ControlsConfig.Instance.BUTTON_CONTROL_UPRIGHT_IMG, ControlsConfig.Instance.BUTTON_CONTROL_UPRIGHT_DOWN_IMG);
				m_downLeftGroundBtn = new ArrowButton(downLeftBtnRect, 0.5f*CTRL_BTN_OFFSET, ControlsConfig.Instance.BUTTON_CONTROL_LEFTSMALL_IMG, ControlsConfig.Instance.BUTTON_CONTROL_LEFTSMALL_DOWN_IMG);
				m_downRightGroundBtn = new ArrowButton(downRightBtnRect, 0.5f*CTRL_BTN_OFFSET, ControlsConfig.Instance.BUTTON_CONTROL_RIGHTSMALL_IMG, ControlsConfig.Instance.BUTTON_CONTROL_RIGHTSMALL_DOWN_IMG);
				m_downLeftAirBtn = new ArrowButton(downLeftBtnRect, 0.5f*CTRL_BTN_OFFSET, ControlsConfig.Instance.BUTTON_CONTROL_DOWNLEFT_IMG, ControlsConfig.Instance.BUTTON_CONTROL_DOWNLEFT_DOWN_IMG);
				m_downRightAirBtn = new ArrowButton(downRightBtnRect, 0.5f*CTRL_BTN_OFFSET, ControlsConfig.Instance.BUTTON_CONTROL_DOWNRIGHT_IMG, ControlsConfig.Instance.BUTTON_CONTROL_DOWNRIGHT_DOWN_IMG);
			}
		}
		
		public override void SaveCurrentAsCalibration () { }
		
		public override void Draw()
		{
			base.Draw();
			// reset GUI matrix
			Matrix4x4 oldGUImatrix = GUI.matrix;
			GUI.matrix = Matrix4x4.identity;
			
			// draw and calculate is down states
			m_upBtn.Draw ();
			m_downBtn.Draw ();
			m_leftBtn.Draw ();
			m_rightBtn.Draw ();
			if (!m_isSimple)
			{
				m_upLeftBtn.Draw ();
				m_upRightBtn.Draw ();
				if (IsInAir)
				{
					m_downLeftAirBtn.Draw ();
					m_downRightAirBtn.Draw ();
				}
				else
				{
					m_downLeftGroundBtn.Draw ();
					m_downRightGroundBtn.Draw ();
				}
			}
			
			// update axis results
			m_axis = Vector2.zero;
			if (!m_isSimple && !IsInAir)
			{
				if (m_downLeftGroundBtn.IsDown)
				{
					m_axis.x -= 0.5f; // no down buttons, but half left right buttons
				}
				if (m_downRightGroundBtn.IsDown)
				{
					m_axis.x += 0.5f; // no down buttons, but half left right buttons
				}
			}
			if (m_leftBtn.IsDown || (!m_isSimple && (m_upLeftBtn.IsDown || (m_downLeftAirBtn.IsDown && IsInAir))))
			{
				m_axis.x -= 1;
			}
			if (m_rightBtn.IsDown || (!m_isSimple && (m_upRightBtn.IsDown || (m_downRightAirBtn.IsDown && IsInAir))))
			{
				m_axis.x += 1;
			}
			if (m_upBtn.IsDown || (!m_isSimple && (m_upRightBtn.IsDown || m_upLeftBtn.IsDown)))
			{
				m_axis.y += 1;
			}
			if (m_downBtn.IsDown || (!m_isSimple && ((m_downRightAirBtn.IsDown && IsInAir) || (m_downLeftAirBtn.IsDown && IsInAir))))
			{
				m_axis.y -= 1;
			}
			m_axis.x = Mathf.Clamp(m_axis.x, -1f, 1f);
			
			GUI.matrix = oldGUImatrix;
		}
		
		private float GetRealSize (float p_size)
		{
			float result;
			float fact = Mathf.Clamp01(p_size);
			if (p_size != fact)
			{
				Debug.LogError("PlayerInputAccelerator: Sensitivity: value must be inside [0,1]!");
			}
			fact = 1f-fact;
			// convert [0,1] to inverted [0.75,1.5]
			if (fact >= 0.5f)
			{
				result = 1.5f - fact; // -> [0.5,1]
				result = Mathf.Sqrt (result); // -> [0.7,1]
			}
			else
			{
				result = 2f - 2f*fact; // -> [1, 2]
				result = Mathf.Pow (result, 0.25f); // -> [1,1.19]
			}
			return result;
		}
	}
}
