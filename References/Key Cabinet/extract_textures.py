from pathlib import Path
import struct,json
root=Path(__file__).resolve().parents[2];folder=root/'Assets/Building/Models/Door Key';cache=root/'References/Key Cabinet/TextureCache';cache.mkdir(exist_ok=True)
raw=(folder/'Door Key.glb').read_bytes();length,_=struct.unpack_from('<II',raw,12);g=json.loads(raw[20:20+length]);n,_=struct.unpack_from('<II',raw,20+length);binary=raw[28+length:28+length+n]
paths=[]
for i,image in enumerate(g['images']):
 b=g['bufferViews'][image['bufferView']];start=b.get('byteOffset',0);p=cache/(str(i)+'.ktx2');p.write_bytes(binary[start:start+b['byteLength']]);paths.append(str(p))
 image.clear();image['uri']='Textures/Image'+str(i)+'.png'
for texture in g['textures']:
 ext=texture.pop('extensions',{}).get('KHR_texture_basisu')
 if ext:texture['source']=ext['source']
for key in ['extensionsUsed','extensionsRequired']:
 if key in g:g[key]=[e for e in g[key] if e!='KHR_texture_basisu']
g['buffers'][0]['uri']='Geometry.bin';(folder/'Geometry.bin').write_bytes(binary);(folder/'Decoded.gltf').write_text(json.dumps(g),encoding='utf8');(cache/'Files.json').write_text(json.dumps(paths),encoding='utf8')
print('Extracted compressed textures for existing local KTX decoder.')
