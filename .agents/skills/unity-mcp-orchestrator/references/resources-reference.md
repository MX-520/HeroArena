# 资源发现参考

不同 Unity MCP 版本可能提供 editor state、project info、instances、scene hierarchy、gameobject/component、custom tools 等只读资源。先查看当前服务资源目录或工具说明。

可能遇到的 URI 线索包括 mcpforunity://editor/state、mcpforunity://project/info、mcpforunity://instances；只有服务实际列出时才使用，不能把参考当已连接证据。

检查目标工程/实例、场景路径、Play Mode、编译/导入、工具就绪和阻塞原因；对层级结果按真实 next_cursor 分页，不硬编码响应结构。

当前实际工具中没有资源读取能力时，可用服务提供的查询工具或本地文件核对，说明无法观察的运行状态。
