using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Builds Assets/Scenes/GameScene.unity from the menu Paw Rescue > Build Game Scene.
// The first time, it copies TestEnemy (which already has the player, camera, GameManager and UI wired up)
// and removes the test level. Then it lays out the whole level from the design document under a "Level"
// object and adds the extra UI (pause, friends, waves, messages). Safe to run again: it deletes and
// rebuilds "Level" and updates the UI in place, so tweak numbers here rather than in the scene.
public static class GameSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string TemplateScenePath = "Assets/Scenes/TestEnemy.unity";
    private const string KitPath = "Assets/Models/Kenney Platform Kits/FBX format/";
    private const string PetsPath = "Assets/Models/Kenney Cube Pets/FBX format/";
    private const string UiPath = "Assets/Images/PNG/";

    private const float PetPivotHeight = 1.25f; // The pet's pivot sits this far above its feet
    private const float PickupHeight = 0.8f;     // Coins and hearts float this far above the ground
    private const int InvisibleWallLayer = 2;    // Ignore Raycast: the camera looks straight through these walls

    private static Material kitMaterial;
    private static Material petMaterial;
    private static Material particleMaterial;
    private static Font font;       // Titles and buttons
    private static Font narrowFont; // Body text (Unity's built-in sans, bold)
    private static GameObject coinPrefab;
    private static GameObject heartPrefab;
    private static GameObject enemyPrefab;

    private static Transform level;
    private static Transform ground;
    private static Transform decor;
    private static Transform coins;
    private static Transform pickups;
    private static Transform hazards;
    private static Transform platforms;
    private static Transform enemies;
    private static Transform patrolSpots;
    private static Transform cages;

    [MenuItem("Paw Rescue/Build Game Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            AssetDatabase.CopyAsset(TemplateScenePath, ScenePath);
        var scene = EditorSceneManager.OpenScene(ScenePath);

        LoadAssets();
        foreach (string testObject in new[] { "Environment", "HeartPickups", "Enemies", "EnemyPartol Spots", "Level" })
        {
            GameObject old = GameObject.Find(testObject);
            if (old != null) Object.DestroyImmediate(old);
        }

        level = new GameObject("Level").transform;
        ground = Group("Ground");
        decor = Group("Decor");
        coins = Group("Coins");
        pickups = Group("Hearts");
        hazards = Group("Hazards");
        platforms = Group("Platforms");
        enemies = Group("Enemies");
        patrolSpots = Group("PatrolSpots");
        cages = Group("FriendCages");

        PetController player = Object.FindAnyObjectByType<PetController>();
        player.transform.SetPositionAndRotation(new Vector3(0f, PetPivotHeight, -6f), Quaternion.identity);

        BuildStartMeadow();
        BuildPandaGrove();
        BuildPenguinPeak();
        BuildCowPasture();
        FinalGate gate = BuildGate();
        ArenaWaves arena = BuildAlienCamp(gate);

        SetUpPlayerAndCamera(player);
        BuildUI(arena);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AddSceneToBuild();
        Debug.Log("Paw Rescue: GameScene built and saved.");
    }

    // ---------------------------------------------------------------- Level

    // Safe wide island with a coin line and a raised block that teaches the double jump
    private static void BuildStartMeadow()
    {
        for (int x = -1; x <= 1; x++)
            for (int z = -1; z <= 1; z++)
                Tile(x * 6f, z * 6f, 0f, false);

        GameObject step = Spawn("block-grass-large", ground, new Vector3(5f, 0f, 4f), 0f, Vector3.one * 1.5f, kitMaterial);
        AddBoxCollider(step);

        foreach (float z in new[] { 0f, 2.5f, 5f }) Coin(0f, 0f, z);
        Coin(5f, 1.5f, 3.3f);
        Coin(5f, 1.5f, 4.7f);

        Spawn("sign", decor, new Vector3(-2.5f, 0f, -4f), 180f, Vector3.one * 3f, kitMaterial);
        Spawn("tree", decor, new Vector3(-7.5f, 0f, -7.5f), 20f, Vector3.one * 2f, kitMaterial);
        Spawn("tree", decor, new Vector3(7.5f, 0f, -7f), 70f, Vector3.one * 2f, kitMaterial);
        Spawn("tree", decor, new Vector3(-7.5f, 0f, 7.5f), 0f, Vector3.one * 2f, kitMaterial);
        Spawn("flowers", decor, new Vector3(-3.5f, 0f, -1f), 0f, Vector3.one * 3f, kitMaterial);
        Spawn("flowers", decor, new Vector3(3.5f, 0f, -4.5f), 45f, Vector3.one * 3f, kitMaterial);
        Spawn("grass", decor, new Vector3(6.5f, 0f, -2f), 0f, Vector3.one * 3f, kitMaterial);
    }

    // Zone 1 teaches stomping: a guard alien, spikes, small gaps, then a patrolling alien and the Panda cage
    private static void BuildPandaGrove()
    {
        foreach (float x in new[] { -3f, 3f })
        {
            Tile(x, 15f, 0f, false);
            Tile(x, 21f, 0f, false);
            Tile(x, 30f, 0f, false);
            Tile(x, 36f, 0f, false);
        }

        Enemy("Guard", enemies, new Vector3(2.5f, 0f, 21f), 180f, 6f);
        Spikes(-4f, 17.5f, 0f);
        Spikes(-4f, 19.5f, 0f, 1.75f);
        Spikes(4.2f, 16.5f, 0f, 0.8f);
        foreach (Vector3 c in new[] { new Vector3(-3f, 0f, 13.5f), new Vector3(3f, 0f, 13.5f), new Vector3(0f, 0f, 17.5f),
                                      new Vector3(0f, 0f, 20f), new Vector3(-4.5f, 0f, 22.5f), new Vector3(4.5f, 0f, 23f) })
            Coin(c.x, c.y, c.z);

        Enemy("Patrol", enemies, new Vector3(-4f, 0f, 29.5f), 90f, 5f, new Vector3(-4f, 0f, 29.5f), new Vector3(4f, 0f, 29.5f));
        Spikes(-4.5f, 33.5f, 0f);
        Spikes(4.5f, 33.5f, 0f, 1.75f);
        foreach (Vector3 c in new[] { new Vector3(-4f, 0f, 28f), new Vector3(4f, 0f, 28f), new Vector3(-2f, 0f, 31.5f),
                                      new Vector3(2f, 0f, 31.5f), new Vector3(0f, 0f, 30f), new Vector3(-4.5f, 0f, 38f),
                                      new Vector3(4.5f, 0f, 38f) })
            Coin(c.x, c.y, c.z);

        Cage("Panda", 0, "animal-panda", new Vector3(0f, 0f, 36.5f), new Vector3(-4.5f, 0f, 133.5f));

        Spawn("tree", decor, new Vector3(-5.2f, 0f, 12.8f), 10f, Vector3.one * 2f, kitMaterial);
        Spawn("tree", decor, new Vector3(5.2f, 0f, 12.8f), 80f, Vector3.one * 2f, kitMaterial);
        Spawn("tree", decor, new Vector3(-5.2f, 0f, 38.4f), 40f, Vector3.one * 2f, kitMaterial);
        Spawn("flowers", decor, new Vector3(5f, 0f, 36f), 0f, Vector3.one * 3f, kitMaterial);
    }

    // Zone 2 (snow) is about timing: a moving platform, a patrolling alien, then falling platforms and the Penguin cage
    private static void BuildPenguinPeak()
    {
        MovingPlatform(new Vector3(0f, -1.5f, 41.5f), new Vector3(0f, 0f, 9.5f));
        foreach (float z in new[] { 44.5f, 47f, 49.5f }) Coin(0f, 0f, z);

        foreach (float x in new[] { -3f, 3f })
        {
            Tile(x, 56f, 0f, true);
            Tile(x, 62f, 0f, true);
            Tile(x, 80f, 0f, true);
            Tile(x, 86f, 0f, true);
        }

        Enemy("Patrol", enemies, new Vector3(-4f, 0f, 59f), 90f, 5f, new Vector3(-4f, 0f, 59f), new Vector3(4f, 0f, 59f));
        foreach (Vector3 c in new[] { new Vector3(-4f, 0f, 54.5f), new Vector3(4f, 0f, 54.5f),
                                      new Vector3(-4f, 0f, 63.5f), new Vector3(4f, 0f, 63.5f) })
            Coin(c.x, c.y, c.z);

        // A heart on a small floating block off to the side, reached with a double jump
        GameObject ledge = Spawn("block-snow-large", ground, new Vector3(9.6f, 0f, 59f), 0f, Vector3.one * 1.5f, kitMaterial);
        AddBoxCollider(ledge);
        Heart(9.6f, 1.5f, 59f);

        FallingPlatform(new Vector3(0f, 0f, 68.5f));
        FallingPlatform(new Vector3(0f, 0f, 73f));
        Coin(0f, 0f, 68.5f);
        Coin(0f, 0f, 73f);

        foreach (Vector3 c in new[] { new Vector3(-4f, 0f, 78.5f), new Vector3(4f, 0f, 78.5f),
                                      new Vector3(-4.5f, 0f, 84f), new Vector3(4.5f, 0f, 84f) })
            Coin(c.x, c.y, c.z);

        Cage("Penguin", 1, "animal-penguin", new Vector3(0f, 0f, 86.5f), new Vector3(4.5f, 0f, 133.5f));

        Spawn("tree-pine-snow", decor, new Vector3(-5.2f, 0f, 64.3f), 0f, Vector3.one * 2f, kitMaterial);
        Spawn("tree-pine-snow", decor, new Vector3(5.2f, 0f, 53.7f), 30f, Vector3.one * 2f, kitMaterial);
        Spawn("tree-pine-snow", decor, new Vector3(-5.2f, 0f, 88.3f), 60f, Vector3.one * 2f, kitMaterial);
        Spawn("tree-pine-snow-small", decor, new Vector3(5.2f, 0f, 77.7f), 0f, Vector3.one * 2f, kitMaterial);
    }

    // Zone 3 combines everything: a saw, two patrolling aliens with crossing routes,
    // and a spring up to the upper island with the Cow cage. A second spring leads back up from the gate island.
    private static void BuildCowPasture()
    {
        foreach (float x in new[] { -3f, 3f })
        {
            Tile(x, 95f, 0f, false);
            Tile(x, 101f, 0f, false);
            Tile(x, 107f, 0f, false);
            Tile(x, 116f, 5f, false);  // Upper island
            Tile(x, 126f, 0f, false);  // Gate island
            Tile(x, 132f, 0f, false);
        }

        Saw(new Vector3(-5f, 1f, 97f), new Vector3(10f, 0f, 0f));
        Enemy("Patrol", enemies, new Vector3(-4f, 0f, 100.5f), 90f, 5f, new Vector3(-4f, 0f, 100.5f), new Vector3(4f, 0f, 100.5f));
        Enemy("Patrol", enemies, new Vector3(1.5f, 0f, 105f), 180f, 5f, new Vector3(1.5f, 0f, 105f), new Vector3(1.5f, 0f, 99f));
        foreach (Vector3 c in new[] { new Vector3(0f, 0f, 93f), new Vector3(-4f, 0f, 93f), new Vector3(4f, 0f, 93f),
                                      new Vector3(0f, 0f, 97f), new Vector3(-4.5f, 0f, 102f), new Vector3(4.5f, 0f, 102f),
                                      new Vector3(0f, 0f, 102.5f), new Vector3(-2f, 0f, 106f), new Vector3(-4f, 0f, 109f),
                                      new Vector3(4.5f, 0f, 105.5f) })
            Coin(c.x, c.y, c.z);

        Spring(new Vector3(3.5f, 0f, 108.5f));
        Spring(new Vector3(-3.5f, 0f, 124.5f));

        foreach (Vector3 c in new[] { new Vector3(-4.5f, 5f, 114f), new Vector3(4.5f, 5f, 114f),
                                      new Vector3(-4.5f, 5f, 118.3f), new Vector3(1.5f, 5f, 113.5f) })
            Coin(c.x, c.y, c.z);
        Heart(4.5f, 5f, 118.3f);

        Cage("Cow", 2, "animal-cow", new Vector3(-1f, 5f, 117f), new Vector3(4.5f, 0f, 129.5f));

        Spawn("tree", decor, new Vector3(-5.2f, 0f, 92.8f), 0f, Vector3.one * 2f, kitMaterial);
        Spawn("tree", decor, new Vector3(-5.2f, 0f, 109.2f), 50f, Vector3.one * 2f, kitMaterial);
        Spawn("tree", decor, new Vector3(5.2f, 0f, 123.7f), 20f, Vector3.one * 2f, kitMaterial);
        Spawn("flowers", decor, new Vector3(-4.5f, 5f, 113.6f), 0f, Vector3.one * 3f, kitMaterial);
        Spawn("flowers", decor, new Vector3(-1.5f, 0f, 128f), 30f, Vector3.one * 3f, kitMaterial);
    }

    // The big door on the bridge to the camp, shut until all three friends are free
    private static FinalGate BuildGate()
    {
        foreach (float z in new[] { 138f, 144f, 150f }) Tile(0f, z, 0f, false);

        var gateObject = new GameObject("FinalGate");
        gateObject.transform.SetParent(level);
        gateObject.transform.position = new Vector3(0f, 0f, 138f);
        gateObject.layer = InvisibleWallLayer; // The camera looks through the invisible blocker

        var blocker = gateObject.AddComponent<BoxCollider>();
        blocker.center = new Vector3(0f, 7f, 0f);
        blocker.size = new Vector3(16f, 14f, 1.2f); // Wider and taller than any jump around it

        GameObject door = Spawn("door-rotate-large", gateObject.transform, gateObject.transform.position, 0f,
            new Vector3(6.4f, 5f, 4f), kitMaterial);
        door.name = "Door";
        GameObject sign = WorldLabel(gateObject.transform, new Vector3(0f, 6.3f, -1f), "FREE ALL 3 FRIENDS", 56, new Color(1f, 0.85f, 0.3f));
        sign.name = "LockedSign";

        var gate = gateObject.AddComponent<FinalGate>();
        Set(gate, "door", door.transform);
        Set(gate, "blocker", blocker);
        Set(gate, "lockedSign", sign);
        Set(gate, "openDepth", 5.2f);
        return gate;
    }

    // The fenced final arena with two waves of aliens. Invisible walls keep the pet inside during the fight.
    private static ArenaWaves BuildAlienCamp(FinalGate gate)
    {
        Transform camp = Group("AlienCamp");
        for (int x = -1; x <= 1; x++)
            foreach (float z in new[] { 156f, 162f, 168f })
                Tile(x * 6f, z, 0f, false);

        // Fences on the edges (the bridge comes in through a gap in the south side)
        FenceLine(camp, new Vector3(-7.5f, 0f, 170.6f), new Vector3(1f, 0f, 0f), 6, 0f);
        FenceLine(camp, new Vector3(8.6f, 0f, 169.5f), new Vector3(0f, 0f, -1f), 6, 90f);
        FenceLine(camp, new Vector3(-8.6f, 0f, 154.5f), new Vector3(0f, 0f, 1f), 6, 270f);
        FenceLine(camp, new Vector3(-7.5f, 0f, 153.4f), new Vector3(1f, 0f, 0f), 2, 180f);
        FenceLine(camp, new Vector3(4.5f, 0f, 153.4f), new Vector3(1f, 0f, 0f), 2, 180f);

        Transform walls = new GameObject("InvisibleWalls").transform;
        walls.SetParent(camp);
        InvisibleWall(walls, new Vector3(0f, 4f, 171.6f), new Vector3(20f, 8f, 1f));
        InvisibleWall(walls, new Vector3(9.6f, 4f, 162f), new Vector3(1f, 8f, 20f));
        InvisibleWall(walls, new Vector3(-9.6f, 4f, 162f), new Vector3(1f, 8f, 20f));
        InvisibleWall(walls, new Vector3(-6.4f, 4f, 152.4f), new Vector3(6.4f, 8f, 1f));
        InvisibleWall(walls, new Vector3(6.4f, 4f, 152.4f), new Vector3(6.4f, 8f, 1f));

        var respawn = new GameObject("CampRespawn").transform;
        respawn.SetParent(camp);
        respawn.position = new Vector3(0f, PetPivotHeight, 158f);

        // Wave 1: three guards. Wave 2: two guards and three patrolling aliens.
        Transform wave1 = new GameObject("Wave1").transform;
        wave1.SetParent(camp);
        Enemy("Guard", wave1, new Vector3(-5f, 0f, 165f), 180f, 30f);
        Enemy("Guard", wave1, new Vector3(5f, 0f, 165f), 180f, 30f);
        Enemy("Guard", wave1, new Vector3(0f, 0f, 168f), 180f, 30f);

        Transform wave2 = new GameObject("Wave2").transform;
        wave2.SetParent(camp);
        Enemy("Guard", wave2, new Vector3(-6f, 0f, 168f), 180f, 30f);
        Enemy("Guard", wave2, new Vector3(6f, 0f, 168f), 180f, 30f);
        Enemy("Patrol", wave2, new Vector3(-6f, 0f, 160f), 90f, 7f, new Vector3(-6f, 0f, 160f), new Vector3(6f, 0f, 160f));
        Enemy("Patrol", wave2, new Vector3(6f, 0f, 164f), 270f, 7f, new Vector3(6f, 0f, 164f), new Vector3(-6f, 0f, 164f));
        Enemy("Patrol", wave2, new Vector3(0f, 0f, 168.5f), 180f, 7f, new Vector3(0f, 0f, 168.5f), new Vector3(0f, 0f, 156f));

        var triggerObject = new GameObject("CampTrigger");
        triggerObject.transform.SetParent(camp);
        triggerObject.transform.position = new Vector3(0f, 3f, 162.5f);
        var trigger = triggerObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(17f, 6f, 15f);

        ParticleSystem spawnEffect = Burst(camp, "SpawnPoof", Vector3.zero, new Color(0.7f, 1f, 0.6f));
        var arena = triggerObject.AddComponent<ArenaWaves>();
        Set(arena, "waves", new Object[] { wave1.gameObject, wave2.gameObject });
        Set(arena, "gate", gate);
        Set(arena, "respawnPoint", respawn);
        Set(arena, "spawnEffect", spawnEffect);

        wave1.gameObject.SetActive(false);
        wave2.gameObject.SetActive(false);
        return arena;
    }

    // ---------------------------------------------------------------- Level pieces

    // One 6 x 6 m island block with its top at "top"
    private static void Tile(float x, float z, float top, bool snow)
    {
        string model = snow ? "block-snow-overhang-large" : "block-grass-overhang-large";
        GameObject tile = Spawn(model, ground, new Vector3(x, top - 3f, z), 0f, Vector3.one * 3f, kitMaterial);
        AddBoxCollider(tile);
    }

    private static void Coin(float x, float groundTop, float z)
    {
        var coin = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab, coins);
        coin.transform.position = new Vector3(x, groundTop + PickupHeight, z);
    }

    private static void Heart(float x, float groundTop, float z)
    {
        var heart = (GameObject)PrefabUtility.InstantiatePrefab(heartPrefab, pickups);
        heart.transform.position = new Vector3(x, groundTop + PickupHeight, z);
    }

    // A spike trap that pops in and out; startDelay shifts its timing against its neighbours
    private static void Spikes(float x, float z, float top, float startDelay = 0f)
    {
        GameObject spikes = Spawn("trap-spikes-large", hazards, new Vector3(x, top, z), 0f, Vector3.one * 3f, kitMaterial);
        BoxCollider box = AddBoxCollider(spikes);
        box.center = new Vector3(box.center.x, 0.1f, box.center.z); // 0.6 m tall in world space (the model is scaled by 3)
        box.size = new Vector3(box.size.x * 0.85f, 0.2f, box.size.z * 0.85f);
        spikes.AddComponent<Hazard>();

        var trap = spikes.AddComponent<SpikeTrap>();
        Set(trap, "spikes", spikes.transform.Find("spikes"));
        Set(trap, "startDelay", startDelay);
    }

    private static void Saw(Vector3 position, Vector3 travel)
    {
        var saw = new GameObject("Saw");
        saw.transform.SetParent(hazards);
        saw.transform.position = position;
        GameObject blade = Spawn("saw", saw.transform, position, 0f, Vector3.one * 3f, kitMaterial);
        blade.name = "Blade";

        saw.AddComponent<Rigidbody>().isKinematic = true;
        var sphere = saw.AddComponent<SphereCollider>();
        sphere.radius = 1.05f;
        saw.AddComponent<Hazard>();
        var mover = saw.AddComponent<Saw>();
        Set(mover, "blade", blade.transform);
        Set(mover, "travel", travel);
    }

    private static void MovingPlatform(Vector3 position, Vector3 travel)
    {
        GameObject platform = Spawn("block-moving-large", platforms, position, 0f, Vector3.one * 3f, kitMaterial);
        platform.name = "MovingPlatform";
        AddBoxCollider(platform);
        platform.AddComponent<Rigidbody>().isKinematic = true;
        Set(platform.AddComponent<MovingPlatform>(), "travel", travel);
    }

    // A platform with its top at the given position's height
    private static void FallingPlatform(Vector3 topCentre)
    {
        var root = new GameObject("FallingPlatform");
        root.transform.SetParent(platforms);
        root.transform.position = topCentre + Vector3.down * 0.6f;
        GameObject visual = Spawn("platform", root.transform, root.transform.position, 0f, Vector3.one * 3f, kitMaterial);
        visual.name = "Visual";

        AddBoxCollider(root);
        root.AddComponent<Rigidbody>().isKinematic = true;
        Set(root.AddComponent<FallingPlatform>(), "visual", visual.transform);
    }

    private static void Spring(Vector3 position)
    {
        var root = new GameObject("Spring");
        root.transform.SetParent(platforms);
        root.transform.position = position;
        GameObject visual = Spawn("spring", root.transform, position, 0f, Vector3.one * 2.5f, kitMaterial);
        visual.name = "Visual";

        AddBoxCollider(root);
        Set(root.AddComponent<Spring>(), "visual", visual.transform);
    }

    private static void Enemy(string kind, Transform parent, Vector3 position, float yaw, float searchRadius,
        Vector3? spot1 = null, Vector3? spot2 = null)
    {
        var enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab, parent);
        enemy.name = kind + " Alien";
        enemy.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

        var controller = enemy.GetComponent<EnemyController>();
        Set(controller, "searchRadius", searchRadius);
        bool patrols = spot1.HasValue && spot2.HasValue;
        Set(controller, "willPatrol", patrols);
        if (!patrols) return;

        Set(controller, "spot1", Spot(enemy.name + " Spot1", spot1.Value));
        Set(controller, "spot2", Spot(enemy.name + " Spot2", spot2.Value));
    }

    private static Transform Spot(string name, Vector3 position)
    {
        var spot = new GameObject(name).transform;
        spot.SetParent(patrolSpots);
        spot.position = position;
        return spot;
    }

    // A fence ring with a friend inside, a lock, a price sign, a flag and the checkpoint in front
    private static void Cage(string friendName, int index, string animalModel, Vector3 position, Vector3 waitPosition)
    {
        var root = new GameObject(friendName + "Cage");
        root.transform.SetParent(cages);
        root.transform.position = position;

        var cage = new GameObject("Cage").transform;
        cage.SetParent(root.transform, false);
        foreach (float yaw in new[] { 0f, 90f, 180f, 270f })
        {
            GameObject fence = Spawn("fence-straight", cage, position, yaw, Vector3.one * 3f, kitMaterial);
            AddBoxCollider(fence);
        }
        Spawn("lock", cage, position + new Vector3(0f, 0.2f, -1.6f), 0f, Vector3.one * 3f, kitMaterial);

        GameObject friend = Spawn(animalModel, root.transform, position, 180f, Vector3.one, petMaterial);
        friend.name = friendName;
        Spawn("flag", root.transform, position + new Vector3(2.8f, 0f, -1.6f), 0f, Vector3.one * 3f, kitMaterial);

        var checkpoint = new GameObject("Checkpoint").transform;
        checkpoint.SetParent(root.transform);
        checkpoint.position = position + new Vector3(0f, PetPivotHeight, -3.2f);

        var waitSpot = new GameObject(friendName + "WaitSpot").transform;
        waitSpot.SetParent(root.transform);
        waitSpot.SetPositionAndRotation(waitPosition, Quaternion.Euler(0f, 180f, 0f));

        GameObject label = WorldLabel(root.transform, new Vector3(0f, 3.4f, 0f), "10 COINS", 64, new Color(1f, 0.85f, 0.3f));
        ParticleSystem effect = Burst(root.transform, "CelebrateStars", new Vector3(0f, 1.2f, 0f), new Color(1f, 0.9f, 0.3f));

        var trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 1.5f, 0f);
        trigger.size = new Vector3(4.6f, 3f, 4.6f);

        var friendCage = root.AddComponent<FriendCage>();
        Set(friendCage, "friendIndex", index);
        Set(friendCage, "friendName", friendName);
        Set(friendCage, "cage", cage.gameObject);
        Set(friendCage, "friend", friend.transform);
        Set(friendCage, "checkpoint", checkpoint);
        Set(friendCage, "waitSpot", waitSpot);
        Set(friendCage, "costLabel", label.GetComponentInChildren<Text>());
        Set(friendCage, "celebrateEffect", effect);
    }

    // A row of fences starting at "start", one every 3 m along "step"
    private static void FenceLine(Transform parent, Vector3 start, Vector3 step, int count, float yaw)
    {
        for (int i = 0; i < count; i++)
        {
            // The fence model sits 1.26 m in front of its pivot, so pull the pivot back to put it on the line
            Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
            Vector3 position = start + step * (3f * i) - rotation * Vector3.forward * 1.26f;
            Spawn("fence-straight", parent, position, yaw, Vector3.one * 3f, kitMaterial);
        }
    }

    private static void InvisibleWall(Transform parent, Vector3 centre, Vector3 size)
    {
        var wall = new GameObject("Wall");
        wall.transform.SetParent(parent);
        wall.transform.position = centre;
        wall.layer = InvisibleWallLayer;
        wall.AddComponent<BoxCollider>().size = size;
    }

    // A one-shot burst of little stars (rescues and arena spawns)
    private static ParticleSystem Burst(Transform parent, string name, Vector3 localPosition, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;

        var particles = go.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = particles.main;
        main.duration = 1f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = 0.9f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
        main.startColor = color;
        main.gravityModifier = 0.6f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var size = particles.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = particleMaterial;
        return particles;
    }

    // A sign floating in the world that always turns to face the camera
    private static GameObject WorldLabel(Transform parent, Vector3 localPosition, string text, int fontSize, Color color)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace; // Before placing it, or the canvas resizes itself
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * 0.01f;
        ((RectTransform)go.transform).sizeDelta = new Vector2(700f, 120f);
        go.AddComponent<Billboard>();

        Text label = MakeText(go.transform, "Text", text, fontSize, color, Vector2.zero, new Vector2(700f, 120f), true);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.sizeDelta = Vector2.zero;
        return go;
    }

    private static void SetUpPlayerAndCamera(PetController player)
    {
        // Squash and stretch on the body
        Set(Get<PetSquash>(player.transform), "body", player.transform.Find("Body Root"));

        // The camera ignores the arena's invisible walls
        CameraFollow cameraFollow = Object.FindAnyObjectByType<CameraFollow>();
        var so = new SerializedObject(cameraFollow);
        so.FindProperty("collisionLayers").intValue = ~(1 << InvisibleWallLayer);
        so.ApplyModifiedPropertiesWithoutUndo();
        cameraFollow.transform.SetPositionAndRotation(new Vector3(0f, 5f, -14f), Quaternion.Euler(18f, 0f, 0f));
    }

    // ---------------------------------------------------------------- UI

    // Screens are cards in the style of the Kenney UI Pack samples: a coloured frame with a white title,
    // a light grey body, a coloured main button and white secondary buttons.
    private static void BuildUI(ArenaWaves arena)
    {
        UIManager ui = Object.FindAnyObjectByType<UIManager>(FindObjectsInactive.Include);
        Transform canvas = ui.transform;
        Transform start = canvas.Find("StartPanel");
        Transform hud = canvas.Find("HUD");
        Transform gameOver = canvas.Find("GameOverPanel");
        Transform win = canvas.Find("WinPanel");
        Transform pausePanel = Child(canvas, "PausePanel");
        pausePanel.SetSiblingIndex(hud.GetSiblingIndex() + 1);

        // Start screen: red "Game Title" card
        Transform body = ScreenCard(start, "Red", new Vector2(600f, 540f), "PAW RESCUE", 50, 96f);
        Narrow(MakeText(body, "Goal", "Collect coins, free your friends\nand clear the Alien Camp!", 26, DarkText,
            new Vector2(0f, 140f), new Vector2(540f, 80f), false));
        Button play = MakeButton(body, "PlayButton", "PLAY", "Red", new Vector2(0f, 30f), new Vector2(440f, 86f), Color.white, PrimarySprite);
        Button options = MakeButton(body, "OptionsButton", "OPTIONS", "Grey", new Vector2(0f, -70f), new Vector2(440f, 72f), DarkText, SecondarySprite);
        Button quit = MakeButton(body, "QuitButton", "QUIT", "Grey", new Vector2(0f, -158f), new Vector2(440f, 72f), DarkText, SecondarySprite);

        // Options screen: green "Options" card with the controls, opened from the start screen
        Transform optionsPanel = Child(canvas, "OptionsPanel");
        optionsPanel.SetSiblingIndex(start.GetSiblingIndex() + 1);
        body = ScreenCard(optionsPanel, "Green", new Vector2(600f, 460f), "OPTIONS", 46, 96f);
        Narrow(MakeText(body, "Heading", "CONTROLS", 24, DarkText, new Vector2(0f, 135f), new Vector2(500f, 36f), false));
        Transform controls = Box(body, "ControlsBox", "Extra/Default/input_outline_square", new Vector2(0f, 30f), new Vector2(500f, 150f));
        Text controlsText = Narrow(MakeText(controls, "Controls",
            "WASD   Move\nSPACE   Jump (twice for a double jump)\nARROWS / DRAG   Camera\nESC   Pause", 21, DarkText,
            Vector2.zero, new Vector2(470f, 135f), false));
        controlsText.lineSpacing = 1.15f;
        Button optionsBack = MakeButton(body, "BackButton", "BACK", "Green", new Vector2(0f, -110f), new Vector2(400f, 80f), Color.white, PrimarySprite);
        optionsPanel.gameObject.SetActive(false);

        // Pause screen: green "Options" card
        body = ScreenCard(pausePanel, "Green", new Vector2(500f, 440f), "PAUSED", 46, 96f);
        Button resume = MakeButton(body, "ResumeButton", "RESUME", "Green", new Vector2(0f, 88f), new Vector2(400f, 86f), Color.white, PrimarySprite);
        Button restart = MakeButton(body, "RestartButton", "RESTART", "Grey", new Vector2(0f, -10f), new Vector2(400f, 72f), DarkText, SecondarySprite);
        Button pauseMenu = MakeButton(body, "MainMenuButton", "MAIN MENU", "Grey", new Vector2(0f, -102f), new Vector2(400f, 72f), DarkText, SecondarySprite);

        // Game over: red card
        body = ScreenCard(gameOver, "Red", new Vector2(560f, 460f), "OH NO!", 50, 96f);
        Narrow(MakeText(body, "Message", "Your puppy ran out of hearts.", 28, DarkText, new Vector2(0f, 105f), new Vector2(500f, 60f), false));
        Button retry = MakeButton(body, "RetryButton", "RETRY", "Red", new Vector2(0f, 0f), new Vector2(400f, 86f), Color.white, PrimarySprite);
        Button gameOverMenu = MakeButton(body, "MainMenuButton", "MAIN MENU", "Grey", new Vector2(0f, -100f), new Vector2(400f, 72f), DarkText, SecondarySprite);

        // Win: blue "Level complete" card with three stars over the top and a yellow-framed reward box
        body = ScreenCard(win, "Blue", new Vector2(600f, 540f), "ALL FRIENDS SAVED!", 36, 140f);
        Transform card = win.Find("Card");
        Star(card, "StarLeft", new Vector2(-120f, 268f), 110f, -12f);
        Star(card, "StarRight", new Vector2(120f, 268f), 110f, 12f);
        Star(card, "StarMiddle", new Vector2(0f, 290f), 140f, 0f);
        Transform reward = Box(body, "RewardBox", "Yellow/Default/button_square_border", new Vector2(0f, 90f), new Vector2(440f, 150f));
        Narrow(MakeText(reward, "Heading", "YOUR RESCUE", 22, DarkText, new Vector2(0f, 42f), new Vector2(400f, 34f), false));
        Text stats = MakeText(reward, "Stats", "TIME 0:00\nCOINS 0", 34, DarkText, new Vector2(0f, -18f), new Vector2(420f, 90f), false);
        Button playAgain = IconButton(body, "PlayAgainButton", new Vector2(-75f, -75f), "Extra/Default/icon_repeat_dark", 0f, "PLAY AGAIN");
        Button winMenu = IconButton(body, "MainMenuButton", new Vector2(75f, -75f), "Extra/Default/icon_play_dark", 180f, "MENU");

        // HUD: hearts and coins stay where they are; friends, waves, pause button and messages are rebuilt
        foreach (string old in new[] { "Friends", "WaveBox", "PauseButton", "Message" })
        {
            Transform oldChild = hud.Find(old);
            if (oldChild != null) Object.DestroyImmediate(oldChild.gameObject);
        }
        Transform coinBox = hud.Find("CoinBox");
        Image coinBoxImage = coinBox.GetComponent<Image>();
        coinBoxImage.sprite = LoadSprite("Grey/Default/button_square_depth_flat", true);
        coinBoxImage.type = Image.Type.Sliced;
        coinBox.Find("CoinText").GetComponent<Text>().color = DarkText;

        Transform friends = Box(hud, "Friends", "Blue/Default/button_square_depth_gradient", Vector2.zero, new Vector2(340f, 112f));
        Place((RectTransform)friends, new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(340f, 112f), new Vector2(0.5f, 1f));
        string[] names = { "PANDA", "PENGUIN", "COW" };
        var icons = new Object[3];
        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * 105f;
            Image star = Get<Image>(Child(friends, names[i] + "Icon"));
            star.sprite = LoadSprite("Yellow/Default/star", false);
            star.preserveAspect = true;
            Place(star.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, 14f), new Vector2(58f, 58f), new Vector2(0.5f, 0.5f));
            icons[i] = star;
            Narrow(MakeText(friends, names[i] + "Name", names[i], 17, Color.white, new Vector2(x, -34f), new Vector2(110f, 24f), false));
        }

        Transform waveBox = Box(hud, "WaveBox", "Red/Default/button_square_depth_flat", Vector2.zero, new Vector2(420f, 60f));
        Place((RectTransform)waveBox, new Vector2(0.5f, 1f), new Vector2(0f, -142f), new Vector2(420f, 60f), new Vector2(0.5f, 1f));
        Text waveText = MakeText(waveBox, "WaveText", "WAVE 1/2   ALIENS 3", 26, Color.white, new Vector2(0f, 3f), new Vector2(400f, 50f), false);

        Button pause = MakeButton(hud, "PauseButton", "II", "Blue", Vector2.zero, new Vector2(76f, 76f), Color.white, "button_square_depth_gradient");
        Place((RectTransform)pause.transform, new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(76f, 76f), new Vector2(1f, 1f));

        Text message = MakeText(hud, "Message", "Panda is free!", 44, Color.white, new Vector2(0f, 150f), new Vector2(1200f, 70f), true);

        pausePanel.gameObject.SetActive(false);

        Set(ui, "playButton", play);
        Set(ui, "quitButton", quit);
        Set(ui, "optionsPanel", optionsPanel.gameObject);
        Set(ui, "optionsButton", options);
        Set(ui, "optionsBackButton", optionsBack);
        Set(ui, "pausePanel", pausePanel.gameObject);
        Set(ui, "pauseButton", pause);
        Set(ui, "resumeButton", resume);
        Set(ui, "restartButton", restart);
        Set(ui, "pauseMenuButton", pauseMenu);
        Set(ui, "retryButton", retry);
        Set(ui, "gameOverMenuButton", gameOverMenu);
        Set(ui, "playAgainButton", playAgain);
        Set(ui, "winMenuButton", winMenu);
        Set(ui, "friendIcons", icons);
        Set(ui, "lockedColor", new Color(0.12f, 0.25f, 0.45f, 0.6f));
        Set(ui, "arena", arena);
        Set(ui, "waveBox", waveBox.gameObject);
        Set(ui, "waveText", waveText);
        Set(ui, "messageText", message);
        Set(ui, "winStatsText", stats);
    }

    private static readonly Color DarkText = new Color(0.27f, 0.29f, 0.36f);
    private const string PrimarySprite = "button_rectangle_depth_gradient"; // Coloured main button
    private const string SecondarySprite = "button_rectangle_depth_flat";   // White-grey button

    // Clears a full-screen panel, dims the game behind it and puts a card in the middle.
    // Returns the card's grey body, where the content goes (its centre is (0, 0)).
    private static Transform ScreenCard(Transform panel, string color, Vector2 size, string title, int titleSize, float headerHeight)
    {
        for (int i = panel.childCount - 1; i >= 0; i--) Object.DestroyImmediate(panel.GetChild(i).gameObject);
        Stretch((RectTransform)panel);
        Image dim = Get<Image>(panel);
        dim.sprite = null;
        dim.color = new Color(0.05f, 0.09f, 0.18f, 0.6f);

        Transform card = Box(panel, "Card", color + "/Default/button_square_depth_flat", Vector2.zero, size);

        Text heading = MakeText(card, "Title", title, titleSize, Color.white,
            new Vector2(0f, size.y * 0.5f - headerHeight + 48f), new Vector2(size.x - 40f, 70f), false);
        Shadow shadow = Get<Shadow>(heading.transform);
        shadow.effectColor = new Color(0f, 0f, 0f, 0.25f);
        shadow.effectDistance = new Vector2(0f, -3f);

        Transform body = Box(card, "Body", "Grey/Default/button_square_depth_flat", Vector2.zero, Vector2.zero);
        var rect = (RectTransform)body;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(12f, 16f);
        rect.offsetMax = new Vector2(-12f, -headerHeight);
        return body;
    }

    // A 9-sliced Kenney panel centred on its parent
    private static Transform Box(Transform parent, string name, string sprite, Vector2 position, Vector2 size)
    {
        Transform box = Child(parent, name);
        Image image = Get<Image>(box);
        image.sprite = LoadSprite(sprite, true);
        image.type = Image.Type.Sliced;
        image.color = Color.white;
        Place((RectTransform)box, new Vector2(0.5f, 0.5f), position, size, new Vector2(0.5f, 0.5f));
        return box;
    }

    private static void Star(Transform parent, string name, Vector2 position, float size, float tilt)
    {
        Image star = Get<Image>(Child(parent, name));
        star.sprite = LoadSprite("Yellow/Default/star", false);
        star.preserveAspect = true;
        star.raycastTarget = false;
        Place(star.rectTransform, new Vector2(0.5f, 0.5f), position, new Vector2(size, size), new Vector2(0.5f, 0.5f));
        star.rectTransform.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }

    // A round yellow button with an icon and a small caption underneath
    private static Button IconButton(Transform parent, string name, Vector2 position, string icon, float iconRotation, string caption)
    {
        Transform buttonTransform = Child(parent, name);
        Image image = Get<Image>(buttonTransform);
        image.sprite = LoadSprite("Yellow/Default/button_round_depth_gradient", false);
        image.type = Image.Type.Simple;
        Button button = Get<Button>(buttonTransform);
        button.targetGraphic = image;
        Place((RectTransform)buttonTransform, new Vector2(0.5f, 0.5f), position, new Vector2(104f, 104f), new Vector2(0.5f, 0.5f));

        Image iconImage = Get<Image>(Child(buttonTransform, "Icon"));
        iconImage.sprite = LoadSprite(icon, false);
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
        Place(iconImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 4f), new Vector2(48f, 48f), new Vector2(0.5f, 0.5f));
        iconImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, iconRotation);

        Narrow(MakeText(buttonTransform, "Caption", caption, 18, DarkText, new Vector2(0f, -72f), new Vector2(160f, 26f), false));
        return button;
    }

    private static Text Narrow(Text text)
    {
        text.font = narrowFont;
        text.fontStyle = FontStyle.Bold;
        return text;
    }

    // A Kenney UI button in the same style as the existing PLAY / RETRY buttons
    private static Button MakeButton(Transform parent, string name, string label, string color, Vector2 position, Vector2 size,
        Color labelColor, string sprite = "button_rectangle_depth_gloss")
    {
        Transform buttonTransform = Child(parent, name);
        Image image = Get<Image>(buttonTransform);
        image.sprite = LoadSprite(color + "/Default/" + sprite, true);
        image.type = Image.Type.Sliced;
        Button button = Get<Button>(buttonTransform);
        button.targetGraphic = image;
        Place((RectTransform)buttonTransform, new Vector2(0.5f, 0.5f), position, size, new Vector2(0.5f, 0.5f));

        int fontSize = size.y < 90f ? 32 : 38;
        MakeText(buttonTransform, "Label", label, fontSize, labelColor, new Vector2(0f, 4f), size - new Vector2(20f, 16f), false);
        return button;
    }

    private static Text MakeText(Transform parent, string name, string text, int fontSize, Color color, Vector2 position,
        Vector2 size, bool outline)
    {
        Transform textTransform = Child(parent, name);
        Text label = Get<Text>(textTransform);
        label.text = text;
        label.font = font;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = TextAnchor.MiddleCenter;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.raycastTarget = false;
        Place(label.rectTransform, new Vector2(0.5f, 0.5f), position, size, new Vector2(0.5f, 0.5f));
        if (outline)
        {
            Outline line = Get<Outline>(textTransform);
            line.effectDistance = new Vector2(3f, -3f);
            line.effectColor = new Color(0f, 0f, 0f, 0.6f);
        }
        return label;
    }

    private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size, Vector2 pivot)
    {
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    // Finds a child UI object by name, or makes it
    private static Transform Child(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null) return child;
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static T Get<T>(Transform t) where T : Component
    {
        T component = t.GetComponent<T>();
        return component != null ? component : t.gameObject.AddComponent<T>();
    }

    // Loads a Kenney UI image as a sprite, switching its import settings to Sprite if needed
    private static Sprite LoadSprite(string name, bool sliced)
    {
        string path = UiPath + name + ".png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        Vector4 border = sliced ? new Vector4(16f, 20f, 16f, 16f) : Vector4.zero;
        if (importer.textureType != TextureImporterType.Sprite || importer.spriteBorder != border)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = border;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ---------------------------------------------------------------- Helpers

    private static void LoadAssets()
    {
        kitMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_PlatForm.mat");
        petMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_CubePets.mat");
        particleMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RunDust.mat");
        font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Kenney Future.ttf");
        narrowFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Clean sans like the UI Pack samples
        coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Coin.prefab");
        heartPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Heart.prefab");
        enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
    }

    private static Transform Group(string name)
    {
        var group = new GameObject(name).transform;
        group.SetParent(level);
        return group;
    }

    // Places a Kenney model (Platformer Kit name, or a full path) and gives it the project's toon material
    private static GameObject Spawn(string model, Transform parent, Vector3 position, float yaw, Vector3 scale, Material material)
    {
        string path = model.StartsWith("Assets/") ? model
            : (model.StartsWith("animal-") ? PetsPath : KitPath) + model + ".fbx";
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) throw new System.Exception("Model not found: " + path);

        var go = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
        go.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
        go.transform.localScale = scale;

        foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
        {
            var materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++) materials[i] = material;
            renderer.sharedMaterials = materials;
        }
        return go;
    }

    // A BoxCollider on the object that wraps all of its meshes (children included)
    private static BoxCollider AddBoxCollider(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);

        var box = go.AddComponent<BoxCollider>();
        box.center = go.transform.InverseTransformPoint(bounds.center);
        Vector3 size = go.transform.InverseTransformVector(bounds.size);
        box.size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
        return box;
    }

    // Sets a [SerializeField] on a component by name, as if typed into the Inspector
    private static void Set(Object target, string field, object value)
    {
        var so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);
        if (property == null) throw new System.Exception($"{target.GetType().Name} has no field {field}");

        switch (value)
        {
            case Object[] list:
                property.arraySize = list.Length;
                for (int i = 0; i < list.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
                break;
            case Object reference: property.objectReferenceValue = reference; break;
            case bool flag: property.boolValue = flag; break;
            case int number: property.intValue = number; break;
            case float number: property.floatValue = number; break;
            case string text: property.stringValue = text; break;
            case Vector3 vector: property.vector3Value = vector; break;
            case Color color: property.colorValue = color; break;
            default: throw new System.Exception("Unsupported value for " + field);
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Builds the Windows player to Builds/Windows/PawRescue.exe (outside Assets, ignored by git)
    [MenuItem("Paw Rescue/Build Windows")]
    public static void BuildWindows()
    {
        AddSceneToBuild();
        PlayerSettings.productName = "Paw Rescue";
        PlayerSettings.runInBackground = true;

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "Builds/Windows/PawRescue.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"Paw Rescue: Windows build {report.summary.result}, {report.summary.totalErrors} errors, " +
                  $"{report.summary.totalSize / (1024 * 1024)} MB at {report.summary.outputPath}");
    }

    // GameScene first in the build, the test scenes kept but switched off
    private static void AddSceneToBuild()
    {
        var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.path != ScenePath) scenes.Add(new EditorBuildSettingsScene(scene.path, false));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
