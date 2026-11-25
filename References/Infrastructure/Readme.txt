BUILDING SERVICES

Current scenes are Alley and Apartment. Both have one scene-owned Building Services object and one player-owned Interaction UI. No manager or UI uses DontDestroyOnLoad or automatic spawning. Power states and monitor channel choices persist as session data through the existing scene transition; they reset for a new play session. Both scenes and the shared visual entrance preview have been saved.

CCTV
Eight Apartment channels cover the entrance, lobby, residential corridors on floors 2–6, and moving elevator cabin. Camera heads/hoods reuse the existing Security Camera model. New supported mounts, lenses and cable connections remain editable. The cabin camera follows the actual moving cabin.

All four original scanned CRTs use actual RenderTextures on their existing fitted curved screen meshes. Aim at a screen or one of the first four labeled console buttons and press E to cycle its channel; the other displayed channels are skipped so four distinct views remain available. Grayscale/desaturation, light scanlines/noise, camera/location identification and a live JST timestamp are applied only to the monitor material. Old screenshot assets remain for rollback, but are not used as the live footage.

Normal scheduling renders at most one selected channel per player frame, targeting 6Hz per channel at 384x256. Cameras are disabled as automatic scene cameras and render explicitly without post-processing/shadow passes. Unselected channels are not refreshed. Each channel exposes Feed Enabled, Location Available, resolution and its power circuit. Switching off a camera/channel produces NO SIGNAL, not a frozen substitute. Switching off monitor power produces a dark screen.

Alley is still an exterior scene with a ground-floor preview. Its live entrance/lobby/cabin previews are connected; upper-floor channels honestly report NO SIGNAL because those interiors are loaded in Apartment. No fake upper-floor video is displayed.

POWER
Fourteen shared circuit assets live in Assets/Building/Circuits: Lobby, Security Room, Second/Third/Fourth/Fifth/Sixth Floor, Stairwell, Elevator, CCTV Cameras, CCTV Monitors, Entrance, Emergency, Maintenance. The main breaker controls normal supply. Emergency is an independent maintained backup circuit. Circuits, receivers and switches are editable and reference the same assets; runtime state is centralized in BuildingPower.

The existing narrow maintenance room beside the security room is now Electrical Room. Its doorway is fitted through the existing wall; the main street and room footprints were preserved. It contains aged PBR cabinets, a main breaker, fourteen branch breakers, analog voltage meters, connected conduits/junctions, Japanese electrical signage, spare ceramic fuses, a tool shelf, a fluorescent fixture and backup lighting. The old bucket and broom are grouped as service supplies. The door opens mechanically with E. Six local lighting switches also control their corresponding lobby/corridor circuits.

LightCircuit disables the actual lights and changes per-renderer emissive/base-color properties without editing shared materials. Controllable indoor lighting is realtime; old baked lightmap/probe illumination is removed from the controlled geometry so disabled lighting does not leave its baked contribution behind. The render pipeline, sky and exterior street layout are unchanged. Existing reflection-probe environments remain an approximation rather than a new dynamic GI solution.

The elevator rejects new floor requests without normal power. Loss of power during travel closes/interlocks the doors and uses battery rescue to Emergency Floor, configured to ground floor by default so the electrical room is reachable with stairs locked. It opens after reaching that real landing. If already stopped, it holds its doors open. Main supply restoration restores ordinary service. Backup cabin/landing lights remain available. No supernatural power failures or story triggers were implemented.

MAILBOXES AND NOTICES
Assets/Building/Apartments.asset is derived from the actual numbered apartment entrances: six units each on residential floors 2–6, thirty total (201–206 through 601–606). Ground floor is administrative and has no invented 101-series boxes. The shared Mailboxes prefab, Apartment bank and Alley preview match this directory. The bank is mounted with its rear at the wall, not buried in it. Brushed metal, varied tarnish/handling wear, proper slots/locks/hinges, and larger matte printed inserts replace the old high-contrast rust/bright text. Number inserts are about 2.5 times the earlier physical lettering size. Mailboxes have no new gameplay actions.

The pictured lobby notice board, building plan, calendar and key-return paper use the blank off-white material requested earlier. Original printed textures remain available. Newly required electrical labels and warning signs are separate assets.

REUSABLE ASSETS
Assets/Interaction/Prefabs/Interaction UI.prefab
Assets/Building/Prefabs/Distribution Panel.prefab
Assets/Building/Prefabs/Meter Cabinet.prefab
Assets/Building/Prefabs/Electrical Room.prefab
Assets/Building/Prefabs/Surveillance Camera.prefab
Assets/Apartment/Prefabs/Mailboxes.prefab and Elevator Cabin.prefab updated in place, preserving asset GUIDs.

No new third-party 3D downloads or paid services were used. Existing project CRT/camera/cabinet/fluorescent/utility props and free PBR materials were reused. Breaker/meter/mount hardware was modeled as editable Unity geometry. Identification plates, warning signs, voltage dial, mailbox PBR maps and wear/number textures were created locally. Pillow was installed as a free authoring dependency; Unity runtime/builds do not require Python or the system font files used to rasterize the plates. No horror/story/NPC systems were added.

VERIFICATION
Interaction/Checks.txt covers range, wall blocking, smooth morph, hiding gates, optional idle dot, and one reticle/scanner through scene transitions. Functional Checks.txt records all fifteen panel ray targets, individual residential circuits, restored lighting, emission response, physical CCTV console switching, four distinct monitors, and 3250 changed feed bytes after inserting/removing a temporary live 3D marker. Camera power stops camera rendering/produces NO SIGNAL; monitor power stops active feeds/darkens displays; master power turns off normal receiver lights while eight backup lights remain on. Electrical-room entry was tested with the CharacterController.

The latest safety result is in Final Validation.txt: a cut at cabin Y=3.023 returns to ground level 0 with no early door opening; the player exits there. Earlier nearest-floor tests in the detailed log precede this ground-floor safety refinement. The latest all-eight-channel renders pass. Individual feed CPU submission averaged 3.72ms, max 5.00ms in the final manual sample; this is not a claim of a fixed whole-game frame rate on every PC. Normal updates are staggered rather than rendering eight full-resolution views each frame.

Power/channel state, one manager/CCTV system/UI, and thirty mailboxes were verified through Apartment → Alley → Apartment. The background Editor was not advancing game frames, so one frame was explicitly stepped to clear the existing per-frame entrance debounce for the return test. Cabin Geometry Check.txt reports zero external mesh overlaps across the full elevator travel sweep including the new camera/backup fixture. Final Console errors: zero.

Gallery.html contains the workstation, physical cameras, electrical room/panel, on/off corridor comparison, reticle and mailbox screenshots. Backups and authoring scripts are in this folder. Construction/repair Editor methods are one-time tools; do not rerun old full scene builders over the finished scene. Continue by editing the current objects/prefabs/configuration.
