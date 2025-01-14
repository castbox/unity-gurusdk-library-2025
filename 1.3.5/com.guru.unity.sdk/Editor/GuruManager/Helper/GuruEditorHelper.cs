namespace Guru.Editor
{
    using UnityEditor;
    using UnityEngine;
    using System;
    using System.IO;
    
    public class GuruEditorHelper
    {
        public static string GetAssetPath(string filter, bool useFullPath = false)
        {
            var guids = AssetDatabase.FindAssets(filter);
            string path = "";
            string fullPath = "";
            if (guids != null && guids.Length > 0)
            {
                path = AssetDatabase.GUIDToAssetPath(guids[0]);
                fullPath = path.Replace("Assets", Application.dataPath);
                if (File.Exists(fullPath))
                {
                    return useFullPath? fullPath : path;
                }
            }
            return "";
        }
        
        public static string GetAssetPath(string fileName, string typeName = "", bool useFullPath = false)
        {
            var filter = fileName;
            if(!string.IsNullOrEmpty(typeName)) filter = $"{fileName} t:{typeName}";
            return GetAssetPath(filter, useFullPath);
        }

        public static void OpenPath(string path)
        {
#if UNITY_EDITOR_OSX
            EditorUtility.RevealInFinder(path);
            return;
#endif
            Application.OpenURL($"file://{path}");
        }



    }
}