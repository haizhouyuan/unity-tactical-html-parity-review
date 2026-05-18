using UnityEngine;

public class TacticalThirdPersonWeaponVisual : MonoBehaviour
{
    [SerializeField] private TacticalPlayerController player;
    [SerializeField] private TacticalGameManager game;
    [SerializeField] private string fixedWeaponId = "";
    [SerializeField] private bool followCurrentWeapon = true;

    private string activeWeaponId = "";
    private string lastSkinName = "";
    private Color lastPrimaryColor = Color.white;
    private int lastTintedRenderers;

    public string ActiveWeaponId => activeWeaponId;
    public bool FollowCurrentWeapon => followCurrentWeapon;
    public string LastSkinName => lastSkinName;
    public Color LastPrimaryColor => lastPrimaryColor;
    public int LastTintedRenderers => lastTintedRenderers;

    public void ConfigurePlayer(TacticalPlayerController controller, TacticalGameManager manager)
    {
        player = controller;
        game = manager;
        followCurrentWeapon = true;
    }

    public void ConfigureFixedWeapon(string weaponId)
    {
        fixedWeaponId = weaponId;
        followCurrentWeapon = false;
    }

    private void Awake()
    {
        if (game == null)
        {
            game = FindAnyObjectByType<TacticalGameManager>();
        }
    }

    private void Start()
    {
        ForceRefresh();
    }

    private void LateUpdate()
    {
        ForceRefresh();
    }

    public void ForceRefresh()
    {
        activeWeaponId = followCurrentWeapon ? game == null ? "pistol" : game.CurrentWeaponId : fixedWeaponId;
        lastSkinName = game == null ? "黑铁" : game.CurrentSkinName;
        lastPrimaryColor = game == null ? Color.white : game.CurrentSkinPrimary;
        var accent = game == null ? Color.gray : game.CurrentSkinAccent;
        lastTintedRenderers = 0;
        var visible = !followCurrentWeapon || (player != null && player.CameraMode == TacticalCameraMode.ThirdPerson && !player.IsAds);

        foreach (Transform child in transform)
        {
            if (!child.name.StartsWith("Character Weapon - "))
            {
                continue;
            }

            var weaponId = child.name.Substring("Character Weapon - ".Length);
            SetRenderers(child.gameObject, visible && weaponId == activeWeaponId);
            if (followCurrentWeapon && weaponId == activeWeaponId)
            {
                lastTintedRenderers += ApplySkin(child.gameObject, lastPrimaryColor, accent);
            }
        }
    }

    private static void SetRenderers(GameObject root, bool enabled)
    {
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = enabled;
        }
    }

    private static int ApplySkin(GameObject root, Color primary, Color accent)
    {
        var count = 0;
        var block = new MaterialPropertyBlock();
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            var tint = count % 3 == 0 ? accent : primary;
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", tint);
            block.SetColor("_Color", tint);
            renderer.SetPropertyBlock(block);
            count++;
        }

        return count;
    }
}
