---
title: Unity NGUI/UGUI优化
date: 2020-05-08 11:41:32
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