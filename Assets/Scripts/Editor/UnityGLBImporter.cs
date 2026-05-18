using UnityEngine;
using UnityEditor;
using System.IO;

public class GLBImporter : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string assetPath in importedAssets)
        {
            if (assetPath.EndsWith(".glb") || assetPath.EndsWith(".gltf"))
            {
                SetupPBRMaterials(assetPath);
            }
        }
    }

    private static void SetupPBRMaterials(string assetPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null) return;

        var dir = Path.GetDirectoryName(assetPath);
        var assetName = Path.GetFileNameWithoutExtension(assetPath);
        
        var baseColorPath = FirstExistingPath(
            Path.Combine(dir, assetName + "_basecolor.png"),
            Path.Combine(dir, assetName + "_albedo.png"));
        var metallicPath = Path.Combine(dir, assetName + "_metallic.png");
        var roughnessPath = Path.Combine(dir, assetName + "_roughness.png");
        var normalPath = Path.Combine(dir, assetName + "_normal.png");
        var aoPath = FirstExistingPath(
            Path.Combine(dir, assetName + "_ao.png"),
            Path.Combine(dir, assetName + "_occlusion.png"));

        if (baseColorPath == "" && !File.Exists(normalPath) && !File.Exists(metallicPath) && !File.Exists(aoPath))
        {
            return;
        }

        var materialPath = Path.Combine(dir, assetName + "_PBR.mat");
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.name = assetName + "_PBR";
            AssetDatabase.CreateAsset(material, materialPath);
        }

        ConfigureTextureImporter(baseColorPath, false);
        ConfigureTextureImporter(metallicPath, false);
        ConfigureTextureImporter(roughnessPath, false);
        ConfigureTextureImporter(normalPath, true);
        ConfigureTextureImporter(aoPath, false);

        var metallicSmoothnessPath = BuildMetallicSmoothnessMap(dir, assetName, metallicPath, roughnessPath);
        SetTextureIfPresent(material, "_BaseMap", baseColorPath);
        SetTextureIfPresent(material, "_MetallicGlossMap", metallicSmoothnessPath);
        SetTextureIfPresent(material, "_BumpMap", normalPath);
        SetTextureIfPresent(material, "_OcclusionMap", aoPath);
        material.SetFloat("_Smoothness", File.Exists(roughnessPath) ? 1f : 0.48f);
        if (material.HasProperty("_SmoothnessTextureChannel"))
        {
            material.SetFloat("_SmoothnessTextureChannel", 0f);
        }
        material.SetFloat("_BumpScale", 0.7f);
        material.EnableKeyword("_METALLICSPECGLOSSMAP");
        material.EnableKeyword("_NORMALMAP");
        EditorUtility.SetDirty(material);

        var renderers = prefab.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.sharedMaterial = material;
        }

        Debug.Log($"[GLBImporter] PBR material setup complete for {assetName}");
    }

    private static string FirstExistingPath(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                return path;
            }
        }

        return "";
    }

    private static string BuildMetallicSmoothnessMap(string dir, string assetName, string metallicPath, string roughnessPath)
    {
        if (string.IsNullOrEmpty(roughnessPath) || !File.Exists(roughnessPath))
        {
            return metallicPath;
        }

        var roughness = LoadImageFromDisk(roughnessPath);
        if (roughness == null)
        {
            return metallicPath;
        }

        var metallic = LoadImageFromDisk(metallicPath);
        var width = roughness.width;
        var height = roughness.height;
        var packed = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
        var packedPixels = new Color32[width * height];
        var roughnessPixels = roughness.GetPixels32();
        var metallicPixels = metallic == null ? null : metallic.GetPixels32();

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var roughnessValue = GrayscaleByte(roughnessPixels[y * width + x]);
                var metallicValue = metallicPixels == null
                    ? (byte)0
                    : GrayscaleByte(SamplePixel(metallicPixels, metallic.width, metallic.height, x, y, width, height));
                var smoothnessValue = (byte)(255 - roughnessValue);
                packedPixels[y * width + x] = new Color32(metallicValue, metallicValue, metallicValue, smoothnessValue);
            }
        }

        packed.SetPixels32(packedPixels);
        packed.Apply(false, false);

        var packedPath = Path.Combine(dir, assetName + "_metallic_smoothness.png");
        File.WriteAllBytes(packedPath, packed.EncodeToPNG());
        AssetDatabase.ImportAsset(packedPath, ImportAssetOptions.ForceUpdate);
        ConfigureTextureImporter(packedPath, false);

        Object.DestroyImmediate(roughness);
        if (metallic != null)
        {
            Object.DestroyImmediate(metallic);
        }
        Object.DestroyImmediate(packed);

        return packedPath;
    }

    private static Texture2D LoadImageFromDisk(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return null;
        }

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
        if (texture.LoadImage(File.ReadAllBytes(path)))
        {
            return texture;
        }

        Object.DestroyImmediate(texture);
        return null;
    }

    private static Color32 SamplePixel(Color32[] pixels, int sourceWidth, int sourceHeight, int x, int y, int targetWidth, int targetHeight)
    {
        var sourceX = targetWidth <= 1 ? 0 : Mathf.Clamp(Mathf.RoundToInt(x * (sourceWidth - 1f) / (targetWidth - 1f)), 0, sourceWidth - 1);
        var sourceY = targetHeight <= 1 ? 0 : Mathf.Clamp(Mathf.RoundToInt(y * (sourceHeight - 1f) / (targetHeight - 1f)), 0, sourceHeight - 1);
        return pixels[sourceY * sourceWidth + sourceX];
    }

    private static byte GrayscaleByte(Color32 color)
    {
        return (byte)Mathf.Clamp(Mathf.RoundToInt((color.r + color.g + color.b) / 3f), 0, 255);
    }

    private static void ConfigureTextureImporter(string path, bool normalMap)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return;
        }

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
        {
            return;
        }

        var changed = false;
        var desiredType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
        if (importer.textureType != desiredType)
        {
            importer.textureType = desiredType;
            changed = true;
        }

        if (!normalMap && !path.Contains("_basecolor") && !path.Contains("_albedo") && importer.sRGBTexture)
        {
            importer.sRGBTexture = false;
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

    private static void SetTextureIfPresent(Material material, string propertyName, string texturePath)
    {
        if (string.IsNullOrEmpty(texturePath) || !File.Exists(texturePath) || !material.HasProperty(propertyName))
        {
            return;
        }

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture != null)
        {
            material.SetTexture(propertyName, texture);
        }
    }

    [MenuItem("Assets/Import Tactical GLB with PBR")]
    private static void ImportSelectedGLB()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!string.IsNullOrEmpty(path) && (path.EndsWith(".glb") || path.EndsWith(".gltf")))
        {
            SetupPBRMaterials(path);
        }
    }
}
