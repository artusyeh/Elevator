from pathlib import Path
import random, math, json
from PIL import Image, ImageDraw, ImageFont, ImageFilter

root=Path('F:/桌面文件/Elevator')
out=root/'Assets/Building/Textures/Electrical Labels'
out.mkdir(parents=True,exist_ok=True)
def paper(size,seed):
    w,h=size; r=random.Random(seed)
    cloud=Image.new('L',(22,12)); cloud.putdata([r.randrange(110,155) for _ in range(264)])
    cloud=cloud.resize(size,Image.Resampling.BICUBIC).filter(ImageFilter.GaussianBlur(7))
    px=[]
    for y in range(h):
        for x in range(w):
            edge=min(x,w-1-x,y,h-1-y)
            aged=27*math.exp(-edge/(h*.11))+r.uniform(-2,2)+(cloud.getpixel((x,y))-128)*.43
            px.append((int(192-aged),int(175-aged),int(134-aged)))
    im=Image.new('RGB',size); im.putdata(px)
    stains=Image.new('RGBA',size);d=ImageDraw.Draw(stains)
    for i in range(18):
        x=r.randrange(w);y=r.choice([r.randrange(max(1,h//6)),h-1-r.randrange(max(1,h//6))]);rx=r.uniform(.015,.10)*w;ry=r.uniform(.025,.20)*h
        points=[(x+math.cos(a*math.pi/5)*rx*r.uniform(.65,1.1),y+math.sin(a*math.pi/5)*ry*r.uniform(.65,1.1)) for a in range(10)]
        d.polygon(points,fill=(75,59,35,r.randrange(25,85)))
    im=Image.alpha_composite(im.convert('RGBA'),stains.filter(ImageFilter.GaussianBlur(max(1,h*.012))))
    return im

def finish(im,seed):
    w,h=im.size;r=random.Random(seed);mask=Image.new('L',(w,h));d=ImageDraw.Draw(mask)
    step=max(3,w//75)
    top=[(x,r.randrange(1,max(3,h//22))) for x in range(0,w,step)]
    bottom=[(x,h-1-r.randrange(1,max(3,h//20))) for x in range(w-1,-1,-step)]
    d.polygon(top+[(w-1,h//20)]+bottom+[(0,h-1)],fill=255)
    # Lost corners and small edge tears cut through to the real supporting surface.
    corner=r.choice([True,False]);cw=r.randint(max(3,w//35),max(5,w//10));ch=r.randint(max(3,h//8),max(5,h//3))
    if corner:d.polygon([(0,0),(cw,0),(cw*.7,ch*.25),(cw*.3,ch*.8),(0,ch)],fill=0)
    else:d.polygon([(w,0),(w-cw,0),(w-cw*.75,ch*.3),(w-cw*.2,ch*.8),(w,ch)],fill=0)
    for i in range(3):
        x=r.randrange(w);size=r.randint(max(2,h//40),max(3,h//12));y=0 if i%2==0 else h-1;inside=size if y==0 else h-1-size
        d.polygon([(x-size,y),(x+size,y),(x+r.randrange(-2,3),inside)],fill=0)
    dust=Image.new('RGBA',(w,h));dd=ImageDraw.Draw(dust)
    for i in range(max(50,w*h//550)):
        x=r.randrange(w);y=r.choice([r.randrange(max(1,h//5)),h-1-r.randrange(max(1,h//5))]);rad=r.choice([1,1,2,3])
        dd.ellipse((x,y,x+rad,y+rad),fill=(68,59,40,r.randrange(30,110)))
    # Restrained handling smears and paper fold marks, not uniform grime over the lettering.
    x=r.randint(w//5,4*w//5);dd.line([(x,2),(x+int(h*.10),h-3)],fill=(91,73,47,40),width=max(1,h//90))
    dd.line([(x+2,2),(x+int(h*.10)+2,h-3)],fill=(222,205,164,35),width=1)
    im=Image.alpha_composite(im,dust);im.putalpha(mask);return im

names=['Main Supply','Lobby','Security Room','Second Floor','Third Floor','Fourth Floor','Fifth Floor','Sixth Floor','Stairwell','Elevator','CCTV Cameras','CCTV Monitors','Entrance','Emergency','Maintenance']
for i,name in enumerate(names):
    im=paper((1024,260),81+i);d=ImageDraw.Draw(im);text=name.upper();font=ImageFont.truetype('C:/Windows/Fonts/cour.ttf',100)
    while d.textbbox((0,0),text,font=font)[2]>930:font=ImageFont.truetype('C:/Windows/Fonts/cour.ttf',font.size-2)
    d.text((512,126),text,font=font,anchor='mm',fill=(34,31,25,255))
    finish(im,81+i).save(out/(name+' Paper.png'))

for name,source in [('Electrical Warning','Assets/Building/Textures/Electrical Warning.png'),('Caretaker Notes','Assets/Apartment/Textures/Caretaker Notes.png'),('Electrical Room','Assets/Building/Textures/Electrical Room Plate.png')]:
    original=Image.open(root/source).convert('RGB'); im=paper(original.size,341+len(name)); bg=original.getpixel((10,10));pixels=[]
    for a,b in zip(original.getdata(),im.getdata()):
        ink=max(0,min(1,1-sum(a)/max(1,sum(bg))))
        # Preserve the familiar warning symbol and existing wording, soften printed ink.
        pixels.append(tuple(int(b[c]*(1-ink)+(34,31,25)[c]*ink) for c in range(3))+(255,))
    im.putdata(pixels);finish(im,341+len(name)).save(out/(name+' Paper.png'))

for unit in json.loads((root/'References/Infrastructure/Units.json').read_text(encoding='utf8')):
    im=paper((768,232),int(unit));d=ImageDraw.Draw(im)
    d.text((384,110),unit,font=ImageFont.truetype('C:/Windows/Fonts/courbd.ttf',148),anchor='mm',fill=(34,31,25,255))
    finish(im,int(unit)).save(out/('Mailbox '+unit+' Paper.png'))

r=random.Random(51);normal=Image.new('RGB',(256,256));normal.putdata([(128+r.randrange(-2,3),128+r.randrange(-2,3),255) for _ in range(256*256)])
normal.save(out/'Paper Normal.png')
print('Created visibly torn, stained and dusty paper: 18 electrical labels/notices and 30 mailbox number inserts.')
