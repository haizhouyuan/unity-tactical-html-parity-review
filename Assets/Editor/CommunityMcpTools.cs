#if UNITY_EDITOR
using System.Threading.Tasks;
using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Small wrapper menu for the community MCP for Unity package.
/// Keeps the command easy to execute from Codex or by hand.
/// </summary>
public static class CommunityMcpTools
{
    [MenuItem("AI Tools/Start Community Unity MCP Server")]
    public static async void StartCommunityUnityMcpServer()
    {
        EditorConfigurationCache.Instance.SetUseHttpTransport(true);
        EditorConfigurationCache.Instance.SetHttpTransportScope("local");
        EditorConfigurationCache.Instance.SetHttpBaseUrl("http://127.0.0.1:8080");
        EditorPrefs.SetString("MCPForUnity.HttpUrl", "http://127.0.0.1:8080");
        EditorPrefs.SetBool("MCPForUnity.ProjectScopedTools.LocalHttp", true);
        EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);
        EditorPrefs.SetBool("MCPForUnity.ResumeHttpAfterReload", true);
        EditorConfigurationCache.Instance.Refresh();

        var serverStarted = MCPServiceLocator.Server.StartLocalHttpServer(quiet: true);
        Debug.Log("[AI Tools] Community MCP local HTTP server start requested: " + serverStarted);

        await Task.Delay(500);
        var bridgeStarted = await MCPServiceLocator.Bridge.StartAsync();
        Debug.Log("[AI Tools] Community MCP bridge start requested: " + bridgeStarted);

        for (var attempt = 1; attempt <= 8; attempt++)
        {
            await Task.Delay(500);
            var verification = await MCPServiceLocator.Bridge.VerifyAsync();
            Debug.Log("[AI Tools] Community MCP verification attempt " + attempt + ": " + verification.Success + " - " + verification.Message);
            if (verification.Success)
            {
                return;
            }

            await MCPServiceLocator.Bridge.StartAsync();
        }
    }
}
#endif
