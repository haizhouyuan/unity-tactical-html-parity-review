# Prompt To Send To ChatGPT PRO - M108 Visible Value Patch

请你作为高级 Unity/FPS/AI 游戏开发顾问和批量补丁生成者，直接基于下面的 public GitHub 上下文包，生成一个可下载的 consolidated patch package。不要只给方向性建议。

Context README:
https://raw.githubusercontent.com/haizhouyuan/unity-tactical-html-parity-review/main/docs/pro_delegations/M108_visible_value_context_20260519/README_M108_CONTEXT.md

Context ZIP with screenshots, reports, key C# files, diffs, and clean-background references:
https://raw.githubusercontent.com/haizhouyuan/unity-tactical-html-parity-review/main/docs/pro_delegations/M108_visible_value_context_20260519/pro_m108_unity_visible_value_and_asset_chain_20260519.zip

SHA256:
https://raw.githubusercontent.com/haizhouyuan/unity-tactical-html-parity-review/main/docs/pro_delegations/M108_visible_value_context_20260519/SHA256SUMS.txt

核心背景：
- 当前目标不是继续堆 gate/report，而是让 Unity 游戏玩家视角出现明显可见的 PUBG-like 提升。
- 近期 M97/M98/M100 技术上让报告通过了，但实际截图里武器仍然像一个导入物体硬塞到屏幕里，视觉变化太小，用户不能接受。
- 枪口之前朝后，Codex 本地临时把 yaw 从 +92 改到 -88，并把 viewmodel screen area 修到 0.122，但这只是 gate pass，不是视觉质量 pass。
- 官方 Unity MCP 不作为主线；本地使用 CoplayDev/community MCP 运行 Unity `AI Tools/...` 菜单，Codex 会负责本地验证。
- PRO 不能声称 Unity verified；请输出 patch，Codex 会本地 apply + MCP 跑 Unity 验证。

请一次性生成这些可下载文件：
1. `M108_visible_value_weapon_enemy_combat_slice.patch`
2. `M108_visible_value_weapon_enemy_combat_slice_patch.zip`
3. `README_FOR_PATCH.md`
4. `changed_files_manifest.json`
5. `validation_notes.md`
6. 可选：`image_to_3d_homepc_commands.md`

补丁目标：
- 简化/统一第一人称武器 visual path，不要继续叠 M97/M98/M100 多套互相覆盖的逻辑。
- 增加 robust first-person weapon composer：per-asset orientation / scale / offset config，稳定 idle/ADS/fire/reload poses。
- 增加实际动作/效果可见价值：muzzle flash、tracer/hit marker、casing、reload tilt/motion、enemy hit/down feedback。
- 增加一个 debug-friendly combat slice/spawn route：玩家出生后面向可读战斗区，能看到 weapon/enemy/container/loot/building facade，而不是怼到低可见度墙面。
- 资产生成链路也要纳入方案：用 ZIP 里的 PRO clean refs 作为 image-to-3D 输入，为 enemy_tactical/container_cover 给出 HomePC/Hunyuan/Blender 可执行命令或轻量脚本。

硬约束：
- 不要编辑 `Assets/Scenes/*.unity`，通过 Editor API/menu command 改 scene state。
- 不要改 `Packages/*`、`ProjectSettings/*`。
- 不要直接改已有 gate JSON pass/fail 值。
- 不要加入大二进制资产。
- 不要继续扩 OS/gate 架构；这必须是 visible value patch。
- 输出补丁应主要改/add `Assets/Scripts/Tactical/*.cs`、`Assets/Editor/*.cs`、`docs/M108_*.md`，可选轻量 `tools/*.py`。

Codex 本地验收会做：
1. `git apply --check`。
2. 检查 forbidden edits/secrets。
3. 复制到 active Unity project。
4. 通过 community MCP refresh Unity、read Console、执行你的 install/apply menu、执行 capture/proof menu。
5. 看 JSON 和截图，判断是否真的比 M100 截图有明显提升。

请优先产出能应用的 patch 包；如果你认为当前代码路径应该重构，请直接在 patch 里重构最小必要范围，而不是只讲理论。
