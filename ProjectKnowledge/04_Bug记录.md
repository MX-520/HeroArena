# HeroArena Bug记录


## 说明

记录开发过程中遇到的重要问题：

包括：

- Bug现象
- 排查过程
- 根本原因
- 解决方案
- 后续经验


用于：

- 项目维护
- 避免重复踩坑
- 面试复盘



---


# 1. NavMeshAgent无法寻路


## 问题现象

敌人调用：

SetDestination()

出现：

Agent not on NavMesh


表现：

Gobling无法移动。



## 排查过程


检查：

- Navigation Surface
- NavMesh烘焙
- Agent位置
- Agent半径
- Walkable区域



发现：

敌人生成位置没有落在有效NavMesh区域。


## 解决方案


调整：

- Navigation区域
- Agent参数
- 烘焙范围


最终：

Gobling可以正常追踪玩家。


## 经验

使用NavMesh时：

Agent必须处于有效NavMesh区域。

不能只添加组件。



---


# 2. 动态障碍销毁后敌人仍绕行


## 问题现象


障碍物被破坏后：

敌人仍然按照原路线绕行。


## 原因


NavMesh没有及时更新。

敌人仍然使用旧导航数据。


## 解决方案


增加导航更新机制：

障碍物变化后重新更新NavMesh。


## 经验


动态场景变化需要考虑导航数据同步。



---


# 3. 敌人攻击一次后无法连续攻击


## 问题现象


敌人：

第一次攻击正常。

之后：

无法继续攻击。


## 排查过程


检查：

- Animation Event
- Animator Transition
- EnemyController状态


发现：

Animator攻击状态和代码攻击状态不同步。


## 解决方案


增加：

IsAttacking


用于同步：

EnemyController

和

Animator


## 结果


实现：

- 连续攻击
- 攻击结束恢复追踪



---


# 4. 受击后敌人无法继续战斗


## 问题现象


敌人攻击过程中：

受到玩家攻击后：

- 停止追踪
- 不再攻击


## 原因


进入Hurt状态时：

攻击状态没有清理。


## 解决方案


受击时：

- 清除攻击状态
- 恢复AI状态流程



---


# 5. 敌人死亡后仍然移动


## 问题现象


敌人死亡动画播放时：

角色仍会移动。


## 原因


敌人移动由：

NavMeshAgent

控制。


停止：

Rigidbody2D

无法影响NavMesh移动。


## 解决方案


死亡时：

- 停止NavMeshAgent
- 清除移动路径


## 经验


需要明确：

哪个组件负责控制对象行为。



---


# 6. Animator攻击过渡问题


## 问题现象


修改Attack动画Transition后：

出现：

- 攻击卡住
- 追踪时播放攻击动画


## 原因


Animator Transition规则与代码状态机不匹配。


## 解决方案


通过：

- Animator参数
- Animation Event
- EnemyController

保持同步。


## 经验


Animator不是独立系统。

动画状态需要和游戏逻辑状态保持一致。



---


# 后续Bug记录模板


## Bug标题


### 现象


### 排查过程


### 原因


### 解决方案


### 经验

## 2026-10-16 完整Demo迭代：主角射击

已复现：初版子弹在出生点消失，目标HP仍100。根因证据：扫掠记录NavigationWalkable(layer9)在distance0命中；该多边形覆盖可行走区。方案：射击遮挡只查询实际墙体Default与伤害目标层，排除导航代理层。回归：实际命中扣12血与薄墙阻挡均通过（CompleteDemoVerification.RunShooting）。CameraBounds为Trigger，非触发器筛选会跳过。


## 2026-10-16 远程哥布林

已复现：弩手子弹瞄准玩家根坐标时HP仍100。证据：Bafu脚部Collider偏移Y=-0.18037，根缩放3.086，投射物轨迹高于脚部判定。修复：独立ProjectileHitbox身体Trigger，按父受伤组件判定阵营；忽略其他辅助Trigger和友方判定框。回归：实际弩手命中玩家扣8血通过；移动碰撞未扩大，近战不查询新层。


## 2026-10-17 贴石柱敌人摇摆：已复现与定点修复回归

实现：远程哥布林使用独立 RangedShoot Animator override，0.3秒发射事件、0.55秒结束事件，移除弓弩子物体，改用紫色能量弹。开火起点按绝对时间计算1.2秒间隔；步速2.3，不启用Runner动画。复用既有RunnerGroup实现受击/致命伤害同组警戒与持续锁定，同波Runner组和远程组独立。受击/死亡仍取消发射。射线被实际障碍遮挡时重新寻路，贴近2单位开始后退，拉到3单位结束；找不到有效后退路径则短暂停留射击，避免状态反复切换。

石柱已具备独立导航代理碰撞体，无需增加图片。将导航代理按敌人脚部偏移对齐，保留已有增大的代理范围；烘焙半径与Agent半径统一为0.5，体素0.08，生成NavMesh-CombatNavigation.asset，原用户修改的NavMesh-Navigation.asset保留。追击目标投影到可达NavMesh、0.2秒缓存路径，朝向忽略微小水平避让速度。

复现与证据：单敌人8个贴柱位置未复现；普通/Runner/两远程混合测试复现Runner在第2/3柱3.5秒内翻转418/211次。已确认烘焙半径原0.1而Agent半径0.5、脚部中心与导航代理参考点不同；不能据此断言任一项是唯一原因。综合修复后四柱各敌人翻转均不超过4次，近战最近距离0.80/1.10/1.20/0.95，8个单敌人路径均完整。证据：Logs/pillar-crowd-baseline.txt、pillar-crowd-fixed.txt、pillar-probe-fixed.txt。固定测试已通过；复杂动态走位仍需实玩观察。


组合回归：CompleteDemoVerification.RunFeedbackAll通过（Logs/full-demo-feedback-all.txt）。包括三次实际射击间隔/独立动画、受击联动、超出侦测范围持续锁定、单敌人8处及四柱混合敌人、完整旧战斗/掉落/Runner/三波回归和真实右键/10秒多弹道。此次混合回归四柱最大翻转3次，近战最近距离0.80/0.98/1.20/0.94。测试自身的短时增益配置已在Drops检查结束时恢复，避免污染后续10秒检查；曾将攻击间歇误作丢失目标的断言已改为检查锁定及状态。


## 2026-10-17 后续实玩反馈：撤回石柱导航修复

用户实玩反馈：上轮其他障碍物绕行范围过大；代码证据为全局烘焙半径0.1→0.5，影响全部障碍物净空。上轮石柱定点测试不能证明其他障碍物体验正确。按用户要求撤回导航修复，恢复Git 8c4bde5原数据/参数及追击和后退逻辑，保留战斗需求。静态原版一致性检查与组合战斗回归通过；石柱原摇摆问题重新列为暂缓，不沿用之前“已修复”结论。证据Logs/navigation-rollback-static.txt、Logs/full-demo-feedback-all.txt。


## 2026-10-17 隔障碍提前攻击修复

复现：Unity固定薄障碍夹在玩家与普通哥布林脚部碰撞体之间，根节点距离0.9，旧UpdateChase进入Attack并停止导航（Logs/blocked-attack-baseline.txt、Logs/full-demo-blocked-baseline.txt）。根因：原攻击切换只比较根节点距离，未检查实体遮挡，且停止距离与偏移的实际命中圈不一致。厚度和相对方向不同使有些障碍不触发，因此不能推断其他障碍永远没问题。修复：统一检查实际命中范围和障碍遮挡，受阻继续追击，命中时再次检查。回归：同夹障碍条件进入Chase且Agent未停止、隔障碍不扣血；无障碍但实际攻击圈未覆盖时不攻击；靠近后可进入Attack并实际扣血。组合远程/掉落/Runner/三波/右键10秒多弹道均通过（Logs/full-demo-feedback-all.txt、Logs/blocked-attack-fixed.txt）。实际场景普通哥布林和Runner绕过Obstacles_03到达无遮挡攻击位置，PathComplete，均通过（Logs/obstacle-approach.txt、Logs/full-demo-obstacle-approach.txt）；旧贴柱朝向摇摆仍非本轮修复目标。


## 2026-10-17 石柱专项修复与重启后检查

复现：四柱×上下左右贴柱、普通+Runner混合追击，旧版4秒/组共16组，仅16/32到达攻击状态，最多199次翻转，最大实体脚部穿透0.711。根因证据：脚部相对导航根偏移(0.048,-0.432)，半尺寸(0.279,0.183)；柱导航代理明显小于实体碰撞。只扩大/对齐柱代理后穿透归零但仍摇摆，表明不是单纯净空问题；动态Agent避让半径0.5与烘焙0.1不一致、贴柱目标位于受限边缘以及攻击入口边界切换共同影响。方案：仅校准四柱代理，局部匹配避让与缓存可达攻击落点，在稳定落点进入近战。回归：6秒/组16组32/32接近并攻击，最大翻转2，穿透0。证据Logs/pillar-only-original-baseline.txt、pillar-only-geometry-only.txt、pillar-only-fixed.txt；新旧测试时长不同，不能把翻转计数比作为同时间频率。未把单一因素写作唯一根因。其他障碍及原Nav数据保持。


## 2026-10-17 按钮悬停与森林纹饰

用户报告鼠标悬停无明显高亮；源码确认目标仅TMP文字且颜色乘数变化微小。修复为独立背景/文字/角饰反馈，使用unscaledDeltaTime避免暂停冻结。回归通过真实GraphicRaycaster命中与事件分发，检测颜色差值、放大、移出恢复和暂停状态动画；原开始/暂停/重开/返回流程通过。未把“完全没有收到事件”写作根因。
