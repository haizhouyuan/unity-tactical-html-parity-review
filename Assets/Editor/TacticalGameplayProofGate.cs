#if UNITY_EDITOR
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class TacticalGameplayProofGate
{
    private const string ReportPath = "docs/TACTICAL_GAMEPLAY_PROOF_GATE.json";

    [MenuItem("AI Tools/Write Tactical Gameplay Proof Gate")]
    public static void WriteReport()
    {
        Directory.CreateDirectory("docs");

        var gm = UnityEngine.Object.FindAnyObjectByType<TacticalGameManager>();
        var player = UnityEngine.Object.FindAnyObjectByType<TacticalPlayerController>();
        var camera = Camera.main;
        var report = new StringBuilder();
        var passed = gm != null && player != null && camera != null && Application.isPlaying;

        var lobbyCameraPass = false;
        var lobbyHidden = false;
        var panelsPresent = false;
        var pickupPass = false;
        var lootRichnessPass = false;
        var firePass = false;
        var feedbackPass = false;
        var reloadPass = false;
        var damagePass = false;
        var enemyRangedAttackPass = false;
        var healPass = false;
        var spawnPass = false;
        var ladderPass = false;
        var environmentFlowPass = false;
        var deathRestartPass = false;
        var cameraStancePass = false;
        var manualStartFirstPersonPass = false;
        var weaponVisualPass = false;
        var thirdPersonWeaponPass = false;
        var skinVisualPass = false;
        var audioFeedbackPass = false;
        var characterMotionPass = false;
        var firstPersonWeaponPolishPass = false;
        var characterAnimationStatePass = false;
        var characterProceduralLimbPass = false;
        var characterAuthoredClipPass = false;
        var skinPass = false;
        var settingsPass = false;
        var details = new StringBuilder();

        if (passed)
        {
            lobbyCameraPass = ProbeLobbyCamera(player, camera, details);
            gm.StartRound();
            SetFloat(gm, "roundStartTime", Time.time - 10f);
            manualStartFirstPersonPass = ProbeManualStartFirstPersonWeapon(player, details);

            var lobby = FindObjectByName("Lobby Panel");
            var death = FindObjectByName("Death Panel");
            var help = FindObjectByName("Help Panel");
            var settings = FindObjectByName("Settings Panel");
            var skins = FindObjectByName("Skins Panel");
            lobbyHidden = lobby == null || !lobby.activeInHierarchy;
            panelsPresent = help != null && settings != null && skins != null && death != null;

            settingsPass = ProbeSettings(gm);
            skinPass = ProbeSkinRoll(gm);
            pickupPass = ProbePickup(gm, player, details, out lootRichnessPass);
            firePass = ProbeFireHit(gm, player, camera, details, out feedbackPass);
            reloadPass = ProbeReload(gm, details);
            damagePass = ProbeDamage(gm, details);
            enemyRangedAttackPass = ProbeEnemyRangedAttack(gm, player, details);
            healPass = ProbeHealing(gm, details);
            spawnPass = ProbeDynamicSpawn(gm, details);
            ladderPass = ProbeLadder(gm, player, camera, details);
            environmentFlowPass = ProbeEnvironmentFlow(gm, player, camera, details);
            deathRestartPass = ProbeDeathRestart(gm, details);
            cameraStancePass = ProbeCameraStance(gm, player, camera, details);
            weaponVisualPass = ProbeWeaponVisualSwitch(gm, player, details);
            thirdPersonWeaponPass = ProbeThirdPersonNpcWeaponVisuals(gm, player, details);
            skinVisualPass = ProbeSkinVisuals(gm, player, details);
            audioFeedbackPass = ProbeAudioFeedback(gm, details);
            characterMotionPass = ProbeCharacterMotionFeedback(details);
            firstPersonWeaponPolishPass = ProbeFirstPersonWeaponPolish(gm, player, details);
            characterAnimationStatePass = ProbeCharacterAnimationStateEvidence(details);
            characterProceduralLimbPass = ProbeCharacterProceduralLimbEvidence(details);
            characterAuthoredClipPass = ProbeCharacterAuthoredClipEvidence(details);
        }

        passed = passed
            && lobbyCameraPass
            && lobbyHidden
            && panelsPresent
            && settingsPass
            && skinPass
            && pickupPass
            && lootRichnessPass
            && firePass
            && feedbackPass
            && reloadPass
            && damagePass
            && enemyRangedAttackPass
            && healPass
            && spawnPass
            && ladderPass
            && environmentFlowPass
            && deathRestartPass
            && cameraStancePass
            && manualStartFirstPersonPass
            && weaponVisualPass
            && thirdPersonWeaponPass
            && skinVisualPass
            && audioFeedbackPass
            && characterMotionPass
            && firstPersonWeaponPolishPass
            && characterAnimationStatePass
            && characterProceduralLimbPass
            && characterAuthoredClipPass;

        report.AppendLine("{");
        Append(report, "schema", "tactical_gameplay_proof_gate_v1", true);
        Append(report, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(report, "application_is_playing", Application.isPlaying, true);
        Append(report, "passed", passed, true);
        Append(report, "lobby_camera_clear", lobbyCameraPass, true);
        Append(report, "lobby_hidden", lobbyHidden, true);
        Append(report, "ui_panels_present", panelsPresent, true);
        Append(report, "settings_controls_work", settingsPass, true);
        Append(report, "skin_roll_works", skinPass, true);
        Append(report, "pickup_state_mutation", pickupPass, true);
        Append(report, "loot_richness_affects_pickup", lootRichnessPass, true);
        Append(report, "fire_ammo_and_enemy_hit", firePass, true);
        Append(report, "weapon_feedback_spawned", feedbackPass, true);
        Append(report, "reload_state_mutation", reloadPass, true);
        Append(report, "player_damage_state_mutation", damagePass, true);
        Append(report, "enemy_ranged_attack_state_mutation", enemyRangedAttackPass, true);
        Append(report, "heal_item_state_mutation", healPass, true);
        Append(report, "dynamic_spawn_state_mutation", spawnPass, true);
        Append(report, "ladder_floor_and_camera_mutation", ladderPass, true);
        Append(report, "environment_player_flow_verified", environmentFlowPass, true);
        Append(report, "death_overlay_and_restart", deathRestartPass, true);
        Append(report, "camera_ads_stance_jump_mutation", cameraStancePass, true);
        Append(report, "manual_start_first_person_weapon_visible", manualStartFirstPersonPass, true);
        Append(report, "first_person_weapon_visual_switches", weaponVisualPass, true);
        Append(report, "third_person_and_npc_weapon_visuals", thirdPersonWeaponPass, true);
        Append(report, "skin_visuals_apply_to_weapons", skinVisualPass, true);
        Append(report, "procedural_audio_feedback", audioFeedbackPass, true);
        Append(report, "character_motion_feedback", characterMotionPass, true);
        Append(report, "first_person_weapon_polish", firstPersonWeaponPolishPass, true);
        Append(report, "character_animation_state_evidence", characterAnimationStatePass, true);
        Append(report, "character_procedural_limb_animation", characterProceduralLimbPass, true);
        Append(report, "character_authored_clip_animation", characterAuthoredClipPass, true);
        Append(report, "details", details.ToString().Trim(), false);
        report.AppendLine("}");

        File.WriteAllText(ReportPath, report.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] Tactical gameplay proof gate report written to " + ReportPath + " passed=" + passed);
    }

    private static bool ProbeLobbyCamera(TacticalPlayerController player, Camera camera, StringBuilder details)
    {
        var follow = camera.GetComponent<TacticalCameraFollow>();
        follow?.SnapToLobby();
        var distance = Vector3.Distance(player.transform.position, camera.transform.position);
        var cameraForwardClear = camera.transform.position.z > player.transform.position.z + 10f;
        details.Append("lobbyCameraDist ").Append(distance.ToString("F2", CultureInfo.InvariantCulture))
            .Append(" zClear ").Append(cameraForwardClear).Append("; ");
        return distance > 14f && cameraForwardClear;
    }

    private static bool ProbeSettings(TacticalGameManager gm)
    {
        var slider = GetObject<Slider>(gm, "npcStrengthSlider");
        if (slider == null)
        {
            return false;
        }

        slider.value = 1.35f;
        var lootSlider = GetObject<Slider>(gm, "lootRichnessSlider");
        if (lootSlider == null)
        {
            return false;
        }

        lootSlider.value = 1.60f;
        return Mathf.Abs(gm.EnemyStrengthMultiplier - 1.35f) < 0.01f && Mathf.Abs(gm.LootRichnessMultiplier - 1.60f) < 0.01f;
    }

    private static bool ProbeSkinRoll(TacticalGameManager gm)
    {
        SetInt(gm, "coins", 1);
        var skinBefore = GetString(gm, "currentSkinName");
        var shardsBefore = GetInt(gm, "skinShards");
        gm.RollSkin();
        var coinsAfter = GetInt(gm, "coins");
        var skinAfter = GetString(gm, "currentSkinName");
        var shardsAfter = GetInt(gm, "skinShards");
        return coinsAfter <= 0 && (skinAfter != skinBefore || shardsAfter > shardsBefore);
    }

    private static bool ProbePickup(TacticalGameManager gm, TacticalPlayerController player, StringBuilder details, out bool lootRichnessPass)
    {
        lootRichnessPass = false;
        var loots = GetLoots(gm);
        var loot = FindLoot(TacticalLootKind.Ammo);
        var pistol = GetWeaponState(gm, "pistol");
        if (loot == null || loots == null)
        {
            return false;
        }

        var beforeCount = loots.Count;
        var reserveBefore = pistol == null ? -1 : pistol.reserve;
        Teleport(player, loot.transform.position + new Vector3(0.25f, 0f, 0.25f));
        gm.TryPickupNearest();
        var afterCount = loots.Count;
        var reserveAfter = pistol == null ? -1 : pistol.reserve;
        var expectedMinimumGain = Mathf.RoundToInt(24f * gm.LootRichnessMultiplier);
        lootRichnessPass = pistol != null && reserveAfter - reserveBefore >= expectedMinimumGain;
        details.Append("pickupCount ").Append(beforeCount).Append("->").Append(afterCount)
            .Append(" pistolReserve ").Append(reserveBefore).Append("->").Append(reserveAfter)
            .Append(" richness ").Append(gm.LootRichnessMultiplier.ToString("0.00", CultureInfo.InvariantCulture)).Append("; ");
        return afterCount == beforeCount - 1;
    }

    private static bool ProbeFireHit(TacticalGameManager gm, TacticalPlayerController player, Camera camera, StringBuilder details, out bool feedbackPass)
    {
        feedbackPass = false;
        var enemy = UnityEngine.Object.FindAnyObjectByType<TacticalEnemy>();
        gm.SelectWeapon("pistol");
        var state = GetWeaponState(gm, "pistol");
        if (enemy == null || state == null)
        {
            return false;
        }

        Teleport(player, new Vector3(0f, 1.04f, 30f));
        player.ResetView(180f, 0f);
        player.SetCameraMode(TacticalCameraMode.FirstPerson);
        SetBool(player, "ads", true);
        state.reloading = false;
        state.lastShotTime = -999f;
        var follow = camera.GetComponent<TacticalCameraFollow>();
        if (follow != null)
        {
            follow.SnapToPlayer();
        }

        var enemyController = enemy.GetComponent<CharacterController>();
        if (enemyController != null)
        {
            enemyController.enabled = false;
        }

        var aimPoint = camera.transform.position + camera.transform.forward * 5f;
        enemy.transform.position = aimPoint - Vector3.up;
        enemy.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        if (enemyController != null)
        {
            enemyController.enabled = true;
        }

        Physics.SyncTransforms();
        var healthBefore = GetEnemyHealth(enemy);
        var magazineBefore = state.magazine;
        var muzzleBefore = CountObjects("Muzzle Flash");
        var casingBefore = CountObjects("Ejected Casing");
        var tracerBefore = CountObjects("Tracer");
        gm.FireCurrentWeapon();
        Physics.SyncTransforms();
        var healthAfter = GetEnemyHealth(enemy);
        var magazineAfter = state.magazine;
        var muzzleAfter = CountObjects("Muzzle Flash");
        var casingAfter = CountObjects("Ejected Casing");
        var tracerAfter = CountObjects("Tracer");
        var hitMarker = GetObject<Text>(gm, "hitMarkerText");
        feedbackPass = muzzleAfter > muzzleBefore && casingAfter > casingBefore && tracerAfter > tracerBefore && hitMarker != null && hitMarker.text == "X";
        details.Append("fireMag ").Append(magazineBefore).Append("->").Append(magazineAfter)
            .Append(" enemyHp ").Append(healthBefore.ToString("F1", CultureInfo.InvariantCulture))
            .Append("->").Append(healthAfter.ToString("F1", CultureInfo.InvariantCulture))
            .Append(" tracers ").Append(tracerBefore).Append("->").Append(tracerAfter).Append("; ");
        return magazineAfter == magazineBefore - 1 && healthAfter < healthBefore;
    }

    private static bool ProbeReload(TacticalGameManager gm, StringBuilder details)
    {
        var state = GetWeaponState(gm, "pistol");
        if (state == null)
        {
            return false;
        }

        var magazineBefore = state.magazine;
        var reserveBefore = state.reserve;
        gm.Reload();
        var enteredReload = state.reloading;
        state.reloadEndTime = Time.time - 0.1f;
        InvokePrivate(gm, "CompleteReloads");
        details.Append("reload mag ").Append(magazineBefore).Append("->").Append(state.magazine)
            .Append(" reserve ").Append(reserveBefore).Append("->").Append(state.reserve).Append("; ");
        return enteredReload && !state.reloading && state.magazine > magazineBefore && state.reserve < reserveBefore;
    }

    private static bool ProbeDamage(TacticalGameManager gm, StringBuilder details)
    {
        SetFloat(gm, "roundStartTime", Time.time - 10f);
        var hpBefore = GetFloat(gm, "hp");
        gm.DamagePlayer(12f);
        var hpAfter = GetFloat(gm, "hp");
        details.Append("playerHp ").Append(hpBefore.ToString("F1", CultureInfo.InvariantCulture))
            .Append("->").Append(hpAfter.ToString("F1", CultureInfo.InvariantCulture)).Append("; ");
        return hpAfter < hpBefore;
    }

    private static bool ProbeEnemyRangedAttack(TacticalGameManager gm, TacticalPlayerController player, StringBuilder details)
    {
        var enemy = UnityEngine.Object.FindAnyObjectByType<TacticalEnemy>();
        if (enemy == null)
        {
            return false;
        }

        gm.StartRound();
        SetFloat(gm, "roundStartTime", Time.time - 10f);
        SetFloat(gm, "hp", 100f);
        Teleport(player, new Vector3(0f, 1.04f, 30f));
        var enemyController = enemy.GetComponent<CharacterController>();
        if (enemyController != null)
        {
            enemyController.enabled = false;
        }

        enemy.transform.position = player.transform.position + new Vector3(0f, 0f, -8.2f);
        enemy.transform.rotation = Quaternion.LookRotation((player.transform.position - enemy.transform.position).normalized);

        if (enemyController != null)
        {
            enemyController.enabled = true;
        }

        SetEnemyFloat(enemy, "nextShotTime", Time.time - 0.1f);
        var hpBefore = GetFloat(gm, "hp");
        var tracerBefore = CountObjects("Tracer");
        var sfxBefore = gm.SfxEventCount;
        InvokeEnemyUpdate(enemy);
        Physics.SyncTransforms();
        var hpAfter = GetFloat(gm, "hp");
        var tracerAfter = CountObjects("Tracer");
        var sfxAfter = gm.SfxEventCount;
        details.Append("enemyRangedHp ").Append(hpBefore.ToString("F1", CultureInfo.InvariantCulture))
            .Append("->").Append(hpAfter.ToString("F1", CultureInfo.InvariantCulture))
            .Append(" enemyTracers ").Append(tracerBefore).Append("->").Append(tracerAfter)
            .Append(" enemySfx ").Append(sfxBefore).Append("->").Append(sfxAfter).Append("; ");
        return hpAfter < hpBefore && tracerAfter > tracerBefore && sfxAfter > sfxBefore;
    }

    private static bool ProbeHealing(TacticalGameManager gm, StringBuilder details)
    {
        SetFloat(gm, "roundStartTime", Time.time - 10f);
        SetFloat(gm, "hp", 42f);
        SetInt(gm, "firstAid", 1);
        var hpBefore = GetFloat(gm, "hp");
        var firstAidBefore = GetInt(gm, "firstAid");
        gm.UseHeal(TacticalLootKind.FirstAid);
        var hpAfter = GetFloat(gm, "hp");
        var firstAidAfter = GetInt(gm, "firstAid");
        details.Append("healHp ").Append(hpBefore.ToString("F1", CultureInfo.InvariantCulture))
            .Append("->").Append(hpAfter.ToString("F1", CultureInfo.InvariantCulture))
            .Append(" firstAid ").Append(firstAidBefore).Append("->").Append(firstAidAfter).Append("; ");
        return hpAfter >= 75f && firstAidAfter == firstAidBefore - 1;
    }

    private static bool ProbeDynamicSpawn(TacticalGameManager gm, StringBuilder details)
    {
        var enemies = GetEnemies(gm);
        if (enemies == null)
        {
            return false;
        }

        var before = enemies.Count;
        SetFloat(gm, "nextEnemyRespawn", Time.time - 0.1f);
        InvokePrivate(gm, "MaybeSpawnEnemy");
        var after = enemies.Count;
        details.Append("spawnEnemies ").Append(before).Append("->").Append(after).Append("; ");
        return after > before;
    }

    private static bool ProbeLadder(TacticalGameManager gm, TacticalPlayerController player, Camera camera, StringBuilder details)
    {
        var ladder = UnityEngine.Object.FindAnyObjectByType<TacticalLadder>();
        if (ladder == null)
        {
            return false;
        }

        Teleport(player, ladder.transform.position + new Vector3(0.5f, 0f, -0.5f));
        gm.TryPickupNearest();
        var afterUp = player.transform.position;
        var cameraAfterUp = camera.transform.position;
        gm.TryPickupNearest();
        var afterDown = player.transform.position;
        var cameraDistance = Vector3.Distance(afterDown, camera.transform.position);
        details.Append("ladder upY ").Append(afterUp.y.ToString("F2", CultureInfo.InvariantCulture))
            .Append(" camUpY ").Append(cameraAfterUp.y.ToString("F2", CultureInfo.InvariantCulture))
            .Append(" downY ").Append(afterDown.y.ToString("F2", CultureInfo.InvariantCulture))
            .Append(" camDist ").Append(cameraDistance.ToString("F2", CultureInfo.InvariantCulture)).Append("; ");
        return afterUp.y > 3f && afterDown.y < 2f && cameraDistance < 14f;
    }

    private static bool ProbeEnvironmentFlow(TacticalGameManager gm, TacticalPlayerController player, Camera camera, StringBuilder details)
    {
        gm.StartRound();
        var follow = camera.GetComponent<TacticalCameraFollow>();
        var spawn = VisitZone(player, camera, follow, new Vector3(0f, 1.04f, 30f), 180f, "Spawn", "Spawn ", 16f, 8, out var spawnCount);
        var buildingA = VisitZone(player, camera, follow, new Vector3(-38f, 1.04f, -16f), 210f, "A Building", "A ", 16f, 8, out var buildingCount);
        var warehouse = VisitZone(player, camera, follow, new Vector3(0f, 1.04f, -28f), 180f, "Warehouse", "Warehouse", 18f, 7, out var warehouseCount);
        var container = VisitZone(player, camera, follow, new Vector3(0f, 1.04f, 14f), 180f, "Container Yard", "Container", 22f, 6, out var containerCount);
        var nearbyLoot = CountNearby<TacticalLoot>(player.transform.position, 22f);
        var ladder = FindNamedObject<TacticalLadder>("A Interior Ladder") ?? UnityEngine.Object.FindAnyObjectByType<TacticalLadder>();
        var floorBefore = GetInt(gm, "currentFloor");
        var floorAfter = floorBefore;
        if (ladder != null)
        {
            Teleport(player, ladder.transform.position + new Vector3(0.5f, 0f, -0.5f));
            gm.TryPickupNearest();
            floorAfter = GetInt(gm, "currentFloor");
            follow?.SnapToPlayer();
        }

        var upperFloorReached = floorAfter > floorBefore && player.transform.position.y > 3f;
        details.Append("environmentFlow spawn ").Append(spawn).Append("(").Append(spawnCount).Append(")")
            .Append(" buildingA ").Append(buildingA).Append("(").Append(buildingCount).Append(")")
            .Append(" warehouse ").Append(warehouse).Append("(").Append(warehouseCount).Append(")")
            .Append(" container ").Append(container).Append("(").Append(containerCount).Append(")")
            .Append(" nearbyLoot ").Append(nearbyLoot)
            .Append(" floor ").Append(floorBefore).Append("->").Append(floorAfter).Append("; ");
        return spawn && buildingA && warehouse && container && nearbyLoot >= 2 && upperFloorReached;
    }

    private static bool ProbeDeathRestart(TacticalGameManager gm, StringBuilder details)
    {
        var death = FindObjectByName("Death Panel");
        SetFloat(gm, "roundStartTime", Time.time - 10f);
        SetInt(gm, "revives", 0);
        SetFloat(gm, "hp", 20f);
        gm.DamagePlayer(999f);
        var down = gm.IsPlayerDown;
        var deathVisible = death != null && death.activeInHierarchy;
        var hpAfterDeath = GetFloat(gm, "hp");
        gm.StartRound();
        var hpAfterRestart = GetFloat(gm, "hp");
        var deathHiddenAfterRestart = death == null || !death.activeInHierarchy;
        details.Append("deathHp ").Append(hpAfterDeath.ToString("F1", CultureInfo.InvariantCulture))
            .Append(" deathPanel ").Append(deathVisible)
            .Append(" restartHp ").Append(hpAfterRestart.ToString("F1", CultureInfo.InvariantCulture)).Append("; ");
        return down && deathVisible && deathHiddenAfterRestart && hpAfterRestart >= 99f && !gm.IsInLobby;
    }

    private static bool ProbeCameraStance(TacticalGameManager gm, TacticalPlayerController player, Camera camera, StringBuilder details)
    {
        gm.StartRound();
        var follow = camera.GetComponent<TacticalCameraFollow>();
        player.ResetView(180f, 24f);
        follow?.SnapToPlayer();
        var thirdDistance = Vector3.Distance(player.transform.position, camera.transform.position);

        player.SetCameraMode(TacticalCameraMode.FirstPerson);
        player.SetAds(false);
        follow?.SnapToPlayer();
        var firstDistance = Vector3.Distance(player.CameraTarget.position, camera.transform.position);

        player.SetCameraMode(TacticalCameraMode.ThirdPerson);
        player.SetAds(true);
        follow?.SnapToPlayer();
        var adsDistance = Vector3.Distance(player.transform.position, camera.transform.position);

        player.SetAds(false);
        player.SetStance(TacticalStance.Crouch, true);
        var crouchHeight = player.ControllerHeight;
        player.SetStance(TacticalStance.Prone, true);
        var proneHeight = player.ControllerHeight;
        player.SetStance(TacticalStance.Stand, true);
        var standHeight = player.ControllerHeight;
        var jumped = player.TryJump();
        var jumpVelocity = player.VerticalVelocity;

        details.Append("camera third ").Append(thirdDistance.ToString("F2", CultureInfo.InvariantCulture))
            .Append(" first ").Append(firstDistance.ToString("F2", CultureInfo.InvariantCulture))
            .Append(" ads ").Append(adsDistance.ToString("F2", CultureInfo.InvariantCulture))
            .Append(" heights ").Append(standHeight.ToString("F2", CultureInfo.InvariantCulture))
            .Append("/").Append(crouchHeight.ToString("F2", CultureInfo.InvariantCulture))
            .Append("/").Append(proneHeight.ToString("F2", CultureInfo.InvariantCulture))
            .Append(" jump ").Append(jumped).Append(" vy ").Append(jumpVelocity.ToString("F2", CultureInfo.InvariantCulture)).Append("; ");

        return thirdDistance > 5f
            && firstDistance < 0.5f
            && adsDistance < thirdDistance
            && standHeight > crouchHeight
            && crouchHeight > proneHeight
            && jumped
            && jumpVelocity > 2f;
    }

    private static bool ProbeWeaponVisualSwitch(TacticalGameManager gm, TacticalPlayerController player, StringBuilder details)
    {
        var visual = UnityEngine.Object.FindAnyObjectByType<TacticalFirstPersonWeaponVisual>();
        if (visual == null)
        {
            return false;
        }

        player.SetCameraMode(TacticalCameraMode.FirstPerson);
        player.SetAds(false);
        var required = new[] { "pistol", "shotgun", "rifle", "dmr" };
        var switched = 0;
        foreach (var weaponId in required)
        {
            var state = GetWeaponState(gm, weaponId);
            if (state == null)
            {
                continue;
            }

            state.unlocked = true;
            gm.SelectWeapon(weaponId);
            visual.ForceRefresh();
            var root = FindObjectByName("FP Weapon - " + weaponId);
            var enabledRenderers = root == null ? 0 : CountEnabledRenderers(root);
            if (visual.ActiveWeaponId == weaponId && root != null && enabledRenderers > 0)
            {
                switched++;
            }
        }

        details.Append("fpWeaponSwitch ").Append(switched).Append("/").Append(required.Length)
            .Append(" active ").Append(visual.ActiveWeaponId).Append("; ");
        return switched == required.Length;
    }

    private static bool ProbeManualStartFirstPersonWeapon(TacticalPlayerController player, StringBuilder details)
    {
        var visual = UnityEngine.Object.FindAnyObjectByType<TacticalFirstPersonWeaponVisual>();
        if (player == null || visual == null)
        {
            details.Append("manualStart missing-player-or-fp-visual; ");
            return false;
        }

        visual.ForceRefresh();
        var root = FindObjectByName("FP Weapon - pistol");
        var enabledRenderers = root == null ? 0 : CountEnabledRenderers(root);
        var pass = player.CameraMode == TacticalCameraMode.FirstPerson
            && visual.ActiveWeaponId == "pistol"
            && visual.HasActiveHeroWeapon
            && enabledRenderers >= 12;

        details.Append("manualStart camera ").Append(player.CameraMode)
            .Append(" weapon ").Append(visual.ActiveWeaponId)
            .Append(" hero ").Append(visual.HasActiveHeroWeapon)
            .Append(" pistolRenderers ").Append(enabledRenderers)
            .Append(" required>=12")
            .Append("; ");
        return pass;
    }

    private static bool ProbeFirstPersonWeaponPolish(TacticalGameManager gm, TacticalPlayerController player, StringBuilder details)
    {
        var visual = UnityEngine.Object.FindAnyObjectByType<TacticalFirstPersonWeaponVisual>();
        var rifle = GetWeaponState(gm, "rifle");
        if (visual == null || rifle == null)
        {
            details.Append("fpPolish missing-visual-or-rifle; ");
            return false;
        }

        player.SetCameraMode(TacticalCameraMode.FirstPerson);
        player.SetAds(true);
        rifle.unlocked = true;
        rifle.magazine = Mathf.Max(2, rifle.magazine);
        rifle.reserve = Mathf.Max(rifle.reserve, 30);
        rifle.reloading = false;
        rifle.lastShotTime = -999f;

        var shotBefore = visual.ShotPolishEvents;
        var reloadBefore = visual.ReloadPolishEvents;
        var selectBefore = visual.SelectPolishEvents;
        gm.SelectWeapon("rifle");
        visual.ForceRefresh();
        gm.FireCurrentWeapon();
        gm.Reload();
        visual.ApplyPreviewPolish(0.35f, true, 0.85f, 0.75f);

        var shotDelta = visual.ShotPolishEvents - shotBefore;
        var reloadDelta = visual.ReloadPolishEvents - reloadBefore;
        var selectDelta = visual.SelectPolishEvents - selectBefore;
        var polishMagnitude = visual.LastPolishOffset.magnitude;
        var reloadMagnitude = visual.LastReloadOffset.magnitude;
        details.Append("fpPolish active ").Append(visual.ActiveWeaponId)
            .Append(" hero ").Append(visual.HasActiveHeroWeapon)
            .Append(" select/shot/reload ").Append(selectDelta).Append("/")
            .Append(shotDelta).Append("/").Append(reloadDelta)
            .Append(" offset ").Append(polishMagnitude.ToString("F3", CultureInfo.InvariantCulture))
            .Append(" reloadOffset ").Append(reloadMagnitude.ToString("F3", CultureInfo.InvariantCulture))
            .Append(" recoil ").Append(visual.LastRecoilKick.ToString("F3", CultureInfo.InvariantCulture)).Append("; ");

        return visual.HasActiveHeroWeapon
            && visual.ActiveWeaponId == "rifle"
            && selectDelta >= 1
            && shotDelta >= 1
            && reloadDelta >= 1
            && polishMagnitude > 0.03f
            && reloadMagnitude > 0.03f
            && visual.LastRecoilKick > 0.05f;
    }

    private static bool ProbeThirdPersonNpcWeaponVisuals(TacticalGameManager gm, TacticalPlayerController player, StringBuilder details)
    {
        var visuals = UnityEngine.Object.FindObjectsByType<TacticalThirdPersonWeaponVisual>(FindObjectsInactive.Exclude);
        TacticalThirdPersonWeaponVisual playerVisual = null;
        var npcVisuals = 0;
        foreach (var visual in visuals)
        {
            if (visual.FollowCurrentWeapon)
            {
                playerVisual = visual;
            }
            else
            {
                npcVisuals++;
                visual.ForceRefresh();
            }
        }

        if (playerVisual == null)
        {
            return false;
        }

        var required = new[] { "pistol", "shotgun", "rifle", "dmr" };
        var switched = 0;
        player.SetCameraMode(TacticalCameraMode.ThirdPerson);
        player.SetAds(false);
        foreach (var weaponId in required)
        {
            var state = GetWeaponState(gm, weaponId);
            if (state == null)
            {
                continue;
            }

            state.unlocked = true;
            gm.SelectWeapon(weaponId);
            playerVisual.ForceRefresh();
            var root = FindDirectChild(playerVisual.transform, "Character Weapon - " + weaponId);
            if (playerVisual.ActiveWeaponId == weaponId && root != null && CountEnabledRenderers(root.gameObject) > 0)
            {
                switched++;
            }
        }

        var enemyCount = UnityEngine.Object.FindObjectsByType<TacticalEnemy>(FindObjectsInactive.Exclude).Length;
        details.Append("tpWeaponSwitch ").Append(switched).Append("/").Append(required.Length)
            .Append(" npcWeaponVisuals ").Append(npcVisuals).Append("/").Append(enemyCount).Append("; ");
        return switched == required.Length && npcVisuals >= Mathf.Min(enemyCount, 9);
    }

    private static bool ProbeSkinVisuals(TacticalGameManager gm, TacticalPlayerController player, StringBuilder details)
    {
        SetString(gm, "currentSkinName", "霓虹电音");
        var rifle = GetWeaponState(gm, "rifle");
        if (rifle != null)
        {
            rifle.unlocked = true;
        }

        gm.SelectWeapon("rifle");
        var fp = UnityEngine.Object.FindAnyObjectByType<TacticalFirstPersonWeaponVisual>();
        var tp = null as TacticalThirdPersonWeaponVisual;
        foreach (var visual in UnityEngine.Object.FindObjectsByType<TacticalThirdPersonWeaponVisual>(FindObjectsInactive.Exclude))
        {
            if (visual.FollowCurrentWeapon)
            {
                tp = visual;
                break;
            }
        }

        if (fp == null || tp == null)
        {
            return false;
        }

        player.SetCameraMode(TacticalCameraMode.FirstPerson);
        player.SetAds(false);
        fp.ForceRefresh();
        player.SetCameraMode(TacticalCameraMode.ThirdPerson);
        tp.ForceRefresh();

        var expected = gm.CurrentSkinPrimary;
        var fpOk = fp.LastSkinName == gm.CurrentSkinName
            && (fp.LastTintedRenderers > 0 || fp.LastPbrPreservedRenderers > 0)
            && ColorsClose(fp.LastPrimaryColor, expected);
        var tpOk = tp.LastSkinName == gm.CurrentSkinName && tp.LastTintedRenderers > 0 && ColorsClose(tp.LastPrimaryColor, expected);
        var tracerChanged = !ColorsClose(gm.CurrentSkinTracer, new Color(0.980f, 0.800f, 0.082f, 1f));

        details.Append("skinVisual ").Append(gm.CurrentSkinName)
            .Append(" fpTint ").Append(fp.LastTintedRenderers)
            .Append(" fpPbrPreserved ").Append(fp.LastPbrPreservedRenderers)
            .Append(" tpTint ").Append(tp.LastTintedRenderers)
            .Append(" tracerChanged ").Append(tracerChanged).Append("; ");
        return fpOk && tpOk && tracerChanged;
    }

    private static bool ProbeAudioFeedback(TacticalGameManager gm, StringBuilder details)
    {
        var before = gm.SfxEventCount;
        gm.PlayEnemyShotSfx();
        gm.TryPlayFootstepSfx(0.8f);
        var after = gm.SfxEventCount;
        var clips = gm.SfxClipCount;
        var hasSource = gm.HasSfxAudioSource;
        details.Append("sfxEvents ").Append(before).Append("->").Append(after)
            .Append(" clips ").Append(clips)
            .Append(" source ").Append(hasSource).Append("; ");
        return hasSource && clips >= 10 && after >= before + 2 && after >= 7;
    }

    private static bool ProbeCharacterMotionFeedback(StringBuilder details)
    {
        var visuals = UnityEngine.Object.FindObjectsByType<TacticalCharacterMotionVisual>(FindObjectsInactive.Exclude);
        var responsive = 0;
        foreach (var visual in visuals)
        {
            if (!visual.HasVisualRoot)
            {
                continue;
            }

            visual.PulseHit();
            visual.ApplyPreviewMotion(0.85f);
            if (visual.LastMotionOffset > 0.025f)
            {
                responsive++;
            }
        }

        details.Append("characterMotion ").Append(responsive).Append("/").Append(visuals.Length).Append("; ");
        return visuals.Length >= 10 && responsive >= 10;
    }

    private static bool ProbeCharacterAnimationStateEvidence(StringBuilder details)
    {
        var visuals = UnityEngine.Object.FindObjectsByType<TacticalCharacterMotionVisual>(FindObjectsInactive.Exclude);
        var responsive = 0;
        var namedStates = 0;
        foreach (var visual in visuals)
        {
            if (!visual.HasVisualRoot)
            {
                continue;
            }

            visual.ApplyPreviewState(TacticalCharacterVisualState.Walk, 0.85f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Aim, 0.25f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Fire, 0.25f);
            visual.PulseHit();
            visual.ApplyPreviewState(TacticalCharacterVisualState.Down, 0f);
            if (visual.AcceptedPlaceholderAnimationEvidence && visual.LastAnimationEvidence > 0.05f)
            {
                responsive++;
            }

            if (!string.IsNullOrWhiteSpace(visual.LastStateName) && visual.StateChangeCount >= 3)
            {
                namedStates++;
            }
        }

        details.Append("characterAnimationState ").Append(responsive).Append("/")
            .Append(visuals.Length).Append(" named ").Append(namedStates).Append("; ");
        return visuals.Length >= 10 && responsive >= 10 && namedStates >= 10;
    }

    private static bool ProbeCharacterProceduralLimbEvidence(StringBuilder details)
    {
        var visuals = UnityEngine.Object.FindObjectsByType<TacticalCharacterMotionVisual>(FindObjectsInactive.Exclude);
        var rigged = 0;
        var animated = 0;
        foreach (var visual in visuals)
        {
            if (!visual.HasVisualRoot)
            {
                continue;
            }

            visual.ApplyPreviewState(TacticalCharacterVisualState.Walk, 0.9f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Aim, 0.35f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Fire, 0.35f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Down, 0f);
            if (visual.ProceduralLimbRigPartCount >= 12)
            {
                rigged++;
            }

            if (visual.ProceduralLimbAnimationEvidence && visual.LastLimbAnimationEvidence > 0.08f)
            {
                animated++;
            }
        }

        details.Append("characterProceduralLimb ").Append(animated).Append("/")
            .Append(visuals.Length).Append(" rigged ").Append(rigged).Append("; ");
        return visuals.Length >= 10 && rigged >= 10 && animated >= 10;
    }

    private static bool ProbeCharacterAuthoredClipEvidence(StringBuilder details)
    {
        var visuals = UnityEngine.Object.FindObjectsByType<TacticalCharacterClipVisual>(FindObjectsInactive.Exclude);
        var libraryReady = 0;
        var animated = 0;
        foreach (var visual in visuals)
        {
            visual.ApplyPreviewState(TacticalCharacterVisualState.Walk, 0.9f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Aim, 0.35f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Fire, 0.35f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Hit, 0.1f);
            visual.ApplyPreviewState(TacticalCharacterVisualState.Down, 0f);

            if (visual.HasAuthoredClipLibrary && visual.AuthoredClipCount >= 6 && visual.RigPartCount >= 12)
            {
                libraryReady++;
            }

            if (visual.AuthoredClipEvidence && visual.LastClipEvidence > 0.28f && visual.ClipStateChangeCount >= 3)
            {
                animated++;
            }
        }

        details.Append("characterAuthoredClip ").Append(animated).Append("/")
            .Append(visuals.Length).Append(" library ").Append(libraryReady).Append("; ");
        return visuals.Length >= 10 && libraryReady >= 10 && animated >= 10;
    }

    private static void Teleport(TacticalPlayerController player, Vector3 position)
    {
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

    private static TacticalWeaponState GetWeaponState(TacticalGameManager gm, string weaponId)
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

    private static float GetEnemyHealth(TacticalEnemy enemy)
    {
        var field = typeof(TacticalEnemy).GetField("health", BindingFlags.Instance | BindingFlags.NonPublic);
        return field == null ? -1f : (float)field.GetValue(enemy);
    }

    private static int CountObjects(string name)
    {
        var count = 0;
        foreach (var obj in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude))
        {
            if (obj.name == name)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountEnabledRenderers(GameObject root)
    {
        var count = 0;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.enabled)
            {
                count++;
            }
        }

        return count;
    }

    private static bool VisitZone(
        TacticalPlayerController player,
        Camera camera,
        TacticalCameraFollow follow,
        Vector3 position,
        float yaw,
        string label,
        string namePrefix,
        float radius,
        int minimumObjects,
        out int nearby)
    {
        Teleport(player, position);
        player.ResetView(yaw, 18f);
        player.SetCameraMode(TacticalCameraMode.ThirdPerson);
        player.SetAds(false);
        follow?.SnapToPlayer();
        Physics.SyncTransforms();
        nearby = CountNearbyNamed(namePrefix, position, radius);
        var cameraDistance = Vector3.Distance(player.transform.position, camera.transform.position);
        return nearby >= minimumObjects && cameraDistance < 14f && label.Length > 0;
    }

    private static int CountNearbyNamed(string namePrefix, Vector3 position, float radius)
    {
        var count = 0;
        foreach (var obj in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude))
        {
            if (obj.name.Contains(namePrefix, StringComparison.Ordinal)
                && Vector3.Distance(obj.transform.position, position) <= radius)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountNearby<T>(Vector3 position, float radius) where T : Component
    {
        var count = 0;
        foreach (var component in UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Exclude))
        {
            if (Vector3.Distance(component.transform.position, position) <= radius)
            {
                count++;
            }
        }

        return count;
    }

    private static T FindNamedObject<T>(string namePart) where T : Component
    {
        foreach (var component in UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Exclude))
        {
            if (component.name.Contains(namePart, StringComparison.Ordinal))
            {
                return component;
            }
        }

        return null;
    }

    private static void InvokePrivate(TacticalGameManager gm, string methodName)
    {
        typeof(TacticalGameManager).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(gm, null);
    }

    private static void InvokeEnemyUpdate(TacticalEnemy enemy)
    {
        typeof(TacticalEnemy).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(enemy, null);
    }

    private static T GetObject<T>(TacticalGameManager gm, string fieldName) where T : UnityEngine.Object
    {
        return typeof(TacticalGameManager).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(gm) as T;
    }

    private static int GetInt(TacticalGameManager gm, string fieldName)
    {
        return (int)(typeof(TacticalGameManager).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(gm) ?? 0);
    }

    private static void SetInt(TacticalGameManager gm, string fieldName, int value)
    {
        typeof(TacticalGameManager).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(gm, value);
    }

    private static float GetFloat(TacticalGameManager gm, string fieldName)
    {
        return (float)(typeof(TacticalGameManager).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(gm) ?? 0f);
    }

    private static void SetFloat(TacticalGameManager gm, string fieldName, float value)
    {
        typeof(TacticalGameManager).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(gm, value);
    }

    private static void SetEnemyFloat(TacticalEnemy enemy, string fieldName, float value)
    {
        typeof(TacticalEnemy).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(enemy, value);
    }

    private static void SetBool(object target, string fieldName, bool value)
    {
        target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
    }

    private static void SetString(TacticalGameManager gm, string fieldName, string value)
    {
        typeof(TacticalGameManager).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(gm, value);
    }

    private static string GetString(TacticalGameManager gm, string fieldName)
    {
        return (string)(typeof(TacticalGameManager).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(gm) ?? "");
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

    private static Transform FindDirectChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    private static bool ColorsClose(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.01f && Mathf.Abs(a.g - b.g) < 0.01f && Mathf.Abs(a.b - b.b) < 0.01f;
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

    private static string Escape(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
    }
}
#endif
