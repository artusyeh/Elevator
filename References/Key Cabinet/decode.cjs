const fs=require('fs'),path=require('path');
const create=require('C:/Users/PC/.codex/tools/ktxweb/libktx_read.js');
(async()=>{const k=await create({wasmBinary:fs.readFileSync('C:/Users/PC/.codex/tools/ktxweb/libktx_read.wasm')});
for(const f of JSON.parse(fs.readFileSync('References/Key Cabinet/TextureCache/Files.json','utf8'))){const t=new k.ktxTexture(new Uint8Array(fs.readFileSync(f)));t.transcodeBasis(k.TranscodeTarget.RGBA32,0);fs.writeFileSync(f+'.rgba',Buffer.from(t.getImage(0,0,0)));fs.writeFileSync(f+'.json',JSON.stringify({width:t.baseWidth,height:t.baseHeight}));t.delete();}console.log('Key textures decoded.');})();
