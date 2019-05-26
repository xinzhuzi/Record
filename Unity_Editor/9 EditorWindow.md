#使用Editor编写一个window面板

* 1:编写一个bug保存到本地的window面板
```
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BugReportWindow : EditorWindow
{

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
}

```
* 2:上面可以简单的制作一个window面板的编辑器,一些方法的使用需要看[EditorGUI方法介绍]()