---
title: Unity OverDraw优化
date: 2020-05-08 11:41:32
top: 520
categories:
- Unity优化
tags:
- Unity优化
---

# Overdraw

https://zhuanlan.zhihu.com/p/350778355?utm_source=wechat_session&utm_medium=social&utm_oi=1168948434773446656&utm_campaign=shareopn

* 1. 过渡渲染,透明与半透明叠加,导致屏幕上一个像素被多次叠加渲染.
* 2. 你多次绘制同一像素（在一帧中）,即在这一帧内,切换了不同的渲染状态,生成多次 DrawCall,即在同一像素点上,一直叠加半透,透明,不透明的图片.
* 3. 简单的后处理效果（例如 color grading）可能会对每个像素至少计算一次。由于它重画了屏幕上的每个像素，这直接增加了100%（1.0x）的OverDraw。
* 4. OverDraw 可能是导致 60 fps或30 fps（或更差）渲染帧率差距的原因。

# Overdraw性能分析
* 1. 渲染的目的是将图像（以非常快的速度）放置到屏幕上。这是通过特定步骤（Draw Calls）来完成场景渲染。其中某些渲染步骤并不是在屏幕中发生，而是在称为帧缓冲区的中间区域中发生。帧缓冲区就像画家用来画画的一块空画布。如果你想象你是画家，那么……每个笔画都会花一些时间，尤其是当他们不得不更改绘画时使用的颜色时。在图形编程方面，该帧缓冲区是一种特殊的纹理。由于许多原因，绘制到帧缓冲区很昂贵。每次绘制像素时，都需要从该缓冲区传输出和传入关键信息。此绘制操作会占用内存带宽。OverDraw会增加游戏渲染消耗的内存带宽。
* 2. 在TBR（tile-based rendering）的渲染架构上，这种带宽消耗尤其糟糕，碰巧是所有移动平台都是TBR的渲染架构.

# 在Unity中测量OverDraw的三种方法
* 1. OverDraw可视化模式,使用Unity内置的OverDraw视图模式.在 Unity3D中,打开场景视图,在右上角的 Shaded 下,下拉会出现一个OverDraw的可视化模式,在此模式下,最深的色彩表示具有最高OverDraw屏幕区域。如果你看到一个地区有大量的OverDraw，请分析它是由什么原因造成的。很可能是由于渲染半透明元素造成的。
* 2. Unity内置的透支可视化在技术上非常不正确。Unity使用不带Z-Write的附加着色器来显示它，因此你将看到的大多数不透明对象上的OverDraw都是假的。Unity没有考虑到渲染管线所做的Z-Test，正常情况下，很多不透明元素的渲染会被Z-Test剔除掉。但是，只要你知道它的局限性，它仍然是一个有用的工具。
* 3. 如果您使用的是HDRP 7.1+（高清渲染管线），那么你会很高兴听到有增强的OverDraw可视化工具可以使用。你可以通过Window → Render Pipeline → Render Pipeline Debug → Rendering来访问它，并将Fullscreen Debug Mode设置为“ TransparencyOverdraw ”

* 4. 使用RenderDoc分析OverDraw,免费的GPU图形调试器,请自行搜索使用.
* 5. 使用Compute Shader量化OverDraw,https://github.com/Nordeus/Unite2017/tree/master/OverdrawMonitor , 视频 https://www.youtube.com/watch?v=vJZcbscZ4-o&feature=emb_title&ab_channel=Unity
* 6. Snapdragon Profiler,从底层分析游戏性能的工具
* 7. Xcode 的 GPU 分析很精准
# 如何减少OverDraw
* 1. 减少不透明OverDraw,透明渲染通常会与Alpha Blend一起使用，这对于移动设备来说有更加昂贵。不要低估不透明OverDraw产生的性能开销。浪费内存带宽的开销依然很昂贵。即,全屏覆盖
* 2. Unity会尽最大努力从近到远渲染不透明元素。这就是说，我们首先渲染距离摄像机更近的元素，以便使用Z-Test剔除被遮挡的物体的像素。该排序基于相机与每个对象的边界框中心之间的距离。这种排序非常有利于GPU性能,这就是为什么在Unity中应该把渲染天空盒放在渲染不透明几何图形之后的原因。首先渲染天空盒会产生大量OverDraw。但是,由于低效率的渲染策略，会导致GPU OverDraw,某些网格肯定是麻烦制造者.
* 3. Unity Batching导致GPU OverDraw增加,批处理可以帮助你我减少渲染线程的CPU负载,它能够将多个DrawCall合并在一起提交给GPU.但是合并绘制调用意味着Unity将不再能够对其按原有的顺序进行排序，因为现在已经合为一体。这可能会导致严重的OverDraw。这在所有类型的批处理中都会发生：静态，动态和GPU Instance。通常发生的环境不是确定性的，这会使你的性能分析工作更加困难。常见的解决方案是禁用麻烦对象的批处理（或者按区域手动来进行批合）,另一种解决方案是通过将渲染队列更改为Geometry-1，Geometry + 1等来手动设置Unity渲染顺序。这几乎可以完全控制渲染顺序，但这带来了责任/风险（常见的是把地表、天空盒进行延后渲染）.
* 4. 减少透明渲染的OverDraw,由透明度引起的OverDraw对性能特别不利，因为渲染通常伴随Alpha Blend，这需要额外的读取和算术运算。更糟糕的是，透明材质通常不会写入Z-buffer，因此最终会写入更多无法丢弃的像素。你要做的是减少绘制的透明像素的数量及其成本。
```
    减少渲染的透明层的数量。
    减小透明几何图形所需的屏幕尺寸
    您可以按比例缩小透明对象的（屏幕）大小。
    尽最大可能从精灵中删除100％透明纹理像素。
    制作紧密的网格，以减少完全透明的区域。

```
* 5. Unity UI通常会导致OverDraw，因为系统仅支持全四方渲染。将可绘制的UI元素彼此堆叠非常容易。这些渲染为完整的四边形而不是紧密的网格。但是，精灵渲染器支持绘图网格，这些网格可以更准确地表示精灵的不透明部分，以减少过度绘制。
* 6. 粒子系统也擅长在透明层上创建OverDraw。问题在于它们经常将粒子堆叠在一起，因此GPU必须在混乱区域渲染太多像素。根据后期处理效果的实现方式，最终可能会引入一个新层，即每个引入的效果会造成100％OverDraw。这对于移动GPU来说非常不利。（话越少事儿越大）,如果您无法进一步减少OverDraw，请尝试通过additive而不是Alpha Blend来降低其成本。Additive blending比移动设备上的Alpha Blend要廉价得多。
























