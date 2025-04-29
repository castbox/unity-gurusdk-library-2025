
namespace Guru.Debug
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using IngameDebugConsole;
    using Object = UnityEngine.Object;

    /// <summary>
    /// 游戏内 Debug 控制台
    /// </summary>
    public class GuruDebugConsole
    {
        private const string LOG_TAG = "[Console]";
        private const string PREFAB_PATH = "Debug/IngameDebugConsole";

        private static GuruDebugConsole _instance;
        public static GuruDebugConsole Instance
        {
            get
            {
                if (_instance == null) _instance = new GuruDebugConsole();
                return _instance;
            }
        }

        private readonly GameObject _consoleObject;
        private readonly InputField _searchInput;
        private readonly CanvasGroup _consoleCG;

        private bool IsConsoleActive
        {
            get
            {
                if (_consoleCG != null) return _consoleCG.alpha > 0;
                return false;
            }
        }

        private GuruDebugConsole()
        {
            var prefab = Resources.Load(PREFAB_PATH) as GameObject;
            if (prefab == null)
            {
                Debug.LogError("[Debug] Can't find IngameDebugConsole prefab!!");
                return;
            }

            _consoleObject = GameObject.Instantiate(prefab);
            _consoleObject.name = "__ingame_debug_console__";
            Object.DontDestroyOnLoad(_consoleObject);
            // _consoleObject.SetActive(false);
            
            var logWindow = _consoleObject.transform.Find("DebugLogWindow");

            if (logWindow == null)
            {
                Debug.LogWarning($"{LOG_TAG} --- Can't found logWindow!!");
                return;
            }

            _consoleCG = logWindow.GetComponent<CanvasGroup>();
            
            var t = _consoleCG.transform.Find("Buttons/SearchbarSlotTop/Searchbar");
            if (t != null)
            {
                _searchInput = t.GetComponent<InputField>();
            }
        }

        /// <summary>
        /// 显示 InGameConsole 弹窗
        /// </summary>
        /// <param name="popupEnabled">是否显示弹窗关闭后的 popup 小窗（可再次唤起显示 Console）</param>
        public void ShowConsole()
        {
            if (_consoleObject == null) return;
            if (!_consoleObject.activeSelf)
            {
                _consoleObject.SetActive(true);
            }
            DebugLogManager.Instance.ShowLogWindow(); // 显示 Log 窗
            // DebugLogManager.Instance.PopupEnabled = popupEnabled;
        }

        public void HideConsole()
        {
            if (_consoleObject == null) return;
            _consoleObject.SetActive(false);
            DebugLogManager.Instance.HideLogWindow();
        }


        /// <summary>
        /// 添加执行命令和回调 Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="description"></param>
        /// <param name="onCommandExecuted"></param>
        public void AddCommand(string command, string description, Action onCommandExecuted)
        {
            DebugLogConsole.AddCommand(command, description, onCommandExecuted);
        }
        
        /// <summary>
        /// 添加执行命令和回调 Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="description"></param>
        /// <param name="onCommandExecuted"></param>
        /// <typeparam name="T1"></typeparam>
        public void AddCommand<T1>(string command, string description, Action<T1> onCommandExecuted)
        {
            DebugLogConsole.AddCommand(command, description, onCommandExecuted);
        }
        
        /// <summary>
        /// 添加执行命令和回调 Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="description"></param>
        /// <param name="onCommandExecuted"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        public void AddCommand<T1, T2>(string command, string description, Action<T1, T2> onCommandExecuted)
        {
            DebugLogConsole.AddCommand(command, description, onCommandExecuted);
        }
        
        /// <summary>
        /// 添加执行命令和回调 Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="description"></param>
        /// <param name="onCommandExecuted"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        public void AddCommand<T1, T2, T3>(string command, string description, Action<T1, T2, T3> onCommandExecuted)
        {
            DebugLogConsole.AddCommand(command, description, onCommandExecuted);
        }

        public void SetSearchKeyword(string filer)
        {
            if (_searchInput != null)
            {
                _searchInput.text = filer;
            }
            else
            {
                Debug.LogWarning("Can not find <searchInput> on Console!!");
            }
        }


        public void PrintSystemInfo()
        {
            if (!IsConsoleActive)
            {
                ShowConsole();
            }
            DebugLogConsole.LogSystemInfo();
        }

        public void ClearLogs()
        {
            DebugLogManager.Instance.ClearLogs();
        }

    }
}

