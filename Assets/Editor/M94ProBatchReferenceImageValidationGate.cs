#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

/// <summary>
/// Validates the ChatGPT Pro M94-M96 reference-image bundle after Codex stages it
/// under external/pro_outputs/. This gate is intentionally reference-only:
/// it does not import GLBs, does not promote assets, and does not affect M88
/// strict visual pass/fail values.
/// </summary>
public static class M94ProBatchReferenceImageValidationGate
{
    private const string ReportPath = "docs/M94_PRO_BATCH_REFERENCE_IMAGE_VALIDATION.json";
    private const string MarkdownPath = "docs/M94_PRO_BATCH_REFERENCE_IMAGE_VALIDATION.md";
    private const string PreferredBundleRoot = "external/pro_outputs/m94_m96_batch_images_2026-05-19";
    private const string AlternateBundleRoot = "external/pro_outputs/pro_m94_m96_batch_images_2026-05-19";

    private static readonly string[] RequiredClasses =
    {
        "weapon",
        "humanoid",
        "gear",
        "loot",
        "environment_prop"
    };

    private static readonly RequiredImage[] FirstWave =
    {
        new RequiredImage("weapon", "hero_rifle", "front"),
        new RequiredImage("weapon", "hero_rifle", "side"),
        new RequiredImage("weapon", "hero_rifle", "three_quarter"),
        new RequiredImage("humanoid", "enemy_tactical", "front"),
        new RequiredImage("humanoid", "enemy_tactical", "back"),
        new RequiredImage("humanoid", "enemy_tactical", "three_quarter"),
        new RequiredImage("gear", "vest", "front"),
        new RequiredImage("gear", "vest", "side"),
        new RequiredImage("gear", "vest", "three_quarter"),
        new RequiredImage("loot", "medkit", "top"),
        new RequiredImage("loot", "medkit", "front"),
        new RequiredImage("loot", "medkit", "three_quarter"),
        new RequiredImage("environment_prop", "shipping_container", "front"),
        new RequiredImage("environment_prop", "shipping_container", "side"),
        new RequiredImage("environment_prop", "shipping_container", "three_quarter"),
    };

    [MenuItem("AI Tools/Run M94 Pro Batch Reference Image Validation")]
    public static void Run()
    {
        Directory.CreateDirectory("docs");

        var root = ResolveBundleRoot();
        var manifestPath = string.IsNullOrEmpty(root) ? "" : Path.Combine(root, "manifest.json");
        var manifest = File.Exists(manifestPath) ? File.ReadAllText(manifestPath) : "";

        var blockers = new StringBuilder();
        var imageCount = CountManifestEntries(manifest);
        var referenceOnly = imageCount > 0
            && CountLiteral(manifest, "\"usage\"", "\"reference_image_only\"") >= imageCount
            && CountLiteral(manifest, "\"production_status\"", "\"quarantine_reference\"") >= imageCount;
        var firstWavePresent = true;
        foreach (var required in FirstWave)
        {
            var present = ManifestHasEntry(manifest, required.ClassName, required.AssetId, required.View)
                && File.Exists(Path.Combine(root ?? "", "images", required.ClassName, "M94_" + required.ClassName + "_" + required.AssetId + "_" + required.View + "_v01.png"));
            if (!present)
            {
                firstWavePresent = false;
                AppendBlocker(blockers, "missing_first_wave_image:" + required.ClassName + "/" + required.AssetId + "/" + required.View);
            }
        }

        var contactSheetsPresent = true;
        foreach (var className in RequiredClasses)
        {
            if (!File.Exists(Path.Combine(root ?? "", "contact_sheets", className + "_contact_sheet.png")))
            {
                contactSheetsPresent = false;
                AppendBlocker(blockers, "missing_contact_sheet:" + className);
            }
        }

        var promptPacksPresent = true;
        foreach (var className in RequiredClasses)
        {
            if (!File.Exists(Path.Combine(root ?? "", "prompts", className + "_prompts.md")))
            {
                promptPacksPresent = false;
                AppendBlocker(blockers, "missing_prompt_pack:" + className);
            }
        }

        if (string.IsNullOrEmpty(root))
        {
            AppendBlocker(blockers, "bundle_root_missing");
        }

        if (!File.Exists(manifestPath))
        {
            AppendBlocker(blockers, "manifest_missing");
        }

        if (imageCount < FirstWave.Length)
        {
            AppendBlocker(blockers, "manifest_image_count_below_first_wave");
        }

        if (!referenceOnly)
        {
            AppendBlocker(blockers, "manifest_entries_not_all_reference_only_quarantine");
        }

        var passed = !string.IsNullOrEmpty(root)
            && File.Exists(manifestPath)
            && imageCount >= FirstWave.Length
            && referenceOnly
            && firstWavePresent
            && contactSheetsPresent
            && promptPacksPresent;

        WriteJson(root, manifestPath, imageCount, referenceOnly, firstWavePresent, contactSheetsPresent, promptPacksPresent, blockers.ToString(), passed);
        WriteMarkdown(root, manifestPath, imageCount, referenceOnly, firstWavePresent, contactSheetsPresent, promptPacksPresent, blockers.ToString(), passed);
        AssetDatabase.Refresh();

        UnityEngine.Debug.Log("[AI Tools] M94 Pro batch reference image validation written to " + ReportPath + " passed=" + passed);
    }

    private static string ResolveBundleRoot()
    {
        if (Directory.Exists(PreferredBundleRoot))
        {
            return PreferredBundleRoot;
        }

        if (Directory.Exists(AlternateBundleRoot))
        {
            return AlternateBundleRoot;
        }

        return "";
    }

    private static int CountManifestEntries(string manifest)
    {
        if (string.IsNullOrEmpty(manifest))
        {
            return 0;
        }

        return Regex.Matches(manifest, "\"image_file\"\\s*:").Count;
    }

    private static bool ManifestHasEntry(string manifest, string className, string assetId, string view)
    {
        if (string.IsNullOrEmpty(manifest))
        {
            return false;
        }

        var expectedPath = "images/" + className + "/M94_" + className + "_" + assetId + "_" + view + "_v01.png";
        return manifest.IndexOf("\"image_file\"\\s*:", StringComparison.Ordinal) >= 0
            ? Regex.IsMatch(manifest, "\"image_file\"\\s*:\\s*\"" + Regex.Escape(expectedPath) + "\"")
            : manifest.IndexOf(expectedPath, StringComparison.Ordinal) >= 0;
    }

    private static int CountLiteral(string manifest, string keyLiteral, string valueLiteral)
    {
        if (string.IsNullOrEmpty(manifest))
        {
            return 0;
        }

        return Regex.Matches(manifest, Regex.Escape(keyLiteral) + "\\s*:\\s*" + Regex.Escape(valueLiteral)).Count;
    }

    private static void AppendBlocker(StringBuilder blockers, string blocker)
    {
        if (blockers.Length > 0)
        {
            blockers.AppendLine();
        }

        blockers.Append(blocker);
    }

    private static void WriteJson(string root, string manifestPath, int imageCount, bool referenceOnly, bool firstWavePresent, bool contactSheetsPresent, bool promptPacksPresent, string blockers, bool passed)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "m94_pro_batch_reference_image_validation_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "passed", passed, true);
        Append(json, "usage", "reference_image_only", true);
        Append(json, "production_status", "quarantine_reference", true);
        Append(json, "bundle_root", root, true);
        Append(json, "manifest_path", manifestPath, true);
        Append(json, "manifest_image_count", imageCount, true);
        Append(json, "first_wave_required_count", FirstWave.Length, true);
        Append(json, "first_wave_present", firstWavePresent, true);
        Append(json, "contact_sheets_present", contactSheetsPresent, true);
        Append(json, "prompt_packs_present", promptPacksPresent, true);
        Append(json, "all_entries_reference_only_quarantine", referenceOnly, true);
        AppendArray(json, "blockers", blockers, true);
        Append(json, "note", "Reference images are quarantine inputs only. This validation never counts images as production assets.", false);
        json.AppendLine("}");
        File.WriteAllText(ReportPath, json.ToString());
    }

    private static void WriteMarkdown(string root, string manifestPath, int imageCount, bool referenceOnly, bool firstWavePresent, bool contactSheetsPresent, bool promptPacksPresent, string blockers, bool passed)
    {
        var md = new StringBuilder();
        md.AppendLine("# M94 Pro Batch Reference Image Validation");
        md.AppendLine();
        md.AppendLine("Generated: " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        md.AppendLine();
        md.AppendLine("- Passed: `" + passed + "`");
        md.AppendLine("- Bundle root: `" + EscapeMarkdown(root) + "`");
        md.AppendLine("- Manifest: `" + EscapeMarkdown(manifestPath) + "`");
        md.AppendLine("- Manifest image count: `" + imageCount + "`");
        md.AppendLine("- First-wave present: `" + firstWavePresent + "`");
        md.AppendLine("- Contact sheets present: `" + contactSheetsPresent + "`");
        md.AppendLine("- Prompt packs present: `" + promptPacksPresent + "`");
        md.AppendLine("- All entries reference-only quarantine: `" + referenceOnly + "`");
        md.AppendLine();
        md.AppendLine("These images are not production assets and must remain quarantine references until local image-to-3D, cleanup, Unity import, gameplay binding, player-camera visibility, gameplay event evidence, and ledger promotion pass.");
        md.AppendLine();
        md.AppendLine("## Blockers");
        md.AppendLine();
        if (string.IsNullOrWhiteSpace(blockers))
        {
            md.AppendLine("- none");
        }
        else
        {
            foreach (var line in blockers.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                md.AppendLine("- " + line.Trim());
            }
        }

        File.WriteAllText(MarkdownPath, md.ToString());
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

    private static void AppendArray(StringBuilder json, string key, string lines, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": [");
        var split = string.IsNullOrWhiteSpace(lines) ? Array.Empty<string>() : lines.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < split.Length; i++)
        {
            if (i > 0)
            {
                json.Append(", ");
            }

            json.Append("\"").Append(Escape(split[i].Trim())).Append("\"");
        }

        json.Append("]");
        json.AppendLine(comma ? "," : "");
    }

    private static string Escape(string value)
    {
        return (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
    }

    private static string EscapeMarkdown(string value)
    {
        return (value ?? "").Replace("`", "\\`");
    }

    private readonly struct RequiredImage
    {
        public RequiredImage(string className, string assetId, string view)
        {
            ClassName = className;
            AssetId = assetId;
            View = view;
        }

        public readonly string ClassName;
        public readonly string AssetId;
        public readonly string View;
    }
}
#endif
