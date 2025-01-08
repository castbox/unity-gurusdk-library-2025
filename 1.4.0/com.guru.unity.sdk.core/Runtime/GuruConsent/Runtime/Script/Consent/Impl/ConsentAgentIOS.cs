namespace Guru
{
#if UNITY_IOS   
    using System.Runtime.InteropServices;
#endif
    
    public class ConsentAgentIOS:IConsentAgent
    {
        private const string DLL_INTERNAL = "__Internal";
#if UNITY_IOS
        [DllImport(DLL_INTERNAL)]
        private static extern void unityRequestGDPR(string deviceId, int debugGeography); // IOS 调用接口
        [DllImport(DLL_INTERNAL)]
        private static extern void unityInitSDK(string gameobject, string callback); // IOS 调用接口
        [DllImport(DLL_INTERNAL)]
        private static extern string unityGetTCFValue(); // 获取 TFC 值
        [DllImport(DLL_INTERNAL)]
        private static extern string unityGetRegionCode(); // 获取 RegionCode 值
#endif

        private string _objName;
        private string _callbackName;
        
        public void Init(string objectName, string callbackName)
        {
            _objName = objectName;
            _callbackName = callbackName;
        }
        
        
        /// <summary>
        /// 调用请求
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="debugGeography"></param>
        public void RequestGDPR(string deviceId = "", int debugGeography = -1)
        {
#if UNITY_IOS
            string goName = _objName;
            string cbName = _callbackName;
            // UnityEngine.Debug.Log($"[U3D] init SDK -> {goName}:{cbName}");
            unityInitSDK(goName, cbName);
            unityRequestGDPR(deviceId, debugGeography);
#endif        
        }
        
        
        /// <summary>
        /// 获取 DMA 字段
        /// </summary>
        /// <returns></returns>
        public string GetPurposesValue()
        {
#if UNITY_IOS
            return unityGetTCFValue();
#endif
            return "";
        }


        public static string GetRegionCode()
        {
#if UNITY_IOS
            return unityGetRegionCode();
#endif
            return "";
        }
    }
}