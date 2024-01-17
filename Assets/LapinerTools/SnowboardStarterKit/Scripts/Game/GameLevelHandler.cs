using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Snowboard
{
	public class GameLevelHandler : MonoBehaviour
	{
#region Singleton Instance Logic

		public const string RESOURCE_PATH = "GameHandler\\GameLevelHandler";

		protected static GameLevelHandler s_instance;
		/// <summary>
		/// You can use the static Instance property to access the GameLevelHandler API from wherever you need it in your code. See also GameLevelHandler.IsInstanceSet.
		/// </summary>
		public static GameLevelHandler Instance
		{
			get
			{
				// first try to find an existing instance
				if (s_instance == null)
				{
					s_instance = GameObject.FindObjectOfType<GameLevelHandler>();
				}
				// if no instance exists yet, then create a new one
				if (s_instance == null)
				{
					if (Resources.Load<GameLevelHandler>(RESOURCE_PATH) != null)
					{
						s_instance = Instantiate(Resources.Load<GameLevelHandler>(RESOURCE_PATH));
					}
					if (s_instance == null)
					{
						Debug.LogError("GameLevelHandler: could not load resource at path '" + RESOURCE_PATH + "'!");
						s_instance = new GameObject("Fallback GameLevelHandler").AddComponent<GameLevelHandler>();
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
		private GameLevel[] m_levels = new GameLevel[0];
		public GameLevel[] Levels
		{
			get { return m_levels; }
			set { m_levels = value; }
		}

		[SerializeField]
		private int m_selectedLevelIndex = 0;
		public int SelectedLevelIndex
		{
			get { return m_selectedLevelIndex; }
			set { m_selectedLevelIndex = value; }
		}

#endregion

#region Selected Level Values

		private bool m_isLevelFinishedOrFailed;
		public bool IsLevelFinishedOrFailed
		{
			get { return m_isLevelFinishedOrFailed; }
			set { m_isLevelFinishedOrFailed = value; }
		}
#endregion

#region Selected Level Logic

		protected void Awake()
		{
			m_isLevelFinishedOrFailed = false;
		}

		public void Reset()
		{
			m_selectedLevelIndex = 0;
			m_isLevelFinishedOrFailed = false;
		}

		public GameLevel GetLevel()
		{
			if (m_selectedLevelIndex >= 0 && m_levels != null && m_levels.Length > m_selectedLevelIndex)
			{
				return m_levels[m_selectedLevelIndex];
			}
			else
			{
				Debug.LogError("GameLevelHandler: GetSceneName: SceneNames are not properly set!");
				return new GameLevel() { LevelSceneName = "Level 1" };
			}
		}

		public void SelectNextLevel()
		{
			if (m_levels != null && m_levels.Length > 0)
			{
				m_selectedLevelIndex = (m_selectedLevelIndex + 1) % m_levels.Length;
			}
		}

		public void SelectPreviousLevel()
		{
			if (m_levels != null && m_levels.Length > 0)
			{
				m_selectedLevelIndex = (m_selectedLevelIndex - 1) % m_levels.Length;
			}
		}

		public void LoadSelectedLevel()
		{
			IsLevelFinishedOrFailed = false;
			SceneManager.LoadScene(GetLevel().LevelSceneName);
		}

		public void EndLevel(PlayerController p_player)
		{
			IsLevelFinishedOrFailed = true;
			UILevel.Instance.OnLevelEnded(p_player);
		}

		public void GameOver(EGameOverType p_gameoverReason)
		{
			IsLevelFinishedOrFailed = true;
			UILevel.Instance.OnGameOver(p_gameoverReason);
		}

#endregion
	}
}
