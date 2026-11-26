import bpy, math
from pathlib import Path
from mathutils import Vector
bpy.ops.wm.read_factory_settings(use_empty=True)
root=Path(__file__).resolve().parents[2]
folder=root/'Assets/Building/Models/Common Areas/House Slippers';folder.mkdir(exist_ok=True)
def material(n):
    m=bpy.data.materials.new(n);m.diffuse_color=(.28,.30,.26,1);return m
sole=material('Slipper Rubber');cloth=material('Slipper Cloth');lining=material('Slipper Lining')
def mesh(n,v,f,m):
    d=bpy.data.meshes.new(n);d.from_pydata(v,[],f);d.update();o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.data.materials.append(m)
    for p in d.polygons:p.use_smooth=True
    return o
# Soft, shaped insole; heel at negative Y, toe at positive Y.
v=[];f=[];N=64
for z in [.003,.014]:
    for i in range(N):
        a=2*math.pi*i/N;y=math.cos(a)*.132
        w=.045*(.88+.12*(y/.132+1)/2)
        v.append((math.sin(a)*w,y,z+.002*math.cos(a)))
for i in range(N):j=(i+1)%N;f.append((i,j,j+N,i+N))
f.append(tuple(range(N)));f.append(tuple(reversed(range(N,N*2))))
o=mesh('Moulded Sole',v,f,sole);mod=o.modifiers.new('Soft sole edges','BEVEL');mod.width=.0025;mod.segments=3
# A closed toe fabric upper, hollow at the heel opening with a visible rolled edge.
v=[];f=[];rows=15;cols=24
for j in range(rows):
    t=j/(rows-1);y=-.004+t*.133
    w=.046*math.sqrt(max(.015,1-(y/.139)**2))
    for i in range(cols+1):
        a=math.pi*i/cols
        v.append((math.cos(a)*w,y,.016+math.sin(a)*(.044*(1-.56*t))))
for j in range(rows-1):
    for i in range(cols):a=j*(cols+1)+i;f.append((a,a+cols+1,a+cols+2,a+1))
o=mesh('Padded Toe Upper',v,f,cloth);mod=o.modifiers.new('Fabric thickness','SOLIDIFY');mod.thickness=.0018
curve=bpy.data.curves.new('Rolled Opening Seam','CURVE');curve.dimensions='3D';curve.bevel_depth=.0014;curve.bevel_resolution=3
s=curve.splines.new('POLY');s.points.add(cols)
for i in range(cols+1):a=math.pi*i/cols;s.points[i].co=(math.cos(a)*.046,-.004,.016+math.sin(a)*.044,1)
o=bpy.data.objects.new('Rolled Opening Seam',curve);bpy.context.collection.objects.link(o);o.data.materials.append(lining)
for o in list(bpy.context.scene.objects):
    if o.type=='MESH':
        bpy.context.view_layer.objects.active=o;o.select_set(True)
        bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project();bpy.ops.object.mode_set(mode='OBJECT');o.select_set(False)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.convert(target='MESH')
bpy.ops.export_scene.fbx(filepath=str(folder/'House Slipper.fbx'),use_selection=True,object_types={'MESH'},bake_anim=False,axis_forward='-Z',axis_up='Y')
print('Created reusable shaped house slipper, padded upper, lining and seam.')
