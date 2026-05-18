---
name: game-feel-tuning
description: Use when improving tactical game feel: first-person weapon visibility, ADS, recoil, reload, muzzle flash, casing, hit marker, audio, camera shake, and weapon switching.
---

# Game Feel Tuning

Primary files:

- `Assets/Scripts/Tactical/TacticalGameManager.cs`
- `Assets/Scripts/Tactical/TacticalPlayerController.cs`
- `Assets/Scripts/Tactical/TacticalFirstPersonWeaponVisual.cs`
- `Assets/Scripts/Tactical/TacticalThirdPersonWeaponVisual.cs`
- `Assets/Editor/TacticalGameplayProofGate.cs`
- `Assets/Editor/TacticalPlayableRouteGate.cs`

Acceptance:

- first-person weapon visible at spawn;
- each touched weapon can fire and reload;
- ammo mutates;
- hit marker or enemy HP mutates;
- muzzle/tracer/casing/recoil/audio evidence exists;
- player-camera screenshot shows the result.

After changes run `AI Tools/Run Game Feel Evidence Gate` or the full tactical acceptance pipeline. Do not broaden into new assets unless the mission explicitly asks.
