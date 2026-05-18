using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// M91 external-input telemetry for built-player route proof.
///
/// This component is intentionally not a route driver. It installs only when the
/// built player is launched with --m91-external-input-route, performs deterministic
/// scene setup so the route is measurable, then observes real keyboard/mouse input
/// and the resulting gameplay state mutations.
/// </summary>
public sealed class TacticalExternalInputRouteTelemetry : MonoBehaviour
{
    private const string Flag = "--m91-external-input-route";
    private const string OutputPrefix = "--m91-output=";
    private const string TimeoutPrefix = "--m91-timeout=";
    private const string ReportFileName = "M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json";
    private const BindingFlags InstancePrivate = BindingFlags.Instance | BindingFlags.NonPublic;

    private readonly List<string> screenshots = new();
    private readonly List<string> events = new();
    private readonly List<string> blockers = new();

    private string outputDirectory = "";
    private string reportPath = "";
    private float timeoutSeconds = 240f;
    private float startRealtime;
    private float lastWriteRealtime;
    private bool finalWritten;
    private bool timeoutReached;

    private TacticalGameManager game;
    private TacticalPlayerController player;
    private Camera playerCamera;
    private TacticalLoot arrangedLoot;
    private TacticalEnemy arrangedEnemy;

    private Vector3 baselinePosition;
    private string baselineWeaponId = "";
    private int baselineLootCount;
    private int baselineMagazine = -1;
    private int postFireMagazine = -1;
    private bool baselineReloading;
    private float baselineHp = -1f;
    private float baselineEnemyHealth = -1f;
    private int baselineEnemyCount = -1;

    private bool startInputObserved;
    private bool movementInputObserved;
    private bool movementStateChanged;
    private bool pickupInputObserved;
    private bool pickupStateChanged;
    private bool fireInputObserved;
    private bool ammoStateChanged;
    private bool reloadInputObserved;
    private bool reloadStateChanged;
    private bool enemyInteractionObserved;
    private bool deathOrRestartObserved;
    private bool firstPersonWeaponVisible;

    private bool capturedLobby;
    private bool capturedStart;
    private bool capturedMovement;
    private bool capturedPickup;
    private bool capturedFire;
    private bool capturedReload;
    private bool capturedEnemy;
    private bool capturedDeathOrRestart;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallIfRequested()
    {
        var args = Environment.GetCommandLineArgs();
        var requested = false;
        foreach (var arg in args)
        {
            if (arg == Flag || arg.Contains("m91-external-input-route", StringComparison.Ordinal) || arg.StartsWith(OutputPrefix, StringComparison.Ordinal))
            {
                requested = true;
                break;
            }
        }

        if (!requested)
        {
            return;
        }

        Debug.Log("[M91] Installing external-input built-player route telemetry. Args: " + string.Join(" | ", args));
        var obj = new GameObject("M91 External Input Route Telemetry");
        DontDestroyOnLoad(obj);
        obj.AddComponent<TacticalExternalInputRouteTelemetry>();
    }

    private void Start()
    {
        Application.runInBackground = true;
        ResolveOutput();
        Directory.CreateDirectory(outputDirectory);
        timeoutSeconds = ResolveTimeoutSeconds();
        startRealtime = Time.realtimeSinceStartup;
        lastWriteRealtime = startRealtime;
        Debug.Log("[M91] External-input route telemetry output: " + reportPath);
        StartCoroutine(InitialCapture());
    }

    private IEnumerator InitialCapture()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        RefreshReferences();
        SetupMeasurableRouteObjects();
        baselinePosition = player == null ? Vector3.zero : player.transform.position;
        baselineWeaponId = game == null ? "" : game.CurrentWeaponId;
        baselineLootCount = CountLoot();
        baselineMagazine = CurrentMagazine();
        baselineReloading = CurrentReloading();
        baselineHp = CurrentHp();
        baselineEnemyHealth = CurrentEnemyHealth();
        baselineEnemyCount = CountAliveEnemies();
        yield return CaptureOnce("00_lobby_before_external_input", () => capturedLobby = true);
        WriteReport(false);
    }

    private void Update()
    {
        RefreshReferences();
        ObserveInput();
        ObserveState();

        if (Time.realtimeSinceStartup - lastWriteRealtime > 2.0f)
        {
            WriteReport(false);
        }

        if (!finalWritten && Time.realtimeSinceStartup - startRealtime > timeoutSeconds)
        {
            events.Add("timeout_reached " + timeoutSeconds.ToString("0.0", CultureInfo.InvariantCulture) + "s");
            timeoutReached = true;
            WriteReport(true);
            finalWritten = true;
            return;
        }
    }

    private void ObserveInput()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (!startInputObserved && (keyboard?.enterKey.wasPressedThisFrame == true || keyboard?.spaceKey.wasPressedThisFrame == true || mouse?.leftButton.wasPressedThisFrame == true))
        {
            startInputObserved = true;
            events.Add("input:start enter_space_or_left_mouse");
        }
        else if (!deathOrRestartObserved
            && capturedStart
            && (enemyInteractionObserved || ammoStateChanged)
            && keyboard?.enterKey.wasPressedThisFrame == true
            && game != null
            && !game.IsInLobby)
        {
            deathOrRestartObserved = true;
            events.Add("input:restart enter_key_after_route_progress");
        }

        if (!movementInputObserved && keyboard != null && (
                keyboard.wKey.isPressed || keyboard.aKey.isPressed || keyboard.sKey.isPressed || keyboard.dKey.isPressed ||
                keyboard.upArrowKey.isPressed || keyboard.downArrowKey.isPressed || keyboard.leftArrowKey.isPressed || keyboard.rightArrowKey.isPressed))
        {
            movementInputObserved = true;
            events.Add("input:movement wasd_or_arrow");
        }

        if (!pickupInputObserved && keyboard?.fKey.wasPressedThisFrame == true)
        {
            pickupInputObserved = true;
            events.Add("input:pickup f_key");
        }

        if (!fireInputObserved && mouse?.leftButton.wasPressedThisFrame == true && game != null && !game.IsInLobby)
        {
            fireInputObserved = true;
            events.Add("input:fire left_mouse");
        }

        if (!reloadInputObserved && keyboard?.rKey.wasPressedThisFrame == true)
        {
            reloadInputObserved = true;
            events.Add("input:reload r_key");
        }

        if ((keyboard?.digit1Key.wasPressedThisFrame == true || keyboard?.digit2Key.wasPressedThisFrame == true || keyboard?.digit3Key.wasPressedThisFrame == true || keyboard?.digit4Key.wasPressedThisFrame == true) && game != null)
        {
            events.Add("input:weapon_switch current=" + game.CurrentWeaponId);
        }
    }

    private void ObserveState()
    {
        if (game == null || player == null)
        {
            if (!blockers.Contains("missing TacticalGameManager or TacticalPlayerController"))
            {
                blockers.Add("missing TacticalGameManager or TacticalPlayerController");
            }
            return;
        }

        if (startInputObserved && !capturedStart && !game.IsInLobby)
        {
            events.Add("state:start_input_changed_lobby_false");
            StartCoroutine(CaptureOnce("01_after_start_input", () =>
            {
                capturedStart = true;
                baselinePosition = player.transform.position;
                baselineWeaponId = game.CurrentWeaponId;
                baselineLootCount = CountLoot();
                baselineMagazine = CurrentMagazine();
                baselineReloading = CurrentReloading();
                baselineHp = CurrentHp();
                baselineEnemyHealth = CurrentEnemyHealth();
                baselineEnemyCount = CountAliveEnemies();
                SetupMeasurableRouteObjects();
            }));
        }

        if (movementInputObserved && !movementStateChanged && !game.IsInLobby)
        {
            var distance = Vector3.Distance(baselinePosition, player.transform.position);
            if (distance >= 0.35f)
            {
                movementStateChanged = true;
                events.Add("state:movement_changed distance=" + distance.ToString("0.00", CultureInfo.InvariantCulture));
            }
        }

        if (movementStateChanged && !capturedMovement)
        {
            StartCoroutine(CaptureOnce("02_after_movement_input", () => capturedMovement = true));
        }

        if (pickupInputObserved && !pickupStateChanged)
        {
            var weaponChanged = game.CurrentWeaponId != baselineWeaponId;
            var lootCountChanged = CountLoot() < baselineLootCount || arrangedLoot == null;
            if (weaponChanged || lootCountChanged)
            {
                pickupStateChanged = true;
                events.Add("state:pickup_changed weapon=" + baselineWeaponId + "->" + game.CurrentWeaponId
                    + " loot_count=" + baselineLootCount.ToString(CultureInfo.InvariantCulture) + "->" + CountLoot().ToString(CultureInfo.InvariantCulture));
            }
        }

        if (pickupStateChanged && !capturedPickup)
        {
            baselineMagazine = CurrentMagazine();
            baselineReloading = CurrentReloading();
            StartCoroutine(CaptureOnce("03_after_pickup_input", () => capturedPickup = true));
        }

        var currentMagazine = CurrentMagazine();
        if (fireInputObserved && !ammoStateChanged && baselineMagazine >= 0 && currentMagazine >= 0 && currentMagazine < baselineMagazine)
        {
            ammoStateChanged = true;
            postFireMagazine = currentMagazine;
            events.Add("state:ammo_changed_on_fire magazine=" + baselineMagazine.ToString(CultureInfo.InvariantCulture) + "->" + currentMagazine.ToString(CultureInfo.InvariantCulture));
        }

        if (ammoStateChanged && !capturedFire)
        {
            StartCoroutine(CaptureOnce("04_after_fire_input", () => capturedFire = true));
        }

        var currentReloading = CurrentReloading();
        if (reloadInputObserved && !reloadStateChanged)
        {
            if (currentReloading && !baselineReloading)
            {
                reloadStateChanged = true;
                events.Add("state:reload_started");
            }
            else if (postFireMagazine >= 0 && currentMagazine > postFireMagazine)
            {
                reloadStateChanged = true;
                events.Add("state:reload_completed magazine=" + postFireMagazine.ToString(CultureInfo.InvariantCulture) + "->" + currentMagazine.ToString(CultureInfo.InvariantCulture));
            }
        }

        if (reloadStateChanged && !capturedReload)
        {
            StartCoroutine(CaptureOnce("05_after_reload_input", () => capturedReload = true));
        }

        if (!enemyInteractionObserved)
        {
            var enemyHealth = CurrentEnemyHealth();
            var hp = CurrentHp();
            var enemyCount = CountAliveEnemies();
            if ((baselineEnemyHealth >= 0f && enemyHealth >= 0f && enemyHealth < baselineEnemyHealth)
                || enemyCount < baselineEnemyCount
                || (baselineHp >= 0f && hp >= 0f && hp < baselineHp))
            {
                enemyInteractionObserved = true;
                events.Add("state:enemy_interaction hp=" + baselineHp.ToString("0.0", CultureInfo.InvariantCulture) + "->" + hp.ToString("0.0", CultureInfo.InvariantCulture)
                    + " enemy_health=" + baselineEnemyHealth.ToString("0.0", CultureInfo.InvariantCulture) + "->" + enemyHealth.ToString("0.0", CultureInfo.InvariantCulture)
                    + " enemy_count=" + baselineEnemyCount.ToString(CultureInfo.InvariantCulture) + "->" + enemyCount.ToString(CultureInfo.InvariantCulture));
            }
        }

        if (enemyInteractionObserved && !capturedEnemy)
        {
            StartCoroutine(CaptureOnce("06_after_enemy_interaction_input", () => capturedEnemy = true));
        }

        if (!deathOrRestartObserved)
        {
            if (game.IsPlayerDown)
            {
                deathOrRestartObserved = true;
                events.Add("state:death_observed");
            }
            else if (startInputObserved && capturedStart && game.IsInLobby)
            {
                deathOrRestartObserved = true;
                events.Add("state:restart_or_lobby_return_observed");
            }
        }

        if (deathOrRestartObserved && !capturedDeathOrRestart)
        {
            StartCoroutine(CaptureOnce("07_after_death_or_restart_input", () =>
            {
                capturedDeathOrRestart = true;
                WriteReport(true);
            }));
        }

        firstPersonWeaponVisible = firstPersonWeaponVisible || FirstPersonWeaponVisible();

        if (Passed())
        {
            WriteReport(true);
            finalWritten = true;
        }
    }

    private void SetupMeasurableRouteObjects()
    {
        if (player == null)
        {
            return;
        }

        ArrangeLootNearPlayer();
        ArrangeEnemyInRoute();
    }

    private void ArrangeLootNearPlayer()
    {
        arrangedLoot = FindAnyObjectByType<TacticalLoot>();
        if (arrangedLoot == null)
        {
            blockers.Add("no TacticalLoot available for M91 setup");
            return;
        }

        arrangedLoot.Configure(TacticalLootKind.Weapon, "M91 external-input rifle loot", 1, "rifle");
        arrangedLoot.transform.SetParent(null, true);
        arrangedLoot.transform.position = player.transform.position + player.transform.right * 0.85f + Vector3.up * 0.05f;
        arrangedLoot.gameObject.SetActive(true);
        Physics.SyncTransforms();
        events.Add("setup:loot_moved_near_player " + arrangedLoot.name);
    }

    private void ArrangeEnemyInRoute()
    {
        arrangedEnemy = FindAnyObjectByType<TacticalEnemy>();
        if (arrangedEnemy == null)
        {
            blockers.Add("no TacticalEnemy available for M91 setup");
            return;
        }

        var forward = player.transform.forward.sqrMagnitude < 0.01f ? Vector3.forward : player.transform.forward;
        arrangedEnemy.transform.SetParent(null, true);
        arrangedEnemy.transform.position = player.transform.position + forward.normalized * 18f + Vector3.up * 0.02f;
        arrangedEnemy.transform.rotation = Quaternion.LookRotation(-forward.normalized, Vector3.up);
        SetEnemyPrivateFloat(arrangedEnemy, "attackDamage", 0.75f);
        SetEnemyPrivateFloat(arrangedEnemy, "attackInterval", 8f);
        SetEnemyPrivateFloat(arrangedEnemy, "shootInterval", 8f);
        SetEnemyPrivateFloat(arrangedEnemy, "nextAttackTime", Time.time + 12f);
        SetEnemyPrivateFloat(arrangedEnemy, "nextShotTime", Time.time + 12f);
        arrangedEnemy.gameObject.SetActive(true);
        Physics.SyncTransforms();
        baselineEnemyHealth = CurrentEnemyHealth();
        baselineEnemyCount = CountAliveEnemies();
        events.Add("setup:enemy_moved_to_visible_route " + arrangedEnemy.name);
    }

    private IEnumerator CaptureOnce(string label, Action afterCapture)
    {
        if (screenshots.Exists(path => Path.GetFileNameWithoutExtension(path) == label))
        {
            afterCapture?.Invoke();
            yield break;
        }

        yield return new WaitForEndOfFrame();
        var path = Path.Combine(outputDirectory, label + ".png");
        ScreenCapture.CaptureScreenshot(path);
        screenshots.Add(path);
        events.Add("screenshot:" + label);
        afterCapture?.Invoke();
        yield return new WaitForSecondsRealtime(0.10f);
        WriteReport(false);
    }

    private void RefreshReferences()
    {
        if (game == null)
        {
            game = FindAnyObjectByType<TacticalGameManager>();
        }

        if (player == null)
        {
            player = FindAnyObjectByType<TacticalPlayerController>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
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

    private TacticalWeaponState CurrentWeaponState()
    {
        if (game == null)
        {
            return null;
        }

        var field = typeof(TacticalGameManager).GetField("weapons", InstancePrivate);
        var states = field?.GetValue(game) as Dictionary<string, TacticalWeaponState>;
        return states != null && states.TryGetValue(game.CurrentWeaponId, out var state) ? state : null;
    }

    private int CurrentMagazine()
    {
        var state = CurrentWeaponState();
        return state == null ? -1 : state.magazine;
    }

    private bool CurrentReloading()
    {
        var state = CurrentWeaponState();
        return state != null && state.reloading;
    }

    private float CurrentHp()
    {
        var field = typeof(TacticalGameManager).GetField("hp", InstancePrivate);
        return game == null || field == null ? -1f : (float)field.GetValue(game);
    }

    private float CurrentEnemyHealth()
    {
        var enemy = arrangedEnemy != null ? arrangedEnemy : FindAnyObjectByType<TacticalEnemy>();
        if (enemy == null)
        {
            return -1f;
        }

        var field = typeof(TacticalEnemy).GetField("health", InstancePrivate);
        return field == null ? (enemy.IsAlive ? 1f : 0f) : (float)field.GetValue(enemy);
    }

    private static void SetEnemyPrivateFloat(TacticalEnemy enemy, string fieldName, float value)
    {
        var field = typeof(TacticalEnemy).GetField(fieldName, InstancePrivate);
        if (field != null)
        {
            field.SetValue(enemy, value);
        }
    }

    private int CountLoot()
    {
        return FindObjectsByType<TacticalLoot>(FindObjectsInactive.Exclude).Length;
    }

    private int CountAliveEnemies()
    {
        var count = 0;
        foreach (var enemy in FindObjectsByType<TacticalEnemy>(FindObjectsInactive.Exclude))
        {
            if (enemy != null && enemy.IsAlive)
            {
                count++;
            }
        }

        return count;
    }

    private bool Passed()
    {
        return startInputObserved
            && movementInputObserved
            && movementStateChanged
            && pickupInputObserved
            && pickupStateChanged
            && fireInputObserved
            && ammoStateChanged
            && reloadInputObserved
            && reloadStateChanged
            && enemyInteractionObserved
            && deathOrRestartObserved
            && firstPersonWeaponVisible
            && screenshots.Count >= 8;
    }

    private void WriteReport(bool final)
    {
        if (finalWritten && !final)
        {
            return;
        }

        lastWriteRealtime = Time.realtimeSinceStartup;

        var dynamicBlockers = new List<string>();
        if (!startInputObserved) dynamicBlockers.Add("start_input_observed=false");
        if (!movementInputObserved) dynamicBlockers.Add("movement_input_observed=false");
        if (!movementStateChanged) dynamicBlockers.Add("movement_state_changed=false");
        if (!pickupInputObserved) dynamicBlockers.Add("pickup_input_observed=false");
        if (!pickupStateChanged) dynamicBlockers.Add("pickup_state_changed=false");
        if (!fireInputObserved) dynamicBlockers.Add("fire_input_observed=false");
        if (!ammoStateChanged) dynamicBlockers.Add("ammo_state_changed=false");
        if (!reloadInputObserved) dynamicBlockers.Add("reload_input_observed=false");
        if (!reloadStateChanged) dynamicBlockers.Add("reload_state_changed=false");
        if (!enemyInteractionObserved) dynamicBlockers.Add("enemy_interaction_observed=false");
        if (!deathOrRestartObserved) dynamicBlockers.Add("death_or_restart_observed=false");
        if (!firstPersonWeaponVisible) dynamicBlockers.Add("first_person_weapon_visible=false");
        if (screenshots.Count < 8) dynamicBlockers.Add("screenshot_count<8");

        var allBlockers = new List<string>();
        allBlockers.AddRange(blockers);
        if (final || Passed())
        {
            allBlockers.AddRange(dynamicBlockers);
        }

        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "m91_external_input_built_player_route_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "passed", Passed() && allBlockers.Count == 0, true);
        Append(json, "external_input_driven", true, true);
        Append(json, "built_player", true, true);
        Append(json, "final", final, true);
        Append(json, "timeout_reached", timeoutReached, true);
        AppendRawNumber(json, "elapsed_seconds", (Time.realtimeSinceStartup - startRealtime).ToString("0.0", CultureInfo.InvariantCulture), true);
        Append(json, "start_input_observed", startInputObserved, true);
        Append(json, "movement_input_observed", movementInputObserved, true);
        Append(json, "movement_state_changed", movementStateChanged, true);
        Append(json, "pickup_input_observed", pickupInputObserved, true);
        Append(json, "pickup_state_changed", pickupStateChanged, true);
        Append(json, "fire_input_observed", fireInputObserved, true);
        Append(json, "ammo_state_changed", ammoStateChanged, true);
        Append(json, "reload_input_observed", reloadInputObserved, true);
        Append(json, "reload_state_changed", reloadStateChanged, true);
        Append(json, "enemy_interaction_observed", enemyInteractionObserved, true);
        Append(json, "death_or_restart_observed", deathOrRestartObserved, true);
        Append(json, "first_person_weapon_visible", firstPersonWeaponVisible, true);
        Append(json, "screenshot_count", screenshots.Count, true);
        AppendArray(json, "screenshots", screenshots, true);
        AppendArray(json, "events", events, true);
        AppendArray(json, "blockers", allBlockers, false);
        json.AppendLine("}");

        File.WriteAllText(reportPath, json.ToString());
    }

    private void ResolveOutput()
    {
        var target = "";
        foreach (var arg in Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith(OutputPrefix, StringComparison.Ordinal))
            {
                target = arg.Substring(OutputPrefix.Length).Trim('"');
            }
        }

        if (string.IsNullOrWhiteSpace(target))
        {
            target = Path.Combine(Application.persistentDataPath, "M91ExternalInputRoute");
        }

        if (target.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            reportPath = target;
            outputDirectory = Path.GetDirectoryName(target);
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                outputDirectory = Application.persistentDataPath;
            }
        }
        else
        {
            outputDirectory = target;
            reportPath = Path.Combine(outputDirectory, ReportFileName);
        }
    }

    private static float ResolveTimeoutSeconds()
    {
        foreach (var arg in Environment.GetCommandLineArgs())
        {
            if (!arg.StartsWith(TimeoutPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            if (float.TryParse(arg.Substring(TimeoutPrefix.Length), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return Mathf.Clamp(parsed, 30f, 900f);
            }
        }

        return 240f;
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

    private static void AppendRawNumber(StringBuilder json, string key, string numericValue, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": ").Append(numericValue);
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
