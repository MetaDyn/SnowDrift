using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Snowboard
{
	public class GameCharacterHandler : MonoBehaviour
	{
#region Singleton Instance Logic

		public const string RESOURCE_PATH = "GameHandler\\GameCharacterHandler";

		protected static GameCharacterHandler s_instance;
		/// <summary>
		/// You can use the static Instance property to access the GameLevelHandler API from wherever you need it in your code. See also GameLevelHandler.IsInstanceSet.
		/// </summary>
		public static GameCharacterHandler Instance
		{
			get
			{
				// first try to find an existing instance
				if (s_instance == null)
				{
					s_instance = GameObject.FindObjectOfType<GameCharacterHandler>();
				}
				// if no instance exists yet, then create a new one
				if (s_instance == null)
				{
					if (Resources.Load<GameCharacterHandler>(RESOURCE_PATH) != null)
					{
						s_instance = Instantiate(Resources.Load<GameCharacterHandler>(RESOURCE_PATH));
					}
					if (s_instance == null)
					{
						Debug.LogError("GameCharacterHandler: could not load resource at path '" + RESOURCE_PATH + "'!");
						s_instance = new GameObject("Fallback GameCharacterHandler").AddComponent<GameCharacterHandler>();
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

#region Inspector Values and Properties

		[SerializeField]
		private string[] m_playerModelResourcePathes = new string[0];
		public string[] PlayerModelResourcePathes
		{
			get { return m_playerModelResourcePathes; }
			set { m_playerModelResourcePathes = value; }
		}

		[SerializeField]
		private int m_selectedPlayerModelIndex = 0;
		public int SelectedPlayerModelIndex
		{
			get { return m_selectedPlayerModelIndex; }
			set { m_selectedPlayerModelIndex = value; }
		}

#endregion

#region Selected Player Model Logic

		public string GetPlayerModelResourcePath()
		{
			if (m_selectedPlayerModelIndex >= 0 && m_playerModelResourcePathes != null && m_playerModelResourcePathes.Length > m_selectedPlayerModelIndex)
			{
				return m_playerModelResourcePathes[m_selectedPlayerModelIndex];
			}
			else
			{
				Debug.LogError("GameCharacterHandler: GetPlayerModelResourcePath: PlayerModelResourcePathes are not properly set!");
				return "CharacterModels\\PlayerModelDefault";
			}
		}

		public void SelectNextPlayerModel()
		{
			if (m_playerModelResourcePathes != null && m_playerModelResourcePathes.Length > 0)
			{
				m_selectedPlayerModelIndex = (m_selectedPlayerModelIndex + 1) % m_playerModelResourcePathes.Length;
			}
		}

		public void SelectPreviousPlayerModel()
		{
			if (m_playerModelResourcePathes != null && m_playerModelResourcePathes.Length > 0)
			{
				m_selectedPlayerModelIndex = (m_selectedPlayerModelIndex - 1) % m_playerModelResourcePathes.Length;
			}
		}

#endregion
	}
}
