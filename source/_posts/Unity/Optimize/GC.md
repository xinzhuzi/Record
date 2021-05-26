---
title: Unity GC
date: 2020-05-08 11:41:32
top: 529
categories:
- Unity优化
tags:
- Unity优化
---

# 内存管理方式

* 1. **手动管理**:类似C/C++一样使用malloc/free或者new/delete来为对象分配释放内存.优点:速度快,没有额外的开销.缺点:必须清楚的知道每一个对象的使用情况,否则就容易发生内存泄露,内存野指针,空指针,内存偏移出错等等.

* 2. **半自动管理**:引用计数(Reference Count),对象创建出来后,维护一个针对该对象(可以是任何东西)的技术,使用该对象对该计数加 1,需要释放时,再减一,当计数为 0 时,销毁该对象.优点是:把创建和释放时,无需手动,并且在实际使用过程中处理,速度也快.缺点是存在循环引用的问题.例子:Unity 中的PhysX.

* 3. **全自动管理**:追踪式 GC 器(Tracing Garbage Collector), Unity使用的 GC 器是一种叫做标记/清除(Mark/Sweep)的算法,它的思路是当程序需要进行垃圾回收时,从根(GC Root)出发标记所有可达对象,然后回收没有标记的对象.                
Unity 中使用的是 Boehm-Demers-Weiser 的 GC 器:
        * Stop The World:即当发生 GC 时,程序的所有线程都必须停止工作,等 GC 完成才能继续,Unity 不支持多线程 GC,即使 Unity2019后使用的增量式 GC,在回收时也要停掉所有线程.
        * 不分代:.NET 和 Java 会把托管堆分成多个代(Generation),新生代的内存空间非常小,而且一般来说,GC 主要会集中在新生代上,这让每一次 GC 的速度也非常快,但是 unity 的 GC 是完全不分代的,即只要发生 GC,就会对整个托管堆进行 GC(Full GC).
        * 不压缩:不会对堆内存进行碎片整理.
                <!-- ![Google插件3](https://github.com/xinzhuzi/Record/tree/master/source/_posts/Unity/Optimize/GC/1.png) -->
                ![内存碎片](1.png)
        GC 会造成托管堆出现很多这样的空白"间隙",这些间隙并不会合并,当申请一个新对象时,如果没有任何一个间隙大于这个新对象大小,堆内存就会增加.


# 影响 GC 性能的主要因素
* 1. 影响GC速度的因素主要有两个:可达对象数量与托管堆的大小.可达对象是指不会当次GC被回收的对象，减少此类开销的主要方法就是减少对象数量，         
参考以下实现方法:
```
                class Item
                {
                        public int a; 
                        public short b; 
                }
                Item[] items;//对于 Item 数组,每一个元素都会产生一个对象. GC 时都需要遍历


                //而以下代码,不管 a 和 b 有多少个元素,数组都只有一个对象,这样就会减少对象数量.
                class Item
                {
                        public int[] a; 
                        public short[] b; 
                }
                Item item;
```
* 2. 优化托管堆大小,主要通过
1:减少临时分配:临时分配的内存会产生碎片.使用初始化分配,以及预分配策略.
2:减少内存泄露:再也用不到但是又因为存在对其引用无法回收的对象.防止内存无限增长,孤岛效应,循环引用等.需要手动或者工具排查.

* 3. 类与结构体
结构体创建时,要遵守对其规则,尽量紧凑.
![类对象内存结构](2.png)
类对象实例存放在堆中,对象实例一定是会占用堆内存的,而在栈中,保存的是实例的引用.
vtable 是类共有数据(不是类对象的数据,类对象是一个实例,类是一个'工厂'),包含静态变量和方法表(在 Mono 中,结构的静态变量也存放在 vtable 中,它是缓存在一个 tablecache 的哈希表当中的,而 IL2CPP 中类和结构的静态变量存在一个单独的类中).Monitor 是线程同步使用的,这 2 个指针分别占用一个 IntPtr.Size 大小(32位中是 4 字节,64 位中是 8 字节),再下面是所有字段,字段是从第 9 个字节或 17 个字节开始的,字段的对齐规则与结构体的对齐规则相同,区别是 Mono 中对象实例会把引用类型的引用摆在最前面,一个对象实例的大小(instance_size)就是 IntPtr.Size * 2+字段所占大小,结构体被装箱后在堆内存的大小也一样.类最好也遵守对齐规则,尽量紧凑.

* 4. 装箱拆箱
如果结构体实现了某个接口，那么结构体转换为接口会发生装箱。
对值类型实例调用GetType()会发生装箱。
对结构体调用ToString()，GetHashCode():在Mono中，直接调 用不会发生装箱，但是在IL2CPP中却会有装箱。如果重写了这两个方法，调用时就都不会发生装箱，但是如果调用了base.ToString() 或base.GetHashCode()，还是会发生装箱。
某些容器类操作时发生的装箱会在下面提到。

* 5. 参数与泛型
C#中所有参数传递都是值传递，方法传递的参数都是值的副本。
对象和结构体的区别就是，因为栈中存放的只能是对象的 引用，所以传参时，复制的是引用的值，大小是4字节(32位)或8 字节(64位)，也就是IntPtr.Size。如果参数是结构体，则会把整 个结构体都复制一次，大小就是这个结构体的大小，如果传递的结 构体很大，也会造成一定的开销。

* 6.  ref，in和out
这三个关键字，可以使参数按引用传递，ref表示传进的参数可读 写，in表示参数是只读的，out表示参数是只写的，类似返回值。
按引用传参，特别是传递较大的结构体参数，可以减少复制带来的开销。在MSDN的优化建议中也提到，推荐所有大于IntPtr.Size的结构体，传参时都按引用传递。
```
                //错误的写法 1
                public class Test
                {
                        //缓存了一份 Ray 的结构体类型数据
                        public UnityEngine.Ray ray { get; private set; } 
                        ...
                        void Calc()
                        {
                                //这个地方复制了一份用来计算
                                UnityEngine.Ray ray1 = ray; 
                                ...
                                //这个地方又复制了一份
                                Calc1(ray1);
                        }
                        void Calc1(UnityEngine.Ray ray2) 
                        {
                                ...
                        }
                }

                public class Test
                {
                        //缓存了一份 Ray 的结构体类型数据
                        private UnityEngine.Ray _ray;
                        //暴漏给外部的接口只是对这个结构体的引用.
                        //缺点是可以让其他地方随意修改缓存的_ray 变量
                        public ref UnityEngine.Ray ray => ref _ray; 
                        //改成这个样子,通过 readonly 标记其为只读,其他地方也要做相应的更改.
                        public ref readonly UnityEngine.Ray ray => ref _ray; 
                        ...
                        void Calc()
                        {
                                //复制了一份结构体引用,没有复制结构体
                                ref UnityEngine.Ray ray1 = ref ray;
                                ...
                                //复制了一份结构体引用,没有复制结构体
                                Calc1(ref ray1);
                        }
                        void Calc1(ref UnityEngine.Ray ray2)
                        {
                                ...
                        }
                }
        ref return一定不要返回方法内局部变量的引用。
        https://docs.microsoft.com/zh-cn/dotnet/csharp/write-safe-efficient-code
```

* 7. 使用泛型优化装箱
```
                //有时方法为了保证通用性，会使用object作为参数
                //这样如果参数传入值类型，就会产生装箱
                void Func(object o) 
                {
                        ...
                }
                //使用泛型,可以优化装箱
                //但是这样的方法在IL2CPP中会有一个问题，因为IL2CPP是AOT机制的
                //所有泛型调用最后会为每一种类型单独生成代码，增加代码体积，同时也会增加堆内存
                void Func<T>(T o)
                {
                        ...
                }
                //有多少类型T 在使用,就会有多少类生成
                //这是因为IL2CPP中有一个泛型类型共享的机制，如所有引用类型，都会只生成参数是RuntimeObject的函数，
                //还有整数和枚举，也会只生成一个参数是int32_t的函数，
                //但是，对于其它值类型，是没有办法共享的，所以只能针对每一个类型单独生成一个函数。
```
* 8. 可变参数
使用params的可变参数是一种语法糖，它和传入一个数组是等价的。
```
        void Func(params int[] n);
        Func(1,2,3);它其实等价于 Func(new int[]{1,2,3});会临时产生一个数组
        Func();这个调用等价于Func(Array.Empty<int>);这个是C#缓存的一个空数组，并不会临时产生新对象。
        //所以，只要可变参数不为空，就一定会产生临时的数组，大量调用 会产生很多的GC Alloc。


        优化方法是用一系列若干数量的参数的重载方法代替，如C#的 string.Format是这样写的(来自https://github.com/Unity- Technologies/mono/blob/unity- master/mcs/class/referencesource/mscorlib/system/string.cs ):

        public static string Format (string format, object arg0); public static string Format (string format, object arg0, object arg1);
        public static string Format (string format, object arg0, object arg1, object arg2);
        public static string Format (string format, params object[] args);

        //把常用的1个、2个、3个参数的方法单独提出来，剩下的再用可变参 数来做。
```

* 9. Conditional特性
在开发过程中,一般为了调试方便,会在代码中加入很多控制台输出,Debug.Log("123");
真机平台会调用unityLogger.logEnabled或 unityLogger.filterLogType对其进行开关，但是这种并不能阻止传 入参数本身造成的GC Alloc,Debug.Log(123);Debug.Log(string.Format("12{0}", 3));
类似这样的装箱和字符串操作都会产生 GC Alloc,这里可以使用如下方法:
```
        #if UNITY_EDITOR
        Debug.Log(123);
        #endif
        ...
        #if UNITY_EDITOR 
        Debug.Log(string.Format("12{0}", 3)); 
        #endif
        //该方法可以避免在真机上生成代码，可是需要用#if和#endif扩住的 地方太多了，代码可读性会降低，这时可以用一个[Conditional]来 完成这样的操作，如下面方法:
        [Conditional("UNITY_EDITOR")]
        public static void Print(object message) 
        {
                Debug.Log(message);
        }

        Print(123);
        ...
        Print(string.Format("12{0}", 3));
        在真机上不会生成任何代码，也就不会发生GC Alloc了，同时也维 护了代码的可读性。
```

* 10. 对象和结构体在数组中的分布
现在比较流行的DOTS经常提到一个概念，就是缓存命中率。CPU从内存中直接读取的速度是非常慢的,所以会有高速缓存的存在,CPU 读取数据会先从高速缓存中读取,如果缓存中没有该数据,才会从内存中取一片连续的数据放到缓存中,如果先后读取的两个数据不在同一片连续内存中,就会导致 cache miss(缓存没有命中),会从内存中读取数据,读取数据的速度就会变慢.
在存放引用类型的数组中,存放的是对象值的引用,内存中大概是这样的:
![数组内存布局](3.png)
数组中存的是对象的引用,数组所占内存大小是数组长度* IntPtr.Size,而真正的数据要到引用指向的地址上读取,他们的内存是不连续的.这就直接导致了缓存命中率低效.
而存放值类型的数组,里面的内存是连续的,不是指向型的,数组所占内存大小是数组长度 * 单个数据内存大小。
![数组内存布局](4.png)


* 11. 常用容器的数据结构及内存增长方式
List,Stack,Queue这几个容器都是维护着一个一定容量的数组,当添加数据时,如果数组长度小于添加后的长度,容器容量会增长,例子:
```
        if(Count + 1 < array.Length)
        {
                var newArray = new T[array.Length * 2]; 
                Array.Copy(array, 0, newArray, array.Length); 
                array = newArray;
        }
```
        
可以看到,容器增长是靠建立一个新的数组实现的,新数组长度是旧数组的两倍大小(这是为了防止频繁增长,但是在一些高频需要增长的地方,这个 2 倍扩容容易产生垃圾碎片,需要根据实际内存使用情况来扩容这个倍数).建立新数组后,会将数据从旧数组复制到新数组中,最后旧的数组会作为内存垃圾丢弃.当然也可以通过设置 Capacity 或者 TrimExcess 来指定容器的容量,但是也会发生新建数组丢弃旧数组的过程,像 List 这样的数据地址会变的容器不能使用 ref 来存取.              

HashSet,Dictionary 在.NET4.X 中,这两个容器维护了一个 bucket 数组和一个 entry 数组,分别存放索引和数据,当调用 Add 导致容器增长时,会增长到大于旧容量两倍的一个素数的大小,及时调用 TrimExcess 缩小容量,也只能缩小到一个素数的大小

以上类型的容器,需要指定存放数据容量的大小,指定容器初始值,这样可以防止因为容量而拼房的改变.

LinkedList 链表,里面有一个 LinkedListNode对象,该对象的 Next 指向下一个 LinkListNode 对象,这个是完全不连续的内存,LinkListNode 可以用缓存的方式减少 GC Alloc,即尽量不删除节点,来防止 GC.

* 12. 容器产生的装箱和拆箱
foreach 在 unity5.6 已经修正了 bug,但是有些方法为了通用性,传入的参数像是 ICollection<T>,IDictionary<K,V>等接口,还是会发生装箱.Linq 要避免使用,除非你了解它的源码!

在 Dictionary,HashSet 如果 Key 是结构体,对齐进行操作是会发生装箱的,解决办法是实现 IEqualityComparer<T> .此外,如果 Key 是枚举类.NET4.x 之前也是会有装箱的,解决办法是用 int代替或者实现 IEqualityComparer<T>,.Net4.x 以后修复了.

* 13. 对象池
UGUI中有一个通用对象池的实现([ObjectPool.cs](https://github.com/Unity-Technologies/uGUI/blob/2019.1/UnityEngine.UI/UI/Core/Utility/ObjectPool.cs) ):
```
        class ObjectPool<T> where T : new()
        {
                private readonly Stack<T> m_Stack = new Stack<T>();
                private readonly UnityAction<T> m_ActionOnGet;
                private readonly UnityAction<T> m_ActionOnRelease;

                public int countAll { get; private set; }
                public int countActive { get { return countAll - countInactive; } }
                public int countInactive { get { return m_Stack.Count; } }

                public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease)
                {
                        m_ActionOnGet = actionOnGet;
                        m_ActionOnRelease = actionOnRelease;
                }

                public T Get()
                {
                        T element;
                        if (m_Stack.Count == 0)
                        {
                                element = new T();
                                countAll++;
                        }
                        else
                        {
                                element = m_Stack.Pop();
                        }
                        if (m_ActionOnGet != null)
                                m_ActionOnGet(element);
                        return element;
                }

                public void Release(T element)
                {
                        if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
                        if (m_ActionOnRelease != null)
                                m_ActionOnRelease(element);
                        m_Stack.Push(element);
                }
        }
```

NGUI中有也一个通用对象池的实现([BetterListPool.cs](https://github.com/sophiepeithos/optimise-ngui-gc-alloc) ):
```
        using UnityEngine;
        using System.Collections.Generic;
        using LuaInterface;

        public class PoolStack<T>
        {
        public int size = 0;
        T[] buffer = null;

        void Alloc()
        {
                T[] newList = (buffer != null) ? new T[Mathf.Max(buffer.Length << 1, 32)] : new T[32];

                if (buffer != null && size > 0)
                {
                buffer.CopyTo(newList, 0);
                }

                buffer = newList;
        }

        public void Push(T item)
        {
                if (buffer == null || size == buffer.Length) Alloc();
                buffer[size++] = item;
        }

        public T Pop()
        {
                if (buffer != null && size != 0)
                {
                return buffer[--size];                        
                }

                return default(T);
        }
        }

        public class BetterListPool<T>    
        {
        public const int MAX_COUNT = 16;
        PoolStack<T[]>[] pool = new PoolStack<T[]>[MAX_COUNT];
        //public static bool beLog = false;

        public BetterListPool()
        {
                for (int i = 0; i < MAX_COUNT; i++)
                {
                pool[i] = new PoolStack<T[]>();
                }
        }

        public void PreAlloc(int size, int count)
        {
                int n = GetSlot(size);

                for (int i = 0; i < count; i++)
                {
                pool[n].Push(new T[size]);
                }
        }

        int NextPowerOfTwo(int v)
        {
                v -= 1;
                v |= v >> 16;
                v |= v >> 8;
                v |= v >> 4;
                v |= v >> 2;
                v |= v >> 1;
                return v + 1;
        }

        public T[] Alloc(int n)
        {
                n = NextPowerOfTwo(n);
                int pos = GetSlot(n);

                if (pos >= 0 && pos < MAX_COUNT)
                {
                PoolStack<T[]> list = pool[pos];
                int count = list.size;

                if (count > 0)
                {
                        return list.Pop();
                }
                }

                //if (beLog)
                //{
                //    Debugger.LogWarning("Alloc type: {0}, size: {1}", typeof(T), n);
                //}        

                return new T[n];
        }

        public void Collect(T[] buffer)
        {
                int count = buffer.Length;        
                int pos = GetSlot(count);

                if (pos >= 0 && pos < MAX_COUNT)
                {
                PoolStack<T[]> list = pool[pos];
                list.Push(buffer);
                }
        }

        public int GetSlot(int value)
        {
                int len = 0;

                while (value > 0)
                {
                ++len;
                value >>= 1;
                }

                return len;
        }

        //public void PrintCache()
        //{
        //    if (beLog)
        //    {
        //        for (int i = 0; i < MAX_COUNT; i++)
        //        {
        //            if (pool[i] != null && pool[i].size != 0)
        //            {
        //                Debugger.Log("Collect type: {0}, size: {1}, total: {2}", typeof(T), 2 << i, pool[i].size);
        //            }
        //        }
        //    }
        //}
        }

```
一般为了减少临时分配，会经常将临时生成的对象缓存起来供下一次用， 但是如果每个类都单独缓存一份，也会造成内存上的浪费，而对象池则将 整个项目的缓存全部都共用起来，会节省很大一部分内存。注意:每次用 完都需要手动调用Release。

* 14. BaseMeshEffect,UGUI 中修改控制 Mesh 的时候有个特殊的对象池 ListPools.cs

```
        static class ListPool<T>
        {
                // Object pool to avoid allocations.
                private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, Clear);
                static void Clear(List<T> l) { l.Clear(); }

                public static List<T> Get()
                {
                return s_ListPool.Get();
                }

                public static void Release(List<T> toRelease)
                {
                s_ListPool.Release(toRelease);
                }
        }
```
当扩展BaseMeshEffect实现UI特效，经常会用到这个ListPool，如:
```
        public override void ModifyMesh(VertexHelper vh) 
        {
                if (!IsActive())
                return;
                var verts = ListPool<UIVertex>.Get(); vh.GetUIVertexStream(verts);
                ...
                vh.Clear(); vh.AddUIVertexTriangleStream(verts); ListPool<UIVertex>.Release(verts);
        }
```

需要注意一点的是，UIVertex结构体的大小是76字节，GetUIVertexStream是按三角形顶点来输出的，如果对Text类型使用这种 方法修改Mesh，字数很多的时候，会把缓存的List撑到很大，比如内置的 Outline，这样不止会撑大堆内存，在像Image这种一般只有两个三角形六个顶点的对象，使用较大List缓存也会有不必要的Clear开销。如果自定 义的BashMeshEffect想要修改Text，有两种方法:设置两个ListPool，图片和文字分开。
或者使用以下方法代替:
```
        vh.PopulateUIVertex(ref vert, index); 
        vh.SetUIVertex(vert, index); 
        vh.AddVert(vert);
```
用这几种方法可以使用UGUI内部的ListPool(VertexHelper维护的是 List<int>, List<Vertex3>等一系列缓存)，可以减少代码中使用自定义 ListPool带来的开销。


* 15. LinkedList
LinkedList是一串LinkedListNode对象的组合，如果频繁的添 加、删除，会生成很多无用的LinkedListNode对象，下面是用对象池缓 存LinkedListNode的一个实现:
```
        public class PooledLinkedList<T> : LinkedList<T> 
        {
                static Stack<LinkedListNode<T>> s_pool = new Stack<LinkedListNode<T>>(10);
                public PooledLinkedList()
                {
                }
                private LinkedListNode<T> Create(T t) 
                {
                        LinkedListNode<T> node = null; 
                        if (s_pool.Count > 0)
                        {
                                node = s_pool.Pop(); node.Value = t;
                        }
                        else
                        {
                                node = new LinkedListNode<T>(t);
                        }
                        return node;
                }
                public new void AddLast(T t)
                {
                        base.AddLast(Create(t));
                }
                public new void AddFirst(T t)
                {
                        base.AddFirst(Create(t));
                }
                public new void AddAfter(LinkedListNode<T> n, T t)
                {
                        base.AddAfter(n, Create(t));
                }
                public new void AddBefore(LinkedListNode<T> n, T t) 
                {
                        base.AddBefore(n, Create(t));
                }
                public new void Remove(LinkedListNode<T> n)
                {
                        int count = Count;
                        base.Remove(n);
                        if (count != Count)
                        {
                                s_pool.Push(n);
                        }
                }
                public new void RemoveFirst()
                {
                        if (Count <= 0)
                        return;
                        s_pool.Push(this.First);
                        base.RemoveFirst();
                }
                public new void RemoveLast()
                {
                        if (Count <= 0)
                        return;
                        s_pool.Push(this.Last);
                        base.RemoveLast();
                }
                public new void Clear()
                {
                        if (Count <= 0)
                        return;
                        for (var cur = First; cur != null; cur = cur.Next) {
                        s_pool.Push(cur);
                        }
                        base.Clear();
                }
        }


```
* 16. C#的string是不可变的，在通常情况下，一旦生成，就无法被改变(使用 Unsafe除外)，这样会导致两个问题:
        * 每一次拼接或修改字符串都会生成一个新字符串，一般旧的字符串 就会作为垃圾存在。
        * 内存中会出现多份内容一样的字符串资源，如两次用相同方法拼接 的字符串。这样会造成内存浪费。这里有一个例外，就 是“abc”+“def”的形式，会被编译器直接编译成“abcdef”， 当然基本上也没有人会这么写。在C#中有一个stringpool可以解决，就是一个内置的字 符串池，是一个哈希表的数据结构，一般代码中用直接写的字符串， 如“abcdef”，会存在于stringpool中，而拼接的字符串，可以使用 string.Intern(str); 把字符串放在stringpool中并指向在stringpool中的实 例。          

        **自定义string内存池**
        内置的内存池有两个缺点:
        * 无法清空，比如我在这次战斗中经常会生成的字符串，在下一场战斗中不会经常生成，如人名等，使用内置的stringpool会造成内存泄漏。
        * 每次调用string.Intern会将生成的字符串抛弃，如果频繁使用会产 生很多的垃圾。

        首先是要分清哪些字符串是需要在某一时刻清空的，哪些时候是可以常驻
内存的
        * 整数型:比如倒计时”12s“之类的。可以使用单独的一个池常驻内 存，防止每次都生成新的字符串，
        * 常用名称拼接，一般两到三个字符串拼在一起:如 “gun_” + “ak47” 这种形式，可以常驻内存，例子和上面的差不多。
        * xx击杀了yy，这种形式:只在当前战斗有效，如果想缓存，需要单 独一个pool，战斗结束后需要清空。

* 17. 匿名方法
```
        void Func()
        {
                int a = 1; 
                //每次调用时,都会生成一个新的对象,有很高的 GC Alloc
                Call(()=>a = 2);
        }



        int a = 1;
        void Func()
        {
                //调用了类的字段，同样也会在每次运行时生成一个新的对象。
                Call(()=> a = 2); 
        }


        void Func1() {
        ...
        }
        void Func() 
        { 
                //用上面方法做参数，也会生成新的对象。
                Call(Func1); 
        }


        //不使用任何外部变量，在Mono中不会产生GC Alloc。
        int Func(int a)
        {
                return Call(a, (_a)=> {_a = 2; return _a;}); 
        }
        //调用静态变量，在Mono中是不会产生GC Alloc的。
        static int a = 1; 
        void Func()
        {
                Call(()=> a = 2); 
        }
        //注意，在IL2CPP中，以上所有形式都会产生GC Alloc。
        //所以，在任何地方都要避免使用匿名方法，如果需要使用，可以缓存一下
再用,这样，无论是Mono，还是IL2CPP，都不会产生额外的GC Alloc。
        int a = 1;
        Action action;
        void Func()
        {
                if(action == null)
                {
                        action = () => a = 2; 
                }
                Call(action);
        }
```


* 19. 协程,IEnumerator Coroutine1(){}
        * IEnumerator方法会在编译时生成一个类
        * yield return new WaitForSeconds每次都会创建一个对象。这个可以生成一个全局静态类实现.缓存一下使用.

```
        IEnumerator Coroutine1() 
        {
                ...
        }
        WaitForSeconds seconds = new WaitForSeconds(5); 
        IEnumerator Coroutine()
        {
                yield return seconds;
                ...
                yield return Coroutine1(); 
        }
        //缓存是不能使用的，因为Coroutine1生成的类中没有 实现Reset方法，所以需要手动实现一个类:

        class Coroutine1 : IEnumerator 
        {
                public Coroutine1()
                {
                        ...
                }
                public bool MoveNext() {
                        ...
                }
                public void Reset()
                {
                        ...
                }
                public object Current
                {
                        get
                        {
                        ...
                        }
                }
        }
        //上面这个类，即使缓存了一个对象，也需要每次都手动调用Reset方法将 其重置，Coroutine方法变成下面这样:
        Coroutine1 coroutine1 = new Coroutine1(); 
        WaitForSeconds seconds = new WaitForSeconds(5);
        IEnumerator Coroutine()
        {
                while (true)
                {
                        yield return wait;
                        ...
                        c.Reset();
                        yield return coroutine1; 
                }
        }
        这样就不会产生额外的GC Alloc了。
```

* 20. Unity API
        * object.name和object.tag需要避免使用，但是object.CompareTag没有GC Alloc
        * 所有返回是数组的API都会有GC Alloc
        ```
                Text[] texts = GetComponentsInChildren<Text>();
        ```
        * 大部分API也都会提供一个可以传入List参数的方法，每次调用都会生成一个新的数组对象，可以配合ListPool对其进行优化:
        ```
                List<Text> texts = ListPool<Text>.Get(); GetComponentsInChildren(texts);
                ...
                ListPool<Text>.Release(texts);
        ```
        *  var materials = renderer.sharedMaterials;mesh 等,每次调用也会生成新的Material[]数组
        ```
                List<Material> materials = ListPool<Material>.Get(); renderer.GetSharedMaterials(materials);
                ...
                ListPool<Material>.Release(materials);
        ```
        * 还有导航网格，即使缓存了NavMeshPath，navMeshPath.corners也是一个会生成一个数 组，使用GetCornersNonAlloc并传入一个足够大小的数组会解决这个问题。
        ```
                public static Vector3[] cachedPath{get;} = new Vector3[256];
                public static int pathCount{get;private set;}
                private static NavMeshPath navMeshPath;
                public static void CalculatePath(Vector3 startPos, Vector3 endPos) 
                {
                        navMeshPath.ClearCorners();
                        NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, navMeshPath); pathCount = navMeshPath.GetCornersNonAlloc(cachedPath);
                        ...
                }
        ```
        * Physics.RayCastAll也有相应的Physics.RayCastNonAlloc方法。

        * UGUI中,当 Prefab中有大量空的Text，初始化的时候会有一个很严重的GC Alloc，这是因为在初始化 时，会先初始化TextGenerator，如果Text为空，则会先按50个字来初始化，即50个字的 UIVertex和50个字的UICharInfo，这种可以不让它为空，或者填一个空格进去来阻止。

* 21. Protobuf3,它利用变长整数和默认值的方法压缩消息， 提高了消息传输的效率，又用生成代码提高序列化/反序列化速度。Protobuf的所有扩展方法都不建议滥用。
首先，当反序列化字节流的时候，一般不要使用下面的代码:
```
                MemoryStream stream;
                ... 
                message.MergeFrom(stream);
```
传入参数如果是stream，会新建一个CodedInputStream，并新建一个byte[]数组，相当于 把字节流又复制了一份，如果改成这样:
```
                byte[] cachedBytes;
                ... 
                message.MergeFrom(cachedBytes);
```
这样会省掉复制字节流的开销。 也可以用对象池缓存CodedInputStream，类似如下代码:
```
public class NetMsgInStream
{
public CodedInputStream codedStream { get; private set; }

public MemoryStream memoryStream { get; } = new MemoryStream(); public NetMsgInStream()
{
        this.codedStream = new CodedInputStream(memoryStream);
}

public static ObjectPool<NetMsgInStream> pool { get; }
= new ObjectPool<NetMsgInStream>(actionOnRelease: (s) => s.memoryStream.SetLength(0));
}

var stream = NetMsgInStream.pool.Get(); 
message.MergeFrom(stream.codedStream); 
NetMsgInStream.pool.Release(stream);

```

**消息类的缓存:**

频繁生成的消息类，如战斗时的移动同步消息，如果每次接受到消息都新建一个对象，一场战斗下来会有很大的GC Alloc。需要做对象池来缓存.               
Protobuf为了节省传输压力，对默认值进行了优化， 所以，并不是所有字段都会在反序列化的时候赋值，这时如果服务器生成的消息某个字段 是默认值，就不会放入字节流里传给前端。
如果消息在放回缓冲池时没有把所有字段恢复到默认值，下一次反序列化重用到这个对象 时，字段有可能还是上一次的值。所以，在使用protoc生成消息类时，最好也为消息生成一 个Clear方法，清空所有字段。

* 22. Unsafe
Unsafe适用于对于性能有较高要求或者需要做某些特殊操作的时候，它把类似 C++的指针操作暴露出来，让开发时具有很大的灵活性。Unity 2018以后又提 供了UnsafeUtility工具类，让指针操作更加便捷。
比较简单的也比较常用的是ToLower:
```
public static void ToLower(string str) 
{
        fixed (char* c = str)
        {
                int length = str.Length;
                for (int i = 0; i < length; ++i) 
                {
                        c[i] = char.ToLower(c[i]); 
                }
        }
}
```
还有一个比较常用的操作是split，通过分隔符生成一个字符串数组，虽然数组中每个字符串 要缓存起来比较麻烦，但是数组本身是可以缓存出来的:
```
        public static int Split(string str, char split, string[] toFill) 
        {
                if (str.Length == 0)
                {
                        toFill[0] = string.Empty; return 1;
                }
                var length = str.Length; int ret = 0;
                fixed (char* p = str)
                {
                        var start = 0;
                        for (int i = 0; i < length; ++i) 
                        {
                                if (p[i] == split)
                                {
                                        toFill[ret++] = (new string(p, start, i - start));
                                        start = i + 1;
                                        if (i == length - 1)
                                                toFill[ret++] = string.Empty;
                                }
                        }
                        if (start < length)
                        {
                                toFill[ret++] = (new string(p, start, length - start)); 
                        }
                }
                return ret;
        }
```
方法传入了一个缓存的string[]，然后根据split遍历分割字符串。但如果是根据split遍历操作 每一个字符串，也可以缓存一个单独的长度比较大的字符串，每次分割后的字符都复制进 这个缓存里，当然，这就需要动态修改字符串的长度了，字符串的长度修改方法如下:

```
        public static void SetLength(this string str, int length) 
        {
                fixed (char* s = str)
                {
                        int* ptr = (int*)s; 
                        ptr[-1] = length; 
                        s[length] = '\0'; 
                }
        }
```
字符串最后一个字符后面的字符必须是‘\0’，这个和C++是一样的，字符串的首字母地址之 前的一个int代表了字符串的长度。
可以设置字符串长度以后，也就可以实现Substring了:
```
                public static void Substring(string str, int start, int length = 0) {
                if (length <= 0)
                {
                length = str.Length - start;
                }
                if (length > str.Length - start)
                {
                throw new IndexOutOfRangeException($"{length} > {str.Length} - {start}"); }
                fixed (char* c = str)
                {
                UnsafeUtility.MemMove(c, c + start, sizeof(char) * length);
                }
                SetLength(str, length);
                }
```
使用Unsafe操作字符串可以不必生成新的字符串，从而减少GC Alloc，不过需要注意几 点:
指针操作没有越界检查，如果修改字符串的长度，要确保长度小于等于字符串的原 始长度。
谨慎修改intern字符串的内容。 修改字符串内容会使字符串的hashcode发生改变，如果修改的字符串是某个字典的 Key，需要将其从字典中移除，修改后再放进去。

* 23.  非托管堆
相对于托管堆，非托管堆有一个好处，就是可以手动申请和释放，此外，Unity的DOTS大 量使用Native容器也是为了能保证尽量使用连续内存。UnsafeUtility提供了方便的接口手动 管理非托管内存，下面是一个使用非托管堆的UnsafeList示例。
可以使用非托管堆的类型必须是Blittable，也就是必须是结构体，而且里面的字段只包含基 本值类型和Blittable结构体。所以，声明可以写成:
```
        public unsafe struct UnsafeList<T> where T : unmanaged 
        {
                static int alignment = UnsafeUtility.AlignOf<T>();
                static int elementSize = UnsafeUtility.SizeOf<T>();
                const int MIN_SIZE = 4; ArrayInfo* array;
                ...
        }
```
unmanaged约束可以看作是Blittable，但是有个问题就是不包含泛型结构体，如果不需要 泛型结构体可以忽略，或者使用struct约束，但是struct约束就不能用指针T * 来存取数据 了，需要换一种方式。这里还是使用unmanaged约束。
几个静态变量缓存了申请内存所需要的信息，数据信息存在ArrayInfo的指针中:
```
        unsafe struct ArrayInfo 
        {
                public int count;
                public int capacity; 
                public void* ptr;
        }
```
信息包含长度、容量，真正的数据保存在ptr中。首先是构造函数:
```
        public UnsafeList(int capacity)
        {
                capacity = Mathf.Max(MIN_SIZE, capacity);
                array = (ArrayInfo*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<ArrayInfo>(), UnsafeUtility.AlignOf<ArrayInfo>(), Allocator.Persistent);
                array->capacity = capacity;
                array->count = 0;
                array->ptr = UnsafeUtility.Malloc(elementSize * capacity, alignment, Allocator.Persistent);
        }
```
UnsafeUtility提供了多种Allocator，生命周期和性能都不相同，具体可以参见官方文档。 然 后如果不使用这个List，需要手动将其释放:
```
public void Dispose()
{
        UnsafeUtility.Free(array->ptr, Unity.Collections.Allocator.Persistent); 
        UnsafeUtility.Free(array, Unity.Collections.Allocator.Persistent);
}
```
当容量不够时，可以像List一样扩容:
```
        void EnsureCapacity(int newCapacity) 
        {
                if (newCapacity > array->capacity)
                {
                        newCapacity = Mathf.Max(newCapacity, array->count * 2);
                        var newPtr = UnsafeUtility.Malloc(elementSize * newCapacity, alignment, Allocator.Persistent);
                        UnsafeUtility.MemCpy(newPtr, array->ptr, elementSize * array->count); UnsafeUtility.Free(array->ptr, Allocator.Persistent);
                        array->ptr = newPtr;
                        array->capacity = newCapacity;
                }
        }
```
申请新内存，将旧的数据复制到新的内存中，再释放旧内存。有了这个就可以添加数据
了:
```
        public void Add(T t)
        {
                EnsureCapacity(array->count + 1); *((T*)array->ptr + array->count) = t; ++array->count;
                }
                public void Insert(int index, T t)
                {
                EnsureCapacity(array->count + 1);
                if (0 <= index && index <= array->count)
                {
                UnsafeUtility.MemMove((T*)array->ptr + index + 1, (T*)array->ptr + index, (array->count - index) * elementSize);
                *((T*)array->ptr + index) = t;
                ++array->count;
                }
                else
                {
                throw new IndexOutOfRangeException();
                }
        }
```
如果复制的内存区域重叠，不管是向前还是向后，最好都使用memmove，内部会决定要不 要考虑重叠区域。AddRange和Remove也是类似的实现方法。
在List中，Clear方法因为要考虑元素是引用类型的情况，为了能让GC正常回收List中的对 象，必须把所有数据全都归零，但是这里因为不存在这种情况，所以Clear方法很简单:
```
public void Clear() 
{
        array->length = 0; 
}
```
最后是读写，因为索引器的set方法不支持ref参数，所以可以直接用指针:
```
        public T* this[int index] 
        {
                get
                {
                        if (0 <= index && index < array->count) {
                        return ((T*)array->ptr + index);
                        }
                        throw new IndexOutOfRangeException(); }
                        set
                        {
                        if (0 <= index && index < array->count) {
                        *((T*)array->ptr + index) = *value;
                        }
                        throw new IndexOutOfRangeException(); 
                }
        }
```
然后也可以提供一个单独的ref return方法:
```
        public ref T Get(int index) 
        {
                return ref *this[index];
        }
```

* 24. stackalloc、Span<T>和Memory<T>

```
        //申请栈内存
        void Calculate()
        {
                Vector3* s = stackalloc Vector3[10]; 
                ...
        }
```
如果计算只需要一组占用比较小的临时数据，使用stackalloc是一个很好的选择，因为它的 申请速度非常快，而且不需要手动管理，作用域一结束就会自动释放。
Span<T>和Memory<T>这两个类型需要额外的DLL支持。它们可以管理存放在托管堆，非 托管堆和栈内存的数据，因为提供了Slice方法分割内存，还提供了各种Copy方法可以在各 种类型内存中互相拷贝，比直接用指针来方便一些，Span<T>和Memory<T>的区别是， Span<T>是ref类型的，不能用作字段，也不能跨越yield和await使用。一般Span<T>和 Memory<T>的执行效率比直接使用指针要低。有兴趣可以看一下 https://github.com/KumoKyaku/KCP






