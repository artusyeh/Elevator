from pathlib import Path
import urllib.request,json,concurrent.futures,time
ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/'Assets/Apartment/Models'
def get(url):
 for attempt in range(3):
  try:return urllib.request.urlopen(urllib.request.Request(url,headers={'User-Agent':'Unity environment asset acquisition'}),timeout=80).read()
  except Exception:
   if attempt==2:raise
   time.sleep(2)
def save(url,path):
 path.parent.mkdir(parents=True,exist_ok=True)
 if not path.exists():path.write_bytes(get(url))
def ph(asset):
 info=json.loads(get('https://api.polyhaven.com/info/'+asset))
 data=json.loads(get('https://api.polyhaven.com/files/'+asset))
 folder=BASE/asset;res='2k' if asset in ['metal_office_desk','Television_01'] else '1k'
 file=data['gltf'][res]['gltf'];save(file['url'],folder/(asset+'.gltf'))
 for name,item in file['include'].items():save(item['url'],folder/name)
 g=json.loads((folder/(asset+'.gltf')).read_text())
 def uri(item):return g['images'][g['textures'][item['index']]['source']]['uri'] if item else ''
 mats=[]
 for m in g.get('materials',[]):
  p=m.get('pbrMetallicRoughness',{})
  mats.append(dict(name=m['name'],color=uri(p.get('baseColorTexture')),normal=uri(m.get('normalTexture')),arm=uri(p.get('metallicRoughnessTexture')),emission=uri(m.get('emissiveTexture')),factor=p.get('baseColorFactor',[1,1,1,1]),metallic=p.get('metallicFactor',0),roughness=p.get('roughnessFactor',.6),alpha=m.get('alphaMode','OPAQUE'),doubleSided=m.get('doubleSided',False)))
 print('Downloaded',asset,flush=True)
 return dict(asset=asset,source='https://polyhaven.com/a/'+asset,license='CC0-1.0',authors=info.get('authors',{}),materials=mats)
assets=['mounted_fluorescent_lights','Television_01','metal_office_desk','desk_lamp_arm_01','wall_clock','binder_notebook','book_encyclopedia_set_01']
with concurrent.futures.ThreadPoolExecutor(max_workers=3)as pool:records=list(pool.map(ph,assets))
catalog=json.loads((ROOT/'References/SetDressing/tevy-models.json').read_text())['models']
for slug in ['office-chair','fire-extinguisher']:
 m=next(m for m in catalog if m['slug']==slug);assert m['license']=='CC0-1.0'
 save('https://tevy.dev'+m['files'][0]['path'],BASE/slug/(slug+'.glb'))
 records.append(dict(asset=slug,source='https://tevy.dev/models/'+slug,license=m['license'],materials=[]))
tex='terrazzo_tiles';d=json.loads(get('https://api.polyhaven.com/files/'+tex));folder=ROOT/'Assets/Apartment/Textures/Terrazzo'
for key in ['Diffuse','nor_gl','arm']:save(d[key]['2k']['jpg']['url'],folder/(key+'.jpg'))
(BASE/'Sources.json').write_text(json.dumps({'models':records},indent=2),encoding='utf-8')
(ROOT/'References/Apartment/Sources/Assets.json').write_text(json.dumps({'models':records,'textures':[{'asset':tex,'source':'https://polyhaven.com/a/'+tex,'license':'CC0-1.0','width_m':2}]},indent=2),encoding='utf-8')
print('Saved license records and all assets.',flush=True)
