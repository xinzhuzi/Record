---
title: Unity工具使用
date: 2020-05-08 11:41:32
top: 506
categories:
- Unity优化
tags:
- Unity优化
---

# ARM Mobile Studio

# XCode

# AndroidStudio

# RenderDoc

# UWA

# UPR

# Profile

# Frame Debug

# Physics Debug

# 渲染统计窗口
游戏视野（Game View）右上角有一个Stats按钮。这个按钮是按下状态时，会显示一个半透明窗口，它显示对于优化性能很有用的实时渲染统计数据。根据目标平台不同，精确的统计数据也不同。


* Time per frame and FPS:每帧时间和FPS.处理和渲染一帧消耗的时间（它决定FPS）。注意这个时间仅包含每帧的逻辑更新和游戏视图渲染，不包含编辑器绘制屏幕适度、检视器和其它编辑器中才有的处理。          
详情:表示引擎处理和渲染一个游戏帧所花费的时间,该数字主要受到场景中渲染物体数量和 GPU性能的影响，FPS数值越高，游戏场景的动画显示会更加平滑和流畅。一般来说，超过30FPS的画面人眼不会感觉到卡，由于视觉残留的特性，光在视网膜上停止总用后人眼还会保持1/24秒左右的时间，因此游戏画面每秒帧数至少要保证在30以上。另外，Unity中的FPS数值仅包括此游戏Scene里更新和渲染的帧，编辑器中绘制的Scene和其它监视窗口的进程不包括在内。


* CPU:获取到当前占用CPU进行计算的时间绝对值，或时间点，如果Unity主进程处于挂断或休眠状态时，CPU time将会保持不变。

* Render thread:GPU渲染线程处理图像所花费的时间，具体数值由GPU性能来决定.

* Batches:绘制调用,“批处理”的引擎试图将多个渲染对象合并到一个内存块中, 以减少CPU开销。即Batched Draw Calls,是Unity内置的Draw Call Batching技术。        
首先解释下什么叫做“Draw call”，CPU每次通知GPU发出一个glDrawElements（OpenGl中的图元渲染函数)或者 DrawIndexedPrimitive（DirectX中的顶点绘制方法）的过程称为一次Draw call,一般来说，引擎每对一个物体进行一次DrawCall，就会产生一个Batch,这个Batch里包含着该物体所有的网格和顶点数据，当渲染另一个相同的物体时，引擎会直接调用Batch里的信息，将相关顶点数据直接送到GPU,从而让渲染过程更加高效，即Batching技术是将所有材质相近的物体进行合并渲染。主要为 GPU调用数据 节省资源与时间.      
对于含有多个不同Shader和Material的物体，渲染的过程比较耗时，因为会产生多个Batches。每次对物体的材质或者贴图进行修改，都会影响Batches里数据集的构成。因此，如果场景中有大量材质不同的物体，会很明显的影响到GPU的渲染效率。这里说几点关于Batches优化相关的方案:
> 虽然Unity引擎自带Draw Call Batching技术，我们也可以通过手动的方式合并材质接近的物体；      
> 尽量不要修改Batches里物体的Scale，因为这样会生成新的Batch。    
> 为了提升GPU的渲染效率，应当尽可能的在一个物体上使用较少的材质，减少Batches过多的开销； 
> 对于场景中不会运动的物体，考虑设置Static属性,Static声明的物体会自动进行内部批处理优化。    

* Verts：摄像机视野(field of view)内渲染的顶点总数。  

* Tris:   摄像机视野(field of view)内渲染的的三角面总数量。      

> 1. Camera的渲染性能受到Draw calls的影响。之前说过，对一个物体进行渲染，会生成相应的Draw call，处理一个Draw Call的时间是由它上边的Tris和Verts数目决定。尽可能得合并物体，会很大程度的提高性能。举个很简单例子，比如场景一种有1000个不同的物体，每个物体都有10个Tris；场景二中有10个不同的物体，每个物体有1000个Tris。在渲染处理中，场景一中会产生1000个Draw Calls，它的渲染时间明显比场景二慢。     
> 2. Unity stats 视图中的 Tris 和 Verts 并不仅仅是视锥中的梯形内的 Tris 和 Verts，而是Camera中 field of view所有取值下的tris和verts，换句话说，哪怕你在当前game视图中看不到这个 cube，如果当你把 field of view调大到 179 过程中都看不到这个cube，stats面板才不会统计，GPU才不会渲染，否则都会渲染，而且unity不会把模型拆分，这个模型哪怕只有1个顶点需要渲染，unity也会把整个模型都渲出来。（参考自Mess的《Unity Camera组件部分参数详解》）



* Screen:获当前Game屏幕的分辨率大小，后边的2.1MB表示总的内存使用数值。

* SetPass calls:之前有讲到Batches,比如说场景中有100个gameobject,它们拥有完全一样的Material,那么这100个物体很可能会被Unity里的Batching机制结合成一个Batch。所以用“Batches”来描述Unity的渲染性能是不太合适的，它只能反映出场景中需要批处理物体的数量。那么可否用“Draw calls”来描述呢？答案同样是不适合。每一个“Draw calls”是CPU发送个GPU的一个渲染请求，请求中包括渲染对象所有的顶点参数、三角面、索引值、图元个数等，这个请求并不会占用过多的消耗，真正消耗渲染资源的是在GPU得到请求指令后，把指令发送给对应物体的Shader,让Shader读取指令并通知相应的渲染通道（Pass）进行渲染操作。      
场景上有1个GameObject，希望能显示很酷炫的效果，它的Material上带有许多特定的Shader。为了实现相应的效果，Shader里或许会包含很多的Pass,每当GPU即将去运行一个Pass之前，就会产生一个“SetPass call”，因此在描述渲染性能开销上，“SetPass calls”更加有说服力。    
* Shadow casters：表示场景中有多少个可以投射阴影的物体，一般这些物体都作为场景中的光源。
* visible skinned  meshed：渲染皮肤网格的数量。
* Animations:正在播放动画的数量。

```
参考
https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html        
https://docs.unity3d.com/Manual/ProfilerRendering.html      
https://docs.unity3d.com/Manual/RenderingStatistics.html
```