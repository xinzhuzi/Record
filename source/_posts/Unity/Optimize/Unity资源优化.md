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



# 资源规范

* 1 Animation 
```
检查动画曲线精度:动画曲线精度过高会增加动画占用内存; 此规则仅面向以文本格式序列化的*.anim文件中的浮点精度;用文本编辑器打开.anim动画文件，修改m_EditorCurves::curve::m_Curve下的float值的精度。建议用脚本直接将此文件中所有float精度都调整为5位小数以下。动画曲线精度应小于5 ;       
检查动画缩放曲线:动画不应具有缩放曲线,动画中的缩放曲线会增加动画占用内存,用文本编辑器打开.anim动画文件，确认m_EditorCurves和m_FloatCurves下不包括attribute为m_Scale的curve子对象。动画不应具有缩放曲线;
```
* 2 AnimationController 
```
动画控制器中的动画剪辑个数: 动画控制器中动画剪辑数量过多会造成动画初始化耗时过多;动画剪辑个数应小于2
```
* 3 FBX     
```
检查读/写标志:开启FBX资源的读/写标志会导致双倍的内存占用;FBX资源的读/写标志应该被禁用      
检查动画资源压缩方式:动画资源使用最佳压缩方式可以提高加载效率;查看Inspector -> Animation Tab -> Anim. Compression选项;动画资源应该使用最佳压缩方式      
应为网格资源启用OptimizeMesh;为网格资源启用OptimizeMesh可以减少最终游戏包的大小;检查网格资源的OptimizeMesh;查看Inspector -> Model Tab -> Meshes -> Optimize mesh选项;应为网格资源启用OptimizeMesh;        
在FBX资源中有太多的顶点, 默认阈值是500;FBX资源资源中有太多的顶点, 请检查是否必要;检查FBX资源定点数;顶点数量限制;FBX资源资源中有太多的顶点;顶点数量有多少;             
```
* 4 Prefab
```
Prefab Max Particle Limit;渲染Mesh的粒子系统不宜设置过高的粒子总数, 默认阈值为30;当粒子系统渲染Mesh时(Inspector: Particle System -> Renderer -> Render Mode == Mesh), 相比于渲染Billboard时计算资源消耗会明显上升，因此需要对粒子总数加以限制;检查最高粒子总数限制;检查Inspector -> Particle System -> Max Particles配置;渲染Mesh的粒子系统不宜设置过高的粒子总数;              
检查粒子系统的发射速率:当粒子系统渲染Mesh时(Inspector: Particle System -> Renderer -> Render Mode == Mesh), 相比于渲染Billboard时计算资源消耗会明显上升，因此需要对粒子发射速率加以限制;检查Inspector -> Particle System -> Emission -> Rate over Time/Distance配置;渲染Mesh的粒子系统不宜设置过高的粒子发射速率, 不应超过5             
检查Skinned Mesh Renderer:启用Skinned Motion Vectors会使渲染器同时使用当前帧和上一帧的蒙皮网络来渲染目标的动画以提高精度，从而需要双倍大小的缓冲区并占用双倍的显存;启用Skinned Motion Vectors将以消耗双倍内存为代价提高蒙皮网格的精度;      
检查网格读/写标记:被预制件关联的网格资源应该关闭读/写标记;被预制件关联的网格资源应该关闭读/写标记       
```
* 5 Scene
```
检查场景未添加tag的GameObject:场景中的所有GameObject都应当添加tag;场景包含未添加tag的GameObject;          
Scene Multiple Audio Listeners:一个场景不应包含多个Audio Listener;一个场景不应包含多个Audio Listener;检查场景中的Audio Listener;检查场景中所有GameObject下的Audio Listener组件;场景包含多个音频侦听器;                    
检查场景中mesh collider:Mesh Collider可以在场景中提供更精细化的碰撞检测，随之而来也会消耗大量计算资源，建议审慎使用。 检查场景中所有GameObject下的Mesh Collider组件;场景包含了mesh collider;        
检查场景渲染设置:在移动平台，建议在渲染设置中关闭对雾的渲染以节省计算资源。检查Window -> Rendering -> Lighting Settings -> Scene -> Other Settings -> Fog选项;在移动平台，应在设置里关闭雾的渲染;           
Scene Shadow Resolution;场景中灯光的阴影分辨率应与项目设置一致;场景中灯光的阴影分辨率应与项目设置一致;检查场景阴影分辨率;检查Inspector -> Light -> Shadow Type -> Realtime Shadows -> Resolution选项，建议使用'Use Quality Settings';场景中灯光的阴影分辨率应与项目设置一致;            
Scene Rigidbody;场景中的静态GameObject不应关联Rigidbody;静态GameObject的Rigidbody模块是无用的;检查场景中的Rigidbody;静态GameObject不应关联Rigidbody;        
Scene Canvas Component;包含太多组件的Canvas可能会影响UI刷新的性能;包含太多组件的Canvas可能会影响UI刷新的性能，并进一步影响应用的帧率;检查Canvas中的component数量;包含太多组件的Canvas可能会影响UI刷新的性能;            
Scene UI Outside Screen;场景包含屏幕外的UI组件;放置在屏幕外的UI组件尽管不可见仍会消耗渲染资源，建议删掉此组件或者修正可能出现的设置错误;检查屏幕外的UI组件;场景包含屏幕外的UI组件;            
检查场景Animator组件中的ApplyRootMotion选项:如果不需要进行根骨骼动画位移, 建议关闭场景中Animator组件的applyRootMotion选项。场景中包含勾选了applyRootMotion选项的Animator组件;       
检查场景Animator组件中的cullingMode:场景中Animator组件的cullingMode是AlwaysAnimate会增加CPU使用率。场景中包含cullingMode为AlwaysAnimate的Animator组件;
```

* 6 Script
```
检查OnGUI方法:由于内存使用率高，不应使用OnGUI方法;IMGUI是过时的UI系统，仅建议在开发调试时使用。     
检查所有的 Mono 生命周期函数 方法:空方法会导致掉帧,建议去除。     
```
* 7 Shader
```
检查Shader中纹理数量:Shader中过多的纹理可能会增加GPU消耗;Shader中的纹理个数应小于3,Shader Texture Count
```

* 8 Texture
```
检查纹理读/写标记:开启纹理资源的读/写标志会导致双倍的内存占用; 检查Inspector -> Advanced -> Read/Write Enabled选项;纹理资源的读/写标志应被禁用;         
检查Mipmap标记:未压缩的纹理资源启用Mipmap标志会增加内存占用; 检查Inspector -> Advanced -> Generate Mip Maps选项;未压缩的纹理应该禁用mipmap ;        
Texture iOS compression format;检查iOS平台的纹理压缩格式;检查iOS平台的纹理压缩格式;iOS平台纹理压缩格式;如果希望对各平台统一设置压缩格式，检查Inspector -> Default -> Format选项; 如果希望为iOS平台单独设置，打开旁边的iOS选项卡，勾选Override for iOS并检查下面的Format选项;iOS平台的纹理格式应该是astc;对iOS平台使用默认值，但格式不是Automatic;         
Android平台纹理压缩格式:检查Android平台的纹理压缩格式;如果希望对各平台统一设置压缩格式，检查Inspector -> Default -> Format选项; 如果希望为Android平台单独设置，打开旁边的Android选项卡，勾选Override for Android并检查下面的Format选项;对安卓平台使用默认值，但格式不是Automatic;           
纹理资源大小2的幂次:大小非2的幂次的纹理资源将无法使用ETC1和PVRTC压缩格式。在导入时自动伸缩为2的幂次也可能会导致内存占用或者贴图质量问题。 检查Inspector -> Advanced -> Non-Power of 2选项. 建议使用原始大小为2的幂次的贴图;纹理大小不是2的幂次;         
检查纹理是否过大:过大的纹理资源会更多的消耗内存;Custom Parameters: heightThreshold : 512widthThreshold : 512;纹理大于 512 * 512 ;       
检查Aniso级别:检查Inspector -> Aniso Level滑动条;纹理资源的Aniso级别大于1;Aniso级别大于1的纹理资源会增加内存占用和采样率;纹理资源的Aniso级别不应大于1;                
检查纹理资源的过滤模式:纹理的过滤模式一般不建议使用Trilinear，会占用较高的计算资源。 检查Inspector -> Filter Mode选项;纹理使用了Trilinear过滤模式;          
检查纹理资源alpha通道:如果纹理包含空的alpha通道，则应禁用'Alpha源'标志，否则会浪费这部分的内存。 检查Inspector -> Alpha Source选项;应禁用具有空Alpha通道的纹理的‘Alpha源’标志;          
检查纯色纹理:纯色纹理的使用可能可以由一些设置来代替。由于某些情况下纯色纹理是必不可少的，此警告仅会在所使用的纹理较大(>16*16)时才会触发。纯色纹理会浪费内存;        
Texture edge transparent;边缘有较大透明部分的纹理应裁剪以节省内存;边缘有较大透明部分的纹理应裁剪以节省内存;检查纹理透明边缘;边缘有较大透明部分的纹理应裁剪以节省内存;           
检查纹理重复环绕模式:Repeat Wrap模式可能会导致纹理上出现意外的边缘; 检查Inspector -> Wrap Mode选项;重复环绕模式可能会导致纹理上出现意外的边缘;     
检查重复纹理:检查重复纹理;纹理重复          
检查雪碧图纹理填充率:填充率是雪碧图分割后的有效面积与总面积的比率，较低的雪碧图纹理填充率会导致显存的浪费。Custom Parameters: fillRateThreshold : 0.5onlyCheckSprite : True; 尝试重新编排雪碧图，尽量缩小总面积以提高填充率;sprite填充率低于 0.5
```

* 9 Audio
```
启用Force to Mono:检查Inspector -> Force To Mono选项:应为音频资源启用forceMono,如不需要立体声,开启forceMono可以减少内存和磁盘占用;音频应该启用forceMono，以节省存储和内存;      
检查iOS平台的音频压缩格式:检查Inspector -> iOS Tab -> Compression Format选项;iOS平台的音频剪辑应使用MP3格式;        
检查安卓平台的音频压缩格式:检查Inspector -> Android Tab -> Compression Format选项;安卓平台的音频剪辑应使用Vorbis格式;       
检查音频加载类型:检查Inspector -> (Platform Tab) -> Load Type选项;有多少的短音效应使用DecompressOnLoad;有多少的的常规音效应使用CompressedInMemory;有多少的音乐应该使用Streaming;        
```

* 10 EditorSetting
应设置CompanyName;检查公司名称设置;检查File -> Build Settings -> Player Settings -> Player -> Company Name的设置;CompanyName不应设置为DefaultCompany;                       
应设置Build Target Icons;应设置Build Target Icons;检查Build Target图标;检查File -> Build Settings -> Player Settings -> Player -> Default Icon的设置;应设置 %s 的Build Target Icons;                        
应设置开启GraphicsJobs;这项设置会为图形任务开启多线程. 但这个是实验性质的, 会引起新的问题. 请自行测试;检查GraphicsJobs设置;检查Editor -> Project Settings -> PlayerSettings -> Graphic Jobs(Experimental)*的设置;尝试开启graphicJobs并测试;                     
应设置开启BakeCollisionMeshes;这项设置可以减少加载/初始化的时间, 虽然会增加一些构建时间和包体积;检查BakeCollisionMeshes设置;检查Editor -> Project Settings -> PlayerSettings -> PreBake Collision Meshes的设置;如果在项目中启用了physics, 可以考虑开启Prebake Collision Meshes选项;                 
应设置开启StripEngineCode;关闭StripEngineCode会增加包体积;检查StripEngineCode设置;检查Editor -> Project Settings -> PlayerSettings -> Strip Engine Code的设置;关闭StripEngineCode会增加包体积;                  
在Physics设置中应关闭AutoSyncTransforms;AutoSyncTransforms选项是为了兼容老版本的Unity而设立的, 会增加CPU的使用;检查Physics中的AutoSyncTransforms设置:检查Editor -> Project Settings -> Physics -> Auto Sync Transforms的设置;在Physics设置中开启AutoSyncTransforms会增加CPU的使用;                   
在Physics设置中LayerCollisionMatrix中的格子不应该都勾选上;这会增加CPU的负担, 应该取消勾选那些没有必要的格子;检查Physics设置中的LayerCollisionMatrix设置;检查Editor -> Project Settings -> Physics -> Layer Collision Matrix的设置;在Physics设置中LayerCollisionMatrix中的格子不应该都勾选上;                         
在Physics2D设置中应关闭AutoSyncTransforms;AutoSyncTransforms选项是为了兼容老版本的Unity而设立的, 会增加CPU的使用;检查Physics2D中的AutoSyncTransforms设置;检查Editor -> Project Settings -> Physics2D -> Auto Sync Transforms的设置;在Physics2D设置中开启AutoSyncTransforms会增加CPU的使用;          
在Physics2D设置中LayerCollisionMatrix中的格子不应该都勾选上;这会增加CPU的负担, 应该取消勾选那些没有必要的格子;检查Physics2D中LayerCollisionMatrix设置;检查Editor -> Project Settings -> Physics2D -> Layer Collision Matrix的设置:在Physics2D设置中LayerCollisionMatrix中的格子不应该都勾选上;                            
StandardShaderQuality选项在所有Graphics Tier中应相同:这会增加编译时间和包体积, 除非你想要支持很多性能跨度很大的设备;检查Graphics中StandardShaderQuality设置;检查Editor -> Project Settings -> Graphics -> Tiers -> Standard Shader Quality的设置;StandardShaderQuality选项在所有Graphics Tier中应相同;             
Android设置中的ManagedStrippingLevel选项不应为Low或者Disabled:这会增加包体积;检查Android的ManagedStrippingLevel设置;检查Editor -> Project Settings -> PlayerSettings -> Managed Stripping Level的设置;ndroid设置中的ManagedStrippingLevel选项应为Medium或者High;        
iOS设置中的Architecture选项不应为Universal;这会增加包体积. 如果工程并不准备支持32位的 iOS 设备, 将其设为 ARM64;检查iOS的Architecture设置;检查Editor -> Project Settings -> PlayerSettings -> Architecture的设置;iOS设置中的Architecture选项不应为Universal;
iOS设置中的AccelerometerFrequency选项应为 Disabled;如果项目没有用到设备的加速度计, 禁用 AccelerometerFrequency 可以节省一些 CPU 处理时间;检查iOS的AccelerometerFrequency设置;检查Editor -> Project Settings -> PlayerSettings -> Accelerometer Frequency的设置;iOS设置中的AccelerometerFrequency选项应为 Disabled;                
在 iOS 的 GraphicsAPIs 设置里应只包含 Metal:如果设备支持Metal, 在 GraphicsAPIs 里只开启 Metal 可以减少包体积和得到更好的 CPU 表现;检查iOS的GraphicsAPIs设置;检查Editor -> Project Settings -> PlayerSettings -> GraphicsAPIs的设置;在 iOS 的 GraphicsAPIs 设置里应只包含 Metal;           
"Editor iOSManagedStrippingLevel Setting":iOS设置中的ManagedStrippingLevel选项不应为Low;这会增加包体积;检查iOS的ManagedStrippingLevel设置;检查Editor -> Project Settings -> PlayerSettings -> Managed Stripping Level的设置;iOS设置中的ManagedStrippingLevel选项应为Medium或者High;                 
不建议使用Resources系统来管理asset;使用Resources系统可能会延长程序的启动时间。此系统已经过时，不建议使用。检查项目目录下是否存在Resources文件夹;不建议使用Resources系统来管理asset;     
```

11 Mesh
```
Mesh Read&Write Flag:应为网格资源禁用读/写标志;开启Mesh资源的读/写标志会导致双倍的内存占用;检查网格资源读/写标记;网格资源应禁用读/写标志;       
```

12 Model
```
Model Read&Write Flag;应为模型资源禁用读/写标志;开启M资源的读/写标志会导致双倍的内存占用;检查模型资源读/写标记;模型资源应禁用读/写标志;     
```
13 Video
```
导入的视频文件大小应小于某一限制，默认为256MB;检查视频大小;导入的视频文件体积不应过大;视频大小限制;;      
```