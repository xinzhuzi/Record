---
title: Unity 优化总纲
date: 2020-05-08 11:41:32
top: 20
categories:
- Unity优化
tags:
- Unity优化
---

# 帧率

# 后期处理效果

* 1:屏幕区域后效区分化(不同区域强度差异),特效,角色,天空,互相映照会影响彼此,需要区分.
* 2:Distort 效果兼容(需要 Grad)
* 3:不同对象后效的去分化处理,同一对象不同部位区分化
* 4:Blit 带宽性能瓶颈
* 5:Stencil buffer 在 OnRenderImage 之前会被 Clear,依赖 Depth Buffer




# 阴影方案

# shader 优化

# 多核的利用