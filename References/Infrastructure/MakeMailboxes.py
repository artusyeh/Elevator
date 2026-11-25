from pathlib import Path
import json, random
from PIL import Image, ImageDraw, ImageFont, ImageFilter

out=Path('Assets/Building/Textures/Mailboxes');out.mkdir(parents=True,exist_ok=True)
rng=random.Random(17);n=512
albedo=Image.new('RGB',(n,n));mask=Image.new('RGBA',(n,n));normal=Image.new('RGB',(n,n))
patina=Image.new('L',(24,24));patina.putdata([int(128+rng.gauss(0,24)) for _ in range(24*24)]);patina=patina.resize((n,n),Image.Resampling.BICUBIC).filter(ImageFilter.GaussianBlur(8))
for y in range(n):
 row=rng.gauss(0,1.6)
 for x in range(n):
  shade=int(max(0,min(255,154+row+rng.gauss(0,1.2))+(patina.getpixel((x,y))-128)*.7))
  albedo.putpixel((x,y),(shade-2,shade,shade-1))
  mask.putpixel((x,y),(184,255,255,int(86+rng.uniform(-9,9))))
  normal.putpixel((x,y),(128,int(128+rng.uniform(-2,2)),255))
albedo.save(out/'Brushed Steel Color.png');mask.save(out/'Brushed Steel Mask.png');normal.save(out/'Brushed Steel Normal.png')
units=json.loads(Path('References/Infrastructure/Units.json').read_text(encoding='utf-8'))
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',84)
for unit in units:
 paper=random.Random(int(unit));tone=paper.randrange(-9,7)
 im=Image.new('RGB',(384,128),(165+tone,161+tone,141+tone));d=ImageDraw.Draw(im)
 d.text((192,63),unit,anchor='mm',font=font,fill=(47,52,45),stroke_width=0)
 im.save(out/(unit+'.png'))
 # Localized hand grime and modest edge wear; the slot itself remains empty.
 r=random.Random(int(unit));wear=Image.new('RGBA',(512,320),(0,0,0,0));d=ImageDraw.Draw(wear)
 for i in range(65):
  edge=r.choice(['left','right','top','bottom']);x=r.randrange(7,505);y=r.randrange(7,313)
  if edge=='left': x=r.randrange(5,20)
  if edge=='right': x=r.randrange(492,507)
  if edge=='top': y=r.randrange(5,18)
  if edge=='bottom': y=r.randrange(301,315)
  d.ellipse((x-r.randrange(2,10),y-r.randrange(2,7),x+r.randrange(3,12),y+r.randrange(3,8)),fill=(63+r.randrange(0,22),58+r.randrange(0,16),42+r.randrange(0,15),r.randrange(65,155)))
 d.ellipse((419,235,477,282),fill=(45,42,32,70));d.rectangle((89,88,422,100),fill=(60,55,41,52))
 for i in range(3):
  x=r.randrange(95,414);d.line((x,95,x+r.randrange(-5,5),r.randrange(130,215)),fill=(66,58,43,32),width=r.randrange(2,5))
 wear=wear.filter(ImageFilter.GaussianBlur(1.2));d=ImageDraw.Draw(wear)
 for i in range(10):
  x=r.randrange(35,476);y=r.randrange(25,295);d.line((x,y,x+r.randrange(8,40),y+r.randrange(-2,3)),fill=(169,161,130,r.randrange(30,85)),width=1)
 d.rectangle((81,65,431,89),fill=(0,0,0,0));d.rectangle((0,0,511,3),fill=(0,0,0,0));d.rectangle((0,316,511,319),fill=(0,0,0,0))
 wear.save(out/(unit+' Wear.png'))
print('Generated subdued printed inserts for',len(units),'actual units and a fine brushed-steel PBR set.')
