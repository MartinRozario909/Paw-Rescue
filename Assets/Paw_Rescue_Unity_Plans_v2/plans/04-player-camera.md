# Phase 04 — PlayerMotor, double jump, visuals, third-person orbit

**Reference:** `scripts/player.gd`, `scripts/view.gd`, `objects/player.tscn` in the pinned Kenney Godot starter kit. Reproduce **behavior**, not language or frame-dependent numeric constants.

## Player prefab
```text
PetPlayer (Rigidbody; BoxCollider + PetPhysics material; PlayerMotor; PlayerVisuals; PlayerRunReset)
├── ModelRoot (Cube Pets dog FBX or glTF model, Animator)
├── CameraTarget (offset near head/upper body; no yaw inheritance from ModelRoot)
├── (ground check: thin Physics.OverlapBox under the BoxCollider, done in code)
├── ShadowVisual (URP decal or transparent quad projected under pet)
├── RunDust (ParticleSystem; local start/stop)
├── LandingBurst (ParticleSystem burst)
└── FootstepAudio (loop AudioSource, or timed footstep pulses)
```
- **Rigidbody + colliders (required):**
  - The player root has a dynamic `Rigidbody`: Use Gravity on, Is Kinematic off, Freeze Rotation XYZ, Interpolate, Collision Detection Continuous Dynamic, mass 1.
  - It also has a `BoxCollider` fitted to the imported dog (current size `1.3 × 1.6 × 1.38`, centre `(0, -0.4, 0.05)`). The collider uses the zero-friction `PetPhysics.physicMaterial`, so the dog slides along walls instead of sticking.
  - The root is never scaled. Only the visual child may squash and stretch. No `CharacterController`.
- **Gravity** is the Rigidbody's gravity, set project-wide in **Project Settings > Physics > Gravity = (0, -25, 0)**.
- The BoxCollider is also the **pickup collider**. Coins and other collectables are trigger colliders, and their `OnTriggerEnter` checks for `PetController`.
- `PlayerMotor` stores **jumpsUsed (int)**, **jump buffer timer** and **jump-released flag**. It reads planar and vertical velocity from `Rigidbody.linearVelocity` each `FixedUpdate`. `PlayerVisuals` reads values without modifying physics.

## Controller steps
1. [ ] Compute move input `Vector2`; apply normalized max magnitude of 1 (avoid diagonal boost). Compute camera forward/right projected onto XZ, normalize them, build desired planar velocity.
2. [ ] Read input in `Update` and keep jump presses until the next `FixedUpdate`. In `FixedUpdate`, start from the Rigidbody's real planar velocity (so walls can stop the dog) and blend toward the target with `1 - exp(-responsiveness * fixedDeltaTime)`. Use less responsiveness in the air. Don't use Godot's `movement_speed * delta` directly.
3. [ ] Let the **Rigidbody's gravity** pull the dog down; don't add gravity in code. Write the result back with `rb.linearVelocity = new Vector3(planar.x, velocity.y, planar.z)` and clamp the fall speed to `-maxFallSpeed`. Do the ground check with a thin `Physics.OverlapBox` just below the BoxCollider, ignoring triggers and the player's own Rigidbody. When grounded with `velocity.y <= 0.01`, restore `jumpsUsed=0`.
4. [ ] **Double jump:** when buffered jump occurs and `jumpsUsed < 2`, initiate jump. First jump is allowed while grounded/coyote; second jump while airborne. Each jump sets upward velocity to configured impulse (the second can be e.g. 0.9× first). If falling off ledge without jumping, treat an airborne jump as the first jump (policy documented and tested).
5. [ ] **Coyote and buffer:** on edge exit, allow first jump for ~0.10–0.12 s. Hold a ~0.10–0.12 s input buffer before landing; consume once. Prevent accidental repeated jumps from a held button: respond only to press edges.
6. [ ] **Variable height:** on `JumpReleased` with positive upward speed, shorten upward impulse once (e.g. multiply current positive speed by 0.55). Releasing first jump does not automatically exhaust second.
7. [ ] Visual-facing yaw turns smoothly toward actual nonzero planar velocity; keep character mesh separate from CameraTarget. Idle/walk/jump states blend by speed and grounded state. Observe Cube Pets animation clip names during import; remap only real clips. If no needed clip exists, use procedural bob + simple Animator blend.
8. [ ] `ApplyBounce(float speed)`: upward speed = max(current upward speed, bounce impulse) or set deterministic bounce speed; choose and document. **Reset jumpsUsed to 1 or 0 explicitly** after bounce: recommended set `jumpsUsed=1`, allowing **one midair corrective jump** while retaining pad's unique reach. Reapply jump audio/scale and short cooldown; never auto-repeat from same collider overlap.
9. [ ] When falling below an explicit `KillY` (e.g. -20, adjusted to lowest island), call `GameManager.Fail()`; do **not** use Kenney `reload_current_scene()`.
10. [ ] Add reset routine to teleport: set `rb.linearVelocity` and `rb.angularVelocity` to zero, set `rb.position`/`rb.rotation` **and** the transform to the spawn pose, clear jumps, input flags, particles and animator states, and notify the camera to reset.
11. [ ] Rotate the dog with `rb.MoveRotation(...)` in `FixedUpdate`, never `transform.rotation`, so interpolation stays smooth.

## Initial tuning (starting estimates, re-measure in Play Mode)
| Value | Initial | Notes |
|---|---:|---|
| Walk speed | 5.0 m/s (Unity units/s) | Kenney's `250 * delta` cannot be copied numerically |
| Ground acceleration | 22 m/s² | Responsive, not instant |
| Ground deceleration | 28 m/s² | Snappy stops |
| Air steering fraction | 0.65 | Preserve jump control |
| Gravity | -25 m/s² | Rigidbody gravity, set in Project Settings > Physics (Unity default -9.81 felt floaty) |
| First jump height | 1.5 units (current) | Compute `sqrt(2 * -Physics.gravity.y * height)` |
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
