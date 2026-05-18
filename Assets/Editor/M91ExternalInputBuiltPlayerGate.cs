#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// M91 gate writer for external-input built-player telemetry.
/// This command does not drive the built app and does not set passed=true
/// unless a previously captured M91 output JSON exists and reports passed=true
/// with external_input_driven=true.
/// </summary>
public static class M91ExternalInputBuiltPlayerGate
{
    private const string RuntimeScriptPath = "Assets/Scripts/Tactical/TacticalExternalInputRouteTelemetry.cs";
    private const string ReportJsonPath = "docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_GATE.json";
    private const string ReportMarkdownPath = "docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_GATE.md";
    private const string RouteJsonPath = "docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json";
    private const string BuiltAppAbsolutePath = "/Users/yuanshaochen/My project/Builds/M86/TacticalPrototypeM86.app";
    private const string BuiltAppRelativePath = "Builds/M86/TacticalPrototypeM86.app";

    [MenuItem("AI Tools/Run M91 External Input Built Player Gate")]
    public static void RunGate()
    {
        Directory.CreateDirectory("docs");

        var scriptExists = File.Exists(RuntimeScriptPath);
        var builtAppPath = ResolveBuiltAppPath();
        var builtAppExists = !string.IsNullOrEmpty(builtAppPath);
        var routeJsonPath = ResolveRouteJsonPath();
        var routeJsonExists = !string.IsNullOrEmpty(routeJsonPath) && File.Exists(routeJsonPath);
        var routeJson = routeJsonExists ? File.ReadAllText(routeJsonPath) : "";
        var routePassed = ExtractBool(routeJson, "passed");
        var routeExternalInputDriven = ExtractBool(routeJson, "external_input_driven");
        var routeBuiltPlayer = ExtractBool(routeJson, "built_player");
        var screenshotCount = ExtractInt(routeJson, "screenshot_count");
        var passed = scriptExists && builtAppExists && routeJsonExists && routePassed && routeExternalInputDriven && routeBuiltPlayer;

        var blockers = new StringBuilder();
        if (!scriptExists) AppendBlocker(blockers, "missing TacticalExternalInputRouteTelemetry.cs");
        if (!builtAppExists) AppendBlocker(blockers, "missing M86 built app at " + BuiltAppAbsolutePath + " or " + BuiltAppRelativePath);
        if (!routeJsonExists) AppendBlocker(blockers, "missing M91 external-input output JSON; launch the built app and drive it with real keyboard/mouse input");
        if (routeJsonExists && !routePassed) AppendBlocker(blockers, "M91 output JSON exists but passed=false");
        if (routeJsonExists && !routeExternalInputDriven) AppendBlocker(blockers, "M91 output JSON exists but external_input_driven=false");
        if (routeJsonExists && !routeBuiltPlayer) AppendBlocker(blockers, "M91 output JSON exists but built_player=false");

        var launchCommand = "\"" + BuiltAppAbsolutePath + "/Contents/MacOS/My project\""
            + " --m91-external-input-route"
            + " --m91-output=\"/Users/yuanshaochen/My project/docs\"";

        WriteJson(
            passed,
            scriptExists,
            builtAppExists,
            builtAppPath,
            routeJsonExists,
            routeJsonPath,
            routePassed,
            routeExternalInputDriven,
            routeBuiltPlayer,
            screenshotCount,
            launchCommand,
            blockers.ToString());

        WriteMarkdown(
            passed,
            scriptExists,
            builtAppExists,
            builtAppPath,
            routeJsonExists,
            routeJsonPath,
            routePassed,
            routeExternalInputDriven,
            routeBuiltPlayer,
            screenshotCount,
            launchCommand,
            blockers.ToString());

        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] M91 External Input Built Player Gate written to " + ReportJsonPath + " passed=" + passed);
    }

    private static string ResolveBuiltAppPath()
    {
        if (Directory.Exists(BuiltAppAbsolutePath))
        {
            return BuiltAppAbsolutePath;
        }

        if (Directory.Exists(BuiltAppRelativePath))
        {
            return BuiltAppRelativePath;
        }

        return "";
    }

    private static string ResolveRouteJsonPath()
    {
        if (File.Exists(RouteJsonPath))
        {
            return RouteJsonPath;
        }

        var nested = Path.Combine("docs", "M91ExternalInputBuiltPlayerRoute", "M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json");
        return File.Exists(nested) ? nested : "";
    }

    private static void WriteJson(
        bool passed,
        bool scriptExists,
        bool builtAppExists,
        string builtAppPath,
        bool routeJsonExists,
        string routeJsonPath,
        bool routePassed,
        bool routeExternalInputDriven,
        bool routeBuiltPlayer,
        int screenshotCount,
        string launchCommand,
        string blockers)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "m91_external_input_built_player_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "passed", passed, true);
        Append(json, "script_exists", scriptExists, true);
        Append(json, "built_app_exists", builtAppExists, true);
        Append(json, "built_app_path", builtAppPath, true);
        Append(json, "route_json_exists", routeJsonExists, true);
        Append(json, "route_json_path", routeJsonPath, true);
        Append(json, "route_json_passed", routePassed, true);
        Append(json, "route_json_external_input_driven", routeExternalInputDriven, true);
        Append(json, "route_json_built_player", routeBuiltPlayer, true);
        Append(json, "route_json_screenshot_count", screenshotCount, true);
        Append(json, "launch_command", launchCommand, true);
        Append(json, "note", "This gate only passes after an external-input built-player telemetry JSON exists with passed=true and external_input_driven=true. It does not drive input itself.", true);
        AppendArrayFromMultiline(json, "blockers", blockers, false);
        json.AppendLine("}");
        File.WriteAllText(ReportJsonPath, json.ToString());
    }

    private static void WriteMarkdown(
        bool passed,
        bool scriptExists,
        bool builtAppExists,
        string builtAppPath,
        bool routeJsonExists,
        string routeJsonPath,
        bool routePassed,
        bool routeExternalInputDriven,
        bool routeBuiltPlayer,
        int screenshotCount,
        string launchCommand,
        string blockers)
    {
        var md = new StringBuilder();
        md.AppendLine("# M91 External Input Built Player Gate");
        md.AppendLine();
        md.AppendLine("Date: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC");
        md.AppendLine();
        md.AppendLine("This gate verifies whether the built macOS app has been completed through an external keyboard/mouse or manual input route. It does not call gameplay APIs and does not drive the route itself.");
        md.AppendLine();
        md.AppendLine("## Result");
        md.AppendLine();
        md.AppendLine("- `passed`: `" + (passed ? "true" : "false") + "`");
        md.AppendLine("- `script_exists`: `" + (scriptExists ? "true" : "false") + "`");
        md.AppendLine("- `built_app_exists`: `" + (builtAppExists ? "true" : "false") + "`");
        md.AppendLine("- `built_app_path`: `" + EscapeMarkdown(builtAppPath) + "`");
        md.AppendLine("- `route_json_exists`: `" + (routeJsonExists ? "true" : "false") + "`");
        md.AppendLine("- `route_json_path`: `" + EscapeMarkdown(routeJsonPath) + "`");
        md.AppendLine("- `route_json_passed`: `" + (routePassed ? "true" : "false") + "`");
        md.AppendLine("- `route_json_external_input_driven`: `" + (routeExternalInputDriven ? "true" : "false") + "`");
        md.AppendLine("- `route_json_built_player`: `" + (routeBuiltPlayer ? "true" : "false") + "`");
        md.AppendLine("- `route_json_screenshot_count`: `" + screenshotCount.ToString(CultureInfo.InvariantCulture) + "`");
        md.AppendLine();
        md.AppendLine("## Launch Command");
        md.AppendLine();
        md.AppendLine("```bash");
        md.AppendLine(launchCommand);
        md.AppendLine("```");
        md.AppendLine();
        md.AppendLine("## Required External Input Route");
        md.AppendLine();
        md.AppendLine("After launching with the command above, drive the built app manually or with an external input automation tool:");
        md.AppendLine();
        md.AppendLine("1. Press `Enter`, `Space`, or click to start from the lobby.");
        md.AppendLine("2. Move with `WASD` or arrow keys until the player position changes.");
        md.AppendLine("3. Press `F` near the arranged loot.");
        md.AppendLine("4. Left-click to fire.");
        md.AppendLine("5. Press `R` to reload.");
        md.AppendLine("6. Interact with the arranged NPC: either damage it or let it damage/down the player.");
        md.AppendLine("7. Keep playing until `07_after_death_or_restart_input.png` is captured.");
        md.AppendLine();
        md.AppendLine("The telemetry should write:");
        md.AppendLine();
        md.AppendLine("```text");
        md.AppendLine("docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json");
        md.AppendLine("docs/00_lobby_before_external_input.png");
        md.AppendLine("docs/01_after_start_input.png");
        md.AppendLine("docs/02_after_movement_input.png");
        md.AppendLine("docs/03_after_pickup_input.png");
        md.AppendLine("docs/04_after_fire_input.png");
        md.AppendLine("docs/05_after_reload_input.png");
        md.AppendLine("docs/06_after_enemy_interaction_input.png");
        md.AppendLine("docs/07_after_death_or_restart_input.png");
        md.AppendLine("```");
        md.AppendLine();
        md.AppendLine("## Blockers");
        md.AppendLine();
        if (string.IsNullOrWhiteSpace(blockers))
        {
            md.AppendLine("- none");
        }
        else
        {
            foreach (var blocker in blockers.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                md.AppendLine("- " + blocker);
            }
        }

        md.AppendLine();
        md.AppendLine("## Important Non-Claims");
        md.AppendLine();
        md.AppendLine("- This gate does not set `full_visual_asset_gate_passed=true`.");
        md.AppendLine("- This gate does not replace M88 strict visual blockers.");
        md.AppendLine("- This gate does not prove external input unless the route JSON says `external_input_driven=true` and `passed=true`.");
        File.WriteAllText(ReportMarkdownPath, md.ToString());
    }

    private static bool ExtractBool(string json, string key)
    {
        return !string.IsNullOrEmpty(json) && Regex.IsMatch(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*true");
    }

    private static int ExtractInt(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return 0;
        }

        var match = Regex.Match(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*(?<value>-?\\d+)");
        return match.Success && int.TryParse(match.Groups["value"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private static void AppendBlocker(StringBuilder blockers, string blocker)
    {
        if (blockers.Length > 0)
        {
            blockers.AppendLine();
        }

        blockers.Append(blocker);
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

    private static void AppendArrayFromMultiline(StringBuilder json, string key, string multiline, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": [");
        var lines = string.IsNullOrWhiteSpace(multiline)
            ? Array.Empty<string>()
            : multiline.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0)
            {
                json.Append(", ");
            }

            json.Append("\"").Append(Escape(lines[i].Trim())).Append("\"");
        }

        json.Append("]");
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

    private static string EscapeMarkdown(string value)
    {
        return (value ?? "").Replace("`", "\\`");
    }
}
#endif
