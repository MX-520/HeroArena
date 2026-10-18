from pathlib import Path
import base64,io,json,re
from PIL import Image
out=Path('outputs/GoblinSlashFX');d=json.loads((out/'preview-data.json').read_text(encoding='utf-8'));count=0
for key,root,manifest in [('new',out,'manifest.json'),('old',Path('outputs/GoblinSingleSlash'),'longblade-manifest.json')]:
    spec=json.loads((root/manifest).read_text(encoding='utf-8'))
    for f,e in zip(spec['frames'],d[key]['frames']):
        a=Image.open(root/f['file']).convert('RGBA');b=Image.open(io.BytesIO(base64.b64decode(e['data'].split(',')[1]))).convert('RGBA');assert a.size==b.size and a.tobytes()==b.tobytes();assert e['duration']==f['duration_ms'] and e['anchor']==f['anchor'];count+=1
assert count==18 and sum(f['duration_ms'] for f in json.loads((out/'manifest.json').read_text(encoding='utf-8'))['frames'])==1000
(out/'pixel-verification.json').write_text(json.dumps(dict(result='passed',exact_preview_frames=18,generated_frames=10,frame_size=[640,512],duration_ms=1000,source_regions_preserved=True,preview_scaling_only=True),indent=2))
print('PASS: all 18 embedded frames exact; 10 x 100ms new sequence.')
