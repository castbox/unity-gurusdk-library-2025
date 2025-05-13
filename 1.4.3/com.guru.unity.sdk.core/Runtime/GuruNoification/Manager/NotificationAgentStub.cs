

namespace Guru.Notification
{
    
    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    
    /// <summary>
    /// For Editor to use Notifications
    /// </summary>
    public class NotificationAgentStub: INotificationAgent
    {
        private const string STATUS_GRANTED = "granted";
        private const string STATUS_DENIDED = "denied";
        private const string STATUS_NOT_DETERMINED = "not_determined";
        
        private Action<string> _onPermissionCallback;
        private float _delaySeconds = 1.0f;

        private string EditorGrantedStatus
        {
            get => PlayerPrefs.GetString(nameof(EditorGrantedStatus), STATUS_NOT_DETERMINED);
            set => PlayerPrefs.SetString(nameof(EditorGrantedStatus), value);
        }
        
        public void Init()
        {
            Debug.Log($"[SDK][Noti][EDT] --- NotificationAgentStub Init: {EditorGrantedStatus}");
        }

        public string GetStatus() => EditorGrantedStatus;

        public bool IsAllowed()
        {
            return EditorGrantedStatus == STATUS_GRANTED;
        }

        public void RequestPermission(Action<string> callback = null)
        {
            Debug.Log($"[SDK][Noti][EDT] --- RequestPermission ---");
            _onPermissionCallback = callback;
            DelayCallPermissionHandle();
        }
        
        /// <summary>
        /// 延迟模拟回调
        /// </summary>
        private async void DelayCallPermissionHandle()
        {
            await Task.Delay((int)(1000 * _delaySeconds));
            EditorGrantedStatus = STATUS_GRANTED;
            _onPermissionCallback?.Invoke(EditorGrantedStatus);
        }

        public void CreatePushChannels()
        {
            Debug.Log($"[SDK][Noti][EDT] --- CreatePushChannels ---");
        }

    }
}