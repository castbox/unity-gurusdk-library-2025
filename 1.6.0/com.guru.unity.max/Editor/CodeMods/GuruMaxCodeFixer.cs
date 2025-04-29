using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Guru.Editor.Max
{
    public static class GuruMaxCodeFixer
    {   
        
        public static void Compile() => CompilationPipeline.RequestScriptCompilation();
        

        /// <summary>
        /// Gets the path of the asset in the project for a given MAX plugin export path.
        /// </summary>
        /// <param name="exportPath">The actual exported path of the asset.</param>
        /// <returns>The exported path of the MAX plugin asset or the default export path if the asset is not found.</returns>
        public static string GetAssetPathFromPackageForExportPath(string exportPath)
        {
            var defaultPath = Path.Combine(GuruMaxSdkAPI.PackageDataPath, exportPath);
            var assetLabelToFind =
                "l:al_max_export_path-" +
                exportPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var assetGuids = AssetDatabase.FindAssets(assetLabelToFind);
            return assetGuids.Length < 1 ? defaultPath : AssetDatabase.GUIDToAssetPath(assetGuids[0]);
        }
        
        /// <summary>
        /// 更新所有的修改器
        /// </summary>
        public static void ApplyAllMods()
        {

            // --- 修改 AppLovinMax
            Debug.Log($"------ #1. Apply AppLovinMax");
            ApplovinMod.Apply();
            
            // --- 修复 Amazon 依赖
            Debug.Log($"------ #2. Apply Amazon");
            AmazonMod.Apply();
            
            // --- 修复 Pubmatic 依赖
            Debug.Log($"------ #3. Apply Pubmatic");
            PubmaticMod.Apply();
            
            Compile();
        }


#if GURU_SDK_DEV
        
        //---------- 编辑器快捷菜单 -------------------
        
        [MenuItem("Guru/Dev/Max/Apply All Mods")]
        private static void DevInjectCodeFix()
        {
            ApplyAllMods();
        }
        
        
        [MenuItem("Guru/Dev/Max/Show Max Menu")]
        private static void DevRecoverMenu()
        {
            var res = ApplovinMod.MenuItemsRecover();
            if(res) Compile();
        }

#endif

    }





}