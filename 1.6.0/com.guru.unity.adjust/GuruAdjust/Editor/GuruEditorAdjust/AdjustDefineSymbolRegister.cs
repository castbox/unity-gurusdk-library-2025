using UnityEditor;
using UnityEngine;

namespace Guru.Editor.Adjust
{
    [InitializeOnLoad]
    public class AdjustDefineSymbolRegister
    {
        private const string DEFINE_SYMBOL = "GURU_ADJUST";
        private static readonly BuildTargetGroup[] TargetGroups = new []{BuildTargetGroup.Android, BuildTargetGroup.iOS};
        
        static AdjustDefineSymbolRegister()
        {
           
        }
        
        public static bool EnableAdjust
        {
            get
            {
                return EditorPrefs.GetBool($"{Application.identifier.Replace(".", "_")}_guru_adjust", true);
            }
            set
            {
                EditorPrefs.SetBool($"{Application.identifier.Replace(".", "_")}_guru_adjust", value);
            }
        }
        
        public static void AddGuruAdjust()
        {
            EnableAdjust = true;
            foreach (var targetGroup in TargetGroups)
            {
                InjectDefineSymbolsForTarget(targetGroup);
            }
        }
        
        public static void RemoveAdjust()
        {
            EnableAdjust = false;
            foreach (var targetGroup in TargetGroups)
            {
                InjectRemoveDefineSymbolsForTarget(targetGroup);
            }
        }
        
        // 向项目内注入宏定义
        private static void InjectRemoveDefineSymbolsForTarget(BuildTargetGroup targetGroup)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            if (defines.Contains(DEFINE_SYMBOL))
            {
                defines = defines.Replace($";{DEFINE_SYMBOL}", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
                UnityEngine.Debug.Log($"[GuruSDK] remove {DEFINE_SYMBOL} to define symbols");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        
        // 向项目内注入宏定义
        private static void InjectDefineSymbolsForTarget(BuildTargetGroup targetGroup)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            if (!defines.Contains(DEFINE_SYMBOL))
            {
                defines = string.IsNullOrEmpty(defines) ? DEFINE_SYMBOL : defines + ";" + DEFINE_SYMBOL;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
                UnityEngine.Debug.Log($"[GuruSDK] Added {DEFINE_SYMBOL} to define symbols");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}