namespace Guru
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Firebase.Analytics;
	using Firebase.Crashlytics;
	
	//打点模块初始化和基础接口封装
	public partial class Analytics
	{
		private static EventSetting EventSettingAll => EventSetting.GetFullSetting();

		private static bool _isInitOnce;				//Analytics是否初始化完成
		public static bool EnableDebugAnalytics;	//允许Debug包上报打点

		private static bool IsDebug => PlatformUtil.IsDebug();
		private static bool IsFirebaseReady => FirebaseUtil.IsFirebaseInitialized;
		private static bool IsGuruAnalyticsReady => GuruAnalytics.IsReady;
		
		private static AdjustEventDriver _adjustEventDriver;
		private static FBEventDriver _fbEventDriver;
		private static FirebaseEventDriver _firebaseEventDriver;
		private static GuruEventDriver _guruEventDriver;
		private static MidWarePropertiesManager _propertiesManager;
		

		#region 初始化

		/// <summary>
		/// 初始化打点模块
		/// </summary>
		public static void Init()
		{
			if (_isInitOnce) return;
			_isInitOnce = true;
			_adjustEventDriver = new AdjustEventDriver();
			_fbEventDriver = new FBEventDriver();
			_firebaseEventDriver = new FirebaseEventDriver();
			_guruEventDriver = new GuruEventDriver();
			
			_propertiesManager = new MidWarePropertiesManager(_guruEventDriver, _firebaseEventDriver);
		}
		
		/// <summary>
		/// 外部拉起 Firebase 初始化完成回调
		/// </summary>
		public static void OnFirebaseInitCompleted()
		{
			Debug.Log($"{TAG} --- Analytics Init After FirebaseReady:{IsFirebaseReady}");
			
			// --- 初始化 Crashlytics ---
			CrashlyticsAgent.Init();
			FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
			FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));
			
			// SetUserProperty(PropertyFirstOpenTime, FirstOpenTime);
			_firebaseEventDriver.TriggerFlush();
		}
		
		/// <summary>
		/// 上报事件成功率
		/// </summary>
		private static void ReportEventSuccessRate()
		{
			var interval = (DateTime.Now - _lastReportRateDate).TotalSeconds;
			if (interval > _reportSuccessInterval)
			{
				GuruAnalytics.Instance.ReportEventSuccessRate();
				_lastReportRateDate = DateTime.Now;
			}
		}
		
		public static void OnFBInitComplete()
		{
			Debug.Log($"{TAG} --- FBEvent is Ready -> _fbEventDriver.TriggerFlush");
			_fbEventDriver.TriggerFlush();
		}

		public static void OnAdjustInitComplete()
		{
			Debug.Log($"{TAG} --- AdjustEvent is Ready -> _adjustEventDriver.TriggerFlush");
			_adjustEventDriver.TriggerFlush();
		}
		
		private static void OnGuruAnalyticsInitComplete()
		{
			// ShouldFlushGuruEvents();
			CoroutineHelper.Instance.StartDelayed(new WaitForSeconds(0.1f), ShouldFlushGuruEvents);
		}

		/// <summary>
		/// 是否可以发送自打点事件
		/// </summary>
		public static void ShouldFlushGuruEvents()
		{
			if (!_guruEventDriver.IsReady  // Driver 的 Ready 标志位没有打开
			    && IsGuruAnalyticsReady
			    && !string.IsNullOrEmpty(IPMConfig.IPM_UID) // UID 不为空
				) // 自打点库初始化完毕
			{
				Debug.Log($"{TAG} --- GuruEvents is Ready -> _guruEventDriver.TriggerFlush");
				_guruEventDriver.TriggerFlush();
			}
		}


		#endregion

		#region 屏幕(场景)名称
		
		/// <summary>
		/// 设置屏幕名称
		/// </summary>
		/// <param name="screenName"></param>
		/// <param name="className"></param>
		public static void SetCurrentScreen(string screenName, string className = "")
		{
			if (!_isInitOnce)
			{
				return;
			}

			Log.I(TAG,$"SetCurrentScreen -> screenName:{screenName}, className:{className}");
			// Guru自打点直接上报属性
			if (GuruAnalytics.IsReady)
			{
				GuruAnalytics.Instance.SetScreen(screenName);
			}
			
			// Firebase 需要模拟事件上报
			var scEvent = new ScreenViewEvent(screenName, className);
			TrackEvent(scEvent);
		}

		#endregion

		#region 用户属性上报

		/// <summary>
		/// Firebase上报用户ID
		/// </summary>
		/// <param name="uid">通过Auth认证地用户ID</param>
		public static void SetFirebaseUserId(string uid)
		{
			if (!IsFirebaseReady) return;
			Log.I(TAG,$"SetUserIDProperty -> userID:{uid}");
			FirebaseAnalytics.SetUserId(uid);			
			Crashlytics.SetUserId(uid);
		}


		/// <summary>
		/// 设置用户属性
		/// </summary>
		public static void SetUserProperty(string key, string value)
		{
			if (!_isInitOnce)
			{
				throw new Exception($"[{TAG}][SDK] Analytics did not initialized, Call <Analytics.{nameof(Init)}()> first!");
			}
			
			if (IsDebug && !EnableDebugAnalytics)
			{
				Debug.LogWarning($"[{TAG}][SDK] --- SetProperty {key}:{value} can not send int Debug mode. Set <InitConfig.EnableDebugAnalytics> with `true`");
				return;
			}
			
			try
			{
				// 填充相关的追踪事件
				_guruEventDriver.AddProperty(key, value);
				_firebaseEventDriver.AddProperty(key, value);
				ReportEventSuccessRate();
				Debug.Log($"{TAG} --- SetUserProperty -> propertyName:{key}, propertyValue:{value}");
			}
			catch (Exception ex)
			{
				if (FirebaseUtil.IsReady)
				{
					Crashlytics.LogException(ex);
				}
				else
				{
					Debug.Log($"Catch Error: {ex}");
				}
			}
		}
		

		#endregion

		#region 打点上报
		
		/// <summary>
		/// 打点上报事件
		/// </summary>
		/// <param name="trackingEvent"></param>
		/// <exception cref="Exception"></exception>
		public static void TrackEvent(ITrackingEvent trackingEvent)
		{
			if (!_isInitOnce)
			{
				if (IsDebug)
				{
					throw new GuruNotInitializedException();
				}
	
				Debug.LogError("Analytics not initialized");
				return;
			}
			
			if (IsDebug && !EnableDebugAnalytics)
			{
				Debug.LogWarning($"[{TAG}][SDK] --- LogEvent [{trackingEvent.EventName}] can not send int Debug mode. Set <InitConfig.EnableDebugAnalytics> with `true`");
				return;
			}
			var eventSetting = trackingEvent.Setting ?? EventSettingAll;

			var dataStr = "";
			if (trackingEvent.Data != null) dataStr = JsonParser.ToJson(trackingEvent.Data);
			Debug.Log($"{TAG} --- Analytics::TrackEvent: {trackingEvent.EventName} | priority: {trackingEvent.Priority} | data:{dataStr} | eventSetting: {eventSetting}");
			
			try
			{
				// 填充相关的追踪事件
				if (eventSetting.EnableGuruAnalytics)
				{
					_guruEventDriver.AddEvent(trackingEvent);
				}
				if (eventSetting.EnableFirebaseAnalytics )
				{
					_firebaseEventDriver.AddEvent(trackingEvent);
				}
				if (eventSetting.EnableAdjustAnalytics)
				{
					_adjustEventDriver.AddEvent(trackingEvent);
				}
				if (eventSetting.EnableFacebookAnalytics)
				{
					_fbEventDriver.AddEvent(trackingEvent);
				}
			}
			catch (Exception ex)
			{
				if (FirebaseUtil.IsReady)
				{
					Crashlytics.LogException(ex);
				}
				else
				{
					Debug.LogError($"Catch Error: {ex}");
				}
			}
		}
		

		#endregion
		
		#region 通用打点

		
		/// <summary>
		/// Crashlytics 上报 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="isException"></param>
		public static void LogCrashlytics(string msg, bool isException = true)
		{
			if (!_isInitOnce) return;
			if (isException)
			{
				LogCrashlytics(new Exception(msg));
			}
			else
			{
				CrashlyticsAgent.Log(msg);
			}
		}
		
		
		public static void LogCrashlytics(Exception ex)
		{
			if (!_isInitOnce) return;
			CrashlyticsAgent.LogException(ex);
		}
		
		#endregion


	}



}