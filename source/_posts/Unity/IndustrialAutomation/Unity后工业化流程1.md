---
title:  Unity后工业化流程1
date: 2020-06-20 11:41:32
top: 300
categories:
- Unity
tags:
- IndustrialAutomation
---

# 介绍

* 1:打包自动化,快速便捷交付产品给测试,运营,发布商.输出的产品类型为: iOS xxx.ipa;Android xxx.apk;Window xxx.exe;Mac xxx.dmg
* 2:程序通过 [Jenkins](https://www.w3cschool.cn/jenkins/jenkins-5h3228n2.html) 布置打包自动化流程,维护打包机,web 服务器等,只需要在 Jenkins 上面选择参数,点击构建,等待 10 分钟,即可得出最后结果.
* 3:构建打包自动化学习条件
目前在 windows 上面打包 iOS 产品异常麻烦,选择 mac 构建 Android 以及 iOS 产品比较节省学习成本.(windows 和 Mac 都可以构建完成)
```
1:windows 操作系统,windows 命令以及windows bat 文件编写,windows搭建 web 服务.
2:mac操作系统,mac 命令以及mac shell 脚本编写,mac 搭建 web 服务
3:Jenkins 基础,需要知道执行流程以及效果.
4:unity 手动打包,unity 代码打包,unity 提供的打包命令
6:Android Studio 打包,Xcode 打包 以及打包命令
7:Python 基础,需要知道Python 类方法与其他脚本交互的程度.
```
* 4:工作流程
```
1:在 mac 电脑上面安装 Jenkins.(不要使用 pkg 安装,官网下载然后放入应用程序里面即可)
2:编写脚本调用,查看Unity官网命令(https://docs.unity3d.com/Manual/CommandLineArguments.html).编写C#打包脚本.形成调用链
3:开启 Mac 的 Apache 服务器,编写 PHP脚本,外部机器内网访问即可
```
* 5:参考网址
```
https://www.jianshu.com/p/5ad61bb45b32
https://github.com/XINCGer/Unity3DTraining/tree/master/CI
```
* 6:附加静态资源检测
```
https://upr.unity.cn/
```

# Jenkins 安装

https://www.jianshu.com/p/5ad61bb45b32

# 脚本编写

https://www.jianshu.com/p/5ad61bb45b32

# 最终效果

https://www.jianshu.com/p/5ad61bb45b32

# 静态资源检测
* 1:使用 UPR 配合 Jenkins 检查游戏内的静态资源
* 2:直接在 UPR 网站上面查看到项目资源