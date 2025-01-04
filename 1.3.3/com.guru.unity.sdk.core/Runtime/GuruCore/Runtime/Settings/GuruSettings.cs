using System;
using System.Collections.Generic;
using UnityEngine;

namespace Guru
{
	[CreateAssetMenu(fileName = "GuruSettings", menuName = "GuruSettings", order = 0)]
	public partial class GuruSettings : ScriptableObject
	{
		private static GuruSettings _instance;
		public static GuruSettings Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = LoadSettingsAsset();
				}

				return _instance;
			}
		}

		[Header("公司名称")]
		public string CompanyName = "Guru";
		[Header("产品名称")]
		public string ProductName = "Default Product";
		[Header("产品包名")]
		public string GameIdentifier = "com.guru.default.product";
		[Header("产品反馈邮箱（评分反馈邮箱）")]
		public string SupportEmail =  "test@fungame.studio";
		[Header("隐私协议URL")]
		public string PriacyUrl = "";
		[Header("服务条款URL")]
		public string TermsUrl = "";
		[Header("Android商店URL")]
		public string AndroidStoreUrl = "";
		[Header("iOS商店URL")]
		public string IOSStoreUrl = "";
		
		[Header("中台配置")]
		public IPMSetting IPMSetting;
		[Header("打点配置")]
		public AnalyticsSetting AnalyticsSetting;
		[Header("广告配置")]
		public ADSetting ADSetting;
		[Header("Adjust配置")]
		public AdjustSetting AdjustSetting;

		private static GuruSettings LoadSettingsAsset()
		{
			return Resources.Load<GuruSettings>("GuruSettings");
		}
		/// <summary>
		/// 运行时更新Adjust 的 AppToken
		/// </summary>
		/// <param name="androidToken"></param>
		/// <param name="iosToken"></param>
		public void UpdateAdjustTokens(string androidToken, string iosToken)
		{
			if(!string.IsNullOrEmpty(androidToken))
				AdjustSetting.androidAppToken = androidToken;
			if(!string.IsNullOrEmpty(iosToken))
				AdjustSetting.iOSAppToken = iosToken;
		}

		/// <summary>
		/// 运行时更新 Adjust 的事件列表
		/// </summary>
		public void UpdateAdjustEvents(IList<string> events)
		{
			if (events != null && events.Count > 0)
			{
				List<AnalyticsSetting.AdjustEvent> evtList = new List<AnalyticsSetting.AdjustEvent>(events.Count);
				string key, atk, itk;
				string[] tmp;
				for (int i = 0; i < events.Count; i++)
				{
					tmp = events[i].Split(',');
					if (tmp != null && tmp.Length > 2)
					{
						evtList.Add(new AnalyticsSetting.AdjustEvent()
						{
							EventName = tmp[0],
							AndroidToken = tmp[1],
							IOSToken = tmp[2],
						});
					}
				}
				AnalyticsSetting.adjustEventList = evtList;
			}
		}

		public void UpdateAppSettings(string bundleId = "", string fbAppId = "",
			string supportEmail = "",
			string privacyUrl = "", string termsUrl = "", 
			string androidStoreUrl = "", string iosStoreUrl = "", bool usingUUID = false, string cdnHost = "")
		{
			if (!string.IsNullOrEmpty(bundleId)) IPMSetting.bundleId = bundleId;
			if (!string.IsNullOrEmpty(supportEmail)) SupportEmail = supportEmail;
			if (!string.IsNullOrEmpty(privacyUrl)) PriacyUrl = privacyUrl;
			if (!string.IsNullOrEmpty(termsUrl)) TermsUrl = termsUrl;
			if (!string.IsNullOrEmpty(androidStoreUrl)) AndroidStoreUrl = androidStoreUrl;
			if (!string.IsNullOrEmpty(iosStoreUrl)) IOSStoreUrl = iosStoreUrl;
			if (!string.IsNullOrEmpty(fbAppId)) IPMSetting.fbAppId = fbAppId;
			if (!string.IsNullOrEmpty(cdnHost)) IPMSetting.cdnHost = cdnHost;
			IPMSetting.usingUUID = usingUUID;
		}
		
		public string CdnHost() => IPMSetting.cdnHost;
		public bool UsingUUID() => IPMSetting.usingUUID;
	}
	
	[Serializable]
	public class IPMSetting
	{
		[Header("中台项目ID")]
		[SerializeField] private string appID;
		[Header("中台Token有效时间（s）")]
		[SerializeField] internal int tokenValidTime = 604800;
		[Header("应用包名")]
		[SerializeField] internal string bundleId;
		[Header("Facebook App ID")]
		[SerializeField] internal string fbAppId;
		[Header("Facebook Client Token")]
		[SerializeField] internal string fbClientToken;
		[Header("Cdn Host 地址")]
		[SerializeField] internal string cdnHost;
		[Header("是否使用 UUID")] 
		[SerializeField] internal bool usingUUID = true;
		
		public string AppId => appID;
		public int TokenValidTime => tokenValidTime;
		public string AppBundleId => bundleId;
		public string FacebookAppId => fbAppId;
		public string FacebookClientToken => fbClientToken;
	}

	[Serializable]
	public class AnalyticsSetting
	{
		[SerializeField] private int levelEndSuccessNum = 50;
		[Obsolete("Will not use in next version", false)]
		[SerializeField] private bool enalbeFirebaseAnalytics = true;
		[Obsolete("Will not use in next version", false)]
		[SerializeField] private bool enalbeFacebookAnalytics = true;
		[Obsolete("Will not use in next version", false)]
		[SerializeField] private bool enalbeAdjustAnalytics = true;
		
		[SerializeField] internal List<AdjustEvent> adjustEventList;

		public int LevelEndSuccessNum => levelEndSuccessNum;
		public bool EnalbeFirebaseAnalytics => enalbeFirebaseAnalytics;
		public bool EnalbeFacebookAnalytics => enalbeFacebookAnalytics;
		public bool EnalbeAdjustAnalytics => enalbeAdjustAnalytics;
		public List<AdjustEvent> AdjustEventList => adjustEventList;

		[Serializable]
		public class AdjustEvent
		{
			public string EventName;
			public string AndroidToken;
			public string IOSToken;
		}
	}

	[Serializable]
	public class ADSetting
	{
		public string SDK_KEY;
		public string Android_Banner_ID;
		public string Android_Interstitial_ID;
		public string Android_Rewarded_ID;
		public string IOS_Banner_ID;
		public string IOS_Interstitial_ID;
		public string IOS_Rewarded_ID;
		
		public string GetRewardedVideoID()
		{
#if UNITY_IOS
			return IOS_Rewarded_ID;
#else
			return Android_Rewarded_ID;
#endif
		}
    
		public string GetInterstitialID()
		{
#if UNITY_IOS
			return IOS_Interstitial_ID;
#else
			return Android_Interstitial_ID;
#endif
		}
    
		public string GetBannerID()
		{
#if UNITY_IOS
			return IOS_Banner_ID;
#else
			return Android_Banner_ID;
#endif
		}
	}

	[Serializable]
	public class AdjustSetting
	{
		[SerializeField] internal string androidAppToken;
		[SerializeField] internal string iOSAppToken;

		public string AndroidAppToken => androidAppToken;
		public string IOSAppToken => iOSAppToken;

		public string GetAppToken()
		{
			#if UNITY_ANDROID
				return androidAppToken;
			#elif UNITY_IOS
				return iOSAppToken; 
			#else
				return string.Empty;
			#endif 
		}
	}
}