namespace Guru
{
	using System;
	using System.Threading.Tasks;
	using Firebase.Auth;
	using Firebase.Extensions;
	using UnityEngine;
	
	public static partial class FirebaseUtil
	{
		private const float LOGIN_RETRY_MAX_TIME = 60; // 最大请求间隔时间
		private const float LOGIN_RETRY_INTERVAL = 10; // 最大请求间隔时间
		private static float _retryDelayTime = 10; // 登录重试时间

		///  <summary>
		/// 登录 Firebase 用户
		///  </summary>
		///  <param name="authToken"></param>
		///  <param name="onLoginResult"></param>
		///  <param name="autoRetry"></param>
		private static void LoginFirebaseWithToken(Action<bool, FirebaseUser> onLoginResult = null, string authToken = "", bool autoRetry = true)
		{
			var firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
			
			
			// #1 Firebase 已获取用户
			if (firebaseUser != null)
			{
				Log.I(LOG_TAG, $"[Auth] user exists，UserId:{firebaseUser.UserId}");
				onLoginResult?.Invoke(true, firebaseUser);
				return;
			}
			
			if (string.IsNullOrEmpty(authToken)) authToken = IPMConfig.FIREBASE_AUTH_TOKEN;
			Log.I(LOG_TAG, $"[Auth] Firebase Token:{authToken}");

			if (!string.IsNullOrEmpty(authToken) && NetworkUtil.IsNetAvailable)
			{

				LoginFirebase(authToken, autoRetry, onLoginResult);
				return;
			}
			
			// Token 为空 或 网络不可用
			if (autoRetry)
			{
				// 继续重试
				DelayCallFirebaseLogin(Mathf.Min(_retryDelayTime, LOGIN_RETRY_MAX_TIME), onLoginResult, authToken);
				_retryDelayTime += LOGIN_RETRY_INTERVAL; // 最大重试间隔 60s
			}
			else
			{
				// 不再重试
				onLoginResult?.Invoke(false, null);
			}
		}

		private static void LoginFirebase(string authToken = "", bool autoRetry = true, Action<bool, FirebaseUser> onLoginResult = null)
		{
			
			FirebaseAuth.DefaultInstance.SignInWithCustomTokenAsync(authToken)
			.ContinueWithOnMainThread(task =>
			{
				// ----- Task failed ----- 
				if (task.IsCanceled || task.IsFaulted)
				{
					Log.E(LOG_TAG,"[Auth] SignInWithCustomTokenAsync encountered an error: " + task.Exception);
					if (autoRetry)
					{
						DelayCallFirebaseLogin(_retryDelayTime, onLoginResult, authToken); // 自动重试
					}
					else
					{
						onLoginResult?.Invoke(false, null); // 不再重试
					}
					return;
				}
				// ----- Check Result ----- 
				var firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
				bool success = firebaseUser != null;
				onLoginResult?.Invoke(success, firebaseUser);
				_retryDelayTime = LOGIN_RETRY_INTERVAL;
				Analytics.SetSignUpMethod("google"); // 上报用户登录属性
			});
		}
		
		
		/// <summary>
		/// 延迟回调获取 FirebaseUser
		/// </summary>
		/// <param name="delaySeconds"></param>
		/// <param name="callback"></param>
		/// <param name="token"></param>
		private static void DelayCallFirebaseLogin(float delaySeconds, Action<bool, FirebaseUser> callback, string token = "")
		{
			var delay = new WaitForSeconds(delaySeconds);
			CoroutineHelper.Instance.StartDelayed(delay, ()=> LoginFirebaseWithToken(callback, token));
		}
		
	}
}