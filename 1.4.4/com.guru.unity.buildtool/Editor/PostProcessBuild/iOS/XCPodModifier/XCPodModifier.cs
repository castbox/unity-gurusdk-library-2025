#if UNITY_IOS

namespace Guru.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// 针对AmazonSDK iOS平台构建后
    /// BitCode报错的问题
    /// </summary>
    public class XCPodModifier
    {

        /// <summary>
        /// 添加内容
        /// </summary>
        private static readonly string MOD_SCRIPT = @"#Compile bugs fixed by HuYufei 2023-11-16
post_install do |installer|
    installer.pods_project.targets.each do |target|
        target.build_configurations.each do |config|
            config.build_settings['ENABLE_BITCODE'] = 'NO'
            config.build_settings['CODE_SIGNING_ALLOWED'] = 'NO'
	        config.build_settings['IPHONEOS_DEPLOYMENT_TARGET'] = '13.0'
	        xcconfig_path = config.base_configuration_reference.real_path
            xcconfig = File.read(xcconfig_path)
            xcconfig_mod = xcconfig.gsub(/DT_TOOLCHAIN_DIR/, 'TOOLCHAIN_DIR')
            File.open(xcconfig_path, 'w') { |file| file << xcconfig_mod }
        end

        if target.name == 'BoringSSL-GRPC'
            target.source_build_phase.files.each do |file|
                if file.settings && file.settings['COMPILER_FLAGS']
                    flags = file.settings['COMPILER_FLAGS'].split
                    flags.reject! { |flag| flag == '-GCC_WARN_INHIBIT_ALL_WARNINGS' }
                    file.settings['COMPILER_FLAGS'] = flags.join(' ')
                end
            end
         end

    end
end";

        /// <summary>
        /// 构建操作
        /// 构建顺序 45-50 可以保证执行时序在MAX 自身生成podfile之后, 注入需要的逻辑
        /// AmazonSDK使用了45, 工具设为46,确保后发执行
        /// </summary>
        /// <param name="target"></param>
        /// <param name="projPath"></param>
        [PostProcessBuild(46)]
        private static void OnPostProcessBuild(BuildTarget target, string projPath)
        {
            if (target != BuildTarget.iOS)
                return;

            string podPath = Path.Combine(projPath, "Podfile");
            if (File.Exists(podPath))
            {
                bool needFix = false;
                string content = File.ReadAllText(podPath);
                if (!content.Contains("#BITCODE"))
                {
                    content = content + "\n" + MOD_SCRIPT;
                    File.WriteAllText(podPath, content);
                    Debug.Log($"<color=#88ff00>=== Fix Pods BitCode bug ===</color>");
                }


            }
            else
            {
                Debug.LogError($"=== POD not exists, exit pod hook...===");
            }
        }

    }
}

#endif