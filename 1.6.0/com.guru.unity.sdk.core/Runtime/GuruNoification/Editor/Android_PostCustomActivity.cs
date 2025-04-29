

#if UNITY_ANDROID

namespace Guru.Notification
{
    using System;
    using System.IO;
    using UnityEditor.Android;
    using System.Xml;
    
    public class PostCustomActivity: IPostGenerateGradleAndroidProject
    {
        private const int POST_ORDER = 10;
        private const string K_PREMISSION_POST_NOTIFICATIONS = "android.permission.POST_NOTIFICATIONS";
        private const string K_CUSTOM_NOTIFICATION_ACTIVITY = "custom_notification_android_activity";
        private const string V_DEFAULT_GURU_ACTIVITY = "com.google.firebase.messaging.MessageForwardingService";
        const string K_ANDROID_NAMESPACE_URI = "http://schemas.android.com/apk/res/android";
        
        public int callbackOrder => POST_ORDER;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            SetupAndroidManifest(path);
        }
        
        /// <summary>
        /// 设置 Android Manifest
        /// </summary>
        /// <param name="projectPath"></param>
        /// <exception cref="FileNotFoundException"></exception>
        private void SetupAndroidManifest(string projectPath)
        {
            var manifestPath = $"{projectPath}/src/main/AndroidManifest.xml";
            if (!File.Exists(manifestPath))
                throw new FileNotFoundException($"'{manifestPath}' doesn't exist.");

            XmlDocument manifestDoc = new XmlDocument();
            manifestDoc.Load(manifestPath);
            
            InjectAndroidManifest(manifestPath, manifestDoc);

            manifestDoc.Save(manifestPath);
        }
        
        internal static void InjectAndroidManifest(string manifestPath, XmlDocument manifestDoc)
        {
            string mainActivity = GetLauncherActivity(manifestDoc);

            AppendAndroidMetadataField(manifestPath, manifestDoc, K_CUSTOM_NOTIFICATION_ACTIVITY, mainActivity);
            AppendAndroidPermissionField(manifestPath, manifestDoc, K_PREMISSION_POST_NOTIFICATIONS);
            
            UnityEngine.Debug.Log($"<color=#88ff00>Add custom notification activity: {mainActivity} success!!</color>");
        }
        
        internal static void AppendAndroidPermissionField(string manifestPath, XmlDocument xmlDoc, string name, string maxSdk = null)
        {
            AppendAndroidPermissionField(manifestPath, xmlDoc, "uses-permission", name, maxSdk);
        }
        
        internal static void AppendAndroidPermissionField(string manifestPath, XmlDocument xmlDoc, string tagName, string name, string maxSdk)
        {
            var manifestNode = xmlDoc.SelectSingleNode("manifest");
            if (manifestNode == null)
                throw new ArgumentException(string.Format("Missing 'manifest' node in '{0}'.", manifestPath));

            XmlElement metaDataNode = null;
            foreach (XmlNode node in manifestNode.ChildNodes)
            {
                if (!(node is XmlElement) || node.Name != tagName)
                    continue;

                var element = (XmlElement)node;
                var elementName = element.GetAttribute("name", K_ANDROID_NAMESPACE_URI);
                if (elementName == name)
                {
                    if (maxSdk == null)
                        return;
                    var maxSdkAttr = element.GetAttribute("maxSdkVersion", K_ANDROID_NAMESPACE_URI);
                    if (!string.IsNullOrEmpty(maxSdkAttr))
                        return;
                    metaDataNode = element;
                }
            }

            if (metaDataNode == null)
            {
                metaDataNode = xmlDoc.CreateElement(tagName);
                metaDataNode.SetAttribute("name", K_ANDROID_NAMESPACE_URI, name);
            }
            if (maxSdk != null)
                metaDataNode.SetAttribute("maxSdkVersion", K_ANDROID_NAMESPACE_URI, maxSdk);

            manifestNode.AppendChild(metaDataNode);
        }
        
        internal static void AppendAndroidMetadataField(string manifestPath, XmlDocument xmlDoc, string name, string value)
        {
            var applicationNode = xmlDoc.SelectSingleNode("manifest/application");
            if (applicationNode == null)
                throw new ArgumentException(string.Format("Missing 'application' node in '{0}'.", manifestPath));

            var nodes = xmlDoc.SelectNodes("manifest/application/meta-data");
            if (nodes != null)
            {
                // Check if there is a 'meta-data' with the same name.
                foreach (XmlNode node in nodes)
                {
                    var element = node as XmlElement;
                    if (element == null)
                        continue;

                    var elementName = element.GetAttribute("name", K_ANDROID_NAMESPACE_URI);
                    if (elementName == name)
                    {
                        element.SetAttribute("value", K_ANDROID_NAMESPACE_URI, value);
                        return;
                    }
                }
            }

            XmlElement metaDataNode = xmlDoc.CreateElement("meta-data");
            metaDataNode.SetAttribute("name", K_ANDROID_NAMESPACE_URI, name);
            metaDataNode.SetAttribute("value", K_ANDROID_NAMESPACE_URI, value);

            applicationNode.AppendChild(metaDataNode);
        }
        
        internal static string GetLauncherActivity(XmlDocument xmlDoc)
        {
            var applicationNode = xmlDoc.SelectSingleNode("manifest/application");
            if (applicationNode == null)
                throw new ArgumentException($"Missing 'application' node in doc.");
            
            var nodes = xmlDoc.SelectNodes("manifest/application/activity");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    var activityNode = node as XmlElement;
                    if (activityNode == null)
                        continue;

                    if (activityNode.HasChildNodes)
                    {
                        var intentFilterNode = activityNode.SelectSingleNode("intent-filter");
                       
                        if(intentFilterNode == null || !intentFilterNode.HasChildNodes)
                            continue;
                        
                        foreach (XmlElement childNode in intentFilterNode)
                        {
                            if(childNode == null) continue;
                            
                            // 判断 action/category 二者取其一
                            if (childNode.Name == "action" && childNode.OuterXml.Contains("android.intent.action.MAIN"))
                            {
                                var activityName = activityNode.GetAttribute("name", K_ANDROID_NAMESPACE_URI);
                                return activityName;
                            }
                            
                            if (childNode.Name == "category" && childNode.OuterXml.Contains("android.intent.category.LAUNCHER"))
                            {
                                var activityName = activityNode.GetAttribute("name", K_ANDROID_NAMESPACE_URI);
                                return activityName;
                            }
                        }
                    }
                }
            }

            return V_DEFAULT_GURU_ACTIVITY;
        }
        
    }
}

#endif
