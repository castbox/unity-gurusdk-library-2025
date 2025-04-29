
namespace Guru
{
    using Firebase.Analytics;
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// Firebase 专用
    /// </summary>
    internal class FirebaseEventDriver : AbstractEventDriver
    {
        private const string LOG_TAG = "[Firebase]";
        
        protected override void FlushTrackingEvent(ITrackingEvent evt)
        {
            var eventName = evt.EventName;
            var parameters = DictToParameters(evt.Data);
            Debug.Log($"{LOG_TAG} --- driver logEvent: {evt.EventName}");
            
            if (parameters != null)
            {
                FirebaseAnalytics.LogEvent(eventName, parameters);
            }
            else
            {
                FirebaseAnalytics.LogEvent(eventName);
            }
            AnalyticRecordManager.Instance.PushEvent(eventName, evt.Data, (int)evt.Priority, AnalyticSender.Firebase);
        }
        
        /// <summary>
        /// 字典转参数
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        private Parameter[] DictToParameters(Dictionary<string, object> dict)
        {
            if (dict == null) return null;
            
            List<Parameter> paramList = new List<Parameter>();
            foreach (var kv in dict)
            {
                if(kv.Value is string strValue)
                    paramList.Add(new Parameter(kv.Key, strValue));
                else if (kv.Value is bool boolValue)
                    paramList.Add(new Parameter(kv.Key, boolValue ? "true" : "false"));
                else if (kv.Value is int intValue)
                    paramList.Add(new Parameter(kv.Key, intValue));
                else if (kv.Value is long longValue)
                    paramList.Add(new Parameter(kv.Key, longValue));
                else if (kv.Value is float floatValue)
                    paramList.Add(new Parameter(kv.Key, floatValue));
                else if (kv.Value is double doubleValue)
                    paramList.Add(new Parameter(kv.Key, doubleValue));
                else if (kv.Value is decimal decimalValue)
                    paramList.Add(new Parameter(kv.Key, decimal.ToDouble(decimalValue)));
                else
                    paramList.Add(new Parameter(kv.Key, kv.Value.ToString()));
            }

            return paramList.ToArray();
        }



        /// <summary>
        /// 输出属性
        /// </summary>
        protected override void SetUserProperty(string key, string value)
        {
            Debug.Log($"{LOG_TAG} --- driver setUserProperty: {key}={value}");

            FirebaseAnalytics.SetUserProperty(key, value);
            AnalyticRecordManager.Instance.PushProperty(key, value, AnalyticSender.Firebase);
        }

        protected override void ReportUid(string uid)
        {
            FirebaseAnalytics.SetUserId(uid);
            SetUserProperty(Analytics.PropertyUserID, uid);
        }
        
        protected override void ReportDeviceId(string deviceId)
        {
        }
        
        protected override void ReportAdjustId(string adjustId)
        {
        }
        protected override void ReportAppsflyerId(string appsflyerId)
        {
        }
        
        protected override void ReportGoogleAdId(string adId)
        {
        }

        protected override void ReportAndroidId(string adId)
        {
        }

        protected override void ReportIDFV(string idfv)
        {
        }

        protected override void ReportIDFA(string idfa)
        {
        }
    }
}