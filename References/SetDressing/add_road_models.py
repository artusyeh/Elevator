import json,struct,urllib.request
from pathlib import Path
base=Path('Assets/Alley/Models/Street'); d=json.loads((base/'ModelSources.json').read_text());catalog=json.loads(Path('References/SetDressing/tevy-models.json').read_text())['models']
for slug in ['blossom-wave-manhole-cover','street-gully-grate']:
 if any(r['asset']==slug for r in d['models']): continue
 m=next(x for x in catalog if x['slug']==slug);folder=base/slug;folder.mkdir(exist_ok=True);b=urllib.request.urlopen('https://tevy.dev'+m['files'][0]['path']).read();(folder/(slug+'.glb')).write_bytes(b);l=struct.unpack_from('<I',b,12)[0];g=json.loads(b[20:20+l]);r={'asset':slug,'source':'https://tevy.dev/models/'+slug,'license':m['license'],'materials':[]}
 for i,mat in enumerate(g.get('materials',[])):
  p=mat.get('pbrMetallicRoughness',{});r['materials'].append({'name':mat.get('name','Material_'+str(i)),'factor':p.get('baseColorFactor',[1,1,1,1]),'metallic':p.get('metallicFactor',0),'roughness':p.get('roughnessFactor',.6),'alpha':mat.get('alphaMode','OPAQUE'),'doubleSided':mat.get('doubleSided',False)})
 d['models'].append(r)
(base/'ModelSources.json').write_text(json.dumps(d,indent=2),encoding='utf-8')
