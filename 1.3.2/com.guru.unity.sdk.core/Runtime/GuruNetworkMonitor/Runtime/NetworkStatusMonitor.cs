namespace Guru.Network
{
    using System;
    using System.Linq;
    using UnityEngine;
    using Cysharp.Threading.Tasks;
    using System.Collections;

    /// <summary>
    /// 打点链接：https://docs.google.com/spreadsheets/d/1N47rXgjatRHFvzWWx0Hqv5C1D9NHHGbggi6pQ65c-zQ/edit?gid=1858695240#gid=1858695240
    /// 需求链接：https://docs.google.com/document/d/1jEUV1yEjEuIOGgutwvv0x1ZV-GocgpBEbF1SqedRqAw/edit#heading=h.g4sg3xs525my
    /// </summary>
    public class NetworkStatusMonitor
    {
        private const string LOG_TAG = "[NET]";
        private const string NETWORK_STATUS_NOT_SET = "not_set";
        private const int NETWORK_OFFLINE_CHECK_INTERVAL = 4000;
        private static readonly DateTime NETWORK_TRIGGER_AT_NOT_SET = new(1970, 1, 1);

        private const string NETWORK_STATUS_NONE = "none";
        private const string NETWORK_STATUS_MOBILE = "mobile";
        private const string NETWORK_STATUS_WIFI = "wifi";
        private const string NETWORK_STATUS_VPN = "vpn";
        private const string NETWORK_STATUS_WIFI_VPN = "wifi-vpn";
        private const string NETWORK_STATUS_MOBILE_VPN = "mobile-vpn";

        private DateTime TriggerAt
        {
            get => this._saveData.TriggerAt == default ? NETWORK_TRIGGER_AT_NOT_SET : this._saveData.TriggerAt;
            set => this._saveData.TriggerAt = value;
        }

        private string OffLineFromNetworkStatus
        {
            get => this._saveData.OfflineFromNetworkStatus ?? NETWORK_STATUS_NOT_SET;
            set => this._saveData.OfflineFromNetworkStatus = value;
        }

        private string _currentNetworkStatus;

        private string CurrentNetworkStatus
        {
            get => this._currentNetworkStatus;
            set
            {
                Debug.Log($"{LOG_TAG} --- Network status changed:{value}");

                // 用户掉线了，记录下来
                if (NETWORK_STATUS_NONE == value)
                {
                    Debug.Log($"{LOG_TAG} --- Check if user offline");
                    this._userOfflineCheckTimer.Start();
                }
                else
                {
                    this.OffLineFromNetworkStatus = value;
                    this._userOfflineCheckTimer.Stop();
                }

                this._currentNetworkStatus = value;
                Debug.Log($"{LOG_TAG} --- Network status updated :{this._currentNetworkStatus}");
            }
        }

        private readonly Action<string> _onNetworkStatusChanged;
        private readonly Action<string> _onFirstOfflineToday;
        private readonly NetworkMonitorData _saveData;
        private readonly IConnectivityWatcher _connectivityWatcher;
        private readonly GuruTimer _userOfflineCheckTimer;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="onNetworkStatusChanged"></param>
        /// <param name="onFirstOfflineToday"></param>
        public NetworkStatusMonitor(Action<string> onNetworkStatusChanged, Action<string> onFirstOfflineToday)
        {
            this._onNetworkStatusChanged = onNetworkStatusChanged;
            this._onFirstOfflineToday = onFirstOfflineToday;

            // 读取数据
            this._saveData = new NetworkMonitorData();

            // 设置检测用户掉线定时器
            this._userOfflineCheckTimer = new GuruTimer(NETWORK_OFFLINE_CHECK_INTERVAL, onTimerComplete: () =>
            {
                Debug.Log($"{LOG_TAG} --- Try set user offline from network status: {this.OffLineFromNetworkStatus}");

                this.TryReportFirstOfflineToday();
                this.OffLineFromNetworkStatus = NETWORK_STATUS_NONE;
            });

            // 创建 Watcher, 使用 Watcher 来监控之后的网络数据
            this._connectivityWatcher = this.CreateConnectivityWatcher(this.OnWatcherInitComplete);
        }

        private IConnectivityWatcher CreateConnectivityWatcher(Action<bool> onInitResult)
        {
            IConnectivityWatcher watcher = null;
#if UNITY_EDITOR
            watcher = new ConnectivityWatcherEditor(onInitResult);
#elif UNITY_ANDROID
            watcher = new ConnectivityWatcherAndroid(onInitResult);
#elif UNITY_IOS
            watcher = new ConnectivityWatcherIOS(onInitResult);
#endif
            return watcher;
        }

        /// <summary>
        /// 初始化结束
        /// </summary>
        /// <param name="result"></param>
        private void OnWatcherInitComplete(bool result)
        {
            CoroutineHelper.Instance.StartDelayed(0.1f, () =>
            {
                if (!result || this._connectivityWatcher == null)
                {
                    Debug.LogError($"{LOG_TAG} --- init watcher failed");
                    return;
                }

                this._connectivityWatcher.SetNetworkStatusListener(OnNetworkStatusChanged);

                // 获取网络状态
                this.RequestNetworkStatus();

                // 首次进入上报一下网络状态
                this._onNetworkStatusChanged?.Invoke(this.CurrentNetworkStatus);

                // 启动 Watcher 后立即定制一个第二天 0 点的检查事件
                this.AsyncCheckOfflineWhenNextDayBegin();
            });
        }

        /// <summary>
        /// 网络状态更新
        /// </summary>
        /// <param name="list"></param>
        private void OnNetworkStatusChanged(string[] list)
        {
            var status = GetNetworkStatusFromList(list);
            this.CurrentNetworkStatus = status;
            this._onNetworkStatusChanged?.Invoke(status);
        }

        private void TryReportFirstOfflineToday()
        {
            if (ShouldTriggerFirstOfflineToday())
            {
                Debug.Log($"{LOG_TAG} --- Report Offline: {this.OffLineFromNetworkStatus}");
                Debug.Log($"[Firebase] --- driver logEvent: guru_offline  from :{this.OffLineFromNetworkStatus}");

                this._onFirstOfflineToday?.Invoke(this.OffLineFromNetworkStatus);
                this.SetTriggerFirstOfflineDate();
            }
        }

        /// <summary>
        /// 从返回的状态列表中获取和拼接网络状态
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private string GetNetworkStatusFromList(string[] list)
        {
            string status = NETWORK_STATUS_NONE;
            if (list.Contains(NETWORK_STATUS_WIFI))
            {
                status = NETWORK_STATUS_WIFI;
                if (list.Contains(NETWORK_STATUS_VPN))
                {
                    status = NETWORK_STATUS_WIFI_VPN;
                }
            }
            else if (list.Contains(NETWORK_STATUS_MOBILE))
            {
                status = NETWORK_STATUS_MOBILE;
                if (list.Contains(NETWORK_STATUS_VPN))
                {
                    status = NETWORK_STATUS_MOBILE_VPN;
                }
            }

            return status;
        }

        /// <summary>
        /// 获取网络状态
        /// </summary>
        /// <returns></returns>
        public string GetNetworkStatus()
        {
            var list = this._connectivityWatcher.GetNetConnectivity();
            return this.GetNetworkStatusFromList(list);
        }

        /// <summary>
        /// 用户是否已经失去了链接
        /// </summary>
        /// <returns></returns>
        private bool IsUserOffline() => this.GetNetworkStatus() == NETWORK_STATUS_NONE;

        /// <summary>
        /// 当前是可以打点上报
        /// </summary>
        /// <returns></returns>
        private bool IsNotSameDay(DateTime date) => DateTime.UtcNow.DayOfYear != date.DayOfYear && DateTime.UtcNow.Year != date.Year;

        /// <summary>
        /// 设置打点时间
        /// </summary>
        private void SetTriggerFirstOfflineDate() => this.TriggerAt = DateTime.UtcNow;

        private void ResetTriggerFirstOfflineDate()
        {
            this.TriggerAt = NETWORK_TRIGGER_AT_NOT_SET;
            this.OffLineFromNetworkStatus = this.CurrentNetworkStatus;
        }

        private void RequestNetworkStatus()
        {
            this.CurrentNetworkStatus = this.GetNetworkStatusFromList(this._connectivityWatcher.GetNetConnectivity());
        }

        private bool ShouldTriggerFirstOfflineToday()
        {
            // 上次上报和今天不是同一天
            return this.IsNotSameDay(this.TriggerAt);
        }

        private void AsyncCheckOfflineWhenNextDayBegin()
        {
            var date = DateTime.UtcNow.Date + TimeSpan.FromHours(24) - DateTime.UtcNow;
            DelayAction((float) date.TotalSeconds, () =>
            {
                Debug.Log($"{LOG_TAG} --- Reset offline trigger data at {DateTime.UtcNow}");

                this.ResetTriggerFirstOfflineDate();
                this.RequestNetworkStatus();

                // 后立即定制第二天 0 点的检查事件
                this.AsyncCheckOfflineWhenNextDayBegin();
            });
        }

        private Coroutine DelayAction(float delay, Action action)
        {
            return CoroutineHelper.Instance.StartDelayed(delay, action);
        }

        /// <summary>
        /// 网络状态监控器持久化数据
        /// </summary>
        [Serializable]
        class NetworkMonitorData
        {
            private const string K_NETWORK_MONITOR_DATA = "guru_network_monitor_data";

            private string _offlineFromNetworkStatus;

            public string OfflineFromNetworkStatus
            {
                get => this._offlineFromNetworkStatus;
                set
                {
                    this._offlineFromNetworkStatus = value;
                    _ = Save(true);
                }
            }

            private DateTime _triggerAt;

            public DateTime TriggerAt
            {
                get => this._triggerAt;
                set
                {
                    this._triggerAt = value;
                    _ = Save(true);
                }
            }

            public NetworkMonitorData()
            {
                Load(); // 立即加载数据
            }

            private void Load()
            {
                var raw = PlayerPrefs.GetString(K_NETWORK_MONITOR_DATA, "");
                if (!string.IsNullOrEmpty(raw))
                {
                    try
                    {
                        var arr = raw.Split('|');
                        if (arr.Length > 0) this._offlineFromNetworkStatus = arr[0];
                        if (arr.Length > 1) this._triggerAt = DateTime.Parse(arr[1]);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                }
            }

            public async UniTask Save(bool force = false)
            {
                await UniTask.SwitchToMainThread();

                var buffer = $"{_offlineFromNetworkStatus}|{this._triggerAt:g}";
                PlayerPrefs.SetString(K_NETWORK_MONITOR_DATA, buffer);
                if (force) PlayerPrefs.Save();
            }
        }
    }
}