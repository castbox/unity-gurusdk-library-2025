
namespace Guru.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using System;
    
    public class PgyerAPI
    {
        public const string Version = "1.0.0";

        internal static readonly string DefaultBashPathWin = "C:\\Program Files\\Git\\bin\\bash.exe";
        internal static readonly string DefaultBashPathMac = "/bin/bash";
        internal static readonly string ShellFile = "pgyer_upload.sh";
        private static readonly string GuruAPIKey = "20a3d1106b802abbd84ec687eedf17eb";
        private static readonly string PgyerHost = "https://www.pgyer.com";
        internal static string WorkingDir => $"{Application.dataPath.Replace("Assets", "Library")}/guru_publish";

        public static string GetDownloadUrl(string shortUrl) => $"{PgyerHost}/{shortUrl}";

        /// <summary>
        /// 发布产品到蒲公英平台
        /// </summary>
        /// <param name="packagePath"></param>
        /// <param name="apiKey"></param>
        /// <param name="bashPath"></param>
        /// <param name="callback"></param>
        public static void PublishToPgyer(string packagePath, string apiKey = "", string bashPath = "",
            Action<string> callback = null)
        {
            if (File.Exists(packagePath))
            {
                Debug.Log($"=== START PUBLISH APP: {packagePath}");
                CheckWorkingDir();
                CallPublishShell(packagePath, apiKey, bashPath, callback);
            }
        }

        private static void CheckWorkingDir()
        {
            if (!Directory.Exists(WorkingDir))
            {
                Directory.CreateDirectory(WorkingDir);
            }

            var file = $"{WorkingDir}/{ShellFile}";
            if (!File.Exists(file))
            {
                var from = GetShellPath();
                if (File.Exists(from))
                {
                    File.Copy(from, file);
#if UNITY_EDITOR_OSX
                    RunCmd("chmod", $"+x {file}", workpath: WorkingDir);
#endif
                }
                else
                {
                    Debug.LogError($"[Publisher] Source shell file not found :{from}");
                }
            }
        }



        /// <summary>
        /// 获取 CMD 命令路径
        /// </summary>
        /// <returns></returns>
        private static string GetShellPath()
        {
            var path = "";
            var guids = AssetDatabase.FindAssets($"{nameof(PgyerAPI)} t:script");
            if (guids.Length > 0)
            {
                path = Path.Combine(Directory.GetParent(AssetDatabase.GUIDToAssetPath(guids[0])).FullName, ShellFile);
                return Path.GetFullPath(path);
            }

            path = Path.GetFullPath(
                $"{Application.dataPath.Replace("Assets", "Packages")}/com.guru.unity.sdk.core/Editor/BuildTool/{ShellFile}");
            return path;
        }



        private static void RunCmd(string cmd, string args, Action<string> callback = null, string workpath = "")
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = cmd;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            if (!string.IsNullOrEmpty(workpath)) process.StartInfo.WorkingDirectory = workpath;
            process.Start();
            string log = process.StandardOutput.ReadToEnd();
            callback?.Invoke(log);
            process.Close();
        }

        /// <summary>
        /// 在 mac 下进行发布
        /// </summary>
        /// <param name="packagePath"></param>
        /// <param name="apiKey"></param>
        /// <param name="bashPath"></param>
        /// <param name="callback"></param>
        private static void CallPublishShell(string packagePath, string apiKey = "", string bashPath = "",
            Action<string> callback = null)
        {
            if (string.IsNullOrEmpty(bashPath))
            {
#if UNITY_EDITOR_OSX
                bashPath = DefaultBashPathMac;
#elif UNITY_EDITOR_WIN
                bashPath = DefaultBashPathWin.Replace("\\", "/");
#endif
            }

            if (!File.Exists(bashPath))
            {
                string msg = $"Error: Bash file not found at path: {bashPath}! skip publishing!";
                Debug.LogError(msg);
                callback?.Invoke(msg);
                return;
            }

            packagePath = packagePath.Replace("\\", "/");
            if (string.IsNullOrEmpty(apiKey)) apiKey = GuruAPIKey;
            var args = $"-c \"./{ShellFile} -k {apiKey} {packagePath}\"";
            // Debug.Log(bashPath);
            // Debug.Log(args);
            // Debug.Log(WorkingDir);
            RunCmd(bashPath, args, callback, WorkingDir);
        }
        
    }
    
    /// <summary>
    /// Guru 包体上传工具
    /// </summary>
    public class GuruPublishHelper
    {
        // Check Env and Exe files
        private static string EvnCheck()
        {
            if (!Directory.Exists(PgyerAPI.WorkingDir))
                Directory.CreateDirectory(PgyerAPI.WorkingDir);
            
            // #1 --- read from cached file with available path
            string bash_path = "";
            var envFile = $"{PgyerAPI.WorkingDir}/.env";
            if (File.Exists(envFile))
            {
                bash_path = File.ReadAllText(envFile);
                return bash_path;
            }

            // #2 --- Try to find bash exe file from default path
            bash_path = PgyerAPI.DefaultBashPathMac;
#if UNITY_EDITOR_WIN
            bash_path = PgyerAPI.DefaultBashPathWin;
#endif
            if (File.Exists(bash_path))
            {
                bash_path = bash_path.Replace("\\", "/");
                File.WriteAllText(envFile, bash_path);
                return bash_path;
            }

            // #3 --- Try to let user select bash exe file from disk
            string title = "选择 bash 可执行文件";
            string despath = "/bin";
            string exts = "*";

#if UNITY_EDITOR_WIN
            despath = "C:\\Program Files\\";
            title = $"选择 bash 可执行文件, 例如: {despath}\\Git\\bin\\bash.exe";
            exts = "exe";
#endif
            bash_path = EditorUtility.OpenFilePanel(title, despath, exts);
            if (File.Exists(bash_path))
            {
                File.WriteAllText(envFile, bash_path.Replace("\\", "/"));
            }
            return bash_path;
        }
        
        
        [MenuItem("Guru/Publish/Android APK...")]
        private static void EditorPublishAPK()
        {
            SelectAndPublish();
        }
        
        // [MenuItem("Guru/Publish/Publish Release AAB...")]
        // private static void EditorPublishAAB()
        // {
        //     SelectAndPublish("aab");
        // }

        /// <summary>
        /// 直接发布版本
        /// </summary>
        /// <param name="appPath"></param>
        /// <param name="apiKey"></param>
        public static void Publish(string appPath, string apiKey = "")
        {
            string bash_path = EvnCheck();
            if (string.IsNullOrEmpty(bash_path))
            {
                ShowDialog("找不到 Bash 执行文件", $"Bash文件不存在: {bash_path}!");
                return;
            }
            if (!File.Exists(appPath))
            {
                ShowDialog("找不到包体文件", $"包体文件不存在: {appPath}!");
                return;
            }
            
            PgyerAPI.PublishToPgyer(appPath, apiKey, bash_path, OnResponse);
            
        }

        /// <summary>
        /// 选择文件及发布版本
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="apiKey"></param>
        public static void SelectAndPublish(string extension = "apk", string apiKey = "")
        {
            string file = EditorUtility.OpenFilePanel("选择包体", $"~/Downloads", extension);
            Publish(file, apiKey);
        }

        /// <summary>
        /// Show system dialogs
        /// </summary>
        /// <param name="title"></param>
        /// <param name="body"></param>
        /// <param name="callback"></param>
        /// <param name="cancelAction"></param>
        /// <param name="okName"></param>
        /// <param name="cancelName"></param>
        private static void ShowDialog(string title, string body, Action callback = null, Action cancelAction = null, string okName= "OK", string cancelName = "")
        {
            if (EditorUtility.DisplayDialog(title, body, okName, cancelName))
            {
                callback?.Invoke();
            }
            else
            {
                cancelAction?.Invoke();
            }
        }

        /// <summary>
        /// On pgyer response callback
        /// </summary>
        /// <param name="log"></param>
        private static void OnResponse(string log)
        {
            var logPath = $"{PgyerAPI.WorkingDir}/log.txt";
            File.WriteAllText(logPath, log);
            
            bool success = log.Contains(ResponseObject.HeadTag);

            if (success)
            {
                var json = log.Substring(log.IndexOf(ResponseObject.HeadTag, StringComparison.Ordinal));
                var res = ResponseObject.Parse(json);
                if (res != null)
                {
                    var url = PgyerAPI.GetDownloadUrl(res.ShortUrl());
                    ShowDialog($"==== 上传成功 ({PgyerAPI.Version}) ====", $"包体 {res.BuildVersion()} ({res.BuildVersionCode()}) 上传成功!\n下载链接:{url}", () =>
                    {
                        Debug.Log($"INSTALL URL:{url}"); // output url
                        Application.OpenURL(url);
                    });

                    return;
                }
            }
            
            ShowDialog($"==== 上传失败 ({PgyerAPI.Version}) ====", $"上传文件失败, 查看详细日志: \n{logPath}", () =>
            {
#if UNITY_EDITOR_OSX
                EditorUtility.RevealInFinder(PgyerAPI.WorkingDir);
                return;
#endif
                Application.OpenURL(PgyerAPI.WorkingDir);
            });
        }

    }

    [Serializable]
    internal class ResponseObject
    {
        public int code;
        public string message;
        public PublishData data;
        
        public static readonly string HeadTag = "{\"code\":";

        public string BuildVersion() => data?.buildVersion ?? "0.0.0";
        public string BuildVersionCode() => data?.buildVersionNo ?? "0";
        public string ShortUrl() => data?.buildShortcutUrl ?? "#";
        
        
        public static bool IsValid(string json)
        {
            return json.Contains(HeadTag);
        }
        
        public static ResponseObject Parse(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            if (!IsValid(json)) return null;
            return JsonUtility.FromJson<ResponseObject>(json);
        }
    }

    [Serializable]
    internal class PublishData
    {
        public string buildIdentifier;
        public string buildQRCodeURL;
        public string buildShortcutUrl;
        public string buildName;
        public string buildVersion;
        public string buildVersionNo;
        public string buildUpdated;
    }
}