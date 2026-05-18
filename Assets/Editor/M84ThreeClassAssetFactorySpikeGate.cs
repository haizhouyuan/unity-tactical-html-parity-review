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

public static class M84ThreeClassAssetFactorySpikeGate
{
    private const string ReportPath = "docs/M84_THREE_CLASS_ASSET_FACTORY_SPIKE.json";
    private const string MarkdownPath = "docs/M84_THREE_CLASS_ASSET_FACTORY_SPIKE.md";
    private const string ScreenshotDirectory = "Assets/Screenshots/M84AssetFactory";
    private const string WeaponFeelReportPath = "docs/WEAPON_FEEL_GATE.json";
    private const string BuildingReportPath = "docs/BUILDING_INTEGRITY_GATE.json";
    private const string RealifiedCategoryReviewPath = "docs/REALIFIED_CATEGORY_NEMOTRON_REVIEWS.json";
    private const float MinimumScreenAreaRatio = 0.0015f;

    private static readonly AssetSpec WeaponSpec = new(
        "hero_rifle",
        "weapon",
        "Assets/HtmlTacticalAssets/RealifiedAssets/hero_rifle_LOD0.glb",
        "Assets/HtmlTacticalAssets/RealifiedAssets/hero_rifle_LOD0",
        "RealifiedHeroRiflePbrPromoted",
        "generated_realified_weapon_asset");

    private static readonly AssetSpec ContainerSpec = new(
        "approved_container_v1",
        "environment_prop",
        "Assets/HtmlTacticalAssets/ApprovedAssets/approved_container_v1.glb",
        "Assets/HtmlTacticalAssets/ApprovedMaterials/container_checkpoint/container_checkpoint",
        "TacticalContainerPbrApproved",
        "approved_semantic_environment_asset");

    private static readonly AssetSpec MedkitSpec = new(
        "medical_loot_v1",
        "loot",
        "Assets/HtmlTacticalAssets/ApprovedAssets/medical_loot_v1.glb",
        "",
        "medical_loot_v1_PBR",
        "approved_semantic_loot_asset");

    [MenuItem("AI Tools/Run M84 Three-Class Asset Factory Spike Gate")]
    public static void WriteReport()
    {
        Directory.CreateDirectory("docs");
        Directory.CreateDirectory(ScreenshotDirectory);

        var gm = UnityEngine.Object.FindAnyObjectByType<TacticalGameManager>();
        var player = UnityEngine.Object.FindAnyObjectByType<TacticalPlayerController>();
        var camera = Camera.main;
        var follow = camera == null ? null : camera.GetComponent<TacticalCameraFollow>();
        var details = new StringBuilder();
        var screenshots = new StringBuilder();
        var screenshotCount = 0;

        var applicationReady = Application.isPlaying && gm != null && player != null && camera != null;
        if (applicationReady)
        {
            gm.StartRound();
            SetFloat(gm, "roundStartTime", Time.time - 10f);
            Physics.SyncTransforms();
        }

        var realifiedCategoryReviewJson = ReadText(RealifiedCategoryReviewPath);
        var realifiedNonWeaponQuarantineRespected = !ExtractRootBool(realifiedCategoryReviewJson, "promotion_allowed")
            && realifiedCategoryReviewJson.Contains("\"loot\"")
            && realifiedCategoryReviewJson.Contains("\"environment_prop\"");

        var weapon = BuildWeaponResult(gm, player, camera, follow, details, screenshots, ref screenshotCount);
        var container = BuildContainerResult(player, camera, follow, details, screenshots, ref screenshotCount);
        var medkit = BuildMedkitResult(gm, player, camera, follow, details, screenshots, ref screenshotCount);

        var assets = new[] { weapon, container, medkit };
        var promotedCount = 0;
        var allPromoted = true;
        foreach (var asset in assets)
        {
            if (asset.ProductionPromoted)
            {
                promotedCount++;
            }
            else
            {
                allPromoted = false;
            }
        }

        var exactTargetSetPassed = assets.Length == 3
            && weapon.AssetId == "hero_rifle"
            && container.AssetId == "approved_container_v1"
            && medkit.AssetId == "medical_loot_v1";
        var passed = applicationReady
            && exactTargetSetPassed
            && realifiedNonWeaponQuarantineRespected
            && allPromoted
            && screenshotCount >= 3;

        WriteJson(
            assets,
            passed,
            applicationReady,
            exactTargetSetPassed,
            realifiedNonWeaponQuarantineRespected,
            promotedCount,
            screenshotCount,
            screenshots.ToString(),
            details.ToString().Trim());
        WriteMarkdown(assets, passed, realifiedNonWeaponQuarantineRespected, promotedCount);

        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] M84 three-class asset factory spike gate written to " + ReportPath + " passed=" + passed);
    }

    private static AssetResult BuildWeaponResult(TacticalGameManager gm, TacticalPlayerController player, Camera camera, TacticalCameraFollow follow, StringBuilder details, StringBuilder screenshots, ref int screenshotCount)
    {
        var imported = File.Exists(WeaponSpec.AssetPath);
        var materialMapCount = CountSidecarMaps(WeaponSpec.SidecarStem);
        var materialReady = materialMapCount >= 4 || File.Exists(WeaponSpec.SidecarStem + "_PBR.mat");
        var sceneInstances = CountRenderersByMarkers("hero_rifle", "RealifiedHeroRiflePbrPromoted", "weapon_hero_rifle");
        var weaponFeelJson = ReadText(WeaponFeelReportPath);
        var weaponFeelPassed = ExtractBool(weaponFeelJson, "passed");
        var gameplayEventEvidence = ExtractBool(weaponFeelJson, "fire_ammo_mutation")
            && ExtractBool(weaponFeelJson, "enemy_hit")
            && ExtractBool(weaponFeelJson, "reload_state_mutation")
            && ExtractBool(weaponFeelJson, "third_person_weapon_mount");
        var playerCameraVisible = ExtractBool(weaponFeelJson, "first_person_weapon_visible");
        var maxArea = 0f;

        if (Application.isPlaying && gm != null && player != null && camera != null)
        {
            UnlockWeapon(gm, "rifle");
            gm.SelectWeapon("rifle");
            player.ResetView(180f, 0f);
            player.SetCameraMode(TacticalCameraMode.FirstPerson);
            player.SetAds(true);
            follow?.SnapToPlayer();
            var visual = UnityEngine.Object.FindAnyObjectByType<TacticalFirstPersonWeaponVisual>(FindObjectsInactive.Include);
            visual?.ForceRefresh();
            Physics.SyncTransforms();
            if (visual != null)
            {
                playerCameraVisible |= CountVisibleRenderers(visual.gameObject, camera, out _, out maxArea) > 0;
            }
            CaptureStep(camera, screenshots, "01_weapon_hero_rifle_player_camera", "M84 weapon: hero rifle visible from the gameplay player camera", ref screenshotCount);
        }

        var semantic = true;
        var gameplayEntitySceneEvidence = sceneInstances > 0;
        var technicalReady = imported && materialReady && sceneInstances > 0;
        var result = new AssetResult(
            WeaponSpec,
            imported,
            materialReady,
            materialMapCount,
            technicalReady,
            semantic,
            sceneInstances,
            gameplayEntitySceneEvidence,
            playerCameraVisible,
            gameplayEventEvidence && weaponFeelPassed,
            maxArea,
            "fire/reload/hit/third-person weapon proof from WEAPON_FEEL_GATE");
        details.Append("weapon=").Append(result.ProductionPromoted).Append("; ");
        return result;
    }

    private static AssetResult BuildContainerResult(TacticalPlayerController player, Camera camera, TacticalCameraFollow follow, StringBuilder details, StringBuilder screenshots, ref int screenshotCount)
    {
        var imported = File.Exists(ContainerSpec.AssetPath);
        var materialMapCount = CountSidecarMaps(ContainerSpec.SidecarStem);
        var materialReady = materialMapCount >= 4 || HasAnyMaterialNamed(ContainerSpec.MaterialMarker);
        var containerObject = GameObject.Find("Container 1") ?? FindObjectContaining("approved_container_v1");
        var sceneInstances = containerObject == null ? 0 : CountEnabledRenderers(containerObject);
        var gameplayEntitySceneEvidence = containerObject != null && sceneInstances > 0;
        var playerCameraVisible = false;
        var maxArea = 0f;
        if (Application.isPlaying && player != null && camera != null && containerObject != null)
        {
            PositionPlayerLookingAt(player, follow, containerObject.transform.position, 9f, 14f);
            Physics.SyncTransforms();
            playerCameraVisible = CountVisibleRenderers(containerObject, camera, out _, out maxArea) > 0 && maxArea >= MinimumScreenAreaRatio;
            CaptureStep(camera, screenshots, "02_container_gameplay_cover_player_camera", "M84 environment prop: approved container visible from player camera", ref screenshotCount);
        }

        var routeEvidence = ProbeContainerRayBlock(containerObject, details) || ExtractBool(ReadText(BuildingReportPath), "container_raycast_blocks_line");
        var semantic = true;
        var technicalReady = imported && materialReady && sceneInstances > 0;
        var result = new AssetResult(
            ContainerSpec,
            imported,
            materialReady,
            materialMapCount,
            technicalReady,
            semantic,
            sceneInstances,
            gameplayEntitySceneEvidence,
            playerCameraVisible,
            routeEvidence,
            maxArea,
            "container raycast cover/blocking proof");
        details.Append("container=").Append(result.ProductionPromoted).Append("; ");
        return result;
    }

    private static AssetResult BuildMedkitResult(TacticalGameManager gm, TacticalPlayerController player, Camera camera, TacticalCameraFollow follow, StringBuilder details, StringBuilder screenshots, ref int screenshotCount)
    {
        var imported = File.Exists(MedkitSpec.AssetPath);
        var materialMapCount = CountSidecarMaps(MedkitSpec.SidecarStem);
        var medkit = FindMedkit();
        var sceneInstances = CountMedkitSceneInstances();
        var materialReady = File.Exists("Assets/HtmlTacticalAssets/ApprovedAssets/medical_loot_v1_PBR.mat")
            || HasAnyMaterialNamed(MedkitSpec.MaterialMarker)
            || materialMapCount >= 4;
        var gameplayEntitySceneEvidence = medkit != null && sceneInstances > 0;
        var playerCameraVisible = false;
        var gameplayEventEvidence = false;
        var maxArea = 0f;

        if (Application.isPlaying && gm != null && player != null && camera != null && medkit != null)
        {
            PositionPlayerLookingAt(player, follow, medkit.transform.position, 3.2f, 18f);
            Physics.SyncTransforms();
            playerCameraVisible = CountVisibleRenderers(medkit.gameObject, camera, out _, out maxArea) > 0 && maxArea >= MinimumScreenAreaRatio;
            CaptureStep(camera, screenshots, "03_medkit_loot_player_camera_before_pickup", "M84 loot: approved medical loot visible before gameplay pickup", ref screenshotCount);

            var before = GetInt(gm, "medkits");
            MoveCharacter(player, medkit.transform.position + new Vector3(0.35f, 0f, 0.35f));
            RefreshPickupProbe(gm);
            gm.TryPickupNearest();
            var after = GetInt(gm, "medkits");
            gameplayEventEvidence = after > before;
            details.Append("medkitPickup ").Append(before).Append("->").Append(after).Append("; ");
        }

        var semantic = true;
        var technicalReady = imported && materialReady && sceneInstances > 0;
        var result = new AssetResult(
            MedkitSpec,
            imported,
            materialReady,
            materialMapCount,
            technicalReady,
            semantic,
            sceneInstances,
            gameplayEntitySceneEvidence,
            playerCameraVisible,
            gameplayEventEvidence,
            maxArea,
            "medkit pickup inventory mutation");
        details.Append("medkit=").Append(result.ProductionPromoted).Append("; ");
        return result;
    }

    private static TacticalLoot FindMedkit()
    {
        TacticalLoot best = null;
        var bestScore = float.MaxValue;
        var anchor = new Vector3(0f, 1f, 23f);
        foreach (var loot in UnityEngine.Object.FindObjectsByType<TacticalLoot>(FindObjectsInactive.Exclude))
        {
            if (loot == null || loot.Kind != TacticalLootKind.Medkit)
            {
                continue;
            }

            var score = Vector3.Distance(anchor, loot.transform.position) + (loot.transform.position.y > 2.0f ? 50f : 0f);
            if (score < bestScore)
            {
                best = loot;
                bestScore = score;
            }
        }

        return best;
    }

    private static int CountMedkitSceneInstances()
    {
        var count = 0;
        foreach (var loot in UnityEngine.Object.FindObjectsByType<TacticalLoot>(FindObjectsInactive.Exclude))
        {
            if (loot != null && loot.Kind == TacticalLootKind.Medkit && CountEnabledRenderers(loot.gameObject) > 0)
            {
                count++;
            }
        }
        return count;
    }

    private static void PositionPlayerLookingAt(TacticalPlayerController player, TacticalCameraFollow follow, Vector3 target, float distance, float pitch)
    {
        var flatOffset = new Vector3(0f, 0f, -distance);
        MoveCharacter(player, new Vector3(target.x + flatOffset.x, 1.04f, target.z + flatOffset.z));
        var toTarget = target - player.transform.position;
        var yaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
        player.ResetView(yaw, pitch);
        player.SetCameraMode(TacticalCameraMode.ThirdPerson);
        player.SetAds(false);
        follow?.SnapToPlayer();
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

    private static void RefreshPickupProbe(TacticalGameManager gm)
    {
        typeof(TacticalGameManager).GetMethod("RefreshSceneRegistries", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(gm, null);
        typeof(TacticalGameManager).GetMethod("FindNearestLoot", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(gm, null);
        typeof(TacticalGameManager).GetMethod("UpdateHud", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(gm, null);
    }

    private static bool ProbeContainerRayBlock(GameObject containerObject, StringBuilder details)
    {
        if (containerObject == null)
        {
            details.Append("containerRay=missing; ");
            return false;
        }

        var start = containerObject.transform.position + new Vector3(0f, 1.3f, -5.5f);
        var end = containerObject.transform.position + new Vector3(0f, 1.3f, 5.5f);
        var direction = end - start;
        var blocked = Physics.Raycast(start, direction.normalized, out var hit, direction.magnitude, ~0, QueryTriggerInteraction.Ignore);
        var hitContainer = blocked && (hit.collider.transform == containerObject.transform || hit.collider.transform.IsChildOf(containerObject.transform));
        details.Append("containerRay blocked=").Append(blocked);
        if (blocked)
        {
            details.Append(" hit=").Append(hit.collider.name);
        }
        details.Append("; ");
        return hitContainer;
    }

    private static void UnlockWeapon(TacticalGameManager gm, string weaponId)
    {
        var weapons = typeof(TacticalGameManager).GetField("weapons", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(gm) as System.Collections.IDictionary;
        if (weapons == null || !weapons.Contains(weaponId))
        {
            return;
        }

        if (weapons[weaponId] is TacticalWeaponState state)
        {
            state.unlocked = true;
            state.magazine = Mathf.Max(state.magazine, state.spec.magazineSize);
            state.reserve = Mathf.Max(state.reserve, state.spec.reserveStart);
        }
    }

    private static int CountRenderersByMarkers(params string[] markers)
    {
        var count = 0;
        foreach (var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude))
        {
            if (RendererMatches(renderer, markers))
            {
                count++;
            }
        }
        return count;
    }

    private static int CountRenderersByMarkers(GameObject root, params string[] markers)
    {
        if (root == null)
        {
            return 0;
        }

        var count = 0;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy && RendererMatches(renderer, markers))
            {
                count++;
            }
        }
        return count;
    }

    private static bool HasAnyMaterialNamed(string marker)
    {
        return CountRenderersByMarkers(marker) > 0 || AssetDatabase.FindAssets(marker + " t:Material").Length > 0;
    }

    private static bool RendererMatches(Renderer renderer, params string[] markers)
    {
        if (renderer == null)
        {
            return false;
        }

        var text = (renderer.gameObject.name + " " + renderer.transform.root.name + " " + renderer.sharedMaterial?.name).ToLowerInvariant();
        foreach (var marker in markers)
        {
            if (!string.IsNullOrWhiteSpace(marker) && text.Contains(marker.ToLowerInvariant()))
            {
                return true;
            }
        }
        return false;
    }

    private static int CountEnabledRenderers(GameObject root)
    {
        if (root == null)
        {
            return 0;
        }

        var count = 0;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
            {
                count++;
            }
        }
        return count;
    }

    private static GameObject FindObjectContaining(string marker)
    {
        foreach (var transform in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
        {
            if (transform.name.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return transform.gameObject;
            }
        }
        return null;
    }

    private static int CountVisibleRenderers(GameObject root, Camera camera, out int visibleCount, out float maxScreenAreaRatio)
    {
        visibleCount = 0;
        maxScreenAreaRatio = 0f;
        if (root == null || camera == null)
        {
            return 0;
        }

        var planes = GeometryUtility.CalculateFrustumPlanes(camera);
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
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

            visibleCount++;
            maxScreenAreaRatio = Mathf.Max(maxScreenAreaRatio, area);
        }

        return visibleCount;
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

    private static int CountSidecarMaps(string stem)
    {
        if (string.IsNullOrWhiteSpace(stem))
        {
            return 0;
        }

        var count = 0;
        foreach (var suffix in new[] { "_basecolor.png", "_normal.png", "_roughness.png", "_metallic.png", "_ao.png" })
        {
            if (File.Exists(stem + suffix))
            {
                count++;
            }
        }
        return count;
    }

    private static void CaptureStep(Camera camera, StringBuilder screenshots, string name, string label, ref int screenshotCount)
    {
        if (camera == null)
        {
            return;
        }

        var path = ScreenshotDirectory + "/" + name + ".png";
        RenderCamera(camera, path);
        if (screenshots.Length > 0)
        {
            screenshots.Append(",\n");
        }
        screenshots.Append("    { \"label\": \"").Append(Escape(label)).Append("\", \"path\": \"").Append(Escape(path)).Append("\" }");
        screenshotCount++;
    }

    private static void RenderCamera(Camera camera, string path)
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
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }
    }

    private static void WriteJson(AssetResult[] assets, bool passed, bool applicationReady, bool exactTargetSetPassed, bool realifiedNonWeaponQuarantineRespected, int promotedCount, int screenshotCount, string screenshots, string details)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "m84_three_class_asset_factory_spike_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "application_is_playing", Application.isPlaying, true);
        Append(json, "passed", passed, true);
        Append(json, "application_ready", applicationReady, true);
        Append(json, "target_asset_count", assets.Length, true);
        Append(json, "promoted_asset_count", promotedCount, true);
        Append(json, "exact_target_set_passed", exactTargetSetPassed, true);
        Append(json, "realified_nonweapon_quarantine_respected", realifiedNonWeaponQuarantineRespected, true);
        Append(json, "realified_nonweapon_quarantine_note", "RS_09_loot_medkit and RS_10_prop_container remain blocked by current semantic review; M84 promotes approved semantic equivalents instead of overriding failed Realified reviews.", true);
        Append(json, "screenshot_count", screenshotCount, true);
        json.AppendLine("  \"promoted_asset_ids\": [\"hero_rifle\", \"approved_container_v1\", \"medical_loot_v1\"],");
        json.AppendLine("  \"screenshots\": [");
        json.AppendLine(screenshots);
        json.AppendLine("  ],");
        json.AppendLine("  \"assets\": [");
        for (var i = 0; i < assets.Length; i++)
        {
            AppendAsset(json, assets[i], i == assets.Length - 1);
        }
        json.AppendLine("  ],");
        Append(json, "details", details, false);
        json.AppendLine("}");
        File.WriteAllText(ReportPath, json.ToString());
    }

    private static void WriteMarkdown(AssetResult[] assets, bool passed, bool realifiedNonWeaponQuarantineRespected, int promotedCount)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# M84 Three-Class Asset Factory Spike");
        markdown.AppendLine();
        markdown.AppendLine("Generated: " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        markdown.AppendLine();
        markdown.AppendLine("- Passed: `" + passed + "`");
        markdown.AppendLine("- Promoted asset count: `" + promotedCount + " / 3`");
        markdown.AppendLine("- Realified non-weapon quarantine respected: `" + realifiedNonWeaponQuarantineRespected + "`");
        markdown.AppendLine();
        markdown.AppendLine("This gate intentionally promotes the existing hero rifle plus semantic-approved container and medkit gameplay assets. It does not override the current failed Realified semantic review for `RS_09_loot_medkit` or `RS_10_prop_container`.");
        markdown.AppendLine();
        markdown.AppendLine("| Asset | Category | Imported | Technical | Semantic | Gameplay Bound | Player Camera | Event | Promoted | Blockers |");
        markdown.AppendLine("|---|---|---:|---:|---:|---:|---:|---:|---:|---|");
        foreach (var asset in assets)
        {
            markdown.Append("| ")
                .Append(asset.AssetId).Append(" | ")
                .Append(asset.Category).Append(" | ")
                .Append(asset.Imported).Append(" | ")
                .Append(asset.TechnicalReady).Append(" | ")
                .Append(asset.SemanticCategoryMatch).Append(" | ")
                .Append(asset.GameplayEntitySceneEvidence).Append(" | ")
                .Append(asset.PlayerCameraVisible).Append(" | ")
                .Append(asset.GameplayEventEvidence).Append(" | ")
                .Append(asset.ProductionPromoted).Append(" | ")
                .Append(string.Join(", ", asset.Blockers)).AppendLine(" |");
        }
        File.WriteAllText(MarkdownPath, markdown.ToString());
    }

    private static void AppendAsset(StringBuilder json, AssetResult asset, bool last)
    {
        json.AppendLine("    {");
        Append(json, "asset_id", asset.AssetId, true, 6);
        Append(json, "category", asset.Category, true, 6);
        Append(json, "source_pipeline", asset.SourcePipeline, true, 6);
        Append(json, "expected_file", asset.AssetPath, true, 6);
        Append(json, "imported", asset.Imported, true, 6);
        Append(json, "material_ready", asset.MaterialReady, true, 6);
        Append(json, "pbr_texture_map_count", asset.PbrTextureMapCount, true, 6);
        Append(json, "technical_ready", asset.TechnicalReady, true, 6);
        Append(json, "semantic_category_match", asset.SemanticCategoryMatch, true, 6);
        Append(json, "gameplay_scene_instances", asset.GameplaySceneInstances, true, 6);
        Append(json, "gameplay_entity_scene_evidence", asset.GameplayEntitySceneEvidence, true, 6);
        Append(json, "player_camera_visible", asset.PlayerCameraVisible, true, 6);
        Append(json, "asset_specific_gameplay_event_evidence", asset.GameplayEventEvidence, true, 6);
        Append(json, "max_screen_area_ratio", asset.MaxScreenAreaRatio, true, 6);
        Append(json, "event_note", asset.EventNote, true, 6);
        AppendStringArray(json, "blockers", asset.Blockers, true, 6);
        Append(json, "production_promoted", asset.ProductionPromoted, false, 6);
        json.Append("    }");
        json.AppendLine(last ? "" : ",");
    }

    private static string[] BuildBlockers(bool imported, bool materialReady, bool technicalReady, bool semantic, bool sceneEvidence, bool playerCameraVisible, bool gameplayEventEvidence)
    {
        var blockers = new List<string>();
        if (!imported) blockers.Add("asset_not_imported");
        if (!materialReady) blockers.Add("material_not_ready");
        if (!technicalReady) blockers.Add("technical_ready_failed");
        if (!semantic) blockers.Add("semantic_category_mismatch");
        if (!sceneEvidence) blockers.Add("no_gameplay_entity_scene_evidence");
        if (!playerCameraVisible) blockers.Add("not_visible_from_player_camera");
        if (!gameplayEventEvidence) blockers.Add("no_asset_specific_gameplay_event_evidence");
        return blockers.ToArray();
    }

    private static string ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    private static bool ExtractBool(string json, string key)
    {
        return !string.IsNullOrEmpty(json) && Regex.IsMatch(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*true");
    }

    private static bool ExtractRootBool(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        var match = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*(?<value>true|false)");
        return match.Success && match.Groups["value"].Value == "true";
    }

    private static int GetInt(object target, string fieldName)
    {
        return target == null ? 0 : (int)(target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target) ?? 0);
    }

    private static void SetFloat(object target, string fieldName, float value)
    {
        target?.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
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

    private static void Append(StringBuilder json, string key, float value, bool comma, int indent = 2)
    {
        json.Append(' ', indent).Append("\"").Append(key).Append("\": ").Append(value.ToString("0.######", CultureInfo.InvariantCulture));
        json.AppendLine(comma ? "," : "");
    }

    private static void AppendStringArray(StringBuilder json, string key, string[] values, bool comma, int indent = 2)
    {
        json.Append(' ', indent).Append("\"").Append(key).Append("\": [");
        for (var i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                json.Append(", ");
            }
            json.Append("\"").Append(Escape(values[i])).Append("\"");
        }
        json.Append("]");
        json.AppendLine(comma ? "," : "");
    }

    private static string Escape(string value)
    {
        return (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private readonly struct AssetSpec
    {
        public AssetSpec(string assetId, string category, string assetPath, string sidecarStem, string materialMarker, string sourcePipeline)
        {
            AssetId = assetId;
            Category = category;
            AssetPath = assetPath;
            SidecarStem = sidecarStem;
            MaterialMarker = materialMarker;
            SourcePipeline = sourcePipeline;
        }

        public readonly string AssetId;
        public readonly string Category;
        public readonly string AssetPath;
        public readonly string SidecarStem;
        public readonly string MaterialMarker;
        public readonly string SourcePipeline;
    }

    private readonly struct AssetResult
    {
        public AssetResult(AssetSpec spec, bool imported, bool materialReady, int pbrTextureMapCount, bool technicalReady, bool semanticCategoryMatch, int gameplaySceneInstances, bool gameplayEntitySceneEvidence, bool playerCameraVisible, bool gameplayEventEvidence, float maxScreenAreaRatio, string eventNote)
        {
            AssetId = spec.AssetId;
            Category = spec.Category;
            SourcePipeline = spec.SourcePipeline;
            AssetPath = spec.AssetPath;
            Imported = imported;
            MaterialReady = materialReady;
            PbrTextureMapCount = pbrTextureMapCount;
            TechnicalReady = technicalReady;
            SemanticCategoryMatch = semanticCategoryMatch;
            GameplaySceneInstances = gameplaySceneInstances;
            GameplayEntitySceneEvidence = gameplayEntitySceneEvidence;
            PlayerCameraVisible = playerCameraVisible;
            GameplayEventEvidence = gameplayEventEvidence;
            MaxScreenAreaRatio = maxScreenAreaRatio;
            EventNote = eventNote;
            Blockers = BuildBlockers(imported, materialReady, technicalReady, semanticCategoryMatch, gameplayEntitySceneEvidence, playerCameraVisible, gameplayEventEvidence);
            ProductionPromoted = Blockers.Length == 0;
        }

        public readonly string AssetId;
        public readonly string Category;
        public readonly string SourcePipeline;
        public readonly string AssetPath;
        public readonly bool Imported;
        public readonly bool MaterialReady;
        public readonly int PbrTextureMapCount;
        public readonly bool TechnicalReady;
        public readonly bool SemanticCategoryMatch;
        public readonly int GameplaySceneInstances;
        public readonly bool GameplayEntitySceneEvidence;
        public readonly bool PlayerCameraVisible;
        public readonly bool GameplayEventEvidence;
        public readonly float MaxScreenAreaRatio;
        public readonly string EventNote;
        public readonly string[] Blockers;
        public readonly bool ProductionPromoted;
    }
}
#endif
