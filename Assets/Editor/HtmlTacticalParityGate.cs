#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class HtmlTacticalParityGate
{
    private const string ReportPath = "docs/HTML_TACTICAL_PARITY_GATE.json";
    private const string MarkdownPath = "docs/HTML_TACTICAL_PARITY_GATE.md";
    private const string PlayerPovPath = "docs/TACTICAL_PLAYER_POV_GATE.json";
    private const string GameplayPath = "docs/TACTICAL_GAMEPLAY_PROOF_GATE.json";
    private const string RoutePath = "docs/TACTICAL_PLAYABLE_ROUTE_GATE.json";

    [MenuItem("AI Tools/Write HTML Tactical Parity Gate")]
    public static void WriteReport()
    {
        Directory.CreateDirectory("docs");
        var playerPov = ReadText(PlayerPovPath);
        var gameplay = ReadText(GameplayPath);
        var route = ReadText(RoutePath);

        var lobbyDeathRestart = Bool(route, "lobby_visible_before_start")
            && Bool(route, "death_overlay_mutation")
            && Bool(route, "restart_mutation")
            && Bool(gameplay, "death_overlay_and_restart");
        var settingsAndSkins = Bool(playerPov, "settings_panel_present")
            && Bool(playerPov, "skins_panel_present")
            && Bool(gameplay, "settings_controls_work")
            && Bool(gameplay, "skin_roll_works")
            && Bool(gameplay, "skin_visuals_apply_to_weapons");
        var cameraAdsStanceJump = Bool(gameplay, "camera_ads_stance_jump_mutation")
            && Bool(gameplay, "first_person_weapon_visual_switches")
            && Bool(playerPov, "player_character_visible")
            && Bool(playerPov, "first_person_weapon_hidden_in_third_person");
        var weaponHitFeedback = Bool(gameplay, "fire_ammo_and_enemy_hit")
            && Bool(gameplay, "weapon_feedback_spawned")
            && Bool(gameplay, "reload_state_mutation")
            && Bool(gameplay, "procedural_audio_feedback")
            && Bool(gameplay, "first_person_weapon_polish")
            && Bool(route, "fire_state_mutation");
        var npcCombatAndSpawn = Bool(gameplay, "player_damage_state_mutation")
            && Bool(gameplay, "enemy_ranged_attack_state_mutation")
            && Bool(gameplay, "dynamic_spawn_state_mutation")
            && Bool(route, "enemy_ranged_attack_mutation")
            && Bool(route, "dynamic_spawn_mutation");
        var floorLadder = Bool(gameplay, "ladder_floor_and_camera_mutation")
            && Bool(route, "ladder_floor_mutation")
            && Int(playerPov, "ladder_count") >= 4;
        var buildingEnvironment = Bool(gameplay, "environment_player_flow_verified")
            && Bool(route, "building_visited")
            && Bool(route, "warehouse_visited")
            && Bool(route, "container_visited")
            && NestedInt(route, "asset_quality", "wet_asphalt_renderer_count") >= 1
            && NestedInt(route, "asset_quality", "concrete_renderer_count") >= 1
            && Int(playerPov, "spawn_detail_objects") >= 12;
        var glbImportSceneReplacement = Bool(route, "approved_incremental_asset_gate_passed")
            && NestedInt(route, "asset_quality", "category_failed_scene_instances") == 0
            && Int(playerPov, "active_glb_instances") >= 80
            && Int(playerPov, "active_glb_renderers") >= 1000
            && NestedInt(route, "asset_quality", "approved_crate_instances") >= 1
            && NestedInt(route, "asset_quality", "approved_container_instances") >= 1
            && NestedInt(route, "asset_quality", "approved_player_instances") >= 1
            && NestedInt(route, "asset_quality", "approved_enemy_instances") >= 1
            && NestedInt(route, "asset_quality", "approved_weapon_pbr_renderer_instances") >= 4;
        var evidenceValidation = Bool(playerPov, "passed")
            && Bool(gameplay, "passed")
            && Bool(route, "passed")
            && Int(route, "screenshot_count") >= 12
            && File.Exists("Assets/Screenshots/PlayableRoute/08_fire_hit_first_person.png")
            && File.Exists("Assets/Screenshots/tactical_html_replica_current_player_pov_verified.png");

        var passed = lobbyDeathRestart
            && settingsAndSkins
            && cameraAdsStanceJump
            && weaponHitFeedback
            && npcCombatAndSpawn
            && floorLadder
            && buildingEnvironment
            && glbImportSceneReplacement
            && evidenceValidation;

        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "html_tactical_parity_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "passed", passed, true);
        Append(json, "lobby_death_restart_parity", lobbyDeathRestart, true);
        Append(json, "settings_and_skin_parity", settingsAndSkins, true);
        Append(json, "first_third_ads_stance_jump_parity", cameraAdsStanceJump, true);
        Append(json, "weapon_hit_feedback_parity", weaponHitFeedback, true);
        Append(json, "npc_remote_combat_dynamic_spawn_parity", npcCombatAndSpawn, true);
        Append(json, "floor_ladder_parity", floorLadder, true);
        Append(json, "building_environment_detail_parity", buildingEnvironment, true);
        Append(json, "glb_import_scene_replacement_parity", glbImportSceneReplacement, true);
        Append(json, "evidence_validation_parity", evidenceValidation, true);
        Append(json, "active_glb_instances", Int(playerPov, "active_glb_instances"), true);
        Append(json, "active_glb_renderers", Int(playerPov, "active_glb_renderers"), true);
        Append(json, "route_screenshot_count", Int(route, "screenshot_count"), true);
        Append(json, "category_failed_scene_instances", NestedInt(route, "asset_quality", "category_failed_scene_instances"), true);
        Append(json, "full_visual_asset_gate_passed", Bool(route, "full_visual_asset_gate_passed"), true);
        Append(json, "completion_credit", StringField(route, "completion_credit"), true);
        Append(json, "note", Bool(route, "full_visual_asset_gate_passed")
            ? "HTML tactical parity gate passes and full visual gate is already true."
            : "HTML tactical parity gate can pass while full visual PUBG-like realism still remains open.", false);
        json.AppendLine("}");
        File.WriteAllText(ReportPath, json.ToString());

        File.WriteAllText(MarkdownPath, BuildMarkdown(passed, lobbyDeathRestart, settingsAndSkins, cameraAdsStanceJump, weaponHitFeedback, npcCombatAndSpawn, floorLadder, buildingEnvironment, glbImportSceneReplacement, evidenceValidation));
        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] HTML tactical parity gate report written to " + ReportPath + " passed=" + passed);
    }

    private static string BuildMarkdown(bool passed, params bool[] checks)
    {
        var labels = new[]
        {
            "Lobby/death/restart",
            "Settings and skins",
            "First/third person, ADS, crouch/prone/jump",
            "Weapon, reload, hit, audio, and first-person feedback",
            "NPC ranged combat and dynamic spawn",
            "Floors and ladders",
            "Buildings, warehouse, containers, ground materials, and spawn detail",
            "GLB import and scene replacement",
            "Evidence reports and screenshots"
        };
        var md = new StringBuilder();
        md.AppendLine("# HTML Tactical Parity Gate");
        md.AppendLine();
        md.AppendLine("Passed: `" + passed + "`");
        md.AppendLine();
        for (var i = 0; i < checks.Length && i < labels.Length; i++)
        {
            md.Append("- ").Append(labels[i]).Append(": `").Append(checks[i]).AppendLine("`");
        }
        md.AppendLine();
        md.AppendLine("This gate checks HTML-game feature parity. It does not claim final PUBG-like realism.");
        return md.ToString();
    }

    private static string ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    private static bool Bool(string json, string key)
    {
        return !string.IsNullOrEmpty(json) && Regex.IsMatch(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*true");
    }

    private static int Int(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return 0;
        }

        var match = Regex.Match(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*(-?\\d+)");
        return match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }

    private static int NestedInt(string json, string objectKey, string valueKey)
    {
        if (string.IsNullOrEmpty(json))
        {
            return 0;
        }

        var objectMatch = Regex.Match(json, "\\\"" + Regex.Escape(objectKey) + "\\\"\\s*:\\s*\\{(?<body>.*?)\\n\\s*\\}", RegexOptions.Singleline);
        return objectMatch.Success ? Int(objectMatch.Groups["body"].Value, valueKey) : 0;
    }

    private static string StringField(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return "";
        }

        var match = Regex.Match(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*\\\"(?<value>(?:\\\\.|[^\\\"])*)\\\"");
        return match.Success ? Regex.Unescape(match.Groups["value"].Value) : "";
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

    private static string Escape(string value)
    {
        return (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
    }
}
#endif
