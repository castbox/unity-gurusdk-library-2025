#if UNITY_IOS
namespace Guru.Editor
{
    using UnityEditor;
    
    public class SKAdNetworkEditorMenu
    {

        [MenuItem("Guru/Ads/Download Latest SKAdNetwork", false, 1)]
        private static void DownloadLatestNetworks()
        {
            SKAdNetworkImporter.UpdateLatestSKAdNetworks();
        }
    }
}
#endif


