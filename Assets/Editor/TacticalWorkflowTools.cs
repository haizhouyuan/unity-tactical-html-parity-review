#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Deterministic workflow reports for Codex/MCP orchestration.
/// These commands do not mutate gameplay assets or promote candidates.
/// </summary>
public static class TacticalWorkflowTools
{
    private const string TacticalScenePath = "Assets/Scenes/TacticalPrototype.unity";
    private const string HtmlBaselinePath = "reference/html_baseline_final_packet/index.html";
    private const string PreflightReportPath = "docs/TACTICAL_PREFLIGHT_REPORT.json";
    private const string McpSmokeReportPath = "docs/UNITY_MCP_SMOKE_REPORT_LATEST.json";
    private const string GameFeelReportPath = "docs/GAME_FEEL_EVIDENCE_GATE.json";
    private const string AcceptanceReportPath = "docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json";
    private const string GameplayReportPath = "docs/TACTICAL_GAMEPLAY_PROOF_GATE.json";
    private const string RouteReportPath = "docs/TACTICAL_PLAYABLE_ROUTE_GATE.json";
    private const string PlayerPovReportPath = "docs/TACTICAL_PLAYER_POV_GATE.json";
    private const string AssetLedgerPath = "docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json";
    private const string PromotedVisibilityPath = "docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json";
    private const string ManifestPath = "Packages/manifest.json";

    [MenuItem("AI Tools/Run Tactical Preflight")]
    public static void RunTacticalPreflight()
    {
        Directory.CreateDirectory("docs");
        var console = GetConsoleCounts();
        var sceneExists = File.Exists(TacticalScenePath);
        var baselineExists = File.Exists(HtmlBaselinePath);
        var manifest = ReadText(ManifestPath);
        var acceptance = ReadText(AcceptanceReportPath);
        var ledger = ReadText(AssetLedgerPath);
        var visibility = ReadText(PromotedVisibilityPath);
        var passed = sceneExists
            && baselineExists
            && !EditorApplication.isCompiling
            && !EditorApplication.isUpdating
            && console.errors == 0;

        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "tactical_preflight_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "passed", passed, true);
        Append(json, "active_scene", EditorSceneManager.GetActiveScene().path, true);
        Append(json, "tactical_scene_exists", sceneExists, true);
        Append(json, "html_baseline_exists", baselineExists, true);
        Append(json, "editor_is_compiling", EditorApplication.isCompiling, true);
        Append(json, "editor_is_updating", EditorApplication.isUpdating, true);
        Append(json, "application_is_playing", Application.isPlaying, true);
        Append(json, "console_errors", console.errors, true);
        Append(json, "console_warnings", console.warnings, true);
        Append(json, "community_mcp_package_present", manifest.Contains("com.coplaydev.unity-mcp"), true);
        Append(json, "official_unity_ai_assistant_present", manifest.Contains("com.unity.ai.assistant"), true);
        Append(json, "acceptance_report_exists", File.Exists(AcceptanceReportPath), true);
        Append(json, "latest_all_required_current_gates_passed", ExtractBool(acceptance, "all_required_current_gates_passed"), true);
        Append(json, "latest_full_visual_asset_gate_passed", ExtractBool(acceptance, "full_visual_asset_gate_passed"), true);
        Append(json, "latest_asset_gameplay_production_promoted_assets", ExtractNestedInt(ledger, "summary", "production_promoted_assets"), true);
        Append(json, "latest_visible_promoted_classes", ExtractNestedInt(visibility, "summary", "visible_promoted_classes"), true);
        Append(json, "note", "Preflight checks editor readiness and report state. It does not prove gameplay completion.", false);
        json.AppendLine("}");

        File.WriteAllText(PreflightReportPath, json.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] Tactical preflight report written to " + PreflightReportPath + " passed=" + passed);
    }

    [MenuItem("AI Tools/Run Unity MCP Smoke Check")]
    public static void RunUnityMcpSmokeCheck()
    {
        Directory.CreateDirectory("docs");
        var console = GetConsoleCounts();
        var manifest = ReadText(ManifestPath);
        var communityPackage = manifest.Contains("com.coplaydev.unity-mcp");
        var passed = communityPackage && !EditorApplication.isCompiling && !EditorApplication.isUpdating && console.errors == 0;

        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "unity_mcp_smoke_report_v2", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "passed", passed, true);
        Append(json, "transport_checked_by_this_command", false, true);
        Append(json, "transport_note", "This in-Editor command verifies project/editor readiness. MCP transport is proven only when an external client invokes this command and reads this report.", true);
        Append(json, "community_mcp_package_present", communityPackage, true);
        Append(json, "expected_http_endpoint", "http://127.0.0.1:8080/mcp", true);
        Append(json, "editor_is_compiling", EditorApplication.isCompiling, true);
        Append(json, "editor_is_updating", EditorApplication.isUpdating, true);
        Append(json, "application_is_playing", Application.isPlaying, true);
        Append(json, "console_errors", console.errors, true);
        Append(json, "console_warnings", console.warnings, true);
        Append(json, "active_scene", EditorSceneManager.GetActiveScene().path, false);
        json.AppendLine("}");

        File.WriteAllText(McpSmokeReportPath, json.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] Unity MCP smoke report written to " + McpSmokeReportPath + " passed=" + passed);
    }

    [MenuItem("AI Tools/Run Game Feel Evidence Gate")]
    public static void RunGameFeelEvidenceGate()
    {
        Directory.CreateDirectory("docs");
        if (Application.isPlaying)
        {
            TacticalGameplayProofGate.WriteReport();
        }

        var gameplay = ReadText(GameplayReportPath);
        var route = ReadText(RouteReportPath);
        var passed = Application.isPlaying
            && ExtractBool(gameplay, "passed")
            && ExtractBool(gameplay, "manual_start_first_person_weapon_visible")
            && ExtractBool(gameplay, "fire_ammo_and_enemy_hit")
            && ExtractBool(gameplay, "reload_state_mutation")
            && ExtractBool(gameplay, "weapon_feedback_spawned")
            && ExtractBool(gameplay, "first_person_weapon_polish")
            && ExtractBool(route, "spawn_first_person_weapon_visible")
            && ExtractBool(route, "fire_state_mutation");

        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "game_feel_evidence_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "passed", passed, true);
        Append(json, "application_is_playing", Application.isPlaying, true);
        Append(json, "gameplay_report_path", GameplayReportPath, true);
        Append(json, "route_report_path", RouteReportPath, true);
        Append(json, "manual_start_first_person_weapon_visible", ExtractBool(gameplay, "manual_start_first_person_weapon_visible"), true);
        Append(json, "fire_ammo_and_enemy_hit", ExtractBool(gameplay, "fire_ammo_and_enemy_hit"), true);
        Append(json, "reload_state_mutation", ExtractBool(gameplay, "reload_state_mutation"), true);
        Append(json, "weapon_feedback_spawned", ExtractBool(gameplay, "weapon_feedback_spawned"), true);
        Append(json, "first_person_weapon_polish", ExtractBool(gameplay, "first_person_weapon_polish"), true);
        Append(json, "spawn_first_person_weapon_visible", ExtractBool(route, "spawn_first_person_weapon_visible"), true);
        Append(json, "fire_state_mutation", ExtractBool(route, "fire_state_mutation"), true);
        Append(json, "note", Application.isPlaying ? "Game feel gate summarizes current play-mode proof." : "Run this in Play Mode or run the full tactical acceptance pipeline for fresh proof.", false);
        json.AppendLine("}");

        File.WriteAllText(GameFeelReportPath, json.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] Game feel evidence gate written to " + GameFeelReportPath + " passed=" + passed);
    }

    private static (int errors, int warnings, int logs) GetConsoleCounts()
    {
        var logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
        var method = logEntriesType?.GetMethod("GetCountsByType", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null)
        {
            return (0, 0, 0);
        }

        var args = new object[] { 0, 0, 0 };
        method.Invoke(null, args);
        return ((int)args[0], (int)args[1], (int)args[2]);
    }

    private static string ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    private static bool ExtractBool(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        return Regex.IsMatch(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*true");
    }

    private static int ExtractNestedInt(string json, string objectKey, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return 0;
        }

        var objectMatch = Regex.Match(json, "\\\"" + Regex.Escape(objectKey) + "\\\"\\s*:\\s*\\{(?<body>.*?)\\n\\s*\\}", RegexOptions.Singleline);
        if (!objectMatch.Success)
        {
            return 0;
        }

        var match = Regex.Match(objectMatch.Groups["body"].Value, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*(?<value>-?\\d+)");
        return match.Success && int.TryParse(match.Groups["value"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
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

    private static string Escape(string value)
    {
        return (value ?? "")
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }
}
#endif
