#include "LuaCallCPP.h"


void testLuaCallCpp1()
{
    int width = 100;
    int height = 200;
    lua_State *L = lua_open();
    luaopen_base(L);

    lua_settop(L,0);

    {//向 Lua 脚本里面塞入全局变量a
        lua_pushnumber(L,width);
        lua_setglobal(L,"global_a");
        lua_pushnumber(L,height);
        lua_setglobal(L,"global_b");
//        luaL_dofile(L,"../p1.lua");
    }

    {//创建一个表,向表里面塞入变量,在 L
        lua_newtable(L);
        lua_pushstring(L,"width");
        lua_pushnumber(L,width);
        lua_settable(L,-3);//key 和 value 已出栈

        lua_pushstring(L,"height");
        lua_pushnumber(L,height);
        lua_settable(L,-3);//key 和 value 已出栈

        //把 table 注册到 Lua 里面
        lua_setglobal(L,"CTable");
        luaL_dofile(L,"../p1.lua");
    }

    lua_settop(L,0);


    lua_close(L);
}


int LuaCallCppxxxx(lua_State * L)
{
    lua_pushnumber(L,111);
    return 1;//表示在 Lua 脚本里面调用的时候有 0 个返回值
}

int LuaCallCppxxxx1(lua_State * L)
{
    lua_pushnumber(L,222);
    return 1;//表示在 Lua 脚本里面调用的时候有 0 个返回值
}

int LuaCallCppxxxx2(lua_State * L)
{
    lua_Number a = lua_tonumber(L,1);
    printf("Cpp 语言: 获取到 Lua 传入的值为:%f \n",a );

    lua_pushnumber(L,100*a);
    lua_pushnumber(L,200*a);

    return 2;//表示在 Lua 脚本里面调用的时候有 0 个返回值
}

//在 Lua 脚本里面调用 CPP 的方法
void testLuaCallCpp2()
{
    lua_State *L = lua_open();
    luaopen_base(L);
    lua_settop(L,0);

    {
        lua_pushcfunction(L,LuaCallCppxxxx);
        lua_setglobal(L,"LuaCallCppxxxx");
        lua_pushcfunction(L,LuaCallCppxxxx1);
        lua_setglobal(L,"LuaCallCppxxxx1");
        lua_pushcfunction(L,LuaCallCppxxxx2);
        lua_setglobal(L,"LuaCallCppxxxx2");
        luaL_dofile(L,"../p3.lua");
    }

    lua_settop(L,0);


    lua_close(L);
}

static const luaL_reg LuaCallCppLib[] = {
        {"LuaCallCppxxxx",LuaCallCppxxxx},
        {"LuaCallCppxxxx1",LuaCallCppxxxx1},
        {"LuaCallCppxxxx2",LuaCallCppxxxx2},
        {NULL,NULL},
};


//在 Lua 脚本里面调用 CPP 的方法,定义一个模块的方法
void testLuaCallCpp3()
{
    lua_State *L = lua_open();
    luaopen_base(L);
    lua_settop(L,0);

    {
        luaL_openlib(L,"LuaCallCppLib",LuaCallCppLib,0);
        luaL_dofile(L,"../p4.lua");
    }

    lua_settop(L,0);


    lua_close(L);
}


typedef struct STR_WIN {
    int winth;
    int height;
}STR_WIN;

int getWinWinth(lua_State * L)
{
    STR_WIN * p1 = (STR_WIN *)lua_touserdata(L,-1);
    if (p1!=NULL)
    {
        lua_pushnumber(L,p1->height);
        lua_pushnumber(L,p1->winth);
    }
    printf("CPP测试 getWinWinth  height %d   winth %d\n",p1->height,p1->winth);
    return 2;
}

//向 Lua 脚本里面传入 userdata 变量
void testLuaCallCpp4()
{
    lua_State *L = lua_open();
    luaopen_base(L);
    lua_settop(L,0);

    {
//        STR_WIN * p = new STR_WIN();
//        p->height = 900;
//        p->winth = 400;
        //在Lua 堆栈中分配一个sizeof(STR_WIN)大小的字节空间,并将这个userdata入栈,返回一个指向这个空间大小的指针
        STR_WIN * p1 = (STR_WIN *)lua_newuserdata(L,sizeof(STR_WIN));
        p1->height = 1900;
        p1->winth = 1400;

        lua_setglobal(L,"win");//已经注册了 userdata,在 Lua 代码中已经可以使用了.但是 userdata 里面的变量还不可以使用
        lua_pushcfunction(L,getWinWinth);
        lua_setglobal(L,"getWinWinth");//通过方法在 Lua 代码里面进行调用


        luaL_dofile(L,"../userdata.lua");
    }


    lua_settop(L,0);


    lua_close(L);
}