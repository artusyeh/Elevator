import json,struct,urllib.request,concurrent.futures
from pathlib import Path
root=Path(__file__).resolve().parents[2];base=root/'Assets/Alley/Models/Street';catalog=json.loads((root/'References/SetDressing/tevy-models.json').read_text())['models']
slugs=['city-bicycle','cargo-bicycle','waste-bin','metal-bucket','traffic-cone','green-plastic-watering-can','drink-crate-stack','a-frame-chalkboard-sign','wash-bottle','foam-box-herb-planter']
def get(s):
 m=next(x for x in catalog if x['slug']==s);f=base/s;f.mkdir(exist_ok=True);p=f/(s+'.glb')
 if not p.exists():p.write_bytes(urllib.request.urlopen('https://tevy.dev'+m['files'][0]['path'],timeout=30).read())
 return {'asset':s,'source':'https://tevy.dev/models/'+s,'license':m['license'],'dimensions':m['dimensions']}
with concurrent.futures.ThreadPoolExecutor(max_workers=5)as ex:records=list(ex.map(get,slugs))
records.append({'asset':'commuter-bicycle','source':'https://www.innerscene.com/tools/library/3d-parts/city-bicycle-58a2a324','license':'CC0-1.0'})
for r in records:
 b=(base/r['asset']/(r['asset']+'.glb')).read_bytes();length=struct.unpack_from('<I',b,12)[0];g=json.loads(b[20:20+length]);r['materials']=[]
 for i,m in enumerate(g.get('materials',[])):
  p=m.get('pbrMetallicRoughness',{});r['materials'].append({'name':m.get('name','Material_'+str(i)),'color':'','normal':'','arm':'','alpha':m.get('alphaMode','OPAQUE'),'doubleSided':m.get('doubleSided',False),'factor':p.get('baseColorFactor',[1,1,1,1]),'metallic':p.get('metallicFactor',0),'roughness':p.get('roughnessFactor',.6)})
p=base/'ModelSources.json';d=json.loads(p.read_text());d['models']+=records;p.write_text(json.dumps(d,indent=2),encoding='utf-8')
print('Extra free models:',len(records))
