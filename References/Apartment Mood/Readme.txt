Apartment public-space lighting — 25 November 2026

The supplied corridor reference is the visual target: cold desaturated cyan-green fluorescent light, dark ceilings, readable wall/door edges, localized pools and quiet institutional atmosphere. Reference.png is retained outside Assets. Five matching before/after views show the lobby, second-floor corridor, sixth-floor corridor, elevator approach and management-room contrast.

Implemented
Retuned the eighteen existing lobby/corridor ceiling fittings. Existing downward spots now have cool fluorescent color, realistic falloff within a 4.35 m range, soft shadows and broad soft-edged beams that reach the walls. Mild brightness differences between the three fixture positions avoid an identical repeated light pattern. Their existing near-fixture spill sources are confined to 3.25 m and do not cast extra point-light shadows. The first iteration was too dark on the upper walls and was revised after comparing the rendered images. The final beams spread onto the walls while preserving darker door recesses and gaps between sources.

Public fluorescent covers use a separate cyan-neutral emissive material. Existing elevator-entry fixtures and public backup bulkheads now have matching cooler light and lens emission. Cabin lighting, stairwell fixtures and management-room light settings are retained. No new hidden fill sources or light components were added. The source remains the existing physical fitting at every edited position.

Reduced the outdoor-level generic ambient contribution in Apartment and shifted it toward a subdued blue-green neutral. The old warm baked-light assignments and probe contribution were removed from public hallway renderers so those surfaces respond to the current fixtures and electrical state. Management-room renderer lighting settings were retained. This pass uses realtime fixtures and controlled ambient rather than an outdated bake; it does not claim a newly baked GI solution.

Post-processing remains restrained: fixed post exposure +0.55 EV (previously +0.90), contrast +7, saturation -8, white-balance temperature -9 and tint -5, bloom 0.045 above threshold 1.2, vignette 0.10. Existing ACES tonemapping retained. No automatic adaptation, fog, chromatic aberration, grain or dramatic horror filter was added. The external Alley grading asset and the exterior local exposure volume were preserved.

Management contrast
The existing management ceiling source remains at 4400 K, with its original intensity. The desk lamp remains 2850 K, intensity 1.2. Its pool of warm light is visibly different from the cyan public hall through the reception window. The modest global interior grade affects both spaces, but the contrast comes chiefly from their different actual sources.

Preservation
No architecture, model placement, player movement or camera field-of-view settings were changed. The scene has the same 7,933 Transform records. The only changed Transform record in the before/after scene comparison belongs to the existing live mirror's reflection camera, which the capture helper positions while rendering; the room/prop transforms are unchanged. No new lights were added. Shared Residential Corridor, Entrance Interior and Corridor Light prefabs retain their functional circuit references and now carry the public fixture settings. ResidentialFixtures calls the mood helper so future fitting updates retain the treatment.

Files
Assets/Building/Editor/ApartmentMoodPass.cs
Assets/Apartment/Materials/Public Fluorescent Cover.mat
Assets/Apartment/Materials/Public Entrance Cover.mat
Assets/Apartment/Materials/Public Backup Cover.mat
Assets/Apartment/Materials/Apartment Post Processing.asset
Assets/Scenes/Apartment.unity and the three shared lighting/interior prefabs.

All settings remain editable in Unity. The render pipeline and shadow atlas size were not changed. No assets or services were purchased. Existing textures and fixture models were reused.

Verification
Matching after views were rendered from the reloaded scene and again in Play Mode. All six public floor-lighting circuits passed lights-off, fixture-emission-off and restoration checks. The existing elevator circuit also passed. A live CCTV feed rendered successfully. No Console warnings/errors appeared in the final Play Mode batch; no shadow-atlas warning appeared. Public renderer assignments no longer retain the old bake after reload. Geometry Checks.json records the Transform comparison; Runtime Checks.txt records power/CCTV/grade checks.

No restart required. No commit or push performed. The scene and post-processing/lighting assets were saved.
