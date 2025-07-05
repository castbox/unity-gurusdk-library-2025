using UnityEditor;

namespace Guru.Editor.Adjust
{
    public class GuruAdjustCodeFixer
    {
        /// <summary>
        /// 应用所有的补丁
        /// </summary>
        public static void ApplyAllMods()
        {
            AdjustMod.Apply();
        }
        
        
#if GURU_SDK_DEV
        
        //---------- 编辑器快捷菜单 -------------------
        
        [MenuItem("Guru/Dev/Adjust/Apply All Mods")]
        private static void DevInjectCodeFix()
        {
            ApplyAllMods();
        }
        

#endif
        
        

    }
    
}