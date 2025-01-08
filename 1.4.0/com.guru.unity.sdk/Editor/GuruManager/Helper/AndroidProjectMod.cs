

namespace Guru.Editor
{
    using UnityEditor;
    using UnityEngine;
    using System;
    using System.IO;
    using System.Linq;
    
    public class AndroidProjectMod
    {
        private const int TargetSDKVersion = 34;
        private const string K_ANDROID_PLUGINS_NAME = "Plugins/Android";
        
        private const string LauncherName = "launcherTemplate";
        private static readonly string LauncherFullPath = Path.Combine(Application.dataPath, $"{K_ANDROID_PLUGINS_NAME}/{LauncherName}.gradle");
        
        private const string MainName = "mainTemplate";
        private static readonly string MainFullPath = Path.Combine(Application.dataPath,  $"{K_ANDROID_PLUGINS_NAME}/{MainName}.gradle");
        
        private const string BaseProjectName = "baseProjectTemplate";
        private static readonly string BaseProjectFullPath = Path.Combine(Application.dataPath,  $"{K_ANDROID_PLUGINS_NAME}/{BaseProjectName}.gradle");
        
        private const string PropertiesName = "gradleTemplate";
        private const string K_ENABLE_R8 = "android.enableR8";
        private static readonly string PropertiesFullPath = Path.Combine(Application.dataPath,  $"{K_ANDROID_PLUGINS_NAME}/{PropertiesName}.properties");
        
        private const string SettingsName = "settingsTemplate";
        private static readonly string SettingsFullPath = Path.Combine(Application.dataPath,  $"Plugins/Android/{SettingsName}.gradle");
        private const string K_LINE_UNITY_PROJECT = "def unityProjectPath";
        
        
        private const string ProguardUserName = "proguard-user";
        private static readonly string ProguardUserFullPath = Path.Combine(Application.dataPath,  $"{K_ANDROID_PLUGINS_NAME}/{ProguardUserName}.txt");
        
        public static void Apply()
        {
            ApplyLauncher();
            ApplyBaseProjectTemplates();
            ApplyMainTemplates();
            ApplyGradleTemplate();
            ApplySettings();
            ApplyProguardUser();
            CheckTargetSDKVersion();  // 强制修复构建版本号
        }
        
        private static void ApplyLauncher()
        {
            if (!File.Exists(LauncherFullPath))
            {
                CopyFile($"{LauncherName}.txt", LauncherFullPath);
                Debug.Log($"[MOD] --- Copy file to: {LauncherFullPath}");
                return;
            }

            var ptn1 = "**PACKAGING_OPTIONS**";
            var ptn2 = "abortOnError false";
            var lines = File.ReadAllLines(LauncherFullPath);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Contains(ptn1))
                {
                    lines[i] = line.Replace(ptn1, "\n\n\tpackagingOptions {\n\t\texclude(\"META-INF/*.kotlin_module\")\n\t}\n\n");
                }

                if (line.Contains(ptn2))
                {
                    if (lines[i + 1].Contains("}"))
                    {
                        lines[i + 1] = lines[i + 1].Replace("}", "\tcheckReleaseBuilds false\n\t}");
                    }
                }
            }
            Debug.Log($"[MOD] --- Fix file at: {LauncherFullPath}");
            File.WriteAllLines(LauncherFullPath, lines);

        }
        private static void ApplyMainTemplates()
        {
            if (!File.Exists(MainFullPath))
            {
                Debug.Log($"[MOD] --- Copy file to: {MainFullPath}");
                CopyFile($"{MainName}.txt", MainFullPath);
            }
        }
        
        private static void ApplyBaseProjectTemplates()
        {
            if (!File.Exists(BaseProjectFullPath))
            {
                Debug.Log($"[MOD] --- Copy file to: {BaseProjectFullPath}");
                CopyFile($"{BaseProjectName}.txt", BaseProjectFullPath);
            }
        }
        
        private static void ApplyGradleTemplate()
        {
            if (!File.Exists(PropertiesFullPath))
            {
                Debug.Log($"[MOD] --- Copy file to: {PropertiesFullPath}");
                CopyFile($"{PropertiesName}.txt", PropertiesFullPath);
            }

            if (TargetSDKVersion > 33)
            {
                FixGradleTemplate(PropertiesFullPath);
            }
        }

        /// <summary>
        /// 该版本中不再使用 R8
        /// </summary>
        /// <param name="filePath"></param>
        private static void FixGradleTemplate(string filePath)
        {
            if (File.Exists(filePath))
            {
                bool isDirty = false;
                var lines = File.ReadAllLines(filePath);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(K_ENABLE_R8))
                    {
                        lines[i] = $"# {lines[i]}"; // 禁用R8
                        isDirty = true;
                        break;
                    }
                }

                if (isDirty) File.WriteAllLines(filePath, lines);
            }
        }


        /// <summary>
        /// 写入 settings.gradle 配置文件
        /// </summary>
        private static void ApplySettings()
        {
            if (!File.Exists(SettingsFullPath))
            {
                CopyFile($"{SettingsName}.txt", SettingsFullPath);
            }
            FixProjectPathInSettings(SettingsFullPath);
        }
        
        private static void FixProjectPathInSettings(string settingsPath)
        {
            bool isDirty = false;
            if (File.Exists(settingsPath))
            {
                string projectPath = Path.GetFullPath($"{Application.dataPath}/../").Replace("\\", "/");
                var lines = File.ReadAllLines(settingsPath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(K_LINE_UNITY_PROJECT))
                    {
                        lines[i] = $"        def unityProjectPath = $/file:////{projectPath}/$.replace(\"\\\\\", \"/\")";
                        isDirty = true;
                        break;
                    }
                }
                
                if(isDirty)
                    File.WriteAllLines(settingsPath, lines);
            }
        }
        
        
        /// <summary>
        /// 写入所有的配置文件
        /// </summary>
        private static void ApplyProguardUser()
        {
            if (!File.Exists(ProguardUserFullPath))
            {
                CopyFile($"{ProguardUserName}.txt", ProguardUserFullPath);
            }
        }

        private static void CheckTargetSDKVersion()
        {
            var ver = (int) PlayerSettings.Android.targetSdkVersion;
            if (ver < TargetSDKVersion)
            {
                Debug.Log($"[MOD] --- Fix target sdk version -> {TargetSDKVersion}");
                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)TargetSDKVersion;
            }
        }

        #region File IO

        private static string GetMoveFilePath(string fileName)
        {
            var path = GuruEditorHelper.GetAssetPath(nameof(AndroidProjectMod), "Script", true);
            var files = Path.GetFullPath($"{path}/../../Files");
            return $"{files}/{fileName}";
        }
        private static void CopyFile(string fileName, string toPath)
        {
            var from = GetMoveFilePath(fileName);
            if (!string.IsNullOrEmpty(from))
            {
                if (File.Exists(from))
                {
                    File.Copy(from, toPath);
                }
            }
        }

        #endregion


        
    }
}