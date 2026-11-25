from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter
import numpy as np,json,random
root=Path(__file__).resolve().parents[1];base=root/'Assets/Alley/Textures/PBR'
for folder in base.iterdir():
 if not folder.is_dir():continue
 arm=folder/'arm.jpg'
 if arm.exists():
  a=np.asarray(Image.open(arm).convert('RGB'));out=np.empty((*a.shape[:2],4),dtype='uint8');out[:,:,0]=a[:,:,2];out[:,:,1]=a[:,:,0];out[:,:,2]=a[:,:,1];out[:,:,3]=255-a[:,:,1];Image.fromarray(out).save(folder/'Unity Mask.png')
# Cardboard maps use ambientCG channel naming.
f=base/'cardboard';rough=next(f.glob('*Roughness*'));a=np.asarray(Image.open(rough).convert('L'));out=np.zeros((*a.shape,4),dtype='uint8');out[:,:,1]=255;out[:,:,2]=a;out[:,:,3]=255-a;Image.fromarray(out).save(f/'Unity Mask.png')
# Original reusable surface overlays. These are masks, not repurposed reference photos.
dest=root/'Assets/Alley/Textures/Weathering';dest.mkdir(exist_ok=True);random.seed(17)
colors={'Water Stain':(39,36,30),'Dirt':(52,43,31),'Rust':(105,48,20),'Moss':(42,59,30),'Mold':(32,39,33),'Concrete Crack':(37,35,31),'Peeling Paint':(162,154,134),'Discoloration':(87,78,57),'Ground Stain':(35,30,24)}
for name,c in colors.items():
 im=Image.new('RGBA',(512,512));d=ImageDraw.Draw(im)
 if name in ['Water Stain','Rust']:
  for i in range(50):
   x=random.randrange(80,430);y=random.randrange(0,80);h=random.randrange(130,480);w=random.randrange(2,18)
   d.line((x,y,x+random.randrange(-12,12),h),fill=(*c,random.randrange(20,95)),width=w)
 elif name=='Concrete Crack':
  for j in range(4):
   x=random.randrange(60,450);y=30
   for i in range(16):
    nx=x+random.randrange(-25,25);ny=y+random.randrange(16,32);d.line((x,y,nx,ny),fill=(*c,150),width=2)
    if i%4==0:d.line((nx,ny,nx+40,ny+35),fill=(*c,75),width=1)
    x,y=nx,ny
 else:
  for i in range(220):
   x=random.gauss(256,100);y=random.gauss(300,85);r=random.randrange(3,35);d.ellipse((x-r,y-r,x+r,y+r),fill=(*c,random.randrange(8,50)))
 im=im.filter(ImageFilter.GaussianBlur(.5 if name=='Concrete Crack' else 2))
 arr=np.array(im);yy,xx=np.mgrid[0:512,0:512];fade=np.clip(np.minimum.reduce([xx/35,(511-xx)/35,yy/25,(511-yy)/25]),0,1);arr[:,:,3]=(arr[:,:,3]*fade).astype('uint8');Image.fromarray(arr).save(dest/(name+'.png'))
print('Packed Unity masks and nine original weathering overlays.')
