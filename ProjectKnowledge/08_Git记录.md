# HeroArena Git记录


## 说明

记录项目 Git 版本变化。


目的：

- 了解项目发展过程
- 快速定位版本
- 展示持续开发能力


Git平台：

GitHub


仓库：

https://github.com/MX-520/HeroArena



---

# 分支说明


## main


主要稳定版本分支。


用于：

- 保存可运行版本
- 展示项目状态



---

## feature 分支


用于：

开发独立功能。


例如：

- feature/enemy-navigation


完成后：

合并到main。



---

# Commit记录



## 初始化项目


Commit:

Initial commit


内容：

- Unity项目创建
- 基础目录结构
- 初始场景



---


# 玩家系统阶段


## 完成玩家基础控制


内容：

- 玩家移动
- Rigidbody2D控制
- Animator状态


---


## 完成玩家Roll系统


内容：

- Roll移动
- 无敌机制
- 穿敌机制


---


## 完成玩家攻击系统


内容：

- AttackPoint
- 攻击检测
- Animation Event


技术：

Physics2D.OverlapCircleAll


---


# 战斗系统阶段


## 通用伤害系统


内容：

新增：

- IDamageable
- DamageReceiver
- Health


支持：

- 玩家
- 敌人
- 可破坏物



---


# 敌人系统阶段


## Enemy AI基础


内容：

完成：

- Idle
- Chase
- Attack
- Hurt
- Dead


---


## NavMeshPlus 2D寻路


内容：

- 集成NavMeshPlus
- 敌人绕障
- 地图导航


遇到问题：

Agent不在NavMesh。


解决：

调整：

- Agent参数
- 烘焙范围
- Navigation区域



---


## Runner Goblin系统


Commit:

完善哥布林战斗系统与奔跑敌人


内容：

- Runner Goblin
- Run动画
- AxeAttack动画
- 攻击逻辑优化
- 受击恢复优化
- 死亡停止移动


---


# 当前开发阶段


下一阶段：

Wave System


计划Commit：

feat: add enemy wave system



---


# Git提交规范


功能新增：

feat:


例如：

feat: add enemy wave system


Bug修复：

fix:


例如：

fix: solve enemy attack state issue


文档：

docs:


例如：

docs: update project documentation


整理：

chore:



---

# 提交原则


一个Commit尽量对应一个完整功能。


例如：

推荐：

完成敌人AI

↓

提交一次



不推荐：

一天几十次零碎提交。



---

# Git使用习惯


开发流程：

修改代码

↓

测试

↓

确认稳定

↓

Commit

↓

Push


遇到重大问题：

可以通过Commit回退版本。