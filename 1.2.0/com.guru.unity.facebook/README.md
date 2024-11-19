# Facebook unity upm

## 快速开始

- #### 设置Facebook Settings

  打开Facebook/Edito Settings菜单（如果没有自动创建, 路径为:  Assets/FacebookSDK/SDK/Resources/FacebookSettings)。填写App Name、Facebook AppID、ClientToken（使用Login功能的时候提供），最后生成Mainifest。

  <img src="./img/image-20240611%E4%B8%8B%E5%8D%8855024439.png" alt="image-20240611下午55024439"  />

​	

<img src="./img/image-20240611%E4%B8%8B%E5%8D%8860008251.png" alt="image-20240611下午60008251"  />

## 功能说明

- #### 初始化

  > [!CAUTION]
  >
  > 在使用其他功能前必须要初始化

  ```c#
  using Guru.FacebookUnitySDK;
  
  GuruFacebook.Instance.Init((success)=>
  {
  });
  ```

- #### 使用LogEvent

  ```c#
  using Guru.FacebookUnitySDK;
  
  Dictionary<string, object> parameters = new ();
  parameters.Add("level", 1)
  GuruFacebook.Instance.LogAppEvent("eventName", parameters);
  ```

## 目录结构

```cpp
<Guru Facebook>
  |
  |-- Editor				# 编辑器工具，打包的时候会自动复制link.xml到工程目录下                  
  |-- FacebookSDK			# Facebook unity sdk，升级时直接替换此目录即可
  |-- Runtime				# 封装的facebook功能， 诸如登录，LogEvent等等，用户不直接访问Facebook unity sdk，可以更好的解耦。
  |-- Test				# 单元测试
  |-- *********************************************************************************************************************************************
```

