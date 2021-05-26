#ifndef LUA_C_INVOKE_LUACALLCPP_H
#define LUA_C_INVOKE_LUACALLCPP_H

//访问 C 变量以及函数,
//1.调用相应的 API 函数将 C 变量入栈.
//2.调用 lua_setglobal 函数进行注册.

#ifdef __cplusplus //如果在 C++ 语言上面编译的话,就可以识别这个宏
extern "C"
{
#endif

#include "Lua/lua.h"
#include "Lua/lauxlib.h"
#include "Lua/lualib.h"

void testLuaCallCpp1();
void testLuaCallCpp2();
void testLuaCallCpp3();
void testLuaCallCpp4();

#ifdef __cplusplus
};
#endif
#endif //LUA_C_INVOKE_LUACALLCPP_H
