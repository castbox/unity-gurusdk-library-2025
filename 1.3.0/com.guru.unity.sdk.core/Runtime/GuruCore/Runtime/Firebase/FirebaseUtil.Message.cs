


namespace Guru
{
	using System;
	using System.Collections;
	using Firebase.Messaging;
	using Firebase.Extensions;
	using UnityEngine;
	
	public static partial class FirebaseUtil
	{
		private const int _retryTokenDelay = 10;
		// public static bool? IsInitMessage;
		private static bool _isAutoFetchFcmToken = true;
		private static bool _isFetchOnce = false;
		// private static bool _isOnFetching = false;

		public static void SetAutoFetchFcmToken(bool value)
		{
			_isAutoFetchFcmToken = value;
		}

		private static void InitializeMessage()
		{
			// 初始化回调挂载
			if (_isAutoFetchFcmToken)
			{
				StartFetchFcmToken();
			}
			
#if UNITY_EDITOR
			// Editor 下直接返回模拟的 PUSH_TOKEN
			IPMConfig.FIREBASE_PUSH_TOKEN = $"editor-pushtoken-{new Guid().ToString()}";
#endif
			
			// UID 为空则不上报
			if (string.IsNullOrEmpty(IPMConfig.IPM_UID)) 
				return;
			
			// Token 不为空则立即上报
			GuruDeviceInfoUploader.Instance.Upload();
		}
		
		public static void StartFetchFcmToken()
		{
			if (_isFetchOnce) return;
			_isFetchOnce = true;

			// FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
			FirebaseMessaging.TokenReceived += OnTokenReceived;
			FirebaseMessaging.MessageReceived += OnMessageReceived;
			GetFCMTokenAsync();
		}

		/// <summary>
		/// 异步获取 FCM Token
		/// </summary>
		private static void GetFCMTokenAsync()
		{
			if (!NetworkUtil.IsNetAvailable)
			{
				// 无网络直接重新获取
				DelayGetFCMToken(_retryTokenDelay);
				return;
			}

			
			// 使用协程直接获取 PushToken
			IEnumerator OnFetchFCMToken()
			{
				Debug.Log($"[{LOG_TAG}][SDK]--- Start Get FCMToken ---");
				var task = FirebaseMessaging.GetTokenAsync();
				while (!task.IsCompleted)
				{
					yield return null;
				}

				if (task.IsCanceled || task.IsFaulted || task.Exception != null)
				{
					// task 获取失败
					Log.E(LOG_TAG,$"--- FCMToken get failed -> EX:{task.Exception?.Message ?? "NULL"}");
					CrashlyticsAgent.LogException(task.Exception);
					DelayGetFCMToken(_retryTokenDelay);
					yield break;
				}

				var token = task.Result;
				// 取到的值不为空
				if (!string.IsNullOrEmpty(token))
				{
					IPMConfig.FIREBASE_PUSH_TOKEN = token;
					GuruDeviceInfoUploader.Instance.Upload(() =>
					{
						Log.I(LOG_TAG, "--- FCMToken Upload Success!");
					});
					yield break;
				}
				
				DelayGetFCMToken(_retryTokenDelay);

				// 缓存值不为空
				if (!string.IsNullOrEmpty(IPMConfig.FIREBASE_PUSH_TOKEN))
				{
					GuruDeviceInfoUploader.Instance.Upload();
				}
			}
			
			CoroutineHelper.Instance.StartCoroutine(OnFetchFCMToken());
			
			/*
			FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
			{
				if (task.IsCanceled || task.IsFaulted)
				{
					// task 获取失败
					Log.E(LOG_TAG,$"--- FCMToken get failed -> EX:{task.Exception?.Message ?? "NULL"}");
					CrashlyticsAgent.LogException(task.Exception);
					DelayGetFCMToken(_retryTokenDelay);
					return;
				}

				var token = task.Result;

				// 取到的值不为空
				if (!string.IsNullOrEmpty(token))
				{
					IPMConfig.IPM_PUSH_TOKEN = token;
					GuruDeviceInfoUploader.Instance.Upload(() =>
					{
						Log.I(LOG_TAG, "--- FCMToken Upload Success!");
					});
					return;
				}

				DelayGetFCMToken(_retryTokenDelay);

				// 缓存值不为空
				if (!string.IsNullOrEmpty(IPMConfig.FIREBASE_PUSH_TOKEN))
				{
					GuruDeviceInfoUploader.Instance.Upload(() =>
					{
						Log.I(LOG_TAG, "--- FCMToken Upload Success!");
					});
				}
			});
			*/
		}

		private static void DelayGetFCMToken(int seconds = 2)
		{
			CoroutineHelper.Instance.StartDelayed(new WaitForSecondsRealtime(seconds), GetFCMTokenAsync);
		}
		
		private static void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
	        Log.I(LOG_TAG, "--- On FCMToken Received: " + token.Token);
#if UNITY_IOS
	        DeviceUtil.SetiOSBadge();
#endif
        }

		private static void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
	        Log.I(LOG_TAG,"--- OnMessageReceived: " + e.Message);
#if UNITY_IOS
	        DeviceUtil.SetiOSBadge();
#endif
        }
	}
}