/*
** $Id: linit.c,v 1.14.1.1 2007/12/27 13:02:25 roberto Exp $
** Initialization of libraries for lua.c  内嵌库的初始化
** See Copyright Notice in lua.h
*/


#define linit_c
#define LUA_LIB

#include "lua.h"

#include "lualib.h"
#include "lauxlib.h"


static const luaL_Reg lualibs[] = {
  {"", luaopen_base},//模块名为空,因此方位这个模块的函数不需要加模块名前缀,比如 print
  {LUA_LOADLIBNAME, luaopen_package},//package  函数库
  {LUA_TABLIBNAME, luaopen_table}, // table 函数库
  {LUA_IOLIBNAME, luaopen_io}, // io 函数库
  {LUA_OSLIBNAME, luaopen_os},//os 函数库
  {LUA_STRLIBNAME, luaopen_string},// string 函数库
  {LUA_MATHLIBNAME, luaopen_math}, // math 函数库
  {LUA_DBLIBNAME, luaopen_debug}, // debug 函数库
  {NULL, NULL}
};

//打开标准库
LUALIB_API void luaL_openlibs (lua_State *L) {
  const luaL_Reg *lib = lualibs;
  for (; lib->func; lib++) {
	// 向函数栈push函数结构体
    lua_pushcfunction(L, lib->func);
    // 向函数栈push库名(这里似乎没有必要push名字进去)
    lua_pushstring(L, lib->name);
    // 调用注册的C函数, 也就是上面的一堆luaopen_*函数
    // 问题: 为什么这里不直接调用呢?非得压入lua栈中调用?
    // 传入1的原因是要跳过上一步压入的函数名,0的意思是这些函数全都返回值为0,即没有返回值
    lua_call(L, 1, 0);
  }
}

