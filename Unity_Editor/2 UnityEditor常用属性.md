# UnityEditor常用属性

#####即在编写脚本时,在编辑器上面展示出来的属性不够清晰明了,通过使用编辑器属性扩展可以快速理解含义
#####继承自UnityEngine.PropertyAttribute


* 1:使用Range给int、float、long、double限定范围

```
        [Range(1,10)]
        public int t1;
        [Range(10, 100)]
        public float t2;
        [Range(20, 50)]
        public double t3;
        [Range(60, 10000000)]
        public long t4;
```

* 2:字符串显示,Multiline和TextArea基本没什么区别,不过TextArea比Multiline表现要好的多,一般用TextArea足够.

```
    [Multiline(5)]
    public string text1;

    [TextArea(5,10)]
    public string text2;
```

* 3:ContextMenuItem功能,扩展脚本编辑的一些操作.对属性进行的扩展,多个属性可调用一个方法,一个属性可调用多个方法,方便强大

```
    [ContextMenuItem("op1", "number1")]
    [ContextMenuItem("op2", "number2")]
    public int number;

    void number1()
    {
        number = Random.Range(0, 100);
    }

    void number2()
    {
        number = 0;
    }
```

* 4: color颜色使用ColorUsage进行展示

```
    public Color color1;

    [ColorUsage (false)]
    public Color color2;

    [ColorUsage (true, true)]
    public Color color3;
```

* 5: 使用组概念,使其更美观舒服

```
    [Header("Player Settings--------------------------------------------------------------------------")]
    public Player player;
    [Serializable]
    public class Player
    {
        public string name;

        [Range(1, 100)]
        public int hp;
    }

    [Header("Game Settings--------------------------------------------------------------------------")]
    public Color background;
    public Color background1;
```

* 6: Space 设置2个属性之间的纵向空白区域,使看起来更舒服
* 7: Tooltip("属性解释"),鼠标浮在属性上方即可展示出来
* 8: HideInInspector,隐藏一个public的属性,使其不在Inspector中展示,
* 9: RequireComponent当你添加一个A组件,但是这个组件必须引用另一个B组件,这时在A上面写入RequireComponent(B)

```
public class Test2 : MonoBehaviour{}

[RequireComponent(typeof(Test2),typeof(Animation))]
public class Test1 : MonoBehaviour{}
```
* 10: DisallowMultipleComponent 禁止在一个物体上面添加多个相同的类型,再添加的时候会提示你已经添加了一个这样的类型,不能再次添加
* 11: AddComponentMenu 在 Component/Scripts 路径后面接 UI/TColor,即可快速添加脚本

```
using UnityEngine;
[AddComponentMenu("UI/TColor")]
public class TweenColor : MonoBehaviour{}
```
* 12:ExecuteInEditMode属性的作用是在EditMode下也可以执行脚本。Unity中默认情况下，脚本只有在运行的时候才被执行，加上此属性后，不运行程序，也能执行脚本。
与PlayMode不同的是，函数并不会不停的执行。
Update : 只有当场景中的某个物体发生变化时，才调用。
OnGUI : 当GameView接收到一个Event时才调用。
OnRenderObject 和其他的渲染回调函数 : SceneVidw或者GameView重绘时，调用。
Awake与Start调用规则：
Awake：加载时调用。
Start：第一次激活时调用。(刚被挂载上)
这个与运行模式下的调用规则一致。参见《unity-----函数执行顺序》。
   需要注意的是，由于在两种模式下都可以运行，所有，切换模式的那一刻，值得我们留 意一下。
   不管是从编辑模式进入运行模式，还是从运行模式进入编辑模式，unity都会重新加载资源，所以：
当该MonoBehavior在编辑器中被赋于给GameObject的时候，Awake, Start 将被执行。
当Play按钮被按下游戏开始以后，Awake, Start 将被执行。
当Play按钮停止后，Awake, Start将再次被执行。
当在编辑器中打开包含有该MonoBehavior的场景的时候，Awake, Start将被执行。

* 13:ContextMenu 类似于ContextMenuItem
```
        [Range (0, 10)]
        public int number;
        [ContextMenu ("RandomNumber")]
        void RandomNumber ()
        {
            number = Random.Range (0, 100);
        }

        [ContextMenu ("ResetNumber")]
        void ResetNumber ()
        {
            number = 0;
        }
```