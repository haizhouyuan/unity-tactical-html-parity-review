# Unity AI + MCP Workflow Reset

Date: 2026-05-17

## Why This Reset Exists

The current Unity tactical prototype has made useful progress, but the development loop has become inefficient. The main failure mode is not one gameplay bug; it is a workflow bug:

- Unity Editor can remain in `EditorApplication.isCompiling == true` while MCP calls still work.
- New `Assets/Editor/*.cs` menu items may not appear until assembly reload finishes.
- Entering Play Mode while assemblies are locked can run stale code.
- Long Editor scripts are hard to trust when Unity is in a half-reloaded state.

This document changes the working model from "keep pushing Unity live until it works" to a more controlled AI Unity factory loop.

## Local Evidence

Current local runner verification:

```text
~/Projects/local-coding-runners/scripts/quick-verify.sh

llama-server (8083): OK
UI-TARS (8082):     OK
MCP self-test:      OK
```

Kimi handoff says the local screen/quality stack is available:

- Nemotron Observe is the semantic "eyes" layer.
- UI-TARS is the GUI "hands" layer.
- Both are exposed to Kimi through MCP.
- The asset factory has generated textured GLB assets and copied them into Unity.

Unity asset evidence:

```text
/Users/yuanshaochen/My project/Assets/HtmlTacticalAssets/RealifiedAssets/
size: about 347 MB
contains: textured GLB, LOD GLB, and .meta files for weapon/player/enemy/gear/loot/container/crate classes
```

Unity project has CoplayDev MCP for Unity installed in `Packages/manifest.json`:

```json
"com.coplaydev.unity-mcp": "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main"
```

Official Unity MCP is not the active route. It was blocked by Unity plan/seat entitlement, so the current route is the community MCP at:

```text
http://127.0.0.1:8080/mcp
server: mcp-for-unity-server 3.3.1
```

## Community / Official Source Signals

### CoplayDev MCP for Unity

Source: https://github.com/CoplayDev/unity-mcp

Relevant signals:

- It is designed to bridge AI assistants with Unity Editor through MCP.
- It supports HTTP server mode on `localhost:8080`.
- Its tool set includes `execute_menu_item`, `manage_scene`, `manage_asset`, `manage_material`, `manage_script`, `read_console`, `refresh_unity`, `run_tests`, and `execute_code`.
- Its own README frames it as an automation bridge, not as a replacement for disciplined Unity project structure.

Implication for this project:

- Use MCP for Editor operations and evidence, but avoid making it the only way to author complex gameplay.
- Prefer small, deterministic commands and reports over long fragile live sessions.

### CoderGamester MCP Unity

Source: https://github.com/CoderGamester/mcp-unity

Relevant signals:

- It exposes Unity Editor operations to clients including Claude Code, Codex CLI, GitHub Copilot, and other AI coding tools.
- It uses a Unity-side WebSocket bridge plus a Node MCP server.
- It includes tools such as `execute_menu_item`, `update_gameobject`, `update_component`, `add_asset_to_scene`, `create_prefab`, and `run_tests`.

Implication for this project:

- If Coplay MCP remains unstable around compile/reload, CoderGamester MCP is a valid probe lane, but it should be evaluated in a copy/worktree project first.
- Do not install multiple Unity MCP bridges into the same production project until one is selected as primary.

### Unity Editor Reload / Compilation

Sources:

- https://docs.unity3d.com/ScriptReference/EditorApplication.LockReloadAssemblies.html
- https://docs.unity3d.com/ScriptReference/EditorApplication.UnlockReloadAssemblies.html
- https://docs.unity3d.com/ScriptReference/AssetDatabase.Refresh.html

Observed local issue:

```text
Entering playmode with assembly reload locked. Recent script changes might be missing.
In order to allow script updates call EditorApplication.UnlockReloadAssemblies().
```

Implication for this project:

- Never enter Play Mode as part of an acceptance gate while `EditorApplication.isCompiling` or `EditorApplication.isUpdating` is true.
- If Unity reports reload locked, run recovery once, then restart Unity if still stuck.
- Do not repeatedly call `StopAssetEditing` blindly. It produced:

```text
StopAssetEditing invoked without a call to StartAssetEditing.
Assertion failed on expression: 'gRefreshReentrancyCount > 0'
```

### Assembly Definitions / Compile Control

Source: https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html

Implication for this project:

- Split tactical runtime scripts, tactical editor gates, and large imported package code into assembly definition boundaries.
- The tactical acceptance tools should be isolated in an Editor-only assembly, so ordinary runtime edits do not keep invalidating everything.

### Command Line / Tests

Sources:

- https://docs.unity3d.com/Manual/CommandLineArguments.html
- https://docs.unity3d.com/Packages/com.unity.test-framework@1.6/manual/reference-command-line.html

Implication for this project:

- Use batchmode tests in a separate closed-Editor pass when possible.
- Do not run batchmode against the same project while the interactive Editor is already open.

## New Default Workflow

### Tier 1: File-Level Authoring

Use Codex/Kimi/Gemini for code, scene-generation tools, validation scripts, and documentation.

Allowed:

- Edit C# scripts.
- Edit small Editor tools.
- Edit docs and reports.
- Generate deterministic route-gate scripts.

Avoid:

- Direct hand edits to large `.unity` scene YAML.
- Long chains of Unity MCP calls while the Editor is compiling.

### Tier 2: Unity MCP Evidence

Use MCP only after the Editor is stable.

Before running any Play/gate command:

1. `manage_editor stop`
2. `read_console clear`
3. `execute_code` check:

```csharp
return new {
    compiling = UnityEditor.EditorApplication.isCompiling,
    updating = UnityEditor.EditorApplication.isUpdating,
    playing = UnityEditor.EditorApplication.isPlaying
};
```

4. If compiling/updating is false, run `manage_editor play`.
5. Execute exactly one gate.
6. Read Console.
7. Stop Play.

### Tier 3: Heavy Visual / GUI Review

Use Kimi MCP tools:

- `observe_screen` for semantic screen assessment.
- `uitars_run_task` only for bounded GUI actions.

Use these for visual QA, not for primary code edits.

### Tier 4: Asset Factory Integration

Treat the Kimi-produced GLB chain as an asset input, not as completion.

For each imported asset class:

1. Verify Unity import succeeds.
2. Verify material shader and maps.
3. Place into scene or prefab.
4. Bind to gameplay entity.
5. Capture player-camera evidence.
6. Only then count it as part of the HTML-equivalent Unity remake.

## Immediate Changes To The Tactical Project Loop

### 1. Stop relying on newly added menu scripts during a dirty compile session

If Unity does not see a new `MenuItem`, do not keep trying the menu. Either:

- restart Unity cleanly, or
- run the proof as transient `execute_code`, or
- merge the gate into an already-loaded Editor tool after a clean compile.

### 2. Add a clean Unity restart checkpoint

When these symptoms appear:

- `isCompiling == true` for more than 2 minutes,
- new menu item not visible after refresh,
- Editor.log says assembly reload is locked,
- Play Mode enters with stale scripts,

the next action is:

```text
save assets -> quit Unity -> reopen project -> wait until not compiling -> read console -> run one gate
```

### 3. Prefer mission-level probes over micro-fixes

Do not continue writing small gate patches until the Unity control plane is stable. The next useful missions are:

- Unity MCP reliability probe.
- GLB/PBR import validation.
- Player-camera asset binding validation.
- HTML gameplay parity route gate.

## Updated Mission Queue

### M-Unity-01: MCP Reliability And Recovery

Goal:
Determine the stable Unity MCP operating recipe for this project.

Tasks:

- Check editor state resource and console.
- Verify whether `refresh_unity` can ever complete without timeout.
- Verify menu visibility after a clean restart.
- Record when `execute_code` is safer than `execute_menu_item`.
- Produce `docs/UNITY_MCP_RELIABILITY_REPORT.md`.

Acceptance:

- Clear rule for when to restart Unity.
- Clear rule for menu tools vs transient `execute_code`.
- No claim of "MCP good" without a command log.

### M-Unity-02: Imported GLB/PBR Validation

Goal:
Verify Kimi's generated assets are actually usable in Unity URP.

Tasks:

- Refresh assets after a clean Unity state.
- Inspect all GLB assets under `Assets/HtmlTacticalAssets/RealifiedAssets`.
- Check prefab import, renderer count, material shader, texture slots.
- Write report and failure list.

Acceptance:

- `docs/UNITY_GLB_PBR_IMPORT_REPORT.md`
- table by asset id: imported, renderer count, shader, base map, normal map, metallic/smoothness, issues.

### M-Unity-03: Gameplay Binding Upgrade

Goal:
Move from "asset exists" to "gameplay uses asset."

Tasks:

- Choose one weapon, one loot item, one enemy, one environment object.
- Ensure each is visible in player camera and attached to a gameplay entity.
- Gate must record entity id, asset path, renderer count, and screenshot path.

Acceptance:

- Player camera screenshots show the assets in real gameplay context.
- Report proves no evidence-only object was counted.

### M-Unity-04: HTML Parity Route Gate

Goal:
Run a full route: lobby -> start -> spawn -> move -> pickup -> weapon switch/fire -> enemy ranged attack -> ladder -> heal -> death -> restart.

Implementation preference:

- If Unity compile/reload is stable, use a normal Editor menu item.
- If not stable, use transient MCP `execute_code` for the gate.

Acceptance:

- JSON report plus screenshots for every route phase.

## Decision

For the next work segment, do not keep expanding gameplay until `M-Unity-01` and `M-Unity-02` are done.

The fastest path toward the user's real goal is:

1. Stabilize Unity MCP operation.
2. Verify imported GLB/PBR assets.
3. Bind verified assets into gameplay.
4. Only then expand HTML parity features.

## Gemini Read-Only Review

Command:

```bash
PATH="/Users/yuanshaochen/Projects/local-coding-runners/bin:$PATH" \
RUNNER_TIMEOUT_SECONDS=240 \
GEMINI_REVIEW_MODEL=gemini-3.1-pro-preview \
runner-gemini-review /tmp/gemini_unity_mcp_workflow_review.md
```

Limitation:

- Gemini could read `local-coding-runners/HANDOFF.md`.
- Gemini could not read `/Users/yuanshaochen/My project/...` because the runner workspace was limited to `/Users/yuanshaochen/Projects/local-coding-runners`.
- Its review is therefore a workflow critique from the prompt/context, not a direct Unity-project code review.

Useful recommendations from Gemini:

1. Split generated tactical code and Editor tools with assembly definitions to reduce compile/reload blast radius.
2. Prefer `AssetPostprocessor`/automatic import logic for GLB/PBR setup instead of selection-dependent menu tools.
3. Use a visual validation loop: Editor script validates data; Nemotron/UI-TARS validate what actually renders.
4. Avoid writing code while Play Mode is active.
5. Do not trust data-only GLB checks; pink shaders, missing textures, inverted normals, and wrong material slots require visual evidence.

Adopted change:

- The next implementation pass should not be more gameplay feature patches. It should be `M-Unity-01` and `M-Unity-02`.

## M-Unity-02 Result Update

After a clean Unity restart, the community MCP route was able to run transient Editor code and produce the GLB/PBR reports.

Evidence:

- `docs/UNITY_GLB_PBR_IMPORT_REPORT.json`
- `docs/UNITY_GLB_PBR_IMPORT_REPORT.md`
- `docs/UNITY_GLB_PBR_VISUAL_REVIEW.json`
- `docs/UNITY_GLB_PBR_VISUAL_REVIEW.md`
- `Assets/Screenshots/glb_pbr_textured_contact_sheet.png`
- `Assets/Scenes/GLB_PBR_Textured_ContactSheet.unity`

What passed:

- Unity imported all 48 GLBs under `Assets/HtmlTacticalAssets/RealifiedAssets`.
- All 48 imported prefabs had renderer/material evidence.
- glTFast used `Shader Graphs/glTF-pbrMetallicRoughness`.
- The final 12 `*_textured.glb` assets rendered in a normalized review scene.
- Unity Console remained clean during this validation pass.

What failed:

- Category correctness failed. The files named as player, enemy, helmet, vest, ammo, medkit, container, and crate are visually weapon-like in the contact sheet.
- All 12 final assets have identical imported bounds, which is a strong warning that the batch reused the same base geometry or cleanup template across unrelated categories.
- Normal map slots were not detected.

Decision:

- M-Unity-02 is complete as a validation mission, but the asset batch is not approved for gameplay replacement.
- The next asset-factory mission must regenerate or replace the category-failed assets before gameplay binding.

## Updated Mission Queue After Visual Review

### M-Asset-01: Category-Correct Asset Regeneration

Goal:
Regenerate or replace the failed non-weapon assets so that category shape matches asset id.

Priority assets:

1. `RS_04_player_tactical_textured.glb`
2. `RS_05_enemy_tactical_textured.glb`
3. `RS_10_prop_container_textured.glb`
4. `RS_11_prop_crate_textured.glb`
5. `RS_09_loot_medkit_textured.glb`
6. `RS_08_loot_ammo_textured.glb`

Acceptance:

- Each asset has a rendered preview.
- Silhouette matches category.
- It is not a recolored weapon-like mesh.
- It imports in Unity with no Console errors.
- It appears in a normalized contact sheet.

### M-Unity-03 Revised: Gameplay Binding Only For Approved Assets

Do not bind the current failed category assets as if they were production replacements. Bind only:

- existing procedural fallback objects, or
- visually approved regenerated assets, or
- the current weapon-like assets as temporary weapon candidates.

The gameplay-binding report must include `asset_quality_status` and reject `category_failed` assets for completion credit.

## Gemini Read-Only Review Update

Command route:

```bash
CWD="/Users/yuanshaochen/My project" TIMEOUT=240 \
/Users/yuanshaochen/Projects/local-coding-runners/bin/acpx-gemini \
/tmp/gemini_unity_next_missions_review.md
```

Gemini was able to read the Unity project docs and `Assets/Editor/TacticalPrototypeTools.cs`.

Key review result:

- Gameplay parity is relatively strong: lobby, start, movement, loot mutation, weapon fire, NPC ranged attack, ladder use, death, and restart are covered by the route gate.
- Environment parity is medium: approved crate and PBR concrete/asphalt are now bound into the scene.
- Visual asset parity is the critical failure: the 12 generated GLB batch imported but suffers from category silhouette bleed.
- Character/animation parity is still low: current procedural bob/pulse feedback is not a real humanoid rig/animation solution.

Recommended next missions:

1. Category-correct GLB regeneration.
2. Humanoid animation integration.
3. URP post-processing and decal pass.
4. Authored audio asset wiring.
5. HUD iconography upgrade.

Adopted decision:

- Do not continue batch-importing generated assets without a visual category gate.
- Add a category gate before PBR texturing and before Unity gameplay binding.

## M30 Material/Texture Audit Update

After the category/silhouette review, I added a lower-level GLB JSON-chunk audit so the project no longer treats "Unity imported it" as "production PBR asset".

Evidence:

- `docs/REALIFIED_ASSETS_IMPORT_AUDIT.json`
- `docs/REALIFIED_ASSETS_IMPORT_AUDIT.md`
- `tasks/mission_outputs/M30_realified_assets_material_texture_reality_gate/`

Result:

- Total RealifiedAssets GLBs: 48
- Textured probes: 48
- Promotable PBR candidates: 0
- External texture sidecars: 0
- External material files: 0
- Assets with `normalTexture`: 0
- Assets with `occlusionTexture` / AO: 0
- Assets missing primitive `NORMAL`: 12
- Assets missing `TANGENT`: 48

Important distinction:

- `docs/UNITY_GLB_PBR_IMPORT_REPORT.json` proves Unity/GLTFast can import the files and detect embedded base color plus metallic-roughness textures.
- `docs/REALIFIED_ASSETS_IMPORT_AUDIT.json` proves that the same batch is still below the production promotion bar because it lacks normal/AO/tangent coverage.

Adopted gate change:

- `Assets/Editor/TacticalPlayableRouteGate.cs` now reads `docs/REALIFIED_ASSETS_IMPORT_AUDIT.json`.
- `docs/TACTICAL_PLAYABLE_ROUTE_GATE.json` now includes:
  - `realified_audit_present`
  - `realified_audit_total_glb`
  - `realified_audit_textured_probe_count`
  - `realified_audit_promotable_pbr_candidates`
  - `realified_audit_external_texture_sidecars`
  - `realified_audit_external_material_files`

Latest Unity acceptance run after the gate change:

- `playable_route_gate_passed`: true
- `gameplay_proof_gate_passed`: true
- `player_pov_gate_passed`: true
- `console_errors`: 0
- `console_warnings`: 0
- `full_visual_asset_gate_passed`: false

Decision:

- Keep the Kimi-generated RealifiedAssets batch as asset-factory evidence and visual/reference probes.
- Do not promote the batch as gameplay replacement.
- Promote only one asset class at a time after category, PBR, mesh attribute, and player-camera gameplay-binding gates pass.
