# Tactical Manual Play Quickstart

## Playable Scene

Open this scene for gameplay:

- `Assets/Scenes/TacticalPrototype.unity`

Unity menu shortcut:

- `AI Tools > Open Tactical Playable Scene`

These scenes are only asset preview/contact sheets and do not contain the player controller, first-person weapon, shooting, reload, pickup, or NPC combat:

- `Assets/Scenes/GLB_PBR_ContactSheet.unity`
- `Assets/Scenes/GLB_PBR_Textured_ContactSheet.unity`

## Controls

1. Press Play.
2. Press Enter, Space, left click, or the Start button to leave the lobby.
3. The round starts in first-person by default. Press `V` to switch first-person / third-person.
4. Left mouse fires.
5. Right mouse holds ADS.
6. Press `R` to reload after firing a few shots. If the magazine is full, the HUD shows `弹匣已满`.
7. Press `1-4` to switch unlocked weapons. Only the pistol is unlocked at spawn; other weapons must be picked up.
8. Press `F` near loot.
9. Press `C` crouch, `Z` prone, Space jump, Shift sprint.

## Asset Locations

Unity gameplay/imported asset root:

- `Assets/HtmlTacticalAssets/`

Promoted gameplay assets:

- `Assets/HtmlTacticalAssets/ApprovedAssets/`

Generated candidate GLB assets and LODs:

- `Assets/HtmlTacticalAssets/RealifiedAssets/`

Approved PBR texture folders:

- `Assets/HtmlTacticalAssets/ApprovedMaterials/`

Old HTML final-packet GLBs used as fallback/baseline:

- `Assets/HtmlTacticalAssets/models/`

Screenshots and gate evidence:

- `Assets/Screenshots/`
- `docs/TACTICAL_PLAYER_POV_GATE.json`
- `docs/TACTICAL_GAMEPLAY_PROOF_GATE.json`
- `docs/TACTICAL_PLAYABLE_ROUTE_GATE.json`

## Current Truth

The playable route has shooting/reload/weapon-switch code and gates, but the full visual asset gate is still not final. Some generated realified assets remain candidates or are quarantined because they are visually wrong for their intended class.
