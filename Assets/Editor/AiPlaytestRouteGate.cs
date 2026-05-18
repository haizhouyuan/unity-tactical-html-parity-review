#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class AiPlaytestRouteGate
{
    private const string ReportPath = "docs/AI_PLAYTEST_ROUTE_GATE.json";
    private const string RouteReportPath = "docs/TACTICAL_PLAYABLE_ROUTE_GATE.json";
    private const string GameplayReportPath = "docs/TACTICAL_GAMEPLAY_PROOF_GATE.json";
    private const string BuildingReportPath = "docs/BUILDING_INTEGRITY_GATE.json";
    private const string WeaponReportPath = "docs/WEAPON_FEEL_GATE.json";
    private const string PlayerPovReportPath = "docs/TACTICAL_PLAYER_POV_GATE.json";

    [MenuItem("AI Tools/Run AI Playtest Route Gate")]
    public static void WriteReport()
    {
        Directory.CreateDirectory("docs");

        var routeJson = ReadText(RouteReportPath);
        var gameplayJson = ReadText(GameplayReportPath);
        var buildingJson = ReadText(BuildingReportPath);
        var weaponJson = ReadText(WeaponReportPath);
        var playerPovJson = ReadText(PlayerPovReportPath);

        var routePassed = ExtractBool(routeJson, "passed");
        var gameplayPassed = ExtractBool(gameplayJson, "passed");
        var buildingPassed = ExtractBool(buildingJson, "passed");
        var weaponPassed = ExtractBool(weaponJson, "passed");
        var playerPovPassed = ExtractBool(playerPovJson, "passed");

        var startPassed = ExtractBool(routeJson, "start_clicked") && ExtractBool(routeJson, "spawn_visited");
        var movementPassed = ExtractBool(routeJson, "moved_from_spawn") && ExtractBool(routeJson, "player_camera_evidence");
        var traversalPassed = ExtractBool(routeJson, "building_visited")
            && ExtractBool(routeJson, "warehouse_visited")
            && ExtractBool(routeJson, "container_visited")
            && ExtractBool(routeJson, "ladder_floor_mutation")
            && buildingPassed;
        var lootPassed = ExtractBool(routeJson, "pickup_state_mutation")
            && ExtractBool(routeJson, "approved_loot_class_route_evidence");
        var combatPassed = ExtractBool(routeJson, "fire_state_mutation")
            && ExtractBool(routeJson, "enemy_ranged_attack_mutation")
            && ExtractBool(gameplayJson, "fire_ammo_and_enemy_hit")
            && ExtractBool(gameplayJson, "enemy_ranged_attack_state_mutation")
            && weaponPassed;
        var recoveryPassed = ExtractBool(routeJson, "reload_state_mutation")
            || ExtractBool(gameplayJson, "reload_state_mutation")
            || ExtractBool(weaponJson, "reload_state_mutation");
        var deathRestartPassed = ExtractBool(routeJson, "death_overlay_mutation")
            && ExtractBool(routeJson, "restart_mutation")
            && ExtractBool(gameplayJson, "death_overlay_and_restart");
        var noStuckDetected = movementPassed && traversalPassed && ExtractBool(routeJson, "gameplay_route_passed");
        var screenshotEvidence = ExtractInt(routeJson, "screenshot_count") >= 9
            && ExtractInt(weaponJson, "screenshot_count") >= 4
            && playerPovPassed;

        var passed = routePassed
            && gameplayPassed
            && buildingPassed
            && weaponPassed
            && playerPovPassed
            && startPassed
            && movementPassed
            && traversalPassed
            && lootPassed
            && combatPassed
            && recoveryPassed
            && deathRestartPassed
            && noStuckDetected
            && screenshotEvidence;

        var blockers = BuildBlockerList(
            ("route", routePassed),
            ("gameplay", gameplayPassed),
            ("building", buildingPassed),
            ("weapon", weaponPassed),
            ("player_pov", playerPovPassed),
            ("start", startPassed),
            ("movement", movementPassed),
            ("traversal", traversalPassed),
            ("loot", lootPassed),
            ("combat", combatPassed),
            ("reload_or_recovery", recoveryPassed),
            ("death_restart", deathRestartPassed),
            ("no_stuck", noStuckDetected),
            ("screenshot_evidence", screenshotEvidence));

        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "ai_playtest_route_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "application_is_playing", Application.isPlaying, true);
        Append(json, "passed", passed, true);
        Append(json, "route_gate_passed", routePassed, true);
        Append(json, "gameplay_gate_passed", gameplayPassed, true);
        Append(json, "building_gate_passed", buildingPassed, true);
        Append(json, "weapon_gate_passed", weaponPassed, true);
        Append(json, "player_pov_gate_passed", playerPovPassed, true);
        Append(json, "start_passed", startPassed, true);
        Append(json, "movement_passed", movementPassed, true);
        Append(json, "traversal_passed", traversalPassed, true);
        Append(json, "loot_passed", lootPassed, true);
        Append(json, "combat_passed", combatPassed, true);
        Append(json, "reload_or_recovery_passed", recoveryPassed, true);
        Append(json, "death_restart_passed", deathRestartPassed, true);
        Append(json, "no_stuck_detected", noStuckDetected, true);
        Append(json, "screenshot_evidence", screenshotEvidence, true);
        Append(json, "route_report_path", RouteReportPath, true);
        Append(json, "gameplay_report_path", GameplayReportPath, true);
        Append(json, "building_report_path", BuildingReportPath, true);
        Append(json, "weapon_report_path", WeaponReportPath, true);
        Append(json, "player_pov_report_path", PlayerPovReportPath, true);
        Append(json, "blockers", blockers, false);
        json.AppendLine("}");

        File.WriteAllText(ReportPath, json.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] AI playtest route gate report written to " + ReportPath + " passed=" + passed);
    }

    private static string BuildBlockerList(params (string name, bool passed)[] checks)
    {
        var result = new StringBuilder();
        foreach (var check in checks)
        {
            if (check.passed)
            {
                continue;
            }

            if (result.Length > 0)
            {
                result.Append(", ");
            }
            result.Append(check.name);
        }
        return result.Length == 0 ? "none" : result.ToString();
    }

    private static string ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    private static bool ExtractBool(string json, string key)
    {
        return Regex.IsMatch(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*true");
    }

    private static int ExtractInt(string json, string key)
    {
        var match = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*(\\d+)");
        return match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;
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
        return (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
#endif
