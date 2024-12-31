# GURU Max

### VERSION 1.3.0

- Unity插件版本: 6.1.2
- Android: 12.1.0
- iOS: 12.1.0

## 插件介绍

AppLovin插件
本项目是基于AppLovin官方SDK的Unity插件(https://github.com/AppLovin/AppLovin-MAX-Unity-Plugin.git)，进行了一定的封装，方便Unity开发者通过UPM使用AppLovin广告平台。
一些文件根据UPM特定的目录结构要求进行了调整，部分代码进行了修改——都包含在 #region GuruDev中，以便后续跟随官方升级插件。
为了方便UPM控制各个组件的版本，没有包含AppLovin官方的SDK升级工具代码。

## 安装和接入
首先，需要确保项目已经安装external-dependency-manager。这是Google提供的面向Unity为解决Android与iOS原生项目依赖的工具库，目前Google Play Services、FireBase、Facebook、Admob等都使用了改库。
### 插件引入

- 根据文档部署好本机配置后, 请在Unity内部配置如下参数
  - 确保自己拥有该项目的访问权限
  - 确保自己的Github账户可以使用SSH或者UserToken的方式拉取代码. (用户名密码的方式已不被Github支持)
  - 修改位于项目 `Packages/manifest.json` 文件，在`dependencies`中添加
  ```
  {
    "dependencies": {
      "com.guru.unity.max": "ssh://git@git.chengdu.pundit.company/castbox/com.guru.unity.max.git#main",
      ...
    }
  }
  ```
