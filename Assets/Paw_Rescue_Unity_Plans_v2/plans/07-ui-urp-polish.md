# Phase 07 — Kenney UI Pack, URP art integration and presentation

## Required visual assets
- [ ] Player from **Kenney Cube Pets**, ideally dog, imported to Unity with real Animator clips if available.
- [ ] Platforms, terrain, greenery, fencing, scenery from **Kenney Platformer Kit 3D**, collision proxies from Unity primitive colliders.
- [ ] Menu panels/buttons/HUD badge decorations from **Kenney UI Pack**. Don't build all UI as Unity's default gray buttons.
- [ ] Optional extra Kenney CC0 sounds; any third-party fonts/sounds documented in `THIRD_PARTY_NOTICES.md` and submission AI/asset note.

## UI flow / screen definitions
| Screen | Components | Buttons / behavior |
|---|---|---|
| **Start** | Title, pet illustration, one-sentence goal and `Move / Look / Jump` instructions | Play, Quit (Windows only) |
| **HUD** | Paw icon + `0/3`, timer, pause, contextual hint `Collect all 3 stamps` | Pause; touch stick/right-look/jump on mobile |
| **Pause** | Background dim, current run count, control reminder | Resume, Restart Run, Main Menu |
| **Fail** | `Oops! Try again`, optional stamps/time | Retry in place, Main Menu |
| **Win** | `Package delivered!`, count/time, subtle celebration particles | Play Again in place, Main Menu |

### Build steps
1. [ ] Create prefabs for Kenney UI nine-sliced panels/buttons and HUD chip; use native Button components for actual interactivity and navigable keyboard focus.
2. [ ] Add `CanvasScaler` with reference 1280×720 and `SafeAreaRoot` that adapts to Android cutouts/navigation bars. Test 16:9 desktop and narrower/wider landscape screens.
3. [ ] Use `InputSystemUIInputModule`; ensure Raycaster pointers do not send gameplay Look while interacting with menus, Pause/Win/Fail.
4. [ ] Add button focus/hover/pressed and click SFX; use hover only on desktop, pressed state on touch.
5. [ ] Show first-play tutorial prompt or simple corner hint (movement, space jump/double jump, camera swipe). Avoid long unskippable tutorial.
6. [ ] When gameplay is paused via `Time.timeScale=0` (if chosen), ensure UI animation and buttons still work using unscaled time, and input remains cleared. Alternative: state-gated motion and timer with no global timescale change; pick one consistent strategy.
7. [ ] At 3/3 stamps, gate visibly changes and HUD animates; goal/house always visible as a landmark.

## URP visual pipeline
1. [ ] Verify assigned URP asset in Graphics and Quality; color space Linear where supported. Use Universal Renderer with opaque/transparent pass optimized for mobile.
2. [ ] Skybox/pastel blue clear + one Directional Light and soft shadows; baked lighting/static batching for decorative static terrain where practical.
3. [ ] Palette: grassy greens, honey yellow stamps/pad, sky blue void, coral delivery building/gate, orange puppy. Make the paw stamp visually distinct from generic decorative assets.
4. [ ] Reasonable camera fog (URP-supported method) and ambient/environment reflection; avoid screen-space effects whose support is uncertain on Android.
5. [ ] A fake blob shadow below pet can be a transparent quad with contact raycast or a lightweight Decal Projector **if renderer supports it**. Kenney starter uses a Godot Decal, not a Unity Decal; not mandatory to be feature-identical.
6. [ ] Particle material/texture must render in URP and target mobile; test build for magenta shaders.
7. [ ] Limit realtime lights to one main directional plus optional small goal emissive. Preserve 30–60 FPS on test device; avoid heavy postprocess stack.

## Presentation checklist
- [ ] Hero and route visible at start; forward progress obvious from world composition.
- [ ] All 3 Kenney packs visibly represented; UI uses actual UI Pack sprites.
- [ ] Pet model never clips through surface when landing squash plays; camera doesn't obstruct progress.
- [ ] Particles/audio events read clearly but do not overwhelm gameplay.
- [ ] No console compile errors, pink materials, missing textures, missing scripts or overlapping UI.
- [ ] Desktop and Android capture has a clean beginning, mid-level obstacle, stamp, bounce, delivery ending.
