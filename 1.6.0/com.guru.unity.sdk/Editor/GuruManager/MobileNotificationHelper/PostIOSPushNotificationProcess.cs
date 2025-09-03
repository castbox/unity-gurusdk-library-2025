#if UNITY_IOS
#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor.iOS.Xcode;

/*
 * 
 * PostIOSPushNotificationProcess 
 *
 * < 工具说明 >
 * 此类仅用于在 iOS 构建流程中，将各插件或者模块生成的多个 entitlements 文件合并到主要的 Unity-iPhone.entitlements 文件中。
 * 
 * 工具的功能包括：
 * 在项目中，用户可以在以下路径配置 "Assets/Guru/entitlement_settings.txt" 文件用于定义合并规则:
 * - entitlement_settings 整体为一个 Json 格式的配置文件，若此文件存在，则优先使用此文件中定义的规则进行 entitlements 合并
 * - entitlement_settings 中定义了 ReplaceKeys 属性数组，其中每个元素标记覆盖用的 key, 在扫描所有的 entitlements 文件时， 若遇到此 key 的冲突，将会用文档中的值覆盖 Unity-iPhone.entitlements 对应的 Key-Value 的值
 * - 鉴于有些文件包含的特殊的定义 ReplaceKeys 的写法中可以如此定义: "custom_key_1:file_name", 则此时，当读取到的文件名称匹配 file_name(不带扩展名) 时， 才会执行 key-value 的覆盖，否则跳过执行覆盖操作
 * - entitlement_settings 中定义了 RemoveKeys 属性数组, 在此数组内定义的 key 将会从 Unity-iPhone.entitlements 中移除
 * - 若同样的 key 在 ReplaceKeys 以及 RemoveKeys 都存在，则优先执行 RemoveKeys 的操作
 * - 合并后的文件将会保存到 {path-of-xcode-project}/Unity-iPhone.entitlements 文件内
 * - 其他生成的 entitlements 文件暂时保持不变
 * - 签名配置中奖只会指定 Unity-iPhone.entitlements  作为唯一的 entitlements 文件
 * 
 * 为了确保此流程可以顺利的执行，PostProcessBuild 的 Order 被设置到最大值： int.MaxValue - 1
 * 
 */
public class PostIOSPushNotificationProcess
{
    private const int CallbackOrder = int.MaxValue - 1;
    private const string MainTargetName = "Unity-iPhone";
    
    
    [PostProcessBuild(CallbackOrder)]
    public static void OnPostProcessBuild(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS)
        {
            Debug.Log($"Skipping entitlement processing for non-iOS target: {target}");
            return;
        }
        
        if (string.IsNullOrEmpty(buildPath))
        {
            Debug.LogError("Build path cannot be null or empty");
            return;
        }

        ProcessAllEntitlements(buildPath);
    }


    private static void ProcessAllEntitlements(string buildPath)
    {
        var pbxProjectPath = PBXProject.GetPBXProjectPath(buildPath);
        var pbxProject = new PBXProject();
        pbxProject.ReadFromFile(pbxProjectPath);

        var mainTargetGuid = pbxProject.GetUnityMainTargetGuid();

        // Entitlement 名称限定为唯一主文件
        var entitlementFileName = $"{MainTargetName}.entitlements";
        var mainEntitlementPath = $"{buildPath}/{entitlementFileName}";

        // 合并所有 entitlements
        MergeAllEntitlements(mainEntitlementPath, buildPath, pbxProject);
        
        
        // 添加构建文件
        pbxProject.AddFile(entitlementFileName, entitlementFileName);
        pbxProject.SetBuildProperty(mainTargetGuid, "CODE_SIGN_ENTITLEMENTS", entitlementFileName); // 指定唯一的titlement
        
        
        // 保存修改
        pbxProject.WriteToFile(pbxProjectPath);    
        Debug.Log($"<color=#88ff00>[Post] --- Merge all entitlements to {mainEntitlementPath} :: Success!</color>");
    }


    private static void MergeAllEntitlements(string mainEntitlementPath, string buildPath, PBXProject pbxProject)
    {
        var settings = EntitlementSettings.Load();

        var mainDoc = new PlistDocument();
        if(File.Exists(mainEntitlementPath))
        {
            try
            {
                mainDoc.ReadFromFile(mainEntitlementPath); // 文件存在即读取，不存在也没有问题，会直接写入
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to read main entitlement file: {mainEntitlementPath}, Error: {ex.Message}");
            }
        }


        List<string> otherFiles = new List<string>();
        var dirInfo = new DirectoryInfo(buildPath);
        var files = dirInfo.GetFiles("*.entitlements", SearchOption.AllDirectories);
        foreach (var f in files)
        {
            if(f.Name.Equals($"{MainTargetName}.entitlements", StringComparison.OrdinalIgnoreCase))
                continue;
            
            otherFiles.Add(f.FullName);
        }

        if (otherFiles.Count > 0)
        {
            foreach (var filePath  in otherFiles)
            {
                MergeToMainEntitlement(ref mainDoc, filePath, settings, pbxProject);
            }
        }
        
        mainDoc.WriteToFile(mainEntitlementPath); // 保存主文件
    }
    
    /// <summary>
    /// 合并到主 entitlements 中
    /// </summary>
    /// <param name="mainDoc"></param>
    /// <param name="filePath"></param>
    /// <param name="settings"></param>
    private static void MergeToMainEntitlement(ref PlistDocument mainDoc, string filePath, EntitlementSettings? settings = null, PBXProject? pbxProject = null)
    {
        if (!File.Exists(filePath)) return;
        var fileName = Path.GetFileNameWithoutExtension(filePath); // 提前获取文件名
        var otherDoc = new PlistDocument();
        otherDoc.ReadFromFile(filePath);

        var mainRoot = mainDoc.root?.AsDict();
        var otherRoot = otherDoc.root?.AsDict();
        
        if (mainRoot == null || otherRoot == null)
        {
            Debug.LogWarning($"Invalid plist structure in entitlement files");
            return;
        }
        
        foreach (var kvp in otherRoot.values)
        {
            if (mainRoot.values.ContainsKey(kvp.Key))
            {
                // 无配置不移除
                if (settings == null) continue;
                
                // 优先执行移除对应的键
                if (settings.ContainsRemoveKey(kvp.Key))
                {
                    Debug.Log($"[Post] --- Remove key: {kvp.Key}");
                    mainRoot.values.Remove(kvp.Key);
                }
                else
                {
                    // 根据规则处理对应的逻辑
                    var keyInfo = settings.GetKeyInfo(kvp.Key);
                    if (keyInfo != null)
                    {
                        Debug.Log($"[Post] --- Replace key: {kvp.Key}");
                        bool shouldReplace = true; // 通用覆盖方案
                        string? keyFileName = keyInfo.FileName;
                        if (!string.IsNullOrEmpty(keyFileName))
                        {
                            shouldReplace = fileName.Equals(keyFileName, StringComparison.OrdinalIgnoreCase); // 是否为对应的替换文件
                        }

                        if(shouldReplace)
                            mainRoot.values[kvp.Key] = kvp.Value;
                    }
                }

            }
            else
            {
                Debug.Log($"[Post] --- Merge entitlements: {kvp.Key}");
                mainRoot.values.Add(kvp.Key, kvp.Value);
            }
        }

        // TODO: 保留此代码以备之后进行移除
        // if (pbxProject != null)
        // {
        //     // 合并完成，移除源文件
        //     var fileGuid = pbxProject.FindFileGuidByRealPath(filePath);
        //     if(fileGuid == null) return;
        //     pbxProject.RemoveFile(fileGuid);
        //     Debug.Log($"<color=orange>[Post] --- Delete other entitlements file: {filePath} </color>");
        // }
       
    }
    
}


public class EntitlementSettings
{
    public const string EntitlementSettingsName = "entitlement_settings.txt"; // 配置文件
    
    [JsonProperty("replace_keys")] 
    public List<string>? ReplaceKeys;

    [JsonProperty("remove_keys")] 
    public List<string>? RemoveKeys;
    
    
    public static EntitlementSettings? Load()
    {
        var filePath = Path.GetFullPath($"{Application.dataPath}/Guru/{EntitlementSettingsName}");
        if (!File.Exists(filePath))
        {
            return null;
        }
        
        try
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<EntitlementSettings>(json);
        }
        catch (Exception e)
        {
            Debug.LogException(e);

        }
        return null;
    }


    public EntitlementReplaceKeyInfo? GetKeyInfo(string key)
    {
        if (ReplaceKeys == null || string.IsNullOrEmpty(key)) return null;

        foreach (var replaceKey in ReplaceKeys)
        {
            if (string.IsNullOrEmpty(replaceKey)) continue;
            
            var parts = replaceKey.Split(':');
            var keyPart = parts[0];
            
            if (key == keyPart)
            {
                var info = new EntitlementReplaceKeyInfo { Key = keyPart };
                if (parts.Length > 1) info.FileName = parts[1];
                return info;
            }
        }
        return null;
    }

    public bool ContainsRemoveKey(string removeKey)
    {
        return RemoveKeys?.Contains(removeKey) ?? false;
    }

}

public class EntitlementReplaceKeyInfo
{
    public string Key;
    public string? FileName = null;
}


#endif