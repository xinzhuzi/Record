---
title: ToLua C/C++ C# 交互
date: 2020-05-08 11:41:32
top: 701
categories:
- Unity
tags:
- ToLua
---


# C# 需要阅读的书籍
* 1. << NET探秘：MSIL权威指南 >> Serge Lidin 著作,包建强 翻译.
* 2. << NET CLR via C#(第4版) >> Jeffrey Richter 著作,周靖 翻译.
* 3. << 精通.NET互操作：P／Invoke、C..Interop和COM.Interop.黄际洲等 >> 黄际洲 崔晓源 编著.

# C# 需要知道的基础
* 1. C#特性 Attribute 是一种可由用户自由定义的修饰符(Modifier),特性Attribute 的作用是添加元数据.一般用法是,放在方法的上面用反射来遍历所有这种带有特性的方法/类/属性等,用来做一些特殊设置.
* 2. 引用类型-->System.Object.值类型-->System.ValueType-->System.Object.装箱在值类型向引用类型转换时发生, 拆箱在引用类型向值类型转换时发生;在 Rider 编译器中点击 Build-->Build Solution 即可,在查某个C#代码时,找出Tools IL Viewer,就可以看到 IL 代码,只要看到 box [ mscorlib ] System.xxx 等字样,则发生了装箱拆箱.
* 3. ToLua LuaDLL.cs,包含了 C#调用 C/C++ 的函数库.在不同的平台上mono会去加载对应的tolua.dll或者tolua.so等文件并调用对应的函数

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

    C#调用 C/C++------------------------------------------------------------
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
    C/C++调用 C#------------------------------------------------------------

    C/C++ 非托管代码:
    extern "C"
    {
        typedef void(*NoParamDelegate)()
        typedef void(*StringParamDelegate)(const char * del1)
        extern void DoSomething (NoParamDelegate del1, StringParamDelegate del2)
        {
            del1();
            del2("11111");
        }
    }

    C# 与 C/C++ 交互代码:

    [DllImport ("__Internal")]
    private static extern void DoSomething (NoParamDelegate del1, StringParamDelegate del2);

    C# 托管代码:
    delegate void NoParamDelegate ();
    delegate void StringParamDelegate (string str);
    
    [MonoPInvokeCallback(typeof(NoParamDelegate))]
    public static void NoParamCallback() {
        Debug.Log ("Hello from NoParamCallback");//被 C/C++调用了
    }
    
    [MonoPInvokeCallback(typeof(StringParamDelegate))]
    public static void StringParamCallback(string str) {
        Debug.Log(string.Format("Hello from StringParamCallback {0}", str));//被 C/C++调用了
    }

    // 用于进行初始化
    void Start() {
        DoSomething(NoParamCallback, StringParamCallback);
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
    非托管代码分配内存主要有 3 种方式,C 语言的内存分配一般是 malloc/free ,C++语言一般是 new/delete;COM(windows 单独的平台)分配内存的主要方法是 CoTaskMemAlloc/CoTaskmemFree;
    .NET 平台的默认内存分配和回收都是基于 COM(组件对象模型)的,游戏目前不采用这种方式.这种方式是由封送拆收器是能够将其释放掉的.
    如果用的是自己写的 C/C++,.Net 平台不管,这时候,需要定义一个非托管释放内存的函数或者方法,让托管代码去调用,即必须定义 2 个方法由托管方法调用,一个创建,一个销毁.
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

    (1) 必须是托管代码调用本机代码（native code）的情形。
    (2) 托管数据类型必须是可直接复制到本机结构中的（blittable）数据类型，或者能够在满足某些条件下转变成 blittable 类型的数据类型
    (3) 传递的不是引用（out 或 ref）参数
    (4) 被调用代码和调用代码必须处于同一个线程上下文（thread context）或者线程单元（hread apartment）中。
    (5) blittable 的数据类型 System.Byte  System.SByte  System.Int16  System.UInt16  System.Int32  System.UInt 132  System. Int64  System.UInt64  System.Single  System.Double  System.Intptr  System.UIntptr  blittable类型的数组  全是blittable类型的类.这些值说明了一个问题,骚操作不要有,你就能得到很好的性能.
    (6) DllImport 参数中 ExactSpelling 字段设置为 true,显式指定要调用的非托管函数的名称,以优化平台调用在非托管 DLL 中搜索函数的方式.不要使用大量字符串互相调用,转成 byte 数组;

    保证托管代码中定义的数据在内存中的布局与非托管代码中的内存布局相同,这样就能提高性能
```

* 9. Marshal,封送处理,封送处理一般情况下效率不高,因为其过程是将一块内存重新复制一份,变为其他语言可以接收的类型,再传入其他语言.
```
    如果 C/C++写的接口里面分配了内存,但是没有对应的内存释放方法(虽然不可取,但是确实可以这么做),这个情况下怎么把内存释放掉?
    wchar * t GetStringMalloc();//这个里面采用 malloc 来分配内存
    [DllImport(xxx.dll)],CallingConvention = CallingConvention.Cdecl,CharSet = CharSet.Unicode)]
    static extern string GetStringMalloc(); //封送拆收器会将wchar *类型封送成 string 类型
    
    //这种情况,封送拆收器会尝试释放内存,使用的方法是 CoTaskMemFree,当然会失败.也就是最终会造成内存泄露的问题.
    //第一种方法是分配方式由 CoTaskMemAlloc 来分配内存,并且内存增大了;
    //第二种方法是将返回的类型wchar * 修改成 IntPtr 类型,IntPtr 是比较特殊的类型,在封送拆收器将非托管数据封送成 IntPtr 时,直接将非托管指针复制进 IntPtr 的值中,无需经过任何类型的转换或数据复制的过程,另外,此块内存可以在托管方法里面进行释放,托管方法里面再调用非托管方法进行释放
    [DllImport(xxx.dll)],CallingConvention = CallingConvention.Cdecl,CharSet = CharSet.Unicode)]
    static extern IntPtr GetStringMalloc();
    [DllImport(xxx.dll)],CallingConvention = CallingConvention.Cdecl,CharSet = CharSet.Unicode)]
    static extern void free(IntPtr p);

    C# f(){
        IntPtr p = GetStringMalloc();
        string s = Marshal.PtrToStringUni(p);
        //释放函数
        free(p);
    }

    由此可得出结论,一切引用类型/堆上内存分配,不要使用封送内存(内存多复制一份)的方式,直接使用指针就行了,不要搞骚操作,不可取.
    尽量使用 blittable 的数据类型(就是基本数据类型,能不传数组,不传字符串,就不要传);
```

* 10. 再次提醒,骚操作不要有,按照规范来,性能会提高的,不知道的时候一定要去看书,这个地方测试性能比较困难,大概率按照书上的搞就行了.互调一次产生的消耗也是有的,性能损耗在所难免.

* 11. 托管代码与非托管代码的调用流程

![调用过程)](调用过程.png)

```

    (1) 调用 Loadlibrary 加载非托管 DLL 到内存中，并调用 function 获得内存中非托管函数的指针。        
    (2) 为包含非托管函数地址的托管签名生成一个 Dllimport 存根（stub）。
    (3) 压入被调用方保存的寄存器。
    (4) 创建一个 Dlllmport 帧（fame），并将其压入到堆梭帧（stack fr)
    (5) 如果分配了临时内存，则预置一个清除列表，以便调用完成后快速将内存释放掉。
    (6) 封送参数，封送操作可能会分配内存。
    (7) 将垃圾回收（Garbage Collection）模式从协作式（cooperative）改为抢占式 (preemptive），以便垃圾回收可以在任何时间进行。
    (8) 加载目标地址并对其进行调用。
    (9) 如果设置了 Setlasterror，则调用 Getlasterror 并将其返回的结果存储到一个线程上，而该线程则是抽象存储在线程本地存储（Thread Local Storage）中;
    (10) 将垃圾回收模式改回到协作式。
    (11) 如果 Preservesig 为 false 且非托管方法返回一个失败的 HRESULT，则抛出异常。
    (12) 如果没有异常被抛出，将 out 和 ref 引用类型的参数回传（back propagate)
    (13) 将栈指针寄存器（Extended Stack Pointer）恢复成初始值，以还原调用方弹出的参数。

```
* 12. mono 虚拟机可动态加载 dll,更新 Assembly-CSharp.dll ,ILRuntime也是此原理.IL2CPP 不行.反射加载 DLL 也可以.

* 13. Marshal 类有更多的调用非托管 dll 的方式 比如 Marshal.GetDelegateForFunctionPointer(IntPtr p);

# C/C++/C#混合编程/编译

* 1. unsafe static void f(); unsafe{}; fixed(xxx){}; unsafe{fixed(xxx){}}; 等关键字,unsafe关键字不意味着不托管代码
* 2. 直接操作指针与地址;指针的生命规范,不能是C#中的引用类型,不能是泛型类型,并且代码块内部不包含引用类型,使用基本类型操作指针就行了,不要骚操作;有效类型的指针包括枚举,预定义值类型（sbyte, byte, short, ushort, int, uint, long, ulong, char, flaot, double, decimal和bool） 以及指针类型（如 byte** ）. void* 指针也是有效的，它代表指向未知类型的指针.
* 3. 代码定义好指针后,在访问它之前必须为它赋值,语法就是 C/C++ 语法,但是内存可能因为 GC/栈变化 而被迫移动位置,即内存块的首地址变化了.这时候需要一个fixed去固定内存块.fixed语句要求在其作用域内声明指针变量,这样可以防止数据不再固定时访问到fixed语句外的变量;
* 4. 在调用栈上分配数组,栈分配的数据不会被垃圾回收,也不会被终结器清理,和引用类型一样,要求stackalloc(栈分配 byte* bytes = stackalloc byte[ 42 ];)数据是基础类型的数组;尽量少用,没有办法显示地释放stackalloc数据;
* 5. 这几个关键字 unsafe fixed 可以与 Marshal类,形成更复杂的写法,去提高效率.