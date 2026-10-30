# Paw Rescue

A short 3D platformer: a puppy hops across floating islands, collects coins to unlock the cages of its friends (Panda, Penguin and Cow), then stomps two waves of aliens in the Alien Camp.

## Run it
- **Unity 6000.6.0f1** with URP. Open the folder in Unity Hub.
- Open `Assets/Scenes/GameScene.unity` and press Play.
- Windows build: menu **Paw Rescue > Build Windows** writes `Builds/Windows/PawRescue.exe`.

## Controls
| Action | Keys |
|---|---|
| Move | W A S D |
| Jump / double jump | Space (release early for a short hop) |
| Camera | Arrow keys, or drag with the mouse |
| Pause | Esc or P, or the pause button |

## Goal
1. Collect coins. Each cage costs 10 coins (the level has 45).
2. Free all three friends. Each rescue sets a checkpoint.
3. The gate opens. Enter the Alien Camp; the gate shuts behind you.
4. Stomp both waves of aliens to win. Their kicks and the spikes and saw cost a heart; losing all 3 hearts is Game Over.

Retry, Play Again and Main Menu reset the level in place without reloading the scene: every object that changes during a run implements `IResettable`, and `GameManager` calls `ResetRunState()` on all of them.

## Code map (`Assets/Scripts`)
| Script | Job |
|---|---|
| `GameManager` | Game states (Menu, Playing, Paused, GameOver, Won), friends rescued, checkpoint, run time, reset |
| `PetController` | Rigidbody movement, double jump, stomp/spring bounce, knockback, riding moving platforms |
| `HealthSystem`, `Backpack`, `Item` | Hearts, pickups |
| `EnemyController` | Alien: guard or patrol between two spots, chase, kick, get stomped |
| `FriendCage`, `FinalGate`, `ArenaWaves` | Cages, the gate, the camp waves and the win |
| `MovingPlatform`, `FallingPlatform`, `Spring`, `Saw`, `Hazard` | Level mechanics |
| `UIManager` | HUD and menu screens |
| `SoundManager` | Pooled sound effects, generated in code |

`Assets/Editor/GameSceneBuilder.cs` lays out the whole level (menu **Paw Rescue > Build Game Scene**). Positions and numbers live there, so the level can be rebuilt after a change.

Tuning values to try live: `PetController` (run speed, jump height, max jumps), `EnemyController` (speed, search radius, kick timing), `FriendCage` (cost), `Spring` (bounce height).

## Assets and licences
- Kenney [Cube Pets](https://kenney.nl/assets/cube-pets), [Platformer Kit](https://kenney.nl/assets/platformer-kit) and [UI Pack](https://kenney.nl/assets/ui-pack): CC0.
- Game feel (camera, coin bob, dust trail, falling platform, sound pool) was modelled on Kenney's [Starter Kit 3D Platformer](https://github.com/KenneyNL/Starter-Kit-3D-Platformer) (MIT). It was reimplemented in C#; no code was copied.

## AI use
Claude (Anthropic) helped plan the design and wrote most of the C# and the level builder through Claude Code, under my direction. I reviewed the code and tested it in the Unity Editor.

## Tested
- Unity Editor Play Mode on Windows 11.
- Windows x64 build: launches without errors.
