from pathlib import Path
import hashlib,importlib.util,json,base64,io,shutil
import numpy as np
from PIL import Image

out=Path('outputs/GoblinSlashFX').resolve();a=np.array(Image.open(out/'slash-fx-v2.png').convert('RGBA'))
roots=[(182,346),(605,346),(1050,346),(1530,346),(1930,346),(164,670),(650,668),(1120,668),(1580,670),(1960,670)]
edges=[[0,450,800,1240,1680,2172],[0,480,950,1400,1800,2172]]
labels=['竖剑警戒','向后加载','后引剑刃','最大蓄力','过顶挥出/命中开始','前方接触/随动','碎光衰减','回收/余光','收剑','竖剑归位']
(out/'frames').mkdir(exist_ok=True);frames=[];checks=[]
for i,((rx,ry),name) in enumerate(zip(roots,labels)):
    row=i//5;col=i%5;l,r=edges[row][col:col+2];t,b=(0,355) if row==0 else (355,724)
    canvas=np.zeros((512,640,4),dtype=np.uint8);x0,y0=rx-288,ry-448;xs,ys=max(l,x0,0),max(t,y0,0);xe,ye=min(r,x0+640,a.shape[1]),min(b,y0+512,a.shape[0]);canvas[ys-y0:ye-y0,xs-x0:xe-x0]=a[ys:ye,xs:xe]
    assert int((canvas[:,:,3]>80).sum())==int((a[t:b,l:r,3]>80).sum()),f'Visible pixels clipped in frame {i+1}'
    assert np.array_equal(canvas[ys-y0:ye-y0,xs-x0:xe-x0],a[ys:ye,xs:xe])
    filename=f'frames/SlashFX_{i+1:02d}.png';Image.fromarray(canvas).save(out/filename)
    frames.append(dict(file=filename,anchor=[288,448],duration_ms=100,label=f'{i+1:02d} {name}'))
    checks.append(dict(frame=i+1,source_region=[l,t,r-l,b-t],source_root=[rx,ry],visible_alpha_pixels=int((canvas[:,:,3]>80).sum()),copied_rgba_exact=True))
manifest=dict(title='持剑哥布林 · 参考轨迹＋刀光版',note='十帧单次1秒。1–4帧蓄力，5–6帧银白斩击弧与接触闪光，7–8帧碎光衰减，9–10帧收势。特效目前合成在候选帧内，未接入Unity。',loop=False,frames=frames)
(out/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
(out/'extraction-report.json').write_text(json.dumps(dict(source_size=[a.shape[1],a.shape[0]],frame_size=[640,512],root=[288,448],frames=checks,method='Manual nonuniform frame regions isolate each body and detached FX; assigned original RGBA copied, all alpha>80 pixels in each region retained.'),indent=2),encoding='utf-8')
spec=importlib.util.spec_from_file_location('review',r'E:\CodexData\skills\game-2d-animation-designer\scripts\build_review.py');builder=importlib.util.module_from_spec(spec);spec.loader.exec_module(builder);builder.build(out/'manifest.json',out/'review')
strip=Image.new('RGBA',(6400,512))
for i,f in enumerate(frames):strip.paste(Image.open(out/f['file']),(i*640,0))
strip.save(out/'GoblinSlashFX-strip10.png')
oldroot=Path('outputs/GoblinSingleSlash');old=json.loads((oldroot/'longblade-manifest.json').read_text(encoding='utf-8'))
def package(spec,root,scale):
    result=[]
    for f in spec['frames']:
        im=Image.open(root/f['file']).convert('RGBA');buf=io.BytesIO();im.save(buf,format='PNG')
        result.append(dict(label=f['label'],duration=f['duration_ms'],anchor=f['anchor'],data='data:image/png;base64,'+base64.b64encode(buf.getvalue()).decode()))
    return dict(frames=result,scale=scale)
data=dict(old=package(old,oldroot,.80),new=package(manifest,out,.90),total=1000)
(out/'preview-data.json').write_text(json.dumps(data,ensure_ascii=False),encoding='utf-8')
reference=Path(r'E:\UnityProject\HeroArena\Assets\Role\Bafu\spr_basecharacter_attack_strip10.png')
record=dict(date='2026-10-18',request='Reference supplied sword motion trajectory and add effects to the existing long-blade goblin attack.',reference_path=str(reference),reference_sha256=hashlib.sha256(reference.read_bytes()).hexdigest(),reference_role='motion and timing only; not character appearance',tool='built-in image_gen',iterations=2,selected='slash-fx-v2.png',selected_sha256=hashlib.sha256((out/'slash-fx-v2.png').read_bytes()).hexdigest(),frames=10,duration_ms=1000,hit_window_candidate=.4,effects=dict(none=[1,2,3,4,9,10],main_arc=[5,6],contact_flash=[6],decay=[7,8]),preview_normalization='Display-only old scale .80 / new scale .90 to approximately match body height and shared root. PNGs unchanged.',status='generated and extracted candidate; no Unity change',limitations=['Body/weapon details have generative drift; pose 9 hand/perspective and blade proportions need review.','Effects currently baked into composite frames; no independent FX layer delivered.','No automatic force/animation-quality acceptance; in-game scale, timing and damage not tested.'])
(out/'generation-record.json').write_text(json.dumps(record,ensure_ascii=False,indent=2),encoding='utf-8')
print('Ten complete composite frames and normalized comparison data ready.')
