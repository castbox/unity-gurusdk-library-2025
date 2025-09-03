using System;
using Cysharp.Threading.Tasks;

namespace Guru
{
    /// <summary>
    /// 自定义事件驱动器
    /// </summary>
    public abstract class CustomEventDriver: AbstractEventDriver
    {
        // 数据设置和
        public abstract void Prepare(IGuruSDKApiProxy proxy);
        
        // 异步初始化
        public abstract UniTask InitializeAsync();

        // 发送事件
        protected abstract void SendEvent(ITrackingEvent evt);
        
        // 设置用户属性
        protected abstract void SendUserProperty(string key, string value);
        
        // 更新 Consent 事件
        public abstract void SetConsentData(ConsentData consentData);
        
        
        #region 接口实现

        protected override void FlushTrackingEvent(ITrackingEvent evt)
        {
            SendEvent(evt);
        }

        protected override void SetUserProperty(string key, string value)
        {
            SendUserProperty(key, value);
        }

        protected override void ReportUid(string uid)
        {
        }

        protected override void ReportDeviceId(string deviceId)
        {
        }

        protected override void ReportAdjustId(string adjustId)
        {
        }

        protected override void ReportAppsflyerId(string adjustId)
        {
        }

        protected override void ReportGoogleAdId(string googleAdId)
        {
        }

        protected override void ReportAndroidId(string androidId)
        {
        }

        protected override void ReportIDFV(string idfv)
        {
        }

        protected override void ReportIDFA(string idfa)
        {
            
        }

        #endregion
    }


    public interface IGuruSDKApiProxy
    {
        string UID { get; }
        string DeviceId { get; }
        string FirebaseId { get; }
        string FbAppId { get; }
        DateTime FirstOpenDate { get; }
        
        // 向全局全局的用户属性事件
        void ReportUserProperty(string key, string value);
        void ReportAppsFlyerId(string appsFlyerId);
        void ReportAdjustDeviceId(string adjustDeviceId);
        void ReportGoogleAdId(string googleAdId);
    }

}