# Phase 06 — Kenney-inspired juice: particles, animation, procedural FX and audio

**Source:** `scripts/player.gd`, `objects/player.tscn`, `objects/coin.tscn`, `objects/brick.tscn`, `scripts/audio.gd`, `objects/cloud.gd`. No Godot `.res`, `GPUParticles3D` or `.ogg` paths are assumed to directly translate to Unity ParticleSystem settings.

## 1. Pet squash and stretch
Kenney scales visual model to **(0.5, 1.5, 0.5)** when jumping, **(1.25, 0.75, 1.25)** on a fast landing, easing to **(1,1,1)** at a rate roughly `delta * 10` using lerp. The landing check uses previously-grounded flag and fall speed. Unity:
- [ ] Store `ModelRoot` **initial scale** (might not be 1 on imported Cube Pets model). Compute squash as multiplicative factor of base scale, **never** scale the player root, its Rigidbody or its BoxCollider.
- [ ] On jump: apply **stylized** stretch factor (tune; Kenney reference can be exaggerated). On landing after significant downward speed: squash; spring back using exponential damping `1-exp(-lambda*dt)` (frame-rate independent).
- [ ] Cache wasGrounded and detect first `!wasGrounded && isGrounded` transition with downward velocity threshold. Fire one landing sound and particle burst. No repeated stomps while walking.
- [ ] Use Animator for imported actual clips, selecting Idle / Walk / Jump/Fall based on current velocity. Walk speed scales animation playback within safe range.

## 2. Running dust / trail
Kenney `ParticlesTrail` is `GPUParticles3D`, **amount 60**, uses `meshes/dust.res` and a curve that grows then fades. It emits when grounded and speed factor > ~0.75; footsteps start around speed factor > ~0.3 and walk anim near >0.05. Unity design:
- [ ] Child `ParticleSystem` under pet feet, **Simulation Space World** for trail left behind while moving, emission controlled by **normalized speed = horizontalSpeed/maxRunSpeed**.
- [ ] Idle <0.05: no trail; footsteps off; idle animation. 0.05–0.30: walk animation. >0.30: footsteps. >0.75: dust trail (starting thresholds derived from reference).
- [ ] Particle start lifetime ~0.25–0.6 s, small start speed, subtle gravity/size-over-lifetime curve, maxParticles cap; trial count ~60 is **reference, not required emissionRate**.
- [ ] Avoid `Play()/Stop()` every frame. Update emission on state change or `emission.enabled`; clear particles on reset.
- [ ] Use correct URP unlit/particle material and visible soft dust sprite/mesh (CC0 or primitive), avoid GPU particle portability assumptions on mobile.

## 3. Coin/stamp glimmer and pickup burst
Kenney `coin.tscn` has looping GPU particles, then stops emission and removes its mesh on pickup. Unity:
- [ ] Distinctive spinning star/paw stamp with idle `ParticleSystem` emitting slowly.
- [ ] On pickup, stop idle emitter, hide mesh/trigger, play **separate one-shot burst** particle system (child must remain alive after mesh hides), and pooled `coin` SFX.
- [ ] Re-arm idle particles and mesh/collider on `ResetRunState`.

## 4. Breaking block and falling pad effects
Kenney brick has `particles.restart()`, mesh hides, collider disabled; it uses an upward burst with ~5 debris particles. Kenney fall pad enlarges horizontally before falling.
- [ ] Create `BreakBurst` prefab (short lifespan with debris), optionally 5–10 particles. Use only if brick implemented.
- [ ] `FallingPlatform` applies temporary squash/expand, plays fall sound, starts downward acceleration; changes must not mutate initial scale used for reset.
- [ ] `BouncePad` reverses visual compression and emits short glitter/impact burst. This is **our extension**, not starter behavior.

## 5. Audio pool instead of per-frame Instantiate
Kenney `scripts/audio.gd` queues sound paths, reuses **12** AudioStreamPlayers, initializes them around -10 dB and randomizes pitch `0.9..1.1`. Unity:
- [ ] `AudioPool` created with ~8–12 preallocated `AudioSource`s for one-shots. Index available sources rather than Instantiate each pickup. Use `AudioClip` references (Inspector), not Godot resource paths or runtime `Resources.Load` in hot path.
- [ ] Queue / borrow voice for jump, land, coin, bounce, fall, gate, break, menu. Add a short cap/steal policy for repeated triggers. Use `AudioSource.PlayOneShot` with pitch randomization per voice and optional volume range.
- [ ] Footstep loop separate source: pause/stop when not grounded or speed <=0.30; pitch linked smoothly to speed (clamp 0.85–1.5); avoid restarting clip every frame. Optionally replace loop by timed steps synchronized to Animator.
- [ ] Audio should work after pause and restart. Distinguish UI audio from world/game audio if game uses `Time.timeScale=0`.

## 6. Gentle cloud/environment motion
Reference `cloud.gd` randomizes velocity and oscillation frequency and increments Y by cosine; Unity uses deterministic per-instance random seed and **anchored sine position** to avoid drift. Game state can pause decorative movements or leave them cosmetic; document behavior.

## Technical polish and checks
- [ ] Particle pooling/cleanup: no persistent particles after retries, no emitter left off permanently.
- [ ] Runtime FPS/GC checks: no allocations each frame from `new ParticleSystem` or `Instantiate` for footsteps; no `GetComponent` in repeated hot loop.
- [ ] Touch device runs without particle material pink squares; cap emission for mobile.
- [ ] Sound plays distinctly at jump, coin, landing, bounce and terminal state without clipping.
- [ ] Code comments identify source-inspired behaviors without false source claims.
