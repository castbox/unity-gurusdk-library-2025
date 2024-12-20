
namespace Guru
{
    using UnityEngine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Firebase.RemoteConfig;
    using Guru.Ads;
    using Guru.Network;
    
    public partial class GuruSDK: MonoBehaviour
    {
        // SDK_VERSION
        public static string Version => MAIN_VERSION;
        
        // Const
        private const string LOG_TAG = "[Guru]";
        public const string ServicesConfigKey = "guru_services";
        
        private static GuruSDK _instance;
        /// <summary>
        /// 单利引用
        /// </summary>
        public static GuruSDK Instance
        {
            get
            {
                if(null == _instance)
                {
                    _instance = CreateInstance();
                }
                return _instance;
            }
            
        }

        private GuruSDKInitConfig _initConfig;

        private static GuruSDKInitConfig InitConfig => Instance._initConfig;
        private static GuruSDKModel Model => GuruSDKModel.Instance;
        private static GuruServicesConfig _appServicesConfig;
        private static GuruSettings _guruSettings;
        private static GuruSettings GuruSettings
        {
            get
            {
                if (_guruSettings == null) _guruSettings = GuruSettings.Instance;
                return _guruSettings;
            }
        }
        
        /// <summary>
        /// Debug Mode
        /// </summary>
        public static bool DebugModeEnabled 
        {
            get => Model.IsDebugMode;
            set => Model.IsDebugMode = value;
        }

        /// <summary>
        /// 初始化成功标志位
        /// </summary>
        public static bool IsInitialSuccess { get; private set; } = false;
        /// <summary>
        /// Firebase 就绪标志位
        /// </summary>
        public static bool IsFirebaseReady { get; private set; } = false;
        /// <summary>
        /// 服务就绪标志位
        /// </summary>
        public static bool IsServiceReady { get; private set; } = false;

        private Firebase.Auth.FirebaseUser _firebaseUser;
        [Obsolete("获取 FirebaseUser 的属性接口即将废弃，请改用 <GuruSDK.Callbacks.SDK.OnFirebaseUserAuthResult += OnMyGetFirebaseUserCallback> 来异步获取该属性")]
        public static Firebase.Auth.FirebaseUser FirebaseUser => Instance?._firebaseUser ?? null; 
        
        // SDK 启动属性
        private readonly GuruSDKSessionInfo _sessionInfo = new GuruSDKSessionInfo();
        public static double BoostDuration => Instance._sessionInfo.boostDuration;
        
        #region 初始化
        
        private static GuruSDK CreateInstance()
        {
            var go = new GameObject(nameof(GuruSDK));
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<GuruSDK>();
            return _instance;
        }
        
        // TODO : 下个版本需要将 整个 GuruSDK 做功能性的拆分
        
        public static void Init(Action<bool> onComplete = null)
        {
            Init(GuruSDKInitConfig.Builder().Build(), onComplete);
        }
        
        public static void Init(GuruSDKInitConfig config, Action<bool> onComplete = null)
        {
            // ----- First Open Time -----
            // SetFirstOpenTime(GetFirstOpenTime());  // FirstOpenTime 
            LogI($"---- Guru SDK [{Version}] ----\n{config}");
            Instance.StartWithConfig(config, onComplete);
        }

        /// <summary>
        /// 启动SDK
        /// </summary>
        /// <param name="config"></param>
        /// <param name="onComplete"></param>
        private void StartWithConfig(GuruSDKInitConfig config, Action<bool> onComplete)
        {
            IsInitialSuccess = false;
            _initConfig = config;
            _sessionInfo.startTime = DateTime.UtcNow;

            if (config.LogEnabledInDebugMode) Analytics.EnableDebugAnalytics = true; // 允许 Debug 模式下打点
            if (!config.AutoNotificationPermission) FirebaseUtil.SetAutoFetchFcmToken(false); // 不允许自动启动获取 FCM Token
            if (config.DebugMode) {
                DebugModeEnabled = true; // 如果SDK初始化时启动了 Debug 模式，则强制开启，否则优先使用本地缓存的值
                AnalyticRecordManager.Instance.InitRecorder(); // Debug 模式下初始化记录器
            }
            
            InitUpdaters(); // 初始化 Updaters
            InitThreadHandler(); // 初始化线程处理器
            InitNetworkMonitor(); // 网络状态
            InitServices(); // 初始化所有的服务
            
            onComplete?.Invoke(true);
        }
        
        private void InitServices()
        {
            // --- Start Analytics ---
            LogI($"#1.1 ---- Init Analytics ----");
            Analytics.Init();
            ReportBasicUserProperties(); // 立即上报基础用户属性
            
            //--- Start Firebase ---
            LogI($"#1.2 --- InitFirebase ---");
            FirebaseUtil.Init(OnFirebaseDepsCheckResult, 
                OnGetFirebaseId, 
                OnGetGuruUID, 
                OnFirebaseLoginResult); // 确保所有的逻辑提前被调用到
            
            //--- Start Facebook ---
            LogI($"#1.3 --- InitFacebook ---");
            FBService.Instance.StartService(Analytics.OnFBInitComplete);
            
            IsInitialSuccess = true;
        }

        /// <summary>
        /// SDK 启动进程结束
        /// </summary>
        private void OnBoostSessionOver()
        {
            //---------- Event SDK Info ------------
            LogI($"#7 --- SDK is ready, report Info ---");
            _sessionInfo.boostDuration = (DateTime.UtcNow - _sessionInfo.startTime).TotalSeconds;
        }

        /// <summary>
        /// 注入云控参数基础数据
        /// </summary>
        /// <returns></returns>
        private string LoadDefaultGuruServiceJson()
        {
            // 加载本地 Services 配置值
            var txtAsset = Resources.Load<TextAsset>(ServicesConfigKey);
            if (txtAsset != null)
            {
                return txtAsset.text;
            }
            return "";
        }
        
        /// <summary>
        /// 首次拉取云控参数完成
        /// </summary>
        /// <param name="success"></param>
        private void OnFirstFetchRemoteComplete(bool success)
        {
            LogI($"#6 --- Remote fetch complete: {success} ---");
            ABTestManager.Init(); // 启动AB测试解析器
            Callbacks.Remote.InvokeOnRemoteFetchComplete(success);
            UpdateSDKRemoteConfig(); // 启动内置的 RemoteConfig 解析
        }

        private void Update()
        {
            UpdateAllUpdates(); // 驱动所有的更新器
        }


        #endregion

        #region 内置 Remote Config 更新

        /// <summary>
        /// 开始解析内置的云控参数
        /// </summary>
        private void UpdateSDKRemoteConfig()
        {
            // TODO ----------- 
            // Adjust 延迟时间
            if (_remoteConfigManager.TryGetRemoteData(AdjustService.REMOTE_DELAY_TIME_KEY, out var data))
            {
                var dataSource = data.Source switch
                {
                    ValueSource.Local => DelayMinutesSource.Local,
                    ValueSource.Remote => DelayMinutesSource.RemoteConfig,
                    _ => DelayMinutesSource.Default
                };
                AdjustService.Instance.SetAdRevDelayMinutes(data.GetValue(AdjustService.DEFAULT_DELAY_MINUTES), dataSource);
            }
        }
        
        #endregion
        
        #region App Services Update

        /// <summary>
        /// Apply Cloud guru-service configs for sdk assets
        /// </summary>
        private void InitAllGuruServices()
        {
            // -------- Init Analytics ---------
            SetSDKEventPriority();
            // -------- Init Notification -----------
            InitNotiPermission();
            
            bool useKeywords = false;
            bool useIAP = _initConfig.IAPEnabled;
            // bool enableAnaErrorLog = false;
            
            //----------- Set GuruServices ----------------
            var services = GetRemoteServicesConfig();
            if (services != null)
            {
                _appServicesConfig = services;
                useKeywords = _appServicesConfig.GetKeywordsEnabled();
                // 使用初始化配置中的 IAPEnable来联合限定, 如果本地写死关闭则不走云控开启
                useIAP = _initConfig.IAPEnabled && _appServicesConfig.IsIAPEnabled(); 
                // enableAnaErrorLog = _appServicesConfig.EnableAnaErrorLog();
                _appBundleId = _appServicesConfig.app_settings.bundle_id; // 获取BundleId
                
                Try(() =>
                {
                    LogI($"#4.1 --- Start apply services ---");
                    //----------------------------------------------------------------

                    // 自打点设置错误上报
                    // if(enableAnaErrorLog) GuruAnalytics.EnableErrorLog = true;
                    
                    // adjust 事件设置
                    if (null != _appServicesConfig.adjust_settings && null != GuruSettings)
                    {
                        // 更新 Adjust Tokens
                        GuruSettings.UpdateAdjustTokens(
                            _appServicesConfig.adjust_settings.AndroidToken(),
                            _appServicesConfig.adjust_settings.iOSToken());
                        // 更新 Adjust Events
                        GuruSettings.UpdateAdjustEvents(_appServicesConfig.adjust_settings.events);
                    }
                
                    LogI($"#4.2 --- Start GuruSettings ---");
                    // GuruSettings 设置
                    if (null != _appServicesConfig.app_settings)
                    {
                        // 设置 Tch FB Mod
                        Analytics.TchFbMode = _appServicesConfig.GetTchFacebookMode();
                        LogI($"#4.2.1 --- Set TchFbMode: {Analytics.TchFbMode}");
                        
                        // 设置获取设备 UUID 的方法
                        // if (_appServicesConfig.UseUUID())
                        // {
                        //     IPMConfig.UsingUUID = true; // 开始使用 UUID 作为 DeviceID 标识
                        // }
                    
                        if (null !=  GuruSettings)
                        {
                            // 更新和升级 GuruSettings 对应的值
                            GuruSettings.UpdateAppSettings(
                                _appServicesConfig.app_settings.bundle_id,
                                _appServicesConfig.fb_settings?.fb_app_id ?? "",
                                _appServicesConfig.app_settings.support_email,
                                _appServicesConfig.app_settings.privacy_url,
                                _appServicesConfig.app_settings.terms_url,
                                _appServicesConfig.app_settings.android_store,
                                _appServicesConfig.app_settings.ios_store, 
                                _appServicesConfig.parameters?.using_uuid ?? false,
                                _appServicesConfig.parameters?.cdn_host ?? "");
                            
                            _appBundleId = _appServicesConfig.app_settings.bundle_id; // 配置预设的 BundleId
                        }
                    }
                    //---------------------------------
                }, ex =>
                {
                    LogE($"--- ERROR on apply services: {ex.Message}");
                });
          
                
            }
            //----------- Set IAP ----------------
            if (useIAP)
            {
                // InitIAP(_initConfig.GoogleKeys, _initConfig.AppleRootCerts); // 初始化IAP
                Try(() =>
                {
                    LogI($"#4.3 --- Start IAP ---");
                    if (_initConfig.GoogleKeys == null || _initConfig.AppleRootCerts == null)
                    {
                        LogEx("[IAP] GoogleKeys is null when using IAPService! Integration failed. App will Exit");
                    }
                    
                    // 初始化支付服务
                    InitIAP(UID, 
                        _initConfig.GoogleKeys, 
                        _initConfig.AppleRootCerts,
                        _appServicesConfig.AppBundleId(),
                        IDFV); // 初始化IAP
                }, ex =>
                {
                    LogE($"--- ERROR on useIAP: {ex.Message}");
                });
            }

            bool canUseGuruConsent = !InitConfig.UseCustomConsent;
            
            //----------- iOS 判断是否打开苹果审核流程 ----------------
#if UNITY_IOS
            bool isAppReview = _appServicesConfig.GetIsAppReview();
            if(InitConfig.UseCustomConsent || isAppReview)
            {
                canUseGuruConsent = false;
            }

            if (isAppReview)
            {
                // StartAppleReviewFlow(); // 直接显示 ATT 弹窗, 跳过 Consent 流程
                Try(() =>
                {
                    LogI($"#4.5 ---  StartAppleReviewFlow ---");
                    StartAppleReviewFlow(); // 直接显示 ATT 弹窗, 跳过 Consent 流程
                }, ex =>
                {
                    LogE($"--- ERROR on StartAppleReviewFlow: {ex.Message}");
                });
            }
#endif

            if (canUseGuruConsent)
            {
                //----------- Set Consent ----------------
                LogI($"#4.6 --- Start Consent Flow ---");
                Try(StartConsentFlow, ex =>
                {
                    LogE($"--- ERROR on StartConsentFlow: {ex.Message}");
                });
            }
            
#if UNITY_ANDROID
            // Android 命令行调试
            AndroidApplySystemProps(); 
#endif
            
            IsServiceReady = true;

            // 中台服务初始化结束
            Callbacks.SDK.InvokeOnGuruServiceReady();
        }
        
        /// <summary>
        /// Get the guru-service cloud config value;
        /// User can fetch the cloud guru-service config by using Custom Service Key
        /// </summary>
        /// <returns></returns>
        private GuruServicesConfig GetRemoteServicesConfig()
        {

            string defaultJson = GetRemoteString(ServicesConfigKey);
            
            bool useCustomKey = false;
            string key = ServicesConfigKey;
            if (!string.IsNullOrEmpty(_initConfig.CustomServiceKey))
            {
                key = _initConfig.CustomServiceKey;
                useCustomKey = true;
            }
            var json = GetRemoteString(key); // Cloud cached data

            if (string.IsNullOrEmpty(json) && useCustomKey && !string.IsNullOrEmpty(defaultJson))
            {
                // No remote data fetched from cloud, should use default values
                json = defaultJson;
                LogI($"--- No remote data found with: {key}  -> Using default key {ServicesConfigKey} and local data!!!");
            }

            if (!string.IsNullOrEmpty(json))
            {
                return JsonParser.ToObject<GuruServicesConfig>(json);
            }
            
            return null;
        }

        private void Try(Action method, Action<Exception> onException = null, Action onFinal = null)
        {
            if (method == null) return;

            try
            {
                method.Invoke();
            }
            catch (Exception ex)
            {
                LogEx(ex);
                // ignored
                onException?.Invoke(ex);
            }
            finally
            {
                // Finally
                onFinal?.Invoke();
            }
        }


        #endregion

        #region Android 命令行参数

        /// <summary>
        /// Android 设备应用 命令行参数
        /// 调用 adb shell setprop {cmd} {bundleId} 来进行设置
        /// </summary>
        private void AndroidApplySystemProps()
        {
            // BundleID 为空时， 无法获取 prop 值，则直接返回
            if (string.IsNullOrEmpty(_appServicesConfig.AppBundleId()))
            {
                UnityEngine.Debug.Log("[SDK][Debug]--- App Bundle Id is empty, please set it before call <ParseCommandLineArgs>. ---");
                return;
            }
            
#if UNITY_ANDROID
            LogI($"#5.1 --- Android StartAndroidDebug Cmd lines---");
            var systemProp = new Guru.Utils.AndroidSystemPropHelper(_appServicesConfig.AppBundleId());

            if (systemProp.IsDebuggerEnabled())
            {
                UnityEngine.Debug.Log($"[SDK][Debug]--- Show AdStatus");
                // 显示应用调试状态栏
                GuruSDK.Debugger.ShowAdStatus();
            }
            
            if (systemProp.IsWatermarkEnabled())
            {
                UnityEngine.Debug.Log($"[SDK][Debug]--- Show Watermark");
                // 显示应用水印
                // TODO：实现显示水印的功能
            }
            
            if (systemProp.IsConsoleEnabled())
            {
                // 显示控制台
                UnityEngine.Debug.Log($"[SDK][Debug]--- Show Console");
#if GURU_DEBUG_CONSOLE
                ShowDebugConsole();
                return;
#endif
                UnityEngine.Debug.LogWarning("No implement for Show Console");
            }
#endif
        }

        #endregion

        #region Apple 审核流程逻辑

#if UNITY_IOS
        private void StartAppleReviewFlow()
        {
            CheckAttStatus();
        }
#endif
        #endregion
        
        #region Logging

        private static void LogI(object message)
        {
            UnityEngine.Debug.Log($"{LOG_TAG} {message}");
        }

        private static void LogW(object message)
        {
            UnityEngine.Debug.LogWarning($"{LOG_TAG} {message}");
        }

        private static void LogE(object message)
        {
            UnityEngine.Debug.LogError($"{LOG_TAG} {message}");
        }


        private static void LogEx(string message)
        {
            LogEx( new Exception($"{LOG_TAG} {message}"));
        }

        private static void LogEx(Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        
        /// <summary>
        /// 上报崩溃信息
        /// </summary>
        /// <param name="message"></param>
        public static void Report(string message)
        {
            Analytics.LogCrashlytics(message, false);
        }
        
        /// <summary>
        /// 上报异常
        /// </summary>
        /// <param name="message"></param>
        public static void ReportException(string message)
        {
            Analytics.LogCrashlytics(message);
        }
        
        /// <summary>
        /// 上报异常 Exception
        /// </summary>
        /// <param name="ex"></param>
        public static void ReportException(Exception ex)
        {
            Analytics.LogCrashlytics(ex);
        }

        #endregion

        #region 生命周期

        /// <summary>
        /// 暂停时处理
        /// </summary>
        /// <param name="paused"></param>
        private void OnAppPauseHandler(bool paused)
        {
            if(paused) Model.Save(true); // 强制保存数据
            Callbacks.App.InvokeOnAppPaused(paused);
        }
        
        private void OnApplicationPause(bool paused)
        {
            OnAppPauseHandler(paused);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            OnAppPauseHandler(!hasFocus);
        }

        private void OnApplicationQuit()
        {
            Model.Save(true);
            Callbacks.App.InvokeOnAppQuit();
        }

        #endregion

        #region 延迟处理

        /// <summary>
        /// 启动协程
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public static Coroutine DoCoroutine(IEnumerator enumerator)
        {
            return Instance != null ? Instance.StartCoroutine(enumerator) : null;
        }

        public static void KillCoroutine(Coroutine coroutine)
        {
            if(coroutine != null)
                Instance.StopCoroutine(coroutine);
        }
        
        /// <summary>
        /// 延时执行
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="callback"></param>
        public static Coroutine Delay(float seconds, Action callback)
        {
            return DoCoroutine(Instance.OnDelayCall(seconds, callback));
        }

        private IEnumerator OnDelayCall(float delay, Action callback)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null;
            }
            callback?.Invoke();
        }
        
        #endregion

        #region 帧更新 Updater

        
        private List<IUpdater> _updaterRunningList;
        private List<IUpdater> _updaterRemoveList;

        private void InitUpdaters()
        {
            _updaterRunningList = new List<IUpdater>(20);
            _updaterRemoveList = new List<IUpdater>(20);
        }

        private void UpdateAllUpdates()
        {
            int i = 0;
            // ---- Updater Trigger ----
            if (_updaterRunningList.Count > 0)
            {
                i = 0;
                while (i < _updaterRunningList.Count)
                {
                    var updater = _updaterRunningList[i];
                    if (updater != null) 
                    {
                        if (updater.State == UpdaterState.Running)
                        {
                            updater.OnUpdate();
                        }
                        else if(updater.State == UpdaterState.Kill)
                        {
                            _updaterRemoveList.Add(updater);
                        }
                    }
                    else
                    {
                        _updaterRunningList.RemoveAt(i);
                        i--;
                    }
                    i++;
                }

            }

            if (_updaterRemoveList.Count > 0)
            {
                i = 0;
                while (i < _updaterRemoveList.Count)
                {
                    RemoveUpdater(_updaterRemoveList[i]);
                    i++;
                }
                _updaterRemoveList.Clear();
            }
        }

        /// <summary>
        /// 注册帧更新器
        /// </summary>
        /// <param name="updater"></param>
        public static void RegisterUpdater(IUpdater updater)
        {
            Instance.AddUpdater(updater);
            updater.Start();
        }


        private void AddUpdater(IUpdater updater)
        {
            if (_updaterRunningList == null) _updaterRunningList = new List<IUpdater>(20);
            _updaterRunningList.Add(updater);
        }

        private void RemoveUpdater(IUpdater updater)
        {
            if (_updaterRunningList != null && updater != null)
            {
                _updaterRunningList.Remove(updater);
            }
        }

        #endregion

        #region 中台推送管理

        /// <summary>
        /// 设置 Guru 中台的 Push 服务开关
        /// 关闭后不再接受中台的 push 信息
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetPushServiceEnabled(bool enabled)
        {
            GuruDeviceInfoUploader.Instance.SetPushNotificationEnabled(enabled);
        }
        
        #endregion

        #region Deeplink
        
        /// <summary>
        /// 添加回调链接
        /// </summary>
        /// <param name="deeplink"></param>
        private void OnDeeplinkCallback(string deeplink)
        {
           Callbacks.SDK.InvokeDeeplinkCallback(deeplink); // 尝试调用回调
        }
        
        #endregion

        #region 网络状态上报

        private NetworkStatusMonitor _networkStatusMonitor;
        private string _lastNetworkStatus;
        
        private void InitNetworkMonitor()
        {
            _networkStatusMonitor = new NetworkStatusMonitor(Analytics.SetNetworkStatus, 
                lastStatus =>
            {
                LogEvent("guru_offline", new Dictionary<string, dynamic>()
                {
                    ["from"] = lastStatus
                }, new EventSetting()
                {
                    EnableFirebaseAnalytics = true,
                    EnableGuruAnalytics = true
                });
            });
        }
        
        /// <summary>
        /// 获取当前的网络状态
        /// </summary>
        /// <returns></returns>
        private string GetNetworkStatus() => _networkStatusMonitor.GetNetworkStatus();

        
        #endregion

        #region Firebase 服务

        private void OnGetGuruUID(bool success)
        {
            if (success)
            {
                var uid = IPMConfig.IPM_UID;
                
                Model.UserId = uid;
                if (GuruIAP.Instance != null)
                {
                    GuruIAP.Instance.SetUID(uid);
                    GuruIAP.Instance.SetUUID(UUID);
                }
                
                // 自打点设置用户 ID
                Analytics.SetUid(UID);
                Analytics.SetIDFA(IPMConfig.IDFA);
                Analytics.SetIDFV(IPMConfig.IDFV);
                // Crashlytics 设置 uid
                CrashlyticsAgent.SetUserId(UID);
                // 上报所有的事件
                Analytics.ShouldFlushGuruEvents();
            }
            
            Callbacks.SDK.InvokeOnGuruUserAuthResult(success);
        }
        
        private void OnGetFirebaseId(string fid)
        {
            // 初始化 Adjust 服务
            InitAdjustService(fid, InitConfig.OnAdjustDeeplinkCallback);
            // 初始化自打点
            Analytics.InitGuruAnalyticService(fid, Version);

            // SDK 的启动 Session 结束 
            OnBoostSessionOver();
        }
        
        // TODO: 需要之后用宏隔离应用和实现
        // Auth 登录认证
        private void OnFirebaseLoginResult(bool success, Firebase.Auth.FirebaseUser firebaseUser)
        {
            _firebaseUser = firebaseUser;
            Callbacks.SDK.InvokeOnFirebaseAuthResult(success, firebaseUser);
        }

        /// <summary>
        /// 开始各种组件初始化
        /// </summary>
        private void OnFirebaseDepsCheckResult(bool success)
        {
            LogI($"#3 --- On FirebaseDeps: {success} ---");
            IsFirebaseReady = success;
            Callbacks.SDK.InvokeOnFirebaseReady(success);

            Analytics.OnFirebaseInitCompleted();

            LogI($"#3.5 --- Call InitRemoteConfig ---");
            // 开始Remote Manager初始化 
            
            var defaultGuruServiceJson = LoadDefaultGuruServiceJson();

            var _defaults = _initConfig.DefaultRemoteData.ToDictionary(
                entry => entry.Key,
                entry => entry.Value);
            
            if (!string.IsNullOrEmpty(defaultGuruServiceJson))
            {
                _defaults[ServicesConfigKey] = defaultGuruServiceJson;
            }
            
            // RemoteConfigManager.Init(_defaults);
            // RemoteConfigManager.OnFetchCompleted += OnFetchRemoteCallback;
            
            InitRemoteConfigManager(_defaults, DebugModeEnabled);
            
            LogI($"#4 --- Apply remote services config ---");
            // 根据缓存的云控配置来初始化参数
            InitAllGuruServices();
        }

        #endregion
        		
        #region Adjust服务
        
        /// <summary>
        /// 启动 Adjust 服务
        /// </summary>
        private static void InitAdjustService(string firebaseId, Action<string> onDeeplinkCallback = null)
        {
            LogI($"#5 --- InitAdjustService ---");
            // 启动 AdjustService
            string app_token = GuruSettings.Instance.AdjustSetting?.GetAppToken() ?? "";
            string fb_app_id = GuruSettings.Instance.IPMSetting.FacebookAppId;
            bool enabled_deferred_report_ad_revenue = InitConfig.AdjustDeferredReportAdRevenueEnabled;
            int iOSAttWaitingTime = InitConfig.AdjustIOSAttWaitingTime;

            // if (!string.IsNullOrEmpty(IPMConfig.ADJUST_ID))
            //     Analytics.SetAdjustId(IPMConfig.ADJUST_ID); // 二次启动后，若有值则立即上报属性
            
            AdjustService.Instance.Start(app_token, fb_app_id, firebaseId, DeviceId, 
                enabled_deferred_report_ad_revenue, // Adjust 延迟打点开关
                iOSAttWaitingTime, // iOS Att 延迟判定时间
                OnAdjustInitComplete, onDeeplinkCallback ,OnGetGoogleAdId );
        }

        /// <summary>
        /// Adjust 初始化结束
        /// </summary>
        /// <param name="adjustDeviceId"></param>
        /// <param name="idfv"></param>
        /// <param name="idfa"></param>
        private static void OnAdjustInitComplete(string adjustDeviceId)
        {
            UnityEngine.Debug.Log($"{LOG_TAG} --- OnAdjustInitComplete:  adjustId:{adjustDeviceId}");
            
            // 获取 ADID 
            if (string.IsNullOrEmpty(adjustDeviceId)) adjustDeviceId = "not_set";
            Analytics.SetAdjustDeviceId(adjustDeviceId);
            Analytics.OnAdjustInitComplete();
            
            // 确保跑在主线程内再进行赋值
            RunOnMainThread(() =>
            {
                IPMConfig.ADJUST_DEVICE_ID = adjustDeviceId;
            });
        }

        private static void OnGetGoogleAdId(string googleAdId)
        {
            UnityEngine.Debug.Log($"{LOG_TAG} --- OnGetGoogleAdId: {googleAdId}");
            Analytics.SetGoogleAdId(googleAdId);
            
            // 确保跑在主线程内再进行赋值
            RunOnMainThread(() =>
            {
                IPMConfig.GOOGLE_ADID = googleAdId;
            });
        }
       
        #endregion

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
        private static void DebugSendTch02Event()
        {
            AdService.Instance.SetTch02Revenue(0.2);
        }

#endif  
        #endregion
        
    }

}