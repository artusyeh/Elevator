import bpy,bmesh,json,math,random
from pathlib import Path
from mathutils import Vector
root=Path('F:/桌面文件/Elevator');review=root/'References/Stairwell';review.mkdir(parents=True,exist_ok=True)
folder=root/'Assets/Building/Models/Residential Fluorescent';folder.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
def box(name,at,size,bevel=.004):
    bpy.ops.mesh.primitive_cube_add(size=1,location=at);o=bpy.context.object;o.name=name;o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    mod=o.modifiers.new('Rounded edges','BEVEL');mod.width=bevel;mod.segments=4;bpy.ops.object.modifier_apply(modifier=mod.name)
    for p in o.data.polygons:p.use_smooth=True
    return o
box('Housing',(0,0,-.014),(.74,.216,.028),.012)
box('Cover Rim',(0,0,-.029),(.713,.193,.015),.010)
verts=[];faces=[];steps=40
sections=[(-.347,.12),(-.343,.65),(-.331,.94),(-.315,1),(.315,1),(.331,.94),(.343,.65),(.347,.12)]
for x,s in sections:
    for i in range(steps+1):
        a=math.pi*i/steps;rib=.0003*math.sin(a*34)**2
        verts.append((x,-.085*math.cos(a)*s,-.035-(.035*math.sin(a)+rib)*s))
for j in range(len(sections)-1):
    for i in range(steps):
        a=j*(steps+1)+i;faces.append((a,a+1,a+steps+2,a+steps+1))
faces.append(tuple(range(steps,-1,-1)));faces.append(tuple((len(sections)-1)*(steps+1)+i for i in range(steps+1)))
mesh=bpy.data.meshes.new('Prismatic cover');mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new('Diffuser',mesh);bpy.context.collection.objects.link(o)
bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
uv=mesh.uv_layers.new(name='UVMap')
for p in mesh.polygons:
    p.use_smooth=True
    for li in p.loop_indices:
        co=mesh.vertices[mesh.loops[li].vertex_index].co;uv.data[li].uv=((co.x+.347)/.694,(co.y+.085)/.17)
for x in [-.30,.30]:
    for y in [-.097,.097]:box('Cover Clip',(x,y,-.037),(.024,.018,.028),.002)
bpy.ops.wm.save_as_mainfile(filepath=str(review/'Residential Fluorescent.blend'))
bpy.ops.export_scene.fbx(filepath=str(folder/'Residential Fluorescent.fbx'),object_types={'MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y')
# Solid concrete flight: nine risers, 20 cm structural waist, closed side faces.
bpy.ops.wm.read_factory_settings(use_empty=True);rise=2.9/18;going=.26;width=1.28
profile=[(0,0)]
for i in range(9):profile.extend([(i*going,(i+1)*rise),((i+1)*going,(i+1)*rise)])
profile.extend([(9*going,9*rise-.20),(0,-.20)])
verts=[(x,z,y) for x in [-width/2,width/2] for z,y in profile];n=len(profile);faces=[]
faces.extend([tuple(range(n-1,-1,-1)),tuple(range(n,n*2))])
for i in range(n):j=(i+1)%n;faces.append((i,j,n+j,n+i))
mesh=bpy.data.meshes.new('Concrete Flight');mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new('Concrete Flight',mesh);bpy.context.collection.objects.link(o);bpy.context.view_layer.objects.active=o;o.select_set(True)
bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
bevel=o.modifiers.new('Worn concrete edges','BEVEL');bevel.width=.003;bevel.segments=2;bpy.ops.object.modifier_apply(modifier=bevel.name)
bpy.ops.wm.save_as_mainfile(filepath=str(review/'Concrete Flight.blend'))
mesh=o.data;mesh.calc_loop_triangles();vs=[];ns=[];tris=[]
for tri in mesh.loop_triangles:
    base=len(vs)
    for vi in tri.vertices:
        co=mesh.vertices[vi].co;normal=tri.normal;vs.append(dict(x=co.x,y=co.z,z=co.y));ns.append(dict(x=normal.x,y=normal.z,z=normal.y))
    tris.extend([base,base+2,base+1])
(review/'Concrete Flight.json').write_text(json.dumps(dict(vertices=vs,normals=ns,triangles=tris)),encoding='utf8')
print('Created editable residential fluorescent with prismatic cover, aged textures and a solid stair flight.')
