# M110 Asset-Chain Quality Review

**Reviewer:** Kimi (asset-pipeline reviewer)  
**Date:** 2026-05-19  
**Mission:** M110_parallel_img2_3d_visible_assets  
**Scope:** Review 9 PRO clean-background reference images, assess 2 existing M106B textured outputs, define inspection checklist for 2 active M110 GPU jobs, and route next visible-value work.

---

## Executive Verdict

**Status: `partial`** — The pipeline is technically running, but the asset-chain has fundamental structural problems.

- **Reference image quality is good** — ChatGPT PRO generated clean, white-background references with tactical PUBG-like semantics.
- **Reference selection for image-to-3D is poor** — The M106B probe used a front-view humanoid (worst angle for single-image reconstruction). Humanoids should not be routed through Hunyuan3D single-image-to-3D at all.
- **The two active M110 jobs are on the right reference images** — side-profile rifle and 3/4 checkpoint booth are the best candidates in the batch.
- **Unity screenshots (M100/M108) reveal the real problem** — the in-game visual is primitive placeholder geometry (cyan cylinders), not a realistic weapon. Passing JSON gates does not mean visible value is delivered.
- **No evidence renders exist** for M106B outputs. We are flying blind on whether the textured outputs are even importable.

**Bottom line:** Even if M110 jobs succeed, the project still needs (1) Blender cleanup + evidence renders, (2) Unity quarantine import + lighting review, and (3) a realistic humanoid strategy that does not rely on single-image-to-3D.

---

## Reference Image Acceptance / Rejection

| Filename | Asset Class | Verdict | Rationale |
|---|---|---|---|
| `pro_clean_ref_hero_rifle_side_001.png` | weapon | **ACCEPT** | Canonical side profile; clean white bg; full silhouette visible; no self-occlusion. Best possible input for single-image-to-3D. |
| `pro_clean_ref_hero_rifle_perspective_001.png` | weapon | **REJECT** | Perspective angle adds distortion. Duplicate content exists (`pro_clean_ref_hero_rifle_perspective_002.png`). Side view is strictly superior. |
| `pro_clean_ref_hero_rifle_perspective_002.png` | weapon | **REJECT** | Same perspective-distortion issue. Duplicates exist. Not needed when side view is available. |
| `pro_clean_ref_enemy_tactical_front_001.png` | humanoid | **REJECT** | Front view = 100% back-side occlusion. Hunyuan3D will hallucinate anatomy poorly. Full-body humanoid is the hardest class for image-to-3D. Already used in M106B with no evidence render to prove success. |
| `pro_clean_ref_enemy_tactical_ready_001.png` | humanoid | **REJECT** | Ready stance with weapon occluding torso. Dynamic pose + occlusion = guaranteed artifacts. |
| `pro_clean_ref_enemy_tactical_hit_down_001.png` | humanoid | **REJECT** | Extreme dynamic pose (falling, limbs splayed). Single-image-to-3D will produce broken topology. Absolutely unsuitable. |
| `pro_clean_ref_container_cover_001.png` | environment_cover | **ACCEPT** | Clean bg, 3/4 angle, readable geometry. Open-top container barricade is a good cover prop. Already used in M106B with textured output present. |
| `pro_clean_ref_container_side_001.png` | environment_cover | **ACCEPT** | Flat side view, simple corrugated geometry, no interior cavities. Very easy for image-to-3D. Good modular wall piece. |
| `pro_clean_ref_checkpoint_booth_001.png` | environment_building | **ACCEPT** | Clean bg, 3/4 facade angle. Complex interior cavity may confuse image-to-3D, but as a static environment prop it is worth trying. Currently running on M110 GPU1. |

**Accepted: 4** | **Rejected: 5**

### Rejected Image Disposition
- Do not queue rejected images for GPU jobs.
- Front-view humanoids should be routed to a **multi-view or base-mesh rigging pipeline**, not Hunyuan3D single-image.
- If PRO can provide orthogonal sets (side + front + back + top) for humanoids, reconsider.

---

## M110 Inspection Checklist

After GPU0 (`m110_hero_rifle_side_img2_3d_001`) and GPU1 (`m110_checkpoint_booth_img2_3d_001`) finish, Codex must verify:

1. **Job completion artifacts**
   - `job_report.json` exists in the output directory.
   - `shape_out_name` GLB exists and `sha256` is recorded.
   - `textured_out_name` GLB/OBJ exists.

2. **File-size sanity**
   - Rifle shape GLB: expected 300 KB – 2 MB. < 200 KB = likely collapsed geometry.
   - Rifle textured GLB: expected 1 MB – 10 MB. < 500 KB = likely missing textures.
   - Booth shape GLB: expected 500 KB – 3 MB.
   - Booth textured GLB: expected 2 MB – 15 MB (larger due to interior faces).

3. **Texture-map completeness**
   - Albedo/diffuse map present and ≥ 1024 px.
   - Metallic map present.
   - Roughness map present.
   - No pure-black or pure-white texture anomalies.

4. **Geometry sanity (Blender headless or manual open)**
   - Triangle count: rifle 5K–50K; booth 10K–80K.
   - No 2-manifold holes (watertight check).
   - No floating disconnected vertices.
   - For booth: interior cavity is either properly reconstructed or safely filled; open holes are acceptable only if backface culling is off in Unity.

5. **Semantic fidelity**
   - Rifle: stock, receiver, barrel, magazine, and optic are all identifiable and proportionally correct.
   - Booth: container-frame silhouette, window openings, and roof overhang are recognizable.

6. **Evidence render**
   - Generate `blender_preview.png` (or equivalent) showing the textured model under neutral lighting.
   - If evidence render cannot be produced, quarantine the asset until it can.

7. **Cross-check against source**
   - Compare proportions to the reference image. Severe distortion (e.g., compressed barrel, bloated stock) = rerun.

---

## Bind / Quarantine / Rerun Decision Tree

```
For each M110 output (and existing M106B outputs):

1. Did shape + paint stages both complete with exit code 0?
   └─ NO  → RERUN (same source, same GPU, one retry)

2. Are all 6 checklist artifacts present (report, shape GLB, textured GLB,
   albedo, metallic, roughness) and file sizes in expected ranges?
   └─ NO  → QUARANTINE (mark as "incomplete_packet" and block bind)

3. Does Blender evidence render show recognizable semantics with no severe
   artifacts (holes, extrusions, disconnected parts)?
   └─ NO  → QUARANTINE (mark as "geometry_doubt" and request manual review)

4. Is triangle count reasonable and texture resolution ≥ 1024 px?
   └─ NO  → QUARANTINE (mark as "quality_below_threshold")

5. Does the model match reference proportions within ±20%?
   └─ NO  → RERUN (if second fail, REJECT SOURCE)

6. All checks pass?
   └─ YES → BIND to Unity quarantine import queue
            (copy to /Users/yuanshaochen/My project/Assets/Generated/Quarantine/M110/)
```

### Decision Definitions

| Action | Definition | Who |
|---|---|---|
| **BIND** | Asset packet is complete, evidence render looks correct, and it is queued for Unity quarantine import. | Codex |
| **QUARANTINE** | Asset exists but has incomplete evidence, geometry doubts, or quality below threshold. It stays in the asset packet and may be imported to Unity for visual review, but is **not** promoted to production. | Codex |
| **RERUN** | Job failed or output is severely flawed. Run the same source image through Hunyuan3D one more time (different random seed implied). If second rerun also fails, permanently reject the source image. | HomePC GPU |
| **REJECT SOURCE** | Source reference image is fundamentally unsuitable for image-to-3D (front humanoid, dynamic pose, extreme occlusion). Remove from GPU queue. | Kimi reviewer |

### Existing M106B Routing

| Asset | Current State | Decision |
|---|---|---|
| `enemy_tactical_pro_clean_img2_3d_001` | Shape + paint outputs present, NO evidence render, front-view humanoid source | **QUARANTINE** — do not promote. Generate Blender preview first. If preview shows broken anatomy, reject source. |
| `container_cover_pro_clean_img2_3d_001` | Shape + paint outputs present, NO evidence render, 3/4 cover source | **QUARANTINE** — generate Blender preview, then bind if preview is acceptable. |

---

## Next Unity Visible-Value Mission (M111)

**Goal:** Prove that M110 assets actually improve the in-game screenshot, not just the JSON gate.

**Minimal scope:**

1. **Quarantine import**
   - Create `Assets/Generated/Quarantine/M110/`.
   - Import M110 rifle + booth GLBs.
   - Import M106B container_cover GLB (if evidence pass).
   - Do **not** import M106B enemy_tactical until evidence render is done.

2. **Lighting pass**
   - Add a directional light (warm, sunset angle) + ambient probe.
   - Current M100 screenshots are under-lit and flat. Better lighting alone will improve perceived quality.

3. **Combat slice scene**
   - Re-use or extend the debug slice from M108.
   - Place container_cover as cover prop.
   - Place checkpoint_booth as building facade.
   - Spawn player facing a readable courtyard (not a flat wall).

4. **Weapon binding**
   - Bind M110 hero rifle to first-person viewmodel mount.
   - Use the existing `M100HeroWeaponViewmodelController` but with the **real** textured asset.
   - Adjust per-asset offset/scale/orientation config so the rifle looks correct in idle/ADS/fire/reload.

5. **Screenshot proof**
   - Capture: `01_idle`, `02_ads`, `03_fire`, `04_reload`, `05_environment_wide`.
   - Compare side-by-side with M100 screenshots.
   - If the rifle still looks like a primitive cylinder, the asset-chain failed even if the model technically imported.

**Deliverable:** `docs/M111_unity_visible_value_proof.json` + screenshot set.

---

## Next PRO Request

**Trigger condition:** If M110 results are weak (likely, given image-to-3D limitations) OR if M111 screenshots still look primitive.

**Request to ChatGPT PRO:**

> "The single-image-to-3D route is producing models, but Unity screenshots still look primitive. We need higher-fidelity inputs and a multi-view strategy. Please generate:
>
> 1. **Orthogonal reference sets** for the 4 accepted assets:
>    - For rifle: pure side view (already have) + top view + front view + back view.
>    - For container/cover: side + top + front.
>    - For checkpoint booth: front facade + side + top-down.
>    - Each on clean white background, same scale, consistent lighting.
>
> 2. **Texture detail sheets** for the accepted assets:
>    - Close-up of metal wear patterns, rust, paint chips.
>    - These are not for image-to-3D directly; they are for manual texture touch-up in Blender/Substance after generation.
>
> 3. **Humanoid strategy pivot:**
>    - Do NOT generate more single-view humanoid references for image-to-3D.
>    - Instead, provide an A-pose front + A-pose side + A-pose back orthogonal set.
>    - OR provide a concept for a simplified tactical mannequin (low-poly base mesh) that we can texture-map rather than reconstruct from image.
>
> 4. **Modular environment kit pieces:**
>    - Instead of complex assembled props (booth with interior), provide simple modular pieces:
>      - Container wall panel (flat, 2m x 3m)
>      - Container corner post
>      - Corrugated roof panel
>      - Simple sandbag/stacked-crate cover module
>    - These are easier for image-to-3D and can be assembled in Unity."

**Why this request:** Single-image-to-3D from one perspective will always produce back-side hallucinations. Multi-view inputs (Hunyuan3D supports multi-view conditioning) dramatically improve reconstruction accuracy. Modular pieces reduce complexity per job. And humanoids need a different pipeline entirely.

---

## Blockers

| Blocker | Severity | Mitigation |
|---|---|---|
| No Blender evidence renders for any existing textured output | High | Codex must run Blender headless preview on M106B outputs before M111 import. |
| Humanoid source images are unsuitable for single-image-to-3D | High | Reject all 3 humanoid refs from GPU queue. Pivot to multi-view or base-mesh. |
| Unity screenshots still show primitive placeholder weapon | High | M111 must prove real asset import + proper viewmodel config. JSON gates are not enough. |
| M110 jobs are running but no logs are being streamed to reviewer | Medium | Codex must retrieve `job_report.json` from HomePC after completion. |
| No texture detail references for manual touch-up | Low | Address in next PRO request if M111 still weak. |

---

*This review is read-only and does not modify GPU jobs, Unity project files, or promote any asset to production.*
