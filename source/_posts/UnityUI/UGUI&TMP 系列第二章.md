# Image

* 1. 空的 Image会造成一个 DC,并且会打断合批.创建的时候将Image 的 SourceImage 给赋值,并且不设置 RayCastTarget

* 2. 颜色渐变和可以用一个新的material控制，本质上是更改Tint属性，这样既能满足颜色渐变，又能避免网格频繁重建

```c#

    public Color color = Color.white;//使用动画修改此属性,耗时相对于使用直接修改Image.Color属性非常少
    public Image image = null;

    void Start()
    {
        image = GetComponent<Image>();
        image.material = Instantiate(image.material) as Material;
    }


    void LateUpdate()
    {
        image.material.SetColor("_Color", color);
    }


```

* 3. 直接从创建的地方将 Text/Image/RawImage 这 3 种类型修改,设置 raycastTarget属性为 false,可以降低性能.

* 4. Empty4Raycast.cs 配合 button .使用的此脚本可以在有效接受点击事件的情况下，不增加DrawCall和打断合批, 
     只要点击区域，不要显示内容的。可以把空白透明Image替换成qiankanglai提供的Empty4Raycast.
     https://blog.uwa4d.com/archives/fillrate.html

     
* 5. Image叠加层数越多,overdraw 显示的颜色越红,同一个像素在同一帧绘制的次数越多. 
     这时候如果 Image是Sliced类型，中间区域是完全透明(相当于9宫格的边框)，那么尽量将 Fill Center的勾去掉,
     勾选掉即减少 Image 的显示部位.会缓解overdraw.


* 6. 如果一张图片有很多区域是空白的,默认情况下fill rate是矩形区域。PolygonImage。可以减少透明区域的fill。
     https://blog.uwa4d.com/archives/fillrate.html
     即减少 Image 的透明部位.会缓解overdraw.但是可能会使顶点/三角形变多.
     内存是足够的,overdraw 的危害比较大一些,该用就用.


* 7. Image 原生自带的图片是使用 Unity 本身自带的图片,开发过程中不能使用Unity 本身自带的图片,必须由我们本身设计的图片才行.
     在源码 MenuOptions.cs 的 GetStandardResources 方法中,已将其屏蔽.


* 8. 一张图片有很多空白,使用 Use Sprite Mesh 会缓解overdraw,但是增加 Mesh 的线条,在 Scene 窗口中,使用 Wireframe 查看.
     此方式已经和 TexturePacker 工具打出的图片区别没有太大了.
     在一张图上的透明通道很多的情况下的区别是:
     Unity 自带工具打出的顶点数少,overdraw 高一点,TexturePacker打出的顶点数多,overdraw 少一点.
     可以使用TexturePacker导出单张图片,然后使用 SpriteAtlas 组合成图集,再使用 Use Sprite Mesh 即可,
     此方式与直接使用TexturePacker插件效果一致.


