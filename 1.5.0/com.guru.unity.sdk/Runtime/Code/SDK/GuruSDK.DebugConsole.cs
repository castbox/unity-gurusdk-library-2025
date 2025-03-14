

namespace Guru
{
    
    using System;
    using Guru.Ads;

    
    public partial class GuruSDK
    {
               #region 控制台
#if GURU_DEBUG_CONSOLE
        /// <summary>
        /// 显示控制台
        /// </summary>
        /// <param name="popupEnabled">是否显示弹窗关闭后的 popup 小窗（可再次唤起显示 Console）</param>
        public static void ShowDebugConsole()
        {
            if (!UnityEngine.Debug.unityLogger.logEnabled)
            {
                UnityEngine.Debug.unityLogger.logEnabled = true; // 打开日志显示
            }
            Guru.Debug.GuruDebugConsole.Instance.ShowConsole();
        }
        /// <summary>
        /// 添加控制台命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="description"></param>
        /// <param name="onCommandExecuted"></param>
        public static void AddDebugConsoleCommand(string command, string description, Action onCommandExecuted)
        {
            Guru.Debug.GuruDebugConsole.Instance.AddCommand(command, description, onCommandExecuted);
        }
        /// <summary>
        /// 添加控制台命令 参数 1
        /// </summary>
        /// <param name="command"></param>
        /// <param name="description"></param>
        /// <param name="onCommandExecuted"></param>
        /// <typeparam name="T1"></typeparam>
        public static void AddDebugConsoleCommand<T1>(string command, string description, Action<T1> onCommandExecuted)
        {
            Guru.Debug.GuruDebugConsole.Instance.AddCommand(command, description, onCommandExecuted);
        }

        /// <summary>
        /// 添加控制台命令 参数 2
        /// </summary>
        /// <param name="command"></param>
        /// <param name="description"></param>
        /// <param name="onCommandExecuted"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        public static void AddDebugConsoleCommand<T1,T2>(string command, string description, Action<T1,T2> onCommandExecuted)
        {
            Guru.Debug.GuruDebugConsole.Instance.AddCommand(command, description, onCommandExecuted);
        }

        /// <summary>
        /// 添加控制台命令 参数 3
        /// </summary>
        /// <param name="command"></param>
        /// <param name="description"></param>
        /// <param name="onCommandExecuted"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        public static void AddDebugConsoleCommand<T1,T2,T3>(string command, string description, Action<T1,T2,T3> onCommandExecuted)
        {
            Guru.Debug.GuruDebugConsole.Instance.AddCommand(command, description, onCommandExecuted);
        }


        /// <summary>
        /// 添加 Guru 默认的命令
        /// </summary>
        private static void AddGuruCommand()
        {
            // ---------------- Add Adjust Command ---------------- 
            AddDebugConsoleCommand("adjust", "Show Adjust Debug Info", () =>
            {
                Guru.Debug.GuruDebugConsole.Instance.SetSearchKeyword(AdjustService.LOG_TAG);
            });
            
            AddDebugConsoleCommand("guru", "Show SDK Debug Info", () =>
            {
                Guru.Debug.GuruDebugConsole.Instance.SetSearchKeyword(GuruSDK.LOG_TAG);
            });
            
            AddDebugConsoleCommand("ads", "Show Ads Debug Info", () =>
            {
                Guru.Debug.GuruDebugConsole.Instance.SetSearchKeyword(AdService.LOG_TAG);
            });
            
            AddDebugConsoleCommand("max", "Show MAX Debug Info", () =>
            {
                Guru.Debug.GuruDebugConsole.Instance.SetSearchKeyword(AdService.Instance.GetMediationLogTag());
            });
            
            AddDebugConsoleCommand("fb", "Show Facebook Debug Info", () =>
            {
                Guru.Debug.GuruDebugConsole.Instance.SetSearchKeyword(FBService.LOG_TAG);
            });
            
            AddDebugConsoleCommand("firebase", "Show Firebase Debug Info", () =>
            {
                Guru.Debug.GuruDebugConsole.Instance.SetSearchKeyword(FirebaseUtil.LOG_TAG);
            });
            
            AddDebugConsoleCommand("loadrv", "Force to load RV", () =>
            {
                UnityEngine.Debug.Log($"{LOG_TAG} --- Force call load RV");
                LoadRewardAd();
            });
            
            AddDebugConsoleCommand("loadiv", "Force to load IV", () =>
            {
                UnityEngine.Debug.Log($"{LOG_TAG} --- Force call load IV");
                LoadInterstitialAd();
            });
            
            AddDebugConsoleCommand("lttest", "Turn on LT debug mode", () =>
            {
                UnityEngine.Debug.Log($"{LOG_TAG} --- LT debug mode: On");
                Instance.SetLTDebugMode(true);
            });
            
            AddDebugConsoleCommand("lttestoff", "Turn off LT debug mode", () =>
            {
                UnityEngine.Debug.Log($"{LOG_TAG} --- LT debug mode: Off");
                Instance.SetLTDebugMode(false);
            });
            
            AddDebugConsoleCommand("clearuid", "Clear uid debug mode", () =>
            {
                UnityEngine.Debug.Log($"{LOG_TAG} ---clear uid");
                IPMConfig.DebugClearUID();
            });
            
            AddDebugConsoleCommand("clearidfa", "Clear idfa debug mode", () =>
            {
                UnityEngine.Debug.Log($"{LOG_TAG} ---clear idfa");
                DebugClearIdfa();
            });
            
            AddDebugConsoleCommand("clearidfv", "Clear idfv debug mode", () =>
            {
                UnityEngine.Debug.Log($"{LOG_TAG} ---clear idfv");
                DebugClearIdfv();
            });
            
            AddDebugConsoleCommand("clearadjustid", "Clear adjust id debug mode", () =>
            {
                UnityEngine.Debug.Log($"{LOG_TAG} ---clear adjust id");
                IPMConfig.DebugClearAdjustId();
            });
            
            AddDebugConsoleCommand("cleargaid", "Clear gaid debug mode", () =>
            {
                UnityEngine.Debug.Log($"{LOG_TAG} ---clear gaid");
                IPMConfig.DebugClearGoogleAdid();
            });
            
            AddDebugConsoleCommand("clearfirebaseid", "Clear firebase id debug mode", () =>
            {
                UnityEngine.Debug.Log($"{LOG_TAG} ---clear firebase id");
                IPMConfig.DebugClearFirebaseId();
            });
        }


        private static void PrintSystemInfoOnConsole()
        {
            Guru.Debug.GuruDebugConsole.Instance.SetSearchKeyword("");
            Guru.Debug.GuruDebugConsole.Instance.PrintSystemInfo();
        }


        private static void ClearLogs()
        {
            Guru.Debug.GuruDebugConsole.Instance.ClearLogs();
        }
        
        /// <summary>
        /// 测试发送 tch02 事件
        /// </summary>
        private static void DebugSendTch001Event()
        {
            AdService.Instance.SetTch001Revenue(0.01);
        }
        private static void DebugSendTch02Event()
        {
            AdService.Instance.SetTch02Revenue(0.2);
        }

#endif  
        #endregion
    }
}