# 刀剑与魔法音效来源

核对日期：2026-10-17。以下作者发布页均标注 CC0；项目保留作者署名。许可链接：https://creativecommons.org/publicdomain/zero/1.0/

## StarNinjas — 20 Sword Sound Effects (Attacks and Clashes)

- 作者页：https://opengameart.org/node/122900
- 作者说明音源由两把刀实录并加工。原作保留在 `ThirdParty/StarNinjas/Source/`。
- 攻击包：https://opengameart.org/sites/default/files/sword_-_starninjas_1.zip
- 碰撞包：https://opengameart.org/sites/default/files/sword_clash_-_starninjas_0.zip
- 选用 sword.4、sword.10、sword_clash.1、sword_clash.10。
- 游戏版 SteelSwingA/B、SteelImpactA/B 做了起点裁切、轻微变调、限长、淡入淡出及峰值归一化；原始文件没有修改。

## rubberduck — 80 CC0 RPG SFX

- 作者页：https://opengameart.org/content/80-cc0-rpg-sfx
- 原包：https://opengameart.org/sites/default/files/80-CC0-RPG-SFX_0.zip
- 选用 spell_01、spell_02、spell_fire_07，保留在 `ThirdParty/Rubberduck/Source/`。
- ArcaneCast/ArcaneImpact 将上述作者法术素材裁切、轻微变调并叠合，加入短回声；没有添加项目振荡器音源。

逐文件原包URL、原始名称、原包/源文件/剪辑版 SHA-256、派生关系见 `manifest.json`；处理参数源码为 `ThirdParty/prepare_combat_samples.py`，输出指标见 `ThirdParty/combat-edits.json`。工具依赖 numpy 2.5.4、soundfile 0.14.0，本次安装在忽略目录 Logs/AudioTools，未修改系统环境。

原始网页快照及下载包保留在工作区 `Logs/MetalMagicResearch/`。本记录区分原作者音源与项目剪辑版，不把它们称为项目原创录音；CC0来源核验不替代最终听感验收。
