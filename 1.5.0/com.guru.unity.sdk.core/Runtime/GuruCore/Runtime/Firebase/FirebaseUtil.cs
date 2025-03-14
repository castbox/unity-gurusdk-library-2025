
namespace Guru
{
	using System;
	using Firebase;
	using Firebase.Analytics;
	using Firebase.Extensions;
	using Firebase.Auth;
	using UnityEngine;
	
	public static partial class FirebaseUtil
	{
		public const string LOG_TAG = "Firebase";
		private static bool _isReady = false;
		public static bool IsReady => _isReady && IsFirebaseInitialized;

		private static DependencyStatus DependencyStatus = DependencyStatus.UnavailableOther;
		public static bool IsFirebaseInitialized => DependencyStatus == DependencyStatus.Available;

		private static Action<bool> _onCheckAndFixDepsHandler;
		private static Action<string> _onGetFirebaseIdHandler;
		private static Action<bool> _onGetGuruUIDHandler;
		private static Action<bool, FirebaseUser> _onFirebaseLoginResult;

		/// <summary>
		/// 初始化 Firebase
		/// </summary>
		/// <param name="onDepsCheckResult">Firebase 自身解决依赖结果回调</param>
		/// <param name="onGetFirebaseId">异步获取到 FirebaseId 回调</param>
		/// <param name="onGetGuruUIDResult">Firebase 授权回调</param>
		/// <param name="onFirebaseLoginResult"></param>
		public static void Init(Action<bool> onDepsCheckResult, Action<string> onGetFirebaseId = null, 
			Action<bool> onGetGuruUIDResult = null, Action<bool, FirebaseUser>  onFirebaseLoginResult = null)
		{
			_isReady = false;
			_onCheckAndFixDepsHandler = onDepsCheckResult;
			_onGetFirebaseIdHandler = onGetFirebaseId;
			_onGetGuruUIDHandler = onGetGuruUIDResult;
			_onFirebaseLoginResult = onFirebaseLoginResult;
			
			// 初始化 Firebase 依赖
			FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
				DependencyStatus = task.Result;
				if (DependencyStatus == DependencyStatus.Available)
				{
					_isReady = true;
					OnFirebaseDepsCheckSuccess(); // Deps 处理通过
				} 
				else 
				{
					Log.E(LOG_TAG, "Could not resolve all Firebase dependencies: " + DependencyStatus);
				}
				_onCheckAndFixDepsHandler?.Invoke(_isReady);
			});
		}
		
		/// <summary>
		/// Deps 处理通过
		/// 初始化各模块
		/// </summary>
		private static void OnFirebaseDepsCheckSuccess()
		{
			Log.I(LOG_TAG, "Firebase deps check Success");
			
			InitCrashlytics();  // 老项目沿用此逻辑
			InitRemoteConfig();	// 老项目沿用此逻辑
			InitializeMessage(); // 初始化 Messaging 服务
			GetFirebaseIdAsync();  // 开始获取 FirebaseId
			
			StartVerifyTokenAndAuthAsync(); //开始验证授权和Token
		}

		#region FirebaseId

		private static void GetFirebaseIdAsync()
		{
			// 异步获取 FirebaseID
			FirebaseAnalytics.GetAnalyticsInstanceIdAsync()
				.ContinueWithOnMainThread(task =>
				{
					string fid = task.Result;
					if (task.IsCompleted && !string.IsNullOrEmpty(fid))
					{
						bool hasChange = IPMConfig.FIREBASE_ID.Equals(fid) == false;
						// 保存本地ID备份
						IPMConfig.FIREBASE_ID = fid; // 保存FirebaseID
						Debug.Log($"[SDK] --- Get FirebaseID: {fid}");
						
						if (hasChange)
						{
							AuthEventConfigRequest eventConfigRequest = new AuthEventConfigRequest();
							eventConfigRequest.SetRetryTimes(-1).SetRetryWaitSeconds(10).Send();
						}
					}
					else
					{
						Debug.LogError($"[SDK] --- Fetch FirebaseID failed on start!");
					}
					_onGetFirebaseIdHandler?.Invoke(fid);
				});
		}

		#endregion

		#region Token and Auth

		public static void LoginGuru()
		{
			// 没有存储UID时，从中台获取匿名认证授权
			StartGuruLoginWithDeviceId(success =>
			{
				GuruDeviceInfoUploader.Instance.Upload(); //获取 UID 后，需在此上报设备信息
					
				_onGetGuruUIDHandler?.Invoke(success);
				
				if (success) {
					// 用户 UID 不为空
					StartLoginWithFirebase();
				}
				else
				{
					Log.W(LOG_TAG, "Get UID failed...");
				}
			});
		}
		
		/// <summary>
		/// 异步验证所有 Token 有效期
		/// </summary>
		private static void StartVerifyTokenAndAuthAsync()
		{
			if (string.IsNullOrEmpty(IPMConfig.IPM_UID))
			{
				Log.I(LOG_TAG, "No Saved UID，get uid form backend API...");
				LoginGuru();
			}
			else
			{
				_onGetGuruUIDHandler?.Invoke(true); // 用户 UID 不为空则应该直接调用 UID 上报
				
				Log.I(LOG_TAG, $"Saved UID: {IPMConfig.IPM_UID} : Check token expired...");
				// 检查中台 Token 是否过期
				if (IsGuruTokenExpired())
				{
					Log.I(LOG_TAG,"Guru Token is expired, call backend API to refresh");
					RefreshGuruToken();
				}

				// 检查中台 Firebase Token 是否过期
				if (IsFirebaseTokenExpired())
				{
					Log.I(LOG_TAG,"Firebase Token is expired, call backend API to refresh");
					RefreshFirebaseToken(StartLoginWithFirebase); // 重新获取 Firebase Token
				}
				else
				{
					StartLoginWithFirebase();
				}
			}
		}
		

		/// <summary>
		/// 使用设备 ID 进行中台匿名认证
		/// </summary>
		private static void StartGuruLoginWithDeviceId(Action<bool> onLoginResult = null)
		{
			// 没有存储UID时，从中台获取匿名认证授权
			var request = new AuthUserRequest()
				.SetRetryTimes(-1) // 不成功的话会一直请求
				.SetSuccessCallBack(() =>
				{
					onLoginResult?.Invoke(true);
				}).SetFailCallBack(() =>
				{
					onLoginResult?.Invoke(false);
				});
			request.Send();
		}

		/// <summary>
		/// Firebase Token 是否过期
		/// </summary>
		/// <returns></returns>
		private static bool IsFirebaseTokenExpired()
		{
			int currentTimeStamp = TimeUtil.GetCurrentTimeStampSecond();
			return currentTimeStamp - IPMConfig.IPM_AUTH_TOKEN_TIME >= IPMConfig.TOKEN_VALID_TIME;
		}
		
		private static void RefreshFirebaseToken(Action onFirebaseTokenRefreshed = null)
		{
			//中台firebaseToken失效，从中台重新获取firebaseToken
			var request = new RefreshFirebaseTokenRequest()
				.SetRetryTimes(-1)
				.SetSuccessCallBack(()=> onFirebaseTokenRefreshed?.Invoke());
			request.Send();
		}
		
		/// <summary>
		/// Guru Token 是否过期
		/// </summary>
		/// <returns></returns>
		private static bool IsGuruTokenExpired()
		{
			int currentTimeStamp = TimeUtil.GetCurrentTimeStampSecond();
			return currentTimeStamp - IPMConfig.IPM_FIREBASE_TOKEN_TIME >= IPMConfig.FIREBASE_TOKEN_VALID_TIME;
		}

		private static void RefreshGuruToken()
		{
			//中台Token失效，从中台重新获取Token
			var request = new RefreshTokenRequest()
				.SetRetryWaitSeconds(10)
				.SetRetryTimes(-1);  // 不成功的话会一直请求
			request.Send();
		}
		#endregion
		
		#region Firebase 用户登录

		/// <summary>
		/// 开始登录 Firebase
		/// </summary>
		private static void StartLoginWithFirebase()
		{
			LoginFirebaseWithToken(OnFirebaseLoginComplete, IPMConfig.FIREBASE_AUTH_TOKEN); // 成功后进行 Firebase 认证
		}

		/// <summary>
		/// Firebase 认证用户完成
		/// </summary>
		/// <param name="success"></param>
		/// <param name="firebaseUser"></param>
		private static void OnFirebaseLoginComplete(bool success, FirebaseUser firebaseUser = null)
		{
			_onFirebaseLoginResult?.Invoke(success, firebaseUser);
		}
		
		#endregion

	}
}