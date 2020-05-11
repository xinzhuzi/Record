---
title: 1 介绍UnityEditor
date: 2020-05-11 11:41:32
categories:
- UnityEditor
tags:
- UnityEditor
---
# 使用Editor编写一个window面板

* 1:上面可以简单的制作一个window面板的编辑器,一些方法的使用需要看[EditorGUI方法介绍](https://github.com/BingJin-Zheng/Record/blob/master/Unity_Editor/4%20EditorGUI.md)
* 2:编写一个 bug保存到本地 的window面板
```
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BugReportWindow : EditorWindow
{

    Vector2 v2 = new Vector2(0, 0);
    int v = 0;
    string[] Messages = { "Message1", "Message2", "Message3", "Message4" };

    string bugName;

    GameObject bugGameObject;

    string content;

    private bool groupEnabled; //区域开关

    float minVal = -10.0f;
    float minLimit = -20.0f;
    float maxVal = 10.0f;
    float maxLimit = 20.0f;
    string[] options = { "Cube", "Sphere", "Plane" };
    int index = 0;
    int index1 = 0;

    string path;



    [MenuItem("Tools/Bug Reporter")]
    static void CreateWindow()
    {
        GetWindow<BugReportWindow>("Bug Reporter",true);
    }

    /// <summary>
    /// 这个地方和编辑运行时的UI代码不一样
    /// 编辑器UI和运行时UI相比还是比较简单一些的
    /// 这个地方采用的是GUILayout
    /// </summary>
    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(10);


        //绘制标题
        GUI.skin.label.fontSize = 24;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Bug Reporter");

        GUILayout.Space(10);

        //绘制文本
        bugName = EditorGUILayout.TextField("bug name:", bugName);

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        //绘制当前正在编辑的场景
        GUI.skin.label.fontSize = 12;
        GUI.skin.label.alignment = TextAnchor.UpperLeft;
        GUILayout.Label("当前场景:" + EditorSceneManager.GetActiveScene().name);

        //绘制当前时间
        GUILayout.Label("当前时间:" + System.DateTime.Now);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        //绘制对象,这个地方要允许可以选择当前场景的物体
        bugGameObject = EditorGUILayout.ObjectField("bug game object", bugGameObject,typeof(GameObject),true) as GameObject;

        GUILayout.Space(10);
        //绘制描述文本区域
        GUILayout.BeginHorizontal();
        GUILayout.Label("bug详情描述:",GUILayout.MaxWidth(145));
        content = GUILayout.TextArea(content,GUILayout.MaxHeight(60));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("保存错误到本地"))
        {
            SaveBug();
        }

        if (GUILayout.Button("保存错误以及截屏到本地"))
        {
            SaveBugWithScreenshot();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        groupEnabled = EditorGUILayout.BeginToggleGroup("Toggle Group", groupEnabled);

        if (GUILayout.Button("上传到服务器"))
        {
            
        }
        EditorGUILayout.SelectableLabel("文本：可以选择然后复制粘贴");
        string psd = EditorGUILayout.PasswordField("Password:", "2222222222");
        GUILayout.Label(psd);

        EditorGUILayout.MinMaxSlider(ref minVal, ref maxVal, minLimit, maxLimit);
        EditorGUILayout.EndToggleGroup();

        index = EditorGUILayout.Popup(index, options);

        index1 = GUILayout.Toolbar(index1, options, GUILayout.Height(25));

        GUILayout.Label("Save Path", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField(path, GUILayout.ExpandWidth(false));
        if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
            path = EditorUtility.SaveFolderPanel("Path to Save Images", path, Application.dataPath);   //打开保存文件夹面板
        EditorGUILayout.EndHorizontal();

        GUIStyle textStyle = new GUIStyle("textfield");
        GUIStyle buttonStyle = new GUIStyle("button");
        textStyle.active = buttonStyle.active;
        textStyle.onNormal = buttonStyle.onNormal;

        v2 = GUILayout.BeginScrollView(v2, true, true, GUILayout.Width(300), GUILayout.Height(100));
        {
            v = GUILayout.SelectionGrid(v, Messages, 1, textStyle);
        }
        GUILayout.EndScrollView();

        EditorGUILayout.LabelField("路径");
        //获得一个长300的框  
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(600));
        //将上面的框作为文本输入框  
        path = EditorGUI.TextField(rect, path);

        //如果鼠标正在拖拽中或拖拽结束时，并且鼠标所在位置在文本输入框内  
        if ((Event.current.type == EventType.DragUpdated
          || Event.current.type == EventType.DragExited)
          && rect.Contains(Event.current.mousePosition))
        {
            //改变鼠标的外表  
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
            {
                path = DragAndDrop.paths[0];
            }
        }

    }

    void SaveBug()
    {
        Directory.CreateDirectory("Assets/BugReports/");
        StreamWriter sw = new StreamWriter("Assets/BugReports/" + bugName + ".txt");
        sw.WriteLine(bugGameObject.name);
        sw.WriteLine(EditorSceneManager.GetActiveScene().name);
        sw.WriteLine(content);
        sw.WriteLine(System.DateTime.Now);
        sw.Close();
        AssetDatabase.Refresh();
    }

    void SaveBugWithScreenshot()
    {
        Directory.CreateDirectory("Assets/BugReports/");
        StreamWriter sw = new StreamWriter("Assets/BugReports/" + bugName + ".txt");
        sw.WriteLine(bugGameObject.name);
        sw.WriteLine(EditorSceneManager.GetActiveScene().name);
        sw.WriteLine(content);
        sw.WriteLine(System.DateTime.Now);
        sw.Close();
        ScreenCapture.CaptureScreenshot("Assets/BugReports/" + bugName + ".png");
        AssetDatabase.Refresh();
    }


    //更新
    void Update()
    {

    }

    void OnFocus()
    {
        Debug.Log("当窗口获得焦点时调用一次");
    }

    void OnLostFocus()
    {
        Debug.Log("当窗口丢失焦点时调用一次");
    }

    void OnHierarchyChange()
    {
        Debug.Log("当Hierarchy视图中的任何对象发生改变时调用一次");
    }

    void OnProjectChange()
    {
        Debug.Log("当Project视图中的资源发生改变时调用一次");
    }

    void OnInspectorUpdate()
    {
        //Debug.Log("窗口面板的更新");
        //这里开启窗口的重绘，不然窗口信息不会刷新
        this.Repaint();
    }

    void OnSelectionChange()
    {
        //当窗口出去开启状态，并且在Hierarchy视图中选择某游戏对象时调用
        foreach (Transform t in Selection.transforms)
        {
            //有可能是多选，这里开启一个循环打印选中游戏对象的名称
            Debug.Log("OnSelectionChange" + t.name);
        }
    }

    void OnDestroy()
    {
        Debug.Log("当窗口关闭时调用");
    }

}


```
