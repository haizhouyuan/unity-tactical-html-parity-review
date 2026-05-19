#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

public static class M96FinalHumanoidArtReviewGate
{
    private const string ReportPath = "docs/M96_FINAL_HUMANOID_ART_REVIEW_GATE.json";
    private const string MarkdownPath = "docs/M96_FINAL_HUMANOID_ART_REVIEW_GATE.md";
    private const string RoutePath = "docs/TACTICAL_PLAYABLE_ROUTE_GATE.json";
    private const string ApprovalPath = "docs/M96_FINAL_HUMANOID_ART_REVIEW_APPROVAL.json";
    private const string M85Path = "docs/M85_VISUAL_PRODUCTION_PASS.json";
    private const string M87Path = "docs/M87_CLASS_LEVEL_PRODUCTION_VISIBILITY_GATE.json";
    private const string LedgerPath = "docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json";
    private const string M91RoutePath = "docs/M91ExternalInputBuiltPlayerRoute/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json";
    private const string WeaponFeelPath = "docs/WEAPON_FEEL_GATE.json";
    private const string RealifiedCategoryReviewPath = "docs/REALIFIED_CATEGORY_NEMOTRON_REVIEWS.json";
    private const string RealifiedCharacterReviewPath = "docs/realified_category_nemotron_reviews/character.json";
    private const string RealifiedGearReviewPath = "docs/realified_category_nemotron_reviews/gear.json";

    private static readonly string[] EvidencePaths =
    {
        RoutePath,
        ApprovalPath,
        M85Path,
        M87Path,
        LedgerPath,
        M91RoutePath,
        WeaponFeelPath,
        RealifiedCategoryReviewPath,
        RealifiedCharacterReviewPath,
        RealifiedGearReviewPath,
        "Assets/Screenshots/M85VisualProduction/04_after_enemy_character_lighting_route.png",
        "Assets/Screenshots/PlayableRoute/06_approved_vest_prompt.png",
        "Assets/Screenshots/PlayableRoute/08_fire_hit_first_person.png",
        "Assets/Screenshots/PlayableRoute/11_death_overlay.png"
    };

    [MenuItem("AI Tools/Run M96 Final Humanoid Art Review Gate")]
    public static void Run()
    {
        Directory.CreateDirectory("docs");

        var route = ReadText(RoutePath);
        var approval = ReadText(ApprovalPath);
        var m85 = ReadText(M85Path);
        var m87 = ReadText(M87Path);
        var ledger = ReadText(LedgerPath);
        var m91Route = ReadText(M91RoutePath);
        var weaponFeel = ReadText(WeaponFeelPath);
        var characterReview = ReadText(RealifiedCharacterReviewPath);
        var gearReview = ReadText(RealifiedGearReviewPath);
        var m87Character = ExtractClassBody(m87, "character");

        var explicitFinalHumanoidArtReviewPassed = ExtractRootBool(approval, "approved")
            && ExtractRootBool(approval, "player_camera_reviewed")
            && ExtractRootBool(approval, "npc_reviewed")
            && ExtractRootBool(approval, "gear_reviewed")
            && !ExtractRootBool(approval, "proxy_or_placeholder_blocker");
        var checks = new[]
        {
            new Check("input_reports_present", AllFilesExist(RoutePath, M85Path, M87Path, LedgerPath, M91RoutePath, WeaponFeelPath), "M96 can only review existing route, visual, ledger, built-player, and weapon-feel evidence."),
            new Check("non_proxy_humanoid_import_status", ExtractNestedBool(route, "asset_quality", "true_skinned_humanoid_import_passed") && ExtractNestedInt(route, "asset_quality", "approved_player_renderer_instances") >= 1 && ExtractNestedInt(route, "asset_quality", "approved_enemy_renderer_instances") >= 1, "Approved player/enemy evidence includes skinned/imported rendered humanoids."),
            new Check("technical_animation_evidence_separated", ExtractNestedBool(route, "asset_quality", "character_authored_clip_animation_passed") && ExtractNestedBool(route, "asset_quality", "true_skinned_humanoid_import_passed"), "Skinned/imported/animated evidence is present and tracked separately from final art quality."),
            new Check("tactical_silhouette_visible", ExtractRootBool(m87, "passed") && ExtractBool(m87Character, "passed") && ScreenshotExists("Assets/Screenshots/M85VisualProduction/04_after_enemy_character_lighting_route.png"), "Route-level enemy character silhouette is visible under gameplay lighting."),
            new Check("helmet_vest_gear_route_visible", ExtractNestedInt(route, "asset_quality", "approved_helmet_loot_renderer_instances") >= 1 && ExtractNestedInt(route, "asset_quality", "approved_vest_loot_renderer_instances") >= 1 && ScreenshotExists("Assets/Screenshots/PlayableRoute/06_approved_vest_prompt.png"), "Helmet and vest equipment have route-visible approved pickup evidence."),
            new Check("generated_character_assets_final_ready", AssetProductionPromoted(ledger, "player_tactical") && AssetProductionPromoted(ledger, "enemy_tactical") && ExtractBool(characterReview, "promotion_allowed"), "Generated player/enemy humanoid assets must be category-correct, gameplay-bound, and production-promoted."),
            new Check("generated_helmet_vest_gear_final_ready", AssetProductionPromoted(ledger, "helmet") && AssetProductionPromoted(ledger, "vest") && ExtractBool(gearReview, "promotion_allowed"), "Generated helmet/vest gear must be category-correct, gameplay-bound, player-camera visible, and production-promoted."),
            new Check("humanoid_state_evidence_present", ExtractNestedBool(route, "asset_quality", "character_animation_state_evidence_passed") && ExtractNestedBool(route, "asset_quality", "character_authored_clip_animation_passed") && ExtractBool(m91Route, "enemy_interaction_observed") && ExtractBool(m91Route, "death_or_restart_observed"), "Idle/animation-state, combat interaction, and down/restart evidence exists."),
            new Check("aim_fire_hit_state_evidence_present", ExtractBool(weaponFeel, "m92_weapon_production_passed") && ExtractBool(weaponFeel, "first_person_pose_quality_passed") && ExtractBool(weaponFeel, "enemy_hit") && ExtractInt(weaponFeel, "shot_feedback_event_count") >= 1, "Aim/fire/hit evidence exists through weapon feel metrics and route screenshots."),
            new Check("gameplay_entity_binding_present", ExtractRootBool(m87, "passed") && ExtractRootBool(m91Route, "passed") && ExtractBool(m91Route, "external_input_driven") && ExtractBool(m91Route, "built_player"), "Humanoid evidence is tied to gameplay route and external-input built-player evidence."),
            new Check("player_camera_visibility_present", ExtractBool(route, "player_camera_evidence") && ExtractBool(m85, "screenshot_quality_passed") && ScreenshotExists("Assets/Screenshots/M85VisualProduction/04_after_enemy_character_lighting_route.png"), "Humanoid/gear evidence is visible from gameplay/player-camera captures."),
            new Check("explicit_final_humanoid_art_quality_review_passed", explicitFinalHumanoidArtReviewPassed, "Final humanoid/gear art quality must be explicitly approved in docs/M96_FINAL_HUMANOID_ART_REVIEW_APPROVAL.json; import, animation, silhouette, or route visibility alone is insufficient.")
        };

        var blockers = BuildBlockers(checks, route, m85, m87, ledger);
        var passed = blockers.Count == 0;

        WriteJson(checks, blockers, passed);
        WriteMarkdown(checks, blockers, passed);

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("[AI Tools] M96 final humanoid art review gate written to " + ReportPath + " passed=" + passed);
    }

    private static List<string> BuildBlockers(Check[] checks, string route, string m85, string m87, string ledger)
    {
        var blockers = new List<string>();
        foreach (var check in checks)
        {
            if (!check.Passed)
            {
                blockers.Add(check.Key + ": " + check.Meaning);
            }
        }

        var visualBlocker = ExtractString(route, "visual_completion_blocker");
        if (string.IsNullOrEmpty(visualBlocker))
        {
            visualBlocker = ExtractString(m85, "visual_completion_blocker");
        }

        if (!string.IsNullOrEmpty(visualBlocker) && !ContainsAny(blockers, visualBlocker))
        {
            blockers.Add(visualBlocker);
        }

        var characterNote = ExtractClassNote(m87, "character");
        if (!string.IsNullOrEmpty(characterNote) && !ContainsAny(blockers, characterNote))
        {
            blockers.Add(characterNote);
        }

        if ((!AssetProductionPromoted(ledger, "player_tactical") || !AssetProductionPromoted(ledger, "enemy_tactical"))
            && !ContainsAny(blockers, "player_tactical"))
        {
            blockers.Add("player_tactical and enemy_tactical generated assets are still blocked in the gameplay promotion ledger.");
        }

        return blockers;
    }

    private static bool AssetProductionPromoted(string json, string assetId)
    {
        var body = ExtractAssetBody(json, assetId);
        return !string.IsNullOrEmpty(body)
            && ExtractBool(body, "production_promoted")
            && ExtractBool(body, "gameplay_entity_scene_evidence")
            && ExtractBool(body, "player_camera_realified_asset_evidence")
            && ExtractBool(body, "asset_specific_gameplay_event_evidence");
    }

    private static string ExtractAssetBody(string json, string assetId)
    {
        if (string.IsNullOrEmpty(json))
        {
            return "";
        }

        var match = Regex.Match(json, "\\{\\s*\\\"asset_id\\\"\\s*:\\s*\\\"" + Regex.Escape(assetId) + "\\\"(?<body>.*?)\\n\\s*\\}", RegexOptions.Singleline);
        return match.Success ? match.Value : "";
    }

    private static string ExtractClassNote(string json, string category)
    {
        var body = ExtractClassBody(json, category);
        return ExtractString(body, "note");
    }

    private static string ExtractClassBody(string json, string category)
    {
        if (string.IsNullOrEmpty(json))
        {
            return "";
        }

        var match = Regex.Match(json, "\\{\\s*\\\"category\\\"\\s*:\\s*\\\"" + Regex.Escape(category) + "\\\"(?<body>.*?)\\n\\s*\\}", RegexOptions.Singleline);
        return match.Success ? match.Value : "";
    }

    private static bool ExtractBool(string json, string key)
    {
        return !string.IsNullOrEmpty(json) && Regex.IsMatch(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*true");
    }

    private static bool ExtractRootBool(string json, string key)
    {
        return JsonGateReader.RootBool(json, key);
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

    private static string ExtractString(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return "";
        }

        var match = Regex.Match(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*\\\"(?<value>(?:\\\\.|[^\\\"])*)\\\"", RegexOptions.Singleline);
        return match.Success ? Regex.Unescape(match.Groups["value"].Value) : "";
    }

    private static bool AllFilesExist(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (!File.Exists(path))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ScreenshotExists(string path)
    {
        return File.Exists(path) && new FileInfo(path).Length > 0;
    }

    private static bool Contains(string text, string value)
    {
        return !string.IsNullOrEmpty(text) && text.Contains(value);
    }

    private static bool ContainsAny(List<string> values, string needle)
    {
        foreach (var value in values)
        {
            if (value.Contains(needle))
            {
                return true;
            }
        }

        return false;
    }

    private static string ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    private static void WriteJson(Check[] checks, List<string> blockers, bool passed)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "m96_final_humanoid_art_review_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "passed", passed, true);
        Append(json, "decision_policy", "Skinned/imported/animated humanoid evidence is necessary but not sufficient for final humanoid art readiness.", true);
        json.AppendLine("  \"evidence_paths\": [");
        for (var i = 0; i < EvidencePaths.Length; i++)
        {
            AppendArrayValue(json, EvidencePaths[i], i == EvidencePaths.Length - 1, 4);
        }
        json.AppendLine("  ],");
        json.AppendLine("  \"checks\": [");
        for (var i = 0; i < checks.Length; i++)
        {
            AppendCheck(json, checks[i], i == checks.Length - 1);
        }
        json.AppendLine("  ],");
        json.AppendLine("  \"blockers\": [");
        for (var i = 0; i < blockers.Count; i++)
        {
            AppendArrayValue(json, blockers[i], i == blockers.Count - 1, 4);
        }
        json.AppendLine("  ]");
        json.AppendLine("}");
        File.WriteAllText(ReportPath, json.ToString());
    }

    private static void WriteMarkdown(Check[] checks, List<string> blockers, bool passed)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# M96 Final Humanoid Art Review Gate");
        markdown.AppendLine();
        markdown.AppendLine("Generated: " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        markdown.AppendLine();
        markdown.AppendLine("- Passed: `" + passed + "`");
        markdown.AppendLine("- Policy: skinned/imported/animated humanoid evidence is tracked separately from final humanoid art quality.");
        markdown.AppendLine();
        markdown.AppendLine("## Checks");
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
        markdown.AppendLine("## Blockers");
        markdown.AppendLine();
        if (blockers.Count == 0)
        {
            markdown.AppendLine("- None");
        }
        else
        {
            foreach (var blocker in blockers)
            {
                markdown.Append("- ").AppendLine(blocker);
            }
        }

        markdown.AppendLine();
        markdown.AppendLine("## Evidence Paths");
        markdown.AppendLine();
        foreach (var path in EvidencePaths)
        {
            markdown.Append("- `").Append(path).AppendLine("`");
        }

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

    private static void AppendArrayValue(StringBuilder json, string value, bool last, int indent)
    {
        json.Append(' ', indent).Append('"').Append(Escape(value)).Append('"');
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
