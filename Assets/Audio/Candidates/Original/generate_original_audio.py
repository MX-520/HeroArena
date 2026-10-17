from pathlib import Path
import math,random,wave,struct,json,hashlib
base=Path(__file__).resolve().parent.parent
manifest=[]
rate=44100
def synth(name,duration,event,kind):
 rng=random.Random(40617+kind);samples=[];phase=0;filtered=0
 for i in range(int(duration*rate)):
  t=i/rate;x=t/duration;noise=rng.uniform(-1,1);filtered=filtered*0.86+noise*0.14
  if kind==0:
   f=1200*math.exp(-x*2.4)+180;phase+=2*math.pi*f/rate
   value=(math.sin(phase)*0.65+noise*0.18)*math.exp(-x*7)*min(1,t*900)
  elif kind==1:
   f=330+1200*x*x;phase+=2*math.pi*f/rate
   value=(math.sin(phase)+math.sin(phase*1.5)*0.25+filtered*0.4)*(math.sin(math.pi*x)**1.5)*0.55
  elif kind==2:
   f=180*math.exp(-x*5)+55;phase+=2*math.pi*f/rate
   value=(math.sin(phase)*0.6+filtered*1.5+noise*0.12)*math.exp(-x*6)*min(1,t*1500)
  else:
   notes={3:[783.99,987.77,1174.66],4:[523.25,659.25,783.99,1046.5],5:[659.25,523.25,440.0]}[kind]
   step=duration/(len(notes)+1);value=0
   for n,f in enumerate(notes):
    age=t-n*step
    if age>=0:
     value+=(math.sin(2*math.pi*f*age)+0.2*math.sin(2*math.pi*f*2*age))*math.exp(-age*10)*min(1,age*700)*0.5
  # Short fade at both ends, no clipping or clicks from a truncated envelope.
  value*=min(1,t/0.004,(duration-t)/0.018);samples.append(value)
 peak=max(abs(v) for v in samples);scale=0.65/max(peak,0.001)
 target=base/'Original'/name;target.parent.mkdir(exist_ok=True)
 with wave.open(str(target),'wb') as out:
  out.setnchannels(1);out.setsampwidth(2);out.setframerate(rate)
  out.writeframes(b''.join(struct.pack('<h',max(-32767,min(32767,int(v*scale*32767)))) for v in samples))
 manifest.append({'asset':str(target.relative_to(base)).replace('\\','/'),'event_candidate':event,'status':'candidate-not-wired','author':'HeroArena project procedural synthesis','license':'project-original-no-third-party-samples','source':'Assets/Audio/Candidates/Original/generate_original_audio.py','sha256':hashlib.sha256(target.read_bytes()).hexdigest(),'checked_date':'2026-10-17'})

for args in [('PlayerShot.wav',0.14,'玩家射击',0),('MagicCast.wav',0.28,'哥布林魔法发射',1),('MagicImpact.wav',0.38,'魔法弹命中',2),('HealPickup.wav',0.48,'回血/增益拾取',3),('Victory.wav',0.9,'挑战成功短音',4),('Defeat.wav',0.8,'挑战失败短音',5)]:synth(*args)

(base/'Original'/'synthesis_manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
