# 角色动作第一批

2026-10-18。内置 image_gen 生成，以已确认的 `../Hero/HeroReferenceStudy-v2.png` 和当前 Q 版哥布林为身份参考。逐图提示词与输入来源见同名 `.generation.json`。本批为已导入 Unity 的候选动作，未替换正式场景、Prefab 或 Animator。

## 覆盖范围

| 角色 | 动作 | 独立身体帧 |
|---|---|---:|
| 主角 | 待机、行走、奔跑、翻滚、近战、受伤、死亡 | 28 |
| 普通哥布林 | 待机、行走、近战、受伤、死亡 | 16 |
| Runner | 待机、奔跑、挥斧、受伤、死亡 | 16 |
| 远程哥布林 | 待机、行走、施法、受伤、死亡 | 16 |

身体合计 76 帧，另有 3 张独立武器精灵，共生成 22 个 AnimationClip。`GoblinMelee-v2`、`GoblinRunner-v2` 修正了 v1 待机朝向不一致的问题；v1 仅保留追溯，不参与导入。

## 查看与导入

- 本地 `preview.html` 可直接打开，支持播放、切换动作、调速、镜像和逐帧。它由 `inspect_layout.py` 与 `preview-template.html` 生成，不需要网络；预览保留原图像素，不修改 PNG。
- 运行 `python ArtSource/Characters/Actions/inspect_layout.py` 检查 alpha、计算逐行透明间隔、生成 `import-layout.json` 和预览。需要 Pillow/NumPy。切片按实际透明间隔与角色边界，不假设生成网格完全等距。
- Unity 菜单 `Tools/HeroArena/Characters/Prepare Candidate Actions`，或编辑器桥接命令 `prepare-character-actions`，导入到 `Assets/Art/Characters/Generated`。仅在 Edit Mode 执行；Play Mode 下报告错误并保持状态，不保存或关闭用户场景。
- 导入器复制既有动画并替换 Sprite 曲线，保留原时长、帧率、循环与事件。主角近战在 0.333 秒命中、0.75 秒结束；普通哥布林 0.4/0.9 秒，Runner 0.417/0.75 秒，法师 0.3/0.55 秒。

## 本次验证与限制

- 当前 Unity 编辑器已执行导入：79 个 Sprite、22 个 Clip 通过数量、时长、帧率、事件全部字段、循环及新资源引用检查，报告见 `unity-import-report.json`。
- PNG 均为 RGBA；76 帧身体与 3 张武器的主体边界未触及推定切片边界；alpha 原样保留，阈值仅用于检测边界，不代表已经完成清边。
- 本机无头 Edge 验证四角色预览页加载、切换攻击、镜像、逐帧，无页面异常；待机及攻击截图已查看，见本机 `Logs/Characters/actions-*-preview.png`。
- 这不是完整玩法回归。四帧移动循环的左右腿交替、不同动作的头部大小/横向重心、边缘半透明和实际游戏像素密度仍需继续评审；不能因导入成功标记整套动画验收通过。
- 身体动作空手，武器未绑到正式角色。握点、遮挡顺序、各帧剑/斧角度、普通/巨大化及左右朝向的命中范围校准均未完成。不能将空手动作片段直接覆盖正式武器动画。
- 沿用既有循环设置，包括原本循环的受伤/翻滚等；正式接入时结合现有状态机验证退出和死亡重开，避免素材更换改变玩法行为。

来源记录沿用 `../README.md` 与 `ProjectKnowledge/13`；生成资产不自动等于完成商业授权审查。
