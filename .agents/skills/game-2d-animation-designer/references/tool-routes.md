# 工具路线

官方能力核查：2026-10-18；不代表本机已安装或已连接。使用时核对实际版本、账号/费用和宿主工具。

- 保留逐帧质感、复杂转身：分镜→受姿势约束生成/绘制→Aseprite或Pixelorama修帧。
- 可控运动和批量复用：分拆身体部件→补齐遮挡→Spine绑定→曲线/约束→必要替换原画→运行时或透明序列。完整PNG不是可用的分层骨骼文件。
- 仅有展示板且只需裁图：确定性裁切，报告交叠污染；动作修复从单张定稿与分镜重新开始。
- 用户在ChatGPT生成：给规格、逐帧参考和提示说明，接收结果后播放审查，保持用户工具偏好。

| 工具 | 已核查能力 | 实施前检查 |
|---|---|---|
| [Aseprite](https://www.aseprite.org/docs/animation/) | 时间轴、播放、标签、洋葱皮；[CLI](https://www.aseprite.org/docs/cli/)可导出 | 可用合法编辑器、版本/路径；实际输出文件 |
| [Pixelorama](https://orama-interactive.itch.io/pixelorama) | 时间轴/洋葱皮、PNG/GIF/精灵表导出 | GUI读取和CLI导出分别验证；退出0但无文件仍失败 |
| [PixelLab骨架流程](https://www.pixellab.ai/docs/tools/animate-with-skeleton) | 参考图、模板/参考动画骨架、手调、冻结帧和局部修正 | 新旧插件/API限制不同；按端点确认尺寸/帧数与费用 |
| [PixelLab API](https://www.pixellab.ai/pixellab-api) | 核查时skeleton-v3为beta，每帧骨架+一张参考，3–15帧、最大256×256 | 先小样测形象保持、脚接触和握持，不能保证匹配精细画风 |
| [PixelLab MCP](https://www.pixellab.ai/mcp) | 远程角色/动画/图像工具 | 账号、凭据、宿主支持；实际枚举工具才算连接 |
| [Spine](https://esotericsoftware.com/spine-demos) | 骨骼、IK、网格、曲线、替换图；[导出](https://esotericsoftware.com/spine-export)支持序列 | 许可、运行时版本；在目标尺寸看像素轮廓是否被形变损坏 |
| [aseprite-mcp](https://github.com/mattt/aseprite-mcp) | execute_lua、inspect_export，含aseprite-lua skill | 第三方操作桥，不是生成模型；自备编辑器，审查固定版本后小样 |

工具许可、模型条款、参考权利和输出使用分别记录，开源工具不自动授予输入素材权利。按任务需要适配，不为了工具齐全安装一切。生成器不可用时仍交分镜、姿势表和审查材料，并明确绘制待完成。
