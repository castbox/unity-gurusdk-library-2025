using System.IO;
using UnityEditor;
using UnityEngine;

namespace Guru
{
    public class AdjustSignatureHelper
    {
        
        private static readonly string AndroidLib = "adjust-android-signature-3.13.1.aar";
        private static readonly string iOSLib = "AdjustSigSdk.a";

        public static void DeployFiles()
        {
            var dir = GetParentDir();
            var files = $"{dir}/Files";
            if (Directory.Exists(files))
            {
                string from, to;
                bool res;
                from = $"{files}/{AndroidLib}.f";
                to = $"{Application.dataPath}/Plugins/Android/{AndroidLib}";
                res = CopyFile(from, to); // 无需覆盖
                if (res) Debug.Log($"Copy <color=#88ff00>{AndroidLib} to {to}</color> success...");
                from = $"{files}/{AndroidLib}.f.meta";
                to = $"{Application.dataPath}/Plugins/Android/{AndroidLib}.meta";
                CopyFile(from, to);  // 无需覆盖
                
                from = $"{files}/{iOSLib}.f";
                to = $"{Application.dataPath}/Plugins/iOS/{iOSLib}";
                res = CopyFile(from, to);   // 无需覆盖
                if (res) Debug.Log($"Copy <color=#88ff00>{iOSLib} to {to}</color> success...");
                from = $"{files}/{iOSLib}.f.meta";
                to = $"{Application.dataPath}/Plugins/iOS/{iOSLib}.meta";
                CopyFile(from, to);  // 无需覆盖
                
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.Log($"<color=red>Files not found: {files}</color>");
            }
        }


        private static string GetParentDir()
        {
            var guids = AssetDatabase.FindAssets(nameof(AdjustSignatureHelper));
            if (guids != null && guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var dir = Directory.GetParent(Path.GetFullPath(path));
                return dir.FullName;
            }
            return Path.GetFullPath($"{Application.dataPath}/../Packages/com.guru.unity.sdk.core/Runtime/GuruAdjust/Editor/Signature");
        }

        private static bool CopyFile(string from, string to, bool overwrite = false)
        {
            if (File.Exists(to) && !overwrite)
            {
                // 如果目标文件存在， 且不允许覆写， 则不进行拷贝
                return false;
            }
            
            if (File.Exists(from))
            {
                // 确保拷贝目录存在
                var destDir = Directory.GetParent(to);
                if(destDir != null && !destDir.Exists) destDir.Create();
                
                File.Copy(from, to, overwrite);
                return true;
            }
           
            Debug.Log($"<colo=red>File not found: {from}...</color>");
            return false;
        }

    }
}