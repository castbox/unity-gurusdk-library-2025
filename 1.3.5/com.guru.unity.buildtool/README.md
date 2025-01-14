# GURU BuildTool

## 工具内容
- AppBuilder: 包体构建器，可直接配置参数打包
- PostProcessBuild
  - Android 管线
    - AndroidSettingsGradleFixer：修复 settingsTemplate.gradle 内的配置
    - UserProguardHelper：propguard-user.txt 合并器（功能问题，目前停用）
  - iOS 管线
    - XCProjectModifier: Xcode 主项目构建修改器， 注入和生成主要的配置和字段
    - XCPodModifier: Xcode 项目 Pods 依赖注入修改器，完善注入代码
    - XCPrivacyInfoImporter: Xcode 项目 PrivacyInfo 导入器
    - SKAdNetworkImporter：SKADNetwork 注入工具
    - FirebaseDebugViewImporter：FirebaseDebugView 注入工具

## 插件介绍

工具库包含：打包时需要用到的一键出包的包体构建器和构建时需要的各种 PostProcess 构建管线逻辑
