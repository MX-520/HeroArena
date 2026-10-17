# Demo 验证与编辑器接续

当前功能范围见 [01](01_当前开发状态.md)，技术职责见 [02](02_技术架构.md)。本页只维护验证方法与证据解释。

## 选择最小相关回归

| 变更 | 现有入口（CompleteDemoVerification） | 重点 |
|---|---|---|
| 玩家射击 | RunShooting | 实际伤害、无敌、冷却、暂停、薄墙 |
| 波次 | RunWaves | 3/4/6 阵容、13 击杀、导航出生、清理与重置 |
| 玩家输入/增益 | 查 CombatFeedbackVerification 与 CompleteDemoDrops | 真实按键、多弹道主动发射、过期与死亡清理 |
| 隔障碍攻击 | 查 BlockedAttackVerification、ObstacleApproachVerification | 攻击几何、遮挡、上下两侧绕行 |
| 四柱 | RunPillarOnlyFixed | 贴柱、近战落点、多人挤压、翻转次数 |
| UI | 查 CompleteDemoUIVerification、ChineseUIVerification | Raycaster、点击/悬停、宽屏、暂停、胜负、重开 |
| 魔法球 | 查 ArcaneProjectileVerification | 动态球体、拖尾、暂停、命中与清理 |
| 音频与整局 | RunAudioUI / RunAudioAll | 采样推进、暂停、循环、混音、实际战斗事件、重载 |

调用前查实际 public static 方法和测试正文；入口名称、覆盖项可能随功能演进。不能仅凭表格宣称运行过。

## 执行条件

- CompleteDemoVerification 入口会打开正式场景、进入 Play Mode，并在 Finish 中退出 Unity 编辑器；适合独立验证会话，不直接对未保存的交互编辑器调用。
- 在当前编辑器可通过已经连接的工具检查编译、场景、对象和画面；没有 MCP 时可检查现有桥接或用独立 Unity 验证进程，不能对同一工程并发启动两个编辑器。
- 批处理可使用 Unity 的 `-projectPath`、`-executeMethod`、`-logFile` 参数调用已核对的方法。确认异步 Play Mode 测试的退出行为，不套用会提前结束的命令模板。
- 当前不构建/打包；有图像和音频内容时，自动断言之外还应查看实际画面或试听。纯文档修改无需运行游戏回归。

## 打开正式场景

先确认编辑器、工程与未保存场景状态。已有 HeroArenaEditorBridge 读取 Logs/editor-command.txt：

- `open-scene`：退出 Play 后打开 SampleScene；检测到脏场景时采用 Additive 保留，并写 Logs/editor-open-scene.txt。
- `close`：保存所有已打开场景并退出。它有写入副作用，不能只为查询状态发送。
- `measure-character-art`：退出 Play 后读取当前场景玩家的实际近战几何，写 Logs/Characters/attack-reach-baseline.json；不打开/保存场景，不退出编辑器。命令用临时文件写完再原子改名，桥接对短暂文件占用下一帧重试。

工具路径和连接状态以本机现场为准，不把某台机器的安装路径写成所有智能体的前提。受限进程列表可能漏掉宿主 Unity；启动独立验证前交叉核对现有日志、宿主进程和工程占用，不能因未列出进程就删除锁或并发启动。

## 既有证据（历史记录，本轮未重跑）

- Logs/full-demo-audio-all.txt：整局战斗与音频组合回归。
- Logs/gentle-mix-playback-verification.log：双音乐切换、循环、增益及音效压低 BGM。
- Logs/metal-magic-playback-verification.log：金属/法术实际播放与命中反馈。
- 其他专项结果路径见 [Bug记录](04_Bug记录.md) 和 [开发日志](03_开发日志.md)。Logs 未必随仓库存在。

2026-10-17 以前的构建和验证记录见 [历史迭代](archive/2026-10-17/12_完整Demo迭代.md)。旧包不代表现状，旧 PASS 不覆盖后续修改。
