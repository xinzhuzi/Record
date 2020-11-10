---
title: 11 Handles
date: 2020-05-11 11:41:32
top: 11
categories:
- UnityEditor
tags:
- UnityEditor
---

# Handles

* 1: 概念:场景视图的3d GUI 控制
```
using System;
using UnityEngine;

//https://assetstore.unity.com/packages/tools/level-design/curvy-splines-7038
namespace UnityEditor
{
    [CustomEditor(typeof(HandlesScript))]
    public class HandlesScriptEditor : Editor
    {
        HandlesScript hs;
        private void OnEnable()
        {
            hs = target as HandlesScript;
        }

        public override void OnInspectorGUI()
        {
            hs.areaRadius = EditorGUILayout.FloatField("操作手柄的半径", hs.areaRadius);
            hs.size = EditorGUILayout.FloatField("操作手柄的大小", hs.size);
            
            //在Inspector面板上面展示数组
            SerializedProperty nodePointsProperty = serializedObject.FindProperty("nodePoints");
            SerializedProperty nodePointQuaternionsProperty = serializedObject.FindProperty("nodePointQuaternions");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(nodePointsProperty, new GUIContent("位置节点"), true);
            EditorGUILayout.PropertyField(nodePointQuaternionsProperty, new GUIContent("旋转节点"), true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            //给一个物体,添加一个文字描述
            //第一个参数表示当前在场景中展示的位置
            //第二个参数表示显示的名字
            Handles.Label(hs.transform.position + new Vector3(0, 1, 0), "手柄");

            //第一个参数:该旋转操作手柄的初始旋转角度
            //第二个参数:操作手柄的显示位置,一般为物体的中心点
            //第三个参数:操作手柄的半径
            //多用于制作AI，用于判断和指定AI影响范围用的。
            hs.areaRadius = Handles.RadiusHandle(Quaternion.identity, hs.transform.position, hs.areaRadius);

            //第一个参数:可以通过Inspector面板修改值,也是该函数的返回值.
            //第二个参数:操作手柄的位置
            //第三个参数:操作手柄的指向
            //第四个参数:操作手柄的大小
            //第五个参数:操作手柄的显示方式,(箭头ArrowHandleCap,RectangleHandleCap矩形,CircleHandleCap圆形),只要是方法后缀带有Cap的都可以传入
            //第六个参数:一般为0.5f,不知道干什么的
            hs.size = Handles.ScaleValueHandle(hs.size, hs.transform.position, Quaternion.identity, hs.size,Handles.ArrowHandleCap, 0.5f);


            for (int i = 0; i < hs.nodePoints?.Length; i++)
            {
                //第一个参数:该操作手柄位于世界坐标的位置
                //第二个参数:该操作手柄的操作旋转方向
                hs.nodePoints[i] = Handles.PositionHandle(hs.nodePoints[i], hs.nodePointQuaternions[i]);
            }

            for (int i = 0; i < hs.nodePointQuaternions?.Length; i++)
            {
                //第一个参数:该操作手柄的操作旋转方向 
                //第二个参数:该操作手柄位于世界坐标的位置
                hs.nodePointQuaternions[i] = Handles.RotationHandle(hs.nodePointQuaternions[i], hs.nodePoints[i]);
                //画线
                Handles.DrawLine(hs.nodePoints[i], hs.nodePoints[(int)Mathf.Repeat(i + 1, hs.nodePoints.Length)]);
            }

        }


    }
}

```