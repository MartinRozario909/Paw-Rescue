# Phase 13 — Implementation recipes: Godot behaviors translated to Unity C#

**Purpose:** logic reference and pseudocode, not a claim of built/compiled scripts. The coding agent must adapt APIs to Unity/Cinemachine versions in the installed project and validate.

## A. Coordinate, time and physics semantics

| Godot source | Unity translation | Important distinction |
|---|---|---|
| `CharacterBody3D.velocity; move_and_slide()` | `CharacterController.Move(displacement)` | Unity CC expects **displacement** and does **not** apply gravity |
| `Input.get_axis(move_left, move_right)` | Input Action `Move.ReadValue<Vector2>()` | Map `x=horizontal`, `y=forward`; in Unity XZ is horizontal plane |
| `input.rotated(Vector3.UP, view.rotation.y)` | `camRightXZ*x + camForwardXZ*y` | Prevent diagonal multiplier >1; camera yaw only |
| `movement_velocity=input*250*delta` | `targetPlanarVelocity = direction*speed` | In Unity `speed` is units/sec; apply `dt` **only to Move displacement** |
| `gravity+=25*delta; y=-gravity` | `verticalSpeed += gravity*dt; motion.y=verticalSpeed` | Unity gravity negative; jump is positive impulse |
| `lerp_angle` | `Mathf.MoveTowardsAngle` or damped rotate visual yaw | Only character model should face travel; camera target stays stable |
| `lerp(…,delta*10)` | `alpha=1-exp(-lambda*dt)` | Avoid unstable factor >1 on huge frame spikes |
| Godot GPU particles | Unity `ParticleSystem` | Material, emission rate, bursts and simulation space need Unity setup |
| `Audio.play(path)` pooled | `AudioPool.Play(AudioClip)` | Don't load clip path per sound; use serialized asset refs |
| `get_tree().reload_current_scene()` | `GameManager.Fail(); ResetRun()` | Scene stays loaded as required |
| `queue_free()` disappearing platforms/coins | `SetActive(false)`/hide and `IResettable` restore | Clear all run state when retrying |

## B. Camera-relative movement algorithm (C# pseudocode)
```csharp
Vector2 input = Vector2.ClampMagnitude(inputReader.Move, 1f);
Vector3 f = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
Vector3 r = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;
Vector3 direction = Vector3.ClampMagnitude(f * input.y + r * input.x, 1f);
Vector3 targetVelocity = direction * moveSpeed;
float accel = grounded ? groundAcceleration : airAcceleration;
planarVelocity = Vector3.MoveTowards(planarVelocity, targetVelocity, accel * dt);
verticalSpeed += gravity * dt;
CollisionFlags flags = controller.Move((planarVelocity + Vector3.up * verticalSpeed) * dt);
// Evaluate landed transition using previous grounded state and downward speed;
// keep one CharacterController.Move per frame and update jump state afterwards.
```
Use `CharacterController.isGrounded` after Move or an explicit ground probe for reliable contact. Clamp `verticalSpeed` to small negative when grounded; record impact velocity **before** clamping for landing FX.

## C. Double jump state and jump buffering
```text
OnJumpPressed:  jumpBufferTimer = 0.12
Each Update:     jumpBufferTimer = max(0, jumpBufferTimer - dt)
Grounded:        jumpsUsed=0; coyoteTimer=0.10
Not grounded:    coyoteTimer=max(0, coyoteTimer-dt)
CanJump when:    jumpBufferTimer>0 AND
                 ((jumpsUsed==0 AND (grounded OR coyoteTimer>0)) OR
                  (jumpsUsed<2 AND airborneAfterFirstJump) OR
                  (jumpsUsed==0 AND fellFromLedgePolicyAllowsAirJump))
On jump:        verticalSpeed = upwardImpulse; jumpsUsed++;
                 consume buffered edge; start visual stretch and audio
On release:      if verticalSpeed>0 and not cut, verticalSpeed*=0.55;
On BouncePad:    verticalSpeed=bounceImpulse; jumpsUsed=1; consume input buffer;
                 prevent same-collider retrigger until exit/cooldown
```
Ensure jump on landing and double jump cannot both fire in same frame; document edge ordering. Coyote time is **extra improvement** not originally in Kenney GDScript.

## D. Kenney particles / audio threshold table
| When | Reference | Unity effect |
|---|---|---|
| Running floor speed >5% | `player.gd` starts walk | Animator walk blend |
| Grounded speed >30% | sound unpauses, pitch follows factor | Footstep loop/source |
| Grounded speed >75% | trail emits | Footstep dust ParticleSystem emission |
| Jump pressed | model scale `.5,1.5,.5`, jump SFX | visual-only stretch and jump clip + SFX |
| Hard land | scale `1.25,.75,1.25`, land SFX | landing edge → scale/small burst/SFX |
| Coin collected | hides mesh, stops stars, sound | stamp hide, separate pickup burst, pooled SFX |
| Falling platform touched | fall SFX, short XZ stretch, downward acceleration | resettable fall platform |
| Breakable underside collision | burst debris, mesh/collider hidden | optional resettable block |

### Robust burst design
`ParticleSystem.Play()` on a particle child that is **not disabled with the mesh**, otherwise the burst vanishes. Place mesh and particle systems as siblings under persistent prefab root. Use a max-particle cap and GPU-safe URP material. If effects are pooled, ensure `Stop(true, StopEmittingAndClear)` on reset and reinitialize correct emission state.

## E. Stable bob/spin
```csharp
// Cache Vector3 initialLocalPosition, y offset cannot drift:
time += dt;
transform.localPosition = initialLocalPosition + Vector3.up * (Mathf.Sin(time * frequency) * amplitude);
visual.Rotate(Vector3.up, rotationRadiansPerSecond * Mathf.Rad2Deg * dt, Space.Self);
```
Kenney `coin.gd` spins at `2` rad/sec and bobs via cosine integration; this anchored sine is deliberately numerically safer but preserves look. On Reset, reset time, transform and visibility.

## F. Falling platform reset contract
```text
Awake:  cache pose, scale, renderer/collider state
Contact: if Playing and not triggered -> flag true; play sound; squash
Update: if triggered -> fallSpeed += fallAccel * dt;
                        pos += Vector3.down * fallSpeed * dt
        otherwise scale approaches cached scale
Below threshold: disable renderer/collider/trigger (do not Destroy)
ResetRunState: restore cached transform and enabled state; fallSpeed=0;
               triggered=false; clear effects/contacts
```
**Test collision layers:** CharacterController vs animated collider, trigger must register when stepping on pad, physics signal may need a kinematic Rigidbody on moving trigger. Avoid scaled MeshCollider or nonconvex dynamic collider.

## G. Audio pool behavior
```text
Initialize e.g. 12 AudioSources, all inactive/not playing.
Play(clip, volume): borrow an idle voice, optional priority steal/queue;
                   voice.pitch = Random.Range(.9,1.1); voice.PlayOneShot(...)
Each update or when ended: source available for reuse.
Footsteps: separate loop source with pitch responding to normalized speed,
           paused/stopped when airborne or speed <= threshold.
```
Do not call Unity `AudioSource.PlayOneShot` on the same voice if its per-clip pitch is concurrently being changed for other effects.

## H. Editor implementation strategy
- Build scripts as independent compile-ready `.cs` first; use minimal reflection.
- Create an **Editor-only idempotent** setup command that checks scene paths, prefab exists, `SerializedObject` references, and reuses existing art. Agent may invoke through installed Unity Pipeline/CLI after discoverable command registration.
- Save scenes after proper Prefab/AssetDatabase operations; do not hand-author `.unity` YAML unless a supported tool specifically requires it.
- Run compile and Play Mode checks after each subsystem: input, character, camera, collectibles, VFX, HUD. Never bypass Console errors.
