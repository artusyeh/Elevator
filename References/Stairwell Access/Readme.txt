Stairwell key and window update — 25 November 2026

Use
The brass key and split ring are on the security/caretaker desk, next to the logbook and phone. Aim at the key from within 1.9 m and press E at the Pick Up prompt. The existing camera-centered scanner and reticle handle the interaction. The key disappears from the desk when collected. The ordinary cabinet keys remain decoration and were not changed.

The collected key is a reusable stairwell master key. Approach any of the six stairwell access doors; the prompt changes from Locked to Unlock. Press E to unlock and open it. Press E again to close it; subsequent opens work normally. The key is not consumed. Doors remain locked in the saved scene until used with the key, preserving their configurable lock state.

BuildingKeys stores only the session's held key identifiers. The key persists through the existing Alley/Apartment scene transitions and the matching pickup in the Alley entrance preview hides once it has been collected. Returning to Apartment does not respawn it. Starting a new Play/game session clears the key and restores the saved pickup/locked doors. No save-game persistence, inventory HUD or additional story system was added.

Windows
All five upper-floor corridor end windows now use dark, dirty obscured glass. Each material has a distinct age pattern, low actual-scene transmission, broad frost blur, dusty edges and water streaks. The existing small outside lights are reduced. The stairwell glass uses a separate darker smoked preset, with much less milky haze and lower transmission, closer to the earlier dark appearance. The corrected connected frames and raised window elevations were preserved. No rooms, stair layout, player movement or elevator systems were redesigned.

Implementation
Assets/Building/Scripts/BuildingKeys.cs — session-held keys, reset at application/Play start.
Assets/Building/Scripts/KeyPickup.cs — existing IInteractionTarget adapter; hides collected copies.
Assets/Building/Scripts/StairAccessDoor.cs — optional matching Required Key Id permits unlocking; existing Tick/lock API retained.
Assets/Building/Prefabs/Stairwell Key.prefab — reusable pickup with the existing free CC0 brass Door Key model, original PBR materials and a fitted collider. The ring/model is seated 2 mm above the actual desk surface; no Rigidbody or permanent test objects.
Shared Entrance Interior preview, Stairwell, Stairwell Door, Residential Corridor and Corridor Window prefabs updated in place. GUIDs preserved. Authoring helpers preserve the two glass presets. The sourced model is the previously imported CC0 tevy Brass Door Key on Ring; no paid assets or new downloads were needed.

Verification
Runtime Checks.txt records actual camera-centered key detection, range rejection, obstacle occlusion, successful pickup, hidden key, five walked checkpoints from the security desk to the ground stair door, unlocking/opening, physical crossing and closing. The same held key passed opening/closing on all five upper-floor access doors. Alley and Apartment scene loads retained the key and did not respawn collected pickups; the returned Apartment lock could still open. A second Play session reset inventory and restored the pickup and all six locked doors. Saved Checks.txt verifies one scene pickup, 2 mm desk contact clearance, five obscured corridor panes and six default locked doors after a scene reload.

No Unity/Codex restart required. Play Mode was stopped after verification. Everything is free and editable in Unity.
