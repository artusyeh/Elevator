import bpy
from mathutils import Vector
bpy.ops.wm.open_mainfile(filepath='F:/桌面文件/Elevator/References/SetDressing/Models/trashbag.blend')
for o in bpy.data.objects:
 if o.type=='MESH':
  pts=[o.matrix_world@Vector(v)for v in o.bound_box];print(o.name,'verts',len(o.data.vertices),'polys',len(o.data.polygons),'min',[min(p[i]for p in pts)for i in range(3)],'max',[max(p[i]for p in pts)for i in range(3)])
