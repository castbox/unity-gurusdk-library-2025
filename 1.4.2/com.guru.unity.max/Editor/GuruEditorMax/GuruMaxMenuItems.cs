namespace Guru.Editor.Max
{
    using UnityEditor;
    using UnityEngine;
    
    public class GuruMaxMenuItems
    {
    #if GURU_MAX_MENU
        [MenuItem("Guru/AppLovin/Integration Manager")]
    #endif
        private static void ShowGuruMaxIntegrationManager()
        {
            GuruMaxIntegrationManager.Open();
        }
    }
}