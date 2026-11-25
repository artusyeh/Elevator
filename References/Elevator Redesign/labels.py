from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import random,math,sys
root=Path(__file__).resolve().parents[2];out=root/'Assets/Building/Textures/Elevator/Labels';out.mkdir(parents=True,exist_ok=True)
font=next((p for p in [Path('C:/Windows/Fonts/YuGothM.ttc'),Path('C:/Windows/Fonts/msgothic.ttc'),Path('C:/Windows/Fonts/msyh.ttc'),Path('C:/Windows/Fonts/simsun.ttc')] if p.exists()))
def f(size):return ImageFont.truetype(str(font),size)
def plate(name,size,heading,subtitle):
 w,h=size;r=random.Random(name);im=Image.new('RGB',size);d=ImageDraw.Draw(im)
 for y in range(h):
  for x in range(w):
   edge=min(x,y,w-1-x,h-1-y);age=max(0,1-edge/(h*.12));v=r.randrange(-3,4)
   im.putpixel((x,y),(int(202+v-age*40),int(190+v-age*43),int(152+v-age*35)))
 d=ImageDraw.Draw(im);d.rectangle((w*.05,h*.07,w*.95,h*.93),outline=(88,82,62),width=2)
 for text,cy,initial,colour in [(heading,h*.36,int(h*.25),(46,48,39)),(subtitle,h*.72,int(h*.145),(66,63,47))]:
  size=initial
  while d.textbbox((0,0),text,font=f(size))[2]>w*.82:size-=1
  d.text((w*.5,cy),text,font=f(size),fill=colour,anchor='mm')
 im.save(out/(name+'.png'))
if '--plates-only' in sys.argv:
 plate('Capacity Plate',(960,280),'定員 6 名　積載 450 kg','乗用　青葉荘 昇降機')
 plate('Manufacturer Plate',(960,256),'東栄昇降機工業','製造 昭和53年（1978年）　型式 TR-6')
 print('Updated two plates with separate measured text lines.');sys.exit(0)
def paper(name,size,heading,rows,stamp=''):
 im=Image.new('RGB',size,(202,190,152));d=ImageDraw.Draw(im);w,h=size;r=random.Random(name)
 for y in range(h):
  for x in range(w):
   e=min(x,y,w-1-x,h-1-y);age=max(0,1-e/(min(w,h)*.08));v=r.randrange(-4,5)
   im.putpixel((x,y),(int(202+v-age*40),int(190+v-age*43),int(152+v-age*35)))
 d=ImageDraw.Draw(im);d.rectangle((w*.05,h*.04,w*.95,h*.96),outline=(88,82,62),width=2)
 d.text((w*.09,h*.08),heading,font=f(int(w*.071)),fill=(46,48,39))
 y=h*.23;gap=min(h*.105,h*.45/max(1,len(rows)-1))
 for a,b in rows:
  d.text((w*.09,y),a,font=f(int(w*.047)),fill=(59,58,45));d.text((w*.45,y),b,font=f(int(w*.043)),fill=(66,63,47));d.line((w*.08,y+h*.055,w*.92,y+h*.055),fill=(147,136,100),width=1);y+=gap
 if stamp:
  d.rounded_rectangle((w*.63,h*.74,w*.89,h*.87),radius=9,outline=(129,70,55),width=3);d.text((w*.665,h*.77),stamp,font=f(int(w*.051)),fill=(129,70,55))
 im.save(out/(name+'.png'))
paper('Inspection Certificate',(768,1024),'昇降機 定期検査記録',[('建物名','青葉荘'),('検査実施日','令和八年 九月十八日'),('用途','乗用 昇降機'),('停止階','一階 ～ 六階'),('定員／積載','六名 ／ 四五〇 kg'),('管理窓口','一階 管理室')],'点検済')
paper('Maintenance Notice',(640,800),'ご利用の皆様へ',[('扉の開閉時','手を触れないこと'),('荷物の搬入','扉を押さえないこと'),('異音・不具合','管理室へご連絡ください'),('非常の場合','呼出ボタンを押すこと')],'管理室')
if '--notices-only' in sys.argv:
 print('Updated two notices with clearance above their stamps.');sys.exit(0)
plate('Capacity Plate',(960,280),'定員 6 名　積載 450 kg','乗用　青葉荘 昇降機')
plate('Manufacturer Plate',(960,256),'東栄昇降機工業','製造 昭和53年（1978年）　型式 TR-6')
for value in range(1,7):
 im=Image.new('RGBA',(256,256));d=ImageDraw.Draw(im);d.text((128,132),str(value),font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',164),fill=(204,201,179,255),anchor='mm');im.save(out/('Floor '+str(value)+'.png'))
for name in ['Open','Close','Alarm']:
 im=Image.new('RGBA',(256,256));d=ImageDraw.Draw(im);c=(204,201,179,255)
 if name in ['Open','Close']:
  if name=='Open':d.polygon([(40,128),(111,70),(111,186)],fill=c);d.polygon([(216,128),(145,70),(145,186)],fill=c)
  else:d.polygon([(111,128),(40,70),(40,186)],fill=c);d.polygon([(145,128),(216,70),(216,186)],fill=c)
  d.line((128,59,128,197),fill=c,width=8)
 else:
  d.arc((65,55,191,176),180,360,fill=c,width=11);d.line((65,115,60,178,196,178,191,115),fill=c,width=10);d.arc((108,176,148,211),0,180,fill=c,width=8)
 im.save(out/(name+'.png'))
print('Created fictional Japanese labels and button markings; font rendered only, no font distributed.')
