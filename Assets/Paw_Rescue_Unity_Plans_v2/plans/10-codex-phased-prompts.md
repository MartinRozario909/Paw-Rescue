# Phase 10 — Agent prompts for sequential Codex + Unity CLI/MCP execution

> Paste ONE prompt at a time. Agent must inspect existing project and update `plans/11-progress-tracker.md`. **Do not ask agent to implement the entire game at once.**

## Universal preamble (for every prompt)
```text
You are implementing Pawprint Express in the currently open Unity 6 URP project.
Read AGENTS.md, README.md, plans/12-kenney-godot-source-audit.md,
plans/13-porting-recipes.md and the current target phase plan. Do not start a
new project or replace existing user assets/settings. Use official Unity CLI
+ Pipeline (`unity command`/`unity eval`) as preferred automation; `unity mcp`
if structured access is appropriate. Discover installed commands/APIs before
using them. Do NOT install CoplayDev or deprecated Assistant MCP by default.
Implement only this phase. Compile, inspect Console, run appropriate Unity
Play Mode/tests, collect evidence, update tracker and report PASS/BLOCKED/
NOT VERIFIED. Avoid claiming unexecuted checks. Make a focused Git commit
only if the environment/repo allows it and there are no unrelated edits.
```

## Prompt 01 — Setup and asset import
```text
Execute plans/01-unity-cli-setup.md. Audit Unity version, URP, Input System,
Cinemachine, uGUI and existing scenes; configure official Unity CLI Pipeline
connection to Codex if available. Import/organize 3 REQUIRED Kenney packs.
Create a simple URP test scene; verify Editor recognizes prefabs/materials,
zero compilation errors. Show exact CLI/Editor tool operations and results.
```

## Prompt 02 — Bootstrap and lifecycle
```text
Execute plans/02-architecture-scenes.md. Add a minimal state machine,
GameManager, reset registry, timer, player spawn and stub stamps; ensure
Menu/Playing/Paused/Won/Failed. Implement ResetRun without SceneManager.LoadScene
or reloading scenes. Test consecutive fail/win/retry cycles and scene identity.
```

## Prompt 03 — Desktop + touch input
```text
Execute plans/03-input-desktop-touch.md. Create Input System action asset and
InputReader; desktop WASD/arrows/Space/mouse/Esc/R, virtual joystick, touch jump,
right look swipe, pause/retry. Handle distinct pointer IDs and UI hit testing.
Show how touch control scheme switching is avoided. Test gestures with Editor
simulation and mark physical Android validation pending until device tested.
```

## Prompt 04 — Kenney-inspired player + camera
```text
Execute plans/04-player-camera.md and consult source audit. Implement the
player with a dynamic Rigidbody (gravity from Project Settings, -25) and a
BoxCollider (zero-friction material); no CharacterController. Read input in
Update, set Rigidbody.linearVelocity in FixedUpdate with camera-relative
acceleration, double jump, buffer/variable height, MoveRotation facing and reset.
Use Cinemachine 3 Orbit/OrbitalFollow with one input owner; desktop mouse and
mobile drag share orbit state. Verify speed/units are NOT Godot frame-dependent
values. Playtest movement, double jump, pitch clamp and camera orbit in graybox.
```

## Prompt 05 — End-to-end level
```text
Execute plans/05-level-mechanics.md. Make one complete five-section graybox
level with three unique paw stamps, Pawn Bounce spring, 1-2 resettable falling
platforms, goal gate/trigger and fall zone. Reimplement Kenney coin spin/bob
and falling platform activation behavior in Unity. Verify Win only at 3 stamps,
Fail on fall, and in-place reset with every object restored. Do not add art yet.
```

## Prompt 06 — Particle/audio/animation reference port
```text
Execute plans/06-kenney-effects-audio.md. Implement speed-threshold dust trail,
land burst, idle stamp glitter/pickup burst, squash/stretch, step audio pitch,
and an AudioPool reused for jump/land/coin/other short sounds. Use the source
audit for exact reference behaviors and validate effects in URP. Add optional
breakable brick/cloud bob only if core gates pass and schedule permits.
```

## Prompt 07 — UI, art and URP polish
```text
Execute plans/07-ui-urp-polish.md. Replace blockout visuals with Kenney Cube
Pets puppy, Platformer Kit props, UI Pack sprites. Implement start/pause/win/fail
menus, timer/stamps HUD, responsive mobile controls/safe areas and coherent URP
lighting/materials. Keep level collisions and jump spacing unchanged from
playtest. Verify 16:9 and phone aspect screenshot evidence.
```

## Prompt 08 — QA
```text
Execute plans/08-tests-device-qa.md. Run EditMode & PlayMode tests for state,
double jump, resets, collect duplicate prevention, falling platforms and inputs.
Perform manual full runs and document actual hardware/device used for touch.
Reproduce and fix critical bugs. Record Editor/test log paths and screenshots.
```

## Prompt 09 — Release
```text
Execute plans/09-build-submission.md. Build and launch Windows x64, run full
flow in release; optionally build/test Android APK. Prepare source repo readme,
version/credits/AI disclosure, 30-60 sec video checklist and 5-line note with
actual hours. Do not invent upload links, gameplay footage, device checks,
or claim submission email sent. Report exact confirmed deliverables and gaps.
```
