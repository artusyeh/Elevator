import bpy,numpy as np
from pathlib import Path
base=Path('F:/桌面文件/Elevator/Assets/Apartment/Textures')
for folder,name,tint,median in [('peeling_painted_wall','Aged Plaster',(.92,.92,.84),.50),('Terrazzo','Grey Terrazzo',(.83,.85,.82),.48)]:
 image=bpy.data.images.load(str(base/folder/'Diffuse.jpg'));w,h=image.size;p=np.empty(w*h*4,dtype=np.float32);image.pixels.foreach_get(p);p=p.reshape(h,w,4)
 gray=p[:,:,:3].mean(axis=2);v=np.clip(gray/max(.001,float(np.median(gray)))*median,.025,.87)
 out=np.ones_like(p);out[:,:,:3]=v[:,:,None]*np.array(tint)
 image=bpy.data.images.new(name,width=w,height=h,alpha=True);image.pixels.foreach_set(out.reshape(-1));image.file_format='PNG';image.filepath_raw=str(base/folder/(name+'.png'));image.save()
print('Neutral, editable CC0 material derivatives saved; source maps preserved.')
