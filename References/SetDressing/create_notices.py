from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import numpy as np
root=Path(__file__).resolve().parents[2];out=root/'Assets/Alley/Textures/Notices';out.mkdir(exist_ok=True);rng=np.random.default_rng(27)
font='C:/Windows/Fonts/YuGothR.ttc';bold='C:/Windows/Fonts/YuGothB.ttc'
cards={
'Collection Schedule':('ごみ収集日',['青葉町二丁目 集積所','可燃ごみ　月・木','不燃ごみ　第2・第4水','資源ごみ　土','当日の朝8時までに','ネットの下へ出してください','青葉町自治会']),
'Missing Cat':('迷い猫を探しています',['名前：あお　茶トラ・白い胸','青い首輪をしています','見かけた方は管理人室まで','青葉荘 管理人']),
'Neighborhood Notice':('町内会のお知らせ',['夜間はお静かにお願いします','路地は住民の生活道路です','ご協力ありがとうございます','青葉町自治会']),
'Security Notice':('防犯カメラ作動中',['この建物は録画中です','青葉荘 管理室']),
'No Parking':('駐輪禁止',['出入口につき','駐輪はご遠慮ください','青葉荘']),
'Apartment Nameplate':('青葉荘',['管理人室　1階','ご用の方は呼出ボタンを','押してください']),
'Street Address':('青葉町二丁目',['7番12号']),
'Business Hours':('寿司 みなと',['営業時間　17:00–22:30','定休日　水曜日','本日の営業は終了しました']),
'Shoe Repair':('靴修理 あおい',['かかと修理・合鍵','ていねいにお直しします','駅前通り']),
'Festival':('青葉町 夏まつり',['七月二十五日','午後5時〜8時','会場：青葉公園','青葉町商店会']),
'Menu':('お品書き',['にぎり盛合せ　1,200円','ちらし寿司　1,400円','鉄火巻　　　 500円','味噌汁　　　 200円','持ち帰り承ります']),
}
for name,(title,lines)in cards.items():
 w,h=(1024,640) if name=='Street Address' else (768,1024)
 noise=rng.normal(0,2,(h,w,1));paper=np.clip(np.array([223,216,190])+noise,0,255).astype('uint8');im=Image.fromarray(paper);d=ImageDraw.Draw(im)
 red=(110,38,31);ink=(44,45,40);d.rectangle((23,23,w-24,h-24),outline=(83,83,65),width=3)
 fs=min(64,int((w-70)/len(title)));d.text((w/2,90),title,font=ImageFont.truetype(bold,fs),fill=red,anchor='mm');d.line((45,150,w-45,150),fill=red,width=3)
 y=220
 if name=='Missing Cat':
  d.ellipse((260,210,500,435),fill=(145,102,55));d.polygon([(260,280),(260,185),(330,230)],fill=(145,102,55));d.polygon([(440,230),(505,185),(500,280)],fill=(145,102,55));d.ellipse((315,295,331,312),fill=ink);d.ellipse((429,295,445,312),fill=ink);d.line((320,405,450,405),fill=(30,70,116),width=18);y=510
 for line in lines:
  size=min(43,int((w-90)/max(1,len(line))));d.text((w/2,y),line,font=ImageFont.truetype(font,size),fill=ink,anchor='mm');y+=90
 im.save(out/(name+'.png'))
# Cloth artwork for noren; original Japanese text and woven noise.
im=Image.new('RGB',(1536,512),(59,47,39));d=ImageDraw.Draw(im);d.text((768,256),'寿司　みなと',font=ImageFont.truetype(bold,190),fill=(209,196,158),anchor='mm');im.save(out/'Noren.png')
print('Eleven fictional Japanese notices and noren artwork created.')
