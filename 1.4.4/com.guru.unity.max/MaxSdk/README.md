# Guru Ads Mediation







## 开发注意事项

### Android 构建文件配置

- 项目 `BuildSettings/Player Settings/Publishing Settings` 中需要开启
  - [x] Custom Main Manifest
    - Assets/Plugins/Android/AndroidManifest.xml
  - [x] Custom Main Gradle Template
    - Assets/Plugins/Android/mainTemplate.gradle
  - [x] Custom Launcher Gradle Template
    - Assets/Plugins/Android/launcherTemplate.gradle
  - [x] Custom Gradle Properties Template
    - Assets/Plugins/Android/gradleTemplate.properties

  </br>


- 项目中 AndroidManifest.xml 更新需求:
    - 在 <applicaiton> `最后添加 APPLICATION_ID` 数据
  ```xml
  <meta-data android:name="com.google.android.gms.ads.APPLICATION_ID"
               android:value="{your_roject_app_id}" />

  
  // Example APP_ID : ca-app-pub-2436733915645843~5500018314
  
  ```
  - 在 <manifest> 最后添加
  ```xml
  <uses-permission android:name="com.google.android.finsky.permission.BIND_GET_INSTALL_REFERRER_SERVICE" />
  <uses-permission android:name="com.google.android.gms.permission.AD_ID" />
  ```
  </br>

  - 项目的 launcherTemplate.gradle
    - 添加如下代码解决打包报错问题
    - 替换 `**PACKAGING_OPTIONS**` 为以下指定的内容 (Amazon的接入指南)
    ```groovy
    // 加入以下修改
    android {
      ...
  
      configurations {
          all*.exclude module: 'okio'  // 修复okio库声明重复的问题
      }
    
      lintOptions {
          abortOnError false
          checkReleaseBuilds false  // 修复Lint报错的问题
      }
    
      packagingOptions {
        exclude("META-INF/DEPENDENCIES")
        exclude("META-INF/LICENSE")
        exclude("META-INF/LICENSE.txt")
        exclude("META-INF/license.txt")
        exclude("META-INF/NOTICE")
        exclude("META-INF/NOTICE.txt")
        exclude("META-INF/notice.txt")
        exclude("META-INF/ASL2.0")
        exclude("META-INF/*.kotlin_module") 
      } 
      **PLAY_ASSET_PACKS****SPLITS**
    
      ...
    
      

    ```

  - 项目的 mainTemplate.gradle
    - 添加如下代码解决打包报错问题

    ```groovy
  
    android {
      ...
  
      configurations {
          all*.exclude module: 'okio'
      }

    ```
