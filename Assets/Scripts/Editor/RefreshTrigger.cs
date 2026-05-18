using UnityEngine;
using UnityEditor;

public class RefreshTrigger : AssetPostprocessor
{
    static RefreshTrigger()
    {
        Debug.Log("[RefreshTrigger] Editor recompiled - forcing asset refresh");
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }
}
// trigger
