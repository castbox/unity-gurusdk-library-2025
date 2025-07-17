#if UNITY_IOS
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;


namespace Guru.Editor
{
    /// <summary>
    /// 这里修复XCode16打的包在iOS18+的设备中有部分崩溃的问题，参考文档：
    /// https://discussions.unity.com/t/crash-fatal-exception-nsinternalinconsistencyexception/1551190
    /// https://discussions.unity.com/t/crashes-when-you-force-close-an-ios-app-with-another-view-controller-up/787078
    /// </summary>
    public class XCClassModifier
    {
        private static readonly string ModifyCassName = "Classes/UnityAppController.mm";

        private static Dictionary<string, string> ReplaceDict = new Dictionary<string, string>()
        {
            {"[_UnityAppController window].rootViewController = nil;", "//[_UnityAppController window].rootViewController = nil;"},
        };
        
        /// <summary>
        /// 构建操作
        /// 构建顺序 45-50 可以保证执行时序在MAX 自身生成podfile之后, 注入需要的逻辑
        /// AmazonSDK使用了45, 工具设为46,确保后发执行
        /// </summary>
        /// <param name="target"></param>
        /// <param name="projPath"></param>
        [PostProcessBuild(46)]
        private static void OnPostProcessBuild(BuildTarget target, string projPath)
        {
            if (target != BuildTarget.iOS)
                return;

            string classFilePath = Path.Combine(projPath, ModifyCassName);
            if (File.Exists(classFilePath))
            {
                string content = File.ReadAllText(classFilePath);
                foreach (var itemData in ReplaceDict)
                {
                    if (content.Contains(itemData.Value))
                        continue;
                    content = content.Replace(itemData.Key, itemData.Value);
                }
                File.WriteAllText(classFilePath, content);
                Debug.Log($"<color=#88ff00>=== Fix XCode16 + iOS18+ Crash Bug ===</color>");
            }
            else
            {
                Debug.LogError($"=== {ModifyCassName} not exists, exit pod hook...===");
            }
        }
    }
}
#endif