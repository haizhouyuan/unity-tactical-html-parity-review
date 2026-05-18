# Realified Category Nemotron Reviews

- Timestamp: `2026-05-17T19:26:45.671831+00:00`
- Promotion allowed: `False`
- Missing rendered categories: `none`
- Failed categories: `weapon, character, gear, loot, environment_prop`

| Category | Rendered | Verdict | Promotion | Summary |
|---|---:|---|---:|---|
| `weapon` | `True` | `fail` | `False` | Three weapons match expected categories but quality blockers prevent promotion. |
| `character` | `True` | `fail` | `False` | Two weapon assets visible, but expected character categories, causing category mismatch and promotion blocker. |
| `gear` | `True` | `fail` | `False` | Two weapon objects visible, but expected gear items; category mismatch. |
| `loot` | `True` | `fail` | `False` | Two weapon assets visible, mismatching expected loot categories, causing failure. |
| `environment_prop` | `True` | `fail` | `False` | Two weapon assets visible, mismatching expected environment prop categories. |

## Blockers

### weapon
- `expected_assets_not_technically_promotable: RS_12_shotgun_LOD0.glb`
- `missing TANGENT attribute for production normal mapping`
- `missing normalTexture`
- `missing occlusionTexture/AO`
- `missing_normal_texture`
- `missing_occlusion_or_ao_texture`
- `missing_tangent_attribute`

### character
- `category mismatch (visible weapon vs expected character)`
- `expected_assets_not_technically_promotable: RS_04_player_tactical_LOD0.glb, RS_05_enemy_tactical_LOD0.glb`
- `missing_normal_texture`
- `missing_occlusion_or_ao_texture`
- `missing_tangent_attribute`
- `textured_probe_not_production (missing normalTexture, occlusionTexture/AO, TANGENT)`

### gear
- `category mismatch (visible weapon vs expected gear)`

### loot
- `category mismatch (weapon vs loot) for both items`
- `visual objects are rifles, not ammo or medkit`

### environment_prop
- `Category mismatch: visible objects are weapons, expected environment props.`
