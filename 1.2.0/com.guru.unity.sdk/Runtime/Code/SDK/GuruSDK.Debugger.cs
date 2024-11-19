
namespace Guru
{
    using UnityEngine;
    using System;
    
    public partial class GuruSDK
    {
        private static readonly bool _useBaseOptions = true;

        private static GuruDebugger _debugger;

        public static GuruDebugger Debugger
        {
            get
            {
                if (_debugger == null)
                {
                    _debugger = GuruDebugger.Instance;
                    if (_useBaseOptions)
                    {
                        InitDebuggerLayout();
                    }
                }
                return _debugger;
            }
        }
        
        /// <summary>
        /// 显示广告状态
        /// </summary>
        public static bool ShowAdStatus()
        {
            if (!IsServiceReady) return false;
            
            Debugger.ShowAdStatus();
            return true;
        }
        
        /// <summary>
        /// 显示 Debugger
        /// </summary>
        /// <returns></returns>
        public static bool ShowDebugger()
        {
            if (!IsServiceReady) return false;
            
            Debugger.ShowPage(); // 显示 Debugger 界面
            return true;
        }

        private static void InitDebuggerLayout()
        {
            // ------------ Info Page --------------------
            Debugger.AddOption("Info/Guru SDK", ()=>GuruSDK.Version);
            Debugger.AddOption("Info/Unity Version", ()=>Application.unityVersion);
            Debugger.AddOption("Info/Name", ()=> GuruSettings.Instance.ProductName);
            Debugger.AddOption("Info/Bundle Id", ()=> GuruSettings.Instance.GameIdentifier);
            Debugger.AddOption("Info/Version", () =>
            {
                var v = GuruAppVersion.Load();
                return (v == null ? $"{Application.version} (unknown)" : $"{v.version} ({v.code})");
            });
            Debugger.AddOption("Info/Uid", ()=>(string.IsNullOrEmpty(UID) ? "NULL" : UID)).AddCopyButton();
            Debugger.AddOption("Info/Device ID", ()=>(string.IsNullOrEmpty(DeviceId) ? "NULL" : DeviceId)).AddCopyButton();
            Debugger.AddOption("Info/Push Token", ()=>(string.IsNullOrEmpty(PushToken) ? "NULL" : PushToken)).AddCopyButton();
            Debugger.AddOption("Info/Auth Token", ()=>(string.IsNullOrEmpty(AuthToken) ? "NULL" : AuthToken)).AddCopyButton();
            Debugger.AddOption("Info/Firebase Id", ()=>(string.IsNullOrEmpty(FirebaseId) ? "NULL" : FirebaseId)).AddCopyButton();
            Debugger.AddOption("Info/Adjust Id", ()=>(string.IsNullOrEmpty(AdjustId) ? "NULL" : AdjustId)).AddCopyButton();
            Debugger.AddOption("Info/IDFA", ()=>(string.IsNullOrEmpty(IDFA) ? "NULL" : IDFA)).AddCopyButton();
            Debugger.AddOption("Info/GSADID", ()=>(string.IsNullOrEmpty(GSADID) ? "NULL" : GSADID)).AddCopyButton();
            Debugger.AddOption("Info/Debug Mode", ()=>GuruSDK.DebugModeEnabled? "true" : "false");
            Debugger.AddOption("Info/Screen size", ()=>$"{Screen.width} x {Screen.height}");
            Debugger.AddOption("Info/Boost Duration", ()=>$"{BoostDuration:F2}(s)");
            
            // ------------ Ads Page --------------------
            Debugger.AddOption("Ads/Show Ads Mediation Debugger", "", ShowMaxMediationDebugger);
            Debugger.AddOption("Ads/Show Ads Creative Debugger", "", ShowMaxCreativeDebugger);
            Debugger.AddOption("Ads/Banner Id", ()=> GuruSettings.Instance.ADSetting.GetBannerID());
            Debugger.AddOption("Ads/Interstitial Id", ()=> GuruSettings.Instance.ADSetting.GetInterstitialID());
            Debugger.AddOption("Ads/Rewarded Id", ()=> GuruSettings.Instance.ADSetting.GetRewardedVideoID());

            // ------------ Console Page -------------------
            if (!UnityEngine.Debug.unityLogger.logEnabled)
            {
                Debugger.AddOption("Console/Log").AddButton("显示日志", () =>
                {
                    UnityEngine.Debug.unityLogger.logEnabled = true;
                });
            }
#if GURU_DEBUG_CONSOLE
        
            Debugger.AddOption("Console/Console").AddButton("显示控制台", () =>
            {
                ShowDebugConsole();
                Debugger.Close();
            });
            
            Debugger.AddOption("Console/Show System Info").AddButton("打印系统信息", PrintSystemInfoOnConsole);
            Debugger.AddOption("Console/Clear Logs").AddButton("清除所有", ClearLogs);
            Debugger.AddOption("Console/Event: Adjust").AddButton("只显示 [Adjust] 事件", ()=> Guru.Debug.GuruDebugConsole.Instance.SetSearchKeyword("[Adjust]"));
            Debugger.AddOption("Console/Event: Firebase").AddButton("只显示 [Firebase] 事件", ()=> Guru.Debug.GuruDebugConsole.Instance.SetSearchKeyword("[Firebase]"));
            Debugger.AddOption("Console/Event: Facebook").AddButton("只显示 [Facebook] 事件", ()=> Guru.Debug.GuruDebugConsole.Instance.SetSearchKeyword("[FB]"));
            Debugger.AddOption("Console/Event: Guru").AddButton("只显示 [自打点] 事件", ()=> Guru.Debug.GuruDebugConsole.Instance.SetSearchKeyword("[Guru]"));
            Debugger.AddOption("Console/Event: Test Tch02").AddButton("测试 [Tch02] 打点", DebugSendTch02Event);
            
            AddGuruCommand();
#endif
            
            GuruDebugger.OnClosed -= OnDebuggerClosed;
            GuruDebugger.OnClosed += OnDebuggerClosed;
            Callbacks.SDK.InvokeOnDebuggerDisplayed(true);
        }
        
        private static void OnDebuggerClosed()
        {
            GuruDebugger.OnClosed -= OnDebuggerClosed;
            Callbacks.SDK.InvokeOnDebuggerDisplayed(false);
        }
        
        public static GuruDebugger GetGuruDebugger() => GuruDebugger.Instance;
        
        
        /// <summary>
        /// 添加显示
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="contentDel"></param>
        /// <param name="clickHandler"></param>
        /// <returns></returns>
        public static GuruDebugger.OptionLayout AddOption(string uri, GuruDebugger.GetOptionContentDelegate contentDel = null, Action clickHandler = null)
        {
            return Debugger.AddOption(uri, contentDel, clickHandler);
        }


    }
}