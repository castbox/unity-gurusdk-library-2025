# GuruConsent-iOS 

![Swift](https://img.shields.io/badge/Swift-5.0-orange.svg)&nbsp;

## 特性

- [x] 支持系统ATT权限引导弹窗.
- [x] 支持多国语言显示.
- [x] 支持调试EEA地理设置等.
- [x] 支持结果状态回调.

## 准备

将应用 ID 添加到 Info.plist 中:

```
<key>GADApplicationIdentifier</key>
<string>YOUR-APP-ID</string>
```

将ATT跟踪权限添加到 Info.plist 中:

```
<key>NSUserTrackingUsageDescription</key>
<string>This identifier will be used to deliver personalized ads to you.</string>
```

## 安装

GuruConsent 仅支持CocoaPods.

**CocoaPods - Podfile**

```ruby
source 'git@github.com:castbox/GuruSpecs.git'

pod 'GuruConsent'
```

## 使用

首先导入framework:

Swift:

```swift
import GuruConsent
```

Objective-C:

```objc
#import <GuruConsent/GuruConsent-Swift.h>
```

下面是一些简单示例. 支持所有设备和模拟器:

### 方式一: 自动

如果满足显示条件 自动显示弹窗 具有一定的延迟效果 但具体显示时机不定, 受网络影响.

__建议在应用启动后 延后一些调用开始 请务必确保首次启动后已授权网络请求权限再调用__

Swift:

```swift
// 开始请求
GuruConsent.start(from: controller) { result in
    switch result {
    case .success(let status):
        if #available(iOS 14, *) {
            print("ATT 结果: \(ATTrackingManager.trackingAuthorizationStatus)")
        }
        print("GDPR 结果: \(status)")
                
    case .failure(let error):
        print("失败: \(error)")
    }   
}
```

Objective-C:

```objc
// 开始请求
[GuruConsent startFrom:self success:^(enum GuruConsentGDPRStatus status) {
        
    if (@available(iOS 14, *)) {
        NSLog(@"ATT 结果: %lu", (unsigned long)ATTrackingManager.trackingAuthorizationStatus);
    }
    
    switch (status) {
        case GuruConsentGDPRStatusUnknown:
            
            break;
            
        case GuruConsentGDPRStatusRequired:
            
            break;
            
        case GuruConsentGDPRStatusNotRequired:
        
            break;
                
        case GuruConsentGDPRStatusObtained:
            
            break;
            
        default:
            break;
    }
    NSLog(@"GDPR 结果: %ld", (long)status);
        
} failure:^(NSError * _Nonnull error) {
    NSLog(@"失败: %@", error);
}];
```


### 方式二: 手动

先调用准备, 准备完成后在合适的时机手动调用弹窗显示.

__建议在应用启动后 延后一些调用准备 请务必确保首次启动后已授权网络请求权限再调用__

Swift:

```swift
// 准备
GuruConsent.prepare { result in
    switch result {
    case .success(let status):
        print("GDPR 结果: \(status)")
                
    case .failure(let error):
        print("失败: \(error)")
    }   
}


// 显示 请确保status为.required 否则无法显示
GuruConsent.present(from: self) { result in
    switch result {
    case .success(let status):
        if #available(iOS 14, *) {
            print("ATT 结果: \(ATTrackingManager.trackingAuthorizationStatus)")
        }
        print("GDPR 结果: \(status)")
    
    case .failure(let error):
        print("失败: \(error)")
    }
}
```

Objective-C:

```objc
// 准备
[GuruConsent prepareWithSuccess:^(enum GuruConsentGDPRStatus status) {
    
    switch (status) {
        case GuruConsentGDPRStatusUnknown:
            
            break;
            
        case GuruConsentGDPRStatusRequired:
            
            break;
            
        case GuruConsentGDPRStatusNotRequired:
        
            break;
                
        case GuruConsentGDPRStatusObtained:
            
            break;
            
        default:
            break;
    }
    NSLog(@"GDPR 结果: %ld", (long)status);
        
} failure:^(NSError * _Nonnull error) {
    NSLog(@"失败: %@", error);
}];


// 显示 请确保status为.required 否则无法显示
[GuruConsent presentFrom:self success:^(enum GuruConsentGDPRStatus status) {
    
    if (@available(iOS 14, *)) {
        NSLog(@"ATT 结果: %lu", (unsigned long)ATTrackingManager.trackingAuthorizationStatus);
    }
    
    switch (status) {
        case GuruConsentGDPRStatusUnknown:
                
        break;
                
        case GuruConsentGDPRStatusRequired:
                
        break;
                
        case GuruConsentGDPRStatusNotRequired:
                
        break;
                
        case GuruConsentGDPRStatusObtained:
                
        break;
                
        default:
        break;
    }
    NSLog(@"GDPR 结果: %ld", (long)status);
        
} failure:^(NSError * _Nonnull error) {
    NSLog(@"%@", error);
}];
```


### 调试设置 

`testDeviceIdentifiers`获取方式: 当传入空置, 运行调用`GuruConsent.start(from:)` Xcode控制台会输出如下:

```
<UMP SDK> To enable debug mode for this device, set: UMPDebugSettings.testDeviceIdentifiers = @[ @"8C5E8576-5090-4C41-8FC4-A5A80FF77D9E" ];
```
将控制台的`8C5E8576-5090-4C41-8FC4-A5A80FF77D9E` 复制粘贴到代码中, 再次运行即可进行调试.

Swift:

```swift
// 设置调试配置
let debug = GuruConsent.DebugSettings()
debug.testDeviceIdentifiers = ["8C5E8576-5090-4C41-8FC4-A5A80FF77D9E"]
debug.geography = .EEA
GuruConsent.debug = debug
```

Objective-C:

```objc
// 设置调试配置
GuruConsentDebugSettings *debug = [[GuruConsentDebugSettings alloc] init];
debug.testDeviceIdentifiers = @[@"8C5E8576-5090-4C41-8FC4-A5A80FF77D9E"];
debug.geography = GuruConsentDebugSettingsGeographyEEA;
GuruConsent.debug = debug;
```

重置状态

```swift
GuruConsent.reset()
```

## 运行

### 未授权过ATT权限 (非EEA地区): 
![IMG_4886](https://user-images.githubusercontent.com/13112992/201629493-3b95e3e8-ca02-41b6-9a64-1acd11ea4261.PNG)

点击`Continue`按钮弹出ATT授权弹窗

![IMG_4887](https://user-images.githubusercontent.com/13112992/201629676-3ca39406-513a-46ec-b79e-60df4fd7cc88.PNG)

### EEA地区: 
![IMG_4888](https://user-images.githubusercontent.com/13112992/201629612-b736c439-54fe-4c59-97ee-71a6a449f23a.PNG)

未授权过ATT权限 点击同意等按钮弹出ATT授权弹窗

![Simulator Screen Shot - iPhone 14 Pro - 2022-11-14 at 16 40 48](https://user-images.githubusercontent.com/13112992/201629990-ec3776b2-6bd0-4c3e-aba4-48f093216e11.png)


## 参考

[官方文档](https://developers.google.com/admob/ump/ios/quick-start)

## 协议

GuruConsent 使用 MIT 协议. 有关更多信息，请参阅 [LICENSE](LICENSE) 文件.
