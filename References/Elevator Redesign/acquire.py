from pathlib import Path
import urllib.request,json,concurrent.futures
root=Path(__file__).resolve().parents[2]
def get(url):
    return urllib.request.urlopen(urllib.request.Request(url,headers={'User-Agent':'Free Unity elevator material acquisition'}),timeout=60).read()
def acquire(slug):
    folder=root/'Assets/Building/Textures/Elevator'/slug;folder.mkdir(parents=True,exist_ok=True)
    data=json.loads(get('https://api.polyhaven.com/files/'+slug));info=json.loads(get('https://api.polyhaven.com/info/'+slug))
    for key in ['Diffuse','nor_gl','arm']:
        path=folder/(key+'.jpg')
        if not path.exists():path.write_bytes(get(data[key]['2k']['jpg']['url']))
    return dict(asset=slug,source='https://polyhaven.com/a/'+slug,license='CC0-1.0',authors=info.get('authors',{}),resolution='2k',maps=['Diffuse','nor_gl','arm'])
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool: records=list(pool.map(acquire,['blue_metal_plate','metal_plate_02']))
(root/'References/Elevator Redesign/Sources.json').write_text(json.dumps(records,indent=2),encoding='utf8')
print('Downloaded two free CC0 PBR sets; source maps and license records saved.')
