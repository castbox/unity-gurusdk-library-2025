

using System.Threading.Tasks;

namespace Guru
{
    using UnityEngine;
    
    
    public class ConsentAgentStub: IConsentAgent
    {

        private static readonly string Tag = "[Stub]";
        public static int DebugStatusCode { get; set; } = GuruConsent.StatusCode.OBTAINED;
        public static string DebugMessage { get; set; } = "You have already obtained the consent.";

        private static readonly string callbackMsgFmt = @"{""action"":""gdpr"", ""data"": {""status"":$0, ""msg"":""$1"" }}";
        
        
        #region 接口实现

        private string _objName;
        private string _callbackName;
        public void Init(string objectName, string callbackName)
        {
            _objName = objectName;
            _callbackName = callbackName;
        }
        
        public void RequestGDPR(string deviceId = "", int debugGeography = -1)
        {
            Debug.Log($"{Tag} Consent Request -> deviceid: {deviceId} debugGeography: {debugGeography}");

#if UNITY_EDITOR
            SendEditorCallback();
#endif
        }
        
#if UNITY_EDITOR
        
#endif

        /// <summary>
        /// 延迟触发 Consent
        /// </summary>
        private void SendEditorCallback()
        {
            CoroutineHelper.Instance.StartDelayed(2.0f, () =>
            {
                string msg = callbackMsgFmt.Replace("$0", $"{DebugStatusCode}").Replace("$1",DebugMessage);
                var go = GameObject.Find(_objName);
                if (go != null)
                {
                    go.SendMessage(_callbackName, msg);
                }
                else
                {
                    Debug.LogError($"{Tag} Can't find callback object");
                }
            });
        }



        /// <summary>
        /// 获取 DMA 字段
        /// </summary>
        /// <returns></returns>
        public string GetPurposesValue()
        {
            return "";
        }

        #endregion

       
    }
}