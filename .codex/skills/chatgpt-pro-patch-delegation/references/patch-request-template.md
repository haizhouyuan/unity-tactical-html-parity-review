# Patch Request Template

Use this when asking ChatGPT PRO to author a patch for this repository.

```text
请不要只给建议，直接做代码补丁包。

目标仓库：
<github-url>

基线：
- branch: <branch>
- latest commit: <commit sha + subject>
- current local truth: <short bullets>
- gates that must remain false unless genuinely proven: <reports/fields>

目标：
<specific mission id and outcome>

Allowed files:
- <paths>

Forbidden:
- 不手改 Unity scenes / prefab YAML
- 不提交 Builds/
- 不新增 package
- 不手改已有 gate JSON 伪造 passed=true
- 不删除上一阶段 evidence

Implementation requirements:
1. <file + behavior>
2. <file + behavior>
3. <docs/report updates>

Delivery:
- downloadable zip
- patch file(s), separated by scope
- README_FOR_PATCH.md
- changed_files.json
- risk_notes.md
- local checks you could run

If you cannot run Unity, write:
Unity execution not verified by PRO; Codex must apply, run Unity, and verify.
```

## Reference Image Batch Add-On

Use this add-on when asking PRO to generate reference images as asset-factory inputs.

```text
Also return a separate reference-image zip.

Required zip shape:
- manifest.json
- prompts/
- contact_sheets/
- images/weapon/
- images/humanoid/
- images/gear/
- images/loot/
- images/environment_prop/

Every image must use:
images/<class>/M94_<class>_<asset_id>_<view>_v01.png

Every manifest entry must include:
- image_file
- class
- asset_id
- view
- intended_unity_asset_id
- prompt
- negative_prompt
- generator
- dimensions
- usage = reference_image_only
- production_status = quarantine_reference

The images are quarantine references only. Do not claim production-ready, gameplay-bound, semantic-pass, or Unity-promoted status.
```

## Notes

- Give PRO exact file paths and explicit negative constraints.
- Ask it to split optional work into a second patch so Codex can reject risky parts independently.
- Ask it to preserve gate truth, especially `full_visual_asset_gate_passed=false` until local Unity evidence proves otherwise.
