using System.Collections.Generic;
using NUnit.Framework;

#if UNITY_ANDROID
namespace Guru
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    
    /// <summary>
    /// Android混淆器内容填充
    /// 于应用构建前执行
    /// TODO: 目前合并方案尚不完善，停止使用
    /// </summary>
    // public class UserProguardHelper: IPreprocessBuildWithReport
    public class UserProguardHelper
    {
        public int callbackOrder { get; } = 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            // 修复ProguardFile
            OnApplyProguardFiles();
        }
        
        private static void OnApplyProguardFiles()
        {
            string proguardPath = $"{Application.dataPath}/Plugins/Android/proguard-user.txt";
            if (File.Exists(proguardPath))
            {
                DirectoryInfo dir = new DirectoryInfo(Application.dataPath);
                string raw = File.ReadAllText(proguardPath);
                
                if (dir.Exists)
                {
                    var editors = dir.GetDirectories("Editor", SearchOption.AllDirectories);
                    List<FileInfo> files = new List<FileInfo>();
                    foreach (var e in editors)
                    {
                        files.AddRange(e.GetFiles("*Proguards.txt", SearchOption.AllDirectories));
                    }
                    
                    // Debug.Log($"--- Proguard Files: {files.Length}");
                    
                    ProguardItemBuilder builder = new ProguardItemBuilder();

                    var allItems = new List<ProguardItem>(30);
                    
                    string[] lines = null;
                    for (int i = 0; i < files.Count; i++)
                    {
                        lines = File.ReadAllLines(files[i].FullName);
                        var items = builder.BuildItemsFormLines(lines);
                        if(items != null && items.Count > 0) allItems.AddRange(items);
                    }
                    
                    
                    List<string> finalLines = new List<string>(50);
                    foreach (var item in allItems)
                    {
                        finalLines.AddRange(item.lines);
                    }
                    
                    File.WriteAllLines(proguardPath, finalLines.ToArray());
                    Debug.Log($"--- Update proguard-user.txt done! ☀️ ---");
                }
            }
        }

        
        

        




        [MenuItem("Tools/Android/Add proguard-user")]
        private static void EditorAddProguardUser()
        {
            OnApplyProguardFiles();
        }

    }

    internal class ProguardItemBuilder
    {
        
        
        
        
        
        
        public List<ProguardItem> BuildItemsFormLines(string[] lines)
        {
            List<ProguardItem> items = new List<ProguardItem>(30);

            string line = "";
            ProguardItem curItem = null;

            for(int i =0; i < lines.Length; i++)
            {
                line = lines[i];
                
                if(string.IsNullOrEmpty(line)) continue;

                if (curItem == null)
                {
                    curItem = new ProguardItem();
                }

                curItem.Append(line);
                
                if(line.Contains("}"))
                {
                    items.Add(curItem);
                    curItem = null;
                }
            }
            
            return items;
        } 
    }


    internal class ProguardItem
    {
        public List<string> lines = new List<string>();
        public string key = "";

        public void Append(string line)
        {
            if (string.IsNullOrEmpty(key) && 
                line.StartsWith("-"))
            {
                key = line;
            }

            if (lines == null) lines = new List<string>(5);
            lines.Add(line);
        }

    }


}

#endif