using System;
using Firebase.Crashlytics;

namespace Guru
{
	public static partial class FirebaseUtil
	{
		
		
		public static void InitCrashlytics()
		{
			if(!string.IsNullOrEmpty(IPMConfig.IPM_UID))
				SetUserID(IPMConfig.IPM_UID);
			Crashlytics.IsCrashlyticsCollectionEnabled = true;
		}

		/// <summary>
		/// 设置用户ID
		/// </summary>
		public static void SetUserID(string userID)
		{
			if (!IsFirebaseInitialized) return;
			Crashlytics.SetUserId(userID);
		}

		/// <summary>
		/// 设置自定义数据
		/// 崩溃时玩家进度和状态
		/// </summary>
		public static void SetCustomData(string key, string value)
		{
			if (!IsFirebaseInitialized) return;
			Crashlytics.SetCustomKey(key, value);
		}

		/// <summary>
		/// 上报自定义信息日志到Crashlytics
		/// </summary>
		public static void LogMessage(string message)
		{
			if (!IsFirebaseInitialized) return;
			Crashlytics.Log(message);
		}

		/// <summary>
		/// 上报自定义崩溃
		/// </summary>
		public static void LogException(Exception exception)
		{
			if (!IsFirebaseInitialized) return;
			Crashlytics.LogException(exception);
		}
	}
}