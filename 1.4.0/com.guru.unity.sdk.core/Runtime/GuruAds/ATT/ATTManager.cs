#if UNITY_IOS

namespace Guru
{
    using UnityEngine;
    using System;
    using Unity.Advertisement.IosSupport;
    
    public class ATTManager
    {
        public const string Version = "1.1.0";

        public const string ATT_STATUS_AUTHORIZED = "authorized";
        public const string ATT_STATUS_DENIED = "denied";
        public const string ATT_STATUS_RESTRICTED = "restricted";
        public const string ATT_STATUS_NOT_DETERMINED = "notDetermined";
        public const string ATT_STATUS_NOT_APPLICABLE = "notApplicable";
        public const int ATT_REQUIRED_MIN_OS = 14;
        
        //----------  引导类型 ------------
        public const string GUIDE_TYPE_ADMOB = "admob";  // 中台默认使用 Google FundingChoices 作为 Consent 引导， 记作 Admob
        public const string GUIDE_TYPE_CUSTOM = "custom";
        public const string GUIDE_TYPE_MAX = "max";
        private ATTrackingStatusBinding.AuthorizationTrackingStatus _attBeginStatus;
        
        private static ATTManager _instance;
        public static ATTManager Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new ATTManager();
                }

                return _instance;
            }
        }

        private Action _onCheckComplete;
        private bool _autoReCallAtt = false;
        private string _attType = "";
        
        // 构造函数
        private ATTManager()
        {
            
        }

        /// <summary>
        /// 设置初始化数据
        /// </summary>
        /// <param name="attType"></param>
        public void SetInitAttGuidType(string attType)
        {
            _attType = attType;
            // 初始化的时候获取一下状态
            _attBeginStatus = GetAttAuthStatus();
            SetAttStatus(ToStatusString(_attBeginStatus));
        }


        /// <summary>
        /// 请求系统弹窗
        /// </summary>
        public void RequestATTDialog(Action<string> callback = null)
        {
            if (!IsATTSupported())
            {
                callback?.Invoke(ATT_STATUS_NOT_APPLICABLE); //  不支持
                return;
            }
            
            ATTrackingStatusBinding.RequestAuthorizationTracking(value =>{
                callback?.Invoke(ToStatusString(value));
            });
        }

        /// <summary>
        /// 启动时检查状态
        /// </summary>
        /// <param name="onComplete"></param>
        /// <param name="autoReCall"></param>
        /// <param name="attType"></param>
        public void CheckStatus(Action onComplete = null, bool autoReCall = false, string attType = GUIDE_TYPE_ADMOB)
        {
            _onCheckComplete = onComplete;
            _autoReCallAtt = autoReCall;
            _attType = attType;
            
            if (!IsATTSupported())
            {
                SetAttStatus(ATT_STATUS_NOT_APPLICABLE);
                _onCheckComplete?.Invoke(); //  不支持
                return;
            }
            
            CoroutineHelper.Instance.StartDelayed(1.0f, DelayCheckAttStatus);
        }
        


        private ATTrackingStatusBinding.AuthorizationTrackingStatus GetAttAuthStatus()
        {
            return ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        /// <returns></returns>
        public string GetAttStatusString()
        {
            if (!IsATTSupported()) return ATT_STATUS_NOT_APPLICABLE;
            var status = ToStatusString(ATTrackingStatusBinding.GetAuthorizationTrackingStatus());
            if(!string.IsNullOrEmpty(status)) return status;
            return ATT_STATUS_NOT_APPLICABLE;
        }

        /// <summary>
        /// 转字符串
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private string ToStatusString(ATTrackingStatusBinding.AuthorizationTrackingStatus status)
        {
            switch (status)
            {
                case ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED:
                    return ATT_STATUS_NOT_DETERMINED; // 未选择
                case ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED:
                    return ATT_STATUS_AUTHORIZED; // 已选择
                case ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED:
                    return ATT_STATUS_DENIED; // 已拒绝
                case ATTrackingStatusBinding.AuthorizationTrackingStatus.RESTRICTED:
                    return ATT_STATUS_RESTRICTED; // 已关闭
            }
            return ATT_STATUS_RESTRICTED;
        }

        /// <summary>
        /// 状态码转字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ToStatusString(int value) 
            => ToStatusString((ATTrackingStatusBinding.AuthorizationTrackingStatus)value);
        
        /// <summary>
        /// 是否支持ATT
        /// </summary>
        /// <returns></returns>
        private static bool IsATTSupported()
        {
            string version = UnityEngine.iOS.Device.systemVersion;
            
            // Debug.Log($"[ATT] --- Get iOS system version: {version}");

            string tmp = version;
            if (version.Contains(" "))
            {
                var a1 = version.Split(' ');
                tmp = a1[a1.Length - 1];
            }

            string num = tmp;
            if (tmp.Contains("."))
            {
                num = tmp.Split('.')[0];
            }
            
            if (int.TryParse(num, out var ver))
            {
                if (ver >= ATT_REQUIRED_MIN_OS) return true;
            }

            return false;
        }
        
        


        private void DelayCheckAttStatus()
        {
            var status = GetAttAuthStatus(); // 延迟后的状态
            var result = ToStatusString(status);
            SetAttStatus(result);
            
            // 判断用户数是否点击了 Att Dialog
            if (_attBeginStatus == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED
                && status != _attBeginStatus)
            {
             
                // 起始状态为 "未决定"， 之后状态发生变化，则判定为弹窗出现，用户做出了选择
                ReportAttResultEvent(result, _attType);
                SetAttStatus(result);
                _onCheckComplete?.Invoke();
                return;
            }

            // 判断是否需要重新拉起 Att Dialog
            if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED
                && _autoReCallAtt)
            {
                // 起始状态为 "未决定"，未发生变化，且用户未改变
                RequestATTDialog(str =>
                {
                    if (str != ATT_STATUS_NOT_DETERMINED)
                    {
                        ReportAttResultEvent(str, _attType);
                    }
                    _onCheckComplete?.Invoke();
                });
            }

        }



        private void ReportAttResultEvent(string result = "", string type = "")
        {
            if (string.IsNullOrEmpty(type)) type = _attType;
            if (string.IsNullOrEmpty(result)) result = GetAttStatusString();
            Analytics.AttResult(result, _attType);
        }

        private void SetAttStatus(string status)
        {
            Analytics.SetAttStatus(status);
        }

    
        
    }


}

#endif