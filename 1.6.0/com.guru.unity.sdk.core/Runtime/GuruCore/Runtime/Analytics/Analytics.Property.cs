#nullable enable
namespace Guru
{
	/// <summary>
	/// 上报用户属性逻辑
	/// Firebase 中台属性上报： https://docs.google.com/spreadsheets/d/1N47rXgjatRHFvzWWx0Hqv5C1D9NHHGbggi6pQ65c-zQ/edit?gid=1858695240#gid=1858695240
	/// Guru自打点 中台属性上报：https://docs.google.com/spreadsheets/d/1N47rXgjatRHFvzWWx0Hqv5C1D9NHHGbggi6pQ65c-zQ/edit?gid=1736574940#gid=1736574940
	/// </summary>
    public partial class Analytics
    {
        #region Update all neccessary properties
        
        //---------------- 设置所有必要的属性 ---------------------

        /// <summary>
		/// 设置用户ID
		/// </summary>
		public static void SetUid(string uid)
		{
			_propertiesManager.ReportUid(uid);
		}
        
		/// <summary>
		/// 设置用户 b_level
		/// </summary>
		// TODO: 该值不应该有机会被用户调用
		public static void SetBLevel(int bLevel)
		{
			_propertiesManager.ReportBLevel($"{bLevel}");
		}
		
		/// <summary>
		/// 设置用户 b_play
		/// </summary>
		// TODO: 该值不应该有机会被用户调用
		public static void SetBPlay(int bPlay)
		{
			_propertiesManager.ReportBPlay($"{bPlay}");
		}

		/// <summary>
		/// 设置Analytics 实验分组
		/// (Firebase, Guru)
		/// </summary>
		private static void SetAnalyticsExperimentGroup(string groupName)
		{
			_propertiesManager.ReportAnalyticsExperimentGroup(groupName);
		}
		
		/// <summary>
		/// 设置账号登录方法
		/// (Firebase, Guru)
		/// </summary>
		internal static void SetSignUpMethod(string methodName)
		{
			_propertiesManager.ReportSignUpMethod(methodName);
		}
        
		/// <summary>
		/// 设置设备ID
		/// (Firebase, Guru)
		/// </summary>
		public static void SetDeviceId(string deviceId)
		{
			_propertiesManager.ReportDeviceId(deviceId);
		}
		
		/// <summary>
		/// 设置首次启动时间
		/// </summary>
		/// <param name="firstOpenTime"></param>
		public static void SetFirstOpenTime(string firstOpenTime)
		{
			_propertiesManager.ReportFirstOpenTime(firstOpenTime);
		}
		
		/// <summary>
		/// 设置 IsIapUser
		/// (Firebase, Guru)
		/// </summary>
		public static void SetIsIapUser(bool isIapUser)
		{
			_propertiesManager.ReportIsIapUser(isIapUser? "true":"false");
		}
		
		/// <summary>
		/// 设置 Network
		/// (Firebase, Guru)
		/// </summary>
		public static void SetNetworkStatus(string networkStatus)
		{
			_propertiesManager.ReportNetworkStatus(networkStatus);
		}

		/// <summary>
		/// 设置 AdjustId
		/// (Firebase)
		/// </summary>
		public static void SetAdjustDeviceId(string adjustId)
		{
			_propertiesManager.ReportAdjustId(adjustId);
		}
		
		/// <summary>
		/// 设置 AndroidId
		/// (Firebase)
		/// </summary>
		public static void SetAndroidId(string androidId)
		{
			_propertiesManager.ReportAndroidId(androidId);
		}
		
		/// <summary>
		/// 设置 AttStatus
		/// (Firebase)
		/// </summary>
		public static void SetAttStatus(string attStatus)
		{
			_propertiesManager.ReportAttStatus(attStatus);
		}
		
		/// <summary>
		/// 设置 AttStatus
		/// (Firebase)
		/// </summary>
		public static void SetNotiPerm(string notiPrem)
		{
			_propertiesManager.ReportNotiPerm(notiPrem);
		}
		
		/// <summary>
		/// 设置 AdId
		/// </summary>
		public static void SetGoogleAdId(string adId)
		{
			if (string.IsNullOrEmpty(adId)) adId = "not_set";
			_propertiesManager.ReportGoogleAdId(adId);
		}

		public static void SetIDFV(string idfv)
		{
			if (string.IsNullOrEmpty(idfv)) idfv = "not_set";
			_propertiesManager.ReportIDFV(idfv);
		}

		public static void SetIDFA(string idfa)
		{
			if (string.IsNullOrEmpty(idfa)) idfa = "not_set";
			_propertiesManager.ReportIDFA(idfa);
		}

		public static void SetAppsflyerId(string appsflyerId)
		{
			if (string.IsNullOrEmpty(appsflyerId)) appsflyerId = "not_set";
			_propertiesManager.ReportAppsflyerId(appsflyerId);
		}
		
		public static void SetUserCreatedTime(string timestamp)
		{
			_propertiesManager.ReportUserCreatedTimestamp(timestamp);
		}


		#endregion
        

    }

	#region 中台属性管理


    /// <summary>
    /// 全部属性集合
    /// </summary>
    internal class MidWarePropertiesManager
    {
             
        /*
			=======  必打属性  ========
			user_id  (F,G)
			device_id (F,G)
			first_open_time (F,G)
			is_iap_user (F,G)
			network (F,G)

			adjust_id (F)
			att_status (F)
			noti_perm (F)
			=======  必打属性  ========
			
			=======  补充属性  ========
			firebase_id
			idfv
			idfa
			=======  补充属性  ========
		*/
        
		//-------------------- 设置所有的属性 -----------------------


		private readonly GuruEventDriver _guruEventDriver;
		private readonly FirebaseEventDriver _firebaseEventDriver;
		private readonly ExternalEventDriverManager? _extEventDriverManager;
		
		public MidWarePropertiesManager(GuruEventDriver guruDriver, 
			FirebaseEventDriver firebaseDriver, 
			ExternalEventDriverManager? extEventDriverManager)
		{
			_guruEventDriver = guruDriver;
			_firebaseEventDriver = firebaseDriver;
			_extEventDriverManager = extEventDriverManager;
		}



		public void ReportUid(string uid)
        {
			if (string.IsNullOrEmpty(uid)) return; // 空值不予上报
	        _guruEventDriver.SetUid(uid);
	        _firebaseEventDriver.SetUid(uid);
	        _extEventDriverManager?.SetUid(uid);
        }
        public void ReportBLevel(string bLevel)
        {
	        _guruEventDriver.AddProperty(Analytics.PropertyLevel, bLevel);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyLevel, bLevel);
	        _extEventDriverManager?.AddProperty(Analytics.PropertyLevel, bLevel);
        }
        public void ReportBPlay(string bPlay)
        {
	        _guruEventDriver.AddProperty(Analytics.PropertyPlay, bPlay);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyPlay, bPlay);
	        _extEventDriverManager?.AddProperty(Analytics.PropertyPlay, bPlay);
        }

        public void ReportAnalyticsExperimentGroup(string groupName)
        {
	        _guruEventDriver.AddProperty(Analytics.PropertyAnalyticsExperimentalGroup, groupName);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyAnalyticsExperimentalGroup, groupName);
	        _extEventDriverManager?.AddProperty(Analytics.PropertyAnalyticsExperimentalGroup, groupName);
        }
        
        public void ReportSignUpMethod(string methodName)
        {
	        _guruEventDriver.AddProperty(Analytics.PropertySignUpMethod, methodName);
	        _firebaseEventDriver.AddProperty(Analytics.PropertySignUpMethod, methodName);
	        _extEventDriverManager?.AddProperty(Analytics.PropertySignUpMethod, methodName);
        }
        
        public void ReportDeviceId(string deviceId)
        {
	        if (string.IsNullOrEmpty(deviceId)) return; // 空值不予上报
	        
	        _guruEventDriver.SetDeviceId(deviceId);
	        _guruEventDriver.AddProperty(Analytics.PropertyDeviceID, deviceId);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyDeviceID, deviceId);
	        _extEventDriverManager?.SetDeviceId(deviceId);

        }
        
        public void ReportFirstOpenTime(string firstOpenTime)
        {
	        _guruEventDriver.AddProperty(Analytics.PropertyFirstOpenTime, firstOpenTime);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyFirstOpenTime, firstOpenTime);
	        _extEventDriverManager?.AddProperty(Analytics.PropertyFirstOpenTime, firstOpenTime);
        }

        public void ReportIsIapUser(string isIapUser)
        {
	        _guruEventDriver.AddProperty(Analytics.PropertyIsIAPUser, isIapUser);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyIsIAPUser, isIapUser);
	        _extEventDriverManager?.AddProperty(Analytics.PropertyIsIAPUser, isIapUser);
	        
        }
        
        public void ReportNetworkStatus(string networkStatus)
        {
	        _guruEventDriver.AddProperty(Analytics.PropertyNetwork, networkStatus);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyNetwork, networkStatus);
	        _extEventDriverManager?.AddProperty(Analytics.PropertyNetwork, networkStatus);
        }
        
        public void ReportAdjustId(string adjustId)
        {
	        if (string.IsNullOrEmpty(adjustId)) return; // 空值不予上报
	        
	        _guruEventDriver.SetAdjustId(adjustId);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyAdjustId, adjustId);
	        _extEventDriverManager?.SetAdjustId(adjustId);
        }

        public void ReportAppsflyerId(string appsflyerId)
        {
	        if (string.IsNullOrEmpty(appsflyerId)) return;
	        _guruEventDriver.SetAppsflyerId(appsflyerId);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyAppsflyerId, appsflyerId);
	        _extEventDriverManager?.SetAppsflyerId(appsflyerId);
        }

        public void ReportAttStatus(string attStatus)
        {
	        if (string.IsNullOrEmpty(attStatus)) return; // 空值不予上报
	        
	        _guruEventDriver.AddProperty(Analytics.PropertyAttStatus, attStatus);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyAttStatus, attStatus);
	        _extEventDriverManager?.AddProperty(Analytics.PropertyAttStatus, attStatus);
        }
        
        public void ReportNotiPerm(string notiPerm)
        {
	        if (string.IsNullOrEmpty(notiPerm)) return; // 空值不予上报
	        
	        _guruEventDriver.AddProperty(Analytics.PropertyNotiPerm, notiPerm);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyNotiPerm, notiPerm);
	        _extEventDriverManager?.AddProperty(Analytics.PropertyNotiPerm, notiPerm);
        }
        
        public void ReportAndroidId(string androidId)
        {
	        if (string.IsNullOrEmpty(androidId)) return; // 空值不予上报
	        
	        _guruEventDriver.SetAndroidId(androidId);
	        _guruEventDriver.AddProperty(Analytics.PropertyAndroidId, androidId);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyAndroidId, androidId);
	        _extEventDriverManager?.SetAndroidId(androidId);
        }
        
        public void ReportGoogleAdId(string googleAdId)
        {
	        if (string.IsNullOrEmpty(googleAdId)) return; // 空值不予上报
	        
	        _guruEventDriver.SetGoogleAdId(googleAdId);
	        _guruEventDriver.AddProperty(Analytics.PropertyGoogleAdId, googleAdId);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyGoogleAdId, googleAdId);
	        _extEventDriverManager?.SetGoogleAdId(googleAdId);
        }
        
        public void ReportIDFV(string idfv)
        {
	        if (string.IsNullOrEmpty(idfv)) return; // 空值不予上报
	        
	        _guruEventDriver.SetIDFV(idfv);
	        _guruEventDriver.AddProperty(Analytics.PropertyIDFV, idfv);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyIDFV, idfv);
	        _extEventDriverManager?.SetIDFV(idfv);
        }
        public void ReportIDFA(string idfa)
        {
	        if (string.IsNullOrEmpty(idfa)) return; // 空值不予上报
	        
	        _guruEventDriver.SetIDFA(idfa);
	        _guruEventDriver.AddProperty(Analytics.PropertyIDFA, idfa);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyIDFA, idfa);
	        _extEventDriverManager?.SetIDFA(idfa);
        }
        
        public void ReportUserCreatedTimestamp(string timestamp)
        {
	        if (string.IsNullOrEmpty(timestamp)) return; // 空值不予上报
	        
	        _guruEventDriver.AddProperty(Analytics.PropertyUserCreatedTimestamp, timestamp);
	        _firebaseEventDriver.AddProperty(Analytics.PropertyUserCreatedTimestamp, timestamp);
	        _extEventDriverManager?.AddProperty(Analytics.PropertyUserCreatedTimestamp, timestamp);
        }
    }
    
    #endregion
}