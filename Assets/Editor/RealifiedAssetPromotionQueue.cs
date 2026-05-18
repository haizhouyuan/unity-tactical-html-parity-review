#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

public static class RealifiedAssetPromotionQueue
{
    private const string ReportPath = "docs/REALIFIED_ASSET_CLASS_PROMOTION_QUEUE.json";
    private const string MarkdownPath = "docs/REALIFIED_ASSET_CLASS_PROMOTION_QUEUE.md";
    private const string ImportGatePath = "docs/REALIFIED_IMPORT_MATERIAL_GATE.json";
    private const string CategoryReviewPath = "docs/REALIFIED_CATEGORY_NEMOTRON_REVIEWS.json";
    private const string RouteGatePath = "docs/TACTICAL_PLAYABLE_ROUTE_GATE.json";

    private static readonly ClassSpec[] Classes =
    {
        new ClassSpec("weapon", new[] { "hero_rifle", "sidearm", "secondary_weapon", "shotgun" },
            new[] { "realified_sidearm_promotion_scene_instances", "realified_hero_rifle_promotion_scene_instances", "realified_secondary_weapon_promotion_scene_instances" }),
        new ClassSpec("character", new[] { "player_tactical", "enemy_tactical" }, Array.Empty<string>()),
        new ClassSpec("gear", new[] { "helmet", "vest" },
            new[] { "realified_helmet_promotion_scene_instances", "realified_vest_promotion_scene_instances" }),
        new ClassSpec("loot", new[] { "ammo", "medkit" },
            new[] { "realified_ammo_loot_promotion_scene_instances", "realified_medkit_loot_promotion_scene_instances" }),
        new ClassSpec("environment_prop", new[] { "container", "crate" },
            new[] { "realified_container_promotion_scene_instances", "realified_crate_promotion_scene_instances" }),
    };

    [MenuItem("AI Tools/Write Realified Asset Class Promotion Queue")]
    public static void WritePromotionQueue()
    {
        Directory.CreateDirectory("docs");

        var importJson = ReadText(ImportGatePath);
        var categoryJson = ReadText(CategoryReviewPath);
        var routeJson = ReadText(RouteGatePath);
        var results = Classes.Select(spec => BuildResult(spec, importJson, categoryJson, routeJson)).ToArray();
        var promotedCount = results.Count(result => result.ProductionPromoted);
        var technicallyReadyCount = results.Count(result => result.TechnicalReadyAll);
        var semanticPassedCount = results.Count(result => result.SemanticAllowed);
        var sceneEvidenceCount = results.Count(result => result.GameplaySceneEvidencePresent);

        WriteJson(results, promotedCount, technicallyReadyCount, semanticPassedCount, sceneEvidenceCount);
        WriteMarkdown(results, promotedCount, technicallyReadyCount, semanticPassedCount, sceneEvidenceCount);

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("[AI Tools] Realified asset class promotion queue written to " + ReportPath);
    }

    private static ClassResult BuildResult(ClassSpec spec, string importJson, string categoryJson, string routeJson)
    {
        var technicalReadyAssets = spec.AssetIds.Count(assetId => ExtractAssetBool(importJson, assetId, "technical_import_ready"));
        var importedAssets = spec.AssetIds.Count(assetId => ExtractAssetBool(importJson, assetId, "lod0_imported_prefab"));
        var semanticRendered = ExtractRenderedCategory(categoryJson, spec.Category);
        var semanticAllowed = semanticRendered && !ExtractFailedCategories(categoryJson).Contains(spec.Category);
        var sceneEvidenceInstances = spec.SceneEvidenceKeys.Sum(key => ExtractInt(routeJson, key));
        var gameplaySceneEvidencePresent = spec.SceneEvidenceKeys.Length > 0 && spec.SceneEvidenceKeys.All(key => ExtractInt(routeJson, key) > 0);
        var technicalReadyAll = technicalReadyAssets == spec.AssetIds.Length;
        var candidateImported = importedAssets == spec.AssetIds.Length;
        var blockers = BuildBlockers(spec, candidateImported, technicalReadyAll, semanticRendered, semanticAllowed, gameplaySceneEvidencePresent);
        var productionPromoted = technicalReadyAll && semanticAllowed && gameplaySceneEvidencePresent;
        var state = productionPromoted
            ? "production_promoted"
            : candidateImported
                ? "candidate_imported_blocked"
                : "missing_import";

        return new ClassResult(
            spec,
            candidateImported,
            importedAssets,
            technicalReadyAll,
            technicalReadyAssets,
            semanticRendered,
            semanticAllowed,
            gameplaySceneEvidencePresent,
            sceneEvidenceInstances,
            productionPromoted,
            state,
            blockers);
    }

    private static string[] BuildBlockers(ClassSpec spec, bool candidateImported, bool technicalReadyAll, bool semanticRendered, bool semanticAllowed, bool gameplaySceneEvidencePresent)
    {
        var blockers = new StringBuilder();
        AppendBlocker(blockers, candidateImported ? "" : "missing_lod0_import_for_one_or_more_assets");
        AppendBlocker(blockers, technicalReadyAll ? "" : "technical_import_or_pbr_sidecar_incomplete");
        AppendBlocker(blockers, semanticRendered ? "" : "semantic_contact_sheet_missing");
        AppendBlocker(blockers, semanticAllowed ? "" : "semantic_category_review_failed");
        if (spec.SceneEvidenceKeys.Length == 0)
        {
            AppendBlocker(blockers, "no_realified_gameplay_scene_evidence_key_defined");
        }
        else
        {
            AppendBlocker(blockers, gameplaySceneEvidencePresent ? "" : "gameplay_scene_evidence_missing_or_partial");
        }

        return blockers.Length == 0 ? Array.Empty<string>() : blockers.ToString().Split('|');
    }

    private static void AppendBlocker(StringBuilder blockers, string blocker)
    {
        if (string.IsNullOrEmpty(blocker))
        {
            return;
        }

        if (blockers.Length > 0)
        {
            blockers.Append('|');
        }

        blockers.Append(blocker);
    }

    private static void WriteJson(ClassResult[] results, int promotedCount, int technicallyReadyCount, int semanticPassedCount, int sceneEvidenceCount)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "realified_asset_class_promotion_queue_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "import_gate_path", ImportGatePath, true);
        Append(json, "category_review_path", CategoryReviewPath, true);
        Append(json, "route_gate_path", RouteGatePath, true);
        json.AppendLine("  \"summary\": {");
        Append(json, "class_count", results.Length, true, 4);
        Append(json, "technically_ready_classes", technicallyReadyCount, true, 4);
        Append(json, "semantic_passed_classes", semanticPassedCount, true, 4);
        Append(json, "gameplay_scene_evidence_classes", sceneEvidenceCount, true, 4);
        Append(json, "production_promoted_classes", promotedCount, true, 4);
        Append(json, "any_production_promoted", promotedCount > 0, true, 4);
        Append(json, "all_classes_promoted", promotedCount == results.Length, true, 4);
        Append(json, "global_batch_promotion_allowed", promotedCount == results.Length, false, 4);
        json.AppendLine("  },");
        json.AppendLine("  \"classes\": [");
        for (var i = 0; i < results.Length; i++)
        {
            AppendClass(json, results[i], i == results.Length - 1);
        }
        json.AppendLine("  ]");
        json.AppendLine("}");
        File.WriteAllText(ReportPath, json.ToString());
    }

    private static void AppendClass(StringBuilder json, ClassResult result, bool last)
    {
        json.AppendLine("    {");
        Append(json, "category", result.Spec.Category, true, 6);
        AppendStringArray(json, "asset_ids", result.Spec.AssetIds, true, 6);
        Append(json, "candidate_imported", result.CandidateImported, true, 6);
        Append(json, "imported_asset_count", result.ImportedAssetCount, true, 6);
        Append(json, "technical_ready_all", result.TechnicalReadyAll, true, 6);
        Append(json, "technical_ready_asset_count", result.TechnicalReadyAssetCount, true, 6);
        Append(json, "semantic_contact_sheet_rendered", result.SemanticRendered, true, 6);
        Append(json, "semantic_allowed", result.SemanticAllowed, true, 6);
        Append(json, "gameplay_scene_evidence_present", result.GameplaySceneEvidencePresent, true, 6);
        Append(json, "gameplay_scene_evidence_instances", result.GameplaySceneEvidenceInstances, true, 6);
        Append(json, "production_promoted", result.ProductionPromoted, true, 6);
        Append(json, "promotion_state", result.State, true, 6);
        AppendStringArray(json, "blockers", result.Blockers, false, 6);
        json.Append("    }");
        json.AppendLine(last ? "" : ",");
    }

    private static void WriteMarkdown(ClassResult[] results, int promotedCount, int technicallyReadyCount, int semanticPassedCount, int sceneEvidenceCount)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# Realified Asset Class Promotion Queue");
        markdown.AppendLine();
        markdown.AppendLine("Generated: " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        markdown.AppendLine();
        markdown.AppendLine("## Summary");
        markdown.AppendLine();
        markdown.AppendLine("- Technically ready classes: `" + technicallyReadyCount + " / " + results.Length + "`");
        markdown.AppendLine("- Semantic-passed classes: `" + semanticPassedCount + " / " + results.Length + "`");
        markdown.AppendLine("- Gameplay scene evidence classes: `" + sceneEvidenceCount + " / " + results.Length + "`");
        markdown.AppendLine("- Production-promoted classes: `" + promotedCount + " / " + results.Length + "`");
        markdown.AppendLine("- Global batch promotion allowed: `" + (promotedCount == results.Length) + "`");
        markdown.AppendLine();
        markdown.AppendLine("This queue intentionally separates candidate import, technical readiness, semantic review, scene evidence, and production promotion.");
        markdown.AppendLine();
        markdown.AppendLine("## Classes");
        markdown.AppendLine();
        markdown.AppendLine("| Class | Imported | Technical Ready | Semantic Allowed | Scene Evidence | Production Promoted | Blockers |");
        markdown.AppendLine("|---|---:|---:|---:|---:|---:|---|");
        foreach (var result in results)
        {
            markdown.Append("| ")
                .Append(result.Spec.Category)
                .Append(" | ")
                .Append(result.CandidateImported)
                .Append(" | ")
                .Append(result.TechnicalReadyAll)
                .Append(" | ")
                .Append(result.SemanticAllowed)
                .Append(" | ")
                .Append(result.GameplaySceneEvidencePresent)
                .Append(" | ")
                .Append(result.ProductionPromoted)
                .Append(" | ")
                .Append(result.Blockers.Length == 0 ? "" : string.Join(", ", result.Blockers))
                .AppendLine(" |");
        }

        File.WriteAllText(MarkdownPath, markdown.ToString());
    }

    private static string ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    private static bool ExtractAssetBool(string json, string assetId, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        var match = Regex.Match(json,
            "\\\"asset_id\\\"\\s*:\\s*\\\"" + Regex.Escape(assetId) + "\\\"(?<body>.*?)\\\"semantic_promotion_checked_here\\\"",
            RegexOptions.Singleline);
        return match.Success && ExtractBool(match.Groups["body"].Value, key);
    }

    private static bool ExtractRenderedCategory(string json, string category)
    {
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        var match = Regex.Match(json, "\\\"rendered_categories\\\"\\s*:\\s*\\{(?<body>.*?)\\n\\s*\\}", RegexOptions.Singleline);
        return match.Success && ExtractBool(match.Groups["body"].Value, category);
    }

    private static string[] ExtractFailedCategories(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return Array.Empty<string>();
        }

        var match = Regex.Match(json, "\\\"failed_categories\\\"\\s*:\\s*\\[(?<body>.*?)\\]", RegexOptions.Singleline);
        if (!match.Success)
        {
            return Array.Empty<string>();
        }

        return Regex.Matches(match.Groups["body"].Value, "\\\"(?<value>(?:\\\\.|[^\\\"])*)\\\"")
            .Cast<Match>()
            .Select(value => Regex.Unescape(value.Groups["value"].Value))
            .ToArray();
    }

    private static bool ExtractBool(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        return Regex.IsMatch(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*true");
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

    private static void Append(StringBuilder json, string key, string value, bool comma, int indent = 2)
    {
        json.Append(' ', indent);
        json.Append('"').Append(key).Append("\": \"").Append(Escape(value)).Append('"');
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, int value, bool comma, int indent = 2)
    {
        json.Append(' ', indent);
        json.Append('"').Append(key).Append("\": ").Append(value.ToString(CultureInfo.InvariantCulture));
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, bool value, bool comma, int indent = 2)
    {
        json.Append(' ', indent);
        json.Append('"').Append(key).Append("\": ").Append(value ? "true" : "false");
        json.AppendLine(comma ? "," : "");
    }

    private static void AppendStringArray(StringBuilder json, string key, string[] values, bool comma, int indent = 2)
    {
        json.Append(' ', indent);
        json.Append('"').Append(key).Append("\": [");
        for (var i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                json.Append(", ");
            }

            json.Append('"').Append(Escape(values[i])).Append('"');
        }

        json.Append(']');
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
        public ClassSpec(string category, string[] assetIds, string[] sceneEvidenceKeys)
        {
            Category = category;
            AssetIds = assetIds;
            SceneEvidenceKeys = sceneEvidenceKeys;
        }

        public readonly string Category;
        public readonly string[] AssetIds;
        public readonly string[] SceneEvidenceKeys;
    }

    private readonly struct ClassResult
    {
        public ClassResult(
            ClassSpec spec,
            bool candidateImported,
            int importedAssetCount,
            bool technicalReadyAll,
            int technicalReadyAssetCount,
            bool semanticRendered,
            bool semanticAllowed,
            bool gameplaySceneEvidencePresent,
            int gameplaySceneEvidenceInstances,
            bool productionPromoted,
            string state,
            string[] blockers)
        {
            Spec = spec;
            CandidateImported = candidateImported;
            ImportedAssetCount = importedAssetCount;
            TechnicalReadyAll = technicalReadyAll;
            TechnicalReadyAssetCount = technicalReadyAssetCount;
            SemanticRendered = semanticRendered;
            SemanticAllowed = semanticAllowed;
            GameplaySceneEvidencePresent = gameplaySceneEvidencePresent;
            GameplaySceneEvidenceInstances = gameplaySceneEvidenceInstances;
            ProductionPromoted = productionPromoted;
            State = state;
            Blockers = blockers;
        }

        public readonly ClassSpec Spec;
        public readonly bool CandidateImported;
        public readonly int ImportedAssetCount;
        public readonly bool TechnicalReadyAll;
        public readonly int TechnicalReadyAssetCount;
        public readonly bool SemanticRendered;
        public readonly bool SemanticAllowed;
        public readonly bool GameplaySceneEvidencePresent;
        public readonly int GameplaySceneEvidenceInstances;
        public readonly bool ProductionPromoted;
        public readonly string State;
        public readonly string[] Blockers;
    }
}
#endif
