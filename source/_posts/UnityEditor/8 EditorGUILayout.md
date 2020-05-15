---
title: 8 EditorGUILayout
date: 2020-05-11 11:41:32
top: 8
categories:
- Unity
tags:
- UnityEditor
---
# EditorGUILayout 在一个脚本的检视面板上进行提示可视化代码编写.

* 1:使用编辑器编写一个简单的展示界面
* 2:写一个mono脚本,挂在到一个game object上面
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;

    public string playerName;
    public string backStory;
    public float health;
    public float damage;

    public float weaponDamage1, weaponDamage2;

    public string shoeName;
    public int shoeSize;
    public string shoeType;

    void Start()
    {
        health = 50;
    }
}
```
* 3:写一个PlayerInspector脚本,放在Editor目录下面
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//CustomEditor(typeof()) 用于关联你要自定义的脚本
[CustomEditor(typeof(Player))]
//必须要让该类继承自Editor
public class PlayerInspector : Editor
{
    Player player;
    bool showWeapons;
    private void OnEnable()
    {
        //获取当前编辑自定义Inspector的对象
        player = target as Player;
    }

    //执行这一个函数来一个自定义检视面板
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();//有这个API,表示原生的unity自带的自动展示出来的布局,有这个不需要下面的代码
        EditorGUILayout.BeginVertical();

        {
            //空2行
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }


        //绘制Player的基本信息
        EditorGUILayout.LabelField("基本信息");
        player.id = EditorGUILayout.IntField("Player ID", player.id);
        player.playerName = EditorGUILayout.TextField("PlayerName", player.playerName);

        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        //绘制Player的背景故事
        EditorGUILayout.LabelField("背景故事");
        player.backStory = EditorGUILayout.TextArea(player.backStory,GUILayout.MinHeight(60));

        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        //使用滑块制作Player的生命值        
        player.health = EditorGUILayout.Slider("生命值:", player.health, 0, 100);
        Color color = Color.gray;
        if (player.health<20)
        {
            color = Color.red;
        }
        else if (player.health>80)
        {
            color = Color.green;
        }
        GUI.color = color;
        //指定生命值的宽高
        Rect progressRect = GUILayoutUtility.GetRect(50, 50);
        //绘制生命条
        EditorGUI.ProgressBar(progressRect, player.health, "生命值:");
        //用此处理,防止上面的颜色影响下面的颜色
        GUI.color = Color.white;

        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        //使用滑块绘制伤害值
        player.damage = EditorGUILayout.Slider("Damage", player.damage, 0, 20);

        //根据伤害值的大小设置显示的类型和伤害语
        if (player.damage<5)
        {
            EditorGUILayout.HelpBox("伤害太低,打不动敌人", MessageType.Error);
        }
        else if (player.damage>15)
        {
            EditorGUILayout.HelpBox("伤害太高,对玩家不利于成长", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("伤害适中", MessageType.Info);
        }

        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }


        //设置内容折叠
        showWeapons = EditorGUILayout.Foldout(showWeapons, "Weapons");
        if (showWeapons)
        {
            player.weaponDamage1 = EditorGUILayout.FloatField("武器伤害1", player.weaponDamage1);
            player.weaponDamage2 = EditorGUILayout.FloatField("武器伤害1", player.weaponDamage2);
        }

        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        //绘制鞋子的信息
        EditorGUILayout.LabelField("鞋子");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("名字", GUILayout.MaxWidth(60));
        player.shoeName = EditorGUILayout.TextField(player.shoeName);
        EditorGUILayout.LabelField("尺寸", GUILayout.MaxWidth(60));
        player.shoeSize = EditorGUILayout.IntField(player.shoeSize,GUILayout.MaxWidth(120));
        EditorGUILayout.LabelField("类型", GUILayout.MaxWidth(60));
        player.shoeType = EditorGUILayout.TextField(player.shoeType);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
}

```

* 4:分析关键词:Vertical-垂直布局,在这对兄弟里面做的布局都是以垂直方向来排列的。
```
EditorGUILayout.BeginVertical();
//code
EditorGUILayout.EndVertical();
```
* 5:Horizontal-水平布局,在这对兄弟里面做的布局都是以水平方向来排列的。
```
EditorGUILayout.BeginHorizontal();
//code
EditorGUILayout.EndHorizontal();
```
* 6:它们的规律就是方法名都是以 Field 结尾，大伙们可以根据绘制的类型选择相对应的方法。 一般括号里面的参数，第一个为绘制该字段的名字（string 类型），第二个为绘制该字段的值，如下所示： 

```
EditorGUILayout.LabelField()标签字段 
EditorGUILayout.IntField() 整数字段 
EditorGUILayout.FloatField() 浮点数字段 
EditorGUILayout.TextField() 文本字段 
EditorGUILayout.Vector2Field() 二维向量字段 
EditorGUILayout.Vector3Field() 三维向量字段 
EditorGUILayout.Vector4Field() 四维向量字段 
EditorGUILayout.ColorField() 颜色字段
EditorGUILayout.Slider()滑块进度条
```
* 7:EditorGUILayout.Slider() 制作一个滑动条用户可以拖动来改变值，在最小和最大值之间,
GUILayoutUtility获取Rect的通用方法
EditorGUI.ProgressBar（）用于绘制一个进度条，从上可知：
第一个参数是设置进度条的大小，类型是一个 Rect。 
第二个参数是设置显示的值， 
第三个参数是设置进度条的名字
* 8:EditorGUILayout.HelpBox(),帮助框/提示框
* 

