using UnityEngine;
using System.Collections;

namespace Snowboard
{
	public class PlayerScoreTrickSlide : PlayerScoreTrickGroundedSpinBase
	{
		public PlayerScoreTrickSlide(PlayerController p_playerController)
			: base(p_playerController)
		{
		}

		protected override bool IsValidPlayerState
		{
			get
			{
				return
					m_playerStrl.State == PlayerState.Ground ||
					m_playerStrl.State == PlayerState.TakeOff;
			}
		}
		protected override string TrickNamePostFix { get{ return " Slide"; } }
	}
}
