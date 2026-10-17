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

## 2026-10-16 开发分支

分支：codex/runner-wave-challenge。

已提交 dac1a13：feat: 完善Runner同组警戒与持续追击机制，含 Unity Play Mode 检查和文档。两波挑战作为后续独立功能提交，实际哈希以 git log 为准。只提交本任务代码、场景和文档，保留开始任务前已有的规则与 Package 改动。


## 2026-10-17 完整Demo迭代记录

当前开发分支：codex/complete-demo（基于已完成的Runner和两波挑战）。

- af5750b：主角鼠标射击与扫掠子弹检测。
- 86cf87a：远程哥布林、预警、后退及身体子弹判定框。
- 7965fd9：第三波与首版难度曲线。
- 86d9768：敌人/障碍物掉落及四种增益。

完整UI与流程作为独立功能保存，实际哈希查看git log。原有AGENTS、.agentignore、Package和技能目录改动保持原状；本轮不自动推送或合并。


已产生cb4e3fc：完整UI与整局流程，含TMP必要资源、当前截图及README更新。五个功能提交均保存在codex/complete-demo；Windows构建与启动结果在后续文档提交中记录。


## 2026-10-17 实玩反馈修复交付

分支：codex/combat-feedback。

- 8c4bde5：右键近战、保留J；10秒手动三弹道，正常射速。
- 952929d：独立远程施法、移除弓弩、1.2秒实际发射、同组持续步行追踪及石柱导航修复。

组合Play Mode与六张UI画面检查均通过。Windows构建成功，0错误、1警告、93,634,581字节。已打开新版独立游戏和正式SampleScene；交付记录后续提交。原有规则、技能、Package、11_架构风险和原NavMesh-Navigation.asset修改未纳入本轮提交，原NavMesh文件哈希与本轮开工备份一致。


## 2026-10-17 子弹尺寸与导航回退

- b9457c3：哥布林能量弹视觉直径增大50%。
- a1d3eb1：恢复8c4bde5原导航参数、数据与追击/后退逻辑，撤回上轮石柱修复，保留射击及输入改动。

原版静态一致性检查与组合Play Mode回归通过；Windows构建成功，0错误、1警告，93,645,101字节。旧石柱问题按用户要求暂缓。规则、技能、Package、11_架构风险和用户新增NavMesh-Navigation 1资产不纳入本轮提交；修改前用户原NavMesh数据已备份于Logs/BeforeNavigationRollback后按明确要求恢复Git版本。


## 2026-10-17 子弹加倍与隔障碍攻击修复

be45981：子弹视觉在当前尺寸上加倍（缩放2.7）。本轮攻击修复单独提交：只改攻击条件与命中遮挡检查，导航算法/参数/数据不改。固定障碍复现、完整战斗回归、实际场景普通哥布林与Runner从下方绕障攻击均通过。用户要求未到打包阶段，本轮未构建独立程序，仅打开Unity正式场景。用户当前场景、原导航数据及其他规则/Package修改保留未提交。


## 2026-10-17 石柱专项与重启后保存

四柱局部导航校准、局部避让/攻击落点及16组专项验证单独提交。重启后检查Git完整性无报错，原提交和新源码均在；33个未提交文件和日志额外备份至Logs/Recovery-20261017-121055，组合Play Mode回归通过。本轮不打包。提交包含保留用户现有石柱实体形状所需场景组件，其他用户场景/规则/Package/原导航资产修改留在工作区。


## 2026-10-17 中文透明UI

原层级就地修改，透明背景/按钮、完整中文、Noto中文静态图集与单一暂停入口作为独立功能保存。六状态图形检查及按钮/暂停/重开/胜负流程通过，未打包。场景仅暂存本轮UI和波次标题组件，保留用户其他场景/规则/原导航/Package改动。字体按随附OFL许可证纳入仓库。
