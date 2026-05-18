#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class TacticalAcceptancePipeline
{
    private const string ReportPath = "docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json";
    private const string PlayerPovReportPath = "docs/TACTICAL_PLAYER_POV_GATE.json";
    private const string GameplayReportPath = "docs/TACTICAL_GAMEPLAY_PROOF_GATE.json";
    private const string RouteReportPath = "docs/TACTICAL_PLAYABLE_ROUTE_GATE.json";
    private const string BuildingIntegrityReportPath = "docs/BUILDING_INTEGRITY_GATE.json";
    private const string WeaponFeelReportPath = "docs/WEAPON_FEEL_GATE.json";
    private const string AiPlaytestReportPath = "docs/AI_PLAYTEST_ROUTE_GATE.json";
    private const string M84AssetFactorySpikeReportPath = "docs/M84_THREE_CLASS_ASSET_FACTORY_SPIKE.json";
    private const string HtmlParityReportPath = "docs/HTML_TACTICAL_PARITY_GATE.json";
    private const string RealifiedImportMaterialGatePath = "docs/REALIFIED_IMPORT_MATERIAL_GATE.json";
    private const string RealifiedPromotionQueuePath = "docs/REALIFIED_ASSET_CLASS_PROMOTION_QUEUE.json";
    private const string RealifiedAssetGameplayPromotionLedgerPath = "docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json";
    private const string PromotedAssetVisibilityGatePath = "docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json";
    private const string RealifiedAuditPath = "docs/REALIFIED_ASSETS_IMPORT_AUDIT.json";
    private const string RealifiedUrpMaterialPath = "docs/REALIFIED_URP_MATERIAL_VALIDATION.json";
    private const string NemotronReviewPath = "docs/REALIFIED_ASSETS_NEMOTRON_CONTACT_SHEET_REVIEW.json";
    private const string RealifiedCategorySheetPath = "docs/REALIFIED_CATEGORY_CONTACT_SHEETS.json";
    private const string RealifiedCategoryNemotronPath = "docs/REALIFIED_CATEGORY_NEMOTRON_REVIEWS.json";
    private const string RealifiedSourceTracePath = "docs/REALIFIED_ASSET_SOURCE_TRACE.json";
    private const string HtmlBaselineCategorySheetPath = "docs/HTML_BASELINE_CATEGORY_CONTACT_SHEETS.json";
    private const string HtmlBaselineCategoryNemotronPath = "docs/HTML_BASELINE_CATEGORY_NEMOTRON_REVIEWS.json";
    private const string PlayerPovScreenshotPath = "Assets/Screenshots/tactical_html_replica_current_player_pov_verified.png";
    private const string PlayableRouteScreenshotDirectory = "Assets/Screenshots/PlayableRoute";
    private const string FirstPersonFireScreenshotPath = "Assets/Screenshots/PlayableRoute/08_fire_hit_first_person.png";
    private const string RealifiedContactSheetPath = "Assets/Screenshots/glb_pbr_textured_contact_sheet.png";
    private const string RunningKey = "TacticalAcceptancePipeline.Running";
    private const string StageKey = "TacticalAcceptancePipeline.Stage";
    private const string NotesKey = "TacticalAcceptancePipeline.Notes";
    private const string StartedUtcKey = "TacticalAcceptancePipeline.StartedUtc";
    private const string StageStartedKey = "TacticalAcceptancePipeline.StageStarted";
    private const string PlayReadyTicksKey = "TacticalAcceptancePipeline.PlayReadyTicks";
    private const double PipelineTimeoutSeconds = 180.0;

    private enum Stage
    {
        Idle = 0,
        WaitStopped = 10,
        GenerateScene = 20,
        WaitCompileIdle = 30,
        EnterPlayMode = 40,
        WaitPlayModeReady = 50,
        RunGates = 60,
        WaitStoppedAfterGates = 70
    }

    [InitializeOnLoadMethod]
    private static void ResumeIfNeeded()
    {
        if (SessionState.GetBool(RunningKey, false))
        {
            AttachUpdate();
        }
    }

    [MenuItem("AI Tools/Run Tactical Acceptance Pipeline")]
    public static void RunAcceptancePipeline()
    {
        Directory.CreateDirectory("docs");
        SessionState.SetBool(RunningKey, true);
        SessionState.SetString(StartedUtcKey, DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        SessionState.SetString(NotesKey, "");
        SessionState.SetInt(PlayReadyTicksKey, 0);
        SetStage(EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode
            ? Stage.WaitStopped
            : Stage.GenerateScene);

        AppendNote("pipeline started");
        if (EditorApplication.isPlaying)
        {
            AppendNote("requested play mode stop before scene generation");
            EditorApplication.isPlaying = false;
        }

        AttachUpdate();
        WritePipelineReport("running");
        Debug.Log("[AI Tools] Tactical acceptance pipeline started.");
    }

    private static void AttachUpdate()
    {
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    private static void DetachUpdate()
    {
        EditorApplication.update -= Tick;
    }

    private static void Tick()
    {
        if (!SessionState.GetBool(RunningKey, false))
        {
            DetachUpdate();
            return;
        }

        if (ElapsedSeconds() > PipelineTimeoutSeconds)
        {
            AppendNote("pipeline timed out");
            WritePipelineReport("timeout");
            StopPipeline();
            return;
        }

        try
        {
            switch ((Stage)SessionState.GetInt(StageKey, (int)Stage.Idle))
            {
                case Stage.WaitStopped:
                    TickWaitStopped();
                    break;
                case Stage.GenerateScene:
                    TickGenerateScene();
                    break;
                case Stage.WaitCompileIdle:
                    TickWaitCompileIdle();
                    break;
                case Stage.EnterPlayMode:
                    TickEnterPlayMode();
                    break;
                case Stage.WaitPlayModeReady:
                    TickWaitPlayModeReady();
                    break;
                case Stage.RunGates:
                    TickRunGates();
                    break;
                case Stage.WaitStoppedAfterGates:
                    TickWaitStoppedAfterGates();
                    break;
                default:
                    SetStage(Stage.GenerateScene);
                    break;
            }
        }
        catch (Exception ex)
        {
            AppendNote("exception: " + ex.GetType().Name + " " + ex.Message);
            Debug.LogException(ex);
            WritePipelineReport("failed");
            StopPipeline();
        }
    }

    private static void TickWaitStopped()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        AppendNote("editor stopped");
        SetStage(Stage.GenerateScene);
    }

    private static void TickGenerateScene()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        ClearConsole();
        AppendNote("console cleared");
        AssetDatabase.Refresh();
        AppendNote("asset database refresh requested");
        RealifiedImportMaterialGate.ValidateRealifiedImportAndMaterials();
        AppendNote("realified import/material gate ran");
        TacticalPrototypeTools.CreateTacticalPrototype();
        AppendNote("tactical scene generated");
        RealifiedCategoryContactSheet.RenderAll();
        AppendNote("realified category contact sheets rendered");
        HtmlBaselineCategoryContactSheet.RenderAll();
        AppendNote("HTML baseline category contact sheets rendered");
        SetStage(Stage.WaitCompileIdle);
    }

    private static void TickWaitCompileIdle()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        AppendNote("editor compile/update idle");
        SetStage(Stage.EnterPlayMode);
    }

    private static void TickEnterPlayMode()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            SetStage(Stage.WaitPlayModeReady);
            return;
        }

        AppendNote("requested play mode start");
        EditorApplication.isPlaying = true;
        SetStage(Stage.WaitPlayModeReady);
    }

    private static void TickWaitPlayModeReady()
    {
        if (!EditorApplication.isPlaying)
        {
            return;
        }

        var ticks = SessionState.GetInt(PlayReadyTicksKey, 0) + 1;
        SessionState.SetInt(PlayReadyTicksKey, ticks);
        if (ticks < 20)
        {
            return;
        }

        AppendNote("play mode ready");
        SetStage(Stage.RunGates);
    }

    private static void TickRunGates()
    {
        if (!EditorApplication.isPlaying)
        {
            AppendNote("cannot run gates outside play mode");
            WritePipelineReport("failed");
            StopPipeline();
            return;
        }

        TacticalPlayableRouteGate.WriteRouteReport();
        RealifiedAssetPromotionQueue.WritePromotionQueue();
        TacticalPlayerPovGate.CaptureVerifiedScreenshot();
        TacticalPlayerPovGate.WriteReport();
        BuildingIntegrityGate.WriteReport();
        PromotedAssetPlayerCameraVisibilityGate.WriteReport();
        TacticalGameplayProofGate.WriteReport();
        RealifiedAssetGameplayPromotionLedger.WriteLedger();
        HtmlTacticalParityGate.WriteReport();
        WeaponFeelGate.WriteReport();
        AiPlaytestRouteGate.WriteReport();
        M84ThreeClassAssetFactorySpikeGate.WriteReport();
        AppendNote("playable route, player POV, promoted asset visibility, gameplay proof, HTML parity, weapon feel, AI playtest, and M84 asset factory spike gates ran");
        WritePipelineReport("gates_ran");
        SetStage(Stage.WaitStoppedAfterGates);
        EditorApplication.isPlaying = false;
    }

    private static void TickWaitStoppedAfterGates()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        AppendNote("play mode stopped after gates");
        WritePipelineReport("complete");
        StopPipeline();
        Debug.Log("[AI Tools] Tactical acceptance pipeline complete. Report: " + ReportPath);
    }

    private static void StopPipeline()
    {
        SessionState.SetBool(RunningKey, false);
        SessionState.SetInt(StageKey, (int)Stage.Idle);
        SessionState.SetInt(PlayReadyTicksKey, 0);
        DetachUpdate();
    }

    private static void SetStage(Stage stage)
    {
        SessionState.SetInt(StageKey, (int)stage);
        SessionState.SetFloat(StageStartedKey, (float)EditorApplication.timeSinceStartup);
        AppendNote("stage -> " + stage);
    }

    private static void AppendNote(string note)
    {
        var notes = SessionState.GetString(NotesKey, "");
        var line = DateTime.UtcNow.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + " " + note;
        SessionState.SetString(NotesKey, string.IsNullOrEmpty(notes) ? line : notes + "\n" + line);
    }

    private static double ElapsedSeconds()
    {
        var started = SessionState.GetString(StartedUtcKey, "");
        if (!DateTime.TryParse(started, null, DateTimeStyles.RoundtripKind, out var startedAt))
        {
            return 0.0;
        }

        return (DateTime.UtcNow - startedAt).TotalSeconds;
    }

    private static void WritePipelineReport(string status)
    {
        Directory.CreateDirectory("docs");
        var consoleCounts = GetConsoleCounts();
        var playerPov = ReadGate(PlayerPovReportPath);
        var gameplay = ReadGate(GameplayReportPath);
        var route = ReadGate(RouteReportPath);
        var buildingIntegrity = ReadGate(BuildingIntegrityReportPath);
        var weaponFeel = ReadGate(WeaponFeelReportPath);
        var aiPlaytest = ReadGate(AiPlaytestReportPath);
        var m84AssetFactorySpike = ReadGate(M84AssetFactorySpikeReportPath);
        var htmlParity = ReadGate(HtmlParityReportPath);
        var routeJson = ReadText(RouteReportPath);
        var htmlParityJson = ReadText(HtmlParityReportPath);
        var importMaterialGateJson = ReadText(RealifiedImportMaterialGatePath);
        var promotionQueueJson = ReadText(RealifiedPromotionQueuePath);
        var assetGameplayPromotionJson = ReadText(RealifiedAssetGameplayPromotionLedgerPath);
        var promotedAssetVisibilityJson = ReadText(PromotedAssetVisibilityGatePath);
        var auditJson = ReadText(RealifiedAuditPath);
        var urpJson = ReadText(RealifiedUrpMaterialPath);
        var nemotronJson = ReadText(NemotronReviewPath);
        var categorySheetJson = ReadText(RealifiedCategorySheetPath);
        var categoryNemotronJson = ReadText(RealifiedCategoryNemotronPath);
        var sourceTraceJson = ReadText(RealifiedSourceTracePath);
        var htmlBaselineCategorySheetJson = ReadText(HtmlBaselineCategorySheetPath);
        var htmlBaselineCategoryNemotronJson = ReadText(HtmlBaselineCategoryNemotronPath);
        var allGatesPassed = playerPov.Passed && gameplay.Passed && route.Passed && buildingIntegrity.Passed && weaponFeel.Passed && aiPlaytest.Passed && m84AssetFactorySpike.Passed && htmlParity.Passed;
        var visualPolishPassed = ExtractBool(routeJson, "visual_polish_gate_passed");
        var incrementalAssetsPassed = ExtractBool(routeJson, "approved_incremental_asset_gate_passed");
        var fullVisualPassed = ExtractBool(routeJson, "full_visual_asset_gate_passed");
        var semanticReviewPassed = ExtractRootBool(nemotronJson, "promotion_allowed");
        var categorySemanticReviewPassed = ExtractRootBool(categoryNemotronJson, "promotion_allowed");
        var htmlBaselineCategorySemanticReviewPassed = ExtractRootBool(htmlBaselineCategoryNemotronJson, "promotion_allowed");
        var htmlBaselineCategorySheetCount = ExtractInt(htmlBaselineCategorySheetJson, "category_count");
        var semanticReviewCurrent = SemanticReviewIsCurrent(auditJson, nemotronJson);
        var report = new StringBuilder();

        report.AppendLine("{");
        Append(report, "schema", "tactical_acceptance_pipeline_v1", true);
        Append(report, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(report, "status", status, true);
        Append(report, "stage", ((Stage)SessionState.GetInt(StageKey, (int)Stage.Idle)).ToString(), true);
        Append(report, "elapsed_seconds", ElapsedSeconds(), true);
        Append(report, "application_is_playing", EditorApplication.isPlaying, true);
        Append(report, "editor_is_compiling", EditorApplication.isCompiling, true);
        Append(report, "editor_is_updating", EditorApplication.isUpdating, true);
        Append(report, "console_errors", consoleCounts.errors, true);
        Append(report, "console_warnings", consoleCounts.warnings, true);
        Append(report, "console_logs", consoleCounts.logs, true);
        Append(report, "player_pov_gate_passed", playerPov.Passed, true);
        Append(report, "gameplay_proof_gate_passed", gameplay.Passed, true);
        Append(report, "playable_route_gate_passed", route.Passed, true);
        Append(report, "building_integrity_gate_passed", buildingIntegrity.Passed, true);
        Append(report, "weapon_feel_gate_passed", weaponFeel.Passed, true);
        Append(report, "ai_playtest_route_gate_passed", aiPlaytest.Passed, true);
        Append(report, "m84_three_class_asset_factory_spike_passed", m84AssetFactorySpike.Passed, true);
        Append(report, "html_tactical_parity_gate_passed", htmlParity.Passed, true);
        Append(report, "approved_incremental_asset_gate_passed", incrementalAssetsPassed, true);
        Append(report, "visual_polish_gate_passed", visualPolishPassed, true);
        Append(report, "full_visual_asset_gate_passed", fullVisualPassed, true);
        Append(report, "all_required_current_gates_passed", allGatesPassed && incrementalAssetsPassed && visualPolishPassed, true);
        Append(report, "semantic_review_passed", semanticReviewPassed, true);
        Append(report, "semantic_review_current_for_audit", semanticReviewCurrent, true);
        Append(report, "category_contact_sheet_count", ExtractInt(categorySheetJson, "category_count"), true);
        Append(report, "category_semantic_review_passed", categorySemanticReviewPassed, true);
        Append(report, "category_failed_categories", ExtractStringArray(categoryNemotronJson, "failed_categories"), true);
        Append(report, "realified_source_trace_path", RealifiedSourceTracePath, true);
        Append(report, "realified_source_trace_total_glb", ExtractInt(sourceTraceJson, "total_unity_glb"), true);
        Append(report, "realified_source_trace_hash_matches", ExtractInt(sourceTraceJson, "source_hash_matches"), true);
        Append(report, "realified_source_trace_lod0_failures", ExtractInt(sourceTraceJson, "lod0_or_primary_failures"), true);
        Append(report, "realified_source_trace_nonweapon_semantic_failures", ExtractInt(sourceTraceJson, "nonweapon_semantic_failures"), true);
        Append(report, "realified_source_trace_promotable", ExtractInt(sourceTraceJson, "total_unity_glb") > 0
            && ExtractInt(sourceTraceJson, "source_hash_matches") > 0
            && ExtractInt(sourceTraceJson, "lod0_or_primary_failures") == 0
            && ExtractInt(sourceTraceJson, "nonweapon_semantic_failures") == 0, true);
        Append(report, "realified_source_trace_verdict", ExtractString(sourceTraceJson, "verdict"), true);
        Append(report, "html_baseline_category_contact_sheet_count", htmlBaselineCategorySheetCount, true);
        Append(report, "html_baseline_category_semantic_review_passed", htmlBaselineCategorySemanticReviewPassed, true);
        Append(report, "html_baseline_category_failed_categories", ExtractStringArray(htmlBaselineCategoryNemotronJson, "failed_categories"), true);
        Append(report, "html_parity_asset_baseline_present", htmlBaselineCategorySheetCount >= 5, true);
        Append(report, "realified_import_material_gate_path", RealifiedImportMaterialGatePath, true);
        Append(report, "realified_import_presence_passed", ExtractNestedBool(importMaterialGateJson, "summary", "import_presence_passed"), true);
        Append(report, "realified_pbr_sidecar_presence_passed", ExtractNestedBool(importMaterialGateJson, "summary", "pbr_sidecar_presence_passed"), true);
        Append(report, "realified_material_technical_passed", ExtractNestedBool(importMaterialGateJson, "summary", "material_technical_passed"), true);
        Append(report, "realified_technical_ready_classes", ExtractNestedInt(importMaterialGateJson, "summary", "technical_ready_classes"), true);
        Append(report, "realified_import_material_production_promoted", ExtractNestedBool(importMaterialGateJson, "summary", "production_promoted"), true);
        Append(report, "realified_promotion_queue_path", RealifiedPromotionQueuePath, true);
        Append(report, "realified_promotion_technically_ready_classes", ExtractNestedInt(promotionQueueJson, "summary", "technically_ready_classes"), true);
        Append(report, "realified_promotion_semantic_passed_classes", ExtractNestedInt(promotionQueueJson, "summary", "semantic_passed_classes"), true);
        Append(report, "realified_promotion_gameplay_scene_evidence_classes", ExtractNestedInt(promotionQueueJson, "summary", "gameplay_scene_evidence_classes"), true);
        Append(report, "realified_promotion_production_promoted_classes", ExtractNestedInt(promotionQueueJson, "summary", "production_promoted_classes"), true);
        Append(report, "realified_promotion_any_production_promoted", ExtractNestedBool(promotionQueueJson, "summary", "any_production_promoted"), true);
        Append(report, "realified_promotion_all_classes_promoted", ExtractNestedBool(promotionQueueJson, "summary", "all_classes_promoted"), true);
        Append(report, "realified_promotion_global_batch_allowed", ExtractNestedBool(promotionQueueJson, "summary", "global_batch_promotion_allowed"), true);
        Append(report, "realified_asset_gameplay_promotion_ledger_path", RealifiedAssetGameplayPromotionLedgerPath, true);
        Append(report, "realified_asset_gameplay_partial_assets", ExtractNestedInt(assetGameplayPromotionJson, "summary", "technical_semantic_scene_partial_assets"), true);
        Append(report, "realified_asset_gameplay_production_promoted_assets", ExtractNestedInt(assetGameplayPromotionJson, "summary", "production_promoted_assets"), true);
        Append(report, "realified_asset_gameplay_any_production_promoted", ExtractNestedBool(assetGameplayPromotionJson, "summary", "any_production_promoted"), true);
        Append(report, "promoted_asset_player_camera_visibility_path", PromotedAssetVisibilityGatePath, true);
        Append(report, "promoted_asset_visibility_gate_passed", ExtractBool(promotedAssetVisibilityJson, "passed"), true);
        Append(report, "promoted_asset_visibility_production_promoted_classes", ExtractNestedInt(promotedAssetVisibilityJson, "summary", "production_promoted_classes"), true);
        Append(report, "promoted_asset_visibility_visible_promoted_classes", ExtractNestedInt(promotedAssetVisibilityJson, "summary", "visible_promoted_classes"), true);
        Append(report, "promoted_asset_visibility_visible_promoted_objects", ExtractNestedInt(promotedAssetVisibilityJson, "summary", "visible_promoted_objects"), true);
        Append(report, "promoted_asset_visibility_candidate_visible_objects", ExtractNestedInt(promotedAssetVisibilityJson, "summary", "candidate_visible_objects"), true);
        Append(report, "promoted_asset_visibility_blocked_reason", ExtractNestedString(promotedAssetVisibilityJson, "summary", "blocked_reason"), true);
        Append(report, "realified_audit_total_glb", ExtractInt(auditJson, "total_glb"), true);
        Append(report, "realified_audit_texture_sidecars", ExtractInt(auditJson, "external_texture_sidecars"), true);
        Append(report, "realified_audit_sidecar_promotable_count", ExtractInt(auditJson, "sidecar_promotable_count"), true);
        Append(report, "realified_urp_material_count", ExtractInt(urpJson, "material_count"), true);
        Append(report, "realified_urp_sidecar_pbr_valid_materials", ExtractNestedInt(urpJson, "summary", "sidecar_pbr_valid_materials"), true);
        Append(report, "nemotron_review_verdict", ExtractString(nemotronJson, "overall_verdict"), true);
        Append(report, "nemotron_review_summary", ExtractString(nemotronJson, "summary"), true);
        Append(report, "completion_credit", ExtractString(routeJson, "completion_credit"), true);
        Append(report, "html_tactical_parity_note", ExtractString(htmlParityJson, "note"), true);
        Append(report, "visual_completion_blocker", ExtractString(routeJson, "visual_completion_blocker"), true);
        Append(report, "player_pov_gate_path", PlayerPovReportPath, true);
        Append(report, "gameplay_proof_gate_path", GameplayReportPath, true);
        Append(report, "playable_route_gate_path", RouteReportPath, true);
        Append(report, "building_integrity_gate_path", BuildingIntegrityReportPath, true);
        Append(report, "weapon_feel_gate_path", WeaponFeelReportPath, true);
        Append(report, "ai_playtest_route_gate_path", AiPlaytestReportPath, true);
        Append(report, "m84_three_class_asset_factory_spike_path", M84AssetFactorySpikeReportPath, true);
        Append(report, "html_tactical_parity_gate_path", HtmlParityReportPath, true);
        Append(report, "promoted_asset_visibility_gate_path", PromotedAssetVisibilityGatePath, true);
        Append(report, "realified_audit_path", RealifiedAuditPath, true);
        Append(report, "realified_urp_material_validation_path", RealifiedUrpMaterialPath, true);
        Append(report, "nemotron_review_path", NemotronReviewPath, true);
        Append(report, "realified_category_contact_sheet_path", RealifiedCategorySheetPath, true);
        Append(report, "realified_category_nemotron_reviews_path", RealifiedCategoryNemotronPath, true);
        Append(report, "html_baseline_category_contact_sheet_path", HtmlBaselineCategorySheetPath, true);
        Append(report, "html_baseline_category_nemotron_reviews_path", HtmlBaselineCategoryNemotronPath, true);
        Append(report, "player_pov_screenshot_path", PlayerPovScreenshotPath, true);
        Append(report, "playable_route_screenshot_directory", PlayableRouteScreenshotDirectory, true);
        Append(report, "first_person_fire_screenshot_path", FirstPersonFireScreenshotPath, true);
        Append(report, "realified_contact_sheet_path", RealifiedContactSheetPath, true);
        Append(report, "notes", SessionState.GetString(NotesKey, ""), false);
        report.AppendLine("}");

        File.WriteAllText(ReportPath, report.ToString());
        AssetDatabase.Refresh();
    }

    private static GateResult ReadGate(string path)
    {
        if (!File.Exists(path))
        {
            return new GateResult(false, false);
        }

        var json = File.ReadAllText(path);
        return new GateResult(true, ExtractBool(json, "passed"));
    }

    private static bool ExtractBool(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        return Regex.IsMatch(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*true");
    }

    private static bool ExtractRootBool(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        var match = Regex.Match(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*(?<value>true|false)");
        return match.Success && match.Groups["value"].Value == "true";
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

    private static int ExtractNestedInt(string json, string objectKey, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return 0;
        }

        var objectMatch = Regex.Match(json, "\\\"" + Regex.Escape(objectKey) + "\\\"\\s*:\\s*\\{(?<body>.*?)\\n\\s*\\}", RegexOptions.Singleline);
        return objectMatch.Success ? ExtractInt(objectMatch.Groups["body"].Value, key) : 0;
    }

    private static bool ExtractNestedBool(string json, string objectKey, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        var objectMatch = Regex.Match(json, "\\\"" + Regex.Escape(objectKey) + "\\\"\\s*:\\s*\\{(?<body>.*?)\\n\\s*\\}", RegexOptions.Singleline);
        return objectMatch.Success && ExtractBool(objectMatch.Groups["body"].Value, key);
    }

    private static string ExtractString(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return "";
        }

        var match = Regex.Match(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*\\\"(?<value>(?:\\\\.|[^\\\"])*)\\\"");
        return match.Success ? Regex.Unescape(match.Groups["value"].Value) : "";
    }

    private static string ExtractNestedString(string json, string objectKey, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return "";
        }

        var objectMatch = Regex.Match(json, "\\\"" + Regex.Escape(objectKey) + "\\\"\\s*:\\s*\\{(?<body>.*?)\\n\\s*\\}", RegexOptions.Singleline);
        return objectMatch.Success ? ExtractString(objectMatch.Groups["body"].Value, key) : "";
    }

    private static string ExtractStringArray(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return "";
        }

        var match = Regex.Match(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*\\[(?<body>.*?)\\]", RegexOptions.Singleline);
        if (!match.Success)
        {
            return "";
        }

        var values = Regex.Matches(match.Groups["body"].Value, "\\\"(?<value>(?:\\\\.|[^\\\"])*)\\\"");
        var builder = new StringBuilder();
        foreach (Match value in values)
        {
            if (builder.Length > 0)
            {
                builder.Append(", ");
            }

            builder.Append(Regex.Unescape(value.Groups["value"].Value));
        }

        return builder.ToString();
    }

    private static bool SemanticReviewIsCurrent(string auditJson, string nemotronJson)
    {
        var auditTimestamp = ExtractString(auditJson, "timestamp_utc");
        var reviewTimestamp = ExtractString(nemotronJson, "review_timestamp_utc");
        if (!DateTime.TryParse(auditTimestamp, null, DateTimeStyles.RoundtripKind, out var auditTime)
            || !DateTime.TryParse(reviewTimestamp, null, DateTimeStyles.RoundtripKind, out var reviewTime))
        {
            return false;
        }

        return reviewTime.ToUniversalTime() >= auditTime.ToUniversalTime();
    }

    private static string ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    private static (int errors, int warnings, int logs) GetConsoleCounts()
    {
        var logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
        var method = logEntriesType?.GetMethod("GetCountsByType", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null)
        {
            return (0, 0, 0);
        }

        var args = new object[] { 0, 0, 0 };
        method.Invoke(null, args);
        return ((int)args[0], (int)args[1], (int)args[2]);
    }

    private static void ClearConsole()
    {
        var logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
        var method = logEntriesType?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        method?.Invoke(null, null);
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

    private static void Append(StringBuilder json, string key, double value, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": ").Append(value.ToString("0.###", CultureInfo.InvariantCulture));
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

    private readonly struct GateResult
    {
        public GateResult(bool exists, bool passed)
        {
            Exists = exists;
            Passed = exists && passed;
        }

        public readonly bool Exists;
        public readonly bool Passed;
    }
}
#endif
