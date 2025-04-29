namespace Guru
{
	using System;
	using System.Globalization;
	using UnityEngine;
	
	/// <summary>
	/// 中台配置信息参数
	/// </summary>
	public static class IPMConfig
	{
		//TODO: 下个版本解除 GuruSettings 依赖
		public static string IPM_X_APP_ID => GuruSettings.Instance.IPMSetting.AppId;
		//TODO: 下个版本解除 GuruSettings 依赖
		public static int TOKEN_VALID_TIME => GuruSettings.Instance.IPMSetting.TokenValidTime;
		
		public static readonly int FIREBASE_TOKEN_VALID_TIME = TimeUtil.HOUR_TO_SECOND;
		// public static bool UsingUUID = GuruSettings.Instance.UsingUUID(); // 不再使用 UUID 
		
		public static readonly string IPM_APP_PACKAGE_NAME = Application.identifier;
		public static string IPM_APP_VERSION = Application.version;
		public static string IPM_IOS_APP_GROUP = "group." + Application.identifier;
		public static string IPM_BRAND = "";
		public static string IPM_LANGUAGE = "";
		public static string IPM_MODEL = "";
		public static string IPM_TIMEZONE = "";
		public static string IPM_LOCALE  = "";
		public static string IPM_COUNTRY_CODE = RegionInfo.CurrentRegion.TwoLetterISORegionName;
		public static bool IPM_NEW_USER = false;


		private static string _deviceId;
		/// <summary>
		/// 中台设备 ID
		/// 此 ID 由中台后台通过项目组传入的 secret 值（一般登录默认匿名登录，secret = deviceId）生成的 UID，例如: BS-YYYYY
		/// 文档参考：https://github.com/castbox/backend-dev/blob/main/saas/%E4%B8%AD%E5%8F%B0%E6%9C%8D%E5%8A%A1%E6%8E%A5%E5%85%A5%E6%89%8B%E5%86%8C.md#5221%E5%8C%BF%E5%90%8D%E6%8E%88%E6%9D%83
		/// </summary>
		public static string IPM_DEVICE_ID
		{
			get
			{
				if (!string.IsNullOrEmpty(_deviceId))
				{
					return _deviceId;
				}
				
				// 读入已经缓存过的 Devices ID
				_deviceId = SavedDeviceId;  
				if (!string.IsNullOrEmpty(_deviceId))
				{
					return _deviceId;
				}
				
				_deviceId = SystemInfo.deviceUniqueIdentifier;
				SavedDeviceId = _deviceId; // 缓存到已保存的 DeviceId 中 
				return _deviceId;
			}
		}

		/// <summary>
		/// 已缓存的 DevicesID
		/// </summary>
		private static string SavedDeviceId
		{
			get => PlayerPrefs.GetString(nameof(SavedDeviceId), "");
			set => PlayerPrefs.SetString(nameof(SavedDeviceId), value);
		}


		private static string _uuid;
		/// <summary>
		/// 用户的 UUID
		/// 注意：必须是根据中台返回的 UID 生成的 UUID
		/// 需求详见：https://www.tapd.cn/33527076/prong/stories/view/1133527076001020001
		/// </summary>
		public static string IPM_UUID
		{
			get
			{
				if (!string.IsNullOrEmpty(_uuid)) return _uuid;
				
				// 获取本地缓存
				_uuid = PlayerPrefs.GetString(nameof(IPM_UUID), "");
				if (!string.IsNullOrEmpty(_uuid)) return _uuid;
				
				// 在没有其他源生成 UUID 时，主动生成 UUID
				if (string.IsNullOrEmpty(IPM_UID)) return ""; // 必须以中台获取 UID 作为填充 KEY，否则返回空值 
				
				_uuid = IDHelper.GenUUID(IPM_UID);
				IPM_UUID = _uuid;
				return _uuid;
			}
			set => PlayerPrefs.SetString(nameof(IPM_UUID), value);
		}

		private static string _uid;
		/// <summary>
		/// 客户端缓存的中台后台生成的 UID
		/// doc: https://github.com/castbox/backend-dev/blob/main/saas/%E4%B8%AD%E5%8F%B0%E6%9C%8D%E5%8A%A1%E6%8E%A5%E5%85%A5%E6%89%8B%E5%86%8C.md#5221%E5%8C%BF%E5%90%8D%E6%8E%88%E6%9D%83
		/// </summary>
		public static string IPM_UID
		{
			get
			{
				if(!string.IsNullOrEmpty(_uid)) return _uid;
				_uid = PlayerPrefs.GetString(nameof(IPM_UID), "");
				return _uid;
			}
			set
			{
				if (string.IsNullOrEmpty(value)) return;
				_uid = value;
				PlayerPrefs.SetString(nameof(IPM_UID), value);
			}
		}

		public static void DebugClearUID()
		{
			_uid = string.Empty;
			PlayerPrefs.SetString(nameof(IPM_UID), "");
		}

		/// <summary>
		/// 后台生成的用户整型ID，与uid唯一对应
		/// doc: https://github.com/castbox/backend-dev/blob/main/saas/%E4%B8%AD%E5%8F%B0%E6%9C%8D%E5%8A%A1%E6%8E%A5%E5%85%A5%E6%89%8B%E5%86%8C.md#5221%E5%8C%BF%E5%90%8D%E6%8E%88%E6%9D%83
		/// </summary>
		public static long IPM_UID_INT
		{
			get => Convert.ToInt64(PlayerPrefs.GetString(nameof(IPM_UID_INT), "0"));
			set => PlayerPrefs.SetString(nameof(IPM_UID_INT), value.ToString());
		}

		private static string _ipmAuthToken;
		/// <summary>
		/// 授权token，用于访问API
		/// doc: https://github.com/castbox/backend-dev/blob/main/saas/%E4%B8%AD%E5%8F%B0%E6%9C%8D%E5%8A%A1%E6%8E%A5%E5%85%A5%E6%89%8B%E5%86%8C.md#5221%E5%8C%BF%E5%90%8D%E6%8E%88%E6%9D%83
		/// </summary>
		public static string IPM_AUTH_TOKEN
		{
			get
			{
				if(!string.IsNullOrEmpty(_ipmAuthToken)) return _ipmAuthToken;
				// 兼容老值
				_ipmAuthToken = PlayerPrefs.GetString("IPM_TOKEN", "");
				if (!string.IsNullOrEmpty(_ipmAuthToken))
				{
					PlayerPrefs.DeleteKey("IPM_TOKEN");
					PlayerPrefs.SetString(nameof(IPM_AUTH_TOKEN), _ipmAuthToken);
					return _ipmAuthToken;
				}
				// 本地取值
				_ipmAuthToken = PlayerPrefs.GetString(nameof(IPM_AUTH_TOKEN), "");
				return _ipmAuthToken;
			}
			set
			{
				if (string.IsNullOrEmpty(value)) return;
				_ipmAuthToken = value;
				PlayerPrefs.SetString(nameof(IPM_AUTH_TOKEN), value);
			}
		}

		/// <summary>
		/// 授权token 生成时间，生成时立即标记
		/// </summary>
		public static int IPM_AUTH_TOKEN_TIME
		{
			get => PlayerPrefs.GetInt(nameof(IPM_AUTH_TOKEN_TIME), 0);
			set => PlayerPrefs.SetInt(nameof(IPM_AUTH_TOKEN_TIME), value);
		}


		private static string _firebaseAuthToken;
		/// <summary>
		/// Firebase Auth Token，用于Firebase的登录
		/// firebaseToken 不能用于发送Push消息，不要与 Firebase Push Token 混淆。
		/// 定义相关文档：https://firebase.google.com/docs/auth/admin/create-custom-tokens
		/// doc: https://github.com/castbox/backend-dev/blob/main/saas/%E4%B8%AD%E5%8F%B0%E6%9C%8D%E5%8A%A1%E6%8E%A5%E5%85%A5%E6%89%8B%E5%86%8C.md#5221%E5%8C%BF%E5%90%8D%E6%8E%88%E6%9D%83
		/// </summary>
		public static string FIREBASE_AUTH_TOKEN
		{
			get
			{
				if(!string.IsNullOrEmpty(_firebaseAuthToken)) return _firebaseAuthToken;	
				// 老 Key 兼容
				_firebaseAuthToken = PlayerPrefs.GetString("IPM_FIREBASE_TOKEN", "");
				if (!string.IsNullOrEmpty(_firebaseAuthToken))
				{
					PlayerPrefs.DeleteKey("IPM_FIREBASE_TOKEN");
					PlayerPrefs.SetString(nameof(FIREBASE_AUTH_TOKEN), _firebaseAuthToken);
					return _firebaseAuthToken;
				}	
				// 本地缓存值
				_firebaseAuthToken = PlayerPrefs.GetString(nameof(FIREBASE_AUTH_TOKEN), "");
				return _firebaseAuthToken;
			}
			set
			{
				if (string.IsNullOrEmpty(value)) return;
				_firebaseAuthToken = value;
				PlayerPrefs.SetString(nameof(FIREBASE_AUTH_TOKEN), value);
			}
		}


		/// <summary>
		/// Firebase Auth Token 生成的时间
		/// </summary>
		public static int IPM_FIREBASE_TOKEN_TIME
		{
			get => PlayerPrefs.GetInt(nameof(IPM_FIREBASE_TOKEN_TIME), 0);
			set => PlayerPrefs.SetInt(nameof(IPM_FIREBASE_TOKEN_TIME), value);
		}
		
		private static string _firebasePushToken;
		/// <summary>
		/// Firebase Push Token，用于Firebase的发送推送消息
		/// 注意和 Auth Token 不同
		/// doc: https://firebase.google.com/docs/reference/unity/class/firebase/messaging/firebase-messaging#class_firebase_1_1_messaging_1_1_firebase_messaging_1ab3a6bd25b8efe25fa8a6cf5019b9b9f7
		/// </summary>
		public static string FIREBASE_PUSH_TOKEN
		{
			get
			{
				if(!string.IsNullOrEmpty(_firebasePushToken)) return _firebasePushToken;
				// 兼容老值
				_firebasePushToken = PlayerPrefs.GetString("IPM_PUSH_TOKEN", "");
				if (!string.IsNullOrEmpty(_firebasePushToken))
				{
					PlayerPrefs.DeleteKey("IPM_PUSH_TOKEN");
					PlayerPrefs.SetString(nameof(FIREBASE_PUSH_TOKEN), _firebasePushToken);
					return _firebasePushToken;
				}
				// 本地缓存值
				_firebasePushToken = PlayerPrefs.GetString(nameof(FIREBASE_PUSH_TOKEN), "");
				return _firebasePushToken;
			}
			set
			{
				if (string.IsNullOrEmpty(value)) return;
				_firebasePushToken = value;
				PlayerPrefs.SetString(nameof(FIREBASE_PUSH_TOKEN), value);
			}
		}

		//是否已经成功上报过设备信息
		public static bool IS_UPLOAD_DEVICE_SUCCESS
		{
			get => PlayerPrefs.GetInt(nameof(IS_UPLOAD_DEVICE_SUCCESS), 0) == 1;
			set => PlayerPrefs.SetInt(nameof(IS_UPLOAD_DEVICE_SUCCESS), value ? 1 : 0);
		}

		private static string _userCreatedTimestamp;
		/// <summary>
		/// 用户创建时间，用于标记用户创建的时间
		/// 请 APP 将 createdAtTimestamp 返回值在客户端打点时放到 user_properties 里，
		/// 对应打点的属性key为：user_created_timestamp，BI后续会用这个属性做用户方面的相关数据分析和统计。
		/// doc: https://firebase.google.com/docs/reference/unity/class/firebase/messaging/firebase-messaging#class_firebase_1_1_messaging_1_1_firebase_messaging_1ab3a6bd25b8efe25fa8a6cf5019b9b9f7
		/// </summary>
		public static string USER_CREATED_TIMESTAMP
		{
			get
			{
				if (!string.IsNullOrEmpty(_userCreatedTimestamp)) return _userCreatedTimestamp;
				// 兼容老值
				_userCreatedTimestamp = PlayerPrefs.GetString("IPM_CREATED_TIMESTAMP", "");
				if (!string.IsNullOrEmpty(_userCreatedTimestamp))
				{
					PlayerPrefs.DeleteKey("IPM_CREATED_TIMESTAMP");
					PlayerPrefs.SetString(nameof(USER_CREATED_TIMESTAMP), _userCreatedTimestamp);
					return _userCreatedTimestamp;
				}
				// 缓存值
				_userCreatedTimestamp = PlayerPrefs.GetString(nameof(USER_CREATED_TIMESTAMP), "");
				return _userCreatedTimestamp;
			}
			set
			{
				_userCreatedTimestamp = value;
				PlayerPrefs.SetString(nameof(USER_CREATED_TIMESTAMP), value);
			}
		}

		public static string GetCountryCode()
		{
#if UNITY_EDITOR
			return "US";
#else
			return IPM_COUNTRY_CODE.ToUpper();
#endif
		}

		public static string GetDeviceType()
		{
			#if UNITY_IOS
				return "iOS";
			#else
				return "android";
			#endif
		}
		
		//------------ 2023-04 new IDs --------------
		private static string _firebaseId = "";
		/// <summary>
		/// Firebase Analytics ID
		/// 通过 FirebaseAnalytics.GetAnalyticsInstanceIdAsync 获取
		/// doc: https://firebase.google.com/docs/reference/unity/class/firebase/analytics/firebase-analytics#getanalyticsinstanceidasync
		/// </summary>
		public static string FIREBASE_ID
		{
			get
			{
				if (string.IsNullOrEmpty(_firebaseId))
				{
					_firebaseId = PlayerPrefs.GetString(nameof(FIREBASE_ID), "");
				}
				return _firebaseId;
			}
			set
			{
				if(string.IsNullOrEmpty(value)) return;
				
				_firebaseId = value;
				PlayerPrefs.SetString(nameof(FIREBASE_ID), value);
			}
		}
		
		private static string _appsflyerId = "";
		public static string APPSFLYER_ID
		{
			get
			{
				if (string.IsNullOrEmpty(_appsflyerId))
				{
					_appsflyerId = PlayerPrefs.GetString(nameof(APPSFLYER_ID), "");
				}
				return _appsflyerId;
			}
			set
			{
				if(string.IsNullOrEmpty(value)) return;
				
				_appsflyerId = value;
				PlayerPrefs.SetString(nameof(_appsflyerId), value);
			}
		}

		public static void DebugClearFirebaseId()
		{
			_firebaseId = "test";
			PlayerPrefs.SetString(nameof(FIREBASE_ID), "test");
		}

		private static string _adjustDeviceId = "";
		/// <summary>
		/// Adjust 平台的 DeviceID
		/// 通过 Adjust.getAdid() 接口获取
		/// Editor 下直接返回空值。 SDK 做了预处理
		/// doc: https://dev.adjust.com/en/sdk/unity/features/device-info/
		/// </summary>
		public static string ADJUST_DEVICE_ID
		{
			get
			{
#if UNITY_EDITOR
				return "editor_fake_adjust_id";
#endif
				if (string.IsNullOrEmpty(_adjustDeviceId))
				{
					_adjustDeviceId = PlayerPrefs.GetString(nameof(ADJUST_DEVICE_ID), "not_set");
				}

				return _adjustDeviceId;
			}
			set
			{
				if (string.IsNullOrEmpty(value)) return;
				if (_adjustDeviceId.Equals(value)) return;
				
				_adjustDeviceId = value;
				PlayerPrefs.SetString(nameof(ADJUST_DEVICE_ID), value);
			}
		}

		public static void DebugClearAdjustId()
		{
			_adjustDeviceId = "test";
			PlayerPrefs.SetString(nameof(ADJUST_DEVICE_ID), "test");
		}
		

		private static string _idfa = "";
		/// <summary>
		/// Apple 平台专用的用户的广告 ID
		/// 中台采用 Unity 内置的方法获取该值
		/// 此值和 ATT 授权关系密切
		/// Android 平台做了预处理
		/// doc: https://www.adjust.com/glossary/idfa/
		/// </summary>
		public static string IDFA
		{
			get
			{
#if UNITY_ANDROID
				return "no_support_for_android";
#endif

#if UNITY_IOS
				if (string.IsNullOrEmpty(_idfa))
					_idfa = UnityEngine.iOS.Device.advertisingIdentifier;
#endif
				
				if (string.IsNullOrEmpty(_idfa)) _idfa = "not_set";
				return _idfa;
			}
			set
			{
				if(string.IsNullOrEmpty(value)) return;
				_idfa = value;
			}
		}

		private static string _idfv = "";
		/// <summary>
		/// Apple 平台专用的用户的厂商 ID
		/// 中台采用 Unity 内置的方法获取该值
		/// Android 平台做了预处理
		/// doc: https://www.adjust.com/glossary/idfv/
		/// </summary>
		public static string IDFV
		{
			get
			{
#if UNITY_ANDROID
				return "no_support_for_android";
#endif
				
#if UNITY_IOS
				if (string.IsNullOrEmpty(_idfv))
					_idfv = UnityEngine.iOS.Device.vendorIdentifier;
#endif
				
				if (string.IsNullOrEmpty(_idfv)) _idfv = "not_set";
				return _idfv;
			}
			set
			{
				if(string.IsNullOrEmpty(value)) return;
				_idfv = value;
			}
		}
		
		private static string _googleAdid = "";
		/// <summary>
		/// Google 平台专用的用户广告 ID
		/// 中台从 Adjust 服务内获取该值
		/// iOS 平台做了预处理
		/// doc: https://support.google.com/googleplay/android-developer/answer/6048248?hl=zh-Hans
		/// </summary>
		public static string GOOGLE_ADID
		{
			get
			{
#if UNITY_IOS
				return "no_support_for_ios";
#endif
				if (string.IsNullOrEmpty(_googleAdid))
				{
					_googleAdid = PlayerPrefs.GetString(nameof(GOOGLE_ADID), "");
				}
				return _googleAdid;
			}
			set
			{
				if (string.IsNullOrEmpty(value)) return;
				if (_googleAdid.Equals(value)) return;
				
				_googleAdid = value;
				PlayerPrefs.SetString(nameof(GOOGLE_ADID), value);
			}
		}

		public static void DebugClearGoogleAdid()
		{
			_googleAdid = "test";
			PlayerPrefs.SetString(nameof(GOOGLE_ADID), "test");
		}
		
		public static string ADJUST_GOOGLE_ADID
		{
			get => PlayerPrefs.GetString(nameof(ADJUST_GOOGLE_ADID), "");
			set => PlayerPrefs.SetString(nameof(ADJUST_GOOGLE_ADID), value);
		}
		
		private static string _firstOpenTime = "";
		/// <summary>
		/// 应用首次启动时间（本地记录）
		/// </summary>
		public static string FIRST_OPEN_TIME
		{
			get
			{
				if (!string.IsNullOrEmpty(_firstOpenTime)) return _firstOpenTime;
				
				_firstOpenTime = PlayerPrefs.GetString(nameof(FIRST_OPEN_TIME), "");
				if (!string.IsNullOrEmpty(_firstOpenTime)) return _firstOpenTime;
				
				_firstOpenTime = TimeUtil.GetCurrentTimeStamp().ToString();
				PlayerPrefs.SetString(nameof(FIRST_OPEN_TIME), _firstOpenTime);
				return _firstOpenTime; 
			}
		}


		private static string _androidId = "";

		/// <summary>
		/// Android 平台的用户 ID
		/// 从 Android 平台内部类获取
		/// iOS 做了预处理
		/// doc: https://developer.android.com/reference/android/provider/Settings.Secure#ANDROID_ID
		/// </summary>
		public static string ANDROID_ID
		{
			get
			{
#if UNITY_EDITOR
				return System.Guid.NewGuid().ToString("N");
#elif UNITY_IOS
				return "no_support_for_ios";
#endif

				if (!string.IsNullOrEmpty(_androidId)) return _androidId;

				_androidId = "not_set";
#if UNITY_ANDROID
				try
				{
					AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
					AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
					AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
					AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure");
					_androidId = secure.CallStatic<string>("getString", contentResolver, "android_id");
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
#endif
				return _androidId;
			}
		}
		

		/// <summary>
		/// 获取首次启动的日期
		/// </summary>
		/// <returns></returns>
		public static DateTime GetFirstOpenDate()
		{
			if (long.TryParse(FIRST_OPEN_TIME, out var longValue))
			{
				return TimeUtil.ConvertTimeSpanToDateTime(longValue);
			}
			return new DateTime(1970, 1, 1);
		}


	}
}