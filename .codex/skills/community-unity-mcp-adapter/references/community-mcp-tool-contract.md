# Community Unity MCP Tool Contract

This reference records the project assumptions for the non-official CoplayDev/community Unity MCP bridge.

It is intentionally not a full tool manual. Codex should inspect the active tool list/resources in the connected Unity session before using any tool.

## Bridge Identity

Current intended bridge family:

```text
CoplayDev / MCP for Unity / com.coplaydev.unity-mcp
```

Do not treat this as the same tool surface as official Unity MCP.

## Preferred Operating Contract

```text
resources first
  -> read console
  -> run one AI Tools menu command
  -> read console
  -> read JSON report
  -> capture player-camera evidence if needed
```

## Common Community MCP Resources

Expected resource patterns may include:

```text
mcpforunity://editor/state
mcpforunity://project/info
mcpforunity://instances
mcpforunity://scene/gameobject-api
mcpforunity://scene/cameras
mcpforunity://rendering/stats
mcpforunity://pipeline/renderer-features
```

Use resources to understand current state before mutation.

## Common Community MCP Tools

Expected tool names may include:

```text
execute_menu_item
read_console
find_gameobjects
manage_scene
manage_gameobject
manage_components
manage_asset
manage_prefabs
manage_camera
manage_graphics
manage_physics
manage_build
run_tests
get_test_job
batch_execute
set_active_instance
unity_docs
unity_reflect
```

Do not assume all tools are enabled. Always inspect active tool availability when possible.

## Project-Safe Commands

The safest Unity commands for this repo are deterministic menu commands under:

```text
AI Tools/...
```

Known examples:

```text
AI Tools/Run Tactical Preflight
AI Tools/Run Unity MCP Smoke Check
AI Tools/Run Game Feel Evidence Gate
AI Tools/Run Tactical Acceptance Pipeline
```

Future examples:

```text
AI Tools/Run Building Integrity Gate
AI Tools/Run Weapon Feel Gate
AI Tools/Run AI Playtest Route
AI Tools/Run Three-Class Asset Factory Spike
```

## Community MCP Smoke Definition

A smoke check is not complete unless it records:

```yaml
unity_connected:
correct_instance_active:
editor_state_read:
is_compiling:
is_updating:
domain_reload_pending:
console_errors:
menu_items_visible:
preflight_command_executed:
json_report_written:
external_mcp_transport_proven:
notes:
```

An in-Editor smoke command may prove editor readiness, but it does not by itself prove that an external MCP client successfully transported the command. The final report must distinguish these.

## Command Budget

Use a small command budget:

- One gate command per evidence step.
- Prefer `batch_execute` only for discovery or bounded repeated checks.
- Do not run broad mutations through MCP.
- Do not use tool loops without a stop condition.

## Recovery

If the bridge is stale:

1. Stop changing the project.
2. Read Console.
3. Read editor state.
4. If compile/reload locked, switch to `unity-compile-recovery`.
5. If wrong instance, select active instance.
6. If menus are missing, restart Unity and wait for compile idle.
7. Run only `AI Tools/Run Tactical Preflight` after recovery.
