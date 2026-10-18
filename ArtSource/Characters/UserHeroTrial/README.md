# 用户提供的白发主角：四方向素材裁剪

2026-10-18。用户提供 `主角四方向像素待机动画展示板.png` 并要求试效果；本目录保存原始展示板副本 `source-board.png`，未调用图片生成工具，也未替换正式主角或将其标记为新定稿。

## 来源与实际条件

- 原图 1536×1024，RGBA，但 alpha 范围为 176–251，棋盘格已绘制进 RGB，不是可直接使用的透明精灵表。
- 展示文字标注“每方向32帧、32×32”，实际小人约 40–55 像素宽、67–80 像素高，编号/间距不规则且部分相邻轮廓接触；未按这些标注伪造规则帧数。
- `extract_board.py` 从每方向选取前 8 个独立轮廓，基于连通轮廓去底；用轮廓闭合及孔洞填充保护与灰底近似的白发；Unity 实际渲染发现部分帧仍有发丝孔洞后，扩大轮廓阈值并在头部局部加强闭合。未重绘角色，未缩放原像素，按头部横向位置和脚底对齐到 96×112 格，组合为 `UserHeroIdleTrial.png`。
- 精确来源哈希、每帧原图矩形及限制见 `extraction-report.json`。抠图处理是近似试验，不能等同于原始透明帧。

## 查看与 Unity 导入

- 直接打开本地 `preview.html`，四方向各 8 帧，可切换 4/8/12 fps、逐帧和背景。由脚本及 `preview-template.html` 重建，无网络依赖。
- 运行 `python ArtSource/Characters/UserHeroTrial/extract_board.py` 重建切片、布局和预览；需要 Pillow/NumPy，原始根目录文件须仍存在，且尺寸符合本图。
- Unity 菜单 `Tools/HeroArena/Characters/Prepare User Hero Idle Trial`，或桥接 `prepare-user-hero-trial`。导入到独立 `Assets/Art/Characters/Generated/UserHeroTrial` 动画目录，纹理为同级 `UserHeroIdleTrial.png`；不更改场景/Prefab/控制器。
- 四方向 Clip 复用原待机时间轴（0.75 秒、12 fps、循环），事件为空；网页试播帧率可调，用于目视评审。此处未实现运行时四方向控制逻辑。
- 已在无编辑器占用/工程锁的独立 Unity 批处理会话执行导入：32 Sprite、4 Clip 的数量、引用、时长、帧率、循环和事件检查通过，进程退出码 0，报告见 `unity-import-report.json`。未构建，未执行玩法回归。

## 观察

网页实际截图已检查，白发与深色服装在绿色背景上分离明显。初版去底误删白发，已通过轮廓闭合后再填充修正；该问题不能当作源图的发型缺陷。侧面帧存在明显倾身/脚步变化，试播更接近轻微踏步，且存在帧间造型变化，尚不能认定为自然无缝待机。

用户新偏好：今后图片创作由用户通过 ChatGPT 完成，本智能体负责确定输入规格、处理既有素材、接入与验证。正式替换应取得独立真实透明帧或规则精灵表，避免从带背景/文案的展示板反推正式资产。

## 完整裁剪交付（当前使用）

用户指出先前每方向只剪 8 帧不完整，已重新按原图从左到右逐个核对并导出全部可见人物：下 31、左 29、右 29、上 28，共 117 帧。原板文字写每方向 32 帧，与实际画出的数量不符；`FullCropped/source-frame-audit.png` 在每个原人物上标号，可直接核对。没有复制帧或生成缺失帧凑数。

- `FullCropped/Down`、`Left`、`Right`、`Up`：117 张独立透明 PNG，统一 96×112、脚底 y=104，保持原像素、不缩放。
- `FullCropped/*-sheet.png`：各方向单行精灵表，列数分别 31/29/29/28。
- `UserHeroIdleFull.png`：总表 2976×448，31 列、四行，顺序下/左/右/上；短行末尾是透明空格，不是有效帧。
- `UserHeroIdleFullCrops.zip`：以上所有裁剪图、编号核对图和逐帧来源报告。
- `crop_all_frames.py`：从保留的 `source-board.png` 重建完整导出；`FullCropped/crop-report.json` 保存每帧原图区域、头部中心、实际数量、输出检查和限制。

已读回所有 PNG 验证尺寸、透明留边、与总表像素一致、脚底对齐，ZIP 完整性通过；已查看全部 117 帧接触表及源图编号图。源图人物间存在围巾相接/重叠，按头部中点分隔，仍可能有围巾切边、邻帧碎片及浅色边缘；被遮挡的像素不能从此展示板恢复。该结果是完整裁剪，不代表正式动作质量验收。

之前 `Cropped/`、`UserHeroIdleTrial.png`、`UserHeroIdleCrops.zip` 和 `extract_board.py` 仅为每方向 8 帧的旧试验子集；不再作为完整素材交付。Unity 试播报告同为历史证据。用户已要求撤回独立预览、仅裁剪素材，本次没有导入 Unity、恢复用户删除的 Characters 目录或改动场景。

## 用户导入原图的 Unity 切片

按用户后续要求，直接对 `Assets/Role/Test/Player/主角四方向像素待机动画展示板.png` 建立117个 Multiple Sprite，命名 Idle_Down_01 等，Point 过滤、无压缩。源图哈希与本目录原板相同，使用 FullCropped 报告中逐帧矩形，数量、唯一名称和每个矩形尺寸实际导入检查通过。只修改原图导入设置，原 PNG 像素与背景保持原样；没有绑定动画或修改场景。可通过 UserHeroBoardSlicing 菜单或现有桥接重新切片。
