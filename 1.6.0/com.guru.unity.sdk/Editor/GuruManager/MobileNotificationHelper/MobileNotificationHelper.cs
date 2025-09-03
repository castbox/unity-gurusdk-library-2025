#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 移动通知插件自动修改
/// </summary>
public static class MobileNotificationHelper
{
    private const string KManifestJsonName = "manifest.json";
    private const string MobileNotificationPluginId = "com.unity.mobile.notifications";
    
    /// <summary>
    /// 插件是否已经安装
    /// </summary>
    /// <returns></returns>
    public static bool IsMobileNotificationPluginInstalled()
    {

        var manifestPath = Path.GetFullPath($"{Application.dataPath}/../Packages/{KManifestJsonName}");
        
        if (File.Exists(manifestPath))
        {
            var manifestJson = File.ReadAllText(manifestPath);
            return manifestJson.Contains(MobileNotificationPluginId);
        }

        return false;
    }


 

}


/// <summary>
/// 移动端通知插件自动修改启
/// </summary>
[InitializeOnLoad]
internal class MobileNotificationAutoModifier
{
    
    static MobileNotificationAutoModifier()
    {
        if (!MobileNotificationHelper.IsMobileNotificationPluginInstalled())
        {
            return;
        }

        // 自动加载移动通知开关
        CheckAndSetMobileEnabledForPlatform();
    }
    
    private static void CheckAndSetMobileEnabledForPlatform()
    {
        NotificationsSettings settings = NotificationsSettings.Load();

        if (settings == null)
        {
            return;
        }
        
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            // iOS 平台统一打开 Push 权限开关       
            settings.SetIOSValue("UnityAddRemoteNotificationCapability", "True"); // 添加权限开关， 同时会影响到 iOS 内的插件宏定义，  这里为自动添加
            settings.SetIOSValue("UnityRemoteNotificationForegroundPresentationOptions", "0"); // 从默认的 All 改为 Nothing， 确保 Push 消息不会在 App 位于前台时显示
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            // Android 暂时没有对应的处理    
        }
    }
    
}


[Serializable]
internal class NotificationsSettings
{
    private const string KFileName = "NotificationsSettings.asset";

    [JsonProperty("MonoBehaviour")]
    public SettingsRoot root;
    
    
    private static string GetFilePath()
    {
        return Path.GetFullPath($"{Application.dataPath}/../ProjectSettings/{KFileName}");
    }
    
    
    public static NotificationsSettings? Load()
    {
        var filePath = GetFilePath();
        if (!File.Exists(filePath))
            return null;
        try
        {
            return JsonConvert.DeserializeObject<NotificationsSettings>(File.ReadAllText(filePath));

        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        return null;
    }

    [Serializable]
    internal class SettingsRoot
    {
        [JsonProperty("m_Enabled")]
        public bool enabled;
        
        [JsonProperty("m_iOSNotificationSettingsValues")]
        public SettingsDict iOSNotificationSettingsValues;
        
        [JsonProperty("m_AndroidNotificationSettingsValues")]
        public SettingsDict AndroidNotificationSettingsValues;
    }

    [Serializable]
    internal class SettingsDict
    {
        [JsonProperty("m_Keys")]
        public List<string> keys;
        
        [JsonProperty("m_Values")]
        public List<string> values;


        public bool ContainsKey(string key)
        {
            if (keys == null || keys.Count == 0) return false;
            return keys.Contains(key);
        }

        /// <summary>
        /// 获取 Value 值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out string value)
        {
            value = string.Empty;
            bool result = false;
            
            if (values == null || values.Count == 0) return result;

            try
            { 
                if (ContainsKey(key))
                {
                    var idx = keys.IndexOf(key);
                    if (idx >= values.Count)
                        return result;   
                
                    value = values[idx];
                    result = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
            return result;
        }

        /// <summary>
        /// 设置 Value 值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool SetValue(string key, string value)
        {
            if (TryGetValue(key, out var oldValue))
            {
                if (oldValue == value) 
                    return false;
                
                var idx = keys.IndexOf(key);
                values[idx] = value;
            }
            else
            {
                keys.Add(key);
                values.Add(value);
            }
            return true;
        }

    }
    
    public void Save()
    {
        var filePath = GetFilePath();
        File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
    
    public bool SetAndroidValue(string key, string value)
    {
        var result = root?.AndroidNotificationSettingsValues?.SetValue(key, value) ?? false;
        if (result) Save();
        return result;
    }
    
    public bool SetIOSValue(string key, string value)
    {
        var result = root?.iOSNotificationSettingsValues?.SetValue(key, value) ?? false;
        if (result) Save();
        return result;
    }


}



