namespace Guru.Editor.Adjust
{
    using UnityEditor;
    using UnityEngine;
    
    public class GuruAdjustMenuItems
    {
        
        [MenuItem("Guru/Adjust/Enable Adjust")]
        public static void AddGuruAdjust()
        {
            AdjustDefineSymbolRegister.AddGuruAdjust();
        }
        
        [MenuItem("Guru/Adjust/Disable Adjust")]
        public static void RemoveAdjust()
        {
            AdjustDefineSymbolRegister.RemoveAdjust();
        }
        
        // TBD add Editor Menus
#if GURU_SDK_DEV
        
#endif
    }
}