using System.IO;
using UnityEngine;

namespace Guru
{
    public class GuruAppVersion
    {
        public const string BuildInfoName = "build_info";

        public string raw;
        public string version;
        public string code;

        public override string ToString()
        {
            return $"{version}-{code}";
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public GuruAppVersion()
        {
            version = Application.version;
            code = "0";
            raw = $"{version}-{code}";
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="version"></param>
        /// <param name="code"></param>
        public GuruAppVersion(string version, string code)
        {
            this.version = version;
            this.code = code;
            this.raw = $"{version}-{code}";
        }

        public static GuruAppVersion Load()
        {
            var raw = Resources.Load<TextAsset>(BuildInfoName)?.text??"";
            return GuruAppVersion.Parse(raw);
        }

        
        protected static GuruAppVersion Parse(string raw)
        {
            var a = new GuruAppVersion();
            if (string.IsNullOrEmpty(raw))
            {
                return a;
            }
            
            a.raw = raw;
            var arr = raw.Split('-');
            if (arr.Length > 0) a.version = arr[0];
            if (arr.Length > 1) a.code = arr[1];

            if (string.IsNullOrEmpty(a.version))
            {
                a.version = Application.version;
            }

            if (string.IsNullOrEmpty(a.code))
            {
                a.code = "0";
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

            var dir = $"{Application.dataPath}/Guru/Resources";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var path = $"{dir}/{BuildInfoName}.txt";
            File.WriteAllText(path,  $"{version}-{code}");
        }

    }


 
}