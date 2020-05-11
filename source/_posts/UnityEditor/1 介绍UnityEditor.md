---
title: 1 介绍UnityEditor
date: 2020-05-11 11:41:32
categories:
- UnityEditor
tags:
- UnityEditor
---

# 第一篇 UnityEditor


* 1:编写游戏快捷工具,使游戏的产出更高效,包括不限于(可视化蓝图,打包流程,服务器辅助工具,音视频,优化等等)
* 2:[官网](https://docs.unity3d.com/Manual),自行搜索unityeditor即可
* 3:已知的UI编写方式有4种.第一种是运行时的UI.这个不再此讨论. ***第二种*** 是MonoBehaviour里面的特性Attribute编辑,可以在脚本的检视面板上面有较简单的展示,一般情况下足够使用了.
 ***第三种*** 就是当第二种的特性不满足了,需要在Editor里面单独写一个编辑脚本,这个时候需要自己进行布局排列,涉及到了EditorGUILayout/EditorGUI等的使用,使用 
```
CustomEditor(typeof(xxx)) 
public class xxx:Editor{
     //在这个里面进行布局操作
     public override void OnInspectorGUI(){}
}
```
 ***第四种*** 就是自己生成一个window面板,编写一个类似动画状态机的工具让开发人员使用,开发比较复杂的工具时,比如行为树,是需要强大的数据结构作为支撑的,涉及到了GUI/GUILayout等的使用.这一点需要好好学习.
```
public class xxx : EditorWindow 
{
    //这个地方需要添加一个可以将这个window展示出来的标签
    [MenuItem("Tools/Bug Reporter")]
    static void showWindow()
    {
        EditorWindow.GetWindow(typeof(xxx));
    }
    //在这个方法里面进行UI的展示操作
    private void OnGUI()
    {
        
    }
}
```

EditorGUILayout与GUILayout区别基本区别就是EditorGUILayout每一个UI组件都有一个前缀名字,而GUILayout时没有的,要想提示更丰满一般采用EditorGUILayout,比如
```
GUILayout.TextField("bug name");
string bugName;
bugName = EditorGUILayout.TextField("bug name", bugName);
```

---
# 特殊文件夹


* 1:Editor 文件夹 即项目路径: project/Assets/Editor,也可以随意在Assets/下面的任一文件夹下写入Editor/都会被unity识别
* 2:Editor文件夹是为了使用编辑API的特别的文件夹。通常编辑API不能在运行时操作。
* 3: Assembly-CSharp.dll 中不能 使用 UnityEditor.dll
* 4:UnityEditor会被unity编译成Assembly-CSharp-Editor.dll
* 5:在「Standard Assets」「Pro Standard Assets」「Plugins」生成Editor文件夹,则会被unity编译成 Assembly-CSharp-Editor-firstpass.dll 类库(一般不要使用)
* 6:尽量让 Editor 的模块引用运行时模块代码,不要用运行时模块引用Editor模块代码
* 7:如果不在Editor中编写的脚本想使用Editor模块的代码,需要使用unity的宏编译,使用了宏编译,运行时会自动将宏编译以及宏编译内的代码移除

```
    using UnityEngine;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    public class NewBehaviourScript : MonoBehaviour
    {
        void OnEnable ()
        {
            #if UNITY_EDITOR
            EditorWindow.GetWindow<ExampleWindow> ();
            #endif
        }
}
```

* 8:Editor Default Resources 文件夹,存放只有Editor模块可以使用的资源,类似于Resources文件夹,可以使用如下代码快速获取assets资源,这个文件夹下的资源不会被运行时使用(***这里所有的运行时,同一指打包后的app使用的脚本或者资源***)

```
    var tex = EditorGUIUtility.Load ("logo.png") as Texture;
```

* 9:查看所有内置资源

```
        [MenuItem("Tools/Test1")]
        static void GetBultinAssetNames()
        {
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var info = typeof(EditorGUIUtility).GetMethod("GetEditorAssetBundle", flags);
            var bundle = info.Invoke(null, new object[0]) as AssetBundle;

            foreach (var n in bundle.GetAllAssetNames())
            {
                Debug.Log(n);
            }
        }
```


---