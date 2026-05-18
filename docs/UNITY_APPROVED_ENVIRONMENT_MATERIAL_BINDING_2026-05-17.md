# Unity Approved Environment Material Binding

Date: 2026-05-17

## Result

Approved PBR environment material packets were imported into Unity and wired into the generated HTML tactical replica scene.

## Imported Texture Sets

Wet asphalt:

- `wet_asphalt_basecolor.png`
- `wet_asphalt_normal.png`
- `wet_asphalt_roughness.png`
- `wet_asphalt_metallic.png`
- `wet_asphalt_ao.png`

Concrete wall:

- `concrete_wall_basecolor.png`
- `concrete_wall_normal.png`
- `concrete_wall_roughness.png`
- `concrete_wall_metallic.png`
- `concrete_wall_ao.png`
- `concrete_wall_height.png`

Tactical crate:

- `tactical_crate_basecolor.png`
- `tactical_crate_normal.png`
- `tactical_crate_roughness.png`
- `tactical_crate_metallic.png`
- `tactical_crate_ao.png`

Container/checkpoint prop:

- `container_checkpoint_basecolor.png`
- `container_checkpoint_normal.png`
- `container_checkpoint_roughness.png`
- `container_checkpoint_metallic.png`
- `container_checkpoint_ao.png`

Player tactical:

- `player_tactical_basecolor.png`
- `player_tactical_normal.png`
- `player_tactical_roughness.png`
- `player_tactical_metallic.png`
- `player_tactical_ao.png`

Enemy tactical:

- `enemy_tactical_basecolor.png`
- `enemy_tactical_normal.png`
- `enemy_tactical_roughness.png`
- `enemy_tactical_metallic.png`
- `enemy_tactical_ao.png`

Unity destination:

- `Assets/HtmlTacticalAssets/ApprovedMaterials/wet_asphalt/`
- `Assets/HtmlTacticalAssets/ApprovedMaterials/concrete_wall/`
- `Assets/HtmlTacticalAssets/ApprovedMaterials/tactical_crate/`
- `Assets/HtmlTacticalAssets/ApprovedMaterials/container_checkpoint/`
- `Assets/HtmlTacticalAssets/ApprovedMaterials/player_tactical/`
- `Assets/HtmlTacticalAssets/ApprovedMaterials/enemy_tactical/`

## Code Integration

Updated `Assets/Editor/TacticalPrototypeTools.cs`:

- Added `ApprovedMaterialRoot`.
- Added `SavePbrMaterial`.
- Added texture import configuration for normal maps and non-color maps.
- Replaced generated road material with `TacticalWetAsphaltApproved`.
- Replaced generated wall and floor materials with `TacticalConcreteWallApproved` and `TacticalConcreteFloorApproved`.
- Added `TacticalCratePbrApproved` and `TacticalContainerPbrApproved`.
- Applied those PBR materials to editor-instantiated crate/container GLB renderers through the `AttachHtmlGlb(..., overrideMaterial)` path.
- Added `TacticalPlayerPbrApproved` and `TacticalEnemyPbrApproved`.
- Applied those PBR materials to player, enemy, and runtime reinforcement-template character GLB renderers.

## Unity Verification

Community MCP generated the tactical scene and verified:

- `TacticalWetAsphaltApproved`: exists, URP/Lit, 5 texture slots.
- `TacticalConcreteWallApproved`: exists, URP/Lit, 5 texture slots.
- `TacticalConcreteFloorApproved`: exists, URP/Lit, 5 texture slots.
- Road renderers using wet asphalt material: 2.
- Concrete renderers using concrete material: 71.
- Crate renderers using `TacticalCratePbrApproved`: 22.
- Container renderers using `TacticalContainerPbrApproved`: 990.
- Player renderers using `TacticalPlayerPbrApproved`: 36.
- Enemy renderers using `TacticalEnemyPbrApproved`: 396.
- Route gate: PASS.
- Console errors after compile: 0.

Latest route report:

- `docs/TACTICAL_PLAYABLE_ROUTE_GATE.json`

Player-camera evidence:

- `Assets/Screenshots/PlayableRoute/02_spawn_exit_route.png`
- `Assets/Screenshots/PlayableRoute/03_building_a_entry.png`
- `Assets/Screenshots/PlayableRoute/04_warehouse_route.png`

## Remaining Limits

This improves the current Unity visual baseline, but it is not yet final PUBG-like material quality. The current URP/Lit wiring uses base, normal, metallic, and AO texture slots plus scalar smoothness. A later pass should pack or wire roughness correctly and add decals, dirt, wetness variation, and lighting/postprocessing.
