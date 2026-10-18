"""Crop every visible small character on the supplied board; no generated frames."""
from pathlib import Path
import hashlib
import json
import zipfile
import numpy as np
from PIL import Image, ImageDraw, ImageFilter
from extract_board import fill_inside, components

ROOT = Path(__file__).resolve().parent
# Head centres checked against the numbered source audit, independent of labels.
ROWS = [
    ('Down', 428, 500, [108,155,201,248,290,334,378,423,468,518,562,610,656,701,746,795,841,890,938,986,1032,1078,1125,1171,1216,1263,1310,1357,1404,1449,1497]),
    ('Left', 551, 629, [106,156,204,253,298,351,401,455,505,556,601,651,699,746,790,841,890,941,993,1043,1089,1139,1192,1243,1290,1343,1390,1441,1494]),
    ('Right', 684, 760, [110,162,211,260,311,359,408,456,505,556,602,651,700,748,795,844,894,945,994,1045,1095,1145,1195,1245,1292,1343,1389,1441,1495]),
    ('Up', 820, 904, [109,156,206,256,306,359,411,463,513,564,614,665,715,765,815,866,917,969,1020,1072,1124,1176,1230,1283,1336,1389,1442,1493]),
]

def export():
    src = Image.open(ROOT/'source-board.png').convert('RGBA')
    source = np.array(src)
    folder = ROOT/'FullCropped'
    folder.mkdir(exist_ok=True)
    atlas = Image.new('RGBA', (96*31,112*4))
    audit = Image.new('RGB',(1536,460),'#20392b')
    draw = ImageDraw.Draw(audit)
    report = dict(status='passed', source_sha256=hashlib.sha256((ROOT/'source-board.png').read_bytes()).hexdigest(),
                  frame_size=[96,112], source_claimed_frames_per_direction=32, actual_counts={}, frames=[],
                  limitations=['The presentation board visibly contains 31/29/29/28 characters, not 32 per direction.',
                               'Touching scarves are separated at the midpoint between neighbouring heads; occluded pixels cannot be recovered.',
                               'Baked background removal is approximate. No missing frames are duplicated or generated.'])
    entries=[]
    for row,(direction,top,bottom,centres) in enumerate(ROWS):
        report['actual_counts'][direction]=len(centres)
        audit.paste(src.crop((0,top,1536,bottom)).convert('RGB'),(0,row*115+20))
        output=folder/direction
        output.mkdir(exist_ok=True)
        boundaries=[80]+[round((a+b)/2) for a,b in zip(centres,centres[1:])]+[1529]
        for index,centre in enumerate(centres):
            left,right=boundaries[index:index+2]
            rgb=source[top:bottom,left:right,:3]
            val=rgb.astype(int)
            raw=(val.min(2)<185)|((val.max(2)-val.min(2))>35)
            # Keep the actor's main silhouette, discarding detached neighbour pixels.
            objects=components(raw)
            if not objects:raise ValueError((direction,index,'No actor'))
            obj=max(objects,key=lambda c:c['area'])
            mask=np.zeros(raw.shape,bool)
            yy,xx=zip(*obj['points']);mask[np.array(yy),np.array(xx)]=True
            # Padding preserves silhouette pixels at a crop boundary during closing.
            pad=8
            padded=np.pad(mask,pad)
            closed=np.array(Image.fromarray(padded.astype('uint8')*255).filter(ImageFilter.MaxFilter(3)).filter(ImageFilter.MinFilter(3)))>0
            head=np.array(Image.fromarray(closed.astype('uint8')*255).filter(ImageFilter.MaxFilter(7)).filter(ImageFilter.MinFilter(7)))>0
            end=pad+obj['y']+round(obj['height']*.55)
            closed[:end]=head[:end]
            mask=fill_inside(closed)[pad:-pad,pad:-pad]
            yy,xx=np.where(mask)
            x0,x1,y0,y1=int(xx.min()),int(xx.max())+1,int(yy.min()),int(yy.max())+1
            cut=np.zeros((y1-y0,x1-x0,4),dtype='uint8')
            cut[:,:,:3]=rgb[y0:y1,x0:x1];cut[:,:,3]=mask[y0:y1,x0:x1]*255
            offset=round(48-(centre-(left+x0)))
            cell=Image.new('RGBA',(96,112));cell.paste(Image.fromarray(cut),(offset,104-cut.shape[0]))
            if offset<0 or offset+cut.shape[1]>96 or cut.shape[0]>104:raise ValueError('Clipped output canvas')
            path=output/f'{direction}_{index+1:02d}.png';cell.save(path);entries.append(path)
            atlas.paste(cell,(index*96,row*112))
            report['frames'].append(dict(direction=direction,index=index+1,source_rect=[left,top,right-left,bottom-top],head_centre=centre,path=path.relative_to(ROOT).as_posix()))
            draw.text((centre-6,row*115+3),str(index+1),fill='yellow')
            draw.line((centre,row*115+17,centre,row*115+23),fill='red')
        strip=atlas.crop((0,row*112,96*len(centres),(row+1)*112))
        path=folder/f'{direction}-sheet.png';strip.save(path);entries.append(path)
    atlas.save(ROOT/'UserHeroIdleFull.png')
    audit.save(folder/'source-frame-audit.png');entries.append(folder/'source-frame-audit.png')
    report['total_frames']=len(report['frames'])
    # Read back every delivered file and compare with the corresponding atlas cell.
    for f in report['frames']:
        image=Image.open(ROOT/f['path'])
        row=[r[0] for r in ROWS].index(f['direction']);col=f['index']-1
        assert image.size==(96,112) and image.mode=='RGBA'
        assert image.tobytes()==atlas.crop((col*96,row*112,(col+1)*96,(row+1)*112)).tobytes()
        assert image.getchannel('A').getextrema()==(0,255)
        box=image.getbbox();assert box and box[0]>0 and box[2]<96 and box[3]==104
    report['checks']=['Every visible head has exactly one numbered output.','All output PNGs read back and match atlas pixels.','All frames have transparent padding and the same foot baseline.']
    reportpath=folder/'crop-report.json';reportpath.write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8');entries.append(reportpath)
    with zipfile.ZipFile(ROOT/'UserHeroIdleFullCrops.zip','w',zipfile.ZIP_DEFLATED) as archive:
        archive.write(ROOT/'UserHeroIdleFull.png','UserHeroIdleFull.png')
        for path in entries:archive.write(path,path.relative_to(folder).as_posix())
    with zipfile.ZipFile(ROOT/'UserHeroIdleFullCrops.zip') as archive:assert archive.testzip() is None
    print(json.dumps(dict(counts=report['actual_counts'],total=report['total_frames']),ensure_ascii=False))

if __name__=='__main__':export()
