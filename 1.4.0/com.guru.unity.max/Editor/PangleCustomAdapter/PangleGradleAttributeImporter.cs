

namespace Guru.Editor
{
    using UnityEngine;
    using UnityEditor;
    using System.IO;
    using System.Text.RegularExpressions;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    
    public class PangleGradleAttributeImporter : IPreprocessBuildWithReport
    {
        private const string GRADLE_TEMPLATE_PATH = "Assets/Plugins/Android/mainTemplate.gradle";
    
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android)
            {
                Debug.Log("Preprocessing Android build - Updating Gradle template...");
                ModifyGradleTemplate();
            }
        }
        
        [InitializeOnLoadMethod]
        private static void OnProjectLoadedInEditor()
        {
            EditorApplication.delayCall += ModifyGradleTemplate;
        }
        
        /// <summary>
        /// 向 mainTemplate.gradle  注入需要的更新语句
        /// </summary>
        private static void ModifyGradleTemplate()
        {
            if (!File.Exists(GRADLE_TEMPLATE_PATH)) return;

            string content = File.ReadAllText(GRADLE_TEMPLATE_PATH);

            // 查找并替换 bytedance-adapter 实现
            string pattern = @"implementation\s+'com\.applovin\.mediation:bytedance-adapter:6\.4\.0\.5\.0'.*?$";
            string replacement = "implementation('com.applovin.mediation:bytedance-adapter:6.4.0.5.0') {\n        exclude group: 'com.pangle.global', module: 'ads-sdk'\n    }";

            content = Regex.Replace(content, pattern, replacement, RegexOptions.Multiline);

            File.WriteAllText(GRADLE_TEMPLATE_PATH, content);
            Debug.Log("Updated Gradle template file with custom bytedance configuration");
        }

        // 添加菜单项以手动触发更新
#if GURU_SDK_DEV
        [MenuItem("Tools/Guru/Update Gradle Template")]
#endif
        static void UpdateGradleTemplateMenuItem()
        {
            ModifyGradleTemplate();
        }
    }
}