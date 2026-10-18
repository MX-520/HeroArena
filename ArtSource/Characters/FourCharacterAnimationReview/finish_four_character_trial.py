from pathlib import Path
import html,json
root=Path(__file__).resolve().parent.parent/'outputs/FourCharacterTrial'
d=json.loads((root/'trial-manifest.json').read_text(encoding='utf-8'))
findings=[
 dict(role='hero',clip='walk',frames='R02F01–08',level='需补画',title='步态阶段辨识不足',evidence='多数帧保持近似的前后腿轮廓，经过姿势与左右换重不清楚；披风与身体前倾变化比步态更明显。',fix='保留形象，先重画接触/下沉/经过/抬升四阶段与反侧四阶段，画出遮挡下的骨架再加衣服。'),
 dict(role='hero',clip='attack',frames='R04F04→05→06',level='需明确设计',title='单斩还是连斩尚不明确',evidence='第一次大刀光之后再次举剑，再出现第二次大刀光；整行不能无条件当作一次单斩。',fix='若是单斩，选定一次挥砍并补收势；若是两连斩，明确第二次蓄力、命中与连击接口。'),
 dict(role='hero',clip='attack',frames='R04F02–06',level='需修图布局',title='邻帧剑刃和刀光穿插',evidence='切片中可以看到邻格剑尖/披风/刀光碎片，且部分刀光被矩形边缘截断。这是源图布局污染，不是重新设置pivot能解决的。',fix='回到独立透明帧制作，身体/武器/刀光分层；不要继续扩大重叠矩形作为生产资产。'),
 dict(role='melee',clip='walk',frames='R02F03→04、F07→08',level='需审查补画',title='步态与握剑姿态同时变化',evidence='从斜持剑变成横向伸剑，步态中存在近似重复腿型；不易读出一个稳定的左右交替循环。',fix='先固定持械方案，建立不带披风和武器的换重骨架；不要用握剑变化充当行走帧变化。'),
 dict(role='melee',clip='attack',frames='R03F01→02→03→08',level='可保留结构',title='预备、举剑、挥出和返回都已有',evidence='四个角色中，这行攻击的阶段结构较完整；不能只因帧少就判为失败。02→03较大跳变是否可读，需正常速度与刀尖弧进一步确认。',fix='保留关键姿势方向，补或重画清楚的命中/随动姿势；先移除刀光检查身体，再制作独立刀光。'),
 dict(role='melee',clip='attack',frames='R03F03–07',level='需修图布局',title='刀光跨越相邻帧区域',evidence='几段刀光相接，矩形切片会截断当前拖影并带入上一帧残留。',fix='逐帧透明源或独立特效序列；切片自动化不能恢复被遮挡内容。'),
 dict(role='runner',clip='run',frames='R03F03–05',level='需补画',title='奔跑中斧头结构不稳定',evidence='F03斧刃不可辨，F04出现另一种弯曲轮廓，F05只剩明显杆状部分；缺少足以解释这些变化的转向/遮挡过程。',fix='保留一份刚性斧头图，以固定握点附着；确需转面时补对应视角，不重生成每帧斧头结构。'),
 dict(role='runner',clip='attack',frames='R04F06',level='可通过选帧避免',title='只有刀光，没有可辨角色身体',evidence='此格主要是红白弧光；若当成完整Sprite帧，身体会在该曝光期间消失。',fix='身体动画跳过这格；若它本来是独立特效，则重新分类并在独立层播放，保留身体姿势。'),
 dict(role='runner',clip='attack',frames='R04F05→07→08',level='仍需补画',title='跳过刀光格后，收势仍不完整',evidence='F05前伸之后F07回到低位持斧，F08回警戒；仅删除F06不能补齐中间的手与斧头回收路径。',fix='增加一张明确的随动与一张回收关键姿势；重排试验只验证身体不消失。'),
 dict(role='caster',clip='walk',frames='R02F01–08',level='证据不足/需骨架',title='披风摆动明显，脚步换重不清楚',evidence='脚部被袍摆遮挡，多帧身体姿态相近；静态序列不足以确认合理的左右支撑交替。',fix='先画袍下的腿和髋骨架，确认步态后重新加袍；也可明确设计为悬浮移动，但须统一玩法和视觉。'),
 dict(role='caster',clip='cast',frames='R03F06→07→08→R04F01',level='可通过重组改善',title='蓄力后先回落，再进入释放',evidence='举高蓄力在F06后收回低位，接着R04又开始发射；把两行简单串接会先卸掉聚力姿态。',fix='按蓄力→指向→释放→恢复重组。试验跳过R03F07/08，但真正的举杖到前指过渡仍需补画。'),
 dict(role='caster',clip='cast',frames='R04F06（连续序列第14帧）',level='可通过分类避免',title='纯飞行弹体混入身体序列',evidence='该格只有向右的紫色弹体，没有可辨身体；角色动画与弹体资产混排。',fix='从身体序列剔除，作为独立Projectile/VFX候选。发射后由游戏对象移动，不用角色Sprite跟着变成弹体。'),
]
(root/'findings.json').write_text(json.dumps(findings,ensure_ascii=False,indent=2),encoding='utf-8')

parts=['# 四角色动画设计师试用报告','',
'本轮只抽查四张原图的移动与战斗相关行：8段原始序列、71个曝光位置；另有2段原帧重排试验、17个曝光位置。不是全图全部姿势裁切。动作命名与行分组为本次推断，原作者未提供时间轴。',
'','结论：四张图均适合保留角色形象；当前抽查序列需要修正，不能直接判为合格游戏动画。持剑哥布林的攻击阶段最完整，可优先作为修帧样本；持斧哥布林和法师存在纯特效格混入身体序列的问题。',
'','## 证据与边界','',
'已逐帧查看全部八段的联系表，并检查编号源图与切片。以下证据来自这些逐帧静态对照；播放器的正常速度/慢放/逐帧功能由浏览器另行验证。正常速度播放的美术效果仍需在审查页确认；游戏内移速、脚滑与命中窗口未测。',
'','原行统一100ms曝光便于比较；并非建议所有动画都用10fps。暂定根锚点用于排除明显的画布位移，不能作为已校准的生产pivot。矩形里的邻格残留来自源图，不重绘、不清背景，也不把残留判为角色自身解剖问题。',
'','## 问题与修正','']
for role in ['hero','melee','runner','caster']:
    title=next(c['character'] for c in d['clips'] if c['role']==role)
    parts+=['### '+title,'']
    for f in findings:
        if f['role']!=role:continue
        parts += [f"- **{f['frames']} · {f['title']}（{f['level']}）**：{f['evidence']} 修正：{f['fix']}",'']
parts+=['## 两个重排试验','',
'持斧攻击：保留R04F01/02/03/04/05/07/08，曝光依次100/130/130/60/80/140/160ms，总长800ms。用途是移除身体消失的纯刀光格，并让蓄力与收势相对更长。斧头结构、握点与缺失收势没有修好。',
'','法师施法：保留R03F01/03/04/05/06、R04F01/03/05/07/08，曝光100/100/100/140/100/60/60/80/120/140ms，总长1000ms。跳过回落/重复姿势及纯弹体格，测试聚力到释放的结构。举杖到前指仍需绘制过渡。',
'','两个版本都只使用原图像素，时长是审查候选，未覆盖游戏参数；它们不是已修复成片。',
'','## 下一轮制作任务','',
'1. 主角：先以既定形象重做原地行走骨架，补明确经过姿势；挥剑先确定单斩还是两连斩。',
'2. 持剑哥布林：保留举剑和结束姿势，重做一张清楚的命中及一张随动，刀光独立。',
'3. 持斧哥布林：先固定斧头设计与握点，重画奔跑R03F03–05；攻击补回收姿势。',
'4. 法师：角色身体、杖上魔法与飞行弹体分开；补“举杖聚力→前指”的过渡，用袍下骨架验证行走。',
'','建议先完成主角行走和持剑哥布林单斩两段，建立通过标准后再批量扩展。没有调用生图或付费服务，没有修改正式Unity角色/动画/场景。']
(root/'审查报告.md').write_text('\n'.join(parts)+'\n',encoding='utf-8')

page='''<!doctype html><html lang="zh-CN"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>四角色动作试用</title>
<style>:root{color-scheme:dark;font-family:system-ui,sans-serif}body{margin:0;background:#0f1921;color:#edf4f7}main{max-width:1400px;margin:auto;padding:28px}h1{font-size:30px;margin:0 0 10px}p{color:#b8c9d2;line-height:1.7}button,select,a.link{background:#203846;color:#eef5f7;border:1px solid #466474;padding:12px 16px;border-radius:8px;margin:5px 6px 5px 0;cursor:pointer;text-decoration:none}button.active{background:#176c60;border-color:#79dfbf}nav{margin:20px 0}select{max-width:100%}.views{display:grid;grid-template-columns:1fr;gap:16px}.views.compare{grid-template-columns:repeat(auto-fit,minmax(min(100%,520px),1fr))}section.panel{background:#14242e;border:1px solid #304d5d;border-radius:12px;overflow:hidden}h2{font-size:18px;margin:16px}iframe{width:100%;height:1100px;border:0;background:#111b23}.tag{color:#edd398;font-size:14px}article{padding:16px;background:#1b303c;border-left:3px solid #5dbba4;border-radius:6px;margin:12px 0}article h3{margin:0 0 8px;font-size:17px}article p{margin:6px 0}a{color:#81e5cd}#audit{display:block;max-width:100%;margin:20px auto;border:1px solid #35515f;border-radius:8px}summary{cursor:pointer;padding:16px}label{display:inline-block;margin:12px 0}footer{color:#9db3c1;font-size:13px;margin-top:30px}</style>
<main><h1>四角色动作试用</h1><p>原始序列 × 8 · 原帧重排试验 × 2 · 71个原始曝光位置<br>先看原始版本，再对照具体帧。重排可以去掉不该出现的格子，补画仍然负责结构和运动连续性。</p>
<span class="tag">本轮为素材诊断与序列编辑试验 · 尚未重做角色原画或接入游戏</span>
<nav id="roles"></nav><select id="clips" aria-label="选择动作"></select> <label id="compareLabel"><input id="compare" type="checkbox">同时对照重排试验</label> <a class="link" href="审查报告.md">完整审查报告</a>
<div id="views" class="views"><section class="panel"><h2 id="primaryTitle"></h2><iframe id="primary" title="动作审查"></iframe></section><section id="editedPanel" class="panel" hidden><h2 id="editedTitle"></h2><iframe id="edited" title="重排试验"></iframe></section></div>
<h2>这一角色的审查意见</h2><div id="issues"></div><details><summary>查看原图与本次切片编号（只标注抽查区域）</summary><img id="audit" alt="原图切片编号"></details>
<footer>根锚点为人工暂定。原行100ms等曝光用于比较，不是最终动作规格。缺少原作者时间轴，行分组为候选。逐帧静态视觉审查、浏览器播放功能和游戏内动作验收分别记录。</footer></main>
<script id="trial" type="application/json">__DATA__</script><script>
const data=JSON.parse(document.getElementById('trial').textContent),$=id=>document.getElementById(id);let role='hero';
const roleList=[['hero','白发主角'],['melee','持剑哥布林'],['runner','持斧哥布林'],['caster','紫衣法师']];
function render(){document.querySelectorAll('#roles button').forEach(b=>b.classList.toggle('active',b.dataset.role===role));const clips=data.trial.clips.filter(c=>c.role===role&&c.kind==='source');$('clips').replaceChildren(...clips.map(c=>new Option(c.title+' · '+c.count+'帧',c.action)));$('audit').src=role+'-source-audit.png';$('issues').replaceChildren(...data.findings.filter(f=>f.role===role).map(f=>{const el=document.createElement('article');const h=document.createElement('h3');h.textContent=f.frames+' · '+f.title;const p=document.createElement('p');p.textContent=f.evidence;const fix=document.createElement('p');fix.textContent='修正：'+f.fix;const small=document.createElement('span');small.className='tag';small.textContent=f.level;el.append(h,small,p,fix);return el}));update()}
function update(){const c=data.trial.clips.find(c=>c.role===role&&c.action===$('clips').value);const edit=data.trial.clips.find(e=>e.role===role&&e.action===c.action+'-edit');$('primary').src=c.path;$('primaryTitle').textContent=c.character+' / '+c.title+'（原始行序）';$('compareLabel').hidden=!edit;const show=!!edit&&$('compare').checked;$('editedPanel').hidden=!show;$('views').classList.toggle('compare',show);if(show){$('edited').src=edit.path;$('editedTitle').textContent=edit.title}else $('edited').src='about:blank'}
roleList.forEach(([id,name])=>{const b=document.createElement('button');b.textContent=name;b.dataset.role=id;b.onclick=()=>{role=id;render()};$('roles').append(b)});$('clips').onchange=update;$('compare').onchange=update;render();window.fourCharacterTrial={data};
</script></html>'''
page=page.replace('<style>','<style>[hidden]{display:none!important}')
page=page.replace('__DATA__',json.dumps(dict(trial=d,findings=findings),ensure_ascii=False).replace('<','\\u003c'))
(root/'index.html').write_text(page,encoding='utf-8')
for c in d['clips']:
    p=root/c['path'];s=p.read_text(encoding='utf-8');s=s.replace('</style>','.stage{max-width:calc(100% - 44px)} </style>');p.write_text(s,encoding='utf-8')
print('Index and 12 evidence-based findings written.')
