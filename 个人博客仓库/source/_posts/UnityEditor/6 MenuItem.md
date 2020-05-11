---
title: 1 介绍UnityEditor
date: 2020-05-11 11:41:32
categories:
- UnityEditor
tags:
- UnityEditor
---
# MenuItem

* 1:是在Unity editer上侧的菜单栏或上下文菜单上追加项目所需的功能.
* 2:放在编辑器下面的静态方法上面,priority参数指定顺序,值越小越显示再上面
* 3:让menu item不生效,再unity里面查看区别
```
    using UnityEditor;

    public class EditorTest2 : Editor
    {
        [MenuItem("CustomMenu/Example/Child1")]
        static void GetBultinAssetNames()
        {
        }

        [MenuItem("CustomMenu/Example/Child2")]
        static void Example2()
        {

        }

        [MenuItem("CustomMenu/Example/Child2", true)]
        static bool ValidateExample2()
        {
            return false;
        }
    }
```
* 4:在菜单前面有个对号,后面 %#g 表示快捷键,百度查一下即可
```
    [MenuItem("CustomMenu/Example  %#g")]
    static void Example()
    {
        var menuPath = "CustomMenu/Example";
        var _checked = Menu.GetChecked(menuPath);
        Menu.SetChecked(menuPath, !_checked);
    }
```
* 5:CONTEXT/组件名/(自定义的)组件方法名字  
```
    using UnityEditor;

    public class NewBehaviourScript
    {
        [MenuItem("CONTEXT/Transform/Example1")]
        static void Example1 () { }

        [MenuItem("CONTEXT/Component/Example2")]
        static void Example2 () { }

        [MenuItem("CONTEXT/Transform/Example3")]
        static void Example1 (MenuCommand menuCommand)
        {
            //実行した Transform の情報が取得できる
            Debug.Log (menuCommand.context);
        }
    }
}
