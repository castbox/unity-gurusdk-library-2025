namespace Guru
{
    using System;
    
    /// <summary>
    /// 自打点代理接口
    /// </summary>
    public interface IAnalyticsAgent
    {
        void Init(string appId, string deviceInfo, string baseUrl, string[] uploadIpAddress, Action onInitComplete, bool isDebug = false);
        void SetScreen(string screenName);
        void SetAdId(string id);
        void SetUserProperty(string key, string value);
        void SetFirebaseId(string id);
        void SetAdjustId(string id);
        void SetDeviceId(string deviceId);
        void SetUid(string uid);
        bool IsDebug { get; }
        bool EnableErrorLog { get; set; }
        void LogEvent(string eventName, string parameters, int priority = 0);
        void ReportEventSuccessRate(); // 上报任务成功率
        void InitCallback(string objName, string method); // 设置回调对象参数
        int GetEventCountTotal(); // 获取事件总数
        int GetEventCountUploaded(); // 获取成功上报的事件数量
        string GetAuditSnapshot(); // 获取 AuditSnapshot 字段
    }
}