# Unity GLB/PBR Import Precheck

Date: 2026-05-17

## Scope

Path inspected:

```text
/Users/yuanshaochen/My project/Assets/HtmlTacticalAssets/RealifiedAssets/
```

## File Inventory

```text
GLB files: 48
external image textures in folder: 0
external .mat files in folder: 0
folder size: about 347 MB
```

This means the current assets rely on embedded GLB material/texture data rather than sidecar PNG/KTX2 texture files.

## Embedded GLB Metadata Probe

A direct GLB JSON chunk scan found this pattern for every GLB:

```text
meshes: 1
materials: 1
textures: 2
images: 2
pbr slots counted: 2
```

Interpretation:

- The files are not empty geometry shells.
- They do contain embedded texture/image references.
- They are not full production PBR packets with separate basecolor/normal/roughness/metallic/AO maps.
- Current evidence is closer to "embedded textured GLB" than "complete asset packet."

## Important Importer Issue

Current importer:

```text
Assets/Scripts/Editor/UnityGLBImporter.cs
```

It searches for sidecar files:

```text
*_albedo.png
*_metallic.png
*_roughness.png
*_normal.png
```

But the RealifiedAssets folder currently has zero external image textures. Therefore, this importer will not validate or wire most of the actual embedded GLB texture data.

Additional concern:

- URP Lit does not use a raw roughness texture through `_SmoothnessTextureChannel` as a texture slot.
- glTF metallic-roughness uses packed channels; Unity smoothness is the inverse of roughness and usually needs packing or a material conversion step.

## Current Conclusion

Kimi's asset chain produced useful GLB assets, and Unity has imported file-level assets. However, the remaining Unity-side validation is real:

1. Confirm the GLB importer package creates usable prefab/material data.
2. Confirm material shaders render as URP/Lit or compatible.
3. Confirm embedded maps are visible in Unity materials.
4. Confirm player/enemy/weapon/loot/container instances in the tactical scene use these imported assets, not only procedural fallback shapes.
5. Capture a rendered evidence scene, then use Nemotron/UI-TARS or screenshot inspection to judge visual quality.

## Recommended Next Gate

Create `AI Tools/Write GLB PBR Import Report` or an MCP transient `execute_code` equivalent that writes:

```text
docs/UNITY_GLB_PBR_IMPORT_REPORT.md
docs/UNITY_GLB_PBR_IMPORT_REPORT.json
Assets/Screenshots/glb_pbr_contact_sheet.png
```

Required fields by asset:

- path
- imported GameObject exists
- mesh count
- renderer count
- material count
- shader names
- texture property names actually assigned
- normal map present
- metallic/roughness or smoothness source present
- visible preview instance created
- pass/fail reason

