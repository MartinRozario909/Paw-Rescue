# Paw Rescue — Game Design Document

**Status:** Design for development, written 2026-10-29. **This story replaces the "Pawprint Express" delivery and paw-stamp story** in `README.md` and `plans/00`–`plans/05`. The technical rules in those plans still apply: Unity 6 URP, Input System, **Rigidbody and colliders for physics**, restart without scene reload, and the three required Kenney packs.

**Deadline:** Sunday 2026-11-01, 23:59 Bangladesh time.
**Style reference:** Kenney 3D Platformer Starter Kit. The game uses that kit's look and feel (a hopping character, spinning coins, a dust trail, squash and stretch, and a smooth orbit camera). It does not copy the kit's level.

---

## 1. Story

A brave puppy's friends, **Panda**, **Penguin** and **Cow**, have been locked in cages by a gang of grumpy **aliens**. The puppy hops across floating islands collecting **gold coins** and uses them to unlock each friend's cage. When all three friends are free, the gate to the **Alien Camp** opens. The puppy must defeat every alien in the camp to end the game.

**One-line pitch:** Hop, stomp and collect coins to free your friends, then clear the Alien Camp.

---

## 2. Core gameplay loop

1. Explore an island and collect gold coins.
2. Avoid obstacles, and stomp the aliens or dodge their kicks.
3. Reach a friend's cage with enough coins and unlock it. The friend is rescued.
4. Repeat for all three friends.
5. The final gate opens. Enter the Alien Camp and defeat all the aliens.
6. You win.

Losing all 3 hearts means **Game Over**. Retry resets the level in place, without reloading the scene.

---

## 3. Player (the puppy)

| Feature | Behaviour | Status |
|---|---|---|
| Move | WASD, relative to the camera, with smooth acceleration | Done (`PetController`) |
| Jump | Space. Double jump. Releasing Space early gives a short hop. A press just before landing is buffered. | Done |
| Physics body | Dynamic **Rigidbody** (gravity on, rotation frozen, Interpolate, Continuous Dynamic) and a **BoxCollider** (1.3 × 1.6 × 1.38) with the zero-friction `PetPhysics` material. The box stands on the ground, blocks walls and collects coins. | Done |
| Gravity | Rigidbody gravity of **-25**, set in Project Settings > Physics, for a snappy Kenney-style arc | Done |
| Camera | Follow-orbit camera. Rotate with arrow keys, mouse drag or touch drag. | Done (`CameraFollow`) |
| Animation | Idle, Run and Jump switch instantly. Win and Lose triggers exist. | Done (`PetAnimationController`) |
| Dust trail | Shows while grounded and running at 75% speed or more | Done (`PetTrail`) |
| Health | **3 hearts** | Done (`HealthSystem`) |
| Stomp | Landing on an alien's head defeats it and bounces the puppy up | Done (`EnemyController`, `PetController.Bounce`) |
| Hurt | An alien's kick or a hazard costs 1 heart, knocks the puppy back and gives 1.5 s of invulnerability (the puppy blinks) | Kick done (`PetController.Knockback`); hazards to do |
| Fall | Falling off the world costs 1 heart and respawns the puppy at the last checkpoint | Done, but respawns at the start until checkpoints exist |
| Squash and stretch | Stretches on jump and squashes on landing, visual only | Nice to have |

### Health rules
- The puppy starts with **3 hearts**. The maximum is 3.
- Damage sources are an alien's kick, spikes, saws and falling off the world. Bumping into an alien's body does no damage.
- After a hit, the puppy is **invulnerable for 1.5 s**, so one touch can't drain several hearts.
- At **0 hearts** the game enters the **Game Over** state and the Retry button appears.
- *Optional:* a `heart.fbx` pickup in a hard-to-reach spot restores 1 heart.

### Stomp rules
- A stomp counts when the puppy lands on the alien and its feet are **at or above the top of the alien's head** (within a 0.25 m `stompMargin`).
- A stomp costs the alien 1 health (default 1, so one stomp defeats it) and bounces the puppy up by about 1 jump height. After the bounce the puppy still has **1 air jump**, so it can chain stomps.
- Any other contact does nothing. Only the kick hurts.

---

## 4. Coins and rescuing friends

| Item | Model | Rule |
|---|---|---|
| Gold coin | `coin-gold.fbx` | +1 coin. Spins and bobs (Kenney coin style). Plays a sound and a sparkle on pickup. |
| Friend cage | Cube Pets `animal-panda`, `animal-penguin`, `animal-cow` inside a cage or fence ring with a `lock.fbx` | Needs **10 coins** to open. Walk into it with enough coins and it opens automatically. |
| Checkpoint | `flag.fbx` beside each cage | Rescuing a friend sets the respawn point there. |

- The whole level has **about 45 coins** and the cages need 30 in total. This gives a buffer, so the player never gets stuck.
- Unlocking a cage **spends** the coins. The HUD counter drops by 10.
- A cage without enough coins shows `Need 10 coins` above it.
- A rescued friend plays a happy hop and a burst of stars, then follows or waits near the final gate. It's cosmetic only and has no AI pathing.
- Friends rescued are shown on the HUD as 3 animal icons that light up.

---

## 5. Enemies (the grumpy aliens)

There is **one enemy type: the Alien**. It uses the Platformer Kit model `character-oodi.fbx` and is the `Enemy` prefab (`Assets/Prefabs/Enemy.prefab`, `EnemyController`, `EnemyAnimator`). Aliens walk on the ground and **kick** the puppy. There are no flying or charging enemies.

Each alien is set up in the Inspector in one of two modes:

| Mode | Behaviour | Status |
|---|---|---|
| **Guard** | Waits at its start spot. When the puppy comes within the search radius (6 m), it walks over and kicks. If the puppy gets away, it walks back to its spot. | Done (`EnemyController`) |
| **Patrol** | Tick **Will Patrol** and set **Spot1** and **Spot2**. The alien walks back and forth between the two spots and rests at each one for a random time between **Time To Stand Min** and **Max**. When the puppy comes within the search radius, it leaves the route, walks over and kicks. If the puppy gets away, it goes back to the spot it was heading for and carries on. A ledge in the way makes it turn round. | Done (`EnemyController`). With Will Patrol off, or a spot empty, the alien behaves as a Guard. |

`EnemyController` runs these states: **Idle** (standing at its spot or resting at a patrol spot), **Patrolling**, **Attacking** (walking up to the puppy and kicking), **GoingBack** and **Death**.

**Kick rules (done):**
- When the puppy is within the attack range (1.5 m), the alien stops, turns to face the puppy and kicks. Kicks alternate between the right and left leg.
- A kick hurts only while the foot is out in front (0.2–0.4 s into the kick animation) and only if the puppy is inside the `AttackCollider` trigger. Each kick can hit once.
- A kick costs 1 heart and knocks the puppy back (`PetController.Knockback`).
- There is a 1 s cooldown between kicks, so the puppy has time to jump onto the alien's head.

**Other rules:**
- Aliens do **not** walk off ledges (a ray checks for ground ahead). Movement stays on their own island.
- Bumping into an alien's body does no damage. Only the kick hurts.
- A stomp plays the death animation. After 1 s, the alien's model is hidden and its colliders are disabled. **It is not destroyed**, so Retry can bring it back.
- Speed, search radius, attack range, kick timing and the number of stomps to defeat an alien are all tunable in the Inspector.
- *Optional:* the other Platformer Kit aliens (`character-oobi`, `-ooli`, `-oopi`, `-oozi`) can be used as colour variants. Check first that their animation clips work with `EnemyAnimator`.

---

## 6. Obstacles and hazards (Platformer Kit)

| Obstacle | Model | Behaviour |
|---|---|---|
| Spikes | `trap-spikes.fbx`, `spike-block.fbx` | Static. Touching costs 1 heart. |
| Saw | `saw.fbx` | Spins and moves along a line between two points. Costs 1 heart. |
| Moving platform | `block-moving.fbx` | Moves between two points. The puppy rides it. |
| Falling platform | `platform.fbx` | Shakes, then drops 0.5 s after the puppy lands. Resets on Retry. |
| Spring | `spring.fbx` | Launches the puppy high to reach upper islands. |
| Gaps | — | Falling off costs 1 heart and respawns the puppy at the checkpoint. |

---

## 7. Level layout (one scene: `Scene1`)

```text
[Start Meadow] → [Zone 1: Panda] → [Zone 2: Penguin] → [Zone 3: Cow] → [Gate] → [Alien Camp]
```

| Area | Teaches / tests | Contents | Coins |
|---|---|---|---|
| **Start Meadow** | Move, camera, jump, double jump | Safe wide island, a coin line, a sign | 5 |
| **Zone 1 — Panda Grove** | Stomping | 1 Guard alien, then 1 Patrol alien, spikes, small gaps, Panda cage | 13 |
| **Zone 2 — Penguin Peak** (snow blocks) | Timing | Moving platforms, a falling platform, 1 Patrol alien on the landing island, Penguin cage | 13 |
| **Zone 3 — Cow Pasture** | Combining skills | A Saw, 2 Patrol aliens with crossing routes, a Spring to an upper island, Cow cage | 14 |
| **Gate** | Goal check | A `door-large` stays closed until 3/3 friends are rescued | — |
| **Alien Camp** (final arena) | Final test | Fenced arena. **2 waves**: Wave 1 is 3 Guard aliens; Wave 2 is 2 Guard and 3 Patrol aliens. The gate behind the player locks during the fight. | — |

The game is won when **all aliens in the Camp are defeated**. The rescued friends cheer and the **Win** screen appears.

Design rules:
- Measure gap sizes from the real jump distance in Play Mode. Don't guess them.
- The next goal (cage, gate or camp) should always be visible from the current area.
- Islands should be wide with clear edges. Readability matters more than difficulty.

---

## 8. Game states and UI (Kenney UI Pack)

```text
Menu → Playing ⇄ Paused
          ├→ GameOver → Retry → Playing   (in-place reset, no scene reload)
          └→ Won      → Play Again → Playing
```

| Screen | Contents |
|---|---|
| **Start** | Title "Paw Rescue", the puppy, the goal ("Collect coins, free your friends, clear the Alien Camp!"), controls, Play and Quit |
| **HUD** | ❤❤❤ hearts, coin count, 3 friend icons, wave counter in the Camp, pause button |
| **Pause** | Resume, Restart, Main Menu |
| **Game Over** | "Oh no!", Retry, Main Menu |
| **Win** | "All friends saved!", time, coins, Play Again, Main Menu |

Rules:
- Only the `Playing` state allows movement, damage, pickups and the timer.
- Retry or Play Again resets **everything** through `ResetRunState()`: player, hearts, coins, cages, friends, enemies, platforms, gate, camp waves and camera.
- The cursor is free in menus.

---

## 9. Code architecture

| Script | Responsibility | Status |
|---|---|---|
| `PetController` | Rigidbody movement in `FixedUpdate`, ground check, double jump (the Rigidbody does gravity). `Bounce()` (stomp) and `Knockback(Vector3)` (kick) set `Rigidbody.linearVelocity`. | Done |
| `PetAnimationController` | Animator parameters | Done |
| `CameraFollow` | Orbit and follow camera | Done |
| `PetTrail` | Running dust | Done |
| `PlayerHealth` | Hearts, invulnerability, blink, death event | New |
| `GameManager` | State machine, coins, friends rescued, checkpoint, win/lose, `ResetRun()` | Stub exists, needs filling |
| `IResettable` | `ResetRunState()` interface. GameManager calls it on Retry. | New |
| `Coin` | Spin, bob, pickup | New |
| `FriendCage` | Coin cost, unlock, friend celebration, checkpoint | New |
| `EnemyController` | The alien: guard or patrol, chase within the search radius, ledge check, kick with `AttackCollider`, stomp on the head, death and hide, reset | Done |
| `Hazard` | Damage on touch (spikes, saw) | New |
| `MovingPlatform` / `FallingPlatform` / `Spring` | Level mechanics | New |
| `FinalGate` / `ArenaWaves` | Gate unlock at 3/3 friends, wave spawning, win | New |
| `UIController` | HUD and menu screens | New |

Technical notes:
- **Physics rule: everything uses Rigidbody + colliders.**
  - **Player:** a dynamic Rigidbody plus a BoxCollider (see section 3).
  - **Aliens:** a **dynamic Rigidbody** (gravity on, rotation frozen) that sets `Rigidbody.linearVelocity` in `FixedUpdate` and turns with `Rigidbody.MoveRotation`. The solid body collider is what the player lands on to stomp (checked in `OnCollisionEnter`). The kick uses a separate `AttackCollider` **trigger** in front of the feet, read with `Physics.OverlapBox` only while the foot is out. When an alien dies, its Rigidbody becomes kinematic and its colliders are disabled.
  - **Moving platforms:** a **kinematic Rigidbody** moved with `Rigidbody.MovePosition` in `FixedUpdate`, plus a solid collider so the player can stand on them.
  - **Coins, cages, hazards and the final gate:** trigger colliders that react in `OnTriggerEnter` when the player's BoxCollider touches them.
  - **Static ground:** a plain collider (kinematic Rigidbody optional).
  - No `CharacterController` anywhere.
- Communicate through C# events (`CoinsChanged`, `HealthChanged`, `FriendRescued`, `StateChanged`). The UI must not poll every frame.
- Never `Destroy` level objects that Retry has to restore. Hide them and disable them instead.

---

## 10. Build order and timeline

| Day | Priority | Tasks |
|---|---|---|
| **Thu 29 Oct** | P0 | ✅ Movement, camera, animation, dust. `GameManager` states, `PlayerHealth`, `Coin`, fall and respawn |
| **Fri 30 Oct** | P0 | ✅ Alien enemy (guard, patrol, kick, stomp). Spikes, `FriendCage` ×3, graybox of all zones, final gate, Camp waves, Retry |
| **Sat 31 Oct** | P1 | Kenney UI screens and HUD, moving, falling and spring platforms, sounds, art pass |
| **Sun 1 Nov** (until 23:59 BDT) | P0 | Full playtests, bug fixes, Windows build, 30–60 s video, five-line note, submit |

### Cut order (if late)
1. Alien patrol mode; place more Guard aliens instead.
2. Falling platform and saw.
3. Camp Wave 2, keeping a single larger wave.
4. Heart pickup, friend follow animation and squash/stretch.

**Never cut:** 3 hearts, coins, stomping, the 3 friend cages, the final Camp fight, the Start, Game Over and Win screens, Retry without scene reload, and a working Windows build.

---

## 11. Tuning values (starting points, tweak in Play Mode)

| Value | Start |
|---|---|
| Run speed | 6 m/s |
| Jump height / double jump | 1.5 m / 1.35 m |
| Gravity | -25 m/s² (Project Settings > Physics > Gravity; affects every Rigidbody) |
| Stomp bounce height | 1.5 m |
| Knockback | 6 m/s horizontal, 4 m/s up |
| Invulnerability | 1.5 s |
| Coins per cage | 10 |
| Coin spin / bob | 2 rad/s / 0.15 m |
| Alien walk speed | 2.5 m/s |
| Alien search radius / attack range | 6 m / 1.5 m |
| Alien kick hit window / cooldown | 0.2–0.4 s into the kick / 1 s |
| Alien kick damage / stomps to defeat | 1 heart / 1 |
| Kill height (fall) | y = -15 |

---

## 12. Kenney assets used

- **Cube Pets:** dog (player), panda, penguin and cow (friends)
- **Platformer Kit:** `character-oodi` alien (enemy), grass and snow blocks, coin-gold, lock, flag, door-large, fence, spikes, saw, spring, moving and falling platforms, trees, flowers, heart
- **UI Pack:** buttons, panels, heart and coin icons, HUD frames

All three packs are CC0.
