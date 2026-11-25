from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import math

out=Path('Assets/Building/Textures');out.mkdir(parents=True,exist_ok=True)
mono='C:/Windows/Fonts/consola.ttf'
sans='C:/Windows/Fonts/arial.ttf'
jp='C:/Windows/Fonts/YuGothR.ttc'
if not Path(jp).exists(): jp='C:/Windows/Fonts/YuGothM.ttc'
places=['ENTRANCE','LOBBY','2F CORRIDOR','3F CORRIDOR','4F CORRIDOR','5F CORRIDOR','6F CORRIDOR','LIFT CAR']
for i,place in enumerate(places):
 im=Image.new('RGBA',(384,256),(0,0,0,0));d=ImageDraw.Draw(im)
 d.text((13,10),f'CAM {i+1:02}  {place}',font=ImageFont.truetype(mono,13),fill=(230,240,230,220))
 im.save(out/f'{place.title()} Label.png')
im=Image.new('RGBA',(384,256),(0,0,0,0));d=ImageDraw.Draw(im)
d.text((192,128),'NO SIGNAL',anchor='mm',font=ImageFont.truetype(mono,17),fill=(235,240,235,230));im.save(out/'No Signal.png')
names=['LOBBY','SECURITY ROOM','SECOND FLOOR','THIRD FLOOR','FOURTH FLOOR','FIFTH FLOOR','SIXTH FLOOR','STAIRWELL','ELEVATOR','CCTV CAMERAS','CCTV MONITORS','ENTRANCE','EMERGENCY','MAINTENANCE','MAIN SUPPLY','MONITOR 1','MONITOR 2','MONITOR 3','MONITOR 4']
for name in names:
 im=Image.new('RGB',(384,72),(198,194,169));d=ImageDraw.Draw(im);d.rectangle((2,2,381,69),outline=(75,72,59),width=2)
 d.text((192,36),name,anchor='mm',font=ImageFont.truetype(sans,24),fill=(38,39,34));im.save(out/f'{name.title()} Plate.png')
im=Image.new('RGB',(256,320),(201,191,142));d=ImageDraw.Draw(im)
d.polygon([(128,18),(24,183),(232,183)],fill=(199,166,39),outline=(36,34,22),width=7)
d.polygon([(134,43),(92,113),(124,106),(112,157),(166,88),(133,93)],fill=(30,29,20))
d.text((128,224),'高電圧',anchor='mm',font=ImageFont.truetype(jp,36),fill=(38,32,23))
d.text((128,275),'DANGER',anchor='mm',font=ImageFont.truetype(sans,27),fill=(38,32,23));im.save(out/'Electrical Warning.png')
im=Image.new('RGB',(384,144),(198,194,169));d=ImageDraw.Draw(im);d.text((192,52),'電気室',anchor='mm',font=ImageFont.truetype(jp,45),fill=(38,39,34));d.text((192,108),'ELECTRICAL ROOM',anchor='mm',font=ImageFont.truetype(sans,24),fill=(38,39,34));im.save(out/'Electrical Room Plate.png')
im=Image.new('RGB',(512,512),(206,202,179));d=ImageDraw.Draw(im)
for n in range(31):
 a=math.radians(145-n*3.6667);r=174;r2=150 if n%5==0 else 162
 p=lambda radius:(256+math.cos(a)*radius,300-math.sin(a)*radius)
 d.line((p(r),p(r2)),fill=(33,34,29),width=4 if n%5==0 else 2)
 if n%5==0:d.text(p(132),str(n*10),anchor='mm',font=ImageFont.truetype(sans,25),fill=(33,34,29))
d.text((256,350),'V',anchor='mm',font=ImageFont.truetype(sans,42),fill=(33,34,29));d.text((256,407),'AC  0 - 300',anchor='mm',font=ImageFont.truetype(sans,23),fill=(33,34,29))
mask=Image.new('L',(512,512),0);ImageDraw.Draw(mask).ellipse((4,4,507,507),fill=255);im=im.convert('RGBA');im.putalpha(mask);im.save(out/'Voltage Dial.png')
print('Generated CCTV identification overlays, panel labels, warning signs and voltage dial.')
