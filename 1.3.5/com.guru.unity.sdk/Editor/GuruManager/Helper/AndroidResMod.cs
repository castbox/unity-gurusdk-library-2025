namespace Guru.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    
    public class AndroidResMod
    {
        private static readonly string NetworkSecurityXmlName = "network_security_config.xml";
        private static readonly string LibNetworkSecurity = "GuruNetworkSecurity";
        private static readonly string LibNetworkSecurityPackageName = "com.guru.unity.sdk.android.res.network.security";
        private static string OldSecurityXml = Path.Combine(Application.dataPath, $"Plugins/Android/res/xml/{NetworkSecurityXmlName}");
        
        /// <summary>
        /// 应用补丁
        /// </summary>
        public static void Apply()
        {
            DeployNetworkSecurity();
        }

        /// <summary>
        /// 部署网络安全配置
        /// </summary>
        private static void DeployNetworkSecurity()
        {
            if(File.Exists(OldSecurityXml)) File.Delete(OldSecurityXml); // 清理旧文件

            if (!AndroidLibHelper.IsEmbeddedAndroidLibExists(LibNetworkSecurity))
            {
                string dir = AndroidLibHelper.CreateLibRoot(LibNetworkSecurityPackageName, LibNetworkSecurity);
                var d = GuruEditorHelper.GetAssetPath(nameof(AndroidResMod), "Script", true);
                if (!string.IsNullOrEmpty(d))
                {
                    var from = $"{Directory.GetParent(d)?.FullName ?? ""}/../Files/{NetworkSecurityXmlName}";
                    if (File.Exists(from))
                    {
                        string toDir = $"{dir}/res/xml";
                        if(!Directory.Exists(toDir))Directory.CreateDirectory(toDir);
                        string to = $"{toDir}/{NetworkSecurityXmlName}";
                        FileUtil.CopyFileOrDirectory(from, to);
                    }
                }

            }
        }



    }
}