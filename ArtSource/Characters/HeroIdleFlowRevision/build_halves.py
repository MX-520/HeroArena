from pathlib import Path
from PIL import Image
import numpy as np
import argparse,json,hashlib

parser=argparse.ArgumentParser(description='Pack two generated six-pose rows without drawing or tweening')
parser.add_argument('--source-root',type=Path,default=Path(__file__).resolve().parent)
parser.add_argument('--out',type=Path,required=True)
args=parser.parse_args();stage=args.source_root.resolve();out=args.out.resolve();out.mkdir(parents=True,exist_ok=True)
folder=stage/'Hero';(folder/'frames').mkdir(parents=True,exist_ok=True)
strip=Image.new('RGBA',(7680,640));frames=[];checks=[]
for half in [1,2]:
 src=stage/f'half{half}-board.png';im=Image.open(src).convert('RGBA');a=np.array(im)
 for col in range(6):
  i=(half-1)*6+col;l,r=round(col*im.width/6),round((col+1)*im.width/6)
  region=a[:,l:r];ys,xs=np.nonzero(region[:,:,3]>80);baseline=int(ys.max())+1
  fy,fx=np.nonzero(region[max(0,baseline-16):baseline,:,3]>80);rootx=l+round((int(fx.min())+int(fx.max())+1)/2);rooty=baseline
  dx,dy=320-rootx,544-rooty
  # Exclude transparent padding/noise beyond a small margin around the complete visible silhouette.
  t,b=max(0,int(ys.min())-12),min(im.height,int(ys.max())+13)
  canvas=Image.new('RGBA',(640,640));canvas.paste(im.crop((l,t,r,b)),(l+dx,t+dy));arr=np.array(canvas)
  assert int((region[:,:,3]>80).sum())==int((arr[:,:,3]>80).sum()),f'Visible clipping in {i+1}'
  vy,vx=np.nonzero(region[:,:,3]>80);assert np.array_equal(region[vy,vx],arr[vy+dy,vx+l+dx])
  filename=f'frames/Idle_{i+1:02d}.png';canvas.save(folder/filename);strip.paste(canvas,(i*640,0))
  frames.append({'file':filename,'anchor':[320,544],'duration_ms':125,'label':f'{i+1:02d}'})
  checks.append({'frame':i+1,'board':src.name,'board_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'source_region':[l,t,r-l,b-t],'source_root':[rootx,rooty],'alpha_gt80_preserved':True,'visible_rgba_exact':True,'visible_pixels':int((arr[:,:,3]>80).sum()),'padding_policy':'12px margin outside visible alpha>80 silhouette; any lower-alpha noise farther away excluded'})
dest=out/'Hero-Idle-Flow-strip12.png';strip.save(dest)
for i,f in enumerate(frames):assert strip.crop((i*640,0,(i+1)*640,640)).tobytes()==Image.open(folder/f['file']).tobytes()
(folder/'manifest.json').write_text(json.dumps({'title':'主角 · 头腰曲线与披风联动候选','note':'12帧/1.5秒；两段六姿势按原序连接，无程序补帧，未接入Unity。','loop':True,'frames':frames},ensure_ascii=False,indent=2),encoding='utf-8')
(stage/'extraction-report.json').write_text(json.dumps({'method':'six frames from each of two one-row boards, original order; no drawing/tweening/resizing','cell':[640,640],'root':[320,544],'frames':checks,'png':dest.name,'sha256':hashlib.sha256(dest.read_bytes()).hexdigest()},indent=2),encoding='utf-8')
print('PASS: 12 silhouettes and original visible RGBA preserved; final 7680x640 matches every extracted frame.')
