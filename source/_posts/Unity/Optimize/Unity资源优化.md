---
title: Unity 资源优化
date: 2020-05-08 11:41:32
top: 11
categories:
- Unity优化
tags:
- Unity优化
---

# 资源加载模块的核心问题

***

## 1:加载时瓶颈?


引擎阶段(加载阶段)|人工阶段(加载阶段)       
------|-------     
上一场景资源卸载 |       
当前场景资源加载  |      
当前场景加载后处理(GC耗时,GO销毁)      | 当前场景中,资源加载,
当前场景实例化          | 当前场景中,AB 加载 ,实例化
当前场景Active/Deactive | 当前场景中的 GameObject,Active/Deactive,实际初始化设置.

引擎函数|人工函数       
------|---     
EarlyUpdate.UpdatePreloading|PreLateUpdate.ScriptRunBehaviourLateUpdate
Loading.UpdatePreloading|LateBehaviorUpdate;  AssetBundles.Loadxxx 加载,
UpdatePreloading|(ToLua 第三方库) LuaLooper.LateUpdate;
Application.WaitForAsyncOperationToComplete|
Preload Sing Step(真正耗时的函数)|
Application.LoadLevelAsync;    GarbageCollectAssetsProfile(卸载上一个场景的函数,这个函数是调用 Resources.UnloadUnusedAssets();产生的(手动或者自动触发),会有大量耗时记录)|
;    Loading.LoadFileHeaders(Loading ReadObject,这一系列函数表示在加载新场景,读取头文件,读取GameObject),LoadAwakeFromLoad(加载后处理,mesh,shader,Shader.Parse)|
UnityEngine.SetupCoroutine(Coroutine InvokeMoveNext)(Instantiate实例化,Active,Deactive)|
GC.Collect;    GC.FindLiveObjects,GC.MarkDependencies;UnloadScene|

***

## 2:资源加载是否合理?


### 纹理
* 1:Mac Size,分辨率尽量小于 1024,长宽需要是 2 的幂次方.
* 2:Format,格式尽量选用 Unity 官方推荐的格式.
* 3:Read/Write 尽量关闭
* 4:Mipmap 尽量关闭
* 5:Texture streaming

### 网格
* 1:Model 里面的 Meshs 设置,资源的顶点数量与顶点属性数量
* 2:Mesh Compression 尽量选 On
* 3:Read/Write Enabled 尽量不勾选 
* 4:Normals尽量让 Unity 自己计算
* 5:Blend Shape Normals
* 6:Tangents 尽量少
* 7:Swap UVs 尽量不勾选
* 8:Generate Lightmap UVs,尽量不勾选
* 9:以上选择,全是为了数据量小而选择的,不为了表现,如果为了表现,需要进行取舍

### 动画片段
* 1:片段数量
* 2:压缩模式/动画精度/数据精度
* 3:动画模式,Humanoid要比Generic 小很多

### 音频片段

* 1:音频数量,
* 2:加载方式,Load Type-->选择 Streaming 是最快的
* 3:压缩格式,Compression Format-->PCM(不压缩),Vorbis(ogg 的格式),ADPCM(轻度压缩),MP3
* 4:内存最小的选择是  :加载方式选择Streaming,压缩格式选择Vorbis,Vorbis 的 Quality 可以进一步控制,建议 50%
* 5:建议使用 MP3 格式的音频文件,如果内存压力过大,可以考虑 Streaming 加载方式或较小 Quality 质量的 Vorbis 格式
* 6:如果是非及时使用音效(背景音乐),建议开启 Load in Background 来提升加载效率
* 7:如果 存在大量频繁使用的音效,建议选择 Decompressed On Load 来降低 CPU 开销

###  Particle System

* 1:粒子系统数量
* 2:材质,很多粒子加载,会将相同Material 加载进内存.


### 资源加载卸载的 API

* 1:Resources.Load
* 2:Resources.LoadAsync
* 3:AssetBundle.Load
* 4:AssetBundle.LoadAsync
* 5:异步加载时开启Application.backgroundLoadingPriority=ThreadPriority.High;表示异步操作可以在主线程的单帧花费最长时间。单帧花费时间越多，可加载的数据越多，因此帧率将有所下降，较为影响游戏性能，但可减少加载资源的时间，能更快的进入游戏！反之，单帧花费时间越少，可加载的数据越少，对游戏的游戏性能影响较小，可在游戏进行时有很好的后台加载。[官方文档](https://docs.unity3d.com/2017.1/Documentation/ScriptReference/Application-backgroundLoadingPriority.html)
* 6:QualitySettings.asyncUploadTimeSlice = 2;为了读取数据和上传纹理数据，重复使用了一个大小可以控制的环形缓冲区。使用asyncUploadTimeSlice以毫秒为单位设置异步纹理上传的时间片框架。最小值为1，最大值为33。
* 7:QualitySettings.asyncUploadBufferSize = 4;为了读取数据和上传纹理数据，重复使用了一个大小可以控制的环形缓冲区。设置异步纹理上传的缓冲区大小。大小的单位是兆字节。最小值为2，最大值为512。虽然缓冲区会自动调整大小以适应当前加载的最大纹理，但建议将该值设置为场景中使用的最大纹理的大小，以避免重新调整缓冲区的大小会导致性能损失。

## 3:引擎如何加载一个 Prefab?

为何我们调整了 资源加载卸载的API 中提到的三个参数,异步加载就变得快了.我们就要了解引擎是如何加载一个 prefab 的.一个 UI 的 Prefab 可能含有 Texture,Mesh,Material 等等很多东西.

* 1:加载 Mesh 并传输到 GPU;异步串行;异步串行加载时会寻找 AssetBundle 里面含有哪些资源,然后反序列化这些资源,然后先加载这个 Prefab 的 Mesh 传到 GPU 里面;
* 2:加载 Texture 并传输到 GPU;异步串行;
* 3:加载 shader & Material;异步串行;前期是加载 shader,后期是主线程解析 shader
* 4:加载 Object & Component;异步串行;
* 5:加载 AnimationClip 资源;异步串行;完全可以放在异步里面进行加载,不会卡主线程
* 6:加载 Audio 资源;异步串行;使用 Streaming 模式是最快的.
* 7:大量资源的异步加载均可在子线程中进行
* 8:主线程主要处理 AwakeFromLoad & Shader 解析
* 9:主线程可以留出大量空间供逻辑代码使用


***
## 4:如何优化加载?

* 1:加快 Mesh,Texture等资源的异步加载效率
* 2:加快处理 AwakeFromLoad & Shader 解析
* 3:Async Upload Pipeline
> * A:加载线程异步加载 Texture 和 Mesh 资源,用了 Mesh 的压缩,需要在主线程进行解压,是不能进行异步加载的,地形的 Texture 开了 Read/Write 是不能进行异步加载的
> * B:通过渲染线程直接将其传送到 GPU
> * C:目前 Unity 都支持,
> * D:代码里面异步加载方法 AsyncUploadBuffer 加载完毕之后,直接回放到渲染线程进行渲染,渲染线程中的渲染方法AsyncUploadTimeSlice将数据上传
* 4:优化方法:将QualitySettings.asyncUploadBufferSize = 4;改为QualitySettings.asyncUploadBufferSize = 16;表示引擎从硬盘里面每次读取 4M,变为读取 16M,读取时都是在子线程.这个参数里面的 buff 缓冲可以认为是流式的读取
* 5:优化方法:QualitySettings.asyncUploadTimeSlice = 2;改为QualitySettings.asyncUploadBufferSize = 8;表示渲染线程每一帧使用 2ms 去干活,变为渲染线程每一帧使用  8ms 去干活,有可能就多开了一个渲染线程
* 6:AUP 让内存占用更加可控,加载 Upload Buffer 会增加一个恒定的内存,除此之外几乎没有副作用,在内存允许的情况下,可以设置 16MB 或者 32MB.对于asyncUploadTimeSlice方法,如果设置过大,会产生卡顿现象.
* 7:压缩主线程上面的资源,控制异步加载资源在主线程的"后加载"耗时,各种资源的 AwakeFromLoad,Shader.Parse,设置代码 Application.backgroundLoadingPriority=ThreadPriority.High;
* 8:Texture.AwakeFromLoad 是从主线程里面将Texture传到 GPU
* 9:优化异步加载:调整 Upload Buffer 和 Upload Time Slice,调整 BackLoadingPriority,开启多线程渲染,调整资源的加载顺序(比如 shader)
* 10:异步加载时需要注意代码的写法,不要搞成一帧加载很多.AssetBundle 加载的写法需要注意
* 11:需要查看实例化操作是否合理,实例化的频率,大概需要 10K 帧有大的浮动;  一般正常的加载方式是-->AB a0-->AB a1(依赖 a0)-->加载 a1-->instantiate a1;错误加载方式-->AB a1(依赖 a0)-->加载 a1-->instantiate a1-->AB a0 这种情况下 a0 资源会出现在其他错误的地方;
* 12:需要查看Active/Deactive操作是否合理,尤其是在 NGUI/UGUI 上面

***
## 5:资源内存

* 1:有很多纹理在内存里面,但是没有进行渲染,也就是没有用到.需要查看 UI图集,技能特效.
* 2:注意纹理的分辨率大小,以及 Mipmap 是否物尽其用
* 3:在品质设置里面有个 Texture Streaming 选项中,有个参数:Memory Budget,在其足够时,Scene Texture 是满 Mipmap 加载,动态加载的 GO Texture 是最比配的 mipmap 进行加载和实例化,渲染时,则按照距离原因和 budget 进行调整.再其不足时,Scene Texture按照最低配加载,已加载 Streamed Texture 按照距离 Remove Mipmap,越远的越先 Remove 高 Level Mipmap ,动态加载的 GO Texture 只按最低配 Mipmap 加载,实例化和渲染.
* 4:Max Level Reduction设定为 1 表示加载 mipmap 第一级,设置为 2 加载第二级
* 5:在 mobile 端一定要设置 QualitySettings.streamingMipmapsActive = true;
* 6:网格:关注顶点渲染密度.
* 7:音频片段,Streaming 加载方式比较适合于背景音乐,对于同时播放的小音频文件,建议通过 Decompressed 方式.

*** 


## 6:AB 包
* 1:建议目前全部采用 LZ4 压缩
* 2:本地加载,建议使用 LoadFromFile(Async)来加载
* 3:根据机型,选择适合的 Coroutine 次数
* 4:如果自己压缩,建议使用 Gzip+LoadFromFile(Async)来加载
* 5:AssetBundle.LoadAll 比 Asset.Load OneByOne 要好很多.
* 6:切换场景时,尽可能使用 AssetBundle.Load 来提升加载效率
* 7:小,细碎的同种类资源打包在一起(Shader,ParticleSystem)


*** 
## 7:Mono堆内存

* 1:子线程 Mono 分配,多见于和服务器交互.
* 2:Lua 内存检测,关注 Destroyed 总数.
* 3:Instantiate 实例化频率,实例化的耗时.
* 4:Active/Deactive 频率和耗时.复杂的界面/带有动画组件的 GameObject 不要频繁使用