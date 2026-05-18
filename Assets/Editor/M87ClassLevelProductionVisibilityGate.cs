#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class M87ClassLevelProductionVisibilityGate
{
    private const string ReportPath = "docs/M87_CLASS_LEVEL_PRODUCTION_VISIBILITY_GATE.json";
    private const string MarkdownPath = "docs/M87_CLASS_LEVEL_PRODUCTION_VISIBILITY_GATE.md";
    private const string RoutePath = "docs/TACTICAL_PLAYABLE_ROUTE_GATE.json";
    private const string M84Path = "docs/M84_THREE_CLASS_ASSET_FACTORY_SPIKE.json";
    private const string M85Path = "docs/M85_VISUAL_PRODUCTION_PASS.json";
    private const string LegacyVisibilityPath = "docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json";

    private static readonly ClassSpec[] Classes =
    {
        new ClassSpec(
            "weapon",
            "hero_rifle",
            "generated Realified hero rifle plus weapon-feel route evidence",
            "Assets/Screenshots/M84AssetFactory/01_weapon_hero_rifle_player_camera.png",
            "weapon production evidence uses the promoted hero rifle and fire/reload/hit proof"),
        new ClassSpec(
            "character",
            "approved_player_enemy_tactical_detail",
            "approved player/enemy GLBs plus tactical detail kit and authored clip evidence",
            "Assets/Screenshots/M85VisualProduction/04_after_enemy_character_lighting_route.png",
            "character evidence is route-level intermediate production credit, not final commercial humanoid quality"),
        new ClassSpec(
            "gear",
            "approved_helmet_vest_loot",
            "approved helmet and vest equipment pickups",
            "Assets/Screenshots/PlayableRoute/06_approved_vest_prompt.png",
            "gear evidence uses approved equipment pickup visibility and route mutation"),
        new ClassSpec(
            "loot",
            "medical_loot_v1_and_approved_ammo",
            "approved medkit and ammo loot route evidence",
            "Assets/Screenshots/PlayableRoute/06_approved_medkit_prompt.png",
            "loot evidence uses approved medkit/ammo visibility and pickup mutation"),
        new ClassSpec(
            "environment_prop",
            "approved_container_v1_and_tactical_crate_v1",
            "approved container and crate cover/route evidence",
            "Assets/Screenshots/M84AssetFactory/02_container_gameplay_cover_player_camera.png",
            "environment evidence uses approved semantic props and cover/blocking proof")
    };

    [MenuItem("AI Tools/Run M87 Class-Level Production Visibility Gate")]
    public static void Run()
    {
        Directory.CreateDirectory("docs");

        var route = ReadText(RoutePath);
        var m84 = ReadText(M84Path);
        var m85 = ReadText(M85Path);
        var legacy = ReadText(LegacyVisibilityPath);
        var results = new ClassResult[Classes.Length];
        for (var i = 0; i < Classes.Length; i++)
        {
            results[i] = Evaluate(Classes[i], route, m84, m85);
        }

        var passedClasses = 0;
        foreach (var result in results)
        {
            if (result.Passed)
            {
                passedClasses++;
            }
        }

        var legacyRealifiedGatePassed = ExtractBool(legacy, "passed");
        var legacyProductionClasses = ExtractNestedInt(legacy, "summary", "production_promoted_classes");
        var passed = passedClasses == results.Length
            && !legacyRealifiedGatePassed
            && ExtractBool(route, "gameplay_route_passed")
            && ExtractBool(m84, "passed")
            && ExtractBool(m85, "passed");

        WriteJson(results, passed, passedClasses, legacyRealifiedGatePassed, legacyProductionClasses);
        WriteMarkdown(results, passed, passedClasses, legacyRealifiedGatePassed, legacyProductionClasses);

        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] M87 class-level production visibility gate written to " + ReportPath + " passed=" + passed);
    }

    private static ClassResult Evaluate(ClassSpec spec, string route, string m84, string m85)
    {
        switch (spec.Category)
        {
            case "weapon":
                return BuildResult(
                    spec,
                    ExtractAssetPromoted(m84, "hero_rifle")
                    && ExtractNestedBool(route, "asset_quality", "first_person_weapon_polish_passed")
                    && ExtractBool(route, "fire_state_mutation"),
                    "m84_hero_rifle_promoted_and_weapon_feel_route_passed");
            case "character":
                return BuildResult(
                    spec,
                    ExtractNestedBool(route, "asset_quality", "true_skinned_humanoid_import_passed")
                    && ExtractNestedBool(route, "asset_quality", "tactical_character_detail_kit_passed")
                    && ExtractNestedBool(route, "asset_quality", "character_authored_clip_animation_passed")
                    && ExtractBool(m85, "screenshot_quality_passed"),
                    "approved_character_glbs_detail_kit_and_authored_clip_evidence_passed");
            case "gear":
                return BuildResult(
                    spec,
                    ExtractBool(route, "approved_loot_class_route_evidence")
                    && ExtractNestedInt(route, "asset_quality", "approved_helmet_loot_renderer_instances") >= 1
                    && ExtractNestedInt(route, "asset_quality", "approved_vest_loot_renderer_instances") >= 1
                    && route.Contains("approvedClass Vest"),
                    "approved_helmet_vest_pickup_route_evidence_passed");
            case "loot":
                return BuildResult(
                    spec,
                    ExtractAssetPromoted(m84, "medical_loot_v1")
                    && ExtractBool(route, "approved_loot_class_route_evidence")
                    && route.Contains("approvedClass Ammo")
                    && route.Contains("approvedClass Medkit"),
                    "approved_ammo_medkit_route_evidence_passed");
            case "environment_prop":
                return BuildResult(
                    spec,
                    ExtractAssetPromoted(m84, "approved_container_v1")
                    && ExtractBool(route, "container_visited")
                    && ExtractNestedBool(route, "asset_quality", "approved_incremental_assets_passed"),
                    "approved_container_crate_route_and_cover_evidence_passed");
            default:
                return BuildResult(spec, false, "unknown_class");
        }
    }

    private static ClassResult BuildResult(ClassSpec spec, bool passed, string reason)
    {
        var screenshotExists = File.Exists(spec.ScreenshotPath);
        var finalPassed = passed && screenshotExists;
        return new ClassResult(
            spec.Category,
            spec.AssetId,
            spec.Source,
            spec.ScreenshotPath,
            screenshotExists,
            finalPassed,
            finalPassed ? "" : reason + "_or_screenshot_missing",
            spec.Note,
            reason);
    }

    private static bool ExtractAssetPromoted(string json, string assetId)
    {
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        var match = Regex.Match(
            json,
            "\\{(?<body>[^\\{\\}]*\\\"asset_id\\\"\\s*:\\s*\\\"" + Regex.Escape(assetId) + "\\\".*?\\})",
            RegexOptions.Singleline);
        return match.Success && ExtractBool(match.Groups["body"].Value, "production_promoted");
    }

    private static bool ExtractBool(string json, string key)
    {
        return !string.IsNullOrEmpty(json) && Regex.IsMatch(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*true");
    }

    private static bool ExtractNestedBool(string json, string objectKey, string key)
    {
        var body = ExtractObjectBody(json, objectKey);
        return ExtractBool(body, key);
    }

    private static int ExtractNestedInt(string json, string objectKey, string key)
    {
        var body = ExtractObjectBody(json, objectKey);
        if (string.IsNullOrEmpty(body))
        {
            return 0;
        }

        var match = Regex.Match(body, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*(?<value>-?\\d+)");
        return match.Success && int.TryParse(match.Groups["value"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
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

    private static void WriteJson(ClassResult[] results, bool passed, int passedClasses, bool legacyRealifiedGatePassed, int legacyProductionClasses)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "m87_class_level_production_visibility_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "passed", passed, true);
        Append(json, "evidence_credit_policy", "approved_equivalent_or_realified_gameplay_bound_asset", true);
        Append(json, "legacy_realified_batch_visibility_gate_passed", legacyRealifiedGatePassed, true);
        Append(json, "legacy_realified_batch_production_promoted_classes", legacyProductionClasses, true);
        Append(json, "class_count", results.Length, true);
        Append(json, "passed_class_count", passedClasses, true);
        Append(json, "route_report_path", RoutePath, true);
        Append(json, "m84_report_path", M84Path, true);
        Append(json, "m85_report_path", M85Path, true);
        Append(json, "legacy_visibility_report_path", LegacyVisibilityPath, true);
        json.AppendLine("  \"classes\": [");
        for (var i = 0; i < results.Length; i++)
        {
            AppendClass(json, results[i], i == results.Length - 1);
        }
        json.AppendLine("  ],");
        Append(json, "full_visual_asset_gate_intentionally_not_overridden", true, true);
        Append(json, "remaining_blocker", "This gate proves class-level routed/approved production visibility. It does not make the failed Realified batch production-ready and does not close final PUBG-like visual quality.", false);
        json.AppendLine("}");
        File.WriteAllText(ReportPath, json.ToString());
    }

    private static void WriteMarkdown(ClassResult[] results, bool passed, int passedClasses, bool legacyRealifiedGatePassed, int legacyProductionClasses)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# M87 Class-Level Production Visibility Gate");
        markdown.AppendLine();
        markdown.AppendLine("Generated: " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        markdown.AppendLine();
        markdown.AppendLine("- Passed: `" + passed + "`");
        markdown.AppendLine("- Passed classes: `" + passedClasses + " / " + results.Length + "`");
        markdown.AppendLine("- Credit policy: `approved_equivalent_or_realified_gameplay_bound_asset`");
        markdown.AppendLine("- Legacy Realified batch visibility gate passed: `" + legacyRealifiedGatePassed + "`");
        markdown.AppendLine("- Legacy Realified batch production-promoted classes: `" + legacyProductionClasses + "`");
        markdown.AppendLine();
        markdown.AppendLine("This gate is a route-level production visibility reconciliation. It does not override the failed Realified batch semantic review and does not close final PUBG-like visual quality.");
        markdown.AppendLine();
        markdown.AppendLine("| Class | Asset Credit | Passed | Screenshot | Note |");
        markdown.AppendLine("|---|---|---:|---|---|");
        foreach (var result in results)
        {
            markdown.Append("| ")
                .Append(result.Category)
                .Append(" | `")
                .Append(result.AssetId)
                .Append("` | ")
                .Append(result.Passed)
                .Append(" | `")
                .Append(result.ScreenshotPath)
                .Append("` | ")
                .Append(result.Note)
                .AppendLine(" |");
        }

        File.WriteAllText(MarkdownPath, markdown.ToString());
    }

    private static void AppendClass(StringBuilder json, ClassResult result, bool last)
    {
        json.AppendLine("    {");
        Append(json, "category", result.Category, true, 6);
        Append(json, "asset_id", result.AssetId, true, 6);
        Append(json, "source", result.Source, true, 6);
        Append(json, "evidence_reason", result.EvidenceReason, true, 6);
        Append(json, "screenshot_path", result.ScreenshotPath, true, 6);
        Append(json, "screenshot_exists", result.ScreenshotExists, true, 6);
        Append(json, "passed", result.Passed, true, 6);
        Append(json, "blocker", result.Blocker, true, 6);
        Append(json, "note", result.Note, false, 6);
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

    private static void Append(StringBuilder json, string key, int value, bool comma, int indent = 2)
    {
        json.Append(' ', indent).Append('"').Append(key).Append("\": ").Append(value.ToString(CultureInfo.InvariantCulture));
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

    private readonly struct ClassSpec
    {
        public ClassSpec(string category, string assetId, string source, string screenshotPath, string note)
        {
            Category = category;
            AssetId = assetId;
            Source = source;
            ScreenshotPath = screenshotPath;
            Note = note;
        }

        public readonly string Category;
        public readonly string AssetId;
        public readonly string Source;
        public readonly string ScreenshotPath;
        public readonly string Note;
    }

    private readonly struct ClassResult
    {
        public ClassResult(string category, string assetId, string source, string screenshotPath, bool screenshotExists, bool passed, string blocker, string note, string evidenceReason)
        {
            Category = category;
            AssetId = assetId;
            Source = source;
            ScreenshotPath = screenshotPath;
            ScreenshotExists = screenshotExists;
            Passed = passed;
            Blocker = blocker;
            Note = note;
            EvidenceReason = evidenceReason;
        }

        public readonly string Category;
        public readonly string AssetId;
        public readonly string Source;
        public readonly string ScreenshotPath;
        public readonly bool ScreenshotExists;
        public readonly bool Passed;
        public readonly string Blocker;
        public readonly string Note;
        public readonly string EvidenceReason;
    }
}
#endif
