from pathlib import Path
import urllib.request,json,concurrent.futures
root=Path('F:/桌面文件/Elevator');review=root/'References/Service Shelf';review.mkdir(parents=True,exist_ok=True)
def get(url):
    return urllib.request.urlopen(urllib.request.Request(url,headers={'User-Agent':'Unity free environment asset acquisition'}),timeout=60).read()
records=[]
for asset,name in [('screwdriver','Screwdriver'),('pliers','Pliers')]:
    folder=root/'Assets/Building/Models'/name;folder.mkdir(parents=True,exist_ok=True)
    files=json.loads(get('https://api.polyhaven.com/files/'+asset));info=json.loads(get('https://api.polyhaven.com/info/'+asset));g=files['gltf']['2k']['gltf']
    paths=[(g['url'],folder/'Source.gltf')]+[(v['url'],folder/k) for k,v in g['include'].items()]
    def save(item):
        url,path=item;path.parent.mkdir(parents=True,exist_ok=True)
        if not path.exists():path.write_bytes(get(url))
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(save,paths))
    records.append(dict(asset=asset,name=name,source='https://polyhaven.com/a/'+asset,license='CC0-1.0',authors=info.get('authors',{}),files=[str(p.relative_to(root)).replace('\\','/') for _,p in paths]))
(review/'Sources.json').write_text(json.dumps(records,indent=2),encoding='utf8')
print('Downloaded original CC0 screwdriver and pliers models and 2K PBR textures.')
