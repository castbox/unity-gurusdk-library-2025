#if UNITY_ANDROID

using System;
using System.Collections.Generic;
using UnityEditor.Android;
using UnityEngine;
using Debug=UnityEngine.Debug;

namespace Guru.Editor
{
    public class AndroidGradleOutputDeps: IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 1;
        
        /// <summary>
        /// 生成Android项目后执行逻辑
        /// </summary>
        /// <param name="buildPath"></param>
        public void OnPostGenerateGradleAndroidProject(string buildPath)
        {
#if UNITY_EDITOR_WIN
            // Windows 下暂不支持上报和调用
            return;
#endif
            Debug.Log($"<color=#88ff00>---- Android Projct start build {buildPath}</color>");
            VersionTrackerHelper.InstallAndRun(buildPath);
        }
    }
}


#endif