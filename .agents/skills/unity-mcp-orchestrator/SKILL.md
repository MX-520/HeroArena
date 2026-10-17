---
name: unity-mcp-orchestrator
description: 在实际连接的 Unity MCP 环境检查编辑器、操作对象与资产、验证结果；接口以当前服务 schema 为准。
---

# Unity MCP 操作

先确认本次宿主确实提供 Unity MCP 工具及目标工程连接；工程里安装包不等于当前可调用。

1. 查询实例、工程、场景与编辑器状态，确认目标及编译/导入是否完成。
2. 按当前工具 schema 和只读资源定位对象/组件，再实施已授权操作；不要将参考里的工具名、URI 或参数当成固定 API。
3. 独立查询可批量执行；对象创建、组件配置、引用绑定等有依赖的修改按顺序执行。
4. 脚本更改后等待编译并读错误；写入工具已触发导入时不重复刷新。画面变更查看实际截图，交互和玩法验证真实运行结果。
5. 断线/忙碌时检查 domain reload、实例和工具组；有限重试后走已有编辑器桥接或报告缺口，不把缺工具当作已完成操作。

不要为调用某个 API 随意升级包。不得隐式关闭/保存用户编辑器或运行旧 Setup；现有批处理验证会退出 Unity，先读项目 12 文档。

按需参考：[资源发现](references/resources-reference.md)、[工具 schema 核对](references/tools-reference.md)、[项目操作流程](references/workflows.md)。示例是查找线索，当前服务 schema 和实际结果优先。
