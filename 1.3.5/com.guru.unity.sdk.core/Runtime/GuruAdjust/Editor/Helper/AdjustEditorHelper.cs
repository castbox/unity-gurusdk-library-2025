namespace Guru.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEngine; 
    using System;

    /// <summary>
    /// Adjust SDK 编辑器辅助工具类
    /// </summary>
    public class AdjustEditorHelper 
    {
        // 需要清理的旧版本文件路径列表
        private static readonly string[] LEGACY_FILE_PATHS = new string[]
        {
            "Plugins/Android/adjust-android-signature-3.13.1.aar",
            "Plugins/iOS/AdjustSigSdk.a"
        };

        /// <summary>
        /// 清理不再使用的旧版本文件
        /// </summary>
        public static void RemoveOldSignatureFiles()
        {
            foreach(var filePath in LEGACY_FILE_PATHS)
            {
                var fullPath = Path.GetFullPath($"{Application.dataPath}/{filePath}");
                if (File.Exists(fullPath))
                {
                    try
                    {
                        File.Delete(fullPath);
                        Debug.Log($"<color=green>[Adjust] 成功删除旧文件: {filePath}</color>");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"<color=red>[Adjust] 删除文件失败: {filePath}, 错误信息: {ex.Message}</color>");
                    }
                }
            }
        }
    }
}