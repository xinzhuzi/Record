/*
Copyright (c) 2015-2017 topameng(topameng@qq.com)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Security;

namespace LuaInterface
{
    public enum LuaTypes
    {
        LUA_TNONE = -1, //none
        LUA_TNIL = 0,  // nil
        LUA_TBOOLEAN = 1, //bool
        LUA_TLIGHTUSERDATA = 2,//lightuserdata C 管理内存
        LUA_TNUMBER = 3, //number double
        LUA_TSTRING = 4, //string
        LUA_TTABLE = 5, //table
        LUA_TFUNCTION = 6,//function
        LUA_TUSERDATA = 7,//userdata
        LUA_TTHREAD = 8,//thread

    }

    //统计Lua使用的内存量
    //double MemCount = (double)lua_gc(L, LUA_GCCOUNTB, 0) + (double)(lua_gc(L, LUA_GCCOUNT, 0) * 1024);
    public enum LuaGCOptions
    {
        LUA_GCSTOP = 0,     //Lua  停止 GC
        LUA_GCRESTART = 1,   //Lua 重启 GC
        LUA_GCCOLLECT = 2,  //Lua 执行 GC  整体的垃圾收集,一直到全部回收
        LUA_GCCOUNT = 3,   // 内存使用量
        LUA_GCCOUNTB = 4,  // 内存使用量
        LUA_GCSTEP = 5,    //进行单次的垃圾收集
        LUA_GCSETPAUSE = 6,//暂停 GC
        LUA_GCSETSTEPMUL = 7,//gcstepmul就是相对于回收速率/分配速率的一个比值
    }
    
    //线程状态
    public enum LuaThreadStatus
    {
        LUA_YIELD = 1,
        LUA_ERRRUN = 2,
        LUA_ERRSYNTAX = 3,
        LUA_ERRMEM = 4,
        LUA_ERRERR = 5,
    }
    //堆栈钩子,为了 debug 信息使用
    public enum LuaHookFlag
    {
        LUA_HOOKCALL = 0,
        LUA_HOOKRET	 = 1,
        LUA_HOOKLINE = 2,
        LUA_HOOKCOUNT = 3,
        LUA_HOOKTAILRET = 4,
    }
    //堆栈钩子,为了 debug 信息使用
    public enum LuaMask
    {
        LUA_MASKCALL = 1, //1 << LUA_HOOKCALL   //在函数被调用时触发
        LUA_MASKRET	= 2, //(1 << LUA_HOOKRET)    //在函数返回时被触发;
        LUA_MASKLINE = 4,//	(1 << LUA_HOOKLINE)    //在每执行一行代码时被触发
        LUA_MASKCOUNT = 8, //	(1 << LUA_HOOKCOUNT)//每执行 count 条 lua 指令触发一次,这里的 count 在 lua_sethook 函数的第三个参数中传入,使用其他 hook 类型时,其他参数无效.
    }


    public class LuaIndexes
    {
        public static int LUA_REGISTRYINDEX = -10000;// 注册表 C 代码使用
        public static int LUA_ENVIRONINDEX  = -10001;// env 表 Lua 代码使用
        public static int LUA_GLOBALSINDEX  = -10002;// _G 表 Lua 代码使用
    }

    public class LuaRIDX
    {
        public int LUA_RIDX_MAINTHREAD = 1;//主线程/主 lua_State
        public int LUA_RIDX_GLOBALS = 2;//全局
        public int LUA_RIDX_PRELOAD = 25;//预加载
        public int LUA_RIDX_LOADED = 26;//加载
    }

    public static class ToLuaFlags
    {
        public const int INDEX_ERROR = 1;       //Index 失败提示error信息，false返回nil
        public const int USE_INT64 = 2;         //是否luavm内部支持原生int64(目前用的vm都不支持, 默认false)
    }
    
    //在函数 lua_getinfo 中,在调试过程中,需要知道当前虚拟机的一些状态,为此,Lua 提供了 lua_Debug 结构体,里面的成员变量用来保存当前程序的一些信息
    [StructLayout(LayoutKind.Sequential)]
    public struct Lua_Debug
    {        
        public int eventcode;        ////用于表示触发hook 的事件,事件类型就是LUA_MASKCALL,LUA_MASKRET,LUA_MASKLINE,LUA_MASKCOUNT
        public IntPtr _name;	            /* (n) 当前所在函数的名称*/                
        public IntPtr _namewhat;	        /* (n) `global', `local', `field', `method' name 域的含义,可能取值为 global,local,method,field 或者空字符串,空字符串意味着 Lua 无法找到这个函数名字  */        
        public IntPtr _what;	            /* (S) `Lua', `C', `main', `tail' 函数类型,如果是普通的函数,就是 Lua,如果是 C 函数就是 C,如果是 Lua的主代码段,结果为 main */        
        public IntPtr _source;	            /* (S) 函数定义位置,如果函数在字符串内被定义(通过 loadstring 函数),source 就是该字符串,如果函数在文件中被定义,source 就是带@前缀的文件名 */        
        public int currentline;	            /* (l) 当前所在行号 */        
        public int nups;		            /* (u) number of upvalues 该函数 upvalue 的数量 */        
        public int linedefined;     	    /* (S) source 中函数被定义处的行号*/        
        public int lastlinedefined; 	    /* (S) 该函数最后一行代码在源代码中的行号*/        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] _short_src;                //source 的简短版本(60个字符以内),对错误信息很有用
        public int i_ci;                    /* active function 存放当前函数在 lua_state 结构体的 callinfo 数组中的索引,通过这个变量,就能在 lua_state 结构体中拿到对应的 callinfo 数据 */
        
        //下面的方法传入上面的参数,调用 lua dll 中的方法之后返回的数据,也就是 C# 调用 C,又在 C# 层做的封装
        string tostring(IntPtr p)
        {
            if (p != IntPtr.Zero)
            {
                int len = LuaDLL.tolua_strlen(p);
                return LuaDLL.lua_ptrtostring(p, len);
            }

            return string.Empty;
        }

        public string namewhat
        {
            get
            {
                return tostring(_namewhat);
            }
        }

        public string name
        {
            get
            {
                return tostring(_name);
            }
        }

        public string what
        {
            get
            {
                return tostring(_what);
            }
        }

        public string source
        {
            get
            {
                return tostring(_source);
            }
        }        

        int GetShortSrcLen(byte[] str)
        {
            int i = 0;

            for (; i < 128; i++)
            {
                if (str[i] == '\0')
                {
                    return i;
                }
            }

            return i;
        }

        public string short_src
        {
            get
            {
                if (_short_src == null)
                {
                    return string.Empty;
                }

                int count = GetShortSrcLen(_short_src);
                return Encoding.UTF8.GetString(_short_src, 0, count);
            }
        }        
    }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA_10_0
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int LuaCSFunction(IntPtr luaState);        
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LuaHookFunc(IntPtr L, ref Lua_Debug ar);
#else
    public delegate int LuaCSFunction(IntPtr luaState);    
    public delegate void LuaHookFunc(IntPtr L, ref Lua_Debug ar);    
#endif

    public class LuaDLL
    {
        public static string version = "1.0.7.386";
        public static int LUA_MULTRET = -1; //在 lua_State 调用这个栈位置地方的方法
        public static string[] LuaTypeName = { "none", "nil", "boolean", "lightuserdata", "number", "string", "table", "function", "userdata", "thread" };        

#if !UNITY_EDITOR && UNITY_IPHONE
        const string LUADLL = "__Internal";
#else
        const string LUADLL = "tolua";//tolua 的 外放 C 的 API
#endif
        /*
        ** third party library
        */
        //注册 protobuf 在 Lua 中使用的方法 pb.pack,pb.unpack,pb.__gc,pb.setdefault,
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_pb(IntPtr L);
        
        //使用 FFI 加载链接库,打开链接库,方便不同语言的方法转换调用
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_ffi(IntPtr L);
        
        //打开 Lua 位运算的库,位运算的方法有  tobit,bnot,band,bor,bxor,lshift,rshift,arshift,rol,bswap,tohex
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_bit(IntPtr L);
        
        //打开一个结构体类型,结构体库的方法有,pack,unpack,size
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_struct(IntPtr L);

        //lpeg库,LPEG是一个供lua使用的基于 Parsing Expression Grammars 的模式匹配库
        //LPEG 的函数主要分为三类，第一类是创建Pattern的构造函数，第二类是 Capture 函数， 第三类则是 match 等函数。 Capture 就是指一个Pattern，当前匹配时会产生某些捕获的值。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_lpeg(IntPtr L);             
        
        //LuaSocket 是 Lua 的网络模块库，它可以很方便地提供 TCP、UDP、DNS、FTP、HTTP、SMTP、MIME 等多种网络协议的访问操作。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_socket_core(IntPtr L);
        
        //LuaSocket 的工具包,对传输的字节做的优化
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_mime_core(IntPtr L);
        
        //cjson 一个优秀的解析 json 的库
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_cjson(IntPtr L);
        
        //luaopen_cjson_safe 安全
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_cjson_safe(IntPtr L);

        /*
         ** pseudo-indices 伪指数
         */
        public static int lua_upvalueindex(int i)
        {
            return LuaIndexes.LUA_GLOBALSINDEX - i;
        }

        /*
         * state manipulation  luajit64位不能用这个函数
         */
        //[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr lua_newstate(LuaAlloc f, IntPtr ud);                      //luajit64位不能用这个函数

        //关闭 lua_state,关闭当前打开的 lua 虚拟栈,只有在游戏退出时才使用一次,一般一个游戏也就加载一次 lua_State,服务器可能加载多次(skynet)
        //销毁所有在指定的Lua State上的所有对象，同时释放所有该State使用的动态分配的空间。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_close(IntPtr luaState);
        
        //主要作用是当前第一次创建的 lua_State 因为其他原因堆栈溢出(例如方法递归调用,这个栈溢出了),要重新创建一个 lua_State,它本身与线程没有任何关系
        //一般是在 C 里面去调用,一般的用法是
        //s = lua_newthread(source);//创建一个新的lua_State对象
        //lua_setglobal(source, "___safe_thread_vm_"); -- 将调用栈记录到全局变量
        //lua_gc(source, LUA_GCCOLLECT); -- 垃圾回收，上一次的调用栈，会在此时被完全回收。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]                        //[-0, +1, m]  
        public static extern IntPtr lua_newthread(IntPtr L);
        
        //当调用无保护的 lua_call(不是 lua_pcall,lua_xpcall),如果调用栈发生错误(lua_error),那么默认行为是直接退出宿主程序
        //要避免这样的情况,一种方法是定义自己的 panic 函数,并作为参数调用 lua_atpanic,此外为了避免退出宿主程序,自定义的 panic 函数应该用不返回
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_atpanic(IntPtr luaState, IntPtr panic);

        //返回 lua_State 栈中的元素个数,同时也是栈顶元素的索引，因为栈底是1，所以栈中有多少个元素，栈顶索引就是多少
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gettop(IntPtr luaState);
        
        //用于把堆栈的栈顶索引设置为指定的数值;一个栈原来有8个元素，调用函数设置index为7，就是把堆栈的元素数设置为7，也就是删掉一个元素，而且是栈顶元素;
        //这个是用的正数，也就是相对于栈底元素设置的,栈底一般为正值一般为 1;如果是相对于栈顶元素，一般用负值为-1
        //例子，栈的初始状态为10 20 30 40 50 *（从栈底到栈顶，“*”标识为栈顶，有：
        // lua_settop(L, -3)      --> 10 20 30 *
        // lua_settop(L,  6)      --> 10 20 30 nil nil nil *
        //需要与lua_remove对比着看
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_settop(IntPtr luaState, int top);
        
        //将栈中的某个值取出,并将其压入栈顶;例子:
        //栈的初始状态为10 20 30 40 50 *（从栈底到栈顶，“*”标识为栈顶）
        //有：    lua_pushvalue(L, 3)    --> 10 20 30 40 50 30*;
        //lua_pushvalue(L,3)是取得原来栈中的第三个元素，压到栈顶;
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushvalue(IntPtr luaState, int idx);
        
        //lua_remove删除给定索引的元素，并将这一索引之上的元素来填补空缺;例子:
        //栈的初始状态为10 20 30 40 50 *（从栈底到栈顶，“*”标识为栈顶，有：
        //lua_remove(L, -3)      --> 10 20 40 50*
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_remove(IntPtr luaState, int idx);
        
        //插入,将栈顶的元素移动到 idx 处 ,在上移给定索引之上的所有元素后再在指定位置插入新元素;例子:
        //lua_insert(L, -1)      --> 30 10 20 30 40*  (没影响)
        //lua_insert(L, -2)      --> 30 10 20 40 30*
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_insert(IntPtr luaState, int idx);
        
        
        //Lua_replace将栈顶元素压入指定位置而不移动任何元素,因此指定位置的元素的值被替换
        //例子，栈的初始状态为10 20 30 40 50 *（从栈底到栈顶，“*”标识为栈顶，有：
        //lua_replace(L, 2)      --> 10 50 30 40 *    //把50替换到索引的位置，同时去掉栈顶元素
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_replace(IntPtr luaState, int index);
        
        //确保堆栈中至少有额外的可用堆栈插槽。 如果不能将堆栈增加到该大小，则返回false。 这个函数永远不会缩小堆栈； 如果堆栈已经大于新的大小， 它保持不变。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_checkstack(IntPtr luaState, int extra);
        
        //这个作用一般是将新的协同程序中的值移动到其他地方使用
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_xmove(IntPtr from, IntPtr to, int n);
        
        //判断lua_State栈中的某个 idx 是否是一个 number 的值,类似于查看功能
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_isnumber(IntPtr luaState, int idx);
        
        //判断lua_State栈中的某个 idx 是否是一个 string 的值,类似于查看功能
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_isstring(IntPtr luaState, int index);
        
        //判断lua_State栈中的某个 idx 是否是一个 c function 的值,类似于查看功能
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_iscfunction(IntPtr luaState, int index);
        
        //判断lua_State栈中的某个 idx 是否是一个 userdata 的值,类似于查看功能
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_isuserdata(IntPtr luaState, int stackPos);
        
        //判断lua_State栈中的某个 idx 是否是一个 type 的值,类似于查看功能
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaTypes lua_type(IntPtr luaState, int index);

        public static string lua_typename(IntPtr luaState, LuaTypes type)
        {
            int t = (int)type;
            return LuaTypeName[t + 1];
        }
        
        //在lua_State栈中,如果依照 Lua 中 == 操作符语义，索引 index1 和 index2 中的值相同的话，返回 1 。否则返回 0 。如果任何一个索引无效也会返回 0
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_equal(IntPtr luaState, int idx1, int idx2);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        
        //如果两个索引 index1 和 index2 处的值简单地相等（不调用元方法）则返回 1 。否则返回 0 。如果任何一个索引无效也返回 0 。
        public static extern int lua_rawequal(IntPtr luaState, int idx1, int idx2);
        
        //如果索引 index1 处的值小于索引 index2 处的值时，返回 1 ；否则返回 0 。其语义遵循 Lua 中的< 操作符（就是说，有可能调用元方法）。如果任何一个索引无效，也会返回 0 。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_lessthan(IntPtr luaState, int idx1, int idx2);
        
        //把给定索引处的 Lua 值转换为 lua_Number 这样一个 C 类型（参见 lua_Number ）。这个 Lua 值必须是一个数字或是一个可转换为数字的字符串（参见 §2.2.1 ）；否则，lua_tonumber 返回 0 。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern double lua_tonumber(IntPtr luaState, int idx);
        
        //把给定索引处的 Lua 值转换为 lua_Integer 这样一个有符号整数类型。这个 Lua 值必须是一个数字或是一个可以转换为数字的字符串（参见 §2.2.1）；否则，lua_tointeger 返回 0 。
        public static int lua_tointeger(IntPtr luaState, int idx)
        {
            return tolua_tointeger(luaState, idx);
        }
        
        //把指定的索引处的的 Lua 值转换为一个 C 中的 boolean 值（ 0 或是 1 ）。和 Lua 中做的所有测试一样，lua_toboolean 会把任何不同于false 和nil 的值当作 1 返回；否则就返回 0 。如果用一个无效索引去调用也会返回 0 。（如果你想只接收真正的 boolean 值，就需要使用lua_isboolean 来测试值的类型。）
        public static bool lua_toboolean(IntPtr luaState, int idx)
        {
            return tolua_toboolean(luaState, idx);            
        }
        
        /*
         * 把给定索引处的 Lua 值转换为一个 C 字符串。如果 len 不为 NULL ，它还把字符串长度设到*len 中。这个 Lua 值必须是一个字符串或是一个数字；否则返回返回NULL 。
         * 如果值是一个数字，lua_tolstring 还会把堆栈中的那个值的实际类型转换为一个字符串。（当遍历一个表的时候，把lua_tolstring 作用在键上，这个转换有可能导致lua_next 弄错。）
         * lua_tolstring 返回 Lua 状态机中字符串的以对齐指针。这个字符串总能保证 （ C 要求的）最后一个字符为零 ('\0') ，而且它允许在字符串内包含多个这样的零。
         * 因为 Lua 中可能发生垃圾收集，所以不保证lua_tolstring 返回的指针，在对应的值从堆栈中移除后依然有效。
         */
        public static IntPtr lua_tolstring(IntPtr luaState, int index, out int strLen)               //[-0, +0, m]
        {            
            return tolua_tolstring(luaState, index, out strLen);
        }
        
        //lua 表,恒返回 0,在lua_State,返回 obj 的个数
        public static int lua_objlen(IntPtr luaState, int idx)
        {
            return tolua_objlen(luaState, idx);
        }
        
        //把给定索引处的 Lua 值转换为一个 C 函数。这个值必须是一个 C 函数；如果不是就返回 NULL 。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_tocfunction(IntPtr luaState, int idx);
        
        //如果给定索引处的值是一个完整的 userdata ，函数返回内存块的地址。如果值是一个 light userdata ，那么就返回它表示的指针。否则，返回NULL 。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_touserdata(IntPtr luaState, int idx);
        
        //把给定索引处的值转换为一个 Lua 线程（由 lua_State* 代表）。这个值必须是一个线程；否则函数返回NULL 。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_tothread(IntPtr L, int idx);
        
        //把给定索引处的值转换为一般的 C 指针 (void*) 。这个值可以是一个 userdata ，table ，thread 或是一个 function ；否则，lua_topointer 返回NULL 。不同的对象有不同的指针。不存在把指针再转回原有类型的方法。
        //这个函数通常只为产生 debug 信息用。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_topointer(IntPtr L, int idx);

        //把一个 nil 压栈。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushnil(IntPtr luaState);
        
        //把一个数字 n 压栈。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushnumber(IntPtr luaState, double number);
        
        //
        public static void lua_pushinteger(IntPtr L, int n)
        {
            lua_pushnumber(L, n);
        }                
        
        //把指针 s 指向的长度为 len 的字符串压栈。 Lua 对这个字符串做一次内存拷贝（或是复用一个拷贝），因此s 处的内存在函数返回后，可以释放掉或是重用于其它用途。字符串内可以保存有零字符。
        public static void lua_pushlstring(IntPtr luaState, byte[] str, int size)                   //[-0, +1, m]
        {
            if (size >= 0x7fffff00)
            {
                throw new LuaException("string length overflow");
            }

            tolua_pushlstring(luaState, str, size);
        }
        
        //把指针 s 指向的以零结尾的字符串压栈。 Lua 对这个字符串做一次内存拷贝（或是复用一个拷贝），因此s 处的内存在函数返回后，可以释放掉或是重用于其它用途。字符串中不能包含有零字符；第一个碰到的零字符会认为是字符串的结束。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushstring(IntPtr luaState, string str);                      //[-0, +1, m]
        
        //把一个新的 C closure 压入堆栈。
        //当创建了一个 C 函数后，你可以给它关联一些值，这样就是在创建一个 C closure （参见 §3.4）；接下来无论函数何时被调用，这些值都可以被这个函数访问到。为了将一些值关联到一个 C 函数上，首先这些值需要先被压入堆栈（如果有多个值，第一个先压）。接下来调用lua_pushcclosure 来创建出 closure 并把这个 C 函数压到堆栈上。参数n 告之函数有多少个值需要关联到函数上。lua_pushcclosure 也会把这些值从栈上弹出。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushcclosure(IntPtr luaState, IntPtr fn, int n);              //[-n, +1, m]
        
        //把 b 作为一个 boolean 值压入堆栈。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushboolean(IntPtr luaState, int value);

        public static void lua_pushboolean(IntPtr luaState, bool value)
        {
            lua_pushboolean(luaState, value ? 1 : 0);
        }
        
        //把一个 light userdata 压栈。userdata 在 Lua 中表示一个 C 值。 light userdata 表示一个指针。 它是一个像数字一样的值： 你不需要专门创建它，它也没有独立的 metatable ， 而且也不会被收集（因为从来不需要创建）。 只要表示的 C 地址相同，两个 light userdata 就相等。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushlightuserdata(IntPtr luaState, IntPtr udata);
        
        //把 L 中提供的线程压栈。 如果这个线程是当前状态机的主线程的话，返回 1 。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_pushthread(IntPtr L);

        //把 t[k] 值压入堆栈， 这里的 t 是指有效索引 index 指向的值， 而 k 则是栈顶放的值。
        //这个函数会弹出堆栈上的 key （把结果放在栈上相同位置）。 在 Lua 中，这个函数可能触发对应 "index" 事件的元方法 （参见 §2.8）。
        public static void lua_gettable(IntPtr L, int idx)
        {
            if (LuaDLL.tolua_gettable(L, idx) != 0)
            {
                string error = LuaDLL.lua_tostring(L, -1);
                throw new LuaException(error);
            }
        }
        
        //把 t[k] 值压入堆栈， 这里的 t 是指有效索引 index 指向的值。 在 Lua 中，这个函数可能触发对应 "index" 事件的元方法 （参见 §2.8）。
        public static void lua_getfield(IntPtr L, int idx, string key)
        {
            if (LuaDLL.tolua_getfield(L, idx, key) != 0)
            {
                string error = LuaDLL.lua_tostring(L, -1);
                throw new LuaException(error);
            }
        }
        
        //类似于 lua_gettable，但是作一次直接访问（不触发元方法）。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawget(IntPtr luaState, int idx);
        
        //把 t[n] 的值压栈，这里的 t 是指给定索引 index 处的一个值。这是一个直接访问；就是说，它不会触发元方法。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawgeti(IntPtr luaState, int idx, int n);
        
        //创建一个新的空 table 压入堆栈。 这个新 table 将被预分配 narr 个元素的数组空间 以及 nrec 个元素的非数组空间。 当你明确知道表中需要多少个元素时，预分配就非常有用。 如果你不知道，可以使用函数 lua_newtable。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_createtable(IntPtr luaState, int narr, int nrec);             //[-0, +1, m]        
        
        //这个函数分配分配一块指定大小的内存块， 把内存块地址作为一个完整的 userdata 压入堆栈，并返回这个地址。
        //userdata 代表 Lua 中的 C 值。 完整的 userdata 代表一块内存。 它是一个对象（就像 table 那样的对象）： 你必须创建它，它有着自己的元表，而且它在被回收时，可以被监测到。 一个完整的 userdata 只和它自己相等（在等于的原生作用下）。
        //当 Lua 通过 gc 元方法回收一个完整的 userdata 时， Lua 调用这个元方法并把 userdata 标记为已终止。 等到这个 userdata 再次被收集的时候，Lua 会释放掉相关的内存。
        public static IntPtr lua_newuserdata(IntPtr luaState, int size)                             //[-0, +1, m]
        {
            return tolua_newuserdata(luaState, size);
        }
        
        //把给定索引指向的值的元表压入堆栈。 如果索引无效，或是这个值没有元表， 函数将返回 0 并且不会向栈上压任何东西。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_getmetatable(IntPtr luaState, int objIndex);
        
        //把索引处值的环境表压入堆栈。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_getfenv(IntPtr luaState, int idx);

        //作一个等价于 t[k] = v 的操作， 这里 t 是一个给定有效索引 index 处的值， v 指栈顶的值， 而 k 是栈顶之下的那个值。
        //这个函数会把键和值都从堆栈中弹出。 和在 Lua 中一样，这个函数可能触发 "newindex" 事件的元方法 （参见 §2.8）。
        public static void lua_settable(IntPtr L, int idx)
        {
            if (tolua_settable(L, idx) != 0)
            {
                string error = LuaDLL.lua_tostring(L, -1);
                throw new LuaException(error);
            }
        }
        
        //做一个等价于 t[k] = v 的操作， 这里 t 是给出的有效索引 index 处的值， 而 v 是栈顶的那个值。
        //这个函数将把这个值弹出堆栈。 跟在 Lua 中一样，这个函数可能触发一个 "newindex" 事件的元方法 （参见 §2.8）。
        public static void lua_setfield(IntPtr L, int idx, string key)
        {
            if (tolua_setfield(L, idx, key) != 0)
            {
                string error = LuaDLL.lua_tostring(L, -1);
                throw new LuaException(error);
            }
        }
        
        //类似于 lua_settable， 但是是作一个直接赋值（不触发元方法）。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawset(IntPtr luaState, int idx);                             //[-2, +0, m]
        
        //等价于 t[n] = v， 这里的 t 是指给定索引 index 处的一个值， 而 v 是栈顶的值。
        //函数将把这个值弹出栈。 赋值操作是直接的；就是说，不会触发元方法。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawseti(IntPtr luaState, int tableIndex, int index);          //[-1, +0, m]
        
        //把一个 table 弹出堆栈，并将其设为给定索引处的值的 metatable 。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_setmetatable(IntPtr luaState, int objIndex);
        
        //从堆栈上弹出一个 table 并把它设为指定索引处值的新环境。 如果指定索引处的值即不是函数又不是线程或是 userdata ， lua_setfenv 会返回 0 ， 否则返回 1 。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_setfenv(IntPtr luaState, int stackPos);

        //调用一个函数。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_call(IntPtr luaState, int nArgs, int nResults);               //[-(nargs+1), +nresults, e]       
        
        //以保护模式调用一个函数。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_pcall(IntPtr luaState, int nArgs, int nResults, int errfunc);
        
        //以保护模式调用 C 函数 func 。 func 只有能从堆栈上拿到一个参数，就是包含有 ud 的 light userdata。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_cpcall(IntPtr L, IntPtr func, IntPtr ud);
        
        //加载一个 Lua chunk 。 如果没有错误， lua_load 把一个编译好的 chunk 作为一个 Lua 函数压入堆栈。 否则，压入出错信息。
        //[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        //public static extern int lua_load(IntPtr luaState, LuaChunkReader chunkReader, ref ReaderInfo data, string chunkName);
        
        //把函数 dump 成二进制 chunk 。 函数接收栈顶的 Lua 函数做参数，然后生成它的二进制 chunk 。
        //若被 dump 出来的东西被再次加载，加载的结果就相当于原来的函数。 当它在产生 chunk 的时候，lua_dump 通过调用函数 writer （参见 lua_Writer） 来写入数据，后面的 data 参数会被传入 writer 。
        //[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        //public static extern int lua_dump(IntPtr L, LuaWriter writer, IntPtr data);

        //切出一个 coroutine 。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_yield(IntPtr L, int nresults);                                 //[-?, +?, e]       
        
        //在给定线程中启动或继续一个 coroutine 。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_resume(IntPtr L, int narg);
        
        //返回线程 L 的状态。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_status(IntPtr L);

       //控制垃圾收集器。
       // LUA_GCSTOP: 停止垃圾收集器。
       // LUA_GCRESTART: 重启垃圾收集器。
       // LUA_GCCOLLECT: 发起一次完整的垃圾收集循环。
       // LUA_GCCOUNT: 返回 Lua 使用的内存总量（以 K 字节为单位）。
       // LUA_GCCOUNTB: 返回当前内存使用量除以 1024 的余数。
       // LUA_GCSTEP: 发起一步增量垃圾收集。 步数由 data 控制（越大的值意味着越多步）， 而其具体含义（具体数字表示了多少）并未标准化。 如果你想控制这个步数，必须实验性的测试 data 的值。 如果这一步结束了一个垃圾收集周期，返回返回 1 。
       // LUA_GCSETPAUSE: 把 data/100 设置为 garbage-collector pause 的新值（参见 §2.10）。 函数返回以前的值。
       // LUA_GCSETSTEPMUL: 把 arg/100 设置成 step multiplier （参见 §2.10）。 函数返回以前的值。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gc(IntPtr luaState, LuaGCOptions what, int data);              //[-0, +0, e]
        
        //从栈上弹出一个 key（键）， 然后把索引指定的表中 key-value（健值）对压入堆栈 （指定 key 后面的下一 (next) 对）。 如果表中以无更多元素， 那么 lua_next 将返回 0 （什么也不压入堆栈）。
        //C#中的迭代器
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_next(IntPtr luaState, int index);                              //[-1, +(2|0), e]        
        
        //连接栈顶的 n 个值， 然后将这些值出栈，并把结果放在栈顶。 如果 n 为 1 ，结果就是一个字符串放在栈上（即，函数什么都不做）； 如果 n 为 0 ，结果是一个空串。 连接依照 Lua 中创建语义完成（参见 §2.5.4 ）。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_concat(IntPtr luaState, int n);                               //[-n, +1, e]

        //从堆栈中弹出 n 个元素。
        public static void lua_pop(IntPtr luaState, int amount)
        {
            LuaDLL.lua_settop(luaState, -(amount) - 1);
        }
        
        //创建一个空 table ，并将之压入堆栈。 它等价于 lua_createtable(L, 0, 0) 。
        public static void lua_newtable(IntPtr luaState)
        {
            LuaDLL.lua_createtable(luaState, 0, 0);
        }
        
        //把 C 函数 f 设到全局变量 name 中。
        public static void lua_register(IntPtr luaState, string name, LuaCSFunction func)
        {
            lua_pushcfunction(luaState, func);
            lua_setglobal(luaState, name);
        }
        
        //将一个 C 函数压入堆栈。 这个函数接收一个 C 函数指针，并将一个类型为 function 的 Lua 值 压入堆栈。当这个栈顶的值被调用时，将触发对应的 C 函数。
        public static void lua_pushcfunction(IntPtr luaState, LuaCSFunction func)
        {
            IntPtr fn = Marshal.GetFunctionPointerForDelegate(func);
            lua_pushcclosure(luaState, fn, 0);
        }
        
        //当给定索引的值是一个函数（ C 或 Lua 函数均可）时，返回 1 ，否则返回 0 。
        public static bool lua_isfunction(IntPtr luaState, int n)
        {
            return lua_type(luaState, n) == LuaTypes.LUA_TFUNCTION;
        }
        
        //当给定索引的值是一个 table 时，返回 1 ，否则返回 0 。
        public static bool lua_istable(IntPtr luaState, int n)
        {
            return lua_type(luaState, n) == LuaTypes.LUA_TTABLE;
        }
        
        //当给定索引的值是一个 light userdata 时，返回 1 ，否则返回 0 。
        public static bool lua_islightuserdata(IntPtr luaState, int n)
        {
            return lua_type(luaState, n) == LuaTypes.LUA_TLIGHTUSERDATA;
        }
        
        //当给定索引的值是 nil 时，返回 1 ，否则返回 0 。
        public static bool lua_isnil(IntPtr luaState, int n)
        {
            return (lua_type(luaState, n) == LuaTypes.LUA_TNIL);
        }
        
        //当给定索引的值类型为 boolean 时，返回 1 ，否则返回 0 。
        public static bool lua_isboolean(IntPtr luaState, int n)
        {
            LuaTypes type = lua_type(luaState, n);
            return type == LuaTypes.LUA_TBOOLEAN || type == LuaTypes.LUA_TNIL;
        }
        
        //当给定索引的值是一个 thread 时，返回 1 ，否则返回 0 。
        public static bool lua_isthread(IntPtr luaState, int n)
        {
            return lua_type(luaState, n) == LuaTypes.LUA_TTHREAD;
        }
        
        //
        public static bool lua_isnone(IntPtr luaState, int n)
        {
            return lua_type(luaState, n) == LuaTypes.LUA_TNONE;
        }
        
        //
        public static bool lua_isnoneornil(IntPtr luaState, int n)
        {
            return lua_type(luaState, n) <= LuaTypes.LUA_TNIL;
        }
        
        //从堆栈上弹出一个值，并将其设到全局变量 name 中。
        public static void lua_setglobal(IntPtr luaState, string name)
        {
            lua_setfield(luaState, LuaIndexes.LUA_GLOBALSINDEX, name);
        }
        
        //把全局变量 name 里的值压入堆栈。
        public static void lua_getglobal(IntPtr luaState, string name)
        {
            lua_getfield(luaState, LuaIndexes.LUA_GLOBALSINDEX, name);
        }
        
        //
        public static string lua_ptrtostring(IntPtr str, int len)
        {
            string ss = Marshal.PtrToStringAnsi(str, len);

            if (ss == null)
            {
                byte[] buffer = new byte[len];
                Marshal.Copy(str, buffer, 0, len);
                return Encoding.UTF8.GetString(buffer);
            }

            return ss;
        }
        
        //等价于 lua_tolstring ，而参数 len 设为 NULL 
        public static string lua_tostring(IntPtr luaState, int index)
        {
            int len = 0;
            IntPtr str = tolua_tolstring(luaState, index, out len);

            if (str != IntPtr.Zero)
            {
                return lua_ptrtostring(str, len);
            }

            return null;
        }
        
        //创建一个新的 lua_State。它使用基于标准C realloc函数的分配器调用lua_newstate，然后设置一个panic函数(参见lua_atpanic)，该函数在发生致命错误时向标准错误输出输出一条错误消息。
        public static IntPtr lua_open()
        {
            return luaL_newstate();
        }

        public static void lua_getregistry(IntPtr L)
        {
            lua_pushvalue(L, LuaIndexes.LUA_REGISTRYINDEX);
        }

        public static int lua_getgccount(IntPtr L)
        {
            return lua_gc(L, LuaGCOptions.LUA_GCCOUNT, 0);
        }

        /*
         ** ======================================================================
         ** Debug API
         ** =======================================================================
         */

        //获取解释器的运行时栈的信息
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_getstack(IntPtr L, int level, ref Lua_Debug ar);
        
        //返回一个指定的函数或函数调用的信息
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_getinfo(IntPtr L, string what, ref Lua_Debug ar);
        
        //从给定活动记录中获取一个局部变量的信息
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern string lua_getlocal(IntPtr L, ref Lua_Debug ar, int n);
        
        //设置给定活动记录中的局部变量的值
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern string lua_setlocal(IntPtr L, ref Lua_Debug ar, int n);
        
        //获取一个 closure 的 upvalue 信息
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern string lua_getupvalue(IntPtr L, int funcindex, int n);
        
        //设置 closure 的 upvalue 的值
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern string lua_setupvalue(IntPtr L, int funcindex, int n);
        
        //设置一个调试用钩子函数
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_sethook(IntPtr L, LuaHookFunc func, int mask, int count);
        
        //返回当前的钩子函数
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaHookFunc lua_gethook(IntPtr L);
        
        //返回当前的钩子掩码 (mask) 
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gethookmask(IntPtr L);
        
        //返回当前钩子记数
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gethookcount(IntPtr L);

        //lualib.h 意思是加载Lua 的所有标准库
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_openlibs(IntPtr luaState);

        //lauxlib.h 
        public static int abs_index(IntPtr L, int i)
        {
            return (i > 0 || i <= LuaIndexes.LUA_REGISTRYINDEX) ? i : lua_gettop(L) + i + 1;
        }
        
        //不赞成使用luaL_getn和luaL_setn(来自辅助库)函数。使用lua_objlen代替luaL_getn，使用nothing代替luaL_setn。
        public static int luaL_getn(IntPtr luaState, int i)
        {
            return (int)tolua_getn(luaState, i);
        }
        
        //从对象在索引obj的元表中推送字段e到堆栈上。如果对象没有元表，或者元表没有这个字段，返回0，不推送任何内容。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_getmetafield(IntPtr luaState, int stackPos, string field);           //[-0, +(0|1), m]
        
        //如果在索引obj上的对象有一个元表，这个元表有一个字段e，这个函数调用这个字段并将对象作为它的唯一参数传递。在本例中，该函数返回1并将调用返回的值压入堆栈。如果没有元表或元方法，该函数返回0(不将任何值压入堆栈)。
        public static int luaL_callmeta(IntPtr L, int stackPos, string field)                              //[-0, +(0|1), m]
        {
            stackPos = abs_index(L, stackPos);

            if (luaL_getmetafield(L, stackPos, field) == 0)  /* no metafield? */
            {
                return 0;
            }

            lua_pushvalue(L, stackPos);

            if (lua_pcall(L, 1, 1, 0) != 0)
            {
                string error = LuaDLL.lua_tostring(L, -1);
                lua_pop(L, 1);
                throw new LuaException(error);
            }

            return 1;
        }

        public static int luaL_argerror(IntPtr L, int narg, string extramsg)
        {
            if (tolua_argerror(L, narg, extramsg) != 0)
            {
                string error = LuaDLL.lua_tostring(L, -1);
                lua_pop(L, 1);
                throw new LuaException(error);
            }

            return 0;
        }
        
        //生成一个错误消息
        public static int luaL_typerror(IntPtr L, int stackPos, string tname, string t2 = null)
        {
            if (t2 == null)
            {
                t2 = luaL_typename(L, stackPos);
            }

            string msg = string.Format("{0} expected, got {1}", tname, t2);
            return luaL_argerror(L, stackPos, msg);
        }
        
        
        //检查函数参数narg是否为字符串并返回该字符串;如果l不是NULL，则用字符串的长度填充*l。
        public static string luaL_checklstring(IntPtr L, int numArg, out int len)
        {
            IntPtr str = tolua_tolstring(L, numArg, out len);

            if (str == IntPtr.Zero)
            {
                luaL_typerror(L, numArg, "string");
                return null;
            }

            return lua_ptrtostring(str, len);
        }
        
        //如果函数参数narg是一个字符串，返回这个字符串。如果该参数不存在或为nil，则返回d。否则，将引发错误。
        public static string luaL_optlstring(IntPtr L, int narg, string def, out int len)
        {
            if (lua_isnoneornil(L, narg))
            {
                len = def != null ? def.Length : 0;
                return def;
            }

            return luaL_checklstring(L, narg, out len);
        }
        
        //检查函数参数narg是否为数字并返回该数字。
        public static double luaL_checknumber(IntPtr L, int stackPos)
        {
            double d = lua_tonumber(L, stackPos);

            if (d == 0 && LuaDLL.lua_isnumber(L, stackPos) == 0)
            {
                luaL_typerror(L, stackPos, "number");
                return 0;
            }

            return d;
        }
        
        //如果函数参数narg是一个数字，则返回这个数字。如果该参数不存在或为nil，则返回d。否则，将引发错误。
        public static double luaL_optnumber(IntPtr L, int idx, double def)
        {
            if (lua_isnoneornil(L, idx))
            {
                return def;
            }

            return luaL_checknumber(L, idx);
        }
        
        //检查函数参数narg是否是一个数字，并将该数字转换为lua_Integer。
        public static int luaL_checkinteger(IntPtr L, int stackPos)
        {
            int d = tolua_tointeger(L, stackPos);

            if (d == 0 && lua_isnumber(L, stackPos) == 0)
            {
                luaL_typerror(L, stackPos, "number");
                return 0;
            }

            return d;
        }

        public static int luaL_optinteger(IntPtr L, int idx, int def)
        {
            if (lua_isnoneornil(L, idx))
            {
                return def;
            }

            return luaL_checkinteger(L, idx);
        }

        public static bool luaL_checkboolean(IntPtr luaState, int index)
        {
            if (lua_isboolean(luaState, index))
            {
                return lua_toboolean(luaState, index);
            }

            luaL_typerror(luaState, index, "boolean");
            return false;
        }
        
        //将堆栈大小增加到top + sz元素，如果堆栈不能增加到那个大小，就会产生一个错误。msg是一个附加文本进入错误消息。
        public static void luaL_checkstack(IntPtr L, int space, string mes)
        {
            if (lua_checkstack(L, space) == 0)
            {
                throw new LuaException(string.Format("stack overflow {0}", mes));
            }
        }
        
        //检查函数参数narg的类型是否为t。
        public static void luaL_checktype(IntPtr L, int narg, LuaTypes t)
        {
            if (lua_type(L, narg) != t)
            {
                luaL_typerror(L, narg, lua_typename(L, t));
            }
        }
        
        //检查函数在narg位置是否有任何类型的参数(包括nil)。
        public static void luaL_checkany(IntPtr L, int narg)
        {
            if (lua_type(L, narg) == LuaTypes.LUA_TNONE)
            {
                luaL_argerror(L, narg, "value expected");
            }
        }
        
        //检查函数参数narg是否是tname类型的用户数据(请参阅luaL_newmetatable)。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_newmetatable(IntPtr luaState, string meta);                               //[-0, +1, m]
        
        //检查函数参数narg是否是tname类型的用户数据(请参阅luaL_newmetatable)。
        public static IntPtr luaL_checkudata(IntPtr L, int ud, string tname)
        {
            IntPtr p = lua_touserdata(L, ud);

            if (p != IntPtr.Zero)
            {
                if (lua_getmetatable(L, ud) != 0)
                {
                    lua_getfield(L, LuaIndexes.LUA_REGISTRYINDEX, tname);  /* get correct metatable */

                    if (lua_rawequal(L, -1, -2) != 0)
                    {  /* does it have the correct mt? */
                        lua_pop(L, 2);  /* remove both metatables */
                        return p;
                    }
                }
            }

            luaL_typerror(L, ud, tname);    /* else error */
            return IntPtr.Zero;             /* to avoid warnings */
        }
        
        //将一个字符串压入堆栈，该字符串标识调用堆栈中控件在lvl级的当前位置。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_where(IntPtr luaState, int level);                                           //[-0, +1, e]

        //[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        //public static extern int luaL_error(IntPtr luaState, string message);

        public static int luaL_throw(IntPtr L, string message)
        {
            tolua_pushtraceback(L);
            lua_pushstring(L, message);
            lua_pushnumber(L, 1);

            if (lua_pcall(L, 2, -1, 0) == 0)
            {
                message = lua_tostring(L, -1);
            }
            else
            {
                lua_pop(L, 1);
            }

            throw new LuaException(message, null, 2);
        }
        
        //在索引t处的表中为堆栈顶部的对象创建并返回一个引用(并弹出该对象)。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_ref(IntPtr luaState, int t);                                                  //[-1, +0, m]
        
        //从索引t处的表中释放引用ref(请参阅luaL_ref)。条目从表中删除，以便可以收集所引用的对象。引用ref也被释放以再次使用。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_unref(IntPtr luaState, int registryIndex, int reference);
        
        //以Lua块的形式加载文件。这个函数使用lua_load将块加载到名为filename的文件中。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_loadfile(IntPtr luaState, string filename);                                   //[-0, +1, e]
        
        //作为Lua块加载缓冲区。这个函数使用lua_load加载大小为sz的buff所指向的缓冲区中的块。
        public static int luaL_loadbuffer(IntPtr luaState, byte[] buff, int size, string name)
        {
            return tolua_loadbuffer(luaState, buff, size, name);
        }
        
        //加载作为Lua块的字符串。这个函数使用lua_load将块加载到零终止字符串s中。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_loadstring(IntPtr luaState, string chunk);
        
        //创建一个新的Lua状态。它使用基于标准C realloc函数的分配器调用lua_newstate，然后设置一个panic函数(参见lua_atpanic)，该函数在发生致命错误时向标准错误输出输出一条错误消息。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr luaL_newstate();
        
        //通过将出现的字符串p替换为字符串r来创建字符串s的副本。将得到的字符串压入堆栈并返回。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr luaL_gsub(IntPtr luaState, string str, string pattern, string replacement);     //[-0, +1, e]  

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr luaL_findtable(IntPtr luaState, int idx, string fname, int szhint = 1);

        /*
         ** ===============================================================
         ** some useful functions
         ** ===============================================================
        */
        public static string luaL_typename(IntPtr luaState, int stackPos)
        {
            LuaTypes type = LuaDLL.lua_type(luaState, stackPos);
            return lua_typename(luaState, type);
        }
        
        //加载并运行给定的文件。
        public static bool luaL_dofile(IntPtr luaState, string fileName)                                              //[-0, +1, e]
        {
            int result = luaL_loadfile(luaState, fileName);

            if (result != 0)
            {
                return false;
            }

            return LuaDLL.lua_pcall(luaState, 0, LUA_MULTRET, 0) == 0;
        }
        
        //加载并运行给定的字符串
        public static bool luaL_dostring(IntPtr luaState, string chunk)
        {
            int result = LuaDLL.luaL_loadstring(luaState, chunk);

            if (result != 0)
            {
                return false;
            }

            return LuaDLL.lua_pcall(luaState, 0, LUA_MULTRET, 0) == 0;
        }
        
        
        //在注册表中将与名称tname关联的元表压入堆栈
        public static void luaL_getmetatable(IntPtr luaState, string meta)
        {
            LuaDLL.lua_getfield(luaState, LuaIndexes.LUA_REGISTRYINDEX, meta);
        }

        //在索引t的表中为堆栈顶部的对象创建并返回引用（并弹出对象）。
        /* compatibility with ref system */
        public static int lua_ref(IntPtr luaState)
        {
            return LuaDLL.luaL_ref(luaState, LuaIndexes.LUA_REGISTRYINDEX);
        }
        
        //
        public static void lua_getref(IntPtr luaState, int reference)
        {
            lua_rawgeti(luaState, LuaIndexes.LUA_REGISTRYINDEX, reference);
        }

        public static void lua_unref(IntPtr luaState, int reference)
        {
            luaL_unref(luaState, LuaIndexes.LUA_REGISTRYINDEX, reference);
        }

        /*
        ** ======================================================
        ** tolua libs 这是 tolua C 在 Lua C 上面封装的一层接口
        ** =======================================================
        */
        
        //tolua 写的 C 代码库,使用这种 API 与 Lua的 API 相仿.
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_openlibs(IntPtr L);
        
        //声明并注册了int64元表
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_openint64(IntPtr L);
        
        //tolua 里面打开 Lua 的库
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_openlualibs(IntPtr L);
        
        //
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr tolua_tag();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_newudata(IntPtr luaState, int val);                         //[-0, +0, m]

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_rawnetobj(IntPtr luaState, int obj);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool tolua_pushudata(IntPtr L, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool tolua_pushnewudata(IntPtr L, int metaRef, int index);             //[-0, +0, m]

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_beginpcall(IntPtr L, int reference);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushtraceback(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_getvec2(IntPtr luaState, int stackPos, out float x, out float y);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_getvec3(IntPtr luaState, int stackPos, out float x, out float y, out float z);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_getvec4(IntPtr luaState, int stackPos, out float x, out float y, out float z, out float w);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_getclr(IntPtr luaState, int stackPos, out float r, out float g, out float b, out float a);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_getquat(IntPtr luaState, int stackPos, out float x, out float y, out float z, out float w);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_getlayermask(IntPtr luaState, int stackPos);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushvec2(IntPtr luaState, float x, float y);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushvec3(IntPtr luaState, float x, float y, float z);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushvec4(IntPtr luaState, float x, float y, float z, float w);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushquat(IntPtr luaState, float x, float y, float z, float w);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushclr(IntPtr luaState, float r, float g, float b, float a);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushlayermask(IntPtr luaState, int mask);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool tolua_isint64(IntPtr luaState, int stackPos);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern long tolua_toint64(IntPtr luaState, int stackPos);

        public static long tolua_checkint64(IntPtr L, int stackPos)
        {
            long d = tolua_toint64(L, stackPos);

            if (d == 0 && !tolua_isint64(L, stackPos))
            {
                luaL_typerror(L, stackPos, "long");
                return 0;
            }

            return d;
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushint64(IntPtr luaState, long n);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool tolua_isuint64(IntPtr luaState, int stackPos);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong tolua_touint64(IntPtr luaState, int stackPos);

        public static ulong tolua_checkuint64(IntPtr L, int stackPos)
        {
            ulong d = tolua_touint64(L, stackPos);

            if (d == 0 && !tolua_isuint64(L, stackPos))
            {
                luaL_typerror(L, stackPos, "ulong");
                return 0;
            }

            return d;
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushuint64(IntPtr luaState, ulong n);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_setindex(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_setnewindex(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int toluaL_ref(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void toluaL_unref(IntPtr L, int reference);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr tolua_getmainstate(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_getvaluetype(IntPtr L, int stackPos);                

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool tolua_createtable(IntPtr L, string fullPath, int szhint = 0);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool tolua_pushluatable(IntPtr L, string fullPath);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool tolua_beginmodule(IntPtr L, string name);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_endmodule(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool tolua_beginpremodule(IntPtr L, string fullPath, int szhint = 0);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_endpremodule(IntPtr L, int reference);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool tolua_addpreload(IntPtr L, string path);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_beginclass(IntPtr L, string name, int baseMetaRef, int reference = -1);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_endclass(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_function(IntPtr L, string name, IntPtr fn);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr tolua_tocbuffer(string name, int sz);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_freebuffer(IntPtr buffer);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_variable(IntPtr L, string name, IntPtr get, IntPtr set);


        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_constant(IntPtr L, string name, double val);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_beginenum(IntPtr L, string name);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_endenum(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_beginstaticclass(IntPtr L, string name);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_endstaticclass(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_require(IntPtr L, string fileName);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_getmetatableref(IntPtr L, int pos);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_setflag(int bit, bool flag);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool tolua_isvptrtable(IntPtr L, int index);

        public static int toluaL_exception(IntPtr L, Exception e)
        {            
            LuaException.luaStack = new LuaException(e.Message, e, 2);            
            return tolua_error(L, e.Message);
        }

        public static int toluaL_exception(IntPtr L, Exception e, object o, string msg)
        {
            if (o != null && !o.Equals(null))
            {
                msg = e.Message;
            }
            
            LuaException.luaStack = new LuaException(msg, e, 2);
            return tolua_error(L, msg);
        }

        //适配函数
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_loadbuffer(IntPtr luaState, byte[] buff, int size, string name);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool tolua_toboolean(IntPtr luaState, int index);

        
        //把给定索引处的 Lua 值转换为 lua_Integer 这样一个有符号整数类型。这个 Lua 值必须是一个数字或是一个可以转换为数字的字符串（参见 §2.2.1）；否则，lua_tointeger 返回 0 。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_tointeger(IntPtr luaState, int idx);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr tolua_tolstring(IntPtr luaState, int index, out int strLen);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushlstring(IntPtr luaState, byte[] str, int size);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_objlen(IntPtr luaState, int stackPos);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr tolua_newuserdata(IntPtr luaState, int size);                     //[-0, +1, m]

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_argerror(IntPtr luaState, int narg, string extramsg);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_error(IntPtr L, string msg);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_getfield(IntPtr L, int idx, string key);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_setfield(IntPtr L, int idx, string key);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_gettable(IntPtr luaState, int idx);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_settable(IntPtr luaState, int idx);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_getn(IntPtr luaState, int stackPos);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_strlen(IntPtr str);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushcfunction(IntPtr L, IntPtr fn);

        public static void tolua_pushcfunction(IntPtr luaState, LuaCSFunction func)
        {
            IntPtr fn = Marshal.GetFunctionPointerForDelegate(func);
            tolua_pushcfunction(luaState, fn);
        }

        public static string tolua_findtable(IntPtr L, int idx, string name, int size = 1)
        {
            int oldTop = lua_gettop(L);
            IntPtr p = LuaDLL.luaL_findtable(L, idx, name, size);

            if (p != IntPtr.Zero)
            {
                LuaDLL.lua_settop(L, oldTop);
                int len = LuaDLL.tolua_strlen(p);
                return LuaDLL.lua_ptrtostring(p, len);
            }

            return null;
        }

        public static IntPtr tolua_atpanic(IntPtr L, LuaCSFunction func)
        {
            IntPtr fn = Marshal.GetFunctionPointerForDelegate(func);
            return lua_atpanic(L, fn);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr tolua_buffinit(IntPtr luaState);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_addlstring(IntPtr b, string str, int l);      
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_addstring(IntPtr b, string s);                
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_addchar(IntPtr b, byte s);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushresult(IntPtr b);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_update(IntPtr L, float deltaTime, float unscaledDelta);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_lateupdate(IntPtr L);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_fixedupdate(IntPtr L, float fixedTime);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_regthis(IntPtr L, IntPtr get, IntPtr set);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_where(IntPtr L, int level);

        public static void tolua_bindthis(IntPtr L, LuaCSFunction get, LuaCSFunction set)
        {
            IntPtr pGet = IntPtr.Zero;
            IntPtr pSet = IntPtr.Zero;

            if (get != null)
            {
                pGet = Marshal.GetFunctionPointerForDelegate(get);
            }

            if (set != null)
            {
                pSet = Marshal.GetFunctionPointerForDelegate(set);
            }

            tolua_regthis(L, pGet, pSet);
        }
        
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_getclassref(IntPtr L, int pos);
    }
}
