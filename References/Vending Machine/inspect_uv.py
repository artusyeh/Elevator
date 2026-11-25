import bpy,json
bpy.ops.wm.open_mainfile(filepath='F:/桌面文件/Elevator/References/Vending Machine/Japanese Vending Machine.blend')
for o in bpy.data.objects:
 if o.type!='MESH':continue
 print(o.name,'UVs',[(u.name, len(u.data))for u in o.data.uv_layers])
 for m in o.data.materials:
  if not m or not m.use_nodes:continue
  print(m.name, [(n.type,n.label,n.image.name if n.type=='TEX_IMAGE' and n.image else '')for n in m.node_tree.nodes]);print('links',[(l.from_node.name,l.from_socket.name,l.to_node.name,l.to_socket.name)for l in m.node_tree.links])
