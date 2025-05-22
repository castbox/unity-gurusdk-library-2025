using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Guru
{
    public static class I2EditorHelper
    {
        /// <summary>
        /// 搜索制定路径下的资源
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="searchPattern"></param>
        /// <returns>返回相对路径</returns>
        public static List<string> SearchFilePath(List<string> paths, string searchPattern)
        {
            if(paths == null || paths.Count == 0)
            {
                return null;
            }

            List<string> result = new List<string>();
            foreach (string path in paths)
            {
                SearchFilePath(path, searchPattern, ref result);
            }

            return result;
        }

        /// <summary>
        /// 搜索指定路径下的资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern">"*.prefab"</param>
        /// <returns>返回相对路径</returns>
        public static void SearchFilePath(string path, string searchPattern, ref List<string> result)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (result == null)
            {
                return;
            }

            // 判断是否是文件夹
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    foreach (var f in files)
                    {
                        string unityPath = f.Replace(Application.dataPath, "Assets");
                        result.Add(unityPath);
                    }
                }

                return;
            }

            if (File.Exists(path))
            {
                // 判断文件后缀
                string filePattern = searchPattern.Remove(0, 1);
                if (Path.GetExtension(path) == filePattern)
                {
                    string unityPath = path.Replace(Application.dataPath, "Assets");
                    result.Add(unityPath);
                }
            }
        }
    }
}