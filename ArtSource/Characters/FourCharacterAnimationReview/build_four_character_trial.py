"""Audit selected motion rows from the four user-supplied sheets; preserve pixels."""
from pathlib import Path
import argparse, hashlib, importlib.util, json, shutil
from PIL import Image, ImageDraw

ap=argparse.ArgumentParser()
ap.add_argument('--project',type=Path,default=Path(r'E:\UnityProject\HeroArena'))
ap.add_argument('--out',type=Path,required=True)
ap.add_argument('--review-script',type=Path,default=Path(r'E:\CodexData\skills\game-2d-animation-designer\scripts\build_review.py'))
args=ap.parse_args();out=args.out.resolve();out.mkdir(parents=True,exist_ok=True)
spec=importlib.util.spec_from_file_location('review_builder',args.review_script)
builder=importlib.util.module_from_spec(spec);spec.loader.exec_module(builder)
(out/'sources').mkdir(exist_ok=True)
records=[];clips=[]

def strip(top,bottom,edges,ground,roots=None):
    return [dict(rect=[x,top,r-x,bottom-top],anchor=[(roots[i] if roots else (x+r)/2)-x,ground-top]) for i,(x,r) in enumerate(zip(edges,edges[1:]))]

hero_attack=[(20,504,198,670),(205,508,430,670),(398,525,608,670),(577,489,817,670),(802,489,981,670),(976,503,1252,679),(1243,512,1448,671)]
roles=[
 dict(id='hero',title='白发主角',suffix='13_57_34-3.png',segments=[
  ('walk','行走候选',True,[(2,strip(180,340,[30,208,381,554,728,896,1078,1250,1428],331,[124,298,474,650,820,990,1164,1340]))]),
  ('attack','挥剑候选',False,[(4,[dict(rect=[x,y,r-x,b-y],anchor=[root-x,657-y]) for (x,y,r,b),root in zip(hero_attack,[128,300,485,685,905,1090,1340])])])]),
 dict(id='melee',title='持剑哥布林',suffix='13_57_32-2.png',segments=[
  ('walk','行走候选',True,[(2,strip(269,451,[4,179,363,547,727,911,1091,1268,1448],438,[88,270,450,630,810,990,1170,1350]))]),
  ('attack','挥剑候选',False,[(3,strip(462,676,[4,180,365,552,735,929,1100,1280,1448],656,[87,278,450,635,820,1000,1180,1360]))])]),
 dict(id='runner',title='持斧哥布林',suffix='13_57_36-4.png',segments=[
  ('run','奔跑候选',True,[(3,strip(386,550,[3,185,368,551,736,917,1090,1263,1448],546,[95,275,458,638,825,1007,1185,1365]))]),
  ('attack','挥斧候选',False,[(4,strip(551,744,[3,199,386,562,737,936,1091,1244,1448],733,[100,298,480,650,840,1015,1165,1345]))])]),
 dict(id='caster',title='紫衣法师',suffix='13_57_26-1.png',segments=[
  ('walk','移动候选',True,[(2,strip(178,330,[20,207,386,560,736,912,1090,1266,1448],326,[112,289,465,643,820,996,1173,1350]))]),
  ('cast','蓄力与释放候选',False,[(3,strip(323,509,[18,208,390,577,747,916,1090,1267,1448],501,[110,288,487,666,841,1010,1178,1354])),
                                     (4,strip(510,662,[17,211,392,575,747,929,1077,1264,1448],657,[112,293,469,644,820,1002,1163,1345]))])])
]

for role in roles:
    source=next(args.project.glob('*'+role['suffix']));dest=out/'sources'/(role['id']+'.png');shutil.copy2(source,dest)
    im=Image.open(source).convert('RGBA');audit=Image.new('RGB',im.size,'#2c3e47');audit.paste(im,mask=im.getchannel('A'));draw=ImageDraw.Draw(audit)
    alpha=im.getchannel('A');hist=alpha.histogram()
    records.append(dict(role=role['id'],title=role['title'],source=source.name,sha256=hashlib.sha256(source.read_bytes()).hexdigest(),size=list(im.size),alpha_minmax=alpha.getextrema(),fully_transparent_pixels=hist[0]))
    for action,title,loop,rows in role['segments']:
        frames=[]
        for row,slices in rows:
            for i,f in enumerate(slices,1):
                label=f'R{row:02d}F{i:02d}'
                frames.append(dict(file='../sources/'+role['id']+'.png',rect=f['rect'],anchor=f['anchor'],duration_ms=100,label=label))
                x,y,w,h=f['rect'];draw.rectangle((x,y,x+w-1,y+h-1),outline='#65e2bd',width=2);draw.text((x+3,y+3),label,fill='white',stroke_width=2,stroke_fill='#101820')
        folder=out/(role['id']+'-'+action);folder.mkdir(exist_ok=True)
        manifest=dict(title=role['title']+' / '+title,note='原始像素与原行序列。每帧100ms仅用于等曝光审查，不是最终动画时长；根锚点按躯干位置手工暂定，不能据此判断游戏内滑步。部分武器/特效与邻帧交叠，矩形会保留碎片。',loop=loop,frames=frames)
        path=folder/'manifest.json';path.write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
        builder.build(path,folder)
        clips.append(dict(role=role['id'],character=role['title'],action=action,title=title,path=folder.name+'/review.html',count=len(frames),manifest=folder.name+'/manifest.json',kind='source'))
    audit.save(out/(role['id']+'-source-audit.png'))

# Editorial experiments only: same exact source pixels; no invented or repaired poses.
for roleid,action,keep,durations,title in [
 ('runner','attack',[0,1,2,3,4,6,7],[100,130,130,60,80,140,160],'挥斧重排试验：跳过纯刀光格'),
 ('caster','cast',[0,2,3,4,5,8,10,12,14,15],[100,100,100,140,100,60,60,80,120,140],'施法重排试验：移除回退与纯弹体格')]:
    original=out/(roleid+'-'+action)/'manifest.json';m=json.loads(original.read_text(encoding='utf-8'))
    m['frames']=[dict(m['frames'][i],duration_ms=t) for i,t in zip(keep,durations)]
    m['title']=next(r['title'] for r in roles if r['id']==roleid)+' / '+title
    m['note']='仅用原图重新选帧并按阶段分配曝光，展示序列编辑的影响。没有补画、修复结构或清除交叠；不是合格成片，也未接入游戏。根锚点人工暂定，相邻帧碎片仍然保留。'
    folder=out/(roleid+'-'+action+'-edit');folder.mkdir(exist_ok=True);p=folder/'manifest.json';p.write_text(json.dumps(m,ensure_ascii=False,indent=2),encoding='utf-8');builder.build(p,folder)
    clips.append(dict(role=roleid,character=next(r['title'] for r in roles if r['id']==roleid),action=action+'-edit',title=title,path=folder.name+'/review.html',count=len(m['frames']),manifest=folder.name+'/manifest.json',kind='editorial-experiment'))

report=dict(sources=records,clips=clips,source_frames=sum(c['count'] for c in clips if c['kind']=='source'),editorial_frames=sum(c['count'] for c in clips if c['kind']!='source'),scope='Selected movement and combat rows only, not all poses on each sheet; action grouping inferred, not supplied by artist.')
(out/'trial-manifest.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(dict(source_frames=report['source_frames'],editorial_frames=report['editorial_frames'],clips=len(clips))))
