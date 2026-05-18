# M92 Weapon Production Pass Mission Contract

Date: 2026-05-18

Baseline commit: `0ec70ae Add M91 external input built player route gate`

## Mission

Upgrade the current tactical weapon experience from a functional gate pass to a more production-oriented weapon-feel pass without claiming final visual asset completion.

This mission is scoped to weapon feel, weapon visuals, and weapon evidence. It must not alter maps, scenes, packages, builds, or M88 pass/fail values.

## Current Ground Truth

- M91 external-input built-player route evidence is complete in the local project, but this patch does not edit M91 evidence.
- M88 strict full visual gate remains failed.
- Existing `WEAPON_FEEL_GATE.json` proves functional weapon feel, but not final weapon art quality.
- The remaining M88 blockers include final weapon art review and final humanoid art review.

## Scope

Allowed files:

- `Assets/Scripts/Tactical/TacticalFirstPersonWeaponVisual.cs`
- `Assets/Scripts/Tactical/TacticalThirdPersonWeaponVisual.cs`
- `Assets/Scripts/Tactical/TacticalGameManager.cs`
- `Assets/Scripts/Tactical/TacticalWeaponSpec.cs`
- `Assets/Editor/WeaponFeelGate.cs`
- `Assets/Editor/TacticalAcceptancePipeline.cs`
- `README.md`
- `docs/LONG_RUNNING_AI_GAME_STUDIO_TODO_2026-05-18.md`
- this mission contract
- completion template

## Explicitly Out Of Scope

- No `Assets/Scenes/*.unity` edits.
- No `.prefab` YAML edits.
- No package or project settings edits.
- No build artifacts.
- No M88 pass/fail value changes.
- No M91 evidence edits.
- No generated batch class promotion.
- No non-weapon map, character, or environment art pass.

## Intended Runtime Improvements

### First-person weapon

- More readable idle and movement sway.
- ADS has a more stable pose than hip fire.
- Fire creates a stronger but controlled kick, side offset, and roll response.
- Reload creates a visible lower/side pose with roll.
- Weapon selection creates a visible raise/lower transition.
- These changes must not mutate gameplay state; they are visual pose polish only.

### Third-person weapon

- Mount quality metrics are exposed.
- Active character weapon visibility is more measurable.
- Fire/select pulse is available for lightweight third-person visual feedback.
- Skin tint evidence remains measurable.

### Gate output

`AI Tools/Run Weapon Feel Gate` should emit M92-specific fields, including:

- `m92_weapon_production_passed`
- `first_person_pose_quality_passed`
- `recoil_peak_observed`
- `recoil_peak_value`
- `reload_pose_magnitude_observed`
- `reload_pose_magnitude_value`
- `ads_stability_observed`
- `ads_stability_value`
- `shot_feedback_event_count`
- `third_person_mount_quality_passed`
- `third_person_mount_quality_score`
- `third_person_fire_pulse_events`
- `missing_feedback_hooks`

## Definition Of Done For Local Codex

Codex/local Unity must apply this patch, refresh Unity, enter Play Mode or run the tactical pipeline, then run:

```text
AI Tools/Run Weapon Feel Gate
AI Tools/Run Tactical Acceptance Pipeline
```

M92 may be considered locally verified only if:

- Unity Console has no compile errors.
- `docs/WEAPON_FEEL_GATE.json` is regenerated.
- `m92_weapon_production_passed=true` in `docs/WEAPON_FEEL_GATE.json`.
- Screenshot evidence under `Assets/Screenshots/WeaponFeel/` is regenerated.
- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json` includes the M92 fields.
- `full_visual_asset_gate_passed` remains whatever the real strict visual gate reports; this mission must not force it true.

## Non-Claims

This mission does not claim:

- final commercial weapon art quality;
- final humanoid art quality;
- generated batch class promotion;
- full visual asset gate closure;
- external built-player verification.
