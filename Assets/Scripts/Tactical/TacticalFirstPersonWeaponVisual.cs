using System.Collections.Generic;
using UnityEngine;

public class TacticalFirstPersonWeaponVisual : MonoBehaviour
{
    [SerializeField] private TacticalPlayerController player;

    private sealed class WeaponPose
    {
        public Transform root;
        public Vector3 baseLocalPosition;
        public Quaternion baseLocalRotation;
        public Vector3 baseLocalScale;
    }

    private readonly List<WeaponPose> weaponPoses = new();
    private TacticalGameManager game;
    private string activeWeaponId = "";
    private string lastSkinName = "";
    private Color lastPrimaryColor = Color.white;
    private int lastTintedRenderers;
    private int lastPbrPreservedRenderers;
    private Transform activeWeaponRoot;
    private WeaponPose activePose;
    private float swayPhase;
    private float recoilKick;
    private float reloadBlend;
    private float selectBlend;
    private float reloadDuration = 1f;
    private Vector3 lastPolishOffset;
    private Vector3 lastReloadOffset;
    private float lastRecoilKick;
    private float recoilSideKick;
    private float recoilRollKick;
    private float peakRecoilKickObserved;
    private float peakPoseMagnitudeObserved;
    private float reloadPoseMagnitudeObserved;
    private float selectRaiseMagnitudeObserved;
    private float lastAdsStability;
    private float lastMovementSwayMagnitude;
    private float lastPoseMagnitude;
    private int shotPolishEvents;
    private int reloadPolishEvents;
    private int selectPolishEvents;
    private bool hasActiveHeroWeapon;

    public string ActiveWeaponId => activeWeaponId;
    public string LastSkinName => lastSkinName;
    public Color LastPrimaryColor => lastPrimaryColor;
    public int LastTintedRenderers => lastTintedRenderers;
    public int LastPbrPreservedRenderers => lastPbrPreservedRenderers;
    public Vector3 LastPolishOffset => lastPolishOffset;
    public Vector3 LastReloadOffset => lastReloadOffset;
    public float LastRecoilKick => lastRecoilKick;
    public float PeakRecoilKickObserved => peakRecoilKickObserved;
    public float PeakPoseMagnitudeObserved => peakPoseMagnitudeObserved;
    public float ReloadPoseMagnitudeObserved => reloadPoseMagnitudeObserved;
    public float SelectRaiseMagnitudeObserved => selectRaiseMagnitudeObserved;
    public float LastAdsStability => lastAdsStability;
    public float LastMovementSwayMagnitude => lastMovementSwayMagnitude;
    public float LastPoseMagnitude => lastPoseMagnitude;
    public float RecoilSideKick => recoilSideKick;
    public float RecoilRollKick => recoilRollKick;
    public int ShotPolishEvents => shotPolishEvents;
    public int ReloadPolishEvents => reloadPolishEvents;
    public int SelectPolishEvents => selectPolishEvents;
    public bool HasActiveHeroWeapon => hasActiveHeroWeapon;

    public void SetPlayer(TacticalPlayerController controller)
    {
        player = controller;
    }

    public void NotifyShot(TacticalWeaponSpec spec)
    {
        var weaponRecoil = spec == null ? 0.12f : Mathf.Max(spec.visualRecoilKick, Mathf.Clamp(spec.recoil * 5.5f, 0.08f, 0.24f));
        recoilKick = Mathf.Clamp(recoilKick + weaponRecoil, 0f, 0.32f);
        recoilSideKick = Mathf.Clamp(recoilSideKick + Random.Range(-0.040f, 0.040f), -0.075f, 0.075f);
        recoilRollKick = Mathf.Clamp(recoilRollKick + Random.Range(-3.2f, 3.2f), -5.0f, 5.0f);
        peakRecoilKickObserved = Mathf.Max(peakRecoilKickObserved, recoilKick);
        shotPolishEvents++;
    }

    public void NotifyReload(float duration)
    {
        reloadDuration = Mathf.Max(0.2f, duration);
        reloadBlend = 1f;
        reloadPoseMagnitudeObserved = Mathf.Max(reloadPoseMagnitudeObserved, 0.10f);
        reloadPolishEvents++;
    }

    public void NotifyWeaponSelected()
    {
        selectBlend = 1f;
        selectRaiseMagnitudeObserved = Mathf.Max(selectRaiseMagnitudeObserved, 0.08f);
        selectPolishEvents++;
    }

    public void ApplyPreviewPolish(float speed01, bool ads, float recoil01, float reload01)
    {
        swayPhase += 0.25f + Mathf.Clamp01(speed01) * 0.45f;
        recoilKick = Mathf.Clamp01(recoil01) * 0.22f;
        reloadBlend = Mathf.Clamp01(reload01);
        ApplyPolishPose(ads);
        peakRecoilKickObserved = Mathf.Max(peakRecoilKickObserved, recoilKick);
        reloadPoseMagnitudeObserved = Mathf.Max(reloadPoseMagnitudeObserved, lastReloadOffset.magnitude);
        peakPoseMagnitudeObserved = Mathf.Max(peakPoseMagnitudeObserved, lastPoseMagnitude);
        selectRaiseMagnitudeObserved = Mathf.Max(selectRaiseMagnitudeObserved, selectBlend * 0.08f);
    }

    private void Awake()
    {
        game = FindAnyObjectByType<TacticalGameManager>();
    }

    private void Start()
    {
        ForceRefresh();
    }

    private void LateUpdate()
    {
        ForceRefresh();
        TickPolish(Time.deltaTime);
    }

    public void ForceRefresh()
    {
        var visible = player != null && (player.CameraMode == TacticalCameraMode.FirstPerson || player.IsAds);
        activeWeaponId = game == null ? "pistol" : game.CurrentWeaponId;
        lastSkinName = game == null ? "黑铁" : game.CurrentSkinName;
        lastPrimaryColor = game == null ? Color.white : game.CurrentSkinPrimary;
        var accent = game == null ? Color.gray : game.CurrentSkinAccent;
        lastTintedRenderers = 0;
        lastPbrPreservedRenderers = 0;
        var hasWeaponGroups = false;
        activeWeaponRoot = null;
        activePose = null;

        foreach (Transform child in transform)
        {
            if (!child.name.StartsWith("FP Weapon - "))
            {
                continue;
            }

            hasWeaponGroups = true;
            var weaponId = child.name.Substring("FP Weapon - ".Length);
            var pose = EnsurePose(child, weaponId);
            SetRenderers(child.gameObject, visible && weaponId == activeWeaponId);
            if (weaponId == activeWeaponId)
            {
                activeWeaponRoot = child;
                activePose = pose;
                lastTintedRenderers += ApplySkin(child.gameObject, lastPrimaryColor, accent, out var preserved);
                lastPbrPreservedRenderers += preserved;
            }
        }

        if (hasWeaponGroups)
        {
            hasActiveHeroWeapon = visible && activeWeaponRoot != null && CountEnabledRenderers(activeWeaponRoot.gameObject) > 0;
            ApplyPolishPose(player != null && player.IsAds);
            return;
        }

        SetRenderers(gameObject, visible);
        lastTintedRenderers = ApplySkin(gameObject, lastPrimaryColor, accent, out var fallbackPreserved);
        lastPbrPreservedRenderers = fallbackPreserved;
        activeWeaponRoot = transform;
        activePose = EnsurePose(transform, "fallback");
        hasActiveHeroWeapon = visible && CountEnabledRenderers(gameObject) > 0;
        ApplyPolishPose(player != null && player.IsAds);
    }

    private void TickPolish(float deltaTime)
    {
        if (activeWeaponRoot == null || activePose == null)
        {
            lastPolishOffset = Vector3.zero;
            lastReloadOffset = Vector3.zero;
            lastRecoilKick = 0f;
            return;
        }

        var dt = Mathf.Max(deltaTime, 0.0001f);
        var ads = player != null && player.IsAds;
        swayPhase += dt * (ads ? 2.2f : 3.8f);
        recoilKick = Mathf.MoveTowards(recoilKick, 0f, dt * 4.8f);
        recoilSideKick = Mathf.MoveTowards(recoilSideKick, 0f, dt * 6.2f);
        recoilRollKick = Mathf.MoveTowards(recoilRollKick, 0f, dt * 7.4f);
        reloadBlend = Mathf.MoveTowards(reloadBlend, 0f, dt / reloadDuration);
        selectBlend = Mathf.MoveTowards(selectBlend, 0f, dt * 5.5f);
        ApplyPolishPose(ads);
    }

    private void ApplyPolishPose(bool ads)
    {
        if (activeWeaponRoot == null || activePose == null)
        {
            return;
        }

        var movementSpeed01 = 0f;
        if (player != null)
        {
            var controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                movementSpeed01 = Mathf.Clamp01(controller.velocity.magnitude / 7.5f);
            }
        }

        var swayStrength = ads ? 0.18f : 1f;
        var stride = Mathf.Sin(swayPhase);
        var idleOffset = new Vector3(stride * 0.008f * swayStrength, Mathf.Cos(swayPhase * 0.7f) * 0.006f * swayStrength, 0f);
        var movementOffset = new Vector3(
            Mathf.Sin(swayPhase * 1.25f) * 0.014f * movementSpeed01 * swayStrength,
            Mathf.Cos(swayPhase * 1.75f) * 0.012f * movementSpeed01 * swayStrength,
            Mathf.Sin(swayPhase * 0.85f) * 0.010f * movementSpeed01 * swayStrength);
        lastMovementSwayMagnitude = idleOffset.magnitude + movementOffset.magnitude;
        var recoilOffset = new Vector3(recoilSideKick + 0.016f * recoilKick, -0.038f * recoilKick, -0.30f * recoilKick);
        lastReloadOffset = new Vector3(-0.050f * reloadBlend, -0.155f * reloadBlend, 0.080f * reloadBlend);
        var selectOffset = new Vector3(0.010f * selectBlend, -0.045f * selectBlend, -0.080f * selectBlend);
        lastPolishOffset = idleOffset + movementOffset + recoilOffset + lastReloadOffset + selectOffset;
        lastPoseMagnitude = lastPolishOffset.magnitude;
        lastRecoilKick = recoilKick;
        peakPoseMagnitudeObserved = Mathf.Max(peakPoseMagnitudeObserved, lastPoseMagnitude);
        reloadPoseMagnitudeObserved = Mathf.Max(reloadPoseMagnitudeObserved, lastReloadOffset.magnitude);
        selectRaiseMagnitudeObserved = Mathf.Max(selectRaiseMagnitudeObserved, selectOffset.magnitude);
        lastAdsStability = ads ? Mathf.Clamp01(1f - lastMovementSwayMagnitude / 0.055f) : Mathf.Clamp01(0.55f - movementSpeed01 * 0.15f);

        var reloadPitch = 18f * reloadBlend;
        var reloadRoll = 8f * reloadBlend;
        var selectYaw = -3.5f * selectBlend;
        var recoilPitch = -7f * recoilKick;
        var recoilYaw = recoilSideKick * 36f;
        var recoilRoll = recoilRollKick;
        var idleRoll = stride * 1.2f * swayStrength;

        activeWeaponRoot.localPosition = activePose.baseLocalPosition + lastPolishOffset;
        activeWeaponRoot.localRotation = activePose.baseLocalRotation * Quaternion.Euler(reloadPitch + recoilPitch, selectYaw + recoilYaw, reloadRoll + idleRoll + recoilRoll);
        activeWeaponRoot.localScale = activePose.baseLocalScale * (1f + selectBlend * 0.018f);
    }

    private WeaponPose EnsurePose(Transform root, string weaponId)
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
            if (IsHiddenSourceGlbRenderer(renderer))
            {
                renderer.enabled = false;
                continue;
            }

            renderer.enabled = enabled;
        }
    }

    private static bool IsHiddenSourceGlbRenderer(Renderer renderer)
    {
        var current = renderer.transform;
        while (current != null)
        {
            if (current.name.Contains("FP Hidden Source GLB", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static int ApplySkin(GameObject root, Color primary, Color accent, out int pbrPreserved)
    {
        var count = 0;
        pbrPreserved = 0;
        var block = new MaterialPropertyBlock();
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (UsesAuthoredPbrTexture(renderer))
            {
                pbrPreserved++;
                continue;
            }

            var tint = count % 3 == 0 ? accent : primary;
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", tint);
            block.SetColor("_Color", tint);
            renderer.SetPropertyBlock(block);
            count++;
        }

        return count;
    }

    private static bool UsesAuthoredPbrTexture(Renderer renderer)
    {
        foreach (var material in renderer.sharedMaterials)
        {
            if (material == null)
            {
                continue;
            }

            if (material.name.Contains("PBR") || material.name.Contains("PbrApproved"))
            {
                return true;
            }

            if (material.HasProperty("_BaseMap") && material.GetTexture("_BaseMap") != null)
            {
                return true;
            }

            if (material.HasProperty("_BumpMap") && material.GetTexture("_BumpMap") != null)
            {
                return true;
            }
        }

        return false;
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
}
