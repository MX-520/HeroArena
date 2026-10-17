"""Edit retained CC0 source recordings/effects into short game cues; no synthetic oscillators.

Requires numpy and soundfile. From project root, run with those modules on PYTHONPATH.
Source OGG files are preserved byte for byte. Output: mono 44.1 kHz PCM16, peak <= 0.72.
"""
from pathlib import Path
import hashlib
import json
import numpy as np
import soundfile as sf

ROOT = Path(__file__).resolve().parent
RATE = 44100


def source(author, name, speed=1.0):
    data, rate = sf.read(ROOT / author / 'Source' / name, always_2d=True)
    data = data.mean(axis=1)
    data -= data.mean()
    # Remove leading silence, retain 4 ms before the first audible transient.
    active = np.flatnonzero(abs(data) > abs(data).max() * .018)
    if len(active):
        data = data[max(0, int(active[0]) - int(rate * .004)):]
    data = np.interp(np.arange(0, len(data), rate / RATE * speed), np.arange(len(data)), data)
    return data / max(float(abs(data).max()), .001)


def cue(author, name, duration, layers, echo=False):
    out = np.zeros(round(duration * RATE))
    for data, gain, offset in layers:
        start = round(offset * RATE)
        count = min(len(data), len(out) - start)
        out[start:start + count] += data[:count] * gain
    if echo:
        dry = out.copy()
        for delay, gain in [(.027, .12), (.061, .055)]:
            n = round(delay * RATE)
            out[n:] += dry[:-n] * gain
    out -= out.mean()
    fade_in, fade_out = round(.0015 * RATE), round(.045 * RATE)
    out[:fade_in] *= np.linspace(0, 1, fade_in)
    out[-fade_out:] *= np.linspace(1, 0, fade_out)
    out *= .72 / max(float(abs(out).max()), .001)
    target = ROOT / author / (name + '.wav')
    sf.write(target, out, RATE, subtype='PCM_16')
    assert float(abs(out).max()) <= .721
    return {'asset': str(target.relative_to(ROOT.parent)).replace('\\', '/'),
            'seconds': duration, 'peak': .72,
            'sha256': hashlib.sha256(target.read_bytes()).hexdigest()}


if __name__ == '__main__':
    attack_a = source('StarNinjas', 'sword.4.ogg', .94)
    attack_b = source('StarNinjas', 'sword.10.ogg', 1.04)
    clash_a = source('StarNinjas', 'sword_clash.1.ogg', .92)
    clash_b = source('StarNinjas', 'sword_clash.10.ogg', .96)
    spell_a = source('Rubberduck', 'spell_01.ogg', .92)
    spell_b = source('Rubberduck', 'spell_02.ogg', 1.02)
    fire = source('Rubberduck', 'spell_fire_07.ogg', .82)
    report = [
        cue('StarNinjas', 'SteelSwingA', .36, [(attack_a, 1, 0)]),
        cue('StarNinjas', 'SteelSwingB', .44, [(attack_b, 1, 0)]),
        cue('StarNinjas', 'SteelImpactA', .40, [(clash_a, 1, 0)]),
        cue('StarNinjas', 'SteelImpactB', .40, [(clash_b, 1, 0)]),
        cue('Rubberduck', 'ArcaneCast', .44, [(spell_b, 1, 0), (fire, .22, .012)], True),
        cue('Rubberduck', 'ArcaneImpact', .56, [(spell_a, .9, 0), (fire, .55, 0)], True),
    ]
    (ROOT / 'combat-edits.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(report, indent=2))
