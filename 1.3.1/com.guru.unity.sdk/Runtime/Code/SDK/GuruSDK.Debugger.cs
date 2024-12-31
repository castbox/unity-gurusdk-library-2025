
using System.Collections.Generic;

namespace Guru
{
    using UnityEngine;
    using System;
    
    public partial class GuruSDK
    {
        private static readonly bool _useBaseOptions = true;

        private const string TAB_NAME_REMOTE = "Remote";
        
        private static GuruDebugger _debugger;
        private static GuruDebugger Debugger
        {
            get
            {
                if (_debugger == null)
                {
                    _debugger = GuruDebugger.Instance;
                    if (_useBaseOptions)
                    {
                        InitBaseOptionLayout();
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
            if (!IsServiceReady) {return false;}
            
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
        
        
        
        

        private static void InitBaseOptionLayout()
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

            // ------------ Log Enabled -------------------
            if (!UnityEngine.Debug.unityLogger.logEnabled)
            {
                Debugger.AddOption("Console/Log").AddButton("显示日志", () =>
                {
                    UnityEngine.Debug.unityLogger.logEnabled = true;
                });
            }
            
#if GURU_DEBUG_CONSOLE
            // ------------ Console Page -------------------
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
            
            // ------------ RemoteConfig Page -------------------
            // 显示 Debug 内的信息：
            DebuggerSetRemoteConfigValues(GetAllConfigValues());
            
            // ------------ Vibration Page -------------------
            DebugVibrationSection();
            
            // ------------ Callbacks ------------------
            GuruDebugger.BeforeTabPageRender -= BeforeTabPageRender;
            GuruDebugger.BeforeTabPageRender += BeforeTabPageRender;
            
            GuruDebugger.OnClosed -= OnDebuggerClosed;
            GuruDebugger.OnClosed += OnDebuggerClosed;
            Callbacks.SDK.InvokeOnDebuggerDisplayed(true);


        }
        
        /// <summary>
        /// Debugger 关闭回调逻辑 
        /// </summary>
        private static void OnDebuggerClosed()
        {
            GuruDebugger.OnClosed -= OnDebuggerClosed;
            Callbacks.SDK.InvokeOnDebuggerDisplayed(false);
        }
        
        public static GuruDebugger GetGuruDebugger() => GuruDebugger.Instance;

        /// <summary>
        /// Tab 页面渲染前插入操作逻辑
        /// </summary>
        /// <param name="tabName"></param>
        private static void BeforeTabPageRender(string tabName)
        {
            if (tabName == TAB_NAME_REMOTE)
            {
                DebuggerSetRemoteConfigValues(GetAllConfigValues());
            }
        }
        
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
        
        #region Remote Configs
        
        /// <summary>
        /// 显示所有的配置页面
        /// </summary>
        /// <param name="configValues"></param>
        private static void DebuggerSetRemoteConfigValues(Dictionary<string, GuruConfigValue> configValues)
        {
            Debugger.DeleteTable(TAB_NAME_REMOTE);
            
            Debugger.AddOption($"{TAB_NAME_REMOTE}/Fetch").AddButton("Fetch All", () => { FetchAllRemote();});
            foreach (var (key, value)  in configValues)
            {
                
                var source = value.Source.ToString();
                var color = value.Source switch
                {
                    ValueSource.Default =>  "#666666",
                    ValueSource.Local =>  "yellow",
                    ValueSource.Remote =>  "#88FF00",
                    _ => "white"
                };
                var uri = $"{TAB_NAME_REMOTE}/{key}\n<size=10><color={color}>{source}</color></size>";
                Debugger.AddOption(uri, () => $"{value.Value}");
            }
        }

        #endregion
        
        #region Vibration

        /// <summary>
        /// 显示震动调试界面
        /// </summary>
        private static void DebugVibrationSection()
        {
            // --- 震动能力显示 ---
            var txt_yes = "<color=88ff00>是</color>";
            var txt_no = "<color=red>否</color>";
            var supportVibrate = HasVibrationCapability();
            var supportCustom = SupportsCustomVibration();
            var label = $"Vibration/支持 [震动]";
            Debugger.AddOption(label, () => HasVibrationCapability() ? txt_yes : txt_no);
            label = $"Vibration/支持 [振幅调整]";
            Debugger.AddOption(label, () => SupportsAmplitudeControl() ? txt_yes : txt_no);
            label = $"Vibration/支持 [震动特效]";
            Debugger.AddOption(label, () => SupportsVibrationEffect() ? txt_yes : txt_no);
            label = $"Vibration/支持 [自定义震动]";
            Debugger.AddOption(label, () => supportCustom ? txt_yes : txt_no);
            
            if (supportVibrate)
            {
                // --- 震动测试按钮 ---
                Debugger.AddOption("Vibration/Light").AddButton("Light", () => Vibrate(VibrateType.Light));
                Debugger.AddOption("Vibration/Medium").AddButton("Medium", () => Vibrate(VibrateType.Medium));
                Debugger.AddOption("Vibration/Heavy").AddButton("Heavy", () => Vibrate(VibrateType.Heavy));
                Debugger.AddOption("Vibration/Double").AddButton("Double", () => Vibrate(VibrateType.Double));
                Debugger.AddOption("Vibration/Selection").AddButton("Selection", () => Vibrate(VibrateType.Selection));
            }
            else
            {
                Debugger.AddOption("Vibration/测试 [震动能力]", () => "<color=yellow>设备不支持震动，无测试项目</color>");
            }

            if (supportCustom)
            {
                // Debugger.AddOption("Vibration/测试[自定义参数]", () => "<color=#666666>暂无测试项目</color>"); 
            }
            else
            {
                Debugger.AddOption("Vibration/测试 [自定义参数]", () => "<color=yellow>设备不支持自定义参数，无测试项目</color>");
            }
        }

        
        
        
        
        #endregion
        
    }
}