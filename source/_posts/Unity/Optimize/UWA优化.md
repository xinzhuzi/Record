---
title: Unity UWA工具检测优化
date: 2020-05-08 11:41:32
categories:
- Unity优化
tags:
- Unity优化
---
# 性能优化大合集,请看下面的官方介绍
* UWA 优化百科向导: https://blog.uwa4d.com/archives/Index.html  
* UWA Unity性能优化大合集，All In One ! https://blog.uwa4d.com/archives/allinone.html
* 如何看懂性能报告  https://blog.uwa4d.com/archives/Simple_PA_General.html


# 性能总结
* 查看这个性能分析图,性能总结--> https://www.uwa4d.com/demo/pa.html?v=5x&platform=android&name=jws&device=RedMiNote2 
>* 总体帧数：以一秒 30 帧为例,10 分钟测试,理想性能帧数可以跑到16000~18000帧,如果项目帧数低于这个范围，那么你的游戏可能正在经受一定的帧率卡顿问题,需要查看某一段时间内的性能瓶颈       
>* 总场景数     
>* GC次数：测试过程中系统垃圾回收操作（Garbage Collection）的调用次数,每次GC调用均会造成一定程度上的卡顿，降低了项目运行的流畅度,如果开发人员的逻辑代码分配堆内存过大过快的话，则GC调用的次数也会随之增加,标准为1000帧/次以上调用 GC,这个值越大调用一次 GC 越好.      
>* CPU均值：测试过程中平均每帧的CPU占用.高/中/低档的主流机型,设备性能越好，CPU耗时的均值肯定是越低        
>* CPU 占用(帧数占比):这个地方是 4 个图,CPU耗时超过33ms的帧数耗时超过了68%（请注意，UWA的建议是低于10%），超过50ms的帧数耗时达到35%.      
>* 总体 CPU 耗时走势:如果某条线特别高,需要看下是否可以进行优化,对比测试       
>* 总内存,堆内存优化:根据不同游戏进行不同的设定,定制一个合理的内存峰值才是正确的,也需要根据不同机器的性能评定.目前Unity所使用的Mono版本有这么个特点,Mono的堆内存一旦分配，就不会返还给系统.这意味着Mono的堆内存是只升不降的.IL2CPP版本在堆内存分配方面和Mono 最大的不同主要是Reserved Total 是可以下降的，而 Mono的 Reserved Total 只会上升不会下降.    
>* 关于任务列表:UWA 的测评报告还根据性能状态给出了优化任务列表,按照问题的严重程度设置了优先级，并对每个优化任务给出了需要检查或优化的步骤。     
>* 高 CPU 占用函数:       
>>Camera.Render    
>>UIPanel.LateUpdate()     
>>UnityEngine.SetupCoroutine() <Coroutine- InvokeMoveNext>     
>>Animators.Update     
>>Loading.UpdatePreloading     
>>NavMeshManager       
>>UICamera.Update()        
>>UIRect.Update()      
>>Physics.Processing       
>>LoadingManager.Update()      
>>Monobehaviour.OnMouse_       
>>SystemInfoPrinter.Update()       
>>TionManager.Update()     
>>MeshSkinning.Update      
>>AINormalWarrior.Update()     
>>Physics.ProcessReports       
>>SoundManager.Update()        
>>Destroy      
>>UIRect.Start()       
>>TweenEffectBase.Update()     

>* 高堆内存分配函数:     

>>UIPanel.LateUpdate()    
>>UnityEngine.SetupCoroutine() <Coroutine- InvokeMoveNext>    
>>UICamera.Update()   
>>TionManager.Update()    
>>Physics.ProcessReports  
>>LoadingManager.Update() 
>>Animators.Update    
>>BattleWinWnd.ShowContinueTip()  
>>CheatManager.Update()   
>>Destroy 
>>UIRect.Start()  
>>DynamicContent.Update() 
>>EnemyHintAgent.Update() 
>>Loading.UpdatePreloading    
>>UIMessageBoxManager.Update()    
>>GroupTutorialSystem.Update()    
>>iTween.Update() 
>>LevelManager.Update()   
>>AINormalWarrior.Update()    
>>PlayerBattleHUD.LateUpdate()


# 性能优化---CPU篇
* 查看这个性能分析图,总体性能趋势--> https://www.uwa4d.com/demo/pa.html?v=5x&platform=android&name=jws&device=RedMiNote2 
* CPU方面的性能开销主要可归结为两大类：引擎模块性能开销(渲染模块、动画模块、物理模块、UI模块、粒子系统、加载模块和GC调用等等)和自身代码性能开销(第三方,以及 Unity 管理不到的内存)
>* 渲染模块 :
>>（1）降低Draw Call,与维持总线带宽平衡,Draw Call是渲染模块优化方面的重中之重,一般来说，Draw Call越高，则渲染模块的CPU开销越大,降低Draw Call的方法则主要是减少所渲染物体的材质种类,并通过Draw Call Batching(https://docs.unity3d.com/Manual/DrawCallBatching.html)来减少其数量,游戏性能并非Draw Call越小越好,决定渲染模块性能的除了Draw Call之外，还有用于传输渲染数据的总线带宽.当我们使用Draw Call Batching将同种材质的网格模型拼合在一起时，可能会造成同一时间需要传输的数据（Texture、VB/IB等）大大增加，以至于造成带宽“堵塞”，在资源无法及时传输过去的情况下，GPU只能等待，从而反倒降低了游戏的运行帧率。Draw Call和总线带宽是天平的两端，我们需要做的是尽可能维持天平的平衡，任何一边过高或过低，对性能来说都是无益的。
>> (2) 简化资源,每帧渲染的三角形面片数、网格和纹理资源的具体使用情况    

>* UI 模块
>> 在NGUI的优化方面，UIPanel.LateUpdate为性能优化的重中之重,它是NGUI中CPU开销最大的函数，没有之一.
>> 对于UIPanel.LateUpdate的优化，主要着眼于UIPanel的布局，其原则如下：
>>> * 尽可能将动态UI元素和静态UI元素分离到不同的UIPanel中（UI的重建以UIPanel为单位），从而尽可能将因为变动的UI元素引起的重构控制在较小的范围内；
>>> * 尽可能让动态UI元素按照同步性进行划分，即运动频率不同的UI元素尽可能分离放在不同的UIPanel中；
>>> * 控制同一个UIPanel中动态UI元素的数量，数量越多，所创建的Mesh越大，从而使得重构的开销显著增加。比如，战斗过程中的HUD运动血条可能会出现较多，此时，建议研发团队将运动血条分离成不同的UIPanel，每组UIPanel下5~10个动态UI为宜。这种做法，其本质是从概率上尽可能降低单帧中UIPanel的重建开销。
>* 加载模块     
>> 加载模块的性能开销比较集中，主要出现于场景切换处(前一场景的场景卸载和下一场景的场景加载)，且CPU占用峰值均较高            
>> 场景卸载情况下的 Destroy,引擎在切换场景时会收集未标识成“DontDestoryOnLoad”的GameObject及其Component，然后进行Destroy。同时，代码中的OnDestory被触发执行，这里的性能开销主要取决于OnDestroy回调函数中的代码逻辑。         
>> 场景卸载情况下的 Resources.UnloadUnusedAssets 一般情况下，场景切换过程中，该API会被调用两次，一次为引擎在切换场景时自动调用，另一次则为用户手动调用（一般出现在场景加载后，用户调用它来确保上一场景的资源被卸载干净）。在我们测评过的大量项目中，该API的CPU开销主要集中在500ms~3000ms之间。其耗时开销主要取决于场景中Asset和Object的数量，数量越多，则耗时越慢。         
>> 场景加载情况下的  资源加载,资源加载几乎占据了整个加载过程的90%时间以上，其加载效率主要取决于资源的加载方式（Resource.Load或AssetBundle加载）、加载量（纹理、网格、材质等资源数据的大小）和资源格式（纹理格式、音频格式等）等等。不同的加载方式、不同的资源格式，其加载效率可谓千差万别，所以我们在UWA测评报告中，特别将每种资源的具体使用情况进行展示，以帮助用户可以立刻查找到问题资源并及时进行改正。          
>> 场景加载情况下的 Instantiate实例化 :在场景加载过程中，往往伴随着大量的Instantiate实例化操作，比如UI界面实例化、角色/怪物实例化、场景建筑实例化等等。在Instantiate实例化时，引擎底层会查看其相关的资源是否已经被加载，如果没有，则会先加载其相关资源，再进行实例化，这其实是大家遇到的大多数“Instantiate耗时问题”的根本原因，这也是为什么我们在之前的AssetBundle文章中所提倡的资源依赖关系打包并进行预加载，从而来缓解Instantiate实例化时的压力（关于AssetBundle资源的加载，则是另一个很大的Story了，我们会在以后的AssetBundle加载技术专题中进行详细的讲解）          
>> 场景加载情况下的 SerializedField序列化开销 Instantiate实例化的性能开销还体现在脚本代码的序列化上，如果脚本中需要序列化的信息很多，则Instantiate实例化时的时间亦会很长。最直接的例子就是NGUI，其代码中存在很多SerializedField标识，从而在实例化时带来了较多的代码序列化开销。因此，在大家为代码增加序列化信息时，这一点是需要大家时刻关注的
> * 代码效率    
>> 性能开销都遵循着“二八原则”,即80%的性能开销都集中在20%的函数上
>> Camera.Render    
>> MovementScript.FixedUpdate()         
>> ParticleSystem.Update        
>> Physics.Simulate         
>> TerrainManager.LoadTerrainObject()[Coroutine:MoveNext]       
>> TerrainManager.LoadTerrainSplices()[Coroutine:MoveNext]       
>> ParticleSystem.WaitForUpdateThreads()         
>> GameStarter.Update()         
>> 等等

# 性能优化，进无止境-内存篇
* 1 内存的开销无外乎以下三大部分:1.Unity资源内存占用；2. Unity引擎模块自身内存占用；3.Unity用户管理内存;4.第三方插件内存(比如 toLua),不属于Unity 。
> * 资源内存占用,在一个较为复杂的大中型项目中，资源的内存占用往往占据了总体内存的70%以上.纹理（Texture）、网格（Mesh）、动画片段（AnimationClip）、音频片段（AudioClip）、材质（Material）、着色器（Shader）、字体资源（Font）以及文本资源（Text Asset）等等.其中，纹理、网格、动画片段和音频片段则是最容易造成较大内存开销的资源
>> 纹理,纹理资源可以说是几乎所有游戏项目中占据最大内存开销的资源,一个6万面片的场景，网格资源最大才不过10MB，但一个2048x2048的纹理，可能直接就达到16MB.需要优化的点      
>> 纹理格式:Android平台的ETC、iOS平台的PVRTC、Windows PC上的DXT,具体的采用何种方式对纹理进行操作需要根据项目情况来看.
>> 在使用硬件支持的纹理格式时，你可能会遇到以下几个问题：
>>> * 色阶问题,由于ETC、PVRTC等格式均为有损压缩，因此，当纹理色差范围跨度较大时，均不可避免地造成不同程度的“阶梯”状的色阶问题。因此，很多研发团队使用RGBA32/ARGB32格式来实现更好的效果。但是，这种做法将造成很大的内存占用。比如，同样一张1024x1024的纹理，如果不开启Mipmap，并且为PVRTC格式，则其内存占用为512KB，而如果转换为RGBA32位，则很可能占用达到4MB。所以，研发团队在使用RGBA32或ARGB32格式的纹理时，一定要慎重考虑，更为明智的选择是尽量减少纹理的色差范围，使其尽可能使用硬件支持的压缩格式进行储存。
>>> * ETC1 不支持透明通道问题,在Android平台上，对于使用OpenGL ES 2.0的设备，其纹理格式仅能支持ETC1格式，该格式有个较为严重的问题，即不支持Alpha透明通道，使得透明贴图无法直接通过ETC1格式来进行储存。对此，我们建议研发团队将透明贴图尽可能分拆成两张，即一张RGB24位纹理记录原始纹理的颜色部分和一张Alpha8纹理记录原始纹理的透明通道部分。然后，将这两张贴图分别转化为ETC1格式的纹理，并通过特定的Shader来进行渲染，从而来达到支持透明贴图的效果。该种方法不仅可以极大程度上逼近RGBA透明贴图的渲染效果，同时还可以降低纹理的内存占用，是我们非常推荐的使用方式。建议:尽量采用 ETC2,在 Android 设备支持OpenGL ES 3.0的上面进行市场分流

































## Overview 类型
* 1 Profiler中WaitForTargetFPS详解 https://blog.csdn.net/suifcd/article/details/50942686
该参数一般出现在 CPU开销过低，且通过设定了目标帧率的情况下（Application.targetFrameRate）。当上一帧低于目标帧率时，将会在本帧产生一个WaitForTargetFPS的空闲等待耗时，以维持目标帧率。
Gfx.WaitForPresent && Graphics.PresentAndSync
这两个参数在Profiler中经常出现CPU占用较高的情况，且仅在发布版本中可以看到。究其原因，其实是CPU和GPU之间的垂直同步（VSync）导致的，之所以会有两种参数，主要是与项目是否开启多线程渲染有关。当项目开启多线程渲染时，你看到的则是Gfx.WaitForPresent；当项目未开启多线程渲染时，看到的则是Graphics.PresentAndSync。
Graphics.PresentAndSync 是指主线程进行Present时的等待时间和等待垂直同步的时间。Gfx.WaitForPresent其字面意思同样也是进行Present时需要等待的时间，但这里其实省略了很多的内容。其真实的意思应该是为了在渲染子线程（Rendering Thread）中进行Present，当前主线程（MainThread）需要等待的时间。

当项目开启多线程程渲染时，引擎会将Present等相关工作尽可能放到渲染线程去执行，即主线程只需通过指令调用渲染线程，并让其进行Present，从而来降低主线程的压力。但是，当CPU希望进行Present操作时，其需要等待GPU完成上一次的渲染。如果GPU渲染开销很大，则CPU的Present操作将一直处于等待操作，其等待时间，即为当前帧的Gfx.WaitForPresent时间，如下图所示。
![多线程渲染1](多线程渲染1.png)

同理，当项目未开启多线程渲染时，引擎会在主线程中进行Present(当前绝大多数的移动游戏均在使用该中操作)，当然，Present操作同样需要等待GPU完成上一次的渲染。如果GPU渲染开销很大，则CPU的Present操作将一直处于等待操作，其等待时间，即为当前帧的Graphics.PresentAndSync时间:
![多线程渲染2](多线程渲染2.png)
所以，如果你的项目中，Gfx.WaitForPresent或Graphics.PresentAndSync的CPU耗时非常高时，其实并不是它们自己做了什么神秘的操作，而是你当前的渲染任务太重，GPU负载过高所致。

同时，对于开启垂直同步的项目而言，Gfx.WaitForPresent 和 Graphics.PresentAndSync也会出现CPU占用较高的情况。在解释这种问题之前，我们先以“大家乘坐地铁”来举个例子。一般来说，地铁到达每一站的时间均是平均且一定的，假设每10分钟一班接走一批乘客。但是几乎没有多少乘客可以按点到达，如果提前两分钟到达，则只需要等待两分钟即可乘上地铁，但是，如果你错过了，哪怕只差了一分钟，那么你也不得不再等待九分钟才能乘上地铁。

上述的情况我们经常会遇到。在GPU的渲染流水线中，其转换front buffer和back buffer的工作原理和“乘坐地铁”其实是一致的。大家可以把GPU的流水线简单地想象成为一列地铁。对于移动设备来说，GPU的帧率一般为30帧/秒或60帧/秒，即VSync每33ms或每16.6ms“到站一次”，CPU的Present即为“乘客乘上地铁”，然后前往各自的目的地。与乘客的早到和晚到一样，CPU的Present也会出现类似的情况，比如：

● CPU端开销非常小，Present在很早即被执行，但此时VSync还没到，则会出现较高的等待时间，即Gfx.WaitForPresent 和 Graphics.PresentAndSync的CPU开销看上去很高。
● CPU端开销很高，使得Present执行时错过了VSync操作，这样，Present将不得不等待下一次VSync的到来，从而造成了Gfx.WaitForPresent 和 Graphics.PresentAndSync的CPU开销较高。这种情况在CPU端加载过量资源时特别容易发生，比如WWW加载较大的AssetBundle、Resource.Load加载大量的Texture等等。

通过以上的讲解，我们希望此刻的你已经对Gfx.WaitForPresent 和 Graphics.PresentAndSync已经有了深入的理解。这两个参数无论CPU占用多少，其实都不是这两个参数的自身问题，而是项目的其他部分造成。对此，我们做一个总结，以方便你进一步加深印象。

造成这两个参数的CPU占用较高的原因主要有以下三种原因：

```

● CPU开销非常低，所以CPU在等待GPU完成渲染工作或等待VSync的到来；
● CPU开销很高，使Present错过了当前帧的VSync，即不得不等待下一次VSync的到来；
● GPU开销很高，CPU的Present需要等待GPU上一帧渲染工作的完成。    

最后，如何优化并降低这两个参数的CPU占用呢？ 那就是，忽略Gfx.WaitForPresent 和 Graphics.PresentAndSync这两个参数，优化其他你能优化的一切！

```
# UnityEngine.SetupCoroutine:InvokeMoveNext

