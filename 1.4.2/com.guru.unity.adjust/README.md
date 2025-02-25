# GURU Adjust

### VERSION 0.1.0

## 插件介绍

### <b>Adjust插件</b>

版本依赖:
- Unity: `4.36.0`
- Android: `4.37.0`
- iOS: `4.36.0`
- Windows: `4.17.0`


本项目是基于[Adjust官方的Unity SDK插件](https://github.com/adjust/unity_sdk)，进行了一定的封装.

方便Unity开发者通过UPM使用 Adjust 相关的功能。





## 安装和接入


### 插件引入

- 根据文档部署好本机配置后, 请在Unity内部配置如下参数
- - 确保自己拥有该项目的访问权限
  - 确保自己的Github账户可以使用SSH或者UserToken的方式拉取代码. (用户名密码的方式已不被Github支持)
  - 修改位于项目 `Packages/manifest.json` 文件，在`dependencies`中添加
  ```
  {
  "dependencies": {
    "com.guru.unity.adjust": "git@github.com:castbox/upm_guru_adjust.git#0.1.0",
    ...
    }
  }
  ```