import argparse
import json
import math
import os
import sys
from pathlib import Path

import bpy
from mathutils import Vector


def clear_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()


def import_model(path: Path) -> None:
    suffix = path.suffix.lower()
    if suffix in {".glb", ".gltf"}:
        bpy.ops.import_scene.gltf(filepath=str(path))
    elif suffix == ".obj":
        bpy.ops.wm.obj_import(filepath=str(path))
    else:
        raise ValueError(f"Unsupported model suffix: {suffix}")


def mesh_objects():
    return [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]


def scene_bounds(objects):
    min_v = Vector((math.inf, math.inf, math.inf))
    max_v = Vector((-math.inf, -math.inf, -math.inf))
    for obj in objects:
        for corner in obj.bound_box:
            world = obj.matrix_world @ Vector(corner)
            min_v.x = min(min_v.x, world.x)
            min_v.y = min(min_v.y, world.y)
            min_v.z = min(min_v.z, world.z)
            max_v.x = max(max_v.x, world.x)
            max_v.y = max(max_v.y, world.y)
            max_v.z = max(max_v.z, world.z)
    return min_v, max_v


def recenter(objects):
    min_v, max_v = scene_bounds(objects)
    center = (min_v + max_v) * 0.5
    for obj in objects:
        obj.location -= center


def add_lighting() -> None:
    bpy.ops.object.light_add(type="AREA", location=(0.0, -4.0, 5.0))
    key = bpy.context.object
    key.name = "M110 Preview Key Light"
    key.data.energy = 500
    key.data.size = 5.0

    bpy.ops.object.light_add(type="POINT", location=(-3.0, 3.0, 2.0))
    fill = bpy.context.object
    fill.name = "M110 Preview Fill Light"
    fill.data.energy = 90


def setup_camera(objects):
    min_v, max_v = scene_bounds(objects)
    size = max((max_v - min_v).length, 0.01)
    extent = max(max_v.x - min_v.x, max_v.y - min_v.y, max_v.z - min_v.z, 0.01)

    bpy.ops.object.camera_add(location=(extent * 0.9, -extent * 2.4, extent * 0.65), rotation=(math.radians(68), 0, math.radians(23)))
    camera = bpy.context.object
    bpy.context.scene.camera = camera

    target = Vector((0.0, 0.0, 0.0))
    direction = target - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    camera.data.lens = 55
    camera.data.dof.use_dof = False
    return size


def collect_report(model_path: Path, objects):
    material_names = []
    texture_images = []
    triangle_count = 0
    vertex_count = 0
    for obj in objects:
        mesh = obj.data
        vertex_count += len(mesh.vertices)
        for poly in mesh.polygons:
            triangle_count += max(1, len(poly.vertices) - 2)
        for slot in obj.material_slots:
            if slot.material:
                material_names.append(slot.material.name)
                for node in slot.material.node_tree.nodes if slot.material.node_tree else []:
                    if node.type == "TEX_IMAGE" and node.image:
                        texture_images.append(
                            {
                                "name": node.image.name,
                                "filepath": bpy.path.abspath(node.image.filepath) if node.image.filepath else "",
                                "size": list(node.image.size),
                            }
                        )

    min_v, max_v = scene_bounds(objects)
    dimensions = max_v - min_v
    return {
        "model_path": str(model_path),
        "object_count": len(objects),
        "mesh_names": [obj.name for obj in objects],
        "vertex_count": vertex_count,
        "triangle_count": triangle_count,
        "bounds_min": [round(min_v.x, 4), round(min_v.y, 4), round(min_v.z, 4)],
        "bounds_max": [round(max_v.x, 4), round(max_v.y, 4), round(max_v.z, 4)],
        "dimensions": [round(dimensions.x, 4), round(dimensions.y, 4), round(dimensions.z, 4)],
        "material_count": len(set(material_names)),
        "material_names": sorted(set(material_names)),
        "texture_images": texture_images,
    }


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--model", required=True)
    parser.add_argument("--out-png", required=True)
    parser.add_argument("--out-json", required=True)
    argv = sys.argv
    if "--" in argv:
        argv = argv[argv.index("--") + 1 :]
    else:
        argv = []
    args = parser.parse_args(argv)

    model_path = Path(args.model).expanduser().resolve()
    out_png = Path(args.out_png).expanduser().resolve()
    out_json = Path(args.out_json).expanduser().resolve()
    out_png.parent.mkdir(parents=True, exist_ok=True)
    out_json.parent.mkdir(parents=True, exist_ok=True)

    clear_scene()
    import_model(model_path)
    objects = mesh_objects()
    if not objects:
        raise RuntimeError("No mesh objects imported.")

    recenter(objects)
    add_lighting()
    setup_camera(objects)

    engines = {item.identifier for item in bpy.types.RenderSettings.bl_rna.properties["engine"].enum_items}
    bpy.context.scene.render.engine = "BLENDER_EEVEE_NEXT" if "BLENDER_EEVEE_NEXT" in engines else "BLENDER_EEVEE"
    if hasattr(bpy.context.scene, "eevee"):
        bpy.context.scene.eevee.taa_render_samples = 64
    bpy.context.scene.render.resolution_x = 1400
    bpy.context.scene.render.resolution_y = 900
    bpy.context.scene.view_settings.view_transform = "Filmic"
    bpy.context.scene.view_settings.look = "Medium High Contrast"
    bpy.context.scene.world.color = (0.04, 0.045, 0.05)
    bpy.context.scene.render.filepath = str(out_png)
    bpy.ops.render.render(write_still=True)

    report = collect_report(model_path, objects)
    report["preview_png"] = str(out_png)
    with out_json.open("w", encoding="utf-8") as f:
        json.dump(report, f, ensure_ascii=False, indent=2)


if __name__ == "__main__":
    main()
