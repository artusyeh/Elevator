from pathlib import Path
import json,urllib.request,concurrent.futures,hashlib
root=Path(__file__).resolve().parents[1];catalog=json.loads((root/'References/material-catalog.json').read_text(encoding='utf-8'))
out=root/'Assets/Alley/Textures/PBR';out.mkdir(exist_ok=True)
assets={'Concrete':'concrete_wall_006','Plaster':'painted_plaster_wall','Tiles':'square_tiled_wall','Blocks':'concrete_block_wall','Asphalt':'asphalt_02','Corrugated':'corrugated_iron','Steel':'rusty_metal_02','Painted Metal':'painted_metal_shutter','Wood':'weathered_brown_planks','Plastic':'weathered_pvc_tank','Rubber':'rubber_tiles'}
def request(url):return urllib.request.urlopen(urllib.request.Request(url,headers={'User-Agent':'Mozilla/5.0'}),timeout=40).read()
def collect(pair):
 name,asset=pair
 try:
  if asset not in catalog:
   if name=='Wood':asset='wooden_planks'
  data=json.loads(request('https://api.polyhaven.com/files/'+asset));folder=out/asset;folder.mkdir(exist_ok=True)
  rec={'category':name,'asset':asset,'source':'https://polyhaven.com/a/'+asset,'license':'CC0-1.0','authors':catalog[asset].get('authors'),'dimensions_mm':catalog[asset].get('dimensions',[300,300]),'maps':{}}
  for channel in ['Diffuse','nor_gl','arm','Rough','AO','Metal']:
   if channel not in data:continue
   formats=data[channel].get('2k',data[channel].get('1k',{}));file=formats.get('jpg',formats.get('png'))
   if not file:continue
   path=folder/(channel+Path(file['url']).suffix)
   if not path.exists():path.write_bytes(request(file['url']))
   rec['maps'][channel]={'path':str(path.relative_to(root)).replace('\\','/'),'url':file['url'],'sha256':hashlib.sha256(path.read_bytes()).hexdigest()}
  print(name,asset,len(rec['maps']),flush=True);return rec
 except Exception as e: print('FAILED',name,str(e),flush=True);return {'category':name,'error':str(e)}
with concurrent.futures.ThreadPoolExecutor(max_workers=5) as ex:results=list(ex.map(collect,assets.items()))
(out/'Sources.json').write_text(json.dumps(results,ensure_ascii=False,indent=2),encoding='utf-8')
