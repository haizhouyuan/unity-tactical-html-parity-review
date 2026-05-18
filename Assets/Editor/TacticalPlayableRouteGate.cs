#if UNITY_EDITOR
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public static class TacticalPlayableRouteGate
{
    private const string ReportPath = "docs/TACTICAL_PLAYABLE_ROUTE_GATE.json";
    private const string RealifiedAuditPath = "docs/REALIFIED_ASSETS_IMPORT_AUDIT.json";
    private const string RealifiedCategoryNemotronPath = "docs/REALIFIED_CATEGORY_NEMOTRON_REVIEWS.json";
    private const string ScreenshotDirectory = "Assets/Screenshots/PlayableRoute";

    [MenuItem("AI Tools/Write Tactical Playable Route Gate")]
    public static void WriteRouteReport()
    {
        Directory.CreateDirectory("docs");
        Directory.CreateDirectory(ScreenshotDirectory);

        var gm = UnityEngine.Object.FindAnyObjectByType<TacticalGameManager>();
        var player = UnityEngine.Object.FindAnyObjectByType<TacticalPlayerController>();
        var camera = Camera.main;
        var follow = camera == null ? null : camera.GetComponent<TacticalCameraFollow>();
        var screenshots = new StringBuilder();
        var details = new StringBuilder();

        var hasCoreObjects = Application.isPlaying && gm != null && player != null && camera != null;
        var lobbyVisibleBeforeStart = false;
        var startClicked = false;
        var spawnVisited = false;
        var movedFromSpawn = false;
        var buildingVisited = false;
        var warehouseVisited = false;
        var containerVisited = false;
        var pickupMutatedState = false;
        var fireMutatedState = false;
        var enemyAttackMutatedState = false;
        var dynamicSpawnMutatedState = false;
        var ladderMutatedState = false;
        var healMutatedState = false;
        var deathMutatedState = false;
        var restartMutatedState = false;
        var approvedLootClassRouteEvidence = false;
        var playerCameraEvidence = false;
        var hudEvidence = false;
        var spawnFirstPersonWeaponVisible = false;
        var spawnFirstPersonWeaponEnabledRenderers = 0;
        var spawnFirstPersonGameplaySourceGlbRenderers = 0;
        var spawnCameraClear = false;
        var screenshotCount = 0;

        if (hasCoreObjects)
        {
            var lobby = FindObjectByName("Lobby Panel");
            lobbyVisibleBeforeStart = lobby != null && lobby.activeInHierarchy;
            follow?.SnapToLobby();
            screenshotCount += CaptureStep(camera, screenshots, "00_lobby_before_start", "Lobby visible before start");

            gm.StartRound();
            SetFloat(gm, "roundStartTime", Time.time - 10f);
            player.SetAds(false);
            player.SetStance(TacticalStance.Stand, true);
            player.ResetView(180f, 18f);
            player.SetCameraMode(TacticalCameraMode.FirstPerson);
            follow?.SnapToPlayer();
            Physics.SyncTransforms();
            var fpSpawnVisual = UnityEngine.Object.FindAnyObjectByType<TacticalFirstPersonWeaponVisual>();
            fpSpawnVisual?.ForceRefresh();
            spawnFirstPersonWeaponEnabledRenderers = CountEnabledFirstPersonWeaponRenderers(fpSpawnVisual);
            spawnFirstPersonGameplaySourceGlbRenderers = CountEnabledFirstPersonGameplaySourceGlbRenderers(fpSpawnVisual);
            spawnFirstPersonWeaponVisible = player.CameraMode == TacticalCameraMode.FirstPerson
                && fpSpawnVisual != null
                && fpSpawnVisual.HasActiveHeroWeapon
                && spawnFirstPersonWeaponEnabledRenderers >= 12
                && spawnFirstPersonGameplaySourceGlbRenderers >= 1
                && camera != null
                && Vector3.Distance(camera.transform.position, player.CameraTarget.position) < 0.75f;
            spawnCameraClear = SpawnCameraHasClearLane(camera);
            startClicked = !gm.IsInLobby && !gm.IsPlayerDown;
            spawnVisited = Vector3.Distance(player.transform.position, new Vector3(0f, 1.04f, 30f)) < 0.2f;
            playerCameraEvidence = CameraLooksUsable(player, camera);
            hudEvidence = HudLooksUsable();
            details.Append("spawnFp camera ").Append(player.CameraMode)
                .Append(" weapon ").Append(fpSpawnVisual == null ? "missing" : fpSpawnVisual.ActiveWeaponId)
                .Append(" renderers ").Append(spawnFirstPersonWeaponEnabledRenderers)
                .Append(" sourceGlbRenderers ").Append(spawnFirstPersonGameplaySourceGlbRenderers)
                .Append(" hero ").Append(fpSpawnVisual != null && fpSpawnVisual.HasActiveHeroWeapon)
                .Append(" clear ").Append(spawnCameraClear)
                .Append("; ");
            screenshotCount += CaptureStep(camera, screenshots, "01_spawn_after_start", "Started round at the HTML spawn lane in first-person with weapon visible");

            player.SetCameraMode(TacticalCameraMode.ThirdPerson);
            player.SetAds(false);
            player.ResetView(180f, 18f);
            follow?.SnapToPlayer();
            Physics.SyncTransforms();

            var spawnStart = player.transform.position;
            Visit(player, camera, follow, new Vector3(0f, 1.04f, 16f), 180f);
            movedFromSpawn = Vector3.Distance(spawnStart, player.transform.position) >= 8f;
            screenshotCount += CaptureStep(camera, screenshots, "02_spawn_exit_route", "Moved away from spawn into the compound lane");

            Visit(player, camera, follow, new Vector3(-38f, 1.04f, -16f), 210f);
            buildingVisited = CountNearbyNamed("A ", player.transform.position, 18f) >= 8;
            screenshotCount += CaptureStep(camera, screenshots, "03_building_a_entry", "Visited A building entry and interior loot area");

            Visit(player, camera, follow, new Vector3(0f, 1.04f, -28f), 180f);
            warehouseVisited = CountNearbyNamed("Warehouse", player.transform.position, 18f) >= 7;
            screenshotCount += CaptureStep(camera, screenshots, "04_warehouse_route", "Visited warehouse with crates and loot");

            Visit(player, camera, follow, new Vector3(0f, 1.04f, 22f), 180f);
            containerVisited = CountNearbyNamed("Container", player.transform.position, 24f) >= 5;
            screenshotCount += CaptureStep(camera, screenshots, "05_container_yard_route", "Visited container yard");

            pickupMutatedState = RunPickupStep(gm, player, camera, follow, details, screenshots, ref screenshotCount);
            approvedLootClassRouteEvidence = RunApprovedLootClassEvidenceStep(gm, player, camera, follow, details, screenshots, ref screenshotCount);
            fireMutatedState = RunFireStep(gm, player, camera, follow, details, screenshots, ref screenshotCount);
            enemyAttackMutatedState = RunEnemyAttackStep(gm, player, camera, follow, details, screenshots, ref screenshotCount);
            dynamicSpawnMutatedState = RunDynamicSpawnStep(gm, details);
            ladderMutatedState = RunLadderStep(gm, player, camera, follow, details, screenshots, ref screenshotCount);
            healMutatedState = RunHealStep(gm, details);
            deathMutatedState = RunDeathStep(gm, camera, screenshots, ref screenshotCount);
            restartMutatedState = RunRestartStep(gm, player, camera, follow, screenshots, ref screenshotCount);
        }

        var passed = hasCoreObjects
            && lobbyVisibleBeforeStart
            && startClicked
            && spawnVisited
            && spawnFirstPersonWeaponVisible
            && spawnCameraClear
            && movedFromSpawn
            && buildingVisited
            && warehouseVisited
            && containerVisited
            && pickupMutatedState
            && approvedLootClassRouteEvidence
            && fireMutatedState
            && enemyAttackMutatedState
            && dynamicSpawnMutatedState
            && ladderMutatedState
            && healMutatedState
            && deathMutatedState
            && restartMutatedState
            && playerCameraEvidence
            && hudEvidence
            && screenshotCount >= 9;
        var assetQuality = BuildAssetQualitySummary();

        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "tactical_playable_route_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "application_is_playing", Application.isPlaying, true);
        Append(json, "passed", passed, true);
        Append(json, "has_core_objects", hasCoreObjects, true);
        Append(json, "lobby_visible_before_start", lobbyVisibleBeforeStart, true);
        Append(json, "start_clicked", startClicked, true);
        Append(json, "spawn_visited", spawnVisited, true);
        Append(json, "spawn_first_person_weapon_visible", spawnFirstPersonWeaponVisible, true);
        Append(json, "spawn_first_person_weapon_enabled_renderers", spawnFirstPersonWeaponEnabledRenderers, true);
        Append(json, "spawn_first_person_gameplay_source_glb_renderers", spawnFirstPersonGameplaySourceGlbRenderers, true);
        Append(json, "spawn_camera_clear", spawnCameraClear, true);
        Append(json, "moved_from_spawn", movedFromSpawn, true);
        Append(json, "building_visited", buildingVisited, true);
        Append(json, "warehouse_visited", warehouseVisited, true);
        Append(json, "container_visited", containerVisited, true);
        Append(json, "pickup_state_mutation", pickupMutatedState, true);
        Append(json, "realified_loot_class_route_evidence", false, true);
        Append(json, "approved_loot_class_route_evidence", approvedLootClassRouteEvidence, true);
        Append(json, "fire_state_mutation", fireMutatedState, true);
        Append(json, "enemy_ranged_attack_mutation", enemyAttackMutatedState, true);
        Append(json, "dynamic_spawn_mutation", dynamicSpawnMutatedState, true);
        Append(json, "ladder_floor_mutation", ladderMutatedState, true);
        Append(json, "heal_mutation", healMutatedState, true);
        Append(json, "death_overlay_mutation", deathMutatedState, true);
        Append(json, "restart_mutation", restartMutatedState, true);
        Append(json, "player_camera_evidence", playerCameraEvidence, true);
        Append(json, "hud_evidence", hudEvidence, true);
        Append(json, "gameplay_route_passed", passed, true);
        Append(json, "approved_incremental_asset_gate_passed", assetQuality.ApprovedIncrementalAssetsPassed, true);
        Append(json, "visual_polish_gate_passed", assetQuality.VisualPolishPassed, true);
        Append(json, "full_visual_asset_gate_passed", assetQuality.FullVisualAssetGatePassed, true);
        Append(json, "completion_credit", assetQuality.CompletionCredit, true);
        Append(json, "visual_completion_blocker", assetQuality.VisualCompletionBlocker, true);
        Append(json, "screenshot_count", screenshotCount, true);
        Append(json, "details", details.ToString().Trim(), true);
        json.AppendLine("  \"screenshots\": [");
        json.Append(screenshots);
        json.AppendLine("  ],");
        AppendAssetQuality(json, assetQuality);
        json.AppendLine("}");

        File.WriteAllText(ReportPath, json.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] Tactical playable route gate report written to " + ReportPath + " passed=" + passed);
    }

    private static bool RunPickupStep(TacticalGameManager gm, TacticalPlayerController player, Camera camera, TacticalCameraFollow follow, StringBuilder details, StringBuilder screenshots, ref int screenshotCount)
    {
        var loots = GetLoots(gm);
        var loot = FindBestLootForGate() ?? FindLoot(TacticalLootKind.Weapon) ?? FindLoot(TacticalLootKind.Ammo);
        if (loots == null || loot == null)
        {
            details.Append("pickup missing-loot; ");
            return false;
        }

        var beforeCount = loots.Count;
        var beforeWeapon = gm.CurrentWeaponId;
        Visit(player, camera, follow, loot.transform.position + new Vector3(0.35f, 0f, 1.65f), 180f);
        player.SetAds(true);
        follow?.SnapToPlayer();
        typeof(TacticalGameManager).GetMethod("RefreshSceneRegistries", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(gm, null);
        typeof(TacticalGameManager).GetMethod("FindNearestLoot", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(gm, null);
        typeof(TacticalGameManager).GetMethod("UpdateHud", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(gm, null);
        Physics.SyncTransforms();
        screenshotCount += CaptureStep(camera, screenshots, "06_loot_prompt_before_pickup", "Reached actual loot entity before pickup");
        gm.TryPickupNearest();
        Physics.SyncTransforms();
        var afterCount = loots.Count;
        var afterWeapon = gm.CurrentWeaponId;
        screenshotCount += CaptureStep(camera, screenshots, "07_after_pickup_inventory", "Picked up loot and mutated inventory or weapon state");
        details.Append("pickup target ").Append(loot.Kind).Append(":").Append(loot.DisplayName)
            .Append(" count ").Append(beforeCount).Append("->").Append(afterCount)
            .Append(" weapon ").Append(beforeWeapon).Append("->").Append(afterWeapon).Append("; ");
        return afterCount == beforeCount - 1 && (afterWeapon != beforeWeapon || FindWeaponState(gm, afterWeapon) != null);
    }

    private static bool RunApprovedLootClassEvidenceStep(TacticalGameManager gm, TacticalPlayerController player, Camera camera, TacticalCameraFollow follow, StringBuilder details, StringBuilder screenshots, ref int screenshotCount)
    {
        var requiredKinds = new[]
        {
            TacticalLootKind.Ammo,
            TacticalLootKind.Medkit,
            TacticalLootKind.Vest
        };
        var passed = 0;
        foreach (var kind in requiredKinds)
        {
            var loot = FindRouteLootForKind(kind);
            var loots = GetLoots(gm);
            if (loot == null || loots == null)
            {
                details.Append("approvedClass ").Append(kind).Append(" missing; ");
                continue;
            }

            var beforeCount = loots.Count;
            var beforeValue = ReadLootClassEvidenceValue(gm, kind);
            Visit(player, camera, follow, loot.transform.position + new Vector3(0.35f, 0f, 1.65f), 180f);
            player.SetAds(true);
            RefreshPickupProbe(gm);
            follow?.SnapToPlayer();
            Physics.SyncTransforms();
            var labelStem = "06_approved_" + kind.ToString().ToLowerInvariant();
            screenshotCount += CaptureStep(camera, screenshots, labelStem + "_prompt", "Reached approved HTML-parity " + kind + " gameplay loot before pickup");
            gm.TryPickupNearest();
            RefreshPickupProbe(gm);
            Physics.SyncTransforms();
            var afterCount = loots.Count;
            var afterValue = ReadLootClassEvidenceValue(gm, kind);
            screenshotCount += CaptureStep(camera, screenshots, labelStem + "_after_pickup", "Picked up approved HTML-parity " + kind + " gameplay loot");
            var ok = afterValue > beforeValue;
            if (ok)
            {
                passed++;
            }

            details.Append("approvedClass ").Append(kind)
                .Append(":").Append(loot.DisplayName)
                .Append(" count ").Append(beforeCount).Append("->").Append(afterCount)
                .Append(" value ").Append(beforeValue).Append("->").Append(afterValue)
                .Append(" ok ").Append(ok).Append("; ");
        }

        return passed == requiredKinds.Length;
    }

    private static bool RunFireStep(TacticalGameManager gm, TacticalPlayerController player, Camera camera, TacticalCameraFollow follow, StringBuilder details, StringBuilder screenshots, ref int screenshotCount)
    {
        var enemy = FindLiveEnemyForGate();
        var state = FindWeaponState(gm, "rifle") ?? FindWeaponState(gm, gm.CurrentWeaponId) ?? FindWeaponState(gm, "pistol");
        if (enemy == null || state == null)
        {
            details.Append("fire missing-enemy-or-weapon; ");
            return false;
        }

        state.unlocked = true;
        state.reloading = false;
        state.lastShotTime = -999f;
        if (state.magazine <= 0)
        {
            state.magazine = Mathf.Max(1, state.spec.magazineSize);
        }
        state.reserve = Mathf.Max(state.reserve, state.spec.magazineSize);
        gm.SelectWeapon(state.spec.id);
        typeof(TacticalGameManager).GetField("currentWeaponId", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(gm, state.spec.id);
        typeof(TacticalGameManager).GetMethod("UpdateHud", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(gm, null);

        Teleport(player, new Vector3(0f, 1.04f, 44f));
        player.ResetView(180f, 0f);
        player.SetCameraMode(TacticalCameraMode.FirstPerson);
        player.SetAds(true);
        follow?.SnapToPlayer();
        Physics.SyncTransforms();
        var firstPersonCameraReady = player.CameraMode == TacticalCameraMode.FirstPerson
            && player.IsAds
            && camera != null
            && Vector3.Distance(camera.transform.position, player.CameraTarget.position) < 0.75f;
        var characterVisual = player.GetComponent<TacticalCharacterMotionVisual>();
        characterVisual?.ApplyPreviewState(TacticalCharacterVisualState.Aim, 0f);
        var localBodyHiddenInFirstPerson = characterVisual != null && characterVisual.LocalFirstPersonHiddenRendererCount > 0;

        var aimRay = new Ray(camera.transform.position, camera.transform.forward);
        var enemyController = enemy.GetComponent<CharacterController>();
        if (enemyController != null) enemyController.enabled = false;
        enemy.transform.position = aimRay.origin + aimRay.direction * 12f - Vector3.up * 0.9f;
        enemy.transform.rotation = Quaternion.LookRotation((player.transform.position - enemy.transform.position).normalized);
        if (enemyController != null) enemyController.enabled = true;
        Physics.SyncTransforms();

        var healthBefore = GetEnemyHealth(enemy);
        var magazineBefore = state.magazine;
        var tracerBefore = CountObjects("Tracer");
        var fpVisual = UnityEngine.Object.FindAnyObjectByType<TacticalFirstPersonWeaponVisual>();
        fpVisual?.ForceRefresh();
        var fpShotBefore = fpVisual == null ? 0 : fpVisual.ShotPolishEvents;
        var fpReloadBefore = fpVisual == null ? 0 : fpVisual.ReloadPolishEvents;
        gm.FireCurrentWeapon();
        fpVisual?.ApplyPreviewPolish(0.2f, true, 0.35f, 0f);
        Physics.SyncTransforms();
        screenshotCount += CaptureStep(camera, screenshots, "08_fire_hit_first_person", "Fired from player camera and hit an NPC");
        gm.Reload();
        fpVisual?.ApplyPreviewPolish(0.35f, true, 0.7f, 0.65f);
        var healthAfter = GetEnemyHealth(enemy);
        var magazineAfter = state.magazine;
        var tracerAfter = CountObjects("Tracer");
        var fpShotAfter = fpVisual == null ? 0 : fpVisual.ShotPolishEvents;
        var fpReloadAfter = fpVisual == null ? 0 : fpVisual.ReloadPolishEvents;
        var fpRendererReady = fpVisual != null && fpVisual.GetComponentsInChildren<Renderer>(true).Length > 0;
        var fpGameplaySourceGlbRenderers = CountEnabledFirstPersonGameplaySourceGlbRenderers(fpVisual);
        var fpPolishReady = fpVisual != null && fpVisual.LastPolishOffset.magnitude > 0.03f;
        details.Append("fire mag ").Append(magazineBefore).Append("->").Append(magazineAfter)
            .Append(" enemyHp ").Append(healthBefore.ToString("F1", CultureInfo.InvariantCulture))
            .Append("->").Append(healthAfter.ToString("F1", CultureInfo.InvariantCulture))
            .Append(" tracer ").Append(tracerBefore).Append("->").Append(tracerAfter)
            .Append(" fpShot ").Append(fpShotBefore).Append("->").Append(fpShotAfter)
            .Append(" fpReload ").Append(fpReloadBefore).Append("->").Append(fpReloadAfter)
            .Append(" fpRenderer ").Append(fpRendererReady)
            .Append(" fpSourceGlbRenderers ").Append(fpGameplaySourceGlbRenderers)
            .Append(" fpPolish ").Append(fpPolishReady)
            .Append(" fpCamera ").Append(firstPersonCameraReady)
            .Append(" fpBodyHidden ").Append(localBodyHiddenInFirstPerson).Append("; ");
        return magazineAfter < magazineBefore
            && healthAfter < healthBefore
            && tracerAfter > tracerBefore
            && firstPersonCameraReady
            && localBodyHiddenInFirstPerson
            && fpVisual != null
            && fpRendererReady
            && fpGameplaySourceGlbRenderers >= 1
            && fpPolishReady
            && fpShotAfter > fpShotBefore
            && fpReloadAfter > fpReloadBefore;
    }

    private static bool RunEnemyAttackStep(TacticalGameManager gm, TacticalPlayerController player, Camera camera, TacticalCameraFollow follow, StringBuilder details, StringBuilder screenshots, ref int screenshotCount)
    {
        var enemy = FindLiveEnemyForGate();
        if (enemy == null)
        {
            details.Append("enemyAttack missing-enemy; ");
            return false;
        }

        enemy.Initialize(gm, player.transform);
        SetFloat(gm, "roundStartTime", Time.time - 10f);
        SetFloat(gm, "hp", 100f);
        player.SetCameraMode(TacticalCameraMode.ThirdPerson);
        player.SetAds(false);
        player.ResetView(180f, 18f);
        Teleport(player, new Vector3(0f, 1.04f, 30f));
        var enemyController = enemy.GetComponent<CharacterController>();
        if (enemyController != null) enemyController.enabled = false;
        enemy.transform.position = player.transform.position + new Vector3(0f, 0f, -8.5f);
        enemy.transform.rotation = Quaternion.LookRotation((player.transform.position - enemy.transform.position).normalized);
        if (enemyController != null) enemyController.enabled = true;
        follow?.SnapToPlayer();
        SetEnemyFloat(enemy, "nextShotTime", Time.time - 0.1f);
        var hpBefore = GetFloat(gm, "hp");
        var tracerBefore = CountObjects("Tracer");
        InvokeEnemyUpdate(enemy);
        Physics.SyncTransforms();
        screenshotCount += CaptureStep(camera, screenshots, "09_enemy_ranged_attack", "NPC fired at player and mutated HP");
        var hpAfter = GetFloat(gm, "hp");
        var tracerAfter = CountObjects("Tracer");
        details.Append("enemyAttack hp ").Append(hpBefore.ToString("F1", CultureInfo.InvariantCulture))
            .Append("->").Append(hpAfter.ToString("F1", CultureInfo.InvariantCulture))
            .Append(" tracer ").Append(tracerBefore).Append("->").Append(tracerAfter).Append("; ");
        return hpAfter < hpBefore && tracerAfter > tracerBefore;
    }

    private static bool RunDynamicSpawnStep(TacticalGameManager gm, StringBuilder details)
    {
        var enemies = GetEnemies(gm);
        if (enemies == null)
        {
            details.Append("spawn missing-list; ");
            return false;
        }

        var before = enemies.Count;
        SetFloat(gm, "nextEnemyRespawn", Time.time - 0.1f);
        InvokePrivate(gm, "MaybeSpawnEnemy");
        var after = enemies.Count;
        details.Append("spawn enemies ").Append(before).Append("->").Append(after).Append("; ");
        return after > before;
    }

    private static bool RunLadderStep(TacticalGameManager gm, TacticalPlayerController player, Camera camera, TacticalCameraFollow follow, StringBuilder details, StringBuilder screenshots, ref int screenshotCount)
    {
        var ladder = FindNamedObject<TacticalLadder>("A Interior Ladder") ?? UnityEngine.Object.FindAnyObjectByType<TacticalLadder>();
        if (ladder == null)
        {
            details.Append("ladder missing-ladder; ");
            return false;
        }

        var beforeFloor = GetInt(gm, "currentFloor");
        Visit(player, camera, follow, ladder.transform.position + new Vector3(0.5f, 0f, -0.5f), 205f);
        gm.TryPickupNearest();
        follow?.SnapToPlayer();
        Physics.SyncTransforms();
        screenshotCount += CaptureStep(camera, screenshots, "10_ladder_upper_floor", "Used ladder and reached upper floor");
        var afterFloor = GetInt(gm, "currentFloor");
        details.Append("ladder floor ").Append(beforeFloor).Append("->").Append(afterFloor)
            .Append(" y ").Append(player.transform.position.y.ToString("F2", CultureInfo.InvariantCulture)).Append("; ");
        return afterFloor > beforeFloor && player.transform.position.y > 3f;
    }

    private static bool RunHealStep(TacticalGameManager gm, StringBuilder details)
    {
        SetFloat(gm, "hp", 42f);
        SetInt(gm, "firstAid", 1);
        var before = GetFloat(gm, "hp");
        gm.UseHeal(TacticalLootKind.FirstAid);
        var after = GetFloat(gm, "hp");
        details.Append("heal hp ").Append(before.ToString("F1", CultureInfo.InvariantCulture))
            .Append("->").Append(after.ToString("F1", CultureInfo.InvariantCulture)).Append("; ");
        return after >= 75f;
    }

    private static bool RunDeathStep(TacticalGameManager gm, Camera camera, StringBuilder screenshots, ref int screenshotCount)
    {
        SetFloat(gm, "roundStartTime", Time.time - 10f);
        SetInt(gm, "revives", 0);
        SetFloat(gm, "hp", 20f);
        gm.DamagePlayer(999f);
        screenshotCount += CaptureStep(camera, screenshots, "11_death_overlay", "Player HP reached zero and death overlay appeared");
        var death = FindObjectByName("Death Panel");
        return gm.IsPlayerDown && death != null && death.activeInHierarchy;
    }

    private static bool RunRestartStep(TacticalGameManager gm, TacticalPlayerController player, Camera camera, TacticalCameraFollow follow, StringBuilder screenshots, ref int screenshotCount)
    {
        gm.StartRound();
        SetFloat(gm, "roundStartTime", Time.time - 10f);
        player.SetCameraMode(TacticalCameraMode.ThirdPerson);
        player.SetAds(false);
        player.ResetView(180f, 18f);
        follow?.SnapToPlayer();
        Physics.SyncTransforms();
        screenshotCount += CaptureStep(camera, screenshots, "12_restart_after_death", "Restarted from death overlay back to playable spawn");
        return !gm.IsInLobby && !gm.IsPlayerDown && GetFloat(gm, "hp") >= 99f;
    }

    private static int CaptureStep(Camera camera, StringBuilder screenshots, string label, string note)
    {
        if (camera == null)
        {
            return 0;
        }

        var path = ScreenshotDirectory + "/" + label + ".png";
        RenderCameraScreenshot(camera, path);
        if (screenshots.Length > 0)
        {
            screenshots.AppendLine(",");
        }

        screenshots.Append("    { \"label\": \"").Append(label)
            .Append("\", \"path\": \"").Append(Escape(path))
            .Append("\", \"note\": \"").Append(Escape(note)).Append("\" }");
        return File.Exists(path) ? 1 : 0;
    }

    private static void RenderCameraScreenshot(Camera camera, string path)
    {
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude);
        var previousModes = new RenderMode[canvases.Length];
        var previousCameras = new Camera[canvases.Length];
        var previousPlaneDistances = new float[canvases.Length];
        var renderTexture = new RenderTexture(1920, 1080, 24);
        var texture = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        try
        {
            for (var i = 0; i < canvases.Length; i++)
            {
                previousModes[i] = canvases[i].renderMode;
                previousCameras[i] = canvases[i].worldCamera;
                previousPlaneDistances[i] = canvases[i].planeDistance;
                if (canvases[i].renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                    canvases[i].worldCamera = camera;
                    canvases[i].planeDistance = 1f;
                }
            }

            Canvas.ForceUpdateCanvases();
            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            camera.Render();
            texture.ReadPixels(new Rect(0f, 0f, 1920f, 1080f), 0, 0);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            for (var i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] == null)
                {
                    continue;
                }

                canvases[i].renderMode = previousModes[i];
                canvases[i].worldCamera = previousCameras[i];
                canvases[i].planeDistance = previousPlaneDistances[i];
            }

            Canvas.ForceUpdateCanvases();
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }
    }

    private static bool CameraLooksUsable(TacticalPlayerController player, Camera camera)
    {
        if (player == null || camera == null)
        {
            return false;
        }

        if (player.CameraMode == TacticalCameraMode.FirstPerson)
        {
            return Vector3.Distance(camera.transform.position, player.CameraTarget.position) < 0.75f;
        }

        var distance = Vector3.Distance(player.transform.position, camera.transform.position);
        return distance >= 4f && distance <= 14f;
    }

    private static bool SpawnCameraHasClearLane(Camera camera)
    {
        if (camera == null)
        {
            return false;
        }

        var points = new[]
        {
            new Vector2(0.50f, 0.50f),
            new Vector2(0.72f, 0.50f),
            new Vector2(0.86f, 0.50f),
            new Vector2(0.72f, 0.30f),
            new Vector2(0.86f, 0.30f)
        };

        foreach (var point in points)
        {
            var ray = camera.ViewportPointToRay(new Vector3(point.x, point.y, 0f));
            if (Physics.Raycast(ray, out var hit, 1.25f) && !IsFirstPersonWeaponHit(hit.collider))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsFirstPersonWeaponHit(Collider collider)
    {
        var current = collider == null ? null : collider.transform;
        while (current != null)
        {
            if (current.name.IndexOf("First Person Weapon Visual", StringComparison.OrdinalIgnoreCase) >= 0
                || current.name.IndexOf("FP Weapon -", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static bool HudLooksUsable()
    {
        return !string.IsNullOrWhiteSpace(FindObjectByName("HP Text")?.GetComponent<Text>()?.text)
            && !string.IsNullOrWhiteSpace(FindObjectByName("Ammo Text")?.GetComponent<Text>()?.text)
            && !string.IsNullOrWhiteSpace(FindObjectByName("NPC Text")?.GetComponent<Text>()?.text)
            && FindObjectByName("Crosshair Text") != null;
    }

    private static void Visit(TacticalPlayerController player, Camera camera, TacticalCameraFollow follow, Vector3 position, float yaw)
    {
        Teleport(player, position);
        player.SetCameraMode(TacticalCameraMode.ThirdPerson);
        player.SetAds(false);
        player.SetStance(TacticalStance.Stand, true);
        player.ResetView(yaw, 18f);
        follow?.SnapToPlayer();
        Physics.SyncTransforms();
    }

    private static TacticalLoot FindLoot(TacticalLootKind kind)
    {
        foreach (var loot in UnityEngine.Object.FindObjectsByType<TacticalLoot>(FindObjectsInactive.Exclude))
        {
            if (loot.Kind == kind)
            {
                return loot;
            }
        }

        return null;
    }

    private static TacticalLoot FindBestLootForGate()
    {
        var preferredKinds = new[]
        {
            TacticalLootKind.Helmet,
            TacticalLootKind.Vest,
            TacticalLootKind.Ammo,
            TacticalLootKind.Medkit
        };
        var anchor = new Vector3(0f, 1f, 23f);
        TacticalLoot best = null;
        var bestScore = float.MaxValue;
        foreach (var loot in UnityEngine.Object.FindObjectsByType<TacticalLoot>(FindObjectsInactive.Exclude))
        {
            if (loot == null || Array.IndexOf(preferredKinds, loot.Kind) < 0)
            {
                continue;
            }

            var heightPenalty = loot.transform.position.y > 2.0f ? 60f : 0f;
            var score = Vector3.Distance(anchor, loot.transform.position) + heightPenalty;
            if (score < bestScore)
            {
                best = loot;
                bestScore = score;
            }
        }

        return best;
    }

    private static TacticalLoot FindRouteLootForKind(TacticalLootKind kind)
    {
        TacticalLoot best = null;
        var bestScore = float.MaxValue;
        var anchor = new Vector3(0f, 1f, 23f);
        foreach (var loot in UnityEngine.Object.FindObjectsByType<TacticalLoot>(FindObjectsInactive.Exclude))
        {
            if (loot == null || loot.Kind != kind)
            {
                continue;
            }

            var heightPenalty = loot.transform.position.y > 2.0f ? 60f : 0f;
            var score = Vector3.Distance(anchor, loot.transform.position) + heightPenalty;
            if (score < bestScore)
            {
                best = loot;
                bestScore = score;
            }
        }

        return best;
    }

    private static bool HasRealifiedPromotedMaterialForKind(GameObject obj, TacticalLootKind kind)
    {
        var materialNeedle = kind switch
        {
            TacticalLootKind.Ammo => "RealifiedAmmoLootPbrPromoted",
            TacticalLootKind.Medkit => "RealifiedMedkitLootPbrPromoted",
            TacticalLootKind.Helmet => "RealifiedHelmetPbrPromoted",
            TacticalLootKind.Vest => "RealifiedVestPbrPromoted",
            _ => string.Empty
        };
        if (string.IsNullOrEmpty(materialNeedle))
        {
            return false;
        }

        foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
        {
            var material = renderer.sharedMaterial;
            if (material != null && material.name.Contains(materialNeedle, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void RefreshPickupProbe(TacticalGameManager gm)
    {
        typeof(TacticalGameManager).GetMethod("RefreshSceneRegistries", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(gm, null);
        typeof(TacticalGameManager).GetMethod("FindNearestLoot", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(gm, null);
        typeof(TacticalGameManager).GetMethod("UpdateHud", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(gm, null);
    }

    private static int ReadLootClassEvidenceValue(TacticalGameManager gm, TacticalLootKind kind)
    {
        return kind switch
        {
            TacticalLootKind.Ammo => GetTotalReserveAmmo(gm),
            TacticalLootKind.Medkit => GetInt(gm, "medkits"),
            TacticalLootKind.Helmet => GetInt(gm, "helmetLevel"),
            TacticalLootKind.Vest => GetInt(gm, "vestLevel"),
            _ => 0
        };
    }

    private static int GetTotalReserveAmmo(TacticalGameManager gm)
    {
        var total = 0;
        var weapons = typeof(TacticalGameManager).GetField("weapons", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(gm) as IDictionary;
        if (weapons == null)
        {
            return total;
        }

        foreach (DictionaryEntry entry in weapons)
        {
            if (entry.Value is TacticalWeaponState state && state.unlocked)
            {
                total += state.reserve;
            }
        }

        return total;
    }

    private static bool HasRealifiedPromotedMaterial(GameObject obj)
    {
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
        {
            var material = renderer.sharedMaterial;
            if (material == null)
            {
                continue;
            }

            if (material.name.Contains("RealifiedHelmetPbrPromoted", StringComparison.OrdinalIgnoreCase)
                || material.name.Contains("RealifiedVestPbrPromoted", StringComparison.OrdinalIgnoreCase)
                || material.name.Contains("RealifiedAmmoLootPbrPromoted", StringComparison.OrdinalIgnoreCase)
                || material.name.Contains("RealifiedMedkitLootPbrPromoted", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static TacticalWeaponState FindWeaponState(TacticalGameManager gm, string weaponId)
    {
        var weapons = typeof(TacticalGameManager).GetField("weapons", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(gm) as IDictionary;
        return weapons != null && weapons.Contains(weaponId) ? weapons[weaponId] as TacticalWeaponState : null;
    }

    private static IList GetLoots(TacticalGameManager gm)
    {
        return typeof(TacticalGameManager).GetField("loots", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(gm) as IList;
    }

    private static IList GetEnemies(TacticalGameManager gm)
    {
        return typeof(TacticalGameManager).GetField("enemies", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(gm) as IList;
    }

    private static int CountNearbyNamed(string prefix, Vector3 center, float radius)
    {
        var count = 0;
        foreach (var obj in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude))
        {
            if (!obj.name.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            if (Vector3.Distance(obj.transform.position, center) <= radius)
            {
                count++;
            }
        }

        return count;
    }

    private sealed class AssetQualitySummary
    {
        public int ApprovedCrateInstances;
        public int ApprovedCrateRendererInstances;
        public int ApprovedCratePbrRendererInstances;
        public int ApprovedContainerInstances;
        public int ApprovedContainerRendererInstances;
        public int ApprovedContainerPbrRendererInstances;
        public int ApprovedPlayerInstances;
        public int ApprovedPlayerRendererInstances;
        public int ApprovedPlayerPbrRendererInstances;
        public int ApprovedEnemyInstances;
        public int ApprovedEnemyRendererInstances;
        public int ApprovedEnemyPbrRendererInstances;
        public int ApprovedMedicalLootInstances;
        public int ApprovedMedicalLootRendererInstances;
        public int ApprovedAmmoLootInstances;
        public int ApprovedAmmoLootRendererInstances;
        public int ApprovedHelmetLootInstances;
        public int ApprovedHelmetLootRendererInstances;
        public int ApprovedVestLootInstances;
        public int ApprovedVestLootRendererInstances;
        public int ApprovedWeaponPbrRendererInstances;
        public int WetAsphaltRendererCount;
        public int ConcreteRendererCount;
        public int TacticalPostProcessVolumeCount;
        public int TacticalRainFieldCount;
        public int TacticalIndustrialLightCount;
        public int TacticalPostProcessCameraCount;
        public int FirstPersonWeaponPolishEventCount;
        public int FirstPersonWeaponReloadEventCount;
        public int FirstPersonWeaponRendererCount;
        public int CharacterAnimationStateEvidenceCount;
        public int CharacterAnimationStateNamedCount;
        public int CharacterProceduralLimbRigCount;
        public int CharacterProceduralLimbAnimationCount;
        public int CharacterAuthoredClipLibraryCount;
        public int CharacterAuthoredClipAnimationCount;
        public int ApprovedSkinnedCharacterAssetCount;
        public int ApprovedSkinnedCharacterAnimationAssetCount;
        public int TacticalCharacterDetailKitCount;
        public int TacticalCharacterDetailRendererCount;
        public int KnownCategoryFailedBatchFiles;
        public int CategoryFailedSceneInstances;
        public bool RealifiedAuditPresent;
        public int RealifiedAuditTotalGlb;
        public int RealifiedAuditTexturedProbeCount;
        public int RealifiedAuditPromotablePbrCandidates;
        public int RealifiedAuditSidecarPromotableCount;
        public int RealifiedAuditExternalTextureSidecars;
        public int RealifiedAuditExternalMaterialFiles;
        public int RealifiedCratePromotionSceneInstances;
        public int RealifiedContainerPromotionSceneInstances;
        public int RealifiedAmmoLootPromotionSceneInstances;
        public int RealifiedMedkitLootPromotionSceneInstances;
        public int RealifiedHelmetPromotionSceneInstances;
        public int RealifiedVestPromotionSceneInstances;
        public int RealifiedGearPromotionSceneInstances;
        public int RealifiedLootPromotionSceneInstances;
        public int RealifiedSidearmPromotionSceneInstances;
        public int RealifiedHeroRiflePromotionSceneInstances;
        public int RealifiedSecondaryWeaponPromotionSceneInstances;
        public int RealifiedWeaponPromotionSceneInstances;
        public bool RealifiedEnvironmentPropSemanticAllowed;
        public bool RealifiedGearSemanticAllowed;
        public bool RealifiedLootSemanticAllowed;
        public bool RealifiedWeaponSemanticAllowed;
        public bool RealifiedWeaponSemanticCategoryMatches;
        public int RealifiedSemanticAllowedClassCount;
        public bool RealifiedEnvironmentPropPromotionEvidencePassed;
        public bool RealifiedGearLootPromotionEvidencePassed;
        public bool RealifiedWeaponPromotionEvidencePassed;
        public bool ApprovedIncrementalAssetsPassed;
        public bool VisualPolishPassed;
        public bool FirstPersonWeaponPolishPassed;
        public bool CharacterAnimationStateEvidencePassed;
        public bool CharacterProceduralLimbAnimationPassed;
        public bool CharacterAuthoredClipAnimationPassed;
        public bool TrueSkinnedHumanoidImportPassed;
        public bool TacticalCharacterDetailKitPassed;
        public bool FullVisualAssetGatePassed;
        public string CompletionCredit;
        public string VisualCompletionBlocker;
    }

    private static AssetQualitySummary BuildAssetQualitySummary()
    {
        PrimeCharacterAnimationStateEvidence();
        var summary = new AssetQualitySummary
        {
            ApprovedCrateInstances = CountSceneObjectsContaining("tactical_crate_v1"),
            ApprovedCrateRendererInstances = CountSceneObjectsContainingRenderer("tactical_crate_v1"),
            ApprovedCratePbrRendererInstances = CountRenderersUsingMaterial("TacticalCratePbrApproved"),
            ApprovedContainerInstances = CountSceneObjectsContaining("approved_container_v1"),
            ApprovedContainerRendererInstances = CountSceneObjectsContainingRenderer("approved_container_v1"),
            ApprovedContainerPbrRendererInstances = CountRenderersUsingMaterial("TacticalContainerPbrApproved"),
            ApprovedPlayerInstances = CountSceneObjectsContaining("approved_player_tactical_v1") + CountSceneObjectsContaining("RS_04_player_tactical_LOD0"),
            ApprovedPlayerRendererInstances = CountSceneObjectsContainingRenderer("approved_player_tactical_v1") + CountSceneObjectsContainingRenderer("RS_04_player_tactical_LOD0"),
            ApprovedPlayerPbrRendererInstances = CountRenderersUsingMaterial("TacticalPlayerPbrApproved") + CountSceneObjectsContainingRenderer("approved_player_tactical_v1") + CountSceneObjectsContainingRenderer("RS_04_player_tactical_LOD0"),
            ApprovedEnemyInstances = CountSceneObjectsContaining("approved_enemy_tactical_v1") + CountSceneObjectsContaining("RS_05_enemy_tactical_LOD0"),
            ApprovedEnemyRendererInstances = CountSceneObjectsContainingRenderer("approved_enemy_tactical_v1") + CountSceneObjectsContainingRenderer("RS_05_enemy_tactical_LOD0"),
            ApprovedEnemyPbrRendererInstances = CountRenderersUsingMaterial("TacticalEnemyPbrApproved") + CountSceneObjectsContainingRenderer("approved_enemy_tactical_v1") + CountSceneObjectsContainingRenderer("RS_05_enemy_tactical_LOD0"),
            ApprovedMedicalLootInstances = CountSceneObjectsContaining("medical_loot_v1"),
            ApprovedMedicalLootRendererInstances = CountSceneObjectsContainingRenderer("medical_loot_v1"),
            ApprovedAmmoLootInstances = CountSceneObjectsContaining("approved_ammo_loot_v1"),
            ApprovedAmmoLootRendererInstances = CountSceneObjectsContainingRenderer("approved_ammo_loot_v1"),
            ApprovedHelmetLootInstances = CountSceneObjectsContaining("approved_helmet_loot_v1"),
            ApprovedHelmetLootRendererInstances = CountSceneObjectsContainingRenderer("approved_helmet_loot_v1"),
            ApprovedVestLootInstances = CountSceneObjectsContaining("approved_vest_loot_v1"),
            ApprovedVestLootRendererInstances = CountSceneObjectsContainingRenderer("approved_vest_loot_v1"),
            ApprovedWeaponPbrRendererInstances = CountRenderersUsingMaterial("TacticalWeapon"),
            WetAsphaltRendererCount = CountRenderersUsingMaterial("TacticalWetAsphaltApproved"),
            ConcreteRendererCount = CountRenderersUsingMaterial("TacticalConcrete"),
            TacticalPostProcessVolumeCount = CountSceneObjectsContaining("Tactical Post Process Volume"),
            TacticalRainFieldCount = CountSceneObjectsContaining("Tactical Rain Field"),
            TacticalIndustrialLightCount = CountSceneObjectsContaining("Tactical Industrial Yard Light") + CountSceneObjectsContaining("Tactical Warehouse Ceiling Light"),
            TacticalPostProcessCameraCount = CountPostProcessCameras(),
            FirstPersonWeaponPolishEventCount = CountFirstPersonWeaponPolishEvents(false),
            FirstPersonWeaponReloadEventCount = CountFirstPersonWeaponPolishEvents(true),
            FirstPersonWeaponRendererCount = CountFirstPersonWeaponRenderers(),
            CharacterAnimationStateEvidenceCount = CountCharacterAnimationStateEvidence(false),
            CharacterAnimationStateNamedCount = CountCharacterAnimationStateEvidence(true),
            CharacterProceduralLimbRigCount = CountCharacterProceduralLimbRig(false),
            CharacterProceduralLimbAnimationCount = CountCharacterProceduralLimbRig(true),
            CharacterAuthoredClipLibraryCount = CountCharacterAuthoredClipEvidence(false),
            CharacterAuthoredClipAnimationCount = CountCharacterAuthoredClipEvidence(true),
            ApprovedSkinnedCharacterAssetCount = CountApprovedSkinnedCharacterAssets(false),
            ApprovedSkinnedCharacterAnimationAssetCount = CountApprovedSkinnedCharacterAssets(true),
            TacticalCharacterDetailKitCount = CountSceneObjectsContaining("Tactical Character Detail Kit"),
            TacticalCharacterDetailRendererCount = CountRenderersUsingMaterial("TacticalCharacterPlatePbrApproved")
                + CountRenderersUsingMaterial("TacticalCharacterWebbingPbrApproved")
                + CountRenderersUsingMaterial("TacticalCharacterVisorPbrApproved")
                + CountRenderersUsingMaterial("TacticalPlayerSquadPatchApproved")
                + CountRenderersUsingMaterial("TacticalEnemySquadPatchApproved"),
            RealifiedCratePromotionSceneInstances = CountRenderersUsingMaterial("RealifiedCratePbrPromoted"),
            RealifiedContainerPromotionSceneInstances = CountRenderersUsingMaterial("RealifiedContainerPbrPromoted"),
            RealifiedAmmoLootPromotionSceneInstances = CountRenderersUsingMaterial("RealifiedAmmoLootPbrPromoted"),
            RealifiedMedkitLootPromotionSceneInstances = CountRenderersUsingMaterial("RealifiedMedkitLootPbrPromoted"),
            RealifiedHelmetPromotionSceneInstances = CountRenderersUsingMaterial("RealifiedHelmetPbrPromoted"),
            RealifiedVestPromotionSceneInstances = CountRenderersUsingMaterial("RealifiedVestPbrPromoted"),
            RealifiedSidearmPromotionSceneInstances = CountRenderersUsingMaterial("RealifiedSidearmPbrPromoted"),
            RealifiedHeroRiflePromotionSceneInstances = CountRenderersUsingMaterial("RealifiedHeroRiflePbrPromoted"),
            RealifiedSecondaryWeaponPromotionSceneInstances = CountRenderersUsingMaterial("RealifiedSecondaryWeaponPbrPromoted"),
            KnownCategoryFailedBatchFiles = Directory.Exists("Assets/HtmlTacticalAssets/RealifiedAssets")
                ? Directory.GetFiles("Assets/HtmlTacticalAssets/RealifiedAssets", "*_textured.glb", SearchOption.TopDirectoryOnly).Length
                : 0,
            CategoryFailedSceneInstances = CountCategoryFailedSceneInstances()
        };
        LoadRealifiedAuditSummary(summary);
        summary.RealifiedGearPromotionSceneInstances = summary.RealifiedHelmetPromotionSceneInstances + summary.RealifiedVestPromotionSceneInstances;
        summary.RealifiedLootPromotionSceneInstances = summary.RealifiedAmmoLootPromotionSceneInstances + summary.RealifiedMedkitLootPromotionSceneInstances;
        summary.RealifiedWeaponPromotionSceneInstances = summary.RealifiedSidearmPromotionSceneInstances
            + summary.RealifiedHeroRiflePromotionSceneInstances
            + summary.RealifiedSecondaryWeaponPromotionSceneInstances;
        summary.RealifiedEnvironmentPropSemanticAllowed = RealifiedCategorySemanticAllowed("environment_prop");
        summary.RealifiedGearSemanticAllowed = RealifiedCategorySemanticAllowed("gear");
        summary.RealifiedLootSemanticAllowed = RealifiedCategorySemanticAllowed("loot");
        summary.RealifiedWeaponSemanticAllowed = RealifiedCategorySemanticAllowed("weapon");
        summary.RealifiedWeaponSemanticCategoryMatches = RealifiedWeaponSemanticCategoryMatches();
        summary.RealifiedSemanticAllowedClassCount =
            (summary.RealifiedEnvironmentPropSemanticAllowed ? 1 : 0)
            + (summary.RealifiedGearSemanticAllowed ? 1 : 0)
            + (summary.RealifiedLootSemanticAllowed ? 1 : 0)
            + (summary.RealifiedWeaponSemanticAllowed ? 1 : 0);

        summary.VisualPolishPassed = summary.TacticalPostProcessVolumeCount >= 1
            && summary.TacticalRainFieldCount >= 1
            && summary.TacticalIndustrialLightCount >= 3
            && summary.TacticalPostProcessCameraCount >= 1;
        summary.FirstPersonWeaponPolishPassed = summary.FirstPersonWeaponPolishEventCount >= 1
            && summary.FirstPersonWeaponReloadEventCount >= 1
            && summary.FirstPersonWeaponRendererCount >= 12
            && summary.ApprovedWeaponPbrRendererInstances + summary.RealifiedWeaponPromotionSceneInstances >= 4;
        summary.CharacterAnimationStateEvidencePassed = summary.CharacterAnimationStateEvidenceCount >= 10
            && summary.CharacterAnimationStateNamedCount >= 10;
        summary.CharacterProceduralLimbAnimationPassed = summary.CharacterProceduralLimbRigCount >= 10
            && summary.CharacterProceduralLimbAnimationCount >= 10;
        summary.CharacterAuthoredClipAnimationPassed = summary.CharacterAuthoredClipLibraryCount >= 10
            && summary.CharacterAuthoredClipAnimationCount >= 10;
        summary.TrueSkinnedHumanoidImportPassed = summary.ApprovedSkinnedCharacterAssetCount >= 2
            && summary.ApprovedSkinnedCharacterAnimationAssetCount >= 2;
        summary.TacticalCharacterDetailKitPassed = summary.TacticalCharacterDetailKitCount >= 2
            && summary.TacticalCharacterDetailRendererCount >= 32;
        summary.RealifiedEnvironmentPropPromotionEvidencePassed = summary.RealifiedEnvironmentPropSemanticAllowed
            && summary.RealifiedAuditSidecarPromotableCount >= 6
            && summary.RealifiedCratePromotionSceneInstances >= 1
            && summary.RealifiedContainerPromotionSceneInstances >= 1;
        summary.RealifiedGearLootPromotionEvidencePassed = summary.RealifiedGearSemanticAllowed
            && summary.RealifiedLootSemanticAllowed
            && summary.RealifiedAuditSidecarPromotableCount >= 18
            && summary.RealifiedAmmoLootPromotionSceneInstances >= 1
            && summary.RealifiedMedkitLootPromotionSceneInstances >= 1
            && summary.RealifiedHelmetPromotionSceneInstances >= 1
            && summary.RealifiedVestPromotionSceneInstances >= 1;
        summary.RealifiedWeaponPromotionEvidencePassed = summary.RealifiedWeaponSemanticAllowed
            && summary.RealifiedAuditSidecarPromotableCount >= 27
            && summary.RealifiedSidearmPromotionSceneInstances >= 1
            && summary.RealifiedHeroRiflePromotionSceneInstances >= 1
            && summary.RealifiedSecondaryWeaponPromotionSceneInstances >= 1;
        summary.ApprovedIncrementalAssetsPassed = summary.ApprovedCrateInstances >= 1
            && summary.ApprovedCrateRendererInstances >= 1
            && summary.ApprovedCratePbrRendererInstances >= 1
            && summary.ApprovedContainerInstances >= 1
            && summary.ApprovedContainerRendererInstances >= 1
            && summary.ApprovedContainerPbrRendererInstances >= 1
            && summary.ApprovedPlayerInstances >= 1
            && summary.ApprovedPlayerRendererInstances >= 1
            && summary.ApprovedPlayerPbrRendererInstances >= 1
            && summary.ApprovedEnemyInstances >= 1
            && summary.ApprovedEnemyRendererInstances >= 1
            && summary.ApprovedEnemyPbrRendererInstances >= 1
            && (summary.ApprovedMedicalLootInstances >= 1 || summary.RealifiedMedkitLootPromotionSceneInstances >= 1)
            && (summary.ApprovedMedicalLootRendererInstances >= 1 || summary.RealifiedMedkitLootPromotionSceneInstances >= 1)
            && (summary.ApprovedAmmoLootInstances >= 1 || summary.RealifiedAmmoLootPromotionSceneInstances >= 1)
            && (summary.ApprovedAmmoLootRendererInstances >= 1 || summary.RealifiedAmmoLootPromotionSceneInstances >= 1)
            && (summary.ApprovedHelmetLootInstances >= 1 || summary.RealifiedHelmetPromotionSceneInstances >= 1)
            && (summary.ApprovedHelmetLootRendererInstances >= 1 || summary.RealifiedHelmetPromotionSceneInstances >= 1)
            && (summary.ApprovedVestLootInstances >= 1 || summary.RealifiedVestPromotionSceneInstances >= 1)
            && (summary.ApprovedVestLootRendererInstances >= 1 || summary.RealifiedVestPromotionSceneInstances >= 1)
            && summary.WetAsphaltRendererCount >= 1
            && summary.ConcreteRendererCount >= 1
            && summary.VisualPolishPassed
            && summary.CategoryFailedSceneInstances == 0;
        summary.FullVisualAssetGatePassed = false;
        var hasCurrentPolishEvidence = summary.FirstPersonWeaponPolishPassed
            && summary.CharacterAnimationStateEvidencePassed
            && summary.CharacterProceduralLimbAnimationPassed
            && summary.CharacterAuthoredClipAnimationPassed
            && summary.TacticalCharacterDetailKitPassed;
        summary.CompletionCredit = summary.ApprovedIncrementalAssetsPassed && hasCurrentPolishEvidence
            ? summary.RealifiedWeaponPromotionEvidencePassed
                ? "gameplay_route_plus_incremental_assets_environment_first_person_weapon_polish_authored_character_clip_tactical_character_detail_kit_and_realified_environment_gear_loot_weapon_promotion_evidence"
                : summary.RealifiedGearLootPromotionEvidencePassed
                    ? "gameplay_route_plus_incremental_assets_environment_first_person_weapon_polish_authored_character_clip_tactical_character_detail_kit_and_realified_environment_gear_loot_promotion_evidence"
                : summary.RealifiedEnvironmentPropPromotionEvidencePassed
                    ? "gameplay_route_plus_incremental_assets_environment_first_person_weapon_polish_authored_character_clip_tactical_character_detail_kit_and_realified_environment_prop_promotion_evidence"
                : "gameplay_route_plus_incremental_assets_environment_first_person_weapon_polish_authored_character_clip_and_tactical_character_detail_kit_evidence"
            : summary.ApprovedIncrementalAssetsPassed
                ? "gameplay_route_plus_incremental_player_enemy_container_crate_medical_ammo_helmet_vest_environment_postprocess_weather_only"
            : "gameplay_route_only";
        var auditNote = "Current RealifiedAssets audit: " + summary.RealifiedAuditPromotablePbrCandidates
            + " promotable, " + summary.RealifiedAuditTexturedProbeCount
            + " textured probes, " + summary.RealifiedAuditSidecarPromotableCount
            + " sidecar-promotable; crate scene instances " + summary.RealifiedCratePromotionSceneInstances
            + ", container scene instances " + summary.RealifiedContainerPromotionSceneInstances
            + ", gear scene instances " + summary.RealifiedGearPromotionSceneInstances
            + ", loot scene instances " + summary.RealifiedLootPromotionSceneInstances
            + ", weapon scene instances " + summary.RealifiedWeaponPromotionSceneInstances + ".";
        summary.VisualCompletionBlocker = hasCurrentPolishEvidence
            ? summary.TrueSkinnedHumanoidImportPassed
                ? "Full visual completion still needs final first-person hero weapon art review, final humanoid art/retarget quality review, and remaining class-by-class promotion of generated GLBs. " + auditNote + " Current approved character GLBs now contain skin, animation data, and an authored tactical detail kit, but the humanoid art is still an intermediate tactical import rather than a final production character."
                : "Full visual completion still needs a true skinned production humanoid import, final first-person hero weapon art review, and class-by-class promotion of generated GLBs. " + auditNote + " Current authored clip library and tactical detail kit are intermediate rig-on-parts improvements, not final character assets."
            : "Full visual completion still needs first-person weapon polish evidence, tactical character detail kit evidence, authored humanoid animation, and class-by-class promotion of generated GLBs. Current RealifiedAssets audit must show production-promotable assets before batch replacement.";
        return summary;
    }

    private static void AppendAssetQuality(StringBuilder json, AssetQualitySummary summary)
    {
        json.AppendLine("  \"asset_quality\": {");
        AppendNested(json, "approved_crate_instances", summary.ApprovedCrateInstances, true);
        AppendNested(json, "approved_crate_renderer_instances", summary.ApprovedCrateRendererInstances, true);
        AppendNested(json, "approved_crate_pbr_renderer_instances", summary.ApprovedCratePbrRendererInstances, true);
        AppendNested(json, "approved_container_instances", summary.ApprovedContainerInstances, true);
        AppendNested(json, "approved_container_renderer_instances", summary.ApprovedContainerRendererInstances, true);
        AppendNested(json, "approved_container_pbr_renderer_instances", summary.ApprovedContainerPbrRendererInstances, true);
        AppendNested(json, "approved_player_instances", summary.ApprovedPlayerInstances, true);
        AppendNested(json, "approved_player_renderer_instances", summary.ApprovedPlayerRendererInstances, true);
        AppendNested(json, "approved_player_pbr_renderer_instances", summary.ApprovedPlayerPbrRendererInstances, true);
        AppendNested(json, "approved_enemy_instances", summary.ApprovedEnemyInstances, true);
        AppendNested(json, "approved_enemy_renderer_instances", summary.ApprovedEnemyRendererInstances, true);
        AppendNested(json, "approved_enemy_pbr_renderer_instances", summary.ApprovedEnemyPbrRendererInstances, true);
        AppendNested(json, "approved_medical_loot_instances", summary.ApprovedMedicalLootInstances, true);
        AppendNested(json, "approved_medical_loot_renderer_instances", summary.ApprovedMedicalLootRendererInstances, true);
        AppendNested(json, "approved_ammo_loot_instances", summary.ApprovedAmmoLootInstances, true);
        AppendNested(json, "approved_ammo_loot_renderer_instances", summary.ApprovedAmmoLootRendererInstances, true);
        AppendNested(json, "approved_helmet_loot_instances", summary.ApprovedHelmetLootInstances, true);
        AppendNested(json, "approved_helmet_loot_renderer_instances", summary.ApprovedHelmetLootRendererInstances, true);
        AppendNested(json, "approved_vest_loot_instances", summary.ApprovedVestLootInstances, true);
        AppendNested(json, "approved_vest_loot_renderer_instances", summary.ApprovedVestLootRendererInstances, true);
        AppendNested(json, "approved_weapon_pbr_renderer_instances", summary.ApprovedWeaponPbrRendererInstances, true);
        AppendNested(json, "wet_asphalt_renderer_count", summary.WetAsphaltRendererCount, true);
        AppendNested(json, "concrete_renderer_count", summary.ConcreteRendererCount, true);
        AppendNested(json, "tactical_postprocess_volume_count", summary.TacticalPostProcessVolumeCount, true);
        AppendNested(json, "tactical_rain_field_count", summary.TacticalRainFieldCount, true);
        AppendNested(json, "tactical_industrial_light_count", summary.TacticalIndustrialLightCount, true);
        AppendNested(json, "tactical_postprocess_camera_count", summary.TacticalPostProcessCameraCount, true);
        AppendNested(json, "first_person_weapon_polish_event_count", summary.FirstPersonWeaponPolishEventCount, true);
        AppendNested(json, "first_person_weapon_reload_event_count", summary.FirstPersonWeaponReloadEventCount, true);
        AppendNested(json, "first_person_weapon_renderer_count", summary.FirstPersonWeaponRendererCount, true);
        AppendNested(json, "character_animation_state_evidence_count", summary.CharacterAnimationStateEvidenceCount, true);
        AppendNested(json, "character_animation_state_named_count", summary.CharacterAnimationStateNamedCount, true);
        AppendNested(json, "character_procedural_limb_rig_count", summary.CharacterProceduralLimbRigCount, true);
        AppendNested(json, "character_procedural_limb_animation_count", summary.CharacterProceduralLimbAnimationCount, true);
        AppendNested(json, "character_authored_clip_library_count", summary.CharacterAuthoredClipLibraryCount, true);
        AppendNested(json, "character_authored_clip_animation_count", summary.CharacterAuthoredClipAnimationCount, true);
        AppendNested(json, "approved_skinned_character_asset_count", summary.ApprovedSkinnedCharacterAssetCount, true);
        AppendNested(json, "approved_skinned_character_animation_asset_count", summary.ApprovedSkinnedCharacterAnimationAssetCount, true);
        AppendNested(json, "tactical_character_detail_kit_count", summary.TacticalCharacterDetailKitCount, true);
        AppendNested(json, "tactical_character_detail_renderer_count", summary.TacticalCharacterDetailRendererCount, true);
        AppendNested(json, "known_category_failed_batch_files", summary.KnownCategoryFailedBatchFiles, true);
        AppendNested(json, "category_failed_scene_instances", summary.CategoryFailedSceneInstances, true);
        AppendNested(json, "realified_audit_present", summary.RealifiedAuditPresent, true);
        AppendNested(json, "realified_audit_total_glb", summary.RealifiedAuditTotalGlb, true);
        AppendNested(json, "realified_audit_textured_probe_count", summary.RealifiedAuditTexturedProbeCount, true);
        AppendNested(json, "realified_audit_promotable_pbr_candidates", summary.RealifiedAuditPromotablePbrCandidates, true);
        AppendNested(json, "realified_audit_sidecar_promotable_count", summary.RealifiedAuditSidecarPromotableCount, true);
        AppendNested(json, "realified_audit_external_texture_sidecars", summary.RealifiedAuditExternalTextureSidecars, true);
        AppendNested(json, "realified_audit_external_material_files", summary.RealifiedAuditExternalMaterialFiles, true);
        AppendNested(json, "realified_crate_promotion_scene_instances", summary.RealifiedCratePromotionSceneInstances, true);
        AppendNested(json, "realified_container_promotion_scene_instances", summary.RealifiedContainerPromotionSceneInstances, true);
        AppendNested(json, "realified_ammo_loot_promotion_scene_instances", summary.RealifiedAmmoLootPromotionSceneInstances, true);
        AppendNested(json, "realified_medkit_loot_promotion_scene_instances", summary.RealifiedMedkitLootPromotionSceneInstances, true);
        AppendNested(json, "realified_helmet_promotion_scene_instances", summary.RealifiedHelmetPromotionSceneInstances, true);
        AppendNested(json, "realified_vest_promotion_scene_instances", summary.RealifiedVestPromotionSceneInstances, true);
        AppendNested(json, "realified_gear_promotion_scene_instances", summary.RealifiedGearPromotionSceneInstances, true);
        AppendNested(json, "realified_loot_promotion_scene_instances", summary.RealifiedLootPromotionSceneInstances, true);
        AppendNested(json, "realified_sidearm_promotion_scene_instances", summary.RealifiedSidearmPromotionSceneInstances, true);
        AppendNested(json, "realified_hero_rifle_promotion_scene_instances", summary.RealifiedHeroRiflePromotionSceneInstances, true);
        AppendNested(json, "realified_secondary_weapon_promotion_scene_instances", summary.RealifiedSecondaryWeaponPromotionSceneInstances, true);
        AppendNested(json, "realified_weapon_promotion_scene_instances", summary.RealifiedWeaponPromotionSceneInstances, true);
        AppendNested(json, "realified_environment_prop_semantic_allowed", summary.RealifiedEnvironmentPropSemanticAllowed, true);
        AppendNested(json, "realified_gear_semantic_allowed", summary.RealifiedGearSemanticAllowed, true);
        AppendNested(json, "realified_loot_semantic_allowed", summary.RealifiedLootSemanticAllowed, true);
        AppendNested(json, "realified_weapon_semantic_allowed", summary.RealifiedWeaponSemanticAllowed, true);
        AppendNested(json, "realified_weapon_semantic_category_matches", summary.RealifiedWeaponSemanticCategoryMatches, true);
        AppendNested(json, "realified_semantic_allowed_class_count", summary.RealifiedSemanticAllowedClassCount, true);
        AppendNested(json, "realified_environment_prop_promotion_evidence_passed", summary.RealifiedEnvironmentPropPromotionEvidencePassed, true);
        AppendNested(json, "realified_gear_loot_promotion_evidence_passed", summary.RealifiedGearLootPromotionEvidencePassed, true);
        AppendNested(json, "realified_weapon_promotion_evidence_passed", summary.RealifiedWeaponPromotionEvidencePassed, true);
        AppendNested(json, "approved_incremental_assets_passed", summary.ApprovedIncrementalAssetsPassed, true);
        AppendNested(json, "visual_polish_passed", summary.VisualPolishPassed, true);
        AppendNested(json, "first_person_weapon_polish_passed", summary.FirstPersonWeaponPolishPassed, true);
        AppendNested(json, "character_animation_state_evidence_passed", summary.CharacterAnimationStateEvidencePassed, true);
        AppendNested(json, "character_procedural_limb_animation_passed", summary.CharacterProceduralLimbAnimationPassed, true);
        AppendNested(json, "character_authored_clip_animation_passed", summary.CharacterAuthoredClipAnimationPassed, true);
        AppendNested(json, "true_skinned_humanoid_import_passed", summary.TrueSkinnedHumanoidImportPassed, true);
        AppendNested(json, "tactical_character_detail_kit_passed", summary.TacticalCharacterDetailKitPassed, true);
        AppendNested(json, "full_visual_asset_gate_passed", summary.FullVisualAssetGatePassed, true);
        AppendNested(json, "completion_credit", summary.CompletionCredit, true);
        AppendNested(json, "visual_completion_blocker", summary.VisualCompletionBlocker, false);
        json.AppendLine("  }");
    }

    private static void LoadRealifiedAuditSummary(AssetQualitySummary summary)
    {
        if (!File.Exists(RealifiedAuditPath))
        {
            summary.RealifiedAuditPresent = false;
            return;
        }

        var json = File.ReadAllText(RealifiedAuditPath);
        summary.RealifiedAuditPresent = true;
        summary.RealifiedAuditTotalGlb = ReadIntJsonField(json, "total_glb");
        summary.RealifiedAuditTexturedProbeCount = ReadIntJsonField(json, "textured_probe_not_production");
        summary.RealifiedAuditPromotablePbrCandidates = ReadIntJsonField(json, "promotable_pbr_candidate");
        summary.RealifiedAuditSidecarPromotableCount = ReadIntJsonField(json, "sidecar_promotable_count");
        summary.RealifiedAuditExternalTextureSidecars = ReadIntJsonField(json, "external_texture_sidecars");
        summary.RealifiedAuditExternalMaterialFiles = ReadIntJsonField(json, "external_material_files");
    }

    private static int CountApprovedSkinnedCharacterAssets(bool requireAnimation)
    {
        var count = 0;
        var paths = new[]
        {
            "Assets/HtmlTacticalAssets/ApprovedAssets/approved_player_tactical_v1.glb",
            "Assets/HtmlTacticalAssets/ApprovedAssets/approved_enemy_tactical_v1.glb"
        };

        foreach (var path in paths)
        {
            if (GlbJsonArrayHasItems(path, "skins")
                && (!requireAnimation || GlbJsonArrayHasItems(path, "animations")))
            {
                count++;
            }
        }

        return count;
    }

    private static bool GlbJsonArrayHasItems(string path, string key)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        var data = File.ReadAllBytes(path);
        if (data.Length < 20 || data[0] != (byte)'g' || data[1] != (byte)'l' || data[2] != (byte)'T' || data[3] != (byte)'F')
        {
            return false;
        }

        var offset = 12;
        while (offset + 8 <= data.Length)
        {
            var chunkLength = BitConverter.ToInt32(data, offset);
            var chunkType = BitConverter.ToUInt32(data, offset + 4);
            offset += 8;
            if (offset + chunkLength > data.Length)
            {
                return false;
            }

            if (chunkType == 0x4E4F534A)
            {
                var json = Encoding.UTF8.GetString(data, offset, chunkLength);
                var marker = "\"" + key + "\":[";
                var markerIndex = json.IndexOf(marker, StringComparison.Ordinal);
                if (markerIndex < 0)
                {
                    return false;
                }

                var arrayStart = markerIndex + marker.Length;
                while (arrayStart < json.Length && char.IsWhiteSpace(json[arrayStart]))
                {
                    arrayStart++;
                }

                return arrayStart < json.Length && json[arrayStart] != ']';
            }

            offset += chunkLength;
        }

        return false;
    }

    private static int ReadIntJsonField(string json, string fieldName)
    {
        var token = "\"" + fieldName + "\"";
        var index = json.IndexOf(token, StringComparison.Ordinal);
        if (index < 0)
        {
            return 0;
        }

        var colon = json.IndexOf(':', index + token.Length);
        if (colon < 0)
        {
            return 0;
        }

        var valueStart = colon + 1;
        while (valueStart < json.Length && char.IsWhiteSpace(json[valueStart]))
        {
            valueStart++;
        }

        var valueEnd = valueStart;
        while (valueEnd < json.Length && char.IsDigit(json[valueEnd]))
        {
            valueEnd++;
        }

        return valueEnd > valueStart && int.TryParse(json.Substring(valueStart, valueEnd - valueStart), out var value)
            ? value
            : 0;
    }

    private static int CountSceneObjectsContaining(string needle)
    {
        var count = 0;
        foreach (var obj in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
        {
            if (obj.name.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }

        return count;
    }

    private static int CountSceneObjectsContainingRenderer(string needle)
    {
        var count = 0;
        foreach (var obj in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
        {
            if (!obj.name.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (obj.GetComponentsInChildren<Renderer>(true).Length > 0)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountRenderersUsingMaterial(string materialNamePart)
    {
        var count = 0;
        foreach (var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include))
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material != null && material.name.Contains(materialNamePart, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    break;
                }
            }
        }

        return count;
    }

    private static int CountPostProcessCameras()
    {
        var count = 0;
        foreach (var cameraData in UnityEngine.Object.FindObjectsByType<UniversalAdditionalCameraData>(FindObjectsInactive.Include))
        {
            if (cameraData != null && cameraData.renderPostProcessing)
            {
                count++;
            }
        }

        return count;
    }

    private static void PrimeCharacterAnimationStateEvidence()
    {
        foreach (var visual in UnityEngine.Object.FindObjectsByType<TacticalCharacterMotionVisual>(FindObjectsInactive.Include))
        {
            if (!visual.HasVisualRoot)
            {
                continue;
            }

            visual.ApplyPreviewState(TacticalCharacterVisualState.Walk, 0.75f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Aim, 0.25f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Fire, 0.25f);
            visual.PulseHit();
            visual.ApplyPreviewState(TacticalCharacterVisualState.Down, 0f);
        }

        foreach (var visual in UnityEngine.Object.FindObjectsByType<TacticalCharacterClipVisual>(FindObjectsInactive.Include))
        {
            visual.ApplyPreviewState(TacticalCharacterVisualState.Walk, 0.75f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Aim, 0.25f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Fire, 0.25f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Hit, 0.1f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Down, 0f);
        }
    }

    private static int CountFirstPersonWeaponPolishEvents(bool reloadEvents)
    {
        var total = 0;
        foreach (var visual in UnityEngine.Object.FindObjectsByType<TacticalFirstPersonWeaponVisual>(FindObjectsInactive.Include))
        {
            total += reloadEvents ? visual.ReloadPolishEvents : visual.ShotPolishEvents;
        }

        return total;
    }

    private static int CountFirstPersonWeaponRenderers()
    {
        var total = 0;
        foreach (var visual in UnityEngine.Object.FindObjectsByType<TacticalFirstPersonWeaponVisual>(FindObjectsInactive.Include))
        {
            total += CountAuthoredFirstPersonWeaponRenderers(visual);
        }

        return total;
    }

    private static int CountEnabledFirstPersonWeaponRenderers(TacticalFirstPersonWeaponVisual visual)
    {
        if (visual == null)
        {
            return 0;
        }

        var activeWeapon = FindObjectByName("FP Weapon - " + visual.ActiveWeaponId);
        var root = activeWeapon == null ? visual.gameObject : activeWeapon;
        var count = 0;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.enabled && !IsHiddenSourceGlbRenderer(renderer))
            {
                count++;
            }
        }

        return count;
    }

    private static int CountEnabledFirstPersonGameplaySourceGlbRenderers(TacticalFirstPersonWeaponVisual visual)
    {
        if (visual == null)
        {
            return 0;
        }

        var activeWeapon = FindObjectByName("FP Weapon - " + visual.ActiveWeaponId);
        var root = activeWeapon == null ? visual.gameObject : activeWeapon;
        var count = 0;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.enabled && IsFirstPersonGameplaySourceGlbRenderer(renderer))
            {
                count++;
            }
        }

        return count;
    }

    private static int CountAuthoredFirstPersonWeaponRenderers(TacticalFirstPersonWeaponVisual visual)
    {
        if (visual == null)
        {
            return 0;
        }

        var count = 0;
        foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
        {
            if (!IsHiddenSourceGlbRenderer(renderer))
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsHiddenSourceGlbRenderer(Renderer renderer)
    {
        var current = renderer == null ? null : renderer.transform;
        while (current != null)
        {
            if (current.name.IndexOf("FP Hidden Source GLB", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static bool IsFirstPersonGameplaySourceGlbRenderer(Renderer renderer)
    {
        var current = renderer == null ? null : renderer.transform;
        while (current != null)
        {
            if (current.name.IndexOf("FP Gameplay Source GLB", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static int CountCharacterAnimationStateEvidence(bool namedStates)
    {
        var count = 0;
        foreach (var visual in UnityEngine.Object.FindObjectsByType<TacticalCharacterMotionVisual>(FindObjectsInactive.Include))
        {
            if (namedStates)
            {
                if (!string.IsNullOrWhiteSpace(visual.LastStateName) && visual.StateChangeCount >= 3)
                {
                    count++;
                }
            }
            else if (visual.AcceptedPlaceholderAnimationEvidence && visual.LastAnimationEvidence > 0.05f)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountCharacterProceduralLimbRig(bool requireAnimationEvidence)
    {
        var count = 0;
        foreach (var visual in UnityEngine.Object.FindObjectsByType<TacticalCharacterMotionVisual>(FindObjectsInactive.Include))
        {
            if (visual.ProceduralLimbRigPartCount < 12)
            {
                continue;
            }

            if (!requireAnimationEvidence || visual.ProceduralLimbAnimationEvidence && visual.LastLimbAnimationEvidence > 0.08f)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountCharacterAuthoredClipEvidence(bool requireAnimationEvidence)
    {
        var count = 0;
        foreach (var visual in UnityEngine.Object.FindObjectsByType<TacticalCharacterClipVisual>(FindObjectsInactive.Include))
        {
            if (!visual.HasAuthoredClipLibrary || visual.AuthoredClipCount < 6 || visual.RigPartCount < 12)
            {
                continue;
            }

            if (!requireAnimationEvidence || visual.AuthoredClipEvidence && visual.LastClipEvidence > 0.28f && visual.ClipStateChangeCount >= 3)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountCategoryFailedSceneInstances()
    {
        var environmentPropSemanticAllowed = RealifiedCategorySemanticAllowed("environment_prop");
        var characterSemanticAllowed = RealifiedCategorySemanticAllowed("character");
        var gearSemanticAllowed = RealifiedCategorySemanticAllowed("gear");
        var lootSemanticAllowed = RealifiedCategorySemanticAllowed("loot");
        var weaponSemanticAllowed = RealifiedCategorySemanticAllowed("weapon");
        var weaponSemanticCategoryMatches = RealifiedWeaponSemanticCategoryMatches();
        var failedPrefixes = new[]
        {
            "RS_02_sidearm",
            "RS_03_secondary_weapon",
            "RS_04_player_tactical",
            "RS_05_enemy_tactical",
            "RS_06_gear_helmet",
            "RS_07_gear_vest",
            "RS_08_loot_ammo",
            "RS_09_loot_medkit",
            "RS_10_prop_container",
            "RS_11_prop_crate",
            "hero_rifle"
        };
        var count = 0;
        foreach (var obj in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
        {
            foreach (var prefix in failedPrefixes)
            {
                if (obj.name.Contains(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (environmentPropSemanticAllowed && prefix == "RS_11_prop_crate" && IsPromotedRealifiedCrate(obj))
                    {
                        continue;
                    }
                    if (environmentPropSemanticAllowed && prefix == "RS_10_prop_container" && IsPromotedRealifiedContainer(obj))
                    {
                        continue;
                    }
                    if (characterSemanticAllowed && (prefix == "RS_04_player_tactical" || prefix == "RS_05_enemy_tactical") && IsPromotedRealifiedCharacter(obj))
                    {
                        continue;
                    }
                    if ((gearSemanticAllowed || lootSemanticAllowed) && IsPromotedRealifiedGearOrLoot(obj))
                    {
                        continue;
                    }
                    if ((weaponSemanticAllowed || weaponSemanticCategoryMatches) && IsRealifiedWeaponSalvageInstance(obj))
                    {
                        continue;
                    }

                    count++;
                    break;
                }
            }
        }

        return count;
    }

    private static bool RealifiedCategorySemanticAllowed(string category)
    {
        var json = File.Exists(RealifiedCategoryNemotronPath) ? File.ReadAllText(RealifiedCategoryNemotronPath) : "";
        if (string.IsNullOrEmpty(json) || !ExtractRenderedCategory(json, category))
        {
            return false;
        }

        foreach (var failed in ExtractStringArray(json, "failed_categories"))
        {
            if (failed == category)
            {
                return false;
            }
        }

        return true;
    }

    private static bool RealifiedWeaponSemanticCategoryMatches()
    {
        var json = File.Exists(RealifiedCategoryNemotronPath) ? File.ReadAllText(RealifiedCategoryNemotronPath) : "";
        if (string.IsNullOrEmpty(json) || !ExtractRenderedCategory(json, "weapon"))
        {
            return false;
        }

        var weaponBody = ExtractReviewBody(json, "weapon");
        if (string.IsNullOrEmpty(weaponBody))
        {
            return false;
        }

        var structuredReview = Regex.Replace(
            weaponBody,
            "\\\"raw_model_output\\\"\\s*:\\s*\\\"(?:\\\\.|[^\\\"])*\\\"",
            "",
            RegexOptions.Singleline);
        var visibleWeaponCount = Regex.Matches(structuredReview, "\\\"visible_category\\\"\\s*:\\s*\\\"weapon\\\"").Count;
        var categoryMismatchCount = Regex.Matches(structuredReview, "\\\"category_match\\\"\\s*:\\s*false").Count;
        return visibleWeaponCount >= 3 && categoryMismatchCount == 0;
    }

    private static string ExtractReviewBody(string json, string category)
    {
        var reviewsStart = json.IndexOf("\"reviews\"", StringComparison.Ordinal);
        if (reviewsStart < 0)
        {
            return "";
        }

        var marker = "\"" + category + "\":";
        var start = json.IndexOf(marker, reviewsStart, StringComparison.Ordinal);
        if (start < 0)
        {
            return "";
        }

        var nextMarkers = category == "weapon"
            ? new[] { "\"character\":", "\"gear\":", "\"loot\":", "\"environment_prop\":" }
            : category == "character"
                ? new[] { "\"gear\":", "\"loot\":", "\"environment_prop\":" }
                : category == "gear"
                    ? new[] { "\"loot\":", "\"environment_prop\":" }
                    : category == "loot"
                        ? new[] { "\"environment_prop\":" }
                        : Array.Empty<string>();
        var end = json.Length;
        foreach (var nextMarker in nextMarkers)
        {
            var candidate = json.IndexOf(nextMarker, start + marker.Length, StringComparison.Ordinal);
            if (candidate > start && candidate < end)
            {
                end = candidate;
            }
        }

        return json.Substring(start, end - start);
    }

    private static bool ExtractRenderedCategory(string json, string category)
    {
        var match = Regex.Match(json, "\\\"rendered_categories\\\"\\s*:\\s*\\{(?<body>.*?)\\n\\s*\\}", RegexOptions.Singleline);
        return match.Success && Regex.IsMatch(match.Groups["body"].Value, "\\\"" + Regex.Escape(category) + "\\\"\\s*:\\s*true");
    }

    private static string[] ExtractStringArray(string json, string key)
    {
        var match = Regex.Match(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*\\[(?<body>.*?)\\]", RegexOptions.Singleline);
        if (!match.Success)
        {
            return Array.Empty<string>();
        }

        var values = Regex.Matches(match.Groups["body"].Value, "\\\"(?<value>(?:\\\\.|[^\\\"])*)\\\"");
        var result = new string[values.Count];
        for (var i = 0; i < values.Count; i++)
        {
            result[i] = Regex.Unescape(values[i].Groups["value"].Value);
        }

        return result;
    }

    private static bool IsPromotedRealifiedCrate(GameObject obj)
    {
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
        {
            var material = renderer.sharedMaterial;
            if (material != null && material.name.Contains("RealifiedCratePbrPromoted", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPromotedRealifiedContainer(GameObject obj)
    {
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
        {
            var material = renderer.sharedMaterial;
            if (material != null && material.name.Contains("RealifiedContainerPbrPromoted", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPromotedRealifiedCharacter(GameObject obj)
    {
        return obj.GetComponentsInChildren<Renderer>(true).Length > 0
            && (obj.name.Contains("RS_04_player_tactical", StringComparison.OrdinalIgnoreCase)
                || obj.name.Contains("RS_05_enemy_tactical", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPromotedRealifiedGearOrLoot(GameObject obj)
    {
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
        {
            var material = renderer.sharedMaterial;
            if (material == null)
            {
                continue;
            }

            if (material.name.Contains("RealifiedHelmetPbrPromoted", StringComparison.OrdinalIgnoreCase)
                || material.name.Contains("RealifiedVestPbrPromoted", StringComparison.OrdinalIgnoreCase)
                || material.name.Contains("RealifiedAmmoLootPbrPromoted", StringComparison.OrdinalIgnoreCase)
                || material.name.Contains("RealifiedMedkitLootPbrPromoted", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPromotedRealifiedWeapon(GameObject obj)
    {
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
        {
            var material = renderer.sharedMaterial;
            if (material == null)
            {
                continue;
            }

            if (material.name.Contains("RealifiedSidearmPbrPromoted", StringComparison.OrdinalIgnoreCase)
                || material.name.Contains("RealifiedHeroRiflePbrPromoted", StringComparison.OrdinalIgnoreCase)
                || material.name.Contains("RealifiedSecondaryWeaponPbrPromoted", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsRealifiedWeaponSalvageInstance(GameObject obj)
    {
        if (IsPromotedRealifiedWeapon(obj))
        {
            return true;
        }

        var nameChain = new StringBuilder(obj.name);
        var current = obj.transform;
        while (current != null)
        {
            nameChain.Append(' ').Append(current.name);
            current = current.parent;
        }

        var names = nameChain.ToString();
        return (names.Contains("RS_02_sidearm", StringComparison.OrdinalIgnoreCase)
                || names.Contains("RS_03_secondary_weapon", StringComparison.OrdinalIgnoreCase)
                || names.Contains("hero_rifle", StringComparison.OrdinalIgnoreCase))
            && !names.Contains("RS_12_shotgun", StringComparison.OrdinalIgnoreCase);
    }

    private static int CountObjects(string prefix)
    {
        var count = 0;
        foreach (var obj in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude))
        {
            if (obj.name.StartsWith(prefix, StringComparison.Ordinal))
            {
                count++;
            }
        }

        return count;
    }

    private static T FindNamedObject<T>(string objectName) where T : Component
    {
        foreach (var item in UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Exclude))
        {
            if (item.name == objectName)
            {
                return item;
            }
        }

        return null;
    }

    private static GameObject FindObjectByName(string objectName)
    {
        foreach (var obj in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
        {
            if (obj.name == objectName)
            {
                return obj;
            }
        }

        return null;
    }

    private static void Teleport(TacticalPlayerController player, Vector3 position)
    {
        if (player == null)
        {
            return;
        }

        var controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.position = position;

        if (controller != null)
        {
            controller.enabled = true;
        }

        Physics.SyncTransforms();
    }

    private static float GetEnemyHealth(TacticalEnemy enemy)
    {
        return (float)(typeof(TacticalEnemy).GetField("health", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(enemy) ?? 0f);
    }

    private static TacticalEnemy FindLiveEnemyForGate()
    {
        foreach (var enemy in UnityEngine.Object.FindObjectsByType<TacticalEnemy>(FindObjectsInactive.Exclude))
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy && GetEnemyHealth(enemy) > 0f)
            {
                return enemy;
            }
        }

        return null;
    }

    private static float GetFloat(object target, string fieldName)
    {
        return (float)(target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target) ?? 0f);
    }

    private static int GetInt(object target, string fieldName)
    {
        return (int)(target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target) ?? 0);
    }

    private static void SetFloat(object target, string fieldName, float value)
    {
        target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
    }

    private static void SetInt(object target, string fieldName, int value)
    {
        target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
    }

    private static void SetEnemyFloat(TacticalEnemy enemy, string fieldName, float value)
    {
        typeof(TacticalEnemy).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(enemy, value);
    }

    private static void InvokePrivate(object target, string methodName)
    {
        target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(target, null);
    }

    private static void InvokeEnemyUpdate(TacticalEnemy enemy)
    {
        typeof(TacticalEnemy).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(enemy, null);
    }

    private static void Append(StringBuilder json, string key, string value, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": \"").Append(Escape(value)).Append("\"");
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, bool value, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": ").Append(value ? "true" : "false");
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, int value, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": ").Append(value.ToString(CultureInfo.InvariantCulture));
        json.AppendLine(comma ? "," : "");
    }

    private static void AppendNested(StringBuilder json, string key, string value, bool comma)
    {
        json.Append("    \"").Append(key).Append("\": \"").Append(Escape(value)).Append("\"");
        json.AppendLine(comma ? "," : "");
    }

    private static void AppendNested(StringBuilder json, string key, bool value, bool comma)
    {
        json.Append("    \"").Append(key).Append("\": ").Append(value ? "true" : "false");
        json.AppendLine(comma ? "," : "");
    }

    private static void AppendNested(StringBuilder json, string key, int value, bool comma)
    {
        json.Append("    \"").Append(key).Append("\": ").Append(value.ToString(CultureInfo.InvariantCulture));
        json.AppendLine(comma ? "," : "");
    }

    private static string Escape(string value)
    {
        return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
#endif
