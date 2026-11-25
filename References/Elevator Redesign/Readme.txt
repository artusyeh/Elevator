Elevator interior visual redesign

Scene: Assets/Scenes/Apartment.unity. The Elevator Cabin, Elevator Landing and Entrance Interior shared assets are updated in place; Unity GUIDs and existing functional object references are retained. Alley has the matching entrance preview. The Editor is left in the saved Apartment scene, outside Play Mode.

Visual changes
- Nine individually finished folded metal wall panels replace the visible continuous pale wall surfaces. Original structural colliders and cabin dimensions remain in place. Faded olive finishes have different photographic texture offsets and tones; chips and corrosion concentrate at folds, fasteners and lower sections.
- Replaced the luminous ceiling slab with a pressed-steel twin-tube fluorescent fixture, ceramic end sockets, retaining clips and a yellowed protective cover. Neutral 5100 K realtime lighting is dimmer than the previous warm setup. Restrained lower fill approximates bounced light in the moving cabin; URP has no newly enabled baked or realtime GI system. The existing render pipeline and apartment lighting are unchanged.
- Added moulded 36 mm coin-pattern charcoal/green rubber flooring with worn tread, scuffs, peripheral dirt and photographic PBR variation. The moulded relief is shader based; existing floor/apron geometry and collision heights are preserved. Surface mapping is in object space so it travels with the cabin.
- The original planar mirror renderer, camera and reflection texture remain live. Its new material adds restrained wiping marks, fine scratches and irregular loss of silver at the edges. Frame and handrails use worn metal, with proper flanges, screws and rear supports.
- The control panel retains all original button interaction components, colliders, caps, queue behavior, open/close/alarm actions and red digital display. New rolled bezels, separate illuminated rings, printed button markings, aged brushed metal, localized finger wear, screws and an emergency speaker grille improve the physical appearance. Six floor numbers match the six serviced building levels. No new emergency-call gameplay or audio was invented.
- Added a guarded ventilation fan with blades, motor hub, wire cage and fasteners; a separate ceiling louver; fictional Japanese inspection/maintenance papers, capacity sign and manufacturer plate. These are editable mounted objects with individual texture assets. The latest request to include inspection papers supersedes the earlier removal of the old seam-crossing paper. The old notice was not restored.

Reference comparison
Primary target: Concept Reference.png, copied from the user's supplied original elevator image. It is outside Assets and never used as a final texture. Compared directly with the entrance, inside, mirror, controls and ceiling captures. The redesign follows its muted olive enamel, chipped joins, tarnished metal, aged fluorescent hardware, fan, printed papers and claustrophobic proportions. The cabin is cooler and dimmer than before, with readable physical detail. The game render remains more regular and less densely damaged than the photographic concept; no claim of exact photographic equivalence is made. Raw captures are unretouched.

Assets and cost
- Downloaded Blue Metal Plate and Metal Plate 02 from Poly Haven in 2K, CC0. Original albedo, OpenGL normal and ARM maps are preserved. ARM is repacked as Unity metallic/AO/smoothness with correct linear imports. See Sources.json and https://polyhaven.com/license .
- Reused existing CC0 Rubber Tiles and Weathered PVC Tank maps already in the project, plus the original CCTV model, mirror, player and lift infrastructure.
- Created fixture/fan/fastener/button meshes, shader finishes, deterministic Japanese labels and button graphics locally. Printed fonts are rasterized from the system; no font files are redistributed. No paid assets, paid APIs, image generation service, or new Blender dependency was used.
- All new materials, texture sources, geometry and authoring tools are editable in Unity. The coin tread and localized finish controls are shader parameters rather than destructive geometry.

Validation
Visual Checks.txt: both shaders supported, no shader compilation errors; button references intact; 4/4 framed signs backed by real wall colliders; no new finish meshes outside the existing cabin envelope; Alley mirror/CCTV preview retained.
Runtime Checks.txt: 9/9 camera-centred interaction rays; actual floor-six interaction and illuminated ring; full top-floor and ground-floor rides with door opening, exit and reentry; live cabin CCTV render; live mirror texture binding; elevator-circuit light/emission switching; independent emergency light; normal lighting restored.
Gallery.html: six player-height viewpoints, original concept and before/after comparisons. Floor includes a downward player-height view. Geometry envelope checks complement visual inspection; they are not a proof against every possible future collision after other edits.

Editor work
CabinVisuals.cs applies the visual finish without replacing functional cabin objects. CabinVisualChecks.cs records targeted checks. CabinRedesign.cs captures views. CabinWorkBridge.cs is an editor-only, project-local command helper used because Unity MCP tools were unavailable in this session; it accepts only public parameterless ApartmentBuilder methods. It is not included in runtime builds. Legacy builder/finish methods can overwrite curated content and should not be rerun over the finished scene.

No manual restart or setup steps remain for this redesign.

Final renderer validation: resolved a Unity local-keyword-space mismatch by embedding the project's existing shadow/depth passes directly in Cabin Surface instead of referencing the Lit shader with UsePass. Repeated actual runtime interaction, ride, mirror, CCTV and power checks after the fix; all passed. Final console has zero errors. Existing project-wide punctual-light shadow-atlas pressure still produces resolution-reduction warnings (212 shadow maps share the configured 4096 atlas); no global lighting/pipeline settings were changed during this cabin task. This remains a wider project rendering-performance/quality limitation, documented in Final Console.txt and Editor.log.
