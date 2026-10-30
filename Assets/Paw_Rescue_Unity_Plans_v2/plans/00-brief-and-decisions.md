# Phase 00 — Brief, gameplay, and binding decisions

## What the test actually asks
Build one small complete 3D platformer with a pet hero, visually appealing and pleasant controls. **Required:** start screen, clear goal, gameplay, win and fail states, **restart without scene reload**, HUD/menus using Kenney UI Pack. Must use Cube Pets, Platformer Kit (3D), UI Pack. Any engine, AI allowed if disclosed. Deliver playable web/Android/Windows build, source link, 30–60 s gameplay video, five-line note. Deadline 2026-11-01 23:59 Bangladesh. Evaluation: completion, feel, looks, design, explainable/changeable code; 20-minute walkthrough with small live code change.

## Original game design (not from Godot repo)
**Pawprint Express — The Lost Delivery:** a Cube Pets puppy transports a tiny parcel over floating islands to a house; collect **3 golden paw stamps**, unlock a gate and enter delivery trigger. One continuous 2–3 minute level. Falling below islands means fail; retry restores initial level in place. **Paw Bounce** spring makes a higher route reachable. Tiny optional finish time score. No combat, NPC AI or extra levels.

## Starter kit borrow vs design extensions
| Feature | Basis | Priority |
|---|---|---|
| Third-person camera-relative locomotion, double jump | Kenney `scripts/player.gd` | P0 |
| Orbit rotation/zoom, camera smoothing | Kenney `scripts/view.gd` | P0 (zoom UI optional) |
| Trail dust, footstep pitch, idle/walk/jump, landing squash | Kenney `scripts/player.gd`, `objects/player.tscn` | P1 |
| Coin rotating/bobbing and particles | Kenney `objects/coin.gd` | P1 (adapt to paw stamps) |
| Falling platform fall acceleration and squash | Kenney `objects/platform_falling.gd` | P1 (1–2 platforms) |
| Small sound-player pool with pitch variance | Kenney `scripts/audio.gd` | P1 |
| Breakable brick from below, debris | Kenney `objects/brick.gd` | P2 |
| Cloud bobbing motion | Kenney `objects/cloud.gd` | P2 |
| Start/win/fail/paused menus, restart in place, gate, Paw Bounce | **Our added functionality** for test | P0 |
| Touch joystick/right drag/jump buttons | **Our Unity-only extension** | P0 |

## Explicit quality rules
- Player can finish entirely without collecting unrelated decorative coins; exactly three **required** stamps.
- Two jumps maximum per airborne sequence; bounce must interact predictably with extra jump (see Phase 04); tune level so bounce path still has value.
- UI tells player `Collect 3 stamps to unlock delivery!` and shows `0/3 ... 3/3`.
- Menu → Playing → Won/Failed → in-place Retry; Pause → Resume without unexpected input.
- Desktop + mobile camera look follow the same yaw/pitch state; no movement in UI-covered touch region.
- Pet character is recognizable from Cube Pets, not a Kenney demo robot; core 3D level uses required Platformer Kit.

## Non-goals
No save system, inventory, multiplayer, procedural worlds, cloud backend, custom shader graph pipeline, DOTS/ECS. Do not add automatic scene reload on failure.

## Acceptance gate
- [ ] Scope documented; no feature conflicts in plans.
- [ ] Level route and three stamp positions are clear.
- [ ] Required delivery package content confirmed.
- [ ] Project decisions are reflected in code phase tracker.
