#include <iostream>


extern "C"
{
    #include "lua.h"
    #include "lauxlib.h"
    #include "lualib.h"
}


int main(int argc, const char * argv[]) {
    
    lua_State * L = lua_open();

    lua_close(L);
    
    // insert code here...
    std::cout << "Hello, World!\n";
    return 0;
}
