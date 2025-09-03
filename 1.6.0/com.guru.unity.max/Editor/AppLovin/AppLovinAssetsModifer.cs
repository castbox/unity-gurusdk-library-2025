using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


/************************ Applovin 文件修改器 ************************
 * Author: HuYufei
 * First Version: 1.0.0
 * Commit Date: 2025/7/31
 *
 * Example Path:
 * - com.guru.sdk.framework.ads.max/Runtime/Core/MaxSdk/Scripts/IntegrationManager/Editor/AppLovinProcessGradleBuildFile.cs
 * - com.guru.sdk.framework.ads.max/Runtime/Core/MaxSdk/Scripts/IntegrationManager/Editor/AppLovinSettings.cs
 *
 * ---- 主要功能 ----
 * 1. 在 AppLovinSettings 上添加 Guru 的扩展属性，如 qualityServiceVersion， 用于项目组可手动配置对应的 Applovin AdReview 插件的版本号
 * 2. 修改 AppLovinProcessGradleBuildFile 内注入 baseTemplate 的代码，可实现打包时动态注入 #1 中的配置
 * 3. 留作之后扩展和修改 AppLovin 插件使用
 *
/************************ Applovin 文件修改器 ************************/


namespace Guru
{
    /// <summary>
    /// AppLovin 组件修改器
    /// </summary>
    public class AppLovinAssetsModifer
    {
        private const string DefaultQualityServiceVersion = "5.9.1";
        private const string FileAppLovinProcessGradleBuildFile = "AppLovinProcessGradleBuildFile";
        private const string FileAppLovinSettings = "AppLovinSettings";

        private const string KGuruExtensionsBegin = "GuruExtensionBegin";
        private const string KGuruExtensionsEnd = "GuruExtensionEnd";
        private static readonly string AdReviewTemplate = @"//---------------- GuruExtensionBegin ----------------//
    #region Guru 属性扩展
    [Header(""[AdReview] 服务版本"")]
    [SerializeField] private string qualityServiceVersion = ""%VERSION%"";
    public string QualityServiceVersion
    {
        get => Instance.qualityServiceVersion;
        set => Instance.qualityServiceVersion = value;
    }
    #endregion
//---------------- GuruExtensionEnd ----------------//
";

        private const string PattenQualityServicePluginRoot = "string QualityServicePluginRoot =";
        private const string FixedQualityServicePluginRoot = "        private static string QualityServicePluginRoot = $\"    id 'com.applovin.quality' version '{ AppLovinSettings.Instance.QualityServiceVersion }' apply false // NOTE: Requires version 4.8.3+ for Gradle version 7.2+ \"; // Service version fixed by Guru.";
        
        
        private const string PattenQualityServiceDependencyClassPath = "string QualityServiceDependencyClassPath =";
        private const string FixedQualityServiceDependencyClassPath = "        private static string QualityServiceDependencyClassPath = $\"classpath 'com.applovin.quality:AppLovinQualityServiceGradlePlugin:{ AppLovinSettings.Instance.QualityServiceVersion }'\"; // Service version fixed by Guru.";
        

        #region ApplovinSetting

        /*
         * 功能描述：
         * 此功能针对 AppLovinSettings.cs 文件进行主动修改
         * - 1. 利用 AssetDatabase 查找 AppLovinSettings.cs 文件是否存在，如果不存在，则返回 false
         * - 2. 读取 AppLovinSettings.cs 文件的文本内容， 先判断文档内是否包含 GuruExtensionsBegin 及 GuruExtensionEnd 字段
         * - 2.1 如果包含，则将 GuruExtensionsBegin 到 GuruExtensionEnd 之间的内容进行替换， 替换内容详见 3
         * - 2.2 如果不包含，则直接将相关的内容添加到文件结尾的 "\n}" 之前， 添加内容详见 3
         * - 3 方法 GetAdReviewContent 将返回需要扩展的文本内容
         * - 4 修改后将所有文本内容覆盖写入到 AppLovinSettings.cs 文件中
         */
        internal bool ModifyAppLovinSettings()
        {
            // 1. 查找 AppLovinSettings.cs 文件
            var guids = AssetDatabase.FindAssets($"{FileAppLovinSettings} t:script");
            if (guids.Length == 0)
            {
                Debug.LogError($"未找到 {FileAppLovinSettings} 文件");
                return false;
            }

            var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var filePath = Path.GetFullPath(Application.dataPath + "/../" + assetPath);

            try
            {
                // 2. 读取文件内容
                var fileContent = File.ReadAllText(filePath);

                // 生成修改的字段
                var fixContent = GetAdReviewContent(DefaultQualityServiceVersion);

                string newContent;

                // 2.1 检查是否已包含 Guru 扩展标记
                if (fileContent.Contains(KGuruExtensionsBegin) && fileContent.Contains(KGuruExtensionsEnd))
                {
                    // 宽松查找：查找包含标记关键字的任何行
                    var beginIndex = FindLineContaining(fileContent, KGuruExtensionsBegin);
                    var endIndex = FindLineContaining(fileContent, KGuruExtensionsEnd);

                    if (beginIndex >= 0 && endIndex >= 0 && endIndex > beginIndex)
                    {
                        // 找到结束标记行的结尾
                        var endLineIndex = fileContent.IndexOf('\n', endIndex);
                        if (endLineIndex == -1) endLineIndex = fileContent.Length;
                        else endLineIndex++; // 包含换行符

                        newContent = fileContent.Substring(0, beginIndex) +
                                   fixContent +
                                   fileContent.Substring(endLineIndex);

                        Debug.Log($"成功替换了 {FileAppLovinSettings} 中已存在的 Guru 扩展内容");
                    }
                    else
                    {
                        // 如果无法正确定位标记，尝试删除所有包含这些标记的内容块
                        Debug.LogWarning($"在 {FileAppLovinSettings} 中找到了扩展标记，但格式可能不正确，尝试强制清理并重新添加");

                        // 强制清理策略：删除包含关键字的所有行及其周围内容
                        newContent = CleanupAndAddContent(fileContent, fixContent);
                    }
                }
                else
                {
                    // 2.2 如果不包含，则在文件结尾的 "\n}" 之前添加内容
                    var lastBraceIndex = fileContent.LastIndexOf("\n}");
                    if (lastBraceIndex == -1)
                    {
                        Debug.LogError($"{FileAppLovinSettings} 文件格式不正确，未找到结尾的大括号");
                        return false;
                    }

                    newContent = fileContent.Substring(0, lastBraceIndex) +
                               "\n" + fixContent +
                               fileContent.Substring(lastBraceIndex);
                }

                // 4. 写入修改后的内容
                File.WriteAllText(filePath, newContent);

                // 刷新 AssetDatabase
                AssetDatabase.Refresh();

                Debug.Log($"成功修改 {FileAppLovinSettings} 文件，添加了 AdReview 扩展");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"修改 {FileAppLovinSettings} 文件时发生错误: {ex.Message}");
                return false;
            }
        }


        private string GetAdReviewContent(string version)
        {
            var content = AdReviewTemplate.Replace("%VERSION%", version);
            return content;
        }

        /// <summary>
        /// 查找包含指定文本的行的起始位置
        /// </summary>
        private int FindLineContaining(string content, string searchText)
        {
            var lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(searchText))
                {
                    // 计算该行在原始字符串中的起始位置
                    int position = 0;
                    for (int j = 0; j < i; j++)
                    {
                        position += lines[j].Length + 1; // +1 for the '\n'
                    }
                    return position;
                }
            }
            return -1;
        }

        /// <summary>
        /// 强制清理已存在的扩展内容并添加新内容
        /// </summary>
        private string CleanupAndAddContent(string fileContent, string newContent)
        {
            var lines = fileContent.Split('\n').ToList();

            // 查找并删除包含 Guru 扩展标记的行以及它们之间的所有内容
            int beginLineIndex = -1;
            int endLineIndex = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains(KGuruExtensionsBegin))
                {
                    beginLineIndex = i;
                }
                if (lines[i].Contains(KGuruExtensionsEnd))
                {
                    endLineIndex = i;
                    break;
                }
            }

            // 如果找到了标记，删除相关行
            if (beginLineIndex >= 0 && endLineIndex >= 0 && endLineIndex >= beginLineIndex)
            {
                // 删除从开始标记到结束标记的所有行
                for (int i = endLineIndex; i >= beginLineIndex; i--)
                {
                    lines.RemoveAt(i);
                }
                Debug.Log($"已清理 {endLineIndex - beginLineIndex + 1} 行旧的扩展内容");
            }
            else if (beginLineIndex >= 0)
            {
                // 如果只找到开始标记，删除从开始标记到文件末尾前的所有可疑内容
                Debug.LogWarning("只找到开始标记，将尝试清理到下一个大括号");

                // 查找开始标记后的第一个 "}" 或类似结构
                for (int i = beginLineIndex; i < lines.Count; i++)
                {
                    if (lines[i].Trim().StartsWith("}") ||
                        lines[i].Contains("endregion") ||
                        lines[i].Contains("//---"))
                    {
                        // 删除到这一行（包含）
                        for (int j = i; j >= beginLineIndex; j--)
                        {
                            lines.RemoveAt(j);
                        }
                        break;
                    }
                }
            }

            // 重新组装内容并在结尾的 "}" 之前添加新内容
            var result = string.Join("\n", lines);
            var lastBraceIndex = result.LastIndexOf("\n}");

            if (lastBraceIndex >= 0)
            {
                result = result.Substring(0, lastBraceIndex) +
                        "\n" + newContent +
                        result.Substring(lastBraceIndex);
            }
            else
            {
                // 如果没找到结尾大括号，直接追加
                result += "\n" + newContent;
            }

            return result;
        }


        #endregion



        #region AppLovinProcessGradleBuildFile

        /*
         * 功能描述：
         * 此功能针对 AppLovinProcessGradleBuildFile.cs 文件进行主动修改
         * - 1. 利用 AssetDatabase 查找 AppLovinProcessGradleBuildFile.cs 文件是否存在，如果不存在，则返回 false
         * - 2. 读取 AppLovinProcessGradleBuildFile.cs 文件的文本内容
         * - 2.1 如果某行包含 PattenQualityServicePluginRoot 的内容，则将此行替换为 FixedQualityServicePluginRoot
         * - 3 修改后将所有文本内容覆盖写入到 AppLovinProcessGradleBuildFile.cs 文件中
         */
        internal bool ModifyAndroidProcessGradleBuildFile()
        {
            try
            {
                // 1. 利用 AssetDatabase 查找 AppLovinProcessGradleBuildFile.cs 文件是否存在
                var guids = AssetDatabase.FindAssets($"{FileAppLovinProcessGradleBuildFile} t:script");
                if (guids.Length == 0)
                {
                    Debug.LogError($"未找到 {FileAppLovinProcessGradleBuildFile} 文件");
                    return false;
                }

                var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                var filePath = Path.GetFullPath(Application.dataPath + "/../" + assetPath);

                // 2. 读取 AppLovinProcessGradleBuildFile.cs 文件的文本内容
                var fileContent = File.ReadAllText(filePath);
                var originalContent = fileContent;
                bool modified = false;

                // 2.1 如果某行包含 PattenQualityServicePluginRoot 的内容，则将此行替换为 FixedQualityServicePluginRoot
                if (fileContent.Contains(PattenQualityServicePluginRoot))
                {
                    var lines = fileContent.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains(PattenQualityServicePluginRoot))
                        {
                            lines[i] = FixedQualityServicePluginRoot;
                            modified = true;
                            Debug.Log($"已替换包含 '{PattenQualityServicePluginRoot}' 的行");
                            break; // 只替换第一个匹配的行
                        }
                    }
                    fileContent = string.Join("\n", lines);
                }

                // 额外处理：如果某行包含 PattenQualityServiceDependencyClassPath 的内容，也进行替换
                if (fileContent.Contains(PattenQualityServiceDependencyClassPath))
                {
                    var lines = fileContent.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains(PattenQualityServiceDependencyClassPath))
                        {
                            lines[i] = FixedQualityServiceDependencyClassPath;
                            modified = true;
                            Debug.Log($"已替换包含 '{PattenQualityServiceDependencyClassPath}' 的行");
                            break; // 只替换第一个匹配的行
                        }
                    }
                    fileContent = string.Join("\n", lines);
                }

                if (!modified)
                {
                    Debug.LogWarning($"在 {FileAppLovinProcessGradleBuildFile} 文件中未找到需要替换的内容");
                    return false; // 找不到匹配内容则返回失败
                }

                // 3. 修改后将所有文本内容覆盖写入到 AppLovinProcessGradleBuildFile.cs 文件中
                File.WriteAllText(filePath, fileContent);

                // 刷新 AssetDatabase
                AssetDatabase.Refresh();

                Debug.Log($"成功修改 {FileAppLovinProcessGradleBuildFile} 文件，已修复 Quality Service 版本引用");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"修改 {FileAppLovinProcessGradleBuildFile} 文件时发生错误: {ex.Message}");
                return false;
            }
        }



        #endregion

    }





    public static class AppLovinModiferMenuItem
    {

        
        
        [MenuItem("Guru/PBC/Ads/Fix Max Assets...")]
        static void ModifyAppLovinAssets()
        {
            var modifier = new AppLovinAssetsModifer();

            bool fixSettingResult = modifier.ModifyAppLovinSettings();
            bool fixGradleBuildFileResult = modifier.ModifyAndroidProcessGradleBuildFile();
            
            bool result = fixSettingResult && fixGradleBuildFileResult;

            string title= result ? "修改成功" : "修改失败";
            
            string message = "";
            message += $"AppLovin Settings:\n{(fixSettingResult ? "修改成功" : "修改失败")}\n";
            message += $"AndroidProcessGradleBuildFile:\n{(fixGradleBuildFileResult ? "修改成功" : "修改失败")}\n";

            if (EditorUtility.DisplayDialog(title, message, "确定"))
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }


        }
        


    }


}