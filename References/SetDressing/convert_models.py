import bpy,json
from pathlib import Path
root=Path('F:/桌面文件/Elevator')
source=json.loads((root/'Assets/Alley/Models/Street/ModelSources.json').read_text())
for record in source['models']:
 asset=record['asset'];folder=root/'Assets/Alley/Models/Street'/asset
 bpy.ops.wm.read_factory_settings(use_empty=True)
 path=folder/(asset+'.gltf')
 if not path.exists():path=folder/(asset+'.glb')
 if not path.exists():continue
 bpy.ops.import_scene.gltf(filepath=str(path))
 bpy.ops.wm.save_as_mainfile(filepath=str(root/'References/SetDressing/Models'/(asset+'.blend')))
 bpy.ops.export_scene.fbx(filepath=str(folder/(asset+'.fbx')),object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,add_leaf_bones=False,path_mode='RELATIVE')
 print(asset,'converted',sum(len(o.data.polygons) for o in bpy.data.objects if o.type=='MESH'),'faces')
