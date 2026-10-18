from pathlib import Path
import base64,hashlib,io,json,re
from PIL import Image
root=Path('outputs/FourCharacterTrial');m=json.loads((root/'trial-manifest.json').read_text(encoding='utf-8'));count=0
for source in m['sources']:
    assert hashlib.sha256((root/'sources'/(source['role']+'.png')).read_bytes()).hexdigest()==source['sha256']
for clip in m['clips']:
    path=root/clip['manifest'];spec=json.loads(path.read_text(encoding='utf-8'))
    page=(root/clip['path']).read_text(encoding='utf-8')
    data=json.loads(re.search(r'<script id="data" type="application/json">(.*?)</script>',page,re.S)[1])
    assert len(data['frames'])==len(spec['frames'])==clip['count']
    for expected,actual in zip(spec['frames'],data['frames']):
        source=Image.open(path.parent/expected['file']).convert('RGBA');x,y,w,h=expected['rect']
        embedded=Image.open(io.BytesIO(base64.b64decode(actual['data'].split(',')[1]))).convert('RGBA')
        assert embedded.size==(w,h) and embedded.tobytes()==source.crop((x,y,x+w,y+h)).tobytes()
        assert expected['anchor']==actual['anchor'] and expected['duration_ms']==actual['duration']
        count+=1
assert count==88
(root/'pixel-verification.json').write_text(json.dumps(dict(result='passed',source_hashes=4,source_frames=71,editorial_frames=17,exact_frame_comparisons=count,anchors_and_durations=True),indent=2))
print('PASS: four source hashes; 88/88 frame crops, anchors and durations match.')
