# 音效与背景音乐（首轮接入）

准备与接入日期：2026-10-17。42个音频中25个被播放库引用（包含2首已启用BGM），7个原声仅留档，10个旧候选未使用。当前映射为 `Assets/Audio/HeroArenaAudio.asset`，由 `AudioIntegrationSetup.Apply` 配置。

## 授权

- `ThirdParty/Kenney`：作者官方 Interface Sounds、Impact Sounds、RPG Audio 发布页及原包 License.txt 均为 CC0。允许商业使用、修改和分发，不强制署名。三个原始许可保存在 `Licenses/`。
- `ThirdParty/CleytonKauffman`：Forest Whisper Theme、RPG Battle Theme II 作者发布页标注 CC0。前者原包 readme 也明确 CC0，已保留。项目将保留作者署名：`Music by Cleyton Kauffman - https://soundcloud.com/cleytonkauffman`。
- `ThirdParty/StarNinjas` / `ThirdParty/Rubberduck`：作者发布页均为CC0，实录刀剑与作者法术音效；原声保留在Source，游戏剪辑及署名见 `Licenses/CC0-Combat-Sources.md`。
- `ThirdParty/ZaneLittle`：Flowerbed Fields作者页标注CC0，来源与原文件哈希见 `Licenses/CC0-Adventure-Music.md`。
- `ThirdParty/KarateStudios`：作者页CC0的RPG Overworld，保留署名Karate Studios；出处、原文件/剪辑关系见 `Licenses/CC0-Gentle-Overworld.md`。
- `Original`：本项目用自写振荡器、噪声及包络合成的九个声音，没有外部采样或参考歌曲旋律，可随项目商用；原始生成代码随附。没有调用第三方 AI 音乐服务。

来源、每个文件的原始名称、下载地址、许可、核对日期、用途候选和 SHA-256 见 `manifest.json`。授权核对依据见 `Licenses/SOURCES.md`。下载原包与原始页面备份在工作区 `Logs/AudioPreparation`，正式交付记录为本目录的许可文件和 manifest。

## 当前播放映射

2026-10-17 最新设置：按用户要求重新启用轻快冒险背景音乐，`HeroArenaAudio.musicEnabled=true`；首页/结算使用Flowerbed Fields，游戏内改用较舒缓的RPG Overworld，进入游戏交叉淡入0.8秒。原ForestWhisper/BattleThemeII留作候选。现有按钮与战斗音效保持。

| 场景／反馈 | 文件 |
|---|---|
| 首页／胜负结算 | Zane Little Music 的 FlowerbedFields，循环、立体声流式 |
| 游戏内三波／波间休整 | Karate Studios 的 RPGOverworld，较舒缓的探索冒险曲，循环/流式；换波不重播 |
| 按钮悬停／点击／返回／波次开始 | select / click / back / confirmation |
| 玩家／敌人近战挥动 | StarNinjas 实录刀刃剪辑 SteelSwingA / SteelSwingB |
| 主角近战实际命中 | StarNinjas 金属碰撞剪辑 SteelImpactA / SteelImpactB，仅实际扣血时播放一次 |
| 翻滚／草地脚步 | cloth1 / footstep_grass_000～002 |
| 玩家／敌人受伤与死亡 | impactPunch_heavy / medium |
| 木质／石质破坏 | impactWood_heavy / impactPlate_medium |
| 玩家射击 | Original 的 PlayerShot |
| 魔法发射／命中 | rubberduck 素材剪辑 ArcaneCast / ArcaneImpact |
| 回血／增益拾取 | HealPickup / handleCoins |
| 成功／失败 | Victory / Defeat |

## 实现与验证

- GameAudio随场景创建和销毁，2个音乐通道交叉淡入0.8秒；12个固定短音效声部，含3个不受暂停影响的界面／死亡／结束提示声部。按优先级抢占低优先级音效，避免战斗声吞掉重要提示。
- 暂停冻结普通战斗音效，背景音乐继续并降低至25%；界面声音正常响应。回到菜单、重开不会积累旧音源。
- 独立随机源选变体／轻微变调，不改变掉落、波次等Unity随机序列。单类音效有冷却与并发限制；Master/Music/SFX为代码增益通道，数值存入PlayerPrefs，尚无AudioMixer资产或音量UI。
- 主角射击成功时每次齐射一声；近战起手、实际翻滚、实际位移脚步；远程敌人实际发射／碰撞；生命真实受伤／死亡；成功拾取及实际障碍破坏。护罩、无敌及无效治疗不会误播放受伤／治疗声。
- 自动化技术验证覆盖上述事件、音源播放与采样推进、暂停/恢复、音量静音、声部限制、三波胜负、重开和场景重载。证据在 `Logs/full-demo-audio-all.txt` 与对应日志。

这是一轮可玩版本，未完成主观听感、循环接缝、削波及长时间疲劳验收；后续根据实际游玩调整。没有构建打包。角色/敌人旧图像授权风险另见 `ProjectKnowledge/13_素材授权与替换计划.md`，不因音频采用CC0便视作全部游戏资源可商用。

## 2026-10-17 首次打击感调整（音源已被下述CC0版本替换）

按实玩反馈停止使用原knifeSlice摩擦声。新增0.16/0.17秒挥击变体和0.20秒近战接触声，使用短空气脉冲、阻尼低频和干脆瞬态；空挥只播放挥击，DetectHit确实造成扣血才叠加接触声，每次DetectHit最多一声，无敌拦截不报命中。玩家/敌人挥击gain为0.42/0.30，接触声0.48。
魔法发射改为0.23秒压力脉冲/颗粒电裂，命中改为0.32秒低频冲击/非整数倍谐振碎裂；去掉原单一正弦下滑音和较长软起音。发射/命中gain为0.38/0.48。背景音乐继续关闭，其他音频保持。
五个新制/重制声音峰值统一约0.80，文件末端淡出，源码可逐字节重现。单文件检查不代表多人混音或主观听感已经最终验收。

## 2026-10-17 网络样例与当前音源

按用户要求从网上核验并采用两套CC0原声：StarNinjas实录刀剑、rubberduck法术音效。保留7个未修改OGG原声；6个游戏剪辑版为单声道44.1kHz PCM16，峰值0.72，1.5ms起音淡入与45ms尾部淡出。
挥击保留刀刃短促金属共鸣（0.36/0.44秒）；接触声保留碰撞余响（各0.40秒），每次真实命中随机选一个；法术发射0.44秒、命中0.56秒，叠合作者法术和火焰冲击纹理并附短回声。gain为玩家挥击0.46、敌人挥击0.30、近战接触0.50、法术发射0.40、法术命中0.50。原先五个项目合成战斗声音保留作候选，当前不播放。
处理代码、派生表、来源、原包和文件哈希均随附。可复现剪辑与导入/播放技术检查不代表主观听感最终通过，仍待实玩试听。BGM继续关闭，出生/重生、伤害、动画、导航和UI不因本轮音源替换而修改。

## 2026-10-17 轻快冒险BGM

当前启用Flowerbed Fields [Loop]（Zane Little Music，CC0），时长105.93秒、44.1kHz立体声，保留原OGG字节。开始界面立即播放，进入战斗同曲连续，暂停压低至25%，恢复回升；重载不复制音源。库的musicGain=0.45，乘在已有Master/Music用户偏好之后，不影响SFX，避免覆盖已接受的战斗声音。完整听感平衡与循环衔接留待实玩确认。

## 2026-10-17 整体提高音量、音效优先与舒缓游戏曲

音乐库musicGain从0.45提高到0.70（原曲约+3.84dB），新增sfxGain=1.65使按钮、动作、命中、拾取等音效约+4.35dB；用户Master/Music/SFX偏好不被覆盖。默认偏好下，单音效增益总系数从0.60变为0.99，再乘各cue增益。
游戏内替换为Karate Studios的RPG Overworld（CC0），原FLAC剪辑成56.52秒/48kHz立体声WAV，去尾部静音、首尾短交叠、峰值约0.60；首页保留Flowerbed Fields。两首音乐以0.8秒交叉淡入，换波不重播。
普通战斗声部中priority>=2的有效声音播放时，音乐内容在0.16秒保护窗口内短暂降到68%，再平滑恢复；UI声音不会触发这层压低。暂停的25%压低继续保留。整体听感与音效是否足够突出仍需实玩确认。
