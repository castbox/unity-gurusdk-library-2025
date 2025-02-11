using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Guru
{
    public static class RemoveMissingScripts
    {
        internal static void RemoveAll(List<string> paths)
        {
            List<string> folders = I2EditorHelper.SearchFilePath(paths, "*.prefab");
            int count = folders.Count;
            int index = 0;

            List<GameObject> forList = new List<GameObject>();
            try
            {
                foreach (string path in folders)
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
                    HandleObj(gameObject);
                    PrefabUtility.SavePrefabAsset(gameObject);

                    index++;

                    EditorUtility.DisplayProgressBar("清除missing", $"移除中：{index}/{count}",
                        (float) index / count);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
            }
        }

        private static void HandleObj(GameObject gameObject)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            if (count > 0)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
            }

            foreach (Transform child in gameObject.transform)
            {
                HandleObj(child.gameObject);
            }
        }
    }
}