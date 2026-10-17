# 音效与背景音乐（首轮接入）

准备与接入日期：2026-10-17。24个候选中23个已接入，error_001保留未使用。当前映射为 `Assets/Audio/HeroArenaAudio.asset`，由 `AudioIntegrationSetup.Apply` 配置。

## 授权

- `ThirdParty/Kenney`：作者官方 Interface Sounds、Impact Sounds、RPG Audio 发布页及原包 License.txt 均为 CC0。允许商业使用、修改和分发，不强制署名。三个原始许可保存在 `Licenses/`。
- `ThirdParty/CleytonKauffman`：Forest Whisper Theme、RPG Battle Theme II 作者发布页标注 CC0。前者原包 readme 也明确 CC0，已保留。项目将保留作者署名：`Music by Cleyton Kauffman - https://soundcloud.com/cleytonkauffman`。
- `Original`：本项目用自写振荡器、噪声及包络合成的六个声音，没有外部采样或参考歌曲旋律，可随项目商用；原始生成代码随附。没有调用第三方 AI 音乐服务。

来源、每个文件的原始名称、下载地址、许可、核对日期、用途候选和 SHA-256 见 `manifest.json`。授权核对依据见 `Licenses/SOURCES.md`。下载原包与原始页面备份在工作区 `Logs/AudioPreparation`，正式交付记录为本目录的许可文件和 manifest。

## 当前播放映射

| 场景／反馈 | 文件 |
|---|---|
| 首页／胜负结算 | ForestWhisper，循环、流式 |
| 全部三波战斗及波间休整 | BattleThemeII，循环、流式，换波不重播 |
| 按钮悬停／点击／返回／波次开始 | select / click / back / confirmation |
| 玩家／敌人近战挥动 | knifeSlice / knifeSlice2 |
| 翻滚／草地脚步 | cloth1 / footstep_grass_000～002 |
| 玩家／敌人受伤与死亡 | impactPunch_heavy / medium |
| 木质／石质破坏 | impactWood_heavy / impactPlate_medium |
| 玩家射击／魔法发射／魔法命中 | PlayerShot / MagicCast / MagicImpact |
| 回血／增益拾取 | HealPickup / handleCoins |
| 成功／失败 | Victory / Defeat |

## 实现与验证

- GameAudio随场景创建和销毁，2个音乐通道交叉淡入0.8秒；12个固定短音效声部，含3个不受暂停影响的界面／死亡／结束提示声部。按优先级抢占低优先级音效，避免战斗声吞掉重要提示。
- 暂停冻结普通战斗音效，背景音乐继续并降低至25%；界面声音正常响应。回到菜单、重开不会积累旧音源。
- 独立随机源选变体／轻微变调，不改变掉落、波次等Unity随机序列。单类音效有冷却与并发限制；Master/Music/SFX为代码增益通道，数值存入PlayerPrefs，尚无AudioMixer资产或音量UI。
- 主角射击成功时每次齐射一声；近战起手、实际翻滚、实际位移脚步；远程敌人实际发射／碰撞；生命真实受伤／死亡；成功拾取及实际障碍破坏。护罩、无敌及无效治疗不会误播放受伤／治疗声。
- 自动化技术验证覆盖上述事件、音源播放与采样推进、暂停/恢复、音量静音、声部限制、三波胜负、重开和场景重载。证据在 `Logs/full-demo-audio-all.txt` 与对应日志。

这是一轮可玩版本，未完成主观听感、循环接缝、削波及长时间疲劳验收；后续根据实际游玩调整。没有构建打包。角色/敌人旧图像授权风险另见 `ProjectKnowledge/13_素材授权与替换计划.md`，不因音频采用CC0便视作全部游戏资源可商用。
