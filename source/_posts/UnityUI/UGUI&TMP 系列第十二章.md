---
title: Unity UGUI优化
date: 2020-05-11 11:41:32
top: 112
categories:
- UnityUI
tags:
- UnityUI
---


# 依照 UGUI&TMP 的 UI 框架

* 1. 使用优化规则以空间换时间所做的 UI 框架.

* 2. GitHub: 

# UI 框架介绍

* 1. 开箱即用,Lua 或者 C# 及其方便,以 tolua 框架为底制作,移植及其方便.

* 2. 支持 2019.4 以及 2020 版

* 3. 先将设置 UI 环境,在 Project Settings的 Editor 选项中,将 Editing Environments 中的 UIEnvironment 设置为 UIEditorScene.      

Sprite Packer 选择 Sprite Atlas V2即可.         

添加 Layer --> NoGraphics.      

在 Package Manager 中添加 2D Sprite 插件

* 4. 没有 AB 加载模块,所有的 UI 都从 Resources 文件夹下模拟加载,如有需求,请自行编写.

* 5. 