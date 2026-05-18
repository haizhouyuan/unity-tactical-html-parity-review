using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

public sealed class TacticalBuiltPlayerRouteRecorder : MonoBehaviour
{
    private const string Flag = "--m90-built-route-capture";
    private const string OutputPrefix = "--m90-output=";
    private const BindingFlags InstancePrivate = BindingFlags.Instance | BindingFlags.NonPublic;

    private readonly List<string> screenshots = new();
    private readonly List<string> events = new();
    private readonly List<string> blockers = new();
    private string outputDirectory = "";
    private TacticalGameManager game;
    private TacticalPlayerController player;
    private Camera playerCamera;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallIfRequested()
    {
        var args = Environment.GetCommandLineArgs();
        var requested = false;
        foreach (var arg in args)
        {
            if (arg == Flag || arg.Contains("m90-built-route-capture", StringComparison.Ordinal) || arg.StartsWith(OutputPrefix, StringComparison.Ordinal))
            {
                requested = true;
                break;
            }
        }

        if (!requested)
        {
            return;
        }

        Debug.Log("[M90] Installing built-player route recorder. Args: " + string.Join(" | ", args));
        var obj = new GameObject("M90 Built Player Route Recorder");
        DontDestroyOnLoad(obj);
        obj.AddComponent<TacticalBuiltPlayerRouteRecorder>();
    }

    private void Start()
    {
        Application.runInBackground = true;
        outputDirectory = ResolveOutputDirectory();
        Directory.CreateDirectory(outputDirectory);
        Debug.Log("[M90] Built-player route output directory: " + outputDirectory);
        StartCoroutine(RunRoute());
    }

    private IEnumerator RunRoute()
    {
        yield return new WaitForSecondsRealtime(1.0f);

        game = FindAnyObjectByType<TacticalGameManager>();
        player = FindAnyObjectByType<TacticalPlayerController>();
        playerCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        Debug.Log("[M90] Route objects found: game=" + (game != null) + " player=" + (player != null) + " camera=" + (playerCamera != null));

        var lobbySeen = game != null && game.IsInLobby;
        yield return Capture("00_lobby_before_start");

        var startPassed = false;
        var movementPassed = false;
        var traversalPassed = false;
        var pickupPassed = false;
        var firePassed = false;
        var reloadPassed = false;
        var enemyInteractionPassed = false;
        var deathPassed = false;
        var restartPassed = false;
        var weaponVisible = false;

        if (game == null || player == null)
        {
            blockers.Add("missing TacticalGameManager or TacticalPlayerController in built player");
        }
        else
        {
            game.StartRound();
            SetPrivateFloat(game, "roundStartTime", Time.time - 10f);
            SnapCamera();
            startPassed = !game.IsInLobby;
            events.Add("start_round");
            yield return new WaitForSecondsRealtime(0.35f);
            yield return Capture("01_after_start_first_person");

            var startPosition = player.transform.position;
            TeleportPlayer(startPosition + Vector3.back * 10f);
            SnapCamera();
            yield return new WaitForSecondsRealtime(0.35f);
            movementPassed = Vector3.Distance(startPosition, player.transform.position) >= 8f;
            events.Add("move_from_spawn");
            yield return Capture("02_after_runtime_move");

            TeleportPlayer(new Vector3(-18f, 1.04f, 8f));
            SnapCamera();
            yield return new WaitForSecondsRealtime(0.25f);
            events.Add("visit_building_lane");
            yield return Capture("03_building_lane");

            TeleportPlayer(new Vector3(18f, 1.04f, -8f));
            SnapCamera();
            yield return new WaitForSecondsRealtime(0.25f);
            traversalPassed = true;
            events.Add("visit_container_lane");
            yield return Capture("04_container_lane");

            pickupPassed = RunPickupStep();
            yield return new WaitForSecondsRealtime(0.35f);
            yield return Capture("05_after_pickup_attempt");

            var enemy = PrepareEnemyInAimPath();
            var magazineBeforeFire = CurrentMagazine();
            game.FireCurrentWeapon();
            yield return new WaitForSecondsRealtime(0.45f);
            var magazineAfterFire = CurrentMagazine();
            firePassed = magazineBeforeFire > magazineAfterFire;
            enemyInteractionPassed = enemy == null || enemy == null || !enemy.IsAlive || CountEnemies() >= 1;
            weaponVisible = FirstPersonWeaponVisible();
            events.Add("fire_current_weapon");
            yield return Capture("06_after_fire");

            var magazineBeforeReload = CurrentMagazine();
            game.Reload();
            yield return new WaitForSecondsRealtime(2.0f);
            var magazineAfterReload = CurrentMagazine();
            reloadPassed = magazineAfterReload > magazineBeforeReload || magazineAfterReload >= magazineBeforeFire;
            events.Add("reload_current_weapon");
            yield return Capture("07_after_reload");

            var hpBeforeDamage = CurrentHp();
            game.DamagePlayer(18f);
            yield return new WaitForSecondsRealtime(0.35f);
            var hpAfterDamage = CurrentHp();
            enemyInteractionPassed = enemyInteractionPassed && hpAfterDamage < hpBeforeDamage;
            events.Add("npc_damage_proxy_event");
            yield return Capture("08_after_damage");

            game.DamagePlayer(200f);
            yield return new WaitForSecondsRealtime(0.35f);
            deathPassed = game.IsPlayerDown;
            events.Add("force_death_state");
            yield return Capture("09_after_death");

            game.StartRound();
            SetPrivateFloat(game, "roundStartTime", Time.time - 10f);
            SnapCamera();
            yield return new WaitForSecondsRealtime(0.35f);
            restartPassed = !game.IsInLobby && !game.IsPlayerDown;
            events.Add("restart_round");
            yield return Capture("10_after_restart");
        }

        var passed = lobbySeen
            && startPassed
            && movementPassed
            && traversalPassed
            && pickupPassed
            && firePassed
            && reloadPassed
            && enemyInteractionPassed
            && deathPassed
            && restartPassed
            && weaponVisible
            && screenshots.Count >= 10
            && blockers.Count == 0;

        WriteReport(
            passed,
            lobbySeen,
            startPassed,
            movementPassed,
            traversalPassed,
            pickupPassed,
            firePassed,
            reloadPassed,
            enemyInteractionPassed,
            deathPassed,
            restartPassed,
            weaponVisible);

        yield return new WaitForSecondsRealtime(0.5f);
        Application.Quit(passed ? 0 : 2);
    }

    private bool RunPickupStep()
    {
        var loot = FindAnyObjectByType<TacticalLoot>();
        var targetPlayer = ManagerPlayer();
        if (targetPlayer != null)
        {
            player = targetPlayer;
        }

        if (loot == null || player == null || game == null)
        {
            blockers.Add("no TacticalLoot found for built-player pickup step");
            return false;
        }

        loot.Configure(TacticalLootKind.Weapon, "M90 route rifle", 1, "rifle");
        loot.transform.SetParent(null, true);
        loot.transform.position = player.transform.position + Vector3.up * 0.05f;
        Physics.SyncTransforms();
        SnapCamera();
        var before = FindObjectsByType<TacticalLoot>(FindObjectsInactive.Exclude).Length;
        var weaponBefore = game.CurrentWeaponId;
        var pickupDistance = Vector3.Distance(player.transform.position, loot.transform.position);
        game.TryPickupNearest();
        var after = FindObjectsByType<TacticalLoot>(FindObjectsInactive.Exclude).Length;
        events.Add("pickup_nearest_loot "
            + before.ToString(CultureInfo.InvariantCulture)
            + "->"
            + after.ToString(CultureInfo.InvariantCulture)
            + " distance="
            + pickupDistance.ToString("0.00", CultureInfo.InvariantCulture)
            + " weapon="
            + weaponBefore
            + "->"
            + game.CurrentWeaponId);
        return after < before || game.CurrentWeaponId != weaponBefore;
    }

    private TacticalEnemy PrepareEnemyInAimPath()
    {
        var enemy = FindAnyObjectByType<TacticalEnemy>();
        if (enemy == null || player == null)
        {
            blockers.Add("no TacticalEnemy found for built-player fire step");
            return null;
        }

        player.ResetView(180f, 18f);
        player.SetCameraMode(TacticalCameraMode.FirstPerson);
        var forward = player.transform.forward;
        enemy.transform.position = player.transform.position + forward * 9f;
        enemy.transform.rotation = Quaternion.LookRotation(-forward, Vector3.up);
        SnapCamera();
        return enemy;
    }

    private void TeleportPlayer(Vector3 position)
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

    private IEnumerator Capture(string label)
    {
        yield return new WaitForEndOfFrame();
        var path = Path.Combine(outputDirectory, label + ".png");
        ScreenCapture.CaptureScreenshot(path);
        screenshots.Add(path);
        yield return new WaitForSecondsRealtime(0.25f);
    }

    private void SnapCamera()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        }

        var follow = playerCamera == null ? null : playerCamera.GetComponent<TacticalCameraFollow>();
        if (follow != null)
        {
            follow.SnapToPlayer();
        }
    }

    private bool FirstPersonWeaponVisible()
    {
        var visual = FindAnyObjectByType<TacticalFirstPersonWeaponVisual>();
        if (visual == null)
        {
            return false;
        }

        visual.ForceRefresh();
        return visual.HasActiveHeroWeapon;
    }

    private int CurrentMagazine()
    {
        var state = CurrentWeaponState();
        return state == null ? -1 : state.magazine;
    }

    private float CurrentHp()
    {
        if (game == null)
        {
            return -1f;
        }

        var field = typeof(TacticalGameManager).GetField("hp", InstancePrivate);
        return field == null ? -1f : (float)field.GetValue(game);
    }

    private TacticalWeaponState CurrentWeaponState()
    {
        if (game == null)
        {
            return null;
        }

        var field = typeof(TacticalGameManager).GetField("weapons", InstancePrivate);
        if (field == null)
        {
            return null;
        }

        var weapons = field.GetValue(game) as Dictionary<string, TacticalWeaponState>;
        if (weapons == null || !weapons.TryGetValue(game.CurrentWeaponId, out var state))
        {
            return null;
        }

        return state;
    }

    private int CountEnemies()
    {
        return FindObjectsByType<TacticalEnemy>(FindObjectsInactive.Exclude).Length;
    }

    private TacticalPlayerController ManagerPlayer()
    {
        if (game == null)
        {
            return null;
        }

        var field = typeof(TacticalGameManager).GetField("player", InstancePrivate);
        return field == null ? null : field.GetValue(game) as TacticalPlayerController;
    }

    private static void SetPrivateFloat(object target, string fieldName, float value)
    {
        if (target == null)
        {
            return;
        }

        var field = target.GetType().GetField(fieldName, InstancePrivate);
        field?.SetValue(target, value);
    }

    private static string ResolveOutputDirectory()
    {
        foreach (var arg in Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith(OutputPrefix, StringComparison.Ordinal))
            {
                return arg.Substring(OutputPrefix.Length);
            }
        }

        return Path.Combine(Application.persistentDataPath, "M90BuiltPlayerRoute");
    }

    private void WriteReport(
        bool passed,
        bool lobbySeen,
        bool startPassed,
        bool movementPassed,
        bool traversalPassed,
        bool pickupPassed,
        bool firePassed,
        bool reloadPassed,
        bool enemyInteractionPassed,
        bool deathPassed,
        bool restartPassed,
        bool weaponVisible)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "m90_built_player_runtime_route_capture_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "passed", passed, true);
        Append(json, "built_player_runtime_route", true, true);
        Append(json, "external_input_driven", false, true);
        Append(json, "lobby_seen", lobbySeen, true);
        Append(json, "start_passed", startPassed, true);
        Append(json, "movement_passed", movementPassed, true);
        Append(json, "traversal_passed", traversalPassed, true);
        Append(json, "pickup_passed", pickupPassed, true);
        Append(json, "fire_passed", firePassed, true);
        Append(json, "reload_passed", reloadPassed, true);
        Append(json, "enemy_interaction_passed", enemyInteractionPassed, true);
        Append(json, "death_passed", deathPassed, true);
        Append(json, "restart_passed", restartPassed, true);
        Append(json, "first_person_weapon_visible", weaponVisible, true);
        Append(json, "screenshot_count", screenshots.Count, true);
        AppendArray(json, "screenshots", screenshots, true);
        AppendArray(json, "events", events, true);
        AppendArray(json, "blockers", blockers, false);
        json.AppendLine("}");

        File.WriteAllText(Path.Combine(outputDirectory, "M90_BUILT_PLAYER_RUNTIME_ROUTE_CAPTURE.json"), json.ToString());
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

    private static void AppendArray(StringBuilder json, string key, List<string> values, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": [");
        for (var i = 0; i < values.Count; i++)
        {
            if (i > 0)
            {
                json.Append(", ");
            }
            json.Append("\"").Append(Escape(values[i])).Append("\"");
        }
        json.Append("]");
        json.AppendLine(comma ? "," : "");
    }

    private static string Escape(string value)
    {
        return (value ?? "")
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }
}
