
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine; 
using System;

namespace Guru.Editor
{
    /// <summary>
    /// Adjust SDK 编辑器辅助工具类
    /// </summary>
    public static class AdjustFileCleaner 
    {
        // 需要清理的旧版本文件路径列表
        private static readonly string[] AndroidUnusedFiles = new string[]
        {
            "adjust-android-signature-"
        };
        
        private static readonly string[] iOSUnusedFiles = new string[]
        {
            "AdjustSigSdk"
        };
        
        // private static readonly string[] LegacyFilePaths = new string[]
        // {
        //     "Plugins/Android/adjust-android-signature-3.13.1.aar",
        //     "Plugins/iOS/AdjustSigSdk.a"
        // };

        /// <summary>
        /// 清理不再使用的旧版本文件
        /// </summary>
        public static void RemoveOldSignatureFiles()
        {
            
            try
            {
                var fullPath = Path.GetFullPath($"{Application.dataPath}/Plugins/Android/");
                RemoveUnusedFilesAtPath(fullPath, AndroidUnusedFiles);
                
                
                fullPath = Path.GetFullPath($"{Application.dataPath}/Plugins/iOS/");
                RemoveUnusedFilesAtPath(fullPath, iOSUnusedFiles);
            }
            catch (Exception ex)
            {
                Debug.Log($"<color=red>[Adjust] 删除文件失败, 错误信息: {ex.Message}</color>");
            }
            
        }


        private static void RemoveUnusedFilesAtPath(string rootDir, string[] patten)
        {
            var direInfo = new DirectoryInfo(rootDir);
            if (!direInfo.Exists)
            {
                Debug.LogError($"Directory {rootDir} does not exist.");
                return;
            }

            var removeFiles = new List<FileInfo>();
            
            foreach (var file in direInfo.GetFiles())
            {
                if (file.Exists)
                {
                    foreach (var p in patten)
                    {
                        if (file.Name.Contains(p))
                        {
                            removeFiles.Add(file);
                        }
                    }
                }
            }

            if (removeFiles.Count == 0)
            {
                Debug.Log("<color=yellow>No removed files found.</color>");
                return;
            }

            foreach (var f in removeFiles)
            {
                Debug.Log($"--- Remove {f.FullName}.");
                f.Delete();
            }

            AssetDatabase.Refresh();
        }

    }
}