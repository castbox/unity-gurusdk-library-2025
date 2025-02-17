using System.Collections;
using System.Collections.Generic;
using Guru.Editor.Max;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Guru.Editor
{
    public class GuruMaxTest
    {
        [Test]
        public void TestSettingsPath()
        {
            var settings = AppLovinSettings.Instance;

            var path = AssetDatabase.GetAssetPath(settings.GetInstanceID());
            Debug.Log($"path: {path}");
            Debug.Log($"SdkKey: {settings.SdkKey}");
            Debug.Log($"AdMobAndroidAppId: {settings.AdMobAndroidAppId}");
            Debug.Log($"AdMobIosAppId: {settings.AdMobIosAppId}");
        }

#if GURU_SDK_DEV
        // 测试API检测地址
        [Test]
        public void Test_GetAPIPath()
        {
            var aipPath = GuruMaxSdkAPI.DevPackageRoot;
            Debug.Log($"--- aipPath: {aipPath}");
        }
#endif
    }
}
