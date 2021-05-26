/*
** $Id: lstate.h,v 2.24.1.2 2008/01/03 15:20:39 roberto Exp $
** Global State
** See Copyright Notice in lua.h
*/

#ifndef lstate_h
#define lstate_h

#include "lua.h"

#include "lobject.h"
#include "ltm.h"
#include "lzio.h"



struct lua_longjmp;  /* defined in ldo.c */


/* table of globals */
#define gt(L)	(&L->l_gt)

/* registry 表,是全局唯一的,它存放在 global_state 结构体中,这个结构体在整个运行环境中只有一个 */
#define registry(L)	(&G(L)->l_registry)


/* extra stack space to handle TM calls and some other extras */
#define EXTRA_STACK   5


#define BASIC_CI_SIZE           8

#define BASIC_STACK_SIZE        (2*LUA_MINSTACK)



typedef struct stringtable {//散列数组,专门用于存放字符串
  GCObject **hash;
  lu_int32 nuse;  /* number of elements 元素数量 */
  int size;       // hash桶数组大小
} stringtable;


/*
** informations about a call
*/
typedef struct CallInfo {
  StkId base;  /* base for this function */
  StkId func;  /* function index in the stack */
  StkId	top;  /* top for this function */
  const Instruction *savedpc;
  int nresults;  /* expected number of results from this function */
  int tailcalls;  /* number of tail calls lost under this entry */
} CallInfo;



#define curr_func(L)	(clvalue(L->ci->func))
#define ci_func(ci)	(clvalue((ci)->func))
#define f_isLua(ci)	(!ci_func(ci)->c.isC)
#define isLua(ci)	(ttisfunction((ci)->func) && f_isLua(ci))


/*
** `global state', shared by all threads of this state
*/
typedef struct global_State {
  stringtable strt;  /* hash table for strings 所有字符串的 hash 表 */
  lua_Alloc frealloc;  /* function to reallocate memory */
  void *ud;         /* auxiliary data to `frealloc' */
  lu_byte currentwhite;//存放当前 GC 的白色
  lu_byte gcstate;  /* state of garbage collector 存放 GC 状态 (lgc.h 17-21行) */
  int sweepstrgc;  /* position of sweep in `strt' 字符串回收阶段,每次针对字符串散列桶的一组字符串进行回收,这个值用于记录对应的散列桶索引 */
  GCObject *rootgc;  /* list of all collectable objects 存放待 GC 对象的链表,所有对象创建之后都会放入该链表中 */
  GCObject **sweepgc;  /* position of sweep in `rootgc' 待处理的回收数据都存放在 rootgc 链表中,由于回收阶段不是一次性全部回收这个链表的所有数据,所以使用这个变量来保存当前回收的位置,下一次从这个位置开始继续回收操作 */
  GCObject *gray;  /* list of gray objects 存放灰色节点的链表 */
  GCObject *grayagain;  /* list of objects to be traversed atomically 存放需要一次性扫描处理的灰色节点的链表,也就是说,这个链表上所有数据的处理需要一步到位,不能被打断 */
  GCObject *weak;  /* list of weak tables (to be cleared) 存放弱表的链表*/
  GCObject *tmudata;  /* last element of list of userdata to be GC ; 所有有GC方法的udata都放在tmudata链表中,这个成员指向这个链表的最后一个元素 */
  Mbuffer buff;  /* temporary buffer for string concatentation */
  lu_mem GCthreshold; // 一个阈值，当这个totalbytes大于这个阈值时进行自动GC
  lu_mem totalbytes;  /* number of bytes currently allocated  // 保存当前分配的总内存数量 */
  // 一个估算值，根据这个计算GCthreshold
  lu_mem estimate;  /* an estimate of number of bytes actually in use */
  // 当前待GC的数据大小，其实就是累加totalbytes和GCthreshold的差值
  lu_mem gcdept;  /* how much GC is `behind schedule' */
  // 可以配置的一个值，不是计算出来的，根据这个计算GCthreshold，以此来控制下一次GC触发的时间
  int gcpause;  /* size of pause between successive GCs */
  // 每次进行GC操作回收的数据比例，见lgc.c/luaC_step函数
  int gcstepmul;  /* GC `granularity' */
  lua_CFunction panic;  /* to be called in unprotected errors */
  TValue l_registry;//这个 registry 表只能由 C 代码访问,不能由 Lua 代码访问.key 必须是字符串类型的.这个表提供全局变量的存储,env 表提供的是函数内全局变量的存储.
  struct lua_State *mainthread;
  UpVal uvhead;  /* head of double-linked list of all open upvalues */
  struct Table *mt[NUM_TAGS];  /* metatables for basic types */
  TString *tmname[TM_N];  /* array with tag-method names */
} global_State;


/*
** `per thread' state
 * 虚拟栈使用 索引(index) 来引用栈中的元素.
 * 第一个被压入栈中的元素为 1,第二个被压入栈中的元素为 2;
 * 可以用负数索引 -1 表示栈顶,-2 表示在-1 之前被压入栈的元素
 * 在不同的场景下使用不同的索引方式
*/
struct lua_State {
  CommonHeader;
  lu_byte status;
  StkId top;  /* first free slot in the stack */
  StkId base;  /* base of current function */
  global_State *l_G;
  CallInfo *ci;  /* call info for current function */
  const Instruction *savedpc;  /* `savedpc' of current function */
  StkId stack_last;  /* last free slot in the stack */
  StkId stack;  /* stack base */
  CallInfo *end_ci;  /* points after end of ci array*/
  CallInfo *base_ci;  /* array of CallInfo's */
  int stacksize;
  int size_ci;  /* size of array `base_ci' */
  unsigned short nCcalls;  /* number of nested C calls */
  unsigned short baseCcalls;  /* nested C calls when resuming coroutine */
  lu_byte hookmask;
  lu_byte allowhook;
  int basehookcount;
  int hookcount;
  lua_Hook hook;
  TValue l_gt;  /* table of globals */
  TValue env;  /* temporary place for environments */
  GCObject *openupval;  /* list of open upvalues in this stack */
  GCObject *gclist;
  struct lua_longjmp *errorJmp;  /* current error recover point */
  ptrdiff_t errfunc;  /* current error handling function (stack index) */
};


#define G(L)	(L->l_G)


/*
** Union of all collectable objects
 * 所有需要进行垃圾回收的数据类型
*/
union GCObject {
  GCheader gch;
  union TString ts;
  union Udata u;
  union Closure cl;
  struct Table h;
  struct Proto p;
  struct UpVal uv;
  struct lua_State th;  /* thread */
};


/* macros to convert a GCObject into a specific value */
#define rawgco2ts(o)	check_exp((o)->gch.tt == LUA_TSTRING, &((o)->ts))
#define gco2ts(o)	(&rawgco2ts(o)->tsv)
#define rawgco2u(o)	check_exp((o)->gch.tt == LUA_TUSERDATA, &((o)->u))
#define gco2u(o)	(&rawgco2u(o)->uv)
#define gco2cl(o)	check_exp((o)->gch.tt == LUA_TFUNCTION, &((o)->cl))
#define gco2h(o)	check_exp((o)->gch.tt == LUA_TTABLE, &((o)->h))
#define gco2p(o)	check_exp((o)->gch.tt == LUA_TPROTO, &((o)->p))
#define gco2uv(o)	check_exp((o)->gch.tt == LUA_TUPVAL, &((o)->uv))
#define ngcotouv(o) \
	check_exp((o) == NULL || (o)->gch.tt == LUA_TUPVAL, &((o)->uv))
#define gco2th(o)	check_exp((o)->gch.tt == LUA_TTHREAD, &((o)->th))

/* macro to convert any Lua object into a GCObject */
#define obj2gco(v)	(cast(GCObject *, (v)))


LUAI_FUNC lua_State *luaE_newthread (lua_State *L);
LUAI_FUNC void luaE_freethread (lua_State *L, lua_State *L1);

#endif
