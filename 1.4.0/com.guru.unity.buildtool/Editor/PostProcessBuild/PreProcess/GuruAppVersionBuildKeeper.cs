
namespace Guru.BuildTool
{
    using UnityEngine;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using System.IO;

    
    /// <summary>
    /// 此类主要用于在构建的时候检查 GuruAppVersion 是否已经存在，若不存在，则强制生成对应的文件， 确保构建顺利进行
    /// </summary>
    public class GuruAppVersionBuildKeeper:IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        
        /// <summary>
        /// 流程入口
        /// </summary>
        /// <param name="report"></param>
        public void OnPreprocessBuild(BuildReport report)
        {
            // 创建
            var guruVersion = GuruAppVersion.CreateLocalGuruVersion();
            if (guruVersion == null)
            {
                throw new FileNotFoundException($"GuruVersion is not exists: {GuruAppVersion.DefaultFilePath}");
            }

            Debug.Log($"<color=#88ff00>Current build_info: {guruVersion}</color>");
        }
        
    }
}