# Unity MCP Status - 2026-05-16

## Summary

Unity AI Assistant is installed and the Unity MCP Server settings page is available. The Unity Bridge is running, and Codex global config points to the local Unity relay for this project.

However, the official Unity MCP connection is currently blocked by Unity plan entitlement, not by local path/configuration.

## Project

```text
/Users/yuanshaochen/My project
```

Unity version:

```text
6000.4.7f1
```

## Codex MCP Config

Configured in:

```text
/Users/yuanshaochen/.codex/config.toml
```

Server entry:

```toml
[mcp_servers.unity-mcp]
command = "/Users/yuanshaochen/.unity/relay/relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64"
args = ["--mcp", "--project-path", "/Users/yuanshaochen/My project"]
enabled = true
startup_timeout_sec = 30.0
tool_timeout_sec = 120.0
```

## Unity Editor State

Unity page:

```text
Edit > Project Settings > AI > Unity MCP Server
```

Observed:

- Authorization notice accepted.
- Unity Bridge status: `Running`.
- Codex connection was recorded by Unity.
- MCP tools list is visible.

## Blocking Evidence

Unity recorded the Codex MCP client in:

```text
Library/AI.MCP/connections-v2.asset
```

Relevant status:

```text
ClientInfo:
  Name: codex-mcp-client
Status: 4
ValidationReason: Your Unity plan doesn't include MCP connections. Upgrade your Unity plan to add more.
```

Interpretation:

The official Unity MCP path is configured locally but blocked by Unity plan entitlement. Restarting Codex or changing relay path will not fix this unless the Unity account/plan enables MCP connections.

## Working Fallback

The project includes a safe Editor menu tool:

```text
Assets/Editor/AiGameProjectTools.cs
```

Unity menu:

```text
AI Tools > Validate Project Setup
```

This was executed successfully in Unity. Unity Editor log confirms all required folders are present and project files exist.

## Recommended Next Steps

1. If official Unity MCP is required, enable a Unity plan/trial/subscription that includes MCP connections, then reconnect Codex.
2. Until then, use:
   - Codex file edits for C# and project files.
   - `Assets/Editor/` menu tools for scene/prefab automation.
   - Computer Use for safe Unity menu execution and visual checks.
   - Batchmode only when this project is closed in Unity.
3. Optionally evaluate a community Unity MCP bridge if official MCP entitlement remains blocked.
