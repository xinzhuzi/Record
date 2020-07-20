---
title: 7 CustomEditor_PropertyDrawer
date: 2020-05-11 11:41:32
top: 7
categories:
- Unity
tags:
- UnityEditor
---
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
* 5:完整的表示并使用PropertyDrawer,他是将一个数据类,完整的显示在一个脚本的Inspector面板上面
```

    public enum Sex
    {
        famale,//女同志
        male//男同志
    }

    [System.Serializable]
    public class Persion
    {
        public string name;

        public Sex sex;

        public int age;

        public string description;
    }

```
```
    using UnityEngine;

    public class ShowPersionInfo : MonoBehaviour
    {
        public Persion persion;
    }
```
```
using System;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Persion))] //自定义属性控制
public class PersionPropertiesDrawer : PropertyDrawer
{
    Rect top, middleLeft, middleRight, bottom;//绘制指定的区域
    SerializedProperty persionName, sex, age, description;//绘制对应的序列化属性
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);
        //Debug.Log("label:" + label.text);
        //Debug.Log(position);
        //Debug.Log("OnGUI:" + property.name);

        //这个地方是将property转换成下一个property,也就是从persion这个属性转成Persion类中的name,sex,age,description属性了
        //while (property.NextVisible(true))
        //{
        //    //Debug.Log("OnGUI:" + property.name);
        //}
        top = new Rect(position.x, position.y, position.width, position.height / 4);
        middleLeft = new Rect(position.x, position.y + position.height / 4, position.width/2, position.height / 4/2);
        middleRight = new Rect(position.x + position.width/2, position.y + position.height / 4, position.width/2, position.height / 4);
        bottom = new Rect(position.x, position.y + position.height / 2, position.width, position.height / 2);

        //获取对应的序列化属性
        //property这个属性对应的是 ShowPersionInfo 脚本中的Persion对象的persion属性,
        //里面有几个Persion对象的persion属性,就调用这个方法绘制多少次
        persionName = property.FindPropertyRelative("name");//通过属性的名字获取对应的序列化对象SerializedProperty
        sex = property.FindPropertyRelative("sex");
        age = property.FindPropertyRelative("age");
        description = property.FindPropertyRelative("description");

        //绘制属性
        //EditorGUI.PropertyField第一个参数绘制该属性在Inspector在面板的位置.
        EditorGUI.PropertyField(top, persionName);
        EditorGUI.PropertyField(middleLeft, sex);
        EditorGUI.PropertyField(middleRight, age);
        description.stringValue = EditorGUI.TextArea(bottom, description.stringValue);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        while (property.NextVisible(true))
        {
            //Debug.Log("GetPropertyHeight:" + property.name);
        }
        return base.GetPropertyHeight(property, label) * 4;
    }

    /**
     * 1.OnGUI 和 GetPropertyHeight 里的 property 参数是同一个参数。该参数里存放的是 Persion 里的属性信息。
     * 2.OnGUI 和 GetPropertyHeight 里的 Label 参数也是同一个参数，该参数里存放的是 Persion 类的类名。
     * 3.position参数指的是需要在Inspector面板中绘制的区域信息,即当前脚本挂在在到GameObject上面的Inspector上的区域
     * 4.在Inspector面板中的一行高度为 16 
     */
}
```