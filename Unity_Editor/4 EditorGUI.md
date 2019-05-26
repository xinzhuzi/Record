# EdirotGUILayout / EdirotGUI 界面

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