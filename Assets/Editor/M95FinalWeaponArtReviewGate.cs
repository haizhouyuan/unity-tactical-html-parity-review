#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class M95FinalWeaponArtReviewGate
{
    private const string ReportPath = "docs/M95_FINAL_WEAPON_ART_REVIEW_GATE.json";
    private const string MarkdownPath = "docs/M95_FINAL_WEAPON_ART_REVIEW_GATE.md";
    private const string WeaponFeelPath = "docs/WEAPON_FEEL_GATE.json";
    private const string ApprovalPath = "docs/M95_FINAL_WEAPON_ART_REVIEW_APPROVAL.json";
    private const string M91GatePath = "docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_GATE.json";
    private const string M91RoutePath = "docs/M91ExternalInputBuiltPlayerRoute/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json";
    private const string PlayableRoutePath = "docs/TACTICAL_PLAYABLE_ROUTE_GATE.json";
    private const string AcceptancePath = "docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json";
    private const string GlbPbrReviewPath = "docs/UNITY_GLB_PBR_VISUAL_REVIEW.json";
    private const string HeroRifleGlbPath = "Assets/HtmlTacticalAssets/RealifiedAssets/hero_rifle_LOD0.glb";
    private const string HeroRifleMaterialPath = "Assets/HtmlTacticalAssets/RealifiedAssets/hero_rifle_LOD0_PBR.mat";
    private const string HeroRifleTexturePrefix = "Assets/HtmlTacticalAssets/RealifiedAssets/hero_rifle_LOD0_";

    [MenuItem("AI Tools/Run M95 Final Weapon Art Review Gate")]
    public static void Run()
    {
        Directory.CreateDirectory("docs");

        var weaponFeel = ReadText(WeaponFeelPath);
        var approval = ReadText(ApprovalPath);
        var m91Gate = ReadText(M91GatePath);
        var m91Route = ReadText(M91RoutePath);
        var playableRoute = ReadText(PlayableRoutePath);
        var acceptance = ReadText(AcceptancePath);
        var glbPbrReview = ReadText(GlbPbrReviewPath);
        var evidence = BuildEvidencePaths(weaponFeel, m91Route);

        var weaponFeelScreenshots = ExtractStringArrayObjectValues(weaponFeel, "screenshots", "path");
        var weaponFeelScreenshotFilesExist = CountExistingPaths(weaponFeelScreenshots) >= 4;
        var m91Screenshots = ExtractStringArray(m91Route, "screenshots");
        var m91ScreenshotFilesExist = CountExistingPaths(m91Screenshots) >= 8;
        var cleanBuiltPlayerRoute = ExtractRootBool(m91Gate, "passed")
            && ExtractRootBool(m91Route, "passed")
            && ExtractBool(m91Route, "external_input_driven")
            && ExtractBool(m91Route, "built_player")
            && ExtractBool(m91Route, "fire_input_observed")
            && ExtractBool(m91Route, "ammo_state_changed")
            && ExtractBool(m91Route, "reload_state_changed")
            && ExtractBool(m91Route, "first_person_weapon_visible")
            && ExtractInt(m91Route, "screenshot_count") >= 8
            && m91ScreenshotFilesExist;

        var firstPersonFraming = ExtractRootBool(weaponFeel, "passed")
            && ExtractBool(weaponFeel, "m92_weapon_production_passed")
            && ExtractBool(weaponFeel, "first_person_weapon_visible")
            && ExtractBool(weaponFeel, "first_person_pose_quality_passed")
            && ExtractBool(weaponFeel, "ads_readable")
            && HasPathEnding(weaponFeelScreenshots, "01_first_person_ads_rifle.png")
            && weaponFeelScreenshotFilesExist;

        var thirdPersonMount = ExtractBool(weaponFeel, "third_person_weapon_mount")
            && ExtractBool(weaponFeel, "third_person_mount_quality_passed")
            && ExtractFloat(weaponFeel, "third_person_mount_quality_score") >= 0.45f
            && ExtractInt(weaponFeel, "third_person_fire_pulse_events") >= 1
            && HasPathEnding(weaponFeelScreenshots, "04_third_person_weapon_mount.png")
            && weaponFeelScreenshotFilesExist;

        var adsReloadFireFeedback = ExtractBool(weaponFeel, "fire_ammo_mutation")
            && ExtractBool(weaponFeel, "enemy_hit")
            && ExtractBool(weaponFeel, "weapon_feedback_spawned")
            && ExtractBool(weaponFeel, "recoil_polish_evidence")
            && ExtractBool(weaponFeel, "reload_state_mutation")
            && ExtractBool(weaponFeel, "recoil_peak_observed")
            && ExtractBool(weaponFeel, "reload_pose_magnitude_observed")
            && ExtractBool(weaponFeel, "ads_stability_observed")
            && ExtractInt(weaponFeel, "shot_feedback_event_count") >= 4
            && HasPathEnding(weaponFeelScreenshots, "02_first_person_fire_feedback.png")
            && HasPathEnding(weaponFeelScreenshots, "03_first_person_reload_state.png")
            && weaponFeelScreenshotFilesExist;

        var materialSidecar = FileExists(HeroRifleGlbPath)
            && FileExists(HeroRifleMaterialPath)
            && FileExists(HeroRifleTexturePrefix + "basecolor.png")
            && FileExists(HeroRifleTexturePrefix + "normal.png")
            && FileExists(HeroRifleTexturePrefix + "metallic.png")
            && FileExists(HeroRifleTexturePrefix + "roughness.png")
            && FileExists(HeroRifleTexturePrefix + "ao.png");

        var visibleScreenshots = weaponFeelScreenshotFilesExist
            && cleanBuiltPlayerRoute
            && HasPathEnding(weaponFeelScreenshots, "01_first_person_ads_rifle.png")
            && HasPathEnding(weaponFeelScreenshots, "02_first_person_fire_feedback.png")
            && HasPathEnding(weaponFeelScreenshots, "03_first_person_reload_state.png")
            && HasPathEnding(weaponFeelScreenshots, "04_third_person_weapon_mount.png");

        var gameplayBoundHeroWeapon = ExtractNestedBool(playableRoute, "asset_quality", "first_person_weapon_polish_passed")
            || ExtractBool(acceptance, "weapon_feel_gate_passed")
            || ExtractBool(acceptance, "m92_weapon_production_passed");

        var explicitFinalWeaponArtReview = ExtractRootBool(approval, "approved")
            && ExtractRootBool(approval, "player_camera_reviewed")
            && ExtractRootBool(approval, "first_person_reviewed")
            && ExtractRootBool(approval, "third_person_reviewed")
            && !ExtractRootBool(approval, "placeholder_or_procedural_blocker");
        var contactSheetOnlyWarningPresent = glbPbrReview.IndexOf("not proof that assets are bound to gameplay entities", StringComparison.OrdinalIgnoreCase) >= 0;
        var placeholderRiskCleared = explicitFinalWeaponArtReview
            && !ContainsAny(glbPbrReview, "procedural", "placeholder", "fallback");

        var checks = new[]
        {
            new Check("weapon_feel_gate_passed", ExtractRootBool(weaponFeel, "passed") && ExtractBool(weaponFeel, "m92_weapon_production_passed"), "M92 weapon feel gate passed in play mode."),
            new Check("first_person_weapon_framing_passed", firstPersonFraming, "First-person rifle framing and ADS readability are supported by screenshot evidence."),
            new Check("third_person_npc_mount_passed", thirdPersonMount, "Third-person/NPC rifle mount has quality and pulse evidence."),
            new Check("ads_reload_fire_feedback_passed", adsReloadFireFeedback, "ADS, reload, fire, recoil, muzzle/tracer/casing, hit, and ammo mutation evidence all exist."),
            new Check("material_pbr_sidecar_present", materialSidecar, "Hero rifle GLB, PBR material, and basecolor/normal/metallic/roughness/AO sidecars exist."),
            new Check("visible_screenshot_evidence_paths_exist", visibleScreenshots, "Required first-person, fire, reload, third-person, and external-input route screenshots exist on disk."),
            new Check("playable_route_weapon_evidence_passed", cleanBuiltPlayerRoute && gameplayBoundHeroWeapon, "Playable route evidence includes external-input built-player weapon use and gameplay-bound weapon polish."),
            new Check("explicit_final_weapon_art_review_approval", explicitFinalWeaponArtReview, "Final weapon art approval must come from docs/M95_FINAL_WEAPON_ART_REVIEW_APPROVAL.json, not renderer/material counts."),
            new Check("placeholder_procedural_block_risk_cleared", placeholderRiskCleared, "Placeholder/procedural-block risk must be cleared by final review and not by contact-sheet-only evidence.")
        };

        var blockers = BuildBlockers(checks);
        var passed = blockers.Count == 0;
        WriteJson(checks, blockers, evidence, passed, contactSheetOnlyWarningPresent);
        WriteMarkdown(checks, blockers, passed);

        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] M95 final weapon art review gate written to " + ReportPath + " passed=" + passed);
    }

    private static List<EvidencePath> BuildEvidencePaths(string weaponFeel, string m91Route)
    {
        var paths = new List<EvidencePath>
        {
            new EvidencePath(WeaponFeelPath),
            new EvidencePath(ApprovalPath),
            new EvidencePath(M91GatePath),
            new EvidencePath(M91RoutePath),
            new EvidencePath(PlayableRoutePath),
            new EvidencePath(AcceptancePath),
            new EvidencePath(GlbPbrReviewPath),
            new EvidencePath(HeroRifleGlbPath),
            new EvidencePath(HeroRifleMaterialPath),
            new EvidencePath(HeroRifleTexturePrefix + "basecolor.png"),
            new EvidencePath(HeroRifleTexturePrefix + "normal.png"),
            new EvidencePath(HeroRifleTexturePrefix + "metallic.png"),
            new EvidencePath(HeroRifleTexturePrefix + "roughness.png"),
            new EvidencePath(HeroRifleTexturePrefix + "ao.png")
        };

        foreach (var path in ExtractStringArrayObjectValues(weaponFeel, "screenshots", "path"))
        {
            paths.Add(new EvidencePath(path));
        }

        foreach (var path in ExtractStringArray(m91Route, "screenshots"))
        {
            paths.Add(new EvidencePath(path));
        }

        return paths;
    }

    private static List<string> BuildBlockers(Check[] checks)
    {
        var blockers = new List<string>();
        foreach (var check in checks)
        {
            if (!check.Passed)
            {
                blockers.Add(check.Key + ": " + check.Meaning);
            }
        }

        return blockers;
    }

    private static void WriteJson(Check[] checks, List<string> blockers, List<EvidencePath> evidence, bool passed, bool contactSheetOnlyWarningPresent)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "m95_final_weapon_art_review_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "passed", passed, true);
        Append(json, "policy", "Strict final weapon art readiness. M92 feel evidence is necessary but not sufficient; final quality cannot be claimed from renderer counts or contact sheets alone.", true);
        Append(json, "weapon_feel_gate_path", WeaponFeelPath, true);
        Append(json, "approval_path", ApprovalPath, true);
        Append(json, "m91_gate_path", M91GatePath, true);
        Append(json, "m91_route_path", M91RoutePath, true);
        Append(json, "playable_route_path", PlayableRoutePath, true);
        Append(json, "acceptance_report_path", AcceptancePath, true);
        Append(json, "glb_pbr_visual_review_path", GlbPbrReviewPath, true);
        Append(json, "contact_sheet_only_warning_present", contactSheetOnlyWarningPresent, true);
        AppendChecks(json, checks, true);
        AppendStringArray(json, "blockers", blockers, true);
        AppendEvidence(json, evidence, false);
        json.AppendLine("}");
        File.WriteAllText(ReportPath, json.ToString());
    }

    private static void WriteMarkdown(Check[] checks, List<string> blockers, bool passed)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# M95 Final Weapon Art Review Gate");
        markdown.AppendLine();
        markdown.AppendLine("Generated: " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        markdown.AppendLine();
        markdown.AppendLine("- Passed: `" + passed + "`");
        markdown.AppendLine("- Policy: final weapon art readiness requires gameplay evidence, visible screenshots, PBR sidecars, and explicit final art review clearance.");
        markdown.AppendLine();
        markdown.AppendLine("| Check | Passed | Meaning |");
        markdown.AppendLine("|---|---:|---|");
        foreach (var check in checks)
        {
            markdown.Append("| `").Append(check.Key).Append("` | ")
                .Append(check.Passed)
                .Append(" | ")
                .Append(check.Meaning)
                .AppendLine(" |");
        }

        markdown.AppendLine();
        markdown.AppendLine("## Blockers");
        if (blockers.Count == 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("- None.");
        }
        else
        {
            foreach (var blocker in blockers)
            {
                markdown.AppendLine("- " + blocker);
            }
        }

        File.WriteAllText(MarkdownPath, markdown.ToString());
    }

    private static string ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    private static bool FileExists(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        if (File.Exists(path))
        {
            return true;
        }

        var normalized = NormalizeLegacyProjectPath(path);
        return normalized != path && File.Exists(normalized);
    }

    private static string NormalizeLegacyProjectPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return "";
        }

        var marker = "/docs/";
        var index = path.IndexOf(marker, StringComparison.Ordinal);
        if (index >= 0)
        {
            return "docs/" + path.Substring(index + marker.Length);
        }

        marker = "/Assets/";
        index = path.IndexOf(marker, StringComparison.Ordinal);
        return index >= 0 ? "Assets/" + path.Substring(index + marker.Length) : path;
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
        return match.Success && int.TryParse(match.Groups["value"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }

    private static float ExtractFloat(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return 0f;
        }

        var match = Regex.Match(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*(?<value>-?\\d+(\\.\\d+)?)");
        return match.Success && float.TryParse(match.Groups["value"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0f;
    }

    private static bool ExtractNestedBool(string json, string objectKey, string key)
    {
        return ExtractBool(ExtractObjectBody(json, objectKey), key);
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

    private static List<string> ExtractStringArray(string json, string key)
    {
        var values = new List<string>();
        if (string.IsNullOrEmpty(json))
        {
            return values;
        }

        var arrayMatch = Regex.Match(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*\\[(?<body>.*?)\\]", RegexOptions.Singleline);
        if (!arrayMatch.Success)
        {
            return values;
        }

        foreach (Match match in Regex.Matches(arrayMatch.Groups["body"].Value, "\\\"(?<value>(?:\\\\.|[^\\\"])*)\\\""))
        {
            values.Add(UnescapeJsonString(match.Groups["value"].Value));
        }

        return values;
    }

    private static List<string> ExtractStringArrayObjectValues(string json, string arrayKey, string objectValueKey)
    {
        var values = new List<string>();
        if (string.IsNullOrEmpty(json))
        {
            return values;
        }

        var arrayMatch = Regex.Match(json, "\\\"" + Regex.Escape(arrayKey) + "\\\"\\s*:\\s*\\[(?<body>.*?)\\]", RegexOptions.Singleline);
        if (!arrayMatch.Success)
        {
            return values;
        }

        foreach (Match match in Regex.Matches(arrayMatch.Groups["body"].Value, "\\\"" + Regex.Escape(objectValueKey) + "\\\"\\s*:\\s*\\\"(?<value>(?:\\\\.|[^\\\"])*)\\\""))
        {
            values.Add(UnescapeJsonString(match.Groups["value"].Value));
        }

        return values;
    }

    private static string UnescapeJsonString(string value)
    {
        return (value ?? "").Replace("\\/", "/").Replace("\\\"", "\"").Replace("\\\\", "\\");
    }

    private static int CountExistingPaths(List<string> paths)
    {
        var count = 0;
        foreach (var path in paths)
        {
            if (FileExists(path))
            {
                count++;
            }
        }

        return count;
    }

    private static bool HasPathEnding(List<string> paths, string fileName)
    {
        foreach (var path in paths)
        {
            if (path.EndsWith(fileName, StringComparison.OrdinalIgnoreCase) && FileExists(path))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsAny(string text, params string[] terms)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        foreach (var term in terms)
        {
            if (text.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static void AppendChecks(StringBuilder json, Check[] checks, bool comma)
    {
        json.AppendLine("  \"checks\": [");
        for (var i = 0; i < checks.Length; i++)
        {
            json.AppendLine("    {");
            Append(json, "key", checks[i].Key, true, 6);
            Append(json, "passed", checks[i].Passed, true, 6);
            Append(json, "meaning", checks[i].Meaning, false, 6);
            json.Append("    }");
            json.AppendLine(i == checks.Length - 1 ? "" : ",");
        }
        json.AppendLine(comma ? "  ]," : "  ]");
    }

    private static void AppendEvidence(StringBuilder json, List<EvidencePath> evidence, bool comma)
    {
        json.AppendLine("  \"evidence_paths\": [");
        for (var i = 0; i < evidence.Count; i++)
        {
            json.AppendLine("    {");
            Append(json, "path", evidence[i].Path, true, 6);
            Append(json, "exists", FileExists(evidence[i].Path), false, 6);
            json.Append("    }");
            json.AppendLine(i == evidence.Count - 1 ? "" : ",");
        }
        json.AppendLine(comma ? "  ]," : "  ]");
    }

    private static void AppendStringArray(StringBuilder json, string key, List<string> values, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": [");
        for (var i = 0; i < values.Count; i++)
        {
            if (i > 0)
            {
                json.Append(", ");
            }
            json.Append('"').Append(Escape(values[i])).Append('"');
        }
        json.AppendLine(comma ? "]," : "]");
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

    private readonly struct EvidencePath
    {
        public EvidencePath(string path)
        {
            Path = path;
        }

        public readonly string Path;
    }
}
#endif
