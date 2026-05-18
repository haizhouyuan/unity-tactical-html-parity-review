#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ResearchPacketCandidateContactSheet
{
    private const string OutputDirectory = "Assets/Screenshots/ResearchPacketCandidateSheets";
    private const string ReportPath = "docs/RESEARCH_PACKET_CANDIDATE_CONTACT_SHEETS.json";
    private const int Width = 1600;
    private const int Height = 900;
    private const int ContactSheetLayer = 31;

    private static readonly CandidateSpec[] Candidates =
    {
        new CandidateSpec(
            "player_tactical_candidate",
            "character",
            "Assets/HtmlTacticalAssets/ResearchPacketCandidates/player_tactical_candidate/player_tactical_candidate.glb",
            "Assets/HtmlTacticalAssets/ResearchPacketCandidates/player_tactical_candidate/player_tactical_candidate_PBR.mat",
            new[]
            {
                "Assets/HtmlTacticalAssets/ResearchPacketCandidates/player_tactical_candidate/player_tactical_candidate_basecolor.png",
                "Assets/HtmlTacticalAssets/ResearchPacketCandidates/player_tactical_candidate/player_tactical_candidate_normal.png",
                "Assets/HtmlTacticalAssets/ResearchPacketCandidates/player_tactical_candidate/player_tactical_candidate_roughness.png",
                "Assets/HtmlTacticalAssets/ResearchPacketCandidates/player_tactical_candidate/player_tactical_candidate_metallic.png",
                "Assets/HtmlTacticalAssets/ResearchPacketCandidates/player_tactical_candidate/player_tactical_candidate_ao.png",
            }),
        new CandidateSpec(
            "enemy_tactical_candidate",
            "character",
            "Assets/HtmlTacticalAssets/ResearchPacketCandidates/enemy_tactical_candidate/enemy_tactical_candidate.glb",
            "Assets/HtmlTacticalAssets/ResearchPacketCandidates/enemy_tactical_candidate/enemy_tactical_candidate_PBR.mat",
            new[]
            {
                "Assets/HtmlTacticalAssets/ResearchPacketCandidates/enemy_tactical_candidate/enemy_tactical_candidate_basecolor.png",
                "Assets/HtmlTacticalAssets/ResearchPacketCandidates/enemy_tactical_candidate/enemy_tactical_candidate_normal.png",
                "Assets/HtmlTacticalAssets/ResearchPacketCandidates/enemy_tactical_candidate/enemy_tactical_candidate_roughness.png",
                "Assets/HtmlTacticalAssets/ResearchPacketCandidates/enemy_tactical_candidate/enemy_tactical_candidate_metallic.png",
                "Assets/HtmlTacticalAssets/ResearchPacketCandidates/enemy_tactical_candidate/enemy_tactical_candidate_ao.png",
            }),
    };

    [MenuItem("AI Tools/Render Research Packet Candidate Contact Sheets")]
    public static void RenderAll()
    {
        Directory.CreateDirectory(OutputDirectory);
        Directory.CreateDirectory("docs");
        AssetDatabase.Refresh();

        var oldActive = RenderTexture.active;
        var renderResult = RenderCandidates();

        var report = new StringBuilder();
        report.AppendLine("{");
        Append(report, "schema", "research_packet_candidate_contact_sheets_v1", true);
        Append(report, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(report, "output_directory", OutputDirectory, true);
        Append(report, "contact_sheet_path", renderResult.Path, true);
        Append(report, "rendered", !string.IsNullOrEmpty(renderResult.Path) && File.Exists(renderResult.Path), true);
        Append(report, "candidate_count", Candidates.Length, true);
        report.AppendLine("  \"candidates\": [");
        for (var i = 0; i < renderResult.Candidates.Count; i++)
        {
            var candidate = renderResult.Candidates[i];
            report.AppendLine("    {");
            Append(report, "asset_id", candidate.AssetId, true, 6);
            Append(report, "expected_category", candidate.ExpectedCategory, true, 6);
            Append(report, "unity_path", candidate.UnityPath, true, 6);
            Append(report, "material_path", candidate.MaterialPath, true, 6);
            Append(report, "sha256", candidate.Sha256, true, 6);
            Append(report, "loaded", candidate.Loaded, true, 6);
            Append(report, "renderer_count", candidate.RendererCount, true, 6);
            Append(report, "material_count", candidate.MaterialCount, true, 6);
            Append(report, "texture_count", candidate.TextureCount, true, 6);
            Append(report, "all_textures_present", candidate.AllTexturesPresent, false, 6);
            report.Append("    }");
            report.AppendLine(i == renderResult.Candidates.Count - 1 ? "" : ",");
        }

        report.AppendLine("  ]");
        report.AppendLine("}");
        File.WriteAllText(ReportPath, report.ToString());

        RenderTexture.active = oldActive;
        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] Research packet candidate contact sheet written to " + ReportPath);
    }

    private static CandidateRenderResult RenderCandidates()
    {
        var root = new GameObject("Research Packet Candidate Contact Sheet");
        var cameraObject = new GameObject("Research Packet Candidate Contact Sheet Camera");
        var lightObject = new GameObject("Research Packet Candidate Contact Sheet Key Light");
        var records = new List<CandidateRenderRecord>();

        try
        {
            root.hideFlags = HideFlags.HideAndDontSave;
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            lightObject.hideFlags = HideFlags.HideAndDontSave;

            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 2.2f;
            light.transform.rotation = Quaternion.Euler(52f, -34f, 0f);

            var slotSpacing = 4.8f;
            for (var i = 0; i < Candidates.Length; i++)
            {
                var spec = Candidates[i];
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.UnityPath);
                if (prefab == null)
                {
                    Debug.LogWarning("[ResearchPacketCandidateContactSheet] Missing candidate: " + spec.UnityPath);
                    records.Add(new CandidateRenderRecord(spec, false, 0, 0));
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (instance == null)
                {
                    records.Add(new CandidateRenderRecord(spec, false, 0, 0));
                    continue;
                }

                instance.name = spec.AssetId;
                instance.hideFlags = HideFlags.HideAndDontSave;
                instance.transform.SetParent(root.transform, false);
                SetLayerRecursively(instance, ContactSheetLayer);
                instance.transform.position = new Vector3((i - (Candidates.Length - 1) * 0.5f) * slotSpacing, 0f, 0f);
                instance.transform.rotation = Quaternion.Euler(0f, 28f, 0f);
                NormalizeInstance(instance);
                ApplyCandidateMaterial(instance, spec);
                AddLabel(root.transform, instance, spec.AssetId, spec.ExpectedCategory);

                var renderers = instance.GetComponentsInChildren<Renderer>();
                var materialCount = 0;
                foreach (var renderer in renderers)
                {
                    materialCount += renderer.sharedMaterials.Length;
                }

                records.Add(new CandidateRenderRecord(spec, true, renderers.Length, materialCount));
            }

            var bounds = GetBounds(root);
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.20f, 0.22f, 0.24f);
            camera.cullingMask = 1 << ContactSheetLayer;
            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(2.8f, bounds.size.x * 0.34f, bounds.size.y * 0.98f);
            camera.transform.position = bounds.center + new Vector3(0f, bounds.size.y * 0.35f + 2.4f, -8.5f);
            camera.transform.LookAt(bounds.center + Vector3.up * bounds.extents.y * 0.15f);
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 100f;

            var texture = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = texture;
            camera.Render();
            RenderTexture.active = texture;
            var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            image.Apply();
            RenderTexture.active = null;

            var outputPath = OutputDirectory + "/research_packet_character_candidates_contact_sheet.png";
            File.WriteAllBytes(outputPath, image.EncodeToPNG());

            UnityEngine.Object.DestroyImmediate(image);
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(texture);
            return new CandidateRenderResult(outputPath, records);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(lightObject);
        }
    }

    private static void ApplyCandidateMaterial(GameObject instance, CandidateSpec spec)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(spec.MaterialPath);
        if (material == null)
        {
            return;
        }

        foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
        {
            var materials = renderer.sharedMaterials;
            for (var i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }

            renderer.sharedMaterials = materials;
        }
    }

    private static void AddLabel(Transform root, GameObject instance, string assetId, string category)
    {
        var bounds = GetBounds(instance);
        var labelObject = new GameObject("Label - " + assetId);
        labelObject.hideFlags = HideFlags.HideAndDontSave;
        labelObject.transform.SetParent(root, false);
        labelObject.transform.position = new Vector3(bounds.center.x, bounds.min.y - 0.32f, bounds.center.z);
        labelObject.transform.rotation = Quaternion.Euler(72f, 0f, 0f);
        labelObject.layer = ContactSheetLayer;

        var text = labelObject.AddComponent<TextMesh>();
        text.text = category + "\n" + assetId;
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
            instance.transform.localScale *= 2.35f / maxDimension;
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

    private static string ComputeSha256(string unityPath)
    {
        if (string.IsNullOrEmpty(unityPath) || !File.Exists(unityPath))
        {
            return "";
        }

        using var stream = File.OpenRead(unityPath);
        using var sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }

    private readonly struct CandidateSpec
    {
        public readonly string AssetId;
        public readonly string ExpectedCategory;
        public readonly string UnityPath;
        public readonly string MaterialPath;
        public readonly string[] TexturePaths;

        public CandidateSpec(string assetId, string expectedCategory, string unityPath, string materialPath, string[] texturePaths)
        {
            AssetId = assetId;
            ExpectedCategory = expectedCategory;
            UnityPath = unityPath;
            MaterialPath = materialPath;
            TexturePaths = texturePaths;
        }
    }

    private readonly struct CandidateRenderResult
    {
        public readonly string Path;
        public readonly List<CandidateRenderRecord> Candidates;

        public CandidateRenderResult(string path, List<CandidateRenderRecord> candidates)
        {
            Path = path;
            Candidates = candidates;
        }
    }

    private readonly struct CandidateRenderRecord
    {
        public readonly string AssetId;
        public readonly string ExpectedCategory;
        public readonly string UnityPath;
        public readonly string MaterialPath;
        public readonly string Sha256;
        public readonly bool Loaded;
        public readonly int RendererCount;
        public readonly int MaterialCount;
        public readonly int TextureCount;
        public readonly bool AllTexturesPresent;

        public CandidateRenderRecord(CandidateSpec spec, bool loaded, int rendererCount, int materialCount)
        {
            AssetId = spec.AssetId;
            ExpectedCategory = spec.ExpectedCategory;
            UnityPath = spec.UnityPath;
            MaterialPath = spec.MaterialPath;
            Sha256 = ComputeSha256(spec.UnityPath);
            Loaded = loaded;
            RendererCount = rendererCount;
            MaterialCount = materialCount;
            TextureCount = spec.TexturePaths.Length;
            AllTexturesPresent = true;
            foreach (var texturePath in spec.TexturePaths)
            {
                if (!File.Exists(texturePath))
                {
                    AllTexturesPresent = false;
                    break;
                }
            }
        }
    }
}
#endif
