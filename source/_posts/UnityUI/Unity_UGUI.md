---
title: Unity UGUI优化
date: 2020-05-11 11:41:32
top: 2
categories:
- UnityUI
tags:
- UnityUI
---

# UGUI 介绍

* 1 Unity 原生的 UI 系统,与 NGUI 的作者是一个,但是 UGUI 有官方的加持,意思就是,UGUI 的有些代码放在了 C++ 层管理,而 NGUI 是没有代码放在 C++ 层管理的.
* 2 UGUI,NGUI都属于游戏运行时展示的 UI; UIWidgets 属于为 APP 设计的 UI ,用于APP 展示,和游戏关系不太大;FGUI 在 Unity,Coco,UE游戏引擎上面都可以使用.
* 3 编辑器 UI,意思就是只在 Unity 编辑器里面搭建的UI,用于帮助工具,方便使用而创建的;有一套 GUI,已逐渐不再使用,就是在OnGui()函数里写的那些东西; IMGUI 则替代了GUI原来的作用：用于游戏调试和自定义Inspector面板,目前都使用 IMGUI;UI Element是 2019 版最新的 UI 系统,替代 IMGUI 而存在的;
* 4 UGUI,NGUI,UI Element,IMGUI,GUI,UIWidgets,FGUI都可以使用,其中UGUI,NGUI,FGUI是一类,UI Element,IMGUI,GUI是一类,UIWidgets是一类;艹,耍我们就跟耍猴子一样,这么多 UI,UI 仔都不一定能全部了解.
* 5 参考 https://gameinstitute.qq.com/community/detail/112745   https://v.qq.com/x/page/l0329fvbrfn.html 
* 6 UGUI仓库 https://bitbucket.org/Unity-Technologies/ui   https://github.com/Unity-Technologies/uGUI

# RectTransform

* 1 Pivot 轴心的概念,在RectTransform的 Inspector 展示的效果里面,是一个小圆,在某一点上面的一个蓝色的圆,轴心是相对于当前控件本身的位置,轴心的位置会影响旋转、大小调整或缩放的结果.一般情况下,轴心是在矩形框内部的;但是也有在外部的情况,这种情况不好控制,一般是旋转使用.矩形旋转围绕的轴心点的位置，定义为矩形本身大小的一个比例。0,0 相当于左下角，而 1,1 相当于右上角。
* 2 Anchors 锚点的概念,在RectTransform的 Inspector 展示的效果里面,是 4 个花瓣,4 个花瓣可以分开,也就是说,锚点有 4 个,锚点是相对于当前控件的父控件来描述其相对位置的.用来解决,父控件进行的变换,对子控件的影响,当父控件进行旋转、大小调整或缩放,子控件也相应的进行旋转、大小调整或缩放.

# UGUI 的 C++ 层
* 1 有些控件表面上是属于 UGUI 的,但是在 UGUI 的源码里面又找不到其核心代码,只是在 C# 层暴漏了接口.
* 2 Canvas.cs CanvasGroup.cs CanvasRender.cs 在 UGUI 的源码里面都看不见其核心代码,Canvas里面的方法可能会在子线程中执行

# UI整体介绍

* 1 可视化 UI : Text(文本),Image(图像),Raw Image(原始图像),Mask(遮罩);
* 2 事件交互 UI : 按钮 (Button),开关 (Toggle),开关组 (Toggle Group),滑动条 (Slider),滚动条 (Scrollbar),下拉选单 (Dropdown),输入字段 (Input Field),滚动矩形/滚动视图 (Scroll Rect/Scroll View);
* 3 自动布局:布局元素,Minimum width(最小宽度);Minimum height(最小高度);Preferred width(首选/偏好的宽度);Preferred height(首选/偏好的高度);Flexible width(灵活/最大的宽度);Flexible height(灵活/最大的高度); 布局组中的布局元素大小设置的基本原则如下:1)首先分配最小大小。2)如果有足够的可用空间，则分配偏好大小。3)如果有额外的可用空间，则分配灵活大小。布局元素组件(Layout Element),内容大小适配器 (Content Size Fitter),宽高比适配器 (Aspect Ratio Fitter)
* 4 富文本等等

# UGUI核心类

## EventSystem 簇
* 1: 这个时间系统,是一些列类组合而成的,复杂度大于 NGUI; EventTriggerType,EventData,InputModules,Raycasters
* 2: EventSystem 的核心原理是使用 Camera.ScreenPointToRay ,Physics.Raycast(这个地方用了反射) 等,创建 RaycastHit/RaycastHit2D,最终生成RaycastResult发送出去;
* 3: 事件也是从 Input 类里面拿到的;包装层数很多,包装了很多东西供上层使用.

## RectTransform 
* 1: 瞄点,轴心,坐标值,旋转值,缩放值
* 2: 在 UnityEngine 命名空间内,看不到源码
* 3: 一切布局系统的基础,没有这个就没有布局系统,功能上面类似于 UIRect


## CanvasRenderer
* 1: CanvasRenderer类比 NGUI 的.
* 2: 需要原材料 mat,颜色,mesh,透明度等.
* 3: 当只有渲染器CanvasRenderer存在时,Unity内才可能出现正常的可视化界面,否则无法出现正常的可视化图像.
* 4: VBO (Vertex buffer object)是显卡存储空间里一块缓存区BUFFER，用于存储和顶点以及其属性相关的信息（顶点信息，颜色信息，法线信息，纹理坐标信息和索引信息等）,Mesh 的另一种叫法;如果出现的 xxxVBO,要明确这是在改变顶点数据.
* 5: AddUIVertexStream 此类可以直接添加 UI 的顶点数据流.此类比MeshRenderer要更加适合 UI.
* 6: 单独为 UI 写了一个类似于MeshRenderer的渲染器,而不是用的MeshRenderer,这种可能性更大;

## VertexHelper
* 1: 这个类记录了,位置,顶点,UI,颜色,三角形,三角形排列顺序.类似于 NGUI 的UIGeometry类
* 2: 这个使用的是原生的 List;ObjectPool类似于 NGUI 的 BetterList ,单独为 Mesh 做的优化类,VertexHelper使用的数据结构类就是ObjectPool;使用栈的模型,因为整个 UI 树就是一个栈的模型.

## Graphic 

* 1: Mesh 数据组装类,类比 NGUI 的 UIWidget,是所有 Mesh 的原材料类;
* 2: 这个类将 Mesh 数据塞给 CanvasRenderer;
* 3: Text-->MaskableGraphic-->Graphic-->UIBehaviour-->MonoBehaviour
* 4: Image-->MaskableGraphic-->Graphic-->UIBehaviour-->MonoBehaviour


## Canvas

* 1: Canvas 类比 NGUI 的 panel;三种模式,Screen Space - Overlay：不需要指定UI相机，渲染会覆盖整个画面，永远在屏幕的最上面,自身就是一个 Batches;Screen Space - Camera：需要指定UI相机，画布会被放置在相机前，通过该相机渲染;World Space：把画布当成普通的3D对象放置在世界坐标系中，画布可以自由移动旋转,类似于 3D 物体;       
* 2: Canvas Batch--> Canvas下的UI元素最终都会被Batch到同一个Mesh中，而在Batch前，会根据这些UI元素的材质（通常就是Atlas）以及渲染顺序进行重排，在不改变渲染结果的前提下，尽可能将相同材质的UI元素合并在同一个SubMesh中，从而把DrawCall降到最低。Batch的结果会被缓存复用，直到这个Canvas被标记为dirty。
* 3: 影响合批的因素: Unity官方的重要提示：当给定Canvas上的任何可绘制UI元素发生更改时，Canvas必须重新执行合批过程。此过程重新分析Canvas上的每个可绘制UI元素，不管它是否被修改。注意，“更改”是指影响UI元素外观的任何变动，包括修改sprite renderer的sprite、transform的position和scale、文本网格的text等。
* 4: Canvas嵌套：Canvas可以嵌套使用，一个子Canvas下dirty的子物体不会触发父Canvas的rebuild。
* 5: UI顶点属性变化会引发网格更新:       
修改Image、Text的color属性，会改变UIVertex.color;        
修改RectTransform的Size、Anchors、Pivot等，会改变UIVertex.position;     
注意：在UGUI中颜色的变化是通过修改顶点色实现的，避免生成了新的DrawCall;         
注意：UIVertex.position记录的是本地空间下的坐标;            



![重建流程](重建流程.png)


* 6: 合批调用栈,取用 Profiler Frame Debugger
```
Camera.Render
    Drawing
        Camera.RenderSkybox
            Draw Mesh
        Render.TransparentGeometry
            RenderForwardAlpha.Render
                RenderForwardAlpha.RenderLoopJob
                    Canvas.RenderSubBath
                        Draw Mesh
```
* 7: Pixel Perfect 每次 UI 控件移动,都需要对 RectTransform 进行微调,相关的顶点移动了,也就会产生 sendWillRenderCanvas

****

## Layout 布局类

* 1: Layout 系列组件一直在 rebuild,重绘频率太高,不建议使用,建议去优化
* 2: 如果只是为了第一次的自动排序的,可以在显示之后禁用相关的组件,触摸时开启,无触摸时关闭



# 问题以及解决方案

* 1: 导致UI重建的/Batches过多/浪费性能的原因:
```
    UI 的重叠,拓扑关系变得复杂,直接导致 Batches 升高.      
    渲染顺序.靠近根节点显示在底层，而靠近叶子节点显示在顶层；这样的渲染方式使得调整UI的层级比较方便和直观。UI 的层级尽量保持同步.       
    图集的分配不合理.       
    Layout  重绘频率太高.(不停的 UI 运动).Canvas的Pixel Perfect选项,ui元素在发生位置变化时，造成layout Rebuild;        
    Graphic 重建频率太高.(不停的添加删除,尽量不用SetActive,使用 scale)         
    Text的Best Fit选项,这个选项可以动态的调整字体大小以适应UI布局而不会超框.代价很高.outline,shadow.不建议用               
    UGUI的touch处理消耗也可能会成为性能热点,UGUI在默认情况下会对所有可见的Graphic组件调用raycast,touch事件的grahic，一定要禁用raycast,例如纯展示的 Image/Text       
    减少 Mask 组件的使用,因为 mask 里面有 mat 的切换,使用了切割顶点技术Clip.    
```
* 2: 一个界面占用多少个 DC 合适?
>  一个界面尽量保证功能的情况下占有 3 个 DC 即可,Common,Font,Texture 等最原始的可以保证 UI 功能.如果特别炫酷的可能需要 20 个 DC 左右,也就保证完成即可.

* 3: 如何定位 CPU 的性能热点函数.
```
    profiler    
    Frame Debugger       

    查看根函数可以更快的知道性能问题
    Canvas.BuildBatch:合并Canvas节点下所有UI元素的网格，合并后的网格会缓存起来，只有其下面的UI元素的网格发生改变时才会重新合并.
    Canvas.SendWillRenderCanvases:UI元素的网络变化主要是因为此调用时，rebuild了Layout或者graphic(Text,Image);原因-->例如增加删除UI对象，UI元素的顶点，rec尺寸改变等
    这里的 重建布局与重建UI 与 NGUI 有异曲同工之处

``` 

* 4: 如何定位 GPU 的性能热点并优化
```
    渲染数据过多,Mesh(VBO),顶点,三角形,颜色,UV,法线,光线等导致 shader 计算量太大.            
    渲染状态切换太频繁,单位时间内,mat 在 GPU 中切换多次进行渲染.         
    渲染过度(overdraw),含义是在一个像素点上面渲染了很多次,简单解释就是,UI 叠加,物体被遮挡了,但是仍然进入了渲染管道.         


    根据以上三条定理进行渲染优化:
    不可见的物体不让其进入渲染管道.例如不可见 UI,直接设置 scale 为 0.01;在摄像机照射范围内,物体被遮挡情况下,直接进行算法切割,也就是所谓的 Occlude 优化.使用多边形镂空脚本PolygonImage(https://blog.uwa4d.com/archives/fillrate.html).                
    减少 Mesh 的数据,这个做法就很多了.          
    渲染状态,记录 mat 切换的所有事件,自己做个工具,然后根据某一段切换频率是否过快,来进行优化.    

```

* 5: 以上问题以及优化方案不可能完全满足你的需求,UI 业务的变化神鬼莫测,但是万变不离其宗,掌握了万剑归宗的剑法,即可对所有性能下降的方面做出平衡性极佳的操作.


* 1 该过程由CanvasUpdateRegistry监听Canvas的WillRenderCanvases（上图中1）而执行,主要是对当前标记为dirty的layout和graphic执行rebuild。也就是顶点数据改变了;
* 2 在rebuild layout之前会对Layout rebuild queue中的元素依据它们在heiarchy中的层次进行排序（上图中的2），排列的结果是越靠近根的节点越会被优先处理。
* 3 rebuild layout（上图中的3）,主要是执行ILayoutElement和ILayoutController接口中的方法来计算位置，Rect的大小等布局信息。
* 4 rebulid graphic（上图中的4）,主要是调用UpdateGeometry重建网格的顶点数据（上图中5）以及调用UpdateMeterial更新CanvasRender的材质信息（上图中6）。
* 5 为什么在rebuild layout的时候，要优先处理根节点的元素?

**** 

合批原理            

* 1 UGUI的合批规则是进行重叠检测，然后分层合并。
* 2 第一步计算每个UI元素的层级号(类似于 NGUI 的 Depth)：如果有一个UI元素，它所占的矩形范围内，如果没有任何UI在它的底下，那么它的层级号就是0（最底下）；如果有一个UI在其底下且该UI可以和它Batch，那它的层级号与底下的UI层级一样；如果有一个UI在其底下但是无法与它Batch，那它的层级号为底下的UI的层级+1；如果有多个UI都在其下面，那么按前两种方式遍历计算所有的层级号，其中最大的那个作为自己的层级号。
* 3 第二步合并相同层级中可以Batch的元素作为一个批次，并对批次进行排序 ：有了层级号之后，Unity会将每一层的所有元素进行一个排序（按照材质、纹理等信息），合并掉可以Batch的元素成为一个批次。经过以上排序，就可以得到一个有序的批次序列了。这时Unity会再做一个优化，即如果相邻间的两个批次正好可以Batch的话就会进行Batch。合批的Batch数据，最后会分别放在CanvasMesh的SubMesh里。

# 常见问题
开发过程中四个常见的问题        
过多的GPU片段着色器使用率（如屏幕填充率过高）       
过多的CPU时间开销在重建一个画布上       
过多的CPU时间开销在生成顶点上（通常是文本）     
过多的画布重建次数      

针对这四个问题来分组介绍优化策略        
### 网格重建优化策略(优化 Mesh)        
> * 1 使用尽可能少的UI元素：在制作UI时，一定要仔细查检UI层级，删除不必要的UI元素，这样可以减少深度排序的时间以及Rebuild的时间。
> * 2 减少Rebuild的频率：将动态UI元素（频繁改变例如顶点、alpha、坐标和大小等的元素）与静态UI元素分离出来，放到特定的Canvas中。
> * 3 谨慎使用UI元素的active操作：因为它们会触发耗时较高的rebuild。使用 Scale 代替.
> * 4 谨慎使用Canvas的Pixel Perfect选项：该选项的开启会导致UI元素在发生位移时，其长宽会被进行微调（为了对齐像素），从而造成layout Rebuild。（比如ScrollRect滚动时，会使得Canvas.SendWillRenderCanvas消耗较高）
> * 5 Animator最佳用法： Animator每帧都会改变元素，即使动画中的数值没有变化，因为Animator没有空指令检查。对于仅响应事件时才变化的元素，可以自行编写代码或使用第三方补间插件。换成 DoTween
> * 6 谨慎用Tiled类型的Image

### 屏幕填充率优化策略(OverDraw)      
> * 1 禁用不可见的面板：比如当打开一个系统时如果完全挡住了另外一个系统，则可以将被遮挡住的系统面板禁用。（龙与少女优化方案：通过修改Canvas对象的Layer隐藏面板。）
> * 2 不要使用空的Image做按键响应：在Unity中Raycast使用Graphic作为基本元素来检测touch。如果使用空的image也会产生不必要的overdraw。可以实现一个只在逻辑上响应Raycast但是不参与绘制的组件即可。使用网上的 Empty4Raycast 方案
> * 3 Polygon Mode Sprites：如果图片边缘有大片留白就会产生很多无用填充。Unity和Texture Packer目前都支持了Polygon Mode，也就是说将原来的矩形Sprite用更加紧致的Polygon来描述。
> * 4 Image Fill Center：在Image Type选项为Sliced的情况下，不需要Fill Center的时候去掉勾选。

### 合批优化策略(DrawCall)        
> * 1 相同层级原则：父节点下所有子节点，尽量保持相同的层次结构。相同层级下的UI元素可以Batch.
> * 2 Mask组件：Mask组件使用了模版缓存，Mask中的UI元素无法与外界UI元素合批，Mask组件还会额外增加2个DrawCall.
> * 3 隐藏的Image：Image组件中sprite为空，都是占用drawcall渲染的，并且还会打断前后元素的合批。
> * 4 Screen Space-Camera模式：一个Canvas中的任何一个UI元素只要在屏幕中，则这个Canvas中的其他UI元素即使在屏幕外DrawCall仍不会减少。
> * 5 Hierarchy穿插重叠问题：如下图红点和Icon在不同图集中，如果红点稍微大一点，遮挡了旁边的Icon，就不能合批，须要调整Icon和红点的节点关系，4个Icons放在一个节点下，4个红点放在一个借点下。

### 字体优化策略(Font)        
> * 1  字体图集的重建机制：当一个新文字出现的时候，会被添加到字体图集，如果图集已经没有空余的地方，那么图集会被重建。图集会以相同的尺寸重建，打包当前激活的所有UI text组件中要显示的文字，如果发现图集尺寸不够用的时候，图集会重新扩充尺寸。
> * 2 后备字体机制：对于字体库里没有的文字，会被放进后备字体图集里，后背字体图集会常驻内存里，不会被销毁。后备字体取自于系统自带的系统字库Arial.ttf，在发布的游戏安装包里该字库是不存在的。我们在一些Unity开发的游戏里，偶尔会发现一些生僻字的字形和其它常见文字的字形不统一。
> * 3 Text的网格重建：Text组件被重新启用的时候，会重建Text的网格。如果含有大量的文字，会造成严重的CPU开销。
> * 4 提前生成动态字体：准备游戏非常常用的文字集合，通过Font.RequestCharactersInTexture接口提前放入字体图集里。注意使用Font.textureRebuilt 委托，在字体图集被重新重建的时候，把我们提前准备的文字集合再次添加进去。
> * 5 使用美术数字：游戏的分数，可以使用美术数字（精灵图片）来代替Text组件
> * 6 谨慎使用Text的Best Fit选项：虽然这个选项可以动态的调整字体大小以适应UI布局而不会超框，但其代价是很高的，Unity会为用到的该元素所用到的所有字号生成图元保存在图集里，不但增加额外的生成时间，还会使得字体对应的图集变大。
> * 7 减少长文本Text的变动，慎用UI/Effect：描边和阴影效果都会增大四倍的顶点数

### 滚动视图优化策划(ScrollView)        
> * 1  有两种方法填充滚动视图:用所有需要出现在滚动视图的元素填充滚动视图(子控件很少的情况下使用).用池处理这些元素，根据需要重新放置它们的位置(子控件很多,或者无限的情况下使用)
> * 2 RectMask2D组件：俩种方法可以通过给滚动视图添加一个RectMask2D组件来提高性能。该组件确保在滚动视图窗口外面的滚动视图元素不会出现在可画的元素列表中，省去了该元素的batch。
> * 3 一种简单的缓存池策略：在UI中布局中，使用带有Layout Element组件的对象占位（ Slot ）。给可见UI元素实例一个池，来填充滚动视图看可见区域，Slot作为父物体来定位。
> * 4 基于位置的缓存池策略：通过移动布局里UI元素的RectTransforms坐标值，来排序显示位置。通常写一个自定义的滚动视图类或者写一个自定义布局组的组件。

### 其它优化策略        
> * 1 禁用无用的Raycast：UGUI的touch处理消耗也可能会成为性能热点。因为UGUI在默认情况下会对所有可见的Graphic组件调用raycast。对于不需要接收touch事件的grahic，一定要禁用raycast。（龙与少女为策划提供了检视的辅助脚本）
> * 2 OverrideSorting：子Canvas中的OverrideSorting属性将会造成Graphic Raycast测试停止遍历Transform层级。
> * 3 UI对象的坐标Z值：Z值不为零的时候会影响对象渲染顺序并不能合批。（例如：龙与少女里的阵型界面都是修改Spine的SortingOrder来实现位置排序）
> * 4 网格开销巨大：如果出现了WaitingForJob或PutGeometryJobFence，则说明合并网格开销巨大（子线程网格合并）
> * 5 高级技巧：对于处于选中播放动画的需求，并且所处canvas下内容比较多的情况下，可以单独把选中对象放到预先建好的动态canvas里，取消选中时再放回去。
> * 6 CanvasGroup的使用:在窗口的GameObject上添加一个CanvasGroup，通过控制它的Alpha值来淡入或淡出整个窗口;在窗口的GameObject上添加一个CanvasGroup，通过设置它Interactable值来控制底层所有控件的交互开关;在 UI 元素或其某个父元素上放置画布组 (Canvas Group) 组件并将其 Block Raycasts 属性设置为 false 来使一个或多个 UI 元素不阻止鼠标事件。
> * 7 使用 Profiler 以及 FrameDebug 检测

# 规范
规范化的重要性：有规范就会有约束有限制，在一个团队的角度上来讲，大家遵守同一套规范，可以避免多余的沟通，增加开发效率，是保证团队协作、项目稳定推进的利器。

设计模板：根据游戏风格和类型设计几套模板：尺寸(比如大中小三套)、布局(比如左右,左中右等)、样式(一级底+二级底+三级底)等，根据游戏内容选择模板，既保持UI统一，又能方便拼UI，大概百分之九十的窗口都在这几个模板中选择。其他比如充值等需要有表现力的窗口再自由设计尺寸和布局。

路径一致性：美术UI目录和客户端目录保持一致，可以很方便替换新版UI，而不会出现名字不一致，目录不一致，策划找瞎眼的情况。

图片命名：可以参考功能、颜色、尺寸等特点命名，命名尽量使用英文，可以添加前缀表示所属功能，后缀也可以使用拼音。

公用图集：多个面板都会用到的图片放到公用图集里面。为了减少合批的障碍，有必要的时候，需要复制公共图片到单独的面板图集里。

合理的出图尺寸：可以减小硬盘大小，减少第一次导入项目时的图片序列化时间。

图片分类：图片可以根据用途分为UISprite、UIFrame 、 Icon、Photo、背景原画。
UISprite：尽量九宫格或者平铺，并且尽量复用。
UIFrame：是一些尺寸巨大的背景框。
Photo：英雄形象等大尺寸图片合并图集太大，因此不会打进图集。

字体大小：研发过程中确立大中小几号字体。每级再分三类，一共九种字体大小。

色值表：颜色可以由美术出一张色值表，包括一种颜色的RGBA值和16进制值，方便开发人员快速定位准确颜色。颜色值可以存储在unity的颜色模板里。
