using UnityEngine;

public static class LuaConst
{
    
#if UNITY_EDITOR//下面都是在开发时使用到的文件夹
    public static string luaDir = Application.dataPath + "/GameLogic/Lua";//lua逻辑代码目录
    public static string toluaDir = Application.dataPath + "/ThirdPartyLibraries/ToLua/Lua";//tolua lua文件目录
#endif
    

#if UNITY_STANDALONE
    public static string osDir = "Win";
#elif UNITY_ANDROID
    public static string osDir = "Android";            
#elif UNITY_IPHONE
    public static string osDir = "iOS";        
#else
    public static string osDir = "";        
#endif
    
    public static string luaResDir = string.Format("{0}/{1}/Lua", Application.persistentDataPath, osDir);      //手机运行时lua文件下载目录    

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN    
    public static string zbsDir = "D:/ZeroBraneStudio/lualibs/mobdebug";        //ZeroBraneStudio目录       
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	public static string zbsDir = "/Applications/ZeroBraneStudio.app/Contents/ZeroBraneStudio/lualibs/mobdebug";
#else
    public static string zbsDir = luaResDir + "/mobdebug/";
#endif
}
