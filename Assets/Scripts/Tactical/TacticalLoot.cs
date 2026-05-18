using UnityEngine;

public enum TacticalLootKind
{
    Ammo,
    Bandage,
    FirstAid,
    Medkit,
    Revive,
    Vest,
    Helmet,
    Weapon
}

public class TacticalLoot : MonoBehaviour
{
    [SerializeField] private TacticalLootKind kind;
    [SerializeField] private string weaponId;
    [SerializeField] private int level = 1;
    [SerializeField] private string displayName = "Loot";

    public TacticalLootKind Kind => kind;
    public string WeaponId => weaponId;
    public int Level => level;
    public string DisplayName => displayName;

    public void Configure(TacticalLootKind lootKind, string name, int lootLevel = 1, string lootWeaponId = "")
    {
        kind = lootKind;
        displayName = name;
        level = lootLevel;
        weaponId = lootWeaponId;
        gameObject.name = "Loot - " + displayName;
    }
}
