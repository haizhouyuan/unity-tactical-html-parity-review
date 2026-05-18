# AI Game Studio OS

Date: 2026-05-18

This project is not only a Unity tactical prototype. It is an AI-native game development closed-loop experiment using the tactical/FPS slice as the testbed.

The goal is to make AI work auditable across the full loop:

```text
player idea
  -> design interpretation
  -> mission packet
  -> implementation
  -> Unity integration
  -> gameplay proof
  -> visual and semantic review
  -> promote, quarantine, or rollback
  -> next player feedback
```

This document defines the operating model for future Codex/Kimi/Gemini/MiniMax missions. It does not declare the game complete.

## Current Repository Role

The current public repo is a review/export repository for a Unity recreation of an HTML/Three.js tactical prototype.

The repo already has several operating-system components:

- `AGENTS.md` with project rules.
- `.codex/skills/` with focused project skills.
- `Assets/Editor/TacticalWorkflowTools.cs` with report-oriented Unity menu commands.
- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json` and related gate reports.
- Asset promotion ledgers and visibility gates.
- HTML baseline packet under `reference/html_baseline_final_packet/`.

This OS document does not replace those artifacts. It defines how future work should use them together.

## Current Product Truth

The current Unity project has useful route evidence, but the production visual goal is still open.

Current gate snapshot:

- `all_required_current_gates_passed=true`
- `full_visual_asset_gate_passed=false`
- `category_semantic_review_passed=false`
- `realified_asset_gameplay_production_promoted_assets=1`
- `promoted_asset_visibility_gate_passed=false`

Interpretation:

- The Unity route is testable.
- The HTML tactical parity work has a working evidence chain.
- Production-quality visual asset replacement is not complete.
- Generated/imported assets must not receive completion credit until promoted by the asset standard.

## North Star

The north star is:

```text
AI can help a parent and child move from an idea to a playable, reviewable Unity tactical slice through repeatable missions and evidence gates.
```

This means the system optimizes for:

- playable proof over impressive screenshots;
- player-camera evidence over contact sheets;
- mission contracts over vague prompts;
- deterministic Unity menu commands over fragile GUI clicking;
- asset promotion ledgers over filename-based claims;
- small, reviewable diffs over large uncontrolled rewrites.

## Operating Lanes

These lanes are responsibility labels for mission contracts. They are not permanent agents and should not be turned into a large virtual organization until the workflow proves it needs that scale.

| Lane | Responsibility | Typical outputs |
| --- | --- | --- |
| Creative Direction | Preserve player fantasy, child/parent intent, tone, and game identity. | GDD updates, intent interpretation, mission priorities. |
| Technical Direction | Keep Unity architecture, MCP workflow, project safety, and gate design coherent. | editor tools, report schemas, workflow rules. |
| Gameplay Engineering | Implement movement, weapons, loot, NPCs, health, armor, HUD, route behavior. | scripts, prefabs, gameplay reports. |
| Asset Pipeline | Generate, import, validate, and promote assets. | GLBs, material reports, promotion ledger updates. |
| Visual Direction | Improve readability, realism, lighting, materials, camera framing, screenshots. | style guides, visual gates, before/after evidence. |
| QA / Playtest | Prove route playability and find regressions. | AI playtest reports, screenshots, failure taxonomy. |
| Evidence Audit | Judge whether claims are supported. | pass/fail summaries, incomplete evidence notes. |
| Release / LiveOps | Package, changelog, public review packet, build sanity. | release notes, build reports, feedback loop entries. |

Mission contracts should select one primary lane and, if necessary, one reviewer lane.

## Core Rules

1. Do not claim completion from a screenshot alone.
2. Do not claim production asset completion from a GLB existing on disk.
3. Do not count contact-sheet visibility as gameplay integration.
4. Do not hand-edit large `.unity` or `.prefab` YAML unless explicitly scoped and justified.
5. Do not install new Unity packages during gate missions unless the mission is specifically about package evaluation.
6. Do not use multiple Unity MCP bridges in the active project unless the current active bridge is blocked and the mission is a controlled probe.
7. Do not broaden a mission into unrelated systems.
8. Do not overwrite acceptance report values by hand.
9. Every significant gameplay or asset claim needs JSON evidence and, where visual, player-camera evidence.

## Operating Principles

### Player Intent Is First-Class Data

Player feedback should not be treated as disposable chat context. It should be captured, interpreted, linked to a mission, and closed with evidence.

Examples:

- "The gun looks wrong" becomes a weapon visual fidelity mission.
- "The player cannot enter the first floor" becomes a building integrity mission.
- "NPC bullets appear to pass through containers" becomes a cover/line-of-sight proof mission.
- "This weapon should match a reference image" becomes an asset reference and semantic review mission.

### Mission Scope Beats Broad Autonomy

Every AI implementation pass must have scope, out-of-scope items, allowed files, forbidden changes, evidence requirements, and rollback notes.

Do not mix these in one mission:

- workflow/process changes;
- gameplay changes;
- asset generation/import;
- asset promotion;
- visual polish;
- build/release changes.

If a mission discovers a larger issue, record a follow-up mission instead of broadening the current mission.

### HTML Parity Is Not Visual Completion

HTML tactical parity passing means the Unity prototype covers expected tactical behaviors. It does not mean production visual quality is complete.

Future agents must preserve this distinction:

- tactical/current gates may pass;
- full visual asset gate may still fail;
- semantic category review may still fail;
- production-promoted gameplay assets may still be too few.

### Evidence Is The Unit Of Completion

Each mission should leave behind reviewable evidence:

- JSON report;
- screenshot or video path when visual/gameplay proof is required;
- Console status if Unity was used;
- menu command run;
- changed files;
- remaining risks.

## Active Workflow

The preferred workflow is:

```text
Mission template
  -> repo ground-truth read
  -> small code/editor-tool change
  -> Unity AI Tools menu command
  -> MCP read_console/read_report
  -> JSON/screenshot evidence
  -> audit
  -> commit or rollback
```

MCP is a transport layer, not the design brain. It should run short, deterministic editor commands and read state. Long chains of tiny MCP GameObject edits are fragile and should be avoided.

## Phase Plan

### M0: Docs-only OS pass

Purpose:

- make future missions consistent;
- establish standards before more gameplay/asset work.

Allowed:

- docs and README.

Forbidden:

- Unity scene edits;
- package changes;
- gameplay scripts;
- generated assets;
- rewritten gate values.

### M0.5: Active Unity session verification

Purpose:

- prove the active Unity editor session can see the current repo scripts and execute the current `AI Tools/...` menu commands.

Required proof:

- Unity compile/update idle;
- Console read;
- `AI Tools/Run Tactical Preflight` executed;
- `docs/TACTICAL_PREFLIGHT_REPORT.json` exists;
- `AI Tools/Run Unity MCP Smoke Check` executed;
- `docs/UNITY_MCP_SMOKE_REPORT_LATEST.json` exists.

If M0.5 fails, stop gameplay work and repair Unity/MCP/editor session state first.

### M81: Building Integrity Gate

Purpose:

- prove the player can navigate the tactical building and that cover/collision behaves correctly.

### M82: Weapon Feel Gate

Purpose:

- prove first-person and third-person weapon visibility, firing, reload, recoil, hit feedback, and ammo state.

### M83: AI Playtest Route

Purpose:

- run a deterministic route from lobby/start through movement, building entry, pickup, firing, NPC interaction, reload, death/restart or equivalent route completion.

### M84: Three-class Asset Factory Spike

Purpose:

- promote exactly one weapon, one environment prop, and one loot item through the full asset standard before scaling generation.

## Definition Of A Valid Mission

A valid mission must include:

- `mission_id`;
- lane;
- player problem or workflow problem;
- repo ground truth;
- scope and out of scope;
- allowed and forbidden files;
- Unity menu commands to run, if any;
- evidence required;
- rollback plan;
- definition of done.

Use `docs/MISSION_TEMPLATE.md` for the exact template.

## Definition Of Progress

Progress is not measured by number of generated files. Progress is measured by:

- stronger gates;
- fewer false completion paths;
- player-camera verified gameplay;
- promoted assets that are tied to real gameplay entities;
- repeatable Unity menu commands;
- clean reports that future agents can read without guessing.

## Related Documents

- `docs/AI_CLOSED_LOOP_PIPELINE.md`
- `docs/PLAYER_INTENT_LOG.md`
- `docs/MISSION_TEMPLATE.md`
- `docs/ASSET_PROMOTION_STANDARD.md`
- `docs/AI_GAME_DEV_OS_STRATEGY_TRIAGE_2026-05-18.md`
