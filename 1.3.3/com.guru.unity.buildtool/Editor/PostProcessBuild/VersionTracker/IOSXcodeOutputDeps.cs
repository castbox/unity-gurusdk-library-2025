#if UNITY_IOS

using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Guru.Editor
{
    public class IOSXcodeOutputDeps
    {
        // <summary>
        /// 构建操作
        /// 构建顺序 45-50 可以保证执行时序在MAX 自身生成podfile之后, 注入需要的逻辑
        /// AmazonSDK使用了45, 工具设为 > 45, 确保后发执行
        /// </summary>
        /// <param name="target"></param>
        /// <param name="projPath"></param>
        [PostProcessBuild(1000)]
        private static void OnPostProcessBuild(BuildTarget target, string projPath)
        {
            string podlock = Path.Combine(projPath, "Podfile.lock");
            if (File.Exists(podlock))
            {
                VersionTrackerHelper.InstallAndRun(projPath);
            }
            else
            {
                Debug.LogError($"=== POD install not success, exit deps hook...===");
            }
        }
    }
}


#endif