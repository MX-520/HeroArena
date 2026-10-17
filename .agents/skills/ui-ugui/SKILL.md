---
name: ui-ugui
description: 编辑和验证 Unity uGUI Canvas、RectTransform、TMP、布局与按钮交互；适用于现有 UI 修改和新界面。
---

# Unity uGUI

先检查现有 Canvas、层级、组件、序列化引用与运行脚本，复用项目风格和已存在的 TMP 字体。

- 原位调整目标属性，保留引用和回调；重建层级需有实际理由并迁移引用。
- RectTransform 检查 anchor/pivot/offset 与最终尺寸；先查父级 LayoutGroup 对子尺寸的控制，避免互相争夺。
- ContentSizeFitter 与 LayoutGroup 可在职责合理时共存；重点判断同一轴的尺寸所有权，不按组件组合一概判错。
- 中文字体检查缺字、实际对比、边框/描边与用户要求；装饰关闭 raycastTarget，交互目标保留射线。
- 检查实际 EventSystem/Input 模块、GraphicRaycaster、遮挡和 Button 事件。以完整菜单为需求时接通交互，不默认只交付外观。
- 暂停反馈用 unscaled 时间，验证悬停/移出/点击、不同画幅和菜单/暂停/结算流程。
- 必要时用 UnityEngine.UI 完整类型名消除命名冲突；脚本仍按项目目录归属。

ScrollRect 的条件性配置见 [scrollview-setup.md](references/scrollview-setup.md)。TMP 资源已存在时不重复导入；避免会阻塞自动化的交互导入弹窗，确需安装时核对当前版本支持的非交互 API。

验证画面与实际事件，不以颜色字段已经修改代替可见性和交互结果。
