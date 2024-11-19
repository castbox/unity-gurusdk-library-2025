namespace Guru.Editor
{
    using System;
    using System.IO;
    using UnityEngine;
    
    /// <summary>
    /// Create androidlib assets
    /// </summary>
    public class AndroidLibHelper
    {
        
        private static readonly string PluginsRoot = "Plugins/Android";
        private static readonly string Extends = "androidlib";
        private static readonly string ProjectPropertiesName = "project.properties";
        private static readonly string ProjectPropertiesContent= "target=android-9\nandroid.library=true";
        private static readonly string AndroidManifestName = "AndroidManifest.xml";
        private static readonly string AndroidManifestContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\"\n          package=\"{0}\"\n          android:versionCode=\"1\"\n          android:versionName=\"1.0\">\n</manifest>";

        
        public static bool IsEmbeddedAndroidLibExists(string fileName)
        {
            string dir = Path.GetFullPath($"{Application.dataPath}/{PluginsRoot}/{fileName}.{Extends}");
            return Directory.Exists(dir);
        }


        public static string CreateLibRoot(string packageName, string fileName = "")
        {
            if (string.IsNullOrEmpty(packageName)) return "";
            
            if(string.IsNullOrEmpty(fileName)) fileName = packageName;
            
            string dir = Path.GetFullPath($"{Application.dataPath}/{PluginsRoot}/{fileName}.{Extends}");
            if (Directory.Exists(dir))
            {
                return dir;
            }
            Directory.CreateDirectory(dir);

            string path = "";
            string content = "";
            
            //------ Create project.properties ------
            content = ProjectPropertiesContent;
            path = $"{dir}/{ProjectPropertiesName}";
            File.WriteAllText(path, content);
            // ------ Create AndroidManifest.xml ------
            content = AndroidManifestContent.Replace("{0}", packageName);
            path = $"{dir}/{AndroidManifestName}";
            File.WriteAllText(path, content);
            
            return dir;
        }

    }
}