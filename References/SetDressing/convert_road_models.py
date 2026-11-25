import bpy
from pathlib import Path
base=Path('F:/桌面文件/Elevator/Assets/Alley/Models/Street')
for slug in ['blossom-wave-manhole-cover','street-gully-grate']:
 bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=str(base/slug/(slug+'.gltf')));bpy.ops.wm.save_as_mainfile(filepath='F:/桌面文件/Elevator/References/SetDressing/Models/'+slug+'.blend');bpy.ops.export_scene.fbx(filepath=str(base/slug/(slug+'.fbx')),object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,add_leaf_bones=False,path_mode='RELATIVE')
