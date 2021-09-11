---
title: Unity 优化总纲
date: 2020-05-08 11:41:32
top: 505
categories:
- Unity优化
tags:
- Unity优化
---

# 优化总纲



* 1. UI 优化;链接: https://pan.baidu.com/s/1vPUeK-am-240LpJ-RbDfow 提取码: covu 

* 2. Lua 优化工具;  https://github.com/leinlin/Miku-LuaProfiler

* 3. Unity 官方工具;  https://upr.unity.cn/

* 4. UWA 官方工具;  https://www.uwa4d.com/

* 5. UWA 优化视频;  链接: https://pan.baidu.com/s/1IMfw-TW8u4SOfPLGKty10w 提取码: 61d1 

* 6. 优化文章;      https://thegamedev.guru/#

* 7. Unity 官方文章; https://developer.unity.cn/plate/performance 中文:https://learn.u3d.cn/tutorial/mobile-game-optimization , https://docs.unity3d.com/cn/2020.3/Manual/analysis.html

* 8. Unity：电子游戏优化终极指南: https://www.bilibili.com/video/BV1RQ4y1C7v8


# 编程代码规范

* 1:脚本行数最多500行。
* 2:如果要发行其他国家的版本需要重新Copy一份客户端代码,在另行修改
* 3:c#扩展做链式语法非常容易,比如项目里面的WWWFormEx类的数据装载,非常好写.
* 4:写好代码一定要多个平台测试
* 5:编程代码规范不是一成不变的,需要根据人数,风格进行有效的变换


# 善用工具检测

* 1:使用Unity自带的工具 [profile](https://unity3d.com/cn/learn/tutorials/temas/performance-optimization/diagnosing-performance-problems-using-profiler-window?playlist=44069),Frame Debug,Physics Debug,[数据类型](https://unity3d.com/cn/learn/tutorials/topics/scripting/data-types),[对象池](https://unity3d.com/cn/learn/tutorials/topics/scripting/object-pooling),[官方视频](https://learn.unity.com/tutorial/fixing-performance-problems#),
* 2:使用 UPR
* 3:使用 UWA
* 4:使用 XCode 编辑器
* 5:使用 Android studio 编辑器
* 6:君子善用其器.一定要先了解一下这个工具再对其进行使用.


# 宏观性能关注点

* 1:FPS 帧率需要大于 30 帧
* 2:PSS 内存,越低越好,但需要根据渠道而定
* 3:Mono峰值,小于 40M
* 4:温度均值,越低越好
* 5:能耗均值,电量,越低越好
* 6:网络上传
* 7:网络下载


# 微观性能关注点

模块    |          前期           |         中期        |后期&上线
-------|-------------------------|--------------------|--------------
渲染模块|Draw Call,Triangle,vertex|不透明,半透明,Culling|图像后处理
逻辑代码| 插件,第三方库调研,bug      |CPU,堆内存,调用次数  |bug
UI 模块|全屏,半屏,组织结构          |overdraw,重建 CPU   |Draw Call
UGUI的API|Canvas.BuildBatch,Canvas.SendWillRenderCanvases|EventSystem.Update|RenderSubBatch
加载模块|缓存池,序列化第三方库        |关注调用频率        |关注耗时
加载模块的API|Loading.UpdatePreloading,Resources.UnloadUnusedAssets|GameObject.Instantiate|GC.Collect
资源使用|分辨率,格式,顶点数,骨骼数    |数量,Mipmap,资源利用率,沉余|调用次数
内存占用|资源,AB 包,Mono,Lua       |内存峰值,堆内存      |内存泄露
粒子系统|使用指标                  |总体数量,active 数量 |Overdraw
粒子系统的 API|ParticleSystem.Update,ParticleSystem.SubmitVBO,ParticleSystem.Draw|ParticleSystem.ScheduleGeometryJobs
动画系统|                        |数量,AC 制作,CPU      |
动画系统的 API|Animators.Update,Animation.Update|MeshSkinning.Update|Animator.Initialize


# 官方数值建议


数值名称    |     官方建议数值
-----------|-------------------------
ReservedMono峰值(MB)|小于 80MB
DrawCall峰值(次)| 小于 250 次
平均帧率|大于 25
纹理资源峰值(MB)|小于 50MB    
网格资源峰值(MB)|小于 20MB
动画资源峰值(MB)|小于 15MB
音频资源峰值(MB)|小于 15MB
Tris 峰值(面)  |小于 200000(20W 面)             


# CPU 优化

* 1:简单代码控制,避免CPU资源浪费
* 2:避免使用闭包
* 3:MonoBehaviour优化
* 4:Component的优化
* 5:GameObject的优化


# GPU 优化



# 内存优化



# UI 优化

* 1:NGUI 的优化
* 2:UGUI 的优化


# 资源优化

* AssetBundle
* 打包


# 渲染优化

* shader


# 扩展阅读

* 1:[守望先锋:ECS架构](http://gad.qq.com/article/detail/28682),[Unity官方文档](https://docs.unity3d.com/Packages/com.unity.entities@0.1/manual/index.html),[云风](http://blog.codingnow.com/2017/06/overwatch_ecs.html),[ECS博客](http://t-machine.org/index.php/2007/09/03/entity-systems-are-the-future-of-mmog-development-part-1/),[国外谈论](https://www.youtube.com/watch?v=lNTaC-JWmdI),[Entitas-CSharp]( https://github.com/sschmid/Entitas-CSharp),[ET框架](https://github.com/egametang/ET)
* 2:推荐想学习优化的同学一本书,<<Unity 游戏优化 2>>,作者是:克里斯*迪金森,翻译:蔡俊鸿,雷鸿飞.


# 代码控制

* 1: 尽量不用foreach,全使用for,因为foreach产生GC
* 2: 字典替换成下面的写法

        var enumerator = m_Dictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var element = enumerator.Current;
            element.Value.UpdateComponent(deltaTime);
        }
* 3: 字符串 + 号拼接不超过约10次,不会产生GC,如果超过10次,要使用StringBuilder进行拼接,不会产生GC
* 4: Struct 与 Class  如何装箱或拆箱少，堆内存少,Struct 在栈中不产生 GC，class 在堆中，会产生 GC。对 Struct 的结点修改时，修改完以后记得重新赋值。因为 Struct 赋值是 copy
而不是引用，修改完以后，以前的不生效。
* 5: 堆栈的空间有限，对于大量的逻辑的对象，创建类要比创建结构好一些。
结构表示轻量对象，并且结构的成本较低，适合处理大量短暂的对象。
在表现抽象和多级别的对象层次时，类是最好的选择。
大多数情况下该类型只是一些数据时，结构是最佳的选择。
* 6: 数组，ArrayList，List 的区别;

>数组：内存中是连续存储的，索引速度非常快，赋值与修改元素也很简单。但不
利于动态扩展以及移动。
>ArrayList: 因为数组的缺点，就产生了 ArrayList。ArrayList：使用该类时必须进行引用，同时继承了 IList 接口，提供了数据存储和检索，ArrayList 对象的大小动态伸缩，支持不同类型的结点。
ArrayList 虽然很完美，但结点类型是 Object，故不是类型安全的，也可能发生
装箱和拆箱操作，带来很大的性能耗损。对象是值类型的话会带来装箱拆箱操作
>List 是泛型接口，规避了 ArrayList 的两个问题。利于动态扩展以及移动,但是搜索速度慢

* 7: 不要把枚举当 TKey (字典的key)使用，不要把枚举转成 string 使用。

# 闭包

* 1:变量的作用域,成员变量作用于类、局部变量作用于函数、次局部变量作用于函数局部
片段。生命周期：变量随着其寄存对象生而生和消亡（不包括非实例化的 static 和 const
对象）。
* 2:委托概念：是一个类型安全的对象，它指向程序中另一个以后会被调用的方法（或
多个方法）。通俗的说，委托是一个可以引用方法的对象，当创建一个委托，也
就创建一个引用方法的对象，进而就可以调用那个方法，即委托可以调用它所指
的方法。如何没有涉及到闭包的话，委托代码只生产一个函数而不是一个类。lamda表达式(闭包)
* 3:    闭包概念：函数和与其相关的引用环境组合而成的实体。本质 1：代码块依然维护着它第一个被创建时环境（执行上下文）- 即它仍可以使用创建它的方法中局部变量，即使那个方法已经执行完了。(循环引用不释放);本质 2 Closures close over variables, not over values。闭包关闭的是变量，而不是值.闭包引用了外部变量就会生成一个新得类.函数调用频繁不使用闭包

# MonoBehaviour 优化

* 1:如果没有相应的事件处理，删除对应的空函数
* 2:Update 优化 在 update 中尽量不要调用查找对象或组件方法如 FindByTag 或 Find 等等。可
以在 start 中先缓存下来，然后使用。 如果没必要每帧的逻辑，可以降低频率，方法如下：

        Void Update（）{if（Time.frameCount%6==0）{DoSomething();}}
* 3:如果没必要每帧的逻辑，可以使用周期性的协程,如果没必要每帧的逻辑，可以使用InvokeRepeating("DoSomeThing",0.5f,1.0f);
* 4:Gameobject 不可见时，设置 enabled = false 时，update 就会停止调用。
* 5:协程有优化:yield return null;每帧产生9个字节的GC垃圾,其余函数也会产生GC垃圾,需要使提前预生成方式

        WaitForSeconds wfs = new WaitForSeconds(0.1f);
        IEnumerator AtlasTextureSetting()
        {
            yield return wfs;
        }

# Component 优化

* 1:使用内建的数组，比如用 Vector3.zero 而不是 new Vector(0, 0, 0);
* 2:transform.localRotation = Quaternion.Euler(Vector3.zero);transform.localScale =Vector3.one;transform.localPosition =Vector3.one;等

# GameObject 相关优化

* 1:（脚本和本地引擎 C++代码之间的通信开销）Gameobject 缓存：类似组件的缓存策略。查找对象标签：if (go.CompareTag (“xxx”)来代替 if (go.tag == “xxx”)，因为内部循环调用对象分配的标签属性以及拷贝额外内存。SendMessageUpwards、SendMessage：少用这两个函数，使用委托替代。缓存组件：调用 GetComponent 函数耗性能，用变量先缓存到内存在使用, 有必要时记得更新缓存组件。

# NGUI 相关优化

* 1: Canvas.BuildBatch()，
合批 Canvas 下所有网格，这个性能热点在 5.2 版本后挪到了子线程去做减轻了
主线程的压力，而 NGUI 作为一个插件没法做到这一点，网格合批的性能热点还
是耗在主线程的 UIPanel.LateUpdate()；
* 2 : UGUI 的 UIMesh 生成是通过底层 C++代码实
现的，而 NGUI 只能通过上层的不断创建 Vertex List，这样在堆内存的管理上，
UGUI 确实要好很多，带来的隐形收益就是 GC 触发次数会少很多。

# UI 资源规范（内存优化）

* 1:任何的 UI 图集最大 size 1024*1024（内存优化）；
* 2:同一个界面出现的 UI 资源尽量放到一个图集，重复利用的公用资源放
common（DrawCall 优化）；
* 3:能用九宫格的尽量用九宫格来减小原图大小（内存优化）；
* 4.美术给过来的 UI 原图 size 尽量小，对于一些全屏的 loading 原画图，原画大
小是 1280 * 720，让美术按照比例高度缩小到 500，
这样一张 1024*1024 的图集就可以放两张原图了，提升图集利用率。对于一些
600*400 类似大小的原图，就尽量按比例把最长边压小到 500，这样出来的图
集就是 512 * 512 而不是 1024 * 1024（内存优化）；
* 5.对于特别长条的 UI 原图，例如 1000*100，如果由于加入这个长条的原图导
致图集大小变大而且利用率很低的话，要把 1000*100 的原图拆分成两张图
500*100，在制作界面的时候用两个 Image 拼接即可，这样可以把 1024 的图
集缩小到 512（内存优化）；
* 6.图集利用率低于 1/3 的时候，要考虑和其他同一个 size 的图集合并以提升利
用率。合并的原则是不改变任何一个图集的大小，这样即可完全省掉一张图集（内
存优化、安装包量优化）；
* 7.尽量复用 UI 资源，减少不必要的原图，例如一个卡牌分了五种品质原画底图，
白蓝黄绿紫，就不要使用五张大底图了，让美术同事画一个灰色原图，Image 在
使用的时候直接按需求修改顶点色即可（内存优化）；
* 8.关闭 mipmaps（内存优化）。

# GPU 优化

#### Shader优化

* 1:Fog { Mode Off }，最早有一个版本我们没有关闭 Fog
* 2:Fragment 剔除掉 Alpha 为 0 的像素点，减少 OverDraw；
* 3:OverDraw 优化,在每帧绘制中，如果一个像素被反复绘制的次数越多，那么它占用的资源也必然
更多。目前在移动设备上，OverDraw 的压力主要来自半透明物体。因为多数情
况下，半透明物体需要开启 Alpha Blend 且关闭 ZWrite，同时如果我们绘制
像 alpha=0 这种实际上不会产生效果的颜色上去，也同样有 Blend 操作，这是
一种极大的浪费。我们的 UI 绘制是 Alpha Blend 且关闭 ZWrite，因此 UI OverDraw 的优化主要
是在制作界面的时候减少 UI 重叠层级（和策划、美术 pk）。除此之外还是有一
些我们程序可以控制的优化点：1.对于九宫格的 Image，如果去掉 fillcenter 不影响最后出来的效果就要把
fillcenter 去掉，可以减少中间一片的像素绘制；2.看不见的元素且没有逻辑功能要 disable 或者挪出裁剪区域，而不要通过设置Alpha=0 来隐藏；3.不要使用一张 Alpha=0 的 Image 来实现放大响应区域的功能；4.UI 底层系统来控制隐藏看不见的元素，例如打开全屏 UI 的时候把下面看不见的 UI 挪出裁减区域、关闭主相机渲染。

#### CPU优化

* 1:优化DrawCall、Canvas.SendWillRenderCanvases()、Canvas.BuildBatch()
* 2:DrawCall,DrawCall 是 CPU 调用底层图形接口，频繁的调用对 CPU 性能的影响是很明显
的。优化思路很简单，合批绘制。UGUI 本身的动态合批机制会帮我们尽量的去
优化合批，我们要做的就是弄清楚它的合批机制然后让 UI 元素尽量合批绘制。合理分配图集，同一个界面上的图尽量打到一个图集，多个界面复用的图，放到 common；
* 3:制作界面的时候，相邻节点尽量使用同一个图集的图片；
* 4:Text 本身也是用的 Font Texture，不同字体的 Text 也是来自不同的图集，所
以在布局界面的时候也要尽量避免穿插打断绘制流程；
* 5:DrawCall 的数量不是完全由 Hierarchy 的布局决定，和 UI 的位置也有关系，
这个位置不是指的 Rectranform 上面的 size 位置重叠就一定打断绘制，而是真
实的三角面的位置是否重叠。这个可以在 Scene 视图下用线框模式(Texture
Wire)去观察；
* 6:.少用 Mask 组件，Mask 实现的原理是 Stencil Buffer，往模版缓存里绘制，
模版缓存里的东西才是可见的。模板缓存会打断所有的合批，Mask 的子节点和
外面的节点无法合批，模板缓存自己占一个 DrawCall。Unity5.2 之后的版本建
议使用 2D Rect Mask 替代。

# Profiler介绍及优化

* 1: WaitForTargetFPS: Vsync(垂直同步)功能，即显示当前帧的CPU等待时间
* 2: Camera.Render: 相机渲染准备工作的CPU占用量 
* 3: Shader.Parse: 资源加入后引擎对Shader的解析过程
* 4: Reserved Total:系统在当前帧的申请内存
* 5: GameObjects in Scene:当前帧场景中的GameObject数量
* 6: Total Objects in Scene:当前帧场景中的Object数量(除GameObject外，还有Component等). 
* 7: Total Object Count: Object数据 + Asset数量. 
* 8: Assets: Texture2d:记录当前帧内存中所使用的纹理资源情况，包括各种GameObject的纹理、天空盒纹理以及场景中所用的Lightmap资源.
* 9: Scene Memory:记录当前场景中各个方面的内存占用情况，包括GameObject、所用资源、各种组件以及GameManager等（天般情况通过AssetBundle加载的不会显示在这里). 
* 10: Other:ManagedHeap.UseSize:代码在运行时造成的堆内存分配，表示上次GC到目前为止所分配的堆内存量. SerializedFile(3): WebStream:这个是由WWW来进行加载的内存占用. System.ExecutableAndDlls:不同平台和不同硬件得到的值会不一样。 

#####[优化重点](https://blog.csdn.net/yangyy753/article/details/47025205)
> A:CPU-GC Allow:1.检测任何一次性内存分配大于2KB的选项 2.检测每帧都具有20B以上内存分配的选项. 
> B:Time ms:记录游戏运行时每帧CPU占用（特别注意占用5ms以上的）. 
> C:Memory Profiler-Other:1.ManagedHeap.UsedSize: 移动游戏建议不要超过20MB. 2.SerializedFile: 通过异步加载(LoadFromCache、WWW等)的时候留下的序列化文件,可监视是否被卸载.  3.WebStream: 通过异步WWW下载的资源文件在内存中的解压版本,比SerializedFile大几倍或几十倍,重点监视.
> D: Memory Profiler-Assets: 1.Texture2D: 重点检查是否有重复资源和超大Memory是否需要压缩等. 2.AnimationClip: 重点检查是否有重复资源.  3.Mesh： 重点检查是否有重复资源. 
> E:Device.Present: 1.GPU的presentdevice确实非常耗时，一般出现在使用了非常复杂的shader. 
2.GPU运行的非常快，而由于Vsync的原因，使得它需要等待较长的时间. 
3.同样是Vsync的原因，但其他线程非常耗时，所以导致该等待时间很长，比如：过量AssetBundle加载时容易出现该问题.4.Shader.CreateGPUProgram:Shader在runtime阶段（非预加载）会出现卡顿(华为K3V2芯片). 
> F:StackTraceUtility.PostprocessStacktrace()和StackTraceUtility.ExtractStackTrace():  1.一般是由Debug.Log或类似API造成. 2.游戏发布后需将Debug API进行屏蔽. 
> G:GC.Collect: 原因: 1.代码分配内存过量(恶性的) 2.一定时间间隔由系统调用(良性的). 占用时间：1.与现有Garbage size相关 2.与剩余内存使用颗粒相关（比如场景物件过多，利用率低的情况下，GC释放后需要做内存重排) 
> H:GarbageCollectAssetsProfile:1.引擎在执行UnloadUnusedAssets操作(该操作是比较耗时的,建议在切场景的时候进行). 2.尽可能地避免使用Unity内建GUI，避免GUI.Repaint过渡GC Allow. 3.if(other.tag == GearParent.MogoPlayerTag)改为other.CompareTag(GearParent.MogoPlayerTag).因为other.tag为产生180B的GC Allow.
> I:少用foreach，因为每次foreach为产生一个enumerator(约16B的内存分配)，尽量改为for. Lambda表达式，使用不当会产生内存泄漏. 尽量少用LINQ:1.部分功能无法在某些平台使用. 2.会分配大量GC Allow.
> J:控制StartCoroutine的次数：  1.开启一个Coroutine(协程)，至少分配37B的内存. 2.Coroutine类的实例 -- 21B.  3.Enumerator -- 16B.缓存组件: 1.每次GetComponent均会分配一定的GC Allow. 2.每次Object.name都会分配39B的堆内存.
> K:1:许多贴图采用的Format格式是ARGB 32 bit所以保真度很高但占用的内存也很大。在不失真的前提下，适当压缩贴图，使用ARGB 16 bit就会减少一倍，如果继续Android采用RGBA Compressed ETC2 8 bits（iOS采用RGBA Compressed PVRTC 4 bits），又可以再减少一倍。把不需要透贴但有alpha通道的贴图，全都转换格式Android：RGB Compressed ETC 4 bits，iOS：RGB Compressed PVRTC 4 bits。2:当加载一个新的Prefab或贴图，不及时回收，它就会永驻在内存中，就算切换场景也不会销毁。应该确定物体不再使用或长时间不使用就先把物体制空(null)，然后调用Resources.UnloadUnusedAssets()，才能真正释放内存。3:有大量空白的图集贴图，可以用TexturePacker等工具进行优化或考虑合并到其他图集中。4:要保证每张图得像素宽高都是4得倍数,即除4余0.
> L:AudioClip:播放时长较长的音乐文件需要进行压缩成.mp3或.ogg格式，时长较短的音效文件可以使用.wav 或.aiff格式。


