# Phase 02 — Scenes, prefabs, components, run-state architecture

**Exit:** Basic start/play/pause/win/fail lifecycle and in-place reset work without sprite polish or final level.

## Scene strategy
Two scenes for authoring simplicity: `Bootstrap.unity` with menu + bootstrap controller; `Game.unity` containing gameplay and its own HUD. **Once `Game.unity` has loaded, retry is always an in-place state reset**; only switching between MainMenu and Game scenes is allowed for navigation. Alternative: one scene with UI panels, if that is simpler to inspect; choose once and document.

```text
Game
├── RunRoot
│   ├── GameManager (run-state authority)
│   ├── RunTimer
│   ├── Player (CharacterController + camera anchor)
│   └── SpawnPoint
├── World
│   ├── StartGarden / BrokenCrossing / BounceGarden
│   ├── SkyBridge / DeliveryIsland
│   ├── Stamps[3], BouncePad, FallingPlatforms
│   ├── GoalGate, GoalTrigger, FallVolume
│   └── DecorativeClouds
├── Main Camera (CinemachineBrain)
├── CM_Orbit (CinemachineCamera + OrbitalFollow)
├── AudioRoot (AudioPool/AudioManager)
└── UI
    ├── Canvas + SafeAreaRoot (Kenney UI)
    ├── HUD + TouchControls
    ├── PausePanel
    ├── WinPanel
    └── FailPanel
```

## C# components and responsibilities
| Class | Owns | Must not own |
|---|---|---|
| `GameManager` | state machine, collected stamp ID set, time, Win/Fail/Reset | player input, animation visuals |
| `RunResetRegistry` | list of `IResettable.ResetRunState()` instances | gameplay state |
| `InputReader` | values/edges Move, Jump, Look, Pause | transform or physics updates |
| `TouchLookArea` | pointer-owned right swipe deltas | move/jump binding |
| `PlayerMotor` | CharacterController planar/vertical motion, double jump, bounce | particle artwork |
| `PlayerVisuals` | animation, mesh rotation, squash/stretch | speed/physics calculations |
| `OrbitCameraInput` | mouse/gamepad/touch look aggregation | follow target movement |
| `PawStamp` | trigger, bob/spin, visual hide & reset | global stamp count |
| `FallingPlatform` | trigger, fall acceleration, reset pose | scene reload |
| `BouncePad` | spring logic + reset of effect cooldown | gate unlock |
| `GoalGate` / `GoalTrigger` | visual lock and delivery trigger | run state timer |
| `EffectsPlayer` / `AudioPool` | VFX and queued/pooled playback | game rules |
| `UIController` | menus, HUD, touch control visibility | authoritative stamp counting |

## State transitions
```text
Menu -> Playing -> Paused -> Playing
             |            -> Menu (optional)
             +-> Won -> Retry -> Playing
             +-> Failed -> Retry -> Playing
```
- `Playing` is only state allowing input, collection, time increment, trigger activation.
- `GameManager.Win()` checks **3/3 stamps**; `Fail()` is idempotent.
- `GameManager.ResetRun()` transaction: set temporary resetting flag; stop timer/input; reset player pose + physics + remaining jumps + visuals; reset stamps, gate, springs, falling-platform position/scale/colliders/velocity, particles/audio as needed; reset camera orbit; clear counters; re-enable HUD; change state to Playing.
- Use event subscription/unsubscription carefully (`OnEnable`/`OnDisable`) and null-safe dependencies; avoid stale events across scenes.
- Use stable inspector IDs or explicitly serialized ID fields for stamps. Do not use runtime `FindObjectsOfType()` every frame.

## Build/test tasks
1. [ ] Create empty Game scene and startup HUD with state text, use reusable Prefabs.
2. [ ] Add `GameManager`, `RunResetRegistry`, `GameState` enum with strict transition guards.
3. [ ] Add stub player/resettable stamp, observable counter and timer.
4. [ ] Run simulated Win/Fail/Retry twice **while checking activeScene handle remains unchanged**.
5. [ ] Implement basic overlays and button hookups before styling.

## Gate
- [ ] No scene reload on Retry. Scene identity remains constant.
- [ ] Repeating Win/Retry, Fail/Retry does not double-register events or stamps.
- [ ] Paused/terminal states freeze gameplay input/timer while UI remains responsive.
