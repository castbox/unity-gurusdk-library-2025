using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Guru
{
    public class ScriptReplaceWindow : EditorWindow
    {
        private enum ReplaceType
        {
            Prefab,
            ScriptableObject,
        }
        
        [MenuItem("Guru/I2/ScriptReplaceWindow")]
        public static void Open()
        {
            var wnd = GetWindow<ScriptReplaceWindow>("ScriptReplaceWindow");
            wnd.Show();
            wnd.minSize = new Vector2(600, 400);
            wnd.maxSize = new Vector2(600, 400);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            DrawMonoScript();
            EditorGUILayout.Space(20);
            DrawScriptFile();
            EditorGUILayout.Space(20);
            DrawReplace();
            EditorGUILayout.Space(20);
            EditorGUILayout.EndVertical();
        }
 
        private MonoBehaviour _mono;
        private void DrawMonoScript()
        {
            _mono = EditorGUILayout.ObjectField("脚本组件(脚本在dll时使用)", _mono, typeof(MonoBehaviour), true) as MonoBehaviour;
            if (_mono)
            {
                string guid;
                long fid;
                MonoScript script = MonoScript.FromMonoBehaviour(_mono);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(script, out guid, out fid);
                EditorGUILayout.TextField("FileID", fid.ToString());
                EditorGUILayout.TextField("GUID", guid);
            }
        }
 
        private MonoScript _script;
        private void DrawScriptFile()
        {
            _script = EditorGUILayout.ObjectField("脚本文件", _script, typeof(MonoScript), true) as MonoScript;
            if (_script)
            {
                string guid;
                long fid;
                var path = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(_script, out guid, out fid);
                EditorGUILayout.TextField("FileID", fid.ToString());
                EditorGUILayout.TextField("GUID", guid);
            }
        }
        
        private string _oldFileID;
        private string _oldGUID;
        private string _newFileID;
        private string _newGUID;
        private Object _folderObj;
        private ReplaceType _replaceType;
        private void DrawReplace()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("替换");
            _oldFileID = EditorGUILayout.TextField("Old FileID", _oldFileID);
            _oldGUID = EditorGUILayout.TextField("Old GUID", _oldGUID);
            EditorGUILayout.Space(10);
            _newFileID = EditorGUILayout.TextField("New FileID", _newFileID);
            _newGUID = EditorGUILayout.TextField("New GUID", _newGUID);
            _folderObj = EditorGUILayout.ObjectField("替换文件夹" ,_folderObj, typeof(Object), false);
            _replaceType = (ReplaceType) EditorGUILayout.EnumPopup("替换类型", _replaceType);
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Replace"))
            {
                string path = AssetDatabase.GetAssetPath(_folderObj);
                Replace(path);
            }
            EditorGUILayout.EndVertical();
        }

        private void Replace(string path)
        {
            if(string.IsNullOrEmpty(_oldFileID) || string.IsNullOrEmpty(_oldGUID) || string.IsNullOrEmpty(_newFileID) || string.IsNullOrEmpty(_newGUID))
            {
                Debug.LogError("信息不完整");
                return;
            }

            try
            {
                List<string> files = new List<string>(10);
                string searchPattern = _replaceType == ReplaceType.Prefab ? "*.prefab" : "*.asset";
                I2EditorHelper.SearchFilePath(path, searchPattern, ref files);
                for (int j = 0; j < files.Count; j++)
                {
                    string file = files[j];

                    string fullPath = Path.GetFullPath(file);
                    string text = File.ReadAllText(fullPath);
                    bool isOld = text.Contains(_oldGUID);
                    if (isOld)
                    {
                        string[] lines = File.ReadAllLines(fullPath);
                        for (int i = 0; i < lines.Length; i++)
                        {
                            string line = lines[i];
                            bool isOldLine = line.Contains(_oldGUID);
                            if (isOldLine)
                            {
                                string newLine = line.Replace(_oldGUID, _newGUID);
                                string newLine2 = newLine.Replace(_oldFileID, _newFileID);
                                lines[i] = newLine2;
                            }
                        }

                        File.WriteAllLines(fullPath, lines);
                    }

                    EditorUtility.DisplayProgressBar("替换中", file, (float) (j + 1) / files.Count);
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
    }
}