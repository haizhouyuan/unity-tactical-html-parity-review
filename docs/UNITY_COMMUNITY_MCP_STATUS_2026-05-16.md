# Unity Community MCP Status - 2026-05-16

## Summary

The project now uses the community `MCP For Unity` package as the working Unity MCP path.

Official Unity MCP is still blocked by Unity plan / seat entitlement. The community MCP path is installed, configured, and verified through the MCP protocol.

## Installed Package

- Unity package: `com.coplaydev.unity-mcp`
- Source: `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`
- Imported package cache: `Library/PackageCache/com.coplaydev.unity-mcp@13fb3ee12774`

## Codex MCP Config

Configured in `/Users/yuanshaochen/.codex/config.toml`:

```toml
[mcp_servers.unityMCPCommunity]
url = "http://localhost:8080/mcp"
enabled = true
startup_timeout_sec = 20.0
tool_timeout_sec = 120.0
```

Codex may need a restart or MCP refresh before this appears as a normal first-class tool server in the app. The endpoint itself has already been tested directly.

## Unity-Side Startup

In Unity:

1. Open `Window > MCP For Unity > Toggle MCP Window`.
2. Confirm:
   - Transport: `HTTP Local`
   - HTTP URL: `http://127.0.0.1:8080`
   - Local Server: `Stop Server` button visible means server is running.
3. Click `Start Session` if the status says `No Session`.
4. A working state shows `Session Active (My project)`.

The project also has a helper menu:

- `AI Tools > Start Community Unity MCP Server`

If the server is running but Unity says `No Session`, the MCP endpoint can list tools but Unity-only calls such as `read_console` will return `no_unity_session`. Click `Start Session` in the MCP For Unity window.

## Verification Performed

Direct MCP protocol verification:

- `initialize`: passed
- `tools/list`: passed
- `execute_menu_item` for `AI Tools/Create Star Collector Prototype`: passed
- `execute_menu_item` for `AI Tools/Write Project Report`: passed
- `read_console`: passed after `Start Session`
- `manage_scene get_active`: passed, active scene is `Assets/Scenes/Main.unity`
- `manage_scene get_hierarchy`: passed, scene contains player, camera, game manager, and 10 stars
- `manage_editor play`: passed
- `manage_camera screenshot`: passed
- `manage_editor stop`: passed

## Game Prototype Created Through MCP

- Game: `Star Collector Adventure / 星星收集小冒险`
- Scene: `Assets/Scenes/Main.unity`
- Evidence screenshot: `Assets/Screenshots/star_collector_clean_playmode.png`
- Report: `docs/UNITY_PROJECT_REPORT.md`

Play Mode verification:

- Entered Play Mode through MCP.
- Captured Game View screenshot through MCP.
- Console was cleared before verification.
- No compile errors were reported during the clean Play Mode pass.
- Current remaining console messages were URP depth-surface warnings, not script compile errors.

## Known Notes

- The player controller uses Unity's new Input System (`UnityEngine.InputSystem.Keyboard`) because this project has legacy `UnityEngine.Input` disabled.
- The community MCP HTTP service listens on `127.0.0.1:8080`.
- The server process is launched by the Unity package through `uvx --from mcpforunityserver==9.6.8`.
- Official Unity MCP entitlement errors are unrelated to this community MCP route.
