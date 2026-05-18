---
name: unity-compile-recovery
description: Use when Unity MCP is connected but menus/tools are missing, Unity is compiling/updating for too long, or assembly reload is locked.
---

# Unity Compile Recovery

Symptoms:

- menu command missing after adding an Editor script;
- `isCompiling` or `isUpdating` stays true for more than 2 minutes;
- Console has no clear compile error but new classes are not visible;
- assembly reload lock warning appears.

Protocol:

1. Stop polling.
2. Save assets/project if possible.
3. Quit Unity cleanly.
4. Reopen the project.
5. Wait until compile/update idle.
6. Read Console.
7. Run one gate only.

Reference:

- `docs/UNITY_MCP_RELIABILITY_REPORT.md`

Do not repeatedly run refresh/import/menu commands while reload is dirty.
