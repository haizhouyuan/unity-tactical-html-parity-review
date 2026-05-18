#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class PromotedAssetPlayerCameraVisibilityGate
{
    private const string ReportPath = "docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json";
    private const string MarkdownPath = "docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.md";
    private const string PromotionQueuePath = "docs/REALIFIED_ASSET_CLASS_PROMOTION_QUEUE.json";
    private const string PlayerPovScreenshotPath = "Assets/Screenshots/tactical_html_replica_current_player_pov_verified.png";
    private const float MinimumScreenAreaRatio = 0.002f;

    private static readonly ClassSpec[] Classes =
    {
        new ClassSpec("weapon", new[] { "hero_rifle", "sidearm", "secondary_weapon", "shotgun" },
            new[] { "hero_rifle", "rs_02_sidearm", "rs_03_secondary_weapon", "rs_12_shotgun", "realifiedherorifle", "realifiedsidearm", "realifiedsecondaryweapon" }),
        new ClassSpec("character", new[] { "player_tactical", "enemy_tactical" },
            new[] { "rs_04_player_tactical", "rs_05_enemy_tactical", "realifiedplayer", "realifiedenemy" }),
        new ClassSpec("gear", new[] { "helmet", "vest" },
            new[] { "rs_06_gear_helmet", "rs_07_gear_vest", "realifiedhelmet", "realifiedvest" }),
        new ClassSpec("loot", new[] { "ammo", "medkit" },
            new[] { "rs_08_loot_ammo", "rs_09_loot_medkit", "realifiedammoloot", "realifiedmedkitloot" }),
        new ClassSpec("environment_prop", new[] { "container", "crate" },
            new[] { "rs_10_prop_container", "rs_11_prop_crate", "realifiedcontainer", "realifiedcrate" }),
    };

    [MenuItem("AI Tools/Write Promoted Asset Player Camera Visibility Gate")]
    public static void WriteReport()
    {
        Directory.CreateDirectory("docs");

        var queueJson = ReadText(PromotionQueuePath);
        var camera = Camera.main;
        var planes = camera == null ? null : GeometryUtility.CalculateFrustumPlanes(camera);
        var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);
        var results = Classes.Select(spec => BuildResult(spec, queueJson, camera, planes, renderers)).ToArray();
        var productionPromotedClasses = results.Count(result => result.ProductionPromoted);
        var visiblePromotedClasses = results.Count(result => result.VisiblePromoted);
        var visiblePromotedObjects = results.Sum(result => result.VisiblePromotedObjectCount);
        var candidateVisibleObjects = results.Sum(result => result.CandidateVisibleObjectCount);
        var blockedReason = productionPromotedClasses == 0
            ? "no_production_promoted_assets"
            : visiblePromotedClasses < productionPromotedClasses
                ? "one_or_more_promoted_classes_not_visible_in_player_camera"
                : "";
        var passed = Application.isPlaying
            && camera != null
            && productionPromotedClasses > 0
            && visiblePromotedClasses == productionPromotedClasses;

        WriteJson(results, passed, productionPromotedClasses, visiblePromotedClasses, visiblePromotedObjects, candidateVisibleObjects, blockedReason);
        WriteMarkdown(results, passed, productionPromotedClasses, visiblePromotedClasses, visiblePromotedObjects, candidateVisibleObjects, blockedReason);

        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] Promoted asset player camera visibility gate written to " + ReportPath + " passed=" + passed);
    }

    private static ClassResult BuildResult(ClassSpec spec, string queueJson, Camera camera, Plane[] planes, Renderer[] renderers)
    {
        var queueBody = ExtractClassBody(queueJson, spec.Category);
        var productionPromoted = ExtractBool(queueBody, "production_promoted");
        var queueAssetIds = ExtractStringArray(queueBody, "asset_ids");
        var queueBlockers = ExtractStringArray(queueBody, "blockers");
        var assetIds = queueAssetIds.Length > 0 ? queueAssetIds : spec.AssetIds;
        var candidateVisibleCount = 0;
        var visiblePromotedCount = 0;
        var maxArea = 0f;
        var visibleNames = new StringBuilder();

        if (camera != null && planes != null)
        {
            foreach (var renderer in renderers)
            {
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!Matches(renderer, spec.Markers))
                {
                    continue;
                }

                if (!GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
                {
                    continue;
                }

                var area = ScreenAreaRatio(camera, renderer.bounds);
                if (area <= 0f)
                {
                    continue;
                }

                candidateVisibleCount++;
                maxArea = Mathf.Max(maxArea, area);
                AppendUniqueName(visibleNames, renderer.gameObject.name);
                if (productionPromoted && area >= MinimumScreenAreaRatio)
                {
                    visiblePromotedCount++;
                }
            }
        }

        var visiblePromoted = productionPromoted && visiblePromotedCount > 0 && maxArea >= MinimumScreenAreaRatio;
        var blockers = BuildVisibilityBlockers(productionPromoted, visiblePromoted, queueBlockers);
        return new ClassResult(
            spec.Category,
            assetIds,
            productionPromoted,
            candidateVisibleCount,
            visiblePromotedCount,
            visiblePromoted,
            maxArea,
            visibleNames.ToString(),
            blockers);
    }

    private static string[] BuildVisibilityBlockers(bool productionPromoted, bool visiblePromoted, string[] queueBlockers)
    {
        var blockers = new StringBuilder();
        foreach (var blocker in queueBlockers)
        {
            AppendBlocker(blockers, blocker);
        }

        if (!productionPromoted)
        {
            AppendBlocker(blockers, "class_not_production_promoted");
        }
        else if (!visiblePromoted)
        {
            AppendBlocker(blockers, "production_promoted_class_not_visible_in_player_camera");
        }

        return blockers.Length == 0 ? Array.Empty<string>() : blockers.ToString().Split('|');
    }

    private static bool Matches(Renderer renderer, string[] markers)
    {
        var text = (renderer.gameObject.name + " " + renderer.transform.root.name + " " + renderer.sharedMaterial?.name).ToLowerInvariant();
        return markers.Any(marker => text.Contains(marker));
    }

    private static float ScreenAreaRatio(Camera camera, Bounds bounds)
    {
        var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        var anyFront = false;
        var center = bounds.center;
        var extents = bounds.extents;
        for (var x = -1; x <= 1; x += 2)
        for (var y = -1; y <= 1; y += 2)
        for (var z = -1; z <= 1; z += 2)
        {
            var point = center + Vector3.Scale(extents, new Vector3(x, y, z));
            var viewport = camera.WorldToViewportPoint(point);
            if (viewport.z <= 0f)
            {
                continue;
            }

            anyFront = true;
            min.x = Mathf.Min(min.x, Mathf.Clamp01(viewport.x));
            min.y = Mathf.Min(min.y, Mathf.Clamp01(viewport.y));
            max.x = Mathf.Max(max.x, Mathf.Clamp01(viewport.x));
            max.y = Mathf.Max(max.y, Mathf.Clamp01(viewport.y));
        }

        if (!anyFront || min.x > max.x || min.y > max.y)
        {
            return 0f;
        }

        return Mathf.Max(0f, max.x - min.x) * Mathf.Max(0f, max.y - min.y);
    }

    private static void WriteJson(ClassResult[] results, bool passed, int productionPromotedClasses, int visiblePromotedClasses, int visiblePromotedObjects, int candidateVisibleObjects, string blockedReason)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "promoted_asset_player_camera_visibility_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "application_is_playing", Application.isPlaying, true);
        Append(json, "passed", passed, true);
        Append(json, "promotion_queue_path", PromotionQueuePath, true);
        Append(json, "player_pov_screenshot_path", PlayerPovScreenshotPath, true);
        Append(json, "camera_present", Camera.main != null, true);
        Append(json, "minimum_screen_area_ratio", MinimumScreenAreaRatio, true);
        json.AppendLine("  \"summary\": {");
        Append(json, "class_count", results.Length, true, 4);
        Append(json, "production_promoted_classes", productionPromotedClasses, true, 4);
        Append(json, "visible_promoted_classes", visiblePromotedClasses, true, 4);
        Append(json, "visible_promoted_objects", visiblePromotedObjects, true, 4);
        Append(json, "candidate_visible_objects", candidateVisibleObjects, true, 4);
        Append(json, "blocked_reason", blockedReason, false, 4);
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
        Append(json, "category", result.Category, true, 6);
        AppendStringArray(json, "asset_ids", result.AssetIds, true, 6);
        Append(json, "production_promoted", result.ProductionPromoted, true, 6);
        Append(json, "candidate_visible_object_count", result.CandidateVisibleObjectCount, true, 6);
        Append(json, "visible_promoted_object_count", result.VisiblePromotedObjectCount, true, 6);
        Append(json, "visible_promoted", result.VisiblePromoted, true, 6);
        Append(json, "max_screen_area_ratio", result.MaxScreenAreaRatio, true, 6);
        Append(json, "visible_object_names", result.VisibleObjectNames, true, 6);
        AppendStringArray(json, "blockers", result.Blockers, false, 6);
        json.Append("    }");
        json.AppendLine(last ? "" : ",");
    }

    private static void WriteMarkdown(ClassResult[] results, bool passed, int productionPromotedClasses, int visiblePromotedClasses, int visiblePromotedObjects, int candidateVisibleObjects, string blockedReason)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# Promoted Asset Player Camera Visibility Gate");
        markdown.AppendLine();
        markdown.AppendLine("Generated: " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        markdown.AppendLine();
        markdown.AppendLine("## Summary");
        markdown.AppendLine();
        markdown.AppendLine("- Passed: `" + passed + "`");
        markdown.AppendLine("- Production-promoted classes: `" + productionPromotedClasses + " / " + results.Length + "`");
        markdown.AppendLine("- Visible promoted classes: `" + visiblePromotedClasses + " / " + productionPromotedClasses + "`");
        markdown.AppendLine("- Visible promoted objects: `" + visiblePromotedObjects + "`");
        markdown.AppendLine("- Candidate visible objects: `" + candidateVisibleObjects + "`");
        markdown.AppendLine("- Blocked reason: `" + blockedReason + "`");
        markdown.AppendLine();
        markdown.AppendLine("Candidate visibility is reported for diagnosis only. A class counts only after the promotion queue marks it production-promoted.");
        markdown.AppendLine();
        markdown.AppendLine("## Classes");
        markdown.AppendLine();
        markdown.AppendLine("| Class | Production Promoted | Candidate Visible Objects | Visible Promoted Objects | Max Screen Area | Blockers |");
        markdown.AppendLine("|---|---:|---:|---:|---:|---|");
        foreach (var result in results)
        {
            markdown.Append("| ")
                .Append(result.Category)
                .Append(" | ")
                .Append(result.ProductionPromoted)
                .Append(" | ")
                .Append(result.CandidateVisibleObjectCount)
                .Append(" | ")
                .Append(result.VisiblePromotedObjectCount)
                .Append(" | ")
                .Append(result.MaxScreenAreaRatio.ToString("0.####", CultureInfo.InvariantCulture))
                .Append(" | ")
                .Append(result.Blockers.Length == 0 ? "" : string.Join(", ", result.Blockers))
                .AppendLine(" |");
        }

        File.WriteAllText(MarkdownPath, markdown.ToString());
    }

    private static string ExtractClassBody(string json, string category)
    {
        if (string.IsNullOrEmpty(json))
        {
            return "";
        }

        var match = Regex.Match(json,
            "\\{\\s*\\\"category\\\"\\s*:\\s*\\\"" + Regex.Escape(category) + "\\\"(?<body>.*?)\\n\\s*\\}",
            RegexOptions.Singleline);
        return match.Success ? match.Groups["body"].Value : "";
    }

    private static bool ExtractBool(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        return Regex.IsMatch(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*true");
    }

    private static string[] ExtractStringArray(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return Array.Empty<string>();
        }

        var match = Regex.Match(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*\\[(?<body>.*?)\\]", RegexOptions.Singleline);
        if (!match.Success)
        {
            return Array.Empty<string>();
        }

        return Regex.Matches(match.Groups["body"].Value, "\\\"(?<value>(?:\\\\.|[^\\\"])*)\\\"")
            .Cast<Match>()
            .Select(value => Regex.Unescape(value.Groups["value"].Value))
            .ToArray();
    }

    private static string ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : "";
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

    private static void AppendUniqueName(StringBuilder names, string value)
    {
        if (string.IsNullOrEmpty(value) || names.ToString().Contains(value))
        {
            return;
        }

        if (names.Length > 0)
        {
            names.Append(", ");
        }

        names.Append(value);
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

    private static void Append(StringBuilder json, string key, float value, bool comma, int indent = 2)
    {
        json.Append(' ', indent);
        json.Append('"').Append(key).Append("\": ").Append(value.ToString("0.######", CultureInfo.InvariantCulture));
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
        public ClassSpec(string category, string[] assetIds, string[] markers)
        {
            Category = category;
            AssetIds = assetIds;
            Markers = markers;
        }

        public readonly string Category;
        public readonly string[] AssetIds;
        public readonly string[] Markers;
    }

    private readonly struct ClassResult
    {
        public ClassResult(
            string category,
            string[] assetIds,
            bool productionPromoted,
            int candidateVisibleObjectCount,
            int visiblePromotedObjectCount,
            bool visiblePromoted,
            float maxScreenAreaRatio,
            string visibleObjectNames,
            string[] blockers)
        {
            Category = category;
            AssetIds = assetIds;
            ProductionPromoted = productionPromoted;
            CandidateVisibleObjectCount = candidateVisibleObjectCount;
            VisiblePromotedObjectCount = visiblePromotedObjectCount;
            VisiblePromoted = visiblePromoted;
            MaxScreenAreaRatio = maxScreenAreaRatio;
            VisibleObjectNames = visibleObjectNames;
            Blockers = blockers;
        }

        public readonly string Category;
        public readonly string[] AssetIds;
        public readonly bool ProductionPromoted;
        public readonly int CandidateVisibleObjectCount;
        public readonly int VisiblePromotedObjectCount;
        public readonly bool VisiblePromoted;
        public readonly float MaxScreenAreaRatio;
        public readonly string VisibleObjectNames;
        public readonly string[] Blockers;
    }
}
#endif
