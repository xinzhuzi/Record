cmake_minimum_required(VERSION 3.17)
project(Lua_C_Invoke)

set(CMAKE_CXX_STANDARD 20)

link_directories(Lua)

add_executable(Lua_C_Invoke main.cpp LuaCallCPP.cpp LuaCallCPP.h)

target_link_libraries(Lua_C_Invoke liblua.a)
