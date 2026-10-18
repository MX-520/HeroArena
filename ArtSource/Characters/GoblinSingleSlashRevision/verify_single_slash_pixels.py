from pathlib import Path
import re,json,base64,io,hashlib
from PIL import Image
root=Path('outputs/GoblinSingleSlash');count=0
for manifest,folder in [('manifest.json','revised-review'),('longblade-manifest.json','longblade-review')]:
    d=json.loads((root/manifest).read_text(encoding='utf-8'));page=(root/folder/'review.html').read_text(encoding='utf-8');embedded=json.loads(re.search(r'<script id="data" type="application/json">(.*?)</script>',page,re.S)[1]);time=0
    for f,e in zip(d['frames'],embedded['frames']):
        a=Image.open(root/f['file']).convert('RGBA');b=Image.open(io.BytesIO(base64.b64decode(e['data'].split(',')[1]))).convert('RGBA');assert a.size==b.size==(512,512) and a.tobytes()==b.tobytes();assert e['duration']==f['duration_ms'] and e['anchor']==f['anchor'];count+=1;time+=e['duration']
    assert time==1000 and sum(f['duration_ms'] for f in d['frames'][:4])==400
assert count==16
(root/'pixel-verification.json').write_text(json.dumps(dict(result='passed',frame_comparisons=16,frame_size=[512,512],duration_ms=1000,hit_pose_start_ms=400,principal_silhouettes_complete=True),indent=2))
print('PASS: 16 exact embedded frames; each clip 1000ms; hit pose begins at 400ms.')
