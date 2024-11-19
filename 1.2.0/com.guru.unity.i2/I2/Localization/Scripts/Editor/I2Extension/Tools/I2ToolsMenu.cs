using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Guru
{
    public static class I2ToolsMenu
    {
        [MenuItem("Assets/Guru/I2/添加TextFontHelper")]
        public static void SetUpTextFontHelper()
        {
            string[] guids = Selection.assetGUIDs;
            List<string> modifiedGuids = guids.Select(AssetDatabase.GUIDToAssetPath).ToList();
            AddOrRemoveFontHelper.AddAllFontFixer(modifiedGuids);
        }
        
        [MenuItem("Assets/Guru/I2/移除TextFontHelper")]
        public static void RemoveTextFontHelper()
        {
            string[] guids = Selection.assetGUIDs;
            List<string> modifiedGuids = guids.Select(AssetDatabase.GUIDToAssetPath).ToList();
            AddOrRemoveFontHelper.RemoveAllFontFixer(modifiedGuids);
        }

        [MenuItem("Assets/Guru/I2/移除Missing脚本")]
        public static void RemoveMiss()
        {
            string[] guids = Selection.assetGUIDs;
            List<string> modifiedGuids = guids.Select(AssetDatabase.GUIDToAssetPath).ToList();
            RemoveMissingScripts.RemoveAll(modifiedGuids);
        }
    }
}