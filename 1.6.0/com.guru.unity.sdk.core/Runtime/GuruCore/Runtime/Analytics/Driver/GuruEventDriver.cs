
namespace Guru
{
    using UnityEngine;
    public class GuruEventDriver: AbstractEventDriver
    {
        private string logTag => "[Guru]";
        
        protected override void FlushTrackingEvent(ITrackingEvent evt)
        {
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation

            var eventName = evt.EventName;
            GuruAnalytics.Instance.LogEvent(eventName, evt.Data, evt.Priority);
            Debug.Log($"{logTag} --- LogEvent: {evt}");
            AnalyticRecordManager.Instance.PushEvent(eventName, evt.Data, (int)evt.Priority, AnalyticSender.Guru);
        }
        
        /// <summary>
        /// 输出属性
        /// </summary>
        protected override void SetUserProperty(string key, string value)
        {
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            GuruAnalytics.Instance.SetUserProperty(key, value);
            Debug.Log($"{logTag} --- SetProperty: {key}:{value}");
            AnalyticRecordManager.Instance.PushProperty(key, value, AnalyticSender.Guru);
        }
        
        //---------------- 单独实现所有的独立属性打点 ------------------
        
        /// <summary>
        /// 设置用户ID
        /// </summary>
        protected override void ReportUid(string uid)
        {
            GuruAnalytics.Instance.SetUid(uid);
            Debug.Log($"{logTag} --- SetUid: {uid}");
            SetUserProperty(Analytics.PropertyUserID, uid);
        }

        /// <summary>
        /// 设置设备ID
        /// (Firebase, Guru)
        /// </summary>
        protected override void ReportDeviceId(string deviceId)
        {
            GuruAnalytics.Instance.SetDeviceId(deviceId);
            Debug.Log($"{logTag} --- ReportDeviceId: {deviceId}");
        }

        /// <summary>
        /// 设置 AdjustId
        /// (Firebase)
        /// </summary>
        protected override void ReportAdjustId(string adjustId)
        {
            GuruAnalytics.Instance.SetAdjustId(adjustId);
            Debug.Log($"{logTag} --- SetAdjustId: {adjustId}");
        }
        
        /// <summary>
        /// 设置 AppsflyerId
        /// (Firebase)
        /// </summary>
        protected override void ReportAppsflyerId(string appsflyerId)
        {
            GuruAnalytics.Instance.SetAppsflyerId(appsflyerId);
            Debug.Log($"{logTag} --- SetAppsflyerId: {appsflyerId}");
        }

        protected override void ReportAndroidId(string androidId)
        {
            GuruAnalytics.Instance.SetAndroidId(androidId);
            Debug.Log($"{logTag} --- ReportAndroidId: {androidId}");
        }

        /// <summary>
        /// 设置 AdId
        /// </summary>
        protected override void ReportGoogleAdId(string adId)
        {
            GuruAnalytics.Instance.SetAdId(adId);
            Debug.Log($"{logTag} --- SetAdId: {adId}");
        }

        /// <summary>
        /// 设置 IDFV
        /// </summary>
        protected override void ReportIDFV(string idfv)
        {
            GuruAnalytics.Instance.SetIDFV(idfv);
            Debug.Log($"{logTag} --- SetIDFV: {idfv}");
        }

        /// <summary>
        /// 设置 IDFA
        /// </summary>
        protected override void ReportIDFA(string idfa)
        {
            GuruAnalytics.Instance.SetIDFA(idfa);
            Debug.Log($"{logTag} --- SetIDFA: {idfa}");
        }
    }
}