# Layout

* 1. Layout 静态时可以用,动态时最好不要使用.非要用的话就加一个 canvas,动态时不让它与外部 UI 进行合批
    
* 2. 推荐插件  UITableView 来解决重用 cell ,使 UI 布局的 Cell 计算是按需计算,而不是按次计算.
    
* 3. 能少用就少用,用了尽量不要动态!

# ScrollView

* 1. 在 ViewPort 上使用 RectMask2D 比使用 Mask 与 Image 少一个 Batch,添加 Canvas 进行动静分离.
    原因是每当滑动时,组件都会被移动,需要重新布局,重新渲染.
    
* 2. 当 scroll view 上有很多 Layout UI重建成本非常高,不建议嵌套使用.
    
* 3. 推荐插件  UITableView 来解决重用 cell ,使 UI 布局的 Cell 计算是按需计算,而不是按次计算.
    
* 4. UITableView 缓存池,按需计算

