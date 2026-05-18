# AI Closed-Loop Pipeline

Date: 2026-05-18

This document defines the operational loop for AI-assisted game development in this repository.

The loop exists to prevent the recurring failure mode where an agent creates files, screenshots, or candidate assets but the actual player route remains incomplete.

## Pipeline Summary

```text
1. Player feedback
2. Intent capture
3. Design interpretation
4. Mission contract
5. Implementation or asset generation
6. Unity editor command
7. MCP transport execution
8. JSON report and screenshot/video evidence
9. Semantic and visual review
10. Promote, quarantine, or rollback
11. Retro and next mission
```

No step may claim completion by skipping the evidence stage.

## Step 1: Player Feedback

Inputs:

- child/player comments;
- parent direction;
- manual playtest notes;
- reviewer findings;
- agent audit reports.

Output:

- an entry in `docs/PLAYER_INTENT_LOG.md` or a mission-specific intent log.

Rule:

- preserve the original complaint or desire where safe, then add an interpreted need.

## Step 2: Intent Capture

Classify the input:

- `gameplay`: movement, shooting, pickup, enemy combat, ladder, death, restart.
- `visual`: weapons, characters, buildings, trees, lighting, material quality.
- `UX`: HUD, menus, prompts, settings, feedback.
- `bug`: broken route, invisible gun, missing asset, wrong spawn, blocked door.
- `toolchain`: MCP, Unity compile, asset import, agent orchestration.
- `asset`: generation, import, material, semantic class, promotion.
- `release`: build, packaging, public review, changelog.

Each captured intent should name required evidence.

## Step 3: Design Interpretation

Before implementation, decide whether the request is:

- a bug;
- a gameplay design change;
- a visual fidelity request;
- a toolchain/process improvement;
- a reference asset request;
- a deferred idea.

Design interpretation prevents the agent from turning every request into broad code changes.

## Step 4: Mission Contract

Every non-trivial task becomes a mission contract using `docs/MISSION_TEMPLATE.md`.

The mission must state:

- the player problem;
- the repo ground truth;
- what is in scope;
- what is explicitly out of scope;
- what files can be changed;
- what files must not be changed;
- which Unity menu commands will be run;
- what JSON and screenshot evidence is required;
- the rollback plan.

## Step 5: Implementation Or Asset Generation

Implementation should be narrow:

- prefer small runtime scripts;
- prefer Editor tools over hand-editing scene YAML;
- prefer deterministic menu commands over long MCP command chains;
- update schemas before reports only when the mission requires it;
- do not change report values manually.

## Step 6: Unity Editor Command

Unity operations should be exposed as `AI Tools/...` menu commands.

Examples:

- `AI Tools/Run Tactical Preflight`
- `AI Tools/Run Tactical Acceptance Pipeline`
- `AI Tools/Run Game Feel Evidence Gate`
- `AI Tools/Capture Tactical Verified Player POV Screenshot`
- future: `AI Tools/Run Building Integrity Gate`
- future: `AI Tools/Run AI Playtest Route`

Rules:

- run one deterministic command at a time;
- wait for compile/update idle before invoking commands;
- if a newly added menu is invisible, treat the active Unity session as stale and run M0.5 recovery, not more gameplay work.

## Step 7: Gate Execution

Choose the smallest relevant gate.

Examples:

- workflow readiness: `AI Tools/Run Tactical Preflight`;
- MCP readiness: `AI Tools/Run Unity MCP Smoke Check`;
- building problems: future `AI Tools/Run Building Integrity Gate`;
- weapon feel: future `AI Tools/Run Weapon Feel Gate`;
- playability route: future `AI Tools/Run AI Playtest Route`;
- asset proof: promoted asset visibility/material/import gates.

Do not run broad gates to hide a narrow failure. Narrow gates should fail loudly when their exact responsibility is not met.

## Step 8: MCP Transport Execution

MCP should perform short transport operations:

- initialize/list tools;
- read Console;
- execute a known menu item;
- read generated report;
- optionally capture or reference screenshots.

MCP should not be used as a long-lived design loop that makes dozens of tiny GameObject edits.

## Step 9: Evidence

Required evidence depends on mission type.

Gameplay missions:

- route JSON;
- player-camera screenshot;
- state deltas such as ammo, health, inventory, NPC damage, pickup count.

Asset missions:

- import report;
- material/PBR report;
- semantic review;
- gameplay binding report;
- player-camera visibility screenshot;
- gameplay event report.

Toolchain missions:

- command log;
- MCP smoke report;
- Console status;
- clear failure reason if blocked.

Documentation missions:

- changed docs;
- `git diff --check`;
- secret scan;
- no Unity scene/package/script changes.

Evidence should live in predictable paths when possible:

```text
docs/gates/
Assets/Screenshots/
docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json
docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json
```

## Step 10: Semantic And Visual Review

Generated or imported assets need review beyond import success.

Review questions:

- Does the object visually match its declared category?
- Is it visible from the player camera?
- Is it bound to a real gameplay entity?
- Does it participate in an event?
- Is it being counted because of evidence, not because of filename or contact sheet?

If semantic review fails, the asset goes to quarantine or remains a candidate.

## Step 11: Promote, Quarantine, Or Roll Back

Promotion requires the full `docs/ASSET_PROMOTION_STANDARD.md`.

Outcomes:

- `promote`: all gates pass, ledger updated.
- `quarantine`: asset exists but cannot count toward completion.
- `rollback`: change regresses gameplay, scene, console, or gate.
- `defer`: idea is valid but needs a later mission.

## Step 12: Retro And Next Mission

Every mission closeout should answer:

- What changed?
- What evidence proves it?
- What remains unproven?
- Which false completion path was prevented?
- What is the next smallest useful mission?

Do not let lessons remain inside chat only. Add durable notes when the lesson affects future agents.

## Failure Handling

If Unity is compile/reload locked:

1. stop polling after 2 minutes;
2. save if possible;
3. restart Unity or ask for a clean restart;
4. wait for compile/update idle;
5. read Console;
6. run exactly one gate.

If MCP fails:

1. distinguish editor readiness from external MCP transport;
2. run local static validation if possible;
3. write a blocker report instead of pretending the gate passed.

If assets look wrong:

1. do not bind them to gameplay;
2. mark semantic/category failure;
3. preserve them as candidates only if useful for diagnostics.

## Anti-Patterns

### Fake Completion By Existence

Bad:

```text
The GLB exists, so the asset is done.
```

Good:

```text
The GLB is imported, semantically correct, gameplay-bound, visible from player camera, and participates in a gameplay event.
```

### Fake Completion By Screenshot

Bad:

```text
The contact sheet looks good, so the asset is production-ready.
```

Good:

```text
The asset appears in actual gameplay from the player camera and the visibility gate records the screenshot.
```

### Broadening A Mission Mid-Flight

Bad:

```text
While fixing the ladder, also rewrite the gun system and UI.
```

Good:

```text
Record weapon/UI issues as follow-up missions and keep the ladder mission isolated.
```

## Done Definition

A closed-loop mission is done only when:

- the scoped implementation is complete;
- Unity Console has no new compile errors, if Unity was touched;
- scene/prefab changes are saved, if any;
- required JSON reports exist;
- required screenshots exist for visual/player-camera claims;
- asset promotion claims trace to gameplay events;
- changed files are reviewable;
- unresolved risks are listed.
