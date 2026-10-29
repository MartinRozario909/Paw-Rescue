# AGENTS.md — Pawprint Express Unity agent instructions (v2)

## Objective and constraints
Implement the plans in `README.md` + `plans/*.md` in **Unity 6 URP**, not Godot. Use Kenney's *Godot* source as a behavior specification. Target a polished, reliable **one-level** platformer before the Nagorik deadline. No code or test may be claimed complete unless actually verified.

## Tool policy
1. Inspect existing repo, `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt`, current scene and changed files first. **Never create another Unity project or overwrite scenes, .meta GUIDs, input assets, settings, or user changes without inspection.**
2. **Official Unity CLI + Pipeline are primary.** Discover actual tool surface (`unity --help`, `unity pipeline list`, `unity list`, `unity command`, Codex-installed skill), then choose CLI direct commands for routine work; use `unity mcp` only when structured MCP calls are useful. Do not guess unavailable commands or infer success from tool existence. See Phase 01.
3. Use source files for C# and `Editor/` generation scripts for *idempotent* prefab/scene creation. Drive Unity Editor via installed Pipeline to refresh/compile/execute; verify results in Editor/Play Mode, not merely filesystem existence.
4. If CLI/Pipeline is unavailable, **report the blocker and use ordinary Unity Editor UI**; do not silently install CoplayDev or the deprecated `com.unity.ai.assistant` MCP. Never claim CLI checks were run if not executed.
5. For classic Unity Editor `-batchmode` tests/builds, avoid concurrent writers to the same project (close interactive Editor first or create an isolated checkout); capture command, version, exit code, Editor.log, test results and artifact path.
6. Use **Input System only**; `InputSystemUIInputModule`, touch joystick + jump, right-side drag look, keyboard/mouse and optional physical gamepad. Handle multitouch ownership and input clearing after pause/fail/reset.
7. Use `CharacterController` and explicit gravity, double jump, and character yaw. **Do not attach Rigidbody locomotion**. Cinemachine 3 follows a separate CameraTarget; only one system owns orbit axes.
8. Preserve clear gameplay states `Menu, Playing, Paused, Won, Failed`; implement `ResetRun()` **without reloading Unity scenes**. Collectibles/falling platforms, camera, input, HUD, timer, gate and player all reset.
9. Godot behaviors: preserve design intent (double jump, trail threshold, coin bob, landing deformation, breaking bricks, fall platforms, audio pool), but use frame-independent Unity implementation. `250 * delta` from Godot is NOT Unity speed units; `CharacterController.Move` doesn't apply gravity.
10. Phase gating: do not proceed until compile passes, tests/smoke checks pass and evidence is added to tracker. User can stop between phases. Avoid unrelated features and packages.

## Format of phase report
`Phase XX: PASS / BLOCKED / NOT VERIFIED` with changed files, exact agent/Editor/CLI actions, Unity version, compile results, Play Mode proof and screenshots/log locations, input devices tried, blockers and next phase. Do not label an untested visual as verified. Update checklist and make a focused commit.

## Asset & license policy
- Required primary packs: [Cube Pets](https://kenney.nl/assets/cube-pets), [Platformer Kit](https://kenney.nl/assets/platformer-kit), [UI Pack](https://kenney.nl/assets/ui-pack). CC0 per Kenney.
- Starter kit *code* is MIT; if copying/adapting substantial source, preserve MIT notice in distribution and `THIRD_PARTY_NOTICES.md`. Prefer independent Unity implementation. Starter kit's bundled art/audio are stated CC0.
- Do not claim the starter kit supplies Cube Pets assets or that its sample level must be ported. Avoid adding original Godot assets to the Unity project unless necessary and provenance is recorded.

## When late
Cut optional *brick, clouds, moving platforms, zoom UI and score medals* before any mandatory state, scene, control or submission component. Do not cut in-place restart, three required Kenney packs, tested desktop build or source/video/note.
