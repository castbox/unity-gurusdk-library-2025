namespace Guru.Editor.Adjust
{
    using System.IO;
    using System.Linq;
    using AdjustSdk;
    using UnityEditor;
    using UnityEditor.Compilation;
    using UnityEngine;
    
    public class AdjustMod
    {
        public static string Tag = "[MOD]";

        private static string CodeReplaceSample =
            "AssetDatabase.GUIDToAssetPath(guids[0]).Replace(\"AdjustSettings.cs\", \"AdjustSettings.asset\")";

        private static string CodeIOSFrameworkAdServices = "private bool _iOSFrameworkAdServices";
        private static string CodeIOSFrameworkATT = "private bool _iOSFrameworkAppTrackingTransparency";
        
        
        
        /// <summary>
        /// 应用补丁
        /// </summary>
        public static void Apply()
        {
            var mod = new AdjustMod();
            mod.FixSettingsInstancePath();
        }
        
        /// <summary>
        /// 修复示例地址
        /// </summary>
        private void FixSettingsInstancePath()
        {
            var guids = AssetDatabase.FindAssets($"{nameof(AdjustSettings)} t:Script");
            if (guids.Length > 0)
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    var p = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if(p.Contains($"{nameof(AdjustSettings)}.cs"))
                    {
                        var path = Path.GetFullPath(p);
                        if (File.Exists(path))
                        {
                            InjectCodeAtPath(path);
                            return;
                        }
                        break;
                    }
                
                }
            }
            Debug.Log($"{Tag}<color=orange>--- Guru Adjust inject failed</color>");
        }

        /// <summary>
        /// 注入代码逻辑
        /// </summary>
        /// <param name="path"></param>
        private void InjectCodeAtPath(string path)
        {
            // ---------- Inject Code ----------
            string indent = "\t\t\t\t";
            string Info = $"{indent}// ************ Auto fixed by Guru Adjust ************";
             
            string buffer = $"{Info}";
            // buffer += $"\n{indent}if(System.IO.File.Exists(\"{GuruAdjustSdkAPI.AdjustSettingsPackagePath}\")) System.IO.File.Delete(\"{GuruAdjustSdkAPI.AdjustSettingsPackagePath}\");";
            buffer += $"\n{indent}if(!System.IO.Directory.Exists(\"{GuruAdjustSdkAPI.AdjustSettingsRootDir}\")) System.IO.Directory.CreateDirectory(\"{GuruAdjustSdkAPI.AdjustSettingsRootDir}\");";
            buffer += $"\n{indent}var assetPath = \"{GuruAdjustSdkAPI.AdjustSettingsAssetPath}\";";
            buffer += $"\n{Info}";
            
            var lines = File.ReadLines(path).ToList();
            string line = "";
            bool isDirty = false;
            for (int i = 0; i < lines.Count; i++)
            {
                line = lines[i];
                if (line.Contains(CodeIOSFrameworkAdServices) && line.Contains("false"))
                {
                    lines[i] = line.Replace("false", "true"); // 允许引入AdService
                    isDirty = true;
                    continue;
                }
                
                if (line.Contains(CodeIOSFrameworkATT) && line.Contains("false"))
                {
                    lines[i] = line.Replace("false", "true"); // 允许引入AdService
                    isDirty = true;
                    continue;

                }
                
                if (line.Contains(CodeReplaceSample) && !line.Contains("//"))
                {
                    lines[i] = $"//{line}";
                    lines.Insert(i+1, buffer);
                    isDirty = true;
                    break;
                }
            }

            if (isDirty)
            {
                File.WriteAllLines(path, lines.ToArray());
                Debug.Log($"{Tag}<color=#88ff00>--- Guru Adjust inject success</color>");
                CompilationPipeline.RequestScriptCompilation();
            }
        }


        
        /// <summary>
        /// 创建接口
        /// </summary>
        /// <param name="instance"></param>
        public static void CreateInstance (AdjustSettings instance)
        {
            // 删除旧文件
            if(File.Exists(GuruAdjustSdkAPI.AdjustSettingsPackagePath))
                File.Delete(GuruAdjustSdkAPI.AdjustSettingsPackagePath);
            
            // 创建新父目录
            if (!Directory.Exists(GuruAdjustSdkAPI.AdjustSettingsPackagePath))
                Directory.CreateDirectory(GuruAdjustSdkAPI.AdjustSettingsPackagePath);
            
            // 创建对象
            var assetPath = GuruAdjustSdkAPI.AdjustSettingsAssetPath;
            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}