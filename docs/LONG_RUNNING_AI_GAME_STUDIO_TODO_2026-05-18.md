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
- `full_visual_asset_gate_passed=false`
- `category_semantic_review_passed=false`
- `realified_asset_gameplay_production_promoted_assets=1`
- `promoted_asset_visibility_gate_passed=false`

Interpretation:

- Gameplay/current route evidence exists.
- HTML parity evidence is useful but not final visual completion.
- Production visual assets are still the main unfinished goal.
- M0 docs are complete.
- Next executable work starts at M0.5.

## Master Queue

### M0.5: Active Unity Session Verification

Status: blocked on 2026-05-18 because the active Unity session is `/Users/yuanshaochen/My project`, not this public review checkout.

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
- [ ] Confirm `Assets/Editor/TacticalWorkflowTools.cs` is imported.
- [ ] Run `AI Tools/Run Tactical Preflight`.
- [ ] Confirm `docs/TACTICAL_PREFLIGHT_REPORT.json` exists.
- [ ] Run `AI Tools/Run Unity MCP Smoke Check`.
- [ ] Confirm `docs/UNITY_MCP_SMOKE_REPORT_LATEST.json` exists.
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
- MCP transport reachable; active project mismatch prevents completion.

### M0.5R: Unity Compile/Reload Recovery

Status: next.

Goal:

- Recover from stale Unity editor AppDomain, missing menu items, compile lock, or MCP session mismatch.

Checklist:

- [ ] Stop polling after 2 minutes of compile/reload lock.
- [ ] Save if Unity allows it.
- [ ] Restart Unity on this repo checkout.
- [ ] Wait for compile/update idle.
- [ ] Read Console.
- [ ] Re-run only `AI Tools/Run Tactical Preflight`.
- [ ] Re-run only `AI Tools/Run Unity MCP Smoke Check`.
- [ ] Write blocker report if the commands remain unavailable.

Definition of done:

- M0.5 passes, or a blocker report names the exact failed layer: Unity project path, compile errors, menu import, MCP connection, or command execution.

### M81: Building Integrity Gate

Status: todo after M0.5.

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

- [ ] Create or update a mission contract for M81.
- [ ] Add `AI Tools/Run Building Integrity Gate` if missing.
- [ ] Gate starts player outside a building.
- [ ] Gate proves player can enter first floor.
- [ ] Gate proves player can reach a valid second-floor/upper-floor landing.
- [ ] Gate records floor/support height transitions.
- [ ] Gate verifies balcony/fall/support behavior.
- [ ] Gate verifies door/collider blockers do not block valid entry.
- [ ] Gate verifies container/cover blocks NPC line-of-sight or shots.
- [ ] Gate emits JSON report.
- [ ] Gate emits or references player-camera/debug screenshot evidence.
- [ ] Tactical acceptance pipeline still passes.

Definition of done:

- Building integrity JSON passes.
- Player route is not broken.
- Cover/collision proof exists.
- No new Console compile errors.

### M82: Weapon Feel Gate

Status: todo after M81 or parallel only if M81 is blocked by Unity session/tooling.

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

- [ ] Create or update a mission contract for M82.
- [ ] Add `AI Tools/Run Weapon Feel Gate` if missing.
- [ ] Prove first-person weapon visible at spawn.
- [ ] Prove fire event changes ammo and spawns feedback.
- [ ] Prove reload state mutation.
- [ ] Prove hit marker or enemy damage event.
- [ ] Prove recoil/camera/weapon motion is visible or reportable.
- [ ] Prove muzzle flash/tracer/casing/audio hooks exist or explicitly report missing.
- [ ] Prove third-person weapon mount does not intersect torso.
- [ ] Capture player-camera screenshots for first-person and third-person weapon states.
- [ ] Update route/gate evidence without hand-editing report values.

Definition of done:

- Weapon Feel Gate passes for the current hero weapon.
- At least one player-camera screenshot shows correct weapon visibility.
- No static showcase proof is counted as gameplay proof.

### M83: AI Playtest Route

Status: todo after M81 and M82 have stable primitives.

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

- [ ] Create mission contract for M83.
- [ ] Add `AI Tools/Run AI Playtest Route` if missing.
- [ ] Simulate or script deterministic player route.
- [ ] Record positions, state transitions, inventory, weapon state, enemy state.
- [ ] Capture player-camera screenshots at route milestones.
- [ ] Write route JSON with pass/fail and failure taxonomy.
- [ ] Detect stuck/no-progress conditions.
- [ ] Detect missing weapon/loot/enemy visibility.
- [ ] Detect blocked pickup/fire/reload.
- [ ] Run acceptance pipeline after playtest route.

Definition of done:

- AI playtest route JSON exists and passes.
- The route can fail with useful reasons when a core action breaks.
- It does not depend on fixed showcase cameras.

### M84: Three-Class Asset Factory Spike

Status: todo after M0.5, but gameplay binding steps should wait for M81/M82/M83 primitives where relevant.

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

- [ ] Create mission contract for M84.
- [ ] For each class, record source/provenance and intended semantic class.
- [ ] Import into Unity.
- [ ] Validate material/PBR state.
- [ ] Run semantic review.
- [ ] Bind to gameplay entity only if semantic-ready.
- [ ] Prove player-camera visibility.
- [ ] Prove gameplay event:
  - weapon fires/reloads/hits;
  - container blocks line-of-sight/shots;
  - medkit is picked up or mutates state.
- [ ] Update promotion ledger.
- [ ] Update promoted asset player-camera visibility gate.

Definition of done:

- One weapon, one environment prop, and one loot item reach production-promoted status.
- No candidate-only asset receives completion credit.

### M85: Visual Production Pass

Status: todo after M81-M84 produce honest gates.

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

- [ ] Create mission contract for M85.
- [ ] Capture before screenshots from player route.
- [ ] Define visual style guide for small tactical slice.
- [ ] Add scoped decals/clutter/material variation.
- [ ] Tune lighting/postprocess without hiding asset defects.
- [ ] Capture after screenshots from same route.
- [ ] Run AI/human visual review.
- [ ] Run acceptance pipeline.

Definition of done:

- Before/after player-camera evidence exists.
- Gameplay route still passes.
- Full visual asset gate improves without hiding failed categories.

### M86: Build / Release Gate

Status: todo after route/visual gates stabilize.

Primary lane: release/liveops.

Goal:

- Produce a reviewable build/release packet rather than only editor evidence.

Checklist:

- [ ] Create mission contract for M86.
- [ ] Choose build target.
- [ ] Run preflight.
- [ ] Build in Unity or document blocker.
- [ ] Run launch smoke test.
- [ ] Capture build screenshots/video.
- [ ] Write changelog.
- [ ] Update README current evidence snapshot.
- [ ] Record known limitations.

Definition of done:

- Build artifact exists or blocker is explicit.
- Build can launch.
- Release notes include current gate truth.

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

- [ ] M0.5 active Unity session verification passes.
- [ ] M81 building integrity gate passes.
- [ ] M82 weapon feel gate passes.
- [ ] M83 AI playtest route passes.
- [ ] M84 three-class asset factory spike passes.
- [ ] M85 visual production pass improves player-camera realism without gate regression.
- [ ] M86 build/release gate produces a launchable review packet or explicit blocker.
- [ ] `full_visual_asset_gate_passed=true`.
- [ ] promoted asset visibility gate passes at class level.
- [ ] the player route can be manually played with visible weapon, pickup, NPC combat, reload, building traversal, and restart.

## Immediate Next Action

Run M0.5. If M0.5 is blocked, run M0.5R before touching gameplay.
