# UnityEditor概览


* 1:编写游戏快捷工具,使游戏的产出更高效,包括不限于(可视化蓝图,打包流程,服务器辅助工具,音视频,优化等等)
* 2:[官网](https://docs.unity3d.com/Manual),自行搜索unityeditor即可
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