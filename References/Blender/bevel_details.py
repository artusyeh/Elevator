import bpy, os
from mathutils import Vector
from pathlib import Path
root=Path('F:/桌面文件/Elevator')
# A fresh background scene, independent of any open Blender document.
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.scene.unit_settings.system='METRIC'
for key in (root/'References/Blender/BevelSizes.txt').read_text().splitlines():
    dims=[float(x) for x in key.split('_')]
    if min(dims)<=0:continue
    bpy.ops.mesh.primitive_cube_add(size=1)
    o=bpy.context.object;o.name=key;o.dimensions=dims
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    bevel=o.modifiers.new('Edge radius','BEVEL');bevel.width=min(.016,min(dims)*.18);bevel.segments=3
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    for p in o.data.polygons:p.use_smooth=True
    weighted=o.modifiers.new('Face normals','WEIGHTED_NORMAL');weighted.keep_sharp=True;weighted.weight=50
    bpy.ops.object.modifier_apply(modifier=weighted.name)
    # Normalize vertices back to a unit box. Unity retains each object's existing transform.
    normals=[Vector((n.vector.x*dims[0],n.vector.y*dims[1],n.vector.z*dims[2])).normalized() for n in o.data.corner_normals]
    for v in o.data.vertices:
        for axis in range(3):v.co[axis]/=dims[axis]
    o.data.update()
    o.data.normals_split_custom_set(normals)
(root/'Assets/Alley/Models').mkdir(exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(root/'References/Blender/Architectural Edges.blend'))
bpy.ops.export_scene.fbx(filepath=str(root/'Assets/Alley/Models/Architectural Edges.fbx'),use_selection=False,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,add_leaf_bones=False)
print('Exported',len(bpy.data.objects),'beveled box meshes')
