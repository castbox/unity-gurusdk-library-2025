# com.guru.unity.i2
#### 本项目在i2的基础上添加了印地语和泰语修复
#### 功能：
+ 移除missing脚本（目录右键 Guru-> I2-> 移除Missing脚本）
+ 一键添加/移除TextFontHelper（目录右键 Guru-> I2-> 添加/移除TextFontHelper）
+ 印地语的富文本修复，字体修复（提供了默认富文本修复DefaultRichTextFixer，各项目可继承IHindiRichTextFixer扩展）
+ 在添加Text和Tmp组件时自动添加TextFontHelper，需添加 ENABLE_I2_AUTO_FIX 宏定义(基于Inspector，需选中并显示Inspector面板时可自动添加)
+ 脚本修复工具
  + 打开工具面板：Guru-> I2-> ScriptReplaceWindow 
  + 获取missing脚本的fileID和guid（文本编辑器打开预制体，找到对应missing脚本的fileID和guid），复制到工具面板的Old FileID和Old GUID
  + 工具面板点击脚本文件选择最新脚本，复制fileID和guid到工具面板的New FileID和New GUID
  + 选择替换文件夹
  + 选择替换类型
  + 点击替换
#### 用法：
+ 新项目接入流程：
  + 导库
  + 创建FixFontAsset:在指定目录右键 Create-> Guru-> Create FixFontAsset
  + 在FixFontAsset添加修复字体(包内提供了泰语和印地语字体I2/Localization/Fonts)
  + 如果需要支持运行时切换多语言需要切换后调用I2Supporter.FixAll()
+ 老项目接入流程：
  + 接入前先确保git已备份 
  + 导库
  + 确保删除老版本废弃文件,删除老版本i2插件，删除老版本字体修复工具TextFontHelper，删除老版本FixFontAsset等（没有跳过），解决报错
  + 使用脚本修复工具修复I2Localize的missing脚本
    + 获取missing的guid和fileID:文本编辑器打开预制体，根据脚本中的字段定位对应的missing脚本，找到对应的fileID和guid，复制到工具面板的Old FileID和Old GUID
    + 获取新脚本的guid和fileID:工具中选择脚本文件(Packages/I2 Localization/I2/Localization/Scripts/Localize.cs)，复制guid和fileID到New GUID和New FileID
    + 选择替换文件夹，选择替换类型，点击替换
  + 使用脚本修复工具修复I2Languages.asset（修复前checkout I2Languages）
    + 获取missing的guid和fileID:文本编辑器打开I2Languages，复制到工具面板的Old FileID和Old GUID
    + 获取新脚本的guid和fileID:工具中选择脚本文件(Packages/I2 Localization/I2/Localization/Scripts/LanguageSource/LanguageSourceAsset.cs)，复制guid和fileID到New GUID和New FileID
    + 选择替换文件夹，选择替换类型，点击替换
  + 移除missing脚本
  + 一键添加TextFontHelper
  + 创建FixFontAsset:在指定目录右键 Create-> Guru-> Create FixFontAsset
  + 在FixFontAsset添加修复字体(包内提供了泰语和印地语字体I2/Localization/Fonts)
  + 如果需要支持运行时切换多语言需要切换后调用I2Supporter.FixAll()
+ 注意：
  + 本库提供的TMP字体均为动态字体
  + 替换完成后最好重启一下unity