import bpy
bpy.ops.wm.open_mainfile(filepath='F:/桌面文件/Elevator/References/Vending Machine/Japanese Vending Machine.blend')
for o in bpy.data.objects:
 if o.type=='MESH':print(o.name,'pos',tuple(o.location),'rot',tuple(o.rotation_euler),'scale',tuple(o.scale))
