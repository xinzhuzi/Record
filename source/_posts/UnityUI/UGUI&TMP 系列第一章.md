---
title: Unity UGUI优化
date: 2020-05-11 11:41:32
top: 100
categories:
- UnityUI
tags:
- UnityUI
---



# UGUI

* 1. 参考资料

* 2. https://www.jianshu.com/p/9bd461de19a7  博客
* 3. UWA
* 4. https://blog.uwa4d.com/archives/video_UI.html
* 5. https://blog.uwa4d.com/archives/1875.html
* 6. Unity 4.6f 源码
* 7. 何冠峰(文西) 的 UGUI 原理和优化 PPT


* 8. https://www.cnblogs.com/alps/p/7773149.html 
* 9. https://zhuanlan.zhihu.com/p/340508875
* 10. https://blog.csdn.net/zcaixzy5211314/article/details/86515168
* 11. https://github.com/BlueMonk1107/UGUISolution
* 12. https://github.com/ExcelDataReader/ExcelDataReader
* 13. https://github.com/monitor1394/unity-ugui-XCharts  雷达图    
* 14. https://blog.csdn.net/cyf649669121/article/details/83661023
     https://blog.csdn.net/cyf649669121/article/details/83785539
     https://blog.csdn.net/cyf649669121/article/details/86484168
     EventSystem耗时过长


# 学习资料群:  
1. 因涉及到收费插件,请加QQ 群:861960832

# 1.UI 基础:  
1. √ UGUI整体解决方案.
2. TMP 视频.  需要自己先尝试一下, TMP 自带的例子.
3. NGUI 与 UGUI 进行对比学习.


# 2. UI 扩展:
1. mob-sakai的 UI 方案:UIEffect & ParticleEffectForUGUI & SoftMaskForUGUI 特殊效果
2. Psd 2 Unity uGUI Pro 3.4.0 扩展插件,输出的都是半成品,美术极度反感,一定要慎重使用.
3. ui-extensions 一套UI框架(GitHub)
4. unity-ugui-XCharts 雷达图
5. Unity手游UI框架一站式解决方案_by_卢成浩,UI框架搭建
6. DoozyUI Complete UI Management System.unitypackage  assetstore的 UI动画方案
7. I2 Localization.unitypackage 多主题,多地区,多语言解决方案,过于臃肿,不建议使用
8. New UI Widgets v1.14.1 ,UGUI 的扩展方案
9. UGUI Super ScrollView ,滑动视图方案.
10. puremvc-csharp-multicore-framework-master Unity.PureMVC-master ,  MVC 模式,可以借鉴,但不使用.鉴于 UI 变动过于频繁,不建议使用乱七八糟的模式,以易用,好用为基础,直接选用 MVC 模式.(除了 MVC,其他都是异端)
11. UGUI-Editor-master ,UI 的辅助插件,为了开发人更容易制作 UI.
12. Optimized ScrollView Adapter 5.3.1 比 UGUI Super ScrollView ,滑动视图方案好,UGUI Super ScrollView缓存池子有 bug.

# 3. UI 优化: 
1. UI 模块优化案例分析, UWA 优化视频
2. Unity引擎渲染、UI、逻辑代码模块的量化分析和优化方法_by_UWA官方 
3. Unity引擎UI模块知识Tree.pdf
4. UGUI原理和优化.pptx
5. UGUI DrawCall与Canvas的Rebuild  偏邪教用法,不建议使用,不用看
6. UGUIOptimizeExample-master 合批优化规则
7. Unity 4.6 C++ 的 UI 源码
8. UnityResourceStaticAnalyzeTool-master UI 的合批分析是参照Unity源码编写的,可以深度理解合批原理.
9. LoopScrollRect 滑动视图解决方案.



# 4. UI 综合项目
1. UIPure 纯净版,支持 2019,2020 版本.包含分析文章,Unity4.6 UGUI 的 C++源码部分,合批规则在BatchSorting.cpp 类中的 PrepareDepthEntries 中;重叠,material.id,texture.id 三个核心点.
2. UIExample 例子版,将所需要的例子导入进项目中进行翻阅例子学习 UI.
3. UGUI&TMP 制作以及验证.
