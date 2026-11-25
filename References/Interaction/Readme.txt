Interaction UI

Shared editable prefab: Assets/Interaction/Prefabs/Interaction UI.prefab. Each scene's Player has an InteractionScanner and one scene-owned UI instance. There is no DontDestroyOnLoad UI or automatic runtime spawning. Normal scene transitions destroy the previous scene's UI. Player movement is unchanged.

Appearance: 3.5 reference pixels at 1080p, white at 40% opacity. A vector Graphic morphs to a 12px hollow white circle with a 1px stroke and 90% opacity in about 0.14 seconds. CanvasScaler uses a 1920x1080 reference, matching height; the graphic anchor is exactly the screen center. Prompt uses a plain 14px sans-serif font, no background, outline, shadow or glow. Disable Show Default Dot on InteractionReticle to preserve only interaction indicators.

Range: InteractionScanner.interactionDistance is the configurable global maximum. Targets can also set a shorter per-target range. First solid collider wins; player-owned colliders are ignored, trigger volumes ignored, and a saturated non-alloc ray buffer falls back to a complete query. The reticle and E interaction use the same camera-centered query. Elevator left-click remains supported; entrance scene travel stays on E. Existing entrance proximity input is bypassed while a scanner exists.

Future hooks: implement IInteractionTarget or add InteractionTarget, choose a verb/range, wire its UnityEvent and enable Available. The default adapter is unavailable until deliberately configured. Building Services now uses the same interface for CCTV, breakers/local lighting switches and the electrical-room door. Telephone, document inspection, private apartment door and pickup interactions remain unimplemented.

Hide and suppress interaction with SetMenuOpen(bool), SetCutscenePlaying(bool), SetDocumentOpen(bool), or the corresponding serialized fields. These gates compose independently. UI/input also hide when the cursor is released, time is paused, the player is disabled, or its camera is no longer the main camera. Future menus/document readers must call these gates for their own states.

Verification: near/far button detection, a temporary wall occluder, disabled idle dot with preserved ring/prompt, smooth intermediate morph, menu/cutscene/document hiding, cursor release, one UI/scanner across Apartment-to-Alley-to-Apartment travel. Actual vector mesh (260 vertices) and 1080p rendered ring were inspected. See Checks.txt, Idle.png and Interaction.png. Initial same-frame scene-load requests in the log precede the confirmed post-load checks; scene loading completes on the next Unity frame.

Requested notice cleanup: Building Notices, Building Plan, Calendar and Key Return surfaces use Blank Paper.mat. Their frames and editable objects remain. The shared Entrance Interior preview is updated. Original printed textures remain available in the project.
