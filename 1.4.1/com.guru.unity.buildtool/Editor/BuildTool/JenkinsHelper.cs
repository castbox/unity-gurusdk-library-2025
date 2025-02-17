namespace Guru.Editor
{
    using System;
    using UnityEngine;
    using System.Linq;
    
    public class JenkinsHelper
    {
        public const string DefaultArgsTag = "+args";
        
        public static AppBuildParam ParseJenkinsBuildParam(string[] commandlineArgs, string argsTag = "")
        {
            if (string.IsNullOrEmpty(argsTag)) argsTag = DefaultArgsTag;
            int len = commandlineArgs.Length;

            Debug.Log($"------------ Jenkins set commands: {len} ------------");

            var buildParam = new AppBuildParam()
            {
                BuilderType = AppBuilderType.Jenkins,
                TargetName = "Android",
                IsBuildAAB = false,
                IsBuildSymbols = false,
                AutoPublish = false,
            };
            
            string p = "";
            for (int i = 0; i < len; i++)
            {
                p = commandlineArgs[i];
                // Debug.Log($"--- [{i}]: {p}");

                if (p.StartsWith(argsTag))
                {
                    // Debug.Log($"--- find param: {p}");
                    var args = p.Split('-').ToList();
                    if (args.Count > 1)
                    {
                        // Debug.Log($"--- ENV: {args[1]}");
                        if (args[1].ToUpper() == "RELEASE")
                        {
                            buildParam.IsBuildRelease = true;
                            buildParam.IsBuildShowLog = false;
                            buildParam.IsBuildSymbols = true;
                        }
                        else
                        {
                            buildParam.IsBuildRelease = false;
                            buildParam.IsBuildShowLog = true;
                            buildParam.IsBuildSymbols = false;
                        }
                    }
                    if (args.Count > 2)
                    {
                        // Debug.Log($"--- VERSION: {args[2]}");
                        buildParam.BuildVersion = args[2];
                    }
                }
            }
            
            return buildParam;
        }



        /// <summary>
        /// 获取构建参数
        /// </summary>
        /// <param name="argsTag"></param>
        /// <returns></returns>
        public static AppBuildParam GetBuildParams(string argsTag = "")
        {
            return ParseJenkinsBuildParam(Environment.GetCommandLineArgs(), argsTag);
        }
        
    }


    

}