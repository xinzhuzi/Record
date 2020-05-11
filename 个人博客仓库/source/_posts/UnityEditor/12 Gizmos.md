# Gizmos

* 1:创建 Gizmos文件夹,可以在Scene视图中做一个预览的线,展示摄像机轨迹,即辅助线框
所有Gizmos的绘制必须在脚本的OnDrawGizmos或OnDrawGizmosSelected里编写,必须于Scene视图下，于Game视图下不起作用;
使用Gizmos.DrawIcon(transform.position, "0.png", true), 可以在Scene视图里给某个坐标绘制一个icon。
它的好处是可以传一个Vecotor3 作为图片显示的位置。 参数2就是图片的名字，当然这个图片必须放在Gizmos文件夹下面。
 * 2:
 ```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosScript : MonoBehaviour
{
    [SerializeField]
    float areaRadius;

    [SerializeField]
    float size;

    [SerializeField]
    Vector3[] nodePoints;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        //绘制线框球体第一个参数就是起点的位置，第二个参数就是半径。 
        Gizmos.DrawWireSphere(this.transform.localPosition, areaRadius);
        Gizmos.color = Color.cyan;

        //第一个参数就是起点的位置，第二个参数就是指定的位置。 
        Gizmos.DrawLine(this.transform.localPosition, transform.position + transform.forward * size);

        for (int i = 0; i < nodePoints?.Length; i++)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(nodePoints[i], areaRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(nodePoints[i], nodePoints[(int)Mathf.Repeat(i+1,nodePoints.Length)]);
        }
        Gizmos.DrawIcon(this.transform.position, "heart");
    }

    private void OnDrawGizmosSelected()
    {
    }
}

 ```