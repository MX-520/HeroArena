# 主角Idle：可见头腰曲线与披风波浪候选

2026-10-18用户进一步指出：哥布林Idle的头部/腰部浮动曲线明显自然，主角披风也要动。上一版过度抑制运动，不应把脚底稳定等同于头腰近乎静止。本轮维持空手、轻像素绘景、三分之四朝右与稳定接地目标，增强头腰高低差和披风轮廓变化。

## 制作方式与选稿

将[主角原板](../HeroIdlePlantedRevision/Hero-board.png)作为身份/画风输入，[哥布林](../FourCharacterIdleTrial/Sword-board.png)作为运动参照，[认可披风](../HeroIdleCapeWaveRevision/Hero-board.png)作为布料波浪参照；新增[动作草案](pose-guide.png)，以十二个采样展示腰→胸肩→头→披风末端的延迟关系。姿势图是简化构造参考，不是角色成品；pose-targets.json内参数是设计目标，不是生成结果的实测值。不得把程序画的构造图称为已完成角色动画。

内置image_gen共三次调用：先生成整组十二帧，再试两组六姿势分段。分段原板half1/half2的身体尺度和边界衔接不一致，未采用拼接结果。最终选择Hero-board.png整组原画，保留较一致的比例与完整角色；不是把失败分段重新命名成定稿。分段原图/提示词保留供复盘，不能当成最终交付。

## 最终候选与使用

[Hero-Idle-Flow-strip12.png](Hero-Idle-Flow-strip12.png)：7680×640透明RGBA，横排12格，每格640×640，根锚点[320,544]，8fps/1.5秒循环。Unity Multiple/640×640网格、Max Size 8192、Point/无压缩；PPU/Visual比例按实际显示体高校准。用户副本在outputs/HeroIdleFlow，只有一张PNG，不附对照页或ZIP。

最终提取命令：`python build_idle.py --source-root 本目录 --out 新输出目录`；build_halves.py仅复现未采用的分段试验，勿覆盖最终图集。原画时序见[manifest](Hero/manifest.json)，来源/SHA与选择依据见generation-record.json。

## 观察和验证边界

静态逐帧查看：头部有更可见的高低变化；披风前段展开、随后回落/卷曲，末段再抬起，已区别于上一版近乎固定的披风轮廓。诊断中头部剪影顶部约225–240格内像素，远端红布区域平均Y约344–386；这些只是剪影/颜色变化，不能等同于眼线、腰部实际轨迹、相位正确或自然度已验收。motion-inspection.json保存定义与逐帧值。

头腰与披风仍有部分帧的衔接待评审，尤其4→5、8→9和12→1；生成结果存在细节/体积、鞋型差异，不宣称靴底逐像素冻结或精确正弦/骨骼曲线已落实。没有把整个成图平移/缩放当作动作制作，也没有程序模糊补帧。此稿待用户按正常速度确认，不能仅因变化加大就标记为“自然流畅通过”。

技术检查：按alpha>80实际空白带划分原板三行，避免等分时切到鞋底；十二帧全部alpha>0像素完整提取，所属原板RGBA原样保留；横排各格与独立帧逐像素一致。内部浏览器12帧定位、控制、超过两轮正常速度循环通过，无脚本错误。证据见extraction-report.json、browser-verification.json和Hero/technical-report.json。播放功能证据不替代动作验收。

未修改Unity正式Clip/Prefab/场景/排序，未做游戏回归或构建。内部播放脚本依赖本机Edge/Playwright，需先用动画skill审查器生成Hero/review/review.html；素材授权仍按项目13核验。
