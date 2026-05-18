#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

public static class M94GeneratedBatchClassPromotionGate
{
    private const string ReportPath = "docs/M94_GENERATED_BATCH_CLASS_PROMOTION_GATE.json";
    private const string MarkdownPath = "docs/M94_GENERATED_BATCH_CLASS_PROMOTION_GATE.md";
    private const string PromotionLedgerPath = "docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json";
    private const string PromotionQueuePath = "docs/REALIFIED_ASSET_CLASS_PROMOTION_QUEUE.json";
    private const string VisibilityGatePath = "docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json";
    private const string ProValidationPath = "docs/M94_PRO_BATCH_REFERENCE_IMAGE_VALIDATION.json";
    private const string ProOutputsRoot = "external/pro_outputs";

    private static readonly RequiredClass[] RequiredClasses =
    {
        new RequiredClass("weapon", "weapon"),
        new RequiredClass("humanoid", "character"),
        new RequiredClass("gear", "gear"),
        new RequiredClass("loot", "loot"),
        new RequiredClass("environment_prop", "environment_prop"),
    };

    [MenuItem("AI Tools/Run M94 Generated Batch Class Promotion Gate")]
    public static void Run()
    {
        Directory.CreateDirectory("docs");

        var ledgerJson = ReadText(PromotionLedgerPath);
        var queueJson = ReadText(PromotionQueuePath);
        var visibilityJson = ReadText(VisibilityGatePath);
        var proValidationJson = ReadText(ProValidationPath);
        var results = RequiredClasses
            .Select(requiredClass => BuildClassResult(requiredClass, ledgerJson, queueJson, visibilityJson, proValidationJson))
            .ToArray();
        var blockers = results
            .Where(result => !result.ProductionPromoted)
            .SelectMany(result => result.Blockers.Select(blocker => result.Name + ":" + blocker))
            .ToArray();
        var passed = results.All(result => result.ProductionPromoted);

        WriteJson(results, blockers, passed);
        WriteMarkdown(results, blockers, passed);

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("[AI Tools] M94 generated batch class promotion gate written to " + ReportPath + " passed=" + passed);
    }

    private static ClassResult BuildClassResult(RequiredClass requiredClass, string ledgerJson, string queueJson, string visibilityJson, string proValidationJson)
    {
        var queueBody = ExtractObjectWithCategory(queueJson, "classes", "category", requiredClass.SourceCategory);
        var ledgerAssets = ExtractObjectsWithCategory(ledgerJson, "assets", "category", requiredClass.SourceCategory);
        var m94LedgerAssets = ledgerAssets
            .Where(IsM94GeneratedBatchAsset)
            .ToArray();
        var visibilityBody = ExtractObjectWithCategory(visibilityJson, "classes", "category", requiredClass.SourceCategory);
        var assetIds = ExtractAssetIds(queueBody, m94LedgerAssets);
        var legacyPromotedAssetIds = ledgerAssets
            .Where(asset => ExtractBool(asset, "production_promoted") && !IsM94GeneratedBatchAsset(asset))
            .Select(asset => ExtractString(asset, "asset_id"))
            .Where(assetId => !string.IsNullOrEmpty(assetId))
            .ToArray();
        var referenceImageCount = CountProReferenceFiles(requiredClass.Name);
        var proReferenceValidationPassed = ExtractRootBool(proValidationJson, "passed");
        var m94GeneratedBatchAssetFound = m94LedgerAssets.Length > 0;
        var candidate = m94GeneratedBatchAssetFound;
        var imported = m94LedgerAssets.Any(asset => ExtractBool(asset, "imported"));
        var semanticReviewPassed = m94LedgerAssets.Any(asset => ExtractBool(asset, "semantic_category_match"));
        var gameplayBound = m94LedgerAssets.Any(asset => ExtractBool(asset, "gameplay_entity_scene_evidence"));
        var playerCameraVisible = m94LedgerAssets.Any(asset => ExtractBool(asset, "player_camera_realified_asset_evidence"))
            && ExtractBool(visibilityBody, "visible_promoted");
        var gameplayEventProven = m94LedgerAssets.Any(asset => ExtractBool(asset, "asset_specific_gameplay_event_evidence"));
        var productionPromoted = m94LedgerAssets.Any(asset => ExtractBool(asset, "production_promoted"));
        var promotionState = productionPromoted
            ? "production_promoted"
            : referenceImageCount > 0 && !m94GeneratedBatchAssetFound
                ? proReferenceValidationPassed ? "validated_reference_image_only" : "unvalidated_reference_image_only"
                : candidate
                    ? "candidate_blocked"
                    : "missing_candidate";
        var promotedAssetIds = m94LedgerAssets
            .Where(asset => ExtractBool(asset, "production_promoted"))
            .Select(asset => ExtractString(asset, "asset_id"))
            .Where(assetId => !string.IsNullOrEmpty(assetId))
            .ToArray();
        var blockers = BuildBlockers(candidate, imported, semanticReviewPassed, gameplayBound, playerCameraVisible, gameplayEventProven, productionPromoted, referenceImageCount, proReferenceValidationPassed, m94GeneratedBatchAssetFound, legacyPromotedAssetIds);

        return new ClassResult(
            requiredClass.Name,
            requiredClass.SourceCategory,
            assetIds,
            promotedAssetIds,
            legacyPromotedAssetIds,
            referenceImageCount > 0,
            referenceImageCount,
            proReferenceValidationPassed,
            m94GeneratedBatchAssetFound,
            candidate,
            imported,
            semanticReviewPassed,
            gameplayBound,
            playerCameraVisible,
            gameplayEventProven,
            productionPromoted,
            promotionState,
            blockers);
    }

    private static string[] BuildBlockers(
        bool candidate,
        bool imported,
        bool semanticReviewPassed,
        bool gameplayBound,
        bool playerCameraVisible,
        bool gameplayEventProven,
        bool productionPromoted,
        int referenceImageCount,
        bool proReferenceValidationPassed,
        bool m94GeneratedBatchAssetFound,
        string[] legacyPromotedAssetIds)
    {
        if (productionPromoted)
        {
            return Array.Empty<string>();
        }

        var blockers = new List<string>();
        if (referenceImageCount > 0)
        {
            blockers.Add("pro_outputs_are_quarantine_reference_image_only");
        }

        if (referenceImageCount > 0 && !proReferenceValidationPassed)
        {
            blockers.Add("pro_reference_images_not_validated");
        }

        if (legacyPromotedAssetIds.Length > 0)
        {
            blockers.Add("legacy_production_promotions_do_not_count_for_m94_batch");
        }

        if (!m94GeneratedBatchAssetFound)
        {
            blockers.Add("missing_m94_generated_batch_asset_marker");
        }

        if (!candidate)
        {
            blockers.Add("missing_generated_batch_candidate");
        }

        if (!imported)
        {
            blockers.Add("no_unity_import_evidence");
        }

        if (!semanticReviewPassed)
        {
            blockers.Add("no_passing_semantic_review_evidence");
        }

        if (!gameplayBound)
        {
            blockers.Add("no_gameplay_binding_evidence");
        }

        if (!playerCameraVisible)
        {
            blockers.Add("no_player_camera_visibility_evidence");
        }

        if (!gameplayEventProven)
        {
            blockers.Add("no_gameplay_event_evidence");
        }

        blockers.Add("not_recorded_as_production_promoted_in_gameplay_ledger");
        return blockers.ToArray();
    }

    private static bool IsM94GeneratedBatchAsset(string assetJson)
    {
        if (string.IsNullOrEmpty(assetJson))
        {
            return false;
        }

        return ExtractBool(assetJson, "m94_generated_batch")
            || string.Equals(ExtractString(assetJson, "generation_batch"), "M94", StringComparison.OrdinalIgnoreCase)
            || ExtractString(assetJson, "source_batch_id").StartsWith("M94", StringComparison.OrdinalIgnoreCase)
            || ExtractString(assetJson, "source_pipeline").IndexOf("m94", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string[] ExtractAssetIds(string queueBody, string[] ledgerAssets)
    {
        var assetIds = new List<string>();
        assetIds.AddRange(ExtractStringArray(queueBody, "asset_ids"));
        foreach (var asset in ledgerAssets)
        {
            var assetId = ExtractString(asset, "asset_id");
            if (!string.IsNullOrEmpty(assetId))
            {
                assetIds.Add(assetId);
            }
        }

        return assetIds.Distinct().OrderBy(assetId => assetId, StringComparer.Ordinal).ToArray();
    }

    private static int CountProReferenceFiles(string className)
    {
        if (!Directory.Exists(ProOutputsRoot))
        {
            return 0;
        }

        return Directory.GetFiles(ProOutputsRoot, "*", SearchOption.AllDirectories)
            .Count(path => IsReferenceFileForClass(path, className));
    }

    private static bool IsReferenceFileForClass(string path, string className)
    {
        var extension = Path.GetExtension(path);
        if (!string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(extension, ".webp", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var normalized = path.Replace('\\', '/');
        return normalized.IndexOf("/" + className + "/", StringComparison.OrdinalIgnoreCase) >= 0
            || Path.GetFileNameWithoutExtension(path).IndexOf("M94_" + className + "_", StringComparison.OrdinalIgnoreCase) >= 0
            || Path.GetFileNameWithoutExtension(path).IndexOf(className + "_contact_sheet", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string[] ExtractObjectsWithCategory(string json, string arrayKey, string categoryKey, string category)
    {
        return ExtractArrayObjects(json, arrayKey)
            .Where(body => string.Equals(ExtractString(body, categoryKey), category, StringComparison.Ordinal))
            .ToArray();
    }

    private static string ExtractObjectWithCategory(string json, string arrayKey, string categoryKey, string category)
    {
        return ExtractObjectsWithCategory(json, arrayKey, categoryKey, category).FirstOrDefault() ?? "";
    }

    private static string[] ExtractArrayObjects(string json, string arrayKey)
    {
        var arrayBody = ExtractArrayBody(json, arrayKey);
        if (string.IsNullOrEmpty(arrayBody))
        {
            return Array.Empty<string>();
        }

        var objects = new List<string>();
        var depth = 0;
        var start = -1;
        for (var i = 0; i < arrayBody.Length; i++)
        {
            if (arrayBody[i] == '{')
            {
                if (depth == 0)
                {
                    start = i;
                }

                depth++;
            }
            else if (arrayBody[i] == '}')
            {
                depth--;
                if (depth == 0 && start >= 0)
                {
                    objects.Add(arrayBody.Substring(start, i - start + 1));
                    start = -1;
                }
            }
        }

        return objects.ToArray();
    }

    private static string ExtractArrayBody(string json, string arrayKey)
    {
        if (string.IsNullOrEmpty(json))
        {
            return "";
        }

        var marker = "\"" + arrayKey + "\"";
        var keyIndex = json.IndexOf(marker, StringComparison.Ordinal);
        if (keyIndex < 0)
        {
            return "";
        }

        var start = json.IndexOf('[', keyIndex);
        if (start < 0)
        {
            return "";
        }

        var depth = 0;
        for (var i = start; i < json.Length; i++)
        {
            if (json[i] == '[')
            {
                depth++;
            }
            else if (json[i] == ']')
            {
                depth--;
                if (depth == 0)
                {
                    return json.Substring(start + 1, i - start - 1);
                }
            }
        }

        return "";
    }

    private static string[] ExtractStringArray(string json, string key)
    {
        var body = ExtractArrayBody(json, key);
        if (string.IsNullOrEmpty(body))
        {
            return Array.Empty<string>();
        }

        return Regex.Matches(body, "\\\"(?<value>(?:\\\\.|[^\\\"])*)\\\"")
            .Cast<Match>()
            .Select(match => Regex.Unescape(match.Groups["value"].Value))
            .ToArray();
    }

    private static bool ExtractBool(string json, string key)
    {
        return !string.IsNullOrEmpty(json) && Regex.IsMatch(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*true");
    }

    private static bool ExtractRootBool(string json, string key)
    {
        return JsonGateReader.RootBool(json, key);
    }

    private static string ExtractString(string json, string key)
    {
        var match = Regex.Match(json ?? "", "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*\\\"(?<value>(?:\\\\.|[^\\\"])*)\\\"", RegexOptions.Singleline);
        return match.Success ? Regex.Unescape(match.Groups["value"].Value) : "";
    }

    private static string ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    private static void WriteJson(ClassResult[] results, string[] blockers, bool passed)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "m94_generated_batch_class_promotion_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "passed", passed, true);
        AppendStringArray(json, "required_classes", RequiredClasses.Select(requiredClass => requiredClass.Name).ToArray(), true);
        Append(json, "gameplay_promotion_ledger_path", PromotionLedgerPath, true);
        Append(json, "promotion_queue_path", PromotionQueuePath, true);
        Append(json, "visibility_gate_path", VisibilityGatePath, true);
        Append(json, "pro_reference_validation_path", ProValidationPath, true);
        Append(json, "pro_outputs_root", ProOutputsRoot, true);
        Append(json, "pro_outputs_policy", "reference_image_only_quarantine_never_production_promotion", true);
        json.AppendLine("  \"class_results\": [");
        for (var i = 0; i < results.Length; i++)
        {
            AppendClassResult(json, results[i], i == results.Length - 1);
        }

        json.AppendLine("  ],");
        AppendStringArray(json, "blockers", blockers, false);
        json.AppendLine("}");
        File.WriteAllText(ReportPath, json.ToString());
    }

    private static void AppendClassResult(StringBuilder json, ClassResult result, bool last)
    {
        json.AppendLine("    {");
        Append(json, "class", result.Name, true, 6);
        Append(json, "source_report_category", result.SourceCategory, true, 6);
        AppendStringArray(json, "asset_ids", result.AssetIds, true, 6);
        AppendStringArray(json, "promoted_asset_ids", result.PromotedAssetIds, true, 6);
        AppendStringArray(json, "legacy_promoted_asset_ids_not_counted", result.LegacyPromotedAssetIds, true, 6);
        Append(json, "reference_image_only", result.ReferenceImageOnly, true, 6);
        Append(json, "reference_image_count", result.ReferenceImageCount, true, 6);
        Append(json, "pro_reference_validation_passed", result.ProReferenceValidationPassed, true, 6);
        Append(json, "m94_generated_batch_asset_found", result.M94GeneratedBatchAssetFound, true, 6);
        Append(json, "candidate", result.Candidate, true, 6);
        Append(json, "imported", result.Imported, true, 6);
        Append(json, "semantic_review_passed", result.SemanticReviewPassed, true, 6);
        Append(json, "gameplay_bound", result.GameplayBound, true, 6);
        Append(json, "player_camera_visible", result.PlayerCameraVisible, true, 6);
        Append(json, "gameplay_event_proven", result.GameplayEventProven, true, 6);
        Append(json, "production_promoted", result.ProductionPromoted, true, 6);
        Append(json, "promotion_state", result.PromotionState, true, 6);
        AppendStringArray(json, "blockers", result.Blockers, false, 6);
        json.Append("    }");
        json.AppendLine(last ? "" : ",");
    }

    private static void WriteMarkdown(ClassResult[] results, string[] blockers, bool passed)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# M94 Generated Batch Class Promotion Gate");
        markdown.AppendLine();
        markdown.AppendLine("Generated: " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        markdown.AppendLine();
        markdown.AppendLine("- Passed: `" + passed + "`");
        markdown.AppendLine("- Policy: files, contact sheets, and Pro image outputs are not production promotion evidence.");
        markdown.AppendLine("- Pro outputs under `" + ProOutputsRoot + "` are counted only as `reference_image_only` quarantine inputs.");
        markdown.AppendLine("- Existing legacy promotion evidence is listed but cannot satisfy this M94 batch gate without an M94/source-batch marker.");
        markdown.AppendLine();
        markdown.AppendLine("| Class | Reference Only | M94 Asset | Candidate | Imported | Semantic | Gameplay Bound | Player Camera | Gameplay Event | Promoted | Blockers |");
        markdown.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|");
        foreach (var result in results)
        {
            markdown.Append("| ")
                .Append(result.Name)
                .Append(" | ")
                .Append(result.ReferenceImageOnly)
                .Append(" | ")
                .Append(result.M94GeneratedBatchAssetFound)
                .Append(" | ")
                .Append(result.Candidate)
                .Append(" | ")
                .Append(result.Imported)
                .Append(" | ")
                .Append(result.SemanticReviewPassed)
                .Append(" | ")
                .Append(result.GameplayBound)
                .Append(" | ")
                .Append(result.PlayerCameraVisible)
                .Append(" | ")
                .Append(result.GameplayEventProven)
                .Append(" | ")
                .Append(result.ProductionPromoted)
                .Append(" | ")
                .Append(result.Blockers.Length == 0 ? "" : string.Join(", ", result.Blockers))
                .AppendLine(" |");
        }

        if (blockers.Length > 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("## Blockers");
            markdown.AppendLine();
            foreach (var blocker in blockers)
            {
                markdown.Append("- `").Append(blocker).AppendLine("`");
            }
        }

        File.WriteAllText(MarkdownPath, markdown.ToString());
    }

    private static void Append(StringBuilder json, string key, string value, bool comma, int indent = 2)
    {
        json.Append(' ', indent).Append('"').Append(key).Append("\": \"").Append(Escape(value)).Append('"').AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, int value, bool comma, int indent = 2)
    {
        json.Append(' ', indent).Append('"').Append(key).Append("\": ").Append(value.ToString(CultureInfo.InvariantCulture)).AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, bool value, bool comma, int indent = 2)
    {
        json.Append(' ', indent).Append('"').Append(key).Append("\": ").Append(value ? "true" : "false").AppendLine(comma ? "," : "");
    }

    private static void AppendStringArray(StringBuilder json, string key, string[] values, bool comma, int indent = 2)
    {
        json.Append(' ', indent).Append('"').Append(key).Append("\": [");
        for (var i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                json.Append(", ");
            }

            json.Append('"').Append(Escape(values[i])).Append('"');
        }

        json.Append(']').AppendLine(comma ? "," : "");
    }

    private static string Escape(string value)
    {
        return (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
    }

    private readonly struct RequiredClass
    {
        public RequiredClass(string name, string sourceCategory)
        {
            Name = name;
            SourceCategory = sourceCategory;
        }

        public readonly string Name;
        public readonly string SourceCategory;
    }

    private readonly struct ClassResult
    {
        public ClassResult(
            string name,
            string sourceCategory,
            string[] assetIds,
            string[] promotedAssetIds,
            string[] legacyPromotedAssetIds,
            bool referenceImageOnly,
            int referenceImageCount,
            bool proReferenceValidationPassed,
            bool m94GeneratedBatchAssetFound,
            bool candidate,
            bool imported,
            bool semanticReviewPassed,
            bool gameplayBound,
            bool playerCameraVisible,
            bool gameplayEventProven,
            bool productionPromoted,
            string promotionState,
            string[] blockers)
        {
            Name = name;
            SourceCategory = sourceCategory;
            AssetIds = assetIds;
            PromotedAssetIds = promotedAssetIds;
            LegacyPromotedAssetIds = legacyPromotedAssetIds;
            ReferenceImageOnly = referenceImageOnly;
            ReferenceImageCount = referenceImageCount;
            ProReferenceValidationPassed = proReferenceValidationPassed;
            M94GeneratedBatchAssetFound = m94GeneratedBatchAssetFound;
            Candidate = candidate;
            Imported = imported;
            SemanticReviewPassed = semanticReviewPassed;
            GameplayBound = gameplayBound;
            PlayerCameraVisible = playerCameraVisible;
            GameplayEventProven = gameplayEventProven;
            ProductionPromoted = productionPromoted;
            PromotionState = promotionState;
            Blockers = blockers;
        }

        public readonly string Name;
        public readonly string SourceCategory;
        public readonly string[] AssetIds;
        public readonly string[] PromotedAssetIds;
        public readonly string[] LegacyPromotedAssetIds;
        public readonly bool ReferenceImageOnly;
        public readonly int ReferenceImageCount;
        public readonly bool ProReferenceValidationPassed;
        public readonly bool M94GeneratedBatchAssetFound;
        public readonly bool Candidate;
        public readonly bool Imported;
        public readonly bool SemanticReviewPassed;
        public readonly bool GameplayBound;
        public readonly bool PlayerCameraVisible;
        public readonly bool GameplayEventProven;
        public readonly bool ProductionPromoted;
        public readonly string PromotionState;
        public readonly string[] Blockers;
    }
}
#endif
