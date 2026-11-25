import bpy,math,random
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree
root=Path('F:/桌面文件/Elevator');out=root/'Assets/Alley/Models/Garbage';out.mkdir(parents=True,exist_ok=True)
source=root/'References/SetDressing/Models/trashbag.blend'
for index,(name,sx,sy,sz,lean) in enumerate([('Tied Bag',1,.90,.9,.025),('Full Bag',1.1,1.03,.78,-.018),('Small Bag',.83,.82,1,.035)]):
 bpy.ops.wm.open_mainfile(filepath=str(source));rng=random.Random(61+index)
 shell=next(o for o in bpy.data.objects if o.type=='MESH');shell.name='Plastic Shell';bpy.context.view_layer.objects.active=shell;shell.select_set(True);bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 verts=[v.co.copy()for v in shell.data.vertices];centre=Vector(((min(v.x for v in verts)+max(v.x for v in verts))/2,(min(v.y for v in verts)+max(v.y for v in verts))/2,min(v.z for v in verts)))
 for v in shell.data.vertices:
  p=v.co-centre;t=p.z/.575;p.x=p.x*sx*(1+.018*math.sin(t*13+index))+lean*t*t;p.y=p.y*sy*(1+.015*math.cos(t*17+index));p.z*=sz;v.co=p
 shell.data.update();shell.data.materials.clear()
 def material(n,c,rough=.6):
  m=bpy.data.materials.get(n)or bpy.data.materials.new(n);m.diffuse_color=(*c,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*c,1);p.inputs['Roughness'].default_value=rough;return m
 shell.data.materials.append(material('Bag Plastic',(.82,.84,.8),.4))
 for p in shell.data.polygons:p.use_smooth=True
 points=[v.co.copy()for v in shell.data.vertices];faces=[tuple(p.vertices)for p in shell.data.polygons];bvh=BVHTree.FromPolygons(points,faces)
 palettes=[material('Paper',(.60,.59,.51)),material('Cardboard',(.33,.28,.20)),material('Pale Packaging',(.45,.51,.48)),material('Printed Wrapper',(.29,.36,.32))]
 for k in range(18):
  at=Vector((rng.uniform(-.11,.11)*sx,rng.uniform(-.075,.075)*sy,rng.uniform(.085,.37)*sz))
  bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1,location=at);o=bpy.context.object;o.name='Crumpled Paper'if k%3 else'Packaging';o.scale=(rng.uniform(.045,.075),rng.uniform(.045,.068),rng.uniform(.035,.06));o.rotation_euler=(rng.random()*3,rng.random()*3,rng.random()*3)
  for v in o.data.vertices:v.co*=1+rng.uniform(-.12,.12)
  bpy.context.view_layer.update()
  for retry in range(9):
   valid=True
   for v in o.data.vertices:
    p=o.matrix_world@v.co;near,n,_,dist=bvh.find_nearest(p)
    if near is None or (p-near).dot(n)>-.008:valid=False;break
   if valid:break
   o.scale*=.8;bpy.context.view_layer.update()
  o.data.materials.append(palettes[k%len(palettes)])
  for p in o.data.polygons:p.use_smooth=True
 # A light tie band remains a separate editable part.
 z=.49*sz;bpy.ops.mesh.primitive_torus_add(major_radius=.022,minor_radius=.002,major_segments=40,minor_segments=8,location=(lean*(z/(.575*sz))**2,0,z));o=bpy.context.object;o.name='Bag Tie';o.scale.y=.75;o.data.materials.append(material('Bag Tie',(.71,.74,.69),.45))
 bpy.ops.wm.save_as_mainfile(filepath=str(root/'References/Garbage'/(name+'.blend')))
 bpy.ops.export_scene.fbx(filepath=str(out/(name+'.fbx')),object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,add_leaf_bones=False)
 print(name,'exported')
