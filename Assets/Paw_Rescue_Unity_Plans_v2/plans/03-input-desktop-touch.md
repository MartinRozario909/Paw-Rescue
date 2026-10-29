# Phase 03 — Unified keyboard, mouse, touch and optional gamepad

**Entry:** Scene/manager scaffolding exists. **Exit:** keyboard + dual-touch movement/look/jump and pause/restart reliably operate same gameplay systems.

## Input architecture
Unity **Input System** only. Action asset: `Assets/_Game/Settings/Input/PawprintControls.inputactions`. Use `PlayerInput` or an explicit `InputActionAsset` through `InputReader`; **do not let both execute actions**. Prefer explicit `InputReader` so mobile synthetic gamepad and keyboard can coexist without uncontrolled control-scheme switching.

| Action | Kind | Desktop | Physical gamepad | Mobile |
|---|---|---|---|---|
| `Move` | Value Vector2 | WASD and arrows composite | left stick | `OnScreenStick` virtual `<Gamepad>/leftStick` |
| `Jump` | Button | Space | south button | `OnScreenButton` virtual `<Gamepad>/buttonSouth` |
| `Look` | PassThrough Vector2 | `<Mouse>/delta` | `<Gamepad>/rightStick` | pointer-owned `TouchLookArea` drag delta |
| `Pause` | Button | Esc | Start | Kenney UI Button |
| `Restart` | Button | R (terminal state only) | menu retry | Kenney Retry button |
| `Zoom` | Value/axis (optional) | mouse scroll | triggers | pinch optional/cut |
| UI | UI map | mouse click, keyboard submit | gamepad selection | touches through InputSystemUIInputModule |

**Important:** Mouse delta = pixels per frame; right stick = normalized rate. Normalize/look-sensitivity by device source; **never multiply mouse delta by `deltaTime` twice**. InputReader emits `JumpPressed` edge, `JumpReleased` edge, and `Move` continuous value. Clear held/input edges on pause, focus loss and reset.

## 1. Desktop
1. [ ] Create `Gameplay` and `UI` maps; add bindings and binding display labels.
2. [ ] WASD/arrows map to XZ camera-relative world direction in Motor, not raw world axes.
3. [ ] Mouse look freely orbits only in `Playing`. For desktop, lock/hide cursor while playing; reveal for Pause, Won, Failed, Menu. Start screen shows `Mouse: Look`; choose no right-button modifier.
4. [ ] Sample held inputs once per frame (Update); route to PlayerMotor + OrbitCameraInput. Disallow gameplay input when Unity app loses focus.
5. [ ] Verify keyboard and mouse simultaneously while jump is held/released; no stick drift or camera snap.

## 2. Touch layout (landscape, two-thumb)
```text
┌──────────────────────────────────────────────────────┐
│ Stamps 0/3                Timer                 Pause│
│                                                      │
│          [game world]       RIGHT SIDE = LOOK AREA   │
│                                                      │
│    (virtual stick)                         (JUMP)    │
└──────────────────────────────────────────────────────┘
```
1. [ ] Add `Canvas` (Screen Space Overlay), `CanvasScaler` Scale With Screen Size (1280×720 reference), `GraphicRaycaster`, **`InputSystemUIInputModule`**, safe area root, `MobileControls` panel.
2. [ ] Use `OnScreenStick` on left with `controlPath=<Gamepad>/leftStick`. Use a visible fixed outer/inner ring from Kenney UI Pack; size >= 130 px effective at reference resolution, accessible for thumbs.
3. [ ] Use `OnScreenButton` on right with `controlPath=<Gamepad>/buttonSouth`; make hit area sufficiently large; place away from look zone. UI pause button occupies top-right.
4. [ ] Create `TouchLookArea` (`IBeginDragHandler`, `IDragHandler`, `IEndDragHandler`, `IPointerDownHandler`, `IPointerUpHandler`) covering **unobstructed** right half of gameplay screen. Use `PointerEventData.pointerId` ownership: first valid pointer captures and releases only itself. Ignore other fingers and overlay UI. Accumulate `eventData.delta` once; clear consumed delta each frame.
5. [ ] Ensure look area does not intercept jump/pause clicks: correct Canvas raycast order (buttons above look area), transparent `Image.raycastTarget=true` only over intended look region; gameplay panels/labels generally `raycastTarget=false`.
6. [ ] For UI dialogs, hide/deactivate look layer and sticks; release captured pointer IDs on state change, focus loss, app pause, disable, orientation/resolution change.
7. [ ] If `OnScreenStick` causes control-scheme auto-switch touch cancellation, do not use `PlayerInput` auto-switch: bind actions across `<Gamepad>` + keyboard explicitly, or configure Unity's documented isolated `OnScreenStick` behavior; test actual device.
8. [ ] Apply touch look sensitivity in degrees/pixel normalized to reference resolution so it feels similar across DPI; clamp pitch via camera system.
9. [ ] Hide mobile control visuals for standalone Windows by runtime platform and an explicit Editor override for testing; **inputs work even if display is hidden**.

## 3. Gamepad and pointer ownership
- Physical gamepad is optional but easy through same actions. Avoid binding mouse delta and gamepad stick into a single value and blindly scaling as though they share units. `OrbitCameraInput` should know source.
- For pinch zoom, if implemented, reserve two pointers **inside right look region** and do not conflict with joystick/jump; **cut pinch zoom** if it complicates the deadline. Mouse wheel zoom remains optional.

## 4. Multi-input tests
- [ ] **2 fingers:** left stick held forward, right thumb drag rotates camera; puppy keeps moving smoothly.
- [ ] **3 touches:** left stick held, right camera drag, jump tap; jump triggers exactly once.
- [ ] Swipe beginning on Jump or Pause **never rotates** camera.
- [ ] Lift/look finger but keep joystick finger: move remains continuous.
- [ ] Pause mid-joystick: no input remains stuck on resume.
- [ ] On Android, a new touch replacing old pointer ID works; no `NullReferenceException`.
- [ ] Keyboard and mouse remain active when running Windows; no device-specific assumptions in Motor.

## Exit gate
A short obstacle run must be possible with both desktop and touchscreen inputs. Device physical validation in Phase 08 is distinct from Editor mouse simulation; do not declare mobile PASS from Unity Remote or mouse simulation alone.
