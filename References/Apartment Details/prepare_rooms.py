from pathlib import Path
import bpy,json,re
root=Path(__file__).resolve().parents[2];base=root/'Assets/Building/Models/Interiors'
records=json.loads((base/'Sources.json').read_text())['models']
for rec in records:
    bpy.ops.wm.read_factory_settings(use_empty=True);name=rec['asset'];folder=base/name
    source=folder/(name+('.gltf' if (folder/(name+'.gltf')).exists() else '.glb'))
    bpy.ops.import_scene.gltf(filepath=str(source))
    if not rec['materials']:
        for m in bpy.data.materials:
            bsdf=next((n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED'),None) if m.use_nodes else None
            color=list(bsdf.inputs['Base Color'].default_value) if bsdf else list(m.diffuse_color)
            rec['materials'].append(dict(name=m.name,color='',normal='',arm='',factor=color,metallic=bsdf.inputs['Metallic'].default_value if bsdf else 0,roughness=bsdf.inputs['Roughness'].default_value if bsdf else .7))
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.export_scene.fbx(filepath=str(folder/(name+'.fbx')),use_selection=True,object_types={'MESH','EMPTY'},bake_anim=False,add_leaf_bones=False,axis_forward='-Z',axis_up='Y')
(base/'Sources.json').write_text(json.dumps({'models':records},indent=2),encoding='utf8')
print('Prepared interior FBX assets with original material factors and PBR records.')
