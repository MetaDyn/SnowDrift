using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Snowboard
{
	public class GameQualityHandler : MonoBehaviour
	{
		#region Singleton Instance Logic

		public const string RESOURCE_PATH = "GameHandler\\GameQualityHandler";

		protected static GameQualityHandler s_instance;
		/// <summary>
		/// You can use the static Instance property to access the GameQualityHandler API from wherever you need it in your code. See also GameQualityHandler.IsInstanceSet.
		/// </summary>
		public static GameQualityHandler Instance
		{
			get
			{
				// first try to find an existing instance
				if (s_instance == null)
				{
					s_instance = GameObject.FindObjectOfType<GameQualityHandler>();
				}
				// if no instance exists yet, then create a new one
				if (s_instance == null)
				{
					if (Resources.Load<GameQualityHandler>(RESOURCE_PATH) != null)
					{
						s_instance = Instantiate(Resources.Load<GameQualityHandler>(RESOURCE_PATH));
					}
					if (s_instance == null)
					{
						Debug.LogError("GameQualityHandler: could not load resource at path '" + RESOURCE_PATH + "'!");
						s_instance = new GameObject("Fallback GameQualityHandler").AddComponent<GameQualityHandler>();
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
		public static bool IsInstanceSet { get { return s_instance != null; } }

		#endregion

		[System.Serializable]
		public class QualityPreset
		{
			public string QualitySettingName = "";
			public bool SimpleTerrainShading = false;
		}

		[SerializeField]
		private QualityPreset[] m_qualityPresets = new QualityPreset[3]
		{
			new QualityPreset() { QualitySettingName = "Low",		SimpleTerrainShading = true },
			new QualityPreset() { QualitySettingName = "Medium",	SimpleTerrainShading = true },
			new QualityPreset() { QualitySettingName = "High",		SimpleTerrainShading = false }
		};
		public QualityPreset[] QualityPresets
		{
			get { return m_qualityPresets; }
			set { m_qualityPresets = value; }
		}

		private int m_originalScreenWidth;
		private int m_originalScreenHeight;

		public void ApplyQualitySetting(int p_qualitySettingIndex)
		{
			if (QualityPresets.Length > 0)
			{
				int index = Mathf.Clamp(p_qualitySettingIndex, 0, QualityPresets.Length - 1);
				if (QualityPresets[index] != null)
				{
					ApplyQualityPreset(QualityPresets[index]);
				}
			}
		}

		public void ApplyQualityPreset(QualityPreset p_qualitySetting)
		{
			if (p_qualitySetting != null)
			{
				ApplyQualitySettings(p_qualitySetting.QualitySettingName);
				ApplyTerrainSettings(p_qualitySetting.SimpleTerrainShading);
			}
		}

        private void Awake()
        {
			m_originalScreenWidth = Screen.width;
			m_originalScreenHeight = Screen.height;
		}

		private void ApplyQualitySettings(string p_qualitySettingName)
        {
			int qualitySettingIndex = Array.IndexOf(QualitySettings.names, p_qualitySettingName);
			if (qualitySettingIndex >= 0 && QualitySettings.GetQualityLevel() != qualitySettingIndex)
            {
				QualitySettings.SetQualityLevel(qualitySettingIndex, true);
			}
		}

		private void ApplyTerrainSettings(bool p_simpleTerrainShading)
		{
			if (p_simpleTerrainShading)
			{
				TerrainData terrainDataCopy = Instantiate(Terrain.activeTerrain.terrainData);
				terrainDataCopy.terrainLayers = new TerrainLayer[0];
				Terrain.activeTerrain.terrainData = terrainDataCopy;
				Material simpleLitTerrainMeterial = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
				simpleLitTerrainMeterial.renderQueue = (int)RenderQueue.Geometry - 100;
				Terrain.activeTerrain.materialTemplate = simpleLitTerrainMeterial;
				Terrain.activeTerrain.Flush();
			}
		}
	}
}
