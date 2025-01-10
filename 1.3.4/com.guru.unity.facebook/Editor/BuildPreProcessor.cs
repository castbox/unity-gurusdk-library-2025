using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildPreProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    private const string LOG_TAG = "[GuruFB] ";

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("Linking FB plugins");

        const string packagesFolderPath = "com.guru.unity.facebook/FacebookSDK";
        const string assetsFolderPath = "Assets/FacebookSDK";
        const string linkFile = "link.xml";

        if (AssetDatabase.IsValidFolder(assetsFolderPath) &&
            File.Exists(Path.Combine(assetsFolderPath, linkFile)))
        {
            Debug.Log(LOG_TAG + "FB link file exists in Assets folder.");
            return;
        }

        try
        {
            string packageDirectory = Path.Combine("Packages", packagesFolderPath);

            // 在Library中的Packages文件夹下找
            if (AssetDatabase.IsValidFolder(packageDirectory))
            {
                AssetDatabase.CopyAsset(Path.Combine(packageDirectory, linkFile),
                    Path.Combine(assetsFolderPath, linkFile));

                Debug.Log(LOG_TAG + "Copied FB link file from library Packages to Assets folder.");
                return;
            }

            // 在Project中的Packages文件夹下找
            if (Directory.Exists(packageDirectory))
            {
                File.Copy(Path.Combine(packagesFolderPath, linkFile),
                    Path.Combine(assetsFolderPath, linkFile), true);

                Debug.Log(LOG_TAG + "Copied FB link file from project packages to Assets folder.");
                return;
            }
        }
        catch (Exception e)
        {
            StopBuildWithMessage("FB link file not copy correctly! " + e.Message);
        }

        StopBuildWithMessage("FB link file not found!");
    }

    private void StopBuildWithMessage(string message)
    {
        throw new BuildPlayerWindow.BuildMethodException(LOG_TAG + message);
    }
}