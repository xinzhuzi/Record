---
title: Tolua C/C++ Lua 交互
date: 2020-05-08 11:41:32
top: 702
categories:
- Unity
tags:
- ToLua
---

# ToLua C/C++ Lua 交互

# Lua C API 简介
* 1. 向 lua_state 栈里面压入一个值:
```
    lua_pushboolean
    lua_pushcclosure
    lua_pushcfunction
    lua_pushfstring
    lua_pushinteger
    lua_pushlightuserdata
    lua_pushliteral
    lua_pushlstring
    lua_pushnil
    lua_pushnumber
    lua_pushstring
    lua_pushthread
    lua_pushvalue 将指定元素,压入栈顶,栈size加一
    lua_pushvfstring
```
从 lua_state 栈里面查询里面的元素:
```
    lua_type(8种类型)
    lua_isboolean
    lua_iscfunction
    lua_isfunction
    lua_islightuserdata
    lua_isnil
    lua_isnone
    lua_isnoneornil
    lua_isnumber
    lua_isstring
    lua_istable
    lua_isthread
    lua_isuserdata
```
获取栈内给定位置的元素值:

```
    lua_toboolean       
    lua_tocfunction
    lua_tointeger
    lua_tolstring
    lua_tonumber
    lua_topointer
    lua_tostring
    lua_tothread
    lua_touserdata
    上面方法都有(lua_State * L,int index) 参数,传入当前的Lua栈,以及栈上的位置
```

其他方法:
```
    这个地方的 index 以正整数为解释
    void lua_settop (lua_State *L, int index);接受任何可接受的索引或0，并将堆栈顶部设置为该索引,大于该索引的,全部被删除。如果新的顶部比旧的大，那么新元素将被填充为nil。如果索引为0，则删除所有堆栈元素。
    void lua_remove (lua_State *L, int index);删除给定有效索引处的元素，并向下移动该索引上方的元素以填充空白。不能用伪索引调用，因为伪索引不是实际的堆栈位置。
    void lua_insert (lua_State *L, int index);将顶部元素移到给定的有效索引中，将该索引上方的元素上移到空白处。无法使用伪索引调用，因为伪索引不是实际的堆栈位置。
    void lua_replace (lua_State *L, int index);将顶部元素移到给定位置,并弹出,而不移动剩下的任何元素,再替换给定位置的值,

    void lua_call (lua_State *L, int nargs, int nresults);要调用一个函数，您必须使用以下协议:首先，要调用的函数被压入栈中;然后，将函数的参数按直接顺序推送;也就是说，首先推送第一个参数。最后调用lua_call;nargs是压入堆栈的参数数量。当调用函数时，所有参数和函数值都将从堆栈中弹出。当函数返回时，函数结果被推送到堆栈上。结果的数量被调整为nresults，除非nresults是LUA_MULTRET。在本例中，函数的所有结果都被推入。Lua会注意返回值是否适合堆栈空间。函数结果按直接顺序被压入栈中(第一个结果被先压入)，因此在调用之后，最后一个结果位于栈的顶部。

```
全局表_G主要是在Lua 代码使用,注册表主要是在 C 代码里面使用,比如 userdata 的元表等,分开之后,不会造成数据混乱,并且一个 lua_State 对应的是一个 _G 与一个注册表.