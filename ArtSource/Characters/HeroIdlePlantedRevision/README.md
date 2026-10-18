# 主角Idle：参考哥布林的脚底接地版

2026-10-18用户要求借鉴已认可哥布林Idle：“脚底始终没动，只动脚上面部分”。使用[持剑哥布林原板](../FourCharacterIdleTrial/Sword-board.png)作为运动机制参考，上一版[主角原板](../HeroIdleWeightShiftRevision/Hero-board.png)保持身份/画风，中性主角第1帧作为鞋底/站距参考；不把哥布林的形象、剑或比例带到主角上。

## 本轮动作目标

稳定脚底和站姿，减少主动蹲起；脚踝上方的膝盖仅柔和响应，胸肩/腰部/手臂略错开地呼吸，披风延迟跟随。继续保留白发红眼、空手、深色皮甲、轻像素绘景风格与红披风波浪。要求生成器在每帧沿用同一鞋底位置、朝向和轮廓，不能靠全图上下平移或缩放伪造动作。

## 交付与复现

[Hero-Idle-Planted-strip12.png](Hero-Idle-Planted-strip12.png)：7680×640透明RGBA，横排12格，每格640×640，根锚点[320,544]；8fps/1.5秒循环，继承上一版规格。Unity使用Multiple/640×640网格、Max Size 8192、Point/无压缩，按期望世界体高校准PPU/Visual比例。只交付这一张PNG，聊天副本在outputs/HeroIdlePlanted，不默认增加对照页/ZIP。

原板为Hero-board.png，完整[提示词](生成提示词.txt)、[帧时序](Hero/manifest.json)与generation-record.json保存参考用途和来源SHA。重新提取：`python build_idle.py --source-root 本目录 --out 新输出目录`；原板行距未假定严格等分，沿实际alpha>80空白带分隔，以避免上一行鞋底混入下一帧。

## 检查与边界

已逐帧静态查看12个人物，检查低点/高点、站距和鞋底，人物保持站立而非明显蹲起，运动意图转向胸腰及手臂的柔和呼吸。静态原画不替代正常速度美术验收；生成器会有局部鞋型、服装和体积差异，本轮不能宣称鞋底逐像素完全相同或脚部零位移。sole-inspection.json记录根对齐后各帧的底部轮廓范围，用来识别明显滑移，不作为自动质量验收。

十二帧全部alpha>0像素完整提取，所属生成原板RGBA原样复制；横排各格与独立帧逐像素一致，未对人物做缩放/关节程序变形。根锚点取靴底区域中点，不能由根已对齐推定左右两脚的每个像素都被冻结。报告见extraction-report.json。

内部无头浏览器12帧定位、前后帧/复位/洋葱皮/剪影控制与超过两轮正常速度循环通过，无脚本错误，见browser-verification.json。这些是播放功能证据，仍待用户确认动作流畅与接地感；未替换正式Unity角色或Clip、未修改SortingOrder/场景、未游戏回归或构建。

内部播放脚本依赖本机Edge/Playwright，需先用动画skill审查器生成Hero/review/review.html。本轮素材来源及许可继续按项目13维护。
