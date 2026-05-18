#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

public static class RealifiedAssetGameplayPromotionLedger
{
    private const string ReportPath = "docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json";
    private const string MarkdownPath = "docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.md";
    private const string ImportGatePath = "docs/REALIFIED_IMPORT_MATERIAL_GATE.json";
    private const string CategoryReviewPath = "docs/REALIFIED_CATEGORY_NEMOTRON_REVIEWS.json";
    private const string M93CorrectedLootReviewPath = "docs/M93_CORRECTED_LOOT_NEMOTRON_REVIEW.json";
    private const string RouteGatePath = "docs/TACTICAL_PLAYABLE_ROUTE_GATE.json";
    private const string GameplayGatePath = "docs/TACTICAL_GAMEPLAY_PROOF_GATE.json";

    private static readonly AssetSpec[] Assets =
    {
        new AssetSpec("hero_rifle", "weapon", "hero_rifle_LOD0.glb", "realified_hero_rifle_promotion_scene_instances", true),
        new AssetSpec("sidearm", "weapon", "RS_02_sidearm_LOD0.glb", "realified_sidearm_promotion_scene_instances", false),
        new AssetSpec("secondary_weapon", "weapon", "RS_03_secondary_weapon_LOD0.glb", "realified_secondary_weapon_promotion_scene_instances", false),
        new AssetSpec("shotgun", "weapon", "RS_12_shotgun_LOD0.glb", "", false),
        new AssetSpec("helmet", "gear", "RS_06_gear_helmet_LOD0.glb", "realified_helmet_promotion_scene_instances", false),
        new AssetSpec("vest", "gear", "RS_07_gear_vest_LOD0.glb", "realified_vest_promotion_scene_instances", false),
        new AssetSpec("ammo", "loot", "RS_08_loot_ammo_LOD0.glb", "realified_ammo_loot_promotion_scene_instances", false),
        new AssetSpec("medkit", "loot", "RS_09_loot_medkit_LOD0.glb", "realified_medkit_loot_promotion_scene_instances", false),
        new AssetSpec("container", "environment_prop", "RS_10_prop_container_LOD0.glb", "realified_container_promotion_scene_instances", false),
        new AssetSpec("crate", "environment_prop", "RS_11_prop_crate_LOD0.glb", "realified_crate_promotion_scene_instances", false),
        new AssetSpec("player_tactical", "character", "RS_04_player_tactical_LOD0.glb", "", false),
        new AssetSpec("enemy_tactical", "character", "RS_05_enemy_tactical_LOD0.glb", "", false),
    };

    [MenuItem("AI Tools/Write Realified Asset Gameplay Promotion Ledger")]
    public static void WriteLedger()
    {
        Directory.CreateDirectory("docs");

        var importJson = ReadText(ImportGatePath);
        var categoryJson = ReadText(CategoryReviewPath);
        var routeJson = ReadText(RouteGatePath);
        var gameplayJson = ReadText(GameplayGatePath);
        var details = ExtractString(routeJson, "details") + " " + ExtractString(gameplayJson, "details");
        var results = Assets.Select(spec => BuildResult(spec, importJson, categoryJson, routeJson, gameplayJson, details)).ToArray();
        var promotedCount = results.Count(result => result.ProductionPromoted);
        var partialCount = results.Count(result => result.TechnicalReady && result.SemanticCategoryMatch && result.GameplayEntitySceneEvidence);

        WriteJson(results, promotedCount, partialCount);
        WriteMarkdown(results, promotedCount, partialCount);

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("[AI Tools] Realified asset gameplay promotion ledger written to " + ReportPath + " promoted=" + promotedCount);
    }

    private static AssetResult BuildResult(AssetSpec spec, string importJson, string categoryJson, string routeJson, string gameplayJson, string routeDetails)
    {
        var imported = ExtractAssetBool(importJson, spec.AssetId, "lod0_imported_prefab");
        var technicalReady = ExtractAssetBool(importJson, spec.AssetId, "technical_import_ready");
        var semanticMatch = ExtractAssetSemanticMatch(categoryJson, spec.Category, spec.ExpectedFile);
        var sceneInstances = string.IsNullOrEmpty(spec.SceneEvidenceKey) ? 0 : ExtractInt(routeJson, spec.SceneEvidenceKey);
        var sceneEvidence = sceneInstances > 0;
        if (spec.AssetId == "ammo")
        {
            semanticMatch = semanticMatch || CorrectedLootSemanticReviewAllows(spec.ExpectedFile);
            sceneEvidence = sceneEvidence || ExtractBool(routeJson, "realified_ammo_loot_route_evidence");
        }
        else if (spec.AssetId == "medkit")
        {
            semanticMatch = semanticMatch || CorrectedLootSemanticReviewAllows(spec.ExpectedFile);
            sceneEvidence = sceneEvidence || ExtractBool(routeJson, "realified_medkit_loot_route_evidence");
        }
        var playerCameraEvidence = spec.AssetId == "hero_rifle"
            ? ExtractInt(routeJson, "spawn_first_person_gameplay_source_glb_renderers") >= 1
                && routeDetails.Contains("active rifle", StringComparison.OrdinalIgnoreCase)
                && ExtractDetailInt(routeDetails, "fpSourceGlbRenderers") >= 1
            : spec.AssetId == "sidearm"
                ? ExtractInt(routeJson, "spawn_first_person_gameplay_source_glb_renderers") >= 1
                : spec.AssetId == "ammo"
                    ? ExtractBool(routeJson, "realified_ammo_loot_route_evidence")
                    : spec.AssetId == "medkit"
                        ? ExtractBool(routeJson, "realified_medkit_loot_route_evidence")
                : false;
        var gameplayEventEvidence = spec.RequiresFireEvent
            ? ExtractBool(routeJson, "fire_state_mutation")
                && ExtractBool(gameplayJson, "fire_ammo_and_enemy_hit")
                && routeDetails.Contains("active rifle", StringComparison.OrdinalIgnoreCase)
            : spec.AssetId == "ammo"
                ? ExtractBool(routeJson, "realified_ammo_loot_route_evidence") && ExtractBool(routeJson, "pickup_state_mutation")
                : spec.AssetId == "medkit"
                    ? ExtractBool(routeJson, "realified_medkit_loot_route_evidence") && ExtractBool(routeJson, "pickup_state_mutation")
                    : false;
        var productionPromoted = imported
            && technicalReady
            && semanticMatch
            && sceneEvidence
            && playerCameraEvidence
            && gameplayEventEvidence;
        var blockers = BuildBlockers(imported, technicalReady, semanticMatch, sceneEvidence, playerCameraEvidence, gameplayEventEvidence);

        return new AssetResult(
            spec,
            imported,
            technicalReady,
            semanticMatch,
            sceneInstances,
            sceneEvidence,
            playerCameraEvidence,
            gameplayEventEvidence,
            productionPromoted,
            blockers);
    }

    private static string[] BuildBlockers(bool imported, bool technicalReady, bool semanticMatch, bool sceneEvidence, bool playerCameraEvidence, bool gameplayEventEvidence)
    {
        var blockers = new StringBuilder();
        AppendBlocker(blockers, imported ? "" : "missing_lod0_import");
        AppendBlocker(blockers, technicalReady ? "" : "technical_import_or_pbr_sidecar_incomplete");
        AppendBlocker(blockers, semanticMatch ? "" : "semantic_category_mismatch_or_failed_review");
        AppendBlocker(blockers, sceneEvidence ? "" : "no_gameplay_entity_scene_evidence");
        AppendBlocker(blockers, playerCameraEvidence ? "" : "no_player_camera_visible_realified_asset_evidence");
        AppendBlocker(blockers, gameplayEventEvidence ? "" : "no_asset_specific_gameplay_event_evidence");
        return blockers.Length == 0 ? Array.Empty<string>() : blockers.ToString().Split('|');
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

    private static bool ExtractAssetSemanticMatch(string json, string category, string expectedFile)
    {
        var body = ExtractReviewBody(json, category);
        if (string.IsNullOrEmpty(body))
        {
            return false;
        }

        var structuredReview = Regex.Replace(
            body,
            "\\\"raw_model_output\\\"\\s*:\\s*\\\"(?:\\\\.|[^\\\"])*\\\"",
            "",
            RegexOptions.Singleline);
        var match = Regex.Match(structuredReview,
            "\\{(?=[^{}]*\\\"expected_file\\\"\\s*:\\s*\\\"" + Regex.Escape(expectedFile) + "\\\")(?<item>[^{}]*)\\}",
            RegexOptions.Singleline);
        return match.Success && ExtractBool(match.Groups["item"].Value, "category_match");
    }

    private static bool CorrectedLootSemanticReviewAllows(string expectedFile)
    {
        var json = ReadText(M93CorrectedLootReviewPath);
        if (string.IsNullOrEmpty(json) || !ExtractBool(json, "promotion_allowed_for_corrected_loot_semantics"))
        {
            return false;
        }

        var match = Regex.Match(json,
            "\\{(?=[^{}]*\\\"expected_file\\\"\\s*:\\s*\\\"" + Regex.Escape(expectedFile) + "\\\")(?<item>[^{}]*)\\}",
            RegexOptions.Singleline);
        return match.Success
            && ExtractBool(match.Groups["item"].Value, "category_match")
            && Regex.IsMatch(match.Groups["item"].Value, "\\\"visible_category\\\"\\s*:\\s*\\\"loot\\\"");
    }

    private static string ExtractReviewBody(string json, string category)
    {
        var reviewsStart = json.IndexOf("\"reviews\"", StringComparison.Ordinal);
        if (reviewsStart < 0)
        {
            return "";
        }

        var marker = "\"" + category + "\":";
        var start = json.IndexOf(marker, reviewsStart, StringComparison.Ordinal);
        if (start < 0)
        {
            return "";
        }

        var ordered = new[] { "weapon", "character", "gear", "loot", "environment_prop" };
        var end = json.Length;
        foreach (var next in ordered)
        {
            if (next == category)
            {
                continue;
            }

            var nextIndex = json.IndexOf("\"" + next + "\":", start + marker.Length, StringComparison.Ordinal);
            if (nextIndex > start && nextIndex < end)
            {
                end = nextIndex;
            }
        }

        return json.Substring(start, end - start);
    }

    private static int ExtractDetailInt(string details, string label)
    {
        var match = Regex.Match(details ?? "", Regex.Escape(label) + "\\s+(?<value>-?\\d+)");
        return match.Success && int.TryParse(match.Groups["value"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private static bool ExtractBool(string json, string key)
    {
        return !string.IsNullOrEmpty(json) && Regex.IsMatch(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*true");
    }

    private static int ExtractInt(string json, string key)
    {
        var match = Regex.Match(json ?? "", "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*(?<value>-?\\d+)");
        return match.Success && int.TryParse(match.Groups["value"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
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

    private static void WriteJson(AssetResult[] results, int promotedCount, int partialCount)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "realified_asset_gameplay_promotion_ledger_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "import_gate_path", ImportGatePath, true);
        Append(json, "category_review_path", CategoryReviewPath, true);
        Append(json, "route_gate_path", RouteGatePath, true);
        Append(json, "gameplay_gate_path", GameplayGatePath, true);
        json.AppendLine("  \"summary\": {");
        Append(json, "asset_count", results.Length, true, 4);
        Append(json, "technical_semantic_scene_partial_assets", partialCount, true, 4);
        Append(json, "production_promoted_assets", promotedCount, true, 4);
        Append(json, "any_production_promoted", promotedCount > 0, false, 4);
        json.AppendLine("  },");
        json.AppendLine("  \"assets\": [");
        for (var i = 0; i < results.Length; i++)
        {
            AppendAsset(json, results[i], i == results.Length - 1);
        }
        json.AppendLine("  ]");
        json.AppendLine("}");
        File.WriteAllText(ReportPath, json.ToString());
    }

    private static void AppendAsset(StringBuilder json, AssetResult result, bool last)
    {
        json.AppendLine("    {");
        Append(json, "asset_id", result.Spec.AssetId, true, 6);
        Append(json, "category", result.Spec.Category, true, 6);
        Append(json, "expected_file", result.Spec.ExpectedFile, true, 6);
        Append(json, "imported", result.Imported, true, 6);
        Append(json, "technical_ready", result.TechnicalReady, true, 6);
        Append(json, "semantic_category_match", result.SemanticCategoryMatch, true, 6);
        Append(json, "gameplay_scene_instances", result.GameplaySceneInstances, true, 6);
        Append(json, "gameplay_entity_scene_evidence", result.GameplayEntitySceneEvidence, true, 6);
        Append(json, "player_camera_realified_asset_evidence", result.PlayerCameraRealifiedAssetEvidence, true, 6);
        Append(json, "asset_specific_gameplay_event_evidence", result.AssetSpecificGameplayEventEvidence, true, 6);
        Append(json, "production_promoted", result.ProductionPromoted, true, 6);
        AppendStringArray(json, "blockers", result.Blockers, false, 6);
        json.Append("    }");
        json.AppendLine(last ? "" : ",");
    }

    private static void WriteMarkdown(AssetResult[] results, int promotedCount, int partialCount)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# Realified Asset Gameplay Promotion Ledger");
        markdown.AppendLine();
        markdown.AppendLine("Generated: " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        markdown.AppendLine();
        markdown.AppendLine("- Technical + semantic + scene partial assets: `" + partialCount + " / " + results.Length + "`");
        markdown.AppendLine("- Production-promoted assets: `" + promotedCount + " / " + results.Length + "`");
        markdown.AppendLine();
        markdown.AppendLine("An asset is production-promoted only when it is imported, technically ready, semantically matched, attached to a gameplay entity, visible from the player camera, and tied to a gameplay event.");
        markdown.AppendLine();
        markdown.AppendLine("| Asset | Technical | Semantic | Scene Entity | Player Camera | Gameplay Event | Promoted | Blockers |");
        markdown.AppendLine("|---|---:|---:|---:|---:|---:|---:|---|");
        foreach (var result in results)
        {
            markdown.Append("| ")
                .Append(result.Spec.AssetId)
                .Append(" | ")
                .Append(result.TechnicalReady)
                .Append(" | ")
                .Append(result.SemanticCategoryMatch)
                .Append(" | ")
                .Append(result.GameplayEntitySceneEvidence)
                .Append(" | ")
                .Append(result.PlayerCameraRealifiedAssetEvidence)
                .Append(" | ")
                .Append(result.AssetSpecificGameplayEventEvidence)
                .Append(" | ")
                .Append(result.ProductionPromoted)
                .Append(" | ")
                .Append(result.Blockers.Length == 0 ? "" : string.Join(", ", result.Blockers))
                .AppendLine(" |");
        }

        File.WriteAllText(MarkdownPath, markdown.ToString());
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

    private readonly struct AssetSpec
    {
        public AssetSpec(string assetId, string category, string expectedFile, string sceneEvidenceKey, bool requiresFireEvent)
        {
            AssetId = assetId;
            Category = category;
            ExpectedFile = expectedFile;
            SceneEvidenceKey = sceneEvidenceKey;
            RequiresFireEvent = requiresFireEvent;
        }

        public readonly string AssetId;
        public readonly string Category;
        public readonly string ExpectedFile;
        public readonly string SceneEvidenceKey;
        public readonly bool RequiresFireEvent;
    }

    private readonly struct AssetResult
    {
        public AssetResult(
            AssetSpec spec,
            bool imported,
            bool technicalReady,
            bool semanticCategoryMatch,
            int gameplaySceneInstances,
            bool gameplayEntitySceneEvidence,
            bool playerCameraRealifiedAssetEvidence,
            bool assetSpecificGameplayEventEvidence,
            bool productionPromoted,
            string[] blockers)
        {
            Spec = spec;
            Imported = imported;
            TechnicalReady = technicalReady;
            SemanticCategoryMatch = semanticCategoryMatch;
            GameplaySceneInstances = gameplaySceneInstances;
            GameplayEntitySceneEvidence = gameplayEntitySceneEvidence;
            PlayerCameraRealifiedAssetEvidence = playerCameraRealifiedAssetEvidence;
            AssetSpecificGameplayEventEvidence = assetSpecificGameplayEventEvidence;
            ProductionPromoted = productionPromoted;
            Blockers = blockers;
        }

        public readonly AssetSpec Spec;
        public readonly bool Imported;
        public readonly bool TechnicalReady;
        public readonly bool SemanticCategoryMatch;
        public readonly int GameplaySceneInstances;
        public readonly bool GameplayEntitySceneEvidence;
        public readonly bool PlayerCameraRealifiedAssetEvidence;
        public readonly bool AssetSpecificGameplayEventEvidence;
        public readonly bool ProductionPromoted;
        public readonly string[] Blockers;
    }
}
#endif
