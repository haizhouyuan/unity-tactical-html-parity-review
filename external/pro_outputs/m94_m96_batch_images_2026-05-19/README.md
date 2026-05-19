# Pro M94-M96 Batch Reference Images

Date: 2026-05-19

This bundle contains the first wave of ChatGPT Pro reference images for the M94-M96 visual-production slice.

## Contents

- 15 reference-only PNG images
- 5 contact sheets
- 5 class prompt packs
- `manifest.json`

## Classes Included

- `weapon`: `hero_rifle` front / side / three_quarter
- `humanoid`: `enemy_tactical` front / back / three_quarter
- `gear`: `vest` front / side / three_quarter
- `loot`: `medkit` top / front / three_quarter
- `environment_prop`: `shipping_container` front / side / three_quarter

## Critical Non-Claims

These images are not production assets.

They are quarantine reference images for local Hunyuan3D, Tripo, TRELLIS, Blender, or Unity concept review workflows.

Do not count any image as production-ready until local pipeline proves:

1. image-to-3D generation or hand modeling,
2. Blender cleanup/scale/pivot/LOD/collider readiness,
3. Unity import,
4. material/PBR sidecar,
5. semantic class review,
6. gameplay binding,
7. player-camera visibility,
8. gameplay event evidence,
9. promotion ledger update.

## Suggested Import Path

```text
external/pro_outputs/m94_m96_batch_images_2026-05-19/
```

Then run:

```text
AI Tools/Run M94 Generated Batch Class Promotion Gate
AI Tools/Run M95 Final Weapon Art Review Gate
AI Tools/Run M96 Final Humanoid Art Review Gate
```

The gates should not pass until local GLB/import/semantic/gameplay evidence exists.
