"""Create the CC0 Karate Studios game loop from an unchanged RPG Overworld FLAC.
Usage: python prepare_overworld_music.py path/to/RPGOverworld.flac
Requires numpy and soundfile; source URL/hash are retained in manifest.json.
"""
from pathlib import Path
import sys
import hashlib
import json
import numpy as np
import soundfile as sf

source = Path(sys.argv[1])
raw, rate = sf.read(source, always_2d=True)
audible = np.flatnonzero(np.max(abs(raw), axis=1) > .001)
start = max(0, int(audible[0]) - int(.004 * rate))
end = min(len(raw), int(audible[-1]) + int(.025 * rate))
data = raw[start:end].copy()
data -= data.mean(axis=0)
# Rotate a short equal-gain end/start overlap to the loop end, eliminating the silent tail.
n = round(.12 * rate)
fade = np.linspace(0, 1, n)[:, None]
seam = data[-n:] * (1 - fade) + data[:n] * fade
data = np.concatenate([data[n:-n], seam])
rms = float(np.sqrt(np.mean(data * data)))
gain = min(.08 / max(rms, 1e-9), .60 / max(float(abs(data).max()), 1e-9))
data *= gain
target = Path(__file__).resolve().parent / 'KarateStudios' / 'RPGOverworld.wav'
target.parent.mkdir(exist_ok=True)
sf.write(target, data, rate, subtype='PCM_16')
decoded, actual_rate = sf.read(target, always_2d=True)
report = {'source_sha256': hashlib.sha256(source.read_bytes()).hexdigest(),
          'source_seconds': len(raw) / rate, 'trim_start': start / rate, 'trim_end': end / rate,
          'seam_overlap_seconds': .12, 'content_gain': gain,
          'seconds': len(decoded) / actual_rate, 'sample_rate': actual_rate,
          'channels': decoded.shape[1], 'peak': float(abs(decoded).max()),
          'rms': float(np.sqrt(np.mean(decoded * decoded))),
          'sha256': hashlib.sha256(target.read_bytes()).hexdigest()}
target.with_suffix('.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
print(json.dumps(report, indent=2))
