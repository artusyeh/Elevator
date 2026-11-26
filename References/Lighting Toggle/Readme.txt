Two apartment lighting moods — 25 November 2026

In Play Mode, press L to switch Normal / Horror. Each press toggles once; holding
the key does not cycle. Normal is the default for a fresh Play/game session.
The selected mood persists when leaving Apartment for Alley and returning.
The L control operates in Apartment; the outdoor Alley environment is preserved.
Menus, document inspection, released cursor and other existing interaction-input
gates prevent the temporary shortcut from triggering while gameplay is inactive.

Both looks are saved, not destructively replaced:
Assets/Building/Lighting/Normal Lighting.asset
Assets/Building/Lighting/Horror Lighting.asset
Assets/Building/Lighting/Normal Post Processing.asset
Assets/Building/Lighting/Horror Post Processing.asset

The normal fixture values and diffuser materials were recovered from the scene
snapshot taken before the cyan mood pass. Original ambient and grading values
were restored from the recorded inspection. The original baked renderer bindings
were recovered by restoring their GI settings and reloading the authoritative
scene against its existing lighting data. Normal restores 294 baked bindings;
Horror disables those bindings and uses the recent cold fixture lighting.
No old scene geometry was restored or substituted.

The controller is on Apartment / Lighting. Inspector settings include Starting
Mood, Enable Keyboard Toggle, Toggle Key, both presets and explicit scene
bindings. Inspector context-menu commands preview Normal and Horror in Edit
Mode. Future game progression can call SetHorror(true/false) or SetMood without
simulating a key press. No day progression or horror events have been added.

Switching updates the same 53 affected existing lights, 30 fixture covers,
ambient colors, post-processing profile and surface-lighting assignments. It
never changes a Light's enabled state. LightCircuit.RefreshEmissionSurfaces
recaches the selected diffuser values and reapplies current electrical power,
so a mood switch cannot turn off breakers back on or make unpowered tubes glow.
Original enable-state caches and non-emitter material property blocks remain
owned by their existing systems.

Normal and Horror post-processing assets own independent component subassets.
The old Apartment profile referenced the Alley profile's components. These are
now separated for the toggle and the Alley profile retains its original grade.
The editor mood helper also ensures future profile edits own their components
instead of silently changing another profile. Both presets remain editable.

Validation
Runtime Checks.txt records actual Input System L press, hold and second-press
behavior; 20 repeated toggles; exact lamp/material/lightmap/profile restoration;
unchanged preset assets; menu/document/cursor gates; all six public floor
circuits and emergency circuit remaining off, with unlit emitters, across mood
switches; existing power state restoration; Apartment -> Alley -> Apartment
selection persistence without duplicate controllers; and Normal restored in a
fresh Play session. Final raw captures show both rendered looks in Play Mode.
Final Console batch: no warnings or errors.

The scene is saved in Normal mode. Existing layout, models, elevator, CCTV,
stairwell key and controls are preserved. Both moods use the existing render
pipeline and shadow budget. No paid assets/services, no commit or push, and no
restart required. Gallery.html contains side-by-side captures.
