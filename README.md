# HeroArena

> 🎮 基于 Unity 2022.3 + C# 开发的 2D 像素风动作 RPG Demo  
> 🚧 个人独立开发中，项目持续迭代

HeroArena 是一个基于 Unity 2022.3 + C# 独立开发的 2D 像素风 Action RPG 战斗 Demo。

项目围绕角色控制、战斗系统、敌人 AI、2D 导航与场景交互构建核心玩法，目前已完成从移动、战斗、闪避、敌人追踪，到可破坏障碍、动态寻路、死亡重生的基础 Gameplay 闭环。

项目开发过程中注重功能模块复用、角色状态约束以及实际问题定位，并持续通过 Git 进行功能迭代与版本管理。

---

## 🎬 游戏演示

![HeroArena Gameplay Demo](Documentation/Media/HeroArena_Demo.gif)
▶️ **[查看完整游戏演示（Bilibili）](https://www.bilibili.com/video/BV1g8He6TEKb)**
---

## ✨ 已实现功能

### Player

- 移动 / 奔跑
- 普通攻击
- 翻滚
- 翻滚期间无敌
- 翻滚穿越敌人
- Hurt 受伤与受伤硬直
- Death 死亡
- Respawn 重生
- Attack / Roll / Hurt / Death 动作状态约束与中断处理


### Enemy

当前敌人：`Gobling`

- Idle / Chase / Attack / Hurt / Death 状态
- 基于 NavMeshPlus 的 2D 寻路与玩家追踪
- 静态地图边界与场景障碍规避
- 根据移动方向调整角色朝向与攻击方向
- 攻击距离判断与攻击冷却
- Animation Event 同步攻击判定
- 受伤、死亡与状态切换处理
- 死亡后禁用攻击、碰撞与伤害接收并延迟销毁

---

### Environment & Navigation

- 可破坏场景障碍物
- 复用 `IDamageable / DamageReceiver / Health` 通用伤害结构
- 使用 NavMeshPlus 构建 2D 可行走区域
- 分离物理碰撞区域与 AI 导航阻挡区域
- 障碍物销毁后动态重建 NavMesh
- 敌人能够根据场景变化重新规划移动路径
- 使用 Cinemachine 实现玩家相机跟随与地图边界限制


## ⚔️ Combat System

基础伤害结构：

```text
Attack
  ↓
IDamageable
  ↓
DamageReceiver
  ↓
Health
  ↓
OnDamaged / OnDied
  ↓
Player / Enemy / Destructible
```

通过 `IDamageable`、`DamageReceiver` 与 `Health` 分离攻击、伤害接收和生命值管理，并复用于 Player、Enemy 与 Destructible。

`Health` 通过事件通知受伤和死亡，使生命值逻辑不直接依赖具体角色或场景对象的行为。

---

## 🎬 Animation & State

角色动画使用 Unity Animator 管理，并通过 Animation Event 同步：

- 攻击伤害判定
- Attack 状态结束
- Hurt 硬直结束

针对 Attack / Roll 被 Hurt 等高优先级状态打断的情况，在代码层实现主动取消和状态清理，避免动画中断后产生状态残留。

Roll Coroutine 同样支持主动取消，确保中断后正确恢复碰撞、无敌和角色状态。

---

## 🔄 Death & Respawn

玩家死亡流程：

```text
HP <= 0
   ↓
OnDied
   ↓
Player Death
   ↓
GameManager
   ↓
Respawn Delay
   ↓
RespawnPoint
   ↓
Restore HP
   ↓
Resume Control
```

`GameManager` 负责玩家重生时间与 RespawnPoint 调度，Player 负责恢复自身生命值和角色状态。

---

## 🛠️ 技术栈

- Unity 2022.3 LTS
- C#
- Unity Input System
- Animator / Animation Event
- Rigidbody2D / Physics2D
- NavMeshPlus / Unity AI Navigation
- Cinemachine
- Coroutine
- Git / GitHub

---

## 📁 项目结构

```text
Assets/
├── Scripts/
│   ├── Player/
│   ├── Enemy/
│   ├── Combat/
│   └── Managers/
├── Prefabs/
│   ├── Player/
│   └── Enemy/
└── ...

Documentation/
└── DevLog/
```


---

## 🗺️ 开发进度

- [x] 玩家移动与角色控制
- [x] 基础战斗与伤害系统
- [x] Roll 闪避 / 无敌 / 敌人穿越
- [x] Hurt / Death / Respawn
- [x] Enemy 状态 AI
- [x] Enemy Attack / Hurt / Death
- [x] 2D NavMesh 寻路与障碍规避
- [x] 可破坏场景障碍
- [x] 障碍销毁后的动态导航更新
- [x] Cinemachine 相机跟随与地图边界限制
- [ ] Gameplay UI / Health UI
- [ ] 战斗反馈与视觉表现优化
- [ ] 更多 Gameplay 内容

---

## 📌 项目状态

HeroArena 目前已完成基础战斗 Gameplay 闭环，并持续进行功能迭代与表现优化。

当前版本重点完成角色控制、战斗系统、敌人状态 AI、2D NavMesh 寻路、可破坏场景、动态导航更新以及死亡重生等功能。

项目主要用于 Unity 游戏客户端开发方向的学习与实践，后续将继续完善 Gameplay UI、战斗反馈与整体演示效果。