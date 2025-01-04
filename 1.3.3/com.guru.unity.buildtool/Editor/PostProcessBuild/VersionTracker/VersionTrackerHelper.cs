namespace Guru.Editor
{
    using System;
    using UnityEditor;
    using System.Diagnostics;
    using System.IO;
    using UnityEngine;
    using Debug=UnityEngine.Debug;
    using System.Collections.Generic;
    
    /// <summary>
    /// Version Track 部署器
    /// 注意：此版本目前只支持 MacOS 运行
    /// </summary>
    public static class VersionTrackerHelper
    {
        private const string Version = "1.2.0";
        
        private const string DEPS_SH_NAME = "deps.sh";
        private const string DEPS_ENV_NAME = ".deps_env";
        private const string DEPAUDIT_CMD_NAME = "depaudit";
        private const string DEPAUDIT_PY_PATH = "~/.guru/guru_config/buildtools/depaudit.py";
        private const string GRADLE_VERSION = "7.5.1"; // 新版本支持 Gradle 7.5.1
        private const string TOOLS_BIN_PATH = "~/dev/flutter/guru_app/tools/bin";
        
        // deps.sh 源文件
        private static string DepsShellSourcePath => $"{GetFilesDirPath()}/{DEPS_SH_NAME}";
        // depaudit 源文件
        private static string DepauditSourcePath => $"{GetFilesDirPath()}/{DEPAUDIT_CMD_NAME}";
        
        private static string _filesDirPath = string.Empty;
        /// <summary>
        /// 获取脚本路径
        /// </summary>
        /// <returns></returns>
        private static string GetFilesDirPath()
        {
            if (!string.IsNullOrEmpty(_filesDirPath)) return _filesDirPath;
            
            var guids = AssetDatabase.FindAssets($"{nameof(VersionTrackerHelper)} t:script");
            if (guids.Length <= 0) return string.Empty;
            
            var sc = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (!File.Exists(sc)) return string.Empty;
            
            var dir = $"{Directory.GetParent(sc)?.FullName ?? "" }/files";
            if (!Directory.Exists(dir)) return string.Empty;
            
            _filesDirPath = dir;
            return _filesDirPath;
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="workpath"></param>
        /// <param name="cmd"></param>
        private static void CallDepsScript(string workpath, string cmd = "")
        {
            if (string.IsNullOrEmpty(cmd)) cmd = DEPS_SH_NAME;
            RunShellCmd(workpath, cmd);
            Debug.Log($"---- running command: {cmd} is over -----");
        }

        // 运行命令
        private static void RunShellCmd(string workpath, string cmd)
        {
            //------ 启动命令 --------
            Process p = new Process();
            p.StartInfo.WorkingDirectory = workpath;
            p.StartInfo.FileName = "/bin/bash";
            p.StartInfo.Arguments = cmd;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            var log = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Debug.Log(log);
        }

        /// <summary>
        /// 创建 ENV 文件
        /// </summary>
        /// <param name="projPath">gradle project 项目路径</param>
        private static void CreateEnvScript(string projPath)
        {
            string buildName = $"1.0.0-00000000";
            string platform = $"editor";
            string dir = projPath;
            
#if UNITY_ANDROID
            buildName = $"{Application.version}-{PlayerSettings.Android.bundleVersionCode}";
            platform = "android";
#elif UNITY_IOS
            buildName = $"{Application.version}-{PlayerSettings.iOS.buildNumber}";
            platform = "ios";
#endif
            
            // 构建 .deps_env 文件内容
            List<string> lines = new List<string>()
            {
                $"export BUILD_NAME={buildName}",
                $"export APP_NAME=\"{PlayerSettings.productName}\"",
                $"export APP_ID={Application.identifier}",
                $"export PLATFORM={platform}",
                $"export DIR=\"{dir}\"",

#if UNITY_ANDROID
                // 打包机上配置的 Gradle 路径
                $"export GRADLE_HOME=\"~/.gradle/{GRADLE_VERSION}\"", 
                "export PATH=$GRADLE_HOME:$PATH", 
                "export PATH=$GRADLE_HOME/bin:$PATH", 
                // 打包机上配置的 Java Home 路径
                $"export JAVA_HOME=\"{UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath}\"", // 打包机上配置的 JAVA 路径
                "export PATH=$JAVA_HOME:$PATH", 
                "export PATH=$JAVA_HOME/bin:$PATH",
#endif
                
            };

            // 添加 depaudit 工具路径
            if (Directory.Exists(TOOLS_BIN_PATH))
            {
                lines.AddRange(new string[]
                {
                    // 打包机上的 tools 路径
                    $"export TOOLS_BIN=\"{TOOLS_BIN_PATH}\"",
                    "export PATH=$TOOLS_BIN:$PATH",
                });
            }
            else 
            {
                if (File.Exists(DepauditSourcePath))
                {
                    var to = $"{projPath}/{DEPAUDIT_CMD_NAME}";
                    File.Copy(DepauditSourcePath,to, true);
                
                    lines.AddRange(new string[]
                    {
                        // 打包机上的 tools 路径
                        $"export TOOLS_BIN=\"{projPath}\"",
                        "export PATH=$TOOLS_BIN:$PATH",
                        $"export {DEPAUDIT_CMD_NAME}=\"{to}\""
                    });
                }
                else
                {
                    Debug.LogError($"Depaudit source File not found: {DepauditSourcePath}");
                }
            }
            
            lines.AddRange(new string[]
            {
                // print env vars
                "echo \"--- BuildName: ${BUILD_NAME}\"",
                "echo \"--- AppName: ${APP_NAME}\"",
                "echo \"--- APP_ID: ${APP_ID}\"",
                "echo \"--- Platform: ${PLATFORM}\"",
                "echo \"--- GRADLE_HOME: ${GRADLE_HOME}\"",
                "echo \"--- JAVA_HOME: ${JAVA_HOME}\"",
                "echo ",    
            });
            
            File.WriteAllLines($"{projPath}/{DEPS_ENV_NAME}", lines.ToArray());
        }
        

        /// <summary>
        /// 安装和运行依赖输出器
        /// </summary>
        /// <param name="buildPath"></param>
        public static void InstallAndRun(string buildPath)
        {
            if (string.IsNullOrEmpty(DepsShellSourcePath) || !File.Exists(DepsShellSourcePath))
            {
                Debug.LogError($"--- deps script file not found, skip output deps...");
                return;
            }
            
            string projPath = buildPath;
#if UNITY_ANDROID
            projPath = Directory.GetParent(buildPath).FullName;
#elif UNITY_IOS
            //TBD
#endif
            //---- Create Env ----
            CreateEnvScript(projPath);
            
            //---- Setup Deps ----
            string to = $"{projPath}/{DEPS_SH_NAME}";
            if (File.Exists(to)) File.Delete(to);
            FileUtil.CopyFileOrDirectory(DepsShellSourcePath, to); //拷贝脚本
            
            try
            {
                Debug.Log($"=== Output build deps data ===");
                CallDepsScript(projPath);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Debug.Log($"=== Output pods deps failed: {ex}");
            }
            
        }
        
    }
}