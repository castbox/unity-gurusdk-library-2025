
namespace Guru.Editor
{
    using UnityEngine;
    public enum AppBuilderType
    {
        Editor = 0,
        Jenkins,
        Others,
    }
    
    public class AppBuildParam
    {
        public const string TargetNameAndroid = "Android";
        public const string TargetNameIOS = "iOS";
        
        //------------ Basic ----------------
        public bool IsBuildRelease = false; // 是否构建发布包体
        public bool IsBuildShowLog = false; // 是否显示日志
        public AppBuilderType BuilderType; // 构建类型
        public string BuildVersion = ""; // 构建版本号, 填写后会依据此版本设置应用的 Version
        public bool AutoSetBuildNumber = true; // 自动设置构建号, 可参考 Guru的SDK 接入说明文档
        public bool UseGuruCerts = true; // 是否使用 Guru 的证书打包
        public string TargetName = "";
        //------------ Android ----------------
        public bool IsBuildAAB; // 是否构建 AAB 包体 ( GooglePlay 发布专用 )
        public bool IsBuildSymbols = false; // 是否需要构建 Symbols.zip 文件 ( GooglePlay 发布专用 )
        public int AndroidTargetVersion = 0; // Android SDK 版本设置 ( GooglePlay 发布专用 )
        public bool AndroidUseMinify = false; // 是否开启 Android 的代码混淆和保护文件
        public bool DebugWithMono = true; // 是否使用 Mono 编译项目 ( Android Debug包专用 )
        public string AndroidKeystorePath = ""; // Android KeyStore 文件名
        public string AndroidKeystorePass = ""; // Android KeyStore 文件名
        public string AndroidAlias = ""; // Android KeyStore 文件名
        public string AndroidAliasPass = ""; // Android KeyStore 文件名
        public string CustomGradlePath = ""; // 自定义 Gradle 路径
        public string CustomJDKRoot = ""; // 自定义 JDK 路径 
        public string CustomNDKRoot = ""; // 自定义 NDK 路径 
        public string CustomAndroidSDKRoot = ""; // 自定义 AndroidSDK 路径 
        //------------ iOS ----------------
        public string IOSTargetVersion = ""; // IOS SDK 版本设置 ( iOS 发布专用 )
        public string IOSTeamId = ""; // IOS 打包 TeamId ( iOS 使用专用的开发证书后开启 )
        public string CompanyName = ""; // 发布厂商的名称
        //------------ Publish -------------
        public bool AutoPublish = false;
        public string PgyerAPIKey = "";
        
        
		    
        public override string ToString()
        {
            return $"build params: \n{JsonUtility.ToJson(this, true)}";
        }


        public static AppBuildParam Build(bool isRelease, AppBuilderType builderType = AppBuilderType.Editor, string version = "", bool autoBuildNumber = true, string companyName = "",  
            string targetName = "", bool buildShowLog = false, bool useGuruCerts = true, 
            bool buildSymbols = false,  bool buildAAB = false, bool useMinify = false,  int androidTargetVersion = 0, bool debugWithMono = true,
            string iOSTargetVersion = "", string iOSTeamId = "")
        {
            return new AppBuildParam()
            {
                TargetName = targetName,
                IsBuildRelease = isRelease,
                IsBuildShowLog = buildShowLog,
                BuilderType = builderType,
                BuildVersion = version,
                AutoSetBuildNumber = autoBuildNumber,
                IsBuildAAB = buildAAB,
                IsBuildSymbols = buildSymbols,
                AndroidTargetVersion = androidTargetVersion,
                AndroidUseMinify = useMinify,
                DebugWithMono = debugWithMono,
                IOSTargetVersion = iOSTargetVersion,
                IOSTeamId = iOSTeamId,
                CompanyName = companyName,
                UseGuruCerts = useGuruCerts,
            };
        }


        /// <summary>
        /// 构建Android参数
        /// </summary>
        /// <param name="isRelease"></param>
        /// <param name="version"></param>
        /// <param name="autoBuildNumber"></param>
        /// <param name="builderType"></param>
        /// <param name="companyName"></param>
        /// <param name="useGuruCerts"></param>
        /// <param name="useMinify"></param>
        /// <param name="androidTargetVersion"></param>
        /// <param name="debugWithMono"></param>
        /// <param name="isBuildAAB"></param>
        /// <returns></returns>
        public static AppBuildParam AndroidParam(bool isRelease, string version = "", bool autoBuildNumber = true,
            AppBuilderType builderType = AppBuilderType.Editor,
            string companyName = "", bool useGuruCerts = true, bool useMinify = false, int androidTargetVersion = 0, 
            bool debugWithMono = true, bool isBuildAAB = false)
        {
            bool buildAAB = isBuildAAB;
            bool buildShowLog = isRelease;
            bool buildSymbols = isRelease;
            string targetName = TargetNameAndroid; 
            return Build(isRelease, builderType, version, autoBuildNumber,companyName, targetName, buildShowLog, useGuruCerts, buildSymbols,  buildAAB, useMinify, androidTargetVersion, debugWithMono);
        }


        /// <summary>
        /// 构建iOS参数
        /// </summary>
        /// <param name="isRelease"></param>
        /// <param name="version"></param>
        /// <param name="autoBuildNumber"></param>
        /// <param name="builderType"></param>
        /// <param name="companyName"></param>
        /// <param name="useGuruCerts"></param>
        /// <param name="iOSTargetVersion"></param>
        /// <param name="iOSTeamId"></param>
        /// <returns></returns>
        public static AppBuildParam IOSParam(bool isRelease, string version = "", bool autoBuildNumber = true, AppBuilderType builderType = AppBuilderType.Editor,
            string companyName = "", bool useGuruCerts = true, string iOSTargetVersion = "", string iOSTeamId = "" )
        {
            bool buildShowLog = isRelease;
            string targetName = TargetNameIOS; 
            return Build(isRelease, builderType, version, autoBuildNumber, companyName, targetName, buildShowLog, useGuruCerts, iOSTargetVersion:iOSTargetVersion, iOSTeamId:iOSTeamId);
        }

    }
}