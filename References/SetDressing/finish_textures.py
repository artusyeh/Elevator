import json
from pathlib import Path
from PIL import Image
import numpy as np
root=Path(__file__).resolve().parents[2];base=root/'Assets/Alley/Models/Street';cache=root/'References/SetDressing/TextureCache';d=json.loads((base/'ModelSources.json').read_text())
for file in cache.glob('*.ktx2.rgba'):
 meta=json.loads(Path(str(file).replace('.rgba','.json')).read_text());name=file.name.removesuffix('.ktx2.rgba');asset,i=name.rsplit('_',1);im=Image.frombytes('RGBA',(meta['width'],meta['height']),file.read_bytes());im.save(base/asset/('Image'+i+'.png'))
for r in d['models']:
 for i,m in enumerate(r['materials']):
  if not m.get('arm') or not m['arm'].startswith('Image'):continue
  a=np.array(Image.open(base/r['asset']/m['arm']).convert('RGB'));o=np.zeros((*a.shape[:2],4),dtype='uint8');o[:,:,0]=a[:,:,2];o[:,:,1]=255;o[:,:,2]=a[:,:,1];o[:,:,3]=255-a[:,:,1];name='Mask'+str(i)+'.png';Image.fromarray(o).save(base/r['asset']/name);m['unityMask']=name
(base/'ModelSources.json').write_text(json.dumps(d,indent=2),encoding='utf-8');print('Texture PNGs and Unity masks saved.')
