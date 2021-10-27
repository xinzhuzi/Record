---
title: Unity UGUI优化第九章
date: 2020-05-11 11:41:32
top: 109
categories:
- UnityUI
tags:
- UnityUI
---

# UI 类别与介绍
* 1. IMGUI是 Unity自带的古老 UI 系统,用于工具扩展.         
     NGUI 是流行的第三方 UI 插件.        
     FairyGUI是跨平台的 UI 系统.       
     UGUI 是官方版本.UIElement 是新版本的 UI 系统.      
     UI_JS,反正我也不知道叫什么名字,就是用写网页搞出来的一个最新 UI 系统,定义就是全平台通用,目前是无法正常开发的.
    
* 2. NGUI 与 UGUI 是使用最多的商业游戏里面的 UI 系统.
     
* 3. 一般是与 Lua 进行组合使用,用于热更新.



# NGUI 与 UGUI 的类功能对比

| 基本含义 | NGUI | UGUI |  
|----|----|----|  
|DrawCall,面板,业务模块分组类|UIPanel|Canvas| 
|Renderer 渲染类(Mesh,几何图形的组装者)|MeshRenderer |CanvasRender| 
|几何图形缓存类,每个 UI 组件都有的|UIGeometry|VertexHelper|
|主摄像机|UICamera|自建或者不提供,Canvas 3种渲染模式|
|事件|UICamera|EventSystem&StandaloneInputModule|
|图片|UISprite|Image|
|图集|UIAtlas|SpriteAtlas|
|文本|UILabel|Text/TMP|
|DrawCall|UIDrawCall|BatchSorting|
|子线程处理Mesh|无|BatchGenerator|




# NGUI 与 UGUI 的一些区别

* 1. UGUI单独为了渲染 UI 写了一个CanvasRender组件,NGUI 用的原生的MeshRenderer.
    
* 2. UGUI 核心源码无法看到,4.6 可以看到,NGUI 全部都可以看到
    
* 3. 优化工具的区别,NGUI 使用的是自定义的 DCTools,PanelTools 等,UGUI 使用的是 Profiler,FrameDebug,memoryProfiler等工具,UGUI 的工具更加优秀
    
* 4. 关于顶点,颜色,UV,法线,切线,三角形顶点缓存等都是内存相关的数据.其中Color32比Color要节省一半内存,这一点 UGUI 比 NGUI(原生) 做的要优秀.
     但是目前我们已经优化了 NGUI 的 UIGeometry中的数据结构以及颜色,改换成Color32了.这个是比较大的内存优化;
    
* 5. 原生的NGUI 的UIGeometry中的数据数据结构容易造成内存碎片,它使用的是 List. UGUI 这一点相对优秀,使用了池子.
     
* 6. UGUI使用了线程合并网格,NGUI 没有使用线程.
    

# NGUI 与 UGUI 的优化对比

| 基本含义 | NGUI | UGUI |  效率 |
|----|----|----|----|
|元素布局方式|UIRect.Update 中更新|CanvasUpdateRegistry.m_LayoutRebuildQueue/Canvas.SendWillRenderCanvas| 持平|
|重建(包括布局)|UIPanel.LateUpdate轮询更新,元素过多情况会导致一些无意义遍历,即元素的 layout,material,mesh等没有变动,也经历了遍历,UIPanel.UpdateWidgets等|CanvasUpdateRegistry,Canvas.SendWillRenderCanvas,纯静态时不消耗|UGUI 优秀|
|缓存机制用法|SetActive(false),UIPanel.list记录了所有激活的UI节点,没有激活的节点不参与更新. 少量使用 Color.a = 0(此方式仍然参与 NGUI 的更新),使用 Timer + 二级缓存(长时间不使用就删除,缓存上限等算法)等.|UGUI使用 Scale =0 ,Alpha = 0,CanvasGroup组件,此方式性能较好,即不会参与更新,也不会产生新mesh,重新合批事不会有大的峰值|缓存机制是 UGUI 更优秀.|
|渲染顺序|NGUI 的渲染顺序是使用 depth,2 个相同的材质的 depth 中间不会掺杂另外一个材质,就会合批. |UGUI 的合并规则主要是 3 个参数,第一个是重叠,第二个是 material.id,第三个是 texture.id,第四个是排序位置,尽量避免重叠,尽量让重叠元素的material.id与 texture.id保持一致,尽量拆分,形成一套多个的模板.|NGUI容易操作,开发时就需要注意,优化时也需要注意,UGUI 需要优化时注意,开发时无需注意,UGUI 优秀|
|工具|NGUI 自提供工具|原生的 Frame Debugger;Profiler,是在 C++中打点获得的|UGUI 更加优秀,贴近原生|
|元素重叠|NGUI 并不是很在意重叠|UGUI 非常在意,需要关注,重叠影响合批,影响 overdraw|NGUI 省心,比较优秀|
|网格更新|NGUI 有 2 种更新方式,UIPanel.FillDrawCall 更新单个 DrawCall. UIPanel.FillAllDrawCall 更新所有的 DrawCall.|Canvas.BuildBatch 更新所有的 DrawCall (在 C++层处理,效率较高,涉及到子线程,子方法:WaitingForJob,PutGeometryJobFence,BatchRenderer.Flush),重建时会整个 canvas 重建|单个Mesh 重建 NGUI 优秀,但是内存方面或者执行效率方面,还是 UGUI 优秀,毕竟在 C++中.|
|内存|NGUI 纯粹是用户堆内存|UGUI 有部分用户堆内存,C++中的 native 内存|内存上是 UGUI 优秀.Unity 中的内存分为,Native(C++)内存,Unity的 Managed Heap(托管堆),代码文件内存(Lua),第三方内存(c++)|
|子线程|无子线程|子线程辅助处理合批等|UGUI 更加优秀|
|总结|NGUI 与 UGUI 的区别主要是在CPU 与 内存处理上面的区别比较大|UGUI 目前已经全方面胜过 NGUI 了|UGUI 更加优秀|


     
* 2. UGUI 网格/material/layout重建,Canvas.SendWillRenderCanvas(这个函数的意思是在渲染前设置一些渲染数据,渲染状态,然后提交到渲染器.
     布局更新,渲染更新 2 种情况,注册到一个容器内,容器内有变更才会更新)
     
* 3. 缓存机制用法:
     NGUI使用 SetActive(false),UIPanel.list记录了所有激活的UI节点,没有激活的节点不参与更新.
     少量使用 Color.a = 0(此方式仍然参与 NGUI 的更新),使用 Timer + 二级缓存(长时间不使用就删除,缓存上限等算法)等. SetActive 此方法有性能缺陷.
     UGUI使用 Scale =0 ,Alpha Group = 0,此方式性能较好,即不会参与更新,也不会产生 mesh.
     缓存机制是 UGUI 更优秀.
     
* 4. 渲染顺序:NGUI 的渲染顺序是使用 depth,2 个相同的材质的 depth 中间不会掺杂另外一个材质,就会合批.                
     UGUI 的合并规则主要是 3 个参数,第一个是重叠,第二个是 material.id,第三个是 texture.id,第四个是排序位置
     尽量避免重叠,尽量让重叠元素的material.id与 texture.id保持一致,尽量拆分,形成一套多个的模板.
     
* 5. 因为 UGUI 的合批规则太过复杂,当我们制作 UGUI 的时候,需要使用 profiler 以及 FrameDebugger 来测试 DC(Draw Mesh),
     在 DC 越少越好的规则下去试验 UI 节点重叠机制,进行优化即可.
     NGUI有明确的工作量,优化方式比较人性化.效果持平.
     
* 6. 特殊图片组合在一起,是可能重叠的,需要使用 Scene 窗口的 Wireframe 模式去查看线条是否重叠,确保不影响目前的效果前提下,不重叠 UI 元素
     
* 7. 网格更新机制:
     NGUI,UIPanel.LateUpdate 两种更新方式,UIPanel.FillDrawCall 更新单个 DrawCall. UIPanel.FillAllDrawCall 更新所有的 DrawCall.
     优化方式:控制减少FillAllDrawCall的调用,拆分 UIPanel.
     UGUI,Canvas.BuildBatch 更新所有的 DrawCall (在 C++层处理,效率较高,涉及到子线程,子方法:WaitingForJob,PutGeometryJobFence,BatchRenderer.Flush)
     优化方式:Canvas中的元素过多情况下,必须拆分 Canvas,动静分离.
     在一个 UIPanel 中的网格重建,有可能只会重建单个 mesh,不会重建所有 mesh,但是在一个 Canvas 中,只要重建,就是重建所有的 mesh
     这点 NGUI 比较优秀.
     
* 8. DC 优化:
     在 UGUI 中,Z 值!=0,导致 DC 暴涨;UI 元素中的Sprite 为 null,Color.a =0,在屏幕外,不会降低 UGUI 的 DC 的.
     在 UGUI 中不规则的重叠 UI,会导致 DC 暴涨.UGUI 中的图集中,如果图片过多,或者 alpha 通道问题,会导致多张图集 Atlas,而不是一张 Atlas,需要注意.
     在 NGUI 中 Depth 的不合理分布;UI 元素中的 Scale = 0 是不会降低 NGUI 的 DC 的;

* 9. 降低界面的渲染开销:检测渲染开销(profiler,FrameDebugger),DrawCall 有多少,Mesh 的内存大小(创建销毁策略),Overdraw(UI 重叠造成重复绘制)
     策略:动静分离;降低每帧更新变为 2 帧或者 3 帧更新;
     在 NGUI 中元素的隐藏显示(SetActive),会导致调用 FillAllDrawCalls 全UIPanel更新网格,
     即影响了现有的 DC 合并,在不影响 DC 合并的情况下隐藏显示 UI 元素,如使用 Scale = 0,添加空白图片/设置alpha来模拟删除图片等等.
     在 NGUI 中,Static,如果当前 UIPanel 都是静态的,可以勾选此项,减少更新位置的部分开销.
     在 NGUI 中,Visible 选项勾选,不需要重新计算 UIPanel 的Mesh包围盒
     在 UGUI 中,不能频繁的设置 UI 元素的布局,设置元素的布局会导致当前 canvas 更新
     

* 10. 高效处理大量 HUD 元素(血条,名字,伤害提示)在 UGUI 中性能比较好.
      元素更新开销:位置更新,布局更新,Layout
      网格更新开销:Mesh,内存,Batch,合批(UGUI 因为使用的 C++层,性能好于 NGUI),能小就小.
      渲染开销:Overdraw
      使用缓存机制,转图片字,隔帧更新,控制总量,能小就小,能少就少,拆分 UIPanel/Canvas.
      
* 11. 总结:NGUI 与 UGUI 的性能对比
      DrawCall 管理:NGUI 优秀
      网格更新机制友好度 : NGUI 优秀
      动态 HUD 界面的网格更新机制:UGUI 优秀
      堆内存控制:UGUI 优秀(C++源码)
      元素位置更新:相差不大.
      渲染开销:相差不大.
      


# 源码窥探

![SetActive](SetActive.png)


![UGUI类结构](UGUI类结构.png)

![ReBuild](ReBuild.png)

![UI源码1](UI源码1.png)


![UI源码2](UI源码2.png)


      
# 文章引用 
* 1. 何冠峰(文西) 的 UGUI 原理和优化 PPT
* 2. https://www.jianshu.com/p/9bd461de19a7  博客
* 3. UWA
* 4. https://blog.uwa4d.com/archives/video_UI.html
* 5. https://blog.uwa4d.com/archives/1875.html
* 6. Unity 4.6f 源码
     




