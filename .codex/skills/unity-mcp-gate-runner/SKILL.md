---
name: unity-mcp-gate-runner
description: Use when running Unity work through Codex/MCP for this tactical HTML parity project; read Console, check compile/update state, execute one deterministic AI Tools menu command, then read JSON evidence before reporting.
---

# Unity MCP Gate Runner

Use MCP as transport only. Do not design by clicking through many GameObjects.

Workflow:

1. Read `AGENTS.md` and the relevant report in `docs/`.
2. Read Unity Console through MCP if available.
3. Check whether Unity is compiling/updating.
4. Execute exactly one deterministic menu command such as `AI Tools/Run Tactical Preflight` or `AI Tools/Run Tactical Acceptance Pipeline`.
5. Read Console again.
6. Read the generated JSON report.
7. Report changed files, menu command, report path, screenshot path, and remaining risks.

If Unity is compile/reload locked for more than 2 minutes, stop and switch to `unity-compile-recovery`.

Never claim completion from Console logs alone. JSON and screenshot evidence must be named.
