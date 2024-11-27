
namespace Guru.Editor
{ 
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using System;
    
    public class AdjustEditorHelper
    {
        private static string[] oldFilesPath = new string[]
        {
            "Plugins/Android/adjust-android-signature-3.13.1.aar",
            "Plugins/iOS/AdjustSigSdk.a"
        };
        
        /// <summary>
        /// 移除不在使用的老文件
        /// </summary>
        public static void RemoveOldSignatureFiles()
        {
            foreach(var p in oldFilesPath)
            {
                if (File.Exists(p))
                {
                    File.Delete(p);
                    Debug.Log($"<color=orange>--- Remove old file: {p}</color>");
                }
            }
        }


    }
}