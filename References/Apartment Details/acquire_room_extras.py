exec((__import__('pathlib').Path(__file__).with_name('acquire_rooms.py')).read_text().split('jobs=')[0])
records=json.loads((base/'Sources.json').read_text())['models']
for name in ['kitchen-base-unit-sink','cream-dome-kettle']:
    if not any(r['asset']==name for r in records):records.append(tevy(name))
(base/'Sources.json').write_text(json.dumps({'models':records},indent=2),encoding='utf8')
folder=root/'Assets/Building/Textures/Interiors';folder.mkdir(parents=True,exist_ok=True)
(folder/'Family Photograph.jpg').write_bytes(get('https://tile.loc.gov/storage-services/service/pnp/stereo/1s30000/1s31000/1s31500/1s31540v.jpg'))
(folder/'Photograph Source.txt').write_text('Library of Congress, Observing New Year\'s Day in a Japanese home, Japan (1905). https://www.loc.gov/pictures/item/2020634608/ Rights advisory: No known restrictions on publication. Used as an ancestral family photograph.',encoding='utf8')
