#if UNITY_IOS

namespace Guru.Editor
{
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using UnityEditor.iOS.Xcode;
    using System;
    using System.IO;

    public class XCPrivacyInfoImporter
    {
        private const string XCPrivacyInfo = "PrivacyInfo.xcprivacy";
        private const string DefaultWorkdir = "Guru/BuildTools/Editor/IOS_POST_PRIVACYINFO";
        private const string SourceFileName = "PrivacyInfo.plist";
        private static string IosPrivacyInfoPath => $"{Application.dataPath}/Plugins/iOS/{SourceFileName}";
        
        [PostProcessBuild(0)]
        public static void OnPostProcessBuild(BuildTarget target,  string buildPath)
        {
            if (target == BuildTarget.iOS)
            {
                AddPrivacyInfo(buildPath);
            }
        }
        
        /// <summary>
        /// 向 XCode 添加隐私清单文件
        /// </summary>
        /// <param name="buildPath"></param>
        private static void AddPrivacyInfo(string buildPath)
        {
            if (CheckEvn())
            {
                var xcprojPath = PBXProject.GetPBXProjectPath(buildPath);
                var xcproj = new PBXProject();
                xcproj.ReadFromFile(xcprojPath);
                
                var dest = $"{buildPath}/{XCPrivacyInfo}";
                FileUtil.ReplaceFile(IosPrivacyInfoPath, dest);

                var mainTarget = xcproj.GetUnityMainTargetGuid();
                var guid = xcproj.AddFile(dest,$"{XCPrivacyInfo}", PBXSourceTree.Source);

                xcproj.AddFileToBuild(mainTarget, guid);
                xcproj.WriteToFile(xcprojPath);
            }
            else
            {
                Debug.LogError("Inject iOS PrivacyInfo failed!");
            }
        }
        
        /// <summary>
        /// 工作目录
        /// </summary>
        /// <returns></returns>
        private static string GetWorkdir()
        {
            var guids = AssetDatabase.FindAssets($"{nameof(XCPrivacyInfoImporter)} t:script");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var dir = Directory.GetParent(path).FullName;
                if (Directory.Exists(dir)) return dir;
            }   
            return DefaultWorkdir;
        }

        /// <summary>
        /// 检查环境
        /// </summary>
        private static bool CheckEvn()
        {
            if (File.Exists(IosPrivacyInfoPath)) return true;
            
            var workdir = GetWorkdir();
            var source = $"{workdir}/{SourceFileName}";
            var toDir = Directory.GetParent(IosPrivacyInfoPath);
            if (!toDir.Exists) toDir.Create();
            if (File.Exists(source))
            {
                FileUtil.ReplaceFile(source, IosPrivacyInfoPath);
                return true;
            }

            Debug.LogError($"--- PrivacyInfo.plist not found，Check file path：{source}");
            return false;
        }

    }
}

#endif