#if UNITY_IOS
namespace Guru.Editor
{
    using UnityEditor;
    
    public class SKAdNetworkEditorMenu
    {
#if GURU_SDK_DEV
        [MenuItem("Guru/Ads/Download Latest SKAdNetwork", false, 1)]
#endif
        private static void DownloadLatestNetworks()
        {
            SKAdNetworkImporter.UpdateLatestSKAdNetworks();
        }
    }
}
#endif


