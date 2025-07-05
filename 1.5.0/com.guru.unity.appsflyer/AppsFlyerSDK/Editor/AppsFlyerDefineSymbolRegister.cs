using UnityEditor;

namespace Guru.Editor.Adjust
{
    [InitializeOnLoad]
    public class AppsFlyerDefineSymbolRegister
    {
        private const string DEFINE_SYMBOL = "GURU_APPSFLYER";
        private static readonly BuildTargetGroup[] TargetGroups = new []{BuildTargetGroup.Android, BuildTargetGroup.iOS};
        
        static AppsFlyerDefineSymbolRegister()
        {
            foreach (var targetGroup in TargetGroups)
            {
                InjectDefineSymbolsForTarget(targetGroup);
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
            }
        }
    }
}