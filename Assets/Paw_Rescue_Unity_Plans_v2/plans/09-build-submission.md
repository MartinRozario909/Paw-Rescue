# Phase 09 — Release builds, repository, gameplay video, five-line note

## Required deliverables (from test brief)
1. **Playable build**: web URL (itch.io/Netlify etc.), Android APK, **or Windows build**.
2. **Source code**: GitHub or GitLab URL.
3. **Gameplay video**: 30–60 seconds.
4. **Five-line note:** game in one sentence; what cut/why; what AI did; what would add in 2 more days; approximate hours spent.

**Deadline:** Sunday 1 November 2026, 11:59 PM **Asia/Dhaka**. Email reply is user's action; plan should provide final links and text, not fabricate sending.

## Windows build — primary
1. [ ] Verify Build Profiles contains `Bootstrap` then `Game` scenes in correct order; platform Windows x86_64.
2. [ ] Use Editor Build Profiles or installed Unity CLI command, **discovered via `unity --help`, `unity command`**. If invoking `-batchmode -executeMethod GameBuild.BuildWindows`, create well-scoped static Editor method and capture log/exit; do **not** run a concurrent Editor on same project.
3. [ ] Copy entire build folder (exe + `*_Data` + Unity Player/support files) and zip it; launch from a clean directory and play to Win and after Retry.
4. [ ] Set icon/title/window size, default quality to stable URP; no debug console left open.

## Android APK — recommended additional validation, NOT required if Windows submitted
1. [ ] Ensure Android Build Support, SDK/NDK/OpenJDK installed via Hub; set input control visibility, touchscreen, landscape orientation, safe area.
2. [ ] Build APK via discovered Editor/CLI command; test on physical device. **Do not promise Android compatibility based on Editor preview alone.**
3. [ ] If Android performance/control bugs threaten core submission, submit stable Windows build and document touch support not validated on hardware, rather than breaking required Windows deadline.

## Web build — optional, only if stable
Unity WebGL/Web build + URP may add time/compatibility risks; attempt only after Windows verified. Upload to itch.io HTML5 or other host, play live from an unrelated browser session, verify saved input pointers and fullscreen behavior. Use a stable build rather than a speculative web link.

## GitHub/GitLab source
- [ ] Commit all Unity project assets/scripts/package manifest/project settings and `.meta` files, source-generated build tools and `README.md`.
- [ ] Exclude Unity `Library`, `Temp`, `Obj`, `Logs`, `Build` (unless release artifact intentionally included), secrets, machine-local caches and generated platform install files.
- [ ] Repo README states Unity version, open/run/build steps, controls, objective, asset licenses, AI disclosure, observed tested build platforms.
- [ ] If substantive starter-kit code is copied, include copyright notice MIT in `THIRD_PARTY_NOTICES.md`; all three Kenney asset packs are CC0. Our design may be inspired by mechanics without copying code.

## 30–60-second video storyboard (example 45 sec)
- 0–6: main menu and clear goal.
- 6–14: keyboard/camera movement, first jump and double jump.
- 14–24: first stamp, running dust, bounce pad.
- 24–32: falling platform + stamp 3 / gate unlock.
- 32–39: delivery goal and win screen.
- 39–45: fail → in-place retry OR short Android touch segment. Use real captured game, not concept image.

## Five-line note template (fill real facts)
```text
1. Pawprint Express is a short 3D platformer where a puppy delivers a package by collecting three paw stamps across floating islands.
2. I cut [actual cut features] because [actual reason tied to quality/time].
3. I used AI for [actual planning/code/verification tasks]; I reviewed, implemented, and tested [actual personal work].
4. With two more days I would add [one or two feasible features].
5. I spent approximately [actual hours] hours on this project.
```

## Release sign-off
- [ ] Last build survives main menu → win, fail → retry → win; no loading/reload for retry.
- [ ] Source and Windows build reproduce final tested version/commit.
- [ ] Video 30–60 seconds, uploaded and accessible; links permission-tested.
- [ ] Five-line note is truthful, not placeholder; email ready before deadline.
