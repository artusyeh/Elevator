Player zoom

Hold the right mouse button to smoothly zoom from the existing 67-degree view to 40 degrees. Release it to restore the normal view. No overlay or movement change.

Configured on the Alley player, Apartment player and shared Apartment Player prefab. PlayerZoom exposes Zoom Field Of View and Transition Speed on the Player in the Inspector. Normal FOV is captured from the existing camera when enabled. Disable the component to turn off the feature.

Restores normal FOV when menus/documents/cutscenes are active, time is paused, the cursor is released, the window loses focus or the component is disabled. Existing center-ray interaction distance is unchanged. Only the player's camera FOV is changed; CCTV cameras remain unchanged and the mirror follows the player's projection through its existing camera copy.

Play Mode checks passed: smooth hold/release, all interaction visibility gates, pause/focus/cursor/disable restoration, equivalent 30/144fps convergence, and actual Input System right-button press/release on a temporary test mouse in both Apartment and Alley. Test mouse removed and original mouse restored. Scene switching retained one zoom component. No Console errors/warnings. No commit or push performed.
