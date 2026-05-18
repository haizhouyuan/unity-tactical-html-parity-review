# Unity MCP And Asset Factory Reset

Date: 2026-05-17

## Verdict

We should stop treating the current problem as "one Unity bug" or "one asset import bug". The real issue is workflow architecture.

The efficient route is:

1. Keep **CoplayDev MCP for Unity** as the active Unity control channel.
2. Keep **Editor menu tools** as the deterministic high-level action layer.
3. Use **batchmode / tests** only when Unity Editor is closed or in a copy/worktree.
4. Use **Nemotron Observe** as visual/semantic reviewer, and **UI-TARS** only for bounded GUI action.
5. Treat the Kimi 12-asset generation batch as **pipeline-ready input**, not production-ready game content.
6. Promote assets only when they pass category correctness, material correctness, gameplay binding, and player-camera evidence.

## Current Local Facts

### Unity Project

- Project path: `/Users/yuanshaochen/My project`
- Unity package manifest includes:
  - `com.coplaydev.unity-mcp`
  - `com.unity.ai.assistant`
  - `com.unity.ai.inference`
  - `com.unity.ai.navigation`
  - `com.unity.render-pipelines.universal`
  - `com.unity.test-framework`
- Active community MCP endpoint: `http://127.0.0.1:8080/mcp`
- Official Unity MCP is not the active route because it was blocked by Unity plan/seat entitlement.

### Playable Route Gate

The current route gate proves a useful HTML-style gameplay skeleton:

- lobby and start
- spawn
- player movement
- building route
- warehouse route
- container route
- pickup mutation
- fire mutation
- enemy ranged attack mutation
- dynamic spawn mutation
- ladder/floor mutation
- heal mutation
- death overlay
- restart
- player-camera screenshots

Current status:

- `gameplay_route_passed`: true
- `approved_incremental_asset_gate_passed`: true
- `full_visual_asset_gate_passed`: false
- `completion_credit`: `gameplay_route_plus_incremental_crate_environment_only`

Meaning: gameplay parity is moving, but visual/asset parity is not complete.

### Local Visual-Agent Stack

Kimi's handoff under `/Users/yuanshaochen/Projects/local-coding-runners/HANDOFF.md` reports:

- Nemotron Observe via llama.cpp on port 8083 is the semantic "eyes" layer.
- UI-TARS via MLX VLM on port 8082 is the GUI "hands" layer.
- `quick-verify.sh` currently passes for both services and MCP self-test.

This is valuable, but it should be used for review/gating and bounded GUI actions, not for replacing deterministic Unity build/test gates.

## Community/Documentation Signals

### CoplayDev MCP For Unity

Source: https://github.com/CoplayDev/unity-mcp

Relevant points:

- Designed to bridge AI assistants with Unity Editor through MCP.
- Installs through Unity Package Manager using:
  `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`
- Starts an HTTP server on `localhost:8080`.
- Exposes tools such as:
  - `read_console`
  - `refresh_unity`
  - `execute_menu_item`
  - `manage_scene`
  - `manage_gameobject`
  - `manage_material`
  - `manage_asset`
  - `manage_script`
  - `manage_camera`
  - `manage_graphics`
  - `manage_animation`
  - `manage_profiler`
  - `run_tests`
  - `batch_execute`
- Its README explicitly recommends `batch_execute` for multi-step operations because it reduces round trips.

Local decision:

- Adopt as primary Unity MCP route.
- Use it for short, explicit operations and evidence capture.
- Do not let long live MCP sessions mutate many systems while Unity is compiling/reloading.

### CoderGamester MCP Unity

Source: https://github.com/CoderGamester/mcp-unity

Relevant points:

- Unity-side WebSocket bridge plus Node MCP stdio server.
- Lists support for clients including Claude Code, Codex CLI, GitHub Copilot, Cursor, Windsurf, and others.
- Exposes scene, GameObject, component, prefab, material, console, package, and test operations.
- Project paths with spaces are supported, though moving to a no-space path can still be useful when troubleshooting.
- Its agent guide warns about:
  - tool/resource name mismatches,
  - long main-thread work blocking Unity,
  - domain reloads stopping/restarting the server,
  - schema compatibility across clients.

Local decision:

- Probe in a copy only if CoplayDev MCP becomes unreliable.
- Do not install a second Unity MCP bridge into the active tactical project today.

### glTFast / GLB Import

Source: https://github.com/atteneder/glTFast

Relevant points:

- glTFast supports glTF 2.0 and works with URP/HDRP/Built-In pipelines.
- Editor import is a first-class workflow: copying `.glb` or `.gltf` files into `Assets` imports them as Unity assets.
- Build output can miss glTF shader variants if shader graphs/variants are not included.

Local decision:

- Unity GLB import success is necessary but not sufficient.
- We need a material report and in-game screenshot after import.
- Build readiness later must include shader variant / glTF material checks.

### Unity Batchmode / Test Workflow

Sources:

- https://docs.unity.com/en-us/build-automation/check-build-results/troubleshoot-build-failures/overview
- Unity Test Framework docs via package docs/context.

Relevant points:

- Unity can execute static Editor methods in batchmode.
- Unity Test Framework can run EditMode/PlayMode tests via command line.
- Same project should not be driven by live Editor and batchmode at the same time.

Local decision:

- Use live MCP for scene/console evidence while Editor is open.
- Use batchmode only when Editor is closed or on a separate copy.

## Correct Layering

```text
Codex / Kimi / Gemini
  |
  | mission contracts and code edits
  v
Editor menu tools / validation scripts
  |
  | executed through Coplay MCP or manually
  v
Unity Editor
  |
  | screenshots, console, route reports
  v
Nemotron Observe review + deterministic JSON gates
  |
  | only if visual and gameplay evidence agree
  v
asset promotion
```

## What The Kimi Asset Handoff Proves

The handoff says:

- 12 textured GLBs were generated.
- 36 LOD GLBs were generated.
- 48 GLBs were copied into Unity.
- A batch generation script exists.

That proves:

- The generation pipeline can produce and move GLB files.
- Unity can import a large batch of generated GLBs.
- The factory can be automated enough to be useful.

It does **not** prove:

- category correctness,
- production-grade silhouettes,
- correct player/enemy/loot identity,
- normal-map completeness,
- Unity URP material correctness in gameplay,
- gameplay binding,
- player-camera visibility,
- or PUBG-like visual quality.

Local evidence already found the important failure:

- The 12 final `*_textured.glb` files render mostly as weapon-like silhouettes even when named as player, enemy, container, crate, medkit, ammo, helmet, or vest.

Therefore the batch is not rejected as useless, but it is demoted to:

```text
pipeline_ready_category_failed_batch
```

Only the approved crate and approved environment materials currently count as gameplay-bound incremental assets.

## New Workflow Rules

### 1. Do Not Bind Whole Batches Blindly

Every generated asset must pass a class-specific visual gate before entering a scene:

- weapon should look like a weapon,
- crate should look like a crate,
- container should look like a container,
- medkit should look like a medkit,
- player/enemy should look like riggable tactical characters.

### 2. Use Nemotron Observe Before Unity Binding

For each contact sheet or Blender preview:

1. Create preview/contact sheet.
2. Ask Nemotron Observe to classify:
   - category match,
   - silhouette completeness,
   - major defects,
   - texture plausibility,
   - whether this is game-ready, probe-only, or reject.
3. Save the decision as JSON.
4. Only import/bind PASS or PARTIAL assets.

### 3. Use MCP For Evidence, Not Hope

Each Unity MCP pass should do one bounded thing:

- refresh assets,
- run one menu tool,
- read console,
- run one route gate,
- save one report,
- stop play mode.

Avoid long, fragile chains while `EditorApplication.isCompiling` or `EditorApplication.isUpdating` is true.

### 4. Keep Editor Tools As Stable Buttons

The most reliable Unity control surface remains:

- `AI Tools/Create Tactical Prototype From HTML`
- `AI Tools/Write Tactical Playable Route Gate`
- asset import/report tools
- future `AI Tools/Classify Realified Asset Batch`
- future `AI Tools/Promote Approved Asset To Gameplay`

These let human, Codex, Kimi, or MCP call the same deterministic operation.

## Immediate Missions

### M16: Community Practice Deep Update

Goal:

- Refresh the community research beyond the older M2 matrix.
- Focus specifically on Unity MCP, AI-assisted Unity production, MCP stability, Editor scripting patterns, glTF/glTFast, and agent-driven Unity workflows.

Output:

- `tasks/mission_outputs/M16_community_unity_mcp_practice_update/final_matrix.md`
- practice cards with `adopt/probe/defer/reject`
- recommended Unity workflow changes

Owner:

- Gemini 3.1 Pro Preview for broad research.
- Codex synthesizes and updates the route.

### M17: Nemotron Asset Quality Gate

Goal:

- Integrate local Nemotron Observe into the asset-factory gate.
- Classify the current 12 generated final GLBs and future batches before Unity gameplay binding.

Output:

- asset contact sheet review JSON
- category PASS/PARTIAL/FAIL decisions
- regenerated asset queue

Owner:

- Kimi with `observe_screen` / `analyze_screenshot`.
- Codex controls file/report integration.

Status:

- First contact-sheet review completed by local Nemotron on 2026-05-17.
- Report: `/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M17_nemotron_asset_quality_gate/asset_quality_review.json`
- Verdict: current 12 textured GLB batch is not production-ready and receives zero completion credit.
- Rejected classes: tactical player, enemy, helmet, vest, ammo, medkit, container, crate.

### M18: Category-Correct Asset Replacement

Goal:

- Replace one failed category in the Unity playable scene with a category-correct asset.

Priority:

1. container
2. loot medkit or ammo
3. player/enemy character

Output:

- approved asset copied into `Assets/HtmlTacticalAssets/ApprovedAssets/`
- scene binding in `TacticalPrototypeTools.cs`
- player-camera route evidence
- updated `TACTICAL_PLAYABLE_ROUTE_GATE.json`

Owner:

- Codex integration.
- Kimi/Gemini review.

### M19: Unity MCP Reliability Gate

Goal:

- Make the Unity MCP loop boring and repeatable.

Required checks:

- read console
- clear console
- refresh assets
- wait for compiling/updating false
- execute menu item
- enter play
- run route gate
- read console
- stop play

Output:

- one Python or shell wrapper for the CoplayDev MCP route
- report JSON
- documented retry rules

## Bottom Line

The correct answer is not to abandon Unity or MCP. The correct answer is to stop letting MCP become a vague live-control surface.

Use Unity like this:

- code and Editor tools are the source of truth,
- CoplayDev MCP is the execution/evidence bridge,
- Nemotron/UI-TARS are review and GUI helpers,
- generated assets are quarantined until they pass visual and gameplay gates.

That is the path that can scale without burning time in invisible Unity states or fake asset-completion reports.
