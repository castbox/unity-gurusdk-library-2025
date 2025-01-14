namespace Guru
{
	using System;
	using System.Collections.Generic;
	using Facebook.Unity;
	using UnityEngine;
	
	public class FBService
	{
		
		private static FBService _instance;
		public static FBService Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new FBService();
				}
				return _instance;
			}
		}


		public const string LOG_TAG = "[FB]";
		private bool _isInitOnce;
		private Action _onInitComplete;
		
		public void StartService(Action onInitComplete = null)
		{
			if(_isInitOnce) return;
			_isInitOnce = true;

			_onInitComplete = onInitComplete;
			// Initialize the Facebook SDK
			FB.Init(InitCallback, OnHideUnity);
		}

		private void InitCallback()
		{

			// Signal an app activation App Event
			FB.ActivateApp();
			FB.Mobile.SetAdvertiserIDCollectionEnabled(true);
			FB.Mobile.SetAutoLogAppEventsEnabled(false); // 关闭自动打点上报
#if UNITY_IOS
			FB.Mobile.SetAdvertiserTrackingEnabled(true);
#endif
			_onInitComplete?.Invoke();
		}

		private void OnHideUnity(bool isGameShown)
		{
			if (!isGameShown)
			{
				// Pause the game - we will need to hide
				// Time.timeScale = 0;
			}
			else
			{
				// Resume the game - we're getting focus again
				// Time.timeScale = 1;
			}
		}

		/// <summary>
		/// 事件上报
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="valueToSum"></param>
		/// <param name="data"></param>
		public static void LogEvent(string eventName, float? valueToSum = null, Dictionary<string, object> data  = null)
		{
			if(!IsReady) return;
			Debug.Log($"{LOG_TAG} --- driver LogEvent: {eventName}");
			FB.LogAppEvent(eventName, valueToSum, data);
		}
		
		public static void LogSpendCredits(float valueToSum, string contentId, string contentType)
		{
			if(!IsReady) return;
			Debug.Log($"{LOG_TAG} --- driver LogSpendCredits: {valueToSum}");
			FB.LogAppEvent(AppEventName.SpentCredits, valueToSum, 
				new Dictionary<string, object>()
				{
					{ AppEventParameterName.ContentID, contentId },
					{ AppEventParameterName.ContentType, contentType }, 
				});
		}

		/// <summary>
		/// 支付上报
		/// </summary>
		/// <param name="valueToSum"></param>
		/// <param name="currency"></param>
		/// <param name="data"></param>
		public static void LogPurchase(float valueToSum, string currency = "USD",
			Dictionary<string, object> data = null)
		{
			if(!IsReady) return;
			Debug.Log($"{LOG_TAG} --- driver LogPurchase: {valueToSum}");
			FB.LogPurchase(valueToSum, currency, data);
		}
		
		private static bool IsReady
		{
			get
			{
				if (!FB.IsInitialized)
				{
					Debug.LogError($"{LOG_TAG} FB is not initialized, please call <FBService.StartService> first.");
					return false;
				}
				return true;
			}
		}


	}
}