# AI Game Dev OS Strategy Triage

Date: 2026-05-18

This document evaluates the broader ChatGPT Pro recommendation to treat the Unity tactical remake as an `AI Game Development Operating System` experiment rather than a one-off Unity bug-fix project.

The recommendation is directionally useful, but it should not be adopted as a large new architecture all at once. The current repository already contains several of the proposed pieces: hardened `AGENTS.md` rules, project-local Codex skills, deterministic Unity `AI Tools/...` menu commands, acceptance reports, HTML parity evidence, asset promotion ledgers, MCP reliability notes, and public review documentation. The right next move is to formalize the OS concept in a small docs-only mission, then return to the playable slice gates.

## Current Ground Truth

Current exported evidence:

- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`
  - `all_required_current_gates_passed=true`
  - `full_visual_asset_gate_passed=false`
  - `category_semantic_review_passed=false`
  - `realified_asset_gameplay_production_promoted_assets=1`
  - `promoted_asset_visibility_gate_passed=false`
- `docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json`
  - 12 assets checked.
  - 3 technical/semantic/scene partial assets.
  - 1 production-promoted asset.
- `docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json`
  - `production_promoted_classes=0`
  - `visible_promoted_objects=0`
  - `candidate_visible_objects=103`
  - `blocked_reason=no_production_promoted_assets`
- `.codex/skills/`
  - 6 focused project-local skills already exist:
    - `unity-mcp-gate-runner`
    - `html-parity-review`
    - `asset-promotion-gate`
    - `player-camera-visual-proof`
    - `unity-compile-recovery`
    - `game-feel-tuning`
- `AGENTS.md`
  - already states tactical parity rules, MCP discipline, evidence gates, asset promotion rules, and Unity file safety.
- `docs/COMMUNITY_UNITY_MCP_AI_GAMEDEV_PLAYBOOK_2026-05-17.md`
  - already states the correct operating loop:
    `mission packet -> code/file edits -> Unity Editor menu command -> MCP execute/read_console -> JSON/screenshots -> semantic review -> promote/quarantine`.

## What Pro Got Right

### 1. The project is larger than a Unity remake

Adopt this. The most valuable framing is not "make one FPS prototype." The project is an experiment in whether AI can help move from:

```text
player idea
  -> design interpretation
  -> prototype
  -> generated assets
  -> Unity integration
  -> gameplay proof
  -> visual/semantic review
  -> release or rollback
  -> next player feedback
```

That is the right north star. It also explains why a pretty contact sheet or a single passed Unity gate is not enough.

### 2. MCP should be transport, not the brain

Already adopted. The current repo instructions and playbook agree with this. The local lesson is stronger than the abstract advice: Unity is stateful, compile/reload can hide menu commands, and long chains of tiny MCP edits are fragile.

The stable interface should remain:

```text
Codex/Kimi/Gemini writes code or Editor tool
  -> Unity deterministic AI Tools menu command
  -> MCP only runs menu/read_console/read_report
  -> JSON/screenshot evidence
```

### 3. Asset generation must become production promotion

Adopt. The repo has already proven the trap: many assets can exist on disk and render in contact sheets while still failing category correctness or gameplay promotion.

The hard standard should remain:

```text
generated/imported GLB
  -> material/PBR validation
  -> semantic class review
  -> gameplay binding
  -> player-camera visibility
  -> gameplay event participation
  -> ledger promotion
```

### 4. Playtest and evidence loops matter as much as code

Adopt. The next practical tools should be gates, not broad rewrites:

- `AI Tools/Run AI Playtest Route`
- `AI Tools/Run Weapon Feel Gate`
- `AI Tools/Run Building Integrity Gate`

These should emit JSON and screenshots, not just Console logs.

## What Is Already Done

The Pro answer proposes several things that are not missing from zero.

| Proposed concept | Current repo state | Decision |
| --- | --- | --- |
| Strong project instructions | `AGENTS.md` already contains Unity file safety, tactical parity rules, MCP discipline, evidence gates, asset promotion rules. | extend later, do not rewrite blindly |
| Codex skills | 6 focused skills already exist under `.codex/skills/`. | expand gradually only after usage proves need |
| MCP playbook | `docs/COMMUNITY_UNITY_MCP_AI_GAMEDEV_PLAYBOOK_2026-05-17.md` already defines the stable loop. | keep as operating doctrine |
| Evidence gates | Tactical route, gameplay proof, HTML parity, promotion, and material reports already exist. | strengthen visual gates, do not duplicate |
| Asset promotion standard | Promotion ledger already distinguishes candidate, partial, and production-promoted assets. | formalize in one standard doc |
| Public review packet | `README.md`, `DEVELOPMENT_RECORD.md`, and multiple docs already expose current state and known gaps. | keep public honesty |
| Local GUI/VLM tooling | Local UI-TARS/Nemotron-style quality gate infrastructure exists outside this repo but must be re-verified before relying on it. | treat as external infrastructure, not assumed-current repo capability |

## What Should Be Adapted

### 1. Sixteen skills are too many for the next step

The proposed 16 skills are a good responsibility map, but implementing all of them now would create process weight before the core playable slice is honest.

Recommended expansion path:

1. Keep current 6 skills.
2. Add new skills only when a mission repeats at least twice.
3. Next likely additions:
   - `ai-playtest-route`
   - `building-integrity-gate`
   - `asset-generation-brief`
   - `post-mission-retro`

### 2. Eight agent lanes should be roles, not fake org chart

The lane model is useful for assigning responsibility:

- Creative direction
- Technical direction
- Gameplay engineering
- Asset pipeline
- Visual direction
- QA/playtest
- Evidence audit
- Release/liveops

But do not create eight always-on agents yet. Use lane labels in mission contracts, then dispatch to Kimi/Gemini/Codex/MiniMax only when a concrete mission needs that role.

### 3. External AI asset APIs should be research lanes, not core dependency

Meshy, Tripo, Hyper3D, Scenario, and similar tools may be useful, but this project has local-first and cost/proxy constraints. They should be treated as optional probes until licensing, cost, output quality, Unity import behavior, and promotion gates are proven.

Immediate priority is still local/generated or hand-authored category-correct assets that pass Unity player-camera evidence.

### 4. ML-Agents or modl.ai-style playtesting is a later phase

The idea is right, but the first step should be deterministic PlayMode/editor route gates:

```text
spawn -> move -> enter building -> climb -> pickup -> fire -> hit NPC -> reload -> restart
```

Only after that route is stable should ML-Agents, external screen-based agents, or learned bots be considered.

### 5. Runtime LLM NPCs are premature

Do not add conversational/autonomous runtime NPCs yet. The game still needs stable tactical basics: movement, weapon feel, cover, building collision, loot, enemy damage, and player-camera evidence.

Use AI for development, review, barks, design iteration, or offline tooling before adding player-facing runtime LLM behavior.

## What Should Be Deferred Or Rejected

| Idea | Decision | Reason |
| --- | --- | --- |
| Build a 49-agent virtual studio | reject for now | Adds coordination overhead before playable slice and gates are strong enough. |
| Implement all 16 proposed skills immediately | defer | Current 6 skills already cover the active workflow. Add based on repeated mission need. |
| Full AI Game Dev OS implementation before gameplay repair | defer | Docs-only OS framing is useful; broad architecture before weapon/building/playtest gates would slow delivery. |
| Directly count generated assets as final game content | reject | Current reports prove this creates false completion. |
| Runtime LLM NPCs now | defer | Base NPC combat and movement must be stable first. |
| Add multiple MCP bridges to active Unity project | reject unless Coplay fails | Existing playbook says one active bridge; multiple bridges increase state/debug risk. |

## Recommended Decision

Choose a small `M0 Docs-Only OS Pass` before more Unity implementation.

This is not a new architecture build. It is a controlled documentation pass that gives future Codex/Kimi/Gemini missions a shared operating contract.

Recommended M0 deliverables:

- `docs/AI_GAME_STUDIO_OS.md`
- `docs/AI_CLOSED_LOOP_PIPELINE.md`
- `docs/PLAYER_INTENT_LOG.md`
- `docs/MISSION_TEMPLATE.md`
- `docs/ASSET_PROMOTION_STANDARD.md`

Constraints:

- No Unity scene changes.
- No package installation.
- No new gameplay code.
- No broad rewrite of current reports.
- Keep changed files reviewable.

After M0, return immediately to concrete gates:

1. `M81 Building Integrity Gate`
   - door entry, floor support, ladder landing, balcony/fall behavior, collision blockers, NPC line-of-sight through cover.
2. `M82 Weapon Feel Gate`
   - first-person visibility, ADS, recoil, muzzle flash, casing, hit marker, reload, ammo state, audio hooks.
3. `M83 AI Playtest Route`
   - deterministic route proof from lobby/start through movement, building, pickup, fire, NPC, restart.
4. `M84 Asset Factory Spike`
   - only 3 classes first: one weapon, one environment prop, one loot item; each must pass the promotion standard.

## Practical Mission Ordering

```text
M0 Docs-only AI Game Studio OS
  -> M81 Building Integrity Gate
  -> M82 Weapon Feel Gate
  -> M83 AI Playtest Route
  -> M84 Three-class Asset Factory Spike
  -> M85 Visual Production Pass
  -> M86 Build/Release Gate
```

This sequence keeps the system honest:

- M0 prevents future agent drift.
- M81/M82/M83 prove the game can actually be played.
- M84 prevents asset factory false completion.
- M85 improves visuals after functionality and promotion standards are stable.
- M86 packages a verified build rather than a screenshot collection.

## My Current Judgment

The broad Pro answer is valuable as a strategic reframing. It should become a north-star document and mission framework, not a reason to pause the playable Unity work for a large process build.

The project has already moved beyond "random AI edits" into an evidence-gated Unity workflow. The biggest remaining gap is not lack of framework; it is that the strongest gates still say the visual asset goal is open:

- gameplay/current gates pass;
- visual asset gate fails;
- semantic category review fails;
- production-promoted gameplay assets are still too few;
- player-camera promoted visibility is not yet passing.

Therefore the next best action is:

1. write the small OS docs so future agents share the same rules;
2. immediately resume gate-driven playable work, especially building integrity, weapon feel, AI playtest route, and narrow asset promotion.

Do not broaden into a virtual studio before the game itself proves the next playable slice.
