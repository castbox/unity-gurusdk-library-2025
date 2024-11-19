# Guru 评价管理器

Version 0.1.0

## 简介

Guru 评价管理器内置了多平台评价管理实现, 包括
- GooglePlay 平台的 InAppReview 接口 
- AppStore 平台的评价接口


## 库依赖

### **Android**

- 依赖于 `com.google.play.review` 的插件, 可在插件的 Package 文件夹内直接安装
- 注意: 由于模板已集成 ExternalDependencyManager (`1.2.174`), 因此无需导入包内的EDM插件.


### **iOS**

- 直接使用 Unity 内部的 `iOS` 的 Review 接口

## 实现方式

### 显示评价

只需调用以下代码即可:

```C#
// 直接显示包内的评价界面
GuruRating.ShowRating();
```


### 提交反馈
    
- 发送反馈时, 实际上是调用了系统的邮件App, 需要提供收件人的邮箱地址
- 邮件的标题主体可以自由定制, 传入参数即可

```C#
// 向 support@fungame.com 的邮箱发送反馈信息. 邮件的标题和信息可以定制
GuruRating.SetEmailFeedback("support@fungame.com", "Thank you for the feedback!");
```

