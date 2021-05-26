/*
Copyright (c) 2015-2017 topameng(topameng@qq.com)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using UnityEngine;
using LuaInterface;
using System.IO;
using System;
using UnityEngine.SceneManagement;


public abstract class LuaClient : MonoBehaviour
{
    public static LuaClient luaInstance
    {
        get;
        protected set;
    }

    protected LuaState luaState = null;
    protected LuaLooper loop = null;
    protected LuaFunction onSocketfunc = null;
    protected LuaFunction onAppPause = null;

    protected bool openLuaSocket = false;
    protected bool beZbStart = false;

    protected virtual LuaFileUtils InitLoader()
    {
        return LuaFileUtils.Instance;       
    }

    protected virtual void LoadLuaFiles()
    {
        OnLoadFinished();
    }

    protected virtual void OpenLibs()
    {
        luaState.OpenLibs(LuaDLL.luaopen_pb);
        luaState.OpenLibs(LuaDLL.luaopen_struct);
        luaState.OpenLibs(LuaDLL.luaopen_lpeg);
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        luaState.OpenLibs(LuaDLL.luaopen_bit);
#endif
        OpenLuaSocket();            
        OpenZbsDebugger();

    }

    public void OpenZbsDebugger(string ip = "localhost")
    {
        if (!Directory.Exists(LuaConst.zbsDir))
        {
            Debugger.LogWarning("ZeroBraneStudio not install or LuaConst.zbsDir not right");
            return;
        }

        if (!string.IsNullOrEmpty(LuaConst.zbsDir))
        {
            luaState.AddSearchPath(LuaConst.zbsDir);
        }

        luaState.LuaDoString(string.Format("DebugServerIp = '{0}'", ip), "@LuaClient.cs");
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int LuaOpen_Socket_Core(IntPtr L)
    {        
        return LuaDLL.luaopen_socket_core(L);
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int LuaOpen_Mime_Core(IntPtr L)
    {
        return LuaDLL.luaopen_mime_core(L);
    }

    protected void OpenLuaSocket()
    {

        luaState.BeginPreLoad();
        luaState.RegFunction("socket.core", LuaOpen_Socket_Core);
        luaState.RegFunction("mime.core", LuaOpen_Mime_Core);                
        luaState.EndPreLoad();                     
    }

    //cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
    protected void OpenCJson()
    {
        luaState.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
        luaState.OpenLibs(LuaDLL.luaopen_cjson);
        luaState.LuaSetField(-2, "cjson");

        luaState.OpenLibs(LuaDLL.luaopen_cjson_safe);
        luaState.LuaSetField(-2, "cjson.safe");                               
    }

    protected virtual void CallMain()
    {
        LuaFunction main = luaState.GetFunction("Main");
        main.BeginPCall();
        main.PCall();
        main.EndPCall();
        main.Dispose();
        main = null;               
    }

    protected virtual void StartMain()
    {
        luaState.DoFile("Main.lua");
        onSocketfunc = luaState.GetFunction("NetworkMgr.OnSocket");
        onAppPause = luaState.GetFunction("OnApplicationPause");
        CallMain();
    }

    protected void StartLooper()
    {
        loop = gameObject.AddComponent<LuaLooper>();
        loop.luaState = luaState;
    }

    protected virtual void Bind()
    {        
        LuaBinder.Bind(luaState);
        DelegateFactory.Init();   
        LuaCoroutine.Register(luaState, this);        
    }
    
    private void Init()
    {        
        GDataCPPRelated.Started = false;
        GDataCPPRelated.OnGameStart();
        InitLoader();
        luaState = new LuaState();
        OpenLibs();
        luaState.LuaSetTop(0);
        Bind();        
        LoadLuaFiles();        
    }

    protected void DoInit()
    {
        luaInstance = this;
        Init();
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    protected virtual void OnApplicationPause(bool bPaused)
    {
        if (onAppPause != null)
        {
            onAppPause.BeginPCall();
            onAppPause.Push(bPaused);
            onAppPause.PCall();
            onAppPause.EndPCall();
            onAppPause.Dispose();
        }
    }

    protected virtual void OnLoadFinished()
    {
        luaState.Start();
        StartLooper();
        StartMain();        
    }

    void OnLevelLoaded(int level)
    {
        if (luaState != null)
        {            
            luaState.RefreshDelegateMap();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnLevelLoaded(scene.buildIndex);
    }


    public virtual void Destroy()
    {
        if (luaState == null) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        luaState.Call("OnApplicationQuit", false);
        DetachProfiler();
        LuaState state = luaState;
        luaState = null;

        if (onSocketfunc != null)
        {
            onSocketfunc.Dispose();
            onSocketfunc = null;
        }

        if (loop != null)
        {
            loop.Destroy();
            loop = null;
        }

        state.Dispose();
        luaInstance = null;
    }

    protected void OnDestroy()
    {
        Destroy();
    }

    protected void OnApplicationQuit()
    {
        Destroy();
    }

    public static LuaState GetMainState()
    {
        return luaInstance.luaState;
    }
    
    public LuaFunction GetOnSocketFunc()
    {
        return onSocketfunc;
    }
    public LuaLooper GetLooper()
    {
        return loop;
    }

    LuaTable profiler = null;

    public void AttachProfiler()
    {
        if (profiler == null)
        {
            profiler = luaState.Require<LuaTable>("UnityEngine.Profiler");
            profiler.Call("start", profiler);
        }
    }
    public void DetachProfiler()
    {
        if (profiler != null)
        {
            profiler.Call("stop", profiler);
            profiler.Dispose();
            LuaProfiler.Clear();
        }
    }
}
