import bpy,bmesh,json
from pathlib import Path
from mathutils import Vector,Matrix
root=Path('F:/桌面文件/Elevator')
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(root/'Assets/Building/Models/Caged Fluorescent/Source.gltf'))
ob=next(o for o in bpy.context.scene.objects if o.type=='MESH');bpy.context.view_layer.objects.active=ob;ob.select_set(True);bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
bm=bmesh.new();bm.from_mesh(ob.data);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00001)
remaining=set(bm.verts);discard=[]
while remaining:
 seed=remaining.pop();group={seed};stack=[seed]
 while stack:
  v=stack.pop()
  for edge in v.link_edges:
   w=edge.other_vert(v)
   if w in remaining:remaining.remove(w);group.add(w);stack.append(w)
 if max(v.co.z for v in group)>-.53:discard.extend(group)
bmesh.ops.delete(bm,geom=discard,context='VERTS');bm.to_mesh(ob.data);bm.free()
top=max(v.co.z for v in ob.data.vertices)
for v in ob.data.vertices:v.co.z-=top
ob.name='Ceiling Fluorescent';bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
bpy.ops.wm.save_as_mainfile(filepath=str(root/'References/Elevator Light/Ceiling Fluorescent.blend'))
bpy.ops.export_scene.fbx(filepath=str(root/'Assets/Building/Models/Caged Fluorescent/Ceiling Fluorescent.fbx'),use_selection=True,object_types={'MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(root/'Assets/Building/Models/Door Key/Decoded.gltf'))
ob=next(o for o in bpy.context.scene.objects if o.type=='MESH');bpy.context.view_layer.objects.active=ob;ob.select_set(True);bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
# Its 85 mm principal axis becomes downward; the source split ring sits at the hook.
rotation=Matrix.Rotation(3.141592653589793/2,4,'Z')@Matrix.Rotation(3.141592653589793/2,4,'Y')
ob.data.transform(rotation);pts=[v.co for v in ob.data.vertices];top=max(p.z for p in pts)
for v in ob.data.vertices:v.co.z-=top
ob.name='Door Key'
tex=root/'Assets/Building/Models/Door Key/Textures';tex.mkdir(exist_ok=True)
records=[]
for mat in ob.data.materials:
 if not mat or not mat.use_nodes:continue
 for node in mat.node_tree.nodes:
  if node.type=='TEX_IMAGE' and node.image and node.image.has_data:
   image=node.image;image.filepath_raw=str(tex/(image.name.replace('/','_')+'.png'));image.file_format='PNG';image.save()
 records.append(dict(name=mat.name))
(root/'References/Key Cabinet/Materials.json').write_text(json.dumps(records,indent=2),encoding='utf8')
bpy.ops.wm.save_as_mainfile(filepath=str(root/'References/Key Cabinet/Door Key.blend'))
bpy.ops.export_scene.fbx(filepath=str(root/'Assets/Building/Models/Door Key/Door Key.fbx'),use_selection=True,object_types={'MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True)
print('Adapted CC0 fixture without suspension hardware; exported hanging door key and its textures.')
