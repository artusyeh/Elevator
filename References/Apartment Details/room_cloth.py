import bpy,math
from pathlib import Path
root=Path(__file__).resolve().parents[2];folder=root/'Assets/Building/Models/Interiors/Linens';folder.mkdir(exist_ok=True)
def mesh(name,vs,fs):
    me=bpy.data.meshes.new(name);me.from_pydata(vs,[],fs);me.update();o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o)
    for p in me.polygons:p.use_smooth=True
    bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project();bpy.ops.object.mode_set(mode='OBJECT');o.select_set(False)
    return o
for name in ['Curtain','Bed Linen','Furniture Dust Sheet','Hanging Shirt']:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    if name=='Curtain':
        vs=[];fs=[];nx=64;ny=20
        for j in range(ny+1):
            t=j/ny
            for i in range(nx+1):
                x=i/nx*.68;vs.append((x,math.sin(i/nx*math.pi*14)*(.035+.015*t),-t*1.55+.008*math.sin(i/nx*math.pi*8)*t))
        for j in range(ny):
            for i in range(nx):a=j*(nx+1)+i;fs.append((a,a+1,a+nx+2,a+nx+1))
        o=mesh(name,vs,fs);s=o.modifiers.new('Hem thickness','SOLIDIFY');s.thickness=.0015
    elif name=='Hanging Shirt':
        outline=[(-.20,0,0),(-.34,0,-.10),(-.30,0,-.24),(-.19,0,-.18),(-.17,0,-.60),(.17,0,-.60),(.19,0,-.18),(.30,0,-.24),(.34,0,-.10),(.20,0,0),(.07,0,-.025),(0,0,-.07),(-.07,0,-.025)]
        o=mesh(name,outline,[tuple(range(len(outline)))]);s=o.modifiers.new('Cloth thickness','SOLIDIFY');s.thickness=.003;b=o.modifiers.new('Cloth edge rounding','BEVEL');b.width=.008;b.segments=3
    else:
        vs=[];fs=[];nx=30;ny=40
        for j in range(ny+1):
            y=-1.05+j/ny*2.1
            for i in range(nx+1):
                x=-.58+i/nx*1.16;edge=max(abs(x)-.45,abs(y)-.91,0);height=.015*math.sin(x*29+y*21)+.009*math.sin(y*51)-edge*(4 if name=='Bed Linen' else 5)
                vs.append((x,y,height))
        for j in range(ny):
            for i in range(nx):a=j*(nx+1)+i;fs.append((a,a+1,a+nx+2,a+nx+1))
        o=mesh(name,vs,fs);s=o.modifiers.new('Cloth thickness','SOLIDIFY');s.thickness=.002
    mat=bpy.data.materials.new('Old Linen');o.data.materials.append(mat);bpy.ops.object.select_all(action='SELECT')
    bpy.ops.export_scene.fbx(filepath=str(folder/(name+'.fbx')),use_selection=True,object_types={'MESH'},use_mesh_modifiers=True,bake_anim=False,axis_forward='-Z',axis_up='Y')
print('Prepared folded curtains, rumpled bedding, dust sheet and laundry garments.')
