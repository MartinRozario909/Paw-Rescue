# Sources, source audit and licensing

## Assignment (primary requirements)
- User-supplied `Nagorik Technical Test_ Pet Platformer.docx`, 2 pages. Deadline Sunday 2026-11-01, 23:59 Bangladesh; playable build + repository + 30–60 sec video + five-line note. This is the authoritative scope; additional game/engine choices in plans are **recommendations**, not test requirements.

## Reviewed source repository (primary implementation reference)
- GitHub: https://github.com/KenneyNL/Starter-Kit-3D-Platformer
- Reviewed commit: https://github.com/KenneyNL/Starter-Kit-3D-Platformer/commit/3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0
- Player logic: https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0/scripts/player.gd
- Camera: https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0/scripts/view.gd
- Audio pool: https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0/scripts/audio.gd
- Input definitions: https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0/project.godot
- HUD: https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0/scripts/hud.gd
- Coins: https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0/objects/coin.gd
- Falling platforms: https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0/objects/platform_falling.gd
- Breakable brick: https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0/objects/brick.gd
- Cloud: https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0/objects/cloud.gd
- Player nodes, particle/audio: https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0/objects/player.tscn
- Demo scene: https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/3fa8a04b1c01ab23db43123d4ce814a34c3fc7f0/scenes/main.tscn
- README/license: https://github.com/KenneyNL/Starter-Kit-3D-Platformer/blob/main/LICENSE.md

**Source-derived vs recommended:** `plans/12-kenney-godot-source-audit.md` distinguishes what Kenney's Godot scripts actually implement from our Unity implementation, additional mobile input and delivery objective. Repository code is MIT; included art/audio CC0 according to README. Unity C# logic should be newly written; preserve notice for copied code where applicable.

## Required Kenney packs (CC0)
- https://kenney.nl/assets/cube-pets
- https://kenney.nl/assets/platformer-kit
- https://kenney.nl/assets/ui-pack

## Official Unity technical documentation
- Unity CLI migration from older embedded MCP: https://docs.unity.com/en-us/unity-cli/replace-mcp-server-unity-cli
- Unity CLI + Pipeline: https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package
- Unity CLI releases/available commands: https://docs.unity.com/en-us/unity-cli/release-notes
- Unity onboarding for third-party agent: https://unity.com/onboardingsuccess/unity-ai
- Rigidbody: https://docs.unity3d.com/ScriptReference/Rigidbody.html
- Rigidbody.linearVelocity: https://docs.unity3d.com/ScriptReference/Rigidbody-linearVelocity.html
- Rigidbody interpolation: https://docs.unity3d.com/ScriptReference/Rigidbody-interpolation.html
- Physics.OverlapBox (ground check): https://docs.unity3d.com/ScriptReference/Physics.OverlapBox.html
- PhysicsMaterial (zero friction player): https://docs.unity3d.com/ScriptReference/PhysicsMaterial.html
- Collider trigger events (OnTriggerEnter): https://docs.unity3d.com/ScriptReference/Collider.OnTriggerEnter.html
- Unity Input System On-Screen controls: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.13/manual/OnScreen.html (confirm installed version)
- Unity Cinemachine Orbital Follow: https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineOrbitalFollow.html (confirm installed version)
- Unity URP: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest (confirm actual installed package)

## Independent implementation & disclosure guidance
- Original idea, three stamps, package-delivery gate and Paw Bounce are new design choices.
- Godot behavior descriptions are reference observations; C# pseudocode in plans is not pasted Godot implementation.
- Include `THIRD_PARTY_NOTICES.md` in shipped code if required by how source is reused. Cite Kenney packs for clarity even though CC0 doesn't require it.
- Technical test explicitly permits AI; disclose specific real uses honestly in the five-line submission note.
