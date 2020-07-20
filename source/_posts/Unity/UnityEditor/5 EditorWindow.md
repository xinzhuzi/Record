---
title: 5 EditorWindow
date: 2020-05-11 11:41:32
top: 5
categories:
- Unity
tags:
- UnityEditor
---
# EditorWindow 编辑器窗口

* 1:介绍,unity中的场景窗口、游戏窗口、电影窗口等，所有这些都是EditorWindow。Unity编辑是集合了具有各种功能的editor窗而成的。

# 编写弹出窗口代码

* 1:第一种写法弹出窗口
```
    public class TestEditorWindow : EditorWindow
    {
        static TestEditorWindow testWindow;

        [MenuItem("Tools/Example")]
        static void Open()
        {
            if (testWindow == null)
            {
                testWindow = CreateInstance<TestEditorWindow>();
            }
            testWindow.Show();
        }
    }
```

* 2:第二种写法,GetWindow方法是上面写的一堆API的集合,并且缓存了一个 TestEditorWindow 再次获取并不会重新生成一个
```
    public class TestEditorWindow : EditorWindow
    {
        static TestEditorWindow testWindow;

        [MenuItem("Tools/Example")]
        static void Open()
        {
            GetWindow<TestEditorWindow>();
        }
    }
```
* 3: EditorWindow.ShowUtility 工具窗口将总是在标准窗口的前面，并在用户切换另一个应用程序时隐藏。
```
        if (exampleWindow == null) {
            exampleWindow = CreateInstance<Example> ();
        }
        exampleWindow.ShowUtility ();
```
* 4:  EditorWindow.ShowPopup 没有关闭按钮,按下esc键关闭
```
    public class TestEditorWindow : EditorWindow
    {
        static TestEditorWindow testWindow;

        [MenuItem("Tools/Example")]
        static void Open()
        {
            
            if (testWindow == null)
            {
                testWindow = CreateInstance<TestEditorWindow>();
            }
            testWindow.ShowPopup();
        }


        void OnGUI()
        {
            if (Event.current.keyCode == KeyCode.Escape)
            {
                testWindow.Close();
            }
        }
    }
```
* 5: PopupWindow,在Editor中再弹出一个窗口
```
    public class TestPopupWindow : EditorWindow
    {
        static TestPopupWindow testWindow;
        [MenuItem("Tools/Pop")]
        static void Open()
        {
            GetWindow<TestPopupWindow>();
        }

        ExamplePupupContent popupContent = new ExamplePupupContent();

        void OnGUI()
        {
            if (GUILayout.Button("PopupContent", GUILayout.Width(128)))
            {
                var activatorRect = GUILayoutUtility.GetLastRect();
                PopupWindow.Show(activatorRect, popupContent);
            }
        }
    }

    public class ExamplePupupContent : PopupWindowContent
    {
        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.LabelField("Lebel");
        }

        public override void OnOpen()
        {
            Debug.Log("打开pop窗口");
        }

        public override void OnClose()
        {
            Debug.Log("关闭pop窗口");
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 100);
        }
    }
```
* 6:点击其他区域会关闭
```
    public class Example : EditorWindow
    {
        static Example exampleWindow;

        [MenuItem("Tools/Drop")]
        static void Open()
        {
            if (exampleWindow == null)
            {
                exampleWindow = CreateInstance<Example>();
            }

            var buttonRect = new Rect(100, 100, 300, 100);
            var windowSize = new Vector2(300, 100);
            exampleWindow.ShowAsDropDown(buttonRect, windowSize);
        }
    }
```
* 7:ScriptableWizard简单制作
```

public class Example1 : ScriptableWizard
{
    [MenuItem("Tools/ScriptableWizard")]
    static void Open()
    {
        DisplayWizard<Example1>("Example Wizard");
    }

    void OnWizardCreate()
    {
        new GameObject("gameObjectName");
    }
    void OnWizardUpdate()
    {
        Debug.Log("Update");
    }
    protected override bool DrawWizardGUI()
    {
        EditorGUILayout.LabelField("Label");
        return true;
    }
    void OnWizardOtherButton()
    {
        var gameObject = GameObject.Find("gameObjectName");

        if (gameObject == null)
        {
            Debug.Log("找不到");
        }
    }
}
```
* 8:PreferenceItem是用于在Unity Preferences中添加菜单的功能。Unity Preferences是为了对Unity编辑产生影响的设定。
```
    using UnityEditor;

    public class Example
    {
        [PreferenceItem("Example")]
        static void OnPreferenceGUI ()
        {

        }
    }
```
* 9:添加菜单的IHasCustomMenu
```
    using UnityEditor;
    using UnityEngine;

    public class Example : EditorWindow, IHasCustomMenu
    {

        public void AddItemsToMenu (GenericMenu menu)
        {
            menu.AddItem (new GUIContent ("example"), false, () => {

            });

            menu.AddItem (new GUIContent ("example2"), true, () => {

            });
        }

        [MenuItem("Window/Example")]
        static void Open ()
        {
            GetWindow<Example> ();
        }
    }
```
* 10:重设图标和title
```
    using UnityEditor;
    using UnityEngine;

    public class Example : EditorWindow
    {
        [MenuItem ("Window/Example")]
        static void SaveEditorWindow ()
        {
            var window = GetWindow<Example> ();

            var icon = AssetDatabase.LoadAssetAtPath<Texture> ("Assets/Editor/Example.png");

            window.titleContent = new GUIContent ("Hoge", icon);
        }
    }
```
* 11:将EditorWindow中的数据保存为ScriptableObject 
```
    using UnityEditor;
    using UnityEngine;
    public class Example : EditorWindow
    {
        [MenuItem ("Assets/Save EditorWindow")]
        static void SaveEditorWindow ()
        {
            AssetDatabase.CreateAsset (CreateInstance<Example> (), "Assets/Example.asset");
            AssetDatabase.Refresh ();
        }

        [SerializeField]
        string text;

        [SerializeField]
        bool boolean;
    }
```