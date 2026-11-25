import bpy,json
from pathlib import Path
root=Path('F:/桌面文件/Elevator');bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(root/'References/Vending Machine/Original/source/FBX/Japanese_Vending_Machine1.fbx'))
out=[]
for o in bpy.data.objects:
 if o.type!='MESH':continue
 corners=[o.matrix_world@__import__('mathutils').Vector(v) for v in o.bound_box]
 out.append({'name':o.name,'vertices':len(o.data.vertices),'polygons':len(o.data.polygons),'bounds':{'min':[min(v[i] for v in corners)for i in range(3)],'max':[max(v[i] for v in corners)for i in range(3)]},'materials':[m.name if m else None for m in o.data.materials]})
(root/'References/Vending Machine/ModelInspection.json').write_text(json.dumps({'objects':out,'images':[{'name':im.name,'size':list(im.size),'path':im.filepath}for im in bpy.data.images]},indent=2),encoding='utf-8');print(json.dumps(out,indent=2))
bpy.ops.wm.save_as_mainfile(filepath=str(root/'References/Vending Machine/Japanese Vending Machine.blend'))
