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

* 4. tolua#集成主要分为两部分，一部分是运行时需要的代码包括一些手写的和自动生成的绑定代码，另一部分是编辑器相关代码，主要提供代码生成、编译lua文件等操作，具体就是Unity编辑器中提供的功能。用mono打开整个tolua#的工程，文件结构大体如下所示：
![ToLua源码结构图](ToLua源码.png)       
```
    Runtime

        Source

            Generate 这个文件里面是生成的绑定代码

            LuaConst.cs 这个文件是一些lua路径等配置文件。

        ToLua

            BaseLua 一些基础类型的绑定代码

            Core 提供的一些核心功能，包括封装的LuaFunction LuaTable LuaThread LuaState LuaEvent、调用tolua原生代码等等。

            Examples 示例

            Misc 杂项，目前有LuaClient LuaCoroutine（协程） LuaLooper（用于tick） LuaResLoader（用于加载lua文件）

            Reflection 反射相关

    Editor

        Editor

            Custom

                CustomSettings.cs 自定义配置文件，用于定义哪些类作为静态类型、哪些类需要导出、哪些附加委托需要导出等。

        ToLua

            Editor

                Extend 扩展一些类的方法。

                ToLuaExport.cs 真正生成lua绑定的代码

                ToLuaMenu.cs Lua菜单上功能对应的代码

                ToLuaTree.cs 辅助树结构
```

* 5. 了解了tolua#的大致文件结构，下面我们来看下tolua#的Generate All 这个功能来分析下它的生成过程。生成绑定代码主要放在ToLuaExport.cs里面，我们并不会对每一个函数进行细致的讲解，如果有什么不了解的地方，可以直接看它的代码。

![ Generate All 流程 ](GenerateAll流程.png)  

GenLuaDelegates函数:生成委托绑定的代码，它会从CustomSettings.customDelegateList里面取出所有自定义导出的委托列表，然后把CustomSettings.customTypeList里面的所有类型中的委托根据一定规则加入到list中，最后调用ToLuaExport.GenDelegates()方法来生成委托绑定的代码，生成的代码在DelegateFactory.cs文件中。      

* 6. GenerateClassWraps 函数,遍历所有需要导出的类，然后调用ToLuaExport.Generate()方法来生成类的绑定代码。
下面我们来看下ToLuaExport.Generate()方法，其基本流程如下所示:

![ Generate Class Wraps ](GenerateClassWraps.png)  


* 7. ToLua 中的核心类       
LuaBaseRef.cs  Lua中对象对应C#中对象的一个基类，主要作用是有一个reference指向lua里面的对象，引用计数判断两个对象是否相等等。        
LuaState.cs 这里面是对真正的lua_State的封装,包括注册各种库方法,基础方法,包括初始化lua路径，加载相应的lua文件，注册我们前面生成的绑定代码以及各种辅助函数。      
ObjectTranslator.cs  接下来，我们着重说一下这个ObjectTranslator这个类，这个类代码不多，它存在的主要意义就是给lua中对C#对象的交互提供了基础，简单来说就是C#中的对象在传给lua时并不是直接把对象暴露给了lua，而是在这个OjbectTranslator里面注册并返回一个索引（可以理解为windows编程中的句柄），并把这个索引包装成一个userdata传递给lua，并且设置元表。具体可以查看tolua_pushnewudata代码.
```
    // tolua# 代码

    static void PushUserData(IntPtr L, object o, int reference)

    {

    　　int index;

    　　ObjectTranslator translator = ObjectTranslator.Get(L);

    　　if (translator.Getudata(o, out index))

    　　{

    　　if (LuaDLL.tolua_pushudata(L, index))

    　　{

    　　return;

    　　}

    　　translator.Destroyudata(index);

    　　}

    　　index = translator.AddObject(o);

    　　LuaDLL.tolua_pushnewudata(L, reference, index);

    }

    // tolua++ 代码

    LUALIB_API void tolua_pushnewudata(lua_State *L, int metaRef, int index)

    {

        lua_getref(L, LUA_RIDX_UBOX);

        tolua_newudata(L, index);

        lua_getref(L, metaRef);

        lua_setmetatable(L, -2);

        lua_pushvalue(L, -1);

        lua_rawseti(L, -3, index);

        lua_remove(L, -2);

    }
```
而在lua需要通过上面传到lua里面的对象调用C#的方法时，它会调用ToLua.CheckObject或者ToLua.ToObject从ObjectTranslator获取真正的C#对象。下面我们把ToLua.ToObject的代码做个示例：
```
    public static object ToObject(IntPtr L, int stackPos)

    {

    　　int udata = LuaDLL.tolua_rawnetobj(L, stackPos);

    　　if (udata != -1)

    　　{

    　　ObjectTranslator translator = ObjectTranslator.Get(L);

    　　return translator.GetObject(udata);

    　　}

    　　return null;

    }
```


