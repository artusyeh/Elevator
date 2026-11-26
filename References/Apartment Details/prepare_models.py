import bpy,json
from pathlib import Path
root=Path(__file__).resolve().parents[2]
base=root/'Assets/Building/Models/Common Areas'
report={}
for record in json.loads((base/'Sources.json').read_text(encoding='utf8'))['models']:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    name=record['asset'];folder=base/name
    bpy.ops.import_scene.gltf(filepath=str(folder/(name+'.gltf')))
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    report[name]=[dict(name=o.name,location=list(o.location),dimensions=list(o.dimensions),vertices=len(o.data.vertices),materials=[m.name for m in o.data.materials])for o in meshes]
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.export_scene.fbx(filepath=str(folder/(name+'.fbx')),use_selection=True,object_types={'MESH','EMPTY'},apply_unit_scale=True,bake_anim=False,add_leaf_bones=False,axis_forward='-Z',axis_up='Y')
(root/'References/Apartment Details/Model geometry.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print('Prepared four editable FBX models and geometry report.')
