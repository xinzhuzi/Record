---
title: ToLua C/C++ C# 交互
date: 2020-05-08 11:41:32
top: 701
categories:
- Unity
tags:
- ToLua
---

# C/C++ C# 交互

* 1. 首先介绍这本书 <<NET CLR via C#(第4版)>> 这本书说明了高级 windows 系开发程序猿需要掌握的技能.以及<<[精通.NET互操作：P／Invoke、C..Interop和COM.Interop].黄际洲等>>这本书.需要深研才能正常理解 toLua 的背景知识

* 2. .NET P/Invoke Platform Invoke(P/Invoke) 平台调用,托管代码与非托管代码的交互,重要区别在于 C++ 与 C# 运行与内存分配的区别;C++ Introp, 主要用于Managed C++(托管C++)中调用C++类库;COM Interop, 主要用于在.NET中调用COM组件和在COM中使用.NET程序集。
```
    平台调用的调用过程          
    > 第一步也就是查找DLL       
    > 将找到的DLL加载到内存中。     
    > 查找函数在内存中的地址并把其参数推入堆栈，来封送所需的数据。CLR只会在第一次调用函数时，才会去查找和加载DLL，并查找函数在内存中的地址。当函数被调用过一次之后，CLR会将函数的地址缓存起来，CLR这种机制可以提高平台调用的效率。在应用程序域被卸载之前，找到的DLL都一直存在于内存中。     
    > 执行非托管函数  

    C++ Interop 
    C++ Interop 允许托管代码和非托管代码存在于一个程序集中，甚至同一个文件中,与平台调用不一样;      
    C++ Interop 是在源代码上直接链接和编译非托管代码来实现与非托管代码进行互操作的，而平台调用是加载编译后生成的非托管DLL并查找函数的入口地址来实现与非托管函数进行互操作的。C++ Interop使用托管C++来包装非托管C++代码，然后编译生成程序集，然后再托管代码中引用该程序集，从而来实现与非托管代码的互操作。   


    在.NET中使用COM组件
    不在这里多做解释

```

* 3. 非托管代码与托管代码的交互方式 
```
    首先要用 C++ 写好代码并导出为 DLL,并且记录下 DLL 外放接口       
    在 C# 中,定义非托管函数声明,也就是 C++的DLL外放接口,使用 DLLImport 特性;        
    在 C# 中,使用代码,调用 DLLImport 特性管理的C++外放接口的函数,就是托管代码调用非托管代码,


    C/C++ 非托管代码:
    extern "C"
    {
        extern void GetAllData(const char *config_name, void **ptrArray, int &arraySize) {}
    }

    C# 与 C/C++ 交互代码:
    
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetAllData")]
    public static extern unsafe void GetAllDataImpl(string config_name, [In, Out] IntPtr[] data, ref int len);

    C# 托管代码:

    private const int PINVOKE_ARRAY_LEN = 65536;
    private static IntPtr[] pInvoke_Array = new IntPtr[PINVOKE_ARRAY_LEN];
    public static unsafe IntPtr[] GetAllData(string config_name, ref int count)
    {
        Array.Clear(pInvoke_Array, 0, pInvoke_Array.Length);
        GetAllDataImpl(config_name, pInvoke_Array, ref count);
        return pInvoke_Array;
    }      

```

* 4. DllImport 在 System.Runtime.InteropServices 命名空间下,名字叫做 DllImportAttribute 继承 Attribute;     
DllImport类中的特性字段的含义
```
CallingConvention 调用约定;         
CallingConvention.Winapi 不是调用约定,而是平台的调用约定;如在 windows 平台上面,默认值是CallingConvention.StdCall,在 windows CE 上面是 CallingConvention.Cdecl;          
CallingConvention.Cdecl 调用者(托管代码C#)清理堆栈。这允许使用可变参数来调用函数，这使得它适合于接受可变数量参数的方法，如 printf();        
CallingConvention.StdCall 被调用(非托管代码C/C++)方清理堆栈。这是使用平台调用非托管函数的默认约定;       
CallingConvention.ThisCall 第一个参数是this指针，它存储在寄存器ECX中。其他参数被推入堆栈。这个调用约定用于调用从非托管DLL导出的类的方法,也就是 C++ 的类实例对象(非静态成员方法.         
CallingConvention.FastCall 此调用约定C#中不受支持       


string EntryPoint  表示要调用的DLL入口点的名称或序号;      
CharSet CharSet     属性是用来确定在托管与非托管调用的过程中用什么字符编码来封送数据; .NET 平台选择CharSet.Unicode,C++选择CharSet.Ansi,如果我们不清楚是那种,就选择CharSet.Auto;                 
bool SetLastError  指示被调用者在从带属性方法返回之前是否调用SetLastError Win32 API函数。           
bool ExactSpelling 控制该字段是否导致公共语言运行库在非托管DLL中搜索指定的入口点名称以外的其他入口点名称.为 true 时,必定指定一个函数名字,而去查找从而提高查找效率.          
bool PreserveSig  指示是否将具有HRESULT或retval返回值的非托管方法直接转换为异常，或者是否将HRESULT或retval返回值自动转换为异常。            
bool BestFitMapping     在将Unicode字符转换为ANSI字符时启用或禁用最佳映射行为。         
bool ThrowOnUnmappableChar    启用或禁用对转换为ANSI "?"字符的不可映射Unicode字符抛出异常。
```

* 5. 问题,非托管代码(C/C++)的对象与内存与托管代码(C#)的对象与内存是否可以互换?如果可以,请描述?如果不可以,请描述?
```
    char,int 等这一类内存结构与布局一致的是可以直接互换的;
    非托管内存与托管内存的结构与布局是不一样的,不能互换,当内存的结构与布局不一致时,需要借助于其他手段.
```
* 6. 如何释放非托管的内存?
```
    .NET 平台的默认内存分配和回收都是基于 COM(组件对象模型)的.
    如果用的是自己写的 C/C++,.Net 平台不管,这时候,需要定义一个非托管释放内存的函数或者方法,让托管代码去调用.
```

* 7. 上面我们展示了一种调用方式,下面我们展示另外一种调用方式,这 2 个都是动态调用.
```
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int add(int x, int y);

    IntPtr dlladdr = Interop.DynamicPinvoke.LoadLibrary(dllpath);
    IntPtr procadd = Win32Api.GetProcAddress(dlladdr, "_add@8");
    add adddelegate = (add)Marshal.GetDelegateForFunctionPointer(procadd, typeof(add));
    int result = adddelegate(10, 20);
    bool isfree = Win32Api.FreeLibrary(dlladdr);

    Marshal是一个很强大的P/Invoke的类，Marshal.GetDelegateForFunctionPointer方法是通过非托管内存指针获取UnmanagedFunctionPointer类型的委托；
```

* 8. 如何提高交互性能?
```
数据封送:是——在托管代码中对非托管函数进行互操作时，需要通过方法的参数和返回值在托管内存和非托管内存之间传递数据的过程，数据封送处理的过程是由CLR(公共语言运行时)的封送处理服务(即封送拆送器)完成的。        

提高调用方法的准确度而提升性能,使用ExactSpelling=true与EntryPoint = "xxx",提高 CLR 查找函数的效率;          
托管代码与非托管代码之间传递参数时,无论传入参数还是传出参数,都需要经过封送拆收器的封送处理,由于封送过程中,会涉及到数据类型的转换,以及在托管内存与非托管内存中来回复制数据,所以数据传递也是性能瓶颈之一;        
CLR 在数据封送时,有 2 种方式,一种是锁定数据(即内存共用),一种是数据复制(即内存多拷贝一份),默认是数据复制.内存共用性能更好.要想被锁定内存,需要达到以下条件,1. 必须是托管代码调用非托管代码,2.托管数据类型必须可以直接复制到本机机构 blittable 中的数据类型(也就是 C类型与 C/C++类型一致),或者在某些情况下可以转换成本机结构数据类型,所以传参时,尽量使用基础类型,不用引用类型,这个网址上面介绍了可以在托管和非托管代码都能使用的类型,3.传递的不是 ref,out 参数,4.被调用的代码与调用代码,必须在同一线程上下文或者线程单元中.            

保证托管代码中定义的数据在内存中的布局与非托管代码中的内存布局相同,这样就能提高性能
```
* 9. 封送处理
```

```
