# 来源与适配记录

核查日期：2026-10-18。

- 原作者：[Matt Pocock / skills](https://github.com/mattpocock/skills)。许可为 MIT，原声明保存在 [LICENSE](../LICENSE)。
- 参考版本：`49dd158d1076134a641b33efb035946536778336`，路径 `skills/engineering/tdd`。[固定版本入口](https://github.com/mattpocock/skills/blob/49dd158d1076134a641b33efb035946536778336/skills/engineering/tdd/SKILL.md)。候选通过官方 skill-installer 下载，入口 SHA256 与固定提交核验一致。
- 本地适配版：`unity-tdd`，不冒充上游原版。保留行为断言、独立预期值和逐个失败到通过原则；重写为 Unity/C# 工作流，移除 TypeScript 电商示例和不可用的外部 skill 强制依赖。已有需求/授权足够时不重复要求确认测试边界。
- Unity 模式和发现机制参考：[Test Framework 1.1 文档](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/edit-mode-vs-play-mode-tests.html)。程序集约束参考：[Unity 2022.3 Assembly Definitions](https://docs.unity3d.com/2022.3/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html)。实际项目应以本地安装版本为准。

本次验证范围：skill 结构、文档链接和安装副本一致性。未修改玩法或新增实际游戏测试；不代表 Unity 测试发现、运行或游戏回归已验证。
