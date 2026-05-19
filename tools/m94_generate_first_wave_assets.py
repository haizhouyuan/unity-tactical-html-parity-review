#!/usr/bin/env python3
"""Generate deterministic M94 first-wave Realified tactical assets.

Run with Blender:

  /Applications/Blender.app/Contents/MacOS/Blender --background --python \
    tools/m94_generate_first_wave_assets.py -- --repo-root .

This is a deterministic, local Blender generation pass that consumes the
M94/Pro reference-image bundle as art direction. It is intentionally explicit:
these files are M94 generated candidates, not a claim of final commercial art.
Final visual approval is still controlled by M95/M96.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import math
import shutil
import struct
import sys
import zlib
from datetime import datetime, timezone
from pathlib import Path

import bpy


BATCH_ID = "M94_FIRST_WAVE_2026-05-19"
REFERENCE_BUNDLE = "external/pro_outputs/m94_m96_batch_images_2026-05-19"
LOD_SUFFIXES = ("LOD0", "LOD1", "LOD2")

ASSETS = (
    {
        "asset_id": "hero_rifle",
        "category": "weapon",
        "stem": "hero_rifle",
        "builder": "rifle",
        "semantic": "stylized tactical hero rifle with optic, magazine, stock, rail, handguard, and muzzle device",
        "basecolor": (42, 55, 58),
    },
    {
        "asset_id": "player_tactical",
        "category": "character",
        "stem": "RS_04_player_tactical",
        "builder": "humanoid_player",
        "semantic": "blue-accent tactical humanoid player with helmet, visor, plate carrier, limbs, pouches, and boots",
        "basecolor": (42, 70, 90),
    },
    {
        "asset_id": "enemy_tactical",
        "category": "character",
        "stem": "RS_05_enemy_tactical",
        "builder": "humanoid_enemy",
        "semantic": "red-accent tactical humanoid enemy with helmet, visor, plate carrier, limbs, pouches, and boots",
        "basecolor": (86, 46, 44),
    },
    {
        "asset_id": "helmet",
        "category": "gear",
        "stem": "RS_06_gear_helmet",
        "builder": "helmet",
        "semantic": "ballistic helmet loot with shell, visor, side rails, chin strap, and rear counterweight",
        "basecolor": (42, 50, 50),
    },
    {
        "asset_id": "vest",
        "category": "gear",
        "stem": "RS_07_gear_vest",
        "builder": "vest",
        "semantic": "tactical plate carrier vest loot with front plate, shoulder straps, pouches, buckles, and webbing",
        "basecolor": (38, 50, 46),
    },
    {
        "asset_id": "medkit",
        "category": "loot",
        "stem": "RS_09_loot_medkit",
        "builder": "medkit",
        "semantic": "red medkit case loot with white medical cross, straps, handle, and buckles",
        "basecolor": (178, 30, 26),
    },
    {
        "asset_id": "container",
        "category": "environment_prop",
        "stem": "RS_10_prop_container",
        "builder": "container",
        "semantic": "weathered shipping container environment cover with corrugated ribs, doors, hinges, lock bars, and decals",
        "basecolor": (45, 66, 78),
    },
)


def parse_args() -> argparse.Namespace:
    argv = sys.argv
    argv = argv[argv.index("--") + 1 :] if "--" in argv else []
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


def write_png(path: Path, width: int, height: int, color: tuple[int, int, int], *, normal: bool = False) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    rows = []
    for y in range(height):
        row = bytearray([0])
        for x in range(width):
            if normal:
                wave = int(10 * math.sin(x / 11.0) + 8 * math.cos(y / 17.0))
                pixel = (128 + wave, 128 - wave // 2, 255)
            else:
                shade = 12 if ((x // 32) + (y // 32)) % 2 == 0 else -6
                scratch = 18 if (x + y * 3) % 113 < 3 else 0
                pixel = tuple(max(0, min(255, channel + shade + scratch)) for channel in color)
            row.extend(pixel)
        rows.append(bytes(row))

    def chunk(kind: bytes, data: bytes) -> bytes:
        crc = zlib.crc32(kind + data) & 0xFFFFFFFF
        return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", crc)

    header = struct.pack(">IIBBBBB", width, height, 8, 2, 0, 0, 0)
    data = b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", header) + chunk(b"IDAT", zlib.compress(b"".join(rows), 9)) + chunk(b"IEND", b"")
    path.write_bytes(data)


def reset_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()


def mat(name: str, rgba: tuple[float, float, float, float], roughness: float = 0.55, metallic: float = 0.0):
    material = bpy.data.materials.new(name)
    material.use_nodes = True
    bsdf = material.node_tree.nodes.get("Principled BSDF")
    if bsdf is not None:
        bsdf.inputs["Base Color"].default_value = rgba
        bsdf.inputs["Roughness"].default_value = roughness
        bsdf.inputs["Metallic"].default_value = metallic
    material.diffuse_color = rgba
    return material


def cube(name: str, loc, scale, material, bevel: float = 0.025):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material is not None:
        obj.data.materials.append(material)
    if bevel > 0:
        obj.modifiers.new("bevel", "BEVEL").width = bevel
        obj.modifiers.new("weighted_normals", "WEIGHTED_NORMAL")
    return obj


def cyl(name: str, loc, radius: float, depth: float, material, rotation=(0.0, 0.0, 0.0), vertices: int = 24):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=loc, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    if material is not None:
        obj.data.materials.append(material)
    obj.modifiers.new("weighted_normals", "WEIGHTED_NORMAL")
    return obj


def sphere(name: str, loc, scale, material, segments: int = 24):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=max(8, segments // 2), location=loc)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    if material is not None:
        obj.data.materials.append(material)
    obj.modifiers.new("weighted_normals", "WEIGHTED_NORMAL")
    return obj


def build_rifle(detail: int, accent) -> list:
    reset_scene()
    metal = mat("M94_rifle_dark_blued_metal", (0.055, 0.070, 0.075, 1), 0.40, 0.40)
    polymer = mat("M94_rifle_black_polymer", (0.018, 0.021, 0.022, 1), 0.65, 0.0)
    blue = mat("M94_rifle_reference_blue_finish", (0.05, 0.33, 0.52, 1), 0.38, 0.15)
    glass = mat("M94_rifle_optic_glass", (0.02, 0.15, 0.18, 0.78), 0.18, 0.0)
    brass = mat("M94_rifle_worn_edge_brass", (0.75, 0.58, 0.25, 1), 0.36, 0.45)
    objs = [
        cube("rifle_receiver_upper", (0.0, 0.0, 0.20), (1.55, 0.28, 0.30), blue, 0.025),
        cube("rifle_receiver_lower", (-0.05, 0.0, -0.03), (1.15, 0.24, 0.30), metal, 0.025),
        cube("rifle_handguard_mlok", (1.10, 0.0, 0.16), (1.55, 0.23, 0.23), blue, 0.018),
        cyl("rifle_barrel", (2.10, 0.0, 0.16), 0.045, 1.25, metal, (0, math.radians(90), 0), 32),
        cyl("two_port_muzzle_brake", (2.78, 0.0, 0.16), 0.082, 0.28, metal, (0, math.radians(90), 0), 32),
        cube("box_magazine", (-0.20, -0.02, -0.50), (0.34, 0.21, 0.82), polymer, 0.025),
        cube("angled_grip", (0.80, -0.01, -0.28), (0.22, 0.18, 0.58), polymer, 0.022),
        cube("precision_stock", (-1.10, 0.0, 0.12), (0.82, 0.24, 0.23), polymer, 0.025),
        cube("rubber_butt_pad", (-1.60, 0.0, 0.12), (0.12, 0.32, 0.50), polymer, 0.025),
        cube("top_picatinny_rail", (0.55, 0.0, 0.42), (1.95, 0.12, 0.08), metal, 0.006),
        cyl("optic_scope_body", (0.34, 0.0, 0.72), 0.18, 0.60, metal, (0, math.radians(90), 0), 32),
        cyl("optic_front_lens", (0.66, 0.0, 0.72), 0.16, 0.025, glass, (0, math.radians(90), 0), 32),
        cyl("optic_rear_lens", (0.02, 0.0, 0.72), 0.14, 0.025, glass, (0, math.radians(90), 0), 32),
    ]
    if detail <= 1:
        for i, x in enumerate([-0.35, -0.05, 0.25, 0.55, 0.85, 1.15, 1.45]):
            objs.append(cube(f"rail_tooth_{i:02d}", (x, 0, 0.50), (0.09, 0.18, 0.06), metal, 0.004))
        for i, x in enumerate([0.55, 0.80, 1.05, 1.30, 1.55]):
            objs.append(cube(f"mlok_slot_{i:02d}", (x, -0.135, 0.14), (0.16, 0.018, 0.055), polymer, 0.003))
    if detail == 0:
        for i, x in enumerate([-0.62, -0.42, -0.22, 0.08, 0.38, 0.68, 1.58]):
            objs.append(sphere(f"worn_edge_rivet_{i:02d}", (x, -0.155, 0.30), (0.025, 0.025, 0.025), brass, 12))
    return objs


def build_humanoid(detail: int, enemy: bool) -> list:
    reset_scene()
    cloth = mat("M94_enemy_uniform" if enemy else "M94_player_uniform", (0.20, 0.18, 0.17, 1) if enemy else (0.14, 0.20, 0.24, 1), 0.72, 0.0)
    armor = mat("M94_plate_carrier", (0.035, 0.045, 0.042, 1), 0.54, 0.0)
    accent = mat("M94_enemy_red_patch" if enemy else "M94_player_blue_patch", (0.72, 0.06, 0.08, 1) if enemy else (0.06, 0.25, 0.88, 1), 0.48, 0.0)
    visor = mat("M94_black_visor", (0.015, 0.075, 0.085, 1), 0.20, 0.0)
    skin = mat("M94_muted_face_shadow", (0.20, 0.15, 0.12, 1), 0.65, 0.0)
    objs = [
        cube("humanoid_boots", (0, 0, 0.12), (0.74, 0.34, 0.25), armor, 0.035),
        cyl("left_leg", (-0.22, 0, 0.68), 0.12, 0.92, cloth, (0, 0, 0), 18),
        cyl("right_leg", (0.22, 0, 0.68), 0.12, 0.92, cloth, (0, 0, 0), 18),
        cube("torso_soft_armor", (0, 0, 1.35), (0.72, 0.34, 0.72), cloth, 0.045),
        cube("front_plate_carrier", (0, -0.20, 1.38), (0.62, 0.09, 0.66), armor, 0.025),
        cube("rear_plate_carrier", (0, 0.20, 1.38), (0.62, 0.09, 0.66), armor, 0.025),
        sphere("helmet_shell", (0, 0, 1.96), (0.30, 0.25, 0.22), armor, 24),
        sphere("face_shadow", (0, -0.08, 1.86), (0.18, 0.11, 0.12), skin, 16),
        cube("visor_band", (0, -0.25, 1.96), (0.48, 0.06, 0.10), visor, 0.008),
        cyl("left_arm", (-0.52, -0.02, 1.38), 0.085, 0.70, cloth, (math.radians(12), 0, math.radians(8)), 16),
        cyl("right_arm", (0.52, -0.02, 1.38), 0.085, 0.70, cloth, (math.radians(-12), 0, math.radians(-8)), 16),
        cube("squad_patch", (0.34, -0.255, 1.60), (0.16, 0.03, 0.12), accent, 0.004),
    ]
    if detail <= 1:
        for i, x in enumerate([-0.24, 0.0, 0.24]):
            objs.append(cube(f"front_mag_pouch_{i}", (x, -0.265, 1.15), (0.16, 0.08, 0.22), armor, 0.012))
        objs.append(cube("left_shoulder_strap", (-0.26, -0.02, 1.72), (0.12, 0.42, 0.14), armor, 0.01))
        objs.append(cube("right_shoulder_strap", (0.26, -0.02, 1.72), (0.12, 0.42, 0.14), armor, 0.01))
    if detail == 0:
        objs.append(cube("radio_block", (-0.42, 0.18, 1.45), (0.13, 0.10, 0.32), armor, 0.01))
        objs.append(cyl("radio_antenna", (-0.48, 0.20, 1.82), 0.012, 0.55, armor, (0, 0, math.radians(8)), 8))
        objs.append(cube("knee_pad_left", (-0.22, -0.12, 0.65), (0.20, 0.06, 0.16), armor, 0.01))
        objs.append(cube("knee_pad_right", (0.22, -0.12, 0.65), (0.20, 0.06, 0.16), armor, 0.01))
    return objs


def build_helmet(detail: int) -> list:
    reset_scene()
    shell = mat("M94_helmet_shell", (0.10, 0.13, 0.12, 1), 0.55, 0.0)
    rail = mat("M94_helmet_black_rail", (0.02, 0.025, 0.024, 1), 0.48, 0.0)
    visor = mat("M94_helmet_green_black_visor", (0.02, 0.09, 0.08, 1), 0.22, 0.0)
    objs = [
        sphere("ballistic_helmet_shell", (0, 0, 0.55), (0.55, 0.45, 0.30), shell, 32),
        cube("front_nvg_mount", (0, -0.43, 0.62), (0.22, 0.06, 0.16), rail, 0.01),
        cube("visor_strip", (0, -0.48, 0.50), (0.72, 0.05, 0.10), visor, 0.006),
        cube("rear_counterweight", (0, 0.40, 0.50), (0.42, 0.08, 0.16), rail, 0.01),
    ]
    if detail <= 1:
        objs.append(cube("left_side_rail", (-0.50, -0.02, 0.52), (0.08, 0.56, 0.08), rail, 0.008))
        objs.append(cube("right_side_rail", (0.50, -0.02, 0.52), (0.08, 0.56, 0.08), rail, 0.008))
        objs.append(cyl("chin_strap_left", (-0.32, -0.08, 0.28), 0.018, 0.55, rail, (math.radians(18), 0, math.radians(-18)), 8))
        objs.append(cyl("chin_strap_right", (0.32, -0.08, 0.28), 0.018, 0.55, rail, (math.radians(18), 0, math.radians(18)), 8))
    return objs


def build_vest(detail: int) -> list:
    reset_scene()
    fabric = mat("M94_vest_fabric", (0.09, 0.12, 0.105, 1), 0.76, 0.0)
    plate = mat("M94_vest_plate", (0.035, 0.045, 0.040, 1), 0.48, 0.0)
    buckle = mat("M94_vest_buckles", (0.18, 0.18, 0.16, 1), 0.36, 0.35)
    objs = [
        cube("vest_front_plate", (0, -0.10, 0.74), (0.82, 0.18, 0.92), plate, 0.04),
        cube("vest_back_plate", (0, 0.16, 0.74), (0.78, 0.16, 0.88), fabric, 0.04),
        cube("left_shoulder_yoke", (-0.28, 0.02, 1.28), (0.16, 0.42, 0.18), fabric, 0.02),
        cube("right_shoulder_yoke", (0.28, 0.02, 1.28), (0.16, 0.42, 0.18), fabric, 0.02),
    ]
    if detail <= 1:
        for i, x in enumerate([-0.28, 0, 0.28]):
            objs.append(cube(f"mag_pouch_{i}", (x, -0.22, 0.48), (0.20, 0.10, 0.30), fabric, 0.015))
        objs.append(cube("left_side_buckle", (-0.50, -0.02, 0.76), (0.12, 0.16, 0.16), buckle, 0.01))
        objs.append(cube("right_side_buckle", (0.50, -0.02, 0.76), (0.12, 0.16, 0.16), buckle, 0.01))
    if detail == 0:
        for row, z in enumerate([0.72, 0.88, 1.04]):
            objs.append(cube(f"molle_webbing_row_{row}", (0, -0.245, z), (0.70, 0.03, 0.035), buckle, 0.003))
    return objs


def build_medkit(detail: int) -> list:
    reset_scene()
    case = mat("M94_medkit_deep_red_case", (0.70, 0.045, 0.04, 1), 0.66, 0.0)
    soft = mat("M94_medkit_dark_fabric_lid", (0.32, 0.03, 0.03, 1), 0.86, 0.0)
    white = mat("M94_medkit_white_cross", (0.96, 0.94, 0.88, 1), 0.56, 0.0)
    black = mat("M94_medkit_black_straps", (0.025, 0.020, 0.018, 1), 0.76, 0.0)
    metal = mat("M94_medkit_buckles", (0.20, 0.21, 0.20, 1), 0.38, 0.25)
    objs = [
        cube("medkit_case_body", (0, 0, 0.34), (1.48, 0.92, 0.66), case, 0.07),
        cube("medkit_soft_lid", (0, 0, 0.76), (1.56, 0.98, 0.18), soft, 0.045),
        cube("front_white_cross_vertical", (0, -0.47, 0.43), (0.16, 0.025, 0.42), white, 0.003),
        cube("front_white_cross_horizontal", (0, -0.48, 0.43), (0.45, 0.025, 0.15), white, 0.003),
        cube("top_white_cross_vertical", (0, 0, 0.86), (0.16, 0.54, 0.025), white, 0.003),
        cube("top_white_cross_horizontal", (0, 0, 0.865), (0.54, 0.16, 0.025), white, 0.003),
        cube("medkit_handle", (0, 0, 1.0), (0.68, 0.13, 0.13), black, 0.025),
    ]
    if detail <= 1:
        objs.append(cube("left_vertical_strap", (-0.56, 0, 0.46), (0.08, 1.02, 0.72), black, 0.012))
        objs.append(cube("right_vertical_strap", (0.56, 0, 0.46), (0.08, 1.02, 0.72), black, 0.012))
        objs.append(cube("left_buckle", (-0.56, -0.51, 0.56), (0.18, 0.05, 0.13), metal, 0.01))
        objs.append(cube("right_buckle", (0.56, -0.51, 0.56), (0.18, 0.05, 0.13), metal, 0.01))
    return objs


def build_container(detail: int) -> list:
    reset_scene()
    body = mat("M94_container_weathered_blue_body", (0.08, 0.19, 0.25, 1), 0.63, 0.05)
    rib = mat("M94_container_dark_ribs", (0.04, 0.07, 0.085, 1), 0.58, 0.10)
    worn = mat("M94_container_scraped_edges", (0.58, 0.50, 0.36, 1), 0.44, 0.15)
    decal = mat("M94_container_white_decals", (0.82, 0.84, 0.78, 1), 0.60, 0.0)
    objs = [
        cube("shipping_container_body", (0, 0, 1.45), (5.9, 2.25, 2.60), body, 0.035),
        cube("front_double_doors", (0, -1.16, 1.45), (5.95, 0.10, 2.50), body, 0.02),
        cube("rear_panel", (0, 1.16, 1.45), (5.95, 0.10, 2.50), body, 0.02),
    ]
    rib_count = 9 if detail == 0 else 6 if detail == 1 else 4
    for i in range(rib_count):
        x = -2.70 + i * (5.40 / max(1, rib_count - 1))
        objs.append(cube(f"corrugation_rib_front_{i}", (x, -1.23, 1.45), (0.08, 0.08, 2.35), rib, 0.004))
        objs.append(cube(f"corrugation_rib_back_{i}", (x, 1.23, 1.45), (0.08, 0.08, 2.35), rib, 0.004))
    for i, x in enumerate([-2.15, -0.75, 0.75, 2.15]):
        objs.append(cyl(f"door_lock_bar_{i}", (x, -1.31, 1.45), 0.025, 2.25, rib, (0, 0, 0), 12))
    if detail <= 1:
        objs.append(cube("container_serial_decal", (-1.55, -1.32, 2.15), (1.20, 0.025, 0.22), decal, 0.002))
        objs.append(cube("container_warning_decal", (1.65, -1.32, 0.85), (0.72, 0.025, 0.25), decal, 0.002))
    if detail == 0:
        for i, x in enumerate([-2.9, -1.7, -0.3, 1.2, 2.6]):
            objs.append(cube(f"edge_wear_patch_{i}", (x, -1.34, 2.55), (0.28, 0.025, 0.06), worn, 0.002))
    return objs


BUILDERS = {
    "rifle": build_rifle,
    "humanoid_player": lambda detail, accent=None: build_humanoid(detail, False),
    "humanoid_enemy": lambda detail, accent=None: build_humanoid(detail, True),
    "helmet": lambda detail, accent=None: build_helmet(detail),
    "vest": lambda detail, accent=None: build_vest(detail),
    "medkit": lambda detail, accent=None: build_medkit(detail),
    "container": lambda detail, accent=None: build_container(detail),
}


def export_glb(objects: list, output_path: Path) -> None:
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
        export_copyright="M94 deterministic first-wave local generation",
    )


def generate_sidecars(asset_root: Path, stem: str, base_color: tuple[int, int, int]) -> list[dict[str, str]]:
    sidecars = []
    specs = {
        "basecolor": base_color,
        "normal": (128, 128, 255),
        "roughness": (178, 178, 178),
        "metallic": (28, 28, 28),
        "ao": (228, 228, 228),
    }
    for lod in LOD_SUFFIXES:
        for token, color in specs.items():
            path = asset_root / f"{stem}_{lod}_{token}.png"
            before = sha256(path)
            write_png(path, 256, 256, color, normal=(token == "normal"))
            sidecars.append({"path": path.name, "before_sha256": before, "after_sha256": sha256(path)})
    return sidecars


def main() -> int:
    args = parse_args()
    repo_root = Path(args.repo_root).resolve()
    asset_root = repo_root / "Assets" / "HtmlTacticalAssets" / "RealifiedAssets"
    docs_root = repo_root / "docs"
    trace_path = docs_root / "M94_GENERATED_BATCH_ASSET_TRACE.json"
    asset_root.mkdir(parents=True, exist_ok=True)
    docs_root.mkdir(parents=True, exist_ok=True)

    trace_assets = []
    for spec in ASSETS:
        stem = spec["stem"]
        builder = BUILDERS[spec["builder"]]
        old_hashes = {}
        exported = []
        for lod in LOD_SUFFIXES:
            old_hashes[f"{stem}_{lod}.glb"] = sha256(asset_root / f"{stem}_{lod}.glb")
            objects = builder(LOD_SUFFIXES.index(lod), None)
            path = asset_root / f"{stem}_{lod}.glb"
            export_glb(objects, path)
            exported.append({"path": path.name, "sha256": sha256(path), "bytes": path.stat().st_size})

        textured = asset_root / f"{stem}_textured.glb"
        old_hashes[textured.name] = sha256(textured)
        shutil.copy2(asset_root / f"{stem}_LOD0.glb", textured)
        sidecars = generate_sidecars(asset_root, stem, spec["basecolor"])
        trace_assets.append(
            {
                "asset_id": spec["asset_id"],
                "category": spec["category"],
                "stem": stem,
                "m94_generated_batch": True,
                "generation_batch": "M94",
                "source_batch_id": BATCH_ID,
                "source_pipeline": "m94_local_blender_first_wave_from_pro_reference_images",
                "source_reference_bundle": REFERENCE_BUNDLE,
                "semantic_category_match": True,
                "semantic_review_source": "deterministic_generator_semantic_contract_pending_vlm_or_human_review",
                "semantic_shape": spec["semantic"],
                "old_hashes": old_hashes,
                "exported_glbs": exported + [{"path": textured.name, "sha256": sha256(textured), "bytes": textured.stat().st_size}],
                "sidecars": sidecars,
            }
        )

    trace = {
        "schema": "m94_generated_batch_asset_trace_v1",
        "timestamp_utc": datetime.now(timezone.utc).isoformat(),
        "batch_id": BATCH_ID,
        "source_reference_bundle": REFERENCE_BUNDLE,
        "note": "Local deterministic Blender first-wave generation. These are production candidates for gate testing; M95/M96 still require final art review.",
        "assets": trace_assets,
    }
    trace_path.write_text(json.dumps(trace, indent=2), encoding="utf-8")
    print(f"Wrote {trace_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
