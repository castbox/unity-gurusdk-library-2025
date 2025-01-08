using System;
using System.Collections.Generic;
using System.IO;
#if TextMeshPro
using TMPro;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Guru
{
    public static class AddOrRemoveFontHelper
    {
        internal static void AddAllFontFixer(List<string> paths)
        {
            List<string> folders = I2EditorHelper.SearchFilePath(paths, "*.prefab");
            AddFontFixer(folders);
        }

        private static void AddFontFixer(List<string> paths)
        {
            try
            {
                int count = paths.Count;
                int index = 0;
                List<GameObject> forList = new List<GameObject>();
                foreach (string path in paths)
                {
                    GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    if (obj == null)
                    {
                        continue;
                    }

                    forList.Add(obj);
                }

                foreach (GameObject gameObject in forList)
                {
                    HandleAdd(gameObject, gameObject);
                    PrefabUtility.SavePrefabAsset(gameObject);

                    index++;

                    EditorUtility.DisplayProgressBar("Add TextFontHelper", $"添加中：{index}/{count}",
                        (float) index / count);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
            }
        }
        
        private static void HandleAdd(GameObject parentObj, GameObject gameObject)
        {
            Text textComp = gameObject.GetComponent<Text>();
            if (textComp != null)
            {
                if (TextFontHelper.Setup(textComp) != null)
                {
                    Debug.Log($"--- add: <color=#88ff00>{parentObj.name}-->{gameObject.name}</color>");
                }
            }
            
#if TextMeshPro
            TMP_Text tmpComp = gameObject.GetComponent<TMP_Text>();
            if (tmpComp != null)
            {
                if (TextFontHelper.Setup(tmpComp) != null)
                {
                    Debug.Log($"--- add: <color=#88ff00>{parentObj.name}-->{gameObject.name}</color>");
                }
            }
#endif

            foreach (Transform child in gameObject.transform)
            {
                HandleAdd(parentObj, child.gameObject);
            }
        }

        internal static void RemoveAllFontFixer(List<string> paths)
        {
            List<string> folders = I2EditorHelper.SearchFilePath(paths, "*.prefab");
            RemoveFontFixer(folders);
        }

        private static void RemoveFontFixer(List<string> paths)
        {
            try
            {
                int count = paths.Count;
                int index = 0;
                List<GameObject> forList = new List<GameObject>();
                foreach (string path in paths)
                {
                    GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    if (obj == null)
                    {
                        continue;
                    }

                    forList.Add(obj);
                }

                foreach (GameObject gameObject in forList)
                {
                    HandleDel(gameObject, gameObject);
                    PrefabUtility.SavePrefabAsset(gameObject);

                    index++;

                    EditorUtility.DisplayProgressBar("Remove TextFontHelper", $"移除中：{index}/{count}",
                        (float) index / count);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
            }
        }
        
        private static void HandleDel(GameObject parentObj, GameObject gameObject)
        {
            TextFontHelper textFontHelper = gameObject.GetComponent<TextFontHelper>();
            if (textFontHelper != null)
            {
                Object.DestroyImmediate(textFontHelper, true);
                Debug.Log($"--- remove: <color=red>{parentObj.name}-->{gameObject.name}</color>");
            }

            foreach (Transform child in gameObject.transform)
            {
                HandleDel(parentObj, child.gameObject);
            }
        }
    }
}