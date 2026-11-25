import bpy,math,json
from pathlib import Path
from mathutils import Vector
root=Path('F:/桌面文件/Elevator');out=root/'Assets/Alley/Models/Lighting';out.mkdir(parents=True,exist_ok=True);src=root/'References/Cleanup/Lighting';src.mkdir(exist_ok=True)
def start():
 bpy.ops.wm.read_factory_settings(use_empty=True)
 global metal,lens,gasket,silver
 def material(n,c,m,r):
  a=bpy.data.materials.new(n);a.diffuse_color=(*c,1);a.use_nodes=True;p=a.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*c,1);p.inputs['Metallic'].default_value=m;p.inputs['Roughness'].default_value=r;return a
 metal=material('Painted Housing',(.12,.14,.13),.45,.4);lens=material('Opal Diffuser',(.72,.76,.7),0,.4);gasket=material('Rubber Gasket',(.018,.022,.02),0,.72);silver=material('Stainless Hardware',(.48,.5,.48),.85,.26)
def box(n,at,size,mat,r=.006):
 bpy.ops.mesh.primitive_cube_add(size=1,location=at);o=bpy.context.object;o.name=n;o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mat);b=o.modifiers.new('Edge Radii','BEVEL');b.width=min(r,min(size)*.35);b.segments=4;bpy.ops.object.modifier_apply(modifier=b.name);return o
def ellipsoid(n,at,scale,mat):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=48,ring_count=24,location=at);o=bpy.context.object;o.name=n;o.scale=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mat)
 for p in o.data.polygons:p.use_smooth=True
 return o
def rod(n,a,b,r,mat):
 d=Vector(b)-Vector(a);bpy.ops.mesh.primitive_cylinder_add(vertices=32,radius=r,depth=d.length,location=(Vector(a)+Vector(b))/2);o=bpy.context.object;o.name=n;o.rotation_euler=d.to_track_quat('Z','Y').to_euler();o.data.materials.append(mat);q=o.modifiers.new('Machined Edge','BEVEL');q.width=min(.0015,r*.2);q.segments=2;bpy.ops.object.modifier_apply(modifier=q.name);return o
def curve(n,points,r,mat):
 c=bpy.data.curves.new(n,'CURVE');c.dimensions='3D';c.bevel_depth=r;c.bevel_resolution=4;s=c.splines.new('BEZIER');s.bezier_points.add(len(points)-1)
 for p,v in zip(s.bezier_points,points):p.co=v;p.handle_left_type='AUTO';p.handle_right_type='AUTO'
 o=bpy.data.objects.new(n,c);bpy.context.collection.objects.link(o);o.data.materials.append(mat);return o
def save(name):
 for o in list(bpy.data.objects):
  if o.type=='CURVE':bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.convert(target='MESH');o.select_set(False)
 bpy.ops.wm.save_as_mainfile(filepath=str(src/(name+'.blend')));bpy.ops.export_scene.fbx(filepath=str(out/(name+'.fbx')),object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,add_leaf_bones=False)
start()
box('Wall Plate',(0,.008,0),(.285,.016,.18),metal,.01);ellipsoid('Gasket',(0,.035,0),(.137,.025,.083),gasket);ellipsoid('Opal Lens',(0,.053,-.007),(.125,.047,.068),lens);box('Weather Hood',(0,.052,.066),(.29,.11,.052),metal,.016)
for z in [-.045,-.012,.021]:rod('Lens Guard',(-.115,.096,z),(.115,.096,z),.003,metal)
for x in [-.121,.121]:rod('Fixing Screw',(x,.015,-.057),(x,.021,-.057),.007,silver)
save('Residential Light')
start();box('Wall Plate',(0,.012,.02),(.095,.024,.145),metal,.012)
for z in [-.025,.065]:rod('Wall Screw',(0,.025,z),(0,.029,z),.006,silver)
curve('Gooseneck Arm',[(0,.02,.045),(0,.14,.085),(0,.27,.075),(0,.31,.018)],.012,metal);rod('Lampholder',(0,.31,.015),(0,.31,-.018),.026,metal)
verts=[];faces=[];profile=[(.025,.018),(.033,.01),(.045,-.008),(.075,-.037),(.11,-.065),(.116,-.074),(.116,-.08)]
for r,z in profile:
 for k in range(64):a=k*math.tau/64;verts.append((math.cos(a)*r,.31+math.sin(a)*r,z))
for j in range(len(profile)-1):
 for k in range(64):a=j*64+k;b=j*64+(k+1)%64;faces.append((a,b,b+64,a+64))
m=bpy.data.meshes.new('Spun Shade');m.from_pydata(verts,[],faces);m.update();o=bpy.data.objects.new('Spun Metal Shade',m);bpy.context.collection.objects.link(o);o.data.materials.append(metal);bpy.context.view_layer.objects.active=o;s=o.modifiers.new('Sheet Thickness','SOLIDIFY');s.thickness=.002;bpy.ops.object.modifier_apply(modifier=s.name)
for p in o.data.polygons:p.use_smooth=True
rod('Recessed Diffuser',(0,.31,-.075),(0,.31,-.078),.103,lens)
save('Shop Entrance Light')
start();box('Die Cast Housing',(0,0,0),(.66,.185,.07),metal,.02);box('Lens Gasket',(0,0,-.037),(.62,.158,.009),gasket,.005);box('Recessed Lens',(0,0,-.044),(.595,.139,.008),lens,.003)
for x in [-.29,0,.29]:
 for y in [-.073,.073]:rod('Lens Screw',(x,y,-.039),(x,y,-.05),.0045,silver)
for x in [-.25,-.20,-.15,-.1,-.05,0,.05,.1,.15,.2,.25]:box('Cooling Fin',(x,0,.043),(.012,.105,.025),metal,.003)
rod('Arm Socket',(0,0,.045),(0,0,.105),.041,metal);rod('Socket Collar',(0,0,.078),(0,0,.09),.052,silver)
box('Cable Gland',(.24,0,.045),(.035,.04,.035),gasket,.008)
save('Alley Light')
print('Three original weatherproof fixtures exported.')
