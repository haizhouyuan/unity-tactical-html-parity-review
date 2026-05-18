# M84 Three-Class Asset Factory Spike Mission Contract

Date: 2026-05-18

## Mission

Prove one weapon, one environment prop, and one loot item through the asset-promotion chain before scaling any batch asset work.

## Scope

- Weapon: `hero_rifle`, using the current generated Realified hero rifle already wired into gameplay.
- Environment prop: `approved_container_v1`, using the semantic-approved container asset already placed as gameplay cover.
- Loot: `medical_loot_v1`, using the semantic-approved medkit asset already attached to real `TacticalLootKind.Medkit` pickups.

## Explicit Non-Scope

- Do not promote the failed Realified non-weapon batch by hand.
- Do not override `RS_09_loot_medkit` or `RS_10_prop_container` semantic review results.
- Do not treat contact sheets, filenames, or GLB existence as completion.
- Do not run broad visual-production changes in this mission.

## Required Evidence

Each target asset must prove:

- imported asset file exists;
- material/PBR readiness is recorded;
- semantic class is accepted for this M84 target;
- object is bound to a real gameplay entity;
- object is visible from the actual player camera;
- object participates in an asset-specific gameplay event:
  - weapon fires, reloads, hits, and mounts;
  - container blocks a ray/cover line;
  - medkit pickup mutates inventory state.

## Required Outputs

- `Assets/Editor/M84ThreeClassAssetFactorySpikeGate.cs`
- `docs/M84_THREE_CLASS_ASSET_FACTORY_SPIKE.json`
- `docs/M84_THREE_CLASS_ASSET_FACTORY_SPIKE.md`
- `Assets/Screenshots/M84AssetFactory/`
- updated `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`

## Definition Of Done

- `m84_three_class_asset_factory_spike_passed=true`.
- `all_required_current_gates_passed=true`.
- Unity Console has no compile errors or warnings from the gate run.
- The report keeps `full_visual_asset_gate_passed=false` if final visual production remains unfinished.
- The report keeps the failed Realified non-weapon semantic batch quarantined.
