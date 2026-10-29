# Phase 05 — Single-level layout and gameplay systems

**Entry:** Player + camera pass graybox movement tests. **Exit:** full game loop can be completed and restarted repeatedly without any scene reload.

## Level route
One continuous World root, five visible areas (block out with cubes first, add Kenney assets later):

| Region | Gameplay | Design intent |
|---|---|---|
| A. Starting Garden (safe 9×9) | Learn camera/movement; first shallow jump | Teach basic play and highlight route |
| B. Broken Crossing (3–4 landings) | Collect Paw Stamp #1; one safe falling platform | Introduce double jump and responsive recovery |
| C. Bounce Garden | Spring pad launches to raised island; Paw Stamp #2 | Distinct signature Paw Bounce mechanic |
| D. Sky Bridge | Timed/falling platform or fixed narrow bridge; Paw Stamp #3 | Escalation without enemy AI |
| E. Delivery Island | Gate turns green when 3/3, cottage + goal trigger | Explicit victory and satisfying end |

The exact gaps must be derived from measured first-jump + double-jump travel distance, not guessed. Broad platforms, high-contrast edges and visible route markers are more important than a complex map.

## Implement in order
1. [ ] Build whole graybox with Primitive/ProBuilder cubes and **simple BoxCollider** surfaces. Add `Gizmos` waypoint markers, stable start pose and fall volume. Keep camera obstruction in mind.
2. [ ] `PawStamp` prefab based on Kenney `coin.gd`: trigger + unique stamp ID; mesh rotates `2 radians/sec`, bobs using a sine evaluated against **fixed baseline local position** (`baseY + amplitude * sin(frequency*t)`, not accumulating world Y drift). Stop idle particles and hide visuals on collection; play pooled SFX + small burst. Do **not** destroy prefab; implement `ResetRunState()`.
3. [ ] `GameManager.CollectStamp(id)` uses a `HashSet<string>`/IDs to reject duplicate hits. HUD is event-driven. Exactly 3 tracked stamps. On 3/3, activate unlock event; play visual cue.
4. [ ] `BouncePad` with prominent yellow/green springs and top-entry trigger. A single upward impulse, cooldown and short pad squash; optional star burst, Boing sound. Must avoid firing repeatedly while capsule overlaps pad. `ResetRunState()` restores cooldown, transform and particles.
5. [ ] `FallingPlatform` modeled on `platform_falling.gd`: On top collider contact with pet once, squash horizontally and play fall sound, then fall acceleration (Godot used `15 units/s²`; choose Unity-specific from playtest), disable/hide when below fall threshold. **Reset** to cached initial pose and active collider, rather than destroying object as Godot does. Prefer kinematic transform + kinematic Rigidbody if triggers need it; verify CharacterController safely rides until it falls. Do not parent player to falling platform.
6. [ ] `GoalGate` shows closed/locked with 0–2; opens with 3. `DeliveryGoal` triggers Win only for valid 3/3; no win if stood at gate before unlock. Make goal conspicuous via house/flag and a light/audio cue.
7. [ ] `FallVolume`/KillY triggers `Fail()` once. Show fail menu and stop clock; no automatic scene reload.
8. [ ] `ResetRun()` restores all registered initial state and makes all elements playable again. Verify stamp triggers after multiple retries.
9. [ ] Optional `BreakableBrick` from `brick.gd`: only triggers when player upward head contact is confirmed by an **underside sensor** + positive vertical velocity (prevent side break); hide mesh, disable collider, emit debris burst, restore on retry. Never destroy permanent run root. **Cut if not finished by Saturday.**
10. [ ] Optional cosmetic clouds with `CloudBob` `y=baseY+sin(omega*t+phase)*amplitude`; no physics. Use no-gameplay-collision low-cost geometry.

## Key port details
- Kenney coin uses `position.y += cos(time * 5) * delta`, which is approximately periodic but risks drift; Unity implementation should calculate around cached base position each frame.
- Kenney falling platform destroys itself below -10; **Unity reset contract requires hiding/disabling and restoring it** rather than Destroy.
- Kenney breakable brick disables collision after an underside hit and delays deletion 1s; Unity uses a resettable hidden state and `ParticleSystem.Play()`.
- Kenney starter demo has no stamp-locked delivery gate, spring-pad or complete start/win/fail UI; those are **our** systems required/added for this test.

## Acceptance gate
- [ ] Finish normally after all stamps; gate cannot unlock early.
- [ ] Falling platform collapses on contact; resets after retry; no permanent missing platform.
- [ ] Paw Bounce provides meaningful route progression, with no repeat-trigger loop.
- [ ] Intentional fall → Fail panel; terminal keyboard/touch control disabled until retry.
- [ ] At least 3 complete rounds after retries with no scene reload and no duplicate collectible counts.
