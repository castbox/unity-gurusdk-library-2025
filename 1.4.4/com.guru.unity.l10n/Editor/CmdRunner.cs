using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Guru
{
    /// <summary>
    /// 命令行执行器
    /// </summary>
    public static class CmdRunner
    {

        public static Process Build(string cmd, string args, string workingDir = "", Action callback = null)
        {
            // 设置进程参数
            var p = new Process();
            p.StartInfo.FileName = cmd;
            p.StartInfo.Arguments = args;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            if(!string.IsNullOrEmpty(workingDir)) p.StartInfo.WorkingDirectory = workingDir;
            // 一切就绪，启动进程！
            return p;
        }

        /// <summary>
        /// 执行shell脚本
        /// </summary>
        /// <param name="command"></param>
        /// <param name="workingDir"></param>
        /// <param name="callback"></param>
        public static void CallCmd(string cmd, string args, string workingDir, System.Action<int, string> callback = null, bool showOutput = false)
        {
            var p = Build(cmd, args, workingDir);
            p.Start();
            p.WaitForExit();
            int exitCode = p.ExitCode;
            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();
            if (showOutput)
            {
                Debug.Log("Output:\n" + output);
                Debug.Log("Error:\n" + error);
                Debug.Log("Exit code: " + exitCode);
            }
            callback?.Invoke(exitCode, output);
            p.Close();
            p = null;
        }


        /// <summary>
        /// 运行 Shell 脚本
        /// </summary>
        /// <param name="command"></param>
        /// <param name="workingDir"></param>
        /// <param name="callback"></param>
        /// <param name="showOutput"></param>
        public static void RunShell(string command, string workingDir, System.Action<int, string> callback = null, bool showOutput = false)
        {
            string cmd = "/bin/bash";
            string args = $"-c \"{command}\"";
            CallCmd(cmd, args, workingDir, callback, showOutput);
        }

        public static void ShAsync(string command, string workingDir, System.Action<int, string> callback = null,
            bool showOutput = false)
        {
            Thread th = null;
            th = new Thread(() =>
            {
                RunShell(command, workingDir, (code, msg) =>
                {
                    th.Join(0);
                    callback?.Invoke(code, msg);
                }, showOutput);
            });
            th.Start();
        }
        
        
        
        /// <summary>
        /// 打开Shell Command 命令
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="workingDir"></param>
        public static void OpenMacCommand(string filePath, string workingDir)
        {
            string cmd = "open";
            string args = filePath;
            var p = Build(cmd, args, workingDir);
            p.Start();
            
            // TODO 添加进程Watcher
        }
        
        /// <summary>
        /// 打开 windows 批处理文件
        /// </summary>
        /// <param name="batPath"></param>
        public static void CallWinBat(string batPath, string workingDir)
        {
            string cmd = "cmd.exe";
            string args = $"/c {batPath}";
            var p = Build(cmd, args, workingDir);
            p.Start();
        }
        
        
    }
}