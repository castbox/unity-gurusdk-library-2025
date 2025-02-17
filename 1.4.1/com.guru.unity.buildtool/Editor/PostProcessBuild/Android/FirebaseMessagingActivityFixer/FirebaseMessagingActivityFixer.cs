#if UNITY_ANDROID
namespace Guru.BuildTool
{
    using System.IO;
    using UnityEngine;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;

    /// <summary>
    /// MessagingUnityPlayerActivity.java 修改器
    /// 用于在构建的时候将 super.onCreate(savedInstanceState); 替换为 super.onCreate(null);
    /// 可在 PlayerSettings 内指定 NO_FIRBASE_ACTIVITY_FIX 来进行屏蔽
    /// </summary>
    public class FirebaseMessagingActivityFixer: IPreprocessBuildWithReport
    {
        /// <summary>
        /// 执行顺序
        /// </summary>
        public int callbackOrder => 10;

        private const string SOURCE_PATH = "Plugins/Android";
        private const string TARGET_NAME = "MessagingUnityPlayerActivity.java";
        private const string CONTENT_NEED_TO_FIX = "super.onCreate(savedInstanceState);";
        private const string CONTENT_FIXED = "super.onCreate(null);";
        

        public void OnPreprocessBuild(BuildReport report)
        {
#if NO_FIRBASE_ACTIVITY_FIX
            return;
#endif
            var targetPath = GetTargetPath();
            if (!File.Exists(targetPath))
            {
                Debug.LogWarning($"[POST] --- Target file not exist: {targetPath}");
                return;
            }

            var conents = File.ReadAllText(targetPath);

            if (string.IsNullOrEmpty(conents))
            {
                Debug.LogWarning($"[POST] --- Target is empty: {targetPath}");
                return;
            }

            if (!conents.Contains(CONTENT_NEED_TO_FIX))
            {
                Debug.LogWarning($"[POST] --- Target is no need to fix: {targetPath}");
                return;
            }
            
            var newContents = conents.Replace(CONTENT_NEED_TO_FIX, CONTENT_FIXED);
            File.WriteAllText(targetPath, newContents);
            Debug.Log($"[POST] --- <color=#88ff00>Target file has been fixed: {targetPath}</color>");
        }
        
        private string GetTargetPath()
        {
            return Path.Combine(Application.dataPath, SOURCE_PATH, TARGET_NAME);
        }

        



    }
}

#endif