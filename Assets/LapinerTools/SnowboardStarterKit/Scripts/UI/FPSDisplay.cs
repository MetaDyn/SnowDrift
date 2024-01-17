using UnityEngine;

namespace Snowboard
{
	public class FPSDisplay : MonoBehaviour
	{
		public float FontRelScreenSize = 0.025f;

		private float m_deltaTime = 0.0f;
		private GUIStyle m_style;
		private GUIStyle m_styleLog;
		private Rect m_rect;
		private Rect m_rectLog;

		private int m_screenHeight = 0;
		private string m_log = "";

		private void Update()
		{
			if (m_screenHeight != Screen.height)
			{
				int fontHeight = (int)(Screen.height * FontRelScreenSize);
				m_rect = new Rect(0, 0, Screen.width, fontHeight);
				m_style = new GUIStyle();
				m_style.alignment = TextAnchor.UpperLeft;
				m_style.fontSize = fontHeight;
				m_style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);

				m_styleLog = new GUIStyle(m_style);
				m_styleLog.fontSize = m_styleLog.fontSize / 2;
				m_rectLog = new Rect(0, m_rect.height*2f, m_rect.width, Screen.height - m_rect.height * 2f);
			}

			m_deltaTime += (Time.unscaledDeltaTime - m_deltaTime) * 0.1f;
		}

		private void OnEnable()
		{
			Application.logMessageReceived += Log;
		}

		private void OnDisable()
		{
			Application.logMessageReceived -= Log;
		}

		private void OnGUI()
		{
			float fps = 1.0f / m_deltaTime;
			string text = string.Format("{0:0.} fps", fps) + " " + Screen.width + "/" + Screen.height;
			GUI.Label(m_rect, text, m_style);
			GUI.Label(m_rectLog, m_log, m_styleLog);
		}

		private void Log(string logString, string stackTrace, LogType type)
		{
			m_log = type.ToString() + ": " + logString + "\n" + m_log;
			if (m_log.Length > 4000)
			{
				m_log = m_log.Substring(0, 4000);
			}
		}
	}
}
