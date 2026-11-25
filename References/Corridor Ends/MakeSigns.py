from pathlib import Path
import math,random
from PIL import Image,ImageDraw,ImageFont,ImageFilter
root=Path('F:/桌面文件/Elevator');out=root/'Assets/Building/Textures/Floor Signs';out.mkdir(parents=True,exist_ok=True)
for floor in range(1,7):
    r=random.Random(134+floor);w,h=768,576
    clouds=Image.new('L',(24,18));clouds.putdata([r.randrange(90,160) for _ in range(432)]);clouds=clouds.resize((w,h),Image.Resampling.BICUBIC)
    im=Image.new('RGBA',(w,h));pixels=[]
    for y in range(h):
        for x in range(w):
            edge=min(x,w-x-1,y,h-y-1);shade=(clouds.getpixel((x,y))-128)*.17+r.uniform(-1,1)-13*math.exp(-edge/18)
            pixels.append((int(182+shade),int(177+shade),int(153+shade),255))
    im.putdata(pixels);d=ImageDraw.Draw(im)
    d.rounded_rectangle((14,14,w-15,h-15),radius=22,outline=(74,77,67,255),width=4)
    d.text((w/2,h*.48),str(floor)+'F',font=ImageFont.truetype('C:/Windows/Fonts/arialbd.ttf',282),anchor='mm',fill=(78,49,39,255))
    # Chips occur around fixings/edges, with fine worn paint scratches across the enamel.
    for i in range(160):
        x=r.randrange(w);y=r.randrange(h)
        if min(x,w-x-1,y,h-y-1)>30:
            if r.random()>.07:continue
        rad=r.randrange(1,8);d.ellipse((x-rad,y-rad,x+rad*2,y+rad),fill=(91+r.randrange(20),72+r.randrange(15),47,255))
    for i in range(30):
        x=r.randrange(35,w-55);y=r.randrange(25,h-25);d.line((x,y,x+r.randrange(4,38),y+r.randrange(-2,3)),fill=(155,150,129,190),width=1)
    alpha=Image.new('L',(w,h));ImageDraw.Draw(alpha).rounded_rectangle((1,1,w-2,h-2),radius=12,fill=255);im.putalpha(alpha);im.save(out/(str(floor)+'F.png'))

# Dirt is concentrated at seals and in sparse rain streaks; most glass remains transparent.
w,h=1024,768;r=random.Random(31);im=Image.new('RGBA',(w,h));pixels=[]
for y in range(h):
    for x in range(w):
        edge=min(x/w,(w-1-x)/w,y/h,(h-1-y)/h)
        deposit=math.exp(-edge/0.035)
        streak=max(0,math.sin(x*.021+math.sin(y*.008)*.20)-.60)*.16
        a=int(14+deposit*90+streak*100+r.uniform(0,5))
        pixels.append((164,170,160,a))
im.putdata(pixels);d=ImageDraw.Draw(im)
for i in range(45):
    x=r.randrange(w);y=r.choice([r.randrange(70),h-r.randrange(1,110)]);rx=r.randrange(2,15);ry=r.randrange(2,20);d.ellipse((x-rx,y-ry,x+rx,y+ry),fill=(95,102,87,r.randrange(45,105)))
im.save(out/'Window Deposits.png')
print('Created five aged enamel floor signs and a transparent window-deposit texture.')
