# 主角Idle定稿：头腰起伏与披风联动

2026-10-18用户明确反馈“这次满意了”，并要求整理知识库/skill、删除不需要的文件。本目录是当前主角Idle的权威版本（ANIM-003），旧候选已清理，简要迭代依据见[归档](../../../ProjectKnowledge/archive/2026-10-18-idle-iterations.md)。

## 直接使用

[Hero-Idle-Flow-strip12.png](Hero-Idle-Flow-strip12.png)：7680×640透明RGBA，横排12格，每格640×640；8fps、1.5秒循环，格内左上角根锚点[320,544]，对应Unity归一化pivot[0.5,0.15]。Unity使用Multiple、640×640网格、Max Size 8192、Point/无压缩，PPU/Visual比例仍需按实际显示体高校准。单张PNG是默认交付，不另做对照页或ZIP。

白发红眼、空手、深色皮甲、轻像素绘景；脚底稳定的站姿上方有清楚的头腰起伏，披风中段及末端延迟形成波浪。这个已认可效果是后续同类主角动作的参考，不把12帧、具体幅度或时长变成所有角色的模板。

## 保留的制作依据

- [最终生成原板](Hero-board.png)、[有效提示词](生成提示词.txt)、[帧清单](Hero/manifest.json)与12张独立帧。
- [角色/画风输入](references/hero-identity-reference.png)、[哥布林运动输入](../FourCharacterIdleTrial/Sword-board.png)、[已认可披风输入](references/cape-approved-reference.png)。两张来自旧目录的输入已迁移到本目录，SHA256保持不变；各输入职责不能混用。
- [动作构造草案](pose-guide.png)与pose-targets.json：辅助约束头腰/披风延迟关系，不是最终角色或实际运动曲线测量。
- generation-record.json及[用户认可记录](acceptance.json)：标清选稿、认可范围、原输入与清理情况。两组六姿势分段因尺度/边界不一致未选用，其图片/提示词/辅助脚本已删除，原因留在归档中。
- extraction-report.json、browser-verification.json、motion-inspection.json与Hero/technical-report.json保留原检查证据。浏览器播放功能通过不等于自动美术验收；本次视觉认可来自用户。

重新提取：`python build_idle.py --source-root 本目录 --out 新输出目录`。验证源文件verify_idle.cjs沿用本次本机Edge/Playwright路径，需先用动画skill审查器生成Hero/review/review.html；不把它当作跨机器即用的工具。

## 验收范围

用户已认可本版视觉效果。此前检查覆盖12张帧alpha>0像素完整提取、所属原板RGBA保留、横排各格与独立帧逐像素一致，以及12帧定位/控制/超过两轮正常速度循环。清理时复查最终图集和原板哈希、依赖及像素一致性，未重跑Unity游戏测试。

原报告中的候选/待审字样记录当时技术阶段，以acceptance.json中的最新认可为准。仍不宣称精确正弦曲线、靴底逐像素完全固定或严格统一低分辨率像素格已被证明。本任务未实施正式角色替换、Clip事件/碰撞/排序回归或构建；用户测试目录的导入改动需后续按现场核对。许可和参考授权继续按项目13维护。
