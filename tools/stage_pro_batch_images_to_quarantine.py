#!/usr/bin/env python3
"""Stage and validate a ChatGPT Pro M94-M96 reference-image bundle.

This utility is intentionally reference-only. It never imports GLBs, never
promotes assets, and never changes any existing gate pass/fail JSON.

Typical use:

    python tools/stage_pro_batch_images_to_quarantine.py \
      /path/to/pro_m94_m96_batch_images_first_wave_2026-05-19.zip \
      --require-first-wave \
      --report docs/M94_PRO_BATCH_REFERENCE_IMAGE_VALIDATION.json

The staged output defaults to:

    external/pro_outputs/m94_m96_batch_images_2026-05-19/
"""

from __future__ import annotations

import argparse
import json
import shutil
import subprocess
import sys
import tempfile
import zipfile
from pathlib import Path
from typing import Any


DEFAULT_STAGE_DIR = Path("external/pro_outputs/m94_m96_batch_images_2026-05-19")
DEFAULT_REPORT = Path("docs/M94_PRO_BATCH_REFERENCE_IMAGE_VALIDATION.json")
VALIDATOR = Path("tools/validate_pro_batch_images.py")


def extract_or_copy(package: Path, stage_dir: Path) -> Path:
    if stage_dir.exists():
        shutil.rmtree(stage_dir)
    stage_dir.parent.mkdir(parents=True, exist_ok=True)

    if package.is_dir():
        shutil.copytree(package, stage_dir)
        return stage_dir

    if package.is_file() and package.suffix.lower() == ".zip":
        with tempfile.TemporaryDirectory(prefix="m94_pro_batch_") as tmp_dir:
            tmp = Path(tmp_dir)
            with zipfile.ZipFile(package) as archive:
                archive.extractall(tmp)
            children = [child for child in tmp.iterdir() if child.is_dir()]
            if len(children) == 1 and (children[0] / "manifest.json").exists():
                shutil.copytree(children[0], stage_dir)
            else:
                shutil.copytree(tmp, stage_dir)
        return stage_dir

    raise SystemExit(f"Package must be a directory or .zip: {package}")


def run_validator(stage_dir: Path, require_first_wave: bool, report: Path) -> dict[str, Any]:
    if not VALIDATOR.exists():
        raise SystemExit(f"Missing validator: {VALIDATOR}")

    cmd = [sys.executable, str(VALIDATOR), str(stage_dir), "--output", str(report)]
    if require_first_wave:
        cmd.append("--require-first-wave")

    completed = subprocess.run(cmd, text=True, capture_output=True)
    if completed.stdout.strip():
        try:
            payload = json.loads(completed.stdout)
        except json.JSONDecodeError:
            payload = {"validator_stdout": completed.stdout}
    elif report.exists():
        payload = json.loads(report.read_text())
    else:
        payload = {}

    payload["validator_returncode"] = completed.returncode
    payload["validator_stderr"] = completed.stderr
    payload["staged_dir"] = str(stage_dir)
    payload["production_note"] = "Staged images are quarantine reference inputs only, not production assets."
    report.parent.mkdir(parents=True, exist_ok=True)
    report.write_text(json.dumps(payload, ensure_ascii=False, indent=2) + "\n")
    return payload


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("package", type=Path, help="Pro image bundle directory or .zip")
    parser.add_argument("--stage-dir", type=Path, default=DEFAULT_STAGE_DIR)
    parser.add_argument("--report", type=Path, default=DEFAULT_REPORT)
    parser.add_argument("--require-first-wave", action="store_true")
    args = parser.parse_args()

    stage_dir = extract_or_copy(args.package, args.stage_dir)
    report = run_validator(stage_dir, args.require_first_wave, args.report)
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0 if report.get("passed") else 1


if __name__ == "__main__":
    raise SystemExit(main())
