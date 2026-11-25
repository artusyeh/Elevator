from pathlib import Path
import math,random
import bpy

root=Path('F:/桌面文件/Elevator');folder=root/'Assets/Building/Models/Corridor Fluorescent';folder.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
def box(name,at,size,bevel):
    bpy.ops.mesh.primitive_cube_add(size=1,location=at);o=bpy.context.object;o.name=name;o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    mod=o.modifiers.new('Soft moulded edges','BEVEL');mod.width=bevel;mod.segments=4;bpy.ops.object.modifier_apply(modifier=mod.name)
    for p in o.data.polygons:p.use_smooth=True
    mod=o.modifiers.new('Face normals','WEIGHTED_NORMAL');bpy.ops.object.modifier_apply(modifier=mod.name)
    return o
body=box('Painted Housing',(0,0,-.015),(.74,.218,.030),.011)
# Shallow convex, ribbed opal cover with rounded end transitions, rather than exposed tubes.
sections=[(-.347,.07),(-.344,.45),(-.337,.83),(-.325,1),(.325,1),(.337,.83),(.344,.45),(.347,.07)]
verts=[];uv=[];steps=54
for x,scale in sections:
    for i in range(steps+1):
        a=math.pi*i/steps;y=-.086*math.cos(a)*scale;rib=.00025*math.cos(i*math.pi)
        z=-.031-.037*math.sin(a)*scale+rib
        verts.append((x,y,z));uv.append((x/.694+.5,y/.172+.5))
faces=[]
for j in range(len(sections)-1):
    for i in range(steps):
        a=j*(steps+1)+i;b=a+steps+1;faces.append((a,a+1,b+1,b))
faces += [tuple(range(steps,-1,-1)),tuple((len(sections)-1)*(steps+1)+i for i in range(steps+1))]
mesh=bpy.data.meshes.new('Opal Cover');mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new('Opal Cover',mesh);bpy.context.collection.objects.link(o)
layer=mesh.uv_layers.new(name='Cover UV')
for p in mesh.polygons:
    p.use_smooth=True
    for loop in p.loop_indices:layer.data[loop].uv=uv[mesh.loops[loop].vertex_index]
for x in [-.295,.295]:
    for y in [-.089,.089]:box('Cover Clip',(x,y,-.036),(.022,.013,.022),.002)
for x in [-.32,.32]:box('End Cap',(x*1.10,0,-.027),(.031,.19,.022),.006)
bpy.ops.wm.save_as_mainfile(filepath=str(root/'References/Residential Lights/Corridor Fluorescent.blend'))
bpy.ops.export_scene.fbx(filepath=str(folder/'Corridor Fluorescent.fbx'),object_types={'MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True)

