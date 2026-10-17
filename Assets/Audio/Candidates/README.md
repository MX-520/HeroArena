# 音效与背景音乐候选（尚未接入）

准备日期：2026-10-17。当前只有素材与导入配置，游戏内还不会播放这些声音。

## 授权

- `ThirdParty/Kenney`：作者官方 Interface Sounds、Impact Sounds、RPG Audio 发布页及原包 License.txt 均为 CC0。允许商业使用、修改和分发，不强制署名。三个原始许可保存在 `Licenses/`。
- `ThirdParty/CleytonKauffman`：Forest Whisper Theme、RPG Battle Theme II 作者发布页标注 CC0。前者原包 readme 也明确 CC0，已保留。项目将保留作者署名：`Music by Cleyton Kauffman - https://soundcloud.com/cleytonkauffman`。
- `Original`：本项目用自写振荡器、噪声及包络合成的六个声音，没有外部采样或参考歌曲旋律，可随项目商用；原始生成代码随附。没有调用第三方 AI 音乐服务。

来源、每个文件的原始名称、下载地址、许可、核对日期、用途候选和 SHA-256 见 `manifest.json`。授权核对依据见 `Licenses/SOURCES.md`。下载原包与原始页面备份在工作区 `Logs/AudioPreparation`，正式交付记录为本目录的许可文件和 manifest。

## 初步分工

| 场景／反馈 | 文件 | 状态 |
|---|---|---|
| 开始界面／波间休整 | `ThirdParty/CleytonKauffman/ForestWhisper.ogg` | 森林主题循环候选 |
| 波次战斗 | `ThirdParty/CleytonKauffman/BattleThemeII.ogg` | 战斗循环候选 |
| 按钮悬停／点击／返回 | Kenney Interface 的 select / click / back | 待实机听感选择 |
| 近战挥刀 | Kenney RPG 的 knifeSlice / knifeSlice2 | 待实机音量与变调 |
| 翻滚／草地脚步 | cloth1 / footstep_grass_000～002 | 待接动画事件 |
| 玩家／敌人受伤 | impactPunch_medium / heavy | 待区分力度 |
| 木质破坏／硬质命中 | impactWood_heavy / impactPlate_medium | 待实机听感选择 |
| 射击／魔法发射／魔法命中 | Original 的 PlayerShot / MagicCast / MagicImpact | 自制候选 |
| 回血／增益／拾取 | HealPickup / handleCoins | 待分配各道具 |
| 成功／失败 | Original 的 Victory / Defeat | 自制提示短音候选 |

候选基于发布页说明、原包文件与技术检查，不代表完成听感验收。两首音乐使用同一作者，最终是否采用及音量平衡需在接入后试听。

## 下一步接入方案（计划，未实现）

1. 复用 GameFlow.Changed 切换菜单／战斗／胜负音乐；暂停淡出或低通，恢复时延续播放，重开与返回菜单避免重复音源。
2. UI 声音接 ArenaButtonFeedback 的真实进入／点击事件；连续悬停做间隔限制，暂停时仍可播放 UI 音效。
3. PlayerShooting.TryShoot 成功生成子弹才发声；EnemyAttack.DetectHit 真实发射时播放魔法声，命中声接 Projectile 的实际命中分支。
4. 受伤声接 Health.OnDamaged，死亡声接 OnDied。注意现有致命伤不会触发 OnDamaged，应避免死亡漏声或重复受击声；护罩／翻滚无敌挡住伤害时不播放普通受伤声。
5. 拾取声接成功收集，障碍破坏接死亡事件。脚步与挥刀按动作时刻触发，避免用每帧轮询反复播放。
6. 添加 Master／Music／SFX 混音分组、音量保存、同类音效并发上限与轻微变调；BGM 使用流式加载，短音效预加载。
7. 实机验收覆盖持续射击、多人同屏、暂停／恢复、三波胜负、死亡、重开与场景重载；另行检查循环接缝、削波和听觉疲劳。

音频准备步骤不修改 Scene、Prefab、Animator、Input 或战斗逻辑来播放音频，不构建打包。
