#if UNITY_IOS

namespace Guru.Editor
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEditor.iOS.Xcode;
    using UnityEngine;
    using System.IO;
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    /// Fireabse DebugView 开启参数注入
    /// </summary>
    public class FirebaseDebugViewImporter
    {
        public static readonly string Tag = "[POST]";
        private static readonly string CodeFixMark = "CODE_FIX_BY_GURU";
        private static readonly string CodeCmdArgsFix = $"\t\t//--------- {CodeFixMark} --------------\n\t\tNSMutableArray *newArguments = [NSMutableArray arrayWithArray:[[NSProcessInfo processInfo] arguments]];\n\t\t[newArguments addObject:@\"-FIRAnalyticsDebugEnabled\"];\n\t\t[newArguments addObject:@\"-FIRDebugEnabled\"];\n\t\t[[NSProcessInfo processInfo] setValue:[newArguments copy] forKey:@\"arguments\"];";
        private static readonly string CodeDidFinishedLaunch =
            "(BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:";

        /// <summary>
        /// 需要在外部接口调用参数注入
        /// </summary>
        public static bool EnableDebugView = false;  // 默认为False, 需要外部注入

        [PostProcessBuild(1)]
        public static void PostBuildXcodeArgs(BuildTarget target, string buildPath)
        {
            Debug.Log($"{Tag} --- Add Firebase debug args: {EnableDebugView}");
            
            if (target != BuildTarget.iOS) return;
            if (!EnableDebugView) return;

            AddLauncherArgsToSchema(buildPath);
            InjectLaunchCode(buildPath);
        }
        
        /// <summary>
        /// 添加启动参数到Scheme
        /// </summary>
        /// <param name="buildPath"></param>
        private static void AddLauncherArgsToSchema(string buildPath)
        {
            string schemePath = buildPath + "/Unity-iPhone.xcodeproj/xcshareddata/xcschemes/Unity-iPhone.xcscheme";

            var xcscheme = new XcScheme();
            xcscheme.ReadFromFile(schemePath);

            xcscheme.SetMetalValidationOnRun(XcScheme.MetalValidation.Extended);
            xcscheme.SetFrameCaptureModeOnRun(XcScheme.FrameCaptureMode.Metal);
            xcscheme.AddArgumentPassedOnLaunch("-FIRDebugEnabled");

            xcscheme.WriteToFile(schemePath);
        }
        
        /// <summary>
        /// 注入命令行参数
        /// </summary>
        /// <param name="buildPath"></param>
        private static void InjectLaunchCode(string buildPath)
        {
            string path = $"{buildPath}/Classes/UnityAppController.mm";

            if (File.Exists(path))
            {
                List<string> lines = Enumerable.ToList(File.ReadAllLines(path));
                string line = "";
                int idx = -1;
                for (int i = 0; i < lines.Count; i++)
                {
                    line = lines[i];
                    if (line.Contains(CodeDidFinishedLaunch))
                    {
                        // 找到注入行
                        idx = i + 1;
                        if (lines[idx].Contains("{"))
                        {
                            idx++;
                        }
                        if (lines[idx].Contains(@"::printf(""-> applicationDidFinishLaunching()\n"");"))
                        {
                            idx++;
                        }

                        if (lines[idx].Contains(CodeFixMark) || lines[idx+1].Contains(CodeFixMark))
                        {
                            Debug.Log($"{Tag} <color=orange>---- code has already injected, skip... </color>");
                            return;
                        }
                        
                        break;
                    }
                }
                lines.Insert(idx, CodeCmdArgsFix);
                File.WriteAllLines(path, lines.ToArray());
                Debug.Log($"{Tag} <color=#88ff00>---- code has success injected.</color> path:\n{path}");
            }
            else
            {
                Debug.Log($"{Tag} <color=red>---- file not found: {path}, inject failed... </color>");
            }
        }

    }
}

#endif