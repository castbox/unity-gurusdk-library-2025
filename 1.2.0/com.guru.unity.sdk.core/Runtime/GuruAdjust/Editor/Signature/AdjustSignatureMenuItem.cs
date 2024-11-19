namespace Guru
{
    using UnityEditor;
    
    public class AdjustSignatureMenuItem
    {
        [MenuItem("Guru/Adjust/SignatureV3/Deploy Libs")]
        private static void CopyLibsToPlugins()
        {
            AdjustSignatureHelper.DeployFiles();
        }

    }
}