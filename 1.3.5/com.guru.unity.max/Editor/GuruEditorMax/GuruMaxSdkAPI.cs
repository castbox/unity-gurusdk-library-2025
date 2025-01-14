using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using Debug = UnityEngine.Debug;

namespace Guru.Editor.Max
{

    /// <summary>
    /// GuruMaxIntegrationManager Support for MaxPlugins
    /// </summary>
    public class GuruMaxSdkAPI
    {
        // ------------ VERSION INFO ------------
        public const string Version = "0.1.0";
        public const string SdkVersion = "6.1.2";
        // ------------ VERSION INFO ------------

        public const string PackageName = "com.guru.unity.max";
        private static readonly string AppLovinSettingsRootDir = "Assets/Guru/Resources";
        private static string AppLovinSettingsAssetPath = $"{AppLovinSettingsRootDir}/AppLovinSettings.asset";
        
        public static bool DefaultQualityServiceEnabled = true;
        public static bool DefaultUseMaxConsentFlow = false;
        public static bool DefaultAttributionReportEndpoint = true;
        public static bool DefaultAddApsSkAdNetworkIds = true;
        
        

        /// <summary>
        /// GuruMaxIntegrationManager Max 的根目录地址
        /// </summary>
        public static string PackageDataPath
        {
            get
            {
#if GURU_SDK_DEV
                return DevPackageRoot;
#endif
                return $"Packages/{PackageName}";

            }
        }


#if GURU_SDK_DEV
        private static readonly string DefaultDevPackageRoot = $"Assets/__upm/{PackageName}";
        private static string _devPackageRoot = "";
        public static string DevPackageRoot
        {
            get
            {
                if (string.IsNullOrEmpty(_devPackageRoot))
                {
                    _devPackageRoot = DefaultDevPackageRoot;
                    var assets = AssetDatabase.FindAssets($"GuruMaxSdkAPI t:script");
                    if (assets != null && assets.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                        if (File.Exists(path) && path.Replace("\\", "/").Contains("/Editor"))
                        {
                            _devPackageRoot = path.Replace("/Editor", ",").Split(',')[0];
                        }
                    }
                }
                return _devPackageRoot;
            }
        }
#endif    



        /// <summary>
        /// 加载并修复AppLovinSettings组件路径和位置
        /// </summary>
        public static AppLovinSettings LoadOrCreateAppLovinSettings()
        {

            // 若原始文件存在        
            if (File.Exists(Path.Combine(Application.dataPath.Replace("Assets", ""), AppLovinSettingsAssetPath)))
            {
                return AssetDatabase.LoadAssetAtPath<AppLovinSettings>(AppLovinSettingsAssetPath);
            }

            // 否则开始查找文件
            var guids = AssetDatabase.FindAssets("AppLovinSettings t:ScriptableObject");

            int removed = 0;
            if (guids.Length > 0)
            {
                foreach (var g in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);

                    Debug.Log($"--- Found assets at path:{path}");


                    if (!path.StartsWith(AppLovinSettingsRootDir))
                    {
                        AssetDatabase.DeleteAsset(path);
                        removed++;
                    }
                }
            }

            if (guids.Length == 0 || removed >= guids.Length)
            {
                return CreateDefaultAppLovinSettings(); // 创建默认的AppLovin Settings 配置
            }


            return AssetDatabase.LoadAssetAtPath<AppLovinSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));


        }

        /// <summary>
        /// 创建AppLovinSettings 配置默认路径
        /// </summary>
        /// <returns></returns>
        private static AppLovinSettings CreateDefaultAppLovinSettings()
        {
            // Create Root dir
            var expDir =
                new DirectoryInfo(Path.Combine(Application.dataPath.Replace("Assets", ""), AppLovinSettingsRootDir));
            if (!expDir.Exists) expDir.Create();

            // Make a new one
            var settings = ScriptableObject.CreateInstance<AppLovinSettings>();
            settings.QualityServiceEnabled = DefaultQualityServiceEnabled;
            settings.SetAttributionReportEndpoint = DefaultAttributionReportEndpoint;
            settings.ConsentFlowEnabled = DefaultUseMaxConsentFlow;
            settings.AddApsSkAdNetworkIds = DefaultAddApsSkAdNetworkIds;
            AssetDatabase.CreateAsset(settings, AppLovinSettingsAssetPath);
            AssetDatabase.SaveAssetIfDirty(settings);
            AssetDatabase.Refresh();

            Debug.Log($"[Guru] <color=#88ff00>--- Create AppLovinSettings at:</color> \n{AppLovinSettingsAssetPath}");

            return settings;
        }


        /// <summary>
        /// 是否显示MAX菜单
        /// </summary>
        /// <param name="active"></param>
        public static void SetMaxMenuActive(bool active)
        {
            if (active)
            {
                ApplovinMod.MenuItemsRecover();
            }
            else
            {
                ApplovinMod.MenuItemsHide();
            }
            CompilationPipeline.RequestScriptCompilation();
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private static void MoveFile(string from, string to)
        {
            if (!File.Exists(from))
            {
                Debug.Log($"<color=orange>File not found: {from}</color>");
                return;
            }

            if (File.Exists(to)) File.Delete(to);

            File.Move(from, to);
            Debug.Log($"<color=#88ff00>File move: {from} to \n{to}</color>");
        }

    }


}