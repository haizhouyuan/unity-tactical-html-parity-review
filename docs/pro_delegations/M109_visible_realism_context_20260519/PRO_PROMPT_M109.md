M109 request: turn the current M108 visible-value slice into a real visible upgrade, not another gate.

Ground truth from local Codex/Unity run:
- Active Unity project: `/Users/yuanshaochen/My project`
- Unity version: 6000.4.7f1
- MCP route: CoplayDev/community Unity MCP at `http://127.0.0.1:8080/mcp`
- Official Unity MCP is no longer used.
- Latest local M108 proof path in this bundle: `reports/M108_VISIBLE_VALUE_SLICE_PROOF.json`
- Latest M108 proof passed after local Codex tuning:
  - `passed: true`
  - `weapon_screen_area_ratio: 0.219`
  - `muzzle_forward_check: true`
  - `enemy_visual_non_proxy: true`
  - `combat_courtyard_visible: true`
  - `blockers: []`
- But the screenshots in `screenshots/` prove the visual result is still far from PUBG-like:
  - first-person weapon is still a primitive/kitbashed viewmodel, not a convincing tactical rifle;
  - fire/ADS/reload are visible but not high quality;
  - combat courtyard framing is still poor, with wall/sky/flat grey composition;
  - enemy is not a production humanoid, it is still a simple authored proxy;
  - the debug camera/spawn is better than before but not a compelling player-view screenshot.

Local asset-factory state:
- Hunyuan image-to-3D first round has produced textured GLBs for:
  - `enemy_tactical_pro_clean_img2_3d_001`
  - `container_cover_pro_clean_img2_3d_001`
- Both pipelines ended with a Blender/conda teardown segfault, but artifacts exist and SHA256 hashes are in:
  - `reports/enemy_sha256_paint_stage.txt`
  - `reports/container_sha256_paint_stage.txt`
- These assets were copied into Unity quarantine:
  - `Assets/Generated/Quarantine/M106B/enemy_tactical_pro_clean_img2_3d_001/hunyuan_textured_raw.glb`
  - `Assets/Generated/Quarantine/M106B/container_cover_pro_clean_img2_3d_001/hunyuan_textured_raw.glb`
- Unity refresh/import ran successfully with no console errors (`reports/M106B_QUARANTINE_IMPORT_REFRESH.json`).

What I need from you:
Generate a downloadable patch package and standalone `.patch` for `M109 Player-Visible Realism Slice`.

Do not just add another report/gate. I need a patch that makes the next Unity screenshot visibly better.

Please implement a focused Unity/C# patch that:
1. Creates a much better debug/player-view composition for the visible slice:
   - clean open tactical courtyard view;
   - no wall/ceiling blocking the camera;
   - camera has a clear foreground weapon, midground cover/loot, background building/enemy;
   - lighting/color/materials less grey-box, more tactical game screenshot.
2. Reworks the first-person weapon visual:
   - keep current gameplay logic untouched;
   - build a more readable tactical rifle silhouette with receiver, rail, optic, barrel, magazine, grip, stock;
   - add stronger muzzle flash/tracer/casing/hit marker/reload pose;
   - avoid the weapon being backward, under the camera, too flat, or clipped.
3. Uses the quarantine Hunyuan assets opportunistically:
   - if Unity can load `Assets/Generated/Quarantine/M106B/.../hunyuan_textured_raw.glb`, instantiate it as visual detail for enemy/container slice;
   - if not, fall back to authored visual but report clearly that Hunyuan detail was unavailable.
4. Improves the enemy visual slice:
   - player camera must see a tactical enemy silhouette clearly;
   - add at least idle/aim/hit/down states;
   - use Hunyuan enemy GLB as detail if available.
5. Improves the combat courtyard:
   - use container cover, loot prop, building facade, wet ground/decal/clutter/lighting;
   - use Hunyuan container GLB as detail if available;
   - the screenshot should look like a playable combat micro-area, not a documentation gate.
6. Outputs JSON and screenshots:
   - `AI Tools/Install M109 Player Visible Realism Slice`
   - `AI Tools/Run M109 Player Visible Realism Proof`
   - output report: `docs/M109_PLAYER_VISIBLE_REALISM_PROOF.json`
   - screenshots: `Assets/Screenshots/M109PlayerVisibleRealism/`

Constraints:
- Do not edit `.unity` scene YAML directly.
- Do not modify Packages or ProjectSettings.
- Do not change existing M88/M95/M96 pass/fail JSONs.
- Do not claim final art or production promotion.
- Patch should be reviewable source, not opaque binaries.
- PRO cannot verify Unity; Codex will apply locally and run community MCP.

Files included in this bundle:
- `code/M108VisibleValueSliceTool.cs`
- `code/M108FirstPersonWeaponComposer.cs`
- `code/M108EnemyVisualSlice.cs`
- `code/current_m108_local_diff.patch`
- all current M108 screenshots
- current M108 proof JSON
- M106B Unity import refresh report
- Hunyuan enemy/container SHA reports
- clean reference images used for image-to-3D

Desired output:
- `M109_player_visible_realism_slice_patch.zip`
- `M109_player_visible_realism_slice.patch`
- `README_FOR_PATCH.md`
- `changed_files_manifest.json`
- `risk_notes.md`
- `validation_notes.md`
- optional `visual_review_notes.md` explaining what visual problem your patch is solving.
