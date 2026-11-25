# Alley — first playable environment study

## Source status
Four user-supplied JPG attachments are archived in ../Concepts. The three requested named Tokyo concept PNGs were not found in the project, Desktop, Downloads, Pictures or attachment folders searched. Exact matching to those files remains pending. The supplied brief is context; current authorization is a first playable street blockout with one more developed corner, not horror gameplay.

## Reading the attached images
Image 1: low two-story plaster-and-timber houses, shallow tiled eaves, a shuttered business, exposed green downpipe, thin balcony railing, narrow raised footway and roadside guardrails. Tree and garden gate add silhouette variation. Image 2: asymmetric bend, mixed two/three/four-story heights, short facade bays, individual awnings and a taller building closing the vista. Both daylight views are architectural references; their sunlight is not the target night lighting.

Image 3: intimate pedestrian-scale restaurant lane, warm paper lanterns and gridded illuminated glazing, deep eaves, staggered projecting signs, stools and AC condensers. Foreground framing is strongest at the edges, midground signs lead the eye, background turns opaque. Avoid copying its continuous entertainment-strip density across a residential neighborhood. Image 4: wider mixed-use lane, thin painted edge lines, overhead cable layers, small guardrails, tiled facades, elevated outdoor AC condensers, glowing vending display. The vending machine is set against the frontage, not in the circulation path. Upper windows and uneven building tops leave a ribbon of sky.

## Working dimensions (estimates, not measured from photographs)
- Street route: 52 m; approximately 5.3 m between painted edges, 6.2 m gutter-to-gutter; frontage and curb offsets form two subtle bends. Prop clusters reduce local clear space.
- Narrow side passage: approximately 1.6–1.75 m clear. Facade bays 4.5–7 m wide.
- Floor-to-floor: 2.8 m, ground shops 3.0 m, buildings 6–11.5 m.
- Doors: 0.9 x 2.1 m. Vending cabinet: 1.05 x 0.75 x 1.85 m.
- Eye height 1.65 m; walking 2.2 m/s; capsule radius 0.28 m.
- Prop clusters occupy 0.4–1 m of frontage depth; keep roadway traversable.

## Composition and material targets
Near: closed sushi shop, weathered shutter, dark wood, warm lantern and vending light, bicycle silhouettes, curb drainage. Mid: staggered residential entrances, balconies, AC units, potted plants, service pipes. Far: apartment entrance, side-turn occlusion and gaps to panorama. Three selected first-person views test both directions and the vending recess.

Plaster should be slightly mottled rather than distressed ruin; shutters need thin horizontal ribs; metal needs muted specular highlights. Asphalt uses a CC0 photo surface with damp smoothness; puddle and weathering masks remain refinement work. Warm lamps ~2700–3200 K, vending display ~5500–6500 K, cool blue night ambient. Preserve darkness between pools of light; avoid saturated cyberpunk signage. Lights are placed on actual fixtures.

## Background contract
Sky, clouds, stars, distant skyline and Tokyo Tower must remain one seamless 2:1 equirectangular skybox. Initial original procedural panorama is a composition placeholder, not a photograph or final Tokyo reconstruction. No distant tower/building GameObjects. Gaps above low roofs and at the turn reserve skyline views. A licensed photographic/artist-authored final panorama is still needed.

## Authenticity references (viewing only; no texture reuse)
- https://lisaknightcreative.com/portfolio/shinbashi — pipes/AC, vending recesses, bicycles against walls.
- https://note.com/onushi_chan/n/na94c7c48a641 — residential nighttime scale and localized vending light.
- https://itabashi-times.blog.jp/archives/sanshouuo.html — sparse residential/izakaya mix, bicycles and lantern.
Downloaded reference photos retain their original rights and are NOT imported into Assets.

## Production inventory
| Asset family | Production route | First-stage state / next work |
|---|---|---|
| Asphalt / plaster / concrete | Free CC0 Poly Haven photo textures | Asphalt and plaster imported; improve normal/roughness calibration |
| Street, curbs, gutters, junction | Modular Unity mesh geometry | Editable blockout; add bevels, drains and junction markings |
| 2–4 story facades | Modular geometry, later Blender bevels | Individual bay/height/material variation; refine construction joints |
| Sushi facade and closed shutter | Modular geometry + authored Japanese sign texture | Detailed study; refine fittings, closure notices, signage kerning |
| Timber frames / glazing / balconies | Modular geometry | Reusable study modules; add glass depth and interior curtains |
| Roofs / eaves / downpipes | Modular geometry, Blender for curved fittings | Silhouette study; tile and elbow detail needed |
| Vending machine | Original modular prefab | Emissive drink display; placeholder product art and cabinet detail |
| City bicycles | Blender modeling recommended | Geometric stand-ins; spokes, basket, chain, brake cables need refinement |
| AC condensers / meters / utility boxes | Modular prefab then Blender bevel pass | Instanced study assets; fans/grilles represented simply |
| Pole hardware / overhead wires | Cylinders and sampled sagging curves | Plausible placement; refine transformers/insulators and cable attachment |
| Garbage bags / collection net / bins | Blender sculpt/mesh and CC0 materials | Geometric bags/bins; net and localized collection notices pending |
| Crates / cardboard / plant pots | Original reusable geometry | Placeholder forms; add labels, corrugation and foliage |
| Manholes / drain grilles | Geometry + authored roughness/decal masks | Study geometry; replace with detailed modular assets |
| Road paint / damp edge / grime | Authored textures and decals | Edge lines present; localized masks and breakup pending |
| Night panorama / Tokyo Tower | Original or properly licensed equirectangular artwork | Procedural stand-in; final realistic skyline pending |
| First-person controller | Original C# / existing Input System | Walk/look/sprint, collision and cursor release; no interaction systems |
| Audio | Free CC0 recordings, later | Not implemented in this visual study |
| AI generation | Local-only if suitable installed model exists | Ollama text models found; no verified local image/3D generator; no paid API |

## Tool and license notes
Unity 6000.0.60f1, URP 17.0.4 retained. Unity MCP connected. Blender MCP not reachable on 9876. No purchase, paid API, paid model or unverified existing asset pack is used. Original procedural geometry/art can be edited freely in this project. Third-party CC0 sources listed in Assets/Alley/Textures/SOURCES.txt.
