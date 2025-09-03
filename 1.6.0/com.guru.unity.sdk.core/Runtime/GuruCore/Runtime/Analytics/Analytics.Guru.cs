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

	    /// <summary>
	    /// 初始化自打点
	    /// </summary>
	    /// <param name="firebaseId"></param>
	    /// <param name="guruSDKVersion"></param>
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
				    // 上报 guru_analytics_exp 分组 ID，此实验已完结，上报属性需要删除 25-02-08
				    // 需求：https://www.tapd.cn/33527076/prong/stories/view/1133527076001024576?from_iteration_id=1133527076001003267
				    // ReportGuruEXPGroupId();
			    }, IsDebug); // Android 初始化	
			    
		    }
		    catch (Exception ex)
		    {
			    LogCrashlytics(ex);
		    }
	    }
		
	    // 上报实验分组 ID
	    private static void ReportGuruEXPGroupId()
	    {
		    Debug.Log($"{TAG} --- Guru EXP: GroupId: {GuruAnalytics.Instance.ExperimentGroupId}");
		    SetAnalyticsExperimentGroup(GuruAnalytics.Instance.ExperimentGroupId);
	    }
	    
	    


    }
}