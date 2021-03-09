---
title: Unity OverDraw优化
date: 2020-05-08 11:41:32
top: 524
categories:
- Unity优化
tags:
- Unity优化
---

# 渲染 Statistics 窗口

* 1. Game 视图__的右上角有一个 Stats__ 按钮。按下此按钮时将显示一个覆盖窗口，其中显示实时渲染统计信息，这对于优化性能非常有用。显示的具体统计信息根据构建目标而有所不同。UnityEditor.UnityStats 记录了所有的数据.

* 2. 在 UnityEditor.UnityStats 类中:

|属性|作用|
|---|---|
|batches|“批处理 (Batching)”可让引擎尝试将多个对象的渲染组合到一个内存块中以便减少由于资源切换而导致的 CPU 开销。|
|drawCalls|---|
|dynamicBatchedDrawCalls|---|
|staticBatchedDrawCalls|---|
|instancedBatchedDrawCalls|---|
|dynamicBatches|---|
|staticBatches|---|
|instancedBatches|---|
|setPassCalls|---|
|triangles|---|
|vertices|---|
|trianglesLong|---|
|verticesLong|---|
|shadowCasters|---|
|renderTextureChanges|---|
|frameTime|---|
|renderTime|---|
|audioLevel|---|
|audioClippingAmount|---|
|audioDSPLoad|---|
|audioStreamLoad|---|
|renderTextureCount|---|
|renderTextureBytes|---|
|usedTextureMemorySize|---|
|usedTextureCount|---|
|screenRes|---|
|screenBytes|---|
|vboTotal|---|
|vboTotalBytes|---|
|vboUploads|---|
|vboUploadBytes|---|
|ibUploads|---|
|ibUploadBytes|---|
|visibleSkinnedMeshes|---|
|visibleAnimations|---|


* 3. Statistics窗口

|属性|作用|
|-----|-----|
|FPS|Time per frame andFPS,frames per seconds表示引擎处理和渲染一个游戏帧所花费的时间,该数字主要受到场景中渲染物体数量和 GPU性能的影响，FPS数值越高，游戏场景的动画显示会更加平滑和流畅。一般来说，超过30FPS的画面人眼不会感觉到卡，由于视觉残留的特性，光在视网膜上停止总用后人眼还会保持1/24秒左右的时间，因此游戏画面每秒帧数至少要保证在30以上。另外，Unity中的FPS数值仅包括此游戏Scene里更新和渲染的帧，编辑器中绘制的Scene和其它监视窗口的进程不包括在内。|
|CPU|获取到当前占用CPU进行计算的时间绝对值，或时间点，如果Unity主进程处于挂断或休眠状态时，CPU time将会保持不变。|
|Render thread|GPU渲染线程处理图像所花费的时间，具体数值由GPU性能来决定|
|Batches|即Batched Draw Calls,是Unity内置的Draw Call Batching技术。首先解释下什么叫做“Draw call”，CPU每次通知GPU发出一个glDrawElements（OpenGl中的图元渲染函数)或者 DrawIndexedPrimitive（DirectX中的顶点绘制方法）的过程称为一次Draw call,一般来说，引擎每对一个物体进行一次DrawCall，就会产生一个Batch,这个Batch里包含着该物体所有的网格和顶点数据，当渲染另一个相同的物体时，引擎会直接调用Batch里的信息，将相关顶点数据直接送到GPU,从而让渲染过程更加高效，即Batching技术是将所有材质相近的物体进行合并渲染。“批处理”的引擎试图将多个渲染对象合并到一个内存块中, 以减少CPU开销。|
|Saved by batching|将几个批处理进行合并。为了获得好的批处理效果，你应该尽可能的让不同物体共用同一材质。改变渲染状态将批次分解成组织相同的状态.|
|Verts|摄像机视野(field of view)内渲染的顶点总数。  把 field of view调大到 179 过程中都看不到这个cube，stats面板才不会统计，GPU才不会渲染|
|Tris|摄像机视野(field of view)内渲染的的三角面总数量。  把 field of view调大到 179 过程中都看不到这个cube，stats面板才不会统计，GPU才不会渲染|
|Screen|获当前Game屏幕的分辨率大小，后边的2.1MB表示总的内存使用数值。|
|SetPass calls|CPU准备数据,CPU 向 GPU发送指令,渲染 API 调用DrawCall,DrawCall 里面产生 Batch(能合批的就合批了,缓存此 Batch,等下次调用直接发送到 GPU,合并渲染),此时才进入真正消耗渲染资源的一个过程, 将渲染对象的顶点,三角面,索引值,图元个数等传给shader,shader 读取指令并通知相应的渲染通道(pass)进行渲染操作.当 GPU 即将去运行一个 Pass 之前,就会产生一个 SetPass calls.|
|Shadow casters|表示场景中有多少个可以投射阴影的物体，一般这些物体都作为场景中的光源。|
|visible skinned  meshed|渲染皮肤网格的数量。|
|Animations|正在播放动画的数量|

* 4. Rendering Profiler module,在Profiler中,Rendering 中其中参数表示的含义 :

|属性|作用|
|-----|-----|
|__SetPass Calls__|__-----从上向下,从左向右,横排解释其中含义.总统计-----__|
|SetPass Calls|Unity切换通过哪个着色器进行渲染的次数，该次数用于在帧期间渲染GameObject。着色器可能包含多个着色器通道，每个通道都会以不同的方式渲染场景中的GameObject。|
|Draw Calls|在一帧期间发出的绘制调用总数。 Unity问题在将GameObjects呈现到屏幕时进行绘制调用。此数字包括未批处理的绘图调用以及动态和静态批处理的绘图调用。|
|Total Batches|一帧中Unity处理的批次总数。此数字包括静态和动态批次。|
|Tris|在一帧期间Unity处理的所有三角形数量。|
|Verts|帧期间Unity处理的所有顶点数。|
|__(Dynamic Batching)__|__-----从上向下,从左向右,横排解释其中含义,本节包含有关动态批处理的统计信息 。-----__|
|Batched Draw Calls|合并到动态批处理中的Unity的绘制调用数。|
|Batches|框架期间Unity处理的动态批次数。|
|Tris|动态批次中包含的GameObjects中的三角形数量。|
|Verts|动态批处理中包含的GameObjects中的顶点数。|
|__(Static Batching)__	|__-----从上向下,从左向右,横排解释其中含义,本节包含有关静态批处理的统计信息-----__|
|Batched Draw Calls|合并为静态批处理的Unity的绘制调用数。|
|Batches|在一帧期间Unity处理的静态批次数。|
|Tris|静态批处理中包含的GameObjects中的三角形数量。|
|Verts|静态批处理中包含的GameObjects中的顶点数。|
| __(Instancing)__	|__-----从上向下,从左向右,横排解释其中含义,本节包含有关GPU instancing的统计信息。-----__|
|Batched Draw Calls|合并到实例批处理中的Unity的绘制调用数。|
|Batches|Unity在帧期间处理以渲染实例GameObjects的批处理数。|
|Tris|实例化GameObject中的三角形数量。|
|Verts|实例化GameObject中的顶点数。|
| __(杂项)__	|__-----从上向下,杂项信息。-----__|
|Used Textures	|纹理数量 帧期间使用的Unity以及使用的纹理的内存量|
|RenderTextures	|帧期间使用的RenderTextures 的数量以及RenderTextures使用的内存量。|
|RenderTextures Switches	|Unity在帧期间将一个或多个RenderTextures设置为渲染目标的次数。|
|Screen|屏幕的分辨率及其使用的内存量。|
|VRAM Usage	|Unity分配了多少视频内存来渲染帧。|
|VBO Total	|GPU上的计算缓冲区数。|
|VB Uploads	|CPU在一个帧中上载到GPU的几何数量(mesh)。这代表顶点/法线/ texcoord数据。 GPU上可能已经有一些几何图形。此统计信息仅包括Unity在框架中传输的几何|
|IB Uploads	|CPU在一个帧中上载到GPU的几何数量。这表示三角形索引数据。 GPU上可能已经有一些几何图形。此统计信息仅包括Unity在框架中传输的几何。|
|Shadow Casters	|在框架中投射阴影的GameObject的数量。如果GameObject投射多个阴影（因为有多个灯光将其点亮），则它投射的每个阴影只有一个条目|




