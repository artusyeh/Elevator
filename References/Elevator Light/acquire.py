from pathlib import Path
import urllib.request,json,concurrent.futures
root=Path(__file__).resolve().parents[2];folder=root/'Assets/Building/Models/Caged Fluorescent';folder.mkdir(parents=True,exist_ok=True)
def get(url):return urllib.request.urlopen(urllib.request.Request(url,headers={'User-Agent':'Unity free environment asset acquisition'}),timeout=60).read()
j=json.loads((root/'References/Elevator Light/Files.json').read_text(encoding='utf8'))
files=[(j['fbx']['2k']['fbx']['url'],folder/'Caged Fluorescent.fbx')]
g=j['gltf']['2k']['gltf'];files.append((g['url'],folder/'Source.gltf'))
for name,item in g['include'].items():files.append((item['url'],folder/name))
def save(item):
 url,path=item;path.parent.mkdir(parents=True,exist_ok=True)
 if not path.exists():path.write_bytes(get(url))
 return str(path.relative_to(root))
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool: paths=list(pool.map(save,files))
info=json.loads(get('https://api.polyhaven.com/info/caged_hanging_light'))
(root/'References/Elevator Light/Source.json').write_text(json.dumps(dict(asset='Caged Hanging Light',source='https://polyhaven.com/a/caged_hanging_light',license='CC0-1.0',authors=info.get('authors',{}),files=paths),indent=2),encoding='utf8')
preview='https://cdn.polyhaven.com/asset_img/primary/caged_hanging_light.png?height=800&quality=95&v=2fc9ad11'
(root/'References/Elevator Light/Source Preview.png').write_bytes(get(preview))
print('Free CC0 fixture FBX, original PBR maps and license saved.')
gltf=json.loads((folder/'Source.gltf').read_text())
print('Nodes:',json.dumps(gltf.get('nodes',[]))[:1200])
print('Mesh count:',len(gltf.get('meshes',[])))
