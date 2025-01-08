using System.Collections.Generic;
using System.IO;
using System.Linq;
using Guru.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Guru.Editor.Max
{
    public class ApplovinMod: GuruModifier
    {
        
        private const string IntergrationManagerPath = "MaxSdk/Scripts/IntegrationManager/Editor/AppLovinIntegrationManager.cs";
        private const string MenuItemsPath =  "MaxSdk/Scripts/IntegrationManager/Editor/AppLovinMenuItems.cs";
        private const string SettingsPath =   "MaxSdk/Resources/AppLovinSettings.asset";
        
        private static readonly string K_IsPluginOutside = "public static bool IsPluginOutsideAssetsDirectory";
        private const string EXT_BACKUP = ".backup";
        

        public static void Apply()
        {
            ApplovinMod mod = new ApplovinMod();
            // 替换代码内的组件路径
            mod.FixIntegrationManager();
            // 隐藏编辑器菜单
            mod.DoBackup();
            // 删除其他的 AppLovinSettings 组件
            mod.CheckAndCleanOtherSettings();
        }
        

        #region IntegrationManager 注入

        
         /// <summary>
        /// 修复脚本
        /// </summary>
        /// <param name="path"></param>
        [Test]
        public void FixIntegrationManager()
        {
            var path = GetFullPath(IntergrationManagerPath);
            if (!File.Exists(path))
            {
                Debug.LogError($"File not found: {path}");
                return;
            }

            // --------- Scan All the code lines -------------
            List<string> lines = File.ReadLines(path).ToList();

            string line = "";
            int i = 0;
            int incNum = 0;
            int count = lines.Count;
            while (i < count)
            {
                line = lines[i];

                if (line.TrimStart(new char[] { '\t', ' ' }).Contains(K_IsPluginOutside))
                {
                    incNum = FixupIsPluginOutside(lines, i);
                    i += incNum;
                    count = lines.Count;
                }

                i++;
            }
            File.WriteAllLines(path, lines.ToArray());
            Debug.Log($"[GuruMax] <color=#88ff00>--- code fixed: {path} ---</color>");
        }


        private const string MK_PluginOutside_START = "//--- INJECT PluginOutside START ---";
        private const string MK_PluginOutside_OVER = "//--- INJECT PluginOutside OVER ---";
        private int FixupIsPluginOutside(List<string> lines, int startIndex)
        {
            int inc = 0;
            string line = "";

            line = lines[startIndex - 1];
            if (line.Contains(MK_PluginOutside_START)) return 0; // 已经修复过了
            
            line = lines[startIndex + 1].Trim();
            if (line.Contains("{"))
            {
                lines[startIndex + 1] = $"//{lines[startIndex + 1]}";
                lines[startIndex + 2] = $"//{lines[startIndex + 2]}";
                lines[startIndex + 3] = $"//{lines[startIndex + 3]}";
            }
            lines.Insert(startIndex, $"{MK_PluginOutside_START}");
            lines.Insert(startIndex + 2, "\t\t{ get => !(PluginParentDirectory.StartsWith(\"Assets\") || PluginParentDirectory.StartsWith(\"Packages\")); }");
            lines.Insert(startIndex + 3, $"{MK_PluginOutside_OVER}");
            inc += 3;
            
            return inc;
        }
        

        #endregion

        #region MenuItems 隐藏
        [Test]
        public static bool MenuItemsHide()
        {
            return new ApplovinMod().DoBackup();
        }
        [Test]
        public static bool MenuItemsRecover()
        {
            return new ApplovinMod().DoRecover();
        }

        private bool DoBackup()
        {
            var path = GetFullPath(MenuItemsPath);
            string backup = $"{path}.backup";
            if (File.Exists(path) && !path.EndsWith(EXT_BACKUP))
            {
                File.Move(path, backup);
                Debug.Log($"[GuruMax]<color=#88ff00> --- Max Menu backuped </color>");
                return true;
            }
       
            Debug.Log($"<color=orange>File not found or different name: {path}</color>");
            return false;
        }

        private bool DoRecover()
        {
            var path = GetFullPath(MenuItemsPath);
            string backup = $"{path}{EXT_BACKUP}";
            if (path.EndsWith(EXT_BACKUP))
            {
                backup = path;
                path = backup.Replace(EXT_BACKUP, "");
            }

            if (File.Exists(backup))
            {
                File.Move(backup, path);
                Debug.Log($"[GuruMax] --- Max Menu recovered");
                return true;
            }
            
            Debug.LogError($"File not found: {path}");
            return false;
        }



        #endregion

        #region AppLovinSettings
        
        //销毁其他路径的 AppLovinSettings
        [Test]
        public void CheckAndCleanOtherSettings()
        {
            var path = GetFullPath(SettingsPath);

            if (this.FileExists(path))
            {
                this.DeleteFile(path);
                Debug.Log($"[GuruMax]<color=#88ff00> --- AppLovinSettings has been removed: {path} </color>");
            }
        }

        #endregion

    }
}