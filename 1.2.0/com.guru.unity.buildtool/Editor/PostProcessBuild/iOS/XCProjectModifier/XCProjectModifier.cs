#if UNITY_IOS

namespace Guru.Editor
{
	using System.Collections.Generic;
	using System.IO;
	using UnityEditor;
	using UnityEditor.Callbacks;
	using UnityEditor.iOS.Xcode;
	using UnityEditor.iOS.Xcode.Extensions;
	using UnityEngine;
	
	public static class XCProjectModifier
	{
		private const string UNITY_MAIN_TARGET_NAME = "Unity-iPhone";
		private static readonly char DIR_CHAR = Path.DirectorySeparatorChar;
		
		private enum EntitlementOptions {
			AppGroups,
		}
		
		private static readonly string[] FRAMEWORKS_MAIN_TO_ADD = {
		};
		
		private static readonly string[] FRAMEWORKS_UNITY_FRAMEWORK_TO_ADD = {
			"GameKit.framework",
		};

		[PostProcessBuild(1)]
		private static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
		{
			if (buildTarget != BuildTarget.iOS)
			{
				return;
			}
			var mainTargetName = UNITY_MAIN_TARGET_NAME;
			
			var pbxProjectPath = PBXProject.GetPBXProjectPath(buildPath);
			var pbxProject = new PBXProject();
			pbxProject.ReadFromFile(pbxProjectPath);
			
			var mainTargetGuid = pbxProject.GetUnityMainTargetGuid();
			var frameworkTargetGuid = pbxProject.GetUnityFrameworkTargetGuid();
			
			// 配置 Info.plist 文件内容
			ModifyPlistFile(buildPath);
			
			var entitlementFilePath = AddOrUpdateEntitlements(buildPath, 
				pbxProject, 
				mainTargetGuid, 
				mainTargetName,
				new HashSet<EntitlementOptions>
				{
					EntitlementOptions.AppGroups
				});
			
			// Add push notifications as a capability on the main app target
			AddCapabilities(pbxProjectPath, entitlementFilePath, mainTargetName);
			
			//------------------- Project Modifier ----------------------

			// 关闭Bitode
			pbxProject.SetBuildProperty(mainTargetGuid, "ENABLE_BITCODE", "NO");
			pbxProject.SetBuildProperty(frameworkTargetGuid, "ENABLE_BITCODE", "NO");

			// 添加 UnityFramework 版本号
			pbxProject.SetBuildProperty(frameworkTargetGuid, "CURRENT_PROJECT_VERSION", PlayerSettings.bundleVersion);
			pbxProject.SetBuildProperty(frameworkTargetGuid, "MARKETING_VERSION", PlayerSettings.iOS.buildNumber);
			
			// 添加GCC 编译选项
			pbxProject.SetBuildProperty(frameworkTargetGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
			pbxProject.SetBuildProperty(mainTargetGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
			
			// 添加搜索路径
			pbxProject.AddBuildProperty(frameworkTargetGuid, "LD_RUNPATH_SEARCH_PATHS", "/usr/lib/swift");
            
			// 设置主项目的SWIFT构建支持
			pbxProject.SetBuildProperty(mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
			pbxProject.SetBuildProperty(frameworkTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
			
			// Other Frameworks
			foreach(var framework in FRAMEWORKS_MAIN_TO_ADD) {
				pbxProject.AddFrameworkToProject(mainTargetGuid, framework, false);
			}
        
			// Deps Frameworks
			foreach(var framework in FRAMEWORKS_UNITY_FRAMEWORK_TO_ADD) {
				pbxProject.AddFrameworkToProject(frameworkTargetGuid, framework, false);
			}
			//------------------- Project Modifier ----------------------

			// Save Project
			pbxProject.WriteToFile(pbxProjectPath);
		}
		
		/// <summary>
		/// Info.plist 配置
		/// </summary>
		/// <param name="pathToBuildProject"></param>
		private static void ModifyPlistFile(string pathToBuildProject)
		{
			var plistPath = Path.Combine(pathToBuildProject, "Info.plist");
			var plist = new PlistDocument();
			plist.ReadFromFile(plistPath);
			//设置Google AD GADApplicationIdentifier
			plist.root.SetString("NSCalendarsUsageDescription", "Store calendar events from ads");
			//设置Xcode的Att弹窗配置
			plist.root.SetString("NSUserTrackingUsageDescription","By allowing tracking, we'll be able to better tailor ads served to you on this game.");
			// 设置合规出口证明
			plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

			var root = plist.root.values;
			PlistElement atsRoot;
			root.TryGetValue("NSAppTransportSecurity", out atsRoot);

			if (atsRoot == null || atsRoot.GetType() != typeof(PlistElementDict))
			{
				atsRoot = plist.root.CreateDict("NSAppTransportSecurity");
				atsRoot.AsDict().SetBoolean("NSAllowsArbitraryLoads", true);
			}

			var atsRootDict = atsRoot.AsDict().values;
			if (atsRootDict.ContainsKey("NSAllowsArbitraryLoadsInWebContent"))
			{
				atsRootDict.Remove("NSAllowsArbitraryLoadsInWebContent");
			}
        
			plist.WriteToFile(plistPath);
		}

		#region Capabilities
		
		/// <summary>
		/// 添加对应的权限
		/// </summary>
		/// <param name="pbxProjectPath"></param>
		/// <param name="entitlementFilePath"></param>
		/// <param name="mainTargetName"></param>
		private static void AddCapabilities(string pbxProjectPath, string entitlementFilePath, string mainTargetName)
		{
			// NOTE: ProjectCapabilityManager's 4th constructor param requires Unity 2019.3+
			var projCapability = new ProjectCapabilityManager(pbxProjectPath, entitlementFilePath, mainTargetName);
			projCapability.AddInAppPurchase();
			projCapability.AddPushNotifications(false);
			projCapability.WriteToFile();
		}
		
		#endregion
		
		#region Entitlements
		
		private static string AddOrUpdateEntitlements(string buildPath, PBXProject pbxProject, string mainTargetGuid,
			string mainTargetName, HashSet<EntitlementOptions> options)
		{
			string entitlementPath = GetEntitlementsPath(buildPath, pbxProject, mainTargetGuid, mainTargetName);
			var entitlements = new PlistDocument();

			// Check if the file already exisits and read it
			if (File.Exists(entitlementPath)) {
				entitlements.ReadFromFile(entitlementPath);
			}

			// TOOD: This can be updated to use project.AddCapability() in the future
			if (options.Contains(EntitlementOptions.AppGroups) && entitlements.root["com.apple.security.application-groups"] == null) {
				var groups = entitlements.root.CreateArray("com.apple.security.application-groups");
				groups.AddString("group." + PlayerSettings.applicationIdentifier);
			}

			entitlements.WriteToFile(entitlementPath);

			// Copy the entitlement file to the xcode project
			var entitlementFileName = Path.GetFileName(entitlementPath);
			var relativeDestination = mainTargetName + "/" + entitlementFileName;

			// Add the pbx configs to include the entitlements files on the project
			pbxProject.AddFile(relativeDestination, entitlementFileName);
			pbxProject.AddBuildProperty(mainTargetGuid, "CODE_SIGN_ENTITLEMENTS", relativeDestination);

			return entitlementPath;
		}
		
		private static string GetEntitlementsPath(string buildPath, PBXProject pbxProject, string mainTargetGuid, string mainTargetName)
		{
			// Check if there is already an eltitlements file configured in the Xcode project
			var relativeEntitlementPath = pbxProject.GetBuildPropertyForConfig(mainTargetGuid, "CODE_SIGN_ENTITLEMENTS");
			if (relativeEntitlementPath != null) {
				var entitlementPath = buildPath + DIR_CHAR + relativeEntitlementPath;
				if (File.Exists(entitlementPath)) {
					return entitlementPath;
				}
			}
			// No existing file, use a new name
			return buildPath + DIR_CHAR + mainTargetName + DIR_CHAR + mainTargetName + ".entitlements";
		}
		#endregion
		

		
	}
}

#endif