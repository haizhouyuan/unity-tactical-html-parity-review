using System.Collections.Generic;
using UnityEngine;

public class TacticalThirdPersonWeaponVisual : MonoBehaviour
{
    [SerializeField] private TacticalPlayerController player;
    [SerializeField] private TacticalGameManager game;
    [SerializeField] private string fixedWeaponId = "";
    [SerializeField] private bool followCurrentWeapon = true;

    private sealed class WeaponPose
    {
        public Transform root;
        public Vector3 baseLocalPosition;
        public Quaternion baseLocalRotation;
        public Vector3 baseLocalScale;
    }

    private readonly List<WeaponPose> weaponPoses = new();
    private string activeWeaponId = "";
    private string lastSkinName = "";
    private Color lastPrimaryColor = Color.white;
    private int lastTintedRenderers;
    private int lastEnabledRenderers;
    private float lastMountQualityScore;
    private float firePulse;
    private float lastFirePulseScale;
    private Vector3 lastAimPulseOffset;
    private int shotPulseEvents;
    private Transform activeWeaponRoot;

    public string ActiveWeaponId => activeWeaponId;
    public bool FollowCurrentWeapon => followCurrentWeapon;
    public string LastSkinName => lastSkinName;
    public Color LastPrimaryColor => lastPrimaryColor;
    public int LastTintedRenderers => lastTintedRenderers;
    public int LastEnabledRenderers => lastEnabledRenderers;
    public float LastMountQualityScore => lastMountQualityScore;
    public float LastFirePulseScale => lastFirePulseScale;
    public Vector3 LastAimPulseOffset => lastAimPulseOffset;
    public int ShotPulseEvents => shotPulseEvents;

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

    public void NotifyShot(TacticalWeaponSpec spec)
    {
        firePulse = Mathf.Clamp01(firePulse + (spec == null ? 0.55f : Mathf.Clamp(spec.visualRecoilKick * 2.4f, 0.35f, 0.85f)));
        shotPulseEvents++;
    }

    public void NotifyWeaponSelected()
    {
        firePulse = Mathf.Max(firePulse, 0.25f);
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
        firePulse = Mathf.MoveTowards(firePulse, 0f, Time.deltaTime * 5.6f);
        ForceRefresh();
    }

    public void ForceRefresh()
    {
        activeWeaponId = followCurrentWeapon ? game == null ? "pistol" : game.CurrentWeaponId : fixedWeaponId;
        lastSkinName = game == null ? "黑铁" : game.CurrentSkinName;
        lastPrimaryColor = game == null ? Color.white : game.CurrentSkinPrimary;
        var accent = game == null ? Color.gray : game.CurrentSkinAccent;
        lastTintedRenderers = 0;
        lastEnabledRenderers = 0;
        lastFirePulseScale = 0f;
        lastAimPulseOffset = Vector3.zero;
        activeWeaponRoot = null;
        var visible = !followCurrentWeapon || (player != null && player.CameraMode == TacticalCameraMode.ThirdPerson && !player.IsAds);

        foreach (Transform child in transform)
        {
            if (!child.name.StartsWith("Character Weapon - "))
            {
                continue;
            }

            var weaponId = child.name.Substring("Character Weapon - ".Length);
            var pose = EnsurePose(child);
            var activeVisible = visible && weaponId == activeWeaponId;
            SetRenderers(child.gameObject, activeVisible);
            if (weaponId == activeWeaponId)
            {
                activeWeaponRoot = child;
                if (activeVisible)
                {
                    lastTintedRenderers += ApplySkin(child.gameObject, lastPrimaryColor, accent);
                    lastEnabledRenderers = CountEnabledRenderers(child.gameObject);
                }
                ApplyPulse(child, pose, activeVisible);
            }
            else
            {
                child.localPosition = pose.baseLocalPosition;
                child.localRotation = pose.baseLocalRotation;
                child.localScale = pose.baseLocalScale;
            }
        }

        EvaluateMountQuality();
    }

    private void ApplyPulse(Transform child, WeaponPose pose, bool activeVisible)
    {
        var pulseOffset = new Vector3(0.012f * firePulse, 0.006f * firePulse, 0.045f * firePulse);
        lastAimPulseOffset = pulseOffset;
        lastFirePulseScale = firePulse;
        child.localPosition = pose.baseLocalPosition + pulseOffset;
        child.localRotation = pose.baseLocalRotation * Quaternion.Euler(-2.8f * firePulse, 1.2f * firePulse, 0.9f * firePulse);
        child.localScale = pose.baseLocalScale * (1f + (activeVisible ? firePulse * 0.018f : 0f));
    }

    private void EvaluateMountQuality()
    {
        var mountPosition = transform.localPosition;
        var heightScore = Mathf.InverseLerp(0.72f, 1.34f, mountPosition.y);
        var forwardScore = Mathf.InverseLerp(0.38f, 0.86f, mountPosition.z);
        var rendererScore = lastEnabledRenderers > 0 ? 1f : 0f;
        lastMountQualityScore = Mathf.Clamp01((heightScore + forwardScore + rendererScore) / 3f);
    }

    private WeaponPose EnsurePose(Transform root)
    {
        foreach (var pose in weaponPoses)
        {
            if (pose.root == root)
            {
                return pose;
            }
        }

        var newPose = new WeaponPose
        {
            root = root,
            baseLocalPosition = root.localPosition,
            baseLocalRotation = root.localRotation,
            baseLocalScale = root.localScale
        };
        weaponPoses.Add(newPose);
        return newPose;
    }

    private static void SetRenderers(GameObject root, bool enabled)
    {
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = enabled;
        }
    }

    private static int CountEnabledRenderers(GameObject root)
    {
        var count = 0;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.enabled)
            {
                count++;
            }
        }
        return count;
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
