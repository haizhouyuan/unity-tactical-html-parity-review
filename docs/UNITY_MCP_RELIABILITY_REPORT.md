# Unity MCP Reliability Report

Date: 2026-05-17

## Current Status

MCP connection itself works:

- Endpoint: `http://127.0.0.1:8080/mcp`
- Server: `mcp-for-unity-server`
- Version observed: `3.3.1`
- Tools/resources are discoverable.
- `read_console`, `manage_editor`, `execute_code`, and `execute_menu_item` have all responded.

The current problem is Unity Editor state, not MCP transport availability.

## Observed Failure

After adding `Assets/Editor/TacticalPlayableRouteGate.cs`, Unity did not expose the menu:

```text
AI Tools/Write Tactical Playable Route Gate
```

MCP `execute_menu_item` returned:

```text
ExecuteMenuItem failed because there is no menu named 'AI Tools/Write Tactical Playable Route Gate'
```

Reflection through MCP `execute_code` showed:

```text
routeType: false
menuCount: 0 for the new route tool
```

Later polling showed:

```text
EditorApplication.isCompiling == true
EditorApplication.isUpdating == false
```

for several minutes.

Editor log also contained:

```text
Entering playmode with assembly reload locked. Recent script changes might be missing.
In order to allow script updates call EditorApplication.UnlockReloadAssemblies().
```

## Recovery Attempt

MCP recovery command called:

```csharp
UnityEditor.EditorApplication.UnlockReloadAssemblies();
UnityEditor.EditorApplication.UnlockReloadAssemblies();
UnityEditor.AssetDatabase.StopAssetEditing();
UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
UnityEditor.AssetDatabase.Refresh(
    UnityEditor.ImportAssetOptions.ForceUpdate |
    UnityEditor.ImportAssetOptions.ForceSynchronousImport);
```

Partial result:

```text
UnlockReloadAssemblies called once
UnlockReloadAssemblies called twice
StopAssetEditing called
RequestScriptCompilation called
AssetDatabase.Refresh sync called
```

But `isCompiling` stayed true afterward.

Console then reported:

```text
Assertion failed on expression: 'gRefreshReentrancyCount > 0'
StopAssetEditing invoked without a call to StartAssetEditing.
```

## Interpretation

This is a dirty Editor/session state:

- New Editor scripts may be imported on disk but not loaded into the active AppDomain.
- Menu-based gates are unreliable until Unity completes compilation/reload.
- Further repeated `refresh_unity` or `execute_menu_item` calls are likely to waste time.

## Current Rule

If all of the following are true:

- `isCompiling == true` for more than 2 minutes,
- no compile errors are visible through `read_console`,
- new Editor classes are not visible by reflection,
- new menu items are not visible in `mcpforunity://menu-items`,

then do not keep polling. Do this instead:

```text
1. Save assets/project if possible.
2. Quit Unity.
3. Reopen the project.
4. Wait until isCompiling == false and isUpdating == false.
5. Read Console.
6. Then run one gate.
```

## Safer Next Probe

For the next pass, avoid adding a new long Editor menu script during a dirty Unity session. Use one of these:

1. A transient `execute_code` route gate that does not require a new compiled class.
2. A clean Unity restart, then verify `TacticalPlayableRouteGate` loads.
3. Move route-gate logic into an already-visible Editor tool only after a clean compile.

## Decision

Do not claim route-gate support is working yet.

The Unity MCP transport works; the Unity compile/reload state is currently unreliable and needs a clean restart or a different transient-gate strategy.

