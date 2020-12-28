---
title: Tolua C# Lua 交互
date: 2020-05-08 11:41:32
top: 703
categories:
- Unity
tags:
- ToLua
---

# Tolua C# Lua 交互

* 1. C# 代码调用 tolua c#代码,tolua c#代码调用 tolua c 代码,tolua c 代码调用 lua c 代码,lua c 代码调用 lua 代码.
* 2. lua 代码调用 lua c 代码,lua c 代码调用 tolua c 代码,tolua c 代码调用 tolua c#代码, tolua c#代码调用 c# 代码.
* 3. 上面是 C# 调用 lua,或者 lua 调用 c 的过程,中间涉及到的基础背景知识很多,而且每个知识点,都需要去很深入琢磨的.

# C# Unity 虚拟机的关系
* 1. 高级语言,程序员可快速识别语言,程序员编写的语言,C#或者VB;
* 2. CIL/IL（Common Intermediate Language，特指在.Net平台下的IL标准,特指中间语言,CIL类似一个面向对象的汇编语言，并且它是完全基于堆栈的，它运行在虚拟机上（.Net Framework, Mono VM）的语言;         
* 3. .Net Framework/Mono 一种运行中间语言(CIL/IL)的虚拟机,虚拟机会动态的编译成汇编代码(JIT)然后再执行,也被称作为CLR(Common Language Runtime).
* 4. CLI (Common Language Infrastructure)这个术语包含CIL/IL 与 CLR(Common Language Runtime) 的集合,一般不对其进行深入解释与谈论.
* 5. C# 经过编译器(Compiler,在 Rider 里面直接 Run 就可以卡看到 IL 代码)转成了 CIL/IL 中间语言.中间语言 CIL/IL 经过 CLR(也就是虚拟机)转成二进制(0101)机器码执行.
* 6. Unity 是用到了 IL 这一层,并不是专门用的 C#,原来的 Unity 脚本 Boo 也可以转成 IL,而给 Unity 原生配套的虚拟机是 Mono VM.工程链也就是 C# --> IL --> MonoVM-->机器码-->CPU
* 7. IL2CPP 顾名思义是指:把 IL 中间语言转换成 CPP 文件;工程链是 C# --> IL  --> IL2CPP --> C++ Code --> Native C++ Compiler --> Native excutable asm --> IL2CPP VM ; 中间过程中,将 IL 变为 CPP 的原因是运行效率快以及利用各个平台的 C++编译器对代码进行编译期间的优化;为什么最后还要一个 IL2CPP VM,是因为程序员无需太多关心内存管理,所有内存分配和回收都由一个叫做 GC 的组件完成, IL2CPP VM 提供 GC管理,线程创建等工作.
* 8. 由于 C++ 是静态语言,这意味着不能使用动态语言的酷炫特性,运行时生成代码并执行肯定是不可能的,这就是AOT(Ahead Of Time)编译(不是 JIT/Just In Time 编译);即交互方式/方法在打包之前已经确定,不可能运行时生成另外一种交互方式/方法并执行.