---
title: Unity UGUI优化
date: 2020-05-11 11:41:32
top: 110
categories:
- UnityUI
tags:
- UnityUI
---

# 参考文章

https://www.jianshu.com/p/061e67308e5f

https://www.jianshu.com/p/8a9ccf34860e

http://blog.jobbole.com/84323/



不错的文章

Unity GUI(uGUI)使用心得与性能总结
https://www.cnblogs.com/kanekiken/p/7421449.html



Unity 之 UGUI 小总结
https://www.cnblogs.com/kanekiken/p/7421510.html



# 腾讯互娱高级工程师-刘绵光

导语:《御龙在天移动版》是一款 MMORPG 国战手游，全 3D 视角、同屏人数 多和复杂的 UI 逻辑玩法是我们的主要特点。本文将介绍御龙手游在前期 UI 框 架选型的思路和使用 UGUI 开发过程中，遇到的常见性能问题以及解决方案。

# NGUI vs UGUI 选择

我们的项目在 demo 预研阶段使用的是当时最新的 Unity4.5.x 版本，没得选， UI 用的是 NGUI。demo 阶段的 UI 其实只有一个摇杆和四个技能按钮，UI 系统 框架的代码更是一篇空白。在 demo 预研后期，Unity4.6 版本发布了!带来了 全新的 UGUI，还是挖的 NGUI 的作者去做的，应该靠谱，于是我这边开始预研 引入 UGUI 的阶段。

各种测试、源码分析、官方数据、论坛数据对比之后，我们最终决定选择 UGUI， 具体的原因主要有以下几点:

1.UI 的渲染顺序和 Hierarchy 节点布局相关，非常直观，这点与端游的 UI 编辑 器 tenio 是一样的，对于整个端游开发团队转移手游开发可以降低学习成本。重 点是 NGUI 依赖 depth 来设置渲染顺序，这个实在无法认同，对于 MMORPG 复杂的 UI 界面来说后期维护起来会很麻烦;

2.强大的 Rectranform，对于编辑 UI 来说比 NGUI 方便很多;

3. Unity 官方的 UI 系统，在兼容性和后续官方可优化空间上来说，原生的 UI 系 统具有一定优势。在后续的 5.2 版本证明了这一点，例如:Canvas.BuildBatch()， 合批 Canvas 下所有网格，这个性能热点在 5.2 版本后挪到了子线程去做减轻了 主线程的压力，而 NGUI 作为一个插件没法做到这一点，网格合批的性能热点还 是耗在主线程的 UIPanel.LateUpdate();


4. 性能上有一个略微的优势点:UGUI 的 UIMesh 生成是通过底层 C++代码实 现的，而 NGUI 只能通过上层的不断创建 Vertex List，这样在堆内存的管理上， UGUI 确实要好很多，带来的隐形收益就是 GC 触发次数会少很多。

当然，我们选择了 UGUI，并不是说 UGUI 一定比 NGUI 好，衡量一套 UI 系统 的好坏应该是全方位的:性能、配套工具、易用性、稳定性等，UGUI 在配套工 具上还有待加强，我们也相信官方会不断完善改进(5.2 版本确实给力改进了很 多)。

关于 NGUI 和 UGUI 的性能对比，我没有去做一个全方位的真机测试比较，看了 网上不少帖子讨论过这个问题，大家的结论是 UGUI 有略微的优势但差别不大。 现在回过头来看咱们部门上线的两款手游，九龙战(NGUI)和御龙在天移动版 (UGUI)都是性能表现优异的游戏，当然两款游戏的类型不一样，UI 复杂度也 不一样，没有直接的可比性。我个人觉得 NGUI 和 UGUI 都是很好的 UI 框架， 只要把它们的原理特性掌握好，都可以做出性能很好的 UI 界面。

补充一点:我们项目组目前用的 Unity 版本还是 4.6.9，出于稳定性和风险等综 合因素考虑没有贸然升级 5.x 版本，所以 5.2 版本 UGUI 升级带来的各种优化改 进和我们也没啥关系了，只是对新版本做了一些预研的工作，所以接下来讨论的 性能优化都是基于 Unity4.6.9 版本。

# UI 资源规范(内存优化)

客户端做任何的性能优化首先想到的都是规范美术资源，前期不给美术资源定制 一定的规范，后期做优化性能会非常的被动。对于 UI 资源的规范，主要是考虑 的内存优化。

1.任何的 UI 图集最大 size 1024*1024(内存优化);
2.同一个界面出现的 UI 资源尽量放到一个图集，重复利用的公用资源放 common(DrawCall 优化); 3.能用九宫格的尽量用九宫格来减小原图大小(内存优化);
4.美术给过来的 UI 原图 size 尽量小，对于一些全屏的 loading 原画图，原画大 小是 1136*640(我们 UI 的标准分辨率)，让美术按照比例高度缩小到 500，

这样一张 1024*1024 的图集就可以放两张原图了，提升图集利用率。对于一些


600*400 类似大小的原图，就尽量按比例把最长边压小到 500，这样出来的图 集就是 512*512 而不是 1024*1024(内存优化);
5.对于特别长条的 UI 原图，例如 1000*100，如果由于加入这个长条的原图导 致图集大小变大而且利用率很低的话，要把 1000*100 的原图拆分成两张图 500*100，在制作界面的时候用两个 Image 拼接即可，这样可以把 1024 的图 集缩小到 512(内存优化);

6.图集利用率低于 1/3 的时候，要考虑和其他同一个 size 的图集合并以提升利 用率。合并的原则是不改变任何一个图集的大小，这样即可完全省掉一张图集(内 存优化、安装包量优化);
7.尽量复用 UI 资源，减少不必要的原图，例如一个卡牌分了五种品质原画底图， 白蓝黄绿紫，就不要使用五张大底图了，让美术同事画一个灰色原图，Image 在 使用的时候直接按需求修改顶点色即可(内存优化);

8.关闭 mipmaps(内存优化)。

# GPU 优化

谈及 GPU，我们知道它负责整个渲染流水线，从处理 CPU 传递过来的模型数据 开始，进行 Vertex Shader、Fragment Shader 等一系列工作，最后输出屏幕 上的每个像素。因此我们可以从两个方面着手，优化 Shader、减少 OverDraw。

Shader 优化

我们的 UI Shader 是自己编写的，没有使用默认的 UI-Default.shader，因此 Shader 优化非常有必要。

1.Fog { Mode Off }，最早有一个版本我们没有关闭 Fog，导致打开全屏 UI 的 时候帧率明显往下掉(我们的全屏 UI 开启后会关闭主相机，理论上渲染压力和 CPU 消耗会降低)，后来 Adreno Profiler 真机调式 Shader 代码后发现了此问 题;

2.Fragment 剔除掉 Alpha 为 0 的像素点，减少 OverDraw;
3.尽量简化 Shader 代码，最早我们有一些 UI 效果的需求全都写在了一个 Shader 里，其实 99%的 UI 不要这些东西，单独把效果需求代码拎出来独立一 个 Shader。



# OverDraw 优化

在每帧绘制中，如果一个像素被反复绘制的次数越多，那么它占用的资源也必然 更多。目前在移动设备上，OverDraw 的压力主要来自半透明物体。因为多数情 况下，半透明物体需要开启 Alpha Blend 且关闭 ZWrite，同时如果我们绘制 像 alpha=0 这种实际上不会产生效果的颜色上去，也同样有 Blend 操作，这是 一种极大的浪费。

我们的 UI 绘制是 Alpha Blend 且关闭 ZWrite，因此 UI OverDraw 的优化主要 是在制作界面的时候减少 UI 重叠层级(和策划、美术 pk)。除此之外还是有一 些我们程序可以控制的优化点:

1.对于九宫格的 Image，如果去掉 fillcenter 不影响最后出来的效果就要把 fillcenter 去掉，可以减少中间一片的像素绘制; 2.看不见的元素且没有逻辑功能要 disable 或者挪出裁剪区域，而不要通过设置 Alpha=0 来隐藏;

3.不要使用一张 Alpha=0 的 Image 来实现放大响应区域的功能;

4.UI 底层系统来控制隐藏看不见的元素，例如打开全屏 UI 的时候把下面看不见 的 UI 挪出裁减区域、关闭主相机渲染。

# CPU 优化

在我们项目开发的过程中，遇到的大部分 UI 相关性能热点都是和 CPU 消耗相 关的。大致可以分为以下几类:DrawCall、Canvas.SendWillRenderCanvases()、 Canvas.BuildBatch()和其他。

# DrawCall 优化

DrawCall 是 CPU 调用底层图形接口，频繁的调用对 CPU 性能的影响是很明显 的。优化思路很简单，合批绘制。UGUI 本身的动态合批机制会帮我们尽量的去 优化合批，我们要做的就是弄清楚它的合批机制然后让 UI 元素尽量合批绘制。 以下是我们项目组在制作 UI 过程中总结出来了的一些优化 Tips:

1.合理分配图集，同一个界面上的图尽量打到一个图集，多个界面复用的图，放

到 common;

2.制作界面的时候，相邻节点尽量使用同一个图集的图片;


3.Text 本身也是用的 Font Texture，不同字体的 Text 也是来自不同的图集，所 以在布局界面的时候也要尽量避免穿插打断绘制流程;
4.DrawCall 的数量不是完全由 Hierarchy 的布局决定，和 UI 的位置也有关系， 这个位置不是指的 Rectranform 上面的 size 位置重叠就一定打断绘制，而是真 实的三角面的位置是否重叠。这个可以在 Scene 视图下用线框模式(Texture Wire)去观察;

5.少用 Mask 组件，Mask 实现的原理是 Stencil Buffer，往模版缓存里绘制， 模版缓存里的东西才是可见的。模板缓存会打断所有的合批，Mask 的子节点和 外面的节点无法合批，模板缓存自己占一个 DrawCall。Unity5.2 之后的版本建 议使用 2D Rect Mask 替代。

Canvas.SendWillRenderCanvases()

真机 Profiler 会经常看到这个函数的消耗飙高，研究 UGUI 源码得知该 API 为 UI 元素(Graphic 子类)自身发生变化(比如被 Enable、顶点色变化、Text 组 件的文本变化等)时所产生的调用，CPU 飙高大部分情况下主要是因为 UnityEngine.UI.Graphic.UpdateGeometry()重新生成了所有的顶点数据，具 体有哪些操作会导致 Graphic 重新生成顶点数据，可以搜索 UGUI 的源码(4.6.9) Graphic.SetVerticesDirty()。刷新顶点数据的消耗和当前 Graphic 的顶点数正 相关，所以接下来研究一下 UI 顶点数。对于 UI 来说，顶点数量主要是由 Image 控件和 Text 控件产生的。

对于 Image 控件，顶点的数量取决于 ImageType。
1.Simple 模式只有 4 个顶点;
2.Sliced 模式，就是我们常用的九宫格，要分两种情况:勾选了 fillcenter 的顶 点数是 36 个，没勾选 fillcenter 的顶点数是 32 个;
3.Tiled 模式，就是平铺模式，这个定点数量就完全取决 Image 控件的 Rectranform 设置的大小和原图大小，简单来说就是铺开了 N 张图就是 4*N 个 顶点;
4.Filled 模式，一般用来做进度条、技能 cd 转圈等效果，这个顶点数就取决于 Fill Method 和进度值，组合的情况比较多，我就不展开算了，具体数值大家感 兴趣的自行 demo 看下，总之不少于 4 个。
由此可以得出结论，Image 能用 Simple 模式的尽量用 Simple 模式。在项目

组中经常遇到的一种情况是，以前的需求是这个 Image 用九宫格，后来需求变动不需要九宫格了，但是没有把 Image Type 切换会 Simple 从而导致多余的顶 点数。
其实 UI 顶点数的重灾区主要是在 Text 控件，Text 在不添加任何效果的情况下 是一个字符串是 4 个顶点，单独添加 Shadow 后一个字符串顶点数是 8 个，单 独添加 Outline 后一个字符串顶点数是 20 个，Shadow 和 Outline 都加一个字 符串顶点数是 40 个。如果界面上有 N 个字符串，那顶点数就是 N*上面的系数， 这个量级是很可怕的。

因此对于 Text 控件尽量不要去添加 Shadow、Outline 效果，特别是 Outline!

这对 CPU 和 GPU 都很伤。策划和美术想要的文字凸显的效果可以用背景和文 字配色来实现，效果实在无法接受的可以少量使用 Shadow。对于频繁变动的 Text 文本(例如聊天)，更要慎重考虑是否使用 Shadow、Outline。 除了优化 UI 顶点数以外还有一些设置是会影响此函数消耗的，查看源码得知如 果勾选了 Canvas 下的 pixelPerfect，在刷新顶点的时候会做一些额外的操作 RectTransformUtility.PixelAdjustRect，像素对齐调整，此操作很费，所以不要 勾选 Canvas 下的 pixelPerfect

Canvas.BuildBatch()

理解该函数的消耗之前先来说一下 UGUI 的动态合批机制(减 DrawCall)，UGUI 是以 Canvas 为合批的根节点，首先 Canvas 与 Canvas 之间是没法合批的， Canvas 下所有节点的顶点数据会生成一张大的 BatchedMesh，再根据材质纹 理、渲染顺序等把这个 Canvas 下能合批的 Mesh 生成 SubMesh，一个 SubMesh 就是一个绘制批次。每当这个大的 BatchedMesh 中任意一个顶点数 据发生变化的时候，UGUI 目前的实现方式很粗暴，就会直接重新生成 BatchedMesh ， Canvas.BuildBatch() 干 的 就 是 这 个 事 情 。 由 此 可 见 Canvas.BuildBatch()消耗飙高的因素有两点:Canvas 下的顶点数量多和 Canvas 下的顶点发生变化。


通常之前所提到的 Canvas.SendWillRenderCanvases()的调用都会引起 Canvas.BuildBatch()的调用。 理解了该函数的原理之后，优化的思路就清晰了:减少一些频繁变动的 UI 元素 的 Canvas 下的顶点数量。关于优化 UI 顶点数量上面说的比较多了，接下来重 点说下顶点变化。先来看下上面提到的 BatchedMesh

该 Mesh 包含了顶点色，所以修改顶点色、缩放、位移等导致 Mesh 数据变化 的操作，都会引起 Canvas.BuildBatch()。游戏中难免有 UI 频繁移动、缩放、变 颜色的需求，既然一定要变动，那我们就让这些频繁变动的 UI 元素单独放在一 个 Canvas 下，且尽量减少这个频繁变动的 Canvas 下的顶点数量。简单的概括 一句优化思路就是:动静分离。

当然如果 Canvas 分离得太细也有另外一个问题，DrawCall 数量提升，所以要 做一个权衡。

# 其他 UI 卡顿优化建议

除了以上三点，UI 方面造成 CPU 消耗飙高的原因还有很多，就不一一展开详细 解释了。

1.精简拆分 UI Prefab 文件，把打开界面瞬间看不见的 UI 元素尽量拆分，功能 逻辑触发其他 UI 元素时再动态加载，例如:背包、练鼎根据子页签内容拆分， 这样可以减少打开 UI 时卡顿的感觉;
2.对于 ScrollRect 里存在非常多元素的界面，避免一次全部 Initiation，回收重 复里节点元素，刷新数据即可(可以实现一个公用组件);


3.避免频繁 Initiation、Destroy 某个 UI 子元素，类似小地图的 SceneActor 圆 点、寻路路径、YLIconBox 的子节点，使用对象池维护起来重复利用; 4.序列化保存 Prefab 的时候，尽量让 active 的状态和大部分情况下出现的 active 状态一致，可以避免切换 active 状态带来的性能消耗;

5.在 UI 布局位置不会动态调整的情况下，不要偷懒用 Layout 组件实现自动布 局，直接 Rectranform 写死坐标位置即可，Layout 动态计算有开销;
6.UGUI 对比 NGUI 有个不好的地方是所有的 Graphic 组件默认都是参与接受射 线检测，响应 EventSystem 事件的，这样在在长时间按屏幕输入的时候参与遍 历检测的 UI 节点会很多，界面复杂到一定程度会导致 EventSystem.Update 消 耗飙升。建议用 5.2 之后版本的可以让所有没有响应需求的 Graphic 组件 Raycast Target 默认不勾选，4.x 也是可以添加 Canvas Group 然后去掉 Block Raycasts 和 Interactable;

7.不要使用 Text 的 Best Fit 选项，有额外开销;
8.对部分 UI 有频繁显示和隐藏需求的，建议不要直接调用 SetActive，上面有提 到这样会导致顶点数据重新生成，过于频繁调用有性能问题。有两个建议:挪出 裁减区域或者禁用 CanvasRender，具体采用什么方案要看需求，因为禁用 CanvasRender 还是会响应事件检测。