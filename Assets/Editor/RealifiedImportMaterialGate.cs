#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class RealifiedImportMaterialGate
{
    private const string AssetRoot = "Assets/HtmlTacticalAssets/RealifiedAssets";
    private const string ReportPath = "docs/REALIFIED_IMPORT_MATERIAL_GATE.json";
    private const string MarkdownPath = "docs/REALIFIED_IMPORT_MATERIAL_GATE.md";

    private static readonly ClassSpec[] Classes =
    {
        new ClassSpec("weapon", "hero_rifle", "hero_rifle"),
        new ClassSpec("weapon", "sidearm", "RS_02_sidearm"),
        new ClassSpec("weapon", "secondary_weapon", "RS_03_secondary_weapon"),
        new ClassSpec("weapon", "shotgun", "RS_12_shotgun"),
        new ClassSpec("character", "player_tactical", "RS_04_player_tactical"),
        new ClassSpec("character", "enemy_tactical", "RS_05_enemy_tactical"),
        new ClassSpec("gear", "helmet", "RS_06_gear_helmet"),
        new ClassSpec("gear", "vest", "RS_07_gear_vest"),
        new ClassSpec("loot", "ammo", "RS_08_loot_ammo"),
        new ClassSpec("loot", "medkit", "RS_09_loot_medkit"),
        new ClassSpec("environment_prop", "container", "RS_10_prop_container"),
        new ClassSpec("environment_prop", "crate", "RS_11_prop_crate"),
    };

    [MenuItem("AI Tools/Validate Realified Import And Materials")]
    public static void ValidateRealifiedImportAndMaterials()
    {
        Directory.CreateDirectory("docs");
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        var results = Classes.Select(ValidateClass).ToArray();
        var rootExists = Directory.Exists(AssetRoot);
        var glbCount = rootExists ? Directory.GetFiles(AssetRoot, "*.glb", SearchOption.TopDirectoryOnly).Length : 0;
        var pngCount = rootExists ? Directory.GetFiles(AssetRoot, "*.png", SearchOption.TopDirectoryOnly).Length : 0;
        var matCount = rootExists ? Directory.GetFiles(AssetRoot, "*.mat", SearchOption.TopDirectoryOnly).Length : 0;
        var metaCount = rootExists ? Directory.GetFiles(AssetRoot, "*.meta", SearchOption.TopDirectoryOnly).Length : 0;
        var classesWithAllLods = results.Count(result => result.AllLodsPresent);
        var classesWithPbrSidecars = results.Count(result => result.PbrSidecarsComplete);
        var classesWithImportedPrefab = results.Count(result => result.Lod0ImportedPrefab);
        var classesWithAcceptedMaterial = results.Count(result => result.HasAcceptedMaterialShader);
        var technicalReadyClasses = results.Count(result => result.TechnicalImportReady);
        var importPresencePassed = rootExists && glbCount >= Classes.Length * 3 && classesWithAllLods == Classes.Length && classesWithImportedPrefab == Classes.Length;
        var pbrSidecarPresencePassed = classesWithPbrSidecars == Classes.Length;
        var materialTechnicalPassed = classesWithAcceptedMaterial >= Classes.Length;

        WriteJson(
            rootExists,
            glbCount,
            pngCount,
            matCount,
            metaCount,
            classesWithAllLods,
            classesWithPbrSidecars,
            classesWithImportedPrefab,
            classesWithAcceptedMaterial,
            technicalReadyClasses,
            importPresencePassed,
            pbrSidecarPresencePassed,
            materialTechnicalPassed,
            results);
        WriteMarkdown(
            rootExists,
            glbCount,
            pngCount,
            matCount,
            metaCount,
            classesWithAllLods,
            classesWithPbrSidecars,
            classesWithImportedPrefab,
            classesWithAcceptedMaterial,
            technicalReadyClasses,
            importPresencePassed,
            pbrSidecarPresencePassed,
            materialTechnicalPassed,
            results);

        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] Realified import/material gate written to " + ReportPath);
    }

    private static ClassResult ValidateClass(ClassSpec spec)
    {
        var glbPaths = new[]
        {
            BuildPath(spec.Stem + "_LOD0.glb"),
            BuildPath(spec.Stem + "_LOD1.glb"),
            BuildPath(spec.Stem + "_LOD2.glb"),
        };
        var lodsPresent = glbPaths.Select(File.Exists).ToArray();
        var lod0Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(glbPaths[0]);
        var externalMaterials = Directory.Exists(AssetRoot)
            ? Directory.GetFiles(AssetRoot, spec.Stem + "*.mat", SearchOption.TopDirectoryOnly).Select(ToAssetPath).ToArray()
            : Array.Empty<string>();
        var importedMaterialNames = new List<string>();
        var acceptedMaterialShaderCount = 0;

        foreach (var glbPath in glbPaths.Where(File.Exists))
        {
            foreach (var material in AssetDatabase.LoadAllAssetsAtPath(glbPath).OfType<Material>())
            {
                var shaderName = material.shader != null ? material.shader.name : "";
                importedMaterialNames.Add(Path.GetFileName(glbPath) + ":" + material.name + ":" + shaderName);
                if (IsAcceptedShader(shaderName))
                {
                    acceptedMaterialShaderCount++;
                }
            }
        }

        foreach (var materialPath in externalMaterials)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            var shaderName = material != null && material.shader != null ? material.shader.name : "";
            importedMaterialNames.Add(Path.GetFileName(materialPath) + ":" + shaderName);
            if (IsAcceptedShader(shaderName))
            {
                acceptedMaterialShaderCount++;
            }
        }

        var mapPresence = new MapPresence(
            HasTexture(spec.Stem, "basecolor"),
            HasTexture(spec.Stem, "normal"),
            HasTexture(spec.Stem, "roughness"),
            HasTexture(spec.Stem, "metallic") || HasTexture(spec.Stem, "metallic_smoothness"),
            HasTexture(spec.Stem, "ao") || HasTexture(spec.Stem, "orm"));
        var pbrSidecarsComplete = mapPresence.BaseColor && mapPresence.Normal && mapPresence.Roughness && mapPresence.Metallic && mapPresence.AoOrOrm;
        var allLodsPresent = lodsPresent.All(present => present);
        var hasAcceptedMaterialShader = acceptedMaterialShaderCount > 0;
        var technicalImportReady = allLodsPresent && lod0Prefab != null && pbrSidecarsComplete && hasAcceptedMaterialShader;

        return new ClassResult(
            spec,
            glbPaths,
            lodsPresent,
            lod0Prefab != null,
            externalMaterials,
            importedMaterialNames.ToArray(),
            acceptedMaterialShaderCount,
            mapPresence,
            allLodsPresent,
            pbrSidecarsComplete,
            hasAcceptedMaterialShader,
            technicalImportReady);
    }

    private static bool HasTexture(string stem, string token)
    {
        return Directory.Exists(AssetRoot)
            && Directory.GetFiles(AssetRoot, stem + "*" + token + "*.png", SearchOption.TopDirectoryOnly).Length > 0;
    }

    private static string BuildPath(string fileName)
    {
        return AssetRoot + "/" + fileName;
    }

    private static string ToAssetPath(string fullPath)
    {
        var normalized = fullPath.Replace("\\", "/");
        var assetsIndex = normalized.IndexOf("Assets/", StringComparison.Ordinal);
        return assetsIndex >= 0 ? normalized.Substring(assetsIndex) : normalized;
    }

    private static bool IsAcceptedShader(string shaderName)
    {
        return !string.IsNullOrEmpty(shaderName)
            && (shaderName.IndexOf("Universal Render Pipeline/Lit", StringComparison.OrdinalIgnoreCase) >= 0
                || shaderName.IndexOf("Shader Graphs/glTF", StringComparison.OrdinalIgnoreCase) >= 0
                || shaderName.IndexOf("glTFast", StringComparison.OrdinalIgnoreCase) >= 0
                || shaderName.IndexOf("glTF", StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static void WriteJson(
        bool rootExists,
        int glbCount,
        int pngCount,
        int matCount,
        int metaCount,
        int classesWithAllLods,
        int classesWithPbrSidecars,
        int classesWithImportedPrefab,
        int classesWithAcceptedMaterial,
        int technicalReadyClasses,
        bool importPresencePassed,
        bool pbrSidecarPresencePassed,
        bool materialTechnicalPassed,
        ClassResult[] results)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "realified_import_material_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "asset_root", AssetRoot, true);
        Append(json, "asset_root_exists", rootExists, true);
        Append(json, "glb_count", glbCount, true);
        Append(json, "png_count", pngCount, true);
        Append(json, "mat_count", matCount, true);
        Append(json, "meta_count", metaCount, true);
        json.AppendLine("  \"summary\": {");
        Append(json, "class_count", results.Length, true, 4);
        Append(json, "classes_with_all_lods", classesWithAllLods, true, 4);
        Append(json, "classes_with_pbr_sidecars", classesWithPbrSidecars, true, 4);
        Append(json, "classes_with_imported_lod0_prefab", classesWithImportedPrefab, true, 4);
        Append(json, "classes_with_accepted_material_shader", classesWithAcceptedMaterial, true, 4);
        Append(json, "technical_ready_classes", technicalReadyClasses, true, 4);
        Append(json, "import_presence_passed", importPresencePassed, true, 4);
        Append(json, "pbr_sidecar_presence_passed", pbrSidecarPresencePassed, true, 4);
        Append(json, "material_technical_passed", materialTechnicalPassed, true, 4);
        Append(json, "candidate_imported", importPresencePassed, true, 4);
        Append(json, "semantic_promotion_checked_here", false, true, 4);
        Append(json, "production_promoted", false, false, 4);
        json.AppendLine("  },");
        json.AppendLine("  \"classes\": [");
        for (var i = 0; i < results.Length; i++)
        {
            AppendClass(json, results[i], i == results.Length - 1);
        }
        json.AppendLine("  ]");
        json.AppendLine("}");
        File.WriteAllText(ReportPath, json.ToString());
    }

    private static void AppendClass(StringBuilder json, ClassResult result, bool last)
    {
        json.AppendLine("    {");
        Append(json, "category", result.Spec.Category, true, 6);
        Append(json, "asset_id", result.Spec.AssetId, true, 6);
        Append(json, "stem", result.Spec.Stem, true, 6);
        Append(json, "lod0_present", result.LodsPresent[0], true, 6);
        Append(json, "lod1_present", result.LodsPresent[1], true, 6);
        Append(json, "lod2_present", result.LodsPresent[2], true, 6);
        Append(json, "all_lods_present", result.AllLodsPresent, true, 6);
        Append(json, "lod0_imported_prefab", result.Lod0ImportedPrefab, true, 6);
        Append(json, "external_material_count", result.ExternalMaterials.Length, true, 6);
        Append(json, "accepted_material_shader_count", result.AcceptedMaterialShaderCount, true, 6);
        Append(json, "has_accepted_material_shader", result.HasAcceptedMaterialShader, true, 6);
        json.AppendLine("      \"pbr_sidecars\": {");
        Append(json, "basecolor", result.MapPresence.BaseColor, true, 8);
        Append(json, "normal", result.MapPresence.Normal, true, 8);
        Append(json, "roughness", result.MapPresence.Roughness, true, 8);
        Append(json, "metallic_or_metallic_smoothness", result.MapPresence.Metallic, true, 8);
        Append(json, "ao_or_orm", result.MapPresence.AoOrOrm, true, 8);
        Append(json, "complete", result.PbrSidecarsComplete, false, 8);
        json.AppendLine("      },");
        Append(json, "technical_import_ready", result.TechnicalImportReady, true, 6);
        Append(json, "semantic_promotion_checked_here", false, true, 6);
        Append(json, "production_promoted", false, true, 6);
        AppendStringArray(json, "glb_paths", result.GlbPaths, true, 6);
        AppendStringArray(json, "external_materials", result.ExternalMaterials, true, 6);
        AppendStringArray(json, "material_shaders", result.MaterialShaders, false, 6);
        json.Append("    }");
        json.AppendLine(last ? "" : ",");
    }

    private static void WriteMarkdown(
        bool rootExists,
        int glbCount,
        int pngCount,
        int matCount,
        int metaCount,
        int classesWithAllLods,
        int classesWithPbrSidecars,
        int classesWithImportedPrefab,
        int classesWithAcceptedMaterial,
        int technicalReadyClasses,
        bool importPresencePassed,
        bool pbrSidecarPresencePassed,
        bool materialTechnicalPassed,
        ClassResult[] results)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# Realified Import Material Gate");
        markdown.AppendLine();
        markdown.AppendLine("Generated: " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        markdown.AppendLine();
        markdown.AppendLine("## Summary");
        markdown.AppendLine();
        markdown.AppendLine("- Asset root exists: `" + rootExists + "`");
        markdown.AppendLine("- GLB files: `" + glbCount + "`");
        markdown.AppendLine("- PNG files: `" + pngCount + "`");
        markdown.AppendLine("- Material files: `" + matCount + "`");
        markdown.AppendLine("- Meta files: `" + metaCount + "`");
        markdown.AppendLine("- Classes with all LODs: `" + classesWithAllLods + " / " + results.Length + "`");
        markdown.AppendLine("- Classes with complete PBR sidecars: `" + classesWithPbrSidecars + " / " + results.Length + "`");
        markdown.AppendLine("- Classes with imported LOD0 prefab: `" + classesWithImportedPrefab + " / " + results.Length + "`");
        markdown.AppendLine("- Classes with accepted material shader: `" + classesWithAcceptedMaterial + " / " + results.Length + "`");
        markdown.AppendLine("- Technical ready classes: `" + technicalReadyClasses + " / " + results.Length + "`");
        markdown.AppendLine("- Import presence passed: `" + importPresencePassed + "`");
        markdown.AppendLine("- PBR sidecar presence passed: `" + pbrSidecarPresencePassed + "`");
        markdown.AppendLine("- Material technical passed: `" + materialTechnicalPassed + "`");
        markdown.AppendLine("- Production promoted: `false`");
        markdown.AppendLine();
        markdown.AppendLine("This gate checks technical import readiness only. Semantic category promotion and gameplay promotion remain separate fail-closed gates.");
        markdown.AppendLine();
        markdown.AppendLine("## Classes");
        markdown.AppendLine();
        markdown.AppendLine("| Category | Asset | LODs | PBR Sidecars | Imported LOD0 | Accepted Shader | Technical Ready |");
        markdown.AppendLine("|---|---|---:|---:|---:|---:|---:|");
        foreach (var result in results)
        {
            markdown.Append("| ")
                .Append(result.Spec.Category)
                .Append(" | ")
                .Append(result.Spec.AssetId)
                .Append(" | ")
                .Append(result.AllLodsPresent)
                .Append(" | ")
                .Append(result.PbrSidecarsComplete)
                .Append(" | ")
                .Append(result.Lod0ImportedPrefab)
                .Append(" | ")
                .Append(result.HasAcceptedMaterialShader)
                .Append(" | ")
                .Append(result.TechnicalImportReady)
                .AppendLine(" |");
        }

        File.WriteAllText(MarkdownPath, markdown.ToString());
    }

    private static void Append(StringBuilder json, string key, string value, bool comma, int indent = 2)
    {
        json.Append(' ', indent);
        json.Append('"').Append(key).Append("\": \"").Append(Escape(value)).Append('"');
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, int value, bool comma, int indent = 2)
    {
        json.Append(' ', indent);
        json.Append('"').Append(key).Append("\": ").Append(value.ToString(CultureInfo.InvariantCulture));
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, bool value, bool comma, int indent = 2)
    {
        json.Append(' ', indent);
        json.Append('"').Append(key).Append("\": ").Append(value ? "true" : "false");
        json.AppendLine(comma ? "," : "");
    }

    private static void AppendStringArray(StringBuilder json, string key, string[] values, bool comma, int indent = 2)
    {
        json.Append(' ', indent);
        json.Append('"').Append(key).Append("\": [");
        for (var i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                json.Append(", ");
            }

            json.Append('"').Append(Escape(values[i])).Append('"');
        }

        json.Append(']');
        json.AppendLine(comma ? "," : "");
    }

    private static string Escape(string value)
    {
        return (value ?? "")
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    private readonly struct ClassSpec
    {
        public ClassSpec(string category, string assetId, string stem)
        {
            Category = category;
            AssetId = assetId;
            Stem = stem;
        }

        public readonly string Category;
        public readonly string AssetId;
        public readonly string Stem;
    }

    private readonly struct MapPresence
    {
        public MapPresence(bool baseColor, bool normal, bool roughness, bool metallic, bool aoOrOrm)
        {
            BaseColor = baseColor;
            Normal = normal;
            Roughness = roughness;
            Metallic = metallic;
            AoOrOrm = aoOrOrm;
        }

        public readonly bool BaseColor;
        public readonly bool Normal;
        public readonly bool Roughness;
        public readonly bool Metallic;
        public readonly bool AoOrOrm;
    }

    private readonly struct ClassResult
    {
        public ClassResult(
            ClassSpec spec,
            string[] glbPaths,
            bool[] lodsPresent,
            bool lod0ImportedPrefab,
            string[] externalMaterials,
            string[] materialShaders,
            int acceptedMaterialShaderCount,
            MapPresence mapPresence,
            bool allLodsPresent,
            bool pbrSidecarsComplete,
            bool hasAcceptedMaterialShader,
            bool technicalImportReady)
        {
            Spec = spec;
            GlbPaths = glbPaths;
            LodsPresent = lodsPresent;
            Lod0ImportedPrefab = lod0ImportedPrefab;
            ExternalMaterials = externalMaterials;
            MaterialShaders = materialShaders;
            AcceptedMaterialShaderCount = acceptedMaterialShaderCount;
            MapPresence = mapPresence;
            AllLodsPresent = allLodsPresent;
            PbrSidecarsComplete = pbrSidecarsComplete;
            HasAcceptedMaterialShader = hasAcceptedMaterialShader;
            TechnicalImportReady = technicalImportReady;
        }

        public readonly ClassSpec Spec;
        public readonly string[] GlbPaths;
        public readonly bool[] LodsPresent;
        public readonly bool Lod0ImportedPrefab;
        public readonly string[] ExternalMaterials;
        public readonly string[] MaterialShaders;
        public readonly int AcceptedMaterialShaderCount;
        public readonly MapPresence MapPresence;
        public readonly bool AllLodsPresent;
        public readonly bool PbrSidecarsComplete;
        public readonly bool HasAcceptedMaterialShader;
        public readonly bool TechnicalImportReady;
    }
}
#endif
