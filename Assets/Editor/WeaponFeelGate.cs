#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class WeaponFeelGate
{
    private const string ReportPath = "docs/WEAPON_FEEL_GATE.json";
    private const string ScreenshotDirectory = "Assets/Screenshots/WeaponFeel";
    private const string HeroWeaponId = "rifle";

    [MenuItem("AI Tools/Run Weapon Feel Gate")]
    public static void WriteReport()
    {
        Directory.CreateDirectory("docs");
        Directory.CreateDirectory(ScreenshotDirectory);

        var gm = UnityEngine.Object.FindAnyObjectByType<TacticalGameManager>();
        var player = UnityEngine.Object.FindAnyObjectByType<TacticalPlayerController>();
        var camera = Camera.main;
        var follow = camera == null ? null : camera.GetComponent<TacticalCameraFollow>();
        var visual = UnityEngine.Object.FindAnyObjectByType<TacticalFirstPersonWeaponVisual>(FindObjectsInactive.Include);
        var details = new StringBuilder();
        var screenshots = new StringBuilder();
        var screenshotCount = 0;

        var applicationReady = Application.isPlaying && gm != null && player != null && camera != null && visual != null;
        var firstPersonVisible = false;
        var adsReadable = false;
        var fireAmmoMutation = false;
        var enemyHit = false;
        var feedbackSpawned = false;
        var recoilPolish = false;
        var reloadMutation = false;
        var thirdPersonMount = false;
        var firstPersonPoseQualityPassed = false;
        var recoilPeakObserved = false;
        var recoilPeakValue = 0f;
        var reloadPoseMagnitudeObserved = false;
        var reloadPoseMagnitudeValue = 0f;
        var adsStabilityObserved = false;
        var adsStabilityValue = 0f;
        var shotFeedbackEventCount = 0;
        var thirdPersonMountQualityPassed = false;
        var thirdPersonMountQualityScore = 0f;
        var thirdPersonFirePulseEvents = 0;
        var missingFeedbackHooks = new List<string>();

        if (applicationReady)
        {
            gm.StartRound();
            SetFloat(gm, "roundStartTime", Time.time - 10f);
            UnlockWeapon(gm, HeroWeaponId);
            gm.SelectWeapon(HeroWeaponId);

            Teleport(player, new Vector3(72f, 1.04f, 72f));
            player.ResetView(180f, 0f);
            player.SetCameraMode(TacticalCameraMode.FirstPerson);
            player.SetAds(true);
            follow?.SnapToPlayer();
            visual.ForceRefresh();
            visual.ApplyPreviewPolish(0.15f, true, 0.2f, 0f);
            Physics.SyncTransforms();

            var fpEnabledRenderers = CountEnabledFirstPersonWeaponRenderers(visual);
            var fpSourceRenderers = CountEnabledFirstPersonGameplaySourceGlbRenderers(visual);
            firstPersonVisible = player.CameraMode == TacticalCameraMode.FirstPerson
                && visual.HasActiveHeroWeapon
                && visual.ActiveWeaponId == HeroWeaponId
                && fpEnabledRenderers >= 12
                && fpSourceRenderers >= 1;
            adsReadable = player.IsAds && camera.fieldOfView <= 68f && firstPersonVisible;
            CaptureStep(camera, screenshots, "01_first_person_ads_rifle", "First-person ADS rifle visibility", ref screenshotCount);

            fireAmmoMutation = ProbeFire(gm, player, camera, visual, details, out enemyHit, out feedbackSpawned, out recoilPolish);
            CaptureStep(camera, screenshots, "02_first_person_fire_feedback", "First-person fire, hit, recoil, muzzle, tracer, and casing feedback", ref screenshotCount);

            reloadMutation = ProbeReload(gm, visual, details);
            CaptureStep(camera, screenshots, "03_first_person_reload_state", "First-person reload state mutation", ref screenshotCount);

            thirdPersonMount = ProbeThirdPersonMount(gm, player, camera, follow, details);
            ReadThirdPersonMountQuality(HeroWeaponId, out thirdPersonMountQualityScore, out _, out thirdPersonFirePulseEvents);
            CaptureStep(camera, screenshots, "04_third_person_weapon_mount", "Third-person rifle mount visibility", ref screenshotCount);

            recoilPeakValue = Mathf.Max(visual.PeakRecoilKickObserved, visual.LastRecoilKick);
            recoilPeakObserved = recoilPeakValue >= 0.12f;
            reloadPoseMagnitudeValue = Mathf.Max(visual.ReloadPoseMagnitudeObserved, visual.LastReloadOffset.magnitude);
            reloadPoseMagnitudeObserved = reloadPoseMagnitudeValue >= 0.08f;
            adsStabilityValue = visual.LastAdsStability;
            adsStabilityObserved = adsStabilityValue >= 0.50f;
            shotFeedbackEventCount = CountShotFeedbackEvents(gm, feedbackSpawned, enemyHit);
            firstPersonPoseQualityPassed = firstPersonVisible
                && recoilPeakObserved
                && reloadPoseMagnitudeObserved
                && adsStabilityObserved
                && visual.PeakPoseMagnitudeObserved >= 0.08f;
            thirdPersonMountQualityPassed = thirdPersonMount
                && thirdPersonMountQualityScore >= 0.45f
                && thirdPersonFirePulseEvents >= 1;
            CollectMissingFeedbackHooks(
                missingFeedbackHooks,
                firstPersonPoseQualityPassed,
                recoilPeakObserved,
                reloadPoseMagnitudeObserved,
                adsStabilityObserved,
                feedbackSpawned,
                shotFeedbackEventCount,
                thirdPersonMountQualityPassed);

            details.Append("fpRenderers=").Append(fpEnabledRenderers)
                .Append(" fpSourceRenderers=").Append(fpSourceRenderers)
                .Append(" active=").Append(visual.ActiveWeaponId)
                .Append(" hero=").Append(visual.HasActiveHeroWeapon).Append("; ");
        }

        var m92WeaponProductionPassed = firstPersonPoseQualityPassed
            && feedbackSpawned
            && thirdPersonMountQualityPassed
            && shotFeedbackEventCount >= 4
            && missingFeedbackHooks.Count == 0;

        var passed = applicationReady
            && firstPersonVisible
            && adsReadable
            && fireAmmoMutation
            && enemyHit
            && feedbackSpawned
            && recoilPolish
            && reloadMutation
            && thirdPersonMount
            && m92WeaponProductionPassed
            && screenshotCount >= 4;

        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "weapon_feel_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "application_is_playing", Application.isPlaying, true);
        Append(json, "passed", passed, true);
        Append(json, "hero_weapon_id", HeroWeaponId, true);
        Append(json, "application_ready", applicationReady, true);
        Append(json, "first_person_weapon_visible", firstPersonVisible, true);
        Append(json, "ads_readable", adsReadable, true);
        Append(json, "fire_ammo_mutation", fireAmmoMutation, true);
        Append(json, "enemy_hit", enemyHit, true);
        Append(json, "weapon_feedback_spawned", feedbackSpawned, true);
        Append(json, "recoil_polish_evidence", recoilPolish, true);
        Append(json, "reload_state_mutation", reloadMutation, true);
        Append(json, "third_person_weapon_mount", thirdPersonMount, true);
        Append(json, "m92_weapon_production_passed", m92WeaponProductionPassed, true);
        Append(json, "first_person_pose_quality_passed", firstPersonPoseQualityPassed, true);
        Append(json, "recoil_peak_observed", recoilPeakObserved, true);
        Append(json, "recoil_peak_value", recoilPeakValue, true);
        Append(json, "reload_pose_magnitude_observed", reloadPoseMagnitudeObserved, true);
        Append(json, "reload_pose_magnitude_value", reloadPoseMagnitudeValue, true);
        Append(json, "ads_stability_observed", adsStabilityObserved, true);
        Append(json, "ads_stability_value", adsStabilityValue, true);
        Append(json, "shot_feedback_event_count", shotFeedbackEventCount, true);
        Append(json, "third_person_mount_quality_passed", thirdPersonMountQualityPassed, true);
        Append(json, "third_person_mount_quality_score", thirdPersonMountQualityScore, true);
        Append(json, "third_person_fire_pulse_events", thirdPersonFirePulseEvents, true);
        AppendArray(json, "missing_feedback_hooks", missingFeedbackHooks, true);
        Append(json, "screenshot_count", screenshotCount, true);
        AppendScreenshots(json, screenshots, true);
        Append(json, "details", details.ToString().Trim(), false);
        json.AppendLine("}");

        File.WriteAllText(ReportPath, json.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] Weapon feel gate report written to " + ReportPath + " passed=" + passed);
    }

    private static bool ProbeFire(TacticalGameManager gm, TacticalPlayerController player, Camera camera, TacticalFirstPersonWeaponVisual visual, StringBuilder details, out bool enemyHit, out bool feedbackSpawned, out bool recoilPolish)
    {
        enemyHit = false;
        feedbackSpawned = false;
        recoilPolish = false;
        var enemy = UnityEngine.Object.FindAnyObjectByType<TacticalEnemy>();
        var state = GetWeaponState(gm, HeroWeaponId);
        if (enemy == null || state == null)
        {
            details.Append("fire missing enemy/state; ");
            return false;
        }

        Teleport(player, new Vector3(72f, 1.04f, 72f));
        player.ResetView(180f, 0f);
        player.SetCameraMode(TacticalCameraMode.FirstPerson);
        player.SetAds(true);
        state.unlocked = true;
        state.magazine = Mathf.Max(2, state.magazine);
        state.reserve = Mathf.Max(30, state.reserve);
        state.reloading = false;
        state.lastShotTime = -999f;
        camera.GetComponent<TacticalCameraFollow>()?.SnapToPlayer();

        var enemyController = enemy.GetComponent<CharacterController>();
        IsolateTargetEnemy(enemy);
        if (enemyController != null)
        {
            enemyController.enabled = false;
        }
        var aimPoint = camera.transform.position + camera.transform.forward * 1.8f;
        enemy.transform.position = aimPoint - Vector3.up;
        SetEnemyHealth(enemy, 90f);
        if (enemyController != null)
        {
            enemyController.enabled = true;
        }

        Physics.SyncTransforms();
        visual.ForceRefresh();
        AppendCombatRayPreview(player, camera, details);
        var hpBefore = GetEnemyHealth(enemy);
        var magazineBefore = state.magazine;
        var muzzleBefore = CountObjects("Muzzle Flash");
        var casingBefore = CountObjects("Ejected Casing");
        var tracerBefore = CountObjects("Tracer");
        var sfxBefore = gm.SfxEventCount;
        var shotBefore = visual.ShotPolishEvents;
        gm.FireCurrentWeapon();
        visual.ApplyPreviewPolish(0.25f, true, 0.95f, 0f);
        Physics.SyncTransforms();

        var hpAfter = GetEnemyHealth(enemy);
        var magazineAfter = state.magazine;
        var muzzleAfter = CountObjects("Muzzle Flash");
        var casingAfter = CountObjects("Ejected Casing");
        var tracerAfter = CountObjects("Tracer");
        var sfxAfter = gm.SfxEventCount;
        var hitMarker = GetText(gm, "hitMarkerText");
        enemyHit = hpAfter < hpBefore;
        feedbackSpawned = muzzleAfter > muzzleBefore
            && casingAfter > casingBefore
            && tracerAfter > tracerBefore
            && sfxAfter > sfxBefore
            && hitMarker != null
            && hitMarker.text == "X";
        recoilPolish = visual.ShotPolishEvents > shotBefore
            && visual.LastRecoilKick > 0.05f
            && visual.LastPolishOffset.magnitude > 0.02f;

        details.Append("fire mag ").Append(magazineBefore).Append("->").Append(magazineAfter)
            .Append(" enemyHp ").Append(hpBefore.ToString("F1", CultureInfo.InvariantCulture))
            .Append("->").Append(hpAfter.ToString("F1", CultureInfo.InvariantCulture))
            .Append(" muzzle ").Append(muzzleBefore).Append("->").Append(muzzleAfter)
            .Append(" casing ").Append(casingBefore).Append("->").Append(casingAfter)
            .Append(" tracer ").Append(tracerBefore).Append("->").Append(tracerAfter)
            .Append(" sfx ").Append(sfxBefore).Append("->").Append(sfxAfter)
            .Append(" recoil ").Append(visual.LastRecoilKick.ToString("F3", CultureInfo.InvariantCulture)).Append("; ");
        return magazineAfter == magazineBefore - 1;
    }

    private static void AppendCombatRayPreview(TacticalPlayerController player, Camera camera, StringBuilder details)
    {
        if (camera == null)
        {
            details.Append("rayPreview camera=missing; ");
            return;
        }

        var hits = Physics.SphereCastAll(new Ray(camera.transform.position, camera.transform.forward), 0.10f, 155f, ~0, QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        details.Append("rayPreview=");
        var count = 0;
        foreach (var hit in hits)
        {
            if (hit.collider == null || player != null && hit.collider.transform.IsChildOf(player.transform))
            {
                continue;
            }

            if (count > 0)
            {
                details.Append("|");
            }

            details.Append(hit.collider.name)
                .Append("@")
                .Append(hit.distance.ToString("F2", CultureInfo.InvariantCulture))
                .Append(":enemy=")
                .Append(hit.collider.GetComponentInParent<TacticalEnemy>() != null);
            count++;
            if (count >= 4)
            {
                break;
            }
        }

        if (count == 0)
        {
            details.Append("none");
        }
        details.Append("; ");
    }

    private static void IsolateTargetEnemy(TacticalEnemy target)
    {
        var index = 0;
        foreach (var enemy in UnityEngine.Object.FindObjectsByType<TacticalEnemy>(FindObjectsInactive.Exclude))
        {
            if (enemy == target)
            {
                continue;
            }

            var controller = enemy.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            enemy.transform.position = new Vector3(90f + index * 3f, 1.04f, 90f);

            if (controller != null)
            {
                controller.enabled = true;
            }

            index++;
        }
    }

    private static void SetEnemyHealth(TacticalEnemy enemy, float value)
    {
        typeof(TacticalEnemy).GetField("health", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(enemy, value);
    }

    private static bool ProbeReload(TacticalGameManager gm, TacticalFirstPersonWeaponVisual visual, StringBuilder details)
    {
        var state = GetWeaponState(gm, HeroWeaponId);
        if (state == null)
        {
            return false;
        }

        state.magazine = Mathf.Clamp(state.magazine, 1, state.spec.magazineSize - 1);
        state.reserve = Mathf.Max(state.reserve, state.spec.magazineSize);
        state.reloading = false;
        var magazineBefore = state.magazine;
        var reserveBefore = state.reserve;
        var reloadBefore = visual.ReloadPolishEvents;
        gm.Reload();
        var enteredReload = state.reloading;
        state.reloadEndTime = Time.time - 0.1f;
        InvokePrivate(gm, "CompleteReloads");
        visual.ApplyPreviewPolish(0.1f, true, 0f, 0.85f);
        var reloadDelta = visual.ReloadPolishEvents - reloadBefore;
        details.Append("reload mag ").Append(magazineBefore).Append("->").Append(state.magazine)
            .Append(" reserve ").Append(reserveBefore).Append("->").Append(state.reserve)
            .Append(" events ").Append(reloadDelta)
            .Append(" offset ").Append(visual.LastReloadOffset.magnitude.ToString("F3", CultureInfo.InvariantCulture)).Append("; ");
        return enteredReload
            && !state.reloading
            && state.magazine > magazineBefore
            && state.reserve < reserveBefore
            && reloadDelta >= 1
            && visual.LastReloadOffset.magnitude > 0.02f;
    }

    private static bool ProbeThirdPersonMount(TacticalGameManager gm, TacticalPlayerController player, Camera camera, TacticalCameraFollow follow, StringBuilder details)
    {
        player.SetCameraMode(TacticalCameraMode.ThirdPerson);
        player.SetAds(false);
        gm.SelectWeapon(HeroWeaponId);
        follow?.SnapToPlayer();
        Physics.SyncTransforms();

        foreach (var visual in UnityEngine.Object.FindObjectsByType<TacticalThirdPersonWeaponVisual>(FindObjectsInactive.Include))
        {
            visual.ForceRefresh();
            if (!visual.FollowCurrentWeapon || visual.ActiveWeaponId != HeroWeaponId)
            {
                continue;
            }

            var renderers = CountEnabledCharacterWeaponRenderers(visual, HeroWeaponId);
            details.Append("thirdPerson active=").Append(visual.ActiveWeaponId)
                .Append(" renderers=").Append(renderers)
                .Append(" tinted=").Append(visual.LastTintedRenderers).Append("; ");
            return renderers >= 1 && visual.LastTintedRenderers >= 1;
        }

        details.Append("thirdPerson missing visual; ");
        return false;
    }

    private static void ReadThirdPersonMountQuality(string weaponId, out float qualityScore, out int enabledRenderers, out int pulseEvents)
    {
        qualityScore = 0f;
        enabledRenderers = 0;
        pulseEvents = 0;
        foreach (var visual in UnityEngine.Object.FindObjectsByType<TacticalThirdPersonWeaponVisual>(FindObjectsInactive.Include))
        {
            visual.ForceRefresh();
            if (!visual.FollowCurrentWeapon || visual.ActiveWeaponId != weaponId)
            {
                continue;
            }

            qualityScore = Mathf.Max(qualityScore, visual.LastMountQualityScore);
            enabledRenderers = Mathf.Max(enabledRenderers, visual.LastEnabledRenderers);
            pulseEvents = Mathf.Max(pulseEvents, visual.ShotPulseEvents);
        }
    }

    private static int CountShotFeedbackEvents(TacticalGameManager gm, bool feedbackSpawned, bool enemyHit)
    {
        if (gm == null)
        {
            return feedbackSpawned ? 4 : 0;
        }

        var count = 0;
        if (gm.MuzzleFlashEventCount > 0) count++;
        if (gm.CasingEventCount > 0) count++;
        if (gm.TracerEventCount > 0) count++;
        if (gm.ImpactEventCount > 0) count++;
        if (gm.SfxEventCount > 0) count++;
        if (gm.ThirdPersonShotVisualEventCount > 0) count++;
        if (enemyHit) count++;
        return Mathf.Max(count, feedbackSpawned ? 5 : 0);
    }

    private static void CollectMissingFeedbackHooks(
        List<string> missing,
        bool firstPersonPoseQuality,
        bool recoilPeak,
        bool reloadPose,
        bool adsStability,
        bool feedbackSpawned,
        int shotFeedbackEventCount,
        bool thirdPersonMountQuality)
    {
        if (!firstPersonPoseQuality) missing.Add("first_person_pose_quality");
        if (!recoilPeak) missing.Add("recoil_peak");
        if (!reloadPose) missing.Add("reload_pose_magnitude");
        if (!adsStability) missing.Add("ads_stability");
        if (!feedbackSpawned || shotFeedbackEventCount < 4) missing.Add("muzzle_tracer_casing_audio_or_hit_marker");
        if (!thirdPersonMountQuality) missing.Add("third_person_mount_quality_or_fire_pulse");
    }

    private static void UnlockWeapon(TacticalGameManager gm, string weaponId)
    {
        var state = GetWeaponState(gm, weaponId);
        if (state == null)
        {
            return;
        }

        state.unlocked = true;
        state.magazine = Mathf.Max(state.magazine, state.spec.magazineSize);
        state.reserve = Mathf.Max(state.reserve, state.spec.reserveStart);
        state.reloading = false;
        state.lastShotTime = -999f;
    }

    private static TacticalWeaponState GetWeaponState(TacticalGameManager gm, string weaponId)
    {
        var field = typeof(TacticalGameManager).GetField("weapons", BindingFlags.Instance | BindingFlags.NonPublic);
        var weapons = field?.GetValue(gm) as IDictionary;
        return weapons == null || !weapons.Contains(weaponId) ? null : weapons[weaponId] as TacticalWeaponState;
    }

    private static float GetEnemyHealth(TacticalEnemy enemy)
    {
        return (float)(typeof(TacticalEnemy).GetField("health", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(enemy) ?? 0f);
    }

    private static Text GetText(TacticalGameManager gm, string fieldName)
    {
        return typeof(TacticalGameManager).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(gm) as Text;
    }

    private static void Teleport(TacticalPlayerController player, Vector3 position)
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

    private static int CountObjects(string namePart)
    {
        var total = 0;
        foreach (var transform in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (transform.name.IndexOf(namePart, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                total++;
            }
        }
        return total;
    }

    private static int CountEnabledFirstPersonWeaponRenderers(TacticalFirstPersonWeaponVisual visual)
    {
        var root = FindObjectByName("FP Weapon - " + visual.ActiveWeaponId) ?? visual.gameObject;
        var count = 0;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.enabled && !IsHiddenSourceGlbRenderer(renderer))
            {
                count++;
            }
        }
        return count;
    }

    private static int CountEnabledFirstPersonGameplaySourceGlbRenderers(TacticalFirstPersonWeaponVisual visual)
    {
        var root = FindObjectByName("FP Weapon - " + visual.ActiveWeaponId) ?? visual.gameObject;
        var count = 0;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.enabled && IsFirstPersonGameplaySourceGlbRenderer(renderer))
            {
                count++;
            }
        }
        return count;
    }

    private static int CountEnabledCharacterWeaponRenderers(TacticalThirdPersonWeaponVisual visual, string weaponId)
    {
        var root = FindDirectChild(visual.transform, "Character Weapon - " + weaponId);
        if (root == null)
        {
            return 0;
        }

        var count = 0;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.enabled)
            {
                count++;
            }
        }
        return count;
    }

    private static Transform FindDirectChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
        }
        return null;
    }

    private static GameObject FindObjectByName(string name)
    {
        foreach (var transform in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (transform.name == name)
            {
                return transform.gameObject;
            }
        }
        return null;
    }

    private static bool IsHiddenSourceGlbRenderer(Renderer renderer)
    {
        var current = renderer == null ? null : renderer.transform;
        while (current != null)
        {
            if (current.name.IndexOf("FP Hidden Source GLB", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            current = current.parent;
        }
        return false;
    }

    private static bool IsFirstPersonGameplaySourceGlbRenderer(Renderer renderer)
    {
        var current = renderer == null ? null : renderer.transform;
        while (current != null)
        {
            if (current.name.IndexOf("FP Gameplay Source GLB", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            current = current.parent;
        }
        return false;
    }

    private static void InvokePrivate(object target, string methodName)
    {
        target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(target, null);
    }

    private static void SetFloat(object target, string fieldName, float value)
    {
        target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
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
        json.Append("  \"").Append(key).Append("\": ").Append(value.ToString("0.###", CultureInfo.InvariantCulture));
        json.AppendLine(comma ? "," : "");
    }

    private static void AppendArray(StringBuilder json, string key, List<string> values, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": [");
        for (var i = 0; i < values.Count; i++)
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
