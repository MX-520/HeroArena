# HeroArena

> 🎮 基于 Unity 2022.3 + C# 开发的 2D 像素风动作 RPG Demo  
> 🚧 个人独立开发中，项目持续迭代

HeroArena 是一个用于学习和实践 Unity 游戏开发的个人项目。

目前主要围绕 **角色控制、战斗系统、敌人 AI、动画状态以及角色死亡/重生流程** 进行开发，并在实现功能的同时逐步整理代码结构和角色状态管理。

---

## 🎬 游戏演示

> Demo GIF / Video 制作中

后续将在此处添加实机战斗演示。

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
- 自动追踪玩家
- 攻击距离判断与攻击冷却
- 根据玩家位置调整攻击方向
- Animation Event 攻击判定
- 受伤与死亡处理
- 死亡后禁用攻击、碰撞与伤害接收
- 延迟销毁

---

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
Player / Enemy State
```

通过 `IDamageable`、`DamageReceiver` 与 `Health` 分离攻击、伤害接收和生命值管理。

`Health` 通过事件通知角色受伤和死亡，使生命值逻辑不直接依赖具体的 Player 或 Enemy 行为。

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

开发日志记录在 `Documentation/DevLog` 中。

---

## 🗺️ 开发进度

- [x] 玩家基础移动
- [x] 玩家攻击
- [x] Roll / 无敌
- [x] 基础伤害与生命值系统
- [x] 玩家 Hurt / Death
- [x] 玩家 Respawn
- [x] 基础 Enemy AI
- [x] Enemy Attack / Hurt / Death
- [ ] Health UI
- [ ] 更多敌人类型
- [ ] 技能系统
- [ ] 可破坏障碍物
- [ ] 随机道具 / BUFF
- [ ] Boss AI
- [ ] 完整游戏流程

---

## 📌 项目状态

HeroArena 目前处于持续开发阶段。

项目目标是通过实际功能迭代，逐步学习和实践 Unity 2D 游戏中的角色控制、战斗系统、AI、动画状态管理以及基础游戏架构。