using UnityEngine;
using LuaInterface;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LuaInterface
{
    //查看当前有多少 lua 中使用 C# 的对象
    public class LuaSnapShot
    {
        private class CompareObject : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y)
            {
                return object.ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }

        HashSet<object> snapShot0 = new HashSet<object>(new CompareObject());
        HashSet<object> snapShot1 = new HashSet<object>(new CompareObject());

        LuaState L = null;
        LuaFunction find = null;

        public LuaSnapShot(LuaState state)
        {
            L = state;
            find = state.GetFunction("FindObjectInGlobal");
        }

        public void Destroy()
        {
            snapShot0 = null;
            snapShot1 = null;
            L = null;
            find.Dispose();
            find = null;
        }

        public void BeginSnapShot()
        {   
            foreach (var iter in L.translator.objectsBackMap)
            {
                if (iter.Key != null)
                {
                    snapShot0.Add(iter.Key);
                }
            }
        }

        public void EndSnapShot()
        {
            if (snapShot0.Count == 0)
            {
                return;
            }

            L.DoString("collectgarbage('collect')");
            
            foreach (var iter in L.translator.objectsBackMap)
            {
                if (iter.Key != null && !snapShot0.Contains(iter.Key) && L.translator.GetObject(iter.Value) != null)
                {
                    snapShot1.Add(iter.Key);
                }
            }

            snapShot0.Clear();            

            foreach(var iter in snapShot1)
            {
                find.Call(iter);                
            }

            snapShot1.Clear();
        }
    }
}
