import bpy,json,numpy as np
from pathlib import Path
root=Path('F:/桌面文件/Elevator');base=root/'Assets/Apartment';source=json.loads((base/'Models/Sources.json').read_text())
for record in source['models']:
 asset=record['asset'];folder=base/'Models'/asset;bpy.ops.wm.read_factory_settings(use_empty=True)
 path=folder/(asset+'.gltf')
 if not path.exists():path=folder/(asset+'.glb')
 bpy.ops.import_scene.gltf(filepath=str(path))
 if not record['materials']:
  for m in bpy.data.materials:
   p=m.node_tree.nodes.get('Principled BSDF') if m.use_nodes else None
   record['materials'].append(dict(name=m.name,color='',normal='',arm='',factor=list(p.inputs['Base Color'].default_value) if p else list(m.diffuse_color),metallic=p.inputs['Metallic'].default_value if p else 0,roughness=p.inputs['Roughness'].default_value if p else .6,alpha='OPAQUE',doubleSided=False))
 # Convert ORM into Unity metallic/occlusion/smoothness. Preserve source maps.
 for m in record['materials']:
  if not m.get('arm'):continue
  p=folder/m['arm'];image=bpy.data.images.load(str(p),check_existing=True);image.colorspace_settings.name='Non-Color';w,h=image.size
  pixels=np.empty(w*h*4,dtype=np.float32);image.pixels.foreach_get(pixels);pixels=pixels.reshape(h,w,4)
  out=np.ones_like(pixels);out[:,:,0]=pixels[:,:,2];out[:,:,1]=pixels[:,:,0];out[:,:,3]=1-pixels[:,:,1]
  name=p.stem+'_Unity.png';mask=bpy.data.images.new('Unity Mask',width=w,height=h,alpha=True);mask.colorspace_settings.name='Non-Color';mask.pixels.foreach_set(out.reshape(-1));mask.filepath_raw=str(p.parent/name);mask.file_format='PNG';mask.save();m['unityMask']=str(Path(m['arm']).parent/name).replace('\\','/')
 bpy.ops.wm.save_as_mainfile(filepath=str(root/'References/Apartment/Sources'/(asset+'.blend')))
 bpy.ops.export_scene.fbx(filepath=str(folder/(asset+'.fbx')),object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,add_leaf_bones=False,path_mode='RELATIVE')
 print('Converted',asset,flush=True)
# Terrazzo packed mask.
image=bpy.data.images.load(str(base/'Textures/Terrazzo/arm.jpg'));image.colorspace_settings.name='Non-Color';w,h=image.size;p=np.empty(w*h*4,dtype=np.float32);image.pixels.foreach_get(p);p=p.reshape(h,w,4);out=np.ones_like(p);out[:,:,0]=p[:,:,2];out[:,:,1]=p[:,:,0];out[:,:,3]=1-p[:,:,1]
image=bpy.data.images.new('Terrazzo Mask',width=w,height=h,alpha=True);image.colorspace_settings.name='Non-Color';image.pixels.foreach_set(out.reshape(-1));image.file_format='PNG';image.filepath_raw=str(base/'Textures/Terrazzo/Unity Mask.png');image.save()
(base/'Models/Sources.json').write_text(json.dumps(source,indent=2),encoding='utf-8')
print('Apartment source models and PBR masks prepared.',flush=True)
