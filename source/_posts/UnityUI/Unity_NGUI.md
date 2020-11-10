---
title: Unity NGUI
date: 2020-05-08 11:41:32
top: 1
categories:
- UI
tags:
- UI
---


# NGUI 的介绍

* 1: [UnityAssetStore](https://assetstore.unity.com/packages/tools/gui/ngui-next-gen-ui-2413)   
[NGUI](http://www.tasharen.com/)    
[NGUI官方视频](https://www.youtube.com/playlist?list=PLI_jd5nXafTbBrubTmgbodCtWY6BhVxtP)    
[NGUI 文档](http://tasharen.com/ngui/docs/index.html)
* 2: UI 的核心三要素,图片,字体,事件
* 3:参考
```
UWA  https://blog.uwa4d.com/archives/video_UI.html
NGUI性能优化技巧（上） https://v.qq.com/x/page/j0336jncwn5.html
NGUI性能优化技巧（下） https://v.qq.com/x/page/r0342tl5e47.html
NGUI内存优化技巧      https://blog.uwa4d.com/archives/USparkle_NGUI.html
NGUI核心类讲解        https://zhuanlan.zhihu.com/p/102544809
```
# NGUI 核心类 

## Event System (UICamera)
* 1:绘制UI图像的摄像机;事件触发默认在Update中更新记录,一般使用碰撞盒来检测;
* 2:涉及的终端输入有鼠标(Input.GetMouseButton),键盘(Input.GetMouseButton),触摸(Input.GetTouch),手柄;得到屏幕像素点;记录触发点前前后后的一些数据,比如点击点,拖拽时的增量(单位时间内拖动的像素点);
* 3:原理是物理检测,即从当你在屏幕上面触发一个事件,摄像机通过屏幕像素点发送一条射线,射线发送出去之后,使用
```
    Camera-->public Ray ScreenPointToRay(Vector3 pos),
    Physics-->public static bool Raycast(
        Ray ray,
        out RaycastHit hitInfo,
        [DefaultValue("Mathf.Infinity")] float maxDistance,
        [DefaultValue("DefaultRaycastLayers")] int layerMask,
        [DefaultValue("QueryTriggerInteraction.UseGlobal")] QueryTriggerInteraction queryTriggerInteraction)
```
* 4:事件触发核心 API;
```
    World_3D使用的是 Camera.ScreenPointToRay + Physics.Raycast QueryTriggerInteraction.Ignore,不回调触发次数 + Distance 距离;
    UI_3D使用的是 Camera.ScreenPointToRay + Physics.RaycastNonAlloc QueryTriggerInteraction.Collide回调触发次数  + Distance 距离;
    World_2D使用的是 Plane.Raycast + Physics2D.OverlapPoint + Collider2D;
    UI_2D使用的是 Plane.Raycast+ Physics2D.OverlapPointNonAlloc + Collider2D;
```

* 5:各种事件,触摸,按压,拖动,悬浮;任何;OnHover,OnPress,OnSelect,OnClick,OnDoubleClick(1/4s),OnDragStart,OnDrag,OnDragOver,OnDragOut,OnDragEnd,OnPan,OnKey;可以通过自己的 Event 发送,也有通过代理事件Delegate发送出去:
```
    GameObject-->public extern void SendMessage(string methodName, [DefaultValue("null")] object value, [DefaultValue("SendMessageOptions.RequireReceiver")] SendMessageOptions options); 
    也就是说哪个 GameObject 添加了 UIEventListener ,就会被接受发送事件,通过Collider的某些 flag 值判断,从而达到你点击可以触发业务的效果
```



## 渲染器

* 1: 当只有渲染器MeshRenderer存在时,Unity内才可能出现正常的可视化界面,否则无法出现正常的可视化图像.
* 2: 渲染器需要的材料,必须有顶点,顶点索引,material,shader,color,否则无法形成可视化图像.
* 2: 网格面是由三角形组成,mesh.vertices就是用于存储三角形顶点坐标,当顶点数超过3个的时候,我们连接点的顺序不同,就会绘制出不同形状的图形,必须要获得我们想要的连接点的顺序.数组mesh.triangles就是用来记录连接三角形的顺序的.由于每绘制一个三角形需要知道其三个顶点的顺序,那么绘制n个三角形就需要知道3*n个点的顺序.即数组mesh.triangles的长度是3的倍数。
* 3: 三个可以缓存的值;顶点坐标,三角形顶点顺序(也就是索引),顶点数量
* 4: 最基本浅显的道理,一旦上面的某一个值改变了,这个渲染器就会重新渲染,这 3 个值就需要重新缓存.一切的优化都是围绕这个做文章的.
* 5: VBO (Vertex buffer object)是显卡存储空间里一块缓存区BUFFER，用于存储和顶点以及其属性相关的信息（顶点信息，颜色信息，法线信息，纹理坐标信息和索引信息等）,Mesh 的另一种叫法;


## UIRect
* 1: UIRect 规定了一个 UI 矩形框以及相对锚点,程序员可以设置相对锚点,整个类的代码就是确定这个 UI 相对于父 UI 的位置,锚点位置,锚点的边等
* 2: 位置更新,维护父子关系,确定自身是否发生改变
* 3: 对于频繁开关的Widget,可以用 alpha 设置为 0.01与 1 来进行优化,具体看实际效果,或者设置localPosition将控件移出或移入视野;
* 4: 这个类可以将屏幕 4 个角,某一个角设置为 0 点位置,这个在使用的时候非常灵活.

## UIGeometry
* 1: 核心数据类, 记录了顶点,UV(UV这里是指u,v纹理贴图坐标的简称(它和空间模型的X, Y, Z轴是类似的),它定义了图片上每个点的位置的信息.),colors,normal,这个类是作为 UIWidget 的缓存数据类而使用的
* 2: 在 Scene 视图中,使用 Wireframe 模式,在Game 视图中点击 Stats 查看.我们先对三角形以及顶点数进行大概的计算.1 个字母 'A',1 个汉字 '爱' 展示在 UI 上面,有多少个三角形,有多少个顶点,可以确定的是 2 个三角形,4 个顶点,这个计算方式是没有问题的,除非这个字体以及 mat 有其他特殊操作.排除其他因素影响,可以直接计算出来所有的字符的三角形以及顶点数据. 我们简单的计算一下 '爱你,爱世人' 有多少个三角形,以及顶点数据:12 个三角形,24 个顶点.图片的三角形以及顶点数类似于 Font 的基础计算方式,然后一张图片不可能是非常基础的设置,在特殊的情况下设置图片,会增加这个图片的三角形与顶点数,而且随随便便就可以增加很多,比如一图片多平铺,环形切割,只要不是正常的四个角一张图,就会增加,这是与 UI 业务的基础设置,大家对这个需要有个基本的认知.如果我们对这个东西不敏感,做 UI 业务其实是特别失败的,UI 崽的称呼也不那么称职.
* 3: 主要做了什么? 是根据提供的矩阵变换转成顶点数据,再将顶点数据填充指定的缓冲区
* 4: 数据结构的采用,原生的是用 List ,List 的原理,是个数组,初始化只有 4 个大小的元素空间,扩展的时候是 2 倍,当空间不足的时候,肯定要重新创建数组,并将所有数据进行移动,这样非常耗费性能,Mesh 的数据是大量的,明显 List 做这个事情有点不适合.
* 5: 那么每一个 UIWidget 都有一个 UIGeometry,我们要对这个 UIGeometry 做出缓存池,当销毁时放入缓存池,生成时从缓存池子上面拿到,这样就避免的内存的销毁创建的很大开销.

## UIWidget
* 1: UIWidget 适配当前 Wdiget 的矩形,记录矩形的一些数据. depth 深度控制渲染顺序,从最低到最高,并具有排序功能.子控件有更改,会会重新构建缓冲区.对小部件的统筹管理.包含一个UIGeometry的对象.UIWidget 自身有一个 UIDrawCall 的引用(这个地方有复杂的引用关系,但是只要我都打断这个引用,就没关系),这个 DC 不是 UIWidget 创建的,是 UIPanel 里面赋值的引用.
* 2: 数据填充;由UIPanel 类中调用 UIWidget的UpdateGeometry方法(这个时候的 UIWidget 因为子控件变动,位置,mat,透明度等),再去调用子控件的OnFill方法 去填充顶点,颜色,法线等数据,然后由UIPanel去填充UIGeometry对象的缓冲区.这个地方的缓存,只要当前控件上面的位置,mat,透明度等不变动,就不会重新去组装或者创建这些数据.
* 3: UIWidget 的子控件, UILabel,UISprite,UITexture,它们重写OnFill方法.
* 4: 不建议动态修改 depth,与移动位置,与修改颜色,以上操作都会造成开销.尤其是 depth ,修改一次会 重新生成panel的所有drawCalls.


## UIDrawCall
* 1: 记录所有显示的,不显示的 UIDrawCall.单个 UIDrawCall 记录UI簇,顶点,三角形,法线,UV,Color,Mat,shader,Mesh,MeshFilter,MeshRenderer 等
* 2: 记录 RenderQueue 3000 打底的渲染顺序,一直升,是从 UIPanel 传递过来的. 创建 Mat,Shader 并初始化设置,UIDrawCall.UpdateGeometry方法中,创建 Mesh 并设置顶点,三角形,法线,UV,Color等数据,然后将这个 mesh 传递给渲染器MeshRenderer,从而正式在UI上面产生了可视化画面,实现方式为动态合批.
* 3: 更新机制,在一个 DC 的范围内(矩形内),运动,不涉及到其他的 DC,则不会导致所有 DC 进行更新,只会更新一个,例如slider 运动,此时的 mesh 是不断重建的;重建更新即更新所有的DC,开销极大,属于重大级别问题,需要进行优化,即所有的 DC 增加或者减少,会导致重建所有 DC,重建时会将之前的 DC 进行销毁(这点可以做代码优化).
* 4: Mesh 创建,三角形索引缓冲区列表,都需要大量的优化
* 5: NGUI的设计原则:一个UIDrawCall,就是一次Draw Call.多个UIWidget共享一个UIDrawCall.一个 DC 消耗不高,真正高的是每次渲染器切换渲染状态是,就要将所有的数据(渲染状态,顶点索引,三角形索引,UI,纹理,材质,shader)重新重置一遍,非常消耗 CPU 性能.核心技术即完成一次动态合批,一个 DC 就是一个 batch.



## UIPanel
* 1: 记录所有 UIPanel,管理当前UIPanel的所有UIWidget,管理当前UIPanel的所有UIDrawCall; 一堆 flag 值,裁剪,映射,二次数据缓存,深度,渲染顺序,重建,透明度等
* 2: LateUpdate里面,更新所有 UIWidget 的子控件,填充数据,创建UIDrawCall并调用UIDrawCall.UpdateGeometry创建一系列数据,可视化对象等;UIWidget从一系列选择里面拿到对应的UIDrawCall,并从UIDrawCall中拿到顶点,UV,颜色等去设置UIGeometry的缓存.这个地方,重建操作及其耗费性能,一般情况下不会触发重建,一般情况下采取,更新某一个UIPanel的策略;Render Q值变了,则会更新所有的UIDrawCall的所有数据.
* 3: Depth 权重比 UIWidget 的 Depth 权重要高,一个UIPanel同属一个 UI 簇,一个 UI 簇至少有一个 DrawCall.关于 Depth 的合批规则:一般情况下从 0 开始,如果一个字体与图集的 mat 不一样,2 个物体同时存在,则最少是 2 个 DC,这 2 个 DC 是因为 Mesh,mat,shader 都不一样.有的情况下多于 2 个,是因为 mat 互相穿插影响,导致无法合批,造成了多次 DC.基本规则合批是同属于一个 mat 的Depth,不可以有其他 mat 的 depth 穿插进来.
* 4: Render Q 是渲染顺序,3000 打底.一直向上生长
* 5: 如果一个 UIPanel 下面所有的 UI 控件都不会被移动,勾选 Static 设置为静态的,这样就会达到优化的目的,例如循环遍历减低,不会因为 Depth 重建,忽略位置,旋转,缩放等.
* 6: NGUI-->Open-->Panel Tool 会展示一个 Panel 下面的数据,DC,三角形等.辅助优化功能
* 7: UIPanel 里面,在仅仅考虑其他组件,不再用其他UIPanel去影响本 UIPanel 的时候.在考虑子控件 Depth 的时候,需要考虑图集和字体是 2 种不同的 mat,这 2 中 mat 不应该互相穿插出现,而应该做好合理的分层设置,让图集的Depth相聚近一些,字体的 Depth 相聚近一点,这样最优解是 2 个 DC,因为所有字体都会合批成一个 mat,一个 DC,所有图片都会合批成一个 mat,一个 DC;如果有特殊情况,再合批最优的情况下不能再合了,就多出一个 mat 或者 DC.
* 8: 双摄需要将 UI 相机的 ClearFlags 设置为 DepthOnly,不是双摄,千万不要这么设置



# NGUI 辅助功能
* 1: 图集
* 2: shader (同一界面加三层 UIScrollview 会挂,shader 里面只做了三层切割)
* 3: 字体
* 4: 宏定义值 UIDrawCall.cs 类 SHOW_HIDDEN_OBJECTS,取消后直接在Hierarchy 可以看到 DC,在 DC 这个地方的 Inspector 检视面板上,可以看到渲染的一系列东西
* 5: NGUI-->Open-->Panel Tool 查看所有 UIPanel;NGUI-->Open-->Draw Call Tool       任何一个 Panel 的 Inspector 上面都有一个 Show Draw Calls ,显示的是所有 DC 以及 Mat,Render Q;
* 6: 粒子特效插值,重新排列渲染顺序,只需要在 Render Q 上面+1 等即可,即,让粒子特效的Renderer.materials[i].renderQueue + 1达到比UIWidget.drawCall.renderQueue 大的情况即可,如果将粒子的大小设定在某个 UIWdiget 的范围内,则需要进行 shader 裁剪.

# 优化

## 从根本上面谈优化策略
* 1: 上述脚本介绍,让我们从根本上面知道了到底哪些东西占用内存,哪些东西是不合理存在的,又有什么东西是需要严格关注的
* 2: NGUI 会产生的问题,数据量(网格数据)过大,DC 过多,Panel 过多,从这 3 方面去优化.
* 3: 数据的刷新,UI 的移动,切割,变换,都可能造成重新渲染.

## 优化标准以及技巧
* 1: CPU 耗时为 0-5ms;UIPanel.LateUpdate:0.4-8.4ms,每10K 帧的堆内存分配建议<20MB
* 2: CPU 耗时函数 UICamera.Update(),UIRect.Update(),UIPanel.LateUpdate(),UIRect.Start();允许不在战斗时,有瞬间高峰值,但必须在之后平稳,不允许在战斗时,有太大的波动,峰值,以及高消耗.可以使用 UWA 进行性能检测.
* 3: 在 UIDrawCall 里面的 MeshRenderer 被设置好数据之后,还要必须调用 Camera.Render() 的方法才可以正常显示出来,猜测是Camera每一帧自动调用这个函数.在 Profiler 的 CPU 模块下,渲染的调用堆栈方法,ms 数,占用百分比等
```
PlayerLoop
    Camera.Render
        Drawing
            Render.TransparentGeometry
                RenderForwardAlpha.Prepare
                RenderForwardAlpha.Render
                    RenderForwardAlpha.RenderLoopJob
                        Render.Mesh --渲染数据
                    WaitForJobGroupID
                        RenderForwardAlpha.Sort
            Render.Prepare
            Render.OpaqueGeometry
```
* 4: Anchors 执行模式不要放在 Update 里面
* 5: DC 放在 40 个之下,多了之后需要界面向下进行分级处理,使用 SetActive(false) 进行处理
* 6: 顶点,UV,Color,Normal,贴图等数据量过大时,会增加渲染压力,所以从根本上面,去解决这个问题是一个优化方式
* 7: UIPanel 的 Static 是否为静态界面,一般情况下背景界面都是静态的,可以选择点上去的.
* 8: Depth 的排列顺序,会导致 mat 的穿插,进而影响动态合批,增加多个 draw call.
* 9: 重建,一般情况下,是不推荐重建操作,要尽可能不去触发重建操作.
* 10: UI动态变形,三角形增多.Mesh太大则分出一个 UIPanel
* 11: UI重叠,OverDraw 增多.OverDraw的含义
* 12: 对于频繁变动的元素,其所在的 UIDrawCall 面片数越少越好,顶点数据,三角形,Mesh 数据越小越好
* 13: 动静分离;动画,切割,重叠,等放在一个里面,静态不动的放在一个里面.
* 14: profiling 查看开销,UWA,UPR 等性能检测工具
* 15: 


## 优化内存
* 1: UIGeometry的verts、uvs、cols、mRtpVerts缓冲池,UIGeometry的 List 修改为更好表现的 BetterList;UIWidget.OnFill的时候确定了需要顶点数量的情况下从缓存池申请,当UIWidget.OnDestroy的时候放回缓存池,回收的时候以顶点数量从小到大插入，申请的时候找到第一个满足需求顶点数量的。为了给来回关闭打开 UI 降低内存压力,提高性能.
* 2: 从原来的顶点索引的生成算法来看，假设有一个顶点索引缓冲长度为9000，另一个有1w，那么这两个缓冲的前9000个索引的值是一样的，而且Mesh.triangles需要顶点索引缓冲的数量为顶点缓冲数量的三倍，所以最好是有个能够能动态调整Array.Length但Array的元素可以不发生变化的办法。UIDrawCall.GenerateCachedIndexBuffer获取 mesh 的三角形索引缓冲区的数组
* 3: 插入前充分设置List.capacity来减少GC Alloc,在UIDrawCall 的 verts,norms,tans,uvs,uv2,cols等提前设置Capacity值,当这个值扩容时,会重新申请一段数组内存,比如修改扩容方式从 2 倍扩容方式改成 4 倍扩容,就达到优化效果.
* 4: 绝招优化,将所有NGUI 的 List 替换为 BetterList,2 个都是数组,并且将创建出来的BetterList放入池子里面,原因:List 是以 2 倍扩容,而BetterList是以 Length << 1 来扩容的,池子是为了避免一直创建数组和销毁数组,进一步优化顶点数据,UV,颜色,发现,三角形等内存;
* 内存优化就是所谓的算法与数据结构的优化.

# 遗留问题
* 1: 动态字体与静态字体的区别?对 UI 的性能影响?(不建议使用动态字体,只有在大量文字的情况下使用动态字体)
* 2: 复杂场景下,多个人物的血条,人物动态,血条动态?(尽可能使用分层策略减小压力)
* 3: 复杂动画导致的 UI 重建? 分层策略

