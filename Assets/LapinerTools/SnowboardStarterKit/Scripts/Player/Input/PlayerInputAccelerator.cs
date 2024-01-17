using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerInputAccelerator : PlayerInputSimpleDraw
	{
		private const float MAX_AXIS_ACCELERATION = 0.27f;
		
		private float m_sensitivity = 1.0f;
		public float Sensitivity
		{
			get
			{
				return m_sensitivity;
			}
			set
			{
				float fact = Mathf.Clamp01(value);
				if (value != fact)
				{
					Debug.LogError("PlayerInputAccelerator: Sensitivity: value must be inside [0,1]!");
				}
				// convert [0,1] to inverted [0.5,2]
				if (fact >= 0.5f)
				{
					m_sensitivity = 1.5f - fact;
				}
				else
				{
					m_sensitivity = 2f - 2f*fact;
				}
			}
		}
		
		private bool m_useCalibration = false;
		private Quaternion m_calibration = Quaternion.identity;
		public Quaternion Calibration
		{
			get
			{
				return m_calibration;
			}
			set
			{
				if (value != Quaternion.identity)
				{
					m_calibration = value;
					m_useCalibration = true;
				}
				else
				{
					m_useCalibration = false;
				}
			}
		}
		
		public override float AxisX
		{
			get
			{
				Vector3 acceleration = Input.acceleration;
				if (m_useCalibration)
				{
					acceleration = m_calibration * acceleration;
				}
				float result = Mathf.Clamp01((Mathf.Abs(acceleration.x)) / (MAX_AXIS_ACCELERATION*m_sensitivity));
				result *= result;
				return result * Mathf.Sign(acceleration.x);
			}
		}
		
		public override float AxisY
		{
			get
			{
				Vector3 acceleration = Input.acceleration;
				if (m_useCalibration)
				{
					acceleration = m_calibration * acceleration;
				}
				float result = Mathf.Clamp01((Mathf.Abs(acceleration.y)) / (MAX_AXIS_ACCELERATION*m_sensitivity));
				result *= result;
				return result * Mathf.Sign(acceleration.y);
			}
		}
		
		public PlayerInputAccelerator()
		{
			m_jumpBtnRect = new Rect(Screen.width - BIG_CTRL_BTN_SIZE - CTRL_BTN_OFFSET, (Screen.height - BIG_CTRL_BTN_SIZE) * 0.6f, BIG_CTRL_BTN_SIZE, BIG_CTRL_BTN_SIZE);
			m_grabBtnRect = new Rect(m_jumpBtnRect);
			m_grabBtnRect.y = m_grabBtnRect.yMax + CTRL_BTN_OFFSET;
			m_grabBtnRect.x += BIG_CTRL_BTN_SIZE - SMALL_CTRL_BTN_SIZE;
			m_grabBtnRect.width = SMALL_CTRL_BTN_SIZE;
			m_grabBtnRect.height = SMALL_CTRL_BTN_SIZE;
#if SLIDING_SUPPORTED
			m_slideBtnRect = new Rect(CTRL_BTN_OFFSET, (Screen.height - BIG_CTRL_BTN_SIZE) * 0.6f, BIG_CTRL_BTN_SIZE, BIG_CTRL_BTN_SIZE);
#endif
		}
		
		public override void SaveCurrentAsCalibration ()
		{
			Calibration = Quaternion.FromToRotation(Input.acceleration.normalized, Vector3.back); 
		}
	}
}
