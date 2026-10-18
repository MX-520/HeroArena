"""Exact rectangular crops of the user supplied sheet; never resample pixels."""
from pathlib import Path
import hashlib
import json
import zipfile
import sys
from PIL import Image, ImageDraw

SOURCE = Path(r'E:\UnityProject\HeroArena\Assets\Role\Test\Player\TestPlayer.png')
OUT = Path(sys.argv[1])
OUT.mkdir(parents=True, exist_ok=True)
im = Image.open(SOURCE).convert('RGBA')
rows = [
    (20,177,[20,199,363,545,720,901,1073,1240,1390]),
    (180,340,[30,208,381,554,728,896,1078,1250,1428]),
    (346,495,[18,198,364,538,714,895,1083,1252,1438]),
    None,
    (676,803,[15,215,378,544,735,891,1077,1261,1410]),
    (800,943,[18,210,382,557,734,915,1081,1267,1438]),
    (940,1068,[18,212,404,580,781,988,1210,1430]),
]
frames=[]
for ri,row in enumerate(rows,1):
    if row:
        top,bottom,xx=row
        boxes=[(a,top,b,bottom) for a,b in zip(xx,xx[1:])]
    else:
        # Attack artwork touches across poses. Overlap preserves the source effects;
        # it also retains neighbour fragments and is explicitly documented.
        boxes=[(20,504,198,670),(205,508,430,670),(398,525,608,670),
               (577,489,817,670),(802,489,981,670),(976,503,1252,679),(1243,512,1448,671)]
    for ci,box in enumerate(boxes,1):
        x,y,right,bottom=box
        name=f'TestPlayer_R{ri:02d}_{ci:02d}'
        crop=im.crop(box)
        assert crop.getchannel('A').getbbox() is not None
        path=OUT/(name+'.png')
        crop.save(path)
        # Ground at the row baseline; placeholder until animation grouping is approved.
        baseline=[167,331,488,657,797,938,1059][ri-1]
        frames.append(dict(name=name,row=ri,index=ci,source_rect=[x,y,right-x,bottom-y],
                           pivotX=0.5,pivotY=(bottom-baseline)/(bottom-y),
                           sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
report=dict(source='Assets/Role/Test/Player/TestPlayer.png',
    source_sha256=hashlib.sha256(SOURCE.read_bytes()).hexdigest(),size=list(im.size),
    count=len(frames),row_counts=[8,8,8,7,8,8,7],frames=frames,
    limitations=['Attack row has touching/overlapping artwork. Rectangles overlap to retain effects and may include adjacent pose fragments.',
                 'Rows are source order, not verified animation groups. Pivots are provisional row-ground anchors.',
                 'Source alpha includes faint residual pixels. No background cleaning or generated reconstruction was applied.'])
assert len(frames)==54
(OUT/'slicing-layout.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
audit=Image.new('RGB',im.size,'#34454c');audit.paste(im,mask=im.getchannel('A'))
d=ImageDraw.Draw(audit)
for f in frames:
    x,y,w,h=f['source_rect'];d.rectangle((x,y,x+w-1,y+h-1),outline='#58f5cb',width=2)
    d.text((x+3,y+3),f['name'].replace('TestPlayer_',''),fill='white',stroke_width=2,stroke_fill='black')
audit.save(OUT/'TestPlayer-numbered.png')
with zipfile.ZipFile(OUT/'TestPlayer-54-crops.zip','w',zipfile.ZIP_DEFLATED) as z:
    for f in frames:z.write(OUT/(f['name']+'.png'),f['name']+'.png')
    z.write(OUT/'slicing-layout.json','slicing-layout.json')
for f in frames:
    x,y,w,h=f['source_rect']
    assert Image.open(OUT/(f['name']+'.png')).tobytes()==im.crop((x,y,x+w,y+h)).tobytes()
print(json.dumps(dict(count=len(frames),row_counts=report['row_counts'],pixel_comparison='54/54 exact',source_sha256=report['source_sha256'])))
