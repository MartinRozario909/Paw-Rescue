# Phase 08 — Unity tests, Play Mode QA, device checks, evidence

**Important:** A C# file that compiles is not the same as a playable test. The tester must **run the game** via Unity Editor/CLI and report evidence.

## Automated test strategy
Create EditMode tests for pure game logic, PlayMode tests for physics/behaviors in a minimal test scene. Avoid fragile tests that require Kenney assets on every test setup.

| Test group | Cases | Automated? |
|---|---|---|
| Game state | Menu→Playing; Pause toggle; Win requires 3; Fail idempotent; retry returns Playing | EditMode/PlayMode |
| Stamp count | Collect each ID once, reject duplicates; 0/3→3/3; reset restores zero | EditMode |
| Motor jump | grounded first, airborne second, third refused; refreshed on landing; bounce grants intended jumps | PlayMode |
| Camera | yaw 360°, pitch clamp, distance bound, look input clears on pause/reset | PlayMode + manual |
| Fall/respawn | threshold triggers Fail once; transform restored, velocities clear, **same scene** | PlayMode |
| Falling platform | activates once; moves down, restores original pose/collider on retry | PlayMode |
| Particle/audio | landing fires once, dust threshold gating, pooled voices reused, collectible burst resets | PlayMode + visual |
| UI | all buttons navigate correct states; HUD reflects stamp count; pause is interactable | PlayMode + manual |
| Mobile | left joystick + camera swipe + jump simultaneously, pointer release; safe area | Physical device |

## Hands-on playtest script — desktop
1. [ ] Launch Windows build **from extracted release directory**, not only Editor.
2. [ ] Click Play, inspect prompt; camera orbit while idle, move W relative to camera yaw.
3. [ ] Jump once, jump in air again; third button does not produce a jump; verify footstep/dust thresholds on runs.
4. [ ] Collect first/second/third stamp: UI increments once each, sparkles and sound visible.
5. [ ] Step on falling platform, watch fall + squash; intentionally fall, observe Fail, Retry; no loading overlay/scene reload.
6. [ ] Check stamp visibility/trigger counts, spring, falling platform and gate were fully restored.
7. [ ] Use Paw Bounce, then delivery gate; reach house, Win panel; press Play Again and finish another time.
8. [ ] Pause during run and while joystick/mouse look active; Resume causes no motion until input resumes.
9. [ ] Mouse wheel zoom if implemented, camera obstruction against house and fences; no jitter or collision tunnelling.

## Hands-on playtest script — Android touch (if device/toolchain)
1. [ ] Install APK on actual device; inspect notches/aspect ratio and URP materials.
2. [ ] Hold **left stick** while swiping **right look area**; both continuous inputs remain active.
3. [ ] Maintain those two fingers and tap Jump with a third. Use second jump before landing.
4. [ ] Tap Pause with stick held; lift all fingers, Resume; verify no stuck motion.
5. [ ] Pick stamp, trigger spring, fall and retry; check buttons work even when touch left/right controls are displayed.
6. [ ] Test landscape screen rotation/device back button/resolution change when feasible; check low FPS shader issues.
7. [ ] Repeat a full win flow and save brief device clip or screenshot evidence.

## Evidence record template
```text
Phase:
Unity version / URP & Input package versions:
Build target / device:
Exact executed command or manual procedure:
Compile errors/warnings:
Test count + pass/fail:
Result screenshots/videos and log paths:
Defects found + fixes:
Follow-up and remaining risks:
```

## Performance and quality gates
- [ ] Zero compiler errors, no missing component/script references.
- [ ] No exception or continuous warning spam through full start/play/fail/retry/win flow.
- [ ] 60 FPS target on mid-range PC; 30+ FPS mobile target only if Android build attempted and measured; no invented benchmark numbers.
- [ ] No UI elements under device cutout; no touch conflict.
- [ ] 3 in-place retries without stamp duplication or broken object reset.
- [ ] Playtester can understand goal and complete without developer commentary.

## CLI/testing usage
Use discovered `unity test` (see `unity test --help`) or official `unity command` tools if available; call Unity Editor test-runner equivalent if not. Do not invent command flags. If using Unity Editor `-batchmode -runTests`, target project must not be simultaneously open in another Editor; use an isolated clone or stop interactive Editor first.
