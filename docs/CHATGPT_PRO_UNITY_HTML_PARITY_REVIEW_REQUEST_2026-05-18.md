# ChatGPT Pro Review Request: Unity HTML Tactical Parity

Date: 2026-05-18

## What I need from ChatGPT Pro

Please review this Unity project as a code and architecture reviewer, not as a generic game-design advisor.

The goal is to make the Unity project reach at least the functional and presentation level of the previous HTML/Three.js tactical prototype. The current Unity work has improved, but it is still not acceptable as a true replacement. Please identify concrete code-level changes, file-level risks, missing systems, and the shortest implementation path.

## Product goal

Main scene:

- `Assets/Scenes/TacticalPrototype.unity`

The Unity version must reproduce the HTML tactical game baseline at equal or better level:

- lobby/start/death/settings/skin flow
- first-person and third-person modes
- visible first-person weapon at spawn
- ADS, fire, reload, weapon switching
- crouch, prone, jump
- pickup and inventory/state mutation
- NPC ranged combat and hit feedback
- dynamic enemy spawn / survival loop
- floor/ladder/building traversal
- tactical HUD parity with useful status
- imported/approved assets bound to gameplay entities, not just contact sheets
- evidence gates that prove player-camera gameplay, not only fixed screenshots

## Current hard acceptance standard

Do not count an asset as complete unless all three are true:

1. It is attached to a real gameplay entity.
2. It is visible from the player camera during normal Play.
3. It participates in a gameplay event such as fire, reload, pickup, damage, heal, armor equip, enemy attack, or player death.

Contact sheets, close-up galleries, fixed evidence cameras, visual-only overlays, and runtime proxy rigs are not product completion.

## Current known state

Important reports and docs:

- `docs/LONG_RUNNING_TACTICAL_HTML_PARITY_TODO.md`
- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`
- `docs/TACTICAL_GAMEPLAY_PROOF_GATE.json`
- `docs/TACTICAL_PLAYER_POV_GATE.json`
- `docs/TACTICAL_PLAYABLE_ROUTE_GATE.json`
- `docs/HTML_TACTICAL_PARITY_GATE.json`
- `docs/REALIFIED_ASSET_CLASS_PROMOTION_QUEUE.md`
- `docs/REALIFIED_CATEGORY_NEMOTRON_REVIEWS.md`
- `docs/TACTICAL_MANUAL_PLAY_QUICKSTART.md`

Important scripts:

- `Assets/Scripts/Tactical/TacticalGameManager.cs`
- `Assets/Scripts/Tactical/TacticalPlayerController.cs`
- `Assets/Scripts/Tactical/TacticalEnemyController.cs`
- `Assets/Scripts/Tactical/TacticalWeaponSpec.cs`
- `Assets/Editor/TacticalPrototypeTools.cs`
- `Assets/Editor/TacticalAcceptancePipeline.cs`
- `Assets/Editor/TacticalGameplayProofGate.cs`
- `Assets/Editor/TacticalPlayerPovGate.cs`
- `Assets/Editor/TacticalPlayableRouteGate.cs`
- `Assets/Editor/HtmlTacticalParityGate.cs`
- `Assets/Editor/RealifiedAssetPromotionQueue.cs`

Important asset roots:

- `Assets/HtmlTacticalAssets/ApprovedAssets/`
- `Assets/HtmlTacticalAssets/ApprovedMaterials/`
- `Assets/HtmlTacticalAssets/RealifiedAssets/`
- `Assets/HtmlTacticalAssets/models/`
- `Assets/Screenshots/PlayableRoute/`

Current acceptance status from local evidence:

- Main playable scene exists.
- Player can enter first person.
- First-person weapon is visible, but it is still visually crude and blocky.
- Fire/reload/pickup/enemy ranged attack proof gates exist and have passed in recent local runs.
- The generated/realified GLB assets are mostly not production-promoted.
- `full_visual_asset_gate_passed` is still false.
- Realified asset promotion currently reports `0 / 5` production-promoted classes.

## Review tasks

Please answer with concrete, patch-oriented guidance:

1. Which files should be changed first to make manual Play feel like the HTML game?
2. What is the minimum patch to make first-person weapon quality acceptable while keeping gameplay working?
3. Where is the current Unity architecture over-complicated, fragile, or evidence-driven rather than player-driven?
4. Which systems from the HTML baseline are still missing or only superficially implemented?
5. How should asset promotion be wired so generated GLBs appear in the playable game rather than only in contact sheets?
6. How should the gates be rewritten or tightened so they cannot pass on showcase-only assets?
7. What should be delegated to another coding agent, and what should remain local Unity verification?

## Output format requested

Please structure the answer as:

1. Blocking issues, highest severity first.
2. Concrete patch plan for the next 24 hours.
3. File-by-file code review notes.
4. Gate/validation changes.
5. Asset pipeline changes.
6. Risks and tradeoffs.
7. A short prompt that can be given to a coding agent to implement the first patch.

Avoid broad motivational advice. Prefer exact method names, class names, scene objects, and acceptance checks.
