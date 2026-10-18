# 主角Idle轻像素绘景适配

2026-10-18用户确认上一版披风“这个可以”，随后要求主角有一些像素感，以适配地图环境、减少画面割裂。本轮基于[认可披风原板](../HeroIdleCapeWaveRevision/Hero-board.png)，以实际场景使用的[BaseMap](../../../Assets/Maps/BaseMap.png)作为环境笔触、明暗和色调参考，用内置image_gen做整组风格转换。地图不混入透明角色图集。

## 当前候选与交付

[Hero-Idle-Pixel-strip8.png](Hero-Idle-Pixel-strip8.png)：5120×640透明RGBA，横排8格，每格640×640，根锚点[320,544]，5fps/1.6秒循环。保持空手、白发红眼、红披风/深色皮甲与原有披风波浪、呼吸和第6帧眨眼。最终仅一张PNG，用户副本在本聊天outputs/HeroIdlePixel，不另交对照页或ZIP。

本版是轻像素绘景风格候选：轮廓略带阶梯、发梢/布料/护甲用更成块的明暗，描边改为较细的深色选择性轮廓，头发偏暖象牙色、灰影略带环境色，披风深红与皮甲棕灰适度收敛高光。原图仍是高分辨率生成绘制，提示词的80–96像素角色细节预算是风格目标，未声称已转换为严格统一低分辨率像素网格或有限调色板原画。生成编辑不保证身体/披风像素与上一版完全一致，局部装备、脸型和布料仍需实际大小评审。

## 场景依据与检查范围

SampleScene序列化SpriteRenderer引用BaseMap GUID 9212b171fa0f1b44691d2e4482fef993；地图1536×1024、PPU100、Transform缩放2.5642786，原图采样filterMode=1。旧Player Idle来源PPU32/Point，原Bafu Prefab缩放3.086。新图不能照抄旧PPU32或地图PPU100后推定显示大小正确；接入时要结合Visual缩放、相机、脚底和期望世界体高校准，不把风格问题视为已定位的Sorting Layer故障。

内部用HTML canvas把第4帧放到当前地图中心石台背景，按生成帧约0.25倍（地图原像素尺度）进行静态协调性检查：已查看，暖灰发色、深红披风和棕灰皮甲与背景更接近，角色在草地/石台上保持可读。该尺寸为审查近似，不是编辑器实测或正式PPU；没有改Unity场景、地图、Prefab、SortingOrder或Clip。map-check.html记录此本机检查方式，截图只留工作区，不作为用户额外交付。

8帧所有alpha>0像素完整提取，所属原板RGBA原样保留；5120×640横排每格与独立帧逐像素一致；真实alpha、帧序、时序和根锚点一致。报告见extraction-report.json。内部无头浏览器8帧定位、前后帧/复位/洋葱皮/剪影及超过两轮正常速度循环通过，无脚本错误，见browser-verification.json。自动播放功能检查不代替动作美术验收。

Unity建议Multiple、640×640网格、Max Size 8192、Point/无压缩；先按期望角色体高确定PPU/Visual比例，避免缩图或材质平滑抹掉颗粒。尚未导入正式资源或做游戏回归。本轻像素版待用户认可；上一版波浪披风已认可，不能把新画风也写成已认可。

来源与提示词见generation-record.json和[生成提示词](生成提示词.txt)；原板Hero-board.png、独立帧/时序在Hero/。重新整理：`python build_idle.py --source-root 本目录 --out 新输出目录`。内部播放检查脚本依赖本机Edge/Playwright路径，先用动画skill审查器生成Hero/review/review.html后再运行。角色及环境参考的授权状态继续按项目13维护，风格转换不替代授权核验。
