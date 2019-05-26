# CustomEditor 自定义组件编辑器

* 1:首先生成一个继承Monobehiver的脚本名字叫做Character,生成一个类型为int的属性叫做Attack,再生成一个CharacterInspector脚本放在Editor文件夹中
```
    using UnityEngine;
    using UnityEditor;


    [CanEditMultipleObjects]
    [CustomEditor(typeof(Character))]
    public class CharacterInspector : Editor
    {
        SerializedProperty AttackProperty;

        void OnEnable()
        {
            AttackProperty = serializedObject.FindProperty("Attack");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.IntSlider(AttackProperty, 0, 100);

            serializedObject.ApplyModifiedProperties();
        }
    }
```

* 2:PropertyDrawer 的使用
```
    [System.Serializable]
    public class PropertyDrawerExample
    {
        public int minHp;
        public int maxHp;
    }

    public class PropertyDrawer1 : MonoBehaviour
    {
        public PropertyDrawerExample example;
    }

```
```
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(Example))]
    public class PropertyDrawerEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position,
                            SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {

                var minHpProperty = property.FindPropertyRelative("minHp");
                var maxHpProperty = property.FindPropertyRelative("maxHp");

                //表示位置
                var minMaxSliderRect = new Rect(position)
                {
                    height = position.height * 0.5f
                };

                var labelRect = new Rect(minMaxSliderRect)
                {
                    x = minMaxSliderRect.x + EditorGUIUtility.labelWidth,
                    y = minMaxSliderRect.y + minMaxSliderRect.height
                };

                float minHp = minHpProperty.intValue;
                float maxHp = maxHpProperty.intValue;

                EditorGUI.BeginChangeCheck();

    #pragma warning disable CS0618 // 类型或成员已过时
                EditorGUI.MinMaxSlider(label,
                            minMaxSliderRect, ref minHp, ref maxHp, 0, 100);
    #pragma warning restore CS0618 // 类型或成员已过时

                EditorGUI.LabelField(labelRect, minHp.ToString(), maxHp.ToString());

                if (EditorGUI.EndChangeCheck())
                {
                    minHpProperty.intValue = Mathf.FloorToInt(minHp);
                    maxHpProperty.intValue = Mathf.FloorToInt(maxHp);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2;
        }
    }
```
* 3:HasPreviewGUI可以展示预览状态
```
public override bool HasPreviewGUI ()
{
    return true;
}
public override GUIContent GetPreviewTitle ()
{
    return new GUIContent ("title");
}
public override void OnPreviewSettings ()
{
    GUIStyle preLabel = new GUIStyle ("preLabel");
    GUIStyle preButton = new GUIStyle ("preButton");

    GUILayout.Label ("t", preLabel);
    GUILayout.Button ("t2", preButton);
}
public override void OnPreviewGUI (Rect r, GUIStyle background)
{
    GUI.Box (r, "Preview");
}
```
* 4:PreviewRenderUtility在预览状态下,设置一个摄像机,照出当前的物体.待续
* 5:
