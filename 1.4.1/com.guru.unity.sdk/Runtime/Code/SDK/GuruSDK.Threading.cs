namespace Guru
{
    using System;
    using UnityEngine;
    
    public partial class GuruSDK
    {
        private ThreadHandler _threadHandler;

        private void InitThreadHandler()
        {
            _threadHandler = new ThreadHandler();
            RegisterUpdater(_threadHandler);
        }

        private void AddActionToMainThread(Action action)
        {
            _threadHandler?.AddAction(action);
        }


        public static void RunOnMainThread(Action action)
        {
           Instance.AddActionToMainThread(action);
        }
        
        
    }
}