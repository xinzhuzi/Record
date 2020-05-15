---
title: 10 Screenshot
date: 2020-05-11 11:41:32
top: 10
categories:
- Unity
tags:
- UnityEditor
---
# 使用Editor编写一个截屏得window面板

* 1:先理解一些手机概念的定义.[手机参数概念](http://www.woshipm.com/ucd/198774.html)
手机5.2英寸的定义是:手机的对角线是5.2英寸,1英寸（inch）=2.54厘米（cm）,计算方式就是勾股定理,算斜边的长度就是卖手机的时候所说的手机尺寸.
手机分辨率是1920PX*1080PX:PX的意思就是像素,可以默认为像素是分辨率,手机的点,线,面都是由一个个的像素表现的,可以理解为一个像素是一个小网格,这个含义表示,在手机的竖向上面有1920个像素,在手机的横向上面有1080个像素.
屏幕像素密度:即在一个对角线长度为1英寸的正方形内所拥有的像素数
* 2:如果需要查看camera/px/size/unit 摄像机,摄像机的size,像素,单位,x,y,width,height的区别,百度一下.[RenderTexture概念1](https://docs.unity3d.com/ScriptReference/RenderTexture.html),[RenderTexture概念2](https://blog.csdn.net/leonwei/article/details/54972653),[RenderTexture概念3](https://www.jianshu.com/p/334770f39127)
* 3:
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ScreenshotWindow : EditorWindow
{
    int resWidth  = Screen.width * 4;
    int resHeight = Screen.height * 4;

    public Camera currentCamera;

    int scale = 1;

    string path = "";

    bool showPreview = true;

    RenderTexture renderTexture;

    bool isTransparent = false;

    float lastTime;

    string lastPath;


    [MenuItem("Tools/截屏")]
    public static void ShowScreenshotWindow()
    {
        EditorWindow ew = GetWindow<ScreenshotWindow>();
        ew.autoRepaintOnSceneChange = true;//窗口发生变化时,自动重绘
        ew.titleContent = new GUIContent("截屏");
    }


    private void OnGUI()
    {
        {
            EditorGUILayout.LabelField("分辨率", EditorStyles.boldLabel);
            resWidth = EditorGUILayout.IntField("截取宽度:", resWidth);
            resHeight = EditorGUILayout.IntField("截取高度:", resHeight);
            EditorGUILayout.Space();
            scale = EditorGUILayout.IntSlider("缩放", scale, 1, 15);
            //显示帮助信息
            EditorGUILayout.HelpBox("截屏的默认模式是裁剪-所以选择一个合适的宽度和高度。比例是一个因素，以倍增或扩大渲染而不失去质量。", MessageType.None);
            EditorGUILayout.Space();
        }

        {
            GUILayout.Label("Save Path", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(path, GUILayout.MaxWidth(500));
            if (GUILayout.Button("选择文件"))
            {
                path = EditorUtility.SaveFolderPanel("Path to Save Images", path, Application.dataPath);
            }
            EditorGUILayout.EndHorizontal();
            //显示帮助信息
            EditorGUILayout.HelpBox("选择文件夹保存截取的图片", MessageType.None);
            EditorGUILayout.Space();
        }
        {
            GUILayout.Label("Select Camera", EditorStyles.boldLabel);
            currentCamera = EditorGUILayout.ObjectField("选择摄像机", currentCamera, typeof(Camera), true) as Camera;
            if (currentCamera==null)
            {
                currentCamera = Camera.main;
            }
            isTransparent = EditorGUILayout.Toggle("是否需要透明背景", isTransparent);
            EditorGUILayout.HelpBox("选择要捕捉渲染的摄像机,可以使用透明选项使背景透明.", MessageType.None);
            EditorGUILayout.Space();
        }
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("当前屏幕分辨率"))
            {
                resWidth = (int)Handles.GetMainGameViewSize().x;
                resHeight = (int)Handles.GetMainGameViewSize().y;
            }

            if (GUILayout.Button("默认屏幕分辨率"))
            {
                resHeight = 1440;
                resWidth = 2560;
                scale = 1;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("截图将于 " + resWidth * scale + " x " + resHeight * scale + " 像素拍摄", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }
        {
            if (GUILayout.Button("截屏",GUILayout.MinHeight(40)))
            {
                TakeScreenshot();
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Last Screenshot", GUILayout.MaxWidth(160), GUILayout.MinHeight(40)))
            {
                if (lastPath != "")
                {
                    Application.OpenURL("file://" + lastPath);
                }
            }
            if (GUILayout.Button("Open Folder", GUILayout.MaxWidth(100), GUILayout.MinHeight(40)))
            {

                Application.OpenURL("file://" + path);
            }
            if (GUILayout.Button("More Assets", GUILayout.MaxWidth(100), GUILayout.MinHeight(40)))
            {
                Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/publisher/5951");
            }
            EditorGUILayout.EndHorizontal();
        }

    }

    private void TakeScreenshot()
    {
        int resWidthN = resWidth * scale;
        int resHeightN = resHeight * scale;
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        currentCamera.targetTexture = rt;
        TextureFormat tf = TextureFormat.RGB24;
        if (isTransparent)
        {
            tf = TextureFormat.RGBA32;
        }
        Texture2D screenShot = new Texture2D(resWidthN, resHeightN, tf, false);
        currentCamera.Render();//将屏幕渲染到targetTexture里面
        RenderTexture.active = rt;//当前活跃的RenderTexture
        //将GPU中的FrameBufferObject可读对象拷贝到CPU中存储为一个buffer,然后读取到Texture2D中
        screenShot.ReadPixels(new Rect(0, 0, resWidthN, resHeightN), 0, 0);
        currentCamera.targetTexture = null;
        RenderTexture.active = null;
        byte[] bytes = screenShot.EncodeToPNG();//编码成PNG
        lastPath = string.Format("{0}/screen_{1}x{2}_{3}.png", path, resWidthN, resHeightN, System.DateTime.Now.ToString("yyyy-mm-dd_HH-mm-ss"));
        System.IO.File.WriteAllBytes(lastPath, bytes);
        Application.OpenURL(lastPath);//打开图片
        AssetDatabase.Refresh(); 
    }


}

```
* 4:
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ScriptToText : EditorWindow
{
    [MenuItem("Tools/脚本与文件互转")]
    static void ShowScriptToTextWindow()
    {
        GetWindow<ScriptToText>("脚本与文件互转");
    }




    private void OnGUI()
    {
        //整个窗口为水平布局
        GUILayout.BeginHorizontal();
        DrawLeft();
        //GUILayout.Label("|", GUILayout.MinHeight(10000));
        DrawRight();
        GUILayout.EndHorizontal();
    }

    private string scriptsContext = "";
    private Vector2 scrollposition;

    /// <summary>
    /// 绘制左边的区域
    /// </summary>
    private void DrawLeft()
    {
        //局部为垂直窗口区域
        GUILayout.BeginVertical();
        GUILayout.Label("显示脚本中的所有内容:");
        //开始滑动的区域
        scrollposition = GUILayout.BeginScrollView(scrollposition);

        //绘制文本
        scriptsContext = GUILayout.TextArea(scriptsContext, GUILayout.ExpandHeight(true));//动态高度,会占满

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    //文件资源对象
    TextAsset textAssetObject;
    //脚本资源对象
    TextAsset scriptAssetObject;

    //文件保存的路径
    string saveFilePath = "";
    //脚本保存的路径
    string scriptFilePath = "";

    //绘制2D图像
    Texture2D texture2D;

    private void DrawRight()
    {
        //局部为垂直窗口区域
        GUILayout.BeginVertical();
        {

            textAssetObject = (TextAsset)EditorGUILayout.ObjectField("script转txt", textAssetObject, typeof(TextAsset), true);
            EditorGUILayout.LabelField("被保存在    " + saveFilePath + "/" + "    路径下面");
            //绘制Text字段，用户获取用户指定的路径
            if (GUILayout.Button("选择txt保存的路径"))
            {
                saveFilePath = EditorUtility.SaveFolderPanel("Path to Save Images", saveFilePath, Application.dataPath);
            }
            if (GUILayout.Button("script保存为txt"))
            {
                //执行这个方法
                SaveFile(".txt");
            }
            if (textAssetObject!=null && !textAssetObject.text.Equals(""))
            {
                scriptsContext = textAssetObject.text;
            }
        }
        GUILayout.Space(40);
        {

            scriptAssetObject = (TextAsset)EditorGUILayout.ObjectField("txt转script", scriptAssetObject, typeof(TextAsset), true);
            EditorGUILayout.LabelField("被保存在    " + saveFilePath + "/" + "    路径下面");
            //绘制Text字段，用户获取用户指定的路径
            if (GUILayout.Button("选择脚本保存的路径"))
            {
                saveFilePath = EditorUtility.SaveFolderPanel("Path to Save Images", saveFilePath, Application.dataPath);
            }
            if (GUILayout.Button("txt保存为script"))
            {
                //执行这个方法
                SaveFile(".cs");
            }
            if (scriptAssetObject != null && !scriptAssetObject.text.Equals(""))
            {
                scriptsContext = textAssetObject.text;
            }
        }
        GUILayout.Space(40);
        {
            Texture2D texture2D = AssetDatabase.LoadAssetAtPath("Assets/Texture/Battleground_bg.png", typeof(Texture2D)) as Texture2D;
            GUI.DrawTexture(GUILayoutUtility.GetRect(500, 300), texture2D);
        }
        GUILayout.EndVertical();
    }

    //默认文件保存路径
    private const string defaultFilePath = "Assets/TextFiles/";

    //默认脚本保存路径
    private const string defaultScriptPath = "Assets/Scripts/";

    /// <summary>
    /// 脚本转换为文本
    /// </summary>
    void SaveFile(string suffix)
    {
        string path = defaultFilePath;
        if (!saveFilePath.Equals(""))
        {
            path = saveFilePath;
        }
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        //写入文件
        StreamWriter sw = new StreamWriter(defaultFilePath + textAssetObject.name + suffix);
        sw.Write(textAssetObject.text);
        sw.Close();
        AssetDatabase.Refresh();
    }

}

```