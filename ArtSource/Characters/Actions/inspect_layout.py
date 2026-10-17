"""Inspect generated PNGs and write slicing metadata; never modify source pixels.

Requires Pillow and NumPy. Coordinates in import-layout.json are top-left pixels;
Unity converts them on import. Review the resulting animation preview before use.
"""
import base64
import hashlib
import json
import re
from pathlib import Path

import numpy as np
from PIL import Image


ROOT = Path(__file__).resolve().parent
PROJECT = ROOT.parents[2]
SPECS = [
    ("HeroBasic-v1", "Hero", 4, 3, 1.735875),
    ("HeroExtra-v1", "Hero", 4, 4, 1.735875),
    ("GoblinMelee-v2", "GoblinMelee", 4, 4, 1.3279),
    ("GoblinRunner-v2", "GoblinRunner", 4, 4, 1.3279),
    ("GoblinCaster-v1", "GoblinCaster", 4, 4, 1.3279),
    ("Weapons-v1", "Weapons", 3, 1, 1.0),
]


def separators(counts, divisions, radius_fraction=0.14):
    """Find transparent gutters close to expected equal-grid boundaries."""
    length = len(counts)
    result = [0]
    for index in range(1, divisions):
        expected = length * index / divisions
        radius = length / divisions * radius_fraction
        low, high = int(expected - radius), int(expected + radius)
        candidates = np.flatnonzero(counts[low:high] == counts[low:high].min()) + low
        # Prefer center of widest run of equally empty rows/columns.
        runs = np.split(candidates, np.flatnonzero(np.diff(candidates) > 1) + 1)
        run = max(runs, key=lambda values: (len(values), -abs(values.mean() - expected)))
        result.append(int(round(float(run.mean()))))
    return result + [length]


def inspect_sheet(name, actor, columns, rows, world_height):
    path = ROOT / (name + ".png")
    with Image.open(path) as image:
        if image.mode != "RGBA":
            raise ValueError(f"Missing RGBA: {path}")
        pixels = np.array(image)
    alpha = pixels[:, :, 3]
    mask = alpha >= 128
    xs = separators(mask.sum(axis=0), columns)
    ys = separators(mask.sum(axis=1), rows)
    frames, warnings, row_xs = [], [], []
    for row in range(rows):
        row_columns = separators(mask[ys[row]:ys[row+1]].sum(axis=0), columns, 0.23)
        row_xs.append(row_columns)
        for col in range(columns):
            x0, x1 = row_columns[col:col + 2]
            y0, y1 = ys[row:row + 2]
            yy, xx = np.where(mask[y0:y1, x0:x1])
            if len(xx) == 0:
                raise ValueError(f"Empty frame {name}/{row}/{col}")
            left, right = int(xx.min()), int(xx.max())
            top, bottom = int(yy.min()), int(yy.max())
            if left == 0 or top == 0 or right == x1-x0-1 or bottom == y1-y0-1:
                warnings.append(f"Frame {row*columns+col}: content touches inferred cell boundary")
            # Keep two pixels of original transparent padding; never alter alpha.
            cx0, cy0 = max(x0, x0+left-2), max(y0, y0+top-2)
            cx1, cy1 = min(x1, x0+right+3), min(y1, y0+bottom+3)
            # Center on the head instead of trailing scarf or bandana for upright poses.
            upper = mask[y0+top:y0+top+max(1, int((bottom-top+1)*0.42)), x0:x1]
            head_x = np.where(upper)[1]
            anchor_x = x0 + float(np.median(head_x))
            if row == rows-1 and name != "HeroBasic-v1" and col >= 2:
                anchor_x = x0 + (left+right)/2  # horizontal fall/corpse
            if name == "HeroExtra-v1" and row == 1:
                anchor_x = x0 + (left+right)/2  # rotating roll silhouette
            if actor == "Weapons":
                anchor_x = x0 + left + (right-left)*0.13
            width, height = cx1-cx0, cy1-cy0
            pivot_y = (cy1-(y0+bottom+1))/height
            if actor == "Weapons":
                pivot_y = 0.5
            frames.append(dict(name=f"f{row*columns+col:02d}", x=cx0, y=cy0,
                               width=width, height=height,
                               pivotX=(anchor_x-cx0)/width, pivotY=pivot_y,
                               contentHeight=bottom-top+1))
    ppu = float(np.median([f['contentHeight'] for f in frames[:columns]]))/world_height
    if actor == "Weapons":
        ppu = float(frames[0]['width'])  # unit length; actual reach set at integration
    sha = hashlib.sha256(path.read_bytes()).hexdigest()
    return dict(name=name, source=path.relative_to(PROJECT).as_posix(),
                pixelsPerUnit=ppu, frames=frames, actor=actor,
                columns=columns, rows=rows, xSeparators=xs, rowXSeparators=row_xs, ySeparators=ys,
                sha256=sha, warnings=warnings,
                transparentFraction=float((alpha == 0).mean()),
                alphaPolicy="Original source alpha preserved; >=128 used for bounds only")


def clip(actor, name, source, sheet, indices, phases=None):
    if phases is None:
        phases = [i/len(indices) for i in range(len(indices))]
    return dict(actor=actor, name=name, source=source, sheet=sheet,
                frames=[f"f{i:02d}" for i in indices], phases=phases)


def main():
    sheets = [inspect_sheet(*spec) for spec in SPECS]
    player = "Assets/Animations/Player/"
    enemy = "Assets/Animations/Enemy/Gobling/"
    clips = []
    for name, sheet, indices in [
        ("Idle", "HeroBasic-v1", [0, 1, 2, 3]),
        ("Walk", "HeroBasic-v1", [4, 5, 6, 7]),
        ("Attack", "HeroBasic-v1", [8, 9, 10, 11]),
        ("Run", "HeroExtra-v1", [0, 1, 2, 3]),
        ("Roll", "HeroExtra-v1", [4, 5, 6, 7]),
        ("Hurt", "HeroExtra-v1", [8, 9, 10, 11]),
        ("Death", "HeroExtra-v1", [12, 13, 14, 15]),
    ]:
        phases = [0, .4, .5, .8] if name == "Attack" else None
        clips.append(clip("Hero", name, player+name+".anim", sheet, indices, phases))
    for actor, sheet, action, move in [
        ("GoblinMelee", "GoblinMelee-v2", "Attack", "Walk"),
        ("GoblinRunner", "GoblinRunner-v2", "AxeAttack", "Run"),
        ("GoblinCaster", "GoblinCaster-v1", "RangedShoot", "Walk"),
    ]:
        for name, indices in [("Idle", [0, 1, 2, 3]), (move, [4, 5, 6, 7]),
                              (action, [8, 9, 10, 11]), ("Hurt", [12, 13]), ("Death", [14, 15])]:
            phases = None
            if name == "Attack": phases = [0, .4, .5, .8]
            if name == "AxeAttack": phases = [0, .5, .6, .8]
            if name == "RangedShoot": phases = [0, .2, .46, .7]
            clips.append(clip(actor, name, enemy+name+".anim", sheet, indices, phases))
    for c in clips:
        text = (PROJECT/c['source']).read_text(encoding='utf-8-sig')
        c['duration'] = float(re.search(r'm_StopTime: ([^\n]+)', text)[1])
        c['events'] = [dict(time=float(t), function=n.strip()) for t, n in re.findall(
            r'- time: ([^\n]+)\n    functionName: ([^\n]+)', text)]
    result = dict(version=1, status="candidate-import-layout-not-live-integration",
                  sheets=sheets, clips=clips)
    (ROOT/'import-layout.json').write_text(json.dumps(result, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    # Standalone preview uses original PNG data and crop rectangles; it edits no image pixels.
    images = {s['name']: 'data:image/png;base64,'+base64.b64encode((PROJECT/s['source']).read_bytes()).decode() for s in sheets}
    template = (ROOT/'preview-template.html').read_text(encoding='utf-8')
    (ROOT/'preview.html').write_text(template.replace('__LAYOUT__', json.dumps(result)).replace('__IMAGES__', json.dumps(images)), encoding='utf-8')
    print(json.dumps(dict(sheets=len(sheets), frames=sum(len(s['frames']) for s in sheets), clips=len(clips),
                         warnings={s['name']: s['warnings'] for s in sheets if s['warnings']}), ensure_ascii=False))


if __name__ == '__main__':
    main()
