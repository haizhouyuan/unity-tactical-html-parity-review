#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class RealifiedCategoryContactSheet
{
    private const string OutputDirectory = "Assets/Screenshots/RealifiedCategorySheets";
    private const string ReportPath = "docs/REALIFIED_CATEGORY_CONTACT_SHEETS.json";
    private const int Width = 1600;
    private const int Height = 900;
    private const int ContactSheetLayer = 31;

    private static readonly CategorySpec[] Categories =
    {
        new CategorySpec("weapon", new[]
        {
            "RS_02_sidearm_LOD0.glb",
            "RS_03_secondary_weapon_LOD0.glb",
            "RS_12_shotgun_LOD0.glb",
            "hero_rifle_LOD0.glb",
        }),
        new CategorySpec("character", new[]
        {
            "RS_04_player_tactical_LOD0.glb",
            "RS_05_enemy_tactical_LOD0.glb",
        }),
        new CategorySpec("gear", new[]
        {
            "RS_06_gear_helmet_LOD0.glb",
            "RS_07_gear_vest_LOD0.glb",
        }),
        new CategorySpec("loot", new[]
        {
            "RS_08_loot_ammo_LOD0.glb",
            "RS_09_loot_medkit_LOD0.glb",
        }),
        new CategorySpec("environment_prop", new[]
        {
            "RS_10_prop_container_LOD0.glb",
            "RS_11_prop_crate_LOD0.glb",
        }),
    };

    [MenuItem("AI Tools/Render Realified Category Contact Sheets")]
    public static void RenderAll()
    {
        Directory.CreateDirectory(OutputDirectory);
        Directory.CreateDirectory("docs");

        var oldActive = RenderTexture.active;
        var report = new StringBuilder();
        report.AppendLine("{");
        Append(report, "schema", "realified_category_contact_sheets_v1", true);
        Append(report, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(report, "output_directory", OutputDirectory, true);
        Append(report, "category_count", Categories.Length, true);
        report.AppendLine("  \"categories\": [");

        for (var i = 0; i < Categories.Length; i++)
        {
            var category = Categories[i];
            var result = RenderCategory(category);
            report.AppendLine("    {");
            Append(report, "category", category.Name, true, 6);
            Append(report, "path", result.Path, true, 6);
            Append(report, "expected_asset_count", category.Files.Length, true, 6);
            Append(report, "rendered", !string.IsNullOrEmpty(result.Path) && File.Exists(result.Path), true, 6);
            report.AppendLine("      \"assets\": [");
            for (var assetIndex = 0; assetIndex < result.Assets.Count; assetIndex++)
            {
                var asset = result.Assets[assetIndex];
                report.AppendLine("        {");
                Append(report, "file", asset.FileName, true, 10);
                Append(report, "unity_path", asset.UnityPath, true, 10);
                Append(report, "sha256", asset.Sha256, true, 10);
                Append(report, "loaded", asset.Loaded, true, 10);
                Append(report, "renderer_count", asset.RendererCount, true, 10);
                Append(report, "material_count", asset.MaterialCount, false, 10);
                report.Append("        }");
                report.AppendLine(assetIndex == result.Assets.Count - 1 ? "" : ",");
            }

            report.AppendLine("      ]");
            report.Append("    }");
            report.AppendLine(i == Categories.Length - 1 ? "" : ",");
        }

        report.AppendLine("  ]");
        report.AppendLine("}");
        File.WriteAllText(ReportPath, report.ToString());

        RenderTexture.active = oldActive;
        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] Realified category contact sheets written to " + ReportPath);
    }

    private static CategoryRenderResult RenderCategory(CategorySpec category)
    {
        var root = new GameObject("Realified Category Contact Sheet - " + category.Name);
        var cameraObject = new GameObject("Realified Contact Sheet Camera");
        var lightObject = new GameObject("Realified Contact Sheet Key Light");
        var instances = new List<GameObject>();
        var assetRecords = new List<AssetRenderRecord>();

        try
        {
            root.hideFlags = HideFlags.HideAndDontSave;
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            lightObject.hideFlags = HideFlags.HideAndDontSave;

            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 2.1f;
            light.transform.rotation = Quaternion.Euler(52f, -34f, 0f);

            var slotSpacing = 4.6f;
            for (var i = 0; i < category.Files.Length; i++)
            {
                var assetPath = "Assets/HtmlTacticalAssets/RealifiedAssets/" + category.Files[i];
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null)
                {
                    Debug.LogWarning("[RealifiedCategoryContactSheet] Missing asset: " + assetPath);
                    assetRecords.Add(new AssetRenderRecord(category.Files[i], assetPath, Sha256(assetPath), false, 0, 0));
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (instance == null)
                {
                    assetRecords.Add(new AssetRenderRecord(category.Files[i], assetPath, Sha256(assetPath), false, 0, 0));
                    continue;
                }

                instance.name = Path.GetFileNameWithoutExtension(category.Files[i]);
                instance.hideFlags = HideFlags.HideAndDontSave;
                instance.transform.SetParent(root.transform, false);
                SetLayerRecursively(instance, ContactSheetLayer);
                instance.transform.position = new Vector3((i - (category.Files.Length - 1) * 0.5f) * slotSpacing, 0f, 0f);
                instance.transform.rotation = Quaternion.Euler(0f, category.Name == "weapon" ? -24f : 28f, 0f);
                NormalizeInstance(instance);
                AddLabel(root.transform, instance, category.Files[i], category.Name);
                instances.Add(instance);

                var renderers = instance.GetComponentsInChildren<Renderer>();
                var materialCount = 0;
                foreach (var renderer in renderers)
                {
                    materialCount += renderer.sharedMaterials.Length;
                }

                assetRecords.Add(new AssetRenderRecord(
                    category.Files[i],
                    assetPath,
                    Sha256(assetPath),
                    true,
                    renderers.Length,
                    materialCount));
            }

            if (instances.Count == 0)
            {
                return new CategoryRenderResult("", assetRecords);
            }

            var bounds = GetBounds(root);
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.22f, 0.24f, 0.25f);
            camera.cullingMask = 1 << ContactSheetLayer;
            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(2.6f, bounds.size.x * 0.34f, bounds.size.y * 0.92f);
            camera.transform.position = bounds.center + new Vector3(0f, bounds.size.y * 0.35f + 2.4f, -8.5f);
            camera.transform.LookAt(bounds.center + Vector3.up * bounds.extents.y * 0.15f);
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 80f;

            var texture = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = texture;
            camera.Render();
            RenderTexture.active = texture;
            var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            image.Apply();
            RenderTexture.active = null;

            var outputPath = OutputDirectory + "/realified_" + category.Name + "_contact_sheet.png";
            File.WriteAllBytes(outputPath, image.EncodeToPNG());

            UnityEngine.Object.DestroyImmediate(image);
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(texture);
            return new CategoryRenderResult(outputPath, assetRecords);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(lightObject);
        }
    }

    private static void AddLabel(Transform root, GameObject instance, string fileName, string category)
    {
        var bounds = GetBounds(instance);
        var labelObject = new GameObject("Label - " + fileName);
        labelObject.hideFlags = HideFlags.HideAndDontSave;
        labelObject.transform.SetParent(root, false);
        labelObject.transform.position = new Vector3(bounds.center.x, bounds.min.y - 0.32f, bounds.center.z);
        labelObject.transform.rotation = Quaternion.Euler(72f, 0f, 0f);
        labelObject.layer = ContactSheetLayer;

        var text = labelObject.AddComponent<TextMesh>();
        text.text = category + "\n" + Path.GetFileNameWithoutExtension(fileName);
        text.anchor = TextAnchor.UpperCenter;
        text.alignment = TextAlignment.Center;
        text.fontSize = 42;
        text.characterSize = 0.045f;
        text.color = Color.white;
    }

    private static void NormalizeInstance(GameObject instance)
    {
        var bounds = GetBounds(instance);
        var maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (maxDimension > 0.001f)
        {
            instance.transform.localScale *= 2.25f / maxDimension;
        }

        bounds = GetBounds(instance);
        instance.transform.position -= new Vector3(bounds.center.x - instance.transform.position.x, bounds.min.y, bounds.center.z - instance.transform.position.z);
    }

    private static Bounds GetBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(root.transform.position, Vector3.one);
        }

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static void SetLayerRecursively(GameObject root, int layer)
    {
        root.layer = layer;
        foreach (Transform child in root.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private static void Append(StringBuilder builder, string key, string value, bool comma, int indent = 2)
    {
        builder.Append(' ', indent);
        builder.Append('"').Append(key).Append("\": \"").Append(Escape(value)).Append('"');
        builder.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder builder, string key, int value, bool comma, int indent = 2)
    {
        builder.Append(' ', indent);
        builder.Append('"').Append(key).Append("\": ").Append(value.ToString(CultureInfo.InvariantCulture));
        builder.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder builder, string key, bool value, bool comma, int indent = 2)
    {
        builder.Append(' ', indent);
        builder.Append('"').Append(key).Append("\": ").Append(value ? "true" : "false");
        builder.AppendLine(comma ? "," : "");
    }

    private static string Escape(string value)
    {
        return (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private static string Sha256(string unityPath)
    {
        if (string.IsNullOrEmpty(unityPath) || !File.Exists(unityPath))
        {
            return "";
        }

        using var stream = File.OpenRead(unityPath);
        using var sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }

    private readonly struct CategorySpec
    {
        public readonly string Name;
        public readonly string[] Files;

        public CategorySpec(string name, string[] files)
        {
            Name = name;
            Files = files;
        }
    }

    private readonly struct CategoryRenderResult
    {
        public readonly string Path;
        public readonly List<AssetRenderRecord> Assets;

        public CategoryRenderResult(string path, List<AssetRenderRecord> assets)
        {
            Path = path;
            Assets = assets;
        }
    }

    private readonly struct AssetRenderRecord
    {
        public readonly string FileName;
        public readonly string UnityPath;
        public readonly string Sha256;
        public readonly bool Loaded;
        public readonly int RendererCount;
        public readonly int MaterialCount;

        public AssetRenderRecord(
            string fileName,
            string unityPath,
            string sha256,
            bool loaded,
            int rendererCount,
            int materialCount)
        {
            FileName = fileName;
            UnityPath = unityPath;
            Sha256 = sha256;
            Loaded = loaded;
            RendererCount = rendererCount;
            MaterialCount = materialCount;
        }
    }
}
#endif
