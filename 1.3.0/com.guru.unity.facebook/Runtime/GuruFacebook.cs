using System;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;

namespace Guru.FacebookUnitySDK
{
    public class GuruFacebook
    {
        private static GuruFacebook _instance;
        public static GuruFacebook Instance => _instance ??= new GuruFacebook();

        #region GuruFB.Init

        private Action<bool> _initCompletedCallback;

        // Start is called before the first frame update
        public void Init(Action<bool> initCompletedCallback)
        {
            Debug.Log("Facebook init");

            this._initCompletedCallback = initCompletedCallback;

            if (!FB.IsInitialized)
            {
                Debug.Log("Facebook do init");

                // Initialize the Facebook SDK
                FB.Init(InitCallback, OnHideUnity);
            }
            else
            {
                Debug.Log("Facebook already init");

                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
                this._initCompletedCallback?.Invoke(true);
            }
        }

        private void InitCallback()
        {
            if (FB.IsInitialized)
            {
                Debug.Log("Success to Initialize the Facebook SDK");

                // Signal an app activation App Event
                FB.ActivateApp();
                this._initCompletedCallback?.Invoke(true);
                // Continue with Facebook SDK
                // ...
            }
            else
            {
                this._initCompletedCallback?.Invoke(false);
                Debug.LogError("Failed to Initialize the Facebook SDK");
            }
        }

        #endregion

        #region GuruFB.Login

        public void Login()
        {
            if (!FB.IsInitialized)
            {
                Debug.LogError("Facebook SDK is not initialized");
            }

            Debug.Log("Facebook Login");
            var perms = new List<string>() {"public_profile", "email"};
            FB.LogInWithReadPermissions(perms, AuthCallback);
        }

        private void AuthCallback(ILoginResult result)
        {
            if (FB.IsLoggedIn)
            {
                // AccessToken class will have session details
                var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
                // Print current access token's User ID
                Debug.Log(aToken.UserId);
                // Print current access token's granted permissions
                foreach (string perm in aToken.Permissions)
                {
                    Debug.Log(perm);
                }
            }
            else
            {
                Debug.Log("User cancelled login");
            }
        }

        #endregion

        #region GuruFB.LogEvent

        public void LogAppEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!FB.IsInitialized)
            {
                Debug.LogError("Facebook SDK is not initialized");
            }

            Debug.Log($"Facebook LogEvent : {eventName}");
            FB.LogAppEvent(eventName, null, parameters);
        }

        #endregion


        #region GuruFB.Internal

        private void OnHideUnity(bool isGameShown)
        {
            Debug.Log("Facebook onHideUnity");

            if (!isGameShown)
            {
                // Pause the game - we will need to hide
                Time.timeScale = 0;
            }
            else
            {
                // Resume the game - we're getting focus again
                Time.timeScale = 1;
            }
        }

        #endregion
    }
}