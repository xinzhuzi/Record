---
title: 4 EditorGUI
date: 2020-05-11 11:41:32
top: 4
categories:
- Unity
tags:
- UnityEditor
---
# EdirotGUILayout / EdirotGUI 方法的简单使用

* 1:编写一个EditorWindow并展示一个label

```
    public class TestEditorWindow : EditorWindow
    {
        [MenuItem("Window/Example")]
        static void Open()
        {
            GetWindow<TestEditorWindow>();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Example Label");
        }
    }
```
* 2:Toggle
```
    bool showBtn = true;
    void OnGUI()
    {
        showBtn = EditorGUILayout.Toggle("Show Button",showBtn);
        if(showBtn){  //开关点开
            if(GUILayout.Button("Close")){ //绘制按钮
                this.Close(); //关闭面板
            }
        }
    }
```
```
    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        toggleValue = EditorGUILayout.ToggleLeft("Toggle", toggleValue);

        //toggleValue 值每次改变时,EditorGUI.EndChangeCheck()都返回true
        if (EditorGUI.EndChangeCheck())
        {
            if (toggleValue)
            {
                Debug.Log("toggleValue:" + toggleValue);
            }
            else
            {
                Debug.Log("toggleValue:" + toggleValue);
            }
        }
        bool on = GUILayout.Toggle(on, on ? "on" : "off", "button");
    }
```

```
    bool toggleValue;

    Stack<bool> stack = new Stack<bool> ();

    void OnGUI ()
    {
        {
            stack.Push (GUI.changed);
            GUI.changed = false;
        }
        toggleValue = EditorGUILayout.ToggleLeft ("Toggle", toggleValue);
        {
            bool changed = GUI.changed;

            GUI.changed |= stack.Pop ();
        }
        if (changed) {
            Debug.Log ("toggleValue");
        }
    }

```
```
    bool toggleValue;

    Stack<bool> stack = new Stack<bool>();

    void OnGUI()
    {
        stack.Push(GUI.changed);
        GUI.changed = false;
        
        toggleValue = EditorGUILayout.ToggleLeft("Toggle", toggleValue);
        
        bool changed = GUI.changed;
        Debug.Log("changed:" + changed);

        GUI.changed |= stack.Pop();

        if (changed)
        {
            Debug.Log("toggleValue");
        }
    }
```
```
    private bool groupEnabled; //区域开关
    void OnGUI()
    {
        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        ///其他编辑代码
        EditorGUILayout.EndToggleGroup();
    }
```
* 3:ObjectField
```
    void OnGUI ()
    {
        EditorGUILayout.ObjectField (null, typeof(Object), false);

        EditorGUILayout.ObjectField (null, typeof(Material), false);

        EditorGUILayout.ObjectField (null, typeof(AudioClip), false);

        var options = new []{GUILayout.Width (64), GUILayout.Height (64)};

        EditorGUILayout.ObjectField (null, typeof(Texture), false, options);

        EditorGUILayout.ObjectField (null, typeof(Sprite), false, options);
    }
```
* 4:EditorGUI.MultiFloatField
```
    float[] numbers = new float[] {
        0,
        1,
        2
    };

    GUIContent[] contents = new GUIContent[] {
        new GUIContent ("X"),
        new GUIContent ("Y"),
        new GUIContent ("Z")
    };

    void OnGUI()
    {
        EditorGUI.MultiFloatField(
            new Rect(30, 30, 200, EditorGUIUtility.singleLineHeight),
            new GUIContent("Label"),
            contents,
            numbers);
    }
```
* 5: EditorGUI.indentLevel 层级表现
```
void OnGUI ()
{
    EditorGUILayout.LabelField ("Parent");

    EditorGUI.indentLevel++;

    EditorGUILayout.LabelField ("Child");
    EditorGUILayout.LabelField ("Child");

    EditorGUI.indentLevel--;

    EditorGUILayout.LabelField ("Parent");

    EditorGUI.indentLevel++;

    EditorGUILayout.LabelField ("Child");
}
```
* 6:EditorGUILayout.Knob,一个圈圈表现
```
    float angle = 270;

    void OnGUI()
    {
        angle = EditorGUILayout.Knob(Vector2.one * 64,
            angle, 0, 360, "度", Color.gray, Color.red, true);
    }
```
* 7:Scope排版
```
    public class HorizontalScope : GUI.Scope
    {
            
        public HorizontalScope()
        {
            EditorGUILayout.BeginHorizontal();
        }

        protected override void CloseScope()
        {
            EditorGUILayout.EndHorizontal();
        }        
    }

    void OnGUI()
    {

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Button("Button1");
            GUILayout.Button("Button2");
        }
        using (new EditorGUILayout.HorizontalScope ()) {
            one = GUILayout.Toggle (one, "1", EditorStyles.miniButtonLeft);
            two = GUILayout.Toggle (two, "2", EditorStyles.miniButtonMid);
            three = GUILayout.Toggle (three, "3", EditorStyles.miniButtonRight);
        }     
    }
    
```
* 8:Toolbar
```
    int selected;

    void OnGUI()
    {
        selected = GUILayout.Toolbar(selected, new string[] { "1", "2", "3" });
        selected = GUILayout.Toolbar(selected, new string[] { "1", "2", "3" }, EditorStyles.toolbarButton);
        selected = GUILayout.SelectionGrid(selected,new string[] { "1", "2", "3" }, 1, "PreferencesKeysElement");
    }
```
* 9:SelectableLabel 可选择标签(通常用于显示只读信息，可以被复制粘贴)
```
string text="hiahia";
    void OnGUI()
    {
        EditorGUILayout.SelectableLabel(text); //文本：可以选择然后复制粘贴
    }
```
* 10:PasswordField 密码字段
```
    //创建密码字段并可视化
    string text = "Some text here";
    bool showBtn = true;
    void OnGUI() 
    {
        text = EditorGUILayout.PasswordField("Password:",text);
        showBtn = EditorGUILayout.Toggle("Show Button", showBtn);
        if (showBtn)
        {
            EditorGUILayout.LabelField("密码:", text);
        }
    }
```
* 11:Slider 滑动条 IntSlider 整数滑动条 MinMaxSlider 最小最大滑动条
```
    //随机放置选择的物体在最小最大滑动条之间
    float  minVal = -10.0f;
    float minLimit = -20.0f;
    float maxVal = 10.0f;
    float maxLimit = 20.0f;
    void OnGUI()
    {
        EditorGUILayout.LabelField("Min Val:", minVal.ToString());
        EditorGUILayout.LabelField("Max Val:", maxVal.ToString());
        EditorGUILayout.MinMaxSlider(ref minVal,ref  maxVal, minLimit, maxLimit);

    }
```
* 12:Popup弹出选择菜单
```
    string[] options = { "Cube","Sphere","Plane"};
    int index = 0;
    void OnGUI()
    {
        index = EditorGUILayout.Popup(index, options);
    }
```
```
    enum OPTIONS
    {
        CUBE = 0,
        SPHERE = 1,
        PLANE = 2
    }
    public class myEditor3 : EditorWindow {
        OPTIONS op=OPTIONS.CUBE;
        [MenuItem("cayman/tempShow")]
        static void newWelcome()
        {
            EditorWindow.GetWindow(typeof(myEditor3), true, "Eam");
        }
        void OnGUI()
        {
        op = (OPTIONS)EditorGUILayout.EnumPopup("Primitive to create:", op)  ;
        }
    }
```
```
    int selectedSize = 1;
    string[] names = { "Normal","Double","Quadruple"};
    int[] sizes = { 1,2,4};
    void OnGUI()
    {
        selectedSize = EditorGUILayout.IntPopup("Resize Scale: ", selectedSize, names, sizes);
        if (GUILayout.Button("Scale"))
            ReScale();
    }
    void ReScale()
    {
        if (Selection.activeTransform)
            Selection.activeTransform.localScale =new Vector3(selectedSize, selectedSize, selectedSize);
        else Debug.LogError("No Object selected, please select an object to scale.");
    }
```

* 13:ColorField 颜色字段 
```
    Color matColor = Color.white;
    void OnGUI()
    {
        matColor = EditorGUILayout.ColorField("New Color", matColor);
 
    }
```
* 14:Vector2Field 二维向量字段 Vector3Field 三维向量字段(略，同2维)
```
    float distance = 0;
    Vector2 p1, p2;
    void OnGUI()
    {
        p1 = EditorGUILayout.Vector2Field("Point 1:", p1);
        p2 = EditorGUILayout.Vector2Field("Point 2:", p2);
        EditorGUILayout.LabelField("Distance:", distance.ToString());
    }
    void OnInspectorUpdate() //面板刷新
    {
        distance = Vector2.Distance(p1, p2);
        this.Repaint();
    }
```
* 15:TagField 标签字段 LayerField层字段
```
    string tagStr = "";
    int selectedLayer=0;
    void OnGUI()
    {  //为游戏物体设置
        tagStr = EditorGUILayout.TagField("Tag for Objects:", tagStr);
        tagStr = EditorGUILayout.LayerField("Layer for Objects:", selectedLayer);
        if (GUILayout.Button("Set Tag!"))
            SetTags();
        if(GUILayout.Button("Set Layer!"))
            SetLayer();
    }
    void SetTags() {
        foreach(GameObject go in Selection.gameObjects)
            go.tag = tagStr;
    }
     void SetLayer() {
        foreach(GameObject go in Selection.gameObjects)
            go.laye = selectedLayer;
    }
```
* 16:打开保存位置文件夹
```
string path;
GUILayout.Label ("Save Path", EditorStyles.boldLabel);
EditorGUILayout.BeginHorizontal();
EditorGUILayout.TextField(path,GUILayout.ExpandWidth(false));
if(GUILayout.Button("Browse",GUILayout.ExpandWidth(false)))
            path = EditorUtility.SaveFolderPanel("Path to Save Images",path,Application.dataPath);   //打开保存文件夹面板
EditorGUILayout.EndHorizontal();
```
* 17:折叠标签,下面的player.weaponDamage1和player.weaponDamage2属性,都会被折叠在Weapons标签里面
```
        bool showWeapons;
        showWeapons = EditorGUILayout.Foldout(showWeapons, "Weapons");
        if (showWeapons)
        {
            player.weaponDamage1 = EditorGUILayout.FloatField("武器伤害1", player.weaponDamage1);
            player.weaponDamage2 = EditorGUILayout.FloatField("武器伤害1", player.weaponDamage2);
        }
```
* 18:滑动区域 GUILayout.BeginScrollView  GUILayout.EndScrollView();
选择网格 SelectionGrid,SelectionGrid(int 选择的索引,sting[] 显示文字数组，xCount，格式)
```
    GUIStyle textStyle = new GUIStyle("textfield");
    GUIStyle buttonStyle = new GUIStyle("button");
    textStyle.active = buttonStyle.active;
    textStyle.onNormal = buttonStyle.onNormal;

    v2 = GUILayout.BeginScrollView(v2, true, true, GUILayout.Width(300), GUILayout.Height(100));
    {
        v = GUILayout.SelectionGrid(v, Messages, 1, textStyle);
    }
    GUILayout.EndScrollView();
```
* 19:DragAndDrop中的拖拽区域,拖拽一个物体到windos面板上面,得到路径
```
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
```
* 20:Box绘制GUI.Box(xxx,xxx)
* 21:window面板的提示1. 打开一个通知栏 this.ShowNotification(new GUIContent(“This is a Notification”));2. 关闭通知栏 this.RemoveNotification();
* 22: