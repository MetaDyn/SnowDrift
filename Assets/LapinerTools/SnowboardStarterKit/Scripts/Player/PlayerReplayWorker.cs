using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Snowboard
{
	public class PlayerReplayWorker : MonoBehaviour
	{
		public Action OnStart { get; set; }
		public Action OnFinish { get; set; }
		
		public int ModelID { get; private set; }
		public bool IsPaused { get; set; }
		public bool IsStopped { get; set; }
		
		public bool DontDestroyAfterFinish { get; set; }
		
		private GameObject mModel;
		public GameObject Model { get{ return mModel; } }

		private List<PlayerReplay.DataEntryModel> mRecordedModelData;
		private List<PlayerReplay.DataEntryAnimation> mRecordedAnimation;
		private float mReplayStartTime;
		private float mTimeOffsetInSeconds = 0f;
		
		public void Init (string pSavedReplay)
		{
			LoadFromString(pSavedReplay);
		}
		
		public void Init (List<PlayerReplay.DataEntryModel> pRecordedModelData, List<PlayerReplay.DataEntryAnimation> pRecordedApplyData)
		{
			mRecordedModelData = pRecordedModelData;
			mRecordedAnimation = pRecordedApplyData;
		}

		public void Offset (float p_seconds)
		{
			mTimeOffsetInSeconds = p_seconds;
		}
		
		public void Play (GameObject pModel)
		{
			mModel = pModel;
			StartCoroutine(ReplayCoroutine());
		}
		
		public void SetTime (float pTime)
		{
			mReplayStartTime = Time.time - pTime - mTimeOffsetInSeconds;
		}
		
		private void OnDestroy()
		{
			if (OnFinish != null)
			{
				OnFinish();
			}
			OnFinish = null;
		}
		
		private IEnumerator ReplayCoroutine()
		{
			if (OnStart != null)
			{
				OnStart();
			}
			OnStart = null;
			if (mRecordedModelData == null || mRecordedAnimation == null)
			{
				// stop if broken
				GameObject.Destroy(gameObject);
				if (OnFinish != null)
				{
					OnFinish();
				}
				OnFinish = null;
			}
			else
			{
				// init variables
				Vector3 velocity = Vector3.zero;
				int indexModelData = 0;
				int indexApplyData = 0;
				mReplayStartTime = Time.time - mTimeOffsetInSeconds;

				if (mRecordedModelData.Count > 0)
				{
					mModel.transform.position = mRecordedModelData[0].Pos;
					mModel.transform.rotation = Quaternion.Euler(mRecordedModelData[0].EulerRot);

					if (mRecordedModelData.Count > 1)
					{
						velocity = (mRecordedModelData[1].Pos - mRecordedModelData[0].Pos) / (mRecordedModelData[1].Timestamp - mRecordedModelData[0].Timestamp);
					}
				}

				WaitForSeconds wait = new WaitForSeconds(0.00001f);
				while (!IsStopped && mModel != null && (indexModelData + 1 < mRecordedModelData.Count || indexApplyData < mRecordedAnimation.Count) )
				{
					// skip frames if paused
					if (IsPaused)
					{
						mReplayStartTime += Time.deltaTime;
						yield return 0;
						continue;
					}
					
					float currTime = Time.time-mReplayStartTime;
					// apply-able data e.g. animation
					if (indexApplyData < mRecordedAnimation.Count)
					{
						PlayerReplay.DataEntryApplyBase applyData = mRecordedAnimation[indexApplyData];
						float delay = applyData.Timestamp - currTime;
						if (delay <= 0 && mModel != null)
						{
							applyData.Apply(mModel);
							indexApplyData++;
						}
					}
					
					// interpolate (lerp) model data
					currTime += PlayerReplay.DataEntryModel.SMOOTH_TIME;
					if (indexModelData+1 < mRecordedModelData.Count)
					{
						PlayerReplay.DataEntryModel keyA = mRecordedModelData[indexModelData];
						PlayerReplay.DataEntryModel keyB = mRecordedModelData[indexModelData+1];
						if (keyA.Timestamp == keyB.Timestamp)
						{
							Debug.LogError("Zero devision!");
						}
						else if (keyB.Timestamp > currTime)
						{
							float lerpFactor = (currTime - keyA.Timestamp) / (keyB.Timestamp - keyA.Timestamp);
							PlayerReplay.DataEntryModel.Lerp(mModel, keyA, keyB, lerpFactor, ref velocity);
						}
						else if (indexModelData+2 < mRecordedModelData.Count)
						{
							indexModelData++;
							keyA = mRecordedModelData[indexModelData];
							keyB = mRecordedModelData[indexModelData+1];
							if (keyB.Timestamp > currTime)
							{
								float lerpFactor = (currTime - keyA.Timestamp) / (keyB.Timestamp - keyA.Timestamp);
								PlayerReplay.DataEntryModel.Lerp(mModel, keyA, keyB, lerpFactor, ref velocity);
							}
							else
							{
								indexModelData++;
							}
						}
						else
						{
							indexModelData++;
							PlayerReplay.DataEntryModel.Lerp(mModel, keyA, keyB, 1.0f, ref velocity);
						}
					}
					
					// wait for next frame
					yield return wait;
				}

				if (!DontDestroyAfterFinish)
				{
					GameObject.Destroy(gameObject);
				}
				
				if (OnFinish != null)
				{
					OnFinish();
				}
				OnFinish = null;
			}
		}
		
		private void LoadFromString (string pReplayString)
		{
			Stream stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(pReplayString));
			BinaryReader reader = new BinaryReader(stream);
			
			// first read serialization version and check compatibility
			byte version = reader.ReadByte();
			if (version == 1)
			{
				LoadFromReaderVersion1(reader);
			}
			else
			{
				Debug.LogError("Error: This replay/ghost comes from a newer game version, please update your game!");
			}
#if UNITY_METRO && !UNITY_EDITOR
			reader.Dispose();
#else
			reader.Close();
#endif
		}

		private void LoadFromReaderVersion1 (BinaryReader p_reader)
		{
			// read mRecordedModelData
			mRecordedModelData = new List<PlayerReplay.DataEntryModel>();
			int modelDataCount = p_reader.ReadInt32();
			for (int i = 0; i < modelDataCount; i++)
			{
				mRecordedModelData.Add(new PlayerReplay.DataEntryModel(p_reader, 2));
			}
			// read mRecordedApplyData
			mRecordedAnimation = new List<PlayerReplay.DataEntryAnimation>();
			int applyDataCount = p_reader.ReadInt32();
			for (int i = 0; i < applyDataCount; i++)
			{
				mRecordedAnimation.Add(new PlayerReplay.DataEntryAnimation(p_reader, 3));
			}
			// read model string
			ModelID = p_reader.ReadInt32();
		}
	}
}
