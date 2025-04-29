
namespace Guru
{
    using UnityEngine;
    using System.IO;
    using System;
    
    public class GuruAppVersion
    {
        private const string BuildInfoName = "build_info";
        private static readonly string DefaultDir = Path.GetFullPath($"{Application.dataPath}/Guru/Resources");
        public static readonly string DefaultFilePath = $"{DefaultDir}/{BuildInfoName}.txt";
        private const string DefaultBuildNumber = "1";
        public string buildVersion;
        public string buildNumber;

        public override string ToString()
        {
            return $"{buildVersion}-{buildNumber}";
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private GuruAppVersion()
        {
            buildVersion = Application.version;
            buildNumber = DefaultBuildNumber;
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="buildVersion"></param>
        /// <param name="buildNumber"></param>
        public GuruAppVersion(string buildVersion, string buildNumber)
        {
            this.buildVersion = buildVersion;
            this.buildNumber = buildNumber;
        }

        public static GuruAppVersion Load()
        {
            var raw = Resources.Load<TextAsset>(BuildInfoName)?.text ?? "";
            if (!string.IsNullOrEmpty(raw)) 
                return GuruAppVersion.Parse(raw);
            
            Debug.Log($"Load GuruAppVersion Failed, using default value: {raw}");
            return new GuruAppVersion(GetBuildVersion(), "0");
        }

        /// <summary>
        /// 获取 APP 构建号
        /// </summary>
        /// <returns></returns>
        public static string GetBuildNumber()
        {
            var nowDate = DateTime.Now;
            string strYear = nowDate.Year.ToString().Substring(2);
            string strMon = nowDate.Month.ToString("00");
            string strDay = nowDate.Day.ToString("00");
            string strQuarter = ((nowDate.Hour * 60 + nowDate.Minute) / 15).ToString("00");
            // 2024-08-01 08:00:00  to version string: 24080130
            string strBuildNumber = $"{strYear}{strMon}{strDay}{strQuarter}";
            return strBuildNumber;
        }

        /// <summary>
        /// 获取 APP 版本号
        /// </summary>
        /// <returns></returns>
        public static string GetBuildVersion()
        {
            return Application.version;
        }


        private static GuruAppVersion Parse(string raw)
        {
            var a = new GuruAppVersion();
            if (string.IsNullOrEmpty(raw))
            {
                return a;
            }
            
            var arr = raw.Split('-');
            if (arr.Length > 0) a.buildVersion = arr[0];
            if (arr.Length > 1) a.buildNumber = arr[1];

            if (string.IsNullOrEmpty(a.buildVersion))
            {
                a.buildVersion = Application.version;
            }

            if (string.IsNullOrEmpty(a.buildNumber))
            {
                a.buildNumber = DefaultBuildNumber;
            }
            return a;
        }


        /// <summary>
        /// 保存至磁盘
        /// </summary>
        /// <param name="version"></param>
        /// <param name="code"></param>
        public static void SaveToDisk(string version, string code)
        {
            if (string.IsNullOrEmpty(version))
            {
                version = Application.version;
            }

            if (string.IsNullOrEmpty(version))
            {
                version = "1.0.0";
                UnityEngine.Debug.LogError("App Version did not setup right, check PlayerSettings.bundleVersion and set it with the format just like:<1.1.0>");
            }

            if (!Directory.Exists(DefaultDir)) Directory.CreateDirectory(DefaultDir);
            var path = $"{DefaultDir}/{BuildInfoName}.txt";
            File.WriteAllText(path,  $"{version}-{code}");
        }


        #region 编辑器接口
        
#if UNITY_EDITOR

        private static bool IsFileExistsInProject => File.Exists(DefaultFilePath);

        /// <summary>
        /// 创建 GuruVersion 文件
        /// </summary>
        public static GuruAppVersion CreateLocalGuruVersion()
        {
            if (IsFileExistsInProject)
                return GuruAppVersion.Load();

            // 若没有就创建一个新的文件
            var guruApp = new GuruAppVersion(GetBuildVersion(), GetBuildNumber());
            SaveToDisk(guruApp.buildVersion, guruApp.buildNumber);
            return guruApp; 
        }

#endif

        #endregion

    }


 
}