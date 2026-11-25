const fs=require('fs');
const path=require('path');
const create=require('C:/Users/PC/.codex/tools/ktxweb/libktx_read.js');
(async()=>{
 const k=await create({wasmBinary:fs.readFileSync('C:/Users/PC/.codex/tools/ktxweb/libktx_read.wasm')});
 const cache=path.resolve('References/SetDressing/TextureCache');
 for(const file of JSON.parse(fs.readFileSync(path.join(cache,'Files.json'),'utf8'))){
  const t=new k.ktxTexture(new Uint8Array(fs.readFileSync(file)));
  const result=t.transcodeBasis(k.TranscodeTarget.RGBA32,0);
  const pixels=t.getImage(0,0,0);
  fs.writeFileSync(file+'.rgba',Buffer.from(pixels));
  fs.writeFileSync(file+'.json',JSON.stringify({width:t.baseWidth,height:t.baseHeight}));
  t.delete();
 }
 console.log('Decoded compressed model textures.');
})();
