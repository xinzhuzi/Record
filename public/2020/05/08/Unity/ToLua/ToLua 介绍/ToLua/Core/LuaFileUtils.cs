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
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Text;

namespace LuaInterface
{
    /// <summary>
    /// Lua 记录加载的文件,从某一个文件夹内或者从某一个 AssetBundle 内
    /// 一般情况下不用 AssetBundle,还要 load,加载数据量过多时,造成性能问题.
    /// </summary>
    public class LuaFileUtils
    {
        public static LuaFileUtils Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LuaFileUtils();
                }
                return instance;
            }
            protected set
            {
                instance = value;
            }
        }
        
        protected List<string> searchPaths = new List<string>();
        protected Dictionary<string, AssetBundle> zipMap = new Dictionary<string, AssetBundle>();

        protected static LuaFileUtils instance = null;

        public LuaFileUtils()
        {
            instance = this;
        }

        public virtual void Dispose()
        {
            if (instance != null)
            {
                instance = null;
                searchPaths.Clear();

                foreach (KeyValuePair<string, AssetBundle> iter in zipMap)
                {
                    iter.Value.Unload(true);
                }

                zipMap.Clear();
            }
        }

        //格式: 路径/?.lua,添加搜索路径
        public bool AddSearchPath(string path, bool front = false)
        {
            int index = searchPaths.IndexOf(path);

            if (index >= 0)
            {
                return false;
            }

            if (front)
            {
                searchPaths.Insert(0, path);
            }
            else
            {
                searchPaths.Add(path);
            }

            return true;
        }
        
        //删除搜索路径
        public bool RemoveSearchPath(string path)
        {
            int index = searchPaths.IndexOf(path);

            if (index >= 0)
            {
                searchPaths.RemoveAt(index);
                return true;
            }

            return false;
        }
        
        //添加搜索 AB 包
        public void AddSearchBundle(string name, AssetBundle bundle)
        {
            zipMap[name] = bundle;
        }
        
        //在文件路径中查找 lua 文件
        public string FindFile(string fileName)
        {
            if (fileName == string.Empty)
            {
                return string.Empty;
            }

            if (Path.IsPathRooted(fileName))
            {
                if (!fileName.EndsWith(".lua"))
                {
                    fileName += ".lua";
                }

                return fileName;
            }

            if (fileName.EndsWith(".lua"))
            {
                fileName = fileName.Substring(0, fileName.Length - 4);
            }

            string fullPath = null;

            for (int i = 0; i < searchPaths.Count; i++)
            {
                fullPath = searchPaths[i].Replace("?", fileName);

                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return null;
        }
        
        //读取lua 文件为二进制
        public virtual byte[] ReadFile(string fileName)
        {
            string path = FindFile(fileName);
            byte[] str = null;

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                str = File.ReadAllBytes(path);
            }

            return str;
        }

        //
        public virtual string FindFileError(string fileName)
        {
            if (Path.IsPathRooted(fileName))
            {
                return fileName;
            }

            if (fileName.EndsWith(".lua"))
            {
                fileName = fileName.Substring(0, fileName.Length - 4);
            }

            using (CString.Block())
            {
                CString sb = CString.Alloc(512);

                for (int i = 0; i < searchPaths.Count; i++)
                {
                    sb.Append("\n\tno file '").Append(searchPaths[i]).Append('\'');
                }

                sb = sb.Replace("?", fileName);

                return sb.ToString();
            }
        }

        public static string GetOSDir()
        {
            return LuaConst.osDir;
        }
    }
}
