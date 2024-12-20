

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
        
        
        
        
    }
}