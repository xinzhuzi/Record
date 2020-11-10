---
title: Unity 渲染优化
date: 2020-05-08 11:41:32
top: 509
categories:
- Unity优化
tags:
- Unity优化
---


# Draw Call
* setPass Calls: pass 指的是 shader 里面的 pass 代码块;setPass表示材质切换(避免出现 Material Instance);setPass Calls指的是在运行期间,经过了多少次的材质切换;setPass Calls小于或者远小于 Draw Calls/Total Batches 的.当粒子停止播放,但是没有 Deactive 的情况下,引擎还是会画(setPass Calls增加),但是什么都没有画出来(Draw Calls 为 0).
* Batches : 非常重要,在 Frame Debug 里面可以看到当前帧画出的数据,所有右边的值加载一起,就是总量.动态合批,静态合批,字体 Mesh,图片 Mesh 等
* Draw Calls : 与Batches是不相同的,但是这个值目前大部分是与 DrawCalls 相同的,但是这个值一定等于 OpenGL 调用 glDrawElements 的次数,最好的情况下就是一个Batches方法的调用栈里面只有一次glDrawElements方法
* https://github.com/Unity-Technologies/BatchBreakingCause 该项目说明了为什么不能合批的原因,以及做法

# RenderState
* RenderState 切换 
* Unity5.5 版本之前有一个 Material.SetPassFast 的方法,表示渲染状态切换.
* 避免过多的 Instance Material 生成,也就是一个材质被改变了里面的参数,导致当前的Material会变成 Material(Instance),多出来一份甚至几份.使用 MaterialPropertyBlock 功能,https://blog.uwa4d.com/archives/1983.html;       
* Texture2DArray:相同 size/format/flags ,适合场景中存在的大量,多次采样的同种材质,比如:大量同种PBR 材质的怪物,潜在有点事降低 DrawCall;网格不必相同,只需要可以传递参数到 shader 中即可,比如在uv,color 等属性上记录;后续在 UI,大规模场景渲染中具备很大的发挥潜力




# 填充率(FillRate)
##### OverDraw
* 检查标准,总填充数,平均填充倍数,单像素最大填充数(某一帧,某一点上面,过量的填充,比如粒子特效).
* 半透明区域像素在一帧中被渲染多遍,UI 界面,粒子特效.全屏 UI,可以尝试 Deavtive 主场景的(背后场景的,其他的) Camera; 半屏 UI,可以根据需求将主场景(背后的场景,其他的场景)渲染成背景 RT;
* mask 组件的 overdraw,使用 Rect Mask; 
* 使用 Empty Click UI,空纹理检测点击,无渲染;
* 对常驻的粒子特效,比如烟雾,使用插件 OffScreen Particle Rendering 
* Scene View 里面可以看到 Overdraw 不透明也可以叠加,可能不准确,但是足够,使用的方式是 Shader Replacement 做的;透明图片,4 层之后会降低帧率
* 天空盒肯定会先画一遍;Over Shading 出现在小面积或者狭长 Triangle 渲染时,GPU 并不是单像素采样,而是 2x2 方形采样;

##### Over Shading 过着色
* 出现在小面积或狭长 Triangle 渲染时.三角形占用 4 个像素,但是 GPU 画图时,会一次性画很多像素,其他画出的像素就浪费性能.
* 找到使用率低的 Mesh,进行线下简化
* Level Of Detail,进行实时简化
##### 网格资源渲染密度分析
* 渲染网格模型在每单位像素中,网格顶点的渲染数量
* 渲染网格模型平均每帧的渲染像素值

# 带宽(Bandwidth)

# Shader复杂度(ALU)


# 后期处理效果

* 1:屏幕区域后效区分化(不同区域强度差异),特效,角色,天空,互相映照会影响彼此,需要区分.
* 2:Distort 效果兼容(需要 Grad)
* 3:不同对象后效的去分化处理,同一对象不同部位区分化
* 4:Blit 带宽性能瓶颈
* 5:Stencil buffer 在 OnRenderImage 之前会被 Clear,依赖 Depth Buffer

# 问题出在哪里?
* 不透明物体的渲染
```
MeshRenderer.Render 非蒙皮网格渲染
Mesh.DrawVBO        Draw Call
MeshSkinning.Render 蒙皮网格渲染
Material.SetPassFast 设置渲染状态. 调用次数与材质数目成正比,与批次成正比,是在每次提交 Draw Call之前,修改 Render State(混合,深度测试等)
对于不透明物体的渲染,进行优化,需要:减少 Draw Call,减少材质,减少面片数.
对于不透明物体的耗时:与物体个数(Draw Call)成正比,与单个物体面片数不敏感,与场景中使用材质数目成正比;对于 CPU 端耗时的影响程度:物体个数>材质数据>单个物体面片数
```
* 半透明物体的渲染
```
MeshRenderer.Render 场景中半透明渲染
Mesh.DrawVBO        Draw Call
Mesh.CreateVBO      NGUI的消耗一般在这个地方
ParticleSystem.ScheduleGeometryJobs    与在相机窗口内部的粒子系统个数成正比 >> 子线程调用ParticleSystem.GeometryJobs方法
ParticleSystem.SubmitVBO 粒子系统渲染
BatchRenderer.Add   UGUI的消耗一般藏在这里
对于粒子系统,进行优化,需要:减粒子系统个数,减粒子个数

```