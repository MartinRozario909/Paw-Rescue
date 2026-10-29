# Phase 11 — Progress tracker

> Update only after actual changes/tests. `[ ]` means incomplete or not verified; `[x]` requires evidence in the Notes table.

| Phase | Task | Status | Evidence / date |
|---|---|---|---|
| 00 | Brief, source audit and scope agreed | [x] | Plans read; implemented as written, 2026-10-28 |
| 01 | URP Unity project, official CLI/Pipeline connection | [x] | Unity 6000.6.0f1, URP 17.6.0; `unity pipeline install` → com.unity.pipeline 0.9.1-exp.1, `unity status` ready |
| 01 | All three required Kenney packs imported | [x] | Cube Pets (dog), Platformer Kit (blocks/props), UI Pack (cards/buttons/stick) all used in scene |
| 02 | Game lifecycle, in-place reset and timer | [x] | `GameManager` + `RunState` + `RunResetRegistry`; FullRunTests checks scene handle unchanged |
| 03 | Keyboard/mouse action mapping | [x] | `PawprintControls.inputactions` via `InputReader`; build + Play Mode run; keyboard not hand-tested by agent |
| 03 | Touch stick, jump, camera swipe (3-touch) | [ ] | Editor: stick→Move (0,1), Jump button→jump, look-drag pointer ownership verified via simulated pointer events. **Physical device NOT tested** |
| 04 | Camera-relative CharacterController + 2 jumps | [x] | PlayerMotorTests (double jump cap, held jump, bounce, diagonal, reset) pass |
| 04 | Cinemachine orbit, pitch clamp, zoom | [x] | Cinemachine 6.6.0 OrbitalFollow + Deoccluder, single writer `OrbitCameraInput`; touch drag yaw +46.7° observed |
| 05 | 5-area level traversable from start to finish | [x] | FullRunTests autopilot completes A→E using only Move/Jump input (19 s) |
| 05 | 3 stamps, gate, Bounce, fall/fail, resettable falling pad | [x] | FullRunTests + FallingPlatformTests; gate closed/open screenshots in Play Mode |
| 06 | Dust, landing burst, squash/stretch, audio pool | [x] | Implemented (`PlayerEffects`, `PlayerVisuals`, `AudioPool`, generated WAV SFX). Visual/audio feel not hand-tested |
| 07 | Kenney UI, menus, URP, puppy | [x] | Menu/HUD/Pause/Win/Fail captured from Play Mode, 1280×720 |
| 08 | EditMode/PlayMode tests and screenshots | [x] | EditMode 6/6, PlayMode 7/7 (in-Editor via Pipeline `test run`) |
| 08 | Windows and Android touch manual QA (as applicable) | [ ] | Human playtest pending; Android not built |
| 09 | Windows build tested outside Editor | [ ] | Built `Builds/Windows/PawprintExpress.exe` (0 errors); launched 15 s, no exceptions in Player.log. Full human playthrough pending |
| 09 | Source repo, video and 5-line note ready | [ ] | Not a git repo yet; video/note are user tasks |
| 09 | Submitted email by deadline | [ ] | |

## Phase execution record
| Phase/date | Commit SHA | Editor/CLI executed | Tests/results | Screenshots/logs | Blockers |
|---|---|---|---|---|---|
| Initial plan, 2026-10-28 | n/a | n/a | n/a | n/a | — |
| 01–09 build, 2026-10-28 | none (no git repo) | Unity CLI 1.0.0-beta.13 + Pipeline: `pipeline install`, `package add com.unity.cinemachine@6.6.0`, `recompile`, `eval PawWorldBuilder.BuildAll()`, `test run`, `build run` | EditMode 6/6, PlayMode 7/7 | Play Mode captures (not kept); Player.log at `%USERPROFILE%/AppData/LocalLow/DefaultCompany/Paw World/` | Editor auto-added `com.unity.ai.assistant` + `com.unity.ai.inference` to the manifest (not requested by the plan) |

## Mandatory regression checklist
- [x] Start→play→fail→Retry restores input/timer/ground/platforms/particles/stamps/gate. (FullRunTests)
- [ ] Start→play→3 stamps→gate→Win→Play Again works **three times** without loading scene. (automated once; human x3 pending)
- [x] Double jump cap of 2, input edges, bounce behavior, camera never fights player visual.
- [ ] Desktop move+orbit and mobile left-stick+look+jump simultaneously. (Editor simulation only)
- [ ] Particle trails and footsteps stop on idle, air, pause and fail. (implemented, not hand-verified)
- [x] No console exceptions, missing references or magenta URP materials. (Editor console clean; build log clean except stripped post-FX shader notices)
- [ ] Source + playable build + 30–60 s video + five-line note all ready.
