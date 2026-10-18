from pathlib import Path
from PIL import Image
import numpy as np
import hashlib, json
import argparse

parser = argparse.ArgumentParser(description='Extract generated 4x3 idle boards without redrawing or resizing visible pixels.')
parser.add_argument('--source-root', type=Path, default=Path(__file__).resolve().parent)
parser.add_argument('--out', type=Path, required=True)
args = parser.parse_args()
stage = args.source_root.resolve()
out = args.out.resolve()
out.mkdir(parents=True, exist_ok=True)
roles = {'Hero':'主角（空手）头腰曲线与披风联动Idle版'}
report = {'tool':'built-in image_gen', 'status':'technical extraction only; current user acceptance is recorded separately in acceptance.json', 'cell':[640,640], 'anchor':[320,544], 'frames_per_role':12, 'duration_ms':1500, 'roles':{}}
for role, title in roles.items():
    src = stage / f'{role}-board.png'
    if not src.exists():
        continue
    im = Image.open(src).convert('RGBA')
    a = np.array(im)
    row_edges = [0]
    counts = (a[:,:,3]>80).sum(axis=1)
    for boundary in [im.height/3,im.height*2/3]:
        lo,hi = round(boundary-im.height/24),round(boundary+im.height/24)
        spans=[];start=None
        for y in range(lo,hi+1):
            empty = y<hi and counts[y]==0
            if empty and start is None: start=y
            if not empty and start is not None:
                spans.append((start,y));start=None
        assert spans, 'Inspect row separation manually'
        first,last=max(spans,key=lambda span:span[1]-span[0])
        row_edges.append((first+last)//2)
    row_edges.append(im.height)
    print('Inspected empty-band row boundaries:',row_edges)
    folder = stage / role
    (folder / 'frames').mkdir(parents=True, exist_ok=True)
    strip = Image.new('RGBA',(7680,640))
    specs, checks = [], []
    for i in range(12):
        col, row = i%4, i//4
        l,r = round(col*im.width/4), round((col+1)*im.width/4)
        t,b = row_edges[row:row+2]
        region = a[t:b,l:r]
        ys,xs = np.nonzero(region[:,:,3]>80)
        baseline = int(ys.max())+1
        # The bottom sixteen opaque rows isolate the planted boots, not the moving cape/weapon.
        fy,fx = np.nonzero(region[max(0,baseline-16):baseline,:,3]>80)
        rootx = l+round((int(fx.min())+int(fx.max())+1)/2)
        rooty = t+baseline
        dx,dy = 320-rootx,544-rooty
        canvas = Image.new('RGBA',(640,640))
        canvas.paste(im.crop((l,t,r,b)),(l+dx,t+dy))
        arr = np.array(canvas)
        assert int((region[:,:,3]>0).sum()) == int((arr[:,:,3]>0).sum()), f'Clipped alpha: {role}{i+1}'
        # Verify every nontransparent original RGBA value was copied exactly.
        vy,vx = np.nonzero(region[:,:,3]>0)
        assert np.array_equal(region[vy,vx],arr[vy+t+dy,vx+l+dx]), f'Pixel alteration: {role}{i+1}'
        filename = f'frames/Idle_{i+1:02d}.png'
        canvas.save(folder / filename)
        strip.paste(canvas,(i*640,0))
        specs.append({'file':filename,'anchor':[320,544],'duration_ms':125,'label':f'{i+1:02d}'})
        checks.append({'frame':i+1,'source_rect':[l,t,r-l,b-t],'source_root':[rootx,rooty],'root_method':'midpoint of opaque boot soles in bottom sixteen rows; provisional game root','alpha_pixels_retained':int((arr[:,:,3]>0).sum()),'visible_rgba_exact':True,'opaque_bounds':canvas.getbbox()})
    dest = out / f'{role}-Idle-Flow-strip12.png'
    strip.save(dest)
    manifest = {'title':title+' · Idle','note':'12帧/1.5秒循环；脚底接地、可见头腰错相曲线与披风波浪；空手、轻像素风和披风波浪；未接入Unity。','loop':True,'frames':specs}
    (folder / 'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
    for i in range(12):
        assert strip.crop((i*640,0,(i+1)*640,640)).tobytes() == Image.open(folder / specs[i]['file']).tobytes()
    report['roles'][role] = {'source':src.name,'source_size':list(im.size),'source_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'png':dest.name,'png_sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),'actual_alpha_extrema':strip.getextrema()[3],'frames':checks}
(stage / 'extraction-report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('Built and verified:', ', '.join(report['roles']))
