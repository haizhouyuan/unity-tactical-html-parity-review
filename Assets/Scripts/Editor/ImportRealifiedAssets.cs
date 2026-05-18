using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class ImportRealifiedAssets : AssetPostprocessor
{
    [MenuItem("Tactical Assets/Import Realified Hero Rifle")]
    static void ImportHeroRifle()
    {
        string sourcePath = "Assets/HtmlTacticalAssets/RealifiedAssets/hero_rifle_textured.glb";
        
        // Force import
        AssetDatabase.ImportAsset(sourcePath, ImportAssetOptions.ForceUpdate);
        
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
        if (prefab == null)
        {
            Debug.LogError("[ImportRealifiedAssets] Failed to import hero_rifle_textured.glb");
            return;
        }
        
        Debug.Log($"[ImportRealifiedAssets] Imported: {prefab.name}");
        
        // Check material setup
        var renderers = prefab.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            Material mat = renderer.sharedMaterial;
            if (mat != null)
            {
                Debug.Log($"[ImportRealifiedAssets] Material: {mat.name}, Shader: {mat.shader.name}");
                Debug.Log($"  - Albedo: {mat.GetTexture("_BaseMap") != null}");
                Debug.Log($"  - Metallic: {mat.GetTexture("_MetallicGlossMap") != null}");
            }
        }
        
        // Create prefab instance in scene for preview
        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        instance.name = "hero_rifle_preview";
        instance.transform.position = Vector3.zero;
        
        Debug.Log("[ImportRealifiedAssets] Hero rifle imported and instantiated in scene");
    }
    
    [MenuItem("Tactical Assets/Import All Batch Assets")]
    static void ImportAllBatchAssets()
    {
        string[] glbFiles = Directory.GetFiles("Assets/HtmlTacticalAssets/RealifiedAssets", "*.glb", SearchOption.TopDirectoryOnly);
        
        int imported = 0;
        foreach (string glbPath in glbFiles)
        {
            string assetPath = glbPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                Debug.Log($"[ImportRealifiedAssets] Imported: {prefab.name}");
                imported++;
            }
        }
        
        Debug.Log($"[ImportRealifiedAssets] Total imported: {imported}/{glbFiles.Length}");
    }
}
