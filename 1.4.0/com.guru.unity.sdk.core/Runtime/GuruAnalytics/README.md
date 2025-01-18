# Guru Unity Analytics

GuruAnalyticsLib 的 Unity 插件库

## 研发步骤:
-**Android**
  - 插件库内的 .aar 通过 [guru_analytics](https://github.com/castbox/guru_analytics) 项目直接构建
  - 打开 `guru_analytics` 工程，用 AndroidStudio 构建 aar 包体
    ```shell
    gradle publishToMavenLocal
    ```
  - 构建后请改名为 `guru-analytics-{version}.aar`， 其中 `version` 需要对齐引入库的版本
  - 请删除旧版本的 aar, 将新版本的 aar 文件放置于 `./Runtime/GuruAnalytics/Plugins/Android` 目录下 


- **iOS**
  - 插件库内的文件 通过 [GuruAnalytics_iOS](https://github.com/castbox/GuruAnalytics_iOS) 项目
  - (1) 请将 repo 内的两个文件夹 `Assets` 和 `Classses` 拷贝至 `./Runtime/GuruAnalytics/Plugins/iOS/GuruAnalytics` 目录下:
  - (2) 请将部署到 Unity 内所有的 `.swift` 文件的 meta 属性内， 取消 iOS 文件属性. (因为打包时会按照 POD 导入)
  - 注意及时更新 `GuruAnalyticsLib.podspec`文件内的更新内容
      ```ruby
      # 将 source 内的 git 属性内 git 源屏蔽， 只保留 tag 属性
      # s.source    = { :git => 'git@github.com:castbox/GuruAnalytics_iOS.git', :tag => s.version.to_s }
      s.source    = { :tag => s.version.to_s }
      ```

- 更新注意
  - 升级任意平台的 Native 库版本后，请手动修改此文件内 ChangeLog 的内容
  - 需要记录更新后 SDK 的提交 hash， 更新日志，以及 SDK 桥接文件对应的版本号
  - 目前是使用手动的方式修改源码来固定 SDK 的版本号，此处需要在下个版本做成脚本注入式的逻辑：
  ```swift
  // Packages/.upm/com.guru.unity.sdk.core/Runtime/GuruAnalytics/Plugins/iOS/GuruAnalytics/Classes/Internal/Utility/Constants.swift
  // line 29
  private static let guruAnalyticsSDKVersion: String = {
        return "0.4.1" // <---- 此处强制返回版本号
  //        guard let infoDict = Bundle(for: Manager.self).infoDictionary,
  //              let currentVersion = infoDict["CFBundleShortVersionString"] as? String else {
  //         return ""
  //     }
  //      return currentVersion
    }()
  
  ```
---

## Change Logs

- [SDK Repo [ guru_analytics ] ](git@github.com:castbox/guru_analytics.git)
- [SDK Repo [ GuruAnalytics_iOS ] ](git@github.com:castbox/GuruAnalytics_iOS.git)

### 1.13.1
- Android 端对齐 `1.1.2` （ 24 Spe 18 ）
- Unity 中台更新日期：24/11/20
  > Hash: 6457b242086ca34a5f5576ccfc71855babadaae1
- iOS 端对齐 `0.4.1` （ 25 Jan 15 ）
- Unity 中台更新日期：25/1/17
  > Hash: dc095f10187605f3b8761c2f11e78e3a58ceaf0b
- iOS 库更新，解决浮点数上报不精准的问题


### 1.13.0
- Android 端对齐 `1.1.2` （ 24 Spe 18 ）
- Unity 中台更新日期：24/11/20
  > Hash: 6457b242086ca34a5f5576ccfc71855babadaae1
- iOS 端对齐 `0.3.9` （ 24 Oct 16 ）
- Unity 中台更新日期：24/11/20
  > Hash: aaad44b7743d12d9346b43b2dc0de693bd9bcfaa
- Pod 库改为 本地文件引用 （配合外部发行项目）



### 1.12.1
- Android 端对齐 `1.1.1` （ 24 Jul 25 ）
- Unity 中台更新日期：24/8/7
  > Hash: 6cb6a889022147511fb6bc8a632aa24a54f57c7c
- iOS 端对齐 `0.3.6.1` （ 24 Sep 5 ）
- Unity 中台更新日期：24/9/14
  > Hash: 989023c36d81a6b47c3c1082247b638d849b0f0e
- Pod 库改为 本地文件引用 （配合外部发行项目）

### 1.12.0
- Android 端对齐 `1.1.1` （ 24 Jul 25 ）
- Unity 中台更新日期：24/8/7
  > Hash: 6cb6a889022147511fb6bc8a632aa24a54f57c7c
- iOS 端对齐 `0.3.6` （ 24 May 31 ）
- Unity 中台更新日期：24/8/7
  > Hash: 0cd5ce7aa64e12caa7413c938a3164687b973843
- Pod 库改为 本地文件引用 （配合外部发行项目）


### 1.11.0
- Android 端对齐 `1.0.3`
  > Hash: 1978686dbcba38b7b0421d8b6b2bef111356366b
- iOS 端对齐 `0.3.6`
  > Hash: 0cd5ce7aa64e12caa7413c938a3164687b973843
- Pod 库改为 本地文件引用 （配合外部发行项目）


### 1.9.0
- Android 端对齐 0.3.1+. 
  > Hash: 0457eba963a9049fb6a16708b921573ef36c99b1
- iOS 端对齐 0.3.3
  > Hash: c86d19fb38c8260f468e38d756aca84e89d58c8b
- 新增自打点的错误上报功能, 但需要项目内接入 GuruSDKCallbacks 对象才能完成日志回发的功能
- 错误上报开在 Plugin 外部应关依赖云控开启, 默认关闭. 


### 1.8.4
- 优化Android 端 Worker 调用逻辑, 重启 Worker 有助于让打点数据更准确

### 1.8.3
- 修复 fg 打点上报时长不正确的问题

### 1.8.2
- 修复参数类型转换的BUG, param数据转换为JSON对象

### 1.8.1
- 修复自打点浮点参数精度问题
- 添加太极020数值设置接口

### 1.7.5
- 删除 `androidx.appcompat:appcompat` 库依赖



---

## Document

- 项目整合插件后, **请一定要在各插件的初始化后上报各相关ID**:

- 相关接口如下

- ### UID

  ```C#

    // ---- 需要等待中台初始化后上报: 
    // 上报中台返回的用户ID
    string uid = IPMConfig.IPM_UID 
    GuruAnalytics.SetUid(uid);

  ```

- ### DeviceID
  ```C#
    // 上报设备ID
    string deviceId = IPMConfig.IPM_DEVICE_ID
    GuruAnalytics.SetDeviceId(DeviceID);

  ```

- ### FirebaseID
  ```C#

    // ---- 需要Firebase Analytic 初始化后, 异步获取对应的ID:
    private static async void InitFirebaseAnalytics()
    {
        Debug.Log($"---[ANA] IPM UID: {IPMConfig.IPM_UID}");
        
        var task = FirebaseAnalytics.GetAnalyticsInstanceIdAsync();
        await task;
        if (task.IsCompleted)
        {
            var fid = task.Result;
            if (!string.IsNullOrEmpty(fid))
            {
                Debug.Log($"---[ANA] Firebase ID: {fid}");
                GuruAnalytics.SetFirebaseId(fid);
            }
        }
        else
        {
            Debug.LogError("---- Get Firebase Analytics Instance Fail");
        }
    }


  ```

- ### AdjustID

    ```C#

    // ---- Adjust 启动后调用: 
    string adjustID = Adjust.getAdid();
    GuruAnalytics.SetAdjustId(adjustID);    


    ```

- ### AdID

    ```C#
    string adId = "";
    Adjust.getGoogleAdId(id =>
    {
        Debug.Log($"---- [ADJ] ADId: {id}");
        adId = id;
        GuruAnalytics.SetAdId(id);
    });


    ```

- 上报用户属性:

    ```C#

        string item_category = "main";
        int level = 7;

        GuruAnalytics.SetUserProperty("item_category", item_category);
        GuruAnalytics.SetUserProperty("level", level.ToString());

    ```

- 上报视图名称

    ```C#

        string screenName = "MainView";
        GuruAnalytics.SetScreen(screenName);

    ```


- 上报自定义打点:

    ```C#

        string eventName = "user_get_coin";
        Dictionary<string, dynamic> data = new Dictionary<string, dynamic>()
        {
            { "level", 7 },
            { "user_coin", 105L },
            { "win_rate", 21.25f },
            { "b_level", 7 },
            { "result", "retry" }
        };
        GuruAnalytics.LogEvent(eventName, data);

    ```
---



## 依赖台配置说明

本项目已开始使用 `ExternalDependencyManager` 简称 `EDM` 来解决各种库的依赖问题

详细配置可见: [Dependencies.xml](Editor/Dependencies.xml)

IOS 项目注意配置如下图:

--> 取消勾选 **Link frameworks statically**

![](Editor/imgs/sc01.png)


### Android 项目配置:

于主菜单 `BuildSettings/PlayerSettings/PubishSettings:`

开启如下选项:

- [x] Custom Main Gradle Template
- [x] Custom Properties Gradle Template

之后会在项目的 `Plugins/Android`内生成对应的文件.

(A) 修改 `gradleTemplate.properties`

添加一下内容支持 `AndroidX`

```java
org.gradle.jvmargs=-Xmx**JVM_HEAP_SIZE**M
org.gradle.parallel=true
android.enableR8=false
unityStreamingAssets=.unity3d**STREAMING_ASSETS**
android.useAndroidX=true
android.enableJetifier=true
**ADDITIONAL_PROPERTIES**
```

(B) 修改 `mainTemplate.gradle`

于 `dependency` 内添加如下依赖 (目前会自动添加, 无需手动添加)

```java

dependencies {
    ...

    implementation 'androidx.core:core:1.7.0'
    compile 'com.mapzen:on-the-road:0.8.1'

    // basicDependencies
    implementation 'androidx.appcompat:appcompat:1.5.1'
    implementation 'com.jakewharton.timber:timber:4.7.1'
    implementation 'com.google.code.gson:gson:2.8.5'
    // roomDependencies
    implementation 'androidx.room:room-runtime:2.4.3'
    implementation 'androidx.room:room-rxjava2:2.4.3'
    // retrofitDependencies
    implementation 'com.squareup.retrofit2:retrofit:2.7.1'
    implementation 'com.squareup.retrofit2:converter-gson:2.7.1'
    implementation 'com.squareup.retrofit2:adapter-rxjava2:2.7.1'
    // okhttpDependencies
    implementation 'androidx.work:work-runtime:2.7.1'
    implementation 'androidx.work:work-runtime-ktx:2.7.1'
    implementation 'androidx.work:work-rxjava2:2.7.1'
    // process
    implementation 'androidx.lifecycle:lifecycle-process:2.4.0'
    // okhttp3
    implementation 'com.squareup.okhttp3:okhttp:4.9.3'
    
    ...
}

```

最低 `minTarget` 设置为 **21**

(D) 修改 `proguard-user.txt` 文件, 在最后追加此插件的相关代码

若项目使用了 ProGuard 压缩混淆, 需要修改此文件, 否则可能造成JAVA类无法被找到

```java

...

-keep class com.guru.** { *; }
-keep class guru.core.** { *; }

```


---




## 示例项目

- 示例项目位于 [~Sample](~Sample) 目录内. 详见 [CuruAnalyticsDemo.cs](~Sample/CuruAnalyticsDemo.cs)
- 示例借用了 BallSortPuzzle 的 `AppID` 和 `BundleID`



