using System;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

/* ************************************************************************
 * === Supporter of Firebase App Export XML tool for Guru Team ===
 * 解决由于 Plugin/Firebase.Editor.dll 内写死扫描路径 Assets/Firebase/Editor 导致以下工具无法顺利执行:
 * 1. 插件包含 generate_xml_from_google_services_json 的工具
 *     - MacOSX: generate_xml_from_google_services_json.py 
 *     - Windows: generate_xml_from_google_services_json.exe
 * 2. 插件包含 network_request 的工具
 *     - MacOSX: network_request.py 
 *     - Windows: network_request.exe
 *
 * 中台会在编译时强制部署这些文件， 确保工具在被拉起时在以上路内存在。
 * 方便之后 unityplugins 转 upm 时可用
 * ************************************************************************
 */


/*
 * --------------------------------------------------------------------------------------
 * Firebase.Editor.dll::GenerateXmlFromGoogleServiceJson
 * line:
 * private static PythonExecutor resourceGenerator
 *      = new PythonExecutor(Path.Combine(Path.Combine("Assets", "Firebase"), "Editor"),
 *      "generate_xml_from_google_services_json.py", "8f18ed76c0f04ce0a65736104f913ef8",
 *      "generate_xml_from_google_services_json.exe", "ae88c0972b7448b5b36def1716f1d711");
 * 
 *
 * Firebase.Editor.dll::NetworkRequest
 * line 18
 * private static PythonExecutor executor
 *      = new PythonExecutor(Path.Combine(Path.Combine("Assets", "Firebase"), "Editor"),
 *      "network_request.py", "e6e32fecbfd44fab946fa160e4861924",
 *      "network_request.exe", "d3cd5d0a941c4cdc8ab4b1b684b05191");
 * 
 * 
 *  --- 在 dll 内已经定义了文件的 GUID， 这块需要在下一个版本内确认是否会变更 -- by Yufei 2025-06-28 (当前版本 12.10.0)
 */


namespace Guru
{
        
    [InitializeOnLoad]
    internal static class GuruFirebaseToolFixer
    {
        private const string Version = "v1.0.0";
        
        private const string KGenerateXmlTool = "generate_xml_from_google_services_json";
        private const string KNetworkRequest = "network_request";
#if UNITY_EDITOR_OSX
        private const string ExtName = "py";   
#else   
        private const string ExtName = "exe";
#endif

        private static readonly string[] GenerateXmlToolFileGUIDs = new string[]
            { "8f18ed76c0f04ce0a65736104f913ef8", "ae88c0972b7448b5b36def1716f1d711" };

        private static string generateXmlToolFileGuid
        {
            get
            {
#if UNITY_EDITOR_OSX
                return GenerateXmlToolFileGUIDs[0];
#else
                return GenerateXmlToolFileGUIDs[1];
#endif
            }
        }

        private static readonly string[] NetworkRequestFileGUIDs = new string[]
            { "e6e32fecbfd44fab946fa160e4861924", "d3cd5d0a941c4cdc8ab4b1b684b05191" };
        
        private static string networkRequestFileGuid
        {
            get
            {
#if UNITY_EDITOR_OSX
                return NetworkRequestFileGUIDs[0];
#else
                return NetworkRequestFileGUIDs[1];
#endif
            }
        }

        private static string generateXmlToolFile => $"{KGenerateXmlTool}.{ExtName}";
        private static string networkRequestFile => $"{KNetworkRequest}.{ExtName}";
        
        // 编译时触发
        static GuruFirebaseToolFixer()
        {
            var root = Path.Combine(Application.dataPath, "Firebase", "Editor");
            var file1 = Path.Combine(root, generateXmlToolFile);
            var file2 = Path.Combine(root, networkRequestFile);
            
            // 检查工具是否存在
            if (File.Exists(file1) && File.Exists(file2))
                return; // 2个文件已存在

            // 部署文件到项目中
            DeployFirebaseToolToProject();
            CallEditorCompile();
        }

        /// <summary>
        /// 部署 Xml Tool
        /// </summary>
        /// <returns></returns>
        private static void DeployFirebaseToolToProject()
        {
            var guids = AssetDatabase.FindAssets($"{nameof(GuruFirebaseToolFixer)} t:script");
            if (guids.Length == 0)
            {
                throw new Exception($"Failed to find ToolFixer {nameof(GuruFirebaseToolFixer)}");
            }
            
            var destDir = Path.Combine(Application.dataPath, "Firebase", "Editor");
            if(!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

            var packageRoot = Path.GetFullPath($"{AssetDatabase.GUIDToAssetPath(guids[0])}/../../../");
            var sourceDir = Path.Combine(packageRoot, "Firebase", "Editor");
            string sourcePath = "";
            string destPath = "";
            
            Debug.Log($"==== Package Root: {packageRoot}");
            sourcePath = Path.Combine(sourceDir, generateXmlToolFile);
            destPath = Path.Combine(destDir, generateXmlToolFile);
            File.Copy(sourcePath, destPath, true);
            File.Copy($"{sourcePath}.meta", $"{destPath}.meta", true);
            SetFileGuid($"{destPath}.meta", generateXmlToolFileGuid);
            // Debug.Log($"--- deploy tool: {destDir}");
            
            sourcePath = Path.Combine(sourceDir, networkRequestFile);
            destPath = Path.Combine(destDir, networkRequestFile);
            File.Copy(sourcePath, destPath, true);
            File.Copy($"{sourcePath}.meta", $"{destPath}.meta", true);
            SetFileGuid($"{destPath}.meta", networkRequestFileGuid);
            // Debug.Log($"--- deploy tool: {destDir}");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // 设置文件 GUID
        private static void SetFileGuid(string filePath, string guid)
        {
            if (!File.Exists(filePath))
                return;
            var lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Contains("guid:"))
                {
                    lines[i] = $"guid: {guid}";
                    File.WriteAllLines(filePath, lines);
                    return;
                }
            }
        }


        /// <summary>
        /// 执行强制编译
        /// </summary>
        private static void CallEditorCompile()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.None);
        }
    }
    
    
    
    
}