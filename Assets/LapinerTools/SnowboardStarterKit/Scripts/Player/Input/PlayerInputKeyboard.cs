using UnityEngine;

namespace Snowboard
{
	public class PlayerInputKeyboard : PlayerInputDevice
	{
		public override float AxisX
		{
			get
			{
				if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
				{
					return -1f;
				}
				else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
				{
					return 1f;
				}
				else
				{
					return 0;
				}
			}
		}
		public override float AxisY
		{
			get
			{
				if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
				{
					return -1f;
				}
				else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
				{
					return 1f;
				}
				else
				{
					return 0;
				}
			}
		}

		public override bool IsJump
		{
			get
			{
				return Input.GetKeyDown(KeyCode.Space);
			}
		}
		public override bool IsSlide
		{
			get
			{
				return Input.GetKey(KeyCode.LeftControl);
			}
		}
		public override bool IsGrab
		{
			get
			{
				return Input.GetKey(KeyCode.V);
			}
		}

		public override void Draw () {}

		public override void SaveCurrentAsCalibration () { }
	}
}
