#include <iostream>
#include <stdio.h>
#include <string.h>
#include "LuaCallCPP.h"

extern "C"
{
    #include "Lua/lua.h"
    #include "Lua/lauxlib.h"
    #include "Lua/lualib.h"
}

//Lua 解释器的简单实现
void LuaInterpreter()
{
    lua_State * L = lua_open();//创建一个虚拟机的堆栈.
    luaopen_base(L);//必须打开基础库,否则任何函数都运行不了.

    char  buffer[256];
    while (fgets(buffer,sizeof(buffer),stdin)!=NULL)
    {
        luaL_loadbuffer(L,buffer,strlen(buffer),"stdin");
        lua_pcall(L,0,0,0);
    }


    //加载脚本
//    luaL_loadfile(L,"../test.lua");
//    lua_pcall(L,0,0,0);

    lua_close(L);
}

//Lua 执行 lua脚本
void LuaRunScript()
{
    lua_State * L = lua_open();//创建一个虚拟机的堆栈.
    luaopen_base(L);//必须打开基础库,否则任何函数都运行不了.

    luaL_dofile(L,"../test.lua");//dofile 最后执行了 lua_pcall 所以下面不用执行lua_pcall了
//    lua_pcall(L,0,0,0);

    //C 语言访问 Lua 脚本中的全局变量
    //使用 lua_getglobal 就会将全局变量放入堆栈
    //使用 lua_toxxx 就是查看堆栈对应的元素是什么

    lua_getglobal(L,"a");
    if (1 == lua_isnumber(L,-1))
    {
        lua_Number a = lua_tonumber(L,-1);//取出了值,如果这个值用不到了,就需要被堆栈丢弃,出栈
        lua_pop(L,-1);
        printf("C语言:从 Lua 脚本中拿出的变量 a : %f",a);
    }else{
        printf("打印错误");
    }

    lua_close(L);
}

//Lua 执行 lua脚本
void LuaRunScript1()
{
    lua_Number width = 0;
    lua_Number height = 0;

    lua_State * L = lua_open();//创建一个虚拟机的堆栈.
    luaopen_base(L);//必须打开基础库,否则任何函数都运行不了.

    luaL_dofile(L,"../p1.lua");//dofile 最后执行了 lua_pcall 所以下面不用执行lua_pcall了
//    lua_pcall(L,0,0,0);

    //C 语言访问 Lua 脚本中的全局变量
    //使用 lua_getglobal 就会将全局变量放入堆栈
    //使用 lua_toxxx 就是查看堆栈对应的元素是什么

    lua_getglobal(L,"width");
    lua_getglobal(L,"height");
    if (1 == lua_isnumber(L,-1))
    {
        width = lua_tonumber(L,-2);//取出了值,如果这个值用不到了,就需要被堆栈丢弃,出栈
        height = lua_tonumber(L,-1);//取出了值,如果这个值用不到了,就需要被堆栈丢弃,出栈
        lua_pop(L,-1);
        lua_pop(L,-1);
        printf("C语言:从 Lua 脚本中拿出的变量 width : %f  height:%f",width,height);
    }else{
        printf("打印错误");
    }

    lua_close(L);
}

//Lua 执行 lua脚本,访问Lua 脚本中的表
void LuaRunScript2()
{
    lua_State * L = lua_open();//创建一个虚拟机的堆栈.
    luaopen_base(L);//必须打开基础库,否则任何函数都运行不了.
    luaL_dofile(L,"../p2.lua");//dofile 最后执行了 lua_pcall 所以下面不用执行lua_pcall了

    lua_settop(L,0);

    lua_getglobal(L,"window");
    lua_pushstring(L,"width");
    lua_gettable(L,-2);//栈底
    lua_Number width = lua_tonumber(L,-1);//栈顶
    printf("C语言:从 Lua 脚本中拿出的变量 height : %f \n",width);
    lua_settop(L,0);


    lua_getglobal(L,"window");
    lua_pushstring(L,"height");
    lua_gettable(L,-2);//栈底
    lua_Number height = lua_tonumber(L,-1);//栈顶
    printf("C语言:从 Lua 脚本中拿出的变量 height : %f \n",height);
    lua_settop(L,0);


    lua_close(L);
}


//Lua 执行 lua脚本,访问Lua 脚本中的函数
void LuaRunScript3()
{
    lua_State * L = lua_open();//创建一个虚拟机的堆栈.
    luaopen_base(L);//必须打开基础库,否则任何函数都运行不了.
    luaL_dofile(L,"../p2.lua");//dofile 最后执行了 lua_pcall 所以下面不用执行lua_pcall了

    lua_settop(L,0);

//    lua_getglobal(L,"getWidth");
//    lua_pcall(L,0,1,0);
//    lua_Number Width = lua_tonumber(L,-1);//栈顶
//    printf("C语言:从 Lua 脚本中拿出的变量 Width : %f \n",Width);
//    lua_settop(L,0);
//
//    lua_getglobal(L,"getHeight");
//    lua_pcall(L,0,1,0);
//    lua_Number Height = lua_tonumber(L,-1);//栈顶
//    printf("C语言:从 Lua 脚本中拿出的变量 Height : %f \n",Height);
//    lua_settop(L,0);


    lua_getglobal(L,"getTwo");
    lua_pcall(L,0,2,0);
    lua_Number Width = lua_tonumber(L,-2);
    lua_Number Height = lua_tonumber(L,-1);
    printf("C语言:从 Lua 脚本中拿出的变量 Width : %f  Height : %f \n",Width,Height);
    lua_settop(L,0);

    lua_close(L);
}


int main() {
//    LuaInterpreter();
//    LuaRunScript();
//    LuaRunScript1();
//    LuaRunScript2();
//    LuaRunScript3();


//    testLuaCallCpp1();
//    testLuaCallCpp2();
//    testLuaCallCpp3();
    testLuaCallCpp4();


    return 0;
}
