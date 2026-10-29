# Phase 04 — PlayerMotor, double jump, visuals, third-person orbit

**Reference:** `scripts/player.gd`, `scripts/view.gd`, `objects/player.tscn` in the pinned Kenney Godot starter kit. Reproduce **behavior**, not language or frame-dependent numeric constants.

## Player prefab
```text
PetPlayer (CharacterController; PlayerMotor; PlayerVisuals; PlayerRunReset)
├── ModelRoot (Cube Pets dog FBX or glTF model, Animator)
├── CameraTarget (offset near head/upper body; no yaw inheritance from ModelRoot)
├── GroundCheck (optional small sphere/raycast for slopes/landing)
├── ShadowVisual (URP decal or transparent quad projected under pet)
├── RunDust (ParticleSystem; local start/stop)
├── LandingBurst (ParticleSystem burst)
└── FootstepAudio (loop AudioSource, or timed footstep pulses)
```
- `CharacterController` should be capsule-fit to imported dog model, using non-scaled player root; visual-only transform may squash/stretch. No Rigidbody for locomotion.
- `PlayerMotor` stores **planar velocity (Vector3)**, **verticalSpeed (float)**, **jumpsUsed (int)**, **grounded/coyote timer**, **jump buffer**, **lastFallSpeed**. `PlayerVisuals` reads values without modifying physics.

## Controller steps
1. [ ] Compute move input `Vector2`; apply normalized max magnitude of 1 (avoid diagonal boost). Compute camera forward/right projected onto XZ, normalize them, build desired planar velocity.
2. [ ] Apply horizontal acceleration/deceleration `Vector3.MoveTowards`; air acceleration reduced. Handle variable Time.deltaTime, not Godot `movement_speed * delta` directly.
3. [ ] Apply gravity to vertical speed every frame. When landing, clamp small negative ground-stick velocity and restore `jumpsUsed=0`. Use a grounded check robust to CharacterController.isGrounded describing **last** Move result. Keep one `CharacterController.Move` per Update with combined displacement.
4. [ ] **Double jump:** when buffered jump occurs and `jumpsUsed < 2`, initiate jump. First jump is allowed while grounded/coyote; second jump while airborne. Each jump sets upward velocity to configured impulse (the second can be e.g. 0.9× first). If falling off ledge without jumping, treat an airborne jump as the first jump (policy documented and tested).
5. [ ] **Coyote and buffer:** on edge exit, allow first jump for ~0.10–0.12 s. Hold a ~0.10–0.12 s input buffer before landing; consume once. Prevent accidental repeated jumps from a held button: respond only to press edges.
6. [ ] **Variable height:** on `JumpReleased` with positive upward speed, shorten upward impulse once (e.g. multiply current positive speed by 0.55). Releasing first jump does not automatically exhaust second.
7. [ ] Visual-facing yaw turns smoothly toward actual nonzero planar velocity; keep character mesh separate from CameraTarget. Idle/walk/jump states blend by speed and grounded state. Observe Cube Pets animation clip names during import; remap only real clips. If no needed clip exists, use procedural bob + simple Animator blend.
8. [ ] `ApplyBounce(float speed)`: upward speed = max(current upward speed, bounce impulse) or set deterministic bounce speed; choose and document. **Reset jumpsUsed to 1 or 0 explicitly** after bounce: recommended set `jumpsUsed=1`, allowing **one midair corrective jump** while retaining pad's unique reach. Reapply jump audio/scale and short cooldown; never auto-repeat from same collider overlap.
9. [ ] When falling below an explicit `KillY` (e.g. -20, adjusted to lowest island), call `GameManager.Fail()`; do **not** use Kenney `reload_current_scene()`.
10. [ ] Add reset routine to teleport: temporarily disable CharacterController; place transform at spawn; re-enable; clear vertical/planar velocities, jumps, contact flags, particles, animator states; notify camera to reset.

## Initial tuning (starting estimates, re-measure in Play Mode)
| Value | Initial | Notes |
|---|---:|---|
| Walk speed | 5.0 m/s (Unity units/s) | Kenney's `250 * delta` cannot be copied numerically |
| Ground acceleration | 22 m/s² | Responsive, not instant |
| Ground deceleration | 28 m/s² | Snappy stops |
| Air steering fraction | 0.65 | Preserve jump control |
| Gravity | -22 m/s² | Code controlled, not built-in CC |
| First jump height | 1.8–2.1 units | Compute `sqrt(2 * abs(gravity) * height)` |
| Second jump impulse multiplier | 0.90 | Feel test |
| Jump buffer | 0.12 s | Press slightly before landing |
| Coyote | 0.10 s | Forgiving edges |
| Mesh yaw turning | 12 rad/s approximate | Smooth yaw |
| Camera initial distance | 6 units | Expose zoom 4–10 (starter kit range is 4–16) |
| Camera pitch | e.g. 15–65° downward from horizontal | Define explicit local convention; avoid inversion |

## Third-person camera (Kenney view.gd → Unity)
Reference `view.gd`: target position lerps (factor ~4), rotation lerps (factor ~6), 120 deg/s input yaw/pitch, pitch clamp -80°..-10°, camera zoom 4..16 with default 10. These are **Godot coordinate semantics and reference values**, not mandatory exact Unity axis configuration.

1. [ ] Add `CinemachineBrain` to Main Camera; `CinemachineCamera` with `OrbitalFollow` (or compatible Cinemachine 3 orbit body), Follow=`PetPlayer/CameraTarget`.
2. [ ] Use **one authoritative orbit input path** (e.g. `OrbitCameraInput` sets Cinemachine orbit axes; verify installed Cinemachine APIs; alternatively implement lightweight yaw/pitch pivot if Cinemachine integration blocks). Never have two `InputAxisController`s fighting.
3. [ ] Smooth follow position, yaw/pitch and zoom; mouse right/left unrestricted horizontal 360°. Clamp pitch away from clipping underground / overhead (Unity sign convention may differ from Godot).
4. [ ] Orbit independent of character turning; look input remains functional while standing still. No midair auto recentring.
5. [ ] Zoom with mouse wheel or controller triggers only if tested; apply min/max distance and obstruction spherecast/Deoccluder where supported. Optional pinch-to-zoom cut first.
6. [ ] On retry restore documented default camera orientation and distance; clear look deltas to avoid a one-frame jerk.

## Acceptance tests
- [ ] Standing still: camera rotates 360°, pitch stops at limits, W always advances toward camera projected forward.
- [ ] Run→stop smooth, diagonals no faster than straight, no jitter at uneven delta.
- [ ] First jump, one air jump, third press rejected until ground/bounce reset.
- [ ] Walk off ledge, coyote first jump, early press jump buffer, held jump does not auto-double.
- [ ] Bounce + remaining air jump is controllable; no infinite jumps.
- [ ] Landing callback fires **once** (required for squash/SFX/particles later), no false sound while running down a slope.
- [ ] Camera doesn't intersect platform geometry at common obstacle positions.
- [ ] On restart character returns to exact starting pose, grounded and with 2 jumps available.
