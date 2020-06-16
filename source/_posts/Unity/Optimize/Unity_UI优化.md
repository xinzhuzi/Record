---
title: Unity NGUI/UGUI优化
date: 2020-05-08 11:41:32
top: 8
categories:
- Unity优化
tags:
- Unity优化
---

****

# UI优化
* 1,降低界面的渲染开销.2,降低界面的更新开销(网格合并).3,高效处理大量 HUD 元素.
* NGUI 与 UGUI 的不同点在于:元素更新方式(字/图片更新时的不同), DrawCall合并规则,网格更新机制
* 直播视频 https://blog.uwa4d.com/archives/video_UI.html
* https://blog.uwa4d.com/archives/USparkle_NGUI.html


# NGUI优化
* https://blog.uwa4d.com/archives/USparkle_NGUI.html
* 元素更新方式:一个 UI 展示在界面上面,是将图片以及文字转换成为顶点,UV,颜色,法线,切线,三角形顶点索引等合成 Mesh,然后在 GPU 上面呈现响应的人体识别的像素.
* 使用 BetterList进行优化
* 对于 NGUI 使用大量的 SetActive(false),适量的:Color.a=0,移除,Timer+二级缓存
* 渲染顺序:Draw Call合并规则,以 UIPanel 为单位,将 UIPanel 下面的元素 以 Depth 大小为排序规则,如果是相同的图集,就合并成一个 Draw Call,进行合并.      
检测 Draw Call -->在每个 UIPanel 上面可以调用出 DrawCall Tool           
* 网格更新 UIPanel.LateUpdate 两种更新方式:UIPanel.FillDrawCall 更新单个 DrawCall ,      
UIPanel.FillAllDrawCalls 更新所有 DrawCall,如果这种情况经常发生,CPU会有峰值.
* 一个 DrawCall 对应一个 Mesh,网格重建时是根据不同的 Mesh,例如字体与图片的 mesh 就不同,就会分开重建
* 先拆分 UIPanel,控制 FillAllDrawCalls
* 降低界面的渲染开销:
>* profiling 查看开销
>* DrawCall 多少
>* Mesh.xxxVBO 网格更新创建,网格经常变动,就会不停的刷新 VBO,然后提交到 GPU 进行渲染
>* GPU 的 过度绘制 Overdraw
>* NGUI 产生的 Mesh 都是通过 MeshRenderer 画出来的
>* NGUI 中的 UITexture,单独占用一个 DrawCall; Depth 穿插 同一个图集或者字体,设置相近的 Depth;设置了 Scale=0
>* 动静分离,拆分 UIPanel
>* 降低更新频率,降低小地图的更新频率,一段时间内,小地图内的元素不更新不移动等.
>* 避免"敏感操作".因为元素隐藏和显示,触发了FillAllDrawCalls,添加/删除元素时,穿插了其他的 UIDrawCall 或者自成一个UIDrawCall.     
尝试让插入的元素能够合入现有的 UIDrawCall,通过 Scale =0或者 alpha 接近 0 来隐藏.
>* 优化选项: Static 选项,如果当前这个界面是静态界面,不会发生位移,就可以打开     
 Visible,如果打开,就不需要重新计算控件的包围盒,用在大量计算 mesh 重建上面.例子:在界面上面完全展示出来的,就不需要计算,可以勾选

> * 高效处理大量 HUD (血条),图片尽量使用 Simple/Filled 模式,顶点数,面数尽量降低,尽可能使用相同图集.最好分帧加载 HUD.        
对于伤害数字(飘字),可以转成图片字,不要修改父节点,使用对象重用池,隔帧更新,控制总量.使用 world space 替换 worldtoscreenpoint 主角单独一个 Canvas/UIPanel,这样当主角行动时,不会触发其他重建.将血条替换为 Shadow


****

# UGUI 优化
* 元素更新方式:使用 Scale=0,Alpha Group =0 
* 渲染顺序:Draw Call合并规则, 需要仔细测试,图片和图片在同一级,字体和字体在同一级,如果中间缺一级,可以添加一个中间级别来分割从而减低 draw call 等等操作.      
检测 Draw Call -->使用 Frame Debugger           
在 UGUI 上面对 Draw Call 做优化,1:检测重叠是由控件的包围盒子去检测是否重叠的,需要注意.2:不规则摆放的图片,需要在一个图集里面.3:动态遮挡.4:3D UI 等,都是有非常高的 Draw Call
* 网格更新 Canvas.BuildBatch 更新所有的 DrawCall  ,仅限于当前的这个Canvas  
 WaitingForJob      
 PutGeometryJobFence        
 BatchRenderer.Flush 
* 一个 DrawCall 对应一个 Mesh,网格重建时是根据不同的 Canvas,例如字体与图片的 Canvas 相同,就会一起进行重建
* 拆分 Canvas
* 降低界面的渲染开销:   
>* profiling 查看开销
>* DrawCall 多少
>* Mesh.xxxVBO 网格更新创建,网格经常变动,就会不停的刷新 VBO,然后提交到 GPU 进行渲染
>* GPU 的 过度绘制 Overdraw
>* UGUI 的 Z 值不为 0,与其他的 UI 不同,会被单独合批成为一个 Draw Call,把 UI 移动到屏幕外,不会降低 Draw Call     
>* UGUI 的 DrawCall 上升的主要原因是 Hierarchy 中的 穿插与重叠,例如:scrollveiw 与图标,红点等等
>* UGUI Sprite Packer图集,一组纹理具有相同的 tag, 但是打在了不同的图集里面了,是因为 alpha 通道不同,压缩方式不同.会让人以为本来是一个图集,结果会在不同的图集里面
>* 动静分离
>* 降低更新频率
>* 避免"敏感操作",不要对 UI 进行相同的 position 赋值,除非position进行肉眼可见的改变,每次赋值底层可能都在进行Mesh重建,如果有就会重建.
>* 优化选项

> * 高效处理大量 HUD (血条),图片尽量使用 Simple/Filled 模式,顶点数,面数尽量降低,尽可能使用相同图集.最好分帧加载 HUD.        
对于伤害数字(飘字),可以转成图片字,不要修改父节点,使用对象重用池,隔帧更新,控制总量. 使用 world space 替换 worldtoscreenpoint 主角单独一个 Canvas/UIPanel,这样当主角行动时,不会触发其他重建,使用 textMesh/textpro 第三方库.将血条替换为 Shadow
> * 子线程合批 mesh
> * 检查重叠关系,使用线框模式检查
> * 文字图片分层,最好将所有的文字分在一层(拓扑排序中),放在渲染最顶层
> * 只接受逻辑,不进行绘制
> * 将多边形镂空的脚本,只有可显示区域进行绘制 https://blog.uwa4d.com/archives/fillrate.html
> * 减少事件拦截,不需要事件响应的,不需要勾选 Raycast Target
> * 动静分离时:自动给动态的组件添加上 cavans 
> * 如果有动画一直修改 Image 上面的 color 属性,就是修改顶点属性上面的修改会导致重建网格(mesh),需要写个脚本创建一个 material 在 lateUpdate 里面修改创建的material的颜色达到动画的目的

# Overdraw优化,降低填充率
GPU Overdraw局部像素区域压力过大,单像素填充压力过大.
* 1:在某个背景上有个按钮，要将按钮绘制在背景上，这个就是Overdraw，Overdraw无法避免，只能优化降低

* 2.性能参数        
总填充数值峰:单帧总填充像素数量最大值       
填充倍数峰值:单帧最大填充倍数(10.0X就是该帧刷新10遍)        
单帧填充倍数:该帧总填充数/该帧渲染相机分辨率        
* 3.优化方案
控制绘制顺序:PC上资源无限，一般都是从后往前绘制，但在移动上，尽量从前往后绘制.在Unity中，那些Shader中被设置为“Geometry”队列的对象总是从前往后绘制的，而其他固定队列（如“Transparent”“Overla”等）的物体，则都是从后往前绘制的。这意味这，我们可以尽量把物体的队列设置为“Geometry” 。
尽量减小过度绘制区域:实在需要多层绘制的地方，要尽量减小各部分过度绘制区域，使重合区域小，绘制的像素点也就少一点
注意性能与效果的取舍:UGUI的许多控件有很好的通用性和展示效果，但是可能会耗更多性能
过大的不必要绘制尽量代码实现:例如点击屏幕空白区域返回功能，加透明image会增加很多
UI设计上尽可能简单减少重叠

* 4.针对性优化
1)文字部分
主要原因是使用了Outline，Outline实现方式是将Text的四个顶点传过去复制四份，设置四份偏移量实现效果，将偏移量设置很大之后，可以看到一个Text周围有四个相同的Text        
解决方案:
1.不使用或者使用Shadow(Shadow通过为图像或者文字的Mesh添加顶点实现阴影效果，Outline继承Shadow，在对象四个角上各添加一个Shadow)
2.使用Textmesh Pro(Unity5.5)需要制作相应的字体文件，对于动态生成的文字效果不好，固定字体很好
(https://blog.csdn.net/dark00800/article/details/73011343?utm_source=itdadao&utm_medium=referral)
3.修改Mesh的UV坐标，提取文字原始UV坐标，扩大文字绘图区域，对文字纹理周围像素点采样，新旧颜色融合
(http://gad.qq.com/article/detail/29266)
2)适配IphoneX
适配的需要加了层背景，不是iPhoneX失活就可以

3)背景人物mesh
裁剪小一点更好


1)Mask组件
Unity的Mask组件会增加一层Overdraw，还会多增加4个DrawCall
解决:
1.使用RectMask2D代替，缺点是只能用于矩形
2.对于多边形，用MeshMask，红色为UnityMask，蓝色是MeshMask，UnityMask消耗15个DrawCall，Overdraw2次，MeshMask消耗1个DrawCall，1层OverDraw(https://www.cnblogs.com/leoin2012/p/6822859.html)

2)Image的slide属性
对于slide九宫格图片，可以看情况取消fill center属性，那样中心区域会不渲染，中心区域也就镂空，重合面积也会小

1)重合多的地方尽可能不重合

2)无用的Image
少量的panel或者单纯的空父物体身上加着image，虽然没有给图片，但是还是会渲染

3)移动的波浪图片过大过多(修改高度，宽度)
4)特效粒子效果优化(http://www.u3dnotes.com/archives/807)
粒子效果薄弱的可以使用序列帧动画实现

1)三层大底图，一个纯黑底，还有两层楼梯背景(可以纯黑底图合并到两层)
2)楼梯前底图可以小一点不和最底下图标重叠

部分小细节：
1.slide九宫格图片，取消fill center，中心镂空
2.mask尽量不用，可以用rect mask2D 代替
3.不用UI/Effect，包括Shadow，Outline，Position As UV1
4.不用Image的Tiled类型
5.不用Pixel Perfect
6.动静分离，动态的在父物体上加个Canvas
7.尽量active，不要destroy，也不要设置Alpha=0这样还是会渲染
8.不用BestFit(代价高，Unity会为该元素用到的所有字号生成图元保存在Altlas中，增加额外生成时间，还会使得字体对应的atlas变大)
9.特效粒子

⑴尽量减少alpha = 0的资源的使用，因为这种资源也会参与绘制，占用一定的GPU。 ⑵制作图集的时候，尽量使小图排布紧凑，尽量图集中大面积留白，理由同上。 ⑶避免无用对象及组件的过度使用
如果sprite是中心镂空且切图为九宫格时，可以去除fill center，以减少over draw游戏中许多时候会使用一个透明的Image组件来监听点击事件或者屏蔽Image后面的按钮事件，空的Image可以解决这个问题，用起来也很方便，但是空的Image照旧会参与绘制，从而产生overdraw。解决办法是扩展Graphic组件来替换Image组件。 如果是只要点击区域，不要显示内容的。