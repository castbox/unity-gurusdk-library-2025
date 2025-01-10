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
        private const string FCM_DEFAULT_CHANNEL_ID = "default_notification_channel_id";
        private const string FCM_DEFAULT_CHANNEL_NAME = "fcm_default_channel";
        private const string FCM_DEFAULT_CHANNEL_DESC = "Default channel for firebase cloud messaging";
        private const string STATUS_GRANTED = "granted";
        private const string STATUS_DENIED = "denied";
        // private const string STATUS_NOT_DETERMINED = "not_determined";
        private const int REQUEST_PERMISSION_SDK_VERSION = 33;
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
            UpdateNotiStatus();
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
            RequestAndroidPermission(callback);
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
            
            UpdateNotiStatus();
        }

        /// <summary>
        /// 更新 Notification 状态码
        /// </summary>
        private void UpdateNotiStatus()
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
        private void RequestAndroidPermission(Action<string> callback = null)
        {
            UpdateNotiStatus();

            if (_notiStatus == STATUS_GRANTED)
            {
                callback?.Invoke(_notiStatus);
                return;
            }
            
            _onPermissionCallback = callback;
            TryExecute(() =>
            {
                var sdkInt = GetAndroidSDKVersion();
                if (sdkInt < REQUEST_PERMISSION_SDK_VERSION)
                {
                    // 低版本处理方式
                    Debug.Log($"[SDK][Noti] --- #2 SDK {sdkInt} not requested -> open channel");
                    SetGrantStatus(STATUS_GRANTED);
                    
                    // 注册消息频道
                    AndroidNotificationCenter.RegisterNotificationChannel(new AndroidNotificationChannel(
                        FCM_DEFAULT_CHANNEL_ID, 
                        FCM_DEFAULT_CHANNEL_NAME, 
                        FCM_DEFAULT_CHANNEL_DESC, 
                        Importance.High));
                }
                else
                {
                    // SDK 33 以上，请求弹窗
                    bool hasPermission = Permission.HasUserAuthorizedPermission(PERMISSION_POST_NOTIFICATION);
                    if (hasPermission)
                    {
                        SetGrantStatus(STATUS_GRANTED);
                        callback?.Invoke(STATUS_GRANTED);
                        return;
                    }
                    Debug.Log($"[SDK][Noti] --- #3 SDK {sdkInt} :: Ask Post Permission");
                    Permission.RequestUserPermission(PERMISSION_POST_NOTIFICATION, SetupPermissionCallbacks());
                }
            });
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