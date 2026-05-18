#!/usr/bin/env python3
"""Validate a ChatGPT Pro batch reference-image package.

The package is an asset-factory input, not a production asset. This checker
keeps that boundary explicit by requiring manifest entries to mark every image
as reference-only quarantine material.
"""

from __future__ import annotations

import argparse
import json
import re
import struct
import sys
import tempfile
import zipfile
from pathlib import Path
from typing import Any


VALID_CLASSES = {"weapon", "humanoid", "gear", "loot", "environment_prop"}
REQUIRED_MANIFEST_FIELDS = {
    "image_file",
    "class",
    "asset_id",
    "view",
    "intended_unity_asset_id",
    "prompt",
    "negative_prompt",
    "generator",
    "dimensions",
    "usage",
    "production_status",
}
EXPECTED_CONTACT_SHEETS = {
    "contact_sheets/weapon_contact_sheet.png",
    "contact_sheets/humanoid_contact_sheet.png",
    "contact_sheets/gear_contact_sheet.png",
    "contact_sheets/loot_contact_sheet.png",
    "contact_sheets/environment_prop_contact_sheet.png",
}
FIRST_WAVE = {
    ("weapon", "hero_rifle", "front"),
    ("weapon", "hero_rifle", "side"),
    ("weapon", "hero_rifle", "three_quarter"),
    ("humanoid", "enemy_tactical", "front"),
    ("humanoid", "enemy_tactical", "back"),
    ("humanoid", "enemy_tactical", "three_quarter"),
    ("gear", "vest", "front"),
    ("gear", "vest", "side"),
    ("gear", "vest", "three_quarter"),
    ("loot", "medkit", "top"),
    ("loot", "medkit", "front"),
    ("loot", "medkit", "three_quarter"),
    ("environment_prop", "shipping_container", "front"),
    ("environment_prop", "shipping_container", "side"),
    ("environment_prop", "shipping_container", "three_quarter"),
}
IMAGE_NAME_RE = re.compile(
    r"^images/(?P<class>weapon|humanoid|gear|loot|environment_prop)/"
    r"M94_(?P=class)_(?P<asset_id>[a-z0-9_]+)_(?P<view>front|side|back|top|three_quarter)_v(?P<version>[0-9]{2})\.png$"
)


def load_package(package: Path) -> tuple[Path, tempfile.TemporaryDirectory[str] | None]:
    if package.is_dir():
        return package, None

    if package.is_file() and package.suffix.lower() == ".zip":
        tmp = tempfile.TemporaryDirectory(prefix="pro_batch_images_")
        with zipfile.ZipFile(package) as archive:
            archive.extractall(tmp.name)
        root = Path(tmp.name)
        children = [child for child in root.iterdir() if child.is_dir()]
        if len(children) == 1 and (children[0] / "manifest.json").exists():
            root = children[0]
        return root, tmp

    raise SystemExit(f"Package must be a directory or .zip: {package}")


def png_size(path: Path) -> tuple[int, int] | None:
    try:
        with path.open("rb") as handle:
            header = handle.read(24)
    except OSError:
        return None

    if len(header) < 24 or header[:8] != b"\x89PNG\r\n\x1a\n" or header[12:16] != b"IHDR":
        return None
    return struct.unpack(">II", header[16:24])


def read_manifest(path: Path) -> tuple[list[dict[str, Any]], list[str]]:
    blockers: list[str] = []
    try:
        payload = json.loads(path.read_text())
    except Exception as exc:  # noqa: BLE001 - checker should report parse errors.
        return [], [f"manifest parse failed: {exc}"]

    if isinstance(payload, list):
        entries = payload
    elif isinstance(payload, dict) and isinstance(payload.get("images"), list):
        entries = payload["images"]
    elif isinstance(payload, dict) and isinstance(payload.get("entries"), list):
        entries = payload["entries"]
    elif isinstance(payload, dict) and isinstance(payload.get("items"), list):
        entries = payload["items"]
    else:
        return [], ["manifest must be a list or an object with images/entries/items array"]

    clean_entries: list[dict[str, Any]] = []
    for index, entry in enumerate(entries):
        if not isinstance(entry, dict):
            blockers.append(f"manifest entry {index} is not an object")
            continue
        clean_entries.append(entry)
    return clean_entries, blockers


def validate(package: Path, require_first_wave: bool) -> dict[str, Any]:
    root, tmp = load_package(package)
    try:
        blockers: list[str] = []
        warnings: list[str] = []
        manifest_path = root / "manifest.json"
        if not manifest_path.exists():
            blockers.append("manifest.json missing")
            entries: list[dict[str, Any]] = []
        else:
            entries, manifest_blockers = read_manifest(manifest_path)
            blockers.extend(manifest_blockers)

        image_results: list[dict[str, Any]] = []
        seen_targets: set[tuple[str, str, str]] = set()
        class_counts = {key: 0 for key in sorted(VALID_CLASSES)}

        for index, entry in enumerate(entries):
            entry_blockers: list[str] = []
            missing = sorted(REQUIRED_MANIFEST_FIELDS.difference(entry))
            if missing:
                entry_blockers.append("missing fields: " + ", ".join(missing))

            image_file = str(entry.get("image_file", ""))
            match = IMAGE_NAME_RE.match(image_file)
            if not match:
                entry_blockers.append("image_file does not match M94 naming/path rule")

            class_name = str(entry.get("class", ""))
            asset_id = str(entry.get("asset_id", ""))
            view = str(entry.get("view", ""))
            if class_name not in VALID_CLASSES:
                entry_blockers.append("invalid class")
            else:
                class_counts[class_name] += 1

            if match:
                if match.group("class") != class_name:
                    entry_blockers.append("manifest class does not match image path class")
                if match.group("asset_id") != asset_id:
                    entry_blockers.append("manifest asset_id does not match image path")
                if match.group("view") != view:
                    entry_blockers.append("manifest view does not match image path")

            if entry.get("usage") != "reference_image_only":
                entry_blockers.append("usage must be reference_image_only")
            if entry.get("production_status") != "quarantine_reference":
                entry_blockers.append("production_status must be quarantine_reference")
            if not str(entry.get("prompt", "")).strip():
                entry_blockers.append("prompt is empty")
            if not str(entry.get("negative_prompt", "")).strip():
                entry_blockers.append("negative_prompt is empty")

            image_path = root / image_file
            size = png_size(image_path)
            if not image_path.exists():
                entry_blockers.append("image file missing")
            elif size is None:
                entry_blockers.append("image file is not a valid PNG")
            elif size[0] < 1024 or size[1] < 1024:
                entry_blockers.append(f"PNG dimensions below 1024: {size[0]}x{size[1]}")

            if class_name and asset_id and view:
                seen_targets.add((class_name, asset_id, view))

            image_results.append(
                {
                    "index": index,
                    "image_file": image_file,
                    "class": class_name,
                    "asset_id": asset_id,
                    "view": view,
                    "png_size": list(size) if size else None,
                    "passed": not entry_blockers,
                    "blockers": entry_blockers,
                }
            )

        actual_contact_sheets = {
            str(path.relative_to(root))
            for path in (root / "contact_sheets").glob("*.png")
            if path.is_file()
        }
        missing_contact_sheets = sorted(EXPECTED_CONTACT_SHEETS.difference(actual_contact_sheets))
        if missing_contact_sheets:
            blockers.append("missing contact sheets: " + ", ".join(missing_contact_sheets))

        if require_first_wave:
            missing_first_wave = sorted(
                f"{class_name}/{asset_id}/{view}"
                for class_name, asset_id, view in FIRST_WAVE.difference(seen_targets)
            )
            if missing_first_wave:
                blockers.append("missing first-wave images: " + ", ".join(missing_first_wave))

        failed_images = [item for item in image_results if not item["passed"]]
        if failed_images:
            blockers.append(f"{len(failed_images)} image manifest entries failed validation")

        return {
            "schema": "pro_batch_reference_image_validation_v1",
            "package": str(package),
            "root": str(root),
            "passed": not blockers,
            "manifest_found": manifest_path.exists(),
            "image_entry_count": len(entries),
            "class_counts": class_counts,
            "contact_sheets_found": sorted(actual_contact_sheets),
            "missing_contact_sheets": missing_contact_sheets,
            "require_first_wave": require_first_wave,
            "image_results": image_results,
            "warnings": warnings,
            "blockers": blockers,
            "production_note": "Validated images are still quarantine references, not production assets.",
        }
    finally:
        if tmp is not None:
            tmp.cleanup()


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("package", type=Path, help="Pro image package directory or .zip")
    parser.add_argument("--require-first-wave", action="store_true", help="Require the 15-image first wave")
    parser.add_argument("--output", type=Path, help="Write JSON report to this path")
    args = parser.parse_args()

    report = validate(args.package, args.require_first_wave)
    text = json.dumps(report, ensure_ascii=False, indent=2)
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(text + "\n")
    print(text)
    return 0 if report["passed"] else 1


if __name__ == "__main__":
    sys.exit(main())
