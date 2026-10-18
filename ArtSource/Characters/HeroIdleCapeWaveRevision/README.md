# 主角Idle披风波浪修订

2026-10-18用户反馈四角色Idle“都不错”，只要求主角披风动作幅度更大、更具波形。本次基于[主角原板](../FourCharacterIdleTrial/Hero-board.png)，用内置image_gen定向编辑披风，保留白发红眼、皮甲、空手姿势、脚底、呼吸和第6帧眨眼等设计，敌人素材未改。

## 使用与复现

最终候选：[Hero-Idle-CapeWave-strip8.png](Hero-Idle-CapeWave-strip8.png)。5120×640透明RGBA，横排8格，每格640×640，根锚点[320,544]；5fps、1.6秒循环，Unity使用Multiple/640×640网格、Max Size 8192、Point/无压缩。继承原版规格，不覆盖正式动画。输出副本在本聊天outputs/HeroIdleCapeWave，只有一张PNG，无对照页和ZIP。

提示词见[生成提示词](生成提示词.txt)，原板为Hero-board.png，独立帧与时序见[manifest](Hero/manifest.json)。`python build_idle.py --source-root 本目录 --out 新输出目录`可按原像素重新整理；内部播放脚本需要先用skill审查器生成Hero/review/review.html，依赖本次本机浏览器路径，不视为通用便携工具。

## 动作意图与实际观察

披风根部固定在肩颈，布料中段先形成起伏，自由端延迟跟随；第3–4帧形成更高波峰，第5–7帧回落/卷曲，第8帧接回第1帧。保持原有红色破边布料，不加风线或粒子。静态查看可见S形褶皱与更大的高低差，八帧人物均完整且空手。生成编辑不能保证身体像素与原版完全一致；保留造型/姿势意图，服装细节仍可能有轻微漂移。

## 验证与认可范围

8张帧所有alpha>0像素完整提取，所属RGBA与生成原板逐像素一致，横排图集各格与独立帧逐像素一致。足底16行中点作暂定锚点，未缩放。报告见extraction-report.json。披风远端红色区域平均Y的帧间跨度由约2.64px增加到54.37px，仅作变化诊断，不等于实际尾尖振幅、波形质量或用户验收；记录见cape-motion-measurement.json。

内部浏览器8帧定位、前后帧/复位/洋葱皮/剪影、超过两周期正常速度循环通过，无脚本错误，见browser-verification.json；这只证明播放功能。上一版总体已获用户认可，三类敌人继续沿用；用户随后明确认可本版披风（“这个可以”），并要求主角增加像素感以适配地图；最新 [轻像素绘景候选](../HeroIdlePixelRevision/README.md)仍待视觉确认。本版未接入Unity或验证游戏表现。
