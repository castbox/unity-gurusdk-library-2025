namespace Guru.Notification
{
    using System;
    using UnityEngine;
#if UNITY_ANDROID
    using UnityEngine.Android;
    using Unity.Notifications.Android;
#endif
    
    public class NotificationAgentAndroid : INotificationAgent
    {
        private enum GuruPushPriority
        {
            Critical = 0,
            Urgent = 1,
            High = 2,
            Medium = 3,
        }
        
        // Channel IDs
        private const string PUSH_CHANNEL_CRITICAL = "guru_push_critical";
        private const string PUSH_CHANNEL_URGENT = "guru_push_urgent";
        private const string PUSH_CHANNEL_HIGH = "guru_push_high";
        private const string PUSH_CHANNEL_MEDIUM = "guru_push_medium";
        // Importance
        private const int IMPORTANCE_MAX = 5;
        private const int IMPORTANCE_HIGH = 4;
        private const int IMPORTANCE_DEFAULT = 3;
        private const int IMPORTANCE_LOW = 2;
        
        // Old default channels
        private const string FCM_DEFAULT_CHANNEL_ID = "default_notification_channel_id";
        private const string FCM_DEFAULT_CHANNEL_NAME = "fcm_default_channel";
        private const string FCM_DEFAULT_CHANNEL_DESC = "Default channel for firebase cloud messaging";
        
        private const string STATUS_GRANTED = "granted";
        private const string STATUS_DENIED = "denied";
        // private const string STATUS_NOT_DETERMINED = "not_determined";
        private const int REQUEST_PERMISSION_SDK_VERSION = 33;
        private const int REQUEST_CHANNEL_SDK_VERSION = 26;
        private const string PERMISSION_POST_NOTIFICATION = "android.permission.POST_NOTIFICATIONS";
        
        private bool _initOnce = false;
        private string _notiStatus;

        private string SavedNotiPermStatus
        {
            get => PlayerPrefs.GetString(nameof(SavedNotiPermStatus), "");
            set => PlayerPrefs.SetString(nameof(SavedNotiPermStatus), value);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (!_initOnce) return;
            _initOnce = true;

            _notiStatus = STATUS_DENIED;
            if (!string.IsNullOrEmpty(SavedNotiPermStatus))
            {
                _notiStatus = SavedNotiPermStatus;
            }

#if UNITY_ANDROID
            InitPlugins();
#endif
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        /// <returns></returns>
        public string GetStatus()
        {
            if (!_initOnce) Init();
#if UNITY_ANDROID
            UpdateNotificationStatus();
#endif
            return _notiStatus;
        }
        
        /// <summary>
        /// 设置授权状态
        /// </summary>
        /// <param name="status">授权状态</param>
        private void SetGrantStatus(string status)
        {
            _notiStatus = status;
            SavedNotiPermStatus = status;
        }


        public bool IsAllowed()
        {
            return _notiStatus == STATUS_GRANTED;
        }

        public void RequestPermission(Action<string> callback = null)
        {
#if UNITY_ANDROID
            RequestNotificationPermission(callback);
#endif
        }

        // -------------------- Android 获取状态逻辑 --------------------
        
#if UNITY_ANDROID

        private PermissionStatus _permissionStatus;
        
        private void TryExecute(Action handler)
        {
            try
            {
                handler?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }
        
        /// <summary>
        /// 初始化插件
        /// </summary>
        private void InitPlugins()
        {
            AndroidNotificationCenter.Initialize();
            Debug.Log($"[Noti][AND] --- Notification Service InitPlugins");
            
            UpdateNotificationStatus();
        }

        /// <summary>
        /// 更新 Notification 状态码
        /// </summary>
        private void UpdateNotificationStatus()
        {
          
            TryExecute(() =>
            {
                _permissionStatus = AndroidNotificationCenter.UserPermissionToPost;
                var status = "";
                switch (_permissionStatus)
                {
                    // case PermissionStatus.NotRequested:
                    //     _notiStatus = STATUS_NOT_DETERMINED;
                    //     break;
                    case PermissionStatus.Allowed:
                        status = STATUS_GRANTED;;
                        break;
                    default:
                        status = STATUS_DENIED;
                        break;
                }

                SetGrantStatus(status);
                Debug.LogWarning($"[SDK][AND] --- UpdateNotiStatus:{_notiStatus}  |  UserPermissionToPost:{_permissionStatus}");
            });
        }
        
        private Action<string> _onPermissionCallback;
        private PermissionCallbacks _permissionCallbacks;
        /// <summary>
        /// 请求 Android 消息权限
        /// </summary>
        /// <param name="callback"></param>
        private void RequestNotificationPermission(Action<string> callback = null)
        {
            UpdateNotificationStatus();

            if (_notiStatus == STATUS_GRANTED)
            {
                callback?.Invoke(_notiStatus);
                return;
            }
            
            _onPermissionCallback = callback;

            bool hasPermissionGranted = true;
            TryExecute(() =>
            {
                var sdkInt = GetAndroidSDKVersion();
                if (sdkInt < REQUEST_PERMISSION_SDK_VERSION)
                {
                    // 低版本处理方式
                    Debug.Log($"[SDK][Noti] --- #2 SDK {sdkInt} not requested -> open channel");
                    SetGrantStatus(STATUS_GRANTED);
                }
                else
                {
                    // SDK 33 以上，请求弹窗
                    hasPermissionGranted = Permission.HasUserAuthorizedPermission(PERMISSION_POST_NOTIFICATION);
                    if (hasPermissionGranted)
                    {
                        // 已获取授权
                        SetGrantStatus(STATUS_GRANTED);
                        callback?.Invoke(STATUS_GRANTED);
                    }
                    else
                    {
                        // 未获取授权则取请求
                        Debug.Log($"[SDK][Noti] --- #3 SDK {sdkInt} :: Ask Post Permission");
                        Permission.RequestUserPermission(PERMISSION_POST_NOTIFICATION, SetupPermissionCallbacks());
                    }
                }
                
                // Android 8 (API 26) 以后才有 Channel 概念， 需要分开判断
                // 通知相关的文档：https://developer.android.com/develop/ui/views/notifications?hl=zh-cn#:~:text=The%20notification%20channel%20has%20high,API%20level%2026)%20and%20higher.
                if (sdkInt < REQUEST_CHANNEL_SDK_VERSION) return; // 低系统版本无需创建 Channel

                if (hasPermissionGranted) return; // 用户拒绝 Notification 则无法创建 Channel
                
                // 授权后直接注册 4 个优先级渠道
                // 中台需求链接：https://docs.google.com/document/d/1aBKqXKi88tu4xhQWd46yhqWU3Pu_U5Gkiow_JdLhpLk
                RegisterNotificationChannel(GuruPushPriority.Critical);
                RegisterNotificationChannel(GuruPushPriority.Urgent);
                RegisterNotificationChannel(GuruPushPriority.High);
                RegisterNotificationChannel(GuruPushPriority.Medium);

            });
        }
        
        /// <summary>
        /// 注册消息 Channel
        /// </summary>
        /// <param name="priority"></param>
        private void RegisterNotificationChannel(GuruPushPriority priority)
        {
            try
            {
                string channel_id = priority switch
                {
                    GuruPushPriority.Critical => PUSH_CHANNEL_CRITICAL,
                    GuruPushPriority.Urgent => PUSH_CHANNEL_URGENT,
                    GuruPushPriority.High => PUSH_CHANNEL_HIGH,
                    GuruPushPriority.Medium => PUSH_CHANNEL_MEDIUM,
                    _ => PUSH_CHANNEL_CRITICAL
                };
            
                int importance = priority switch
                {
                    GuruPushPriority.Critical => IMPORTANCE_MAX,
                    GuruPushPriority.Urgent => IMPORTANCE_HIGH,
                    GuruPushPriority.High => IMPORTANCE_DEFAULT,
                    GuruPushPriority.Medium => IMPORTANCE_LOW,
                    _ => IMPORTANCE_MAX
                };

                Debug.Log($"[PUSH] --- RegisterNotificationChannel -> [{channel_id}]  priority:{priority}");
                // 注册渠道
                AndroidNotificationCenter.RegisterNotificationChannel(new AndroidNotificationChannel(
                    channel_id,
                    $"Push {priority.ToString()}",
                    $"Push notification channel for [{priority.ToString()}]",
                    (Importance)importance));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Push] {ex.Message}");
            }
        }
        
        private PermissionCallbacks SetupPermissionCallbacks()
        {
            if(_permissionCallbacks != null) DisposePermissionCallbacks();
            _permissionCallbacks = new PermissionCallbacks();
            _permissionCallbacks.PermissionGranted += OnPermissionGranted;
            _permissionCallbacks.PermissionDenied += OnPermissionDenied;
            _permissionCallbacks.PermissionDeniedAndDontAskAgain += OnPermissionDenied;
            return _permissionCallbacks;
        }
        
        private void DisposePermissionCallbacks()
        {
            if (_permissionCallbacks != null)
            {
                _permissionCallbacks.PermissionGranted -= OnPermissionGranted;
                _permissionCallbacks.PermissionDenied -= OnPermissionDenied;
                _permissionCallbacks.PermissionDeniedAndDontAskAgain -= OnPermissionDenied;
                _permissionCallbacks = null;
            }
        }
        
        /// <summary>
        /// 请求通过
        /// </summary>
        /// <param name="permissionName"></param>
        private void OnPermissionGranted(string permissionName)
        {
            if (permissionName == PERMISSION_POST_NOTIFICATION)
            {
                _notiStatus = STATUS_GRANTED;
                _onPermissionCallback?.Invoke(_notiStatus);
            }
            SetGrantStatus(STATUS_GRANTED);
            DisposePermissionCallbacks();
        }

        /// <summary>
        /// 请求拒绝
        /// </summary>
        /// <param name="permissionName"></param>
        private void OnPermissionDenied(string permissionName)
        {
            if (permissionName == PERMISSION_POST_NOTIFICATION)
            {
                _notiStatus = STATUS_DENIED;
                _onPermissionCallback?.Invoke(_notiStatus);
            }
            SetGrantStatus(STATUS_DENIED);
            DisposePermissionCallbacks();
        }
        
        private int GetAndroidSDKVersion()
        {
            int sdkInt = 999;
            TryExecute(() =>
            {
                using (AndroidJavaClass jc = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    sdkInt = jc.GetStatic<int>("SDK_INT");
                    Debug.LogWarning($"[SDK] --- Android SDK Version:{sdkInt}");
                } 
            });
            return sdkInt;
        }

        
#endif
        

    }
}