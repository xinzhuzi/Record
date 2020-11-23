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

## Lua 基础知识学习

* 1. 网站学习
```
    官网 https://www.lua.org/ 
    视频1 https://www.bilibili.com/video/av39228822/ 
    视频2 https://www.bilibili.com/video/BV1H4411b7o9
```
* 2. Lua 原生的 C 代码已更新至 5.4.1 版本;Lua是动态类型的,通过使用基于寄存器的虚拟机解释字节码来运行，并且具有自动内存管理和增量垃圾收集.
* 3. 先查看一下 Lua 原生C代码的文档,英文文档:https://www.lua.org/manual/5.4/ ;      https://www.lua.org/manual/5.3/manual.html      
中文文档,云风大神翻译的,尽量看这个,https://cloudwu.github.io/lua53doc/contents.html#contents
* 4. 以下操作以云风5.3 为准

# 值与类型
* 1. number 标准 Lua 使用 64 位,LuaJIT 没有 64 位,但是 toLua 里面有个int64 补足了缺陷.
* 2. Userdata ,一种是完全用户数据/ full Userdata,由 Lua 管理的内存对应的对象;轻量用户数据/light Userdata,指一个简单的 C 指针.只有通过C API，才能在Lua中创建或修改Userdata值;用户数据由宿主程序 C/C++ 控制.使用这个类型是为了允许任意C数据存储在Lua变量中.
* 3. thread ;Lua 中的这个类型只表示协程.
* 4. table 是一个关联数组, 关联数组（associative array )是一种常用的抽象数据类型。它有很多别名，例如associative container , map , mapping , dictionary , finite map , table,index 等.它的特点是由一个关键字和其他各种属性组成的集合。典型的操作包括插入，删除和查找等。而用于描述关联数组最常用的是哈希表 （hash table ）和自平衡二叉搜索树（self-balanced binary search tree )(包括红黑树 （red-black tree ）和avl树 （avl tree ），有时可能使用B-tree (适用于关联数组太大的情况，比如数据库等））。哈希表和自平衡二叉搜索树的性能对比如下：平均情况下哈希表的查找和插入操作的复杂度为O（1），而自平 衡二叉搜索树的查找和插入操作的复杂度为O（log(n))。而最坏情况下平衡二叉搜索树的查找和插入操作的复杂度仍为O(log(n))，而哈希表的查 找和插入操作的复杂度可能达到O（n）;       table类型是一个 hash 表,可以用其他任何类型(除了 nil NaN)作为 key 值(索引)使用;表中不能存放 nil;
* 5. 变量并没有持有各个类型的值,而是保存了对这些类型对象的引用,赋值,参数传递,函数返回,都是针对引用而不是针对值进行的操作.
* 6. 全局变量,在定义变量的时候,前面不加上 local 就是全局变量;全局变量保存的位置在一个 table 里面,这个 table 的名字是 _G.    
如果我们想避免这种情况可以使用setfenv,这个函数可以将当前函数/主函数等,重新设置函数内的全局变量的保存位置,原来是全部保存在_G 中的,现在使用setfenv(1, {})表示将当前函数内的全局变量保存在一个空表中,而这个表是空的,所以再使用这个函数内的全局变量就会报错.    https://www.cnblogs.com/guangyun/p/4685353.html
* 7. 元表,getmetatable 获取一个元表,setmetatable 设置一个元表,元表中的元方法可以是:
```
    __add: + 操作, 如果任何不是数字的值（包括不能转换为数字的字符串）做加法， Lua 就会尝试调用元方法。 首先、Lua 检查第一个操作数（即使它是合法的）， 如果这个操作数没有为 "__add" 事件定义元方法， Lua 就会接着检查第二个操作数。 一旦 Lua 找到了元方法， 它将把两个操作数作为参数传入元方法， 元方法的结果（调整为单个值）作为这个操作的结果。 如果找不到元方法，将抛出一个错误。         
    __sub: - 操作,同上       
    __mul: * 操作,同上       
    __div: / 操作,同上       
    __mod: % 操作,同上       
    __pow: ^ （次方）操作,同上       
    __unm: - （取负）操作,同上       
    __idiv: // （向下取整除法）操作,同上       
    __concat: .. （连接）操作。 行为和 "add" 操作类似， 不同的是 Lua 在任何操作数即不是一个字符串 也不是数字（数字总能转换为对应的字符串）的情况下尝试元方法。
    __band: & （按位与）操作。 行为和 "add" 操作类似， 不同的是 Lua 会在任何一个操作数无法转换为整数时尝试取元方法。        
    __bor: | （按位或）操作,同上       
    __bxor: ~ （按位异或）操作,同上       
    __bnot: ~ （按位非）操作,同上       
    __shl: << （左移）操作,同上       
    __shr: >> （右移）操作,同上       
    __len: # （取长度）操作。 如果对象不是字符串，Lua 会尝试它的元方法。 如果有元方法，则调用它并将对象以参数形式传入， 而返回值（被调整为单个）则作为结果。 如果对象是一张表且没有元方法， Lua 使用表的取长度操作（参见 §3.4.7）。 其它情况，均抛出错误。
    __eq: == （等于）操作。 和 "add" 操作行为类似， 不同的是 Lua 仅在两个值都是表或都是完全用户数据 且它们不是同一个对象时才尝试元方法。 调用的结果总会被转换为布尔量。
    __lt: < （小于）操作。 和 "add" 操作行为类似， 不同的是 Lua 仅在两个值不全为整数也不全为字符串时才尝试元方法。 调用的结果总会被转换为布尔量。
    __le: <= （小于等于）操作。 和其它操作不同， 小于等于操作可能用到两个不同的事件。 首先，像 "lt" 操作的行为那样，Lua 在两个操作数中查找 "__le" 元方法。 如果一个元方法都找不到，就会再次查找 "__lt" 事件， 它会假设 a <= b 等价于 not (b < a)。 而其它比较操作符类似，其结果会被转换为布尔量。
    __index: 索引 table[key]。 当 table 不是表或是表 table 中不存在 key 这个键时，这个事件被触发。 此时，会读出 table 相应的元方法。
    尽管名字取成这样， 这个事件的元方法其实可以是一个函数也可以是一张表。 如果它是一个函数，则以 table 和 key 作为参数调用它。 如果它是一张表，最终的结果就是以 key 取索引这张表的结果。 （这个索引过程是走常规的流程，而不是直接索引， 所以这次索引有可能引发另一次元方法。）

    __newindex: 索引赋值 table[key] = value 。 和索引事件类似，它发生在 table 不是表或是表 table 中不存在 key 这个键的时候。 此时，会读出 table 相应的元方法。
    同索引过程那样， 这个事件的元方法即可以是函数，也可以是一张表。 如果是一个函数， 则以 table、 key、以及 value 为参数传入。 如果是一张表， Lua 对这张表做索引赋值操作。 （这个索引过程是走常规的流程，而不是直接索引赋值， 所以这次索引赋值有可能引发另一次元方法。）

    一旦有了 "newindex" 元方法， Lua 就不再做最初的赋值操作。 （如果有必要，在元方法内部可以调用 rawset 来做赋值。）

    __call: 函数调用操作 func(args)。 当 Lua 尝试调用一个非函数的值的时候会触发这个事件 （即 func 不是一个函数）。 查找 func 的元方法， 如果找得到，就调用这个元方法， func 作为第一个参数传入，原来调用的参数（args）后依次排在后面。
```

* 8.  Garbage Collection 垃圾收集;Lua中的垃圾收集器(GC)有两种工作模式:增量式和分代式。      
Lua 实现了一个增量标记-扫描收集器。 它使用这两个数字来控制垃圾收集循环： 垃圾收集器间歇率/garbage-collector pause 和 垃圾收集器步进倍率/garbage-collector step multiplier. 这两个数字都使用百分数为单位 （例如：值 100 在内部表示 1 ）;可以通过 lua_gc 或collectgarbage 控制与收集 Lua 的对象;__gc可以作为元表中的元方法,在垃圾收集的时候进行触发,设置自己的资源管理工作;当一个被标记的对象成为垃圾后,GC 不会立刻回收它,会将其放入链表,收集完成之后,会检查链表中的每个对象的__gc 方法;GC 会回收弱引用的对象, __mode 域是一个包含字符 'k' 的字符串时，这张表的所有键皆为弱引用。 当 __mode 域是一个包含字符 'v' 的字符串时，这张表的所有值皆为弱引用               

* 9. Lua 使用一个 虚拟栈 来和 C 互传值。栈上的的每个元素都是一个 Lua 值 （nil，数字，字符串，等等）。无论何时 Lua 调用 C，被调用的函数都得到一个新的栈， 这个栈独立于 C 函数本身的栈，也独立于之前的 Lua 栈。 它里面包含了 Lua 传递给 C 函数的所有参数， 而 C 函数则把要返回的结果放入这个栈以返回给调用者,也就是交互栈.     
为了正确的和 Lua 通讯， C 函数必须使用下列协议。
```
    C 函数模型      typedef int (*lua_CFunction) (lua_State *L);  这个协议定义了 C 的参数以及返回值传递方法： C 函数通过 Lua 中的栈来接受参数， 参数以正序入栈（第一个参数首先入栈）。

    当函数开始的时候， lua_gettop(L) 可以返回函数收到的参数个数。 第一个参数（如果有的话）在索引 1 的地方， 而最后一个参数在索引 lua_gettop(L) 处。 当需要向 Lua 返回值的时候， C 函数只需要把它们以正序压到堆栈上（第一个返回值最先压入）， 然后返回这些返回值的个数。 在这些返回值之下的，堆栈上的东西都会被 Lua 丢掉。 和 Lua 函数一样，从 Lua 中调用 C 函数也可以有很多返回值。

    实际例子:
    static int foo (lua_State *L) {
       int n = lua_gettop(L);    /* 参数的个数 */
       lua_Number sum = 0.0;
       int i;
       for (i = 1; i <= n; i++) {
         if (!lua_isnumber(L, i)) {
           lua_pushliteral(L, "incorrect argument");
           lua_error(L);
         }
         sum += lua_tonumber(L, i);
       }
       lua_pushnumber(L, sum/n);        /* 第一个返回值 */
       lua_pushnumber(L, sum);         /* 第二个返回值 */
       return 2;                   /* 返回值的个数 */
     }

```

* 10. Lua 的库,分为在 Lua 脚本里面使用的标准库,以及在 C 中为 Lua 编程的辅助库.可以称之为高级 API.
标准库是专门为 Lua 程序员而编写的,为了提高效率.
```
    标准库包括:
        basic library 基础库(assert,collectgarbage,dofile,error,_G,getmetatable,ipairs,load,loadfile,next,pairs,pcall,print...)       
        coroutine library 协程库        
        package library 包管理库        
        string manipulation 字符串控制      
        basic UTF-8 support 基础 UTF-8 支持     
        table manipulation 表控制      
        mathematical functions数学函数        
        input and output 输入输出        
        operating system facilities 操作系工具
        debug facilities 调试工具
```

# Lua 标准版源码阅读顺序
* 1. Lua 文件中的代码功能
```
    重要:
        lmathlib.c 数学函数库
        lstrlib.c 用于字符串操作和模式匹配的标准库
        lapi.c Lua 的 API,API 在内部的实现,
        luaconf.h Lua的配置文件,宏定义等
        lobject.h/lobject.c Lua对象的类型定义,在Lua对象上的一些通用函数,这个类比较重要.
        lstate.h/lstate.c 全局状态,这个类比较重要.
        lopcodes.h/lopcodes.c Lua虚拟机的操作码,需要知道 CPU 实现的细节
        lvm.h/lvm.c 其中 luaV_execute 方法是重要的,是主解析器.也就是所谓的虚拟机.
        ldo.h/ldo.c Lua的堆栈和调用结构
        lstring.h/lstring.c 字符串表(保持所有字符串由Lua处理)
        ltable.h/ltable.c Lua Hash表以及数组,需要算法背景支持
        lvm.h/lvm.c 元方法处理，现在重新读取所有的lvm.c
        ldebug.h/ldebug.c 抽象解释用于查找回溯的对象名称,也执行字节码验证。
        lparser.c/lcode.c 递归下降解析器，锁定一个基于寄存器的VM。从chunk()开始，按照自己的方式完成。最后读取表达式解析器和代码生成器部分。
        lgc.c 增量垃圾收集器

    不重要:
        llex.h/llex.c Lua 脚本的词法分析模块
        lparser.h/lparser.c Lua 脚本的语法分析模块
        lcode.h/lcode.c Lua 脚本的指令生成模块
```