"""Original synthesized sound assets; no samples or external recordings."""
from pathlib import Path
import math,wave,struct,random
ROOT=Path(__file__).resolve().parents[1]/'Assets/SoftStrange/Resources/Audio'
RATE=24000

def save(name,duration,fn):
    frames=bytearray()
    for i in range(int(duration*RATE)):
        t=i/RATE
        for channel in range(2):frames.extend(struct.pack('<h',int(max(-1,min(1,fn(t,channel)))*32767)))
    with wave.open(str(ROOT/(name+'.wav')),'wb') as f:f.setnchannels(2);f.setsampwidth(2);f.setframerate(RATE);f.writeframes(frames)

def note(t,f,length):
    return math.sin(math.tau*f*t)*min(1,t/.012)*math.exp(-t*7/length) if t>=0 else 0
save('pinch',.16,lambda t,c:.20*note(t,740,.20)+.07*note(t,1110,.13))
save('open',.42,lambda t,c:.15*note(t,440,.35)+.12*note(t-.09,660,.30))
save('settle',.65,lambda t,c:.17*note(t,392,.50)+.12*note(t-.10,494,.45)+.09*note(t-.20,588,.40))
# Integer-frequency partials and periodic envelopes make a seamless 24-second loop.
save('quiet-tide',24,lambda t,c:sum(.035*math.sin(math.tau*f*t+c*.18)*(1+.3*math.sin(math.tau*t/24+i)) for i,f in enumerate([110,165,220,330]))+.012*math.sin(math.tau*440*t)*(.5+.5*math.cos(math.tau*t/12)))
print('Created four original stereo WAV files.')
