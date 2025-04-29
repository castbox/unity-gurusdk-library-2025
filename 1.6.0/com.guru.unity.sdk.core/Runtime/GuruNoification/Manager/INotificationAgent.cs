namespace Guru.Notification
{
    using System;
    public interface INotificationAgent
    {
        void Init();

        string GetStatus();

        bool IsAllowed();

        void RequestPermission(Action<string> callback = null);
        
        void CreatePushChannels();
    }
}