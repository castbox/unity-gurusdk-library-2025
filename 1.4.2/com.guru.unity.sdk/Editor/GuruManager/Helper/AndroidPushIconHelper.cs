namespace Guru.Editor
{
    using UnityEditor;
    using UnityEngine;
    using System.IO;

    public class AndroidPushIconHelper
    {
        public static readonly int targetWidth = 96; // 目标宽度
        public static readonly int targetHeight = 96;
        private static readonly string LibName = "SDKRes";
        private static readonly string PackageName = "com.guru.unity.res";
        private static readonly string IconName = "ic_notification.png";

        private static readonly string ColorContent =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<resources>\n    <color name=\"colorAccent\">#{0}</color>\n</resources>";
        private static readonly string ValueContent =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<resources>\n    <string name=\"default_notification_channel_id\" translatable=\"false\">{0}</string>\n</resources>";
        private static readonly string[] iconNames = new string[]
        {
            "drawable-mdpi",
            "drawable-hdpi",
            "drawable-xhdpi",
            "drawable-xxhdpi",
            "drawable-xxxhdpi"
        };
        
        // 此处需要对齐到中台的 push channel 中 high 等级
        // 文档链接：https://docs.google.com/document/d/1aBKqXKi88tu4xhQWd46yhqWU3Pu_U5Gkiow_JdLhpLk/edit?tab=t.0#heading=h.82n7wupa0xzj
        private static readonly string DefaultFirebaseChannelId = "guru_push_high"; 

        /// <summary>
        /// 设置推送图标
        /// </summary>
        /// <param name="source"></param>
        /// <param name="color"></param>
        public static bool SetPushIconAssets(Texture2D source, Color color = default(Color))
        {
            if (source == null)
            {
                Debug.LogError($"=== No Texture2D found ===");
                return false;
            }
            return DeployAllIcons(source, color);
        }
        
        private static Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
        {
            MakeTextureReadable(source);
            
            // Texture2D result = new Texture2D(newWidth, newHeight);
            // Color[] newColors = new Color[newWidth * newHeight];
            //
            // for (int y = 0; y < newHeight; y++)
            // {
            //     for (int x = 0; x < newWidth; x++)
            //     {
            //         // 应用一些缩放逻辑来获取新的颜色值
            //         newColors[x + y * newWidth] = source.GetPixelBilinear((float)x / newWidth * source.width, (float)y / newHeight * source.height);
            //     }
            // }
            //
            // result.SetPixels(newColors);
            // result.Apply();
            // return result;
            
            
            RenderTexture rt=new RenderTexture(newWidth, newHeight,24);
            RenderTexture.active = rt;
            Graphics.Blit(source,rt);
            Texture2D result=new Texture2D(newWidth,newHeight);
            result.ReadPixels(new Rect(0,0,newWidth,newHeight),0,0);
            result.Apply();
            return result;
        }
        
        private static void MakeTextureReadable(Texture2D source)
        {
            if (source.isReadable) return;

            var path = AssetDatabase.GetAssetPath(source);
            TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(path);
            if (!ti.isReadable)
            {
                ti.isReadable = true;
                ti.SaveAndReimport();
            }
        }
        
        
        private static string ColorToHex(Color color)
        {
            return string.Format("{0:X2}{1:X2}{2:X2}", (int)(color.r * 255), (int)(color.g * 255), (int)(color.b * 255));
        }
        

        private static bool DeployAllIcons(Texture2D source, Color color)
        {
            var dir = AndroidLibHelper.CreateLibRoot(PackageName, LibName);
            string path = "";
            string content = "";
            
            var result = ResizeTexture(source, targetWidth, targetHeight);
            byte[] bytes = result.EncodeToPNG();


            var resPath = $"{dir}/res";
            if (!Directory.Exists(resPath))
            {
                Directory.CreateDirectory(resPath);
            }
            
            File.WriteAllBytes($"{resPath}/{IconName}", bytes); // Base Icon;
            // ----- Build all  Icons ------
            foreach (var iconName in iconNames)
            {
                var iconPath = $"{resPath}/{iconName}";
                if (!Directory.Exists(iconPath))
                {
                    Directory.CreateDirectory(iconPath);
                }
                File.WriteAllBytes($"{iconPath}/{IconName}", bytes);
            }

            var valuesPath = $"{resPath}/values";
            if (!Directory.Exists(valuesPath)) Directory.CreateDirectory(valuesPath);
            
            // ----- Build colors.xml ------
            path = $"{valuesPath}/colors.xml";
            content = ColorContent.Replace("{0}", ColorToHex(color));
            File.WriteAllText(path, content);
            // ----- Build strings.xml ------
            path = $"{valuesPath}/strings.xml";
            content = ValueContent.Replace("{0}", DefaultFirebaseChannelId);
            File.WriteAllText(path, content);
            
            // ----- Inject AndroidManifest.xml ------
            var doc = AndroidManifestDoc.Load();
            if (doc != null)
            {
                doc.SetMetadata("com.google.firebase.messaging.default_notification_icon", "@drawable/ic_notification", valueName:"resource");
                doc.SetMetadata("com.google.firebase.messaging.default_notification_color", "@color/colorAccent", valueName:"resource");
                doc.SetMetadata("com.google.firebase.messaging.default_notification_channel_id", "@string/default_notification_channel_id");
                doc.Save();
                Debug.Log("<color=#88ff00> --- Push Icon Build Success --- </color>");
                
                AssetDatabase.Refresh();
                return true;
            }
            
            Debug.LogError("AndroidManifest.xml not found ...");
            return false;
        }


    }
}