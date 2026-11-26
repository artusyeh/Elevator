from pathlib import Path
import urllib.request, json, concurrent.futures

root=Path(__file__).resolve().parents[2]
base=root/'Assets/Building/Models/Common Areas'
def get(url):
    return urllib.request.urlopen(urllib.request.Request(url,headers={'User-Agent':'Free Unity environment asset acquisition'}),timeout=90).read()
def download(asset):
    info=json.loads(get('https://api.polyhaven.com/info/'+asset))
    data=json.loads(get('https://api.polyhaven.com/files/'+asset))
    res='1k'
    file=data['gltf'][res]['gltf']
    folder=base/asset;folder.mkdir(parents=True,exist_ok=True)
    (folder/(asset+'.gltf')).write_bytes(get(file['url']))
    for name,item in file['include'].items():
        p=folder/name;p.parent.mkdir(parents=True,exist_ok=True)
        if not p.exists():p.write_bytes(get(item['url']))
    g=json.loads((folder/(asset+'.gltf')).read_text())
    def uri(item):return g['images'][g['textures'][item['index']]['source']]['uri'] if item else ''
    materials=[]
    for m in g.get('materials',[]):
        p=m.get('pbrMetallicRoughness',{})
        materials.append(dict(name=m['name'],color=uri(p.get('baseColorTexture')),normal=uri(m.get('normalTexture')),arm=uri(p.get('metallicRoughnessTexture')),factor=p.get('baseColorFactor',[1,1,1,1]),metallic=p.get('metallicFactor',1),roughness=p.get('roughnessFactor',1),alpha=m.get('alphaMode','OPAQUE'),doubleSided=m.get('doubleSided',False)))
    print('Downloaded',asset,flush=True)
    return dict(asset=asset,source='https://polyhaven.com/a/'+asset,license='CC0-1.0',authors=info.get('authors',{}),materials=materials)
assets=['modular_pipes','modular_electric_cables','chinese_stool','rubber_boots']
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:records=list(pool.map(download,assets))
base.mkdir(parents=True,exist_ok=True)
(base/'Sources.json').write_text(json.dumps({'models':records},ensure_ascii=False,indent=2),encoding='utf8')
(root/'References/Apartment Details/Sources.json').write_text(json.dumps({'models':records},ensure_ascii=False,indent=2),encoding='utf8')
