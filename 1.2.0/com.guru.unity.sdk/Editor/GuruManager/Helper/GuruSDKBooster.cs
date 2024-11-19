

namespace Guru.Editor
{
    using UnityEditor;
    using System.IO;
    using UnityEngine;
    using System.Collections;
    using Guru.Editor;
    using Unity.EditorCoroutines.Editor;
    
    public class GuruSDKBooster
    {
        
        // [MenuItem("Test/API/Test CR")]
        static void TestCR()
        {
           // EditorHelper.StartCoroutine(OnTestRun());
           EditorCoroutineUtility.StartCoroutineOwnerless(OnTestRun());
        }
        
        static IEnumerator OnTestRun()
        {
            int i = 0;
            while (i < 5)
            {
                Debug.Log($"--- ticket: {i}");
                i++;
                yield return new EditorWaitForSeconds(1);
            }
            Debug.Log($"------- runner end -------");
        }
    }
    
    [InitializeOnLoad]
    internal class BoostOnLoad
    {
        static BoostOnLoad()
        {
            EditorGuruServiceIO.DeployLocalServiceFile();
        }
    }
}