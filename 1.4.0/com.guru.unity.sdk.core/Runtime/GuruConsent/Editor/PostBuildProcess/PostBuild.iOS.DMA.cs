#if UNITY_IOS

namespace Guru.Editor
{
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using System.IO;
    using UnityEditor.iOS.Xcode;
    
    public class PostBuild_DMA
    {

        public static bool DefaultValue = true; // default value
        
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS) return;
            SetInfoPlist(buildPath);
        }

        /// <summary>
        /// inject default values
        /// </summary>
        /// <param name="buildPath"></param>
        private static void SetInfoPlist(string buildPath)
        {
            var infoPlistPath = Path.Combine(buildPath, "Info.plist");
            if (!File.Exists(infoPlistPath))
            {
                Debug.LogError("Info.plist not found");
                return;
            }

            var plist = new PlistDocument();
            plist.ReadFromFile(infoPlistPath);

            var root = plist.root;
            
            //--------- Add Delay Measurement ---------------
            if (GoogleDMAHelper.UsingDelayAppMeasurement)
            {
                root.SetBoolean("GADDelayAppMeasurementInit", true);
            }
            
            //--------- set all default values ----------
            root.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_STORAGE", DefaultValue);
            root.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_ANALYTICS_STORAGE", DefaultValue);
            root.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_PERSONALIZATION_SIGNALS", DefaultValue);
            root.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_USER_DATA", DefaultValue);

            plist.WriteToFile(infoPlistPath);

            Debug.Log(
                $"<color=#88ff00>[Post] consent has inject dma default values {DefaultValue} to {infoPlistPath} </color>");
        }

    }
}

#endif