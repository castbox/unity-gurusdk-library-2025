namespace Guru.Editor
{
	using System.Linq;
	using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.IO;
	using NUnit.Framework;
	
	/// <summary>
	/// 构建工具
	/// </summary>
    public partial class AppBuilder
    {
	    private const int DefaultAndroidTargetSdkVersion = 34;
	    private const string IOSTargetOSVersion = "13.0";
	    private const string GuruIOSTeamId = "39253T242A";
	    private const string GuruKeystoreName = "guru_key.jks";
	    private const string GuruKeystorePass = "guru0622";
	    private const string GuruAliasName = "guru";
	    private const string GuruAliasPass = "guru0622";

	    private const string DEFAULT_GRADLE_PATH_MAC =
		    "/Applications/Unity/Hub/Editor/{0}/PlaybackEngines/AndroidPlayer/Tools/gradle";
	    private const string DEFAULT_JDK_PATH_MAC =
		    "/Applications/Unity/Hub/Editor/{0}/PlaybackEngines/AndroidPlayer/OpenJDK";
	    private const string DEFAULT_NDK_PATH_MAC =
		    "/Applications/Unity/Hub/Editor/{0}/PlaybackEngines/AndroidPlayer/NDK";
	    private const string DEFAULT_ANDROID_SDK_MAC =
		    "/Applications/Unity/Hub/Editor/{0}/PlaybackEngines/AndroidPlayer/SDK";
	    
	    private static string GuruKeystorePath => Application.dataPath + $"/Plugins/Android/{GuruKeystoreName}";
	    private static string ProguardName => "proguard-user.txt";
	    private static string ProguardPath => Application.dataPath + $"/Plugins/Android/{ProguardName}";
	    private static string OutputDirName => "BuildOutput";

	    #region 构建接口

	    /// <summary>
	    /// 直接调用 Build 接口
	    /// </summary>
	    /// <param name="buildParam"></param>
	    /// <returns></returns>
	    public static string Build(AppBuildParam buildParam)
	    {
		    string outputPath = string.Empty;
		    switch (buildParam.TargetName)
		    {
			    case AppBuildParam.TargetNameAndroid:
				    SwitchBuildPlatform(BuildTarget.Android);
				    outputPath = BuildAndroid(buildParam);
				    break;
			    case AppBuildParam.TargetNameIOS:
				    SwitchBuildPlatform(BuildTarget.iOS);
				    outputPath = BuildIOS(buildParam);
				    break;
			    default:
					Debug.Log($"<color=red> Unsupported build target: {buildParam.TargetName}. Skip build...</color>");
				    break;
		    }

		    return outputPath;
	    }


	    #endregion
	    
        #region 构建 Android 接口

        /// <summary>
        /// 构建 Android 包体
        /// </summary>
        /// <param name="buildParam"></param>
        /// <returns></returns>
        public static string BuildAndroid(AppBuildParam buildParam)
	    {
		    // 切换平台
		    SwitchBuildPlatform(BuildTarget.Android);
		    // 打包通用设置
		    ChangeBuildPlayerCommonSetting(buildParam, BuildTargetGroup.Android);
		    // 设置打包环境
		    SetGradlePath(buildParam.CustomGradlePath);
		    SetJDKRoot(buildParam.CustomJDKRoot);
		    SetNDKRoot(buildParam.CustomNDKRoot);
		    SetAndroidSDKRoot(buildParam.CustomAndroidSDKRoot);
		    
		    var isDebug = !buildParam.IsBuildRelease;
		    var useMinify = buildParam.AndroidUseMinify;
	        var buildNumber= GetPlayerSettingsBuildNumberStr(BuildTarget.Android);
	        var androidTargetVersion = buildParam.AndroidTargetVersion == 0 ? DefaultAndroidTargetSdkVersion : buildParam.AndroidTargetVersion;
	        if (buildParam.AutoSetBuildNumber)
	        {
		        buildNumber = CreateGuruBuildNumber();
		        PlayerSettings.Android.bundleVersionCode = int.Parse(buildNumber);
	        }
			// 保存版本信息
			SaveBuildVersion(buildParam.BuildVersion, buildNumber);
			
	        //android专用打包设置
	        EditorUserBuildSettings.buildAppBundle = buildParam.IsBuildAAB;
	        EditorUserBuildSettings.development = isDebug;
#if UNITY_2020_3
	        EditorUserBuildSettings.androidCreateSymbolsZip = buildParam.IsBuildSymbols;
#elif UNITY_2021_3
	        EditorUserBuildSettings.androidCreateSymbols = buildParam.IsBuildSymbols? AndroidCreateSymbols.Public : AndroidCreateSymbols.Disabled; //Android 输出SymbolsZip的选项
#endif
	        PlayerSettings.muteOtherAudioSources = false;
			// ---- 开启 Minify 后需要配置 proguard-user.txt 文件 ---- 
			if (useMinify) DeployProguardTxt();
			PlayerSettings.Android.minifyRelease = useMinify;
			PlayerSettings.Android.minifyDebug = useMinify;
			// ---- 部署 Guru 专用的 Keystore ----
		    if (buildParam.UseGuruCerts && DeployAndroidKeystore())
		    {
			    // ---- 使用 Guru 专用的 KeyStore ----
			    PlayerSettings.Android.useCustomKeystore = true;
			    PlayerSettings.Android.keystoreName = GuruKeystorePath;
			    PlayerSettings.Android.keystorePass = GuruKeystorePass;
			    PlayerSettings.Android.keyaliasName = GuruAliasName;
			    PlayerSettings.Android.keyaliasPass = GuruAliasPass;
		    }
		    else if(!string.IsNullOrEmpty(buildParam.AndroidKeystorePath))
		    {
			    // ---- 使用 Custom 的 KeyStore ----
			    PlayerSettings.Android.useCustomKeystore = true;
			    PlayerSettings.Android.keystoreName = buildParam.AndroidKeystorePath;
			    PlayerSettings.Android.keystorePass = buildParam.AndroidKeystorePass;
			    PlayerSettings.Android.keyaliasName = buildParam.AndroidAlias;
			    PlayerSettings.Android.keyaliasPass = buildParam.AndroidAliasPass;
		    }

		    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64; // 构建 armV7, arm64
	        // PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
	        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)androidTargetVersion;  // 设置 API Version
	        
	        //打包
	        string symbolDefine = buildParam.IsBuildRelease ? GameDefine.MACRO_RELEASE : GameDefine.MACRO_DEBUG;
	        string version = Application.version;
	        string extension = buildParam.IsBuildAAB ? ".aab" : ".apk";
	        if (EditorUserBuildSettings.exportAsGoogleAndroidProject) extension = ""; // 输出工程
		    string outputDir = Path.GetFullPath($"{Application.dataPath }/../{OutputDirName}/Android");
	        var apkPath = $"{outputDir}/{Application.productName.Replace(" ","_")}_{symbolDefine}_{version}_{buildNumber}{extension}";
	        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

	        // BuildOptions opts = isDebug ? BuildOptions.Development : BuildOptions.None;
	        // if (string.IsNullOrEmpty(buildParam.AssetBundleManifestPath))
	        // {
		       //  BuildPipeline.BuildPlayer(
			      //   GetBuildScenes(), 
			      //   apkPath, 
			      //   BuildTarget.Android, 
			      //   opts);
	        // }
	        // else
	        // {
	        
	        var buildPlayerOptions = new BuildPlayerOptions()
	        {
		        scenes = GetBuildScenes(),
		        locationPathName = apkPath,
		        assetBundleManifestPath = buildParam.AssetBundleManifestPath,
		        target = BuildTarget.Android,
		        targetGroup = BuildTargetGroup.Android,
		        extraScriptingDefines =  buildParam.ExtraScriptingDefines,
		        options = isDebug ? BuildOptions.Development : BuildOptions.None,
	        };
	        BuildPipeline.BuildPlayer(buildPlayerOptions);

	        // }

	        if (buildParam.BuilderType == AppBuilderType.Editor)
	        {
		        Open(outputDir);
	        }
	        
	        if (buildParam.AutoPublish)
	        {
		        GuruPublishHelper.Publish(apkPath, buildParam.PgyerAPIKey); // 直接发布版本
	        }
	        return apkPath;
	    }


	    private static void UsePlayerEmbeddedGradlePath()
	    {
		    UnityEditor.Android.AndroidExternalToolsSettings.gradlePath = null;
	    }
	    private static void UsePlayerEmbeddedJDKPath()
	    {
		    UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath = null;
	    }
	    private static void UsePlayerEmbeddedNDKPath()
	    {
		    UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath = null;
	    }
	    private static void UsePlayerEmbeddedAndroidSDKPath()
	    {
		    UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath = null;
	    }


	    /// <summary>
        /// 设置 GradlePath
        /// </summary>
        /// <param name="gradlePath"></param>
	    private static void SetGradlePath(string gradlePath = "")
	    {
		    if (!string.IsNullOrEmpty(gradlePath))
		    {
			    UnityEditor.Android.AndroidExternalToolsSettings.gradlePath = gradlePath;
			    return;
		    }
		    
#if UNITY_EDITOR_OSX
		    // 针对 2021.3.41 MAC 版本，直接强制走Unity 自带的 Gradle 库
		    UsePlayerEmbeddedGradlePath();
#endif
	    }
		
        /// <summary>
        /// 设置自定义的JDK 路径
        /// </summary>
        /// <param name="jdkRoot"></param>
	    private static void SetJDKRoot(string jdkRoot)
	    {
		    if (!string.IsNullOrEmpty(jdkRoot))
		    {
			    UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath = jdkRoot;
			    return;
		    }
		    
#if UNITY_EDITOR_OSX
		    // 针对 2021.3.41 MAC 版本，直接强制走Unity 自带的 JDK 库
		    UsePlayerEmbeddedJDKPath();
#endif
	    }
        
	    /// <summary>
	    /// 设置自定义的 NDK 路径
	    /// </summary>
	    /// <param name="ndkRoot"></param>
	    private static void SetNDKRoot(string ndkRoot)
	    {
		    if (!string.IsNullOrEmpty(ndkRoot))
		    {
			    UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath = ndkRoot;
			    return;
		    }
		    
#if UNITY_EDITOR_OSX
		    // 针对 2021.3.41 MAC 版本，直接强制走Unity 自带的 NDK 库
		    UsePlayerEmbeddedNDKPath();
#endif
	    }
	    
	    
	    /// <summary>
	    /// 设置自定义的 NDK 路径
	    /// </summary>
	    /// <param name="sdkRoot"></param>
	    private static void SetAndroidSDKRoot(string sdkRoot)
	    {
		    if (!string.IsNullOrEmpty(sdkRoot))
		    {
			    UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath = sdkRoot;
			    return;
		    }
		    
#if UNITY_EDITOR_OSX
		    // 针对 2021.3.41 MAC 版本，直接强制走Unity 自带的 SDK 库
		    UsePlayerEmbeddedAndroidSDKPath();
#endif
	    }


	    /// <summary>
		/// 部署 Guru 专用的 Keystore
		/// </summary> 
        private static bool DeployAndroidKeystore()
        {
	        var dir = GetWorkingDir();
	        var from = $"{dir}/{GuruKeystoreName}";
	        var to = GuruKeystorePath;

	        if (File.Exists(to)) return true;
	        
	        if (File.Exists(from))
	        {
		        File.Copy(from, to);
		        return true;
	        }
	        
	        return false;
        }

		/// <summary>
		/// 部署混淆用配置
		/// </summary> 
        private static bool DeployProguardTxt()
        {
	        var dir = GetWorkingDir();
	        var from = $"{dir}/{ProguardName}";
	        var to = ProguardPath;

	        if (File.Exists(to)) return true;
	        
	        if (File.Exists(from))
	        {
		        File.Copy(from, to);
		        return true;
	        }
	        
	        return false;
        }


        #endregion
        
        #region 构建 IOS 接口
        
        public static string BuildIOS(AppBuildParam buildParam)
	    {
	        //切换平台
	        SwitchBuildPlatform(BuildTarget.iOS);
	        //打包通用设置
	        ChangeBuildPlayerCommonSetting(buildParam, BuildTargetGroup.iOS);
	        
	        //修改打包版本号
	        var buildNumber= GetPlayerSettingsBuildNumberStr(BuildTarget.Android);
	        if (buildParam.AutoSetBuildNumber)
	        {
		        buildNumber = CreateGuruBuildNumber();
		        PlayerSettings.iOS.buildNumber = buildNumber;
	        }
	        
	        // 保存版本信息
	        SaveBuildVersion(buildParam.BuildVersion, buildNumber);
	        
	        var isDebug = !buildParam.IsBuildRelease;

	        //ios专用打包设置
	        PlayerSettings.muteOtherAudioSources = false;
	        PlayerSettings.iOS.appInBackgroundBehavior = iOSAppInBackgroundBehavior.Custom;
	        PlayerSettings.iOS.backgroundModes = iOSBackgroundMode.RemoteNotification | iOSBackgroundMode.Fetch; // 后台启动配置
	        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
	        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;

	        var targetVersion = IOSTargetOSVersion;
	        if (!string.IsNullOrEmpty(buildParam.IOSTargetVersion)) targetVersion = buildParam.IOSTargetVersion;	        
	        PlayerSettings.iOS.targetOSVersionString = targetVersion;

	        var teamId = buildParam.IOSTeamId;
	        if (buildParam.UseGuruCerts) teamId = GuruIOSTeamId;
	
	        if (!string.IsNullOrEmpty(teamId))
	        {
		        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
		        PlayerSettings.iOS.appleDeveloperTeamID = teamId;
	        }
	        
		    //打包
	        string outputDir = Path.GetFullPath($"{Application.dataPath }/../{OutputDirName}/Xcode");
	        if (Directory.Exists(outputDir))
	        {
	            Directory.Delete(outputDir, true);
	        }

	        // 构建后打开路径
	        try
	        {
		        // BuildOptions opts = isDebug ? BuildOptions.Development : BuildOptions.None;
		        // BuildPipeline.BuildPlayer(GetBuildScenes(), outputDir, BuildTarget.iOS, opts);
		        
		        var buildPlayerOptions = new BuildPlayerOptions()
		        {
			        scenes = GetBuildScenes(),
			        locationPathName = outputDir,
			        assetBundleManifestPath = buildParam.AssetBundleManifestPath,
			        target = BuildTarget.iOS,
			        targetGroup = BuildTargetGroup.iOS,
			        extraScriptingDefines =  buildParam.ExtraScriptingDefines,
			        options = isDebug ? BuildOptions.Development : BuildOptions.None,
		        };
		        BuildPipeline.BuildPlayer(buildPlayerOptions);
		        
		        if (buildParam.BuilderType == AppBuilderType.Editor)
		        {
			        Open(outputDir);
		        }
	        }
	        catch (Exception e)
	        {
		        Debug.LogError(e.Message);
	        }

	        return outputDir;
	    }       

        #endregion

        #region 通用接口

		/// <summary>
		/// 获取工作目录
		/// </summary>
		/// <returns></returns>
        private static string GetWorkingDir()
        {
	        var guids = AssetDatabase.FindAssets($"{nameof(AppBuilder)} t:Script");
	        if (guids.Length > 0)
	        {
		        foreach (var guid in guids)
		        {
			        var path = AssetDatabase.GUIDToAssetPath(guid);
			        if (path.Contains($"Editor/BuildTool/{nameof(AppBuilder)}"))
			        {
				        return Directory.GetParent(path)!.FullName;
			        }
		        }
	        }
	        return Path.GetFullPath("Packages/com.guru.unity.sdk.core/Editor/BuildTool/");
        }

        /// <summary>
	    /// 平台切换
	    /// </summary>
	    /// <param name="targetPlatform"></param>
	    private static void SwitchBuildPlatform(BuildTarget targetPlatform)
	    {
		    if (EditorUserBuildSettings.activeBuildTarget != targetPlatform)
		    {
			    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(targetPlatform), targetPlatform);
			    AssetDatabase.Refresh();
		    }
	    }
	    
	    private static void ChangeBuildPlayerCommonSetting(AppBuildParam buildParam, BuildTargetGroup buildTargetGroup)
	    {
		    EditorUserBuildSettings.development = !buildParam.IsBuildRelease;
		    EditorUserBuildSettings.allowDebugging = false;
		    EditorUserBuildSettings.connectProfiler = false;
		    EditorUserBuildSettings.buildScriptsOnly = false;

		    var backend = ScriptingImplementation.IL2CPP;
		    if (buildTargetGroup == BuildTargetGroup.Android
		        && !buildParam.IsBuildRelease && buildParam.DebugWithMono)
		    {
			    backend = ScriptingImplementation.Mono2x;
		    }
		    PlayerSettings.SetScriptingBackend(buildTargetGroup, backend);
			
		    var companyName = buildParam.CompanyName;
		    if(string.IsNullOrEmpty(companyName)) companyName = GameDefine.CompanyName;
		    PlayerSettings.companyName = companyName;
		    
		    var bundleVersion = buildParam.BuildVersion;
		    if(!string.IsNullOrEmpty(bundleVersion)) PlayerSettings.bundleVersion = bundleVersion;
		    
		    // -------- Defines --------
		    List<string> defines = new List<string>();
		    var str = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
		    if (!string.IsNullOrEmpty(str))
		    {
			    defines = str.Split(';').ToList();
		    }

		    if (defines.Count > 0)
		    {
			    defines.Remove(GameDefine.MACRO_RELEASE);
			    defines.Remove(GameDefine.MACRO_DEBUG);
		    }
		    
		    defines.Add(buildParam.IsBuildRelease ? GameDefine.MACRO_RELEASE : GameDefine.MACRO_DEBUG);
		    if (!buildParam.IsBuildRelease || buildParam.IsBuildShowLog)
		    {
			    defines.Add(GameDefine.MACRO_LOG);
		    }
		    
		    // defines.Add("mopub_manager");
		    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines.ToArray());
		    PlayerSettings.stripEngineCode = true;
		    PlayerSettings.SetManagedStrippingLevel(buildTargetGroup, ManagedStrippingLevel.Low);
		    PlayerSettings.SetApiCompatibilityLevel(buildTargetGroup, ApiCompatibilityLevel.NET_4_6);
	    }
	    
	    /// <summary>
	    /// 修改打包版本号
	    /// </summary>
	    /// <param name="buildTarget"></param>
	    private static string CreateGuruBuildNumber()
	    {
		    var nowDate = DateTime.Now;
		    string strYear = nowDate.Year.ToString().Substring(2);
		    string strMon = nowDate.Month.ToString("00");
		    string strDay = nowDate.Day.ToString("00");
		    string strQuarter = ((nowDate.Hour * 60 + nowDate.Minute) / 15).ToString("00");
		    // 2024-08-01 08:00:00  to version string: 24080130
		    string strBuildNumber = $"{strYear}{strMon}{strDay}{strQuarter}";
		    return strBuildNumber;
	    }
		
	    /// <summary>
	    /// 获取构建数变量
	    /// </summary>
	    /// <returns></returns>
	    private static string GetPlayerSettingsBuildNumberStr(BuildTarget buildTarget)
	    {
		    if (buildTarget == BuildTarget.iOS)
		    {
			    return PlayerSettings.iOS.buildNumber;
		    }
		    
		    if (buildTarget == BuildTarget.Android)
		    {
			    return PlayerSettings.Android.bundleVersionCode.ToString();
		    } 
		    return "";
	    }


	    private static void SaveBuildVersion(string version, string code)
	    {
		    GuruAppVersion.SaveToDisk(version, code);
	    }

	    /// <summary>
	    /// 获取打包场景
	    /// </summary>
	    /// <returns></returns>
	    private static string[] GetBuildScenes()
	    {
		    List<string> names = new List<string>();
		    foreach (var e in EditorBuildSettings.scenes)
		    {
			    if(e == null)
				    continue;
			    if(e.enabled)
				    names.Add(e.path);
		    }
		    return names.ToArray();
	    }

		/// <summary>
		/// 打开路径
		/// </summary>
		/// <param name="path"></param>
		private static void Open(string path)
	    {
#if UNITY_EDITOR_OSX
		    EditorUtility.RevealInFinder(path);
#else
			Application.OpenURL($"file://{path}");
#endif
		    
	    }
	    #endregion

	    #region 单元测试

		[Test]
	    public static void TEST_GetWorkingDir()
	    {
		    var path = GetWorkingDir();
		    Debug.Log(path);

		    if (Directory.Exists(path))
		    {
			    Open(path);
		    }
		    else
		    {
			    Debug.LogError($"path not found: {path}");
		    }

	    }

	    [Test]
	    public static void TEST_BuildVersionString()
	    {
		    var nowDate = new DateTime(2024, 8, 1, 0, 0, 0);
		    string strYear = nowDate.Year.ToString().Substring(2);
		    string strMon = nowDate.Month.ToString("00");
		    string strDay = nowDate.Day.ToString("00");
		    string strQuarter = ((nowDate.Hour * 60 + nowDate.Minute) / 15).ToString("00");
		    // 2024-08-01 00:00:00  to version string: 24080100
		    string strBuildNumber = $"{strYear}{strMon}{strDay}{strQuarter}";
		    Debug.Log($"Get BuildVersion Code: {strBuildNumber}");
	    }
	    #endregion
	    
    }
}



