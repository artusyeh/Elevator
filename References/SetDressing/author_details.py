import bpy,math,json
from pathlib import Path
from mathutils import Vector
root=Path('F:/桌面文件/Elevator');base=root/'Assets/Alley/Models/Street';records=[]
def mat(name,color,metal=0,rough=.5):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough;return m
def cube(name,at,size,m,r=.008):
 bpy.ops.mesh.primitive_cube_add(size=1,location=at);o=bpy.context.object;o.name=name;o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(m)
 b=o.modifiers.new('Rounded Edges','BEVEL');b.width=min(r,min(size)*.2);b.segments=3;bpy.ops.object.modifier_apply(modifier=b.name);return o
def curve(name,points,radius,m):
 c=bpy.data.curves.new(name,'CURVE');c.dimensions='3D';c.bevel_depth=radius;c.bevel_resolution=3;s=c.splines.new('POLY');s.points.add(len(points)-1)
 for p,v in zip(s.points,points):p.co=(*v,1)
 o=bpy.data.objects.new(name,c);bpy.context.collection.objects.link(o);o.data.materials.append(m);return o
def start():
 bpy.ops.wm.read_factory_settings(use_empty=True)
 global steel,black,paper,red,cloth,wood
 steel=mat('Brushed Steel',(.32,.35,.36),.8,.32);black=mat('Black Polymer',(.025,.03,.028),0,.4);paper=mat('Ivory Enamel',(.58,.6,.53),.15,.48);red=mat('Signal Red',(.34,.06,.04),.1,.42);cloth=mat('Umbrella Fabric',(.055,.075,.085),0,.85);wood=mat('Ash Handle',(.38,.28,.16),0,.6)
def finish(asset):
 f=base/asset;f.mkdir(exist_ok=True)
 for o in list(bpy.data.objects):
  if o.type=='CURVE':bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.convert(target='MESH');o.select_set(False)
 ms=[]
 for m in bpy.data.materials:
  p=m.node_tree.nodes.get('Principled BSDF');ms.append({'name':m.name,'color':'','normal':'','arm':'','alpha':'OPAQUE','doubleSided':False,'factor':list(m.diffuse_color),'metallic':p.inputs['Metallic'].default_value,'roughness':p.inputs['Roughness'].default_value})
 bpy.ops.wm.save_as_mainfile(filepath=str(root/'References/SetDressing/Models'/(asset+'.blend')))
 bpy.ops.export_scene.fbx(filepath=str(f/(asset+'.fbx')),object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,add_leaf_bones=False)
 records.append({'asset':asset,'source':'Original Blender model: References/SetDressing/author_details.py','license':'Project-authored','materials':ms})
start()
cube('Mail Case',(0,0,.18),(.3,.12,.36),steel);cube('Rain Lip',(0,-.018,.37),(.33,.16,.045),steel);cube('Letter Slot',(0,-.069,.28),(.23,.014,.035),black);cube('Nameplate',(0,-.07,.17),(.16,.008,.055),paper);cube('Mail Door',(0,-.068,.11),(.26,.012,.13),steel);cube('Lock',(.09,-.079,.13),(.022,.01,.025),black);finish('wall-mailbox')
start()
cube('Intercom Case',(0,0,.095),(.095,.045,.19),steel);cube('Speaker',(0,-.025,.13),(.067,.007,.065),black)
for z in range(6):cube('Speaker Grille',(0,-.031,.105+z*.01),(.061,.005,.003),steel,.001)
cube('Call Button',(0,-.029,.045),(.042,.014,.027),paper);cube('Nameplate',(0,-.029,.081),(.062,.006,.018),paper);finish('entrance-intercom')
start()
cube('Camera Body',(0,0,.06),(.105,.25,.1),paper);cube('Weather Hood',(0,-.02,.12),(.13,.3,.025),steel);cube('Lens Face',(0,-.13,.06),(.095,.012,.085),black);curve('Mounting Arm',[(0,.05,.01),(0,.05,-.08),(0,.13,-.13)],.018,steel);cube('Mounting Plate',(0,.15,-.13),(.11,.035,.13),steel);finish('security-camera')
start()
curve('Broom Handle',[(0,0,.12),(0,0,1.12)],.014,wood);cube('Broom Head',(0,0,.11),(.28,.065,.065),red)
for j in range(35):
 x=-.13+j*.0076
 curve('Brush Fibers',[(x,0,.085),(x*1.04,.014,.006)],.0035,black)
finish('street-broom')
start()
curve('Umbrella Shaft',[(0,0,.12),(0,0,.94)],.006,steel);curve('Curved Handle',[(.035*math.cos(a),0,.07+.035*math.sin(a)) for a in [i*math.pi/24 for i in range(49)]],.012,black)
verts=[];faces=[]
for j in range(13):
 z=.22+j*.055;r=.012+.055*math.sin(j/12*math.pi)*(.6+j/20)
 for i in range(25):
  a=i*math.pi/12;rr=r*(1+.14*math.cos(a*12));verts.append((rr*math.cos(a),rr*math.sin(a),z))
for j in range(12):
 for i in range(24):a=j*25+i;faces.append((a,a+1,a+26,a+25))
m=bpy.data.meshes.new('Folded Fabric');m.from_pydata(verts,[],faces);m.update();o=bpy.data.objects.new('Folded Canopy',m);bpy.context.collection.objects.link(o);o.data.materials.append(cloth);cube('Closure Strap',(0,0,.48),(.135,.025,.028),black);finish('folded-umbrella')
start()
for panel in range(3):
 verts=[];uvs=[];faces=[]
 for y in range(13):
  for x in range(17):
   u=x/16;v=y/12;px=-1.35+panel*.91+u*.88;z=.66-v*.66-.025*math.sin(u*math.pi);depth=.025*math.sin(u*math.pi*6)+.012*v;verts.append((px,depth,z));uvs.append(((panel+u)/3,1-v))
 for y in range(12):
  for x in range(16):i=y*17+x;faces.append((i,i+1,i+18,i+17))
 m=bpy.data.meshes.new('Noren Cloth');m.from_pydata(verts,[],faces);m.update();layer=m.uv_layers.new()
 for p in m.polygons:
  for l in p.loop_indices:layer.data[l].uv=uvs[m.loops[l].vertex_index]
 o=bpy.data.objects.new('Curtain Panel',m);bpy.context.collection.objects.link(o);o.data.materials.append(cloth);s=o.modifiers.new('Cloth Thickness','SOLIDIFY');s.thickness=.002;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=s.name)
finish('shop-noren')
p=base/'ModelSources.json';d=json.loads(p.read_text());d['models']+=records;p.write_text(json.dumps(d,indent=2),encoding='utf-8')
