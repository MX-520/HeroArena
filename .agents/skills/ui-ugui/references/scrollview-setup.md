# ScrollRect 按需配置

先检查现有布局，不要求所有列表使用同一套组件。

常见结构为 ScrollRect 根 → Viewport → Content → items；ScrollRect 正确引用 viewport 与 content。Viewport 可按项目使用 RectMask2D，或 Image + Mask，不强制两种遮罩同时存在。

内容尺寸可以由代码、LayoutGroup/ContentSizeFitter 或固定尺寸提供；不滚动时先比较 viewport/content 实际尺寸与滚动轴，再查引用和射线。

垂直自适应列表常用 Content 上 VerticalLayoutGroup 布局子项，ContentSizeFitter 用子项 preferred height 控制自身高度。避免父级布局又控制 Content 的同轴尺寸造成争夺。不要仅凭同物体有这两个组件判错。

动态填充要防重复：只清理由此列表拥有的动态项，或复用项；不要无条件删除模板与固定标题。验证重新打开、空列表、长列表、滚动边界和暂停。
