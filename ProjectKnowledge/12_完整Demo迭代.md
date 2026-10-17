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


## 2026-10-17 后续实玩反馈：撤回石柱导航修复

用户反馈其他障碍物绕行范围变大，要求从Git恢复原寻路。上轮全局烘焙半径0.1→0.5会影响所有障碍物净空，定点石柱回归没有覆盖这个体验影响。现从8c4bde5恢复NavMeshAreas、原NavMesh-Navigation.asset、四个石柱导航代理和NavMeshSurface参数/引用；恢复EnemyController原ChasePlayer/Retreat方法和2.5单位后退规则，移除路径缓存/目标投影/朝向阈值/新增视线绕行。普通障碍物碰撞体和既有脚部碰撞设置不改。

保留独立施法动画、1.2秒射击、持续步行追踪/同组警戒、右键/J近战与10秒手动多弹道。子弹视觉从0.9放大到1.35（直径+50%）。配置器不再修改任何导航配置；原SetupEnemiesAndNavigation兼容入口仅配置敌人，推荐入口SetupEnemies。删除已废弃的NavMesh-CombatNavigation资产；用户新建NavMesh-Navigation 1资产保留不动。

静态验证：追击/后退方法与原Git文本一致；原导航数据、NavMeshAreas与原Git字节一致（Logs/navigation-rollback-static.txt）。石柱原摇摆问题暂缓，已从组合回归必过项移除，历史定点通过记录不代表当前版本仍保留该修复。组合Play Mode回归通过（Logs/full-demo-feedback-all.txt），已覆盖原导航参数/引用、放大子弹、独立施法与1.2秒间隔、持续锁定和旧战斗/道具/三波/右键输入。Windows构建成功，0错误、1警告、93,645,101字节（Logs/windows-build.txt、Logs/navigation-rollback-build.log）；新版已启动。


## 2026-10-17 隔障碍提前攻击修复

哥布林子弹视觉缩放2.7（在1.35基础上直径加倍）。敌人攻击入口、维持与动画结束统一检查目标可达：近战需实际攻击圈覆盖玩家碰撞体，近/远程均需无实体遮挡；命中事件再次检查遮挡。受阻时继续原追击。导航参数、网格和场景碰撞体未改；本轮遵照用户要求不构建打包。组合Play Mode回归已通过，实际场景绕障测试已通过：Obstacles_03上方静止玩家，普通哥布林和Runner均从下方绕至无遮挡攻击位置，PathComplete，停止根距离1.20（Logs/obstacle-approach.txt、Logs/full-demo-obstacle-approach.txt）。

复现：Unity固定薄障碍夹在玩家与普通哥布林脚部碰撞体之间，根节点距离0.9，旧UpdateChase进入Attack并停止导航（Logs/blocked-attack-baseline.txt、Logs/full-demo-blocked-baseline.txt）。根因：原攻击切换只比较根节点距离，未检查实体遮挡，且停止距离与偏移的实际命中圈不一致。厚度和相对方向不同使有些障碍不触发，因此不能推断其他障碍永远没问题。修复：统一检查实际命中范围和障碍遮挡，受阻继续追击，命中时再次检查。回归：同夹障碍条件进入Chase且Agent未停止、隔障碍不扣血；无障碍但实际攻击圈未覆盖时不攻击；靠近后可进入Attack并实际扣血。组合远程/掉落/Runner/三波/右键10秒多弹道均通过（Logs/full-demo-feedback-all.txt、Logs/blocked-attack-fixed.txt）。实际场景普通哥布林和Runner绕过Obstacles_03到达无遮挡攻击位置，PathComplete，均通过（Logs/obstacle-approach.txt、Logs/full-demo-obstacle-approach.txt）；旧贴柱朝向摇摆仍非本轮修复目标。


## 2026-10-17 石柱专项修复与重启后检查

四根石柱已完成局部修复：保留全局0.1烘焙半径与其他障碍物，按哥布林脚部中心/尺寸对齐柱子导航代理；石柱附近匹配局部避让半径，选择可达且可攻击的位置。四柱四方向共16组普通+Runner测试，32/32均能接近并攻击，最大朝向翻转2次、测得穿透0。证据Logs/pillar-only-fixed.txt。重启后Git完整性检查通过，未提交文件额外备份Logs/Recovery-20261017-121055。重启后组合Play Mode回归通过（Logs/full-demo-feedback-all.txt，2026-10-17 12:13；Logs/post-reboot-regression.log），Git完整性检查无报错。本轮不打包。

复现：四柱×上下左右贴柱、普通+Runner混合追击，旧版4秒/组共16组，仅16/32到达攻击状态，最多199次翻转，最大实体脚部穿透0.711。根因证据：脚部相对导航根偏移(0.048,-0.432)，半尺寸(0.279,0.183)；柱导航代理明显小于实体碰撞。只扩大/对齐柱代理后穿透归零但仍摇摆，表明不是单纯净空问题；动态Agent避让半径0.5与烘焙0.1不一致、贴柱目标位于受限边缘以及攻击入口边界切换共同影响。方案：仅校准四柱代理，局部匹配避让与缓存可达攻击落点，在稳定落点进入近战。回归：6秒/组16组32/32接近并攻击，最大翻转2，穿透0。证据Logs/pillar-only-original-baseline.txt、pillar-only-geometry-only.txt、pillar-only-fixed.txt；新旧测试时长不同，不能把翻转计数比作为同时间频率。未把单一因素写作唯一根因。其他障碍及原Nav数据保持。

其他障碍物回归：Obstacles_03上方玩家、下方普通/Runner均成功绕行至无遮挡攻击位置（Logs/full-demo-obstacle-approach.txt，2026-10-17 12:14；Logs/pillar-other-obstacle-regression.log）。全局NavMeshAreas、原NavMesh数据和非石柱碰撞体保持本轮开始时状态。


## 2026-10-17 中文与全透明UI

菜单、战斗HUD、波次、道具、暂停/胜负/重开文案全部中文，键位名称保留WASD/Shift/Esc/J。所有非文字/图标/血条的面板、遮罩、按钮和卡片底色alpha为0。战斗仅显示右上角暂停入口；暂停时隐藏该入口，仅中央“已暂停”，底部移除重复提示。已在Unity图形模式通过六状态透明度、中文/字形、暂停唯一性、透明按钮射线、整局流程检查，并检查六张截图；不打包。

按反馈清除全部UI底色与遮罩，保留图标和生命条填充；各画面统一中文。加入Noto Sans CJK SC Regular及OFL许可证，生成静态HeroArenaChinese SDF及细黑描边。透明按钮仍可点击，暂停提示按状态只保留一个。证据Logs/chinese-ui-verified.log、Logs/full-demo-visual.txt（2026-10-17 13:11）、Logs/demo-*.png。六张画面已检查；本轮未打包。


## 2026-10-17 UI可读性与道具栏位置调整

按后续反馈改为紧凑半透明深色信息框，顶部背景alpha0.42、道具卡0.34、底部提示0.24；全屏覆盖层仍完全透明，中央战斗区域不加遮罩。主要文字加粗，白色/浅金色和0.24黑描边增强对比。道具卡由y108下移至76，缩至80×72，保留48×48图标；与底部提示间距8参考像素。中文和单一暂停入口保留。六状态交互/字形/透明上限/下移间距以及2048×940宽屏画面通过，未打包。

修正完全透明后文字融入地图的问题：仅信息区添加单层深色半透明底，缩小顶部与道具卡占用，增加字重和描边并下移道具栏。移除会将整块面板叠亮的Image Outline效果，保留字体描边。修复TMP 3.0.x对“全部字形已存在”的TryAddCharacters返回false导致重复应用误判，只补充缺少字形。最终图形回归通过（Logs/readable-ui-final.log、Logs/full-demo-visual.txt，2026-10-17 13:24），六张界面及Logs/demo-hud-wide.png已检查；未打包。


## 2026-10-17 全屏森林试炼首页

开始界面改为独立全屏森林遗迹插画：红围巾主角、近战/持斧/远程哥布林、金色弹道与紫色能量球呈现战斗玩法。左侧中文标题、开始/退出与四种遗物说明；背景等比铺满，适配普通屏和宽屏。复用原GameFlow、按钮持久回调和全部玩法。六状态交互、中文/字形、1280×720及2048×940菜单/HUD图形回归通过，未打包。
