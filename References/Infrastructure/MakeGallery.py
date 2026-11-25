from pathlib import Path
from urllib.parse import quote
from html import escape
from PIL import Image, ImageDraw, ImageFont

root=Path('References/Infrastructure')
items=[('Mailboxes After.png','Thirty units · aged finish and larger printed numbers'),('Live Workstation.png','Four original CRTs showing independent live feeds'),('Corridor Camera.png','Supported corridor surveillance camera'),('Entrance Camera.png','Entrance surveillance installation'),('Electrical Room.png','Existing maintenance space converted to electrical room'),('Breaker Panel.png','Main and fourteen branch breakers'),('Breaker Interaction.png','The shared reticle and contextual switch prompt'),('Residential Power On.png','Third-floor corridor · normal lighting on'),('Residential Power Off.png','Same view · circuit off, backup supply retained'),('Live Channels.png','Coverage review · raw camera captures; CRT shader treatment shown above')]
names=['Entrance','Lobby','2F Corridor','3F Corridor','4F Corridor','5F Corridor','6F Corridor','Lift Car']
coverage=Image.new('RGB',(768,4*282),(18,20,18));draw=ImageDraw.Draw(coverage)
for i,name in enumerate(names):
 x=(i%2)*384;y=(i//2)*282;coverage.paste(Image.open(root/(name+' Feed.png')),(x,y+22));draw.text((x+8,y+3),name,fill='white')
coverage.save(root/'Live Channels.png')
cards=''.join(f'<figure><a href="{quote(file)}"><img loading="lazy" src="{quote(file)}" alt="{escape(title)}"></a><figcaption>{escape(title)}</figcaption></figure>' for file,title in items)
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Building Review</title><style>body{margin:0;background:#121713;color:#e2e4dc;font:16px/1.6 system-ui,sans-serif}main{max-width:1400px;margin:auto;padding:32px}h1{font-weight:500}a{color:#c9d7c3}section{display:grid;grid-template-columns:repeat(auto-fit,minmax(360px,1fr));gap:24px}figure{margin:0;background:#1c241e;border:1px solid #344237}img{width:100%;display:block}figcaption{padding:12px 16px}.note{max-width:900px;color:#b7c3b5}.links{display:flex;gap:20px;flex-wrap:wrap;margin-bottom:30px}</style><main><h1>Building Review</h1><p class="note">Saved Unity renders. Mailboxes match six units on each residential floor 2–6. The building has live CCTV, physical circuit controls, independent emergency lighting and a contextual interaction reticle.</p><div class="links"><a href="Readme.txt">Implementation and assets</a><a href="Final%20Validation.txt">Final checks</a><a href="Functional%20Checks.txt">Detailed functional checks</a><a href="../Interaction/Readme.txt">Reticle configuration</a><a href="../Interaction/Interaction.png">Reticle · interaction</a><a href="../Interaction/Idle.png">Reticle · idle</a></div><section>'''+cards+'''</section><p class="note">Power/channel selections persist through normal scene transitions. The Alley preview reports NO SIGNAL for upper-floor interiors that are loaded in Apartment. Cameras render only selected feeds at low resolution, staggered across frames. No paid assets, services or horror/story mechanics were added.</p></main></html>'''
(root/'Gallery.html').write_text(html,encoding='utf-8')
thumbs=['Mailboxes After.png','Live Workstation.png','Corridor Camera.png','Electrical Room.png','Residential Power On.png','Residential Power Off.png']
sheet=Image.new('RGB',(1280,3*425),(18,23,19));d=ImageDraw.Draw(sheet);font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',20)
for i,file in enumerate(thumbs):
 x=(i%2)*640;y=(i//2)*425;im=Image.open(root/file).convert('RGB');im.thumbnail((640,395));sheet.paste(im,(x+(640-im.width)//2,y+30));d.text((x+12,y+3),file.removesuffix('.png'),font=font,fill=(222,230,220))
sheet.save(root/'Review.jpg',quality=92)
print('Gallery and screenshot contact sheet written.')
