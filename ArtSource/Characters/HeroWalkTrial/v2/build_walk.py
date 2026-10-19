from pathlib import Path
from PIL import Image
import numpy as np,json,hashlib
p=Path(__file__).parent;out=p/'delivery';out.mkdir(exist_ok=True)
strip=Image.new('RGBA',(6400,640));frames=[];checks=[]
# All single-pose renders have the same 1254x1254 canvas and shared projected root.
# One common nearest-neighbor reduction, never per-foot recentering or limb transforms.
source_root=(686,1097);scaled_size=(502,502);root=(320,544)
dx=root[0]-round(source_root[0]*502/1254);dy=root[1]-round(source_root[1]*502/1254)
for i in range(10):
 src=p/'source-frames'/f'Walk_{i+1:02d}.png';im=Image.open(src).convert('RGBA')
 assert im.size==(1254,1254)
 small=im.resize(scaled_size,Image.Resampling.NEAREST)
 frame=Image.new('RGBA',(640,640));frame.paste(small,(dx,dy))
 assert np.array_equal(np.array(frame)[dy:dy+502,dx:dx+502],np.array(small))
 strip.paste(frame,(i*640,0))
 frames.append({'file':'Hero-Walk-v2-strip10.png','rect':[i*640,0,640,640],'anchor':[320,544],'duration_ms':125,'label':f'{i+1:02d}'})
 checks.append({'frame':i+1,'source':f'../source-frames/{src.name}','source_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'source_size':im.size,'common_source_root':source_root,'scaled_size':scaled_size,'placement':[dx,dy],'bounds':frame.getbbox(),'normalized_pixels_exact':True,'resampling':'nearest; uniform 502/1254 for all frames'})
strip.save(out/'Hero-Walk-v2-strip10.png')
manifest={'title':'主角Walk修订 · 左右承重分开 / 待确认','note':'10帧8fps，1.25秒循环。逐姿势重绘；近侧暖亮/远侧阴影维持身份。未接入Unity。','loop':True,'frames':frames}
(out/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
report={'status':'technical normalization only; visual review pending','frames':10,'cell':[640,640],'size':strip.size,'fps':8,'duration_ms':1250,'root':root,'unity_pivot':[.5,.15],'alpha_extrema':strip.getextrema()[3],'sha256':hashlib.sha256((out/'Hero-Walk-v2-strip10.png').read_bytes()).hexdigest(),'checks':checks,'note':'Original source pixels are resampled once with nearest; not claimed lossless relative to source. No per-frame foot bounding-box/root fitting.'}
(out/'extraction-report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('10 frames, shared root, nearest only, output 6400x640, 8fps / 1.25 seconds')
