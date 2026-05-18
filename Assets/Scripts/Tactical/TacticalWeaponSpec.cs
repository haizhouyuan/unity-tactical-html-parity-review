using UnityEngine;

[System.Serializable]
public class TacticalWeaponSpec
{
    public string id;
    public string displayName;
    public int slot;
    public int magazineSize;
    public int reserveStart;
    public float cooldown;
    public float reloadTime;
    public float damage;
    public float minDamage;
    public float effectiveRange;
    public float range;
    public int pellets;
    public float hipSpread;
    public float adsSpread;
    public float recoil;
    public bool automatic;
    public bool unlockedAtStart;
    public Color color;
    public float visualRecoilKick;
    public float visualReloadPoseMagnitude;
    public float visualSelectRaiseMagnitude;
    public float visualSwayScale;
    public float visualAdsStability;

    public TacticalWeaponSpec(
        string id,
        string displayName,
        int slot,
        int magazineSize,
        int reserveStart,
        float cooldown,
        float reloadTime,
        float damage,
        float range,
        bool automatic,
        bool unlockedAtStart,
        Color color,
        float minDamage = 0f,
        float effectiveRange = 0f,
        int pellets = 1,
        float hipSpread = 0.02f,
        float adsSpread = 0.005f,
        float recoil = 0.02f)
    {
        this.id = id;
        this.displayName = displayName;
        this.slot = slot;
        this.magazineSize = magazineSize;
        this.reserveStart = reserveStart;
        this.cooldown = cooldown;
        this.reloadTime = reloadTime;
        this.damage = damage;
        this.minDamage = minDamage <= 0f ? Mathf.Max(1f, damage * 0.35f) : minDamage;
        this.effectiveRange = effectiveRange <= 0f ? range * 0.55f : effectiveRange;
        this.range = range;
        this.pellets = Mathf.Max(1, pellets);
        this.hipSpread = hipSpread;
        this.adsSpread = adsSpread;
        this.recoil = recoil;
        this.automatic = automatic;
        this.unlockedAtStart = unlockedAtStart;
        this.color = color;
        visualRecoilKick = Mathf.Clamp(recoil * 6.2f, 0.08f, 0.30f);
        visualReloadPoseMagnitude = Mathf.Clamp(reloadTime * 0.11f, 0.09f, 0.24f);
        visualSelectRaiseMagnitude = Mathf.Clamp(0.08f + cooldown * 0.10f, 0.08f, 0.18f);
        visualSwayScale = Mathf.Clamp(0.7f + hipSpread * 8f, 0.75f, 1.45f);
        visualAdsStability = Mathf.Clamp01(1f - adsSpread * 22f);
    }
}

public class TacticalWeaponState
{
    public TacticalWeaponSpec spec;
    public bool unlocked;
    public int magazine;
    public int reserve;
    public bool reloading;
    public float reloadEndTime;
    public float lastShotTime = -999f;

    public TacticalWeaponState(TacticalWeaponSpec spec)
    {
        this.spec = spec;
        unlocked = spec.unlockedAtStart;
        magazine = spec.magazineSize;
        reserve = spec.unlockedAtStart ? spec.reserveStart : 0;
    }
}
