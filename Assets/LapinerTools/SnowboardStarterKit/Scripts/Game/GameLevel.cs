using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Snowboard
{
	[System.Serializable]
	public class GameLevel : MonoBehaviour
	{
		[SerializeField]
		private Sprite m_levelPreviewSprite = null;
		public Sprite LevelPreviewSprite
		{
			get { return m_levelPreviewSprite; }
			set { m_levelPreviewSprite = value; }
		}

		[SerializeField]
		private string m_levelSceneName = null;
		public string LevelSceneName
		{
			get { return m_levelSceneName; }
			set { m_levelSceneName = value; }
		}
	}
}
