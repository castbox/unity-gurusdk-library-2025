using Guru.Editor;
using UnityEditor;

namespace Guru
{
    public class GuruSDKMenuItems
    {

        [MenuItem("Guru/Guru SDK")]
        private static void ShowGuruManager()
        {
            GuruSDKManager.Open();
        }

    }
}