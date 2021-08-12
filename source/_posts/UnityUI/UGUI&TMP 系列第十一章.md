---
title: Unity UGUI优化
date: 2020-05-11 11:41:32
top: 111
categories:
- UnityUI
tags:
- UnityUI
---

# TextMesh Pro 分析


## TextMesh Pro 的用法

    
* 1. 制作静态字体时,选择 Windows/TextMeshPro/Font Asset Creator
     Source Font File 属性需要选择一个可以支持中文的字体,这个字体是原生的 ttf或者 ttc 等等,但是必须支持中文,字体的名字不能是中文.
     Atlas Resolution 选择适当的大小(2048x2048),太小的情况是文字展示不全或者显示不正确/清晰,太大是生成的字体文件超过几十 M,都不可取
     这个设置的太大又会导致静态字体文件过大(中文 7000 多字超过 150M 大小,这个明显接受不了),普通情况下的字体文件是只有20M 大小左右
     Character Set 选择文字字符文件选项.Character File 选择一个文本文件,把需要的字体都输入进去.
     Render Mode 一般选择 SDFAA 模式,这个是字体算法,通过算法生成一张图,然后从图中取出文字再展示到 UI 上面.
     点击 Generate Font Atlas  -->> 然后 Save 保存到项目内即可.
     在生成的静态字体 xxx.asset 的 Inspector 检视面板上可以看到 Atlas Population Mode 是静态的.
     静态字体的打包情况是,只有一个 xxx.asset 大约 20M 左右.
     
* 2. 制作动态字体时,选择一个支持中文的字体,比如 Assets/UIKit/Fonts & Materials/MSYH 微软雅黑字体,         
     制作完成时,一定要查看 console 面板的警告,如果出现某些字体不能正常显示的警告,就说明此动态字体存在缺憾.        
     选中右键 UI/TMP - Font 即制作了一个动态字体. 初始情况下,TMP 的动态字体的 Atlas 图是空的,也表示没有使用过.        
     如果将字体文件设置为 TMP 当前使用的字体文件,即在 UI 中使用,并且给此 TMP - Text 赋值了. 这个字体的 Atlas 图就不是空的.          
     动态图的宽高也是有一定限制的,如果不停的向里面添加文字,会达到上限,这个时候就必须勾选 Multi Atlas Textures 多张图集属性.        
     然后将 Atlas Width 与 Atlas Height 选择 2048 x 2048 即可.      
     在编辑器下,动态字体的图上显示的文字是可以保存下来的,但是在 Runtime 的时候,即手机 app 上,使用过会被重置.
     动态字体的打包情况是,有 2 个,一个是 xxx.asset,大约 3M,加上引用的 xxx.ttf 字体.总大小也是 20M 左右.
     
* 3. 后备字体,相当于很多个字体文件集合成一个字体文件.耗费性能,需要遍历.
     从 TMP - Text 的字体来顺关系,先找这个A字体里面的文字,找不到,去找这个 A 字体的后备字体 B,再循环找 B 的后备字体 C.
     都找不到,就去找 TMP Settings 中的后备字体 D,再循环找后备字体.再找通用默认字体,循环查找通用默认字体的后备字体
     永远找不到的情况下就显示方块.
     请求字符 --> UI 设置的字体 --> 循环查找 UI 设置字体的后备字体 --> 通用的后备字体 --> 循环查找通用的后备字体的后备字体 --> 通用默认字体 --> 循环查找通用默认字体的后备字体 --> 显示方块.
     
         
* 4. 最佳使用方式,静态字体(默认字体,主字体,默认只有一个)+动态字体(后备字体),使用多套字体的情况是需要多套后备字体.
     选择 Edit --> Project Settings --> TextMesh Pro --> Settings
     设置 Default Font Asset 为静态字体,此项为常用字体,可设可不设后备字体.
     设置 Fallback Font Assets(后备字体) 为动态字体,此动态字体不要设置与常用字体的后备字体一致.
     开发与发布时使用静态字体,把动态字体做成通用的后备字体.这样子就避免了字体显示不出来,字体因为快速切换造成的性能损耗.
     

## TextMesh Pro Inspector

* 1. TextMesh Pro 与 UGUI 组合使用时,创建的 TextMeshPro - Text 对象 需要RectTransform,CanvasRender 对象,才可以正常展示出来.

* 2. TextMesh Pro 与 MeshRenderer 组合使用时,创建的 TextMeshPro - Text 对象 需要RectTransform,MeshRenderer 对象,才可以正常展示出来.

* 3. 上面 2 项都需要 TMP 的 shader.

* 4. 如果要使用特殊的字体效果,需要改换 shader,或者自己写一套,这个地方需要检查性能是否有必要.

* 5.

## TextMesh Pro 优化细节

* 1. TMP 的字体如果经常修改,请在其父节点添加一个 canvas 保证不影响其他UI.
    
* 2. 其与优化暂未完成.

## TextMesh Pro 打包

* 1. 动态字体,静态字体,还有原生字体,都需要单独打包,加载时依赖加载,避免出现多份字体文件.
     
* 2. TMP 的 shader 统一放入 Resources 文件夹下,默认提前加载.字体在 Resources 存在一份静态字体和动态字体,作为主字体进行开发以及运行时展示.
     
* 3. 后续shader扩展按照正常加载 AB 模式进行扩展加载.后续字体扩展按照正常加载 AB 模式进行扩展加载.
     
* 4. 图片按照正常的 ab 加载模式加载即可.
     


## TextMesh Pro 原理


* 1. 矢量图(TextMesh Pro)与点阵位图(UGUI/NGUI Text/UILabel).
     SDF 是一个矢量算法,Signed Distance Field(无损缩放),SDFAA是一个通用的,以较小的数又加上了一个抗锯齿的算法去生成图.一般就选择这个.
     记录的是一个点与边缘的距离,距离分正负.
     https://zhuanlan.zhihu.com/p/26217154

* 2. TextMesh Pro中生成的 xxx.asset 字体文件里面包含一张图片,这张图片应该是矢量图?
     这张图片可以与 shader 尽情融合反应(意思是指 shader 可以影响这张图,进而影响TextMesh Pro中的任何字体),
     所以各种特炫酷的效果都能满足了,原来的 Text 是不可以这样做的.

* 3. 点阵位图字体(Text/UILabel),点阵字体系统把每一个字存储成固定大小的点阵位图，常见的大小有16×16和32×32等。其中每个字形都用一组二维像素信息表示.
     优点是具有简单快速的显示方式,且不受硬件限制;缺点是实现高质量的缩放相对困难,特定的点阵字体只能清晰的显示在相应的字号下,
     否则显示的自行只能因被强行放大而失真,产生马赛克锯齿边缘.因此为了适应不同字号,存储量大幅度上升.
    
* 4. 矢量字体(TextMesh Pro),每一个字形都表示成一组数学曲线描述的轮廓,它包含了字形边界上的关键点,连线的导数信息等,在显示字体时,
     渲染引擎通过读取其数学矢量,并进行一定的数学运算来实现渲染,字的内部则通过光栅化来填充.矢量字体主要包括 Type1 和 TrueType 等几类.
     矢量字体的优点是存储空间小,可以无限缩放而不产生变形.缺点是显示系统复杂，需要很多操作才能显示出矢量资源，因而速度较慢，也不适用于一些硬件。

* 5. 动态字体的原理,虽然很方便,但是需要一定的计算量.
     A : 先找到xxx.ttf原生字体,
     B : 然后通过 SDAFF 算法,生成文字到 TMP 的动态字体 xxx.asset 下面的贴图上,
     C : 从贴图上取出文字,显示在屏幕上面.
    
* 6. SDF(Signed Distance Field),有符号距离场,简单理解:屏幕中的每个像素点都需要与一个物体的当前点进行比较,在外部为正,在内部为负,在边缘为 0.
     这样就没有顶点插值,因为每个点都是使用距离来进行计算的.相比于Raytracing光线追踪来说,精度低,性能小.
     SDF 在放大缩小的情况下,它的距离也是按比例放大缩小的,图片或者文字的边缘距离屏幕上像素点是动态变化的,并且也会放大缩小,
     当文字放大是,这个像素距离仍然是比例放大,文字本身的像素不与周围的像素进行插值融合,而是原本自身的像素,所以看起来非常清晰,并不模糊和有锯齿感.

## TextMesh Pro 的优势与不足

* 1. 字体清晰,看起来更舒服,放大缩小完全没有任何模糊问题.相比于原生的Font (Text/UILabel) 都是优势
     
* 2. 图文并排更方便了.
     
* 3. 更加容易做出来特殊效果,制作特效的效率以及效果,完全凌驾于原生的 Text.
     
* 4. TMP 的 Shader 是使用 SDF 算法编写,与UGUI 的 UI-DefaultFont 相比复杂很多,占用内存/显存也更多.
     
* 5. 字体占用磁盘空间与加载到内存中占用大小也比较多,
     按照一套字体来算, TMP 需要使用动态字体和静态字体,UGUI 使用一个 ttf 字体即可.明显是多了大概一倍左右的大小.
     

     
![TMP类图.png](TMP类图.png)
    
## 引用

* 1. https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html 官方文档地址
     
* 2. https://www.youtube.com/channel/UCfZ4egVzhZrnilkOu1Y7w6g 官方作者视频
     http://digitalnativestudios.com/ 官方视频地址
     
* 3. SDF 原理 : https://zhuanlan.zhihu.com/p/26217154     
     https://zhuanlan.zhihu.com/p/93901692
     https://zhuanlan.zhihu.com/p/25102683
     
* 4. https://www.bilibili.com/video/BV1EK411W7Bg?from=search&seid=9862616924654951473
     https://www.youtube.com/playlist?list=PLgmWhKyPSZCRLCMsYovkd49sI8x_v4nt0 (需翻墙)
     SDF 原理(视频)
     
* 5. https://forum.unity.com/forums/ugui-textmesh-pro.60/ 论坛
     
* 6. https://blog.csdn.net/u010019717/article/details/52755185
     
* 7. https://www.bilibili.com/video/BV1Kr4y1T7XB

* 8. https://www.zhihu.com/question/41950114/answer/93445003

* 9. https://baike.baidu.com/item/%E7%9F%A2%E9%87%8F%E5%AD%97%E4%BD%93/4782423?fr=aladdin
     
* 10. https://baike.baidu.com/item/%E5%9B%BE%E5%BD%A2%E4%B8%8E%E5%AD%97%E4%BD%93%E8%AE%BE%E8%AE%A1%E5%9F%BA%E7%A1%80/3026760?fr=aladdin
     图形与字体的设计基础,字体的所有信息
