
namespace Guru.Editor
{
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;
    using System;
    using System.IO;
    using System.Xml;
    
    public static class AndroidManifestMod
    {
        private const string TargetPath = "Plugins/Android/AndroidManifest.xml";
        private const string ValOptimizeInitialization = "com.google.android.gms.ads.flag.OPTIMIZE_INITIALIZATION";
        private const string ValOptimizeAdLoading = "com.google.android.gms.ads.flag.OPTIMIZE_AD_LOADING";

        private const string PermissionReadPostNotifications = "android.permission.POST_NOTIFICATIONS";
        private const string PermissionReadPhoneState = "android.permission.READ_PHONE_STATE";
        private const string PermissionAccessCoarseLocation = "android.permission.ACCESS_COARSE_LOCATION";
        private const string PermissionAccessFineLocation = "android.permission.ACCESS_FINE_LOCATION";
        private const string PermissionReadExternalStorage = "android.permission.READ_EXTERNAL_STORAGE";
        private const string PermissionReadLogs = "android.permission.READ_LOGS";
        private const string NetworkSecurityConfig = "networkSecurityConfig";
        private const string NetworkSecurityConfigValue = "@xml/network_security_config";
        private const string PermissionAdjustReadPermission = "com.adjust.preinstall.READ_PERMISSION"; // Adjust permission
        private const string AdjustQueriesActionValue = "com.attribution.REFERRAL_PROVIDER"; // Adjust action
        
        // Add Permissions
        private static string[] addPermissions = new[]
        {
            PermissionReadPostNotifications,
            PermissionAdjustReadPermission,
        };
        
        // Remove Permissions
        private static string[] removePermissions = new[]
        {
            PermissionReadPhoneState,
            PermissionAccessCoarseLocation,
            PermissionAccessFineLocation,
            PermissionReadExternalStorage,
            PermissionReadLogs,
        };
        

        private static string TargetFullPath = Path.Combine(Application.dataPath, TargetPath);
        
        public static bool IsManifestExist() => File.Exists(TargetFullPath);

        public static void Apply(string urlSchemaList = "")
        {
            if (!IsManifestExist())
            {
                CopyManifest();
            }
            
            FixAndroidManifest(urlSchemaList);
        }

        
        /// <summary>
        /// Fix Android Manifest
        /// </summary>
        private static void FixAndroidManifest(string urlSchemaList = "")
        {
            var doc = AndroidManifestDoc.Load(TargetFullPath);
            
            // --- network_security_config ---
            doc.SetApplicationAttribute(NetworkSecurityConfig, NetworkSecurityConfigValue);
            doc.AddApplicationReplaceItem($"android:{NetworkSecurityConfig}");
            // ---- Metadata ---
            doc.SetMetadata(ValOptimizeInitialization, "true");
            doc.SetMetadata(ValOptimizeAdLoading, "true");
            // ---- Permission ---
            // Add needed permissions
            foreach (var p in addPermissions)
            {
                doc.AddPermission(p);
            }
            // Remove sensitive permissions
            foreach (var p in removePermissions)
            {
                doc.RemovePermission(p);
            }

            // --- Bundle Id ---
            doc.SetPackageName(PlayerSettings.applicationIdentifier);
            
            // --- Adjust Preinstall (Content provider) ---
            doc.AddQueriesIntent(AdjustQueriesActionValue);
            
            // --- Adjust AddAndroidUrlSchema
            if (!string.IsNullOrEmpty(urlSchemaList))
            {
                AddAdjustUrlSchemaList(doc, urlSchemaList);
            }
            
            doc.Save();
        }
        

        /// <summary>
        /// 拷贝 AndroidManifest
        /// </summary>
        private static void CopyManifest()
        {
            if (File.Exists(TargetFullPath)) return;
            
            var path = GuruEditorHelper.GetAssetPath(nameof(AndroidManifestMod), "Script", true);
            if (!string.IsNullOrEmpty(path))
            {
                var from = Path.GetFullPath($"{path}/../../Files/AndroidManifest.txt");
                if (File.Exists(from))
                {
                    File.Copy(from, TargetFullPath);
                }
            }
        }

        /// <summary>
        /// 加入 Adjust URL Schema
        /// 根据 Adjust 官方的文档：https://dev.adjust.com/en/sdk/android/features/deep-links/?version=v4
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="urlSchemaList"></param>
        private static void AddAdjustUrlSchemaList(AndroidManifestDoc doc,  string urlSchemaList)
        {
            var mainActivity = doc.GetMainActivityNode();
            if (mainActivity == null)
            {
                Debug.LogError($"Can not find MainActivity in {TargetPath}");
                return;
            }

            var nodeName = "intent-filter";
            var intentList = mainActivity.SelectNodes(nodeName);
            if (intentList == null)
            {
                Debug.LogError($"Can not find intent-filter in {TargetPath}");
                return;
            }

            XmlElement adjustIntentNode = null;
            foreach( XmlElement node in intentList)
            {
                if (node != null 
                    && node.InnerXml.Contains("category android:name=\"android.intent.category.DEFAULT\"") 
                    && node.InnerXml.Contains("category android:name=\"android.intent.category.BROWSABLE\""))
                {
                    adjustIntentNode = node;
                    break;
                }
            }

            if (adjustIntentNode != null)
            {
                doc.RemoveChildNode(adjustIntentNode, mainActivity);
            }
            
            /************************* 构建 Adjust Schema 样式 ****************************
             *
                <intent-filter android:autoVerify="true">
                    <action android:name="android.intent.action.VIEW" />
                    <category android:name="android.intent.category.DEFAULT" />
                    <category android:name="android.intent.category.BROWSABLE" />
                    <data android:scheme="http" android:host="insights.go.link" />
                    <data android:scheme="https" android:host="insights.go.link" />
                </intent-filter>
             *
             */
            // node
            adjustIntentNode = doc.AddChildNode(nodeName, mainActivity);
            doc.AddAndroidAttribute(adjustIntentNode, "autoVerify", "true");
            // children
            var child = doc.AddChildNode("action", adjustIntentNode);
            doc.AddAndroidAttribute(child, "name", "android.intent.action.VIEW");
            child = doc.AddChildNode("category", adjustIntentNode);
            doc.AddAndroidAttribute(child, "name", "android.intent.category.DEFAULT");
            child = doc.AddChildNode("category", adjustIntentNode);
            doc.AddAndroidAttribute(child, "name", "android.intent.category.BROWSABLE");
            // schemas
            var schemas = urlSchemaList.Trim().Split(',');
            var schemaName = "";
            var hostName = "";
            foreach (var s in schemas)
            {
                if(string.IsNullOrEmpty(s)) continue;
                
                if (s.StartsWith("http"))
                {
                    schemaName = "https";
                    hostName = s.Replace("https", "").Replace("://", "");
                }
                else if (s.StartsWith("http"))
                {
                    schemaName = "http";
                    hostName = s.Replace("http", "").Replace("://", "");
                }
                else
                {
                    schemaName = s;
                }

                // 添加数据节点
                var n = doc.AddChildNode("data", adjustIntentNode);
                doc.AddAndroidAttribute(n, "scheme", schemaName);
                if (!string.IsNullOrEmpty(hostName))
                {
                    doc.AddAndroidAttribute(n, "host", hostName);
                }
            }
        }


        #region Testing

        [Test]
        public static void Test_Injection()
        {
            Apply();
        }

        #endregion
        
    }
}