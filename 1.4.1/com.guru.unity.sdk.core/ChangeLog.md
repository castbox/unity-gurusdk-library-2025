# Guru Template Change Log

## 2.0.3

- Firebase
  - 整体升级到 10.1.1. 作为Unity组统一标准
- Max 广告聚合
  - 升级Max聚合的所有渠道, 对齐[中台需求](https://docs.google.com/spreadsheets/d/161UnDimGerqetIYNiMCfUBmJ7qozht8z1baxnxRdCnI/edit#gid=311231520)  
- GuruBuildTools
  - 新增 Deps Reporter, 用于在Jenkins环境, 上报打包依赖 (Android/iOS). 理论上报告工具不影响本地打包, 不影响打包机打包. 出问题的话也不会中断打包流程.
  - 修复 IOS 打包成功后, 上传TF时报Info.plist内没有设置版本号的报错
- GuruAds
  - 根据中台的需求, 新增广告属性打点:  `bads_imp`, `bads_loaded`
  - 去掉使用三方广告SDK, 需要预先设置 AD_AMAZON 和 AD_PUBMATIC 的流程. 目前默认全部接入 
- GuruAnalytics
  - 更新和添加 Worker 支持, 预期可让用户在线时长更加精准
- DeviceUtil
  - 更新Android端获取系统版本号的的接口, 方便程序判断 Android API 33 以及后继的响应 


## 2.0.2

各模块对应的内容更新

- FBService 
  - 添加IOS平台打点开启语句 `FB.Mobile.SetAdvertiserTrackingEnabled(true);`
- AdjustService 
  - 自动创建生命周期对象, 优化部分逻辑
- Firebase
  - 修复 Firebase(8.1.0) 无法生成 google-service.json 的问题. 替换了Firebase.Editor.dll
- Guru L10n 更新
  - 更新GuruL10N -> 0.4.0 新增单独翻译google sheet的设置
  - 更新插件 backup.csv 缓存路径, 请各个项目将 **`backup.csv`** 提交到代码内进行跟踪!
  - 更新了语言编码去重识别, 根据Alpha表重排的功能
- Guru Analytics
  - 广告打点中加入了 Waterfall 信息上报
  - 添加了用户属性缓存表
- Guru Build Tools
  - 新增 DebugView 联调参数注入, 需要项目安装验证
  - 更新打包管线相关逻辑, IOS Target Version 升级为 13.0
- RemoteConfig
  - 修复了AROConfig的初始化逻辑, 避免初始化时报空引发报错
- IAPService
  - 添加了本地的订单数据缓存, 防止特殊操作的时候, iOS的订单重复上报的问题.


## 2.0.1
- 更新自打点库原生依赖 
  - Android 原生库版本 (0.2.10)
  - IOS 原生库版本 (0.2.16) 
- 更新 L10n 版本 (3.2.0)
  - 更新了导入表格会把code行也导入进来的BUG
  - 更新配置多语言时 nb-NO 语言的识别功能


## 1.6.0
- 添加了 Amazon (1.0.0) 广告扩展，使用说明详见: [GuruAds 扩展 Amazon安装说明](GuruAds/README.md)
- 添加了 GuruAnalytics (1.0.3) 广告自打点插件，使用说明详见: [Guru Unity Analytics 自打点插件](GuruAnalytics/README.md)
- 添加了 StandardProperties 标准属性点类， 用于记录游戏内的标准事件和属性
- 添加了 GuruBuildTool 模块， 收集和整理了目前为止所有 BuildTool 相关的逻辑
- 扩展了GuruSettings的配置格式(Amazon广告配置相关)
- 在 AdjustService 内添加获取 AdjustId 以及 FirebaseID 的逻辑
- 在 AdjustService 内添加上报 DeviceID 的逻辑
- 修复了若干插件显示层以及构建管线的BUG
- 调整了框架整体的文件结构