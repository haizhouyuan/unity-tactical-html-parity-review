import argparse
import json
import sys
from pathlib import Path

import bpy


def clear_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()


def mesh_objects():
    return [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]


def count_triangles(objects) -> int:
    total = 0
    for obj in objects:
        for poly in obj.data.polygons:
            total += max(1, len(poly.vertices) - 2)
    return total


def import_model(path: Path) -> None:
    suffix = path.suffix.lower()
    if suffix in {".glb", ".gltf"}:
        bpy.ops.import_scene.gltf(filepath=str(path))
    elif suffix == ".obj":
        bpy.ops.wm.obj_import(filepath=str(path))
    else:
        raise ValueError(f"Unsupported model suffix: {suffix}")


def apply_decimate(objects, target_triangles: int) -> float:
    before = count_triangles(objects)
    if before <= target_triangles:
        return 1.0
    ratio = max(0.02, min(1.0, target_triangles / max(before, 1)))
    for obj in objects:
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        modifier = obj.modifiers.new(name=f"M110_Decimate_{target_triangles}", type="DECIMATE")
        modifier.ratio = ratio
        modifier.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        obj.select_set(False)
    return ratio


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--model", required=True)
    parser.add_argument("--out-glb", required=True)
    parser.add_argument("--out-json", required=True)
    parser.add_argument("--target-triangles", type=int, required=True)
    argv = sys.argv
    argv = argv[argv.index("--") + 1 :] if "--" in argv else []
    args = parser.parse_args(argv)

    model_path = Path(args.model).expanduser().resolve()
    out_glb = Path(args.out_glb).expanduser().resolve()
    out_json = Path(args.out_json).expanduser().resolve()
    out_glb.parent.mkdir(parents=True, exist_ok=True)
    out_json.parent.mkdir(parents=True, exist_ok=True)

    clear_scene()
    import_model(model_path)
    objects = mesh_objects()
    if not objects:
        raise RuntimeError("No mesh objects imported.")

    before_triangles = count_triangles(objects)
    ratio = apply_decimate(objects, args.target_triangles)
    after_triangles = count_triangles(objects)

    bpy.ops.export_scene.gltf(
        filepath=str(out_glb),
        export_format="GLB",
        export_yup=True,
        export_apply=True,
        export_texcoords=True,
        export_normals=True,
        export_materials="EXPORT",
    )

    report = {
        "model_path": str(model_path),
        "out_glb": str(out_glb),
        "target_triangles": args.target_triangles,
        "before_triangles": before_triangles,
        "after_triangles": after_triangles,
        "decimate_ratio": ratio,
        "object_count": len(objects),
        "mesh_names": [obj.name for obj in objects],
    }
    with out_json.open("w", encoding="utf-8") as f:
        json.dump(report, f, ensure_ascii=False, indent=2)


if __name__ == "__main__":
    main()
