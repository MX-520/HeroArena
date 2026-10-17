"""Extract a user-supplied presentation board for an approximate idle trial.

This is deterministic processing, not image generation. The original board is
preserved. Its checkerboard is baked in, so results are trial assets only.
"""
from collections import deque
from pathlib import Path
import json
import hashlib
import shutil
import base64
import zipfile

import numpy as np
from PIL import Image, ImageFilter

ROOT = Path(__file__).resolve().parent
PROJECT = ROOT.parents[2]
SOURCE = PROJECT / "主角四方向像素待机动画展示板.png"
ROWS = [("Down", 428, 500), ("Left", 551, 629), ("Right", 684, 760), ("Up", 820, 904)]


def components(mask):
    visited = np.zeros(mask.shape, dtype=bool)
    height, width = mask.shape
    result = []
    for y, x in zip(*np.where(mask)):
        if visited[y, x]:
            continue
        q = deque([(int(y), int(x))])
        visited[y, x] = True
        points = []
        while q:
            cy, cx = q.popleft()
            points.append((cy, cx))
            for dy, dx in ((0, 1), (0, -1), (1, 0), (-1, 0), (1, 1), (1, -1), (-1, 1), (-1, -1)):
                ny, nx = cy+dy, cx+dx
                if 0 <= ny < height and 0 <= nx < width and mask[ny, nx] and not visited[ny, nx]:
                    visited[ny, nx] = True
                    q.append((ny, nx))
        if len(points) > 150:
            yy, xx = zip(*points)
            if max(yy)-min(yy) > 30:
                result.append(dict(x=min(xx), y=min(yy), width=max(xx)-min(xx)+1,
                                   height=max(yy)-min(yy)+1, area=len(points), points=points))
    return sorted(result, key=lambda c: c['x'])


def inspect():
    with Image.open(SOURCE) as image:
        rgba = np.array(image.convert('RGBA'))
    if rgba.shape[:2] != (1024, 1536):
        raise ValueError('This extraction layout is specific to the supplied 1536x1024 board.')
    result = {'source_size': [rgba.shape[1], rgba.shape[0]],
              'source_alpha_range': [int(rgba[:, :, 3].min()), int(rgba[:, :, 3].max())],
              'rows': []}
    for name, top, bottom in ROWS:
        rgb = rgba[top:bottom, 80:1529, :3].astype(int)
        mask = (rgb.min(axis=2) < 185) | ((rgb.max(axis=2)-rgb.min(axis=2)) > 35)
        objects = components(mask)
        result['rows'].append(dict(direction=name, top=top, bottom=bottom,
                                   detected_characters=len(objects), components=objects))
    return rgba, result


def fill_inside(mask):
    """Fill only holes enclosed by this character's connected outline."""
    height, width = mask.shape
    outside = np.zeros_like(mask)
    q = deque()
    for y in range(height):
        for x in (0, width-1):
            if not mask[y, x] and not outside[y, x]:
                outside[y, x] = True
                q.append((y, x))
    for x in range(width):
        for y in (0, height-1):
            if not mask[y, x] and not outside[y, x]:
                outside[y, x] = True
                q.append((y, x))
    while q:
        y, x = q.popleft()
        for dy, dx in ((0, 1), (0, -1), (1, 0), (-1, 0)):
            ny, nx = y+dy, x+dx
            if 0 <= ny < height and 0 <= nx < width and not mask[ny, nx] and not outside[ny, nx]:
                outside[ny, nx] = True
                q.append((ny, nx))
    return ~outside


def extract():
    rgba, result = inspect()
    shutil.copyfile(SOURCE, ROOT/'source-board.png')
    sheet = Image.new('RGBA', (96*8, 112*4))
    sheet_frames = []
    for row_index, row in enumerate(result['rows']):
        # Exclude merged/touching figures; select actual independent characters.
        candidates = [c for c in row['components'] if c['width'] < 70]
        if len(candidates) < 8:
            raise ValueError('Insufficient independent trial frames: '+row['direction'])
        selected = candidates[:8]
        row['selected_frames'] = []
        for index, obj in enumerate(selected):
            top = row['top']
            rgb = rgba[top:row['bottom'], 80:1529, :3]
            mask = np.zeros(rgb.shape[:2], dtype=bool)
            yy, xx = zip(*obj['points'])
            mask[np.array(yy), np.array(xx)] = True
            # White hair resembles the checkerboard. Close one-pixel breaks in
            # the dark outline before filling its enclosed highlights.
            closed = Image.fromarray(mask.astype(np.uint8)*255).filter(ImageFilter.MaxFilter(3)).filter(ImageFilter.MinFilter(3))
            mask = np.array(closed) > 0
            # Some presentation frames break the pale hair outline by several
            # pixels. Apply stronger closing to the head only; keep arm/leg gaps.
            head_closed = np.array(Image.fromarray(mask.astype(np.uint8)*255).filter(ImageFilter.MaxFilter(7)).filter(ImageFilter.MinFilter(7))) > 0
            head_end = obj['y'] + round(obj['height'] * 0.55)
            mask[:head_end] = head_closed[:head_end]
            mask = fill_inside(mask)
            y, x = np.where(mask)
            left, right, first, bottom = int(x.min()), int(x.max())+1, int(y.min()), int(y.max())+1
            cut = np.zeros((bottom-first, right-left, 4), dtype=np.uint8)
            cut[:,:,:3] = rgb[first:bottom, left:right]
            cut[:,:,3] = mask[first:bottom, left:right]*255
            upper_y, upper_x = np.where(mask[first:first+max(1, obj['height']//3)])
            anchor_x = float(np.median(upper_x))
            offset_x = round(48-(anchor_x-left))
            offset_y = 104-cut.shape[0]
            if offset_x < 0 or offset_y < 0 or offset_x+cut.shape[1] > 96 or offset_y+cut.shape[0] > 112:
                raise ValueError(f"Trial frame would be clipped: {row['direction']}/{index}")
            cell = Image.new('RGBA', (96,112))
            cell.paste(Image.fromarray(cut), (offset_x,offset_y))
            sheet.paste(cell, (index*96,row_index*112))
            frame = dict(name=f"{row['direction']}{index:02d}", x=index*96,y=row_index*112,
                         width=96,height=112,pivotX=.5,pivotY=8/112)
            sheet_frames.append(frame)
            row['selected_frames'].append(dict(index=index,source_rect=[80+left,top+first,right-left,bottom-first],
                                               anchor_x=anchor_x+80,source_height=cut.shape[0]))
        for obj in row['components']:
            obj.pop('points')
    sheet.save(ROOT/'UserHeroIdleTrial.png')
    # Export the same aligned pixels as ordinary PNG files for manual use.
    with zipfile.ZipFile(ROOT/'UserHeroIdleCrops.zip', 'w', zipfile.ZIP_DEFLATED) as archive:
        archive.write(ROOT/'UserHeroIdleTrial.png', 'UserHeroIdleTrial.png')
        for row_index, (direction, _, _) in enumerate(ROWS):
            folder = ROOT/'Cropped'/direction
            folder.mkdir(parents=True, exist_ok=True)
            strip = sheet.crop((0, row_index*112, 768, (row_index+1)*112))
            strip_path = ROOT/'Cropped'/f'{direction}-sheet.png'
            strip.save(strip_path)
            archive.write(strip_path, f'{direction}-sheet.png')
            for index in range(8):
                path = folder/f'{direction}_{index+1:02d}.png'
                strip.crop((index*96, 0, (index+1)*96, 112)).save(path)
                archive.write(path, f'{direction}/{path.name}')
    result.update(status='approximate-extraction-for-trial-only',
                  source_sha256=hashlib.sha256(SOURCE.read_bytes()).hexdigest(),
                  source_origin='User supplied ChatGPT presentation board; not raw transparent frames.',
                  output='UserHeroIdleTrial.png',frames_per_direction=8,
                  actual_cell=[96,112],original_pixels_resampled=False,
                  limitations=['Baked checkerboard removed using connected silhouettes, local outline closing and enclosed-hole fill; stronger closing limited to the head protects white hair.',
                               'Board contains inconsistent numbering/spacing; claimed 32x32/32 frames not used.',
                               'Selected frames are trial samples, not verified original animation keyframes.',
                               'Do not treat this as final alpha-quality or seamless idle-loop approval.'])
    (ROOT/'extraction-report.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    layout={'sheets':[dict(name='UserHeroIdleTrial',source='ArtSource/Characters/UserHeroTrial/UserHeroIdleTrial.png',
                          pixelsPerUnit=70/1.735875,frames=sheet_frames)],'clips':[]}
    for direction,_,_ in ROWS:
        layout['clips'].append(dict(actor='UserHeroTrial',name='Idle'+direction,
                                    source='Assets/Animations/Player/Idle.anim',sheet='UserHeroIdleTrial',
                                    frames=[f'{direction}{i:02d}' for i in range(8)],phases=[i/8 for i in range(8)]))
    (ROOT/'import-layout.json').write_text(json.dumps(layout,indent=2)+'\n',encoding='utf-8')
    template=(ROOT/'preview-template.html').read_text(encoding='utf-8')
    image='data:image/png;base64,'+base64.b64encode((ROOT/'UserHeroIdleTrial.png').read_bytes()).decode()
    (ROOT/'preview.html').write_text(template.replace('__SHEET__',image),encoding='utf-8')
    print(json.dumps({'directions':4,'trial_frames':32,'output':'UserHeroIdleTrial.png',
                      'source_alpha_range':result['source_alpha_range']},ensure_ascii=False))


if __name__ == '__main__':
    extract()
