# Guru WebView

Verison 0.0.1

内置网页浏览器插件, 底层使用了用了 `UniWebView` 来实现内置网页浏览器功能

目前仅支持内部打开网页.

# Integration

代码调用: 

```C#
// 参数 url 用来填写打开的地址
// 参数 showToolbar 用来控制状态栏是否显示
// 关闭按钮 Done 的文本,当前版本在Android平台不可设置
// 默认情况下, showToolbar = true
GuruWebView.OpenPage("https://m.baidu.com");
```

## Android 平台

单独使用时, 在项目开启了`Minify`功能时, 请在 `Plaugins/Android/proguard-user.txt` 添加如下代码:
```yaml
-keep class com.onevcat.uniwebview.* { *; }
-keep class com.iab.omid.* { *; }
```
对于直接使用中台框架的项目可以自动注入混淆保护


## UniWebView 

插件版本
- Version: 5.0.3


