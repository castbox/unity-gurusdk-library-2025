namespace Guru
{
    using UnityEngine;
    
    public class FBEventDriver: AbstractEventDriver
    {
        private string logTag => "[FB]";
        
        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="evt"></param>
        protected override void FlushTrackingEvent(ITrackingEvent evt)
        {
            var eventName = evt.EventName;
            var data = evt.Data;

            Debug.Log($"{logTag} --- Track FB Event: {eventName}");
            
            switch (evt)
            {
                case IFBSpentCreditsEvent scEvent:
                    // 额外上报SpendCredits 事件
                    FBService.LogSpendCredits(scEvent.Value, scEvent.ContentID, scEvent.ContentType);
                    Debug.Log($"{logTag} --- SpendCredits Event: {eventName} Value: {scEvent.Value} ContentID: {scEvent.ContentID} ContentType: {scEvent.ContentType}");
                    AnalyticRecordManager.Instance.PushEvent(scEvent, evt.Priority);
                    break;
                case IFBPurchaseEvent purchaseEvent:
                    // 额外上报Purchase 事件
                    FBService.LogPurchase(purchaseEvent.Value, purchaseEvent.Currency, evt.Data);
                    Debug.Log($"{logTag} --- Purchase Event: {eventName} Value: {purchaseEvent.Value} Currency: {purchaseEvent.Currency}");
                    AnalyticRecordManager.Instance.PushEvent(evt, AnalyticSender.Facebook);
                    break;
                default:
                    // 通常事件
                    FBService.LogEvent(eventName, null, data);
                    Debug.Log($"{logTag} --- Normal Event: {eventName}");
                    AnalyticRecordManager.Instance.PushEvent(evt.EventName, evt.Data, (int)evt.Priority, AnalyticSender.Facebook);
                    break;
            }
        }
        
        protected override void SetUserProperty(string key, string value)
        {
            
        }
        
        //---------------- 单独实现所有的独立属性打点 ------------------
        
        /// <summary>
        /// 设置用户ID
        /// </summary>
        protected override void ReportUid(string uid)
        {
            
        }
        protected override void ReportDeviceId(string deviceId)
        {
            
        }
        
        /// <summary>
        /// 设置 AdjustId
        /// (Firebase)
        /// </summary>
        protected override void ReportAdjustId(string adjustId)
        {
            
        }
        
        /// <summary>
        /// 设置 AdId
        /// </summary>
        protected override void ReportGoogleAdId(string adId)
        {
            
        }

        protected override void ReportAndroidId(string adId)
        {
            
        }

        /// <summary>
        /// 设置 IDFV
        /// </summary>
        protected override void ReportIDFV(string idfv)
        {
            
        }

        /// <summary>
        /// 设置 IDFA
        /// </summary>
        protected override void ReportIDFA(string idfa)
        {
            
        }
    }
}