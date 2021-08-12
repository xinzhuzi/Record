# 遮罩

# Mask
* 1. Mask组件必须要依赖一个Image组件，裁剪区域就是Image的大小,子节点在Image渲染区域才会显示。主要处理不规则图形遮罩效果
     
* 2. Mask+Image 是 2 个 Batch,这 2 个 Batch 是 Mask 头与 Mask 尾.

* 3. 多个 Mask 组合,没有子节点,并且线框没有重叠,相同的图集可以合批,在相同的 Depth 上,相同的图集.

* 4. Mask 子节点可以与外界的 Mask 子节点合批,不能与外界的 UI 节点合批.
     
* 5. 把 Mask + Image当成一个节点,但是不与普通节点一样.
     Mask 节点无法和没有 Mask的 Image 节点合批,
     但多个Mask内的UI节点间如果符合合批条件，可以合批(Mask 节点在相同的 Hierarchy 层次节点,有相同的图集才会合批)
     且子节点也会合批,相同Mask节点类型下的子节点也会合批.
     
* 6. Mask嵌套不能超过 9 层, 在源码Mask.cs 的 GetModifiedMaterial 方法中.




# Mask原理

* 1. 引用文章:https://zhuanlan.zhihu.com/p/339378916
     
* 2. Mask 原理: Mask 是利用了 GPU 的模板缓冲来实现的,在Mask.cs的 GetModifiedMaterial方法中 StencilMaterial.Add(xxx)是最核心的内容
     它的作用是为 Mask 对象生成一个特殊的材质,这个材质会将 StencilBuffer 的值置为 1
     在其他 MaskableGraphic 组件中,也有相似的代码,作用是为MaskableGraphic生成一个特殊的材质,
     这个材质在渲染时会取出StencilBuffer的值,判断是否为 1,如果是才渲染.

* 3. 能不用就不用,应用场景一般是在特别花哨的界面上面.ScrollView 一般采用的是 RectMask2D 方案



![Mask头尾](Mask头尾.png)


# RectMask2D

* 1. RectMask2D只有 2 个组件,一个是 RectTransform,一个RectMask2D,以 RT 为裁剪区域,子节点在 RT 区域内显示.只能做矩形遮罩.
     
* 2. 看官方手册https://docs.unity3d.com/cn/current/Manual/script-Mask.html
     
* 3. 如果只用固定矩形遮罩,不需要特殊形状遮罩的情况下,可以优先使用 RectMask2D,如果当前设计只有一个遮罩,那么也用RectMask2D.使用 Frame Debugger 对比DrawMesh即可.

* 4. RectMask2D节点下的所有孩子都不能与外界UI节点合批,且多个RectMask2D之间不能合批。RectMask2D节点与外部UI节点上的 image 是可以合批的.

* 5. 计算depth的时候，所有的RectMask2D都按一般UI节点看待，只是它没有CanvasRenderer组件,并且会打断合批.
     在源码 RectMask2D.cs 的 PerformClipping 方法中,他并没有替换材质的过程，没有用到模板缓冲的实现方式，而是通过了canvasRender里面进行了ClipRect的剔除

* 6. 可以节省 DrawCall 与 Overdraw,自身不增加 DC,会增加 Cull(裁剪) 开销(SendWill),持续开销较低,拖动时开销较高.
     如果滚动的时候比较卡顿,需要尝试使用 mask 来替换测试一下.权衡一下 2 个组件,哪个可以接受.

![RectMask2D](RectMask2D.png)




# 通用性总结

* 1. 当一个界面只有一个mask，那么，RectMask2D 优于 Mask
     当需要两个遮罩，并且 Mask 可以进行合批，两者相差不大。
     当大于两个mask，那么，Mask 优于 RectMask2D。
     最终还是要看 Frame Debugger 上面的 DrawMesh 多少来判断用哪个组件比较好.

     
# 引用文章

* 1. https://www.cnblogs.com/moran-amos/p/13883818.html
     
* 2. https://www.pianshen.com/article/21261176829/
     
* 3. https://www.pianshen.com/article/21261176829/

* 4. https://docs.unity3d.com/cn/current/Manual/script-Mask.html


