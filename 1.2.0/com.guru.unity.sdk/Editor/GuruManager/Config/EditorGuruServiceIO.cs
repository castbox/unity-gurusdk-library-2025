using UnityEngine;
using Guru;
using NUnit.Framework;

namespace Guru.Editor
{
    using System.IO;
    using UnityEditor;
    using Guru.LitJson;
    
    public class EditorGuruServiceIO
    {
        internal static readonly string SourceConfigFileName = "guru-service";
        internal const string LocalServicesConfigPath = "Guru/Resources";
        internal const string SourceConfigExtension = ".json";
        internal const string LocalConfigExtension = ".txt";

        internal static string DefaultFilePath =
            Path.GetFullPath(Path.Combine(Application.dataPath, $"{SourceConfigFileName}{SourceConfigExtension}"));

        internal static string SourceServiceFilePath = "";

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns></returns>
        public static GuruServicesConfig LoadConfig()
        {
            var a = AssetDatabase.FindAssets($"*{SourceConfigFileName}* t:TextAsset", new []{"Assets"});
            if (a == null || a.Length == 0)
            {
                UnityEngine.Debug.Log($"<color=orange>--- Can't find guru-services file</color>");
            }
            else
            {
                var p = AssetDatabase.GUIDToAssetPath(a[0]);
                var fp = Path.GetFullPath(p);
                if (File.Exists(fp)) SourceServiceFilePath = fp;
                var t = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                // UnityEngine.Debug.Log($"<color=#88ff00>--- find services file:{p} \n{t.text}</color>");
                return JsonMapper.ToObject<GuruServicesConfig>(t.text);
            }
            return null;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="config"></param>
        internal static void SaveConfig(GuruServicesConfig config = null, string savePath = "")
        {
            if (config == null)
            {
                config = new GuruServicesConfig();    
            }

            var jw = new JsonWriter()
            {
                PrettyPrint = true,
            };
            JsonMapper.ToJson(config, jw);
            var json = jw.ToString();

            if (string.IsNullOrEmpty(savePath)) savePath = SourceServiceFilePath;
            if (string.IsNullOrEmpty(savePath)) savePath = DefaultFilePath;
            
            File.WriteAllText(savePath, json);
            UnityEngine.Debug.Log($"Save config to {savePath}");
        }

        /// <summary>
        /// 创建空配置
        /// </summary>
        /// <returns></returns>
        internal static GuruServicesConfig CreateEmpty()
        {
            var cfg = new GuruServicesConfig();
            cfg.version = 0;
            cfg.app_settings = new GuruAppSettings();
            cfg.ad_settings = new GuruAdSettings();
            cfg.adjust_settings = new GuruAdjustSettings();
            cfg.fb_settings = new GuruFbSettings();
            cfg.parameters = new GuruParameters();
            return cfg;
        }


        [Test]
        public static void Test_SaveConfig()
        {
            var cfg = CreateEmpty(); 
            SaveConfig(cfg);
        }


        public static void DeployLocalServiceFile()
        {
            var deployPath = Path.Combine(Application.dataPath, LocalServicesConfigPath);
            if(!Directory.Exists(deployPath)) Directory.CreateDirectory(deployPath);
            var path = Path.Combine(deployPath, $"{GuruSDK.ServicesConfigKey}{LocalConfigExtension}");

            var config = LoadConfig();
            var from = SourceServiceFilePath;
            if (string.IsNullOrEmpty(from) || !File.Exists(from)) // 文件不存在
            {
                return;
            }

            if (null != config)
            {
                if (File.Exists(path)) File.Delete(path);
                UnityEngine.Debug.Log($"<color=#88ff00> --- setup {GuruSDK.ServicesConfigKey} to local resources.</color>");
                var content = File.ReadAllText(from);
                File.WriteAllText(path, content);
            }
        }
    }
}