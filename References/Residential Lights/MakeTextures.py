from pathlib import Path
import math,random
from PIL import Image,ImageDraw
folder=Path('F:/桌面文件/Elevator/Assets/Building/Models/Corridor Fluorescent')
r=random.Random(42);w,h=1024,256
for name in ['Cover Color','Cover Emission','Housing Color']:
    im=Image.new('RGB',(w,h));px=[]
    for y in range(h):
        for x in range(w):
            edge=min(y,h-1-y)/h;end=min(x,w-1-x)/w
            dirt=math.exp(-edge/.07)*14+math.exp(-end/.035)*22+max(0,math.sin(x*.017)-.3)*3+r.uniform(-1.8,1.8)
            if name=='Cover Color':c=(174-dirt,173-dirt,157-dirt)
            elif name=='Cover Emission':c=(177-dirt*1.5,180-dirt*1.5,170-dirt*1.5)
            else:c=(159-dirt,160-dirt,150-dirt)
            px.append(tuple(max(0,int(v)) for v in c))
    im.putdata(px);d=ImageDraw.Draw(im)
    if name!='Cover Emission':
        for i in range(80):
            x=r.randrange(w);y=r.choice([r.randrange(15),h-1-r.randrange(15)]);d.ellipse((x,y,x+r.randrange(1,5),y+r.randrange(1,3)),fill=(95,91,77))
    im.save(folder/(name+'.png'))
print('Created editable residential ceiling fixture, aged opal cover and restrained emission maps.')

