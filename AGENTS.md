# Unity AI Game Project Instructions

## Project

This is a Unity 6.4 game project for a parent and child to build together. Keep the project beginner-friendly, playable, safe, and easy to understand.

## Main Goal

Help us build a small playable game step by step. Prioritize:

1. A working prototype over large architecture.
2. Clear scene setup over hidden magic.
3. Small, reviewable changes.
4. Child-friendly content.
5. Unity Editor automation through MCP where available.

## Unity Version

Use Unity 6.4 / 6000.4 conventions.

## Repository Layout

- `Assets/Scripts/` for runtime C# scripts.
- `Assets/Editor/` for Unity Editor automation scripts.
- `Assets/Scenes/` for scenes.
- `Assets/Prefabs/` for reusable prefabs.
- `Assets/Materials/` for materials.
- `Assets/Art/` for sprites, textures, generated art, and placeholders.
- `Assets/Audio/` for audio.
- `Assets/Tests/EditMode/` for EditMode tests.
- `Assets/Tests/PlayMode/` for PlayMode tests.
- `ProjectSettings/` and `Packages/` must be version controlled.
- Do not commit `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, or `UserSettings/`.

## Unity File Safety

- Never delete `.meta` files unless deleting the paired asset intentionally.
- Do not hand-edit GUIDs.
- Prefer Unity APIs, MCP tools, or Editor scripts to modify scenes and prefabs.
- Do not directly perform large manual edits to `.unity` or `.prefab` YAML unless there is no safer path.
- Save scenes after Editor/MCP changes.
- After modifying scripts, check Unity Console through MCP if available.

## MCP Workflow

When Unity MCP is available:

1. First read the Unity Console.
2. Inspect the current scene hierarchy.
3. Make small changes.
4. Save the scene.
5. Read the Console again.
6. Summarize changed files and remaining issues.

Prefer using Unity MCP tools for Console, scene, GameObject, asset, script, editor, and command operations. If MCP is unavailable, create safe `Assets/Editor/` menu tools that perform the required Editor actions.

## Tactical Unity / HTML Parity Rules

This repository is a Unity recreation and review export of an existing HTML/Three.js tactical prototype. Do not claim completion from visual inspection alone.

Primary scene:

- `Assets/Scenes/TacticalPrototype.unity`

HTML baseline:

- `reference/html_baseline_final_packet/index.html`

Core rules:

- HTML tactical parity passing is not the same as production visual completion.
- A generated or imported asset is not production-ready unless it is imported successfully, material/PBR validated, semantically correct for its class, bound to a gameplay entity, visible from the actual player camera, involved in a gameplay event, and recorded in a JSON gate/report.
- Contact sheets, fixed showcase cameras, standalone GLBs, filenames, and hash manifests are diagnostic only.
- `full_visual_asset_gate_passed=false` means the broader visual/asset goal is still open.

## Unity MCP Discipline

Use MCP as transport, not as the design brain.

Preferred loop:

1. Read `AGENTS.md` and the relevant docs report.
2. Read Unity Console through MCP.
3. Check whether Unity is compiling or updating.
4. Run one deterministic `AI Tools/...` menu command.
5. Read Console again.
6. Read the generated JSON report.
7. Summarize changed files, menu commands, reports, screenshots, and remaining risks.

Avoid:

- long chains of tiny GameObject edits through MCP;
- repeated refresh while Unity is compiling;
- direct large edits to `.unity` or `.prefab` YAML;
- installing multiple Unity MCP bridges in the active project;
- promoting assets based only on filenames, contact sheets, or showcase scenes.

If Unity is compile/reload locked for more than 2 minutes:

1. Stop polling.
2. Save if possible.
3. Ask for or perform a clean Unity restart.
4. Wait for compile/update idle.
5. Read Console.
6. Run exactly one gate.

## Evidence Gates

After tactical gameplay changes, run:

- `AI Tools/Run Tactical Acceptance Pipeline`

After player-view visual changes, capture:

- `Assets/Screenshots/tactical_html_replica_current_player_pov_verified.png`

After asset promotion changes, update:

- `docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json`
- `docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json`
- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`

Use these available deterministic menu commands as stable workflow entry points:

- `AI Tools/Open Tactical Playable Scene`
- `AI Tools/Run Tactical Preflight`
- `AI Tools/Run Tactical Acceptance Pipeline`
- `AI Tools/Capture Tactical Verified Player POV Screenshot`
- `AI Tools/Write Promoted Asset Player Camera Visibility Gate`
- `AI Tools/Validate Realified Import And Materials`
- `AI Tools/Run Game Feel Evidence Gate`
- `AI Tools/Run Unity MCP Smoke Check`

Do not add or run a generic `Promote One Asset Class To Gameplay` command without a mission-specific class, because promotion requires explicit semantic, player-camera, and gameplay-event evidence.

## Coding Style

- Use simple, readable C#.
- Prefer small MonoBehaviours with clear responsibilities.
- Avoid one giant GameManager script.
- Use serialized fields instead of hard-coded scene references when reasonable.
- Use ScriptableObjects for data that designers or children may edit later.
- Add comments for concepts a beginner should learn.
- Avoid over-engineering patterns unless needed.

## Game Design Rules

- Keep the game child-friendly.
- No violence, horror, gambling, adult content, or manipulative monetization.
- Use placeholder shapes/materials first.
- Make one playable loop before adding advanced features.
- Every feature should have a clear test: what should the player see or do?

## AI / API Key Rules

- Never put OpenAI or other API keys inside Unity client code, scenes, ScriptableObjects, Resources, StreamingAssets, or PlayerPrefs.
- If runtime AI API calls are needed, route them through a backend server or local development proxy.
- Do not print secrets to logs.
- Do not commit secrets.

## Package Rules

- Ask before adding paid Asset Store packages.
- Prefer Unity Registry packages where possible.
- Before adding a package, explain why it is needed.
- For navigation AI, prefer Unity AI Navigation.
- For machine-learning experiments, only add ML-Agents after confirming the gameplay need.

## Definition Of Done

A task is done only when:

1. The requested feature is implemented.
2. Unity Console has no new compile errors.
3. The scene or prefab changes are saved.
4. The diff is reviewable.
5. The final response lists changed files, what was tested, and remaining risks.
