# 用户提供的银发红瞳冒险者行走循环

来源：用户自行导入 `Assets/Role/Test/Player/银发红瞳冒险者行走循环精灵图.png`，1448×1086，RGBA 真透明背景。源图哈希、八帧矩形和脚底/身体中心枢轴见 `slicing-layout.json`，不重绘、不修改原PNG像素。

图中两行各四帧，按从左到右、从上到下的顺序切成 WalkDown_01–08。人物围巾跨越等宽四格边界，因此按透明间隔确定独立轮廓范围并留4像素余量，避免等分裁断围巾。枢轴按浅色头发中心和人物脚底对齐；原图极淡的透明边缘参与视觉质量评审，不将源图当作统一像素密度的定稿。

目标为现有 `Assets/Animations/TestPlayer/walkdown.anim`：12 fps、8个等时长姿态、约0.667秒循环，保留原Clip GUID及事件，不改TestPlayer控制器、场景、移动逻辑或正式角色。导入入口 `Tools/HeroArena/Characters/Prepare TestPlayer WalkDown`；桥接命令 `prepare-testplayer-walkdown`。

验证结果见同目录 `unity-verification.txt`；未运行整局玩法回归，未构建。
