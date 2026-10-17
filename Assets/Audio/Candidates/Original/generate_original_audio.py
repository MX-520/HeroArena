from pathlib import Path
import math,random,wave,struct,json,hashlib
base=Path(__file__).resolve().parent.parent
manifest=[]
rate=44100
def synth(name,duration,event,kind):
 rng=random.Random(40617+kind);samples=[];phase=0;filtered=0;air=0;low=0
 for i in range(int(duration*rate)):
  t=i/rate;x=t/duration;noise=rng.uniform(-1,1);filtered=filtered*0.86+noise*0.14
  if kind==0:
   f=1200*math.exp(-x*2.4)+180;phase+=2*math.pi*f/rate
   value=(math.sin(phase)*0.65+noise*0.18)*math.exp(-x*7)*min(1,t*900)
  elif kind in (1,2,6,7,8):
   # Independent broad-band air, mid-band crack and low body layers.
   # No long sine pitch-drop: that produces the old water/plop character.
   low=low*0.96+noise*0.04;air=air*0.48+noise*0.52
   band=air-low;edge=noise-air
   if kind in (6,7):
    # Compact weapon displacement: rising air sweep into a dry, low transient.
    at=0.061 if kind==6 else 0.052
    sweep=math.exp(-((t-at)/0.029)**2)
    age=max(0,t-at)
    body=(math.sin(2*math.pi*132*age)+.28*math.sin(2*math.pi*287*age))*math.exp(-age*61) if t>=at else 0
    snap=band*math.exp(-age*120) if t>=at else 0
    value=band*.95*sweep+low*2.4*sweep+body*.58+snap*.5
   elif kind==8:
    # Actual contact: hard crack plus damped wood/weapon resonances.
    attack=min(1,t/.0015)
    body=math.sin(2*math.pi*104*t)*math.exp(-t*34)
    resonances=sum(math.sin(2*math.pi*f*t)*math.exp(-t*d)*g for f,d,g in [(211,48,.32),(389,61,.21),(923,95,.12)])
    value=(body*.82+resonances+band*1.6*math.exp(-t*105)+edge*.24*math.exp(-t*170))*attack
   elif kind==1:
    # Arcane launch: pressure pulse, granular electrical cracks, short air burst.
    attack=min(1,t/.002)
    body=(math.sin(2*math.pi*91*t)+.28*math.sin(2*math.pi*183*t))*math.exp(-t*31)
    crack=band*(.35+.65*max(0,math.sin(2*math.pi*117*t))**6)*math.exp(-t*29)
    air_burst=low*3.2*math.exp(-((t-.028)/.024)**2)
    value=(body*.75+crack*1.7+air_burst+edge*.16*math.exp(-t*85))*attack
   else:
    # Arcane impact: front-loaded bass and inharmonic, noise-driven electrical breakup.
    attack=min(1,t/.0015)
    body=(math.sin(2*math.pi*76*t)+.36*math.sin(2*math.pi*139*t))*math.exp(-t*26)
    fracture=sum(math.sin(2*math.pi*f*t)*math.exp(-t*d)*g for f,d,g in [(263,42,.22),(617,59,.13),(1381,83,.08)])
    crack=(band*1.9+edge*.3)*math.exp(-t*78)
    sparks=band*(max(0,math.sin(2*math.pi*83*t))**12)*math.exp(-t*25)*.7
    value=(body*.9+fracture+crack+sparks)*attack
   value=math.tanh(value*1.35) # Soft saturation adds dense harmonics without clipping.
  else:
   notes={3:[783.99,987.77,1174.66],4:[523.25,659.25,783.99,1046.5],5:[659.25,523.25,440.0]}[kind]
   step=duration/(len(notes)+1);value=0
   for n,f in enumerate(notes):
    age=t-n*step
    if age>=0:
     value+=(math.sin(2*math.pi*f*age)+0.2*math.sin(2*math.pi*f*2*age))*math.exp(-age*10)*min(1,age*700)*0.5
  # Short fade at both ends, no clipping or clicks from a truncated envelope.
  value*=min(1,t/0.004,(duration-t)/0.018);samples.append(value)
 peak=max(abs(v) for v in samples);scale=(0.8 if kind in (1,2,6,7,8) else 0.65)/max(peak,0.001)
 target=base/'Original'/name;target.parent.mkdir(exist_ok=True)
 with wave.open(str(target),'wb') as out:
  out.setnchannels(1);out.setsampwidth(2);out.setframerate(rate)
  out.writeframes(b''.join(struct.pack('<h',max(-32767,min(32767,int(v*scale*32767)))) for v in samples))
 manifest.append({'asset':str(target.relative_to(base)).replace('\\','/'),'event_candidate':event,'status':'generated-original','author':'HeroArena project procedural synthesis','license':'project-original-no-third-party-samples','source':'Assets/Audio/Candidates/Original/generate_original_audio.py','sha256':hashlib.sha256(target.read_bytes()).hexdigest(),'checked_date':'2026-10-17'})

for args in [('PlayerShot.wav',0.14,'玩家射击',0),('MagicCast.wav',0.23,'哥布林魔法发射',1),('MagicImpact.wav',0.32,'魔法弹命中',2),('HealPickup.wav',0.48,'回血/增益拾取',3),('Victory.wav',0.9,'挑战成功短音',4),('Defeat.wav',0.8,'挑战失败短音',5),('MeleeSwingA.wav',0.17,'重击挥动变体A',6),('MeleeSwingB.wav',0.16,'重击挥动变体B',7),('MeleeImpact.wav',0.20,'近战实际命中',8)]:synth(*args)

(base/'Original'/'synthesis_manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
