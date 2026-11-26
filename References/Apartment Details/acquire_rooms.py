from pathlib import Path
import json,urllib.request,concurrent.futures
root=Path(__file__).resolve().parents[2];base=root/'Assets/Building/Models/Interiors';base.mkdir(parents=True,exist_ok=True)
def get(u):return urllib.request.urlopen(urllib.request.Request(u,headers={'User-Agent':'Free environment asset acquisition'}),timeout=90).read()
def ph(name):
    info=json.loads(get('https://api.polyhaven.com/info/'+name));data=json.loads(get('https://api.polyhaven.com/files/'+name));item=data['gltf']['1k']['gltf'];folder=base/name;folder.mkdir(exist_ok=True)
    (folder/(name+'.gltf')).write_bytes(get(item['url']))
    for rel,f in item['include'].items():
        p=folder/rel;p.parent.mkdir(parents=True,exist_ok=True)
        if not p.exists():p.write_bytes(get(f['url']))
    g=json.loads((folder/(name+'.gltf')).read_text())
    def uri(i):return g['images'][g['textures'][i['index']]['source']]['uri'] if i else ''
    mats=[]
    for m in g.get('materials',[]):
        p=m.get('pbrMetallicRoughness',{});mats.append(dict(name=m['name'],color=uri(p.get('baseColorTexture')),normal=uri(m.get('normalTexture')),arm=uri(p.get('metallicRoughnessTexture')),factor=p.get('baseColorFactor',[1,1,1,1]),metallic=p.get('metallicFactor',1),roughness=p.get('roughnessFactor',1)))
    print('Downloaded',name,flush=True);return dict(asset=name,source='https://polyhaven.com/a/'+name,license='CC0-1.0',authors=info.get('authors',{}),materials=mats)
def tevy(name):
    folder=base/name;folder.mkdir(exist_ok=True);(folder/(name+'.glb')).write_bytes(get('https://tevy.dev/models/'+name+'/lod0.glb'));print('Downloaded',name,flush=True)
    return dict(asset=name,source='https://tevy.dev/models/'+name,license='CC0-1.0',materials=[])
jobs=[(ph,n) for n in ['old_bed_frame','chinese_tea_table','painted_wooden_chair_02','Shelf_01']]+[(tevy,n)for n in ['washing-machine','refrigerator','wardrobe','laundry-basket','wooden-bed','wooden-chair']]
with concurrent.futures.ThreadPoolExecutor(max_workers=5) as pool:
    futures=[pool.submit(fn,n)for fn,n in jobs];records=[f.result()for f in futures]
(base/'Sources.json').write_text(json.dumps({'models':records},indent=2),encoding='utf8')
