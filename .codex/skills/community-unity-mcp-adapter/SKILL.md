---
name: community-unity-mcp-adapter
description: Use when Codex operates this Unity project through the non-official CoplayDev/community Unity MCP bridge; maps community MCP tools/resources to the repo's deterministic AI Tools gate workflow and prevents assumptions from official Unity MCP.
---

# Community Unity MCP Adapter

This skill is for this repository's current Unity MCP setup when the active bridge is the community `com.coplaydev.unity-mcp` / MCP for Unity bridge, not the official Unity AI Assistant MCP bridge.

Use this skill when a mission mentions any of:

- Unity MCP
- CoplayDev
- community MCP
- MCP for Unity
- `execute_menu_item`
- `read_console`
- `mcpforunity://`
- Unity active session verification
- Unity gate execution through Codex

## Purpose

Codex must not assume the official Unity MCP tool surface.

The official Unity MCP and CoplayDev/community MCP have similar goals but different operational details. This repository should use MCP as transport only:

```text
Codex writes or reviews code/docs
  -> Unity deterministic AI Tools menu command
  -> community MCP executes exactly that command
  -> community MCP reads Console/resources
  -> Codex reads JSON/screenshot evidence
  -> Codex reports pass/fail and risks
```

## Tooling Assumptions

When using the community bridge, prefer these concepts and names:

- Resource-first discovery:
  - `mcpforunity://editor/state`
  - `mcpforunity://project/info`
  - `mcpforunity://scene/gameobject-api`
  - `mcpforunity://instances`
  - `mcpforunity://scene/cameras`
- Read problems:
  - `read_console`
- Run deterministic repo tools:
  - `execute_menu_item`
- Screenshots:
  - `manage_camera(action="screenshot", ...)`
- Test jobs:
  - `run_tests`
  - `get_test_job`
- Bulk discovery or repeated safe calls:
  - `batch_execute`
- Avoid relying on official-only Unity MCP tool names unless the active session proves them available.

## Mandatory Preflight

Before running any Unity menu command through community MCP:

1. Read `AGENTS.md`.
2. Read the mission document or relevant gate report.
3. Read `mcpforunity://editor/state` if available.
4. Confirm:
   - Unity is connected.
   - Correct Unity instance is active.
   - `is_compiling == false`.
   - no domain reload is pending.
   - tools are ready.
5. Read Console errors:
   - `read_console` focused on errors and warnings.
6. Only then run exactly one deterministic menu command.

If Unity is compiling, updating, domain-reloading, or tools are not ready, stop and use `unity-compile-recovery`.

## Correct Command Pattern

Good:

```text
read editor state
read console
execute_menu_item("AI Tools/Run Tactical Preflight")
read console
read docs/TACTICAL_PREFLIGHT_REPORT.json
report evidence
```

Good:

```text
execute_menu_item("AI Tools/Run Building Integrity Gate")
read docs/gates/BUILDING_INTEGRITY_GATE_LATEST.json
capture or verify player-camera screenshot
```

Bad:

```text
create 50 GameObjects with MCP because the user asked for better buildings
```

Bad:

```text
modify scene objects through a long sequence of MCP calls and claim completion without JSON evidence
```

Bad:

```text
assume official Unity MCP bridge settings, relay path, tool registry, or approval flow apply to this community bridge
```

## Unity Menu Execution Rules

When using `execute_menu_item`:

- Execute one `AI Tools/...` command per verification step.
- Do not chain multiple gameplay gates unless the mission explicitly requires it.
- If the menu item is invalid, disabled, or missing:
  1. read Console,
  2. check compile state,
  3. confirm `Assets/Editor/TacticalWorkflowTools.cs` or the relevant editor script imported,
  4. write this as active-session failure,
  5. do not claim Unity verification.

## Community MCP Failure Modes

Treat these as normal and recoverable:

| Symptom | Likely Meaning | Response |
| --- | --- | --- |
| menu item invalid | Unity did not compile/import the editor script or session is stale | read Console, wait/restart Unity, rerun preflight only |
| tools busy | compile/domain reload/update in progress | stop polling aggressively; use compile recovery |
| command silently fails | wrong Unity instance or stale connection | read `mcpforunity://instances`, set active instance if available |
| scene changes not visible | domain reload or scene not active | read editor state and active scene resource |
| screenshot only proves showcase | not gameplay proof | require player-camera route evidence |
| contact sheet looks good | not asset promotion | require gameplay-bound and event evidence |

## Evidence Requirements

A community MCP mission is not complete unless the final answer names:

- active Unity session status,
- tool bridge used,
- command run,
- Console status,
- JSON report path,
- screenshot/video path if visual proof is required,
- what was not verified,
- remaining risks.

## Do Not Do

Do not:

- install or enable another MCP bridge in the active Unity project,
- switch to official Unity MCP assumptions unless the mission explicitly tests official MCP,
- use MCP to do broad manual scene authoring,
- hand-edit large `.unity` or `.prefab` YAML,
- promote assets based on file existence or contact sheets,
- claim active Unity verification when only repo-level static checks ran.

## When To Use Other Skills

- Use `unity-mcp-gate-runner` for the generic gate-running loop.
- Use `unity-compile-recovery` when compile/reload blocks tools.
- Use `building-integrity-gate` if/when added for building route proof.
- Use `game-feel-tuning` for weapon feel after MCP preflight is stable.
- Use `asset-promotion-gate` when promoting assets after gameplay binding.
