

using System.Threading;
using Cysharp.Threading.Tasks;

namespace Guru.Notification
{
    using System;
    using UnityEngine;
    using System.Collections;
#if UNITY_IOS
    using Unity.Notifications.iOS;
#endif

    public class NotificationAgentIOS : INotificationAgent
    {

        private const string STATUS_GRANTED = "granted";
        private const string STATUS_DENIDED = "denied";
        private const string STATUS_PROVISIONAL = "provisional";
        private const string STATUS_NOT_DETERMINED = "not_determined";

        private static bool _initOnce;
        private static int _waitSeconds = 30;
        private string SavedNotiPermStatus
        {
            get => PlayerPrefs.GetString(nameof(SavedNotiPermStatus), "");
            set => PlayerPrefs.SetString(nameof(SavedNotiPermStatus), value);
        }
        

        private string _notiStatus;
        
        public void Init()
        {
            if (_initOnce) return;
            _initOnce = true;

            _notiStatus = SavedNotiPermStatus;
            if (string.IsNullOrEmpty(_notiStatus))
                _notiStatus = STATUS_NOT_DETERMINED;
            
#if UNITY_IOS
            InitPlugins();
#endif
        }

        public string GetStatus()
        {
            if (!_initOnce) Init();
#if UNITY_IOS
            UpdateStatus();
#endif
            return _notiStatus;
        }

        public bool IsAllowed()
        {
            return _notiStatus == STATUS_GRANTED;
        }

        public void RequestPermission(Action<string> callback = null)
        {
            if (!_initOnce) Init();

            if (_notiStatus == STATUS_GRANTED || _notiStatus == STATUS_DENIDED)
            {
                Debug.Log($"[SDK][Noti][iOS] --- Already has Status: {_notiStatus}");
                callback?.Invoke(_notiStatus); // 已获得授权， 直接返回结果
                return;
            }
            
#if UNITY_IOS
            RequestIOSPermission(callback);
#endif
        }
        
        public void CreatePushChannels()
        {
            Debug.Log($"[SDK][Noti][iOS] --- iOS Don't need to Create Push Channels.");
        }


#if UNITY_IOS

        private void InitPlugins()
        {
            UpdateStatus();
        }
        
        /// <summary>
        /// 更新状态
        /// </summary>
        private void UpdateStatus()
        {
            string status = STATUS_NOT_DETERMINED;
            var authorizationStatus = iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus;
            switch (authorizationStatus)
            {
                case AuthorizationStatus.Authorized:
                    status = STATUS_GRANTED;
                    break;
                case AuthorizationStatus.Denied:
                    status = STATUS_DENIDED;
                    break;
                case AuthorizationStatus.NotDetermined:
                    status = STATUS_NOT_DETERMINED;
                    break;
                case AuthorizationStatus.Provisional:
                    status = STATUS_PROVISIONAL;
                    break;
                default:
                    Debug.Log($"[SDK][Noti][iOS] --- Unmarked AuthorizationStatus: {status}");
                    break;
            }

            SetGrantStatus(status);
        }

        private void SetGrantStatus(string status)
        {
            _notiStatus = status;
            SavedNotiPermStatus = status;
        }

        /// <summary>
        /// 请求 IOS 的推送
        /// </summary>
        /// <param name="callback"></param>
        private void RequestIOSPermission(Action<string> callback = null)
        {
            Debug.Log($"[SDK][Noti][iOS] --- RequestIOSPermission start");
            int timePassed = 0;
            using (var req = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge, true))
            {
                IEnumerator WaitForRequest()
                {
                    while (!req.IsFinished && timePassed < _waitSeconds)
                    {
                        timePassed++;
                        yield return new WaitForSeconds(1);
                    }
                    
                    if (req?.Granted == true)
                    {
                        Analytics.OnIOSDeviceTokenReceived(req.DeviceToken);
                    }
                    
                    if (timePassed >= _waitSeconds)
                    {
                        Debug.LogWarning($"[SDK][Noti][iOS] --- RequestIOSPermission timeout");
                    }
                    
                    UpdateStatus();
                    callback?.Invoke(_notiStatus);
                    Debug.Log($"[SDK][Noti][iOS] --- User Selected: {_notiStatus}");
                }
                
                CoroutineHelper.Instance.StartCoroutine(WaitForRequest());
            }
        }
        
#endif
        
        #if UNITY_IOS
        private static async UniTask<string?> WaitForDeviceTokenWithSameRequest(AuthorizationRequest req,
            CancellationToken cancellationToken = default)
        {
            const int maxRetries = 15; // 最大重试次数
            for (var i = 0; i < maxRetries; i++)
            {
                try
                {
                    var token = req.DeviceToken;
                    Log.I($"DeviceToken: {token} (Attempt {i + 1}/{maxRetries})");
                    if (!string.IsNullOrEmpty(token))
                    {
                        Log.I($"Device token obtained and cached: {token.ToSecretString()}");
                        return token;
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellationToken);
                }
                catch (Exception e)
                {
                    Log.W("Error while waiting for device token: " + e.Message);
                }
            }

            return null;
        }
        
        public async UniTask<string?> GetDeviceToken(float timeoutSeconds = 10f,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 先检查当前权限状态
                var settings = iOSNotificationCenter.GetNotificationSettings();
                Log.I($"Current authorization status: {settings.AuthorizationStatus}");
                Log.I($"Alert setting: {settings.AlertSetting}");
                Log.I($"Badge setting: {settings.BadgeSetting}");

                if (settings.AuthorizationStatus == AuthorizationStatus.Authorized)
                {
                    Log.I("Permission granted, requesting device token...");
                    // 已有权限，使用最小权限请求获取token
                    using var req =
                        new AuthorizationRequest(
                            AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound, true);
                    // 带超时的等待
                    await UniTask.WaitUntil(() => req.IsFinished, cancellationToken: cancellationToken)
                        .Timeout(TimeSpan.FromSeconds(timeoutSeconds));
                    
                    Log.I($"Request finished. Granted: {req.Granted}");
                    Log.I($"Device token length: {req.DeviceToken?.Length ?? 0}");
                    Log.I($"Device token value: '{req.DeviceToken}'");

                    if (req.Granted)
                    {
                        Log.I($"GetDeviceToken: Request finished successfully: {req.DeviceToken.ToSecretString()}");
                        if (string.IsNullOrEmpty(req.DeviceToken))
                        {
                            return await WaitForDeviceTokenWithSameRequest(req, cancellationToken);
                        }
                        else
                        {
                            return req.DeviceToken;  
                        }
                    }
                    else
                    {
                        Log.W($"GetDeviceToken: Request finished with error: {req.Error}");
                        return null;
                    }
                }

                Log.W($"No notification permission. Current status: {settings.AuthorizationStatus}");
                return null;
            }
            catch (TimeoutException)
            {
                Log.W($"Device token request timed out after {timeoutSeconds} seconds");
                return null;
            }
            catch (OperationCanceledException)
            {
                Log.W("Device token request was cancelled");
                return null;
            }
            catch (Exception ex)
            {
                Log.W($"Failed to get device token: {ex.Message}");
                return null;
            }

        }
#endif
        
        
    }
}