# Shop-section material and detail pass

## Scope
Worked on approximately the first 15 m of the existing Alley, with some fittings at the 16 m boundary. Original building and road transforms were preserved. No street rebuild, pipeline change, horror mechanic, elevator, NPC or story system was introduced. The rest of the street retains its earlier level of detail.

## Acquired materials
Twelve Poly Haven CC0 sets were downloaded at 2K, with diffuse, OpenGL normal, AO/roughness/metal-packed maps, separate roughness and AO where available:

| Use | Asset |
|---|---|
| Concrete | concrete_wall_006 |
| Painted plaster | painted_plaster_wall |
| Glazed exterior ceramic | rounded_square_tiled_wall |
| Decorative skirting | square_tiled_wall |
| Concrete blocks | concrete_block_wall |
| Asphalt | asphalt_02 |
| Corrugated sheet | corrugated_iron |
| Rusted steel | rusty_metal_02 |
| Painted metal / shutters | painted_metal_shutter |
| Weathered wood | weathered_brown_planks |
| Plastic | weathered_pvc_tank |
| Rubber | rubber_tiles |

Cardboard002 was downloaded from ambientCG at 2K under CC0. Glass uses an original parameter-based URP material over the existing curtain surface; it is not a downloaded photo texture and does not simulate a modeled room or physical refraction.

Full source URLs, author information, dimensions and map hashes are recorded in `Assets/Alley/Textures/PBR/Sources.json`. Poly Haven dimensions in that file are millimeters. Corresponding source pages are `https://polyhaven.com/a/<asset>`; cardboard: https://ambientcg.com/view?id=Cardboard002 .

## Unity configuration
- Albedo imported as sRGB, normals as normal maps, masks/roughness/AO as linear data.
- Source ARM data converted into a Unity mask: R metallic, G AO, B roughness, A smoothness.
- World-space triplanar sampling gives surfaces a consistent meter scale without stretching primitive UVs. Shaders retain URP lighting, shadow, depth, AO, reflection and baked-GI support. Broad surfaces still need a later bespoke breakup pass before reproducing the kit across the entire street.
- Material variants distinguish pale plaster, aged concrete, ceramic-clad apartment fronts, dark wood and metal fittings. The heavier rust treatment is concentrated on older service hardware; painted and aluminum parts remain cleaner.

## Created locally
- Blender 5.3 background Python produced 86 dimension-specific beveled box meshes. 591 existing box mesh references in the shop section were replaced while retaining object transforms and collision volumes. Bevel radii are capped at 16 mm and scaled down for thin fittings. Editable source: `References/Blender/Architectural Edges.blend`; export: `Assets/Alley/Models/Architectural Edges.fbx`.
- Unity geometry adds window reveals, glazing seals, lintels, thin reflective glass, roof flashing, gutters, AC fan guards/hubs/blades, mounting plates/bolts, meter dials, door jambs/headers/mail slots, tiled skirting, drainage grates and a service cabinet in an existing recess.
- Left-side timber trims and right-side aluminum railings provide variation without moving buildings.
- Nine original weathering masks and reusable prefab overlays: water stains, dirt, rust, moss, mold, cracks, peeling paint, discoloration and ground stains. They are placed below windows/pipes, near damp plinths, shutter guides and drain edges. These are thin surface-aligned overlays, not URP DecalProjectors; they can be moved, scaled, duplicated and adjusted in Edit Mode.
- Damp asphalt retains the preceding lighting pass's normal/roughness variation and irregular wetness/puddle mask; new grates and edge staining add surface context.
- No generative AI API, paid service or paid model was used.

## Lighting carried forward
URP 17 remains active. The scene uses warm/neutral practical lights, saved ACES/exposure/bloom settings, SSAO, mixed direct lights with baked indirect GI, 117 light-probe positions and three local reflection probes. Shadowed sources are budgeted to avoid atlas exhaustion. The updated geometry/materials have been rebaked and reflections refreshed. This is probe-based reflection, not ray-traced or screen-space reflection.

Before/after lighting images are in `References/Captures/Lighting/Before` and `After`. Material/detail comparison images are in `References/Captures/Lighting/Surfaces` and `Final Surfaces`. Captures use the first-person camera at approximately 1.7 m eye height.

## Visual review against supplied references
Improved: surface grain, material contrast, warm/cool separation, framing thickness, hardware attachment, curb drainage and localized wear. The supplied JPGs inform layered frontage, restrained residential clutter and practical warm light. The three named primary PNG concepts were still unavailable in the searched project folders; exact comparison to those files remains pending.

Still artificial / next priorities:
- Bicycles remain geometric stand-ins with simplified spokes, chains, brakes and baskets.
- Potted plants remain angular leaf studies; replace them with properly modeled foliage.
- Vending products remain colored cylinders; cabinet controls, labels and printed packaging need a dedicated asset pass.
- Lanterns and shutters have better material/shading but still need bespoke meshes, graphics and fine mechanical detail.
- Glazing has curtain/reflection layers, but rooms behind windows are not modeled.
- Tokyo Tower remains a visibly soft enlarged photographic crop. Sharpening cannot recover absent source detail.
- Close architecture is improved but not yet finished photorealism. Custom roof joints, worn edge masks, tile corner returns and more precise Japanese fittings remain art work.
- The scene has many separate detail renderers. Static batching, shared detail meshes/LODs and a performance pass are needed before expanding this detail level across all 52 m.

## Verification
Zero original mesh transforms changed; no missing scripts or center-route floor gaps. No Console errors/warnings were reported in the last check. See `SurfaceValidation.txt` for counts. Original saved scene snapshots are outside Assets under `References/LightingBackup` and `References/SurfaceBackup`.
