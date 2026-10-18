from pathlib import Path
import json,hashlib,importlib.util
import numpy as np
from scipy.ndimage import label,find_objects,distance_transform_edt
from PIL import Image
out=Path('outputs/GoblinSingleSlash').resolve();source=out/'slash-longblade-v1.png';a=np.array(Image.open(source).convert('RGBA'))
labels,n=label(a[:,:,3]>80);areas=np.bincount(labels.ravel());large=[i for i in range(1,n+1) if areas[i]>4000];assert len(large)==8
objects=find_objects(labels);large.sort(key=lambda i:(objects[i-1][0].start//443,objects[i-1][1].start))
seeds=np.where(np.isin(labels,large),labels,0);_,indices=distance_transform_edt(seeds==0,return_indices=True);owner=seeds[tuple(indices)]
roots=[(214,390),(666,390),(1110,390),(1540,390),(188,824),(655,824),(1106,824),(1546,824)]
original=json.loads((out/'manifest.json').read_text(encoding='utf-8'));frames=[];checks=[]
(out/'longblade-frames').mkdir(exist_ok=True)
for i,(component,(rx,ry),spec) in enumerate(zip(large,roots,original['frames']),1):
    x0,y0=rx-240,ry-448;xs,ys=max(0,x0),max(0,y0);xe,ye=min(a.shape[1],x0+512),min(a.shape[0],y0+512)
    canvas=np.zeros((512,512,4),dtype=np.uint8);part=a[ys:ye,xs:xe].copy();part[owner[ys:ye,xs:xe]!=component]=0;canvas[ys-y0:ye-y0,xs-x0:xe-x0]=part
    assert (canvas[:,:,3]>80).sum()==areas[component],'Principal silhouette clipped'
    assigned=owner[ys:ye,xs:xe]==component;assert np.array_equal(part[assigned],a[ys:ye,xs:xe][assigned])
    name=f'longblade-frames/Slash_{i:02d}.png';Image.fromarray(canvas).save(out/name)
    frames.append(dict(spec,file=name,anchor=[240,448]));checks.append(dict(frame=i,root_source=[rx,ry],principal_pixels=int(areas[component]),source_rgba_preserved=True))
m=dict(original,title='持剑哥布林 · 单斩修正版（加长剑刃）',note='按用户要求，以约20%加长为目标重绘八帧剑刃，实际透视与比例仍需视觉审查；同一八姿势、1秒曝光安排。剑柄/握点按参考保持，生成有轻微细节漂移。未接入Unity。',frames=frames)
(out/'longblade-manifest.json').write_text(json.dumps(m,ensure_ascii=False,indent=2),encoding='utf-8')
(out/'longblade-extraction-report.json').write_text(json.dumps(dict(source_sha256=hashlib.sha256(source.read_bytes()).hexdigest(),generated_size=[a.shape[1],a.shape[0]],frame_size=[512,512],root=[240,448],frames=checks,notes='Non-uniform generated board; did not assume exact grid division. Principal silhouettes and assigned RGBA preserved.'),indent=2),encoding='utf-8')
spec=importlib.util.spec_from_file_location('review',r'E:\CodexData\skills\game-2d-animation-designer\scripts\build_review.py');builder=importlib.util.module_from_spec(spec);spec.loader.exec_module(builder);builder.build(out/'longblade-manifest.json',out/'longblade-review')
strip=Image.new('RGBA',(4096,512))
for i,f in enumerate(frames):strip.paste(Image.open(out/f['file']),(512*i,0))
strip.save(out/'GoblinSingleSlash-longblade-strip8.png')
p=out/'generation-and-design-record.json';d=json.loads(p.read_text(encoding='utf-8'));d['sword_edit']=dict(request='lengthen the sword a little',target='approximately 20% longer blade; preserve hilt and grip',tool='built-in image_gen',reference='slash-clean-4x2.png',selected='slash-longblade-v1.png',sha256=hashlib.sha256(source.read_bytes()).hexdigest(),final_manifest='longblade-manifest.json',exact_percentage_verified=False,limitations='Generative edit; minor pose/detail drift remains, exact identical blade proportions not numerically certified.');p.write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
page=(out/'index.html').read_text(encoding='utf-8').replace('src="revised-review/review.html"','src="longblade-review/review.html"').replace('新绘制动作 · 无刀光、独立透明帧','新绘制动作 · 加长剑刃')
page=page.replace('<span class="tag">','<p>已按你的要求加长剑刃。可切换“加长剑刃版 / 加长前版本”比较，八姿势和1秒曝光安排保持一致。</p><select id="variant" aria-label="选择剑长"><option value="longblade-review/review.html">加长剑刃版</option><option value="revised-review/review.html">加长前版本</option></select><span class="tag">',1)
page=page.replace('<script>const ids=',"<script>document.getElementById('variant').onchange=e=>{document.getElementById('new').src=e.target.value};const ids=")
(out/'index.html').write_text(page,encoding='utf-8')
listener="""<script>window.addEventListener('message',e=>{if(e.source!==parent||e.data?.channel!=='slash-review')return;const d=e.data,$=id=>document.getElementById(id);if(d.action==='restart')$('restart').click();if(d.action==='play'&&$('play').textContent==='播放')$('play').click();if(d.action==='pause'&&$('play').textContent==='暂停')$('play').click();if(d.action==='frame'){$('scrub').value=d.index;$('scrub').dispatchEvent(new Event('input'))}if(d.action==='speed')$('speed').value=d.value;if(d.action==='loop'){$('loop').checked=d.value;$('loop').dispatchEvent(new Event('change'))}});</script>"""
p=out/'longblade-review/review.html';s=p.read_text(encoding='utf-8').replace('</style>','.stage{max-width:calc(100% - 44px)}canvas{width:min(420px,100%)!important}</style>').replace('</html>',listener+'</html>');p.write_text(s,encoding='utf-8')
print('Long-blade version: 8 complete 512px frames, fixed anchor, original timing retained.')
