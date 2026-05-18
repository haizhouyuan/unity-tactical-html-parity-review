#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class M85VisualProductionPassGate
{
    private const string ReportPath = "docs/M85_VISUAL_PRODUCTION_PASS.json";
    private const string MarkdownPath = "docs/M85_VISUAL_PRODUCTION_PASS.md";
    private const string ScreenshotDirectory = "Assets/Screenshots/M85VisualProduction";
    private const string RouteReportPath = "docs/TACTICAL_PLAYABLE_ROUTE_GATE.json";
    private const string BuildingReportPath = "docs/BUILDING_INTEGRITY_GATE.json";
    private const string WeaponFeelReportPath = "docs/WEAPON_FEEL_GATE.json";
    private const string AiPlaytestReportPath = "docs/AI_PLAYTEST_ROUTE_GATE.json";
    private const string M84ReportPath = "docs/M84_THREE_CLASS_ASSET_FACTORY_SPIKE.json";
    private const float MinAverageLuma = 0.10f;
    private const float MaxAverageLuma = 0.82f;

    [MenuItem("AI Tools/Run M85 Visual Production Pass Gate")]
    public static void WriteReport()
    {
        Directory.CreateDirectory("docs");
        Directory.CreateDirectory(ScreenshotDirectory);

        var gm = UnityEngine.Object.FindAnyObjectByType<TacticalGameManager>();
        var player = UnityEngine.Object.FindAnyObjectByType<TacticalPlayerController>();
        var camera = Camera.main;
        var follow = camera == null ? null : camera.GetComponent<TacticalCameraFollow>();
        var applicationReady = Application.isPlaying && gm != null && player != null && camera != null;

        if (applicationReady)
        {
            gm.StartRound();
            SetFloat(gm, "roundStartTime", Time.time - 10f);
            UnlockWeapon(gm, "rifle");
            gm.SelectWeapon("rifle");
            Physics.SyncTransforms();
        }

        var routeJson = ReadText(RouteReportPath);
        var buildingJson = ReadText(BuildingReportPath);
        var weaponJson = ReadText(WeaponFeelReportPath);
        var aiPlaytestJson = ReadText(AiPlaytestReportPath);
        var m84Json = ReadText(M84ReportPath);

        var m85ObjectCount = CountTransformsWithPrefix("M85 ");
        var puddleCount = CountTransformsWithPrefix("M85 Wet Puddle");
        var scuffCount = CountTransformsWithPrefix("M85 Mud Scuff");
        var roadPaintCount = CountTransformsWithPrefix("M85 Faded Road Paint");
        var cableCount = CountTransformsWithPrefix("M85 Cable Bundle");
        var casingCount = CountTransformsWithPrefix("M85 Spent Casing");
        var grimeCount = CountTransformsWithPrefix("M85 Wall Grime");
        var warningMarkerCount = CountTransformsWithPrefix("M85 Checkpoint Warning");
        var rainCount = CountObjectsByName("Tactical Rain Field");
        var postProcessCount = CountObjectsByName("Tactical Post Process Volume");
        var industrialLightCount = CountLightsWithName("Tactical Industrial") + CountLightsWithName("Tactical Warehouse Ceiling Light");
        var wetAsphaltRendererCount = CountRenderersWithMaterial("TacticalWetAsphaltApproved");
        var concreteRendererCount = CountRenderersWithMaterial("TacticalConcrete");
        var fogDensitySafe = RenderSettings.fog && RenderSettings.fogDensity > 0f && RenderSettings.fogDensity <= 0.018f;

        var screenshots = new List<ScreenshotEvidence>();
        if (applicationReady)
        {
            CaptureFirstPersonWeapon(player, follow, camera, screenshots);
            CaptureContainerCover(player, follow, camera, screenshots);
            CaptureBuildingEntry(player, follow, camera, screenshots);
            CaptureEnemyLighting(player, follow, camera, screenshots);
        }

        var screenshotQualityPassed = screenshots.Count >= 4;
        foreach (var shot in screenshots)
        {
            screenshotQualityPassed &= shot.Exists && shot.FileBytes > 120000 && shot.AverageLuma >= MinAverageLuma && shot.AverageLuma <= MaxAverageLuma;
        }

        var beforeReferencesPresent = File.Exists(ScreenshotDirectory + "/00_before_building_entry_route.png")
            && File.Exists(ScreenshotDirectory + "/00_before_container_cover_route.png")
            && File.Exists(ScreenshotDirectory + "/00_before_first_person_weapon_route.png");
        var scopedVisualDensityPassed = m85ObjectCount >= 60
            && puddleCount >= 6
            && scuffCount >= 8
            && roadPaintCount >= 12
            && cableCount >= 4
            && casingCount >= 10
            && grimeCount >= 4
            && warningMarkerCount >= 1;
        var baseVisualSystemsPassed = postProcessCount >= 1
            && rainCount >= 1
            && industrialLightCount >= 3
            && wetAsphaltRendererCount >= 1
            && concreteRendererCount >= 1
            && fogDensitySafe;
        var routeStillPassed = ExtractBool(routeJson, "passed")
            && ExtractBool(buildingJson, "passed")
            && ExtractBool(weaponJson, "passed")
            && ExtractBool(aiPlaytestJson, "passed")
            && ExtractBool(m84Json, "passed");
        var truthfulFullVisualStatus = !ExtractBool(routeJson, "full_visual_asset_gate_passed")
            && !string.IsNullOrWhiteSpace(ExtractString(routeJson, "visual_completion_blocker"));
        var passed = applicationReady
            && beforeReferencesPresent
            && baseVisualSystemsPassed
            && scopedVisualDensityPassed
            && screenshotQualityPassed
            && routeStillPassed
            && truthfulFullVisualStatus;

        WriteJson(
            passed,
            applicationReady,
            beforeReferencesPresent,
            baseVisualSystemsPassed,
            scopedVisualDensityPassed,
            screenshotQualityPassed,
            routeStillPassed,
            truthfulFullVisualStatus,
            m85ObjectCount,
            puddleCount,
            scuffCount,
            roadPaintCount,
            cableCount,
            casingCount,
            grimeCount,
            warningMarkerCount,
            rainCount,
            postProcessCount,
            industrialLightCount,
            wetAsphaltRendererCount,
            concreteRendererCount,
            RenderSettings.fogDensity,
            screenshots,
            ExtractString(routeJson, "visual_completion_blocker"));
        WriteMarkdown(passed, scopedVisualDensityPassed, routeStillPassed, truthfulFullVisualStatus, screenshots.Count);

        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] M85 visual production pass gate written to " + ReportPath + " passed=" + passed);
    }

    private static void CaptureFirstPersonWeapon(TacticalPlayerController player, TacticalCameraFollow follow, Camera camera, List<ScreenshotEvidence> screenshots)
    {
        MoveCharacter(player, new Vector3(0f, 1.04f, 30f));
        player.ResetView(180f, 0f);
        player.SetCameraMode(TacticalCameraMode.FirstPerson);
        player.SetAds(true);
        follow?.SnapToPlayer();
        var visual = UnityEngine.Object.FindAnyObjectByType<TacticalFirstPersonWeaponVisual>(FindObjectsInactive.Include);
        visual?.ForceRefresh();
        visual?.ApplyPreviewPolish(0.16f, true, 0.24f, 0f);
        Physics.SyncTransforms();
        CaptureStep(camera, screenshots, "01_after_first_person_weapon_wet_route", "M85 after: first-person hero rifle over wet tactical road");
    }

    private static void CaptureContainerCover(TacticalPlayerController player, TacticalCameraFollow follow, Camera camera, List<ScreenshotEvidence> screenshots)
    {
        var target = GameObject.Find("Container 1");
        if (target == null)
        {
            return;
        }

        PositionPlayerLookingAt(player, follow, target.transform.position, 9f, 14f);
        CaptureStep(camera, screenshots, "02_after_container_cover_rain_lights", "M85 after: container cover, rain, puddles, decals, and yard lighting");
    }

    private static void CaptureBuildingEntry(TacticalPlayerController player, TacticalCameraFollow follow, Camera camera, List<ScreenshotEvidence> screenshots)
    {
        var target = GameObject.Find("A Door Step 1") ?? GameObject.Find("A Front Wall Left 1");
        if (target == null)
        {
            return;
        }

        PositionPlayerLookingAt(player, follow, target.transform.position + new Vector3(0f, 1.4f, 0f), 7f, 10f);
        CaptureStep(camera, screenshots, "03_after_building_entry_grime_route", "M85 after: building entry with grime, mud scuffs, and route-readable doorway");
    }

    private static void CaptureEnemyLighting(TacticalPlayerController player, TacticalCameraFollow follow, Camera camera, List<ScreenshotEvidence> screenshots)
    {
        var enemy = UnityEngine.Object.FindAnyObjectByType<TacticalEnemy>();
        if (enemy == null)
        {
            return;
        }

        PositionPlayerLookingAt(player, follow, enemy.transform.position + new Vector3(0f, 1.2f, 0f), 10f, 12f);
        CaptureStep(camera, screenshots, "04_after_enemy_character_lighting_route", "M85 after: enemy character silhouette under gameplay lighting");
    }

    private static void PositionPlayerLookingAt(TacticalPlayerController player, TacticalCameraFollow follow, Vector3 target, float distance, float pitch)
    {
        var position = new Vector3(target.x, 1.04f, target.z - distance);
        MoveCharacter(player, position);
        var toTarget = target - player.transform.position;
        var yaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
        player.ResetView(yaw, pitch);
        player.SetCameraMode(TacticalCameraMode.ThirdPerson);
        player.SetAds(false);
        follow?.SnapToPlayer();
        Physics.SyncTransforms();
    }

    private static void MoveCharacter(TacticalPlayerController player, Vector3 position)
    {
        if (player == null)
        {
            return;
        }

        var controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        player.transform.position = position;
        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    private static void CaptureStep(Camera camera, List<ScreenshotEvidence> screenshots, string name, string label)
    {
        if (camera == null)
        {
            return;
        }

        var path = ScreenshotDirectory + "/" + name + ".png";
        var stats = RenderCamera(camera, path);
        screenshots.Add(new ScreenshotEvidence(label, path, stats.exists, stats.bytes, stats.averageLuma));
    }

    private static (bool exists, long bytes, float averageLuma) RenderCamera(Camera camera, string path)
    {
        var renderTexture = new RenderTexture(1600, 900, 24);
        var texture = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        try
        {
            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            camera.Render();
            texture.ReadPixels(new Rect(0f, 0f, 1600f, 900f), 0, 0);
            texture.Apply();
            var averageLuma = ComputeAverageLuma(texture);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            var info = new FileInfo(path);
            return (info.Exists, info.Exists ? info.Length : 0L, averageLuma);
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }
    }

    private static float ComputeAverageLuma(Texture2D texture)
    {
        var pixels = texture.GetPixels32();
        if (pixels.Length == 0)
        {
            return 0f;
        }

        var stride = Mathf.Max(1, pixels.Length / 16000);
        var total = 0.0;
        var count = 0;
        for (var i = 0; i < pixels.Length; i += stride)
        {
            var p = pixels[i];
            total += (0.2126 * p.r + 0.7152 * p.g + 0.0722 * p.b) / 255.0;
            count++;
        }

        return count == 0 ? 0f : (float)(total / count);
    }

    private static int CountTransformsWithPrefix(string prefix)
    {
        var count = 0;
        foreach (var transform in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
        {
            if (transform.name.StartsWith(prefix, StringComparison.Ordinal))
            {
                count++;
            }
        }
        return count;
    }

    private static int CountObjectsByName(string exactName)
    {
        var count = 0;
        foreach (var transform in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
        {
            if (transform.name == exactName)
            {
                count++;
            }
        }
        return count;
    }

    private static int CountLightsWithName(string marker)
    {
        var count = 0;
        foreach (var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude))
        {
            if (light.name.Contains(marker, StringComparison.Ordinal))
            {
                count++;
            }
        }
        return count;
    }

    private static int CountRenderersWithMaterial(string marker)
    {
        var count = 0;
        foreach (var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude))
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material != null && material.name.Contains(marker, StringComparison.Ordinal))
                {
                    count++;
                    break;
                }
            }
        }
        return count;
    }

    private static void UnlockWeapon(TacticalGameManager gm, string weaponId)
    {
        if (gm == null)
        {
            return;
        }

        var weapons = typeof(TacticalGameManager).GetField("weapons", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(gm) as System.Collections.IDictionary;
        if (weapons == null || !weapons.Contains(weaponId) || weapons[weaponId] is not TacticalWeaponState state)
        {
            return;
        }

        state.unlocked = true;
        state.magazine = Mathf.Max(state.magazine, state.spec.magazineSize);
        state.reserve = Mathf.Max(state.reserve, state.spec.reserveStart);
        state.reloading = false;
    }

    private static void SetFloat(object target, string fieldName, float value)
    {
        target?.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
    }

    private static string ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    private static bool ExtractBool(string json, string key)
    {
        return !string.IsNullOrEmpty(json) && Regex.IsMatch(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*true");
    }

    private static string ExtractString(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return "";
        }

        var match = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*\"(?<value>(?:\\\\.|[^\"])*)\"");
        return match.Success ? Regex.Unescape(match.Groups["value"].Value) : "";
    }

    private static void WriteJson(
        bool passed,
        bool applicationReady,
        bool beforeReferencesPresent,
        bool baseVisualSystemsPassed,
        bool scopedVisualDensityPassed,
        bool screenshotQualityPassed,
        bool routeStillPassed,
        bool truthfulFullVisualStatus,
        int m85ObjectCount,
        int puddleCount,
        int scuffCount,
        int roadPaintCount,
        int cableCount,
        int casingCount,
        int grimeCount,
        int warningMarkerCount,
        int rainCount,
        int postProcessCount,
        int industrialLightCount,
        int wetAsphaltRendererCount,
        int concreteRendererCount,
        float fogDensity,
        List<ScreenshotEvidence> screenshots,
        string visualCompletionBlocker)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "m85_visual_production_pass_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "application_is_playing", Application.isPlaying, true);
        Append(json, "passed", passed, true);
        Append(json, "application_ready", applicationReady, true);
        Append(json, "before_reference_screenshots_present", beforeReferencesPresent, true);
        json.AppendLine("  \"before_reference_screenshots\": [");
        json.AppendLine("    \"Assets/Screenshots/M85VisualProduction/00_before_building_entry_route.png\",");
        json.AppendLine("    \"Assets/Screenshots/M85VisualProduction/00_before_container_cover_route.png\",");
        json.AppendLine("    \"Assets/Screenshots/M85VisualProduction/00_before_first_person_weapon_route.png\"");
        json.AppendLine("  ],");
        Append(json, "base_visual_systems_passed", baseVisualSystemsPassed, true);
        Append(json, "scoped_visual_density_passed", scopedVisualDensityPassed, true);
        Append(json, "screenshot_quality_passed", screenshotQualityPassed, true);
        Append(json, "route_still_passed", routeStillPassed, true);
        Append(json, "truthful_full_visual_status_preserved", truthfulFullVisualStatus, true);
        Append(json, "m85_visual_detail_object_count", m85ObjectCount, true);
        Append(json, "wet_puddle_decal_count", puddleCount, true);
        Append(json, "mud_scuff_decal_count", scuffCount, true);
        Append(json, "faded_road_paint_count", roadPaintCount, true);
        Append(json, "cable_bundle_count", cableCount, true);
        Append(json, "spent_casing_count", casingCount, true);
        Append(json, "wall_grime_decal_count", grimeCount, true);
        Append(json, "warning_marker_count", warningMarkerCount, true);
        Append(json, "rain_field_count", rainCount, true);
        Append(json, "postprocess_volume_count", postProcessCount, true);
        Append(json, "industrial_light_count", industrialLightCount, true);
        Append(json, "wet_asphalt_renderer_count", wetAsphaltRendererCount, true);
        Append(json, "concrete_renderer_count", concreteRendererCount, true);
        Append(json, "fog_density", fogDensity, true);
        Append(json, "after_screenshot_count", screenshots.Count, true);
        json.AppendLine("  \"after_screenshots\": [");
        for (var i = 0; i < screenshots.Count; i++)
        {
            AppendScreenshot(json, screenshots[i], i == screenshots.Count - 1);
        }
        json.AppendLine("  ],");
        Append(json, "full_visual_asset_gate_intentionally_not_overridden", true, true);
        Append(json, "visual_completion_blocker", visualCompletionBlocker, false);
        json.AppendLine("}");
        File.WriteAllText(ReportPath, json.ToString());
    }

    private static void WriteMarkdown(bool passed, bool scopedVisualDensityPassed, bool routeStillPassed, bool truthfulFullVisualStatus, int screenshotCount)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# M85 Visual Production Pass");
        markdown.AppendLine();
        markdown.AppendLine("Generated: " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        markdown.AppendLine();
        markdown.AppendLine("- Passed: `" + passed + "`");
        markdown.AppendLine("- Scoped visual density passed: `" + scopedVisualDensityPassed + "`");
        markdown.AppendLine("- Route still passed: `" + routeStillPassed + "`");
        markdown.AppendLine("- Truthful full visual status preserved: `" + truthfulFullVisualStatus + "`");
        markdown.AppendLine("- After screenshot count: `" + screenshotCount + "`");
        markdown.AppendLine();
        markdown.AppendLine("M85 is a player-camera visual production pass for the existing tactical route. It does not claim final PUBG-like visual completion and it does not override `full_visual_asset_gate_passed=false` while final character and generated asset class promotion are still blocked.");
        File.WriteAllText(MarkdownPath, markdown.ToString());
    }

    private static void AppendScreenshot(StringBuilder json, ScreenshotEvidence shot, bool last)
    {
        json.AppendLine("    {");
        Append(json, "label", shot.Label, true, 6);
        Append(json, "path", shot.Path, true, 6);
        Append(json, "exists", shot.Exists, true, 6);
        Append(json, "file_bytes", shot.FileBytes, true, 6);
        Append(json, "average_luma", shot.AverageLuma, false, 6);
        json.Append("    }");
        json.AppendLine(last ? "" : ",");
    }

    private static void Append(StringBuilder json, string key, string value, bool comma, int indent = 2)
    {
        json.Append(' ', indent).Append("\"").Append(key).Append("\": \"").Append(Escape(value)).Append("\"");
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, bool value, bool comma, int indent = 2)
    {
        json.Append(' ', indent).Append("\"").Append(key).Append("\": ").Append(value ? "true" : "false");
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, int value, bool comma, int indent = 2)
    {
        json.Append(' ', indent).Append("\"").Append(key).Append("\": ").Append(value.ToString(CultureInfo.InvariantCulture));
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, long value, bool comma, int indent = 2)
    {
        json.Append(' ', indent).Append("\"").Append(key).Append("\": ").Append(value.ToString(CultureInfo.InvariantCulture));
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, float value, bool comma, int indent = 2)
    {
        json.Append(' ', indent).Append("\"").Append(key).Append("\": ").Append(value.ToString("0.######", CultureInfo.InvariantCulture));
        json.AppendLine(comma ? "," : "");
    }

    private static string Escape(string value)
    {
        return (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private readonly struct ScreenshotEvidence
    {
        public ScreenshotEvidence(string label, string path, bool exists, long fileBytes, float averageLuma)
        {
            Label = label;
            Path = path;
            Exists = exists;
            FileBytes = fileBytes;
            AverageLuma = averageLuma;
        }

        public readonly string Label;
        public readonly string Path;
        public readonly bool Exists;
        public readonly long FileBytes;
        public readonly float AverageLuma;
    }
}
#endif
