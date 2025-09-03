using UnityEditor.Build.Reporting;

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
	    private const int DefaultAndroidTargetSdkVersion = 35; // 2025-08-31 需要合规
	    private const AndroidSdkVersions DefaultAndroidMinApiLevel = AndroidSdkVersions.AndroidApiLevel24;
	    private const string IOSTargetOSVersion = "13.0";
	    private const string GuruIOSTeamId = "39253T242A";
	    private const string GuruKeystoreName = "guru_key.jks";
	    private const string GuruKeystorePass = "guru0622";
	    private const string GuruAliasName = "guru";
	    private const string GuruAliasPass = "guru0622";

		public const string MACRO_RELEASE = "RELEASE";
		public const string MACRO_DEBUG = "DEBUG";
		public const string MACRO_LOG = "ENABLE_LOG";
		public const string DEFAULT_COMPANY_NAME = "Guru Game";
	    
		
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
	        if (!buildParam.IsBuildSymbols)
		    {
			    EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Disabled;
		    }
		    else
		    {
			    if (buildParam.CustomAndroidCreateSymbols == null)
			    {
				    EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;
			    }
			    else
			    {
				     EditorUserBuildSettings.androidCreateSymbols =
					    buildParam.CustomAndroidCreateSymbols switch
					    {
						    "debug" => AndroidCreateSymbols.Debugging,
						    "full" => AndroidCreateSymbols.Public,
						    _ => AndroidCreateSymbols.Disabled
					    };
			    }
		    }
#elif UNITY_6000
		    if (!buildParam.IsBuildSymbols)
		    {
			    UnityEditor.Android.UserBuildSettings.DebugSymbols.level = Unity.Android.Types.DebugSymbolLevel.None;
		    }
		    else
		    {
			    if (buildParam.CustomAndroidCreateSymbols == null)
			    {
				    UnityEditor.Android.UserBuildSettings.DebugSymbols.level = Unity.Android.Types.DebugSymbolLevel.SymbolTable;
			    }
				else
				{
					UnityEditor.Android.UserBuildSettings.DebugSymbols.level =
						buildParam.CustomAndroidCreateSymbols switch
						{
							"debug" => Unity.Android.Types.DebugSymbolLevel.SymbolTable,
							"full" => Unity.Android.Types.DebugSymbolLevel.Full,
							_ => Unity.Android.Types.DebugSymbolLevel.None
						};
				}

		    }
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
	        if ((int)PlayerSettings.Android.minSdkVersion < (int)DefaultAndroidMinApiLevel)
	        {
		        PlayerSettings.Android.minSdkVersion = DefaultAndroidMinApiLevel; // 设置 MinAPI Level
	        }
	        
	        //打包
	        string symbolDefine = buildParam.IsBuildRelease ? MACRO_RELEASE : MACRO_DEBUG;
	        string version = Application.version;
	        string extension = buildParam.IsBuildAAB ? ".aab" : ".apk";
	        if (EditorUserBuildSettings.exportAsGoogleAndroidProject) extension = ""; // 输出工程
		    string outputDir = Path.GetFullPath($"{Application.dataPath }/../{OutputDirName}/Android");
	        var apkPath = $"{outputDir}/{Application.productName.Replace(" ","_")}_{symbolDefine}_{version}_{buildNumber}{extension}";
	        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
	        
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
	        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
	        report.name = $"{Application.productName}_{version}_{buildNumber}_android";
	        PrintBuildReport(report, outputDir);
	        
	        if (buildParam.BuilderType == AppBuilderType.Editor)
	        {
		        Open(outputDir);
	        }
	        
	        // if (buildParam.AutoPublish)
	        // {
		       //  GuruPublishHelper.Publish(apkPath, buildParam.PgyerAPIKey); // 直接发布版本
	        // }
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

	        // var backgroundMode = iOSBackgroundMode.None;

#if UNITY_2021_3 || UNITY_2022
		    var backgroundMode = iOSBackgroundMode.RemoteNotification | iOSBackgroundMode.Fetch;
#elif UNITY_6000
		    var backgroundMode = iOSBackgroundMode.RemoteNotifications | iOSBackgroundMode.BackgroundFetch;
#endif
		    
	        //ios专用打包设置
	        PlayerSettings.muteOtherAudioSources = false;
	        PlayerSettings.iOS.appInBackgroundBehavior = iOSAppInBackgroundBehavior.Custom;
	        PlayerSettings.iOS.backgroundModes = backgroundMode; // 后台启动配置
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
		        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
		        report.name = $"{Application.productName}_{buildParam.BuildVersion}_{buildNumber}_ios";
		        PrintBuildReport(report, Path.GetFullPath($"{outputDir}/../"));
		        
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
		    if(string.IsNullOrEmpty(companyName)) companyName = DEFAULT_COMPANY_NAME;
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
			    defines.Remove(MACRO_RELEASE);
			    defines.Remove(MACRO_DEBUG);
		    }
		    
		    defines.Add(buildParam.IsBuildRelease ? MACRO_RELEASE : MACRO_DEBUG);
		    if (!buildParam.IsBuildRelease || buildParam.IsBuildShowLog)
		    {
			    defines.Add(MACRO_LOG);
		    }

		    if (buildParam.BuilderType == AppBuilderType.Jenkins)
		    {
			    UnityEditor.Android.AndroidExternalToolsSettings.stopGradleDaemonsOnExit = false; // 防止打包机上其他 Android 打包任务被停止
		    }

		    // defines.Add("mopub_manager");
		    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines.ToArray());
		    PlayerSettings.stripEngineCode = true;
		    // Hide Unity Logo
		    PlayerSettings.SplashScreen.show = false;
		    PlayerSettings.SplashScreen.showUnityLogo = false;
		    // Strip Engine Code
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

		/// <summary>
		/// 保存构建版本
		/// </summary>
		/// <param name="version"></param>
		/// <param name="code"></param>
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

	    /// <summary>
	    /// 打印构建报告
	    /// </summary>
	    /// <param name="report"></param>
	    /// <param name="outputPath"></param>
	    private static void PrintBuildReport(BuildReport report = null, string outputPath = null)
	    {
		    try
		    {
			    if (report == null)
			    {
				    Debug.LogError($"=== Can not find Build report after build ===");
				    return;
			    }
			    
			    var header =
				    $"------------------------------------ Build Report [{report.name}] ------------------------------------\n\n";
			    Debug.Log(header);
			    
			    var sb = new System.Text.StringBuilder();
			    sb.Append($"\n\tBuild Result: {report.summary.result}\n\n");
			    sb.Append($"\tPlatform: {report.summary.platform}\n");
			    sb.Append($"\tTotal Time: {report.summary.totalTime}\n");
			    sb.Append($"\tOptions: {report.summary.options}\n");
			    sb.Append($"\tTotal Errors: {report.summary.totalErrors}\n");
			    sb.Append($"\tTotal Warnings: {report.summary.totalWarnings}\n\n");
			    sb.Append($"\tTotal Size: {report.summary.totalSize}\n");
			    
			    if (report.summary.result != BuildResult.Succeeded)
			    {
#if UNITY_2022_3_OR_NEWER
				    sb.Append($"\tSummarizeErrors:\n\t\t{report.SummarizeErrors()}\n\n");
#endif
			    }
			    
			    Debug.Log(sb.ToString());
			    
			    // Files
			    sb.Append($"\n=== Files ===\n\n");
#if UNITY_2022_3_OR_NEWER
			    var files = report.GetFiles().ToList();
#elif UNITY_2021_3 
				var files = report.files.ToList();
#endif
			    if (files == null || files.Count == 0)
			    {
				    sb.Append($"\n\tNo Files Found\n\n");
			    }
				else{
					files.Sort((a, b) =>
					{
						if (a.size > b.size) return -1;
						if (a.size < b.size) return 1;
						return 0;
					});
					foreach (var file in files)
					{
						sb.Append($"[{file.path}]:\n\t[{file.role}]:{file.size * 0.001f:F4}M\n");
					}
				}
			    
			    sb.Append($"\n=== Files ===\n\n");
			    

			    // Steps
			    sb.Append($"\n=== BuildSteps ===\n\n");

			    if (report.steps == null || report.steps.Length == 0)
			    {
					sb.Append($"\n\tNo Steps Found\n\n");    
			    }
			    else
			    {
				    foreach (var step in report.steps)
				    {
					    sb.Append($"\n[{step.name}]: {step.duration}    depth:{step.depth}\n\n");
					    foreach (var m in step.messages)
					    {
						    sb.Append($"\t-[{m.type}]: {m.content}\n");
					    }
				    }
			    }
			    sb.Append($"\n=== BuildSteps ===\n\n");
			    
			    

			    sb.Append($"\n=== PackedAssets ===\n\n");
			    if (report.packedAssets == null || report.packedAssets.Length == 0)
			    {
				    sb.Append($"\n\tNo PackedAssets Found\n\n");
			    }
			    else
			    {
				    foreach (var asset in report.packedAssets)
				    {
					    sb.Append($"[{asset.name ?? "?"}]: {asset.shortPath}\n");
					    foreach (var c in asset.contents)
					    {
						    sb.Append($"[{c.id}][{c.sourceAssetPath}]  size: {c.packedSize} k\n");
					    }
				    }
			    }
			    
			    sb.Append($"\n=== PackedAssets ===\n\n");


			    if (report.strippingInfo != null)
			    {
				    sb.Append($"\n=== StrippingInfo ===\n\n");
				    if (report.strippingInfo != null || !report.strippingInfo.includedModules.Any())
				    {
					    sb.Append($"\n\tNo strippingInfo Modules Found!\n\n");
				    }
				    else
				    {
					    foreach (var module in report.strippingInfo.includedModules)
					    {
						    var reasons = report.strippingInfo.GetReasonsForIncluding(module) ?? null;
						    var reasonStr = "null";
						    if (reasons != null)
						    {
							    reasonStr = string.Join(", ", reasons);
						    }

						    sb.Append($"[ {module} ]: {reasonStr}\n");
					    }
				    }
				    
				    sb.Append($"\n=== StrippingInfo ===\n\n");
			    }

			    var footer =
				    $"\n\n------------------------------------ Build Report End ------------------------------------\n\n";
			    sb.Append(footer);
			    Debug.Log(footer);
			    
			    // Write To File
			    if (outputPath != null)
			    {
				    if (!Directory.Exists(outputPath))
				    { 
					    Directory.CreateDirectory(outputPath);   
				    }

				    var outpath = Path.Combine(outputPath, $"buildreport_{report.name}.log");
				    File.WriteAllText(outpath, sb.ToString());
			    }


		    }
		    catch (Exception e)
		    {
			    Debug.LogError($"Print Build Report failed:\n{e.Message}");
		    }
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



