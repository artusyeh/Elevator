from pathlib import Path
import urllib.request,json,concurrent.futures
root=Path(__file__).resolve().parents[2]
assets=['potted_plant_01','potted_plant_02','potted_plant_04','trashbag','plastic_crate_01','plastic_crate_02','cardboard_box_01','utility_box_01']
catalog=json.loads((root/'References/model-catalog.json').read_text())
def get(u):return urllib.request.urlopen(urllib.request.Request(u,headers={'User-Agent':'Mozilla/5.0'}),timeout=50).read()
def one(asset):
 d=json.loads(get('https://api.polyhaven.com/files/'+asset));res='2k' if 'plant' in asset else '1k';file=d['gltf'][res]['gltf'];folder=root/'Assets/Alley/Models/Street'/asset;folder.mkdir(parents=True,exist_ok=True)
 (folder/(asset+'.gltf')).write_bytes(get(file['url']))
 for name,f in file['include'].items():
  p=folder/name;p.parent.mkdir(parents=True,exist_ok=True)
  if not p.exists():p.write_bytes(get(f['url']))
 gltf=json.loads((folder/(asset+'.gltf')).read_text());materials=[]
 def uri(info):return gltf['images'][gltf['textures'][info['index']]['source']]['uri'] if info else ''
 for m in gltf.get('materials',[]):
  pbr=m.get('pbrMetallicRoughness',{});materials.append({'name':m['name'],'color':uri(pbr.get('baseColorTexture')),'normal':uri(m.get('normalTexture')),'arm':uri(pbr.get('metallicRoughnessTexture')),'alpha':m.get('alphaMode','OPAQUE'),'doubleSided':m.get('doubleSided',False)})
 record={'asset':asset,'source':'https://polyhaven.com/a/'+asset,'license':'CC0-1.0','authors':catalog[asset].get('authors',{}),'materials':materials}
 print(asset,'downloaded',len(materials),'materials',flush=True);return record
with concurrent.futures.ThreadPoolExecutor(max_workers=4)as ex:models=list(ex.map(one,assets))
(root/'Assets/Alley/Models/Street/ModelSources.json').write_text(json.dumps({'models':models},indent=2),encoding='utf-8')
