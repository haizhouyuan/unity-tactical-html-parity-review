#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class TacticalPrototypeTools
{
    private const string ScenePath = "Assets/Scenes/TacticalPrototype.unity";
    private const string ApprovedCrateGlbPath = "ApprovedAssets/tactical_crate_v1.glb";
    private const string RealifiedCrateGlbPath = "RealifiedAssets/RS_11_prop_crate_LOD0.glb";
    private const string ApprovedContainerGlbPath = "ApprovedAssets/approved_container_v1.glb";
    private const string RealifiedContainerGlbPath = "RealifiedAssets/RS_10_prop_container_LOD0.glb";
    private const string RealifiedAmmoLootGlbPath = "RealifiedAssets/RS_08_loot_ammo_LOD0.glb";
    private const string RealifiedMedkitLootGlbPath = "RealifiedAssets/RS_09_loot_medkit_LOD0.glb";
    private const string RealifiedHelmetLootGlbPath = "RealifiedAssets/RS_06_gear_helmet_LOD0.glb";
    private const string RealifiedVestLootGlbPath = "RealifiedAssets/RS_07_gear_vest_LOD0.glb";
    private const string RealifiedSidearmGlbPath = "RealifiedAssets/RS_02_sidearm_LOD0.glb";
    private const string RealifiedHeroRifleGlbPath = "RealifiedAssets/hero_rifle_LOD0.glb";
    private const string RealifiedSecondaryWeaponGlbPath = "RealifiedAssets/RS_03_secondary_weapon_LOD0.glb";
    private const string ApprovedPlayerGlbPath = "ApprovedAssets/approved_player_tactical_v1.glb";
    private const string ApprovedEnemyGlbPath = "ApprovedAssets/approved_enemy_tactical_v1.glb";
    private const string RealifiedPlayerGlbPath = "RealifiedAssets/RS_04_player_tactical_LOD0.glb";
    private const string RealifiedEnemyGlbPath = "RealifiedAssets/RS_05_enemy_tactical_LOD0.glb";
    private const string HtmlPistolGlbPath = "models/pistol_m5_candidate.glb";
    private const string HtmlShotgunGlbPath = "models/shotgun_m5_candidate.glb";
    private const string HtmlRifleGlbPath = "models/rifle_obj_baseline.glb";
    private const string HtmlDmrGlbPath = "models/dmr_m5_candidate.glb";
    private const string DetailedPlayerGlbPath = "models/character_player_final.glb";
    private const string DetailedEnemyGlbPath = "models/character_enemy_final.glb";
    private const string ApprovedMedicalLootGlbPath = "ApprovedAssets/medical_loot_v1.glb";
    private const string ApprovedAmmoLootGlbPath = "ApprovedAssets/approved_ammo_loot_v1.glb";
    private const string ApprovedHelmetLootGlbPath = "ApprovedAssets/approved_helmet_loot_v1.glb";
    private const string ApprovedVestLootGlbPath = "ApprovedAssets/approved_vest_loot_v1.glb";
    private const string ApprovedMaterialRoot = "Assets/HtmlTacticalAssets/ApprovedMaterials";
    private const string ApprovedAnimationRoot = "Assets/HtmlTacticalAssets/ApprovedAnimations";
    private static readonly List<Vector3> LootPositions = new();

    [MenuItem("AI Tools/Create Tactical Prototype From HTML")]
    public static void CreateTacticalPrototype()
    {
        EnsureFolders();
        ImportApprovedAssets();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var materials = CreateMaterials();
        CreateLighting(materials);
        CreateGround(materials);
        CreateCompound(materials);
        CreateBuildings(materials);
        CreateWarehouse(materials);
        CreateContainers(materials);
        CreateYardDetails(materials);
        CreateSpawnStagingArea(materials);
        CreateVisualPolish(materials);

        var ui = CreateHud();
        var player = CreatePlayer(materials);
        var camera = CreateCamera(player, materials);
        var manager = CreateGameManager(player, camera, ui, materials);

        CreateLoot(materials);
        CreateEnemies(materials, manager, player.transform);
        CreateReinforcementTemplate(materials, manager);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        WriteReport();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[AI Tools] Tactical Prototype created at " + ScenePath + ". Press Play to test movement, pickup, fire, reload, healing, armor, enemies, and the compound layout.");
    }

    [MenuItem("AI Tools/Open Tactical Playable Scene")]
    public static void OpenTacticalPlayableScene()
    {
        if (!File.Exists(ScenePath))
        {
            Debug.LogWarning("[AI Tools] Tactical playable scene does not exist yet. Run AI Tools > Create Tactical Prototype From HTML first.");
            return;
        }

        EditorSceneManager.OpenScene(ScenePath);
        Debug.Log("[AI Tools] Opened playable tactical scene: " + ScenePath + ". Contact-sheet scenes are asset previews only and do not contain shooting/reload gameplay.");
    }

    [MenuItem("AI Tools/Write Tactical Prototype Report")]
    public static void WriteReport()
    {
        Directory.CreateDirectory("docs");
        File.WriteAllText("docs/TACTICAL_PROTOTYPE_REPORT.md", @"# Tactical Prototype Report

Generated by `AI Tools > Create Tactical Prototype From HTML`.

## Goal Coverage

This scene is the Unity-side HTML tactical game replica pass. It is intentionally based on the old HTML tactical layout and imports the old final packet GLB assets under `Assets/HtmlTacticalAssets`.

1. Gameplay reproduction:
   - Player movement with WASD / arrow keys.
   - Sprint stamina with Shift.
   - Third-person and first-person camera with V.
   - Mouse look, ADS with right mouse, jump, crouch, and prone.
   - Lobby and death overlays similar to the HTML flow.
   - Lobby and death overlays similar to the HTML flow; the lobby uses a fixed compound overview camera before handing off to player follow on start.
   - Starting or restarting the round clears stale pickup prompts, hit markers, settings/help/skin panels, and invalid weapon HUD state.
   - Starting a round now defaults to first-person camera so manual play immediately shows the current weapon instead of requiring V first.
   - HUD for HP, stamina, weapon, ammo, armor, inventory, NPC count, kills, prompt, and messages.
   - Four weapon states inspired by the HTML game: P-9, BREACH-12, TAC-AR, XMR-7.
   - First-person and third-person player weapon visuals now switch across all four weapon GLBs instead of showing the pistol for every weapon.
   - Left mouse fire, automatic rifle fire, shotgun pellet spread, range falloff, reload timing, muzzle flash, impact sparks, tracers, and procedural SFX feedback.
   - Reload with R.
   - Weapon switch with 1-4.
   - Pickup with F.
   - Healing with 5/6/7.
   - Vest, helmet, ammo, medical, revive crystal, and weapon loot.
   - Settings panel for NPC strength, spawn rhythm, and loot richness; loot richness now changes pickup yield instead of only changing UI text.
   - Skin gacha affects first-person and third-person player weapon tint plus player tracer color, not just HUD text.
   - NPC enemies with patrol, chase, ranged fire, contact damage, health, hit feedback, enemy-shot audio, elimination count, coin rewards, and runtime reinforcements that clone a pre-imported GLB template instead of loading GLBs at spawn time.
   - Character GLB roots and named limb parts receive authored Unity AnimationClip libraries plus procedural fallback motion for walk, aim, fire, hit, and down-state poses, so player/NPC models are not static capsules.

2. Scene reproduction:
   - Spawn point around `(0, 0, 30)`.
   - Compound walls based on the original `160 x 126` layout.
   - A/B/C/D building blockout positions and floor counts.
   - Warehouse around `(0, -28)`.
   - Container yard positions from the HTML data.
   - Loot points placed inside buildings, warehouse crates, and between containers.
   - NPC positions based on the old outer-road / between-building spawn areas.
   - Door openings, windows, awnings, AC units, interior tables, cabinets, ladders, crates, trees, rocks, fence/detail props, and a spawn-side checkpoint/loot staging area.
   - Spawn-side checkpoint clutter is kept to the sides so the old `(0, 0, 30)` player lane has a readable forward view instead of starting inside stacked props.
   - Third-person camera is tuned to the HTML-style far over-shoulder view: it starts at the old `(0, 0, 30)` spawn, faces into the compound, avoids wall clipping, keeps the first-person weapon hidden, and looks beyond the player instead of straight down at the player capsule.
   - Interior ladders now participate in gameplay: nearby ladders show an F prompt, move the player between ground and upper floors, and snap the player camera after the move.

3. HTML final packet GLB asset import:
   - Character GLBs are editor-instantiated on player and NPC gameplay objects.
   - Character GLB placement follows the HTML packet transform (`scale 0.82`, local position near zero) instead of the earlier undersized Unity-only transform.
   - Weapon GLBs are editor-instantiated on the first-person camera, third-person player mount, NPC mounts, and weapon loot.
   - Loot GLBs are editor-instantiated on ammo, medical, armor, helmet, revive, and weapon pickups.
   - Environment GLBs are editor-instantiated on crates, containers, ladders, building details, fences, trees, rocks, and furniture.
   - Runtime reinforcement NPCs clone an editor-generated GLB template, so the dynamic spawn path no longer creates runtime `HtmlGlbAssetMount` loaders.
   - Asset source: `Assets/HtmlTacticalAssets`.
   - Approved PBR material source: `Assets/HtmlTacticalAssets/ApprovedMaterials`.
   - Central roads use the approved wet asphalt PBR material.
   - Building floors, warehouse floor, and compound/building walls use the approved concrete wall PBR material.

## Scene

- `Assets/Scenes/TacticalPrototype.unity`
- This is the playable scene. `GLB_PBR_ContactSheet.unity` and `GLB_PBR_Textured_ContactSheet.unity` are asset-review/contact-sheet scenes only; they do not contain player control, first-person weapons, shooting, reload, pickup, or NPC combat.
- Latest camera-rendered player-view screenshot with HUD: `Assets/Screenshots/tactical_html_replica_current_player_pov_verified.png`
- Latest player-POV gate report: `docs/TACTICAL_PLAYER_POV_GATE.json`
- Latest gameplay proof gate report: `docs/TACTICAL_GAMEPLAY_PROOF_GATE.json`

## Latest Verification

- Unity runtime console errors/warnings during gate execution: 0.
- `StartRound()` resets the player to the HTML spawn lane at `(0.00, 1.04, 30.00)`.
- Player-POV gate: passed.
- Gameplay proof gate: passed.
- Player-POV screenshot now uses direct camera rendering with the HUD canvas temporarily attached to the camera, avoiding stale asynchronous Game View captures while still showing HP/stamina/weapon/ammo/inventory/NPC/skin/crosshair/message text.
- Gameplay proof currently verifies lobby overview camera clearance, lobby hidden after start, settings controls, skin roll, pickup state mutation, loot-richness pickup yield, fire ammo + enemy hit, muzzle/casing/tracer/hit-marker weapon feedback, reload, direct player damage, real NPC ranged fire damage/tracer/SFX, healing item use, dynamic spawn, ladder/camera mutation, player traversal through spawn/building/warehouse/container/upper-floor zones, death overlay/restart, third-person/first-person/ADS camera distances, crouch/prone heights, jump impulse, first-person weapon visual switching, third-person player weapon switching, NPC weapon mounts, skin color application to player weapons/tracer, procedural audio events for gunfire/hit/pickup/reload/enemy-shot/footstep feedback, and procedural character motion feedback.
- Player-POV gate counts: enemies after runtime reinforcement, loot objects, usable ladders, active GLB instances/renderers in the player camera, foreground GLB coverage, zero runtime GLB loader mounts, clear camera-to-player target line, visible player character renderers in the actual player camera frustum, side panels hidden for gameplay, and cleared start-round prompt/hit-marker UI.

## Main Scripts

- `Assets/Scripts/Tactical/TacticalGameManager.cs`
- `Assets/Scripts/Tactical/TacticalPlayerController.cs`
- `Assets/Scripts/Tactical/TacticalCameraFollow.cs`
- `Assets/Scripts/Tactical/TacticalEnemy.cs`
- `Assets/Scripts/Tactical/TacticalLoot.cs`
- `Assets/Scripts/Tactical/TacticalWeaponSpec.cs`
- `Assets/Editor/TacticalPrototypeTools.cs`
- `Assets/Editor/TacticalPlayerPovGate.cs`
- `Assets/Editor/TacticalGameplayProofGate.cs`

## How To Play

1. Open `Assets/Scenes/TacticalPrototype.unity` or use `AI Tools > Open Tactical Playable Scene`.
2. Press Play.
3. Press Enter, Space, left click, or the Start button to leave the lobby. Controls are ignored while the lobby overlay is active.
4. The round starts in first-person by default. Press V to switch between first-person and third-person. The first-person weapon is visible in first-person mode and while holding right mouse for ADS.
5. Fire with left mouse. Press R to reload after firing a few shots; if the magazine is full, the HUD will say `弹匣已满`.
6. Move with WASD / arrow keys. Hold Shift to sprint.
7. Press F near loot.
8. Press 1-4 to switch unlocked weapons. Only the pistol is unlocked at spawn; shotgun/rifle/DMR must be picked up before switching.
9. Press 5/6/7 to use bandage / first aid / medkit.
10. Press O for settings, L for skin gacha, Tab for help.

## Known Limits

- This pass aims to match the HTML tactical game first. It is not yet the later PUBG-like realistic rebuild.
- Imported GLBs are instantiated into gameplay objects at scene-generation time; dynamic runtime NPCs now clone a pre-imported reinforcement template, with the old runtime loader path retained only as a missing-template fallback.
- Settings/help/skin-gacha panels now exist and are included in gates; visual polish and more manual playtesting still need work.
- Audio now has procedural feedback parity for the HTML-style gunfire/hit/pickup/reload/enemy-shot/footstep loop, but it is not yet mixed or authored like a production game.
- Character GLBs now have generated Unity AnimationClip assets for idle/walk/aim/fire/hit/down states, plus procedural root, arm, leg, head, aim/fire/down-state, and hit-pulse motion. This is stronger than static capsules, but a skinned production humanoid import remains below the later realistic-game ambition.
- The main generated scene now instantiates GLBs in the editor, so startup no longer waits for the old 80+ runtime GLB mount load path.
- Repeatable player-POV verification is available via `AI Tools > Write Tactical Player POV Gate Report`, which writes `docs/TACTICAL_PLAYER_POV_GATE.json` while Play Mode is running.
- Repeatable player-view camera evidence is available via `AI Tools > Capture Tactical Verified Player POV Screenshot`, which starts the round, snaps the player camera, clears stale transient UI, temporarily routes the HUD canvas through the camera, and writes `Assets/Screenshots/tactical_html_replica_current_player_pov_verified.png` using a direct camera render instead of an asynchronous Game View screenshot.
- Repeatable gameplay-state verification is available via `AI Tools > Write Tactical Gameplay Proof Gate`, which writes `docs/TACTICAL_GAMEPLAY_PROOF_GATE.json` while Play Mode is running.
");
        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] Wrote docs/TACTICAL_PROTOTYPE_REPORT.md");
    }

    private static void EnsureFolders()
    {
        foreach (var folder in new[] { "Assets/Scenes", "Assets/Materials", "Assets/Prefabs", "Assets/Scripts", "Assets/Scripts/Tactical", "Assets/HtmlTacticalAssets", ApprovedMaterialRoot, ApprovedAnimationRoot, "docs" })
        {
            Directory.CreateDirectory(folder);
        }
    }

    private static void ImportApprovedAssets()
    {
        foreach (var relativePath in new[]
        {
            ApprovedCrateGlbPath,
            ApprovedContainerGlbPath,
            ApprovedPlayerGlbPath,
            ApprovedEnemyGlbPath,
            ApprovedMedicalLootGlbPath,
            ApprovedAmmoLootGlbPath,
            ApprovedHelmetLootGlbPath,
            ApprovedVestLootGlbPath,
            RealifiedCrateGlbPath,
            RealifiedContainerGlbPath,
            RealifiedPlayerGlbPath,
            RealifiedEnemyGlbPath,
            HtmlPistolGlbPath,
            HtmlShotgunGlbPath,
            HtmlRifleGlbPath,
            HtmlDmrGlbPath,
            RealifiedAmmoLootGlbPath,
            RealifiedMedkitLootGlbPath,
            RealifiedHelmetLootGlbPath,
            RealifiedVestLootGlbPath,
            RealifiedSidearmGlbPath,
            RealifiedHeroRifleGlbPath,
            RealifiedSecondaryWeaponGlbPath
        })
        {
            var assetPath = "Assets/HtmlTacticalAssets/" + relativePath;
            if (File.Exists(assetPath))
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            }
        }
    }

    private static Dictionary<string, Material> CreateMaterials()
    {
        var mats = new Dictionary<string, Material>
        {
            ["ground"] = SaveMaterial("TacticalGround", new Color(0.22f, 0.34f, 0.24f), 0.85f),
            ["road"] = SavePbrMaterial("TacticalWetAsphaltApproved", ApprovedMaterialRoot + "/wet_asphalt/wet_asphalt", new Color(0.10f, 0.11f, 0.13f), 0.82f, new Vector2(9f, 18f)),
            ["wall"] = SavePbrMaterial("TacticalConcreteWallApproved", ApprovedMaterialRoot + "/concrete_wall/concrete_wall", new Color(0.42f, 0.48f, 0.54f), 0.48f, new Vector2(3.8f, 1.4f)),
            ["floor"] = SavePbrMaterial("TacticalConcreteFloorApproved", ApprovedMaterialRoot + "/concrete_wall/concrete_wall", new Color(0.44f, 0.46f, 0.48f), 0.42f, new Vector2(7f, 7f)),
            ["roof"] = SaveMaterial("TacticalRoof", new Color(0.16f, 0.20f, 0.25f), 0.70f),
            ["wood"] = SaveMaterial("TacticalWood", new Color(0.45f, 0.31f, 0.18f), 0.65f),
            ["player"] = SaveMaterial("TacticalPlayerBlue", new Color(0.16f, 0.40f, 0.95f), 0.5f),
            ["enemy"] = SaveMaterial("TacticalEnemyRed", new Color(0.78f, 0.22f, 0.26f), 0.55f),
            ["loot"] = SaveMaterial("TacticalLootGold", new Color(1.0f, 0.77f, 0.18f), 0.38f),
            ["medical"] = SaveMaterial("TacticalMedical", new Color(0.92f, 0.93f, 0.90f), 0.45f),
            ["armor"] = SaveMaterial("TacticalArmor", new Color(0.13f, 0.18f, 0.22f), 0.55f),
            ["containerBlue"] = SaveMaterial("TacticalContainerBlue", new Color(0.08f, 0.25f, 0.62f), 0.58f),
            ["containerRed"] = SaveMaterial("TacticalContainerRed", new Color(0.62f, 0.12f, 0.10f), 0.58f),
            ["containerGreen"] = SaveMaterial("TacticalContainerGreen", new Color(0.10f, 0.44f, 0.20f), 0.58f),
            ["cratePbr"] = SavePbrMaterial("TacticalCratePbrApproved", ApprovedMaterialRoot + "/tactical_crate/tactical_crate", new Color(0.48f, 0.34f, 0.18f), 0.52f, new Vector2(1.2f, 1.2f)),
            ["realifiedCratePbr"] = SavePbrMaterial("RealifiedCratePbrPromoted", "Assets/HtmlTacticalAssets/RealifiedAssets/RS_11_prop_crate_LOD0", new Color(0.46f, 0.33f, 0.18f), 0.50f, new Vector2(1.15f, 1.15f)),
            ["containerPbr"] = SavePbrMaterial("TacticalContainerPbrApproved", ApprovedMaterialRoot + "/container_checkpoint/container_checkpoint", new Color(0.22f, 0.30f, 0.36f), 0.46f, new Vector2(1.4f, 1.0f)),
            ["realifiedContainerPbr"] = SavePbrMaterial("RealifiedContainerPbrPromoted", "Assets/HtmlTacticalAssets/RealifiedAssets/RS_10_prop_container_LOD0", new Color(0.22f, 0.30f, 0.36f), 0.46f, new Vector2(1.25f, 1.0f)),
            ["realifiedAmmoLootPbr"] = SavePbrMaterial("RealifiedAmmoLootPbrPromoted", "Assets/HtmlTacticalAssets/RealifiedAssets/RS_08_loot_ammo_LOD0", new Color(0.76f, 0.66f, 0.36f), 0.42f, Vector2.one),
            ["realifiedMedkitLootPbr"] = SavePbrMaterial("RealifiedMedkitLootPbrPromoted", "Assets/HtmlTacticalAssets/RealifiedAssets/RS_09_loot_medkit_LOD0", new Color(0.80f, 0.82f, 0.76f), 0.44f, Vector2.one),
            ["realifiedHelmetPbr"] = SavePbrMaterial("RealifiedHelmetPbrPromoted", "Assets/HtmlTacticalAssets/RealifiedAssets/RS_06_gear_helmet_LOD0", new Color(0.18f, 0.22f, 0.24f), 0.50f, Vector2.one),
            ["realifiedVestPbr"] = SavePbrMaterial("RealifiedVestPbrPromoted", "Assets/HtmlTacticalAssets/RealifiedAssets/RS_07_gear_vest_LOD0", new Color(0.16f, 0.20f, 0.19f), 0.52f, Vector2.one),
            ["playerPbr"] = SavePbrMaterial("TacticalPlayerPbrApproved", ApprovedMaterialRoot + "/player_tactical/player_tactical", new Color(0.20f, 0.32f, 0.42f), 0.50f, Vector2.one),
            ["enemyPbr"] = SavePbrMaterial("TacticalEnemyPbrApproved", ApprovedMaterialRoot + "/enemy_tactical/enemy_tactical", new Color(0.42f, 0.24f, 0.24f), 0.48f, Vector2.one),
            ["characterPlate"] = SaveMaterial("TacticalCharacterPlatePbrApproved", new Color(0.075f, 0.090f, 0.085f), 0.40f),
            ["characterWebbing"] = SaveMaterial("TacticalCharacterWebbingPbrApproved", new Color(0.030f, 0.036f, 0.034f), 0.32f),
            ["characterVisor"] = SaveMaterial("TacticalCharacterVisorPbrApproved", new Color(0.035f, 0.095f, 0.110f), 0.72f),
            ["playerPatch"] = SaveMaterial("TacticalPlayerSquadPatchApproved", new Color(0.10f, 0.34f, 0.92f), 0.42f),
            ["enemyPatch"] = SaveMaterial("TacticalEnemySquadPatchApproved", new Color(0.78f, 0.08f, 0.13f), 0.42f),
            ["weaponSidearmPbr"] = SavePbrMaterial("TacticalWeaponSidearmPbrApproved", ApprovedMaterialRoot + "/weapon_sidearm/weapon_sidearm", new Color(0.24f, 0.25f, 0.26f), 0.46f, Vector2.one),
            ["weaponHeroRiflePbr"] = SavePbrMaterial("TacticalWeaponHeroRiflePbrApproved", ApprovedMaterialRoot + "/weapon_hero_rifle/weapon_hero_rifle", new Color(0.18f, 0.20f, 0.19f), 0.42f, Vector2.one),
            ["weaponSecondaryPbr"] = SavePbrMaterial("TacticalWeaponSecondaryPbrApproved", ApprovedMaterialRoot + "/weapon_secondary/weapon_secondary", new Color(0.22f, 0.23f, 0.25f), 0.44f, Vector2.one),
            ["realifiedSidearmPbr"] = SavePbrMaterial("RealifiedSidearmPbrPromoted", "Assets/HtmlTacticalAssets/RealifiedAssets/RS_02_sidearm_LOD0", new Color(0.24f, 0.25f, 0.26f), 0.46f, Vector2.one),
            ["realifiedHeroRiflePbr"] = SavePbrMaterial("RealifiedHeroRiflePbrPromoted", "Assets/HtmlTacticalAssets/RealifiedAssets/hero_rifle_LOD0", new Color(0.18f, 0.20f, 0.19f), 0.42f, Vector2.one),
            ["realifiedSecondaryWeaponPbr"] = SavePbrMaterial("RealifiedSecondaryWeaponPbrPromoted", "Assets/HtmlTacticalAssets/RealifiedAssets/RS_03_secondary_weapon_LOD0", new Color(0.22f, 0.23f, 0.25f), 0.44f, Vector2.one),
            ["fpGunMetal"] = SaveMaterial("TacticalFpGunMetalPbrApproved", new Color(0.045f, 0.050f, 0.055f), 0.62f),
            ["fpGripRubber"] = SaveMaterial("TacticalFpGripRubberPbrApproved", new Color(0.018f, 0.020f, 0.022f), 0.38f),
            ["fpOpticGlass"] = SaveMaterial("TacticalFpOpticGlassPbrApproved", new Color(0.035f, 0.105f, 0.135f), 0.86f),
            ["fpAccent"] = SaveMaterial("TacticalFpWeaponAccentPbrApproved", new Color(0.72f, 0.63f, 0.38f), 0.48f),
            ["fpGlove"] = SaveMaterial("TacticalFpGlovePbrApproved", new Color(0.022f, 0.026f, 0.025f), 0.34f),
            ["fpSleeve"] = SaveMaterial("TacticalFpSleevePbrApproved", new Color(0.08f, 0.11f, 0.13f), 0.42f)
        };
        return mats;
    }

    private static Material SaveMaterial(string name, Color color, float smoothness)
    {
        var path = "Assets/Materials/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        mat.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Material SavePbrMaterial(string name, string texturePrefix, Color fallbackColor, float smoothness, Vector2 tileScale)
    {
        var baseMapPath = texturePrefix + "_basecolor.png";
        if (!File.Exists(baseMapPath))
        {
            return SaveMaterial(name, fallbackColor, smoothness);
        }

        ConfigureTextureImporter(baseMapPath, false);
        ConfigureTextureImporter(texturePrefix + "_normal.png", true);
        ConfigureTextureImporter(texturePrefix + "_metallic.png", false);
        ConfigureTextureImporter(texturePrefix + "_ao.png", false);
        ConfigureTextureImporter(texturePrefix + "_roughness.png", false);

        var path = "Assets/Materials/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.color = fallbackColor;
        SetTextureIfPresent(mat, "_BaseMap", baseMapPath, tileScale);
        SetTextureIfPresent(mat, "_BumpMap", texturePrefix + "_normal.png", tileScale);
        SetTextureIfPresent(mat, "_MetallicGlossMap", texturePrefix + "_metallic.png", tileScale);
        SetTextureIfPresent(mat, "_OcclusionMap", texturePrefix + "_ao.png", tileScale);
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_BumpScale", 0.65f);
        mat.EnableKeyword("_NORMALMAP");
        mat.EnableKeyword("_METALLICSPECGLOSSMAP");
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void ConfigureTextureImporter(string path, bool normalMap)
    {
        if (!File.Exists(path))
        {
            return;
        }

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
        {
            return;
        }

        var changed = false;
        var desiredType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
        if (importer.textureType != desiredType)
        {
            importer.textureType = desiredType;
            changed = true;
        }

        if (!normalMap && path.Contains("_basecolor") == false && importer.sRGBTexture)
        {
            importer.sRGBTexture = false;
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

    private static void SetTextureIfPresent(Material mat, string propertyName, string texturePath, Vector2 tileScale)
    {
        if (!File.Exists(texturePath) || !mat.HasProperty(propertyName))
        {
            return;
        }

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture == null)
        {
            return;
        }

        mat.SetTexture(propertyName, texture);
        mat.SetTextureScale(propertyName, tileScale);
    }

    private static void CreateLighting(Dictionary<string, Material> materials)
    {
        var sun = new GameObject("Sun Light").AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.25f;
        sun.color = new Color(0.78f, 0.86f, 1f);
        sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(42f, -38f, 0f);

        var fill = new GameObject("Soft Fill Light").AddComponent<Light>();
        fill.type = LightType.Point;
        fill.intensity = 0.65f;
        fill.range = 34f;
        fill.color = new Color(0.50f, 0.62f, 0.74f);
        fill.transform.position = new Vector3(0f, 15f, 10f);
    }

    private static void CreateVisualPolish(Dictionary<string, Material> materials)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.20f, 0.25f, 0.29f);
        RenderSettings.fogDensity = 0.010f;
        RenderSettings.ambientLight = new Color(0.18f, 0.21f, 0.24f);

        var profile = CreateTacticalPostProcessProfile();
        var volumeObject = new GameObject("Tactical Post Process Volume");
        var volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;
        volume.profile = profile;

        CreateIndustrialSpot("Tactical Industrial Yard Light 01", new Vector3(-24f, 7.5f, 18f), new Vector3(58f, 34f, 0f), new Color(1f, 0.78f, 0.48f), 4.7f, 32f, 54f);
        CreateIndustrialSpot("Tactical Industrial Yard Light 02", new Vector3(26f, 7.2f, -18f), new Vector3(58f, -146f, 0f), new Color(0.56f, 0.72f, 1f), 3.7f, 30f, 52f);
        CreateIndustrialSpot("Tactical Warehouse Ceiling Light", new Vector3(0f, 5.4f, -28f), new Vector3(90f, 0f, 0f), new Color(1f, 0.86f, 0.62f), 3.1f, 24f, 68f);

        Box("Tactical Light Housing 01", new Vector3(-24f, 6.7f, 18f), new Vector3(1.2f, 0.32f, 0.52f), materials["armor"]);
        Box("Tactical Light Housing 02", new Vector3(26f, 6.5f, -18f), new Vector3(1.2f, 0.32f, 0.52f), materials["armor"]);
        Box("Tactical Warehouse Light Housing", new Vector3(0f, 5.1f, -28f), new Vector3(2.2f, 0.18f, 0.42f), materials["medical"]);

        CreateM85VisualProductionDetails(materials);
        CreateRainField();
    }

    private static void CreateM85VisualProductionDetails(Dictionary<string, Material> materials)
    {
        var puddle = SaveMaterial("M85WetPuddleDecal", new Color(0.025f, 0.036f, 0.045f), 0.96f);
        var mud = SaveMaterial("M85MudScuffDecal", new Color(0.090f, 0.075f, 0.055f), 0.30f);
        var paint = SaveMaterial("M85FadedRoadPaintDecal", new Color(0.92f, 0.72f, 0.24f), 0.44f);
        var cable = SaveMaterial("M85BlackCableBundle", new Color(0.012f, 0.013f, 0.014f), 0.36f);
        var brass = SaveMaterial("M85SpentCasingMetal", new Color(0.78f, 0.58f, 0.28f), 0.62f);
        var grime = SaveMaterial("M85WallGrimeDecal", new Color(0.050f, 0.058f, 0.055f), 0.28f);
        var warning = SaveMaterial("M85WarningMarkerRed", new Color(0.70f, 0.08f, 0.06f), 0.38f);

        var puddles = new[]
        {
            new Vector3(-4.8f, 0.075f, 20.2f), new Vector3(5.4f, 0.075f, 17.6f),
            new Vector3(-1.8f, 0.075f, 30.8f), new Vector3(8.8f, 0.075f, 8.2f),
            new Vector3(-12.5f, 0.075f, -2.8f), new Vector3(14.2f, 0.075f, -6.8f),
            new Vector3(-3.0f, 0.075f, -23.5f), new Vector3(3.7f, 0.075f, -32.2f)
        };
        for (var i = 0; i < puddles.Length; i++)
        {
            var obj = Box("M85 Wet Puddle Decal " + (i + 1), puddles[i], new Vector3(3.6f + i % 3 * 0.45f, 0.018f, 1.0f + i % 2 * 0.35f), puddle);
            obj.transform.eulerAngles = new Vector3(0f, i * 23f, 0f);
            DisableCollider(obj);
        }

        var scuffs = new[]
        {
            new Vector3(-7.0f, 0.095f, 21.0f), new Vector3(-9.2f, 0.095f, 24.0f),
            new Vector3(9.1f, 0.095f, 20.5f), new Vector3(11.6f, 0.095f, 24.6f),
            new Vector3(-39.0f, 0.095f, -14.2f), new Vector3(-36.0f, 0.095f, -12.4f),
            new Vector3(31.2f, 0.095f, -15.8f), new Vector3(35.4f, 0.095f, -14.8f),
            new Vector3(3.0f, 0.095f, -25.5f), new Vector3(-2.4f, 0.095f, -30.4f)
        };
        for (var i = 0; i < scuffs.Length; i++)
        {
            var obj = Box("M85 Mud Scuff Decal " + (i + 1), scuffs[i], new Vector3(1.6f, 0.016f, 0.22f), mud);
            obj.transform.eulerAngles = new Vector3(0f, 15f + i * 31f, 0f);
            DisableCollider(obj);
        }

        for (var i = 0; i < 10; i++)
        {
            var z = -34f + i * 7.5f;
            var left = Box("M85 Faded Road Paint Stripe L " + (i + 1), new Vector3(-2.2f, 0.11f, z), new Vector3(1.65f, 0.018f, 0.16f), paint);
            var right = Box("M85 Faded Road Paint Stripe R " + (i + 1), new Vector3(2.2f, 0.11f, z), new Vector3(1.65f, 0.018f, 0.16f), paint);
            left.transform.eulerAngles = new Vector3(0f, 4f, 0f);
            right.transform.eulerAngles = new Vector3(0f, -4f, 0f);
            DisableCollider(left);
            DisableCollider(right);
        }

        var cablePositions = new[]
        {
            new Vector3(-11.0f, 0.16f, 17.6f), new Vector3(-8.8f, 0.16f, 17.1f),
            new Vector3(9.8f, 0.16f, 19.5f), new Vector3(12.4f, 0.16f, 19.0f),
            new Vector3(-1.7f, 0.16f, -19.8f), new Vector3(2.6f, 0.16f, -23.8f)
        };
        for (var i = 0; i < cablePositions.Length; i++)
        {
            var obj = Box("M85 Cable Bundle " + (i + 1), cablePositions[i], new Vector3(2.2f, 0.055f, 0.08f), cable);
            obj.transform.eulerAngles = new Vector3(0f, 12f + i * 27f, 0f);
            DisableCollider(obj);
        }

        for (var i = 0; i < 16; i++)
        {
            var x = 70.5f + (i % 4) * 0.22f;
            var z = 70.8f + (i / 4) * 0.18f;
            var casingObject = Box("M85 Spent Casing " + (i + 1), new Vector3(x, 0.13f, z), new Vector3(0.12f, 0.035f, 0.035f), brass);
            casingObject.transform.eulerAngles = new Vector3(0f, i * 19f, 0f);
            DisableCollider(casingObject);
        }

        var grimeTargets = new[]
        {
            new Vector3(-38f, 1.18f, -13.62f), new Vector3(-38f, 2.42f, -13.62f),
            new Vector3(32f, 1.18f, -15.62f), new Vector3(32f, 2.42f, -15.62f),
            new Vector3(0f, 1.30f, -18.62f), new Vector3(0f, 2.62f, -18.62f)
        };
        for (var i = 0; i < grimeTargets.Length; i++)
        {
            var obj = Box("M85 Wall Grime Decal " + (i + 1), grimeTargets[i], new Vector3(2.5f, 0.62f, 0.035f), grime);
            DisableCollider(obj);
        }

        var redMarker = Box("M85 Checkpoint Warning Marker", new Vector3(0f, 0.13f, 16.4f), new Vector3(7.8f, 0.025f, 0.22f), warning);
        DisableCollider(redMarker);
    }

    private static VolumeProfile CreateTacticalPostProcessProfile()
    {
        var path = "Assets/Materials/TacticalPostProcessProfile.asset";
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
        }

        if (!profile.TryGet<ColorAdjustments>(out var color))
        {
            color = profile.Add<ColorAdjustments>(true);
        }
        color.postExposure.Override(-0.08f);
        color.contrast.Override(21f);
        color.saturation.Override(-7f);
        color.colorFilter.Override(new Color(0.92f, 0.97f, 1f));

        if (!profile.TryGet<Bloom>(out var bloom))
        {
            bloom = profile.Add<Bloom>(true);
        }
        bloom.threshold.Override(1.18f);
        bloom.intensity.Override(0.34f);
        bloom.scatter.Override(0.58f);
        bloom.tint.Override(new Color(1f, 0.88f, 0.68f));

        if (!profile.TryGet<Vignette>(out var vignette))
        {
            vignette = profile.Add<Vignette>(true);
        }
        vignette.intensity.Override(0.20f);
        vignette.smoothness.Override(0.46f);
        vignette.color.Override(new Color(0.015f, 0.018f, 0.022f));

        if (!profile.TryGet<FilmGrain>(out var grain))
        {
            grain = profile.Add<FilmGrain>(true);
        }
        grain.intensity.Override(0.11f);
        grain.response.Override(0.72f);

        EditorUtility.SetDirty(profile);
        return profile;
    }

    private static void CreateIndustrialSpot(string name, Vector3 position, Vector3 eulerAngles, Color color, float intensity, float range, float angle)
    {
        var light = new GameObject(name).AddComponent<Light>();
        light.type = LightType.Spot;
        light.transform.position = position;
        light.transform.eulerAngles = eulerAngles;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.spotAngle = angle;
        light.shadows = LightShadows.Soft;
    }

    private static void CreateRainField()
    {
        var rainObject = new GameObject("Tactical Rain Field");
        rainObject.transform.position = new Vector3(0f, 17f, 6f);
        var rain = rainObject.AddComponent<ParticleSystem>();
        var main = rain.main;
        main.loop = true;
        main.duration = 8f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = 2.1f;
        main.startSpeed = 18f;
        main.startSize = 0.035f;
        main.maxParticles = 3600;
        main.startColor = new Color(0.74f, 0.86f, 1f, 0.42f);

        var emission = rain.emission;
        emission.rateOverTime = 620f;

        var shape = rain.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(150f, 1f, 128f);

        var velocity = rain.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = -22f;
        velocity.x = -0.6f;

        var renderer = rainObject.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 6.0f;
        renderer.velocityScale = 0.12f;
        renderer.material = SaveParticleMaterial();
    }

    private static Material SaveParticleMaterial()
    {
        var path = "Assets/Materials/TacticalRainParticle.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            material = new Material(shader != null ? shader : Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(material, path);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", new Color(0.72f, 0.86f, 1f, 0.36f));
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", new Color(0.72f, 0.86f, 1f, 0.36f));
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void CreateGround(Dictionary<string, Material> materials)
    {
        Box("Ground", new Vector3(0f, -0.08f, 5f), new Vector3(180f, 0.16f, 140f), materials["ground"]);
        Box("Central Asphalt Road", new Vector3(0f, 0.01f, 5f), new Vector3(18f, 0.08f, 118f), materials["road"]);
        Box("Cross Asphalt Road", new Vector3(0f, 0.02f, 5f), new Vector3(150f, 0.08f, 12f), materials["road"]);
    }

    private static void CreateCompound(Dictionary<string, Material> materials)
    {
        Box("North Compound Wall", new Vector3(0f, 1.5f, -58f), new Vector3(160f, 3f, 1.2f), materials["wall"]);
        Box("South Compound Wall", new Vector3(0f, 1.5f, 68f), new Vector3(160f, 3f, 1.2f), materials["wall"]);
        Box("West Compound Wall", new Vector3(-82f, 1.5f, 5f), new Vector3(1.2f, 3f, 126f), materials["wall"]);
        Box("East Compound Wall", new Vector3(82f, 1.5f, 5f), new Vector3(1.2f, 3f, 126f), materials["wall"]);
    }

    private static void CreateBuildings(Dictionary<string, Material> materials)
    {
        CreateBuilding("A", new Vector3(-38f, 0f, -22f), new Vector2(18f, 16f), 3, materials);
        CreateBuilding("B", new Vector3(32f, 0f, -24f), new Vector2(20f, 16f), 2, materials);
        CreateBuilding("C", new Vector3(-42f, 0f, 28f), new Vector2(18f, 18f), 2, materials);
        CreateBuilding("D", new Vector3(36f, 0f, 28f), new Vector2(22f, 18f), 3, materials);
    }

    private static void CreateBuilding(string label, Vector3 basePosition, Vector2 size, int floors, Dictionary<string, Material> materials)
    {
        const float floorHeight = 4.1f;
        for (var floor = 0; floor < floors; floor++)
        {
            var y = floor * floorHeight;
            Box(label + " Floor " + (floor + 1), new Vector3(basePosition.x, y + 0.08f, basePosition.z), new Vector3(size.x + 1f, 0.28f, size.y + 1f), materials["floor"]);
            var doorWidth = floor == 0 ? 4.6f : 0f;
            if (doorWidth > 0f)
            {
                Box(label + " Front Wall Left " + (floor + 1), new Vector3(basePosition.x - doorWidth * 0.5f - (size.x - doorWidth) * 0.25f, y + 1.85f, basePosition.z + size.y / 2f), new Vector3((size.x - doorWidth) * 0.5f, 3.4f, 0.45f), materials["wall"]);
                Box(label + " Front Wall Right " + (floor + 1), new Vector3(basePosition.x + doorWidth * 0.5f + (size.x - doorWidth) * 0.25f, y + 1.85f, basePosition.z + size.y / 2f), new Vector3((size.x - doorWidth) * 0.5f, 3.4f, 0.45f), materials["wall"]);
                Box(label + " Door Lintel " + (floor + 1), new Vector3(basePosition.x, y + 3.42f, basePosition.z + size.y / 2f), new Vector3(doorWidth, 0.32f, 0.5f), materials["wall"]);
                var step = Box(label + " Door Step " + (floor + 1), new Vector3(basePosition.x, y + 0.12f, basePosition.z + size.y / 2f + 0.56f), new Vector3(doorWidth + 0.8f, 0.12f, 1.25f), materials["floor"]);
                DisableCollider(step);
            }
            else
            {
                Box(label + " Front Wall " + (floor + 1), new Vector3(basePosition.x, y + 1.85f, basePosition.z + size.y / 2f), new Vector3(size.x, 3.4f, 0.45f), materials["wall"]);
            }
            Box(label + " Back Wall " + (floor + 1), new Vector3(basePosition.x, y + 1.85f, basePosition.z - size.y / 2f), new Vector3(size.x, 3.4f, 0.45f), materials["wall"]);
            Box(label + " Left Wall " + (floor + 1), new Vector3(basePosition.x - size.x / 2f, y + 1.85f, basePosition.z), new Vector3(0.45f, 3.4f, size.y), materials["wall"]);
            Box(label + " Right Wall " + (floor + 1), new Vector3(basePosition.x + size.x / 2f, y + 1.85f, basePosition.z), new Vector3(0.45f, 3.4f, size.y), materials["wall"]);
            Box(label + " Window L " + (floor + 1), new Vector3(basePosition.x - size.x * 0.25f, y + 2.25f, basePosition.z + size.y / 2f + 0.28f), new Vector3(2.15f, 1.1f, 0.08f), materials["containerBlue"]);
            Box(label + " Window R " + (floor + 1), new Vector3(basePosition.x + size.x * 0.25f, y + 2.25f, basePosition.z + size.y / 2f + 0.28f), new Vector3(2.15f, 1.1f, 0.08f), materials["containerBlue"]);
            Box(label + " Window Awning L " + (floor + 1), new Vector3(basePosition.x - size.x * 0.25f, y + 3.02f, basePosition.z + size.y / 2f + 0.56f), new Vector3(2.65f, 0.12f, 0.55f), materials["roof"]);
            Box(label + " Window Awning R " + (floor + 1), new Vector3(basePosition.x + size.x * 0.25f, y + 3.02f, basePosition.z + size.y / 2f + 0.56f), new Vector3(2.65f, 0.12f, 0.55f), materials["roof"]);
            Box(label + " AC Unit " + (floor + 1), new Vector3(basePosition.x + size.x * 0.42f, y + 2.15f, basePosition.z + size.y / 2f + 0.7f), new Vector3(1.0f, 0.55f, 0.35f), materials["medical"]);
            Box(label + " Interior Table " + (floor + 1), new Vector3(basePosition.x - 2f, y + 0.55f, basePosition.z + 1.5f), new Vector3(2.2f, 0.35f, 1.2f), materials["wood"]);
            Box(label + " Interior Cabinet " + (floor + 1), new Vector3(basePosition.x + size.x / 2f - 1.6f, y + 1.0f, basePosition.z - size.y / 2f + 2.0f), new Vector3(1.7f, 2.0f, 1.0f), materials["wood"]);
            LootPositions.Add(new Vector3(basePosition.x - 2f, y + 0.95f, basePosition.z + 1.5f));
            LootPositions.Add(new Vector3(basePosition.x + size.x / 2f - 1.8f, y + 0.95f, basePosition.z - size.y / 2f + 2f));
        }

        Box(label + " Roof", new Vector3(basePosition.x, floors * floorHeight + 0.15f, basePosition.z), new Vector3(size.x + 1.2f, 0.38f, size.y + 1.2f), materials["roof"]);
        var ladder = Box(label + " Interior Ladder", new Vector3(basePosition.x - size.x / 2f + 2.35f, floors * floorHeight * 0.5f, basePosition.z + size.y / 2f - 3.25f), new Vector3(0.9f, floors * floorHeight, 0.12f), materials["loot"]);
        ladder.AddComponent<TacticalLadder>().Configure((floors - 1) * floorHeight + 1.04f, floors);
        AttachHtmlGlb(ladder, "models/prop_ladder_stair_final.glb", Vector3.zero, Vector3.zero, Vector3.one * 0.9f, false);
        AttachHtmlGlb(Anchor(label + " Building Detail GLB", new Vector3(basePosition.x, 0.02f, basePosition.z + size.y / 2f + 1.2f)), "models/prop_building_detail_final.glb", Vector3.zero, Vector3.zero, Vector3.one * 1.1f, false);
        var marker = CreateWorldLabel(label + "栋", new Vector3(basePosition.x, 2.8f, basePosition.z + size.y / 2f + 1.1f));
        marker.transform.SetParent(GameObject.Find(label + " Front Wall 1")?.transform, true);
    }

    private static void CreateWarehouse(Dictionary<string, Material> materials)
    {
        var center = new Vector3(0f, 0f, -28f);
        Box("Warehouse Floor", center + new Vector3(0f, 0.08f, 0f), new Vector3(23f, 0.18f, 19f), materials["floor"]);
        Box("Warehouse North Wall", center + new Vector3(0f, 2.6f, -9f), new Vector3(22f, 5.2f, 0.65f), materials["wall"]);
        Box("Warehouse South Wall", center + new Vector3(0f, 2.6f, 9f), new Vector3(22f, 5.2f, 0.65f), materials["wall"]);
        Box("Warehouse West Wall", center + new Vector3(-11f, 2.6f, 0f), new Vector3(0.65f, 5.2f, 18f), materials["wall"]);
        Box("Warehouse East Wall", center + new Vector3(11f, 2.6f, 0f), new Vector3(0.65f, 5.2f, 18f), materials["wall"]);
        Box("Warehouse Roof", center + new Vector3(0f, 5.55f, 0f), new Vector3(23.2f, 0.55f, 19.2f), materials["roof"]);

        for (var i = 0; i < 5; i++)
        {
            var x = -7f + i * 3.5f;
            var crate = Box("Warehouse Loot Crate " + (i + 1), new Vector3(x, 0.6f, -27f + (i % 2 == 0 ? -3f : 3f)), new Vector3(2.6f, 1.2f, 2f), materials["cratePbr"]);
            AttachHtmlGlb(crate, ApprovedCrateGlbPath, Vector3.zero, Vector3.zero, Vector3.one * 0.95f, true, materials["cratePbr"]);
            LootPositions.Add(new Vector3(x, 1.35f, -27f + (i % 2 == 0 ? -3f : 3f)));
        }
        AttachHtmlGlb(Anchor("Warehouse Furniture GLB", center + new Vector3(4f, 0.25f, 1.5f)), "models/prop_interior_furniture_final.glb", Vector3.zero, Vector3.zero, Vector3.one * 1.0f, false);
    }

    private static void CreateContainers(Dictionary<string, Material> materials)
    {
        var data = new (float x, float z, string mat)[]
        {
            (-10f, 10f, "containerBlue"), (0f, 14f, "containerRed"), (12f, 10f, "containerGreen"),
            (-22f, 5f, "containerRed"), (23f, 3f, "containerBlue"), (-16f, 30f, "containerGreen"),
            (18f, 23.5f, "containerBlue"), (-18f, -5f, "containerBlue"), (18f, -8f, "containerRed")
        };

        for (var i = 0; i < data.Length; i++)
        {
            var container = Box("Container " + (i + 1), new Vector3(data[i].x, 1.62f, data[i].z), new Vector3(7.4f, 3.25f, 2.9f), materials["containerPbr"]);
            AttachHtmlGlb(container, RealifiedContainerGlbPath, Vector3.zero, Vector3.zero, Vector3.one, true, materials["realifiedContainerPbr"]);
            LootPositions.Add(new Vector3(data[i].x, 3.45f, data[i].z));
        }
    }

    private static void CreateYardDetails(Dictionary<string, Material> materials)
    {
        var trees = new[]
        {
            new Vector3(-70f, 0f, -45f), new Vector3(-64f, 0f, 55f), new Vector3(68f, 0f, 48f),
            new Vector3(73f, 0f, -38f), new Vector3(-76f, 0f, 8f), new Vector3(55f, 0f, 18f)
        };
        foreach (var pos in trees)
        {
            AttachHtmlGlb(Anchor("HTML Tree", pos), "models/prop_tree_final.glb", Vector3.zero, Vector3.zero, Vector3.one * 1.1f, false);
        }

        var rocks = new[]
        {
            new Vector3(-55f, 0f, 36f), new Vector3(58f, 0f, 18f), new Vector3(20f, 0f, 58f),
            new Vector3(-38f, 0f, -48f), new Vector3(72f, 0f, 6f)
        };
        foreach (var pos in rocks)
        {
            AttachHtmlGlb(Anchor("HTML Rock Cluster", pos), "models/prop_rock_cluster_final.glb", Vector3.zero, Vector3.zero, Vector3.one, false);
        }

        AttachHtmlGlb(Anchor("HTML Ground Fence North", new Vector3(0f, 0f, -50f)), "models/prop_ground_wall_fence_final.glb", Vector3.zero, Vector3.zero, Vector3.one * 2.4f, false);
        AttachHtmlGlb(Anchor("HTML Ground Fence South", new Vector3(0f, 0f, 58f)), "models/prop_ground_wall_fence_final.glb", Vector3.zero, new Vector3(0f, 180f, 0f), Vector3.one * 1.7f, false);
    }

    private static void CreateSpawnStagingArea(Dictionary<string, Material> materials)
    {
        var leftContainer = Box("Spawn Staging Left Container", new Vector3(-15.2f, 1.62f, 22.8f), new Vector3(5.8f, 3.1f, 2.7f), materials["containerPbr"]);
        AttachHtmlGlb(leftContainer, RealifiedContainerGlbPath, Vector3.zero, Vector3.zero, Vector3.one * 0.92f, true, materials["realifiedContainerPbr"]);
        var rightContainer = Box("Spawn Staging Right Container", new Vector3(15.4f, 1.62f, 20.0f), new Vector3(5.8f, 3.1f, 2.7f), materials["containerPbr"]);
        AttachHtmlGlb(rightContainer, RealifiedContainerGlbPath, Vector3.zero, new Vector3(0f, 180f, 0f), Vector3.one * 0.92f, true, materials["realifiedContainerPbr"]);

        var approvedSideContainer = Box("Approved Container Staging Prop", new Vector3(24.2f, 1.62f, 24.8f), new Vector3(5.8f, 3.1f, 2.7f), materials["containerPbr"]);
        AttachHtmlGlb(approvedSideContainer, RealifiedContainerGlbPath, Vector3.zero, new Vector3(0f, -12f, 0f), Vector3.one * 0.92f, true, materials["realifiedContainerPbr"]);

        for (var i = 0; i < 4; i++)
        {
            var x = i < 2 ? -13.4f + i * 3.6f : 9.8f + (i - 2) * 3.6f;
            var crate = Box("Spawn Supply Crate " + (i + 1), new Vector3(x, 0.62f, 17.2f + (i % 2) * 2.0f), new Vector3(2.2f, 1.2f, 1.6f), materials["cratePbr"]);
            AttachHtmlGlb(crate, ApprovedCrateGlbPath, Vector3.zero, Vector3.zero, Vector3.one * 0.82f, true, materials["cratePbr"]);
        }

        var leftNearCrate = Box("Spawn Near Cover Left", new Vector3(-11.8f, 0.58f, 25.8f), new Vector3(1.85f, 1.05f, 1.45f), materials["cratePbr"]);
        AttachHtmlGlb(leftNearCrate, ApprovedCrateGlbPath, Vector3.zero, new Vector3(0f, 18f, 0f), Vector3.one * 0.78f, true, materials["cratePbr"]);
        var rightNearCrate = Box("Spawn Near Cover Right", new Vector3(11.8f, 0.58f, 24.8f), new Vector3(1.85f, 1.05f, 1.45f), materials["cratePbr"]);
        AttachHtmlGlb(rightNearCrate, ApprovedCrateGlbPath, Vector3.zero, new Vector3(0f, -16f, 0f), Vector3.one * 0.78f, true, materials["cratePbr"]);

        var approvedSideCrate = Box("Approved Crate Staging Prop", new Vector3(-14.6f, 0.62f, 20.6f), new Vector3(2.2f, 1.2f, 1.6f), materials["cratePbr"]);
        AttachHtmlGlb(approvedSideCrate, ApprovedCrateGlbPath, Vector3.zero, new Vector3(0f, 12f, 0f), Vector3.one * 0.82f, true, materials["cratePbr"]);

        Box("Spawn Near Cover Marker Left", new Vector3(-10.2f, 0.06f, 25.3f), new Vector3(2.1f, 0.04f, 0.55f), materials["loot"]);
        Box("Spawn Near Cover Marker Right", new Vector3(10.2f, 0.06f, 25.2f), new Vector3(2.1f, 0.04f, 0.55f), materials["loot"]);

        Box("Spawn Road Barrier Left", new Vector3(-10.8f, 0.62f, 14.2f), new Vector3(3.8f, 1.05f, 0.42f), materials["loot"]);
        Box("Spawn Road Barrier Right", new Vector3(10.9f, 0.62f, 15.2f), new Vector3(3.8f, 1.05f, 0.42f), materials["loot"]);
        Box("Spawn Barrier Stripe Left", new Vector3(-10.8f, 1.18f, 13.98f), new Vector3(3.35f, 0.12f, 0.08f), materials["medical"]);
        Box("Spawn Barrier Stripe Right", new Vector3(10.9f, 1.18f, 14.98f), new Vector3(3.35f, 0.12f, 0.08f), materials["medical"]);

        Box("Spawn Checkpoint Light Pole", new Vector3(-8.8f, 2.7f, 18.2f), new Vector3(0.22f, 5.4f, 0.22f), materials["armor"]);
        Box("Spawn Checkpoint Light Bar", new Vector3(-8.8f, 5.2f, 18.2f), new Vector3(4.6f, 0.18f, 0.18f), materials["armor"]);
        var lamp = new GameObject("Spawn Checkpoint Warm Light").AddComponent<Light>();
        lamp.type = LightType.Point;
        lamp.color = new Color(1f, 0.78f, 0.42f);
        lamp.intensity = 2.0f;
        lamp.range = 16f;
        lamp.transform.position = new Vector3(-8.8f, 4.8f, 18.8f);

        AttachHtmlGlb(Anchor("Spawn Fence Detail Left", new Vector3(-9.5f, 0.05f, 17f)), "models/prop_ground_wall_fence_final.glb", Vector3.zero, new Vector3(0f, 90f, 0f), Vector3.one * 0.72f, false);
        AttachHtmlGlb(Anchor("Spawn Fence Detail Right", new Vector3(9.5f, 0.05f, 17f)), "models/prop_ground_wall_fence_final.glb", Vector3.zero, new Vector3(0f, -90f, 0f), Vector3.one * 0.72f, false);

        LootPositions.Add(new Vector3(-11.8f, 1.25f, 18.1f));
        LootPositions.Add(new Vector3(11.8f, 1.25f, 20.0f));
        LootPositions.Add(new Vector3(-10.2f, 1.25f, 25.3f));
        LootPositions.Add(new Vector3(13.6f, 3.45f, 20.0f));
    }

    private static TacticalPlayerController CreatePlayer(Dictionary<string, Material> materials)
    {
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.05f, 30f);
        player.GetComponent<Renderer>().sharedMaterial = materials["player"];
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        var controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.45f;
        controller.center = new Vector3(0f, 1f, 0f);
        player.AddComponent<TacticalPlayerController>();
        var visual = AttachHtmlGlb(player, RealifiedPlayerGlbPath, new Vector3(0f, -0.02f, 0f), new Vector3(0f, 180f, 0f), Vector3.one * 0.92f, true);
        AddTacticalCharacterDetailKit(visual.transform, false, materials);
        player.AddComponent<TacticalCharacterMotionVisual>().Configure(visual.transform);
        ConfigureCharacterClipVisual(player.gameObject, visual.transform, "player_tactical");

        var cameraTarget = new GameObject("Camera Target").transform;
        cameraTarget.SetParent(player.transform);
        cameraTarget.localPosition = new Vector3(0f, 1.5f, 0f);
        SetObjectField(player.GetComponent<TacticalPlayerController>(), "cameraTarget", cameraTarget);
        return player.GetComponent<TacticalPlayerController>();
    }

    private static void ConfigureCharacterClipVisual(GameObject owner, Transform visualRoot, string libraryName)
    {
        var library = CreateCharacterClipLibrary(visualRoot, libraryName);
        owner.AddComponent<TacticalCharacterClipVisual>().Configure(
            visualRoot,
            library.Idle,
            library.Walk,
            library.Aim,
            library.Fire,
            library.Hit,
            library.Down);
    }

    private static CharacterClipLibrary CreateCharacterClipLibrary(Transform visualRoot, string libraryName)
    {
        var folder = ApprovedAnimationRoot + "/" + libraryName;
        Directory.CreateDirectory(folder);
        return new CharacterClipLibrary
        {
            Idle = CreateCharacterClip(visualRoot, folder + "/" + libraryName + "_idle.anim", libraryName + "_idle", WrapMode.Loop, 1.2f, clip =>
            {
                AddEulerCurves(clip, visualRoot, "head_balaclava", new Vector3(2f, 0f, 0f), new Vector3(-2f, 0f, 0f), new Vector3(2f, 0f, 0f));
                AddEulerCurves(clip, visualRoot, "torso_uniform", new Vector3(0f, -1.5f, 0f), new Vector3(0f, 1.5f, 0f), new Vector3(0f, -1.5f, 0f));
                AddEulerCurves(clip, visualRoot, "forearm_left", new Vector3(-4f, 0f, -4f), new Vector3(-7f, 0f, -2f), new Vector3(-4f, 0f, -4f));
                AddEulerCurves(clip, visualRoot, "forearm_right", new Vector3(-4f, 0f, 4f), new Vector3(-7f, 0f, 2f), new Vector3(-4f, 0f, 4f));
            }),
            Walk = CreateCharacterClip(visualRoot, folder + "/" + libraryName + "_walk.anim", libraryName + "_walk", WrapMode.Loop, 0.8f, clip =>
            {
                AddEulerCurves(clip, visualRoot, "torso_uniform", new Vector3(0f, 3f, 4f), new Vector3(0f, -3f, -4f), new Vector3(0f, 3f, 4f));
                AddEulerCurves(clip, visualRoot, "head_balaclava", new Vector3(4f, 0f, -2f), new Vector3(0f, 0f, 2f), new Vector3(4f, 0f, -2f));
                AddEulerCurves(clip, visualRoot, "upper_arm_left", new Vector3(22f, 0f, -6f), new Vector3(-22f, 0f, 6f), new Vector3(22f, 0f, -6f));
                AddEulerCurves(clip, visualRoot, "forearm_left", new Vector3(-10f, 0f, -4f), new Vector3(12f, 0f, 4f), new Vector3(-10f, 0f, -4f));
                AddEulerCurves(clip, visualRoot, "upper_arm_right", new Vector3(-22f, 0f, 6f), new Vector3(22f, 0f, -6f), new Vector3(-22f, 0f, 6f));
                AddEulerCurves(clip, visualRoot, "forearm_right", new Vector3(12f, 0f, 4f), new Vector3(-10f, 0f, -4f), new Vector3(12f, 0f, 4f));
                AddEulerCurves(clip, visualRoot, "thigh_left", new Vector3(-20f, 0f, 0f), new Vector3(20f, 0f, 0f), new Vector3(-20f, 0f, 0f));
                AddEulerCurves(clip, visualRoot, "shin_left", new Vector3(16f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(16f, 0f, 0f));
                AddEulerCurves(clip, visualRoot, "boot_left", new Vector3(-6f, 0f, 0f), new Vector3(5f, 0f, 0f), new Vector3(-6f, 0f, 0f));
                AddEulerCurves(clip, visualRoot, "thigh_right", new Vector3(20f, 0f, 0f), new Vector3(-20f, 0f, 0f), new Vector3(20f, 0f, 0f));
                AddEulerCurves(clip, visualRoot, "shin_right", new Vector3(0f, 0f, 0f), new Vector3(16f, 0f, 0f), new Vector3(0f, 0f, 0f));
                AddEulerCurves(clip, visualRoot, "boot_right", new Vector3(5f, 0f, 0f), new Vector3(-6f, 0f, 0f), new Vector3(5f, 0f, 0f));
            }),
            Aim = CreateCharacterClip(visualRoot, folder + "/" + libraryName + "_aim.anim", libraryName + "_aim", WrapMode.Loop, 0.9f, clip =>
            {
                AddEulerCurves(clip, visualRoot, "torso_uniform", new Vector3(0f, 5f, 0f), new Vector3(0f, 7f, 1f), new Vector3(0f, 5f, 0f));
                AddEulerCurves(clip, visualRoot, "head_balaclava", new Vector3(8f, 0f, 0f), new Vector3(6f, 0f, 0f), new Vector3(8f, 0f, 0f));
                AddEulerCurves(clip, visualRoot, "upper_arm_left", new Vector3(-36f, 0f, -18f), new Vector3(-38f, 0f, -16f), new Vector3(-36f, 0f, -18f));
                AddEulerCurves(clip, visualRoot, "forearm_left", new Vector3(-24f, 0f, -10f), new Vector3(-26f, 0f, -8f), new Vector3(-24f, 0f, -10f));
                AddEulerCurves(clip, visualRoot, "upper_arm_right", new Vector3(-46f, 0f, 18f), new Vector3(-48f, 0f, 16f), new Vector3(-46f, 0f, 18f));
                AddEulerCurves(clip, visualRoot, "forearm_right", new Vector3(-30f, 0f, 10f), new Vector3(-32f, 0f, 8f), new Vector3(-30f, 0f, 10f));
            }),
            Fire = CreateCharacterClip(visualRoot, folder + "/" + libraryName + "_fire.anim", libraryName + "_fire", WrapMode.Once, 0.35f, clip =>
            {
                AddEulerCurves(clip, visualRoot, "torso_uniform", new Vector3(0f, 7f, 0f), new Vector3(5f, 10f, -3f), new Vector3(0f, 6f, 0f));
                AddEulerCurves(clip, visualRoot, "head_balaclava", new Vector3(7f, 0f, 0f), new Vector3(3f, 0f, 0f), new Vector3(7f, 0f, 0f));
                AddEulerCurves(clip, visualRoot, "upper_arm_left", new Vector3(-36f, 0f, -18f), new Vector3(-46f, 0f, -20f), new Vector3(-36f, 0f, -18f));
                AddEulerCurves(clip, visualRoot, "upper_arm_right", new Vector3(-48f, 0f, 18f), new Vector3(-62f, 0f, 20f), new Vector3(-46f, 0f, 18f));
                AddEulerCurves(clip, visualRoot, "forearm_right", new Vector3(-30f, 0f, 10f), new Vector3(-44f, 0f, 11f), new Vector3(-30f, 0f, 10f));
            }),
            Hit = CreateCharacterClip(visualRoot, folder + "/" + libraryName + "_hit.anim", libraryName + "_hit", WrapMode.Once, 0.45f, clip =>
            {
                AddEulerCurves(clip, visualRoot, "torso_uniform", new Vector3(0f, 0f, 0f), new Vector3(12f, -8f, 9f), new Vector3(0f, -2f, 0f));
                AddEulerCurves(clip, visualRoot, "head_balaclava", new Vector3(0f, 0f, 0f), new Vector3(-10f, 4f, -8f), new Vector3(0f, 0f, 0f));
                AddEulerCurves(clip, visualRoot, "upper_arm_left", new Vector3(0f, 0f, 0f), new Vector3(-18f, 0f, -22f), new Vector3(0f, 0f, -5f));
                AddEulerCurves(clip, visualRoot, "upper_arm_right", new Vector3(0f, 0f, 0f), new Vector3(22f, 0f, 18f), new Vector3(0f, 0f, 5f));
            }),
            Down = CreateCharacterClip(visualRoot, folder + "/" + libraryName + "_down.anim", libraryName + "_down", WrapMode.ClampForever, 0.6f, clip =>
            {
                AddEulerCurves(clip, visualRoot, "torso_uniform", new Vector3(0f, 0f, 0f), new Vector3(-20f, 8f, 26f), new Vector3(-26f, 8f, 58f));
                AddEulerCurves(clip, visualRoot, "head_balaclava", new Vector3(0f, 0f, 0f), new Vector3(-10f, 0f, 16f), new Vector3(-16f, 0f, 24f));
                AddEulerCurves(clip, visualRoot, "upper_arm_left", new Vector3(0f, 0f, 0f), new Vector3(-38f, 0f, -12f), new Vector3(-70f, 0f, -18f));
                AddEulerCurves(clip, visualRoot, "upper_arm_right", new Vector3(0f, 0f, 0f), new Vector3(-38f, 0f, 12f), new Vector3(-70f, 0f, 18f));
                AddEulerCurves(clip, visualRoot, "thigh_left", new Vector3(0f, 0f, 0f), new Vector3(-30f, 0f, 0f), new Vector3(-48f, 0f, 0f));
                AddEulerCurves(clip, visualRoot, "thigh_right", new Vector3(0f, 0f, 0f), new Vector3(-34f, 0f, 0f), new Vector3(-52f, 0f, 0f));
                AddEulerCurves(clip, visualRoot, "shin_left", new Vector3(0f, 0f, 0f), new Vector3(20f, 0f, 0f), new Vector3(34f, 0f, 0f));
                AddEulerCurves(clip, visualRoot, "shin_right", new Vector3(0f, 0f, 0f), new Vector3(24f, 0f, 0f), new Vector3(36f, 0f, 0f));
            })
        };
    }

    private static AnimationClip CreateCharacterClip(Transform visualRoot, string path, string clipName, WrapMode wrapMode, float length, System.Action<AnimationClip> configure)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }

        clip.name = clipName;
        clip.frameRate = 30f;
        clip.legacy = false;
        clip.wrapMode = wrapMode;
        ClearClipCurves(clip);
        configure(clip);
        AddEulerCurves(clip, visualRoot, "glove_left", Vector3.zero, new Vector3(-8f, 0f, 0f), Vector3.zero, length);
        AddEulerCurves(clip, visualRoot, "glove_right", Vector3.zero, new Vector3(-8f, 0f, 0f), Vector3.zero, length);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static void ClearClipCurves(AnimationClip clip)
    {
        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            AnimationUtility.SetEditorCurve(clip, binding, null);
        }
    }

    private static void AddEulerCurves(AnimationClip clip, Transform root, string partName, Vector3 start, Vector3 middle, Vector3 end, float length = 1f)
    {
        var part = FindDescendant(root, partName);
        if (part == null)
        {
            return;
        }

        var path = AnimationUtility.CalculateTransformPath(part, root);
        var midTime = Mathf.Max(0.05f, length * 0.5f);
        AddEulerCurve(clip, path, "x", start.x, middle.x, end.x, midTime, length);
        AddEulerCurve(clip, path, "y", start.y, middle.y, end.y, midTime, length);
        AddEulerCurve(clip, path, "z", start.z, middle.z, end.z, midTime, length);
    }

    private static void AddEulerCurve(AnimationClip clip, string path, string axis, float start, float middle, float end, float midTime, float endTime)
    {
        var binding = EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw." + axis);
        var curve = new AnimationCurve(
            new Keyframe(0f, start),
            new Keyframe(midTime, middle),
            new Keyframe(Mathf.Max(endTime, midTime + 0.05f), end));
        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }

    private static Transform FindDescendant(Transform root, string partName)
    {
        if (root == null)
        {
            return null;
        }

        if (MatchesCharacterPart(root.name, partName))
        {
            return root;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var found = FindDescendant(root.GetChild(i), partName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static bool MatchesCharacterPart(string nodeName, string partName)
    {
        if (nodeName == partName || nodeName.Contains(partName))
        {
            return true;
        }

        return partName switch
        {
            "torso_uniform" => nodeName.Contains("anatomical_torso_underlayer") || nodeName.Contains("front_plate_carrier"),
            "head_balaclava" => nodeName.Contains("human_face_visible_under_helmet") || nodeName.Contains("ballistic_helmet_shell"),
            "upper_arm_left" => nodeName.Contains("upper_arm_cloth_-0.39") || nodeName.Contains("shoulder_armor_pad_-0.39"),
            "forearm_left" => nodeName.Contains("forearm_glove_sleeve_-0.39"),
            "glove_left" => nodeName.Contains("black_tactical_glove_-0.39"),
            "upper_arm_right" => nodeName.Contains("upper_arm_cloth_0.39") || nodeName.Contains("shoulder_armor_pad_0.39"),
            "forearm_right" => nodeName.Contains("forearm_glove_sleeve_0.39"),
            "glove_right" => nodeName.Contains("black_tactical_glove_0.39"),
            "thigh_left" => nodeName.Contains("upper_leg_fatigues_-0.18"),
            "shin_left" => nodeName.Contains("lower_leg_boot_gaiter_-0.18"),
            "boot_left" => nodeName.Contains("boot_with_sculpted_sole_-0.18"),
            "thigh_right" => nodeName.Contains("upper_leg_fatigues_0.18"),
            "shin_right" => nodeName.Contains("lower_leg_boot_gaiter_0.18"),
            "boot_right" => nodeName.Contains("boot_with_sculpted_sole_0.18"),
            _ => false
        };
    }

    private sealed class CharacterClipLibrary
    {
        public AnimationClip Idle;
        public AnimationClip Walk;
        public AnimationClip Aim;
        public AnimationClip Fire;
        public AnimationClip Hit;
        public AnimationClip Down;
    }

    private static void CreatePlayerThirdPersonWeapon(TacticalPlayerController player, TacticalGameManager manager, Dictionary<string, Material> materials)
    {
        var weaponRoot = new GameObject("Third Person Weapon Visual");
        weaponRoot.transform.SetParent(player.transform, false);
        weaponRoot.transform.localPosition = new Vector3(0.22f, 1.16f, 0.64f);
        weaponRoot.transform.localEulerAngles = new Vector3(-7f, 0f, 0f);
        var visual = weaponRoot.AddComponent<TacticalThirdPersonWeaponVisual>();
        visual.ConfigurePlayer(player, manager);
        CreateCharacterWeapon(weaponRoot.transform, "pistol", WeaponGlbPath("pistol"), Vector3.zero, Vector3.one * 0.11f, WeaponPbrMaterial(materials, "pistol"));
        CreateCharacterWeapon(weaponRoot.transform, "shotgun", "models/shotgun_m5_candidate.glb", Vector3.zero, Vector3.one * 0.12f, WeaponPbrMaterial(materials, "shotgun"));
        CreateCharacterWeapon(weaponRoot.transform, "rifle", WeaponGlbPath("rifle"), Vector3.zero, Vector3.one * 0.12f, WeaponPbrMaterial(materials, "rifle"));
        CreateCharacterWeapon(weaponRoot.transform, "dmr", WeaponGlbPath("dmr"), Vector3.zero, Vector3.one * 0.11f, WeaponPbrMaterial(materials, "dmr"));
        visual.ForceRefresh();
    }

    private static void CreateNpcWeapon(GameObject enemy, string weaponId, Dictionary<string, Material> materials)
    {
        var weaponRoot = new GameObject("NPC Weapon Visual");
        weaponRoot.transform.SetParent(enemy.transform, false);
        weaponRoot.transform.localPosition = new Vector3(0.18f, 1.14f, 0.62f);
        weaponRoot.transform.localEulerAngles = new Vector3(-7f, 0f, 0f);
        var visual = weaponRoot.AddComponent<TacticalThirdPersonWeaponVisual>();
        visual.ConfigureFixedWeapon(weaponId);
        CreateCharacterWeapon(weaponRoot.transform, weaponId, WeaponGlbPath(weaponId), Vector3.zero, Vector3.one * (weaponId == "pistol" ? 0.11f : 0.12f), WeaponPbrMaterial(materials, weaponId));
        visual.ForceRefresh();
    }

    private static void CreateNpcWeaponSet(GameObject enemy, Dictionary<string, Material> materials)
    {
        var weaponRoot = new GameObject("NPC Weapon Visual");
        weaponRoot.transform.SetParent(enemy.transform, false);
        weaponRoot.transform.localPosition = new Vector3(0.18f, 1.14f, 0.62f);
        weaponRoot.transform.localEulerAngles = new Vector3(-7f, 0f, 0f);
        var visual = weaponRoot.AddComponent<TacticalThirdPersonWeaponVisual>();
        visual.ConfigureFixedWeapon("pistol");
        CreateCharacterWeapon(weaponRoot.transform, "pistol", WeaponGlbPath("pistol"), Vector3.zero, Vector3.one * 0.11f, WeaponPbrMaterial(materials, "pistol"));
        CreateCharacterWeapon(weaponRoot.transform, "rifle", WeaponGlbPath("rifle"), Vector3.zero, Vector3.one * 0.12f, WeaponPbrMaterial(materials, "rifle"));
        visual.ForceRefresh();
    }

    private static void CreateCharacterWeapon(Transform parent, string weaponId, string path, Vector3 localPosition, Vector3 localScale, Material overrideMaterial)
    {
        var weapon = new GameObject("Character Weapon - " + weaponId);
        weapon.transform.SetParent(parent, false);
        AttachHtmlGlb(weapon, path, localPosition, new Vector3(0f, 180f, 0f), localScale, false, overrideMaterial);
    }

    private static Material WeaponPbrMaterial(Dictionary<string, Material> materials, string weaponId)
    {
        if (materials == null)
        {
            return null;
        }

        return weaponId switch
        {
            "pistol" => materials["realifiedSidearmPbr"],
            "rifle" => materials["realifiedHeroRiflePbr"],
            "shotgun" => materials["weaponSecondaryPbr"],
            "dmr" => materials["realifiedSecondaryWeaponPbr"],
            _ => materials["realifiedSidearmPbr"]
        };
    }

    private static Camera CreateCamera(TacticalPlayerController player, Dictionary<string, Material> materials)
    {
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 9f, 20f);
        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 62f;
        camera.allowHDR = true;
        var cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
        cameraData.renderPostProcessing = true;
        cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        cameraData.antialiasingQuality = AntialiasingQuality.High;
        cameraObject.AddComponent<AudioListener>();
        var follow = cameraObject.AddComponent<TacticalCameraFollow>();
        SetObjectField(follow, "player", player);
        var weaponRoot = new GameObject("First Person Weapon Visual");
        weaponRoot.transform.SetParent(cameraObject.transform, false);
        var weaponVisual = weaponRoot.AddComponent<TacticalFirstPersonWeaponVisual>();
        SetObjectField(weaponVisual, "player", player);
        var weaponLight = new GameObject("First Person Weapon Key Light");
        weaponLight.transform.SetParent(cameraObject.transform, false);
        weaponLight.transform.localPosition = new Vector3(0.18f, -0.12f, 0.55f);
        var localLight = weaponLight.AddComponent<Light>();
        localLight.type = LightType.Point;
        localLight.color = new Color(0.72f, 0.82f, 1f);
        localLight.intensity = 0.75f;
        localLight.range = 2.4f;
        CreateFirstPersonWeapon(weaponRoot.transform, "pistol", WeaponGlbPath("pistol"), new Vector3(0.48f, -0.42f, 0.92f), Vector3.one * 0.16f, WeaponPbrMaterial(materials, "pistol"), materials);
        CreateFirstPersonWeapon(weaponRoot.transform, "shotgun", HtmlShotgunGlbPath, new Vector3(0.50f, -0.44f, 1.02f), Vector3.one * 0.16f, WeaponPbrMaterial(materials, "shotgun"), materials);
        CreateFirstPersonWeapon(weaponRoot.transform, "rifle", WeaponGlbPath("rifle"), new Vector3(0.50f, -0.42f, 0.98f), Vector3.one * 0.20f, WeaponPbrMaterial(materials, "rifle"), materials);
        CreateFirstPersonWeapon(weaponRoot.transform, "dmr", WeaponGlbPath("dmr"), new Vector3(0.52f, -0.43f, 1.02f), Vector3.one * 0.19f, WeaponPbrMaterial(materials, "dmr"), materials);
        return camera;
    }

    private static void CreateFirstPersonWeapon(Transform parent, string weaponId, string path, Vector3 localPosition, Vector3 localScale, Material overrideMaterial, Dictionary<string, Material> materials)
    {
        var weapon = new GameObject("FP Weapon - " + weaponId);
        weapon.transform.SetParent(parent, false);
        var sourcePosition = localPosition;
        var sourceScale = localScale;
        if (weaponId == "rifle")
        {
            sourcePosition = new Vector3(0.62f, -0.48f, 1.08f);
            sourceScale = Vector3.one * 0.080f;
        }
        else if (weaponId == "dmr" || weaponId == "shotgun")
        {
            sourcePosition = localPosition + new Vector3(0.10f, -0.10f, 0.12f);
            sourceScale = localScale * 0.62f;
        }

        var sourceGlb = AttachHtmlGlb(weapon, path, sourcePosition, new Vector3(0f, 180f, 0f), sourceScale, false, overrideMaterial);
        if (sourceGlb != null)
        {
            sourceGlb.name = "FP Gameplay Source GLB - " + weaponId + " - " + sourceGlb.name;
        }
        AddFirstPersonWeaponSilhouette(weapon.transform, weaponId, materials);
    }

    private static void AddFirstPersonWeaponSilhouette(Transform parent, string weaponId, Dictionary<string, Material> materials)
    {
        var isPistol = weaponId == "pistol";
        var barrelLength = isPistol ? 0.30f : weaponId == "shotgun" ? 0.64f : 0.58f;
        var rootX = isPistol ? 0.36f : 0.44f;
        var rootY = isPistol ? -0.49f : -0.40f;
        var rootZ = isPistol ? 0.84f : 0.90f;
        var metal = materials["fpGunMetal"];
        var rubber = materials["fpGripRubber"];
        var glass = materials["fpOpticGlass"];
        var accent = materials["fpAccent"];
        var glove = materials["fpGlove"];
        var sleeve = materials["fpSleeve"];

        var kit = new GameObject("FP authored weapon silhouette - " + weaponId);
        kit.transform.SetParent(parent, false);
        kit.transform.localPosition = Vector3.zero;
        kit.transform.localRotation = Quaternion.identity;

        AddPrimitivePart(kit.transform, "receiver", PrimitiveType.Cube, new Vector3(rootX, rootY, rootZ), Vector3.zero, new Vector3(isPistol ? 0.14f : 0.20f, 0.085f, isPistol ? 0.26f : 0.34f), metal);
        AddPrimitivePart(kit.transform, "upper receiver", PrimitiveType.Cube, new Vector3(rootX, rootY + 0.065f, rootZ + 0.02f), Vector3.zero, new Vector3(isPistol ? 0.13f : 0.19f, 0.032f, isPistol ? 0.22f : 0.32f), metal);
        AddPrimitivePart(kit.transform, "handguard", PrimitiveType.Cube, new Vector3(rootX, rootY + 0.010f, rootZ + barrelLength * 0.40f), Vector3.zero, new Vector3(isPistol ? 0.090f : 0.155f, 0.065f, barrelLength * 0.50f), metal);
        AddPrimitivePart(kit.transform, "barrel", PrimitiveType.Cylinder, new Vector3(rootX, rootY + 0.018f, rootZ + barrelLength * 0.78f), new Vector3(90f, 0f, 0f), new Vector3(0.016f, barrelLength * 0.32f, 0.016f), metal);
        AddPrimitivePart(kit.transform, "muzzle", PrimitiveType.Cylinder, new Vector3(rootX, rootY + 0.018f, rootZ + barrelLength * 1.10f), new Vector3(90f, 0f, 0f), new Vector3(0.026f, 0.040f, 0.026f), metal);
        AddPrimitivePart(kit.transform, "grip", PrimitiveType.Cube, new Vector3(rootX + 0.020f, rootY - 0.135f, rootZ - 0.085f), new Vector3(-13f, 0f, 0f), new Vector3(0.072f, 0.190f, 0.085f), rubber);
        AddPrimitivePart(kit.transform, "trigger guard", PrimitiveType.Cube, new Vector3(rootX, rootY - 0.090f, rootZ - 0.012f), Vector3.zero, new Vector3(0.095f, 0.020f, 0.115f), metal);
        AddPrimitivePart(kit.transform, "trigger", PrimitiveType.Cube, new Vector3(rootX, rootY - 0.115f, rootZ - 0.012f), new Vector3(-12f, 0f, 0f), new Vector3(0.018f, 0.055f, 0.030f), accent);
        AddPrimitivePart(kit.transform, "ejection port", PrimitiveType.Cube, new Vector3(rootX + 0.082f, rootY + 0.038f, rootZ + 0.050f), Vector3.zero, new Vector3(0.010f, 0.040f, 0.085f), accent);
        AddPrimitivePart(kit.transform, "charging handle", PrimitiveType.Cube, new Vector3(rootX, rootY + 0.098f, rootZ - 0.090f), Vector3.zero, new Vector3(0.140f, 0.020f, 0.030f), accent);
        AddPrimitivePart(kit.transform, "right glove firing hand", PrimitiveType.Cube, new Vector3(rootX + 0.055f, rootY - 0.235f, rootZ - 0.090f), new Vector3(-12f, 0f, 0f), new Vector3(0.125f, 0.075f, 0.110f), glove);
        AddPrimitivePart(kit.transform, "right sleeve cuff", PrimitiveType.Cube, new Vector3(rootX + 0.075f, rootY - 0.295f, rootZ - 0.165f), new Vector3(-18f, 0f, 0f), new Vector3(0.155f, 0.080f, 0.145f), sleeve);

        if (!isPistol)
        {
            AddPrimitivePart(kit.transform, "stock", PrimitiveType.Cube, new Vector3(rootX + 0.015f, rootY - 0.025f, rootZ - 0.34f), new Vector3(-5f, 0f, 0f), new Vector3(0.145f, 0.078f, 0.270f), rubber);
            AddPrimitivePart(kit.transform, "top rail", PrimitiveType.Cube, new Vector3(rootX, rootY + 0.094f, rootZ + 0.08f), Vector3.zero, new Vector3(0.165f, 0.018f, 0.52f), metal);
            for (var i = 0; i < 7; i++)
            {
                AddPrimitivePart(kit.transform, "rail tooth " + i, PrimitiveType.Cube, new Vector3(rootX, rootY + 0.115f, rootZ - 0.14f + i * 0.075f), Vector3.zero, new Vector3(0.180f, 0.014f, 0.020f), accent);
            }
            AddPrimitivePart(kit.transform, "optic body", PrimitiveType.Cube, new Vector3(rootX, rootY + 0.155f, rootZ + 0.02f), Vector3.zero, new Vector3(0.120f, 0.070f, 0.120f), metal);
            AddPrimitivePart(kit.transform, "optic mount", PrimitiveType.Cube, new Vector3(rootX, rootY + 0.116f, rootZ + 0.020f), Vector3.zero, new Vector3(0.086f, 0.040f, 0.052f), rubber);
            AddPrimitivePart(kit.transform, "optic lens", PrimitiveType.Cube, new Vector3(rootX, rootY + 0.155f, rootZ + 0.088f), Vector3.zero, new Vector3(0.090f, 0.048f, 0.012f), glass);
            AddPrimitivePart(kit.transform, "optic rear glass", PrimitiveType.Cube, new Vector3(rootX, rootY + 0.155f, rootZ - 0.048f), Vector3.zero, new Vector3(0.088f, 0.047f, 0.012f), glass);
            AddPrimitivePart(kit.transform, "magazine", PrimitiveType.Cube, new Vector3(rootX - 0.004f, rootY - 0.170f, rootZ + 0.060f), new Vector3(8f, 0f, 0f), new Vector3(0.105f, 0.225f, 0.100f), rubber);
            AddPrimitivePart(kit.transform, "foregrip", PrimitiveType.Cube, new Vector3(rootX, rootY - 0.145f, rootZ + 0.260f), new Vector3(8f, 0f, 0f), new Vector3(0.075f, 0.200f, 0.075f), rubber);
            AddPrimitivePart(kit.transform, "left support glove", PrimitiveType.Cube, new Vector3(rootX - 0.085f, rootY - 0.205f, rootZ + 0.260f), new Vector3(6f, 0f, -8f), new Vector3(0.150f, 0.078f, 0.125f), glove);
            AddPrimitivePart(kit.transform, "left sleeve cuff", PrimitiveType.Cube, new Vector3(rootX - 0.145f, rootY - 0.285f, rootZ + 0.180f), new Vector3(-20f, 0f, -10f), new Vector3(0.160f, 0.080f, 0.170f), sleeve);
            AddPrimitivePart(kit.transform, "sling loop front", PrimitiveType.Cube, new Vector3(rootX, rootY + 0.030f, rootZ + barrelLength * 0.58f), Vector3.zero, new Vector3(0.145f, 0.012f, 0.018f), accent);
            AddPrimitivePart(kit.transform, "sling loop rear", PrimitiveType.Cube, new Vector3(rootX, rootY + 0.020f, rootZ - 0.235f), Vector3.zero, new Vector3(0.130f, 0.012f, 0.018f), accent);
            AddPrimitivePart(kit.transform, "paint wear receiver edge", PrimitiveType.Cube, new Vector3(rootX - 0.004f, rootY + 0.094f, rootZ + 0.155f), Vector3.zero, new Vector3(0.168f, 0.006f, 0.040f), accent);
            AddPrimitivePart(kit.transform, "paint wear handguard edge", PrimitiveType.Cube, new Vector3(rootX - 0.004f, rootY + 0.054f, rootZ + 0.380f), Vector3.zero, new Vector3(0.132f, 0.006f, 0.055f), accent);
        }
        else
        {
            AddPrimitivePart(kit.transform, "pistol front sight", PrimitiveType.Cube, new Vector3(rootX, rootY + 0.100f, rootZ + 0.120f), Vector3.zero, new Vector3(0.040f, 0.026f, 0.018f), accent);
            AddPrimitivePart(kit.transform, "pistol rear sight", PrimitiveType.Cube, new Vector3(rootX, rootY + 0.098f, rootZ - 0.090f), Vector3.zero, new Vector3(0.060f, 0.024f, 0.020f), accent);
            AddPrimitivePart(kit.transform, "pistol magazine base", PrimitiveType.Cube, new Vector3(rootX + 0.010f, rootY - 0.245f, rootZ - 0.105f), new Vector3(-8f, 0f, 0f), new Vector3(0.090f, 0.028f, 0.100f), accent);
        }
    }

    private static void AddPrimitivePart(Transform parent, string name, PrimitiveType primitive, Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale, Material material)
    {
        var part = GameObject.CreatePrimitive(primitive);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localEulerAngles = localEulerAngles;
        part.transform.localScale = localScale;
        var renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        var collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }
    }

    private static void AddTacticalCharacterDetailKit(Transform parent, bool enemy, Dictionary<string, Material> materials)
    {
        var kit = new GameObject("Tactical Character Detail Kit - " + (enemy ? "enemy" : "player"));
        kit.transform.SetParent(parent, false);
        kit.transform.localPosition = Vector3.zero;
        kit.transform.localRotation = Quaternion.identity;

        var plate = materials["characterPlate"];
        var webbing = materials["characterWebbing"];
        var visor = materials["characterVisor"];
        var patch = enemy ? materials["enemyPatch"] : materials["playerPatch"];

        AddPrimitivePart(kit.transform, "detail_ballistic_helmet_shell", PrimitiveType.Cube, new Vector3(0f, 1.62f, 0.00f), Vector3.zero, new Vector3(0.54f, 0.18f, 0.46f), plate);
        AddPrimitivePart(kit.transform, "detail_helmet_front_lip", PrimitiveType.Cube, new Vector3(0f, 1.56f, -0.25f), Vector3.zero, new Vector3(0.48f, 0.045f, 0.055f), plate);
        AddPrimitivePart(kit.transform, "detail_helmet_back_lip", PrimitiveType.Cube, new Vector3(0f, 1.56f, 0.25f), Vector3.zero, new Vector3(0.48f, 0.045f, 0.055f), plate);
        AddPrimitivePart(kit.transform, "detail_goggle_lens", PrimitiveType.Cube, new Vector3(0f, 1.50f, -0.265f), Vector3.zero, new Vector3(0.38f, 0.060f, 0.026f), visor);
        AddPrimitivePart(kit.transform, "detail_chest_plate", PrimitiveType.Cube, new Vector3(0f, 1.10f, -0.19f), new Vector3(-5f, 0f, 0f), new Vector3(0.55f, 0.56f, 0.105f), plate);
        AddPrimitivePart(kit.transform, "detail_back_plate", PrimitiveType.Cube, new Vector3(0f, 1.10f, 0.22f), new Vector3(5f, 0f, 0f), new Vector3(0.55f, 0.58f, 0.120f), plate);
        AddPrimitivePart(kit.transform, "detail_soft_backpack", PrimitiveType.Cube, new Vector3(0f, 1.03f, 0.39f), Vector3.zero, new Vector3(0.44f, 0.52f, 0.20f), webbing);
        AddPrimitivePart(kit.transform, "detail_radio_pouch", PrimitiveType.Cube, new Vector3(-0.32f, 1.18f, 0.32f), Vector3.zero, new Vector3(0.12f, 0.26f, 0.11f), webbing);
        AddPrimitivePart(kit.transform, "detail_radio_antenna", PrimitiveType.Cylinder, new Vector3(-0.38f, 1.55f, 0.35f), new Vector3(0f, 0f, 7f), new Vector3(0.012f, 0.36f, 0.012f), webbing);
        AddPrimitivePart(kit.transform, "detail_left_shoulder_pad", PrimitiveType.Cube, new Vector3(-0.39f, 1.24f, -0.01f), new Vector3(0f, 0f, 8f), new Vector3(0.18f, 0.12f, 0.34f), plate);
        AddPrimitivePart(kit.transform, "detail_right_shoulder_pad", PrimitiveType.Cube, new Vector3(0.39f, 1.24f, -0.01f), new Vector3(0f, 0f, -8f), new Vector3(0.18f, 0.12f, 0.34f), plate);
        AddPrimitivePart(kit.transform, "detail_left_arm_patch", PrimitiveType.Cube, new Vector3(-0.47f, 1.14f, -0.16f), new Vector3(0f, 0f, 8f), new Vector3(0.035f, 0.12f, 0.11f), patch);
        AddPrimitivePart(kit.transform, "detail_right_arm_patch", PrimitiveType.Cube, new Vector3(0.47f, 1.14f, -0.16f), new Vector3(0f, 0f, -8f), new Vector3(0.035f, 0.12f, 0.11f), patch);
        AddPrimitivePart(kit.transform, "detail_battle_belt", PrimitiveType.Cube, new Vector3(0f, 0.78f, -0.02f), Vector3.zero, new Vector3(0.58f, 0.085f, 0.34f), webbing);
        AddPrimitivePart(kit.transform, "detail_left_mag_pouch", PrimitiveType.Cube, new Vector3(-0.24f, 0.88f, -0.23f), new Vector3(-8f, 0f, 0f), new Vector3(0.12f, 0.22f, 0.075f), webbing);
        AddPrimitivePart(kit.transform, "detail_right_mag_pouch", PrimitiveType.Cube, new Vector3(0.24f, 0.88f, -0.23f), new Vector3(-8f, 0f, 0f), new Vector3(0.12f, 0.22f, 0.075f), webbing);
        AddPrimitivePart(kit.transform, "detail_left_knee_pad", PrimitiveType.Cube, new Vector3(-0.17f, 0.47f, -0.16f), new Vector3(-7f, 0f, 0f), new Vector3(0.17f, 0.13f, 0.055f), plate);
        AddPrimitivePart(kit.transform, "detail_right_knee_pad", PrimitiveType.Cube, new Vector3(0.17f, 0.47f, -0.16f), new Vector3(-7f, 0f, 0f), new Vector3(0.17f, 0.13f, 0.055f), plate);
        AddPrimitivePart(kit.transform, "detail_left_boot_sole", PrimitiveType.Cube, new Vector3(-0.16f, 0.12f, -0.02f), Vector3.zero, new Vector3(0.20f, 0.07f, 0.36f), webbing);
        AddPrimitivePart(kit.transform, "detail_right_boot_sole", PrimitiveType.Cube, new Vector3(0.16f, 0.12f, -0.02f), Vector3.zero, new Vector3(0.20f, 0.07f, 0.36f), webbing);
    }

    private static TacticalGameManager CreateGameManager(TacticalPlayerController player, Camera camera, HudRefs ui, Dictionary<string, Material> materials)
    {
        var managerObject = new GameObject("Tactical Game Manager");
        var manager = managerObject.AddComponent<TacticalGameManager>();
        SetObjectField(manager, "player", player);
        SetObjectField(manager, "playerCamera", camera);
        SetObjectField(manager, "hpText", ui.hp);
        SetObjectField(manager, "staminaText", ui.stamina);
        SetObjectField(manager, "weaponText", ui.weapon);
        SetObjectField(manager, "ammoText", ui.ammo);
        SetObjectField(manager, "armorText", ui.armor);
        SetObjectField(manager, "inventoryText", ui.inventory);
        SetObjectField(manager, "npcText", ui.npc);
        SetObjectField(manager, "coinText", ui.coin);
        SetObjectField(manager, "skinText", ui.skin);
        SetObjectField(manager, "messageText", ui.message);
        SetObjectField(manager, "promptText", ui.prompt);
        SetObjectField(manager, "crosshairText", ui.crosshair);
        SetObjectField(manager, "hitMarkerText", ui.hitMarker);
        SetObjectField(manager, "skinCoinText", ui.skinCoin);
        SetObjectField(manager, "shardText", ui.shards);
        SetObjectField(manager, "npcStrengthValueText", ui.npcStrengthValue);
        SetObjectField(manager, "spawnRateValueText", ui.spawnRateValue);
        SetObjectField(manager, "lootRichnessValueText", ui.lootRichnessValue);
        SetObjectField(manager, "lobbyPanel", ui.lobbyPanel);
        SetObjectField(manager, "deathPanel", ui.deathPanel);
        SetObjectField(manager, "helpPanel", ui.helpPanel);
        SetObjectField(manager, "settingsPanel", ui.settingsPanel);
        SetObjectField(manager, "skinsPanel", ui.skinsPanel);
        SetObjectField(manager, "npcStrengthSlider", ui.npcStrengthSlider);
        SetObjectField(manager, "spawnRateSlider", ui.spawnRateSlider);
        SetObjectField(manager, "lootRichnessSlider", ui.lootRichnessSlider);
        SetObjectField(manager, "rollSkinButton", ui.rollSkinButton);
        SetObjectField(manager, "closeSkinButton", ui.closeSkinButton);
        CreatePlayerThirdPersonWeapon(player, manager, materials);
        foreach (var button in ui.lobbyPanel.GetComponentsInChildren<Button>(true))
        {
            UnityEventTools.AddPersistentListener(button.onClick, manager.StartRound);
            EditorUtility.SetDirty(button);
        }
        foreach (var button in ui.deathPanel.GetComponentsInChildren<Button>(true))
        {
            UnityEventTools.AddPersistentListener(button.onClick, manager.StartRound);
            EditorUtility.SetDirty(button);
        }
        return manager;
    }

    private static void CreateLoot(Dictionary<string, Material> materials)
    {
        var specs = new (TacticalLootKind kind, string name, int level, string weapon)[]
        {
            (TacticalLootKind.Ammo, "通用弹药", 1, ""),
            (TacticalLootKind.Bandage, "绷带", 1, ""),
            (TacticalLootKind.FirstAid, "急救包", 1, ""),
            (TacticalLootKind.Medkit, "全能医疗箱", 1, ""),
            (TacticalLootKind.Revive, "复活晶石", 1, ""),
            (TacticalLootKind.Vest, "二级防弹衣", 2, ""),
            (TacticalLootKind.Helmet, "二级头盔", 2, ""),
            (TacticalLootKind.Weapon, "武器：BREACH-12", 1, "shotgun"),
            (TacticalLootKind.Weapon, "武器：TAC-AR", 1, "rifle"),
            (TacticalLootKind.Weapon, "武器：XMR-7", 1, "dmr")
        };

        for (var i = 0; i < LootPositions.Count; i++)
        {
            var spec = specs[i % specs.Length];
            var loot = GameObject.CreatePrimitive(spec.kind == TacticalLootKind.Weapon ? PrimitiveType.Cube : PrimitiveType.Sphere);
            loot.transform.position = LootPositions[i];
            loot.transform.localScale = spec.kind == TacticalLootKind.Weapon ? new Vector3(1.1f, 0.28f, 0.45f) : Vector3.one * 0.55f;
            loot.GetComponent<Renderer>().sharedMaterial = spec.kind is TacticalLootKind.FirstAid or TacticalLootKind.Medkit or TacticalLootKind.Bandage ? materials["medical"] :
                spec.kind is TacticalLootKind.Vest or TacticalLootKind.Helmet ? materials["armor"] : materials["loot"];
            loot.AddComponent<TacticalLoot>().Configure(spec.kind, spec.name, spec.level, spec.weapon);
            var lootGlbPath = LootGlbPath(spec.kind, spec.weapon);
            var promotedLoot = IsPromotedRealifiedLootGlb(lootGlbPath);
            var hidePlaceholder = IsApprovedLootGlb(lootGlbPath) || promotedLoot;
            AttachHtmlGlb(loot, lootGlbPath, Vector3.zero, Vector3.zero, Vector3.one * LootGlbScale(spec.kind, hidePlaceholder), hidePlaceholder, LootPbrMaterial(materials, spec.kind, spec.weapon, lootGlbPath));
        }
    }

    private static void CreateEnemies(Dictionary<string, Material> materials, TacticalGameManager manager, Transform playerTransform)
    {
        var spawnAreas = new[]
        {
            new Vector3(-66f, 1f, 48f), new Vector3(-66f, 1f, -44f), new Vector3(66f, 1f, 48f),
            new Vector3(66f, 1f, -44f), new Vector3(0f, 1f, 52f), new Vector3(-8f, 1f, 8f),
            new Vector3(18f, 1f, -4f), new Vector3(-30f, 1f, 6f), new Vector3(42f, 1f, 8f)
        };

        for (var i = 0; i < spawnAreas.Length; i++)
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "NPC Enemy " + (i + 1);
            enemy.transform.position = spawnAreas[i];
            enemy.GetComponent<Renderer>().sharedMaterial = materials["enemy"];
            Object.DestroyImmediate(enemy.GetComponent<CapsuleCollider>());
            var controller = enemy.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.center = new Vector3(0f, 1f, 0f);
            var tacticalEnemy = enemy.AddComponent<TacticalEnemy>();
            tacticalEnemy.Initialize(manager, playerTransform);
            var visual = AttachHtmlGlb(enemy, RealifiedEnemyGlbPath, new Vector3(0f, -0.02f, 0f), new Vector3(0f, 180f, 0f), Vector3.one * 0.92f, true);
            AddTacticalCharacterDetailKit(visual.transform, true, materials);
            enemy.AddComponent<TacticalCharacterMotionVisual>().Configure(visual.transform);
            ConfigureCharacterClipVisual(enemy, visual.transform, "enemy_tactical");
            CreateNpcWeapon(enemy, i % 3 == 0 ? "rifle" : "pistol", materials);
        }
    }

    private static void CreateReinforcementTemplate(Dictionary<string, Material> materials, TacticalGameManager manager)
    {
        var template = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        template.name = "Runtime NPC Reinforcement Template";
        template.transform.position = new Vector3(0f, -120f, 0f);
        template.GetComponent<Renderer>().sharedMaterial = materials["enemy"];
        Object.DestroyImmediate(template.GetComponent<CapsuleCollider>());
        var controller = template.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.45f;
        controller.center = new Vector3(0f, 1f, 0f);
        template.AddComponent<TacticalEnemy>();
        var visual = AttachHtmlGlb(template, RealifiedEnemyGlbPath, new Vector3(0f, -0.02f, 0f), new Vector3(0f, 180f, 0f), Vector3.one * 0.92f, true);
        AddTacticalCharacterDetailKit(visual.transform, true, materials);
        template.AddComponent<TacticalCharacterMotionVisual>().Configure(visual.transform);
        ConfigureCharacterClipVisual(template, visual.transform, "enemy_tactical");
        CreateNpcWeaponSet(template, materials);
        template.SetActive(false);
        SetObjectField(manager, "enemyReinforcementTemplate", template);
    }

    private static HudRefs CreateHud()
    {
        var canvasObject = new GameObject("HUD Canvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        var refs = new HudRefs
        {
            hp = Text(canvasObject.transform, "HP Text", "生命值: 100", new Vector2(20f, -18f)),
            stamina = Text(canvasObject.transform, "Stamina Text", "体力: 100", new Vector2(20f, -44f)),
            weapon = Text(canvasObject.transform, "Weapon Text", "当前武器: P-9 手枪", new Vector2(20f, -70f)),
            ammo = Text(canvasObject.transform, "Ammo Text", "弹药: 15 / 45", new Vector2(20f, -96f)),
            armor = Text(canvasObject.transform, "Armor Text", "防弹衣: 无  头盔: 无", new Vector2(20f, -122f)),
            inventory = Text(canvasObject.transform, "Inventory Text", "背包: 绷带 2  急救 0  全能 0", new Vector2(20f, -148f)),
            npc = Text(canvasObject.transform, "NPC Text", "NPC: 0  淘汰: 0", new Vector2(20f, -174f)),
            coin = Text(canvasObject.transform, "Coin Text", "抽奖币: 0", new Vector2(20f, -200f)),
            skin = Text(canvasObject.transform, "Skin Text", "枪械皮肤: 黑铁", new Vector2(20f, -226f)),
            message = Text(canvasObject.transform, "Message Text", "", new Vector2(0f, -36f), TextAnchor.UpperCenter),
            prompt = Text(canvasObject.transform, "Prompt Text", "", new Vector2(0f, 78f), TextAnchor.LowerCenter),
            crosshair = Text(canvasObject.transform, "Crosshair Text", ".", Vector2.zero, TextAnchor.MiddleCenter),
            hitMarker = Text(canvasObject.transform, "Hit Marker Text", "", new Vector2(0f, 30f), TextAnchor.MiddleCenter)
        };
        var manualHint = Text(
            canvasObject.transform,
            "Manual Play Hint Text",
            "可玩场景: TacticalPrototype | Enter/点击开始 | V 第一/第三人称 | 左键射击 | R 换弹(先开几枪) | 1-4 切枪 | F 拾取",
            new Vector2(0f, 44f),
            TextAnchor.LowerCenter);
        manualHint.fontSize = 17;
        manualHint.color = new Color(1f, 0.92f, 0.45f, 0.95f);
        manualHint.rectTransform.sizeDelta = new Vector2(1680f, 32f);
        refs.crosshair.fontSize = 28;
        refs.crosshair.color = new Color(1f, 1f, 1f, 0.88f);
        refs.crosshair.rectTransform.sizeDelta = new Vector2(70f, 70f);
        refs.hitMarker.fontSize = 38;
        refs.hitMarker.color = new Color(1f, 0.30f, 0.34f, 0.95f);
        refs.hitMarker.rectTransform.sizeDelta = new Vector2(90f, 90f);
        refs.lobbyPanel = CreateOverlayPanel(canvasObject.transform, "Lobby Panel", "房区生存", "按 Enter 或点击开始\nWASD 移动，鼠标瞄准，左键开火，右键 ADS，V 切视角，C/Z 蹲趴。", "开始");
        refs.deathPanel = CreateOverlayPanel(canvasObject.transform, "Death Panel", "已被击倒", "按 Enter 重新进入房区生存。", "重新开始");
        refs.deathPanel.SetActive(false);
        refs.helpPanel = CreateSidePanel(canvasObject.transform, "Help Panel", "键位提示", "WASD 移动\n鼠标 视角\n左键 开火 / 右键 ADS\nV 第一/第三人称\nShift 疾跑\nR 换弹 / 1-4 切枪\nC/Z 蹲下/趴下\nF 拾取 / 5/6/7 治疗\nO 设置 / L 皮肤 / Tab 帮助", new Vector2(-20f, -20f), new Vector2(360f, 318f));
        refs.settingsPanel = CreateSidePanel(canvasObject.transform, "Settings Panel", "游戏设置", "NPC 强度、刷新、物资丰富度会即时影响本局节奏。", new Vector2(-20f, -360f), new Vector2(430f, 254f));
        CreateSliderRow(refs.settingsPanel.transform, "NPC 强度", 0.30f, 1.60f, 0.85f, -88f, out refs.npcStrengthSlider, out refs.npcStrengthValue);
        CreateSliderRow(refs.settingsPanel.transform, "NPC 刷新", 0.30f, 1.40f, 0.45f, -132f, out refs.spawnRateSlider, out refs.spawnRateValue);
        CreateSliderRow(refs.settingsPanel.transform, "物资丰富度", 0.70f, 1.60f, 1.10f, -176f, out refs.lootRichnessSlider, out refs.lootRichnessValue);
        refs.settingsPanel.SetActive(false);

        refs.skinsPanel = CreateSidePanel(canvasObject.transform, "Skins Panel", "枪械皮肤抽奖", "每淘汰 10 个 NPC 获得 1 个抽奖币。抽到重复皮肤会获得碎片，5 个碎片兑换 1 个抽奖币。", new Vector2(-20f, 20f), new Vector2(430f, 256f), TextAnchor.LowerRight);
        refs.skinCoin = Text(refs.skinsPanel.transform, "Skin Coin Text", "抽奖币: 0", new Vector2(22f, -104f));
        refs.shards = Text(refs.skinsPanel.transform, "Shard Text", "碎片: 0", new Vector2(22f, -132f));
        refs.rollSkinButton = CreatePanelButton(refs.skinsPanel.transform, "Roll Skin", "消耗 1 币抽一次", new Vector2(-118f, 42f), new Vector2(182f, 42f));
        refs.closeSkinButton = CreatePanelButton(refs.skinsPanel.transform, "Close Skin", "关闭", new Vector2(-22f, 42f), new Vector2(82f, 42f));
        refs.skinsPanel.SetActive(false);
        return refs;
    }

    private static GameObject CreateSidePanel(Transform parent, string name, string title, string body, Vector2 anchored, Vector2 size, TextAnchor panelAnchor = TextAnchor.UpperRight)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        panel.AddComponent<Image>().color = new Color(0.02f, 0.04f, 0.08f, 0.78f);
        var rect = panel.GetComponent<RectTransform>();
        var anchor = AnchorPoint(panelAnchor);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = anchored;
        rect.sizeDelta = size;

        var titleText = Text(panel.transform, name + " Title", title, new Vector2(20f, -18f));
        titleText.fontSize = 20;
        titleText.rectTransform.sizeDelta = new Vector2(size.x - 40f, 28f);
        var bodyText = Text(panel.transform, name + " Body", body, new Vector2(20f, -52f));
        bodyText.fontSize = 15;
        bodyText.rectTransform.sizeDelta = new Vector2(size.x - 40f, size.y - 72f);
        return panel;
    }

    private static void CreateSliderRow(Transform parent, string label, float min, float max, float value, float y, out Slider slider, out Text valueText)
    {
        var labelText = Text(parent, label + " Label", label, new Vector2(22f, y));
        labelText.rectTransform.sizeDelta = new Vector2(110f, 24f);

        var sliderObject = new GameObject(label + " Slider");
        sliderObject.transform.SetParent(parent, false);
        var sliderRect = sliderObject.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 1f);
        sliderRect.anchorMax = new Vector2(0f, 1f);
        sliderRect.pivot = new Vector2(0f, 1f);
        sliderRect.anchoredPosition = new Vector2(138f, y - 4f);
        sliderRect.sizeDelta = new Vector2(188f, 18f);

        var background = new GameObject("Background");
        background.transform.SetParent(sliderObject.transform, false);
        var backgroundRect = background.AddComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        background.AddComponent<Image>().color = new Color(0.14f, 0.17f, 0.22f, 0.95f);

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObject.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(4f, 0f);
        fillAreaRect.offsetMax = new Vector2(-4f, 0f);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fill.AddComponent<Image>().color = new Color(0.90f, 0.68f, 0.18f, 0.95f);

        slider = sliderObject.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
        slider.fillRect = fillRect;

        valueText = Text(parent, label + " Value", value.ToString("0.00"), new Vector2(342f, y));
        valueText.rectTransform.sizeDelta = new Vector2(70f, 24f);
    }

    private static Button CreatePanelButton(Transform parent, string name, string label, Vector2 anchored, Vector2 size)
    {
        var button = new GameObject(name + " Button");
        button.transform.SetParent(parent, false);
        var rect = button.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = anchored;
        rect.sizeDelta = size;
        button.AddComponent<Image>().color = new Color(0.13f, 0.38f, 0.85f, 0.94f);
        var component = button.AddComponent<Button>();
        var text = Text(button.transform, name + " Button Label", label, Vector2.zero, TextAnchor.MiddleCenter);
        text.fontSize = 16;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        return component;
    }

    private static GameObject CreateOverlayPanel(Transform parent, string name, string title, string body, string buttonLabel)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var image = panel.AddComponent<Image>();
        image.color = new Color(0.02f, 0.04f, 0.08f, 0.86f);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var titleText = Text(panel.transform, name + " Title", title, new Vector2(0f, -265f), TextAnchor.UpperCenter);
        titleText.fontSize = 34;
        titleText.rectTransform.sizeDelta = new Vector2(900f, 54f);

        var bodyText = Text(panel.transform, name + " Body", body, new Vector2(0f, -332f), TextAnchor.UpperCenter);
        bodyText.fontSize = 18;
        bodyText.rectTransform.sizeDelta = new Vector2(900f, 110f);

        var button = new GameObject(name + " Button");
        button.transform.SetParent(panel.transform, false);
        var buttonRect = button.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -74f);
        buttonRect.sizeDelta = new Vector2(240f, 56f);
        button.AddComponent<Image>().color = new Color(0.13f, 0.38f, 0.85f, 0.94f);
        button.AddComponent<Button>();
        var label = Text(button.transform, name + " Button Label", buttonLabel, Vector2.zero, TextAnchor.MiddleCenter);
        label.fontSize = 20;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;
        return panel;
    }

    private static Text Text(Transform parent, string name, string value, Vector2 anchored, TextAnchor anchor = TextAnchor.UpperLeft)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rect = obj.AddComponent<RectTransform>();
        var anchorPoint = AnchorPoint(anchor);
        rect.anchorMin = anchorPoint;
        rect.anchorMax = rect.anchorMin;
        rect.pivot = anchorPoint;
        rect.anchoredPosition = anchored;
        rect.sizeDelta = anchor == TextAnchor.UpperLeft ? new Vector2(520f, 24f) : new Vector2(1300f, 30f);
        var text = obj.AddComponent<Text>();
        text.text = value;
        text.font = GetBuiltinFont();
        text.fontSize = anchor == TextAnchor.UpperLeft ? 16 : 18;
        text.alignment = anchor;
        text.color = Color.white;
        return text;
    }

    private static Vector2 AnchorPoint(TextAnchor anchor)
    {
        return anchor switch
        {
            TextAnchor.UpperLeft => new Vector2(0f, 1f),
            TextAnchor.UpperRight => new Vector2(1f, 1f),
            TextAnchor.UpperCenter => new Vector2(0.5f, 1f),
            TextAnchor.LowerCenter => new Vector2(0.5f, 0f),
            TextAnchor.LowerRight => new Vector2(1f, 0f),
            TextAnchor.MiddleCenter => new Vector2(0.5f, 0.5f),
            _ => new Vector2(0.5f, 0.5f)
        };
    }

    private static Font GetBuiltinFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private static string LootGlbPath(TacticalLootKind kind, string weapon)
    {
        return kind switch
        {
            TacticalLootKind.Ammo => RealifiedAmmoLootGlbPath,
            TacticalLootKind.Bandage => "models/loot_bandage_final.glb",
            TacticalLootKind.FirstAid => "models/loot_firstaid_final.glb",
            TacticalLootKind.Medkit => RealifiedMedkitLootGlbPath,
            TacticalLootKind.Revive => "models/loot_revive_final.glb",
            TacticalLootKind.Vest => RealifiedVestLootGlbPath,
            TacticalLootKind.Helmet => RealifiedHelmetLootGlbPath,
            TacticalLootKind.Weapon when weapon == "pistol" => HtmlPistolGlbPath,
            TacticalLootKind.Weapon when weapon == "shotgun" => HtmlShotgunGlbPath,
            TacticalLootKind.Weapon when weapon == "rifle" => HtmlRifleGlbPath,
            TacticalLootKind.Weapon when weapon == "dmr" => HtmlDmrGlbPath,
            _ => ApprovedAmmoLootGlbPath
        };
    }

    private static bool IsApprovedLootGlb(string relativePath)
    {
        return relativePath == ApprovedMedicalLootGlbPath
            || relativePath == ApprovedAmmoLootGlbPath
            || relativePath == ApprovedHelmetLootGlbPath
            || relativePath == ApprovedVestLootGlbPath;
    }

    private static bool IsPromotedRealifiedLootGlb(string relativePath)
    {
        return relativePath == RealifiedAmmoLootGlbPath
            || relativePath == RealifiedMedkitLootGlbPath
            || relativePath == RealifiedHelmetLootGlbPath
            || relativePath == RealifiedVestLootGlbPath;
    }

    private static Material LootPbrMaterial(Dictionary<string, Material> materials, TacticalLootKind kind, string weapon, string lootGlbPath)
    {
        if (lootGlbPath == RealifiedAmmoLootGlbPath)
        {
            return materials["realifiedAmmoLootPbr"];
        }
        if (lootGlbPath == RealifiedMedkitLootGlbPath)
        {
            return materials["realifiedMedkitLootPbr"];
        }
        if (lootGlbPath == RealifiedHelmetLootGlbPath)
        {
            return materials["realifiedHelmetPbr"];
        }
        if (lootGlbPath == RealifiedVestLootGlbPath)
        {
            return materials["realifiedVestPbr"];
        }

        return kind == TacticalLootKind.Weapon ? WeaponPbrMaterial(materials, weapon) : null;
    }

    private static float LootGlbScale(TacticalLootKind kind, bool approvedLoot)
    {
        if (!approvedLoot)
        {
            return 0.9f;
        }

        return kind switch
        {
            TacticalLootKind.Helmet => 0.86f,
            TacticalLootKind.Vest => 0.74f,
            TacticalLootKind.Ammo => 0.82f,
            _ => 0.72f
        };
    }

    private static string WeaponGlbPath(string weapon)
    {
        return weapon switch
        {
            "pistol" => RealifiedSidearmGlbPath,
            "shotgun" => HtmlShotgunGlbPath,
            "rifle" => RealifiedHeroRifleGlbPath,
            "dmr" => RealifiedSecondaryWeaponGlbPath,
            _ => RealifiedSidearmGlbPath
        };
    }

    private static GameObject AttachHtmlGlb(GameObject target, string relativePath, Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale, bool hidePlaceholder, Material overrideMaterial = null)
    {
        var assetPath = "Assets/HtmlTacticalAssets/" + relativePath;
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (source != null)
        {
            var instance = PrefabUtility.InstantiatePrefab(source, target.scene) as GameObject;
            if (instance != null)
            {
                instance.name = "HTML GLB Instance - " + Path.GetFileNameWithoutExtension(relativePath);
                instance.transform.SetParent(target.transform, false);
                instance.transform.localPosition = localPosition;
                instance.transform.localEulerAngles = localEulerAngles;
                instance.transform.localScale = localScale;

                foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
                {
                    if (overrideMaterial != null)
                    {
                        renderer.sharedMaterial = overrideMaterial;
                    }
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    renderer.receiveShadows = true;
                }

                if (hidePlaceholder)
                {
                    foreach (var renderer in target.GetComponentsInChildren<Renderer>(true))
                    {
                        if (!renderer.transform.IsChildOf(instance.transform))
                        {
                            renderer.enabled = false;
                        }
                    }
                }

                return instance;
            }
        }

        var mount = new GameObject("HTML GLB Mount - " + Path.GetFileNameWithoutExtension(relativePath));
        mount.transform.SetParent(target.transform, false);
        mount.transform.localScale = new Vector3(
            Mathf.Approximately(target.transform.localScale.x, 0f) ? 1f : 1f / target.transform.localScale.x,
            Mathf.Approximately(target.transform.localScale.y, 0f) ? 1f : 1f / target.transform.localScale.y,
            Mathf.Approximately(target.transform.localScale.z, 0f) ? 1f : 1f / target.transform.localScale.z);
        var loader = mount.AddComponent<HtmlGlbAssetMount>();
        loader.Configure(relativePath, localPosition, localEulerAngles, localScale, hidePlaceholder);
        return mount;
    }

    private static GameObject Box(string name, Vector3 position, Vector3 scale, Material material)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        return obj;
    }

    private static void DisableCollider(GameObject obj)
    {
        var collider = obj == null ? null : obj.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    private static GameObject Anchor(string name, Vector3 position)
    {
        var obj = new GameObject(name);
        obj.transform.position = position;
        return obj;
    }

    private static GameObject CreateWorldLabel(string text, Vector3 position)
    {
        var label = new GameObject("Label - " + text);
        label.transform.position = position;
        var mesh = label.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.characterSize = 0.7f;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.color = Color.white;
        mesh.font = GetBuiltinFont();
        var renderer = label.GetComponent<MeshRenderer>();
        if (renderer != null && mesh.font != null)
        {
            renderer.sharedMaterial = mesh.font.material;
        }
        return label;
    }

    private static void SetObjectField(Object target, string fieldName, Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(fieldName).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private struct HudRefs
    {
        public Text hp;
        public Text stamina;
        public Text weapon;
        public Text ammo;
        public Text armor;
        public Text inventory;
        public Text npc;
        public Text coin;
        public Text skin;
        public Text message;
        public Text prompt;
        public Text crosshair;
        public Text hitMarker;
        public Text skinCoin;
        public Text shards;
        public Text npcStrengthValue;
        public Text spawnRateValue;
        public Text lootRichnessValue;
        public GameObject lobbyPanel;
        public GameObject deathPanel;
        public GameObject helpPanel;
        public GameObject settingsPanel;
        public GameObject skinsPanel;
        public Slider npcStrengthSlider;
        public Slider spawnRateSlider;
        public Slider lootRichnessSlider;
        public Button rollSkinButton;
        public Button closeSkinButton;
    }
}
#endif
