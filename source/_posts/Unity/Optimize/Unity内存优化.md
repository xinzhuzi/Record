---
title: Unity 内存优化
date: 2020-05-08 11:41:32
top: 10
categories:
- Unity优化
tags:
- Unity优化
---



# 参考资料
https://docs.unity.cn/cn/current/Manual/BestPracticeGuides.html
https://www.bilibili.com/video/BV1aJ411t7N6
https://zhuanlan.zhihu.com/p/61105374

# GC
* 1:[GC优化](https://mp.weixin.qq.com/s/ARNJtujrHKgxBWuanO0tpA)

# 内存分类
Unity 3D中的内存管理 http://onevcat.com/2012/11/memory-in-unity3d/

# 资源内存
https://www.cnblogs.com/88999660/archive/2013/03/15/2961663.html


# 概念理解

#### 内存
* 物理内存:CPU 访问内存是一个慢速过程,减少Cache Miss,ECS 和 DOTS
* 虚拟内存:内存交换,移动设备不支持内存交换,iOS 可以进行内存压缩,Android 没有内存压缩能力
* 内存杀手 low memory killer(AKA lmk),内存不足时，killer 会出现，从上图底层一层一层地向上杀。（Cached-Previous-Home...）
![内存杀手](内存杀手.png)

#### Unity 内存管理

- Unity 是一个 C++ 引擎
    - 底层代码完全由 C++ 写成
    - 通过 Wrapper 提供给用户 API ; .binding 链接 C#与 C++的语言,目前逐渐转成 C#
    - 用户代码会转换为 CPP 代码 （il2cpp）
        - VM 仍然存在（il2cpp vm）,主要是为了跨平台
    - Editor ,package,都是用 C#写的
- Unity 内存按照分配方式分为：
    - Native Memory
    - Managed Memory
    - Editor & Runtime 是完全不同的
        - 不止是统计看到的内存大小不同，甚至是内存分配时机和方式也不同
        - Asset 在 Runtime 中如果不读取，是不会进内存的，但 Editor 打开就占内存。因为 Editor 不注重 Runtime 的表现，更注重编辑器中编辑时的流畅。
            - 但如果游戏庞大到几十个 G，如果第一次打开项目，会消耗很多时间，有的大的会几天，甚至到一周。
- Unity 内存按照管理者分为：
    - 引擎管理内存
    - 用户管理内存（应优先考虑）
- Unity 检测不到的内存
    - 用户分配的 native 内存
        - 自己写的 Native 插件（C++ 插件）， Unity 无法分析已经编译过的 C++ 是如何去分配和使用内存的。
        - Lua 完全由自己管理内存，Unity 无法统计到内部的使用情况。

#### Unity 游戏应用的4种内存类型
* 1: Unity底层(C++层,本机堆,核心代码,unityengine.dll等一系列别称)占用的内存,包含Scene,Audio,CodeSize,贴图等.Unity使用了自己的一套内存管理机制来使这块内存具有和托管堆类似的功能。基本理念是，如果在这个关卡里需要某个资源，那么在需要时就加载，之后在没有任何引用时进行卸载。听起来很美好也和托管堆一样，但是由于Unity有一套自动加载和卸载资源的机制，让两者变得差别很大。自动加载资源可以为开发者省不少事儿，但是同时也意味着开发者失去了手动管理所有加载资源的权力，这非常容易导致大量的内存占用（贴图什么的你懂的），也是Unity给人留下“吃内存”印象的罪魁祸首。       

* 2: Unity的托管堆（Managed Heap）,托管内存,也被称为用户管理的内存,[Mono](http://www.mono-project.com/Main_Page)项目一个开源的.net框架的一种实现，对于Unity开发，其实充当了基本类库的角色。托管堆用来存放类的实例（比如用new生成的列表，实例中的各种声明的变量等）。“托管”的意思是Mono“应该”自动地改变堆的大小来适应你所需要的内存，并且定时地使用垃圾回收（Garbage Collect）来释放已经不需要的内存。关键在于，有时候你会忘记清除对已经不需要再使用的内存的引用，从而导致Mono认为这块内存一直有用，而无法回收。profile可以检测到.并且需要手机查找才正常,经常需要优化的部分.

* 3: 代码文件的内存.如lua脚本本身的内存.C#泛型类的内存,C++层会将泛型编译成静态类型就多出很多文件,尽量注意不要使用太多泛型模板类,shader写的一个,但是编译成很多份的文件等,都是代码文件会占用的内存.
程序代码包括了所有的Unity引擎，使用的库，以及你所写的所有的游戏代码。在编译后，得到的运行文件将会被加载到设备中执行，并占用一定内存。这部分内存实际上是没有办法去“管理”的，它们将在内存中从一开始到最后一直存在。一个空的Unity默认场景，什么代码都不放，在iOS设备上占用内存应该在17MB左右，而加上一些自己的代码很容易就飙到20MB左右。想要减少这部分内存的使用，能做的就是减少使用的库。 

* 4: 第三方库的占用的内存,也被称为用户管理的内存,但是已经和unity的内存关系上完全无关了,如自己使用C++编译的库,tolua占用的内存,lua文件占用的内存(这个不是unity管理的),lua占用的内存. 并且unity编辑器的profile无法检测到.

# Unity Native Memory 管理以及优化
- Unity 重载了所有分配内存的操作符（C++ alloc、new），使用这些重载的时候，会需要一个额外的 memory label （Profiler-shaderlab-object-memory-detail-snapshot，里面的名字就是 label：指当前内存要分配到哪一个类型池里面）
- 使用重载过的分配符去分配内存时，Allocator 会根据你的 memory label 分配到不同 Allocator 池里面，每个 Allocator 池 单独做自己的跟踪。因此当我们去 Runtime get memory label 下面的池时就可以问 Allocator，里面有多少东西 多少兆。
- Allocator 在 NewAsRoot 中生成,生成了一个 memory island(root)。在这个 memory island(root) 下面会有很多子内存：shader：当我们加载一个 Shader 进内存的时候，会生成一个 Shader 的 root。Shader 底下有很多数据：sub shader、Pass 等会作为 memory (root) 的成员去依次分配。因此当我们最后统计 Runtime 的时候，我们会统计 Root，而不会统计成员，因为太多了没法统计。
- 因为是 C++ 的，因此当我们 delete、free 一个内存的时候会立刻返回内存给系统，与托管内存堆不一样。

- Scene
    - Unity 是一个 C++ 引擎，所有实体最终都会反映在 C++ 上，而不是托管堆里面。因此当我们实例化一个 GameObject 的时候，在 Unity 底层会构建一个或多个 Object 来存储这个 GameObject 的信息，例如很多 Components。因此当 Scene 有过多 GameObject 的时候，Native 内存就会显著上升。
    - 当我们看 Profiler，发现 Native 内存大量上升的时候，应先去检查 Scene。
- Audio
    - DSP buffer （声音的缓冲）
        - 当一个声音要播放的时候，它需要向 CPU 去发送指令——我要播放声音。但如果声音的数据量非常小，就会造成频繁地向 CPU 发送指令，会造成 I\O。
        - 当 Unity 用到 FMOD 声音引擎时（Unity 底层也用到 FMOD），会有一个 Buffer，当 Buffer 填充满了，才会向 CPU 发送 "我要播放声音" 的指令。
        - DSP buffer 会导致两种问题：
            - 如果（设置的） buffer 过大，会导致声音的延迟。要填充满 buffer 是要很多声音数据的，但声音数据又没这么大，因此会导致一定的**声音延迟**。
            - 如果 DSP buffer 太小，会导致 CPU 负担上升，满了就发，消耗增加。

            [Audio](https://docs.unity3d.com/Manual/class-AudioManager.html)

    - Force to mono
        - 在导入声音的时候有一个设置，很多音效师为了声音质量，会把声音设为双声道。但 95% 的声音，左右声道放的是完全一样的数据。这导致了 1M 的声音会变成 2M，体现在包体里和内存里。因此一般对于声音不是很敏感的游戏，会建议改成 Force to mono，强制单声道。
    - Format
    - Compression Format（看文档，有使用建议）
- Code Size
    - C++ 模板泛型的滥用,会影响到 Code Size、打包的速度。会翻译成 CPP 文件,如果泛型过多,则会造成 CPP 过大,每一个泛型都会生成一个对应的 class,都会变成静态类型.
        - 可以参考 [Memory Management in Unity](https://learn.unity.com/tutorial/memory-management-in-unity) 3.IL2CPP & Mono 的 Generic Sharing 部分。
- AssetBundle
    - TypeTree
        - Unity 的每一种类型都有很多数据结构的改变，为了对此做兼容，Unity 会在生成数据类型序列化的时候，顺便会生成 TypeTree：当前我这一个版本里用到了哪些变量，对应的数据类型是什么。在反序列化的时候，会根据 TypeTree 来进行反序列化。
            - 如果上一个版本的类型在这个版本中没有，TypeTree 就没有它，因此不会碰到它。
            - 如果要用一个新的类型，但在这个版本中不存在，会用一个默认值来序列化，从而保证了不会在不同的版本序列化中出错，这个就是 TypeTree 的作用。
        - Build AssetBundle 中有开关可以关掉 TypeTree。当你确认当前 AssetBundle 的使用和 Build Unity 的版本一模一样，这时候可以把 TypeTree 关掉。
            - 例如如果用同样的 Unity 打出来的 AssetBundle 和 APP，TypeTree 则完全可以关掉。
        - TypeTree 好处：
            - 内存减少。TypeTree 本身是数据，也要占内存。
            - 包大小会减少，因为 TypeTree 会序列化到 AssetBundle 包中，以便读取。
            - Build 和运行时会变快。源代码中可以看到，因为每一次 Serialize 东西的时候，如果发现需要 Serialize TypeTree，则会 Serialize 两次：
                - 第一次先把 TypeTree Serialize 出来
                - 第二次把实际的东西 Serialize 出来
                - 反序列化也会做同样的事情，1. TypeTree 反序列化，2. 实际的东西反序列化。
            - 因此如果确定 TypeTree 不会对兼容性造成影响，可以把它关掉。这样对 Size 大小和 Build Runtime 都会获得收益。

    - 压缩方式：
        - Lz4

            [BuildCompression.LZ4](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/BuildCompression.LZ4.html)

            - LZ4HC "Chunk Based" Compression. 非常快
            - 和 Lzma 相比，平均压缩比率差 30%。也就是说会导致包体大一点，但是（作者说）速度能快 10 倍以上。
        - Lzma

            [BuildCompression.LZMA](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/BuildCompression.LZMA.html)

            - Lzma 基本上就不要用了，因为解压和读取速度上都会比较慢。
            - 还会占大量内存
                - 因为是 Steam based 而不是 Chunk Based 的，因此需要一次全解压
                - Chunk Based 可以一块一块解压
                    - 如果发现一个文件在第 5-10 块，那么 LZ4 会依次将 第 5 6 7 8 9 10 块分别解压出来，每次（chunk 的）解压会重用之前的内存，来减少内存的峰值。
        - 预告：中国版 Unity 会在下个版本（1月5号或2月份）推出新的功能：基于 LZ4 的 AssetBundle 加密，只支持 LZ4。
        - Size & count
            - AssetBundle 包打多大是很玄学的问题，但每一个 Asset 打一个 Bundle 这样不太好。
                - 有一种减图片大小的方式，把 png 的头都提出来。因为头的色板是通用的，而数据不通用。AssetBundle 也一样，一部分是它的头，一部分是实际打包的部分。因此如果每个 Asset 都打 Bundle 会导致 AssetBundle 的头比数据还要大。
            - 官方的建议是每个 AssetBundle 包大概 1M~2M 左右大小，考虑的是网络带宽。但现在 5G 的时候，可以考虑适当把包体加大。还是要看实际用户的情况。

- Resource 文件夹（**Do not use it**. 除非在 debug 的时候）
    - Resource 和 AssetBundle 一样，也有头来索引。Resource  在打进包的时候会做一个红黑树，来帮助 Resource 来检索资源在什么位置，
    - 如果 Resource 非常大，那么红黑树也会非常大。
    - 红黑树是不可卸载的。在刚开始游戏的时候就会加载进内存中，会持续对游戏造成内存压力。
    - 会极大拖慢游戏的启动时间。因为红黑树没加载完，游戏不能启动。

- Texture
    - upload buffer，和声音的很像：填满多大，就向 CPU push 一次。
    - r/w
        - Texture 没必要就不要开 read and write。正常 Texture 读进内存，解析完了，放到 upload buffer 里后，内存里的就会 delete 掉。
        - 但如果检测到你开了 r/w 就不会 delete 了，就会在显存和内存中各一份。
    - Mip Maps
        - UI 没必要开，可以省大量内存。
    - Mesh
        - r/w
        - compression
            - 有些版本 Compression 开了不如不开，内存占用可能更严重，具体需要自己试。
    - Assets
        - Assets 的数量实际上和 asset 整个的纹理是有关系的。（？）

        [Memory Management in Unity - Unity Learn](https://learn.unity.com/tutorial/memory-management-in-unity)

#### Unity Managed Memory 优化用户管理内存
[Understanding the managed heap](https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity4-1.html)

- VM 内存池
    - mono 虚拟机的内存池
    - VM 会返还内存给 OS 吗？
        - **会**
    - 返还条件是什么？
        - GC 不会把内存返还给系统
        - 内存也是以 Block 来管理的。当一个 Block 连续六次 GC 没有被访问到，这块内存才会被返还到系统。（mono runtime 基本看不到，IL2cpp runtime 可能会看到多一点）
    - 不会频繁地分配内存，而是一次分配一大块。
- GC 机制（BOEHM Non-generational 不分代的）
    - GC 机制考量
        - Throughput(（回收能力）
            - 一次回收，会回收多少内存
        - Pause times（暂停时长）
            - 进行回收的时候，对主线程的影响有多大
        - Fragmentation（碎片化）
            - 回收内存后，会对整体回收内存池的贡献有多少
        - Mutator overhead（额外消耗）
            - 回收本身有 overhead，要做很多统计、标记的工作
        - Scalability（可扩展性）
            - 扩展到多核、多线程会不会有 bug
        - Protability（可移植性）
            - 不同平台是否可以使用
    - BOEHM
        - Non-generational（不分代的）

            ![https://s3-us-west-2.amazonaws.com/secure.notion-static.com/8934bc1f-3e98-4544-b6de-6ea5b80e2850/Untitled.png](https://s3-us-west-2.amazonaws.com/secure.notion-static.com/8934bc1f-3e98-4544-b6de-6ea5b80e2850/Untitled.png)

            - 分代是指：大块内存、小内存、超小内存是分在不同内存区域来进行管理的。还有长久内存，当有一个内存很久没动的时候会移到长久内存区域中，从而省出内存给更频繁分配的内存。
            - “非代数”是不分代的,指必须扫描整个托管堆，因此在执行收集传递时必须扫描整个堆，因此其性能因堆的大小扩展而降低。
        - Non-compacting（非压缩式）

            ![https://s3-us-west-2.amazonaws.com/secure.notion-static.com/33a4002e-f37e-4405-b9b3-815c0f43caba/Untitled.png](https://s3-us-west-2.amazonaws.com/secure.notion-static.com/33a4002e-f37e-4405-b9b3-815c0f43caba/Untitled.png)

            - 当有内存被回收的时候，压缩内存会把上图空的地方重新排布。是指当释放的内存产生的间隙不会消失。也就是说对象被销毁，内存被释放，这块内存不会马上收集成为空闲内存的一部分，这块内存只能用来存储比释放对象相同或更小的数据，如果内存间隔太小，会产生内存碎片.即使可能有足够的总空间来容纳某个分配，托管堆也无法找到足够大的连续内存块来适合分配。
            - 但 Unity 的 BOEHM 不会！它是非压缩式的。空着就空着，下次要用了再填进去。
                - 历史原因：Unity 和 Mono 合作上，Mono 并不是一直开源免费的，因此 Unity 选择不升级 Mono，与实际 Mono 版本有差距。
                - 下一代 GC
                    - Incremental GC（渐进式 GC）
                        - 现在如果我们要进行一次 GC，主线程被迫要停下来，遍历所有 GC Memory “island”（没听清），来决定哪些 GC 可以回收。
                        - Incremental GC 把暂停主线程的事分帧做了。一点一点分析，主线程不会有峰值。总体 GC 时间不变，但会改善 GC 对主线程的卡顿影响。
                    - SGen 或者升级 Boehm？
                        - SGen 是分代的，能避免内存碎片化问题，调动策略，速度较快
                    - IL2CPP
                        - 现在 IL2CPP 的 GC 机制是 Unity 自己重新写的，是升级版的 Boehm
    - Memory fragmentation 内存碎片化

        ![https://s3-us-west-2.amazonaws.com/secure.notion-static.com/96caa361-8d1a-4f8e-a0b6-87d521bb7f14/Untitled.png](https://s3-us-west-2.amazonaws.com/secure.notion-static.com/96caa361-8d1a-4f8e-a0b6-87d521bb7f14/Untitled.png)

        - 为什么内存下降了，但总体内存池还是上升了？
            - 因为内存太大了，内存池没地方放它，虽然有很多内存可用。（内存已被严重碎片化）
        - 当开发者大量加载小内存，使用释放*N，例如配置表、巨大数组，GC 会涨一大截。
            - 建议先操作大内存，再操作小内存，以保证内存以最大效率被重复利用。
    - Zombie Memory（僵尸内存）
        - 内存泄露说法是不对的，内存只是没有任何人能够管理到，但实际上内存没有被泄露，一直在内存池中，被 zombie 掉了，这种叫 Zombie 内存。
        - 无用内容
            - Coding 时候或者团队配合的时候有问题，加载了一个东西进来，结果从头到尾只用了一次。
            - 有些开发者写了队列调度策略，但是策略写的不好，导致一些他觉得会被释放的东西，没有被释放掉。
            - 找是否有活跃度实际上并不高的内存。
        - 没有释放
        - 通过代码管理和性能工具分析
    - 最佳实践
        - Don't Null it, but Destroy it（显式用 Destory，别用 Null）
        - Class VS Struct,尽量用 Struct
        - Pool In Pool（池中池）
            - VM 本身有内存池，但建议开发者对高频使用的小部件，自己建一个内存池。例如子弹等,高频使用的小部件。
        - Closures and anonymous methods（闭包和匿名函数）
            - 如果看 IL，所有匿名函数和闭包会 new 成一个 class，因此所有变量和要 new 的东西都是要占内存的。这样会导致协程。
                - 有些开发者会在游戏开始启用一个协程，直到游戏结束才释放，这是错误的。
                - 只要协程不被释放掉，所有内存都会在内存里,在协程里面的内存,只要协程不被释放,则会一直在内存里面。
                - 用的时候生产一个协程,不用的时候就释放,这是最好的使用协程的方式
        - Coroutines（协程）
            - 可看做闭包和匿名函数的一个特例
            - 最佳实践：用的时候生产一个，不用的时候 destroy 掉。
        - Configurations（配置表）
            - 不要把整个配置表都扔进去，是否能通过啥来切分下配置表
        - Singleton
            - 慎用,严格检查使用.
            - 有些内存从游戏一开始到游戏死掉，一直在内存中。

# 优化重点以及方向
unity托管内存>第三方库(主要是lua优化)>unity底层优化>代码文件.

#### unity托管内存(用户管理内存)

* c# 一共有几种类型:一共有四类
> 1 值类型。在C#中，所有从System.ValueType继承的类型.bool;byte;char;decimal;double;enum;float;int;long;sbyte;short;ushort;uint;ulong;struct   
> 2 引用类型。类、接口、委托、object对象;string;stringBuilder;class;interface;delegate        
> 3 指针。当我们把对象放到堆内存时，访问该对象，就需要一个指向该对象的引用，也就是指针,我们不需要显式的使用指针，Clr会对引用进行管理;注意区别指针(引用)与引用类型的区别，当我们说类型是引用类型时，指的是它需要通过指针来访问;而指针存储着一个指向内存的地址。        
> 4 指令。处理指令，比如变量声明、数学运算、跳转等

对比参数|值类型	| 引用类型|
-------|------|---|
内存分配(管理) |	线程栈 |	托管堆|
内存回收|	直接释放|	等待 GC (垃圾回收器)回收|
new实例|	返回值本身|	返回内存地址，如果垃圾回收器第0代内存满时，可能引起垃圾回收|
变量赋值|	逐字段复制|	赋值内存地址|
类型特点|	轻量、无额外字段|	需要额外字段（类型地址指针、同步块索引）|
常见类型|	数值类型、枚举类型、struct类型|	string、clas类|
是否支持继承|	值类型都是密封类型，所以不支持继承|	单继承|
接口实现|	支持|	支持|
表现方式|	未装箱、已装箱|	总是已装箱


* 栈的主要功能是什么？
> 栈的主要功能是跟踪线程执行时的代码指针的位置，以及被调用和返回的数据,可以把它看做一个线程的状态，每个线程都有自己独立的栈。
> 当调用函数时，会将函数的参数压入线程栈，在方法内的局部变量也会压入线程栈顶。方法执行结束后， 返回值被返回。C++中也是一样的

* 怎么确定数据分配到了哪个内存区？怎么确定数据分配在栈还是堆上？
> 值类型和指针总分配在被声明的地方，即他们的分配与声明的位置有关，声明在哪儿就分配在哪儿
> 非空引用类型对象和所有装箱值类型对象总是分配在堆内存上;严格来说，必须在托管堆上分配所有非 null 引用类型对象和所有装箱值类型对象
> 栈内存由当前线程管理,堆内存由 GC 管理

* 值类型一定分配在栈上吗？
> 不是,如果声明在函数的局部变量，就分配到线程栈中；如果声明在一个class类中，就分配在堆内存中

* 垃圾回收是如何工作的？垃圾回收的执行过程是什么样的？
> 1.当GC开始调用时，挂起所有正在运行的线程
> 2.检查堆上的每个对象,回收线程会检查内存堆
> 3.搜索当前对象的所有引用
> 4.没有被引用的对象都是垃圾，被标记为可删除
> 5.最后遍历删除被标记为可删除的对象，释放内存
> 6.最后GC会对剩下的对象进行重定位，同时会更新所有指向这些对象的指针（引用）。这一系列操作在性能消耗方面非常昂贵，所以在编写高性能代码时，要注意栈和堆得内存分配。

* 什么情况会触发垃圾回收？
> 代码需要在托管堆上分配内存时，但发现可分配的空间不足的情况下会触发GC
> 手动代码调用触发GC
> Unity不定期的触发GC

* 内存碎片的现象会造成什么问题？
> 1:堆内存是一块连续的内存地址，清理其中垃圾之后，就会造成内存碎片。
> 2:内存间隔太小不够放新的对象
> 3:一个托管堆虽然总空间量已经很大了，但是在这个空间里的内存间隔找不到连续空间来存储新的对象.
> 4:如果有内存压缩的 GC 机制,内存碎片就会被清空还给系统,最新版的 Unity 会添加内存压缩的 GC 机制,目前应该在筹备中

* unity什么时候会给堆内存扩容？
> 如果unity发现托管堆的内存不够分配了，会先进行GC，如果GC之后，发现还是不够分配，就进行托管堆的扩容（扩展），堆扩展的具体数量取决于平台; 但是，大多数Unity平台的大小都是托管堆的两倍。
> 开始分配-->检查托管堆上是否有足够的可分配内存,有就给变量分配内存;没有就触发 Unity 的 GC(垃圾回收)-->检查托管对上是否有足够的可分配内存-->有就给变量分配内存;没有,Unity 就向操作系统申请新的内存(扩充托管对的大小)-->有就给变量分配内存;没有就干掉后台的其他 APP,在没有就再干掉系统服务等等,在没有就杀掉当前 APP

* 从代码上有哪些写法是可以避免内存碎片产生的？
> 不要在频繁调用的函数中反复进行堆内存分配,在像Update()和LateUpdate()这种每帧都调用的函数，可以判断值变化了才调用某个会有堆内存分配的函数，或者计时器到了才调用某个函数
> 清空而不是创建集合,创建新的集合(比如：数组，字典，链表等集合类数据)会导致托管堆上的内存分配 , 如果发现在代码中不止一次地创建新集合，那么我们应该缓存引用到的集合，并使用Clear()清空其内容，而不是重复调用new()
> 尽可能避免C＃中的闭包。在性能敏感的代码中应该尽可能的减少匿名方法和方法引用的使用，尤其是在基于每帧执行的代码中。匿名方法要求该方法能够访问方法范围之外的变量状态，因此已成为闭包。于是C＃通过生成一个匿名类，可以保留闭包所需的外部范围变量。当执行闭包需要实例化其生成的类的副本，并且所有类都是C＃中的引用类型，所以执行闭包需要在托管堆上分配对象。
> 装箱;装箱是Unity中非常常见的非预期的临时内存分配原因之一。只要将值类型值用作引用类型，就会发生这种情况;C＃的IDE和编译器通常不会发出有关装箱的警告，即使它会导致意外的内存分配。这是因为C＃语言是在假设小型临时分配将由分代垃圾收集器和分配大小敏感的内存池有效处理的情况下开发的。虽然Unity的分配器确实使用不同的内存池进行小型和大型分配，但Unity的垃圾收集器是不是分代的，因此不能有效地扫除由装箱生成的小的，频繁的临时分配。在用Unity运行时编写C＃代码时，应尽可能避免使用装箱操作。装箱的一个常见原因是使用enum类型作为词典的键。要解决这个问题，有必要编写一个实现IEqualityComparer接口的自定义类，并将该类的实例指定为Dictionary的比较器;Unity 5.5中的C＃编译器升级显着提高了Unity生成IL的能力。已经从foreach循环中消除了装箱操作。消除了与foreach循环相关的内存开销。(https://blog.csdn.net/salvare/article/details/79935578)
> String相关;在C#中，String字符串是引用类型而不是值类型。C#中的字符串是不可变更的，其引用指向的值在创建后是不可被变更的。因此在创建或者丢弃字符串的时候，会造成托管堆内存分配。推荐做法：减少不必要的字符串创建，提前创建并持有缓存;减少不必要的字符串操作，比如常用的+。每次在对字符串进行操作的时候（例如运用字符串的”+”操作），unity会新建一个字符串用来存储相加后的字符串。然后使之前的旧字符串被标记为废弃，成为内存垃圾。改用StringBuilder类 , StringBuilder就是专门设计用来创建字符串而不产生额外托管堆分配的类，而且可以避免字符串拼接产生垃圾
> 注意由于调用Unity的API所造成的堆内存分配;如果函数需要返回一个数组，则一个新的数组会被创建用作结果返回，简单地缓存一个对数组的引用;函数gameobject.name或gameobject.tag，可以使用一个相关的联合函数。用Input.GetTouch()和Input.touchCount()来代替Input.touches或者用Physics.SphereCastNonAlloc()来代替Physics.SphereCastAll();mesh.vertices 每次调用也是会造成开销的.

* unity的mono堆内存分配后会返还给系统吗？
> 不会，目前Unity所使用的Mono版本存在一个很严重的问题，Mono的堆内存是只升不降。
> 新一代版本会返回给系统,IL2CPP 版本的也会返回


VM : mono的一个VM内存池,虚拟机的内存池.VM内存会返回内存给OS内存,当一块内存 GC 6次没有被访问到,就会将内存放回给OS
GC :
[分代式内存回收](https://www.cnblogs.com/nele/p/5673215.html)
GC机制考量,
Throughput(回收能力):一次GC会回收到多少内存
pause times(暂停时长):一次GC对主线程影响多大,会让主线程暂停多少毫秒
Fragmentation(碎片化):一次GC内存回收之后,会让整块内存的碎片化增加多少,即不连续内存会增加多少.
Mutator overhead(额外消耗):回收本身有消耗,需要考量这个消耗有多大.
Soalability(可扩展性):多核多线程时会不会有其他bug
Portability(可移植性):在其他平台上面是否可以移植

目前GC:
unity现在使用的是:Boehm(Non-generational)非分代式,(Non-compaction)非压缩方式,内存回收机制,是所有内存同一放在一起的.造成主线程的卡顿

- 下一代GC:
    - Incremental GC(渐进式GC),分帧去做GC回收,GC时长还是一样的,但是会避免系统卡顿;目前已转向IL2CPP(升级的Boehm)了.


- 问题:内存下降,但是总体的内存池还是上升了,为什么?
    - 是代码碎片化导致的.因为有一块内存一直插不进去当前内存池里面,只能另行开辟内存.也就是内存碎片化没有被压缩.优化建议,先去对大型内存创建和释放,再对小型内存进行创建和释放.
    - Zimbie Memory(僵尸内存,别名就叫做脏内存,无用内存) :指内存从开启游戏到游戏关闭只用了一次内存.没有被释放,内存也没有泄露,也没办法使用.内存泄漏指得是没有任何人可以访问和管理到它,也没法释放掉.优化建议:不要觉得 obj == null就是释放掉内存了,显示调用destroy方法才可以释放掉.
    - 要常用struct不要用class
    - Pool In Pool (池中池) 高频使用的小部件需要建立一个内存池,不要频繁的去创建销毁Closures and anonymous methods (闭包和匿名函数,协程) 这些东西最终在C++层全部new成一个class了,优化建议:不要用.关于协程的优化建议:用的时候创建,及时销毁.再用时,再创建,再销毁.
    - 配置表优化,优化建议,采用C++管理内存,使用C#接口以及Lua接口去查询,而不是在c#和Lua里面保存内存,不要在c#和Lua里面重新全部记录.例如:从C++接过配置表对象时,在c#里面再保存一份,这是错误的方式.
    - Singleton 单例慎用,仅在必要时用,最好不要用单例模板,太坑爹的设计了,难用不说还特么代码难看懂,恶心.


#### 第三方库(主要是lua优化)-->
- 需要查看tolua源码以及C++基础,然后在tolua的基础上面做优化,需要的个人能力比较强.
以及使用C++库作为插件导入进unity里面使用.

#### unity底层代码优化方向-->
底层会根据类型将内存分配到不同的Allooator的池子里面,使用GetRuntimeMemory
profiler中Memory一项中的Used Total 和 Reserved Total会保持相同的上升或者下降,如果不一致,那就是出bug了.
Used Total 表示当前你使用了多少内存
Reserved Total表示unity申请了多少内存,将要使用到的内存
1: Scene中的GameObject太多,则内存会暴涨.如果在场景中创建了一个GameObject,底层C++会创建一个或者多个object(记录component),去记录这个GameObject的信息.是程序的优化重点,建议:尽量少创建GameObject,并且无用GameObject尽量干掉,优化GameObject个数.
2: Audio中会有个缓存池(DSP buffer),需要用户设置,填充满就会向CPU发送播放指令,发送完播放指令才会播放.设置的过大,则音频填充满就会时间过长,声音延迟,设置的过小,就会向CPU频繁发送.优化建议:测试音频播放密集时与不密集时的样本,并进行大小变换设置.
Force to mono,双声道音频设置,双声道概念:左右声道播放的不一致的声音.优化建议,设置为单声道.
音频格式Format,Mac/iOS上面的音频设置为mp3,有硬件支持.
音频压缩格式Compression Format:在哪种情况下使用哪种形态,需要去unity官方手册上面查询.
3: CodeSize,模板泛型的乱用,会导致打包速度,并且静态代码量增多.模板泛型,(il2cpp)会在C++层将泛型全部转成静态类型的C++,会变的很大.(一个文件2-3m已经很多了吧,官方视频上说有25M!  ...)
4: AssetBundle 
typetree,序列化的时候会生成一个typetree,反序列化的时候会根据这个typetree反序列化.
关闭typetree内存会减小,包大小会减小,build和运行时会变快.
Lz4 压缩方式,快速压缩,包体大Lzma上面30%,不要用Lzma,lzma会增大内存等等副作用.
asset会有头记录部分,以及实际的数据部分,如果每一个都打成ab则有可能发生,头记录部分比实际数据部分还要大,建议是1-2M,5G以后可以加大.
5:Resource 文件夹 ,打进包的时候会做一个红黑树(R-B Tree),会检索数据在什么位置,这个里面的数据很大,则红黑树就会很大.优化建议:不要放大量数据就行了,用ab替换.
6:Texture
upload buffer 这个数据表示填满之后向GPU发送一次,建议测试并平均该值
R/W read and write 一般情况下不要开,开启情况会在显存和内存中各一份.
mip maps UI情况下不要开就行了.
7: mesh   
R/W 一般情况下不要开. compression在某些unity版本上面开了比不开占用更多内存.


###### tips:[加快Unity编辑器下脚本编译速度](https://www.xuanyusong.com/archives/4474)


创建与加载monobehavier以后，内存中有大量这种东西，内存中是没有做过优化的，内存结构不合理，不够紧凑，分散点比较多，cpu也没有将这些事情并行处理，monobehavier里面的方法还是有反射到c++层使用的

***

# C#与非托管DLL进行交互
https://blog.csdn.net/salvare/article/details/80087488

# Unity内存管理的核心问题

* 1:内存使用是否合适? 内存块大小,碎片化.
* 2:内存泄露如何解决? 僵尸内存
* 3:Reserved Total 内存总量尽量小于80M
* 4:纹理资源内存尽量小于 50M,
* 5:网格资源内存尽量小于 20M,模型的 Tangents 尽量选择 None
* 6:RenderTexture 内存峰值,需要控制分辨率,需要关注 Antialiasing(反锯齿)
* 7:ParticleSystem 粒子系统总体使用数量
* 8:AssetBundle资源冗余,如何做到零冗余-->https://blog.uwa4d.com/archives/1577.html
* 9:AssetBundle(SerializedFile)尽量小于 50 个
* 10:关注一次性分配过多内存,关注哪个函数在分配内存.配表解析,New Class,String 操作,Instantiate,格式转换
* 11:内存泄露,每1000 帧需要获取一下堆内存和变量数.