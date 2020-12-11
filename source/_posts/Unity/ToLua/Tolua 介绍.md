---
title: Tolua 介绍
date: 2020-05-08 11:41:32
top: 700
categories:
- Unity
tags:
- ToLua
---

# ToLua 框架介绍
* 1. 框架功能,提供Lua代码逻辑热更,快速开发,编写 UI 超级快;Lua 是胶水语言,代码是 C 实现的,基于 C 可以和任何一个语言进行交互
* 2. 框架地址 https://github.com/topameng/tolua ,这个库是指,可以在 Unity 中使用的 C#代码 + DLL 库 + 例子.
* 3. 框架核心库地址 https://github.com/topameng/tolua_runtime 这个是打成 DLL 的源码,里面是纯 C/C++ 代码;包括 [Luajit](http://luajit.org/) 源码(不是 [Lua](https://www.lua.org/) 核心代码,2 者有区别),cjson 库(C 里面一个解析 json 的牛逼库);protoc-gen-lua, .proto 是一种谷歌发明的一种格式,用来传输网络协议,这个是将以 proto 格式编写的文件,转换成 Lua 代码,即可以在 Lua 里面编写网络协议传输对象,是为了网络服务的;LuaSocket是一个 C 库,专门为在 Lua 里面创建 Socket 而用;lpeg是解析 Lua 语法的库;int64,支持 64 位,原生 Lua 是不支持 64 位的长度的;之后如果想优化或者扩展,一般在这个 C 地方进行优化以及扩展,这样就大大增加了灵活性
* 4. 将核心库的 C 代码编译成为DLL,需要各个平台的 C/C++编译库;Android 平台需要 NDK 支持编译,windows 需要 mingw32/mingw64 支持;mac 平台需要 Xcode 等;
* 5. 2 个作者 https://github.com/topameng  https://github.com/jarjin


# ToLua 准备篇章

* 1. 为什么选择 Lua 作为热更语言,为什么苹果不能热更新?
```
    只有非越狱的iOS系统不能热更

    热更的原理:将需要替换或者新增的二进制代码和资源加载到内存,然后运行他.
        这种事情在windows上有很多方便的方式（例如dll）实现,而在Android虽然没有直接提供简单的方式仍然可以将动态链接库（so）当作数据读入到内存,然后执行之.
        但是,作为没有越狱的iOS系统,苹果因为安全或者其他原因,启动了CPU的 No eXecute bit/edb/EVP(无执行位,即硬件防病毒技术)  ,大致就是将 AppStore 审核过的代码加入签名文件,然后iOS运行app的时候会为 AppStore 审核过的代码开辟专用的内存空间,而其他app中的数据或者通过代码从线上下载的数据加载的时候会将存放的内存空间定义描述符定义为禁止运行,这样地址寄存器将不能够跳转到该空间,因此这部分代码不能运行.
        这种安全技术应该在某些系统（比如安全要求很高的银行计算机）也是使用的的,不过一般我们接触到的电子产品也就只有iOS使用了这种技术.
        Lua是就将其翻译成对应的机器指令，逐条读入，逐条解释翻译.也就是通过软件cpu/虚拟机来执行这些代码，而虚拟机代码在提交的时候已经通过了AppStore之类的审核是可以被cpu执行的，你热更的Lua脚本只是一种数据，被虚拟机加载了而已，因此不会被No eXecute bit技术所限制.其实使用lua的主要原因是lua的字节码执行比较快,并且虚拟机运行比较稳定,维护足够好.同时lua模拟器足够轻量,几百 KB,还没有一张图片大,方便扩展.
    JIT: just in time  即时编译 编译器.使用即时编译器技术可以将翻译过的机器码保存起来,以备下次使用,因此从理论上来说,JIT 技术可以接近以前的纯编译技术.但是 JIT 保存的数据是在内存里面,纯 C/C++ 编译过后的静态机器码是有可能进入到三级缓存里面的,三级缓存比内存更接近 CPU,更加快,这种情况下,JIT 永远比不上.
    JIT 会判断当前的(一串)字节码是否经常调用,如果被经常调用,就直接编译为机器码,运行时,不用再去走字节码-->逐条翻译-->机器码这样的流程了,而是直接走 字节码-->机器码 这样的流程.而直接执行机器码在 iOS 上面是不被允许的.

```
![JIT(just in time/即时编译器技术)](JIT.png)

* 2. ToLua 基于 LuaInterface,LuaInterface 是一个实现 Lua 与微软.Net 平台的 CLR 混合编程的开源库,使得 Lua 脚本可以实例化对象,访问属性,调用方法甚至使用 Lua 函数来处理事件.ToLua 保留了 LuaInterface 基本形式,重写或移除了部分内容,使代码更加简洁,提供了对 Unity 的支持,拓展了 Lua5.1源码.而最大的改进在于,LuaInterface 中 Lua 访问 CLR 需要运行时反射,对于游戏应用来说效率不够理想,ToLua 则提供了一套中间层导出工具,对于需要访问的 CLR,Unity 及其自定义类预生成 wrap 文件,Lua 访问时只访问 wrap 文件,wrap 文件接收 Lua 传递来的参数,进行类型(值,对象,委托)转换,再调用真正工作的 CLR 对象和函数,最后将返回值返回给 Lua,有效的提高了效率.
https://github.com/Jakosa/LuaInterface          

* 3. 核心功能以及文件:提供 Lua-C#值类型,对象类型转化操作交互层,(ObjectTranslator.cs,LuaFunction.cs,ToLua.cs,LuaTable.cs等);提供 Lua 虚拟机创建,启动销毁,require ,dofile,dostring,traceback 等相关支持.(LuaState.cs,LuaStatic.cs);提供导出工具,利用 C#反射,对指定的 C#生成对应的 wrap 文件,启动后将所有 wrap 文件注册到 Lua 虚拟机中.(ToLuaMenu.cs,ToLuaExport.cs,ToLuaTree.cs,LuaBinder.cs,CustomSetting.cs 等);提供 C# 对象和 Lua userdata 对应关系,使该 userdata 能访问对应 C#对象属性,调用对应 C#对象函数,Lua 支持一定的面向对象(类,继承),管理这些对象的内存分配与生命周期,GC.(LuaState.cs);提供支持功能 Lua Coroutine,反射等,Lua 层重写部分性能有问题对象如 Vector 系列.(Vector3.lua 等);

* 4. 