# Phase 14 — Risk register, deadline response, and explicit cut order

| Risk | Impact | Mitigation | Fallback |
|---|---|---|---|
| Unity CLI/Pipeline auth or beta command mismatches | Agent can't manipulate Editor | First verify `unity --help`, `unity pipeline list`, installed skills and Editor path | Manual Unity Editor + C# text edits; no unverified MCP claims |
| URP mobile shader/pink particle materials | Android unplayable / ugly | Verify materials in Android sample build early | Submit tested Windows build; simplify shader/particles |
| `OnScreenStick` virtual gamepad switches `PlayerInput` scheme | Touch stick cancels / look stops | Use explicit InputReader across devices, test multi-finger | Use custom touch joystick adapter without virtual gamepad; document change |
| Double jump + Paw Bounce trivially bypass sections | Level too easy | Measure jump range, spring height and platform placement | Increase route separation / use spatial gate, not extra mechanics |
| Player Rigidbody on moving/falling platforms | Player jitters, slides off or falls through | Platforms use kinematic Rigidbody + `MovePosition` in `FixedUpdate`; player Rigidbody Interpolate + Continuous Dynamic; don't parent the player | Remove falling platform, replace with fixed bridge |
| Player BoxCollider snags on block seams/edges | Dog stops or hops at tile joins | Zero-friction `PetPhysics` material, overlapping ground colliders, flat seams | Slightly round the bottom with a CapsuleCollider *in addition to* the box, or merge ground colliders |
| Touch camera event overlaps jump/pause | Mobile unplayable | One pointer owner, raycast order, three-touch physical test | Restrict look area and enlarge buttons; reduce controls |
| Particle performance / audio pool bugs | Low FPS, crackling | Cap emission, no Instantiate each frame, pitch ranges | Keep only jump, landing, stamp bursts and footstep sound |
| Win/Fail uses scene reload | Fails explicit requirement | `IResettable` registry, test scene handle unchanged | Simplify/reset objects, not reload |
| Imported Cube Pets animation names differ | Animation missing | Inspect FBX clips, map actual names | Procedural idle/walk bob without broken Animator |
| Low deadline / export failures | No valid submission | Windows build smoke test early, final QA before polishing | Prioritize required deliverables over Android/Web |

## Cut order (only after P0 stable)
1. Optional decorative clouds or complicated backgrounds.
2. Optional breakable brick / debris.
3. Additional moving platforms / experimental platform AI.
4. Gamepad zoom / mobile pinch zoom / auto camera re-centering.
5. Timer medal, rankings and extra win animations.
6. Extra VFX sophistication (keep a small set of jump/landing/stamp feedback).

**Never cut:** pet hero, clearly playable single level, required 3 Kenney packs, start/win/fail, in-place retry, keyboard+touch controls as requested, tested playable build, source, 30–60-sec video, five-line note.

## Final readiness rubric
- **Completeness:** start to win and fail→retry without scene reload.
- **Feel:** responsive camera-relative movement and reliable jump/double jump.
- **Visual:** cohesive URP, Cube Pets pet, platformer environment, Kenney UI, readable stamps.
- **Design:** obvious goal and progression, bounce and falling platforms provide meaningful variation.
- **Code:** GameManager and components independently understandable in a 20-minute walkthrough and small live edit possible.

## Interview live-change preparation
Have a pre-identified simple variable/property that safely changes runtime behavior: `walkSpeed`, `maxJumps`, `bounceImpulse`, pickup spin speed, win stamp requirement via Inspector (if suitable), particle threshold. Show where test validates it; avoid relying on recompiling half the project for a trivial demonstration.
