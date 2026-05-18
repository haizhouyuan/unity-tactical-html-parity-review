#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class TacticalPlayerPovGate
{
    private const string ReportPath = "docs/TACTICAL_PLAYER_POV_GATE.json";
    private const string ScreenshotPath = "Assets/Screenshots/tactical_html_replica_current_player_pov_verified.png";

    [MenuItem("AI Tools/Capture Tactical Verified Player POV Screenshot")]
    public static void CaptureVerifiedScreenshot()
    {
        Directory.CreateDirectory("Assets/Screenshots");
        var gm = UnityEngine.Object.FindAnyObjectByType<TacticalGameManager>();
        var player = UnityEngine.Object.FindAnyObjectByType<TacticalPlayerController>();
        var camera = Camera.main;
        var prepared = PrepareStartedPlayerPov(gm, player, camera);
        Canvas.ForceUpdateCanvases();
        if (prepared)
        {
            RenderCameraScreenshot(camera, ScreenshotPath);
        }

        Debug.Log("[AI Tools] Tactical verified player POV camera render written to "
            + ScreenshotPath + " prepared=" + prepared);
    }

    [MenuItem("AI Tools/Write Tactical Player POV Gate Report")]
    public static void WriteReport()
    {
        Directory.CreateDirectory("docs");

        var gm = UnityEngine.Object.FindAnyObjectByType<TacticalGameManager>();
        var player = UnityEngine.Object.FindAnyObjectByType<TacticalPlayerController>();
        var camera = Camera.main;
        var preparedForGate = PrepareStartedPlayerPov(gm, player, camera);
        var lobby = FindObjectByName("Lobby Panel");
        var death = FindObjectByName("Death Panel");
        var help = FindObjectByName("Help Panel");
        var settings = FindObjectByName("Settings Panel");
        var skins = FindObjectByName("Skins Panel");
        var prompt = FindObjectByName("Prompt Text")?.GetComponent<Text>();
        var hitMarker = FindObjectByName("Hit Marker Text")?.GetComponent<Text>();
        var enemies = UnityEngine.Object.FindObjectsByType<TacticalEnemy>(FindObjectsInactive.Exclude);
        var loots = UnityEngine.Object.FindObjectsByType<TacticalLoot>(FindObjectsInactive.Exclude);
        var ladders = UnityEngine.Object.FindObjectsByType<TacticalLadder>(FindObjectsInactive.Exclude);
        var runtimeMounts = UnityEngine.Object.FindObjectsByType<HtmlGlbAssetMount>(FindObjectsInactive.Include);
        var allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        var firstPersonWeaponHidden = FirstPersonWeaponHiddenInThirdPerson();

        var glbInstances = 0;
        var activeGlbInstances = 0;
        var glbRenderers = 0;
        var activeGlbRenderers = 0;
        var glbInstancesInCameraFrustum = 0;
        var spawnDetailObjects = 0;
        var foregroundGlbInstances = 0;
        var planes = camera == null ? null : GeometryUtility.CalculateFrustumPlanes(camera);

        foreach (var obj in allObjects)
        {
            if (obj.name.StartsWith("Spawn ", StringComparison.Ordinal))
            {
                spawnDetailObjects++;
            }

            if (!obj.name.StartsWith("HTML GLB Instance -", StringComparison.Ordinal))
            {
                continue;
            }

            glbInstances++;
            if (obj.activeInHierarchy)
            {
                activeGlbInstances++;
            }

            var renderers = obj.GetComponentsInChildren<Renderer>(true);
            var instanceVisible = false;
            foreach (var renderer in renderers)
            {
                glbRenderers++;
                if (!renderer.enabled)
                {
                    continue;
                }

                activeGlbRenderers++;
                if (planes != null && GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
                {
                    instanceVisible = true;
                }
            }

            if (instanceVisible)
            {
                glbInstancesInCameraFrustum++;
                if (camera != null && Vector3.Distance(camera.transform.position, obj.transform.position) < 34f)
                {
                    foregroundGlbInstances++;
                }
            }
        }

        var lobbyActive = lobby != null && lobby.activeInHierarchy;
        var deathActive = death != null && death.activeInHierarchy;
        var helpPanelPresent = help != null;
        var settingsPanelPresent = settings != null;
        var skinsPanelPresent = skins != null;
        var sidePanelsHidden = (help == null || !help.activeInHierarchy)
            && (settings == null || !settings.activeInHierarchy)
            && (skins == null || !skins.activeInHierarchy);
        var promptCleared = prompt == null || string.IsNullOrWhiteSpace(prompt.text);
        var hitMarkerCleared = hitMarker == null || string.IsNullOrWhiteSpace(hitMarker.text);
        var playerPosition = player == null ? Vector3.zero : player.transform.position;
        var cameraPosition = camera == null ? Vector3.zero : camera.transform.position;
        var cameraPlayerDistance = player == null || camera == null ? 999f : Vector3.Distance(playerPosition, cameraPosition);
        var cameraTargetClear = HasClearCameraTarget(player, camera, out var cameraTargetBlocker);
        var playerCharacterVisible = IsPlayerCharacterVisible(player, camera);
        var passed = Application.isPlaying
            && preparedForGate
            && player != null
            && camera != null
            && !lobbyActive
            && !deathActive
            && cameraTargetClear
            && playerCharacterVisible
            && firstPersonWeaponHidden
            && cameraPlayerDistance >= 1.45f
            && cameraPlayerDistance <= 14f
            && enemies.Length >= 9
            && loots.Length >= 34
            && ladders.Length >= 4
            && glbInstances >= 80
            && activeGlbRenderers >= 1000
            && runtimeMounts.Length == 0
            && spawnDetailObjects >= 12
            && helpPanelPresent
            && settingsPanelPresent
            && skinsPanelPresent
            && sidePanelsHidden
            && promptCleared
            && hitMarkerCleared
            && glbInstancesInCameraFrustum >= 6
            && foregroundGlbInstances >= 3;

        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "tactical_player_pov_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "application_is_playing", Application.isPlaying, true);
        Append(json, "passed", passed, true);
        Append(json, "prepared_for_gate", preparedForGate, true);
        Append(json, "lobby_active", lobbyActive, true);
        Append(json, "death_active", deathActive, true);
        Append(json, "camera_target_clear", cameraTargetClear, true);
        Append(json, "camera_target_blocker", cameraTargetBlocker, true);
        Append(json, "player_character_visible", playerCharacterVisible, true);
        Append(json, "first_person_weapon_hidden_in_third_person", firstPersonWeaponHidden, true);
        Append(json, "help_panel_present", helpPanelPresent, true);
        Append(json, "settings_panel_present", settingsPanelPresent, true);
        Append(json, "skins_panel_present", skinsPanelPresent, true);
        Append(json, "side_panels_hidden_for_gameplay", sidePanelsHidden, true);
        Append(json, "prompt_cleared_on_start", promptCleared, true);
        Append(json, "hit_marker_cleared_on_start", hitMarkerCleared, true);
        Append(json, "enemy_count", enemies.Length, true);
        Append(json, "loot_count", loots.Length, true);
        Append(json, "ladder_count", ladders.Length, true);
        Append(json, "glb_instances", glbInstances, true);
        Append(json, "active_glb_instances", activeGlbInstances, true);
        Append(json, "glb_renderers", glbRenderers, true);
        Append(json, "active_glb_renderers", activeGlbRenderers, true);
        Append(json, "runtime_glb_mounts", runtimeMounts.Length, true);
        Append(json, "spawn_detail_objects", spawnDetailObjects, true);
        Append(json, "glb_instances_in_camera_frustum", glbInstancesInCameraFrustum, true);
        Append(json, "foreground_glb_instances", foregroundGlbInstances, true);
        Append(json, "player_position", FormatVector(playerPosition), true);
        Append(json, "camera_position", FormatVector(cameraPosition), true);
        Append(json, "camera_player_distance", cameraPlayerDistance, true);
        Append(json, "scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().path, false);
        json.AppendLine("}");

        File.WriteAllText(ReportPath, json.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] Tactical player POV gate report written to " + ReportPath + " passed=" + passed);
    }

    private static void RenderCameraScreenshot(Camera camera, string path)
    {
        if (camera == null)
        {
            return;
        }

        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude);
        var previousModes = new RenderMode[canvases.Length];
        var previousCameras = new Camera[canvases.Length];
        var previousPlaneDistances = new float[canvases.Length];
        var renderTexture = new RenderTexture(1920, 1080, 24);
        var texture = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        try
        {
            for (var i = 0; i < canvases.Length; i++)
            {
                previousModes[i] = canvases[i].renderMode;
                previousCameras[i] = canvases[i].worldCamera;
                previousPlaneDistances[i] = canvases[i].planeDistance;
                if (canvases[i].renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                    canvases[i].worldCamera = camera;
                    canvases[i].planeDistance = 1f;
                }
            }

            Canvas.ForceUpdateCanvases();
            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            camera.Render();
            texture.ReadPixels(new Rect(0f, 0f, 1920f, 1080f), 0, 0);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.Refresh();
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            for (var i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] == null)
                {
                    continue;
                }

                canvases[i].renderMode = previousModes[i];
                canvases[i].worldCamera = previousCameras[i];
                canvases[i].planeDistance = previousPlaneDistances[i];
            }

            Canvas.ForceUpdateCanvases();
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }
    }

    private static bool PrepareStartedPlayerPov(TacticalGameManager gm, TacticalPlayerController player, Camera camera)
    {
        if (!Application.isPlaying || gm == null || player == null || camera == null)
        {
            return false;
        }

        gm.StartRound();
        player.SetCameraMode(TacticalCameraMode.ThirdPerson);
        player.SetAds(false);
        player.SetStance(TacticalStance.Stand, true);
        player.ResetView(180f, 18f);
        RefreshWeaponVisuals();
        var follow = camera.GetComponent<TacticalCameraFollow>();
        follow?.SnapToPlayer();
        Physics.SyncTransforms();
        return !gm.IsInLobby && !gm.IsPlayerDown;
    }

    private static void RefreshWeaponVisuals()
    {
        foreach (var firstPersonVisual in UnityEngine.Object.FindObjectsByType<TacticalFirstPersonWeaponVisual>(FindObjectsInactive.Include))
        {
            firstPersonVisual.ForceRefresh();
        }

        foreach (var thirdPersonVisual in UnityEngine.Object.FindObjectsByType<TacticalThirdPersonWeaponVisual>(FindObjectsInactive.Include))
        {
            thirdPersonVisual.ForceRefresh();
        }
    }

    private static bool FirstPersonWeaponHiddenInThirdPerson()
    {
        foreach (var firstPersonVisual in UnityEngine.Object.FindObjectsByType<TacticalFirstPersonWeaponVisual>(FindObjectsInactive.Include))
        {
            foreach (var renderer in firstPersonVisual.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.enabled && renderer.gameObject.activeInHierarchy)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool IsPlayerCharacterVisible(TacticalPlayerController player, Camera camera)
    {
        if (player == null || camera == null)
        {
            return false;
        }

        var planes = GeometryUtility.CalculateFrustumPlanes(camera);
        foreach (var renderer in player.GetComponentsInChildren<Renderer>(true))
        {
            if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (renderer.transform.root != player.transform)
            {
                continue;
            }

            if (renderer.transform.GetComponentInParent<TacticalFirstPersonWeaponVisual>() != null)
            {
                continue;
            }

            if (renderer.transform.GetComponentInParent<TacticalThirdPersonWeaponVisual>() != null)
            {
                continue;
            }

            if (GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasClearCameraTarget(TacticalPlayerController player, Camera camera, out string blockerName)
    {
        blockerName = "";
        if (player == null || camera == null)
        {
            blockerName = "missing-player-or-camera";
            return false;
        }

        var origin = camera.transform.position;
        var target = player.CameraTarget.position;
        var offset = target - origin;
        var distance = offset.magnitude;
        if (distance < 0.05f)
        {
            blockerName = "camera-too-close";
            return false;
        }

        var hits = Physics.SphereCastAll(origin, 0.12f, offset / distance, distance, ~0, QueryTriggerInteraction.Ignore);
        var nearestDistance = distance;
        foreach (var hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            var hitTransform = hit.collider.transform;
            if (hitTransform == player.transform || hitTransform.IsChildOf(player.transform))
            {
                continue;
            }

            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                blockerName = hit.collider.name;
            }
        }

        return blockerName.Length == 0;
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

    private static void Append(StringBuilder json, string key, float value, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": ").Append(value.ToString("F2", CultureInfo.InvariantCulture));
        json.AppendLine(comma ? "," : "");
    }

    private static string FormatVector(Vector3 vector)
    {
        return vector.x.ToString("F2", CultureInfo.InvariantCulture) + ","
            + vector.y.ToString("F2", CultureInfo.InvariantCulture) + ","
            + vector.z.ToString("F2", CultureInfo.InvariantCulture);
    }

    private static string Escape(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private static GameObject FindObjectByName(string objectName)
    {
        foreach (var obj in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
        {
            if (obj.name == objectName)
            {
                return obj;
            }
        }

        return null;
    }
}
#endif
