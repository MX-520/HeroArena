# Git 里程碑

完整分支与提交以 `git log` 为准；当前工作区用 `git status --short` 检查，不在本页维护易过期的分支快照。规则见 [AGENTS.md](../AGENTS.md)。

## 重要阶段

- Runner 与波次：同组警戒、持续追击，两波扩展至三波。
- 完整 Demo：射击、远程敌人、掉落、中文 UI、胜负与重开。
- 实玩修复：输入、多弹道、隔障碍攻击、四柱局部导航和魔法球表现。
- 音频：素材授权清单、战斗反馈、双场景音乐、音效优先混音。
- 资料治理：当前说明与历史分离，精简协作规则和项目 skills。

历史提交说明保存在 [旧 Git 记录](archive/2026-10-17/08_Git记录.md)，其“当前分支/计划提交”等描述仅对当时有效。

## 仓库与交付约定

远程仓库：[MX-520/HeroArena](https://github.com/MX-520/HeroArena)，远程名 `origin`。本地提交与推送分别报告；推送须有相应授权，不默认合并 main，也不将未提交文件当成已上传。

分支按 feature/、fix/、docs/、chore/ 等用途命名；提交与交付文件按功能、版本、验证结果记录，不附加工具/模型参与署名。2026-10-19 已将三个旧工作分支名称规范化，提交对象和历史不重写。

## 常用查询

```text
git status --short
git log -5 --oneline
git log --oneline -- Assets/Scripts/Navigation/PillarNavigation.cs
git diff --stat
git diff --cached --stat
git diff --check
```

选择性暂存实际任务文件；文档整理不顺带提交用户的场景、导航、Packages 或其他改动。
