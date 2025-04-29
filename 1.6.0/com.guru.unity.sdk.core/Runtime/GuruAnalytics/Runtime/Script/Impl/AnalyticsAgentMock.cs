

namespace Guru
{
    using System.Text;
    using UnityEngine;
    using System;
    
    public class AnalyticsAgentMock: IAnalyticsAgent
    {

        public static readonly string TAG = "[EDT]";

        private bool _isShowLog = false;
        private bool _isDebug = false;
        
        private bool _enableErrorLog;

        public bool EnableErrorLog
        {
            get => _enableErrorLog;
            set
            {
                Debug.Log($"{TAG} EnableErrorLog:<color=orange>{value}</color>");
                _enableErrorLog = value;
            }
        }
        
        private int _eventTotalCount = 0;
        private int _eventSuccessCount = 0;
        
        
        public void Init(string appId, string deviceInfo, string baseUrl, string[] uploadIpAddress, string guruSDKVersion,
            Action onInitComplete, bool isDebug = false)
        {
#if UNITY_EDITOR
            _isShowLog = true;
#endif
            _isDebug = isDebug;
          
            Debug.Log($"{TAG} Init {nameof(AnalyticsAgentMock)} with Debug:<color=orange>{isDebug}</color>  appId:{appId}  deviceInfo:{deviceInfo}  baseUrl:{baseUrl}  uploadIpAddress:{string.Join(",", uploadIpAddress ?? Array.Empty<string>())}    guruSDKVersion:{guruSDKVersion}");
            
            onInitComplete?.Invoke();
        }

        
        public void InitCallback(string objName, string method)
        {
            if(_isShowLog) 
                Debug.Log($"{TAG} InitCallback: <color=orange>object:{objName}  method:{method}</color>");   
        }
        
        
        public void SetScreen(string screenName)
        {
            if(_isShowLog) 
                Debug.Log($"{TAG} SetScreen: <color=orange>{screenName}</color>");
        }

        public void SetAdId(string id)
        {
            if(_isShowLog) 
                Debug.Log($"{TAG} SetAdId: <color=orange>{id}</color>");
        }

        public void SetUserProperty(string key, string value)
        {
            if(_isShowLog) 
                Debug.Log($"{TAG} SetUserProperty: <color=orange>{key} : {value}</color>");
        }

        public void SetFirebaseId(string id)
        {
            if(_isShowLog) 
                Debug.Log($"{TAG} SetFirebaseId: <color=orange>{id}</color>");
        }

        public void SetAdjustId(string id)
        {
            if(_isShowLog) 
                Debug.Log($"{TAG} SetAdjustId: <color=orange>{id}</color>");
        }
        
        public void SetAppsflyerId(string id)
        {
            if(_isShowLog) 
                Debug.Log($"{TAG} SetAppsflyerId: <color=orange>{id}</color>");
        }

        public void SetDeviceId(string deviceId)
        {
            if(_isShowLog) 
                Debug.Log($"{TAG} SetDeviceId: <color=orange>{deviceId}</color>");
        }

        public void SetUid(string uid)
        {
            if(_isShowLog) 
                Debug.Log($"{TAG} SetUid: <color=orange>{uid}</color>");
        }

        public bool IsDebug => _isDebug;
        

        public void LogEvent(string eventName, string parameters, int priority = 0)
        {
            if (_isShowLog)
            {
                var raw = parameters.Split(',');
                if (raw.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < raw.Length; i++)
                    {
                        var ss = raw[i];
                        if(string.IsNullOrEmpty(ss)) continue;
                        
                        var p = ss.Split(':');
                        if (p.Length > 0)
                        {
                            var key = p[0].Replace("\"", "").Replace("{", "").Replace("}", "");
                            string v = "";
                            if (p.Length > 1)
                            {
                                v = p[1].Replace("\"", "").Replace("{", "").Replace("}", "");
                                if (!string.IsNullOrEmpty(v) && v.Length > 1)
                                {
                                    var t = v.Substring(0, 1);
                                    int idx = 0;
                                    if (t == "i" || t == "d") idx = 1;
                                    // 字符串解析
                                    switch (t)
                                    {
                                        case "i":
                                            sb.Append($"<color=orange>{key} : [int] {v}</color>\n");
                                            break;
                                        case "d":
                                            sb.Append($"<color=orange>{key} : [double] {v}</color>\n");
                                            break;
                                        default:
                                            sb.Append($"<color=orange>{key} : [string] {v}</color>\n");
                                            break;

                                    }
                                }
                                else
                                {
                                    sb.Append($"<color=orange>{key} : [string] {v}</color>\n"); 
                                }
                            }
                            else
                            {
                                sb.Append($"<color=orange>{key} : [string] {v}</color>\n"); 
                            }
                        }
                    }
                    
                    Debug.Log($"{TAG} LogEvent: GuruAnalytics:<color=orange>{eventName} ({priority})</color>  Properties:\n{sb.ToString()}");
                }
            }

            _eventTotalCount++;
            if (UnityEngine.Random.Range(0, 10) < 8)
            {
                _eventSuccessCount++;
            }
        }
        
        public void ReportEventSuccessRate()
        {
            Debug.Log($"{TAG} Log Event Success Rate");
        }
        
        public void SetTch02Value(double value)
        {
            Debug.Log($"{TAG} Tch02MaxValue: {value}");
        }

        public int GetEventCountTotal() => _eventTotalCount;
        public int GetEventCountUploaded() => _eventSuccessCount;
        public string GetAuditSnapshot() => "";

        #region Editor Test API




        #endregion

    }
}