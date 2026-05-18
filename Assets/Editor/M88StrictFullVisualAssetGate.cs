#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

public static class M88StrictFullVisualAssetGate
{
    private const string ReportPath = "docs/M88_STRICT_FULL_VISUAL_ASSET_GATE.json";
    private const string MarkdownPath = "docs/M88_STRICT_FULL_VISUAL_ASSET_GATE.md";
    private const string RoutePath = "docs/TACTICAL_PLAYABLE_ROUTE_GATE.json";
    private const string AcceptancePath = "docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json";
    private const string M85Path = "docs/M85_VISUAL_PRODUCTION_PASS.json";
    private const string M87Path = "docs/M87_CLASS_LEVEL_PRODUCTION_VISIBILITY_GATE.json";
    private const string LegacyVisibilityPath = "docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json";

    [MenuItem("AI Tools/Run M88 Strict Full Visual Asset Gate")]
    public static void Run()
    {
        Directory.CreateDirectory("docs");

        var route = ReadText(RoutePath);
        var acceptance = ReadText(AcceptancePath);
        var m85 = ReadText(M85Path);
        var m87 = ReadText(M87Path);
        var legacy = ReadText(LegacyVisibilityPath);

        var checks = new[]
        {
            new Check("gameplay_route_passed", ExtractBool(route, "gameplay_route_passed"), "Playable route evidence exists."),
            new Check("all_required_current_gates_passed", ExtractBool(acceptance, "all_required_current_gates_passed"), "Current gameplay/parity gates pass."),
            new Check("m85_visual_production_passed", ExtractBool(m85, "passed"), "Scoped player-camera visual production pass exists."),
            new Check("m87_class_level_visibility_passed", ExtractBool(m87, "passed"), "Five route-level approved-equivalent asset classes have visibility/event evidence."),
            new Check("approved_incremental_assets_passed", ExtractNestedBool(route, "asset_quality", "approved_incremental_assets_passed"), "Approved incremental gameplay-bound assets exist."),
            new Check("first_person_weapon_polish_passed", ExtractNestedBool(route, "asset_quality", "first_person_weapon_polish_passed"), "First-person weapon polish route evidence exists."),
            new Check("character_authored_clip_animation_passed", ExtractNestedBool(route, "asset_quality", "character_authored_clip_animation_passed"), "Character authored clip evidence exists."),
            new Check("true_skinned_humanoid_import_passed", ExtractNestedBool(route, "asset_quality", "true_skinned_humanoid_import_passed"), "Approved player/enemy GLBs include skinned/animation import evidence."),
            new Check("legacy_realified_batch_visibility_gate_passed", ExtractBool(legacy, "passed"), "Legacy Realified batch visibility gate passes."),
            new Check("generated_batch_class_promotion_passed", ExtractNestedInt(acceptance, "summary", "realified_promotion_production_promoted_classes") >= 5 || ExtractInt(acceptance, "realified_promotion_production_promoted_classes") >= 5, "Generated batch class promotion reaches all required classes."),
            new Check("final_weapon_art_review_passed", false, "Manual/VLM review must confirm final weapon art quality, not just functional visibility."),
            new Check("final_humanoid_art_review_passed", false, "Manual/VLM review must confirm final humanoid/gear quality, not just intermediate tactical detail kits."),
            new Check("clean_built_player_gameplay_capture_passed", false, "Built-player route capture must show weapon, pickup, NPC combat, reload, traversal, and restart from the built app.")
        };

        var requiredPassed = true;
        foreach (var check in checks)
        {
            if (!check.Passed)
            {
                requiredPassed = false;
            }
        }

        WriteJson(checks, requiredPassed);
        WriteMarkdown(checks, requiredPassed);

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("[AI Tools] M88 strict full visual asset gate written to " + ReportPath + " passed=" + requiredPassed);
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

    private static bool ExtractNestedBool(string json, string objectKey, string key)
    {
        return ExtractBool(ExtractObjectBody(json, objectKey), key);
    }

    private static int ExtractNestedInt(string json, string objectKey, string key)
    {
        return ExtractInt(ExtractObjectBody(json, objectKey), key);
    }

    private static string ExtractObjectBody(string json, string objectKey)
    {
        if (string.IsNullOrEmpty(json))
        {
            return "";
        }

        var match = Regex.Match(json, "\\\"" + Regex.Escape(objectKey) + "\\\"\\s*:\\s*\\{(?<body>.*?)\\n\\s*\\}", RegexOptions.Singleline);
        return match.Success ? match.Groups["body"].Value : "";
    }

    private static string ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    private static void WriteJson(Check[] checks, bool passed)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "m88_strict_full_visual_asset_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "passed", passed, true);
        Append(json, "full_visual_asset_gate_currently_remains_false", true, true);
        Append(json, "route_report_path", RoutePath, true);
        Append(json, "acceptance_report_path", AcceptancePath, true);
        Append(json, "m85_report_path", M85Path, true);
        Append(json, "m87_report_path", M87Path, true);
        json.AppendLine("  \"checks\": [");
        for (var i = 0; i < checks.Length; i++)
        {
            AppendCheck(json, checks[i], i == checks.Length - 1);
        }
        json.AppendLine("  ],");
        Append(json, "next_executable_slice", "Do not flip full_visual_asset_gate until final weapon art review, final humanoid art review, generated batch class promotion, and clean built-player route capture are all true.", false);
        json.AppendLine("}");
        File.WriteAllText(ReportPath, json.ToString());
    }

    private static void WriteMarkdown(Check[] checks, bool passed)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# M88 Strict Full Visual Asset Gate");
        markdown.AppendLine();
        markdown.AppendLine("Generated: " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        markdown.AppendLine();
        markdown.AppendLine("- Passed: `" + passed + "`");
        markdown.AppendLine("- Current policy: this gate must remain false until all strict production visual checks pass.");
        markdown.AppendLine();
        markdown.AppendLine("| Check | Passed | Meaning |");
        markdown.AppendLine("|---|---:|---|");
        foreach (var check in checks)
        {
            markdown.Append("| `")
                .Append(check.Key)
                .Append("` | ")
                .Append(check.Passed)
                .Append(" | ")
                .Append(check.Meaning)
                .AppendLine(" |");
        }
        markdown.AppendLine();
        markdown.AppendLine("Next executable slice: do not flip `full_visual_asset_gate_passed` until final weapon art review, final humanoid art review, generated batch class promotion, and clean built-player route capture are all true.");
        File.WriteAllText(MarkdownPath, markdown.ToString());
    }

    private static void AppendCheck(StringBuilder json, Check check, bool last)
    {
        json.AppendLine("    {");
        Append(json, "key", check.Key, true, 6);
        Append(json, "passed", check.Passed, true, 6);
        Append(json, "meaning", check.Meaning, false, 6);
        json.Append("    }");
        json.AppendLine(last ? "" : ",");
    }

    private static void Append(StringBuilder json, string key, string value, bool comma, int indent = 2)
    {
        json.Append(' ', indent).Append('"').Append(key).Append("\": \"").Append(Escape(value)).Append('"');
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, bool value, bool comma, int indent = 2)
    {
        json.Append(' ', indent).Append('"').Append(key).Append("\": ").Append(value ? "true" : "false");
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

    private readonly struct Check
    {
        public Check(string key, bool passed, string meaning)
        {
            Key = key;
            Passed = passed;
            Meaning = meaning;
        }

        public readonly string Key;
        public readonly bool Passed;
        public readonly string Meaning;
    }
}
#endif
