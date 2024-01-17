using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LapinerTools.uMyGUI;

namespace Snowboard
{
	public class UILevel : MonoBehaviour
	{
		/// <summary>
		/// You can use the static Instance property to access the UILevel API from wherever you need it in your code. See also UILevel.IsInstanceSet.
		/// </summary>
		public static UILevel Instance
		{
			get
			{
				return GameObject.FindObjectOfType<UILevel>();
			}
		}

		[SerializeField]
		private Text m_scoreText;
		public Text ScoreText
		{
			get { return m_scoreText; }
			set { m_scoreText = value; }
		}

		private bool m_isInitDone = false;
		private int m_levelScore = 0;

		public void OnReportScore(PlayerScoreHandler.ScoreData p_scoreData)
		{
			m_levelScore += p_scoreData.Score;
			if (m_scoreText != null)
			{
				m_scoreText.text = "Score: " + m_levelScore + "\n" + p_scoreData.TrickName;
			}
		}

		public void OnExitButtonClick()
		{
			SceneManager.LoadScene(0);
		}

		public void OnRestartButtonClick()
		{
			GameLevelHandler.Instance.LoadSelectedLevel();
		}

		public void OnLoadNextLevelButtonClick()
		{
			GameLevelHandler.Instance.SelectNextLevel();
			GameLevelHandler.Instance.LoadSelectedLevel();
		}

		public void OnLevelEnded(PlayerController p_player)
		{
			if (GameLevelHandler.Instance.SelectedLevelIndex < GameLevelHandler.Instance.Levels.Length - 1)
			{
				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
					.SetText("Level Finished", "Score: " + m_levelScore + "\nDo you want to try the next level?")
					.ShowButton(uMyGUI_PopupManager.BTN_YES, OnLoadNextLevelButtonClick)
					.ShowButton(uMyGUI_PopupManager.BTN_NO, OnExitButtonClick);
			}
			else
			{
				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
					.SetText("Level Finished", "Score: " + m_levelScore + "\nCongrats, you made it! You finished all levels!")
					.ShowButton(uMyGUI_PopupManager.BTN_OK, OnExitButtonClick);
			}
		}

		public void OnGameOver(EGameOverType p_gameoverReason)
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
				.SetText("Game Over", "Retry?")
				.ShowButton(uMyGUI_PopupManager.BTN_YES, OnRestartButtonClick)
				.ShowButton(uMyGUI_PopupManager.BTN_NO, OnExitButtonClick);
		}

		private void Start()
		{
			GameQualityHandler.Instance.ApplyQualitySetting(GameDatabaseHandler.Instance.GetInt(GameDatabaseHandler.IntVars.QUALITY_SETTING_INDEX));
		}

		private void Update()
		{
			if (!m_isInitDone)
			{
				PlayerController playerCtrl = FindObjectOfType<PlayerController>();
				if (playerCtrl != null)
				{
					m_isInitDone = true;
					playerCtrl.ScoreHandler.OnReportScore += OnReportScore;
				}
			}
		}

		private void OnGUI()
		{
			PlayerInput.Draw();
		}
	}
}
