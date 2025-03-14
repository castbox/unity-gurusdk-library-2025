
using Cysharp.Threading.Tasks;

namespace Guru
{
	using UnityEngine;
	using AdjustSdk;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	
	public enum DelayMinutesSource
	{
		Default = 0,
		Local,
		RemoteConfig
	}
	
	/// <summary>
	/// Adjust 服务组件
	/// Unity 插件下载地址：https://github.com/adjust/unity_sdk
	/// Adjust V4 to V5 升级事项， 诸多接口进行了调整： https://dev.adjust.com/en/sdk/migration/unity/v4-to-v5/
	/// </summary>
	public class AdjustService
	{
		public const string VERSION = "2.0.0";
		public const string LOG_TAG = "[Adjust]";
		public const string REMOTE_DELAY_TIME_KEY = "adjust_adrev_delay_minutes"; // DelayMinutes 运控值 默认 30分钟， 云控配置 1440 （24 小时）
		private const float START_DELAY_SECONDS = 5.0f; // 延迟启动时间(s)
		private const int DEFAULT_ATT_WAIT_SECONDS = 120; // 延迟等待 ATT 用户操作结果(s)
		
		public const float DEFAULT_DELAY_MINUTES = 30;
		private const string REVENUE_CURRENCY_USD = "USD";

		private const string AD_REVENUE_SOURCE_APPLOVIN_MAX = "applovin_max_sdk";
		private const string AD_REVENUE_SOURCE_IRONSOURCE = "ironsource_sdk";

		private const int MAX_FETCH_ADJUST_ID_RETRIES = 2; //拉取 adjust_id最大次数
		private const int NETWORK_CHECK_INTERVAL = 3000;  // 网络检查间隔
		private const int RETRY_INTERVAL = 3000;          // 重试间隔
		private const int BASE_TIMEOUT_SECONDS = 100;      // 基础超时时间
		
		
		// private const string K_IAP_PURCHASE = "iap_purchase"; // 固定点位事件
		// private const string K_SUB_PURCHASE = "sub_purchase"; // 固定点位事件
		
		private string _googleAdId = "";
		public string GoogleAdId // GPS = Google Play Service
		{
			get
			{
				if(string.IsNullOrEmpty(_googleAdId)) FetchGoogleAdIdAsync();
				return _googleAdId; // Google AdId
			}
		}
		
		private string _sdkVersion = "5.0.5";
		public string SdkVersion => _sdkVersion;
		private bool _isReady = false;
		public bool IsReady => _isReady;

		private string _adSourceName;

		private static AdjustService _instance;
		public static AdjustService Instance
		{
			get
			{
				if (_instance == null) _instance = new AdjustService();
				return _instance;
			}
		}

		// ---------------- AdRevenue 打点属性 -------------------- 
		/// <summary>
		/// 应用安装的日志
		/// </summary>
		private readonly DateTime _firstOpenTime;
		/// <summary>
		/// 收益相关数据
		/// </summary>
		private readonly AdjustModel _adjustModel;

		/// <summary>
		/// 暂缓打点策略开关
		/// </summary>
		private bool _deferredReportAdRevenueEnabled = false;
		/// <summary>
		/// 暂缓向Adjust上报adRevenue数据的时间区间，为一个浮点数，单位为分钟；
		/// 当 app_age 小于等于本时间区间时，视为用户处于暂缓上报时间窗口内，暂缓向Adjust上报该用户带来广告收入的数据。
		/// </summary>
		private float _delayMinutes;
		/// <summary>
		/// DelayMinutes 配置来源
		/// </summary>
		private DelayMinutesSource _delayMinutesSource;
		private Coroutine _delayCoroutine;
		

		#region 构造函数

		/// <summary>
		/// 构造函数
		/// </summary>
		private AdjustService()
		{
			_firstOpenTime = IPMConfig.GetFirstOpenDate();
			_adjustModel = AdjustModel.LoadOrCreate();

			Adjust.GetSdkVersion(v => _sdkVersion = v);
		}

		#endregion

		#region 启动服务

		/// <summary>
		/// Adjust启动服务
		/// </summary>
		/// <param name="appToken"></param>
		/// <param name="fbAppId">MIR 追踪 AppID</param>
		/// <param name="firebaseId"></param>
		/// <param name="deviceId"></param>
		/// <param name="delayStrategyEnabled"></param>
		/// <param name="iOSAttWaitingTime"></param>
		/// <param name="onInitComplete">初始化完成的时候会返回 AdjustId </param>
		/// <param name="onDeeplinkCallback"></param>
		/// <param name="onGetGoogleAdIdCallback"></param>
		/// <param name="showLogs"></param>
		public void Start(
			string appToken, 
			string fbAppId = "", 
			string firebaseId = "", 
			string deviceId = "",
			bool delayStrategyEnabled = false,
			int iOSAttWaitingTime = 0,
			Action<string> onInitComplete = null, 
			Action<string> onDeeplinkCallback = null, 
			Action<string> onGetGoogleAdIdCallback = null, 
			Action<string> onGetAdjustIdCallback = null, 
			bool showLogs  = false)
		{
			if (string.IsNullOrEmpty(appToken))
			{
				LogE(LOG_TAG, "Adjust没有设置token，无法进行初始化");
				return;
			}
			
			// 需要在 Adjust.start 前设置 <安装归因参数>
			if (!string.IsNullOrEmpty(firebaseId))
			{
				Adjust.AddGlobalCallbackParameter("user_pseudo_id", firebaseId);
			}

			if (!string.IsNullOrEmpty(deviceId))
			{
				Adjust.AddGlobalCallbackParameter("device_id", deviceId);
			}
			
			// 初始化启动 Config
			AdjustEnvironment environment = GetAdjustEnvironment();
			AdjustConfig config = new AdjustConfig(appToken, environment);
			config.LogLevel = GetLogLevel(showLogs);
			config.IsPreinstallTrackingEnabled = true; // Adjust Preinstall
			// 2025-3-13 【BI】新增用户属性打点 https://www.tapd.cn/33527076/prong/stories/view/1133527076001025406?from_iteration_id=1133527076001003267
			config.AttributionChangedDelegate = OnAttributionChanged;

			// * This setting has been removed in SDK v5.
			// config.SetDelayStart(START_DELAY_SECONDS);  // 延迟 1s 启动 Adjust，保证 <安装归因参数> 成功注入
			if (!string.IsNullOrEmpty(fbAppId))
			{
				// config.setFbAppId(fbAppId); // 注入 MIR ID
				config.FbAppId = fbAppId;
			}
			
#if UNITY_IOS
			if(iOSAttWaitingTime <= 0) 
				iOSAttWaitingTime = DEFAULT_ATT_WAIT_SECONDS;

			// config.setAttConsentWaitingInterval(iOSAttWaitingTime);
			// In SDK v5, you need to assign your delay interval to the AttConsentWaitingInterval property of your AdjustConfig instance.
			config.AttConsentWaitingInterval = iOSAttWaitingTime;
#endif	
			
			// Deeplink Callback
			if (onDeeplinkCallback != null)
			{
				// config.setDeferredDeeplinkDelegate(onDeeplinkCallback);
				config.DeferredDeeplinkDelegate = onDeeplinkCallback;
			}


			// SetupInstance(); // 初始化场景示例
			_deferredReportAdRevenueEnabled = delayStrategyEnabled; // 外部输入的延迟打点策略
			
			// 设置延迟上报的初始值
			SetAdRevDelayMinutes(DEFAULT_DELAY_MINUTES, DelayMinutesSource.Default);
			
			Adjust.InitSdk(config);  // 启动服务
			
			// 异步加载AdId
			FetchGoogleAdIdAsync(onGetGoogleAdIdCallback);
			LogI(LOG_TAG, $"--- Start AdjustService:{VERSION}    SDK:{SdkVersion}    deferred_report_ad_revenue:{_deferredReportAdRevenueEnabled}");
			
			DelayAction((int)(START_DELAY_SECONDS * 1000 + 100), () => {
				_isReady = true;
				onInitComplete?.Invoke(string.Empty);
			}).Forget();
			
			// 异步获取AdjustId
			FetchAdjustIdAsync(onGetAdjustIdCallback).Forget();
			
			// 异步获取Adjust归因
			// 2025-3-13 【BI】新增用户属性打点 https://www.tapd.cn/33527076/prong/stories/view/1133527076001025406?from_iteration_id=1133527076001003267
			CheckAndFetchAdjustAttribution().Forget();
		}
		
		/// <summary>
		/// 检查local归因数据，如果本地没有则异步从Adjust异步获取
		/// </summary>
		private async UniTaskVoid CheckAndFetchAdjustAttribution()
		{
			try
			{
				AdjustAttribution attribution = _adjustModel.Attribution;
				if (attribution == null)
				{
					LogW(LOG_TAG, $"first fetch adjust attribution");
					FetchAdjustAttribution().Forget();
				}
				else
				{
					ProcessAdjustAttributionChanged(attribution);
				}
			}
			catch (TimeoutException)
			{
				LogW(LOG_TAG, $"获取 AdjustAttribution 超时");
			}
		}

		/// <summary>
		/// 异步获取Adjust归因信息
		/// </summary>
		private async UniTaskVoid FetchAdjustAttribution()
		{
			try
			{  
				var tcs = new UniTaskCompletionSource<AdjustAttribution>();
				Adjust.GetAttribution(result =>
				{
					LogI(LOG_TAG, $"Get AdjustAttribution返回结果: {result}");
					tcs.TrySetResult(result);
				});
				AdjustAttribution attribution = await tcs.Task.Timeout(TimeSpan.FromSeconds(15));
				ProcessAdjustAttributionChanged(attribution); 
			}
			catch (Exception ex)
			{
				LogE(LOG_TAG, $"Failed to fetch adjust attribution: {ex.ToString()}");
				ProcessAdjustAttributionChanged(null);
			}
		}
		
		/// <summary>
		/// 归因更新
		/// </summary>
		/// <param name="attribution"></param>
		private void OnAttributionChanged(AdjustAttribution attribution)
		{
			LogI(LOG_TAG, $"--- OnAttributionChanged");
			ProcessAdjustAttributionChanged(attribution);
		}

		/// <summary>
		/// 处理归因
		/// </summary>
		/// <param name="attribution"></param>
		private void ProcessAdjustAttributionChanged(AdjustAttribution attribution = null)
		{
			string channel = attribution == null || string.IsNullOrEmpty(attribution.Network) ?"unknown" : attribution.Network;
			Analytics.SetUserProperty(Analytics.PropertyChannel, channel);
			
			string campaign = attribution == null || string.IsNullOrEmpty(attribution.Campaign) ? "unknown" : attribution.Campaign;
			Analytics.SetUserProperty(Analytics.PropertyCampaign, campaign);
			
			string creative = attribution == null || string.IsNullOrEmpty(attribution.Creative) ? "unknown" : attribution.Creative;
			Analytics.SetUserProperty(Analytics.PropertyCreative, creative);
			
			if (CheckAdjustAttributionChanged(attribution))
			{
				_adjustModel.Attribution = attribution;
				LogI(LOG_TAG, $"adjust attribution changed!");
			}
		}

		private bool CheckAdjustAttributionChanged(AdjustAttribution newValue)
		{
			if (newValue == null)
				return false;

			AdjustAttribution localAttribution = _adjustModel.Attribution;
			if (localAttribution == null)
				return true;
			
			// 如果两个引用相同,则没有变化
			if (ReferenceEquals(localAttribution, newValue))
				return false;
			
			return GuruSDKUtils.StringEquals(localAttribution.TrackerToken, newValue.TrackerToken) == false ||
			       GuruSDKUtils.StringEquals( localAttribution.TrackerName, newValue.TrackerName) == false ||
			       GuruSDKUtils.StringEquals(localAttribution.Network, newValue.Network) == false ||
			       GuruSDKUtils.StringEquals(localAttribution.Campaign, newValue.Campaign) == false ||
			       GuruSDKUtils.StringEquals(localAttribution.Adgroup, newValue.Adgroup) == false ||
			       GuruSDKUtils.StringEquals(localAttribution.Creative, newValue.Creative) == false ||
			       GuruSDKUtils.StringEquals(localAttribution.ClickLabel, newValue.ClickLabel) == false ||
			       GuruSDKUtils.StringEquals(localAttribution.CostType, newValue.CostType) == false ||
			       localAttribution.CostAmount.Equals(newValue.CostAmount) == false ||
			       GuruSDKUtils.StringEquals(localAttribution.CostCurrency, newValue.CostCurrency) == false ||
			       GuruSDKUtils.StringEquals(localAttribution.FbInstallReferrer, newValue.FbInstallReferrer) == false;
		}
		
		private async UniTaskVoid DelayAction(int delayMs, Action callback)
		{
			await UniTask.Delay(delayMs);
			callback?.Invoke();
		}

		private async UniTaskVoid FetchAdjustIdAsync(Action<string> onGetAdjustIdCallback = null)
		{
			int retryCount = 0;
			
			try
			{
				while (retryCount < MAX_FETCH_ADJUST_ID_RETRIES)
				{   
					// 检查网络连接
					while (Application.internetReachability == NetworkReachability.NotReachable)
					{
						LogW(LOG_TAG, "等待网络连接...");
						await UniTask.Delay(NETWORK_CHECK_INTERVAL);
					}
					
					retryCount++;
					int currentTimeout = BASE_TIMEOUT_SECONDS * retryCount;
					LogI(LOG_TAG, $"准备获取 Adjust ID，尝试次数：{retryCount}/{MAX_FETCH_ADJUST_ID_RETRIES}，超时时间：{currentTimeout}秒");
					
					try
					{
						string adid = await GetAdjustIdAsync().Timeout(TimeSpan.FromSeconds(currentTimeout));
						if (!string.IsNullOrEmpty(adid))
						{
							LogI(LOG_TAG, $"成功获取 Adjust ID: {adid}");
							onGetAdjustIdCallback?.Invoke(adid);
							return; // 成功获取后直接返回
						}
						LogW(LOG_TAG, "获取到的 Adjust ID 为空");
					}
					catch (TimeoutException)
					{
						LogW(LOG_TAG, $"第 {retryCount} 次获取 Adjust ID 超时");
					}

					// 检查是否是最后一次尝试
					if (retryCount >= MAX_FETCH_ADJUST_ID_RETRIES)
					{
						LogW(LOG_TAG, "已达到最大重试次数，获取 Adjust ID 失败");
						onGetAdjustIdCallback?.Invoke(string.Empty);
						return; // 达到最大重试次数后直接返回
					}
					await UniTask.Delay(RETRY_INTERVAL);
				}
			}
			catch (Exception ex)
			{
				LogE(LOG_TAG, $"获取 Adjust ID 时发生错误: {ex.Message}");
				onGetAdjustIdCallback?.Invoke(string.Empty);
			}
		}
		
		/// <summary>
		/// 异步获取adjustId
		/// </summary>
		/// <returns>Adjust ID，如果获取失败则返回空字符串</returns>
		private UniTask<string> GetAdjustIdAsync()
		{
			var tcs = new UniTaskCompletionSource<string>();
			Adjust.GetAdid(result =>
			{
				LogI(LOG_TAG, $"GetAdid返回结果: adid = {result}");
				tcs.TrySetResult(result);
			});
			return tcs.Task;
		}
		
		private float GetRemainDelaySeconds()
		{
			// 起始时间 + 延迟时间 - 当前时间
			float delayTime = (float)(_firstOpenTime.AddMinutes(_delayMinutes) - DateTime.UtcNow).TotalSeconds;
			if (delayTime < 0) delayTime = 0;
			return delayTime;
		}

		

		/// <summary>
		/// 异步拉取 Google Ad Id
		/// </summary>
		private void FetchGoogleAdIdAsync(Action<string> onGetGoogleAdIdCallback = null)
		{
			Adjust.GetGoogleAdId(gid =>
			{
				if (!string.IsNullOrEmpty(gid))
				{
					_googleAdId = gid; // 获取Google AD ID 
					onGetGoogleAdIdCallback?.Invoke(_googleAdId); // 返回 GoogleAdid
				}
			});
		}

		/// <summary>
		/// 确保 Adjust 实例在场景中
		/// </summary>
		private void SetupInstance()
		{
			var go = GameObject.Find(nameof(Adjust));
			if (go == null)
			{
				go = new GameObject(nameof(Adjust));
				var ins = go.AddComponent<Adjust>();
				ins.startManually = true;
				ins.launchDeferredDeeplink = true;
				ins.sendInBackground = true;
			}
		}
		
		#endregion
		
		#region 内部回调函数
		/*
		/// <summary>
		/// Session 启动后回调
		/// 回调中可以获取实际的 AdjustID
		/// </summary>
		/// <param name="sessionSuccessData"></param>
		private void OnSessionSuccessCallback(AdjustSessionSuccess sessionSuccessData)
		{
			var adid = sessionSuccessData.Adid;
			LogI(LOG_TAG,$"{LOG_TAG} --- Session tracked successfully! Get Adid: {adid}");
		}
		
		private void OnAttributionChangedCallback(AdjustAttribution attributionData)
		{
			LogI(LOG_TAG, "Attribution changed!");

			if (attributionData.trackerName != null)
			{
				LogI(LOG_TAG, "Tracker name: " + attributionData.trackerName);
			}

			if (attributionData.trackerToken != null)
			{
				LogI(LOG_TAG, "Tracker token: " + attributionData.trackerToken);
			}

			if (attributionData.network != null)
			{
				LogI(LOG_TAG, "Network: " + attributionData.network);
			}

			if (attributionData.campaign != null)
			{
				LogI(LOG_TAG, "Campaign: " + attributionData.campaign);
			}

			if (attributionData.adgroup != null)
			{
				LogI(LOG_TAG, "Adgroup: " + attributionData.adgroup);
			}

			if (attributionData.creative != null)
			{
				LogI(LOG_TAG, "Creative: " + attributionData.creative);
			}

			if (attributionData.clickLabel != null)
			{
				LogI(LOG_TAG , "Click label: " + attributionData.clickLabel);
			}

			if (attributionData.adid != null)
			{
				LogI(LOG_TAG, "ADID: " + attributionData.adid);
			}
		}

		private void OnEventSuccessCallback(AdjustEventSuccess eventSuccessData)
		{
			LogI(LOG_TAG, "Event tracked successfully!");

			if (eventSuccessData.Message != null)
			{
				LogI(LOG_TAG, "Message: " + eventSuccessData.Message);
			}

			if (eventSuccessData.Timestamp != null)
			{
				LogI(LOG_TAG, "Timestamp: " + eventSuccessData.Timestamp);
			}

			if (eventSuccessData.Adid != null)
			{
				LogI(LOG_TAG, "Adid: " + eventSuccessData.Adid);
			}

			if (eventSuccessData.EventToken != null)
			{
				LogI(LOG_TAG, "EventToken: " + eventSuccessData.EventToken);
			}

			if (eventSuccessData.CallbackId != null)
			{
				LogI(LOG_TAG, "CallbackId: " + eventSuccessData.CallbackId);
			}

			if (eventSuccessData.JsonResponse != null)
			{
				LogI(LOG_TAG, "JsonResponse: " + eventSuccessData.GetJsonResponse());
			}
		}

		private void OnEventFailureCallback(AdjustEventFailure eventFailureData)
		{
			LogI(LOG_TAG, "Event tracking failed!");

			if (eventFailureData.Message != null)
			{
				LogI(LOG_TAG, "Message: " + eventFailureData.Message);
			}

			if (eventFailureData.Timestamp != null)
			{
				LogI(LOG_TAG, "Timestamp: " + eventFailureData.Timestamp);
			}

			if (eventFailureData.Adid != null)
			{
				LogI(LOG_TAG, "Adid: " + eventFailureData.Adid);
			}

			if (eventFailureData.EventToken != null)
			{
				LogI(LOG_TAG, "EventToken: " + eventFailureData.EventToken);
			}

			if (eventFailureData.CallbackId != null)
			{
				LogI(LOG_TAG, "CallbackId: " + eventFailureData.CallbackId);
			}

			if (eventFailureData.JsonResponse != null)
			{
				LogI(LOG_TAG, "JsonResponse: " + eventFailureData.GetJsonResponse());
			}

			LogI(LOG_TAG, "WillRetry: " + eventFailureData.WillRetry.ToString());
		}

		private void OnSessionFailureCallback(AdjustSessionFailure sessionFailureData)
		{
			LogE(LOG_TAG,"Session tracking failed!");

			if (sessionFailureData.Message != null)
			{
				LogI(LOG_TAG,"Message: " + sessionFailureData.Message);
			}

			if (sessionFailureData.Timestamp != null)
			{
				LogI(LOG_TAG,"Timestamp: " + sessionFailureData.Timestamp);
			}

			if (sessionFailureData.Adid != null)
			{
				LogI(LOG_TAG,"Adid: " + sessionFailureData.Adid);
			}

			if (sessionFailureData.JsonResponse != null)
			{
				LogI(LOG_TAG,"JsonResponse: " + sessionFailureData.GetJsonResponse());
			}

			LogI(LOG_TAG,"WillRetry: " + sessionFailureData.WillRetry.ToString());
		}
		*/
		#endregion
		
		#region 工具接口

		private static AdjustEnvironment GetAdjustEnvironment()
		{
#if UNITY_EDITOR || DEBUG
			return AdjustEnvironment.Sandbox;
#else
			return AdjustEnvironment.Production;
#endif
		}

		private static AdjustLogLevel GetLogLevel(bool showLogs)
		{
#if UNITY_EDITOR || DEBUG
			return AdjustLogLevel.Verbose;
#endif
			return showLogs? AdjustLogLevel.Verbose : AdjustLogLevel.Suppress;
		}

		private static void LogI(string tag, object content)
		{
			Debug.Log($"{tag} {content}");
		}
		
		private static void LogE(string tag, object content)
		{
			Debug.LogError($"{tag} {content}");
		}
		private static void LogW(string tag, object content)
		{
			Debug.LogWarning($"{tag} {content}");
		}


		#endregion

		#region 广告收入打点


		private bool IsInDelayTimeWindow()
		{
			if (!_deferredReportAdRevenueEnabled) return false;
			
			var appAge = DateTime.UtcNow - _firstOpenTime;
			Debug.Log($"{LOG_TAG} --- AppAge:{appAge.TotalMinutes}  delayMins:{_delayMinutes}  ::  firstOpenTime:{_firstOpenTime:g}    now:{DateTime.UtcNow:g}    dataSource:{_delayMinutesSource}");
			return appAge.TotalMinutes <= _delayMinutes;
		}


		/// <summary>
		/// 设置广告延迟窗口参数
		/// </summary>
		/// <param name="delayMinutes">延迟分钟数</param>
		/// <param name="delayMinutesSource">延迟配置来源</param>
		public void SetAdRevDelayMinutes(float delayMinutes, DelayMinutesSource delayMinutesSource)
		{
			_delayMinutes = delayMinutes;
			_delayMinutesSource = delayMinutesSource;
			Debug.Log($"{LOG_TAG} --- Set delayMinutes:{_delayMinutes}, source:{GetDelayMinutesSourceString(_delayMinutesSource)}");
			
			if (_delayCoroutine != null)
			{
				Debug.Log($"{LOG_TAG} --- Set new delayMinutes:{_delayMinutes}. Stop old Coroutine.");
				CoroutineHelper.Instance.StopCoroutine(_delayCoroutine);
			}
			
			if (IsInDelayTimeWindow())
			{
				float delayTime = GetRemainDelaySeconds();
				Debug.Log($"{LOG_TAG} <color=cyan>--- Will report <AccumulatedAdRevenue> in [{delayTime}] sec </color>");
				_delayCoroutine = CoroutineHelper.Instance.StartDelayed(new WaitForSecondsRealtime(delayTime), TrackAccumulatedAdRevenue);
			}
			else
			{
				// 如果此时已含有累计数据， 则需立即上报
				TrackAccumulatedAdRevenue();
			}
		}

		private string GetDelayMinutesSourceString(DelayMinutesSource source)
		{
			switch (source)
			{
				case DelayMinutesSource.Default:
					return "default";
				case DelayMinutesSource.Local:
					return "local";
				case DelayMinutesSource.RemoteConfig:
					return "remote_config";
			}
			return "default";
		}


		private string GetAppAgeStr(DateTime firstOpenTime)
		{
			var span = DateTime.UtcNow - firstOpenTime;
			return $"{(long)span.TotalMilliseconds}";
		}


		/// <summary>
		/// 上报广告事件
		/// </summary>
		/// <param name="impressionEvent"></param>
		public void TrackAdEvent(AdjustAdImpressionEvent impressionEvent)
		{
			TrackAdEvent(impressionEvent.value, 
				impressionEvent.adSource, 
				impressionEvent.adUnitId, 
				impressionEvent.adPlacement, 
				impressionEvent.adFormat,
				impressionEvent.adPlatform);
		}

		/// <summary>
		/// 广告收入上报 (Adjust 特有的接口)
		/// </summary>
		/// <param name="value">收入价值（USD）</param>
		/// <param name="adSource">广告源</param>
		/// <param name="adUnitId">广告单位 ID</param>
		/// <param name="adPlacement">广告播放场景：main_page</param>
		/// <param name="adFormat">广告类型： BANNER， INTERSTIAL， REWARDED</param>
		/// <param name="adPlatform">广告平台：MAX</param>
		public void TrackAdEvent(double value, string adSource, string adUnitId, string adPlacement, string adFormat, string adPlatform = "MAX")
		{
			// 使用
			_adSourceName = AD_REVENUE_SOURCE_APPLOVIN_MAX;
			if (adPlatform != "MAX")
			{
				_adSourceName = AD_REVENUE_SOURCE_IRONSOURCE;
			}

			if (IsInDelayTimeWindow())
			{
				// 累加本次广告收益
				_adjustModel.AddImpressionRevenue(value);
				// 于值更新后再打印数据
				LogI(LOG_TAG,$"<color=orange>--- Save AdRevenue to model: {_adjustModel}, skip report!</color>");
				return;
			}

			var appAge = GetAppAgeStr(_firstOpenTime);
			// 构建 Adjust 的 AdRevenue 事件
			var adEvent = new AdjustAdRevenue(_adSourceName);
			adEvent.SetRevenue(value, REVENUE_CURRENCY_USD);
			// adEvent.SetAdRevenueNetwork(adSource); // -- V4
			adEvent.AdRevenueNetwork = adSource;
			// adEvent.SetAdRevenueUnit(adUnitId); // -- V4
			adEvent.AdRevenueUnit = adUnitId;
			// adEvent.SetAdRevenuePlacement(adPlacement); // -- V4
			adEvent.AdRevenuePlacement = adPlacement;
			// 每次上报需增加回调参数 AddCallbackParameter
			adEvent.AddCallbackParameter("delay_minutes", $"{_delayMinutes}");
			adEvent.AddCallbackParameter("delay_minutes_source", GetDelayMinutesSourceString(_delayMinutesSource));
			adEvent.AddCallbackParameter("app_age_millis", appAge);
			// 添加合作伙伴参数上报（TikTok）
			adEvent.AddPartnerParameter("ad_unit_id", adUnitId);
			adEvent.AddPartnerParameter("ad_format", adFormat);
			
			// 上报广告收益
			LogI(LOG_TAG,$"<color=#88ff00>--- Report AdRevenue event -> value:{value}\n adSource:{adSource}\n adUnitId:{adUnitId}\n adPlacement:{adPlacement}\n delay_minutes:{_delayMinutes}\n delay_minutes_source:{_delayMinutesSource}\n app_age_millis:{appAge}\n</color>");
			Adjust.TrackAdRevenue(adEvent);
		}

		private void TrackAccumulatedAdRevenue()
		{
			if (!_adjustModel.HasData())
			{
				LogW(LOG_TAG,$"--- Report Accumulated AdRevenue but no data exists, Failed!");
				return;
			}
			
			// 上报累计收益
			var accumulatedRevenue = new AdjustAdRevenue(_adSourceName);
			// accumulatedRevenue.setAdImpressionsCount(_adjustModel.impressionCount); // -- V4
			accumulatedRevenue.AdImpressionsCount = _adjustModel.impressionCount;
			accumulatedRevenue.SetRevenue(_adjustModel.revenue, REVENUE_CURRENCY_USD);
			Adjust.TrackAdRevenue(accumulatedRevenue);
			
			// 记录上报时间, 清除累计数据
			LogI(LOG_TAG,$"<color=yellow>--- Report Accumulated AdRevenue -> {_adjustModel}</color>");
			_adjustModel.SetReportDateAndClear(DateTime.UtcNow);
		}

		#endregion
		
		#region IAP收入上报

		/// <summary>
		/// IAP订阅支付事件上报
		/// </summary>
		/// <param name="token"></param>
		/// <param name="iapEvent"></param>
		public void TrackIapEvent(string token, AdjustIapEvent iapEvent)
		{
			AdjustEvent adjustEvent = new AdjustEvent(token);
			
			// --- 属性赋值 ---
			adjustEvent.SetRevenue(iapEvent.value, iapEvent.currency);
			// adjustEvent.SetProductId(iapEvent.productId); // -- V4
			adjustEvent.ProductId = iapEvent.productId;
			// adjustEvent.setTransactionId(iapEvent.orderId); // -- V4
			adjustEvent.TransactionId = iapEvent.orderId;
			if (!string.IsNullOrEmpty(iapEvent.purchaseToken))
			{
				// adjustEvent.setPurchaseToken(iapEvent.purchaseToken); // -- V4
				adjustEvent.PurchaseToken = iapEvent.purchaseToken;
			}

			if (!string.IsNullOrEmpty(iapEvent.receipt))
			{
				// adjustEvent.setReceipt(iapEvent.receipt); // -- V4
				// adjustEvent.Receipt = iapEvent.receipt;   // Disabled in V5
			}	
			// --- BI 属性对齐 ---
			adjustEvent.AddEventParameter("platform", iapEvent.platform);
			adjustEvent.AddEventParameter("value", $"{iapEvent.value}");
			adjustEvent.AddEventParameter("currency", iapEvent.currency);
			adjustEvent.AddEventParameter("product_id", iapEvent.productId);
			adjustEvent.AddEventParameter("order_id", iapEvent.orderId);
			adjustEvent.AddEventParameter("order_type", iapEvent.orderType);
			adjustEvent.AddEventParameter("trans_ts", $"{iapEvent.transTs}");
			// adjustEvent.AddEventParameter("sandbox", iapEvent.sandbox);

			Adjust.TrackEvent(adjustEvent);
			LogI(LOG_TAG,$"<color=#88ff00>--- Report IAP event -> token:{token}  value:{iapEvent.value}  productId:{iapEvent.productId}  orderId:{iapEvent.orderId}</color>");
		}

		#endregion
		
		#region 普通事件上报

		/// <summary>
		/// 上报通常事件
		/// </summary>
		/// <param name="eventToken"></param>
		/// <param name="data"></param>
		public void TrackEvent(string eventToken, Dictionary<string, object> data)
		{
			var adjustEvent = new AdjustEvent(eventToken);
			if (data != null && data.Count > 0)
			{
				foreach (var kv in data)
				{
					adjustEvent.AddEventParameter(kv.Key, kv.Value.ToString());
				}
			}
			// 普通事件上报
			LogI(LOG_TAG, $"<color=#88ff00>--- Report Normal Event -> token:{eventToken}</color>");
			Adjust.TrackEvent(adjustEvent);
		}
		
		#endregion

		#region ATT 上报

		/// <summary>
		/// 检查新的 ATT 状态
		/// </summary>
		public void CheckNewAttStatus()
		{
			// In SDK v4, you can use the Adjust.checkForNewAttStatus() method to prompt the SDK to read a user’s ATT status and forward the information to Adjust’s servers.
			// This method has been removed in SDK v5.
			// Adjust.checkForNewAttStatus();
		}

		#endregion
	}
}