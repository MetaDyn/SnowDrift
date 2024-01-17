using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Snowboard
{
	public class GameDatabaseHandler : MonoBehaviour
	{
#region Singleton Instance Logic

		public const string RESOURCE_PATH = "GameHandler\\GameDatabaseHandler";

		protected static GameDatabaseHandler s_instance;
		/// <summary>
		/// You can use the static Instance property to access the GameLevelHandler API from wherever you need it in your code. See also GameDatabaseHandler.IsInstanceSet.
		/// </summary>
		public static GameDatabaseHandler Instance
		{
			get
			{
				// first try to find an existing instance
				if (s_instance == null)
				{
					s_instance = GameObject.FindObjectOfType<GameDatabaseHandler>();
				}
				// if no instance exists yet, then create a new one
				if (s_instance == null)
				{
					if (Resources.Load<GameDatabaseHandler>(RESOURCE_PATH) != null)
					{
						s_instance = Instantiate(Resources.Load<GameDatabaseHandler>(RESOURCE_PATH));
					}
					if (s_instance == null)
					{
						Debug.LogError("GameDatabaseHandler: could not load resource at path '" + RESOURCE_PATH + "'!");
						s_instance = new GameObject("Fallback GameDatabaseHandler").AddComponent<GameDatabaseHandler>();
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

		[System.Serializable]
		public class BoolVarDefault
		{
			public string Key;
			public bool Value;

			public BoolVarDefault(string p_key, bool p_value)
			{
				Key = p_key;
				Value = p_value;
			}
		}

		[System.Serializable]
		public class IntVarDefault
		{
			public string Key;
			public int Value;

			public IntVarDefault(string p_key, int p_value)
			{
				Key = p_key;
				Value = p_value;
			}
		}

		public enum BoolVars
		{
			IS_CONTROL_MODE_BUTTONS,
			IS_CONTROL_MODE_JOYSTICK,
			IS_CONTROL_MODE_TILT,
			IS_CONTROL_ON_RIGHT,
			IS_BUTTON_CONTROL_SIMPLE
		}

		public enum IntVars
		{
			QUALITY_SETTING_INDEX
		}

		[SerializeField]
		private BoolVarDefault[] m_BOOL_VARS_DEFAULTS = new BoolVarDefault[5]
		{
			new	BoolVarDefault(BoolVars.IS_CONTROL_MODE_BUTTONS.ToString(), true),
			new	BoolVarDefault(BoolVars.IS_CONTROL_MODE_JOYSTICK.ToString(), false),
			new	BoolVarDefault(BoolVars.IS_CONTROL_MODE_TILT.ToString(), false),
			new	BoolVarDefault(BoolVars.IS_CONTROL_ON_RIGHT.ToString(), false),
			new	BoolVarDefault(BoolVars.IS_BUTTON_CONTROL_SIMPLE.ToString(), false)
		};
		private Dictionary<string, bool> m_boolVarsDefaults;

		[SerializeField]
		private IntVarDefault[] m_INT_VARS_DEFAULTS = new IntVarDefault[1]
		{
			new IntVarDefault(IntVars.QUALITY_SETTING_INDEX.ToString(), 2)
		};
		private Dictionary<string, int> m_intVarsDefaults;


		public void Awake()
		{
			m_boolVarsDefaults = new Dictionary<string, bool>();
			for (int i=0; i<m_BOOL_VARS_DEFAULTS.Length; i++)
			{
				m_boolVarsDefaults.Add(m_BOOL_VARS_DEFAULTS[i].Key, m_BOOL_VARS_DEFAULTS[i].Value);
			}

			m_intVarsDefaults = new Dictionary<string, int>();
			for (int i = 0; i < m_INT_VARS_DEFAULTS.Length; i++)
			{
				m_intVarsDefaults.Add(m_INT_VARS_DEFAULTS[i].Key, m_INT_VARS_DEFAULTS[i].Value);
			}
		}

		public bool GetBool(BoolVars p_var)
		{
			string key = p_var.ToString();
			bool? defaultValue = m_boolVarsDefaults[key];
			return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) == 1 : (defaultValue.HasValue ? defaultValue.Value : false);
		}

		public int GetInt(IntVars p_var)
		{
			string key = p_var.ToString();
			int? defaultValue = m_intVarsDefaults[key];
			return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (defaultValue.HasValue ? defaultValue.Value : -1);
		}

		public void SetBool(BoolVars p_var, bool p_value)
		{
			PlayerPrefs.SetInt(p_var.ToString(), p_value ? 1 : 0);
			PlayerPrefs.Save();
		}

		public void SetInt(IntVars p_var, int p_value)
		{
			PlayerPrefs.SetInt(p_var.ToString(), p_value);
			PlayerPrefs.Save();
		}
	}
}
