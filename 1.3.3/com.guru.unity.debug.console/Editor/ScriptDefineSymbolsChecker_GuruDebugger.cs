namespace Guru.Editor
{
#if UNITY_EDITOR

    using UnityEditor;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor.Build;
    using UnityEngine;
    
    [InitializeOnLoad]
    public static class ScriptDefineSymbolsChecker_GuruDebugger
    {

        private static readonly string[] NecessaryScriptDefineSymbols = new string[]
        {
            "GURU_DEBUG_CONSOLE",
            "GURU_DEBUG"
        };
        
        static ScriptDefineSymbolsChecker_GuruDebugger()
        {
            var res1 = InjectScriptDefineSymbolsForTargeting(NamedBuildTarget.Android, NecessaryScriptDefineSymbols);
            var res2 = InjectScriptDefineSymbolsForTargeting(NamedBuildTarget.iOS, NecessaryScriptDefineSymbols);

            if (!res1 && !res2) return;
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Guru] --- <color=#88ff00>Setup DefineSymbols for DebugConsole success!</color>");
        }
        
        private static bool InjectScriptDefineSymbolsForTargeting(NamedBuildTarget target, string[] newSymbols)
        {
            PlayerSettings.GetScriptingDefineSymbols(target, out var defines);
            
            List<string> addSymbols = new List<string>();
            if (defines.Length == 0)
            {
                addSymbols.AddRange(newSymbols);
            }
            else
            {
                foreach (var s in newSymbols)
                {
                    if (!defines.Contains(s))
                    {
                        addSymbols.Add(s);
                    }
                }
            }

            // 无变化
            if (addSymbols.Count == 0)
                return false;

            List<string> merged = new List<string>();
            merged.AddRange(defines);
            merged.AddRange(addSymbols);
            
            PlayerSettings.SetScriptingDefineSymbols(target, merged.ToArray());
            return true;
        }

    }
        
#endif
}