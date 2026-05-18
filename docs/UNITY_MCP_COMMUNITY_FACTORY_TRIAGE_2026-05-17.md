# Unity MCP / AI Game Factory Community Triage

Date: 2026-05-17

## Controller Verdict

The current inefficiency is not caused by one bad tool. It comes from mixing three different loops as if they were one loop:

1. **Unity control loop**: editor refresh, console, scene generation, Play Mode, route gates.
2. **Asset factory loop**: reference image, Hunyuan/TRELLIS/TextureAlchemy/Blender, GLB export, Unity import.
3. **Semantic quality loop**: screenshots/contact sheets, VLM/Nemotron review, category correctness, player-camera proof.

Community and local evidence both point to the same pattern: MCP is useful when it runs short, explicit editor operations. MCP becomes inefficient when we let it carry long-lived creative judgment, asset quality decisions, or many fragile Unity state transitions.

## Current Local State

### Unity MCP

- Official Unity MCP is still not the working path because Unity's own support article says the `Status: Capacity Limit` / `plan doesn't include MCP connections` error means the Unity AI or license seat is not assigned, and the Editor must be restarted after assignment.
- The working route is **CoplayDev MCP for Unity** at `http://127.0.0.1:8080/mcp`, server version observed locally as `mcp-for-unity-server 3.3.1`.
- CoplayDev exposes the tools we actually need: `batch_execute`, `execute_menu_item`, `execute_code`, `read_console`, `refresh_unity`, `manage_editor`, `manage_scene`, `manage_gameobject`, `manage_asset`, `manage_material`, `run_tests`, `unity_reflect`, and `unity_docs`.
- CoderGamester MCP Unity remains a backup probe, not the active route. Its README describes a Unity WebSocket bridge plus Node MCP server, Codex CLI config, and default port 8090. Do not install it into the active project unless CoplayDev becomes persistently unreliable.

### Unity route gate after current patch

Latest rerun through Coplay MCP:

- `passed`: true
- `gameplay_route_passed`: true
- `approved_incremental_asset_gate_passed`: true
- `visual_polish_gate_passed`: true
- `full_visual_asset_gate_passed`: false
- `completion_credit`: `gameplay_route_plus_incremental_player_enemy_container_crate_medical_ammo_helmet_vest_environment_postprocess_weather_only`
- `approved_crate_instances`: 11
- `approved_crate_renderer_instances`: 11
- `approved_container_instances`: 11
- `approved_container_renderer_instances`: 11
- `approved_player_instances`: 1
- `approved_player_renderer_instances`: 1
- `approved_enemy_instances`: 11
- `approved_enemy_renderer_instances`: 11
- `approved_medical_loot_instances`: 4
- `approved_medical_loot_renderer_instances`: 4
- `approved_ammo_loot_instances`: 4
- `approved_ammo_loot_renderer_instances`: 4
- `approved_helmet_loot_instances`: 4
- `approved_helmet_loot_renderer_instances`: 4
- `approved_vest_loot_instances`: 4
- `approved_vest_loot_renderer_instances`: 4
- `wet_asphalt_renderer_count`: 2
- `concrete_renderer_count`: 71
- `tactical_postprocess_volume_count`: 1
- `tactical_rain_field_count`: 1
- `tactical_industrial_light_count`: 3
- `tactical_postprocess_camera_count`: 1
- `known_category_failed_batch_files`: 12
- `category_failed_scene_instances`: 0

Meaning: the Unity gameplay skeleton and approved player/enemy/container/crate/medical/ammo/helmet/vest/environment/postprocess/weather increments are integrated. The PUBG-like visual goal is still not achieved; the current named blockers are authored humanoid animation and first-person weapon hero-asset polish.

### Kimi asset factory handoff

Kimi's `/Users/yuanshaochen/Projects/local-coding-runners/HANDOFF.md` is valuable, but it proves infrastructure, not final art quality:

- `quick-verify.sh` passes for Nemotron on 8083, UI-TARS on 8082, and MCP self-test.
- The generated 48 Unity GLBs are present under `Assets/HtmlTacticalAssets/RealifiedAssets/`.
- The 12 textured classes are not production-ready because local Nemotron review and our contact-sheet inspection found category mismatch. Most non-weapon classes look like weapon silhouettes.
- The batch should stay quarantined as `pipeline_ready_category_failed_batch` until class-by-class semantic review passes.

## What The Community Signals Change

### 1. Stop trying to make MCP the whole brain

MCP should operate Unity, not decide whether an enemy looks like an enemy. The better split is:

```text
Agent plans / code edits
  -> deterministic Editor menu or execute_code
  -> Unity Console + route gate JSON
  -> screenshot/contact sheet
  -> Nemotron semantic review
  -> asset promotion or quarantine
```

### 2. Use Editor menu tools as the stable high-level API

Unity's `MenuItem` API is exactly the right abstraction for this project: a static method becomes a menu command. We should keep building project-specific commands like:

- `AI Tools/Create Tactical Prototype From HTML`
- `AI Tools/Write Tactical Playable Route Gate`
- `AI Tools/Render GLB Contact Sheet`
- `AI Tools/Validate Approved Assets`
- `AI Tools/Promote Approved Asset To Gameplay`

Then MCP only needs to execute menu items or call the same static methods through `execute_code`.

### 3. Treat Unity reload/compile as a first-class gate

Unity's `AssetDatabase.Refresh` imports changed assets synchronously, but script compilation is asynchronous. That matches the local failure mode where MCP calls work while menu items or assemblies are stale. Before Play Mode gates:

- stop play mode,
- clear console,
- refresh only when needed,
- poll `EditorApplication.isCompiling` and `isUpdating`,
- only then run scene generation and route gate,
- if MCP command disconnects, retry the same static method through `execute_code`.

### 4. Split assemblies before scaling more code

Unity's assembly definition docs say assemblies can reduce unnecessary recompilation and improve architecture control. The next efficiency fix is to isolate:

- tactical runtime scripts,
- tactical editor tools/gates,
- imported package/adapters,
- generated asset validators.

This should reduce the dirty compile/reload pain that made previous work feel like a swamp.

## New Working Rule

A generated asset is not allowed into gameplay just because it exists in Unity.

Promotion requires:

1. import success with clean Console,
2. contact sheet render,
3. semantic category pass by Nemotron or stronger VLM,
4. Unity material slot report,
5. real gameplay entity binding,
6. player-camera route screenshot,
7. route gate JSON referencing renderer count and event participation where relevant.

## Immediate Next Missions

### M20: Unity MCP Efficiency Hardening

Owner: Codex/Kimi

Goal: add one deterministic Unity-side menu command that performs the full acceptance sequence without many MCP round trips.

Output:

- `AI Tools/Run Tactical Acceptance Pipeline`
- writes one JSON report with compile state, console errors, scene generation, route gate, and asset-quality counts.

Acceptance:

- one MCP `execute_menu_item` or `execute_code` call can run the full gate after Editor is stable.

Status: implemented and verified.

- Tool: `AI Tools/Run Tactical Acceptance Pipeline`
- Script: `Assets/Editor/TacticalAcceptancePipeline.cs`
- Report: `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`
- Latest result: `status=complete`, `console_errors=0`, `player_pov_gate_passed=true`, `gameplay_proof_gate_passed=true`, `playable_route_gate_passed=true`, `all_required_current_gates_passed=true`, `full_visual_asset_gate_passed=false`.

### M21: Remaining Category-Correct Asset Regeneration

Owner: Kimi/HomePC + Nemotron review

Status: first pass complete for `ammo`, `helmet`, and `vest`.

Goal: keep the same one-category-at-a-time promotion gate for any future asset class, rather than importing whole generated batches.

Acceptance:

- contact-sheet semantic pass before Unity gameplay import.

### M22: Contact Sheet First Pipeline

Owner: Codex + Unity MCP

Goal: before any asset is copied into gameplay paths, render it in a neutral Unity contact sheet and call Nemotron review.

Acceptance:

- generated `asset_semantic_review.json` controls whether the asset can be promoted.

### M23: Assembly Definition Split

Owner: Codex/Kimi

Goal: reduce Unity reload cost and stale-menu risk.

Acceptance:

- `Assets/Scripts/TacticalRuntime.asmdef`
- `Assets/Editor/TacticalEditor.asmdef`
- no compile errors,
- route gate still passes.

### M25: Humanoid Animation + First-Person Weapon Polish

Owner: Kimi/Codex

Goal: address the current `full_visual_asset_gate_passed=false` blocker directly.

Acceptance:

- gameplay-bound player/enemy animation evidence for at least idle/walk/fire/death or accepted placeholder equivalents;
- first-person weapon has visible near-camera polish, recoil/sway/reload feedback, and route-gated screenshots;
- route gate adds explicit `approved_humanoid_animation` and `approved_fp_weapon_polish` fields.

## Sources Checked

- Unity Support: official MCP capacity-limit / seat assignment cause and fix. https://support.unity.com/hc/en-us/articles/48958235901460-Unity-AI-MCP-Connection-Fails-Unity-AI-Gateway-connection-Error
- CoplayDev Unity MCP GitHub: active tool surface and batch execution support. https://github.com/CoplayDev/unity-mcp
- CoderGamester MCP Unity GitHub: backup Unity MCP architecture and Codex CLI config. https://github.com/CoderGamester/mcp-unity
- Unity Manual: command-line interface. https://docs.unity3d.com/Manual/CommandLineArguments.html
- Unity Manual: organizing scripts into assemblies. https://docs.unity3d.com/Manual/assembly-definition-files.html
- Unity Scripting API: `AssetDatabase.Refresh`. https://docs.unity3d.com/ScriptReference/AssetDatabase.Refresh.html
- Unity Scripting API: `MenuItem`. https://docs.unity3d.com/ScriptReference/MenuItem.html
