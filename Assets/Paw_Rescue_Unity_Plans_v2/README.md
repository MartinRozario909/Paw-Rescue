# Pawprint Express — Complete Unity 6 URP Implementation Plan (v2)

**Planning document only.** No Unity scene, script, successful test or build is asserted to exist yet.

- **Assignment:** Nagorik technical test; one short complete 3D *pet* platformer.
- **Game:** `Pawprint Express: The Lost Delivery` — puppy delivers a parcel across floating islands, collecting **3 gold paw stamps** to open the delivery gate.
- **Primary build:** Windows x64. **Additional touch validation:** Android APK if toolchain/device available; the assignment accepts a web link, APK or Windows build (not all three).
- **Deadline:** **2026-11-01 23:59 Asia/Dhaka (Bangladesh).** Build a submission buffer before that time.
- **Engine:** Unity 6.3 LTS preferred, C#, **URP** (3D Universal template). Use installed/verified package versions, not speculative pinning.
- **Automation:** **Unity official CLI + Unity Pipeline**, `unity command`/`unity eval` when agent can use shell; `unity mcp` when MCP is needed. Codex skill for discovery; human Editor validation remains mandatory. CoplayDev is NOT a dependency.
- **Unity modules:** Input System, Cinemachine 3, uGUI, Unity Test Framework, URP. No DOTS/ECS, DOTween, bespoke physics engine, or third-party character controller.
- **Game feel influence:** audited KenneyNL Godot 4.6 [Starter Kit 3D Platformer](https://github.com/KenneyNL/Starter-Kit-3D-Platformer), pinned for comparison to [`3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0`](https://github.com/KenneyNL/Starter-Kit-3D-Platformer/commit/3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0). **Reimplement behavior idiomatically in Unity**; do not copy GDScript verbatim.

## Start here

1. Read [`AGENTS.md`](AGENTS.md), [`plans/00-brief-and-decisions.md`](plans/00-brief-and-decisions.md), and [`plans/12-kenney-godot-source-audit.md`](plans/12-kenney-godot-source-audit.md).
2. Inspect any existing Unity project **before** touching assets/settings; do not create a duplicate or destroy user edits.
3. Follow numbered phases in order; each phase has steps and an **acceptance gate**.
4. Implement **one input path for movement/jump** across keyboard and virtual controls, and one **camera input aggregator** for mouse/gamepad/right-side touch.
5. First prove **start → play → collect three → win; fall → fail; retry in place**. Then spend remaining time on polish.
6. After every phase: compile, inspect Console, Play Mode smoke test, record proof, update [`plans/11-progress-tracker.md`](plans/11-progress-tracker.md), commit.

## Plan index

| File | Stage / purpose |
|---|---|
| [`AGENTS.md`](AGENTS.md) | Mandatory Codex/agent execution rules |
| [`00-brief-and-decisions`](plans/00-brief-and-decisions.md) | Scope, required vs cut, playable loop |
| [`01-unity-cli-setup`](plans/01-unity-cli-setup.md) | Official Unity CLI/MCP, URP and assets |
| [`02-architecture-scenes`](plans/02-architecture-scenes.md) | Scene, prefab, state/event/reset architecture |
| [`03-input-desktop-touch`](plans/03-input-desktop-touch.md) | Keyboard, mouse, controller, two-thumb mobile |
| [`04-player-camera`](plans/04-player-camera.md) | Unity movement, double jump, Cinemachine orbit |
| [`05-level-mechanics`](plans/05-level-mechanics.md) | Islands, stamps, bounce, falling platforms, goal |
| [`06-kenney-effects-audio`](plans/06-kenney-effects-audio.md) | Dust, stars, squash/stretch, audio pool, clouds |
| [`07-ui-urp-polish`](plans/07-ui-urp-polish.md) | Kenney UI, URP lighting, pet asset, presentation |
| [`08-tests-device-qa`](plans/08-tests-device-qa.md) | Deterministic tests, desktop/touch, screenshots |
| [`09-build-submission`](plans/09-build-submission.md) | Windows, Android optional, GitHub, video, note |
| [`10-codex-phased-prompts`](plans/10-codex-phased-prompts.md) | Copy-ready prompts and agent checkpoints |
| [`11-progress-tracker`](plans/11-progress-tracker.md) | Milestones, evidence, blockers, sign-off |
| [`12-kenney-godot-source-audit`](plans/12-kenney-godot-source-audit.md) | Real source files, exact behavior, Unity equivalents |
| [`13-porting-recipes`](plans/13-porting-recipes.md) | Units, delta-time, code logic / pseudocode and pitfalls |
| [`14-risks-and-cut-order`](plans/14-risks-and-cut-order.md) | Timebox, risks and fallback policies |
| [`SOURCES.md`](SOURCES.md) | Primary source links, attribution/license notes |

## Sequence and delivery gates

| Order | Deliverable | Minimum completion proof |
|---|---|---|
| 01 | Open Unity URP project + verified agent bridge | `unity list` / equivalent, imported assets, zero compiler errors |
| 02 | Scenes, prefabs, run state | Start/play/win/fail/retry states simulate without reloading |
| 03 | Keyboard + multitouch input | Two simultaneous touch pointers do not interfere |
| 04 | Player and camera | Run, stop, jump twice, orbit 360°, zoom clamps |
| 05 | Entire graybox level | All 3 stamps, bounce, fail, win, in-place retry |
| 06 | Kenney-inspired feedback | Movement dust, coins, landing, footsteps and pooled SFX |
| 07 | UI + final assets + URP polish | Required Kenney packs used; legible 16:9 and phone UI |
| 08 | QA | Desktop and touch/device test report; no critical bugs |
| 09 | Submission | Tested build, code URL, 30–60 s video, 5-line note |

## Date-based schedule (planning targets, Bangladesh time)

| Date | Target |
|---|---|
| Wed 28 Oct | Unity setup, inspected reference, core controller and camera |
| Thu 29 Oct | Desktop/touch inputs, complete graybox, restart and transitions |
| Fri 30 Oct | Art integration, source-inspired particles/audio, UI, URP |
| Sat 31 Oct | Device QA, fixes, build, gameplay capture |
| Sun 1 Nov | Final verification, release and email before 23:59 BDT |

**Main constraint:** source reference is for *behaviors* and implementation inspiration, not a mandate to reproduce Kenney's demo level. Requirements come from the provided test document, and our game theme/stamps/bounce are original design choices.
