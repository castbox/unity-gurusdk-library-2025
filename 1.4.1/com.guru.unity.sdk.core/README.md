# Guru SDK Core

**Version 2.2.1**

- 更新 Firebase -> Unity 11.7.0  |  FirebaseSDK 10.20.0 (iOS 10.22.0)
- 添加 GooglePlay DMA 合规逻辑 (2024 年 3 月 6 日之前升级即可)
- 广告渠道 Inmobi 升级 双端版本: Android 10.6.3.0 ,  iOS 10.6.0.0
- 广告渠道 Pubmatic iOS 平台版本指定为 3.2 修复打包报错的问题
- 更新内核的 JsonParser 解析器为 JsonConvert
- fix: 修复配置导出功能解析报错的问题
- fix: 修复 ABTestManager 的解析错误.
- fix: 修复 Tch 打点在不满足 0.01 的情况下补偿的逻辑
- fix: 修复 GuruConsent 在 iOS 上返回 Purpose 乱码的解析问题, 添加了 TCF 映射规则.


**Version 2.1.0**

- 插件整体调整了文件结构，Guru目录整体迁移到新的Repo, 作为模版项目的 submodule, [新repo链接](https://github.com/castbox/upm-guru-sdk-core-proto)
  - 细分了相关模块的路径，添加了对应的 `asmdef` 文件
  - 优化了 AdServices 模块, 添加了对应的数据 Model
  - 优化了 GuruSettings 文件, 添加了对应的属性和赋值接口
  - 修复了部分BUG
- 升级 `Adjust` -> 4.36.0
- 新增 `UniWebview` 控件
- 升级 `I2` -> 2.8.22 f3



**Version 2.0.3**

- [升级须知](#notice)


## 依赖库

### Firebase
- 整体升级为 10.1.1

### AppLovin Max
- 整体升级为 11.11.3
- 详细的广告Adapter版本, [详见这里](https://docs.google.com/spreadsheets/d/161UnDimGerqetIYNiMCfUBmJ7qozht8z1baxnxRdCnI)

---

## 子模块

### GuruCore
GuruSDK 核心逻辑类

### GuruAds
GuruSDK 封装了广告服务相关的接口
- 新增了 Moloco 和 Pubmatic 两个渠道
- 新增了ATTManager 用于管理ATT弹出和相关事件统计

### GuruAnalytics
Guru自打点统计模块
- 更新了用户时长统计修复, 修复 Worker 启动报错的问题

### GuruConsent
使用 Funding Choices 作为数据的启动广告隐私权限引导模块

### GuruBuildTool
构建工具合集
- 更新了SKADNetwork 数据
- 更新 Xcode15 构建支持

### GuruIAP
支付服务相关接口, 底层使用的是 Unity 的 In-App-Purchase 插件

### GuruEntry
游戏入口模块


### GuruL10n
Guru的翻译模块, 内部衔接了 I2 Localization 插件, 外部衔接中台自动翻译接口


### GuruRating
游戏评分模块

### Keywords
Max Keywords 上报模块


---

<span id="notice"></span>
## 升级须知

### Android

- 需要在 `BuildSettings/Player Settings.../Publishing Settings` 内, 开启使用 `Custom Main Gradle Template`
- 可以直接使用中台提供的 `launcherTemplate.gradle` 文件
- 或者在新生成的 `Assets/Plugins/Android/launcherTemplate.gradle` 内添加如下代码:
    ```groove
    android {    
        ...
        
        lintOptions {
            abortOnError false
            checkReleaseBuilds false // <---请添加此行代码
        }
        
        
        // 请将模版内的 **PACKAGING_OPTIONS** 替换为如下代码
        packagingOptions {
            exclude("META-INF/*.kotlin_module")
        }
        
        ...   
    }    
    ```

- Android 构建的最小 Target Version 为 21

### iOS
- 构建相关的升级已经提交至 BuildTools 内
- 其他问题持续收集中