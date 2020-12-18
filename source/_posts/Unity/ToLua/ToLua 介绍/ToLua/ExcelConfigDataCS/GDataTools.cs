using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;

public class GData
{
#if !UNITY_EDITOR && UNITY_IPHONE
        const string LibName = "__Internal";
#else
    const string LibName = "tolua";
#endif

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern UInt16 ReadUInt16(IntPtr data, int offset);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern UInt32 ReadUInt32(IntPtr data, int offset);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 ReadSInt32(IntPtr data, int offset);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern UInt64 ReadUInt64(IntPtr data, int offset);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Int64 ReadSInt64(IntPtr data, int offset);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ReadBool(IntPtr data, int offset);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ReadString")]
    private static extern IntPtr ReadStringImpl(IntPtr data, int offset, ref int size);


    const int BUFFER_SIZE = 65536;
    static byte[] buffer = new byte[BUFFER_SIZE];

    // private static uint STRING_CACHE_CAPACITY = 64;
    // private static Best.LRUCache<Int64, string> m_cache = new Best.LRUCache<long, string>("GDataCache",STRING_CACHE_CAPACITY);
    // private static string tmp = null;
    public static string ReadString(IntPtr data, int offset)
    {
        /*
        Int64 k = data.ToInt64() + offset;
        if (m_cache.TryGet(k, out tmp))
        {
            return tmp;
        }
        */

        int size = 0;
        IntPtr str = ReadStringImpl(data, offset, ref size);
        if (str == IntPtr.Zero)
            return "";

        Array.Clear(buffer, 0, BUFFER_SIZE);
        Marshal.Copy(str, buffer, 0, size);
        string s = System.Text.Encoding.UTF8.GetString(buffer, 0, size);
        //m_cache.Add(k, s);
        return s;
    }

    /*
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetConfigPtr")]
    public static extern IntPtr GetConfigPtr_Impl(string config_name);

    public static IntPtr GetConfigPtr(string config_name)
    {
        if (!GDataCPPRelated.Started)
        {
            Debug.LogWarningFormat("GData: get {0} before game really start.", config_name);
        }

        return GetConfigPtr_Impl(config_name);
    }*/

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint GetEntryCount(string config_name);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetDataPtr_UInt32")]
    public static extern IntPtr GetDataPtr(string config_name, string key_name, UInt32 value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetDataPtr_SInt32")]
    public static extern IntPtr GetDataPtr(string config_name, string key_name, Int32 value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetDataPtr_String")]
    public static extern IntPtr GetDataPtr(string config_name, string key_name, string value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetDataPtrs_UInt32")]
    public static extern IntPtr GetDataPtrs(string config_name, string key_name, UInt32 value, ref int arraySize);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetDataPtrs_SInt32")]
    public static extern IntPtr GetDataPtrs(string config_name, string key_name, Int32 value, ref int arraySize);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetDataPtrs_String")]
    public static extern IntPtr GetDataPtrs(string config_name, string key_name, string value, ref int arraySize);

    /*
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetAllKeys_UInt32")]
    public static extern IntPtr GetAllKeys_UInt32(string config_name, string key_name, ref int arraySize);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetAllKeys_SInt32")]
    public static extern IntPtr GetAllKeys_SInt32(string config_name, string key_name, ref int arraySize);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetAllKeys_String")]
    public static extern IntPtr GetAllKeys_String(string config_name, string key_name, ref int arraySize);
    */

    /// <summary>
    /// C#获取配置二进制配置文件后，调用C++代码进行解析
    /// </summary>
    /// <param name="config_name"></param>
    /// <param name="value"></param>
    /// <param name="results"></param>
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void ParseConfigData(string config_name, bool merged, byte* data, int len);

    //数组最大长度为65536条数据. 如果有策划表条目数量超过65536, 此处需要扩容。
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetAllData")]
    public static extern unsafe void GetAllDataImpl(string config_name, [In, Out] IntPtr[] data, ref int len);

    private const int PINVOKE_ARRAY_LEN = 65536;
    private static IntPtr[] pInvoke_Array = new IntPtr[PINVOKE_ARRAY_LEN];
    public static unsafe IntPtr[] GetAllData(string config_name, ref int count)
    {
        Array.Clear(pInvoke_Array, 0, pInvoke_Array.Length);
        GetAllDataImpl(config_name, pInvoke_Array, ref count);
        return pInvoke_Array;
    }

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaopen_gdata")]
    public static extern IntPtr LuaOpen_GData(IntPtr L);

    public delegate void LogError(IntPtr msg);
    [AOT.MonoPInvokeCallback(typeof(LogError))]
    public static void LogImpl(IntPtr msg)
    {
        if (msg == IntPtr.Zero)
            return;

        byte[] bytes = System.Text.Encoding.Unicode.GetBytes(Marshal.PtrToStringUni(msg)); 
    }
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetLogFunc(LogError func);

    public delegate void ReadConfig(IntPtr configName);
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetReadConfigFunc(ReadConfig func);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public unsafe static extern void OnReadConfig(byte *bytes, int size);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Release")]
    public static extern void ReleaseImpl();

    public static void Release()
    {
        ReleaseImpl();
    }

    public const int ARRAY_COUNT_SIZE = 2;

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetVariableAttributePtr_CSharp")]
    public static extern IntPtr GetVariableAttributePtr(IntPtr dataPtr, int variableAttrIndex);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetRepeatedStructElementPtr_CSharp")]
    public static extern IntPtr GetRepeatedStructElementPtr(IntPtr repeatedStructPtr, int index);
}

public interface ConfigInit
{
    void Init(IntPtr p);
}

public struct UInt32Array : ConfigInit
{
    public int Count
    {
        get
        {
            if (m_data == IntPtr.Zero)
                return 0;
            return GData.ReadUInt16(m_data, 0);
        }
    }

    private IntPtr m_data;

    public void Init(IntPtr data)
    {
        m_data = data;
    }


    public UInt32 this[int i]
    {
        get
        {
            if(i < 0 || i >= Count)
            {
                //Debug.LogErrorFormat("数组越界！");
                return 0;
            }

            return GData.ReadUInt32(m_data, GData.ARRAY_COUNT_SIZE + (int)(i  * 4));
        }
    }

    public static implicit operator List<UInt32>(UInt32Array self)
    {
        List<UInt32> ret = new List<uint>();
        for(int i = 0; i < self.Count; i++)
        {
            ret.Add(self[i]);
        }
        return ret;
    }

}

public struct Int32Array : ConfigInit
{
    public int Count
    {
        get
        {
            if (m_data == IntPtr.Zero)
                return 0;
            return GData.ReadUInt16(m_data, 0);
        }
    }


    public void Init(IntPtr data)
    {
        m_data = data;
    }


    private IntPtr m_data;

    public Int32 this[int i]
    {
        get
        {
            if (i < 0 || i >= Count)
            {
                Debug.LogErrorFormat("数组越界！");
                return 0;
            }

            return GData.ReadSInt32(m_data, GData.ARRAY_COUNT_SIZE + (int)(i * 4));
        }
    }

    public static implicit operator List<Int32>(Int32Array self)
    {
        List<Int32> ret = new List<Int32>();
        for (int i = 0; i < self.Count; i++)
        {
            ret.Add(self[i]);
        }
        return ret;
    }
}

public struct UInt64Array : ConfigInit
{
    public int Count
    {
        get
        {
            if (m_data == IntPtr.Zero)
                return 0;
            return GData.ReadUInt16(m_data, 0);
        }
    }

    private IntPtr m_data;

    public void Init(IntPtr data)
    {
        m_data = data;
    }

    public UInt64 this[int i]
    {
        get
        {
            if(i < 0 || i >= Count)
            {
                Debug.LogErrorFormat("数组越界！");
                return 0;
            }

            return GData.ReadUInt64(m_data, GData.ARRAY_COUNT_SIZE + (int)(i  * 8));
        }
    }

    public static implicit operator List<UInt64>(UInt64Array self)
    {
        List<UInt64> ret = new List<UInt64>();
        for (int i = 0; i < self.Count; i++)
        {
            ret.Add(self[i]);
        }
        return ret;
    }
}

public struct Int64Array : ConfigInit
{
    public int Count
    {
        get
        {
            if (m_data == IntPtr.Zero)
                return 0;
            return GData.ReadUInt16(m_data, 0);
        }
    }

    private IntPtr m_data;

    public void Init(IntPtr data)
    {
        m_data = data;
    }

    public Int64 this[int i]
    {
        get
        {
            if (i < 0 || i >= Count)
            {
                Debug.LogErrorFormat("数组越界！");
                return 0;
            }

            return GData.ReadSInt64(m_data, GData.ARRAY_COUNT_SIZE + (int)(i * 8));
        }
    }

    public static implicit operator List<Int64>(Int64Array self)
    {
        List<Int64> ret = new List<Int64>();
        for (int i = 0; i < self.Count; i++)
        {
            ret.Add(self[i]);
        }
        return ret;
    }
}

public struct BoolArray : ConfigInit
{
    public int Count
    {
        get
        {
            if (m_data == IntPtr.Zero)
                return 0;
                 
            return GData.ReadUInt16(m_data, 0);
        }
    }

    private IntPtr m_data;

    public void Init(IntPtr data)
    {
        m_data = data;
    }

    public bool this[int i]
    {
        get
        {
            if (i < 0 || i >= Count)
            {
                Debug.LogErrorFormat("数组越界！");
                return false;
            }

            return GData.ReadBool(m_data, GData.ARRAY_COUNT_SIZE + (int)(i * 1));
        }
    }

    public static implicit operator List<bool>(BoolArray self)
    {
        List<bool> ret = new List<bool>();
        for (int i = 0; i < self.Count; i++)
        {
            ret.Add(self[i]);
        }
        return ret;
    }
}

public struct StringArray : ConfigInit
{
    public int Count
    {
        get
        {
            if (m_data == IntPtr.Zero)
                return 0;
            return GData.ReadUInt16(m_data, 0);
        }
    }

    private IntPtr m_data;

    public void Init(IntPtr data)
    {
        m_data = data;
    }

    public string this[int i]
    {
        get
        {
            if (i < 0 || i >= Count)
            {
                Debug.LogErrorFormat("数组越界！");
                return null;
            }

            return GData.ReadString(m_data, GData.ARRAY_COUNT_SIZE + (int)(i * 4));
        }
    }

    public static implicit operator List<string>(StringArray self)
    {
        List<string> ret = new List<string>();
        for (int i = 0; i < self.Count; i++)
        {
            ret.Add(self[i]);
        }
        return ret;
    }
}

public struct StructArray<TStruct> where TStruct : struct, ConfigInit
{
    public int Count
    {
        get
        {
            if (m_data == IntPtr.Zero)
                return 0;
            return GData.ReadUInt16(m_data, 0);
        }
    }

    private IntPtr m_data;

    public void Init(IntPtr data)
    {
        m_data = data;
    }

    private static TStruct DummyObj = new TStruct();
    public TStruct this[int i]
    {
        get
        {
            if(i < 0 || i >= Count)
            {
                Debug.LogErrorFormat("数组越界！");
                return DummyObj; 
            }

            TStruct ret = new TStruct();
            ret.Init(GData.GetRepeatedStructElementPtr(m_data, i));
            return ret;
        }
    }
}
///C# - C++ communication


public static class GDataCPPRelated
{
    const string AllConfigBinaryListFileName = "gdata_bytes_list";

    public static bool LazyLoad
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return SystemInfo.systemMemorySize <= 2048;
#elif UNITY_IOS && !UNITY_EDITOR
            return SystemInfo.systemMemorySize <= 1024;
#else
            return true;
#endif
        }
    }

    public static bool Started { get; set; }

    private static void LoadAll()
    {
        byte[] allFileList = ReadConfigFile(AllConfigBinaryListFileName);
        if (allFileList == null)
        {
            Debug.LogErrorFormat("Cannot ReadConfigFile: {0}", AllConfigBinaryListFileName);
            return;
        }

        string fileListStr = System.Text.Encoding.ASCII.GetString(allFileList);
        string[] fileNameArray = fileListStr.Split('\n');
        if (fileNameArray.Length == 0)
        {
            fileNameArray = new string[1];
            fileNameArray[0] = fileListStr;
        }

        bool merged = fileNameArray[0] == "True";

        uint parsedCount = 0;
        int parsedSize = 0;
        float t = Time.realtimeSinceStartup;
        char[] charsToTrim = { '\r' };
        for (int i = 1; i < fileNameArray.Length; i++)
        {
            string fileName = fileNameArray[i];
            fileName = fileName.Split('.')[0];
            if (fileName.Length == 0)
            {
                continue;
            }

            fileName = fileName.Trim(charsToTrim);
            byte[] data = ReadConfigFile(fileName);
            if (data == null)
            {
                Debug.LogErrorFormat("no config file: {0}", fileName);
                continue;
            }
            if (data.Length == 0)
            {
                //Debug.LogWarningFormat("Empty config file: {0}", fileName);
                continue;
            }
            int size = data.Length;
            parsedSize += size;
            parsedCount++;
            unsafe
            {
                fixed (byte* pData = &data[0])
                {
                    GData.ParseConfigData(fileName, merged, pData, size);
                }
            }
        }
    }

    public static void OnGameStart()
    {
        if (Started)
            return;

        Started = true;

        GData.Release();//因为资源热更新，所以需要在游戏真正开始之前重新加载配置表。所以逻辑层需注意，对于在OnGameStart之前加载的配置表对象，千万不要保存引用（因为其中的指针会失效）。
        SetFuncs();

        if (!LazyLoad)
        {
            LoadAll();
        }
    }

    private static void SetFuncs()
    {
        GData.SetLogFunc(GData.LogImpl);
        GData.SetReadConfigFunc(CPPLazyLoadConfig);
    }

    public static void Init()
    {

#if UNITY_EDITOR
        GData.Release();//编辑器下 需要先释放
#endif
        SetFuncs();

#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            LoadAll();
        }
#endif
    }

    [AOT.MonoPInvokeCallback(typeof(GData.ReadConfig))]
    private static void CPPLazyLoadConfig(IntPtr name)
    {
        if (name == null || name == IntPtr.Zero)
            return;

        byte[] bytes = System.Text.Encoding.Unicode.GetBytes(Marshal.PtrToStringUni(name)); 
        string configName = System.Text.Encoding.UTF8.GetString(bytes);
        bytes = ReadConfigFile(configName);
        unsafe
        {
            if (bytes != null && bytes.Length > 0)
            {
                fixed (byte* p = &bytes[0])
                {
                    GData.OnReadConfig(p, bytes.Length);

                    // if (!BuildManifestUtility.GetBuildManifest().IsReleaseVer)
                    //     LogModule.Instance.Trace(LogModule.LogModuleCode.GDataLog, string.Format("GData lazy parse : {0}", configName));
                }
            }

        }
    }

    private static byte[] ReadConfigFile(string fileName)
    {
        byte[] streamBytes = null;
        // string countryName = LocalizationMgr.Instance().curLocalizationInfo.LocalizationTag;
        // if (BuildManifestUtility.GetBuildManifest().IsSmallServer)
        // {
        //     streamBytes = FileUtils.LoadConfigFile("ProtobufDataConfigsSmall/" + countryName + "/" + fileName, "bytes");
        //     if (streamBytes == null)
        //     {
        //         streamBytes = FileUtils.LoadConfigFile("ProtobufDataConfigs/" + countryName + "/" + fileName, "bytes");
        //         if (streamBytes == null)
        //         {
        //             Debug.LogWarningFormat("can not find config:" + "ProtobufDataConfigs/" + countryName + "/" + fileName, "bytes");
        //         }
        //     }
        // }
        // else
        // {
        //     streamBytes = FileUtils.LoadConfigFile("ProtobufDataConfigs/" + countryName + "/" + fileName, "bytes");
        // }

        return streamBytes;
    }
}
