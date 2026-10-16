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
