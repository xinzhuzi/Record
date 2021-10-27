---
title: Unity UGUI优化第八章
date: 2020-05-11 11:41:32
top: 108
categories:
- UnityUI
tags:
- UnityUI
---

# 过渡绘制OverDraw

* 1. OverDraw就是指GPU对屏幕一片区域的重复绘制次数,单位像素的重新绘制次数
    重叠就会产生OverDraw
     
* 2. Image Type 为Sliced、Tiled的Image 不需要填充九宫格的不要勾选Fill Center属性，例如头像边框这种，这可以将中间镂空减少重叠区域
    
* 3. 慎用Mask组件，它自带两层OverDraw；

* 4. 慎用Text组件的OutLine和Shadow，Shadow会增加一层OverDraw，而OutLine是复制了四份Shadow实现的；
    
* 5. 不使用空白或透明的Image，尽管alpha = 0，还是会渲染并增加一层OverDraw，可以重写脚本替代(Empty4Raycast)

