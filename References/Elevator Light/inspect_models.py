import bpy,json,bmesh
from pathlib import Path
from mathutils import Vector
root=Path('F:/桌面文件/Elevator');report={}
bpy.ops.wm.read_factory_settings(use_empty=True)
for name,path in [('Light',root/'Assets/Building/Models/Caged Fluorescent/Source.gltf'),('Key',root/'Assets/Building/Models/Door Key/Door Key.glb')]:
 bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=str(path))
 items=[]
 for ob in bpy.context.scene.objects:
  if ob.type!='MESH':continue
  coords=[ob.matrix_world@v.co for v in ob.data.vertices]
  items.append(dict(name=ob.name,vertices=len(coords),min=[min(v[i] for v in coords) for i in range(3)],max=[max(v[i] for v in coords) for i in range(3)],materials=[m.name if m else '' for m in ob.data.materials]))
  if name=='Light':
   bm=bmesh.new();bm.from_mesh(ob.data);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00001);remaining=set(bm.verts);components=[]
   while remaining:
    seed=remaining.pop();group={seed};stack=[seed]
    while stack:
     v=stack.pop()
     for edge in v.link_edges:
      w=edge.other_vert(v)
      if w in remaining:remaining.remove(w);group.add(w);stack.append(w)
    vs=[ob.matrix_world@v.co for v in group];components.append(dict(n=len(vs),min=[min(v[i] for v in vs) for i in range(3)],max=[max(v[i] for v in vs) for i in range(3)]))
   items[-1]['components']=sorted(components,key=lambda x:-x['n']);bm.free()
 report[name]=items
(root/'References/Elevator Light/Model Inspection.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print('Inspection saved.')
