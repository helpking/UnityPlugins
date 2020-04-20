说明：

`重要提示`

 3.5分支将逐步废弃，以后主分支会逐渐升级到4.6分支版本

* Common
* Utils
* AssetBundles
* BuildSystem(AndroidSDK/iOS-XCode)
* Defines
* Settings
* Command
* Logs
* Dynamic
* Process Python&Bat脚本执行工具 -> Unity3d工程中有时候需要执行一些脚本。来部署一些设定。考虑到OSX系(`Shell`)/Windows系(`*.bat`)都与平台相关，所以选用Python。
* UI : -> 简单的一些信息脚本，不涉及具体的业务。
* SE
* Network : -> 简单的一些接口尚不完整(后续会继续开发)

详细可参看:[1.1.架构](./General.md#11%E6%9E%B6%E6%9E%84-1)

`以后考虑追加功能`:

* xlua/tolua
* AppMain/Manager : -> App启动脚本/管理器脚本插件化
* GameState : -> 游戏状态管理插件（以及不同状态跳转过渡状态设置等）
* ThreadPool : -> 线程池（主要用于数据/通信等，但是Unity3D2018后支持多线粗渲染，所以也可能排除）
* Database : -> 本地化数据库插件(SQLite)
* Localization : -> 本地化插件

# 目录

--------------------------------

## [1.概要](./General.md)

### [1.1.架构](./General.md#11%E6%9E%B6%E6%9E%84-1)

### [1.2.共通](./General.md#12%E5%85%B1%E9%80%9A-1)

#### [1.2.1.类方法扩展](./General.md#121%E7%B1%BB%E6%96%B9%E6%B3%95%E6%89%A9%E5%B1%95-1)

### [1.3.工具](./General.md#13%E5%B7%A5%E5%85%B7-1)

#### [1.3.1.日志](./General.md#131%E6%97%A5%E5%BF%97-1)

##### [1.3.1.1.分类&等级分类](./General.md#1311%E5%88%86%E7%B1%BB%E7%AD%89%E7%BA%A7%E5%88%86%E7%B1%BB-1)

##### [1.3.1.2.重定向](./General.md#1312%E9%87%8D%E5%AE%9A%E5%90%91-1)

##### [1.3.1.3.日志文件输出](./General.md#1313%E6%97%A5%E5%BF%97%E6%96%87%E4%BB%B6%E8%BE%93%E5%87%BA-1)

### [1.4.其他](./General.md#14%E5%85%B6%E4%BB%96-1)

#### [1.4.1.Process](./General.md#141process-1)

## [2.系统设定](./SysSettings.md)

### [2.1.菜单操作](./SysSettings.md#21%E8%8F%9C%E5%8D%95%E6%93%8D%E4%BD%9C-1)

### [2.2.编辑器扩展](./SysSettings.md#22%E7%BC%96%E8%BE%91%E5%99%A8%E6%89%A9%E5%B1%95-1)

#### [2.2.1.顶部/底部Bar](./SysSettings.md#221%E9%A1%B6%E9%83%A8%E5%BA%95%E9%83%A8bar-1)

#### [2.2.2.一般](./SysSettings.md#222%E4%B8%80%E8%88%AC-1)

#### [2.2.3.音效频道](./SysSettings.md#223%E9%9F%B3%E6%95%88%E9%A2%91%E9%81%93-1)

#### [2.2.4.网络](./SysSettings.md#224%E7%BD%91%E7%BB%9C-1)

#### [2.2.5.Tips](./SysSettings.md#225tips-1)

#### [2.2.6.选项](./SysSettings.md#226%E9%80%89%E9%A1%B9-1)

## [3.打包系统](./BuildSystem.md)

### [3.1.命令行参数](./BuildSystem.md#31%E5%91%BD%E4%BB%A4%E8%A1%8C%E5%8F%82%E6%95%B0-1)

#### [3.1.1.Apk/Ipa导出文件命名](./BuildSystem.md#311apkipa%E5%AF%BC%E5%87%BA%E6%96%87%E4%BB%B6%E5%91%BD%E5%90%8D-1)

### [3.2.宏定义](./BuildSystem.md#32%E5%AE%8F%E5%AE%9A%E4%B9%89-1)

### [3.3.AB打包](./BuildSystem.md#33ab%E6%89%93%E5%8C%85-1)

### [3.4.Apk打包](./BuildSystem.md#34apk%E6%89%93%E5%8C%85-1)

### [3.5.Ipa打包](./BuildSystem.md#35ipa%E6%89%93%E5%8C%85-1)

#### [3.5.1.Xcode设定](./BuildSystem.md#351xcode%E8%AE%BE%E5%AE%9A-1)

##### [3.5.1.1.Replace Targets](./BuildSystem.md#3511replace-targets-1)

##### [3.5.1.2.FrameWorks](./BuildSystem.md#3512frameworks-1)

##### [3.5.1.3.Libraries](./BuildSystem.md#3513libraries-1)

##### [3.5.1.4.Include Files](./BuildSystem.md#3514include-files-1)

##### [3.5.1.5.Include Folders](./BuildSystem.md#3515include-folders-1)

##### [3.5.1.6.Linking](./BuildSystem.md#3516linking-1)

##### [3.5.1.7.Language](./BuildSystem.md#3517language-1)

##### [3.5.1.8.Deployment](./BuildSystem.md#3518deployment-1)

##### [3.5.1.9.Build Options](./BuildSystem.md#3519build-options-1)

##### [3.5.1.10.Signing](./BuildSystem.md#35110signing-1)

##### [3.5.1.11.User Defined](./BuildSystem.md#35111user-defined-1)

##### [3.5.1.12.Other Setting](./BuildSystem.md#35112other-setting-1)

## [4.Dynamic](./BuildSystem.md)

### [4.1.地形编辑](./BuildSystem.md)

### [4.2.地形编辑器](./BuildSystem.md)

参考：

1.[Git提交的规范操作](./GitCommitMsg.md)

2.[JobSystem](./JobSystem.md)

3.[编辑器扩展](https://blog.csdn.net/yptianma/article/details/103268505)

--------------------------------

