"""Build a portable animation inspection page; never modify source images.

Usage: python build_review.py manifest.json --out review-directory
Requires Pillow. All paths in the manifest are relative to its directory.
"""
import argparse
import base64
import hashlib
import html
import io
import json
import math
from pathlib import Path

from PIL import Image, ImageDraw


def build(manifest_path, out):
    manifest_path = Path(manifest_path).resolve()
    spec = json.loads(manifest_path.read_text(encoding='utf-8-sig'))
    frames = spec.get('frames', [])
    if not frames:
        raise ValueError('frames must contain at least one frame')
    rendered, details, payload, warnings = [], [], [], []
    hashes = {}
    for i, frame in enumerate(frames):
        path = (manifest_path.parent / frame['file']).resolve()
        with Image.open(path) as source:
            im = source.convert('RGBA')
        if 'rect' in frame:
            r = frame['rect']
            if len(r) != 4 or any(type(v) is not int for v in r):
                raise ValueError(f'frame {i+1}: rect requires four integers')
            x, y, w, h = r
            if min(x, y) < 0 or min(w, h) <= 0 or x+w > im.width or y+h > im.height:
                raise ValueError(f'frame {i+1}: rectangle outside source bounds')
            im = im.crop((x, y, x+w, y+h))
        duration = frame.get('duration_ms', 100)
        if not isinstance(duration, (int, float)) or not math.isfinite(duration) or duration <= 0:
            raise ValueError(f'frame {i+1}: duration_ms must be finite and positive')
        anchor = frame.get('anchor', [im.width/2, im.height])
        if len(anchor) != 2 or any(not isinstance(v, (int, float)) or not math.isfinite(v) for v in anchor):
            raise ValueError(f'frame {i+1}: invalid anchor')
        if not 0 <= anchor[0] <= im.width or not 0 <= anchor[1] <= im.height:
            raise ValueError(f'frame {i+1}: anchor must be inside frame boundaries')
        if 'anchor' not in frame:
            warnings.append(f'frame {i+1}: assumed bottom-centre anchor; review manually')
        alpha = im.getchannel('A')
        bbox = alpha.getbbox()
        if bbox is None:
            warnings.append(f'frame {i+1}: fully transparent')
        if alpha.getextrema()[0] == 255:
            warnings.append(f'frame {i+1}: fully opaque, verify intended background')
        solid = alpha.point(lambda v: 255 if v >= 32 else 0).getbbox()
        if solid and (solid[0] == 0 or solid[1] == 0 or solid[2] == im.width or solid[3] == im.height):
            warnings.append(f'frame {i+1}: visible content touches edge; inspect clipping')
        digest = hashlib.sha256(str(im.size).encode()+im.tobytes()).hexdigest()
        if digest in hashes:
            warnings.append(f'frame {i+1}: pixels equal frame {hashes[digest]+1}; may be intentional hold')
        hashes[digest] = i
        label = str(frame.get('label', f'Frame {i+1}'))
        details.append(dict(index=i+1, label=label, size=list(im.size), anchor=anchor,
                            duration_ms=duration, alpha_bbox=bbox, pixel_sha256=digest))
        buf = io.BytesIO()
        im.save(buf, format='PNG')
        payload.append(dict(label=label, duration=duration, anchor=anchor,
                            data='data:image/png;base64,'+base64.b64encode(buf.getvalue()).decode()))
        rendered.append(im)
    # Place every source on one fixed root, without rescaling or trimming pixels.
    left = math.ceil(max(f['anchor'][0] for f in details))+16
    top = math.ceil(max(f['anchor'][1] for f in details))+16
    width = left+math.ceil(max(im.width-f['anchor'][0] for im, f in zip(rendered, details)))+16
    height = top+math.ceil(max(im.height-f['anchor'][1] for im, f in zip(rendered, details)))+16
    cols = min(len(frames), 6)
    sheet = Image.new('RGB', (width*cols, (height+30)*math.ceil(len(frames)/cols)), '#263540')
    draw = ImageDraw.Draw(sheet)
    for i, (im, f) in enumerate(zip(rendered, details)):
        x, y = (i % cols)*width, (i//cols)*(height+30)
        draw.line((x, y+top, x+width, y+top), fill='#70908c')
        sheet.paste(im, (round(x+left-f['anchor'][0]), round(y+top-f['anchor'][1])), im)
        draw.text((x+5, y+height+5), f'{i+1:02d} | {f["duration_ms"]} ms', fill='white')
    report = dict(status='technical-inspection-only', motion_review='pending', frame_count=len(frames),
                  duration_ms=sum(f['duration_ms'] for f in details), loop=bool(spec.get('loop', True)),
                  root=[left, top], canvas=[width, height], warnings=warnings, frames=details)
    data = dict(title=str(spec.get('title', 'Animation review')), note=str(spec.get('note', '')),
                frames=payload, width=width, height=height, root=[left, top], loop=report['loop'])
    # Escape '<' so untrusted labels cannot terminate the JSON script element.
    encoded = json.dumps(data, ensure_ascii=False).replace('<', '\\u003c')
    page = PAGE.replace('__TITLE__', html.escape(data['title'])).replace('__DATA__', encoded)
    out = Path(out)
    targets = [out/'review.html', out/'contact-sheet.png', out/'technical-report.json']
    if any(p.exists() for p in targets):
        raise FileExistsError('Review outputs already exist; choose a new --out directory')
    if path in [p.resolve() for p in targets] or manifest_path in [p.resolve() for p in targets]:
        raise ValueError('Outputs must not overwrite input files')
    out.mkdir(parents=True, exist_ok=True)
    sheet.save(targets[1])
    targets[2].write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    targets[0].write_text(page, encoding='utf-8')
    print(json.dumps({k: report[k] for k in ['status', 'frame_count', 'duration_ms', 'motion_review']}))


PAGE = r'''<!doctype html><html lang="zh-CN"><meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1"><title>__TITLE__</title>
<style>
:root{color-scheme:dark;font-family:system-ui,sans-serif}body{margin:24px;background:#111b23;color:#e9f0f4;max-width:1100px}h1{font-size:24px}p{color:#bacbd5;line-height:1.7}.stage{padding:20px;border:1px solid #37515e;border-radius:12px;display:inline-block;background:#263540}canvas{image-rendering:pixelated;max-width:100%;height:auto}button,select{padding:9px;margin:4px;border:1px solid #4b6a7b;background:#203746;color:white;border-radius:6px}label{display:inline-block;margin:10px}input[type=range]{width:min(600px,90vw)}#info{font-variant-numeric:tabular-nums}small{color:#e8c58c}
</style>
<h1 id="title"></h1><p id="note"></p><small>候选动作审查页 · 技术检查不代表动作验收。原图像素保持不变，根锚点以清单标注为准。</small>
<p><select id="speed"><option value="0.25">0.25×</option><option value="0.5">0.5×</option><option value="1" selected>1×</option><option value="2">2×</option></select>
<button id="play">播放</button><button id="prev">上一帧</button><button id="next">下一帧</button><button id="restart">回到开头</button>
<label><input id="loop" type="checkbox">循环</label><label><input id="onion" type="checkbox">前帧洋葱皮</label><label><input id="silhouette" type="checkbox">剪影</label><label><input id="guides" type="checkbox" checked>地面与根锚点</label>
<label>背景<select id="background"><option value="#263540">深灰</option><option value="#ddd9d0">浅灰</option><option value="#6b7277">中灰</option></select></label></p>
<div class="stage"><canvas id="canvas"></canvas></div><p id="info"></p><input id="scrub" aria-label="帧位置" type="range" min="0" step="1"><p>正常速度看重量与节奏；慢放定位结构和轨迹；逐帧看握点与遮挡。单次动作关闭循环并检查末帧停留。</p>
<script id="data" type="application/json">__DATA__</script><script>
const d=JSON.parse(document.getElementById('data').textContent),$=id=>document.getElementById(id);
$('title').textContent=d.title;$('note').textContent=d.note;$('loop').checked=d.loop;
const c=$('canvas'),ctx=c.getContext('2d');c.width=d.width;c.height=d.height;c.style.width=Math.min(d.width*2,850)+'px';
let images=[],index=0,playing=false,elapsed=0,last=null;const starts=[];let total=0;d.frames.forEach(f=>{starts.push(total);total+=f.duration});
function pose(i,opacity){const f=d.frames[i];ctx.save();ctx.globalAlpha=opacity;ctx.drawImage(images[i],d.root[0]-f.anchor[0],d.root[1]-f.anchor[1]);ctx.restore()}
function draw(){if(images.length!==d.frames.length)return;ctx.clearRect(0,0,c.width,c.height);if($('onion').checked&&(index>0||$('loop').checked))pose((index+images.length-1)%images.length,.23);pose(index,1);if($('silhouette').checked){ctx.globalCompositeOperation='source-in';ctx.fillStyle='#080b10';ctx.fillRect(0,0,c.width,c.height);ctx.globalCompositeOperation='source-over'}if($('guides').checked){ctx.strokeStyle='#64c7b6';ctx.lineWidth=1;ctx.beginPath();ctx.moveTo(0,d.root[1]+.5);ctx.lineTo(c.width,d.root[1]+.5);ctx.moveTo(d.root[0]-.5,d.root[1]-6);ctx.lineTo(d.root[0]-.5,d.root[1]+6);ctx.stroke()}ctx.globalCompositeOperation='destination-over';ctx.fillStyle=$('background').value;ctx.fillRect(0,0,c.width,c.height);ctx.globalCompositeOperation='source-over';$('scrub').value=index;$('info').textContent=`${index+1} / ${images.length} · ${d.frames[index].label} · ${d.frames[index].duration} ms · 起点 ${starts[index]} ms / 总长 ${total} ms`;}
function pause(){playing=false;$('play').textContent='播放'}
$('play').onclick=()=>{if(index===images.length-1&&!$('loop').checked&&!playing){index=0;elapsed=0}playing=!playing;$('play').textContent=playing?'暂停':'播放';last=null;draw()};
function step(n){pause();index=Math.max(0,Math.min(images.length-1,index+n));elapsed=0;draw()}
$('prev').onclick=()=>step(-1);$('next').onclick=()=>step(1);$('restart').onclick=()=>{pause();index=0;elapsed=0;draw()};
$('scrub').max=d.frames.length-1;$('scrub').value=0;$('scrub').oninput=()=>{pause();index=Number($('scrub').value);elapsed=0;draw()};
['onion','silhouette','guides','background','loop'].forEach(id=>$(id).onchange=draw);
function tick(t){if(last===null)last=t;const dt=t-last;last=t;if(playing){elapsed+=Math.min(dt,200)*Number($('speed').value);while(elapsed>=d.frames[index].duration){elapsed-=d.frames[index].duration;if(index+1<images.length)index++;else if($('loop').checked)index=0;else{pause();elapsed=0;break}}draw()}requestAnimationFrame(tick)}
Promise.all(d.frames.map(f=>new Promise((resolve,reject)=>{const im=new Image;im.onload=()=>resolve(im);im.onerror=reject;im.src=f.data}))).then(all=>{images=all;draw();window.animationReview={data:d,ready:true,get index(){return index}};requestAnimationFrame(tick)}).catch(()=>{$('info').textContent='帧加载失败'});
</script></html>'''


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('manifest', type=Path)
    parser.add_argument('--out', type=Path, required=True)
    args = parser.parse_args()
    try:
        build(args.manifest, args.out)
    except (ValueError, KeyError, OSError) as exc:
        parser.exit(2, f'Animation review failed: {exc}\n')
