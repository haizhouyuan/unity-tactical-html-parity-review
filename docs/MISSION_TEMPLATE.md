# Mission Template

Date: 2026-05-18

Use this template for future Codex/Kimi/Gemini/MiniMax work. A mission is larger than a micro-task but smaller than an unbounded project rewrite.

The goal is to make every task auditable: what problem it solves, what it is allowed to change, what evidence proves it, and how to roll it back.

## Mission Contract

```yaml
mission_id: M000
title: ""
status: proposed | active | completed | blocked | rolled_back
primary_lane: creative_direction | technical_direction | gameplay_engineering | asset_pipeline | visual_direction | qa_playtest | evidence_audit | release_liveops
review_lane: ""

player_problem: ""
workflow_problem: ""

repo_ground_truth:
  files_to_read:
    - README.md
    - AGENTS.md
  current_gate_values:
    - ""
  known_failures:
    - ""

scope:
  - ""

out_of_scope:
  - ""

allowed_files:
  - ""

forbidden_changes:
  - Unity scenes unless explicitly required
  - Unity packages unless explicitly required
  - generated assets unless explicitly required
  - acceptance report values by hand
  - unrelated gameplay systems

allowed_tools:
  - Codex
  - Unity Editor
  - Unity MCP
  - other scoped tools

unity_menu_commands:
  - ""

evidence_required:
  json_reports:
    - ""
  screenshots:
    - ""
  console_status: required | not_required
  player_camera_required: true | false
  gameplay_event_required: true | false

definition_of_done:
  - ""

rollback_plan:
  - ""

follow_up_missions:
  - ""

final_report_must_include:
  - changed files
  - commands run
  - evidence paths
  - Console status if Unity was used
  - unresolved risks
```

## Mission Rules

1. Read `AGENTS.md` first.
2. Read the mission's listed ground-truth reports before editing.
3. Keep scope narrow.
4. Do not change files outside `allowed_files`.
5. Do not broaden into adjacent systems because they are visible.
6. Use Unity Editor APIs and `AI Tools/...` commands for scene/prefab operations.
7. Run the evidence gate named in the mission.
8. If the gate cannot run, write a blocker report and stop claiming completion.
9. Summarize changed files, commands, reports, screenshots, and unresolved risks.

## Standard Mission Types

### Docs-only mission

Allowed:

- `README.md`;
- `AGENTS.md`;
- `docs/*.md`;
- schema docs.

Forbidden:

- Unity scene changes;
- Unity package changes;
- runtime scripts;
- generated assets;
- report value edits.

Required checks:

- `git diff --check`;
- secret scan for obvious keys or private credentials;
- changed-file review.

### Unity gate mission

Allowed:

- scoped Editor scripts;
- scoped runtime scripts;
- generated JSON reports;
- screenshots.

Required checks:

- Unity compile/update idle;
- Console read before and after;
- exactly one primary gate command;
- no new Console compile errors.

### M0.5 active session verification mission

Allowed:

- generated readiness reports;
- command logs;
- no gameplay/script/scene/package changes.

Required checks:

- active Unity project is this checkout;
- compile/update idle;
- Console read through MCP or documented fallback;
- `AI Tools/Run Tactical Preflight` runs;
- `docs/TACTICAL_PREFLIGHT_REPORT.json` exists;
- `AI Tools/Run Unity MCP Smoke Check` runs;
- `docs/UNITY_MCP_SMOKE_REPORT_LATEST.json` exists.

If this mission fails, do not proceed to M81/M82. Repair Unity/MCP session state first.

### Gameplay mission

Allowed:

- scoped gameplay scripts;
- scene/prefab changes only through Unity/Editor tooling;
- route/gate report updates.

Required evidence:

- player-camera proof;
- state mutation proof;
- JSON gate proof.

### Asset mission

Allowed:

- asset candidate files;
- import/material reports;
- semantic review reports;
- promotion ledger updates;
- gameplay binding code only if explicitly scoped.

Required evidence:

- import success;
- material/PBR status;
- category semantic review;
- player-camera visibility if promotion is claimed;
- gameplay event if production promotion is claimed.

## Closeout Format

Use this closeout shape:

```text
Mission: M000 <title>
Status: success | partial | blocked | failed

Changed files:
- ...

Commands / Unity menu items:
- ...

Evidence:
- ...

What is proven:
- ...

What is not proven:
- ...

Risks:
- ...

Next mission:
- ...
```

## Anti-Patterns

Avoid these:

- "I made it better" without JSON or screenshot evidence.
- counting a GLB because it exists;
- counting a screenshot because it is pretty;
- editing acceptance reports manually;
- running many MCP commands while Unity is compiling;
- adding tools or packages in a gameplay mission;
- creating a large architecture before the next playable gate.
- saying a repo-level static change was Unity-verified when the active Unity menu did not run.
