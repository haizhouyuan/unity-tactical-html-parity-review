#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class BuildingIntegrityGate
{
    private const string ReportPath = "docs/BUILDING_INTEGRITY_GATE.json";
    private const string ScreenshotDirectory = "Assets/Screenshots/BuildingIntegrity";

    private static readonly Vector3 BuildingACenter = new(-38f, 0f, -22f);
    private static readonly Vector2 BuildingASize = new(18f, 16f);

    [MenuItem("AI Tools/Run Building Integrity Gate")]
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
        var sceneObjectsReady = FindObject("A Floor 1") != null
            && FindObject("A Interior Ladder") != null
            && FindObject("Container 1") != null;

        var spawnOutsideBuilding = false;
        var doorwayClear = false;
        var firstFloorEntry = false;
        var firstFloorSupport = false;
        var ladderUpperFloor = false;
        var upperFloorSupport = false;
        var roofEdgeDrop = false;
        var coverRayBlocked = false;
        var enemyOpenLaneDamagesPlayer = false;
        var enemyCoverPreventsDamage = false;

        if (applicationReady)
        {
            gm.StartRound();
            SetFloat(gm, "roundStartTime", Time.time - 10f);
            player.ResetView(180f, 18f);
            player.SetCameraMode(TacticalCameraMode.ThirdPerson);
            player.SetAds(false);

            spawnOutsideBuilding = !PointInsideBuildingA(player.transform.position);
            details.Append("spawnOutsideBuilding=").Append(spawnOutsideBuilding).Append("; ");

            doorwayClear = ProbeDoorwayClearance(details);
            firstFloorEntry = ProbeFirstFloorEntry(player, camera, follow, details, screenshots, ref screenshotCount);
            firstFloorSupport = ProbeFloorSupport("A Floor 1", player.transform, player.transform.position + Vector3.up * 2.4f, details);
            ladderUpperFloor = ProbeLadder(player, gm, camera, follow, details, screenshots, ref screenshotCount);
            upperFloorSupport = ProbeFloorSupport("A Floor 3", player.transform, player.transform.position + Vector3.up * 2.4f, details);
            roofEdgeDrop = ProbeRoofEdgeDrop(details);
            coverRayBlocked = ProbeContainerRayBlock(details);
            enemyOpenLaneDamagesPlayer = ProbeEnemyShotDamage(gm, player, false, details);
            enemyCoverPreventsDamage = ProbeEnemyShotDamage(gm, player, true, details);
            CaptureStep(camera, screenshots, "03_cover_block_probe", "Player positioned behind container cover for enemy line-of-sight proof", ref screenshotCount);
        }

        var passed = applicationReady
            && sceneObjectsReady
            && spawnOutsideBuilding
            && doorwayClear
            && firstFloorEntry
            && firstFloorSupport
            && ladderUpperFloor
            && upperFloorSupport
            && roofEdgeDrop
            && coverRayBlocked
            && enemyOpenLaneDamagesPlayer
            && enemyCoverPreventsDamage
            && screenshotCount >= 2;

        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "building_integrity_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "application_is_playing", Application.isPlaying, true);
        Append(json, "passed", passed, true);
        Append(json, "application_ready", applicationReady, true);
        Append(json, "scene_objects_ready", sceneObjectsReady, true);
        Append(json, "spawn_outside_building", spawnOutsideBuilding, true);
        Append(json, "doorway_clear", doorwayClear, true);
        Append(json, "first_floor_entry", firstFloorEntry, true);
        Append(json, "first_floor_support", firstFloorSupport, true);
        Append(json, "ladder_upper_floor", ladderUpperFloor, true);
        Append(json, "upper_floor_support", upperFloorSupport, true);
        Append(json, "roof_edge_drop_clear", roofEdgeDrop, true);
        Append(json, "container_raycast_blocks_line", coverRayBlocked, true);
        Append(json, "enemy_open_lane_damages_player", enemyOpenLaneDamagesPlayer, true);
        Append(json, "enemy_cover_prevents_damage", enemyCoverPreventsDamage, true);
        Append(json, "screenshot_count", screenshotCount, true);
        AppendScreenshots(json, screenshots, true);
        Append(json, "details", details.ToString().Trim(), false);
        json.AppendLine("}");

        File.WriteAllText(ReportPath, json.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] Building integrity gate report written to " + ReportPath + " passed=" + passed);
    }

    private static bool ProbeDoorwayClearance(StringBuilder details)
    {
        var start = new Vector3(BuildingACenter.x, 1.55f, BuildingACenter.z + BuildingASize.y * 0.5f + 1.25f);
        var end = new Vector3(BuildingACenter.x, 1.55f, BuildingACenter.z + BuildingASize.y * 0.5f - 2.2f);
        var direction = end - start;
        var blocked = Physics.Raycast(start, direction.normalized, out var hit, direction.magnitude, ~0, QueryTriggerInteraction.Ignore);
        details.Append("doorwayRay blocked=").Append(blocked);
        if (blocked)
        {
            details.Append(" hit=").Append(hit.collider.name);
        }
        details.Append("; ");
        return !blocked;
    }

    private static bool ProbeFirstFloorEntry(TacticalPlayerController player, Camera camera, TacticalCameraFollow follow, StringBuilder details, StringBuilder screenshots, ref int screenshotCount)
    {
        var start = new Vector3(BuildingACenter.x, 1.04f, BuildingACenter.z + BuildingASize.y * 0.5f + 1.75f);
        var target = new Vector3(BuildingACenter.x, 1.04f, BuildingACenter.z + 1.0f);
        MoveCharacter(player, start);
        player.ResetView(180f, 18f);
        follow?.SnapToPlayer();
        Physics.SyncTransforms();
        CaptureStep(camera, screenshots, "01_building_a_door_entry_start", "Standing outside Building A doorway", ref screenshotCount);

        var direction = target - start;
        var routeClear = ProbePlayerCapsulePath(player, start, target, details);
        MoveCharacter(player, target);

        Physics.SyncTransforms();
        follow?.SnapToPlayer();
        CaptureStep(camera, screenshots, "02_building_a_first_floor_inside", "Moved through doorway into Building A first floor", ref screenshotCount);

        var distance = Vector3.Distance(Flat(player.transform.position), Flat(target));
        var inside = PointInsideBuildingA(player.transform.position);
        details.Append("firstFloorEntry inside=").Append(inside)
            .Append(" routeClear=").Append(routeClear)
            .Append(" distance=").Append(distance.ToString("F2", CultureInfo.InvariantCulture)).Append("; ");
        return routeClear && inside && distance < 1.8f;
    }

    private static bool ProbePlayerCapsulePath(TacticalPlayerController player, Vector3 start, Vector3 target, StringBuilder details)
    {
        var controller = player.GetComponent<CharacterController>();
        var radius = controller == null ? 0.45f : controller.radius;
        var height = controller == null ? 2.05f : controller.height;
        var direction = target - start;
        var distance = direction.magnitude;
        if (distance < 0.1f)
        {
            return true;
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        var bottom = start + Vector3.up * (radius + 0.02f);
        var top = start + Vector3.up * Mathf.Max(radius + 0.04f, height - radius);
        var blocked = Physics.CapsuleCast(bottom, top, radius, direction.normalized, out var hit, distance, ~0, QueryTriggerInteraction.Ignore);

        if (controller != null)
        {
            controller.enabled = true;
        }

        details.Append("entryCapsule blocked=").Append(blocked);
        if (blocked)
        {
            details.Append(" hit=").Append(hit.collider.name);
        }
        details.Append("; ");
        return !blocked;
    }

    private static bool ProbeFloorSupport(string expectedName, Transform playerRoot, Vector3 rayOrigin, StringBuilder details)
    {
        var hits = Physics.RaycastAll(rayOrigin, Vector3.down, 8f, ~0, QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(playerRoot))
            {
                continue;
            }

            var found = hit.collider.name.IndexOf(expectedName, StringComparison.OrdinalIgnoreCase) >= 0;
            details.Append(expectedName).Append(" supportHit=").Append(hit.collider.name)
                .Append(" distance=").Append(hit.distance.ToString("F2", CultureInfo.InvariantCulture)).Append("; ");
            return found;
        }

        details.Append(expectedName).Append(" support=missing; ");
        return false;
    }

    private static bool ProbeLadder(TacticalPlayerController player, TacticalGameManager gm, Camera camera, TacticalCameraFollow follow, StringBuilder details, StringBuilder screenshots, ref int screenshotCount)
    {
        var ladder = FindNamed<TacticalLadder>("A Interior Ladder") ?? UnityEngine.Object.FindAnyObjectByType<TacticalLadder>();
        if (ladder == null)
        {
            details.Append("ladder=missing; ");
            return false;
        }

        var beforeFloor = GetInt(gm, "currentFloor");
        MoveCharacter(player, ladder.transform.position + new Vector3(0.5f, 0f, -0.5f));
        gm.TryPickupNearest();
        Physics.SyncTransforms();
        follow?.SnapToPlayer();
        CaptureStep(camera, screenshots, "02b_ladder_upper_floor", "Used Building A interior ladder and reached upper floor", ref screenshotCount);

        var afterFloor = GetInt(gm, "currentFloor");
        var inside = PointInsideBuildingA(player.transform.position);
        details.Append("ladder floor ").Append(beforeFloor).Append("->").Append(afterFloor)
            .Append(" y=").Append(player.transform.position.y.ToString("F2", CultureInfo.InvariantCulture))
            .Append(" inside=").Append(inside).Append("; ");
        return afterFloor > beforeFloor && player.transform.position.y > 8f && inside;
    }

    private static bool ProbeRoofEdgeDrop(StringBuilder details)
    {
        var roof = FindObject("A Roof");
        if (roof == null)
        {
            details.Append("roof=missing; ");
            return false;
        }

        var centerOrigin = roof.transform.position + Vector3.up * 2f;
        var edgeOrigin = roof.transform.position + new Vector3(0f, 2f, BuildingASize.y * 0.5f + 2.4f);
        var centerSupported = Physics.Raycast(centerOrigin, Vector3.down, out var centerHit, 4f, ~0, QueryTriggerInteraction.Ignore)
            && centerHit.collider.name.IndexOf("A Roof", StringComparison.OrdinalIgnoreCase) >= 0;
        var edgeHasImmediateFloor = Physics.Raycast(edgeOrigin, Vector3.down, out var edgeHit, 2f, ~0, QueryTriggerInteraction.Ignore)
            && edgeHit.collider.name.IndexOf("A Roof", StringComparison.OrdinalIgnoreCase) >= 0;

        details.Append("roof centerSupported=").Append(centerSupported)
            .Append(" edgeImmediateRoof=").Append(edgeHasImmediateFloor).Append("; ");
        return centerSupported && !edgeHasImmediateFloor;
    }

    private static bool ProbeContainerRayBlock(StringBuilder details)
    {
        var container = FindObject("Container 1");
        if (container == null)
        {
            details.Append("container=missing; ");
            return false;
        }

        var start = container.transform.position + new Vector3(0f, 0.2f, -5.5f);
        var end = container.transform.position + new Vector3(0f, 0.2f, 5.5f);
        var direction = end - start;
        var blocked = Physics.Raycast(start, direction.normalized, out var hit, direction.magnitude, ~0, QueryTriggerInteraction.Ignore);
        var hitContainer = blocked && IsSameOrChild(hit.collider.transform, container.transform);
        details.Append("containerRay blocked=").Append(blocked)
            .Append(" hit=").Append(blocked ? hit.collider.name : "none").Append("; ");
        return hitContainer;
    }

    private static bool ProbeEnemyShotDamage(TacticalGameManager gm, TacticalPlayerController player, bool withCover, StringBuilder details)
    {
        var enemy = UnityEngine.Object.FindAnyObjectByType<TacticalEnemy>();
        if (enemy == null)
        {
            details.Append(withCover ? "coverEnemy=missing; " : "openEnemy=missing; ");
            return false;
        }

        var container = FindObject("Container 1");
        var playerPosition = withCover && container != null
            ? container.transform.position + new Vector3(0f, -0.58f, 5.5f)
            : new Vector3(0f, 1.04f, 30f);
        var enemyPosition = withCover && container != null
            ? container.transform.position + new Vector3(0f, -0.58f, -5.5f)
            : new Vector3(0f, 1.04f, 20f);

        MoveCharacter(player, playerPosition);
        MoveEnemy(enemy, enemyPosition);
        SetFloat(gm, "hp", 100f);
        SetFloat(gm, "roundStartTime", Time.time - 10f);
        SetFloat(enemy, "nextShotTime", -10f);
        InvokePrivate(enemy, "Update");
        var after = GetFloat(gm, "hp");
        var damaged = after < 99.9f;
        details.Append(withCover ? "coverShot hp=" : "openShot hp=")
            .Append(after.ToString("F1", CultureInfo.InvariantCulture)).Append("; ");
        return withCover ? !damaged : damaged;
    }

    private static void MoveCharacter(TacticalPlayerController player, Vector3 position)
    {
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

        Physics.SyncTransforms();
    }

    private static void MoveEnemy(TacticalEnemy enemy, Vector3 position)
    {
        var controller = enemy.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        enemy.transform.position = position;
        if (controller != null)
        {
            controller.enabled = true;
        }

        enemy.Initialize(UnityEngine.Object.FindAnyObjectByType<TacticalGameManager>(), UnityEngine.Object.FindAnyObjectByType<TacticalPlayerController>().transform);
        Physics.SyncTransforms();
    }

    private static void CaptureStep(Camera camera, StringBuilder screenshots, string label, string note, ref int screenshotCount)
    {
        if (camera == null)
        {
            return;
        }

        var path = ScreenshotDirectory + "/" + label + ".png";
        RenderCameraScreenshot(camera, path);
        if (screenshots.Length > 0)
        {
            screenshots.AppendLine(",");
        }

        screenshots.Append("    { \"label\": \"").Append(label)
            .Append("\", \"path\": \"").Append(path)
            .Append("\", \"note\": \"").Append(Escape(note)).Append("\" }");
        screenshotCount++;
    }

    private static void RenderCameraScreenshot(Camera camera, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var previousTarget = camera.targetTexture;
        var renderTexture = new RenderTexture(1280, 720, 24);
        var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        camera.targetTexture = renderTexture;
        camera.Render();
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        camera.targetTexture = previousTarget;
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(texture);
        UnityEngine.Object.DestroyImmediate(renderTexture);
    }

    private static bool PointInsideBuildingA(Vector3 point)
    {
        return point.x > BuildingACenter.x - BuildingASize.x * 0.5f + 0.6f
            && point.x < BuildingACenter.x + BuildingASize.x * 0.5f - 0.6f
            && point.z > BuildingACenter.z - BuildingASize.y * 0.5f + 0.6f
            && point.z < BuildingACenter.z + BuildingASize.y * 0.5f - 0.6f;
    }

    private static Vector3 Flat(Vector3 value)
    {
        return new Vector3(value.x, 0f, value.z);
    }

    private static GameObject FindObject(string name)
    {
        foreach (var transform in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
        {
            if (transform.name == name)
            {
                return transform.gameObject;
            }
        }

        return null;
    }

    private static T FindNamed<T>(string name) where T : Component
    {
        foreach (var item in UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Exclude))
        {
            if (item.name == name)
            {
                return item;
            }
        }

        return null;
    }

    private static bool IsSameOrChild(Transform child, Transform parent)
    {
        return child == parent || child.IsChildOf(parent);
    }

    private static void InvokePrivate(object target, string methodName)
    {
        target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(target, null);
    }

    private static int GetInt(object target, string fieldName)
    {
        return (int)(target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target) ?? 0);
    }

    private static float GetFloat(object target, string fieldName)
    {
        return (float)(target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target) ?? 0f);
    }

    private static void SetFloat(object target, string fieldName, float value)
    {
        target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
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

    private static void AppendScreenshots(StringBuilder json, StringBuilder screenshots, bool comma)
    {
        json.AppendLine("  \"screenshots\": [");
        json.Append(screenshots);
        if (screenshots.Length > 0)
        {
            json.AppendLine();
        }

        json.Append("  ]");
        json.AppendLine(comma ? "," : "");
    }

    private static string Escape(string value)
    {
        return (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
#endif
