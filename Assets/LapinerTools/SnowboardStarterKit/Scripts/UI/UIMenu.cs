using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

namespace Snowboard
{
	public class UIMenu : MonoBehaviour
	{
		[SerializeField]
		private Image m_levelPreviewImage;
		public Image LevelPreviewImage
		{
			get { return m_levelPreviewImage; }
			set { m_levelPreviewImage = value; }
		}

		[SerializeField]
		private Transform m_characterPreviewRootTransform;
		public Transform CharacterPreviewRootTransform
		{
			get { return m_characterPreviewRootTransform; }
			set { m_characterPreviewRootTransform = value; }
		}

		[SerializeField]
		private Transform m_PCControlInfoTransform;
		public Transform PCControlInfoTransform
		{
			get { return m_PCControlInfoTransform; }
			set { m_PCControlInfoTransform = value; }
		}

		[SerializeField]
		private Transform m_mobileControlSettingsTransform;
		public Transform MobileControlSettingsTransform
		{
			get { return m_mobileControlSettingsTransform; }
			set { m_mobileControlSettingsTransform = value; }
		}

		[SerializeField]
		private Image m_mobileControlButtonsSimpleLeftImage;
		public Image MobileControlButtonsSimpleLeftImage
		{
			get { return m_mobileControlButtonsSimpleLeftImage; }
			set { m_mobileControlButtonsSimpleLeftImage = value; }
		}

		[SerializeField]
		private Image m_mobileControlButtonsSimpleRightImage;
		public Image MobileControlButtonsSimpleRightImage
		{
			get { return m_mobileControlButtonsSimpleRightImage; }
			set { m_mobileControlButtonsSimpleRightImage = value; }
		}

		[SerializeField]
		private Image m_mobileControlButtonsLeftImage;
		public Image MobileControlButtonsLeftImage
		{
			get { return m_mobileControlButtonsLeftImage; }
			set { m_mobileControlButtonsLeftImage = value; }
		}

		[SerializeField]
		private Image m_mobileControlButtonsRightImage;
		public Image MobileControlButtonsRightImage
		{
			get { return m_mobileControlButtonsRightImage; }
			set { m_mobileControlButtonsRightImage = value; }
		}

		[SerializeField]
		private Image m_mobileControlTiltImage;
		public Image MobileControlTiltImage
		{
			get { return m_mobileControlTiltImage; }
			set { m_mobileControlTiltImage = value; }
		}

		[SerializeField]
		private Image m_mobileControlJoystickLeftImage;
		public Image MobileControlJoystickLeftImage
		{
			get { return m_mobileControlJoystickLeftImage; }
			set { m_mobileControlJoystickLeftImage = value; }
		}

		[SerializeField]
		private Image m_mobileControlJoystickRightImage;
		public Image MobileControlJoystickRightImage
		{
			get { return m_mobileControlJoystickRightImage; }
			set { m_mobileControlJoystickRightImage = value; }
		}

		public void OnPreviousCharacterButtonClick()
		{
			GameCharacterHandler.Instance.SelectPreviousPlayerModel();
			LoadSelectedCharacterPreviewModel();
		}

		public void OnNextCharacterButtonClick()
		{
			GameCharacterHandler.Instance.SelectNextPlayerModel();
			LoadSelectedCharacterPreviewModel();
		}

		public void OnPreviousLevelButtonClick()
		{
			GameLevelHandler.Instance.SelectPreviousLevel();
			LoadSelectedLevelPreviewTexture();
		}

		public void OnNextLevelButtonClick()
		{
			GameLevelHandler.Instance.SelectNextLevel();
			LoadSelectedLevelPreviewTexture();
		}

		public void OnGoButtonClick()
		{
			GameLevelHandler.Instance.LoadSelectedLevel();
		}

		public void OnQualitySettingButtonClick(int p_qualitySettingIndex)
		{
			GameQualityHandler.Instance.ApplyQualitySetting(p_qualitySettingIndex);
			GameDatabaseHandler.Instance.SetInt(GameDatabaseHandler.IntVars.QUALITY_SETTING_INDEX, p_qualitySettingIndex);
		}

		public void OnMobileControlSettingsButtonClick(string p_controlModeConfig)
		{
			string[] controlModeConfig = p_controlModeConfig.Split('|');
			if (controlModeConfig.Length == 3)
			{
				OnMobileControlSettingsButtonClick(controlModeConfig[0], controlModeConfig[1]=="right", controlModeConfig[2]=="simple");
			}
		}

		public void OnMobileControlSettingsButtonClick(string p_controlMode, bool p_isControlOnRightSide, bool p_isControlModeSimple)
		{
			GameDatabaseHandler.Instance.SetBool(GameDatabaseHandler.BoolVars.IS_CONTROL_MODE_BUTTONS, p_controlMode == "buttons");
			GameDatabaseHandler.Instance.SetBool(GameDatabaseHandler.BoolVars.IS_CONTROL_MODE_JOYSTICK, p_controlMode == "joystick");
			GameDatabaseHandler.Instance.SetBool(GameDatabaseHandler.BoolVars.IS_CONTROL_MODE_TILT, p_controlMode == "tilt");

			GameDatabaseHandler.Instance.SetBool(GameDatabaseHandler.BoolVars.IS_CONTROL_ON_RIGHT, p_isControlOnRightSide);
			GameDatabaseHandler.Instance.SetBool(GameDatabaseHandler.BoolVars.IS_BUTTON_CONTROL_SIMPLE, p_isControlModeSimple);

			HighlightSelectedControlMode();
		}
			
		private void Start()
		{
			if (m_characterPreviewRootTransform == null)
			{
				Debug.LogError("UIMenu: Start: m_characterPreviewRootTransform is not set. Please set it in the inspector!");
			}

			if (GameCharacterHandler.Instance.PlayerModelResourcePathes == null || GameCharacterHandler.Instance.PlayerModelResourcePathes.Length == 0)
			{
				Debug.LogError("UIMenu: Start: PlayerModelResourcePathes property is not set on the GameCharacterHandler resource under path " + GameCharacterHandler.RESOURCE_PATH + ". Please set it in the inspector!");
			}

			if (MobileControlSettingsTransform != null)
			{
#if UNITY_IOS || UNITY_ANDROID || UNITY_TIZEN
				MobileControlSettingsTransform.gameObject.SetActive(true);
				HighlightSelectedControlMode();
#else
				MobileControlSettingsTransform.gameObject.SetActive(false);
#endif
			}

			if (PCControlInfoTransform != null)
			{
#if UNITY_IOS || UNITY_ANDROID || UNITY_TIZEN
				PCControlInfoTransform.gameObject.SetActive(false);
#else
				PCControlInfoTransform.gameObject.SetActive(true);
#endif
			}

			GameLevelHandler.Instance.Reset();

			GameQualityHandler.Instance.ApplyQualitySetting(GameDatabaseHandler.Instance.GetInt(GameDatabaseHandler.IntVars.QUALITY_SETTING_INDEX));

			LoadSelectedLevelPreviewTexture();
			LoadSelectedCharacterPreviewModel();
		}

		private void Update()
		{
			if (m_characterPreviewRootTransform != null)
			{
				m_characterPreviewRootTransform.Rotate(0f, Time.deltaTime * 35f, 0f);
			}
		}

		private void LoadSelectedLevelPreviewTexture()
		{
			if (GameLevelHandler.Instance.GetLevel().LevelPreviewSprite != null)
			{
				m_levelPreviewImage.sprite = GameLevelHandler.Instance.GetLevel().LevelPreviewSprite;
			}
			else
			{
				Debug.LogError("UIMenu: LoadSelectedLevelPreviewTexture: could not set level preview sprite!");
			}
		}

		private void LoadSelectedCharacterPreviewModel()
		{
			// destroy old character model
			for (int i = 0; i < m_characterPreviewRootTransform.childCount; i++)
			{
				GameObject.Destroy(m_characterPreviewRootTransform.GetChild(i).gameObject);
			}

			// load new character model
			Transform playerModelTransform = null;
			string playerModelResourcePath = GameCharacterHandler.Instance.GetPlayerModelResourcePath();
			if (!string.IsNullOrEmpty(playerModelResourcePath) &&
				m_characterPreviewRootTransform != null &&
				(playerModelTransform = Instantiate<Transform>(Resources.Load<Transform>(playerModelResourcePath))) != null)
			{
				playerModelTransform.SetParent(m_characterPreviewRootTransform, false);
				playerModelTransform.GetComponent<Animator>().applyRootMotion = false;
				MeshRenderer[] playerModelRenderers = playerModelTransform.GetComponentsInChildren<MeshRenderer>();
				for (int i = 0; i < playerModelRenderers.Length; i++)
				{
					playerModelRenderers[i].shadowCastingMode = ShadowCastingMode.Off;
				}
				SkinnedMeshRenderer[] playerSkinnedModelRenderers = playerModelTransform.GetComponentsInChildren<SkinnedMeshRenderer>();
				for (int i = 0; i < playerSkinnedModelRenderers.Length; i++)
				{
					playerSkinnedModelRenderers[i].shadowCastingMode = ShadowCastingMode.Off;
				}
			}
			else
			{
				Debug.LogError("UIMenu: LoadSelectedLevelPreviewTexture: could not load character preview model on resource path '" + playerModelResourcePath + "'!");
			}
		}

		private void HighlightSelectedControlMode()
		{
			bool isControlModeButtons = GameDatabaseHandler.Instance.GetBool(GameDatabaseHandler.BoolVars.IS_CONTROL_MODE_BUTTONS);
			bool isControlModeJoystick = GameDatabaseHandler.Instance.GetBool(GameDatabaseHandler.BoolVars.IS_CONTROL_MODE_JOYSTICK);
			bool isControlModeTilt = GameDatabaseHandler.Instance.GetBool(GameDatabaseHandler.BoolVars.IS_CONTROL_MODE_TILT);
			bool isControlOnRightSide = GameDatabaseHandler.Instance.GetBool(GameDatabaseHandler.BoolVars.IS_CONTROL_ON_RIGHT);
			bool isControlModeSimple = GameDatabaseHandler.Instance.GetBool(GameDatabaseHandler.BoolVars.IS_BUTTON_CONTROL_SIMPLE);

			if (MobileControlButtonsSimpleLeftImage != null) { MobileControlButtonsSimpleLeftImage.color = isControlModeButtons && isControlModeSimple && !isControlOnRightSide ? Color.white : new Color(0.66666f, 0.66666f, 0.66666f, 1f); }
			if (MobileControlButtonsSimpleRightImage != null) { MobileControlButtonsSimpleRightImage.color = isControlModeButtons && isControlModeSimple && isControlOnRightSide ? Color.white : new Color(0.66666f, 0.66666f, 0.66666f, 1f); }
			if (MobileControlButtonsLeftImage != null) { MobileControlButtonsLeftImage.color = isControlModeButtons && !isControlModeSimple && !isControlOnRightSide ? Color.white : new Color(0.66666f, 0.66666f, 0.66666f, 1f); }
			if (MobileControlButtonsRightImage != null) { MobileControlButtonsRightImage.color = isControlModeButtons && !isControlModeSimple && isControlOnRightSide ? Color.white : new Color(0.66666f, 0.66666f, 0.66666f, 1f); }
			if (MobileControlTiltImage != null) { MobileControlTiltImage.color = isControlModeTilt ? Color.white : new Color(0.66666f, 0.66666f, 0.66666f, 1f); }
			if (MobileControlJoystickLeftImage != null) { MobileControlJoystickLeftImage.color = isControlModeJoystick && !isControlOnRightSide ? Color.white : new Color(0.66666f, 0.66666f, 0.66666f, 1f); }
			if (MobileControlJoystickRightImage != null) { MobileControlJoystickRightImage.color = isControlModeJoystick && isControlOnRightSide ? Color.white : new Color(0.66666f, 0.66666f, 0.66666f, 1f); }
		}
	}
}
