using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Snowboard
{
	public class PlayerScoreHandler
	{
		public class ScoreData
		{
			public EPlayers Player { get; private set; }
			public int Score { get;  private set;}
			public string TrickName { get; private set; }

			public ScoreData(EPlayers p_player, int p_score, string p_trickName)
			{
				Player = p_player;
				Score = p_score;
				TrickName = p_trickName;
			}
		}

		private PlayerController m_playeCtrl;

		private int m_score = 0;
		private string m_trickName = "";
		
		private bool m_levelEnded = false;

		private bool m_isTricksAdded = false;
		private List<IPlayerScoreTrick> m_tricks = new List<IPlayerScoreTrick>();

		public event System.Action<ScoreData> OnReportScore;
		
		public PlayerScoreHandler(PlayerController p_playerController)
		{
			m_playeCtrl = p_playerController;
		}
		
		public void OnUpdate()
		{
			// init tricks
			if (!m_isTricksAdded && PlayerConfig.Instance.ENABLE_TRICK_SCORES)
			{
				m_isTricksAdded = true;
				m_tricks.Add(new PlayerScoreTrickAir(m_playeCtrl));
				m_tricks.Add(new PlayerScoreTrickRail(m_playeCtrl));
				m_tricks.Add(new PlayerScoreTrickAirRot(m_playeCtrl));
				m_tricks.Add(new PlayerScoreTrickSlide(m_playeCtrl));
				m_tricks.Add(new PlayerScoreTrickWallride(m_playeCtrl));
				m_tricks.Add(new PlayerScoreTrickSpeed(m_playeCtrl));
			}

			updateTrickScore(false);
			
			reportScore();
		}

		public void OnDestroy()
		{
			OnReportScore = null;
		}
		
		public void LevelEnd()
		{
			if (!m_levelEnded)
			{
				updateTrickScore(true);
				reportScore();
				m_levelEnded = true;
			}
		}
		
		private void updateTrickScore(bool pLevelEnd)
		{
			m_trickName = "";
			
			string lastConnective = null;
			foreach(IPlayerScoreTrick trick in m_tricks)
			{
				if (!pLevelEnd)
				{
					trick.OnUpdate();
				}
				else
				{
					trick.EndLevel();
				}
				if (trick.Score != 0)
				{
					if (lastConnective != null)
					{
						m_trickName += " " + lastConnective + " ";
					}
					m_trickName += trick.Name;
					m_score += trick.Score;
					lastConnective = trick.Connective;
				}
			}
		}
		
		private void reportScore()
		{
			if (m_trickName != "")
			{
				// clean up trick description
				if (m_trickName.Contains(IPlayerScoreTrick.GRAB_PREFIX))
				{
					m_trickName = IPlayerScoreTrick.GRAB_PREFIX + m_trickName.Replace(IPlayerScoreTrick.GRAB_PREFIX, "");
				}
					
				if (OnReportScore != null)
				{
					OnReportScore(new ScoreData(m_playeCtrl.m_playerIndex, m_score, m_trickName));
				}
				else
				{
					Debug.Log("PlayerScoreHandler: reportScore: " + m_playeCtrl.m_playerIndex + " " + m_score + " " + m_trickName + "\nRegister to PlayerController.ScoreHandler.OnReportScore to handle score data!");
				}
			}
		}
	}

	public abstract class IPlayerScoreTrick
	{
		public const string GRAB_PREFIX = "Grabbed ";
		
		/// <summary>
		/// Generates trick score and names 
		/// </summary>
		public abstract void OnUpdate();
		
		/// <summary>
		/// Generates trick score and names 
		/// </summary>
		public abstract void EndLevel();
		
		/// <summary>
		/// Tricks score (0 when there was no trick)
		/// Must be polled before next OnUpdate() call
		/// </summary>
		public abstract int Score
		{
			get;
		}
		
		/// <summary>
		/// Tricks name (empty when there was no trick)
		/// Must be polled before next OnUpdate() call
		/// </summary>
		public abstract string Name
		{
			get;
		}
		
		/// <summary>
		/// Gets the connective word between this and the next trick ("to" or "+" etc.)
		/// </summary>
		public abstract string Connective
		{
			get;
		}
	}
}
