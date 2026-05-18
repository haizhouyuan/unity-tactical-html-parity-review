# Long Running AI Game Studio TODO

Date: 2026-05-18

This is the master execution checklist for the Unity tactical prototype and AI Game Studio OS work. It starts after M0 and keeps all previously discussed ChatGPT Pro/Codex routes in one place.

This file is a working rail, not a victory declaration. A task is complete only when its evidence exists and the relevant report/gate proves it.

## Nonstop Execution Discipline

When an agent works from this TODO:

1. Start from the first unchecked blocking item.
2. Do not stop after only writing a status summary if a scoped implementation can be advanced.
3. If one lane is blocked, write the blocker report and switch to the next unblocked lane.
4. Do not broaden a mission mid-flight.
5. Do not count Unity scene screenshots, contact sheets, GLBs, or report filenames as completion.
6. Use mission contracts from `docs/MISSION_TEMPLATE.md`.
7. Use asset promotion rules from `docs/ASSET_PROMOTION_STANDARD.md`.
8. Keep Unity MCP as transport, not as the design brain.
9. Prefer deterministic `AI Tools/...` menu commands and JSON evidence.
10. Commit small verified slices.

## Current Ground Truth

Current exported evidence says:

- `all_required_current_gates_passed=true`
- `ai_playtest_route_gate_passed=true`
- `full_visual_asset_gate_passed=false`
- `category_semantic_review_passed=false`
- `realified_asset_gameplay_production_promoted_assets=3`
- `promoted_asset_visibility_gate_passed=true`

Interpretation:

- Gameplay/current route evidence exists.
- HTML parity evidence is useful but not final visual completion.
- Production visual assets are still the main unfinished goal.
- M0 docs are complete.
- M91 external-input built-player route evidence now exists and passes.
- M93 now promotes the corrected Realified ammo and medkit loot through route pickup evidence, but the strict full visual gate remains false.
- Next executable work returns to the remaining M88 strict visual blockers: final weapon art review, final humanoid art review, and the rest of generated-batch class promotion.

## Master Queue

### M0.5: Active Unity Session Verification

Status: resolved via M0.5R on 2026-05-18. The active Unity session is `/Users/yuanshaochen/My project`, not this public review checkout, so the active project needed the workflow tools and HTML baseline synced before verification could pass.

Goal:

- Prove the active Unity session has loaded this checkout and can run the current `AI Tools/...` commands.

Read first:

- `docs/MISSION_TEMPLATE.md`
- `docs/AI_CLOSED_LOOP_PIPELINE.md`
- `docs/UNITY_MCP_RELIABILITY_REPORT.md`
- `docs/UNITY_MCP_SMOKE_REPORT_LATEST.json`

Allowed changes:

- generated readiness reports;
- MCP smoke reports;
- Console/command logs.

Forbidden changes:

- gameplay scripts;
- scenes;
- packages;
- generated assets;
- acceptance report values by hand.

Checklist:

- [x] Probe active Unity project path; result is not this repository checkout.
- [x] Wait for Unity compile/update idle.
- [x] Read Console through MCP or document fallback failure.
- [x] Confirm `Assets/Editor/TacticalWorkflowTools.cs` is imported.
- [x] Run `AI Tools/Run Tactical Preflight`.
- [x] Confirm `docs/TACTICAL_PREFLIGHT_REPORT.json` exists.
- [x] Run `AI Tools/Run Unity MCP Smoke Check`.
- [x] Confirm `docs/UNITY_MCP_SMOKE_REPORT_LATEST.json` exists.
- [x] Record whether this proves editor readiness, external MCP transport, or both.
- [x] If the menu is missing, stop gameplay work and run compile/reload recovery.

Definition of done:

- Console has no compile errors.
- Preflight report exists.
- MCP smoke report exists.
- Final note distinguishes repo-level static readiness from active Unity execution.

Next if blocked:

- M0.5R Unity compile/reload recovery.

Latest evidence:

- `docs/M0_5_ACTIVE_UNITY_SESSION_VERIFICATION_2026-05-18.json`
- `docs/M0_5_ACTIVE_UNITY_SESSION_VERIFICATION_2026-05-18.md`
- `docs/M0_5R_ACTIVE_UNITY_SESSION_VERIFICATION_2026-05-18.json`
- `docs/M0_5R_ACTIVE_UNITY_SESSION_VERIFICATION_2026-05-18.md`
- `docs/TACTICAL_PREFLIGHT_REPORT_ACTIVE_PROJECT_2026-05-18.json`
- `docs/UNITY_MCP_SMOKE_REPORT_ACTIVE_PROJECT_2026-05-18.json`
- MCP transport reachable; active project verified after syncing workflow tools and the HTML baseline into `/Users/yuanshaochen/My project`.

### M0.5R: Unity Compile/Reload Recovery

Status: completed on 2026-05-18.

Goal:

- Recover from stale Unity editor AppDomain, missing menu items, compile lock, or MCP session mismatch.

Checklist:

- [x] Stop polling after 2 minutes of compile/reload lock.
- [x] Save/sync only the required workflow files into the active Unity project.
- [x] Refresh Unity assets.
- [x] Wait for compile/update idle.
- [x] Read Console.
- [x] Re-run only `AI Tools/Run Tactical Preflight`.
- [x] Re-run only `AI Tools/Run Unity MCP Smoke Check`.
- [x] Write pass report with the exact active-project evidence.

Definition of done:

- M0.5 passes, or a blocker report names the exact failed layer: Unity project path, compile errors, menu import, MCP connection, or command execution.

### M0.6: Community Unity MCP Skill Verification

Status: completed on 2026-05-18.

Goal:

- Add and verify a repo-local Codex adapter skill for the current CoplayDev/community Unity MCP bridge.

Checklist:

- [x] Add `.codex/skills/community-unity-mcp-adapter/SKILL.md`.
- [x] Add `.codex/skills/community-unity-mcp-adapter/references/community-mcp-tool-contract.md`.
- [x] Document the skill plan in `docs/COMMUNITY_UNITY_MCP_CODEX_SKILL_PLAN.md`.
- [x] Verify the community MCP bridge exposes `execute_menu_item`, `read_console`, `execute_code`, and `batch_execute`.
- [x] Run `AI Tools/Run Tactical Preflight` through community MCP.
- [x] Copy/read `docs/TACTICAL_PREFLIGHT_REPORT.json`.
- [x] Record that current-session skill selector discovery is not proven until a fresh Codex session loads the new repo-local skill.

Latest evidence:

- `docs/M0_6_COMMUNITY_UNITY_MCP_SKILL_VERIFICATION_2026-05-18.md`
- `docs/M0_6_COMMUNITY_UNITY_MCP_SKILL_VERIFICATION_2026-05-18.json`
- `docs/TACTICAL_PREFLIGHT_REPORT.json`

### M81: Building Integrity Gate

Status: completed on 2026-05-18.

Primary lane: gameplay engineering.

Goal:

- Prove the tactical building, floors, doorways, ladders, balconies, and cover blockers work from the player route.

Player problems covered:

- cannot enter first floor;
- ladder/climb can place player wrong;
- floor support can feel floating or invalid;
- NPC bullets may pass through containers/cover.

Allowed changes:

- focused runtime/building/collision scripts;
- focused Editor/gate script;
- generated building integrity report;
- player-camera/debug screenshots.

Forbidden changes:

- weapon tuning;
- generated asset import;
- visual production pass;
- package changes;
- broad map rebuild.

Checklist:

- [x] Create or update a mission contract for M81.
- [x] Add `AI Tools/Run Building Integrity Gate` if missing.
- [x] Gate starts player outside a building.
- [x] Gate proves player can enter first floor.
- [x] Gate proves player can reach a valid second-floor/upper-floor landing.
- [x] Gate records floor/support height transitions.
- [x] Gate verifies balcony/fall/support behavior.
- [x] Gate verifies door/collider blockers do not block valid entry.
- [x] Gate verifies container/cover blocks NPC line-of-sight or shots.
- [x] Gate emits JSON report.
- [x] Gate emits or references player-camera/debug screenshot evidence.
- [x] Tactical acceptance pipeline still passes.

Definition of done:

- Building integrity JSON passes.
- Player route is not broken.
- Cover/collision proof exists.
- No new Console compile errors.

Latest evidence:

- `docs/M81_BUILDING_INTEGRITY_GATE_2026-05-18.md`
- `docs/M81_BUILDING_INTEGRITY_COMPLETION_2026-05-18.md`
- `docs/BUILDING_INTEGRITY_GATE.json`
- `docs/M81_BUILDING_INTEGRITY_PIPELINE_2026-05-18.json`
- `Assets/Screenshots/BuildingIntegrity/`

### M82: Weapon Feel Gate

Status: completed on 2026-05-18.

Primary lane: gameplay engineering / visual direction.

Goal:

- Prove weapon feel from actual gameplay: first-person visibility, third-person mount, ADS/readability, fire, reload, recoil, muzzle flash, casing, hit marker, ammo state, and audio hooks.

Allowed changes:

- scoped weapon scripts;
- weapon visual/hands/mount tuning;
- weapon feel gate report;
- screenshots.

Forbidden changes:

- building/collision work;
- broad map art pass;
- new asset batch generation;
- package changes.

Checklist:

- [x] Create or update a mission contract for M82.
- [x] Add `AI Tools/Run Weapon Feel Gate`.
- [x] Prove first-person weapon visible at spawn.
- [x] Prove fire event changes ammo and spawns feedback.
- [x] Prove reload state mutation.
- [x] Prove hit marker or enemy damage event.
- [x] Prove recoil/camera/weapon motion is visible or reportable.
- [x] Prove muzzle flash/tracer/casing/audio hooks exist or explicitly report missing.
- [x] Prove third-person weapon mount visibility.
- [x] Capture player-camera screenshots for first-person and third-person weapon states.
- [x] Update route/gate evidence without hand-editing report values.

Definition of done:

- Weapon Feel Gate passes for the current hero weapon.
- At least one player-camera screenshot shows correct weapon visibility.
- No static showcase proof is counted as gameplay proof.

Latest evidence:

- `docs/M82_WEAPON_FEEL_GATE_2026-05-18.md`
- `docs/M82_WEAPON_FEEL_COMPLETION_2026-05-18.md`
- `docs/WEAPON_FEEL_GATE.json`
- `docs/M82_WEAPON_FEEL_PIPELINE_2026-05-18.json`
- `Assets/Screenshots/WeaponFeel/`
- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`

### M83: AI Playtest Route

Status: completed on 2026-05-18.

Primary lane: QA / playtest.

Goal:

- Automate the core playable route so future changes are tested like a player would experience them.

Route:

```text
lobby/start
  -> spawn
  -> move
  -> enter building
  -> climb/reach upper floor
  -> pickup
  -> fire
  -> hit NPC or receive NPC attack
  -> reload
  -> death/restart or route completion
```

Checklist:

- [x] Create mission contract for M83.
- [x] Add `AI Tools/Run AI Playtest Route Gate`.
- [x] Simulate or script deterministic player route through existing route/gameplay gates.
- [x] Record positions, state transitions, inventory, weapon state, and enemy state through aggregated reports.
- [x] Capture or reference player-camera screenshots at route milestones.
- [x] Write route JSON with pass/fail and failure taxonomy.
- [x] Detect stuck/no-progress conditions.
- [x] Detect missing weapon/loot/enemy visibility.
- [x] Detect blocked pickup/fire/reload.
- [x] Run acceptance pipeline after playtest route.

Definition of done:

- AI playtest route JSON exists and passes.
- The route can fail with useful reasons when a core action breaks.
- It does not depend on fixed showcase cameras.

Latest evidence:

- `docs/M83_AI_PLAYTEST_ROUTE_MISSION_CONTRACT_2026-05-18.md`
- `docs/M83_AI_PLAYTEST_ROUTE_GATE_2026-05-18.md`
- `docs/M83_AI_PLAYTEST_ROUTE_COMPLETION_2026-05-18.md`
- `docs/AI_PLAYTEST_ROUTE_GATE.json`
- `docs/M83_AI_PLAYTEST_ROUTE_PIPELINE_2026-05-18.json`
- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`

### M84: Three-Class Asset Factory Spike

Status: completed on 2026-05-18.

Primary lane: asset pipeline.

Goal:

- Prove the asset factory pipeline on exactly three classes before scaling:
  - weapon: GROZA-like/hero weapon candidate;
  - environment prop: container/cover prop;
  - loot: medkit.

Allowed changes:

- asset candidates;
- sidecars;
- import/material reports;
- semantic review reports;
- class-specific gameplay binding only when scoped.

Forbidden changes:

- batch-promoting unreviewed assets;
- using filenames as semantic proof;
- broad visual pass;
- package changes unless specifically approved.

Checklist:

- [x] Create mission contract for M84.
- [x] For each class, record source/provenance and intended semantic class.
- [x] Import into Unity.
- [x] Validate material/PBR state.
- [x] Run semantic review or reuse already-approved semantic assets.
- [x] Bind to gameplay entity only if semantic-ready.
- [x] Prove player-camera visibility.
- [x] Prove gameplay event:
  - weapon fires/reloads/hits;
  - container blocks line-of-sight/shots;
  - medkit is picked up or mutates state.
- [x] Update M84-specific promotion ledger/report.
- [x] Update tactical acceptance pipeline with M84 player-camera visibility and event evidence.

Definition of done:

- One weapon, one environment prop, and one loot item reach production-promoted status.
- No candidate-only asset receives completion credit.
- Existing failed Realified non-weapon semantic reviews stay quarantined instead of being overwritten.

Latest evidence:

- `docs/M84_THREE_CLASS_ASSET_FACTORY_SPIKE_MISSION_CONTRACT_2026-05-18.md`
- `docs/M84_THREE_CLASS_ASSET_FACTORY_SPIKE_COMPLETION_2026-05-18.md`
- `docs/M84_THREE_CLASS_ASSET_FACTORY_SPIKE.json`
- `docs/M84_THREE_CLASS_ASSET_FACTORY_SPIKE.md`
- `docs/M84_THREE_CLASS_ASSET_FACTORY_SPIKE_PIPELINE_2026-05-18.json`
- `Assets/Screenshots/M84AssetFactory/`

### M85: Visual Production Pass

Status: completed on 2026-05-18.

Primary lane: visual direction.

Goal:

- Improve PUBG-like tactical believability without breaking gameplay route.

Scope:

- lighting;
- fog/postprocess;
- decals;
- material variation;
- environmental clutter;
- character silhouette/gear readability;
- weapon hero view polish;
- HUD readability.

Checklist:

- [x] Create mission contract for M85.
- [x] Capture before screenshots from player route.
- [x] Define visual style guide for small tactical slice.
- [x] Add scoped decals/clutter/material variation.
- [x] Tune lighting/postprocess without hiding asset defects.
- [x] Capture after screenshots from same route.
- [x] Run visual review through an M85-specific JSON gate.
- [x] Run acceptance pipeline.

Definition of done:

- Before/after player-camera evidence exists.
- Gameplay route still passes.
- Full visual asset gate improves without hiding failed categories.

Latest evidence:

- `docs/M85_VISUAL_PRODUCTION_PASS_MISSION_CONTRACT_2026-05-18.md`
- `docs/M85_VISUAL_STYLE_GUIDE_2026-05-18.md`
- `docs/M85_VISUAL_PRODUCTION_PASS.json`
- `docs/M85_VISUAL_PRODUCTION_PASS.md`
- `docs/M85_VISUAL_PRODUCTION_PASS_COMPLETION_2026-05-18.md`
- `docs/M85_VISUAL_PRODUCTION_PASS_PIPELINE_2026-05-18.json`
- `Assets/Screenshots/M85VisualProduction/`

### M86: Build / Release Gate

Status: completed on 2026-05-18.

Primary lane: release/liveops.

Goal:

- Produce a reviewable build/release packet rather than only editor evidence.

Checklist:

- [x] Create mission contract for M86.
- [x] Choose build target: `StandaloneOSX`.
- [x] Run preflight through the existing M85 acceptance state.
- [x] Build in Unity.
- [x] Run launch smoke test.
- [x] Capture build screenshot evidence. Note: current screenshot is window/process proof only because macOS captured desktop foreground content.
- [x] Write changelog/release notes.
- [x] Update README current evidence snapshot.
- [x] Record known limitations.

Definition of done:

- Build artifact exists or blocker is explicit.
- Build can launch.
- Release notes include current gate truth.

Latest evidence:

- `docs/M86_BUILD_RELEASE_GATE_MISSION_CONTRACT_2026-05-18.md`
- `docs/M86_BUILD_RELEASE_GATE.json`
- `docs/M86_BUILD_RELEASE_GATE.md`
- `docs/M86_BUILD_RELEASE_COMPLETION_2026-05-18.md`
- `docs/M86_BUILD_RELEASE_PIPELINE_2026-05-18.json`
- `docs/M86_RELEASE_NOTES_2026-05-18.md`
- `docs/M86_LAUNCH_SMOKE.json`
- `docs/M86_LAUNCH_SMOKE.log`
- `docs/M86_LAUNCH_VISUAL_SMOKE.json`
- `docs/M86_LAUNCH_VISUAL_SMOKE.log`
- `Assets/Screenshots/M86BuildRelease/01_build_launch_smoke_screen.png`

Local build artifact:

- `/Users/yuanshaochen/My project/Builds/M86/TacticalPrototypeM86.app`

Important limitation:

- The app bundle is ignored by Git under `Builds/`.
- The visual launch screenshot is not gameplay-quality evidence.

### M87: Class-Level Production Visibility Reconciliation

Status: completed on 2026-05-18.

Primary lane: evidence audit / visual direction.

Goal:

- Close the accounting gap between the old Realified batch visibility gate and the actual route-level approved-equivalent assets now used in gameplay.

Checklist:

- [x] Add `AI Tools/Run M87 Class-Level Production Visibility Gate`.
- [x] Prove `weapon` class with `hero_rifle` player-camera and fire/reload/hit evidence.
- [x] Prove `character` class with approved player/enemy GLBs, tactical detail kit, and authored clip evidence.
- [x] Prove `gear` class with approved helmet/vest pickup route evidence.
- [x] Prove `loot` class with approved medkit/ammo pickup route evidence.
- [x] Prove `environment_prop` class with approved container/crate player-camera and cover/blocking evidence.
- [x] Preserve `legacy_realified_batch_visibility_gate_passed=false`.
- [x] Preserve `full_visual_asset_gate_passed=false`.

Latest evidence:

- `docs/M87_CLASS_LEVEL_PRODUCTION_VISIBILITY_GATE.json`
- `docs/M87_CLASS_LEVEL_PRODUCTION_VISIBILITY_GATE.md`
- `docs/M87_CLASS_LEVEL_PRODUCTION_VISIBILITY_COMPLETION_2026-05-18.md`
- `docs/M87_CLASS_LEVEL_PRODUCTION_VISIBILITY_PIPELINE_2026-05-18.json`

Important limitation:

- This closes class-level route visibility for approved-equivalent/gameplay-bound assets. It does not rescue the failed Realified batch and does not claim final PUBG-like visual quality.

### M88: Strict Full Visual Asset Gate Audit

Status: completed as a blocker audit on 2026-05-18. The gate intentionally remains failed.

Primary lane: evidence audit / visual direction.

Goal:

- Make the strict `full_visual_asset_gate_passed=false` blocker explicit and executable instead of leaving it as a vague warning.

Checklist:

- [x] Add `AI Tools/Run M88 Strict Full Visual Asset Gate`.
- [x] Verify route, M85, M87, approved incremental assets, first-person weapon polish, and character clip/skinned evidence.
- [x] Keep `full_visual_asset_gate_currently_remains_false=true`.
- [x] Identify remaining strict blockers without hand-editing acceptance values.

Latest evidence:

- `docs/M88_STRICT_FULL_VISUAL_ASSET_GATE.json`
- `docs/M88_STRICT_FULL_VISUAL_ASSET_GATE.md`
- `docs/M88_STRICT_FULL_VISUAL_ASSET_GATE_COMPLETION_2026-05-18.md`
- `docs/M88_STRICT_FULL_VISUAL_ASSET_GATE_PIPELINE_2026-05-18.json`

Remaining blockers:

- `legacy_realified_batch_visibility_gate_passed=false`
- `generated_batch_class_promotion_passed=false`
- `final_weapon_art_review_passed=false`
- `final_humanoid_art_review_passed=false`
- `clean_built_player_gameplay_capture_passed=false`

### M89: Built Player Clean Capture

Status: completed as launch/window evidence on 2026-05-18. This does not close the gameplay-route blocker.

Primary lane: release/liveops / evidence audit.

Goal:

- Improve the weak M86 built-player visual smoke by launching the macOS app and capturing a cleaner foreground screenshot.

Checklist:

- [x] Launch `/Users/yuanshaochen/My project/Builds/M86/TacticalPrototypeM86.app`.
- [x] Attempt to bring the built app to the foreground.
- [x] Capture a foreground screenshot.
- [x] Capture the player log.
- [x] Redact local IP addresses from the committed log.
- [x] Record that this is not an input-driven gameplay route.

Latest evidence:

- `docs/M89_BUILT_PLAYER_CLEAN_CAPTURE.json`
- `docs/M89_BUILT_PLAYER_CLEAN_CAPTURE.log`
- `docs/M89_BUILT_PLAYER_CLEAN_CAPTURE_COMPLETION_2026-05-18.md`
- `docs/M89_BUILT_PLAYER_CLEAN_CAPTURE_PIPELINE_2026-05-18.json`
- `Assets/Screenshots/M89BuiltPlayerCapture/01_built_player_frontmost_capture.png`

Important limitation:

- M89 is cleaner built-player launch/window evidence, not route evidence. The strict blocker `clean_built_player_gameplay_capture_passed=false` remains open.

### M90: Built Player Runtime Route Capture

Status: completed on 2026-05-18 as a built-player runtime route. This is not external keyboard/mouse automation.

Primary lane: release/liveops / evidence audit / gameplay proof.

Goal:

- Prove the built macOS app can execute the core tactical gameplay route and capture evidence outside the Unity Editor.

Checklist:

- [x] Add a built-player-only route recorder that is disabled during normal gameplay unless launched with `--m90-built-route-capture`.
- [x] Rebuild the macOS player through `AI Tools/Run M86 Build Release Gate`.
- [x] Launch `/Users/yuanshaochen/My project/Builds/M86/TacticalPrototypeM86.app` with the M90 flag.
- [x] Capture lobby, start, movement, traversal, pickup, fire, reload, damage, death, and restart screenshots.
- [x] Prove first-person weapon visibility.
- [x] Prove pickup changes gameplay state from pistol to rifle.
- [x] Redact local IP addresses from the committed player log.
- [x] Record `external_input_driven=false` so this is not overstated as manual/input-bot proof.

Latest evidence:

- `Assets/Scripts/Tactical/TacticalBuiltPlayerRouteRecorder.cs`
- `docs/M90_BUILT_PLAYER_RUNTIME_ROUTE_CAPTURE.json`
- `docs/M90_BUILT_PLAYER_RUNTIME_ROUTE_CAPTURE.log`
- `docs/M90_BUILT_PLAYER_RUNTIME_ROUTE_CAPTURE_COMPLETION_2026-05-18.md`
- `docs/M90_BUILT_PLAYER_RUNTIME_ROUTE_CAPTURE_PIPELINE_2026-05-18.json`
- `Assets/Screenshots/M90BuiltPlayerRoute/`

Important limitation:

- M90 proves a built-player runtime gameplay route, not external keyboard/mouse automation and not final visual production quality.

### M91: External-Input Built Player Route

Status: completed on 2026-05-18.

Primary lane: release/liveops / QA playtest / evidence audit.

Goal:

- Complement M90 by proving the built macOS app can complete the route through external input rather than direct runtime gameplay API calls.

Checklist:

- [x] Add `TacticalExternalInputRouteTelemetry`, disabled unless launched with `--m91-external-input-route`.
- [x] Add `AI Tools/Run M91 External Input Built Player Gate`.
- [x] Add mission contract.
- [x] Launch `/Users/yuanshaochen/My project/Builds/M86/TacticalPrototypeM86.app` with `--m91-external-input-route`.
- [x] Drive the route with external OS-level keyboard/mouse input automation.
- [x] Capture start, movement, pickup, fire, reload, enemy interaction, and death/restart screenshots.
- [x] Confirm `docs/M91ExternalInputBuiltPlayerRoute/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json` has `passed=true`, `external_input_driven=true`, and `built_player=true`.
- [x] Run `AI Tools/Run M91 External Input Built Player Gate` and confirm the gate report passes.

Latest evidence:

- `Assets/Scripts/Tactical/TacticalExternalInputRouteTelemetry.cs`
- `Assets/Editor/M91ExternalInputBuiltPlayerGate.cs`
- `docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE_MISSION_CONTRACT_2026-05-18.md`
- `docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE_COMPLETION_2026-05-18.md`
- `docs/M91ExternalInputBuiltPlayerRoute/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json`
- `docs/M91ExternalInputBuiltPlayerRoute/`
- `docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_GATE.json`
- `docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_GATE.md`
- `docs/UNITY_MCP_EXECUTE_M91_GATE_PASS.json`

Important limitation:

- M91 does not close `full_visual_asset_gate_passed=false`.
- M91 does not close final weapon art, final humanoid art, or generated batch promotion blockers.
- The Editor gate must not pass unless the external-input route JSON already proves `passed=true`.

### M92: Weapon Production Pass / Weapon Feel Production Upgrade

Status: verified locally through Unity and pushed as commit `ca22c09`.

Primary lane: gameplay engineering / visual direction / evidence audit.

Goal:

- Move the current hero weapon from functional weapon-feel proof toward a more production-like first-person and third-person weapon experience.

Scope:

- first-person idle/movement sway;
- ADS stability;
- fire kick, side offset, roll, and recovery;
- reload pose magnitude;
- select/raise pose;
- third-person mount quality and fire pulse;
- M92-specific fields in `docs/WEAPON_FEEL_GATE.json` and the acceptance pipeline summary.

Forbidden changes:

- no scene edits;
- no package edits;
- no build artifacts;
- no M88 pass/fail edits;
- no M91 evidence edits;
- no generated batch class promotion.

Local verification checklist:

- [x] Apply `m92_weapon_production_pass.patch`.
- [x] Refresh Unity and confirm no compile errors.
- [x] Run `AI Tools/Run Weapon Feel Gate`.
- [x] Confirm `docs/WEAPON_FEEL_GATE.json` includes `m92_weapon_production_passed`.
- [x] Confirm first-person pose quality, recoil peak, reload pose magnitude, ADS stability, and third-person mount quality fields are present.
- [x] Run `AI Tools/Run Tactical Acceptance Pipeline`.
- [x] Confirm `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json` summarizes M92 fields.
- [x] Do not mark `full_visual_asset_gate_passed=true` unless the real strict gate proves it.

Verification evidence:

- `docs/WEAPON_FEEL_GATE.json`: `passed=true`, `m92_weapon_production_passed=true`, `shot_feedback_event_count=7`, `screenshot_count=4`.
- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`: `all_required_current_gates_passed=true`, `stage=WaitStoppedAfterGates`, `m92_weapon_production_passed=true`, `m92_recoil_peak_value=0.32`.
- Unity Console error readback: 0 errors after pipeline run.

Prepared docs:

- `docs/M92_WEAPON_PRODUCTION_PASS_MISSION_CONTRACT_2026-05-18.md`
- `docs/M92_WEAPON_PRODUCTION_PASS_COMPLETION_TEMPLATE_2026-05-18.md`

Important limitation:

- M92 is a scoped weapon production-feel patch. It is not final weapon art review, final humanoid review, or generated batch class promotion.

## Supporting Lanes

### S1: Unity MCP / Toolchain Reliability

Status: ongoing support lane.

Checklist:

- [ ] Keep one active Unity MCP bridge unless explicitly probing a copy/worktree.
- [ ] Maintain smoke report command.
- [ ] Distinguish editor readiness from external MCP transport.
- [ ] Document compile/reload failures.
- [ ] Do not hide MCP failures by doing manual report edits.

### S2: Agent Orchestration

Status: ongoing support lane.

Goal:

- Use Codex/Kimi/Gemini/MiniMax as mission owners, not tiny file workers.

Checklist:

- [ ] Use mission contracts for delegated work.
- [ ] Prefer Kimi/Gemini for complex code/research lanes.
- [ ] Use MiniMax only for simpler mechanical tasks.
- [ ] Use Gemini CLI with the intended high-capability model configuration.
- [ ] Require result JSON/summary/commands/artifacts from external agents.
- [ ] Do not accept agent claims without repo evidence.

### S3: Local AI 3D Asset Factory

Status: support lane feeding M84 and later asset work.

Checklist:

- [ ] Maintain no-paid-proxy discipline for large downloads unless explicitly approved.
- [ ] Keep Hunyuan3D/ComfyUI/Texture/PBR route status documented.
- [ ] Generate assets into quarantine first.
- [ ] Preserve source/provenance sidecars.
- [ ] Run Blender cleanup/LOD/material checks before Unity promotion.
- [ ] Never skip semantic review.
- [ ] Never skip gameplay binding/player-camera/event proof.

### S4: External Review / ChatGPT Pro Loop

Status: support lane.

Checklist:

- [ ] Keep README links current so external reviewers can index quickly.
- [ ] Ask Pro for strategic review only after repo docs/gates are current.
- [ ] Treat Pro answers as suggestions to verify, not orders.
- [ ] Convert useful advice into mission contracts or standards.
- [ ] Reject/defer advice that adds process weight without improving gates.

### S5: Skills And Playbooks

Status: expand only when repeated missions prove need.

Current skills:

- `unity-mcp-gate-runner`
- `html-parity-review`
- `asset-promotion-gate`
- `player-camera-visual-proof`
- `unity-compile-recovery`
- `game-feel-tuning`

Potential future skills:

- [ ] `building-integrity-gate`
- [ ] `ai-playtest-route`
- [ ] `asset-generation-brief`
- [ ] `post-mission-retro`
- [ ] `build-release-check`

Rule:

- Do not create a large skill library before the corresponding mission repeats.

## Completion Criteria For The Whole Track

The track is not done until:

- [x] M0.5 active Unity session verification passes.
- [x] M81 building integrity gate passes.
- [x] M82 weapon feel gate passes.
- [x] M83 AI playtest route passes.
- [x] M84 three-class asset factory spike passes.
- [x] M85 visual production pass improves player-camera realism without gate regression.
- [x] M86 build/release gate produces a launchable review packet or explicit blocker.
- [x] promoted asset visibility gate passes at class level through M87 approved-equivalent route evidence.
- [x] M88 strict full visual gate blocker audit exists.
- [x] M89 built-player foreground launch capture exists.
- [x] M90 built-player runtime gameplay route capture exists.
- [x] M91 external-input built-player route capture passes.
- [x] M92 weapon production pass is locally verified through Unity.
- [ ] `full_visual_asset_gate_passed=true`.
- [x] the player route can be externally or manually played with visible weapon, pickup, NPC combat, reload, building traversal, and restart.

## Immediate Next Action

Return to the M88 strict visual blockers: final weapon art review, final humanoid art review, and generated batch class promotion. M91 closed the external-input route evidence gap and M92 closed the scoped weapon-feel production pass, but neither closes final production visual quality.

### M93: Realified Batch Root Cause / Regeneration Contract

Status: root cause documented; corrected loot fallback assets generated; Unity contact-sheet/import evidence refreshed; local Nemotron semantic review passes for the corrected loot contact sheet; corrected ammo and medkit are now bound into gameplay and promoted from route evidence.

Primary lane: asset pipeline / visual direction / evidence audit.

Current finding:

- Realified non-weapon category contact sheets show weapon-shaped meshes under character and loot labels.
- This is not a Unity binding problem; it is an upstream asset-generation/category problem.
- See `docs/M93_REALIFIED_BATCH_ROOT_CAUSE_AND_REGENERATION_CONTRACT_2026-05-19.md`.

Completed executable slice:

- Corrected `loot` slice now runs through gameplay binding -> player-camera pickup evidence -> promotion ledger update from evidence only.

Checklist:

- [x] Document that the current Realified batch cannot be promoted by ledger edits.
- [x] Inspect the asset-generation/source-trace evidence for the mislabeled non-weapon assets.
- [x] Regenerate or replace one loot-class asset with category-correct geometry.
- [x] Import the corrected asset into Unity with material sidecars.
- [x] Refresh Unity contact-sheet evidence for the corrected loot class.
- [x] Run semantic contact-sheet review for the corrected loot class.
- [x] Bind corrected loot assets into gameplay.
- [x] Capture player-camera pickup evidence.
- [x] Update promotion ledger from evidence only.

Current evidence:

- Corrected generator script: `tools/m93_generate_corrected_loot_assets.py`
- Generation trace: `docs/M93_CORRECTED_LOOT_GENERATION_TRACE.json`
- Unity contact sheet: `Assets/Screenshots/RealifiedCategorySheets/realified_loot_contact_sheet.png`
- Contact-sheet report: `docs/REALIFIED_CATEGORY_CONTACT_SHEETS.json`
- Import/material gate: `docs/REALIFIED_IMPORT_MATERIAL_GATE.json`
- Local Nemotron semantic review: `docs/M93_CORRECTED_LOOT_NEMOTRON_REVIEW.json`
- Player route evidence: `docs/TACTICAL_PLAYABLE_ROUTE_GATE.json`
  - `realified_ammo_loot_route_evidence=true`
  - `realified_medkit_loot_route_evidence=true`
- Gameplay promotion ledger: `docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json`
  - `production_promoted_assets=3`
  - promoted assets include `hero_rifle`, `ammo`, and `medkit`
- Promoted asset visibility gate: `docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json`
  - `passed=true`
  - `production_promoted_classes=2`
  - `visible_promoted_classes=2`
- Completion note: `docs/M93_CORRECTED_LOOT_GAMEPLAY_PROMOTION_COMPLETION_2026-05-18.md`
