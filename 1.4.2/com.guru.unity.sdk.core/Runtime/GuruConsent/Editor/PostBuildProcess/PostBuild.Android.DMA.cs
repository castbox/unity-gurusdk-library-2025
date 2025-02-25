#if UNITY_ANDROID

namespace Guru.Editor
{
    using System.IO;
    using System.Xml;
    using NUnit.Framework;
    using UnityEditor.Android;
    using UnityEngine;
    
    public class PostBuildAndroidDMA: IPostGenerateGradleAndroidProject
    {
        public int callbackOrder { get; } = 1;
        public void OnPostGenerateGradleAndroidProject(string buildPath)
        {
            var dir = $"{buildPath}/src/main";
            OnSetAndroidManifest(dir);
        }
        
        
        #region Android Inject
        /*--------------------------------------------------------------------------------
         *
         *
         * 向 AndroidManifest 中添加以下数据
         * <meta-data android:name="google_analytics_default_allow_analytics_storage" android:value="true" />
         * <meta-data android:name="google_analytics_default_allow_ad_storage" android:value="true" />
         * <meta-data android:name="google_analytics_default_allow_ad_user_data" android:value="true" />
         * <meta-data android:name="google_analytics_default_allow_ad_personalization_signals" android:value="true" />
         *
         * 
        --------------------------------------------------------------------------------*/

        
        public static bool DefaultValue = true; // default value
        
        /// <summary>
        /// 修复 AndroidManifest.xml
        /// </summary>
        /// <param name="path"></param>
        private static void OnSetAndroidManifest(string path)
        {
            var filePath = Path.Combine(path, "AndroidManifest.xml");

            // 预注入 Key
            string[] dma_keys = new string[]
            {
                "google_analytics_default_allow_analytics_storage",
                "google_analytics_default_allow_ad_storage",
                "google_analytics_default_allow_ad_user_data",
                "google_analytics_default_allow_ad_personalization_signals"
            };

            int[] flags = new int[] { 0, 0, 0, 0 };

            string defaultValue = DefaultValue.ToString().ToLower();

            if (File.Exists(filePath))
            {
                var xmlStr = File.ReadAllText(filePath);

                var doc = new XmlDocument();
                doc.LoadXml(xmlStr);

                var root = doc.SelectSingleNode("manifest") as XmlElement;
                if (root == null) return;
                if (root.SelectSingleNode("application") is XmlElement app)
                {
                    var namespace_uri = root.GetAttribute("xmlns:android");
                    
                    var list = app.SelectNodes("meta-data");
                    
                    // ----- refresh exist values --------
                    foreach (XmlElement item in list)
                    {
                        for(int i = 0; i < dma_keys.Length; i++)
                        {
                            if (item.HasAttributes && item.GetAttribute("android:name") == dma_keys[i])
                            {
                                item.SetAttribute("value", namespace_uri, defaultValue);
                                flags[i] = 1;
                            }
                        }
                    }

                    // ------ Inject metadata -----------
                    for (int i = 0; i < flags.Length; i++)
                    {
                        if (flags[i] == 0)
                        {
                            var node = doc.CreateElement("meta-data");
                            node.SetAttribute("name", namespace_uri, dma_keys[i]);
                            node.SetAttribute("value", namespace_uri, defaultValue);
                            app.AppendChild(node);
                        }
                    }

                    // ------- Delay Measurement ------------
                    if (GoogleDMAHelper.UsingDelayAppMeasurement)
                    {
                        var node = doc.CreateElement("meta-data");
                        node.SetAttribute("name", namespace_uri, "com.google.android.gms.ads.DELAY_APP_MEASUREMENT_INIT");
                        node.SetAttribute("value", namespace_uri, "true");
                        app.AppendChild(node);
                    }
                    
                    doc.Save(filePath);
                    Debug.Log($"<color=#88ff00>[Post] inject AndroidManifest.xml at {filePath} success.</color>");
                }

            }
            else
            {
                Debug.LogError($"[Post] can't find AndroidManifest.xml at {filePath}");
            }
        }
        
        #endregion


        #region 单元测试

        [Test]
        public static void Test_AndroidManifestInject()
        {
            OnSetAndroidManifest($"{Application.dataPath}/Plugins/Android");
        }
        
        #endregion
    }
}

#endif
