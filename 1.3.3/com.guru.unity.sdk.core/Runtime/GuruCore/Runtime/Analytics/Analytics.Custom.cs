namespace Guru
{
	using System;
	using Firebase.Crashlytics;
	using UnityEngine;
	
	
	/// <summary>
    /// 自打点逻辑
    /// </summary>
    public partial class Analytics
    {
	    
	    private static DateTime _lastReportRateDate; //上次上报信息的日期
	    private const double _reportSuccessInterval = 120; // 上报频率
#if UNITY_IOS
		private const string VALUE_NOT_FOR_IOS = "not_support_for_ios";
#endif
	    // private const string VALUE_ONLY_FOR_IOS = "idfa_only_for_ios";
	    
	    private static bool _isGuruAnalyticInitOnce = false;

	    public static void InitGuruAnalyticService(string firebaseId, string guruSDKVersion)
	    {
		    if (_isGuruAnalyticInitOnce) return;
		    _isGuruAnalyticInitOnce = true;

		    try
		    {
			    string appId = IPMConfig.IPM_X_APP_ID;
			    string deviceInfo = new DeviceInfoData().ToString();
			    
			    _lastReportRateDate = DateTime.Now;

			    Debug.Log($"{TAG} --- InitGuruAnalyticService: IsDebug:{IsDebug}  firebaseId:{firebaseId}");
			    
			    GuruAnalytics.Instance.Init(appId, deviceInfo, firebaseId, guruSDKVersion, () =>
			    {
				    OnGuruAnalyticsInitComplete();
				    Debug.Log($"{TAG} --- Guru EXP: GroupId: {GuruAnalytics.Instance.ExperimentGroupId}");
				    SetAnalyticsExperimentGroup(GuruAnalytics.Instance.ExperimentGroupId);
			    }, IsDebug); // Android 初始化	
			    
		    }
		    catch (Exception ex)
		    {
			    LogCrashlytics(ex);
		    }
	    }
    }
}