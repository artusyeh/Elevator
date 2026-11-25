from pathlib import Path
import urllib.request,json
root=Path(__file__).resolve().parents[2]
def get(url):return urllib.request.urlopen(urllib.request.Request(url,headers={'User-Agent':'Unity environment asset acquisition'}),timeout=60).read()
for asset in ['dirty_tiles','damaged_plaster','peeling_painted_wall']:
 d=json.loads(get('https://api.polyhaven.com/files/'+asset));folder=root/'Assets/Apartment/Textures'/asset;folder.mkdir(exist_ok=True)
 for key in ['Diffuse','nor_gl','arm']:
  p=folder/(key+'.jpg')
  if not p.exists():p.write_bytes(get(d[key]['2k']['jpg']['url']))
 print('Downloaded CC0 surface',asset,flush=True)
(root/'References/Apartment/Sources/Additional Surfaces.txt').write_text('CC0, Poly Haven: https://polyhaven.com/a/dirty_tiles and https://polyhaven.com/a/damaged_plaster. Original 2K Diffuse, Normal GL, ORM maps preserved. Green tint is an editable Unity material setting.\n')
