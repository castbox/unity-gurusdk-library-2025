namespace Guru.Editor.Adjust
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using AdjustSdk;

    /// <summary>
    /// 修改器API
    /// </summary>
    public static class GuruAdjustSdkAPI
    {
        // ------------ VERSION INFO ------------
        public const string Version = "1.1.0";
        public const string SdkVersion = "5.0.5";
        // ------------ VERSION INFO ------------

        public const string PackageName = "com.guru.unity.adjust";
        public static readonly string AdjustSettingsRootDir = "Assets/Guru/Editor";
        public static string AdjustSettingsAssetPath = $"{AdjustSettingsRootDir}/AdjustSettings.asset";

        public static string PackageEditorRoot
        {
            get
            {
// #if GURU_SDK_DEV
//                 return $"__packages/{PackageName}/Adjust/Editor";  
// #endif
                return $"Packages/{PackageName}/Adjust/Scripts/Editor";
            }
        }
        public static string AdjustSettingsPackagePath = $"{PackageEditorRoot}/AdjustSettings.asset";

        #region AdjustSettings

        /// <summary>
        /// 创建AdjustSettings
        /// </summary>
        /// <returns></returns>
        public static AdjustSettings LoadOrCreateAdjustSettings()
        {
            // 若原始文件存在        
            if (File.Exists(Path.Combine(Application.dataPath.Replace("Assets", ""), AdjustSettingsAssetPath)))
            {
                return AssetDatabase.LoadAssetAtPath<AdjustSettings>(AdjustSettingsAssetPath);
            }
            // 否则开始查找文件
            var guids = AssetDatabase.FindAssets($"{nameof(AdjustSettings)} t:ScriptableObject");
            int removed = 0;
            if (guids.Length > 0)
            {
                foreach (var g in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);

                    Debug.Log($"--- Found assets at path:{path}");


                    if (!path.StartsWith(AdjustSettingsRootDir))
                    {
                        AssetDatabase.DeleteAsset(path);
                        removed++;
                    }
                }
            }

            if (guids.Length == 0 || removed >= guids.Length)
            {
                return CreateDefaultAdjustSettings(); // 创建默认的AppLovin Settings 配置
            }
            
            return AssetDatabase.LoadAssetAtPath<AdjustSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        
        /// <summary>
        /// 创建AppLovinSettings 配置默认路径
        /// </summary>
        /// <returns></returns>
        private static AdjustSettings CreateDefaultAdjustSettings()
        {
            // Create Root dir
            var expDir =
                new DirectoryInfo(Path.Combine(Application.dataPath.Replace("Assets", ""), AdjustSettingsRootDir));
            if (!expDir.Exists) expDir.Create();

            // Make a new one
            var settings = ScriptableObject.CreateInstance<AdjustSettings>();
            SerializedObject so = new SerializedObject(settings);
            SerializedProperty p;
            p = so.FindProperty("m_EditorClassIdentifier._iOSFrameworkAdSupport"); // 引入 AdSupport
            if (p != null)  p.boolValue = true;
            p = so.FindProperty("m_EditorClassIdentifier._iOSFrameworkAdServices"); // 引入 AdServices
            if (p != null)  p.boolValue = true;
            so.ApplyModifiedProperties();
            AssetDatabase.CreateAsset(settings, AdjustSettingsAssetPath);
            AssetDatabase.SaveAssetIfDirty(settings);
            AssetDatabase.Refresh();
            Debug.Log($"[Guru] <color=#88ff00>--- Create AdjustSettings at:</color> \n{AdjustSettingsAssetPath}");
            return settings;
        }
        
        #endregion
        
        public static void ApplyMods()
        {
            
        }
    }
}