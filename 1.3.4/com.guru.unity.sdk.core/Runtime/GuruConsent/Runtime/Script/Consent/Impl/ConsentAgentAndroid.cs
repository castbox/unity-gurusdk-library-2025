

namespace Guru
{
    using System;
    using UnityEngine;
    
    public class ConsentAgentAndroid: IConsentAgent
    {
        private static readonly string ConsentAndroidClassName = "com.guru.unity.consent.Consent";
        
        private string _objName;
        private string _callbackName;
#if UNITY_ANDROID
        private AndroidJavaClass _javaConsentClass;
        private bool _initSuccess = false;
#endif
        
        public void Init(string objectName, string callbackName)
        {
            _objName = objectName;
            _callbackName = callbackName;
#if UNITY_ANDROID
            _initSuccess = false;
            try
            {
                _javaConsentClass = new AndroidJavaClass(ConsentAndroidClassName);
                if (_javaConsentClass != null)
                {
                    _initSuccess = true;
                    // 绑定Unity回调物体
                    _javaConsentClass.CallStatic("initSDK", _objName, _callbackName);
                }
            }
            catch (Exception e)
            {
                GuruConsent.LogException(e);
            }
#endif
        }
        
        
        public void RequestGDPR(string deviceId = "", int debugGeography = -1)
        {
#if UNITY_ANDROID
            if (!_initSuccess) return;
            // request funding choices
            _javaConsentClass.CallStatic("requestGDPR", deviceId, debugGeography);
#endif
        }
        
        /// <summary>
        /// 获取 DMA 字段
        /// </summary>
        /// <returns></returns>
        public string GetPurposesValue()
        {
#if UNITY_ANDROID
            if (!_initSuccess) return "";
            return _javaConsentClass.CallStatic<string>("getDMAValue");      
#endif
            return "";
        }
    }
}