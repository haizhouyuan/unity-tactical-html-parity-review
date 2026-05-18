#!/usr/bin/env python3
"""Generate deterministic corrected Realified loot GLBs for M93.

Run with Blender:

  /Applications/Blender.app/Contents/MacOS/Blender --background --python \
    tools/m93_generate_corrected_loot_assets.py -- --repo-root .

The output intentionally replaces only the two loot stems that had category
failures in the Realified contact sheet:

  RS_08_loot_ammo
  RS_09_loot_medkit

This is a deterministic corrected fallback slice. It is not evidence that the
full AI-generated Realified batch is fixed.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import math
import os
import struct
import sys
import zlib
from datetime import datetime, timezone
from pathlib import Path

import bpy


STEMS = ("RS_08_loot_ammo", "RS_09_loot_medkit")
LOD_SUFFIXES = ("LOD0", "LOD1", "LOD2")


def parse_args() -> argparse.Namespace:
    argv = sys.argv
    if "--" in argv:
        argv = argv[argv.index("--") + 1 :]
    else:
        argv = []
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", default=".", help="unity-tactical-html-parity-review root")
    return parser.parse_args(argv)


def sha256(path: Path) -> str | None:
    if not path.exists():
        return None
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def write_png(path: Path, width: int, height: int, color: tuple[int, int, int]) -> None:
    """Write a small RGB PNG using only the Python standard library."""
    path.parent.mkdir(parents=True, exist_ok=True)
    raw_rows = []
    for y in range(height):
        row = bytearray()
        row.append(0)
        for x in range(width):
            # A tiny deterministic checker/stripe variation prevents these
            # sidecars from looking like empty single-color placeholders.
            shade = 0
            if ((x // 32) + (y // 32)) % 2 == 0:
                shade = 10
            row.extend(
                (
                    max(0, min(255, color[0] + shade)),
                    max(0, min(255, color[1] + shade)),
                    max(0, min(255, color[2] + shade)),
                )
            )
        raw_rows.append(bytes(row))
    raw = b"".join(raw_rows)

    def chunk(kind: bytes, data: bytes) -> bytes:
        crc = zlib.crc32(kind + data) & 0xFFFFFFFF
        return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", crc)

    header = struct.pack(">IIBBBBB", width, height, 8, 2, 0, 0, 0)
    png = b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", header) + chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b"")
    path.write_bytes(png)


def reset_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()


def material(name: str, color: tuple[float, float, float, float], roughness: float = 0.6, metallic: float = 0.0):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf is not None:
        if "Base Color" in bsdf.inputs:
            bsdf.inputs["Base Color"].default_value = color
        if "Roughness" in bsdf.inputs:
            bsdf.inputs["Roughness"].default_value = roughness
        if "Metallic" in bsdf.inputs:
            bsdf.inputs["Metallic"].default_value = metallic
    mat.diffuse_color = color
    return mat


def cube(name: str, loc, scale, mat, bevel: float = 0.03):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if mat is not None:
        obj.data.materials.append(mat)
    if bevel > 0:
        mod = obj.modifiers.new("small bevel", "BEVEL")
        mod.width = bevel
        mod.segments = 2
        obj.modifiers.new("weighted normals", "WEIGHTED_NORMAL")
    return obj


def cylinder(name: str, loc, radius: float, depth: float, mat, rotation=(0.0, 0.0, 0.0), vertices: int = 24):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=loc, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    if mat is not None:
        obj.data.materials.append(mat)
    obj.modifiers.new("weighted normals", "WEIGHTED_NORMAL")
    return obj


def add_ammo_asset(detail: int) -> list[bpy.types.Object]:
    reset_scene()
    mats = {
        "case": material("M93_ammo_olive_case", (0.16, 0.23, 0.15, 1), 0.74, 0.0),
        "edge": material("M93_ammo_black_rubber", (0.02, 0.025, 0.02, 1), 0.82, 0.0),
        "metal": material("M93_ammo_dark_metal", (0.10, 0.11, 0.11, 1), 0.42, 0.15),
        "brass": material("M93_ammo_brass_rounds", (0.93, 0.72, 0.30, 1), 0.32, 0.45),
        "label": material("M93_ammo_yellow_label", (0.96, 0.84, 0.18, 1), 0.55, 0.0),
    }
    objects: list[bpy.types.Object] = []
    objects.append(cube("ammo_box_body", (0, 0, 0.32), (1.8, 1.0, 0.64), mats["case"], 0.055))
    objects.append(cube("ammo_box_lid", (0, 0, 0.72), (1.9, 1.08, 0.14), mats["edge"], 0.035))
    objects.append(cube("ammo_front_label", (0, -0.516, 0.43), (0.92, 0.025, 0.28), mats["label"], 0.004))
    objects.append(cube("ammo_handle_base", (0, 0, 0.91), (0.78, 0.14, 0.10), mats["edge"], 0.025))
    if detail <= 1:
        objects.append(cube("ammo_latch_left", (-0.58, -0.54, 0.62), (0.18, 0.055, 0.14), mats["metal"], 0.012))
        objects.append(cube("ammo_latch_right", (0.58, -0.54, 0.62), (0.18, 0.055, 0.14), mats["metal"], 0.012))
        objects.append(cube("ammo_corner_band_left", (-0.91, 0, 0.36), (0.08, 1.12, 0.64), mats["edge"], 0.014))
        objects.append(cube("ammo_corner_band_right", (0.91, 0, 0.36), (0.08, 1.12, 0.64), mats["edge"], 0.014))
    if detail == 0:
        for index, x in enumerate([-0.45, -0.27, -0.09, 0.09, 0.27, 0.45]):
            objects.append(cylinder(f"visible_brass_round_{index}", (x, -0.66, 0.20), 0.055, 0.34, mats["brass"], (math.radians(90), 0, 0), 24))
            objects.append(cylinder(f"round_tip_{index}", (x, -0.84, 0.20), 0.05, 0.06, mats["metal"], (math.radians(90), 0, 0), 24))
        objects.append(cube("ammo_side_mark_a", (-0.98, 0.0, 0.54), (0.025, 0.55, 0.08), mats["label"], 0.004))
        objects.append(cube("ammo_side_mark_b", (0.98, 0.0, 0.54), (0.025, 0.55, 0.08), mats["label"], 0.004))
    return objects


def add_medkit_asset(detail: int) -> list[bpy.types.Object]:
    reset_scene()
    mats = {
        "case": material("M93_medkit_deep_red_case", (0.78, 0.08, 0.075, 1), 0.68, 0.0),
        "soft": material("M93_medkit_fabric_dark_red", (0.36, 0.035, 0.035, 1), 0.86, 0.0),
        "white": material("M93_medkit_white_cross", (0.96, 0.94, 0.88, 1), 0.58, 0.0),
        "strap": material("M93_medkit_black_strap", (0.025, 0.02, 0.018, 1), 0.75, 0.0),
        "metal": material("M93_medkit_buckle_metal", (0.20, 0.21, 0.20, 1), 0.38, 0.25),
    }
    objects: list[bpy.types.Object] = []
    objects.append(cube("medkit_case_body", (0, 0, 0.33), (1.48, 0.92, 0.66), mats["case"], 0.07))
    objects.append(cube("medkit_soft_lid", (0, 0, 0.76), (1.56, 0.98, 0.18), mats["soft"], 0.045))
    objects.append(cube("medkit_front_cross_vertical", (0, -0.471, 0.43), (0.16, 0.025, 0.42), mats["white"], 0.003))
    objects.append(cube("medkit_front_cross_horizontal", (0, -0.474, 0.43), (0.45, 0.025, 0.15), mats["white"], 0.003))
    objects.append(cube("medkit_top_cross_vertical", (0, 0, 0.855), (0.16, 0.54, 0.025), mats["white"], 0.003))
    objects.append(cube("medkit_top_cross_horizontal", (0, 0, 0.858), (0.54, 0.16, 0.025), mats["white"], 0.003))
    objects.append(cube("medkit_handle", (0, 0, 0.99), (0.68, 0.13, 0.13), mats["strap"], 0.025))
    if detail <= 1:
        objects.append(cube("medkit_vertical_strap_left", (-0.56, 0, 0.46), (0.08, 1.02, 0.72), mats["strap"], 0.012))
        objects.append(cube("medkit_vertical_strap_right", (0.56, 0, 0.46), (0.08, 1.02, 0.72), mats["strap"], 0.012))
        objects.append(cube("medkit_buckle_left", (-0.56, -0.51, 0.56), (0.18, 0.05, 0.13), mats["metal"], 0.01))
        objects.append(cube("medkit_buckle_right", (0.56, -0.51, 0.56), (0.18, 0.05, 0.13), mats["metal"], 0.01))
    if detail == 0:
        objects.append(cube("medkit_side_patch_left", (-0.77, 0, 0.44), (0.025, 0.34, 0.22), mats["white"], 0.003))
        objects.append(cube("medkit_side_patch_right", (0.77, 0, 0.44), (0.025, 0.34, 0.22), mats["white"], 0.003))
        for index, x in enumerate([-0.34, -0.17, 0.17, 0.34]):
            objects.append(cube(f"medkit_lid_seam_{index}", (x, 0.50, 0.77), (0.035, 0.045, 0.12), mats["metal"], 0.004))
    return objects


def export_glb(objects: list[bpy.types.Object], output_path: Path) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.export_scene.gltf(
        filepath=str(output_path),
        export_format="GLB",
        use_selection=True,
        export_apply=True,
        export_materials="EXPORT",
        export_copyright="M93 deterministic corrected fallback",
    )


def generate_sidecars(asset_root: Path, stem: str, base_color: tuple[int, int, int]) -> list[dict[str, str]]:
    sidecars = []
    texture_specs = {
        "basecolor": base_color,
        "normal": (128, 128, 255),
        "roughness": (170, 170, 170),
        "metallic": (24, 24, 24),
        "ao": (230, 230, 230),
    }
    for lod in LOD_SUFFIXES:
        for token, color in texture_specs.items():
            path = asset_root / f"{stem}_{lod}_{token}.png"
            before = sha256(path)
            write_png(path, 256, 256, color)
            after = sha256(path)
            sidecars.append({"path": path.name, "before_sha256": before, "after_sha256": after})
    return sidecars


def main() -> int:
    args = parse_args()
    repo_root = Path(args.repo_root).resolve()
    asset_root = repo_root / "Assets" / "HtmlTacticalAssets" / "RealifiedAssets"
    docs_root = repo_root / "docs"
    docs_root.mkdir(parents=True, exist_ok=True)

    records = []
    for stem in STEMS:
        old_hashes = {}
        for lod in LOD_SUFFIXES:
            old_hashes[f"{stem}_{lod}.glb"] = sha256(asset_root / f"{stem}_{lod}.glb")
        old_hashes[f"{stem}_textured.glb"] = sha256(asset_root / f"{stem}_textured.glb")

        if stem == "RS_08_loot_ammo":
            builder = add_ammo_asset
            base_color = (54, 83, 44)
            semantic_shape = "olive ammo crate with visible brass rounds, latches, handle, and yellow labels"
        else:
            builder = add_medkit_asset
            base_color = (178, 30, 26)
            semantic_shape = "red medkit case with white cross, straps, buckles, and handle"

        exported = []
        for detail, lod in enumerate(LOD_SUFFIXES):
            objects = builder(detail)
            path = asset_root / f"{stem}_{lod}.glb"
            export_glb(objects, path)
            exported.append({"path": path.name, "sha256": sha256(path), "bytes": path.stat().st_size})

        # The old pipeline also has a textured probe file. Use the corrected
        # LOD0 shape there as well so future audits do not rediscover a weapon
        # silhouette under the same loot stem.
        objects = builder(0)
        textured_path = asset_root / f"{stem}_textured.glb"
        export_glb(objects, textured_path)
        exported.append({"path": textured_path.name, "sha256": sha256(textured_path), "bytes": textured_path.stat().st_size})

        sidecars = generate_sidecars(asset_root, stem, base_color)

        records.append(
            {
                "stem": stem,
                "semantic_target": "loot",
                "semantic_shape": semantic_shape,
                "old_hashes": old_hashes,
                "exported": exported,
                "sidecars": sidecars,
            }
        )

    report = {
        "schema": "m93_corrected_loot_generation_trace_v1",
        "timestamp_utc": datetime.now(timezone.utc).isoformat(),
        "scope": "Replace only RS_08_loot_ammo and RS_09_loot_medkit with deterministic category-correct loot GLBs.",
        "completion_claim": "corrected_fallback_assets_generated_not_batch_promotion",
        "not_claimed": [
            "does_not_claim_full_realified_batch_fixed",
            "does_not_claim_semantic_review_passed",
            "does_not_claim_gameplay_promotion_passed",
            "does_not_claim_full_visual_asset_gate_passed",
        ],
        "source_context": [
            "M93 root-cause review found these two Unity Realified loot GLBs rendered as weapon silhouettes.",
            "Existing loot_set_v1 reference/evidence in ai-game-generation-research shows loot-like medkit/ammo cases, not rifles.",
        ],
        "records": records,
    }
    (docs_root / "M93_CORRECTED_LOOT_GENERATION_TRACE.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
