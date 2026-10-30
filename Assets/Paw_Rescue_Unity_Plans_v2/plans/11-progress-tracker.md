# Phase 11 — Progress tracker

> Update only after actual changes/tests. `[ ]` means incomplete or not verified; `[x]` requires evidence in the Evidence column.
> The game is **Paw Rescue** (see `PAW_RESCUE_GAME_DESIGN.md`). The older Pawprint Express rows were removed: that build is not in this repository.

| Area | Task | Status | Evidence / date |
|---|---|---|---|
| Setup | Unity 6000.6.0f1 URP project, three Kenney packs | [x] | Cube Pets (dog, panda, penguin, cow), Platformer Kit (blocks, alien, props), UI Pack (buttons, stars, hearts) all used in `GameScene` |
| Player | Rigidbody movement, double jump, camera, animation, dust, squash/stretch | [x] | `PetController`, `CameraFollow`, `PetAnimationController`, `PetTrail`, `PetSquash` |
| Player | Hearts, kick/hazard damage, knockback, fall → checkpoint respawn | [x] | Play Mode 2026-10-30: spikes cost 1 heart; fall respawned at start (no checkpoint) |
| Enemies | Alien guard/patrol, kick, stomp, reset | [x] | Play Mode 2026-10-30: 8 arena aliens stomped; all 13 back alive after Retry |
| Level | `GameScene` with all zones from the design doc | [x] | Built by **Paw Rescue > Build Game Scene**; 45 coins, 2 hearts |
| Level | Friend cages (10 coins), checkpoint, gate at 3/3 | [x] | Play Mode 2026-10-30: "Need 10 coins" hint with 1 coin; opened and spent 10 with 11; gate opened only on the 3rd friend |
| Level | Moving, falling and spring platforms, saw, spikes | [x] | Play Mode 2026-10-30: platform carried the pet a full cycle; falling platform dropped and hid; spring launched the pet's feet above 6.6 m |
| Camp | Gate locks behind, 2 waves, win | [x] | Play Mode 2026-10-30: wave 1 (3) then wave 2 (5), Won with stats "TIME 1:12 COINS 4" |
| States | Menu, Playing, Paused, GameOver, Won; Retry/Play Again/Main Menu without scene reload | [x] | Play Mode 2026-10-30: scene handle unchanged after Retry; Pause sets timeScale 0, Resume 1 |
| UI | Kenney UI start, HUD (hearts, coins, friends, waves, messages, pause), pause, game over, win | [x] | Objects built and wired by the builder; button clicks not hand-tested |
| Audio | Pooled generated sound effects | [ ] | Implemented (`SoundManager`); not listened to |
| Build | Windows build | [x] | `Builds/Windows/PawRescue.exe`, 0 errors, 128 MB; launched 15 s, no exceptions in Player.log |
| QA | Human playthrough with keyboard and mouse, start to win | [ ] | All Play Mode checks above were driven by scripts (teleporting the pet), not by playing |
| QA | Jump distances and difficulty tuned by hand | [ ] | Gaps are 2.8 m or less, from the calculated jump range; not hand-tested |
| Submit | Source pushed, 30–60 s video, five-line note, email | [ ] | User tasks |

## Mandatory regression checklist
- [x] Start → play → fail → Retry restores coins, cages, enemies, gate, waves and the pet without reloading the scene.
- [ ] Start → 3 friends → gate → camp → Win → Play Again works **three times** by hand. (Script-driven once.)
- [x] Double jump cap of 2, stomp bounce keeps one air jump.
- [ ] Particle trails and sounds stop on pause and fail. (Not hand-verified.)
- [x] No console exceptions in Play Mode; no exceptions in the build's Player.log.
- [ ] Source + playable build + 30–60 s video + five-line note all ready.
