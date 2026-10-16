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
