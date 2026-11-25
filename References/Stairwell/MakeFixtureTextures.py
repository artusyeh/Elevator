from pathlib import Path
import random,math
from PIL import Image,ImageDraw
root=Path('F:/桌面文件/Elevator')
tex=root/'Assets/Building/Textures/Residential Fluorescent';tex.mkdir(parents=True,exist_ok=True)
for name in ['Housing','Diffuser']:
    r=random.Random(37 if name=='Housing' else 91);w,h=1024,256;im=Image.new('RGB',(w,h));pixels=[]
    for y in range(h):
        for x in range(w):
            edge=min(x,w-x-1,y,h-y-1);dust=18*math.exp(-edge/15)+r.uniform(-1.3,1.3)+2*math.sin(x*.025)
            base=(166,165,152) if name=='Housing' else (172,169,148)
            pixels.append(tuple(int(c-dust) for c in base))
    im.putdata(pixels);d=ImageDraw.Draw(im)
    for i in range(90):
        x=r.randrange(w);y=r.choice([r.randrange(20),h-r.randrange(1,20)]);rad=r.randrange(1,4);d.ellipse((x-rad,y-rad,x+rad,y+rad),fill=(103,96,75))
    im.save(tex/(name+'.png'))

