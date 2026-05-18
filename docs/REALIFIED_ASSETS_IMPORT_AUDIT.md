# RealifiedAssets GLB Audit

Date: 2026-05-17T15:47:44.240844+00:00

## Verdict

RealifiedAssets are mostly embedded-textured GLB probes. Assets with complete Unity sidecar PBR maps and sufficient mesh attributes can be promoted as sidecar PBR candidates, but the global visual gate still requires semantic category review and gameplay binding evidence.

## Summary

- Folder: `/Users/yuanshaochen/My project/Assets/HtmlTacticalAssets/RealifiedAssets`
- GLB files: 48
- External texture sidecars: 138
- External material files: 7
- Textured probes: 21
- Promotable PBR candidates: 27
- Sidecar-promotable candidates: 27
- Quarantined: 0

## Important Finding

Every currently audited GLB can have embedded base color and metallic-roughness textures, but a production PBR asset needs more than that. The audit checks for normal maps, AO/occlusion, mesh NORMAL/TANGENT attributes, and promotion status.

## Files

| File | Category | Mesh | Materials | Images | Slots | Sidecars | Status | Reasons |
| --- | --- | ---: | ---: | ---: | --- | --- | --- | --- |
| `RS_02_sidearm_LOD0.glb` | weapon | 1 | 1 | 4 | baseColorTexture, metallicRoughnessTexture, normalTexture, occlusionTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_02_sidearm_LOD1.glb` | weapon | 1 | 1 | 4 | baseColorTexture, metallicRoughnessTexture, normalTexture, occlusionTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_02_sidearm_LOD2.glb` | weapon | 1 | 1 | 4 | baseColorTexture, metallicRoughnessTexture, normalTexture, occlusionTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_02_sidearm_textured.glb` | weapon | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing primitive NORMAL attribute; missing TANGENT attribute for production normal mapping |
| `RS_03_secondary_weapon_LOD0.glb` | weapon | 1 | 1 | 4 | baseColorTexture, metallicRoughnessTexture, normalTexture, occlusionTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_03_secondary_weapon_LOD1.glb` | weapon | 1 | 1 | 4 | baseColorTexture, metallicRoughnessTexture, normalTexture, occlusionTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_03_secondary_weapon_LOD2.glb` | weapon | 1 | 1 | 4 | baseColorTexture, metallicRoughnessTexture, normalTexture, occlusionTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_03_secondary_weapon_textured.glb` | weapon | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing primitive NORMAL attribute; missing TANGENT attribute for production normal mapping |
| `RS_04_player_tactical_LOD0.glb` | character | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing TANGENT attribute for production normal mapping |
| `RS_04_player_tactical_LOD1.glb` | character | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing TANGENT attribute for production normal mapping |
| `RS_04_player_tactical_LOD2.glb` | character | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing TANGENT attribute for production normal mapping |
| `RS_04_player_tactical_textured.glb` | character | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing primitive NORMAL attribute; missing TANGENT attribute for production normal mapping |
| `RS_05_enemy_tactical_LOD0.glb` | character | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing TANGENT attribute for production normal mapping |
| `RS_05_enemy_tactical_LOD1.glb` | character | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing TANGENT attribute for production normal mapping |
| `RS_05_enemy_tactical_LOD2.glb` | character | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing TANGENT attribute for production normal mapping |
| `RS_05_enemy_tactical_textured.glb` | character | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing primitive NORMAL attribute; missing TANGENT attribute for production normal mapping |
| `RS_06_gear_helmet_LOD0.glb` | gear | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_06_gear_helmet_LOD1.glb` | gear | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_06_gear_helmet_LOD2.glb` | gear | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_06_gear_helmet_textured.glb` | gear | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing primitive NORMAL attribute |
| `RS_07_gear_vest_LOD0.glb` | gear | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_07_gear_vest_LOD1.glb` | gear | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_07_gear_vest_LOD2.glb` | gear | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_07_gear_vest_textured.glb` | gear | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing primitive NORMAL attribute |
| `RS_08_loot_ammo_LOD0.glb` | loot | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_08_loot_ammo_LOD1.glb` | loot | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_08_loot_ammo_LOD2.glb` | loot | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_08_loot_ammo_textured.glb` | loot | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing primitive NORMAL attribute |
| `RS_09_loot_medkit_LOD0.glb` | loot | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_09_loot_medkit_LOD1.glb` | loot | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_09_loot_medkit_LOD2.glb` | loot | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_09_loot_medkit_textured.glb` | loot | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing primitive NORMAL attribute |
| `RS_10_prop_container_LOD0.glb` | environment_prop | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_10_prop_container_LOD1.glb` | environment_prop | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_10_prop_container_LOD2.glb` | environment_prop | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `RS_10_prop_container_textured.glb` | environment_prop | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing primitive NORMAL attribute |
| `RS_11_prop_crate_LOD0.glb` | environment_prop | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao, orm | promotable_pbr_candidate | - |
| `RS_11_prop_crate_LOD1.glb` | environment_prop | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao, orm | promotable_pbr_candidate | - |
| `RS_11_prop_crate_LOD2.glb` | environment_prop | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | basecolor, normal, metallic, roughness, ao, orm | promotable_pbr_candidate | - |
| `RS_11_prop_crate_textured.glb` | environment_prop | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing primitive NORMAL attribute |
| `RS_12_shotgun_LOD0.glb` | weapon | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing TANGENT attribute for production normal mapping |
| `RS_12_shotgun_LOD1.glb` | weapon | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing TANGENT attribute for production normal mapping |
| `RS_12_shotgun_LOD2.glb` | weapon | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing TANGENT attribute for production normal mapping |
| `RS_12_shotgun_textured.glb` | weapon | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing primitive NORMAL attribute; missing TANGENT attribute for production normal mapping |
| `hero_rifle_LOD0.glb` | weapon | 1 | 1 | 4 | baseColorTexture, metallicRoughnessTexture, normalTexture, occlusionTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `hero_rifle_LOD1.glb` | weapon | 1 | 1 | 4 | baseColorTexture, metallicRoughnessTexture, normalTexture, occlusionTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `hero_rifle_LOD2.glb` | weapon | 1 | 1 | 4 | baseColorTexture, metallicRoughnessTexture, normalTexture, occlusionTexture | basecolor, normal, metallic, roughness, ao | promotable_pbr_candidate | - |
| `hero_rifle_textured.glb` | weapon | 1 | 1 | 2 | baseColorTexture, metallicRoughnessTexture | - | textured_probe_not_production | missing normalTexture; missing occlusionTexture/AO; missing primitive NORMAL attribute; missing TANGENT attribute for production normal mapping |
