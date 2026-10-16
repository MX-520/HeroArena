# 完整 Demo 迭代（2026-10-16）

计划：玩家射击 → 远程哥布林 → 第三波及难度曲线 → 回血/弹幕/巨化/护罩掉落 → 完整UI与游戏流程。每个模块验证后单独提交。六项均已实现并完成自动验证；下面按开发顺序保留各阶段记录，最终交付结果见文末。首版难度仍待人工实玩验收。

## 已验证：主角射击
左键持续朝鼠标发射，J近战。单发12伤害、0.32秒间隔、速度16、存活3秒。暂停/受击/翻滚/死亡/近战动作期间不能开火。扫掠检测避免穿越薄墙，复用DamageReceiver→Health。Unity Play Mode真实命中、无敌、冷却、墙体和禁射检查通过。未完成手动平衡验收。

## 素材来源
内置 imagegen 生成透明PNG，已保存为 Assets/Art/Combat/CombatAtlas.png。原图保留在本机生成目录。图集为3列2行：Heal、Barrage、Giant、Shield、Bolt、Crossbow。

生成提示词：
Use case: stylized-concept. Asset type: Unity 2D pixel action RPG sprite atlas. Create ONE transparent sprite sheet, landscape 3 columns by 2 rows, six exactly equally sized cells, each icon centered with generous fully transparent margin and no overlap. Top row left to right: red healing potion flask; amber magical rapid-fire icon showing three tiny flying projectiles; purple giant's gauntlet/fist with upward growth spark. Bottom row left to right: cyan protective shield with white glint; a horizontal gold magic arrow projectile pointing right; a small goblin handheld crossbow pointing right, wooden stock with violet energy gem. Sharp chunky pixel art, each icon appears as a 32x32 pixel design enlarged with nearest-neighbor edges, dark brown outlines, limited saturated palette, readable silhouettes. Match a cute top-down forest goblin action game. No characters, no text, no labels, no frames, no background, no ground shadows, no gradients outside icons. All six sprites fully isolated with genuine transparent background. Fixed regular 3x2 grid.


## 2026-10-16 远程哥布林

远程哥布林现已实现验证。40HP、速度2.3、射程6、1.9秒攻击冷却、8伤害、子弹速度6.5；复用0.4秒命中动画事件作为瞄准预警窗口，贴近2.5单位后退。受击/死亡取消瞄准与发射。测试已验证真实命中和中断。


## 2026-10-16 第三波与难度曲线

第三波与难度曲线已实现并通过Play Mode：第一波First Contact为3普通（150总HP）；第二波The Hunt为2普通+2Runner（200总HP）；第三波Crossfire为2普通+2Runner+2弩手（280总HP）。三波分别3/4/6个敌人，保持基础血量而增加追击和远程协同压力，波间休息3秒。累计13次死亡后胜利；重打清零击杀数。六个出生点均验证NavMesh。当前数值为首版，难度体感与通关时间仍需玩家样本，不能视为平衡已验收。


## 2026-10-17 掉落与增益

四种掉落已实现并通过Play Mode：回血25（满血不消耗、上限裁剪）、弹幕6秒三路自动射击、巨化8秒（视觉1.5倍、近战范围1.5倍、近战和子弹伤害1.8倍、移动/翻滚速度1.25倍）、护罩5秒。临时效果同类刷新时间，不叠乘，暂停冻结计时，死亡清除。护罩状态与翻滚无敌独立。巨化仅扩大视觉和攻击，不扩大脚部碰撞，避免穿行与场景卡碰问题。

敌人35%随机掉落，障碍物70%；随机四类等权。每波首个生成敌人保证掉一份回血，死亡仅生成一次。道具30秒后消失；重新开波清理道具与子弹。自动验证涵盖四类Prefab类型、治疗边界、时效刷新及恢复、护罩与翻滚隔离、敌人与障碍物死亡掉落、防重复领取、死亡和重启清理。


## 2026-10-17 完整UI与整局流程（最新状态）

完整UI与游戏流程已实现验证，操作为WASD/方向键移动、Shift奔跑、Space翻滚、LMB射击、J近战、Esc暂停。正式挑战死亡失败；重开和返回菜单重载场景，恢复整局。开始按钮射线命中和回调、暂停冻结、13敌人胜利、失败和恢复世界自动检查通过，六张画面检查通过。难度曲线是首版设计，尚无人工玩家样本，自动强制击杀的计时不作为平衡证据。


### 首版难度曲线依据

| 波次 | 敌人数 | 总HP | 组合目标 | 设计压力预算 |
|---|---:|---:|---|---:|
| First Contact | 3 | 150 | 学习鼠标瞄准、近战与翻滚 | 3 |
| The Hunt | 4 | 200 | 引入同波Runner警戒与持续追击 | 5 |
| Crossfire | 6 | 280 | 快速追击和远程瞄准交叉施压 | 8 |

预算按普通=1、Runner=1.5、弩手=1.5估算，仅为设计比较，不是实测战斗指标。弩手40HP、8伤害、1.9秒冷却、0.4秒预警、弹速6.5；保持各类基础数值，依靠阵容递进，避免简单堆血。每波保证回血、波间3秒提供恢复窗口。之后应记录人工通关时间、各波受伤次数、失败波次和道具使用率再调整；当前不宣称平衡已验收。


## 2026-10-17 最终交付验证

| 检查 | 结果 | 本地证据 |
|---|---|---|
| 主角射击：实际伤害、无敌、冷却、受击/暂停禁射、薄墙 | 通过 | Logs/full-demo-shooting.txt |
| 弩手：预警、实际伤害、受击与死亡中断 | 通过 | Logs/full-demo-ranged.txt |
| 三波：阵容、导航、顺序、13击杀、重置 | 通过 | Logs/full-demo-waves.txt |
| 掉落：四类配置、治疗边界、实际三路22伤害弹幕、刷新/过期、死亡/重启清理 | 通过（增强断言在组合回归中执行） | Logs/full-demo-regression.txt |
| Runner：同组联动、跨组隔离、无敌、致命伤害、禁用/死亡、受击恢复 | 通过 | Logs/full-demo-regression.txt |
| UI：按钮射线和回调、暂停冻结、胜利/失败、整局重开及返回菜单 | 通过 | Logs/full-demo-ui.txt |
| 六个画面：菜单、HUD、第三波、暂停、胜利、失败 | 已检查通过 | Logs/demo-*.png |
| 最终组合回归 | 通过 | Logs/full-demo-regression.txt |
| Windows x64 正式构建 | 成功；0错误、1警告；93,645,341字节 | Logs/windows-build.txt |
| Windows独立运行启动 | 已启动；本次启动日志未发现异常/错误 | Logs/player-smoke.log |

程序：Builds/HeroArena/HeroArena.exe。运行和分发需要保留同目录的HeroArena_Data、MonoBleedingEdge和UnityPlayer.dll等依赖，不能仅复制exe。构建和日志按仓库规则忽略，不提交二进制；构建入口CompleteDemoBuild.BuildWindows可复现。当前Windows程序保持运行供实玩。

功能提交：af5750b（射击）、86cf87a（弩手）、7965fd9（第三波与难度）、86d9768（掉落）、cb4e3fc（完整UI与流程）。独立运行启动检查不等同人工完整通关或平衡验收。


## 2026-10-17 实玩反馈：攻击输入与多弹道

右键近战，保留J兼容；左键持续射击。原Barrage改为10秒三弹道增益，不再自动开火、不提高射速，单轮仍0.32秒。已通过合成鼠标实际输入回调、无左键不自动射击、三条子弹、冷却及完整10秒过期测试。测试中临时忽略无窗口焦点过滤并配对设备，未改正式游戏输入设置。日志Logs/full-demo-feedback-input.txt。


## 2026-10-17 实玩反馈：远程敌人与石柱导航

实现：远程哥布林使用独立 RangedShoot Animator override，0.3秒发射事件、0.55秒结束事件，移除弓弩子物体，改用紫色能量弹。开火起点按绝对时间计算1.2秒间隔；步速2.3，不启用Runner动画。复用既有RunnerGroup实现受击/致命伤害同组警戒与持续锁定，同波Runner组和远程组独立。受击/死亡仍取消发射。射线被实际障碍遮挡时重新寻路，贴近2单位开始后退，拉到3单位结束；找不到有效后退路径则短暂停留射击，避免状态反复切换。

石柱已具备独立导航代理碰撞体，无需增加图片。将导航代理按敌人脚部偏移对齐，保留已有增大的代理范围；烘焙半径与Agent半径统一为0.5，体素0.08，生成NavMesh-CombatNavigation.asset，原用户修改的NavMesh-Navigation.asset保留。追击目标投影到可达NavMesh、0.2秒缓存路径，朝向忽略微小水平避让速度。

复现与证据：单敌人8个贴柱位置未复现；普通/Runner/两远程混合测试复现Runner在第2/3柱3.5秒内翻转418/211次。已确认烘焙半径原0.1而Agent半径0.5、脚部中心与导航代理参考点不同；不能据此断言任一项是唯一原因。综合修复后四柱各敌人翻转均不超过4次，近战最近距离0.80/1.10/1.20/0.95，8个单敌人路径均完整。证据：Logs/pillar-crowd-baseline.txt、pillar-crowd-fixed.txt、pillar-probe-fixed.txt。固定测试已通过；复杂动态走位仍需实玩观察。


组合回归：CompleteDemoVerification.RunFeedbackAll通过（Logs/full-demo-feedback-all.txt）。包括三次实际射击间隔/独立动画、受击联动、超出侦测范围持续锁定、单敌人8处及四柱混合敌人、完整旧战斗/掉落/Runner/三波回归和真实右键/10秒多弹道。此次混合回归四柱最大翻转3次，近战最近距离0.80/0.98/1.20/0.94。测试自身的短时增益配置已在Drops检查结束时恢复，避免污染后续10秒检查；曾将攻击间歇误作丢失目标的断言已改为检查锁定及状态。


本轮交付：图形模式RunVisualUI通过，六张截图已检查（Logs/demo-*.png），HUD显示10秒多弹道和RMB/J。Windows构建成功，0错误、1警告、93,634,581字节，证据Logs/windows-build.txt、Logs/feedback-build.log。新版程序Builds/HeroArena/HeroArena.exe；编辑器通过CombatFeedbackSetup.OpenReviewScene显式打开正式场景。本轮功能提交8c4bde5、952929d。自动定点回归通过不等同所有动态围堵和难度已人工验收。
