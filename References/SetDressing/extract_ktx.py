import json,struct
from pathlib import Path
root=Path(__file__).resolve().parents[2];base=root/'Assets/Alley/Models/Street';cache=root/'References/SetDressing/TextureCache';cache.mkdir(exist_ok=True);files=[]
d=json.loads((base/'ModelSources.json').read_text())
for r in d['models']:
 f=base/r['asset']/(r['asset']+'.glb')
 if not f.exists():continue
 b=f.read_bytes();l=struct.unpack_from('<I',b,12)[0];g=json.loads(b[20:20+l]);offset=20+l;length=struct.unpack_from('<I',b,offset)[0];binary=b[offset+8:offset+8+length]
 for i,img in enumerate(g.get('images',[])):
  v=g['bufferViews'][img['bufferView']];start=v.get('byteOffset',0);payload=binary[start:start+v['byteLength']];p=cache/(r['asset']+'_'+str(i)+'.ktx2');p.write_bytes(payload);files.append(str(p))
 def uri(info):
  if not info:return ''
  t=g['textures'][info['index']];i=t.get('source',t.get('extensions',{}).get('KHR_texture_basisu',{}).get('source'));return 'Image'+str(i)+'.png'
 for i,m in enumerate(g.get('materials',[])):
  pbr=m.get('pbrMetallicRoughness',{});r['materials'][i]['color']=uri(pbr.get('baseColorTexture'));r['materials'][i]['normal']=uri(m.get('normalTexture'));r['materials'][i]['arm']=uri(pbr.get('metallicRoughnessTexture'))
 for wrong in f.parent.glob('Image*.jpg'):
  if wrong.read_bytes().startswith(b'\xabKTX'):wrong.unlink()
(base/'ModelSources.json').write_text(json.dumps(d,indent=2),encoding='utf-8');(cache/'Files.json').write_text(json.dumps(files))
