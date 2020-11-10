---
title: AssetBundle 介绍
date: 2020-05-08 11:41:32
top: 1
categories:
- Unity
tags:
- Unity
---


# 概念
* 1 AssetBundle 是一个存档文件,包含可在运行时加载的特定于平台的资源（模型、纹理、预制件、音频剪辑甚至整个场景);AssetBundle 可以表达彼此之间的依赖关系;目前一般情况下采用 LZ4 压缩方式
* 2 AssetBundle包含 2 种东西,序列化文件和资源文件;序列化文件包含分解为各个对象并写入此单个文件的资源,依赖关系,映射等;资源文件只是为某些资源（纹理和音频）单独存储的二进制数据块，允许我们有效地在另一个线程上从磁盘加载它们.
* 3 打包分为随包资源,以及增量包资源(热更资源).随包资源跟随 apk,ipa 进行安装;将打的增量包上传到平台资源发布(CDN)服务器,游戏内进行下载;

# 本地开发使用的 API
* 1 打 AB 包的 API,在调用 API 之前,需要给资源设置 AssetBundle 名称,可以自动也可以手动,基本上写一套自动设置名字的策略:
```
BuildPipeline.BuildAssetBundles("路径,必须是英文", BuildAssetBundleOptions.xxx, BuildTarget.xxx);
BuildAssetBundleOptions.None:采用LZMA压缩,不建议使用
BuildAssetBundleOptions.UncompressedAssetBundle:不压缩,不建议使用
BuildAssetBundleOptions.ChunkBasedCompression:采用 LZ4 的压缩方法,建议使用
清单捆绑包:此包以其所在的目录（构建 AssetBundle 的目录）命名。也就是AssetBundleManifest
```
* 2 使用开发模式加载资源数据/对象,从本地进行加载时,使用 UnityEditor.AssetDatabase.LoadAssetAtPath 这个 API 即可资源数据/对象,以 UnityEngine.Object 对象返回.
* 3 使用 AB 模式加载资源数据/对象,从本地存储加载时,使用 AssetBundles.LoadFromFile 一般情况下是将所有的资源下载到本地,然后从本地加载 AB 包,AB 包里面包含多个资源(asset),这个资源初始化之后才是可以使用的.
```
第一步:提前加载所有 AB 包的AssetBundleManifest,一般为所有AB包的文件夹的名字,里面包含了所有 AB 包的依赖关系
第二步:加载单个 AB 包,在加载单个 AB 包时,从AssetBundleManifest里面获取所有的 asset 的依赖关系,获取到之后,加载依赖关系的 ab 包
第三步:LoadAsset 加载 AB 包中的资源(asset)之后,Init 这个资源对象之后,才能然后返回给程序使用.这个初始化过后的对象才可以使用.

AssetBundle.LoadFromMemoryAsync:函数,根据传入的字节(使用File.ReadAllBytes(path),将 AB 包读取为二进制)或者 CRC 值进行加载.
AssetBundles.LoadFromFile:函数,根据路径进行从磁盘上面加载
UnityWebRequest.GetAssetBundle 从网络上,根据路径从磁盘上面加载都可以.

```

* 4 
>1 将频繁更新的对象与很少更改的对象拆分到不同的 AssetBundle 中        
>2 将可能同时加载的对象分到一组。例如模型及其纹理和动画       
>3 如果发现多个 AssetBundle 中的多个对象依赖于另一个完全不同的 AssetBundle 中的单个资源，请将依赖项移动到单独的 AssetBundle。如果多个 AssetBundle 引用其他 AssetBundle 中的同一组资源，一种有价值的做法可能是将这些依赖项拉入一个共享 AssetBundle 来减少重复。        
>4 如果不可能同时加载两组对象（例如标清资源和高清资源），请确保它们位于各自的 AssetBundle 中。        
>5 如果一个 AssetBundle 中只有不到 50% 的资源经常同时加载，请考虑拆分该捆绑包     
>6 考虑将多个小型的（少于 5 到 10 个资源）但经常同时加载内容的 AssetBundle 组合在一起     
>7 如果一组对象只是同一对象的不同版本，请考虑使用 AssetBundle 变体        

# AB 包里面有什么

*  有 2 种类型,一种是 serialized file 序列化文件(prefab,Mono,Material,shader),另一种是 resource files 真实的资源(纹理,音频,Mesh)
*  可以使用 AssetStudio 将 ab 包里面的数据解析出来 https://github.com/Perfare/AssetStudio