# Stage one — Alley environment study

Open `Assets/Scenes/Alley.unity`. Press Play, click Game View, WASD to walk, mouse to look, Shift to walk faster, Esc to release mouse. Exit Play Mode before editing. All environment geometry is saved in the scene; the runtime script only controls the player.

The original untitled scene was saved as `Assets/Scenes/PreAlleySetup.unity` before opening the new scene. SampleScene and render-pipeline assignments were left intact. Build settings were not updated.

## Implemented
52 m editable street with two shallow offsets, two-to-four-story facade modules, curb/gutter geometry, roadway edge lines, manholes, narrow side passage, closed sushi-shop study, apartment entrance, AC units/meters/downpipes, balconies, sagging overhead cables, utility poles and fixtures, vending/bicycle/bin prefabs, simple potted plants and boxes. Global URP volume, warm/cool practical lights, a baked reflection probe and a panoramic skybox are included.

The panorama is original procedural placeholder artwork. Sky, stars, distant building silhouettes and a tower silhouette are in its texture, with no skyline or tower geometry. Final photographic skyline, accurate Tokyo Tower lighting, subtle cloud work and seam QA remain future art work. Some initial views conceal the skyline behind the nearby architecture as intended.

## Validation
- C# compile: no errors reported.
- Missing scripts: zero. One player and one camera.
- 52 sampled center-route floor raycasts: no floor gaps after physics synchronization.
- Play Mode controller simulation: forward input for 120 x 1/60-second steps moved 4.40 m; grounded true.
- Five simulated seconds into east facade: collision stopped the capsule at x=2.75 m; no floor penetration.
- Automated keyboard injection did not move the player while Game View lacked focus. Keyboard/mouse feel still needs a human walk-through; deterministic tests exercise the same controller movement function.
- Four 1600 x 1000 camera renders saved in `References/Captures`, at eye height approximately 1.65–1.7 m. Scene left in Edit Mode.

## Art limits
This is an initial blockout with a more developed first corner, not final hyper-realistic art. Bicycles, vegetation, garbage bags, bins, cartons, vending products and the facade kit are geometric placeholders. Authentic roof details, bevels, window interiors, bespoke roughness/damp masks, decals, a final seamless photo panorama and performance optimization remain. Roughly 3,200 small mesh renderers prioritize editability; combine modules and add LODs before expanding the environment.

No horror systems, NPCs, elevator systems, interactions or story logic are implemented. No paid service was used. Blender MCP was unavailable, so scene construction used Unity Editor C# and the connected Unity MCP.

## Reference limitation
The three specifically named concept PNGs were not located. The four supplied JPG attachments were analyzed and archived. `Assets/Reference` appeared during work but contains empty category folders and Unity metadata only. Exact matching against the named primary concepts remains pending their availability. New viewing-only photos are stored outside Assets under `References/RealWorld`.

## Authoring tools
`AlleyBuilder.Build()` creates a fresh scene and refuses to overwrite an existing Alley scene. `AlleyReview.Refine()` applies the saved study refinements once. `AlleyBuilder.Capture()` refreshes the four review images, restoring the player camera afterwards. `AlleyReview.Validate()` checks floor coverage and scene components. The existing saved scene is the authoritative editable result; no regeneration is needed to use it.
