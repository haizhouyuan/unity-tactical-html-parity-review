using System;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using GLTFast.Logging;
using UnityEngine;

public class HtmlGlbAssetMount : MonoBehaviour
{
    [SerializeField] private string assetRelativePath;
    [SerializeField] private Vector3 localPosition;
    [SerializeField] private Vector3 localEulerAngles;
    [SerializeField] private Vector3 localScale = Vector3.one;
    [SerializeField] private bool hidePlaceholderRenderers;

    private bool loaded;

    public bool IsLoaded => loaded;
    public string AssetRelativePath => assetRelativePath;

    public void Configure(string relativePath, Vector3 position, Vector3 eulerAngles, Vector3 scale, bool hidePlaceholder = false)
    {
        assetRelativePath = relativePath;
        localPosition = position;
        localEulerAngles = eulerAngles;
        localScale = scale;
        hidePlaceholderRenderers = hidePlaceholder;
    }

    private async void Start()
    {
        await Load();
    }

    private async Task Load()
    {
        if (string.IsNullOrWhiteSpace(assetRelativePath))
        {
            return;
        }

        var fullPath = Path.Combine(Application.dataPath, "HtmlTacticalAssets", assetRelativePath);
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning("[HTML GLB] Missing asset: " + fullPath);
            return;
        }

        var content = new GameObject("HTML GLB - " + Path.GetFileNameWithoutExtension(assetRelativePath));
        content.transform.SetParent(transform, false);
        content.transform.localPosition = localPosition;
        content.transform.localEulerAngles = localEulerAngles;
        content.transform.localScale = localScale;

        var gltf = new GltfImport(logger: new ConsoleLogger());
        var bytes = await File.ReadAllBytesAsync(fullPath);
        var success = await gltf.Load(bytes, new Uri(fullPath));
        if (!success)
        {
            Debug.LogWarning("[HTML GLB] Failed to load: " + assetRelativePath);
            Destroy(content);
            return;
        }

        success = await gltf.InstantiateMainSceneAsync(content.transform);
        if (!success)
        {
            Debug.LogWarning("[HTML GLB] Failed to instantiate: " + assetRelativePath);
            Destroy(content);
            return;
        }

        foreach (var renderer in content.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        if (hidePlaceholderRenderers)
        {
            foreach (var renderer in transform.parent.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer.transform.IsChildOf(content.transform))
                {
                    renderer.enabled = false;
                }
            }
        }

        loaded = true;
    }
}
