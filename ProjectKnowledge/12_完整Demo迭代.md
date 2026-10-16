# 完整 Demo 迭代（2026-10-16）

计划：玩家射击 → 远程哥布林 → 第三波及难度曲线 → 回血/弹幕/巨化/护罩掉落 → 完整UI与游戏流程。每个模块验证后单独提交。当前仅主角射击完成验证；其余仍为计划。

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
