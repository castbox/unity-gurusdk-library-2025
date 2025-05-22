using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Guru.Editor.Max
{
    public class AmazonMod: GuruModifier
    {
        protected override string TargetPath => $"Amazon/Scripts/Editor/AmazonDependencies.xml";

        private static string SDKManagerPath = "Amazon/Scripts/Editor/AmazonSDKManager.cs";

        public static void Apply()
        {
            var mod = new AmazonMod();
            mod.FixDepsViaPackage();
            mod.HideAmazonMenuItems();
        }
        
        #region 修复依赖

        // [Test]
        public void FixDepsViaPackage()
        {
            string path = GetFullPath(TargetPath);
            
            if (!File.Exists(path))
            {
                Debug.Log($"<color=orange>---- file not found: {path}</color>");
                return;
            }
            
            var doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(path, Encoding.UTF8));
            if (doc.ChildNodes.Count < 2)
            {
                Debug.LogError($"--- Wrong Xml nodes or no root node");
                return;
            }

            var root = doc.ChildNodes[1];
            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name == "iosPods")
                {
                    foreach (XmlElement n in node.ChildNodes)
                    {
                        if (n.GetAttribute("name") == "Amazon-SDK-Plugin")
                        {
                            var p = n.GetAttribute("path");
                            p = p.Replace("Assets", $"Packages/{GuruMaxSdkAPI.PackageName}");
                            n.SetAttribute("path", p);
                        }
                    }
                }
            }

            // 保存文档
            doc.Save(path);

            var xml = File.ReadAllText(path);
            xml = xml.Replace("&gt;", ">").Replace("&lt;", "<");
            File.WriteAllText(path, xml);
            
            Debug.Log($"[GuruMax]<color=#88ff00>--- Fix Amazon depences success: {path}</color>");
        }
        
        #endregion

        #region 隐藏菜单

        [Test]
        public void HideAmazonMenuItems()
        {
            string partten = "[MenuItem (\"Amazon/";
            string path = GetFullPath(SDKManagerPath);
            bool isDirty = false;
            if (File.Exists(path))
            {
                var line = "";
                var lines = File.ReadAllLines(path);
                for (int i = 0; i < lines.Length; i++)
                {
                    line = lines[i];
                    if (line.Contains(partten))
                    {
                        lines[i] = $"//{line}";
                        isDirty = true;
                    }
                }

                if (isDirty)
                {
                    Debug.Log($"[GuruMax]<color=#88ff00>--- Hide Amazon Menu success: {path}</color>");
                    File.WriteAllLines(path, lines);
                    CompilationPipeline.RequestScriptCompilation();
                }

            }
            else
            {
                Debug.LogError($"--- AmazonSDKManager cannot be found");
            }
        }



        #endregion
        

    }
}