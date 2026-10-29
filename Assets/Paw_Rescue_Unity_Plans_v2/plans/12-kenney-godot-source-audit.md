# Phase 12 — Audited Kenney Godot starter-kit source → Unity feature mapping

**Source repo:** https://github.com/KenneyNL/Starter-Kit-3D-Platformer  
**Reviewed default branch:** `main` at **`3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0`**, commit `Upgrade to Godot 4.6` dated 2026-03-12.  
**Method:** checked repository files through GitHub, not simply a video or the README. These are source-derived observations. No Godot code was executed in this review; physics timing and perceived game feel must still be verified in Unity.

## 1. Repository structure checked
```text
scripts/
  player.gd   view.gd   audio.gd   hud.gd   main.gd
objects/
  player.tscn  character.tscn  coin.gd  coin.tscn
  platform_falling.gd   platform_falling.tscn
  brick.gd   brick.tscn   cloud.gd   cloud.tscn
scenes/
  main.tscn   main-environment.tres
project.godot
```

## 2. Exact observed behavior and translation

| Source file / nodes | What **Godot source actually does** | Unity equivalence / deviations |
|---|---|---|
| [`scripts/player.gd`](https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/main/scripts/player.gd) | `CharacterBody3D`; move axis X/Z rotated by view Y, normalize diagonal, velocity lerp at `delta*10`, `move_and_slide()`. `movement_speed=250` multiplies `delta`. | `CharacterController`, camera-projected input, planar velocity in units/s and `Move(velocity*deltaTime)`. **Do not copy movement_speed=250** or double-apply dt. |
| same | Explicit `gravity += 25*delta`, `applied_velocity.y=-gravity`, single jump impulse `jump_strength=7`; `jump_single`/`jump_double` flags allow two total jumps and reset on ground. | Negative gravity / positive upward velocity convention, `jumpsUsed<2`, max two. Coyote, jump buffer and variable height are **Unity improvements**, not explicit in reference. |
| same | Turn character towards velocity using angular interpolation `lerp_angle(delta*10)`. | Rotate mesh root toward planar motion only, smooth yaw; camera orbit separate. |
| same | Idle/Walk/Jump animation selected by grounded and normalized movement; walk speed scaled by movement speed. | Animator states mapped from imported Cube Pets clips (inspect names). Speed parameter & jump/fall transition. |
| same | Footstep `AudioStreamPlayer` paused if not on floor and starts at `speed_factor >0.3`; pitch driven by speed. Dust starts at `speed_factor>0.75`. | `AudioSource` loop/state thresholds and `ParticleSystem` emission in `PlayerEffects`; compute normalized speed from **Unity horizontal speed**, not Godot's frame-dependent ratio. |
| same | On jump Model scale `(0.5,1.5,0.5)`; on hard landing `(1.25,0.75,1.25)`; lerps back to `(1,1,1)`, landing SFX. | Scale *visual-only* child relative to its original imported scale; exponential damping, landing edge only. |
| same | If y < -10 calls `get_tree().reload_current_scene()`. | **DO NOT PORT**; trigger `GameManager.Fail()`. `ResetRun` in place required by assessment. |
| [`objects/player.tscn`](https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/main/objects/player.tscn) | `CharacterBody3D`, CapsuleShape radius 0.3 height 1.0, visual `Character`, `Shadow` decal, `ParticlesTrail` GPUParticles (`amount=60`) using dust mesh/curve, `SoundFootsteps` audio source. | `CharacterController` and model-fit collider, BlobShadow quad/decal, `ParticleSystem` soft dust, `AudioSource`. Model/capsule sizes must reflect **Cube Pets**, not demo avatar. |
| [`scripts/view.gd`](https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/main/scripts/view.gd) | View follows target with position lerp `delta*4`, smooths `rotation_degrees` `delta*6`. Camera position smoothly approaches local `(0,0,zoom)`, initialized 10. Camera rotation rate 120°/s for bound axis; pitch clamp `-80..-10`; zoom clamp 4..16. | Cinemachine 3 Orbital Follow or one custom yaw/pitch pivot. Full yaw and clamped pitch, damped follow/zoom, source-specific mouse/touch sensitivity. |
| [`project.godot`](https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/main/project.godot) | Move WASD+left gamepad stick; Jump Space+gamepad button; camera arrows+right stick; Zoom +/- keys + gamepad triggers. Dead zones 0.25 move, 0.5 for jump/camera/zoom. | Unity Input System composite + gamepad; **additional** mouse delta, touch stick/right swipe/jump on-screen. Match intent, not Godot InputEvent syntax. |
| [`objects/coin.gd`](https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/main/objects/coin.gd) | `Area3D` entry if body has `collect_coin` and not grabbed: increases count, plays coin SFX, `queue_free` mesh, stops particle emitter; spin Y at `2*delta`; move Y by cosine oscillation. | Unity trigger stamps with idempotent collected bool, `GameManager` event count, bobbing/spin, particle/mesh hide. **Do not destroy** stamp object because retry requires reset. |
| [`scripts/hud.gd`](https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/main/scripts/hud.gd) | Listens for collected coin signal and sets label to numeric count. | Subscribe to stamp-count event, Kenney UI badge `0 / 3`; avoid reading GameManager every frame. |
| [`objects/platform_falling.gd`](https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/main/objects/platform_falling.gd) | Body entry triggers fall and SFX, initial scale `(1.25,1,1.25)`, returns scale to identity with lerp `delta*10`, accumulates `fall_velocity += 15*delta`, lowers position by velocity*dt; queues free below y=-10. | Triggered resettable fall platform, saved initial position/scale and fall speed. On fall below threshold hide/deactivate; **do not destroy** for retry. |
| [`objects/brick.gd`](https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/main/objects/brick.gd) | `BottomDetector` body enter checks group `player`; once only, plays break SFX, restarts particles, hides mesh, disables collider, then deletes after 1s. | Optional Unity `BreakableBrick` underside sensor, deactivate mesh/collider, burst and restore on retry. Do not use tag alone to infer upward head impact; verify velocity/direction. |
| [`objects/cloud.gd`](https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/main/objects/cloud.gd) | Random bob speed/frequency (each 0.1..2), Y increments cos-based periodic movement. | Cosmetic anchored sine bob with small randomized amplitude/frequency. |
| [`scripts/audio.gd`](https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/main/scripts/audio.gd) | Autoloaded audio system preallocates **12** `AudioStreamPlayer`s, queues paths, plays from first available, random pitch **0.9..1.1**; initially volume -10 dB. | Preallocated pooled `AudioSource`s with referenced `AudioClip`s + random pitch and optional queue. Separate footstep loop. |
| [`scripts/main.gd`](https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/main/scripts/main.gd) | Adjusts sun/sky exposure under Godot GL Compatibility renderer. | **Do not port literal lighting numbers:** configure URP lighting, shadow/color and mobile quality through Editor, test visually. |
| [`scenes/main.tscn`](https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/main/scenes/main.tscn) | Includes player, camera view, static & falling platforms, coins, bricks, flag, light and HUD with `coin_collected` signal connection. | Build original five-section pet delivery scene with Unity GameObjects/prefabs and event hookups; scene is **design inspiration**, not source map to clone. |

## 3. Explicitly NOT present in starter source (new feature work)
- No complete **main menu, win/fail screen**, nor assignment-compliant **restart without scene reload**; source reloads on falling.
- No **touchscreen virtual controls** or phone safe-area HUD in inspected input configuration.
- No stamps-to-unlock delivery gate, package objective or custom Paw Bounce spring mechanic.
- No Cinemachine (Godot owns camera). Unity translation uses its own Input System and Cinemachine 3 APIs.
- No proof from inspection alone that the reference runs with consistent framerate; its movement value is affected by delta. Must tune in Unity, not claim equivalent speed.

## 4. License handling
- Repository README + LICENSE.md state **MIT license for code** (copyright © 2026 Kenney) and included assets **CC0**. If copying source code in substantial form, include original MIT notice/license in `THIRD_PARTY_NOTICES.md` or `LICENSES/MIT-Kenney-Starter.txt` and respect notice requirement.
- Three **separately required** Kenney packs (Cube Pets / Platformer Kit / UI Pack) are CC0 on their official pages. Their licensing does not automatically make unrelated project code CC0.
- Document inspiration in source attribution even when C# is written independently; be transparent about AI assistance and asset origins.

## 5. Final implementation priorities from audit
**Must port concepts:** camera-relative motion and double jump, orbit camera, stamp bob+pickup, grounded trail/footsteps, landing squash, pooled SFX, falling platform, fail/retry-safe resets.  
**Optional:** breakable brick, ambient cloud bob, camera zoom controls on mobile, cosmetic score.  
**Must not port literally:** Godot nodes, GDScript, `move_and_slide`, double delta movement speed, raw Godot pitch numbers, scene reload/queue_free for runtime objects that must reset.
