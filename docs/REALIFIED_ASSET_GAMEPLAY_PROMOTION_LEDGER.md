# Realified Asset Gameplay Promotion Ledger

Generated: 2026-05-19T00:51:33.8070530Z

- Technical + semantic + scene partial assets: `10 / 12`
- Production-promoted assets: `7 / 12`

An asset is production-promoted only when it is imported, technically ready, semantically matched, attached to a gameplay entity, visible from the player camera, and tied to a gameplay event.

| Asset | Technical | Semantic | Scene Entity | Player Camera | Gameplay Event | Promoted | Blockers |
|---|---:|---:|---:|---:|---:|---:|---|
| hero_rifle | True | True | True | True | True | True |  |
| sidearm | True | True | True | True | False | False | no_asset_specific_gameplay_event_evidence |
| secondary_weapon | True | True | True | False | False | False | no_player_camera_visible_realified_asset_evidence, no_asset_specific_gameplay_event_evidence |
| shotgun | False | True | False | False | False | False | technical_import_or_pbr_sidecar_incomplete, no_gameplay_entity_scene_evidence, no_player_camera_visible_realified_asset_evidence, no_asset_specific_gameplay_event_evidence |
| helmet | True | True | True | False | False | False | no_player_camera_visible_realified_asset_evidence, no_asset_specific_gameplay_event_evidence |
| vest | True | True | True | True | True | True |  |
| ammo | True | True | True | True | True | True |  |
| medkit | True | True | True | True | True | True |  |
| container | True | True | True | True | True | True |  |
| crate | True | False | False | False | False | False | semantic_category_mismatch_or_failed_review, no_gameplay_entity_scene_evidence, no_player_camera_visible_realified_asset_evidence, no_asset_specific_gameplay_event_evidence |
| player_tactical | True | True | True | True | True | True |  |
| enemy_tactical | True | True | True | True | True | True |  |
