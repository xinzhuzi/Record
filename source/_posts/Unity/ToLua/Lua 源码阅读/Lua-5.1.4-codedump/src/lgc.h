/*
** $Id: lgc.h,v 2.15.1.1 2007/12/27 13:02:25 roberto Exp $
** Garbage Collector  垃圾回收
** See Copyright Notice in lua.h
*/

#ifndef lgc_h
#define lgc_h


#include "lobject.h"


/*
** Possible states of the Garbage Collector GC状态
*/
#define GCSpause	0 //暂停阶段
#define GCSpropagate	1//传播阶段,用于遍历灰色节点检查对象的引用情况,
#define GCSsweepstring	2//字符串回收阶段
#define GCSsweep	3//回收阶段,用于对除了字符串之外的所有其他数据类型进行回收
#define GCSfinalize	4//终止阶段


/*
** some userful bit tricks
*/
#define resetbits(x,m)	((x) &= cast(lu_byte, ~(m)))
#define setbits(x,m)	((x) |= (m))
#define testbits(x,m)	((x) & (m))
#define bitmask(b)	(1<<(b))
#define bit2mask(b1,b2)	(bitmask(b1) | bitmask(b2))
#define l_setbit(x,b)	setbits(x, bitmask(b))
#define resetbit(x,b)	resetbits(x, bitmask(b))
#define testbit(x,b)	testbits(x, bitmask(b))
#define set2bits(x,b1,b2)	setbits(x, (bit2mask(b1, b2)))
#define reset2bits(x,b1,b2)	resetbits(x, (bit2mask(b1, b2)))
#define test2bits(x,b1,b2)	testbits(x, (bit2mask(b1, b2)))



/*
** Layout for bit use in `marked' field:  GCHeader 中的 marked 标记字段的值
** bit 0 - object is white (type 0) 第一种白色类型
** bit 1 - object is white (type 1) 第二种白色类型
** bit 2 - object is black 黑色
** bit 3 - for userdata: has been finalized
** bit 3 - for tables: has weak keys 表中的弱key
** bit 4 - for tables: has weak values 表中的弱value
** bit 5 - object is fixed (should not be collected) 不可被回收(lua_state对象)
** bit 6 - object is "super" fixed (only the main thread) 不可被回收(字符串对象)
*/


#define WHITE0BIT	0
#define WHITE1BIT	1
#define BLACKBIT	2
#define FINALIZEDBIT	3
// 弱key
#define KEYWEAKBIT	3
// 弱值
#define VALUEWEAKBIT	4
// 标记这个GC对象不可回收
#define FIXEDBIT	5
#define SFIXEDBIT	6
// 两种白色的或
#define WHITEBITS	bit2mask(WHITE0BIT, WHITE1BIT)

// 将mark位与两个白色位进行比较，只要其中一个置位就是白色的
#define iswhite(x)      test2bits((x)->gch.marked, WHITE0BIT, WHITE1BIT)
// 将mark位与黑色位进行比较
#define isblack(x)      testbit((x)->gch.marked, BLACKBIT)
// 既不是白色，也不是黑色
#define isgray(x)	(!isblack(x) && !iswhite(x))
// 不是当前的白色
#define otherwhite(g)	(g->currentwhite ^ WHITEBITS)

// 如果结点的白色是otherwhite，那么就是一个死结点
// 这个函数都是在mark阶段过后使用的,所以此时的otherwhite其实就是本次GC的白色
#define isdead(g,v)	((v)->gch.marked & otherwhite(g) & WHITEBITS)

// 将节点mark为白色,同时清除黑色/灰色等
#define changewhite(x)	((x)->gch.marked ^= WHITEBITS)
#define gray2black(x)	l_setbit((x)->gch.marked, BLACKBIT)

#define valiswhite(x)	(iscollectable(x) && iswhite(gcvalue(x)))

// 返回当前的白色
#define luaC_white(g)	cast(lu_byte, (g)->currentwhite & WHITEBITS)

// 如果大于阙值,就启动一次GC
#define luaC_checkGC(L) { \
  condhardstacktests(luaD_reallocstack(L, L->stacksize - EXTRA_STACK - 1)); \
  if (G(L)->totalbytes >= G(L)->GCthreshold) \
	luaC_step(L); }


#define luaC_barrier(L,p,v) { if (valiswhite(v) && isblack(obj2gco(p)))  \
	luaC_barrierf(L,obj2gco(p),gcvalue(v)); }

// 回退成黑色
#define luaC_barriert(L,t,v) { if (valiswhite(v) && isblack(obj2gco(t)))  \
	luaC_barrierback(L,t); }

#define luaC_objbarrier(L,p,o)  \
	{ if (iswhite(obj2gco(o)) && isblack(obj2gco(p))) \
		luaC_barrierf(L,obj2gco(p),obj2gco(o)); }

#define luaC_objbarriert(L,t,o)  \
   { if (iswhite(obj2gco(o)) && isblack(obj2gco(t))) luaC_barrierback(L,t); }

LUAI_FUNC size_t luaC_separateudata (lua_State *L, int all);
LUAI_FUNC void luaC_callGCTM (lua_State *L);
LUAI_FUNC void luaC_freeall (lua_State *L);
LUAI_FUNC void luaC_step (lua_State *L);
LUAI_FUNC void luaC_fullgc (lua_State *L);
LUAI_FUNC void luaC_link (lua_State *L, GCObject *o, lu_byte tt);
LUAI_FUNC void luaC_linkupval (lua_State *L, UpVal *uv);
LUAI_FUNC void luaC_barrierf (lua_State *L, GCObject *o, GCObject *v);
LUAI_FUNC void luaC_barrierback (lua_State *L, Table *t);


#endif


/* *
 * 在 Lua 代码中,有 2 种回收方式,一种是自动回收,一种是程序自己调用 API 来触发一次回收.
 *
 *
 * 自动回收会在每次调用内存分配相关的操作时检查是否满足触发条件,这个操作在宏 luaC_checkGC 中进行.
 * 触发自动 GC 的条件就是:totalbytes 大于等于 GCthreshold 值,在这两个变量中,totalbytes 用于保存当前分配的内存大小,而 GCthreshold 是一个阈值,
 * 这个值可以由一些参数影响和控制,由此改变触发的条件.此情况不可控,关闭方式是将GCthreshold设置为一个非常大的值,来达到一直不满足自动触发的条件.
 *
 *
 * 手动 GC 受哪些参数影响? estimate/gcpause 两个成员将影响每次 GCthreshold 的值;#define setthreshold(g)  (g->GCthreshold = (g->estimate/100) * g->gcpause)
 * estimate 是一个预估的当前使用的内存数量,gcpause 则是一个百分比,这个宏的作用就是按照估计值的百分比计算出新的阈值.
 * gcpause 通过 lua_gc 这个C的接口来进行设置,可以看到,百分比越大,下一次开始 GC 的时间就会越长.
 * 另一个影响 GC 进度的参数是 gcstepmul 成员,它同样可以通过 lua_gc 来设置,这个参数将影响每次手动 GC 时调用 singlestep 函数的次数,从而影响 GC 回收的速度.
 * 如果希望关闭 GC,还需要再手动执行完一次 GC 之后,重新设置关闭自动 GC
 *
 * */