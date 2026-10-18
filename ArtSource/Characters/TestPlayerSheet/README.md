# TestPlayer 原图切片

2026-10-18，按用户指定切割 `Assets/Role/Test/Player/TestPlayer.png`。源图为用户提供，1448×1086 RGBA；来源声明与商用授权不在本次切片中独立核验。

- 实际可见人物54帧，按原图从上到下、从左到右编号，七行8/8/8/7/8/8/7。
- 已在原图建立Multiple Sprite切片：`TestPlayer_R01_01`至`TestPlayer_R07_07`（各行数量不同）。保留原图、GUID和100 PPU，Point、无压缩、无mipmap。
- `slicing-layout.json` 保存源图SHA-256、左上角坐标系矩形、临时枢轴和导出PNG哈希。Unity内转为左下角坐标。枢轴仅按行脚底基线暂定，后续动作接入需要校准身体重心。
- `TestPlayer-numbered.png` 用于全帧数量与边界审阅；`slice_testplayer.py <输出目录>` 可重复导出54个PNG和ZIP。需Python、Pillow；不重采样、不重绘、不处理原始透明度。
- `Assets/Editor/TestPlayerSheetSlicing.cs` 和现有编辑器命令 `slice-testplayer-sheet` 可重新应用经SHA核对的坐标。原生Unity验证结果见 `unity-import-report.txt`：54个Sprite名称、矩形、pivot和local file ID通过。

## 限制及未完成项

攻击行部分剑刃与特效和邻帧相接、穿插。切片采用有重叠的矩形保留原图内容，会含少量相邻帧碎片；矩形裁剪无法恢复被遮挡内容。这些帧不能直接认定为干净的生产素材。原图还含低alpha残留，本次保持原像素。

本次54张PNG逐一与源图对应矩形逐像素比较通过。Unity实际导入通过，不代表动作播放、姿态连续性或正式角色替换验收通过。动作分组、帧率、身体锚点和攻击事件尚未接入；未修改Scene、Animator、Clip或正式角色，未打包。检索该纹理GUID，没有既有场景、Prefab或动画引用。
