# Realified Asset Source Trace

Date: `2026-05-17T21:23:26.294466+00:00`

## Verdict

Do not promote the Unity Realified batch. Current Unity GLBs do not trace back by SHA256 to the expected research asset packet outputs, and existing semantic reviews mark non-weapon LOD0 categories as visible weapon silhouettes.

## Summary

- Unity root: `/Users/yuanshaochen/My project/Assets/HtmlTacticalAssets/RealifiedAssets`
- Research assets root: `/Users/yuanshaochen/Projects/ai-game-generation-research/experiments/pubg_like_asset_factory_20260513/assets`
- Unity GLBs: `48`
- Source SHA matches: `0`
- LOD0/primary failures: `12`
- Non-weapon semantic failures: `8`

## Assets

| File | Expected category | Packet | Source SHA match | Visible category | Semantic | Action |
| --- | --- | --- | ---: | --- | --- | --- |
| `RS_02_sidearm_LOD0.glb` | weapon | `sidearm_v1` | no | weapon | match | quarantine_trace_source_before_review |
| `RS_02_sidearm_textured.glb` | weapon | `sidearm_v1` | no | - | not_reviewed | quarantine_trace_source_before_review |
| `RS_03_secondary_weapon_LOD0.glb` | weapon | `secondary_weapon_v1` | no | weapon | match | quarantine_trace_source_before_review |
| `RS_03_secondary_weapon_textured.glb` | weapon | `secondary_weapon_v1` | no | - | not_reviewed | quarantine_trace_source_before_review |
| `RS_04_player_tactical_LOD0.glb` | character | `player_tactical_v1` | no | weapon | mismatch | quarantine_regenerate_or_replace_category_wrong |
| `RS_04_player_tactical_textured.glb` | character | `player_tactical_v1` | no | - | not_reviewed | quarantine_trace_source_before_review |
| `RS_05_enemy_tactical_LOD0.glb` | character | `enemy_tactical_v1` | no | weapon | mismatch | quarantine_regenerate_or_replace_category_wrong |
| `RS_05_enemy_tactical_textured.glb` | character | `enemy_tactical_v1` | no | - | not_reviewed | quarantine_trace_source_before_review |
| `RS_06_gear_helmet_LOD0.glb` | gear | `gear_set_v1` | no | weapon | mismatch | quarantine_regenerate_or_replace_category_wrong |
| `RS_06_gear_helmet_textured.glb` | gear | `gear_set_v1` | no | - | not_reviewed | quarantine_trace_source_before_review |
| `RS_07_gear_vest_LOD0.glb` | gear | `gear_set_v1` | no | weapon | mismatch | quarantine_regenerate_or_replace_category_wrong |
| `RS_07_gear_vest_textured.glb` | gear | `gear_set_v1` | no | - | not_reviewed | quarantine_trace_source_before_review |
| `RS_08_loot_ammo_LOD0.glb` | loot | `loot_set_v1` | no | weapon | mismatch | quarantine_regenerate_or_replace_category_wrong |
| `RS_08_loot_ammo_textured.glb` | loot | `loot_set_v1` | no | - | not_reviewed | quarantine_trace_source_before_review |
| `RS_09_loot_medkit_LOD0.glb` | loot | `loot_set_v1` | no | weapon | mismatch | quarantine_regenerate_or_replace_category_wrong |
| `RS_09_loot_medkit_textured.glb` | loot | `loot_set_v1` | no | - | not_reviewed | quarantine_trace_source_before_review |
| `RS_10_prop_container_LOD0.glb` | environment_prop | `container_checkpoint_v1` | no | weapon | mismatch | quarantine_regenerate_or_replace_category_wrong |
| `RS_10_prop_container_textured.glb` | environment_prop | `container_checkpoint_v1` | no | - | not_reviewed | quarantine_trace_source_before_review |
| `RS_11_prop_crate_LOD0.glb` | environment_prop | `tactical_crate_trellis_texturealchemy_v1` | no | weapon | mismatch | quarantine_regenerate_or_replace_category_wrong |
| `RS_11_prop_crate_textured.glb` | environment_prop | `tactical_crate_trellis_texturealchemy_v1` | no | - | not_reviewed | quarantine_trace_source_before_review |
| `RS_12_shotgun_LOD0.glb` | weapon | `secondary_weapon_v1` | no | weapon | match | quarantine_trace_source_before_review |
| `RS_12_shotgun_textured.glb` | weapon | `secondary_weapon_v1` | no | - | not_reviewed | quarantine_trace_source_before_review |
| `hero_rifle_LOD0.glb` | weapon | `hero_rifle_v1` | no | weapon | match | quarantine_trace_source_before_review |
| `hero_rifle_textured.glb` | weapon | `hero_rifle_v1` | no | - | not_reviewed | quarantine_trace_source_before_review |
