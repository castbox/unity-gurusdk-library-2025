#if UNITY_ANDROID

namespace Guru.BuildTool
{
    using System.IO;
    using UnityEditor.Android;
    using UnityEngine;
    
    public class AndroidSettingsGradleFixer: IPostGenerateGradleAndroidProject
    {
        private const string SettingsGradleName= "settings.gradle";
        private const string K_LINE_UNITYPROJECT = "def unityProjectPath"; 
        
        public int callbackOrder => 1;

        public void OnPostGenerateGradleAndroidProject(string buildPath)
        {
            FixSettingsInAndroidProject(buildPath);
        }

        /// <summary>
        /// 设置项目中的 Settings 文件
        /// </summary>
        /// <param name="buildPath"></param>
        private void FixSettingsInAndroidProject(string buildPath)
        {
            var settingsPath = Path.GetFullPath($"{buildPath}/../{SettingsGradleName}");

            if (File.Exists(settingsPath))
            {
                bool isDirty = false;
                var lines = File.ReadAllLines(settingsPath);
                string projectPath = Path.GetFullPath($"{Application.dataPath}/../").Replace("\\", "/"); // Unity project path
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(K_LINE_UNITYPROJECT))
                    {
                        lines[i] = $"        def unityProjectPath = $/file:////{projectPath}/$.replace(\"\\\\\", \"/\")";
                        isDirty = true;
                        break;
                    }
                }

                if (isDirty)
                {
                    File.WriteAllLines(settingsPath, lines);
                    Debug.Log($"[SDK] --- Fix Unity Project Path at:{settingsPath}");
                }
                
            }
        }

    }
}

#endif