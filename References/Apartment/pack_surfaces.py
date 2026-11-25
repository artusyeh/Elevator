import bpy,numpy as np
from pathlib import Path
base=Path('F:/桌面文件/Elevator/Assets/Apartment/Textures')
for asset in ['dirty_tiles','damaged_plaster','peeling_painted_wall']:
 image=bpy.data.images.load(str(base/asset/'arm.jpg'));image.colorspace_settings.name='Non-Color';w,h=image.size
 p=np.empty(w*h*4,dtype=np.float32);image.pixels.foreach_get(p);p=p.reshape(h,w,4);out=np.ones_like(p);out[:,:,0]=p[:,:,2];out[:,:,1]=p[:,:,0];out[:,:,3]=1-p[:,:,1]
 mask=bpy.data.images.new(asset+' mask',width=w,height=h,alpha=True);mask.colorspace_settings.name='Non-Color';mask.pixels.foreach_set(out.reshape(-1));mask.file_format='PNG';mask.filepath_raw=str(base/asset/'Unity Mask.png');mask.save()
image=bpy.data.images.load(str(base/'dirty_tiles/Diffuse.jpg'));w,h=image.size;p=np.empty(w*h*4,dtype=np.float32);image.pixels.foreach_get(p);p=p.reshape(h,w,4)
gray=p[:,:,:3].mean(axis=2);v=np.clip((gray/max(.001,float(np.median(gray))))**.7*.45,.035,.8)
out=np.ones_like(p);out[:,:,:3]=v[:,:,None]*np.array([.55,.85,.68]);image=bpy.data.images.new('Green Tile Albedo',width=w,height=h,alpha=True);image.pixels.foreach_set(out.reshape(-1));image.file_format='PNG';image.filepath_raw=str(base/'dirty_tiles/Green Albedo.png');image.save()
print('New PBR masks packed.')
