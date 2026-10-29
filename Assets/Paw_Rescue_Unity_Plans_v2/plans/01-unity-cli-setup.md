# Phase 01 — Unity URP project, official CLI/MCP and Kenney assets

**Entry:** Existing project location known or fresh Unity project intentionally created. **Exit:** Editor has URP project, recognized tool bridge, required packs present, zero compilation errors.

## 1. Project / packages
1. [ ] Inspect `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, repository status; use the existing Unity project when present.
2. [ ] If creating new project, select **Unity 6.3 LTS / Universal 3D**. Confirm installed editor and URP template rather than assuming.
3. [ ] Install/verify `com.unity.render-pipelines.universal`, `com.unity.inputsystem`, `com.unity.cinemachine` (v3 installed in Package Manager), `com.unity.ugui`, Unity Test Framework/Test Runner.
4. [ ] Set **Active Input Handling = Input System Package (New)**; Editor may restart. Set 3D physics layers, color space (Linear when supported), MSAA and URP pipeline asset as needed.
5. [ ] Set target 1280×720 for desktop reference and adaptive aspect for mobile; choose portrait? **No, landscape**.

## 2. Official Unity automation — discovery-first
Official docs: [Unity CLI migration](https://docs.unity.com/en-us/unity-cli/replace-mcp-server-unity-cli), [Pipeline install](https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package). The older `com.unity.ai.assistant` embedded MCP server is deprecated; CLI MCP is supported.

In PowerShell **from project root**, after installing Unity CLI from official instructions and opening the target project:

```powershell
unity --version
unity auth login
unity pipeline install
unity pipeline list
unity list
unity mcp configure codex
unity skill install codex
```

6. [ ] For agents with terminal access, discover and favor `unity command` and `unity eval`; for MCP-only agents use `unity mcp` registered with Codex. **Never invent command names or argument schema:** run `unity --help`, `unity command --help`, `unity list`, and installed agent skill first.
7. [ ] Verify the CLI controls **the correct open project**, knows scene/prefab tools, can read Editor state and collect compilation/Console status. Create a disposable test object, observe it in Hierarchy, then remove it.
8. [ ] If CLI install/auth/MCP fails, document failure and use Editor manually. Do not add CoplayDev as a silent dependency. Use classic Editor `-batchmode` only with a safe lock strategy.

## 3. Import Kenney packs (all three mandatory)
- [Cube Pets](https://kenney.nl/assets/cube-pets): choose and preview a **dog/pup**, inspect actual animation clips and import orientation/scale; do not assume names like `Run` until inspected.
- [Platformer Kit](https://kenney.nl/assets/platformer-kit): stage grassy islands, platforms, fences, goal house props where available; create simple collider proxies.
- [UI Pack](https://kenney.nl/assets/ui-pack): use sprites for menu buttons, cards, HUD icons, prompts and pause states, with sliced borders as needed.
- The Godot starter repository is a **behavior reference**, not an additional required asset pack. Imported Godot `.tscn`/`.gd` files won't run in Unity.

## 4. Folder layout
```text
Assets/_Game/
  Art/{KenneyCubePets,KenneyPlatformerKit,KenneyUI,Materials}
  Audio/{Music,SFX}
  Prefabs/{Player,Level,Interactables,VFX,UI}
  Scenes/{Bootstrap,Game}
  Scripts/{Core,Input,Player,Camera,Gameplay,Effects,UI,Editor,Tests}
  Settings/{Input,URP}
  Tests/{EditMode,PlayMode}
```
Keep imported source art unmodified wherever possible; put variants/materials into `_Game`.

## 5. Repo sanity
- Add Unity `.gitignore` for `Library/`, `Temp/`, `Obj/`, `Logs/`, `Build/` unless builds intentionally published as release artifacts.
- Commit `Assets/`, `Packages/`, `ProjectSettings/`, all `.meta`; avoid committing Unity cache or massive source reference repository.
- Record Unity exact version and package versions once Editor reports them.

## Gate
- [ ] URP scene visibly renders and is assigned on quality levels.
- [ ] CLI/Pipeline or manual fallback documented, project ID/path confirmed.
- [ ] All three official asset packs imported and provenance recorded.
- [ ] Unity Console has **zero compilation errors**.
