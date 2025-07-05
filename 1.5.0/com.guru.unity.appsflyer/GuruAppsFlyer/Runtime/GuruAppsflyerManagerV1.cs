#nullable enable
using System;
using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;

namespace Guru
{
    /// <summary>
    /// AppsFlyer attribution manager for handling conversion data and deep links
    /// </summary>
    public class GuruAppsflyerManagerV1 : MonoBehaviour, IAppsFlyerConversionData
    {
        public static GuruAppsflyerManagerV1? Instance { get; private set; }

        // Events for conversion data
        // private readonly BehaviorSubject<AppsflyerConversionData> _conversionDataSubject =
        //     new(new AppsflyerConversionData());
        //
        // private readonly BehaviorSubject<string> _conversionDataFailedSubject = new("");
        //
        // private readonly BehaviorSubject<Dictionary<string, object>> _appOpenAttributionSubject =
        //     new(new Dictionary<string, object>());
        //
        // private readonly BehaviorSubject<string> _appOpenAttributionFailedSubject = new("");
        //
        // public Observable<AppsflyerConversionData> ObservableConversionData => _conversionDataSubject;
        //
        // public Observable<string> ObservableConversionDataFailed => _conversionDataFailedSubject;
        //
        // public Observable<Dictionary<string, object>> ObservableAppOpenAttribution => _appOpenAttributionSubject;
        //
        // public Observable<string> ObservableAppOpenAttributionFailed => _appOpenAttributionFailedSubject;
        //
        // public AppsflyerConversionData ConversionData => _conversionDataSubject.Value;
        //
        // public Dictionary<string, object> AppOpenAttribution => _appOpenAttributionSubject.Value;
        //
        // public string ConversionDataFailed => _conversionDataFailedSubject.Value;
        //
        // public string AppOpenAttributionFailed => _appOpenAttributionFailedSubject.Value;

        
        public Action<AppsflyerConversionData> OnGetConversionData { get; set; }
        public Action<string> OnGetConversionDataFailed { get; set; }

        public Action<Dictionary<string, object>> OnAppOpenAttributionSubject { get; set; }
        public Action<string> OnAppOpenAttributionSubjectFailed { get; set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("AppsFlyerAttributionManager initialized");
            }
            else
            {
                Destroy(gameObject);
            }
        }


        #region IAppsFlyerConversionData Implementation

        // ----------- 外部调用传入值 -----------------
        
        public void onConversionDataSuccess(string conversionData)
        {
            try
            {
                Debug.Log($"Conversion data received: {conversionData}");

                var data = new AppsflyerConversionData(conversionData);

                // Trigger event
                // _conversionDataSubject.AddEx(data);
                OnGetConversionData?.Invoke(data);
                
                // Log important conversion data
                LogConversionData(data);
            }
            catch (Exception e)
            {
                string msg = $"Failed to process conversion data {e.Message}";
                Log.W(msg);
                // _conversionDataFailedSubject.AddIfChanged($"Failed to process: {e.Message}");
                OnGetConversionDataFailed?.Invoke(msg);
            }
            
        }

        public void onConversionDataFail(string error)
        {
            string msg = $"Conversion data failed: {error}";
            Log.W(msg);
            // _conversionDataFailedSubject.AddIfChanged(error);
            OnGetConversionDataFailed?.Invoke(msg);
        }

        public void onAppOpenAttribution(string attributionData)
        {
            try
            {
                Log.I($"App open attribution received: {attributionData}");

                Dictionary<string, object> data = AppsFlyer.CallbackStringToDictionary(attributionData);
                if (data == null) return;

                // _appOpenAttributionSubject.AddIfChanged(data);
                OnAppOpenAttributionSubject?.Invoke(data);
            }
            catch (Exception e)
            {
                var msg = $"Failed to process app open attribution: {e.Message}";
                Log.W(msg);
                // _appOpenAttributionFailedSubject.AddIfChanged($"Failed to process: {e.Message}");
                OnAppOpenAttributionSubjectFailed?.Invoke(msg);
            }
        }

        public void onAppOpenAttributionFailure(string error)
        {
            Log.W($"App open attribution failed: {error}");
            // _appOpenAttributionFailedSubject.AddIfChanged(error);
            OnAppOpenAttributionSubjectFailed?.Invoke(error);
        }

        #endregion

        #region Private Helper Methods

        private void LogConversionData(AppsflyerConversionData conversionData)
        {
            try
            {
                var isOrganic = conversionData.IsOrganicInstall();
                var mediaSource = conversionData.GetMediaSource();
                var campaign = conversionData.GetCampaign();
                var installTime = conversionData.GetInstallTime();

                Log.I($"Conversion Data Summary:");
                Log.I($"  - Organic: {isOrganic}");
                Log.I($"  - Media Source: {mediaSource ?? "N/A"}");
                Log.I($"  - Campaign: {campaign ?? "N/A"}");
                Log.I($"  - Install Time: {installTime?.ToString() ?? "N/A"}");

                conversionData.Dump();
            }
            catch (Exception e)
            {
                Log.W($"Failed to log conversion data: {e.Message}");
            }
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}