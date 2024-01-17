using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Snowboard
{
	public static class PlayerReplay
	{
		public const byte SERIALIZATION_VERSION = 1;
		public const byte SERIALIZATION_DATA_ENTRY_ANIMATION_CODE = 11;
		
		// ####################################################################################
		// SUBCLASSES FOR RECORDING
		// ####################################################################################
		public abstract class DataEntryBase
		{
			public float Timestamp { get; private set; }
			public DataEntryBase(float pTimestamp)
			{
				Timestamp = pTimestamp;
			}
			public DataEntryBase(BinaryReader pReader)
			{
				Timestamp = pReader.ReadSingle();
			}
			public virtual void SaveToBinaryWriter(BinaryWriter pWriter)
			{
				pWriter.Write(Timestamp);
			}
		}
		public abstract class DataEntryApplyBase : DataEntryBase
		{
			public DataEntryApplyBase(float pTimestamp) : base(pTimestamp) {}
			public DataEntryApplyBase(BinaryReader pReader) : base(pReader) {}
			public abstract void Apply(GameObject pModel);
		}
		public class DataEntryModel : DataEntryBase
		{
			public const float SMOOTH_TIME = 0.225f;

			public Vector3 Pos { get; private set; }
			public Vector3 EulerRot { get; private set; }
			public float LookLeftRightAnimTime { get; private set; }
			public float FlipAnimTime { get; private set; }
			public DataEntryModel(float pTimestamp, Vector3 pPos, Vector3 pEulerRot, float pLookLeftRightAnimTime, float pFlipAnimTime)
				: base(pTimestamp)
			{
				Pos = pPos;
				EulerRot = pEulerRot;
				LookLeftRightAnimTime = pLookLeftRightAnimTime;
				FlipAnimTime = pFlipAnimTime;
			}
			public DataEntryModel(BinaryReader pReader, int pVersion)
				: base(pReader)
			{
				Pos = new Vector3(pReader.ReadSingle(), pReader.ReadSingle(), pReader.ReadSingle());
				if (pVersion == 1)
				{
					EulerRot = new Vector3(pReader.ReadSingle(), pReader.ReadSingle(), pReader.ReadSingle());
					LookLeftRightAnimTime = pReader.ReadSingle();
					FlipAnimTime = pReader.ReadSingle();
				}
				else
				{
					EulerRot = new Vector3(
						(((float)pReader.ReadByte()) / 255f) * 360f,
						(((float)pReader.ReadByte()) / 255f) * 360f,
						(((float)pReader.ReadByte()) / 255f) * 360f);
					LookLeftRightAnimTime = ((float)pReader.ReadByte()) / 255f;
					FlipAnimTime = ((float)pReader.ReadByte()) / 255f;
				}
			}
			public override void SaveToBinaryWriter(BinaryWriter pWriter)
			{
				base.SaveToBinaryWriter(pWriter);
				pWriter.Write(Pos.x);
				pWriter.Write(Pos.y);
				pWriter.Write(Pos.z);
				Vector3 euler = EulerRot;
				if (euler.x < 0) { euler.x += 360f; }
				pWriter.Write((byte)(255f*Mathf.Clamp01(euler.x / 360f)));
				if (euler.y < 0) { euler.y += 360f; }
				pWriter.Write((byte)(255f*Mathf.Clamp01(euler.y / 360f)));
				if (euler.z < 0) { euler.z += 360f; }
				pWriter.Write((byte)(255f*Mathf.Clamp01(euler.z / 360f)));
				pWriter.Write((byte)(Mathf.Clamp(255f*LookLeftRightAnimTime, 0f, 255f)));
				pWriter.Write((byte)(Mathf.Clamp(255f*FlipAnimTime, 0f, 255f)));
			}
			public static void Lerp(GameObject pModel, DataEntryModel pFrameA, DataEntryModel pFrameB, float pLerpFactor, ref Vector3 pVelocity)
			{
				Apply(pModel,
					Vector3.Lerp(pFrameA.Pos, pFrameB.Pos, pLerpFactor),
					Quaternion.Slerp(Quaternion.Euler(pFrameA.EulerRot), Quaternion.Euler(pFrameB.EulerRot), pLerpFactor),
					pFrameA.LookLeftRightAnimTime * (1-pLerpFactor) + pFrameB.LookLeftRightAnimTime * pLerpFactor,
					pFrameA.FlipAnimTime * (1-pLerpFactor) + pFrameB.FlipAnimTime * pLerpFactor,
					ref pVelocity);
			}
			private static void Apply(GameObject pModel, Vector3 pPos, Quaternion pRot, float pLookLeftRightAnimTime, float pFlipAnimTime, ref Vector3 pVelocity)
			{
				pModel.transform.position = Vector3.SmoothDamp(pModel.transform.position, pPos, ref pVelocity, SMOOTH_TIME);
				pModel.transform.rotation = pRot;
				Animator anim = pModel.GetComponent<Animator>();
				if (anim != null)
				{
					anim.Play("Look Layer.look", 1, pLookLeftRightAnimTime);
					anim.Play("Flip Layer.flip", 2, pFlipAnimTime);
				}
				else
				{
					Debug.LogError("Animator component not found!");
				}
			}
		}
		public class DataEntryAnimation : DataEntryApplyBase
		{
			private readonly int m_version;
			private int m_animPose;
			private string ActivatedClipName { get; set; }
			private float FadeDuration { get; set; }
			public DataEntryAnimation(float pTimestamp, EAnim pAnim)
				: base(pTimestamp)
			{
				m_version = SERIALIZATION_VERSION;
				m_animPose = (int)pAnim;
			}
			public DataEntryAnimation(BinaryReader pReader, List<string> pAnimClips)
				: base(pReader)
			{
				m_version = 2;
				ActivatedClipName = pAnimClips[pReader.ReadByte()];
				FadeDuration = pReader.ReadSingle();
			}
			public DataEntryAnimation(BinaryReader pReader, int pVersion)
				: base(pReader)
			{
				m_version = pVersion;
				if (pVersion == 1)
				{
					ActivatedClipName = pReader.ReadString();
					FadeDuration = pReader.ReadSingle();
				}
				else
				{
					m_animPose = pReader.ReadByte();
				}
			}
			public override void SaveToBinaryWriter(BinaryWriter pWriter)
			{
				base.SaveToBinaryWriter(pWriter);
				pWriter.Write((byte)m_animPose);
			}
			public override void Apply(GameObject pModel)
			{
				Animator anim = pModel.GetComponent<Animator>();
				if (anim != null)
				{
					if (m_version == 3)
					{
						anim.SetInteger("pose", m_animPose);
					}
					else
					{
						for (int i = 1; i < (int)EAnim.MAX; i++)
						{
							if (ActivatedClipName == ((EAnim)i).ToString())
							{
								anim.SetInteger("pose", i);
								return;
							}
						}
						Debug.LogError("DataEntryAnimation: Apply: could not find animation '" + ActivatedClipName + "'!");
					}
				}
				else
				{
					Debug.LogError("Animator component not found!");
				}
			}
		}
		
		// ####################################################################################
		// Replay implementation
		// ####################################################################################
		
		private const float MODEL_RECORD_PERIOD = 0.25f;

		private static string sGhostName = "";
		private static int sGhostScore = 0;
		private static float sTimeScaleBK = -1;
		private static float sStartTime = 0;
		private static float sLastModelRecordTime = -1;
		private static List<DataEntryModel> sRecordedModelData = new List<DataEntryModel>();
		private static List<DataEntryAnimation> sRecordedAnimData = new List<DataEntryAnimation>();
		private static EAnim sCurrActiveAnim = EAnim.ride;
		private static PlayerReplayWorker sPlayingWorker = null;

		public static string GetGhostName { get { return sGhostName; } }
		public static int GetGhostScore { get { return sGhostScore; } }

		public static bool IsPlaying { get { return sPlayingWorker != null; } }
		
		public static bool IsPaused { get { return sPlayingWorker != null && sPlayingWorker.IsPaused; } }
		
		/// <summary>
		/// Free memory and save start time.
		/// Call in the first level frame to start record.
		/// Call in the last frame to free recorded data.
		/// </summary>
		public static void Reset()
		{
			sGhostName = "";
			sGhostScore = 0;
			sTimeScaleBK = -1;
			sStartTime = Time.time;
			sLastModelRecordTime = -1;
			sRecordedModelData.Clear();
			sRecordedAnimData.Clear();
			sCurrActiveAnim = EAnim.ride;
			if (sPlayingWorker != null)
			{
				GameObject.Destroy(sPlayingWorker.gameObject);
			}
		}
		
		public static void Continue()
		{
			if (sPlayingWorker != null)
			{
				sPlayingWorker.IsPaused = false;
			}
		}
		
		public static void Pause()
		{
			if (sPlayingWorker != null)
			{
				sPlayingWorker.IsPaused = true;
			}
		}
		
		public static void Stop()
		{
			if (sPlayingWorker != null)
			{
				sPlayingWorker.IsStopped = true;
			}
		}
		
		public static void Record(Vector3 pModelPos, Vector3 pModelEulerRot, float pLookLeftRightAnimTime, float pFlipAnimTime)
		{
			if (sPlayingWorker==null)
			{
				sLastModelRecordTime = Time.time;
				sRecordedModelData.Add(new DataEntryModel(Time.time-sStartTime, pModelPos, pModelEulerRot, pLookLeftRightAnimTime, pFlipAnimTime));
			}
		}
		
		public static void Record(EAnim pAnim)
		{
			if (sPlayingWorker==null && sCurrActiveAnim != pAnim)
			{
				sCurrActiveAnim = pAnim;
				sRecordedAnimData.Add(new DataEntryAnimation(Time.time-sStartTime, pAnim));
			}
		}
		
		public static bool IsRecordForModelNeeded { get { return Time.time - sLastModelRecordTime >= MODEL_RECORD_PERIOD; } }
		
		public static float RecordDuration
		{
			get
			{
				if (sRecordedModelData.Count > 0)
				{
					return sRecordedModelData[sRecordedModelData.Count-1].Timestamp;
				}
				else
				{
					return 0;
				}
			}
		}
		
		public static void Replay(GameObject pModel)
		{
			ReplayInit();
			// start playback
			ReplayInitPlayingWorker(pModel);
			sPlayingWorker.Init(sRecordedModelData, sRecordedAnimData);
			sPlayingWorker.Play(pModel);
		}
		
		public static void ReplayGhost(string pGhostData, string pGhostName, int pGhostScore)
		{
			sGhostName = pGhostName;
			sGhostScore = pGhostScore;
			ReplayInit();
			// prepare playback
			ReplayInitPlayingWorker(null);
			sPlayingWorker.Init(pGhostData);
			// get model
			string modelPath = GameCharacterHandler.Instance.GetPlayerModelResourcePath();
			GameObject ghostModel = InstantiatePlayerModel(modelPath, 0);
			// add sphere collider and rigidbody
			ghostModel.AddComponent<SphereCollider>();
			ghostModel.AddComponent<Rigidbody>().isKinematic = true;
			ghostModel.transform.parent = sPlayingWorker.transform;
			// make camera look at playback player
			CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
			cam.target = PlayerAnimationHelper.GetHipsBone(ghostModel);
			cam.cameraState = CameraFollow.State.ReplayCinematic;
			// play
			sPlayingWorker.Play(ghostModel);
		}
		
		public static string SaveToString()
		{
			// compress to reduce size
			Compress();
			// init streams
			MemoryStream stream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(stream);
			// write serialization version
			writer.Write(SERIALIZATION_VERSION);
			// save sRecordedModelData
			writer.Write(sRecordedModelData.Count);
			for (int i = 0; i < sRecordedModelData.Count; i++)
			{
				sRecordedModelData[i].SaveToBinaryWriter(writer);
			}
			// save sRecordedApplyData
			writer.Write(sRecordedAnimData.Count);
			for (int i = 0; i < sRecordedAnimData.Count; i++)
			{
				sRecordedAnimData[i].SaveToBinaryWriter(writer);
			}
			// save model id
			writer.Write(GameCharacterHandler.Instance.GetPlayerModelResourcePath());
			writer.Flush();
			// compress
			string result = System.Text.Encoding.ASCII.GetString(stream.ToArray());
	#if UNITY_METRO && !UNITY_EDITOR
			writer.Dispose();
	#else
			writer.Close();
	#endif
			return result;
		}

		public static GameObject InstantiatePlayerModel(string pModelResourcePath, int pMaterialQuality)
		{
			// instantiate player
			GameObject playerModel = (GameObject)Object.Instantiate(Resources.Load(pModelResourcePath));
			// move to ignore raycast layer
			int ignoreRayLayer = PlayerConfig.Instance.IGNORE_RAYCAST_LAYER;
			Transform[] transforms = playerModel.GetComponentsInChildren<Transform>();
			foreach (Transform t in transforms)
			{
				t.gameObject.layer = ignoreRayLayer;
			}
			// disable all colliders if ghost
			if (pMaterialQuality<0)
			{
				Collider[] colliders = playerModel.GetComponentsInChildren<Collider>();
				foreach (Collider c in colliders)
				{
					c.enabled = false;
					c.isTrigger = true;
				}
			}
			return playerModel;
		}

		private static void ReplayInit()
		{
			sTimeScaleBK = -1;
			if (sPlayingWorker != null)
			{
				GameObject.Destroy(sPlayingWorker.gameObject);
			}
		}
		
		private static void ReplayInitPlayingWorker(GameObject pModel)
		{
			sPlayingWorker = (new GameObject("PlayerReplay")).AddComponent<PlayerReplayWorker>();
			if (pModel != null)
			{
				pModel.transform.parent = sPlayingWorker.transform;
			}
			sPlayingWorker.OnStart += OnReplayStart;
			sPlayingWorker.OnFinish += OnReplayFinish;
		}
		
		private static void OnReplayStart()
		{
			// handle time scale
			sTimeScaleBK = Time.timeScale;
			Time.timeScale = 1f;
		}
		
		private static void OnReplayFinish()
		{
			// handle time scale
			if (sTimeScaleBK != -1)
			{
				Time.timeScale = sTimeScaleBK;
				sTimeScaleBK = -1;
			}
		}

		private static void Compress()
		{
			if (sRecordedModelData.Count > 3)
			{
				DataEntryModel lastUsedData = sRecordedModelData[sRecordedModelData.Count-1];
				const float compressionMaxDifference = 0.5f;
				int applyDataIndex = sRecordedAnimData.Count-1;
				//int oldCount = sRecordedModelData.Count; // for debug see below
				for (int i = sRecordedModelData.Count-2; i > 1; i--)
				{
					while (applyDataIndex > 0 && sRecordedAnimData[applyDataIndex].Timestamp - 0.4 > sRecordedModelData[i].Timestamp)
					{
						applyDataIndex--;
					}

					float timeDiff = sRecordedAnimData[applyDataIndex].Timestamp - sRecordedModelData[i].Timestamp;
					bool isCloseToAnim = timeDiff > 0 && timeDiff < 0.41f;
					if (isCloseToAnim)
					{
						if (applyDataIndex > 0)
						{
							applyDataIndex--;
						}
					}

					if (!isCloseToAnim && GetCompressDifference(sRecordedModelData[i-1], sRecordedModelData[i], lastUsedData) < compressionMaxDifference)
					{
						sRecordedModelData.RemoveAt(i);
					}
					else
					{
						lastUsedData = sRecordedModelData[i];
					}
				}
				//Debug.Log("PlayerReplay: Compress: removed '" + (oldCount - sRecordedModelData.Count) + "/" + oldCount + "(" + ((float)(oldCount - sRecordedModelData.Count) / (float)oldCount * 100f) + "%)' elements with compression max difference '" + compressionMaxDifference + "'");
			}
		}

		private static float GetCompressDifference(DataEntryModel p_start, DataEntryModel p_candidate, DataEntryModel p_end)
		{
			float difference = 0f;
			// calculate the time difference of the candidate
			// 0 -> candidate is close to start time, 0.5 -> candidate is in the middle, 1 -> candidate is close to end time
			float timeFactor = (p_candidate.Timestamp - p_start.Timestamp) / (p_end.Timestamp - p_start.Timestamp);
			// calculate the expected values for the time of the candidate based on start and end
			Vector3 expectedPos = Vector3.Lerp(p_start.Pos, p_end.Pos, timeFactor);
			Vector3 expectedEulerRot = Quaternion.Slerp(Quaternion.Euler(p_start.EulerRot), Quaternion.Euler(p_end.EulerRot), timeFactor).eulerAngles;
			// compare the expected and the real values -> calculate difference
			difference += (expectedPos-p_candidate.Pos).magnitude;
			difference += (expectedEulerRot-p_candidate.EulerRot).magnitude * 0.085f;
			return difference;
		}
	}
}
