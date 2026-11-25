import bpy,json
from pathlib import Path
from mathutils import Matrix,Vector
root=Path('F:/桌面文件/Elevator');records=json.loads((root/'References/Service Shelf/Sources.json').read_text(encoding='utf8'));report=[]
for record in records:
    name=record['name'];folder=root/'Assets/Building/Models'/name
    bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=str(folder/'Source.gltf'))
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];bpy.ops.object.select_all(action='DESELECT')
    for o in meshes:o.select_set(True)
    bpy.context.view_layer.objects.active=meshes[0];bpy.ops.object.convert(target='MESH');bpy.ops.object.join();o=bpy.context.object;bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    dims=[max(v.co[i] for v in o.data.vertices)-min(v.co[i] for v in o.data.vertices) for i in range(3)];axes=sorted(range(3),key=lambda i:dims[i]);length,width,thin=axes[2],axes[1],axes[0]
    basis=[]
    for axis in [length,width,thin]:
        row=[0,0,0];row[axis]=1;basis.append(row)
    rotation=Matrix(basis)
    if rotation.determinant()<0:rotation[1]=-rotation[1]
    o.data.transform(rotation.to_4x4());mins=Vector([min(v.co[i] for v in o.data.vertices) for i in range(3)]);maxs=Vector([max(v.co[i] for v in o.data.vertices) for i in range(3)])
    at=Vector(((mins.x+maxs.x)/2,(mins.y+maxs.y)/2,mins.z))
    for v in o.data.vertices:v.co-=at
    o.name=name;report.append(dict(name=name,size=list(maxs-mins),vertices=len(o.data.vertices)))
    bpy.ops.wm.save_as_mainfile(filepath=str(root/'References/Service Shelf'/(name+'.blend')))
    bpy.ops.export_scene.fbx(filepath=str(folder/(name+'.fbx')),use_selection=True,object_types={'MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True)
(root/'References/Service Shelf/Models.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print('Prepared shelf-resting tool models, preserved UVs and editable Blender sources.')
