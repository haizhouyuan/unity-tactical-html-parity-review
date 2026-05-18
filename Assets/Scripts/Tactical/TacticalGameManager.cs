using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacticalGameManager : MonoBehaviour
{
    [SerializeField] private TacticalPlayerController player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Text hpText;
    [SerializeField] private Text staminaText;
    [SerializeField] private Text weaponText;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text armorText;
    [SerializeField] private Text inventoryText;
    [SerializeField] private Text npcText;
    [SerializeField] private Text coinText;
    [SerializeField] private Text skinText;
    [SerializeField] private Text messageText;
    [SerializeField] private Text promptText;
    [SerializeField] private Text crosshairText;
    [SerializeField] private Text hitMarkerText;
    [SerializeField] private Text skinCoinText;
    [SerializeField] private Text shardText;
    [SerializeField] private Text npcStrengthValueText;
    [SerializeField] private Text spawnRateValueText;
    [SerializeField] private Text lootRichnessValueText;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject skinsPanel;
    [SerializeField] private Slider npcStrengthSlider;
    [SerializeField] private Slider spawnRateSlider;
    [SerializeField] private Slider lootRichnessSlider;
    [SerializeField] private Button rollSkinButton;
    [SerializeField] private Button closeSkinButton;
    [SerializeField] private int maxActiveEnemies = 12;
    [SerializeField] private float enemyRespawnInterval = 5.5f;
    [SerializeField] private GameObject enemyReinforcementTemplate;

    private AudioSource sfxSource;
    private AudioClip pistolSfx;
    private AudioClip shotgunSfx;
    private AudioClip rifleSfx;
    private AudioClip dmrSfx;
    private AudioClip enemyShotSfx;
    private AudioClip hitSfx;
    private AudioClip pickupSfx;
    private AudioClip reloadSfx;
    private AudioClip footstepSfx;
    private AudioClip damageSfx;
    private int sfxEventCount;
    private TacticalFirstPersonWeaponVisual firstPersonWeaponVisual;
    private readonly Dictionary<string, TacticalWeaponState> weapons = new();
    private readonly List<TacticalEnemy> enemies = new();
    private readonly List<TacticalLoot> loots = new();
    private readonly List<TacticalLadder> ladders = new();

    private float hp = 100f;
    private float stamina = 100f;
    private int kills;
    private int vestLevel;
    private int helmetLevel;
    private int bandages = 2;
    private int firstAid;
    private int medkits;
    private int revives;
    private int coins;
    private int skinShards;
    private int nextCoinAt = 10;
    private string currentWeaponId = "pistol";
    private string currentSkinName = "黑铁";
    private TacticalLoot nearestLoot;
    private TacticalLadder nearestLadder;
    private int currentFloor = 1;
    private float messageUntil;
    private float hitMarkerUntil;
    private bool inLobby = true;
    private float nextEnemyRespawn;
    private float roundStartTime;
    private int runtimeEnemyCounter;
    private readonly Vector3[] enemySpawnPoints =
    {
        new Vector3(-66f, 1f, 48f), new Vector3(-66f, 1f, -44f), new Vector3(66f, 1f, 48f), new Vector3(66f, 1f, -44f),
        new Vector3(0f, 1f, 52f), new Vector3(-8f, 1f, 8f), new Vector3(18f, 1f, -4f), new Vector3(-30f, 1f, 6f),
        new Vector3(42f, 1f, 8f), new Vector3(-50f, 1f, -5f), new Vector3(50f, 1f, 24f), new Vector3(0f, 1f, -48f)
    };

    public bool IsPlayerDown => hp <= 0f;
    public bool IsInLobby => inLobby;
    public bool CurrentWeaponAutomatic => weapons.TryGetValue(currentWeaponId, out var state) && state.spec.automatic;
    public string CurrentWeaponId => currentWeaponId;
    public string CurrentSkinName => currentSkinName;
    public Color CurrentSkinPrimary => SkinPrimaryColor(currentSkinName);
    public Color CurrentSkinAccent => SkinAccentColor(currentSkinName);
    public Color CurrentSkinTracer => SkinTracerColor(currentSkinName);
    public int SfxEventCount => sfxEventCount;
    public bool HasSfxAudioSource => sfxSource != null;
    public int SfxClipCount => CountSfxClips();
    public float EnemyStrengthMultiplier => npcStrengthSlider == null ? 0.85f : npcStrengthSlider.value;
    public float LootRichnessMultiplier => lootRichnessSlider == null ? 1.10f : lootRichnessSlider.value;

    private float SpawnRateMultiplier => spawnRateSlider == null ? 0.45f : spawnRateSlider.value;
    private static readonly string[] SkinNames = { "黑铁", "沙漠迷彩", "深海蓝焰", "霓虹电音", "赤焰龙纹", "极光幻影" };

    private void Awake()
    {
        RegisterWeapons();
        EnsureSfx();
    }

    private void Start()
    {
        RefreshSceneRegistries();

        UpdateHud();
        if (lobbyPanel != null) lobbyPanel.SetActive(true);
        if (deathPanel != null) deathPanel.SetActive(false);
        if (helpPanel != null) helpPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (skinsPanel != null) skinsPanel.SetActive(false);
        if (rollSkinButton != null) rollSkinButton.onClick.AddListener(RollSkin);
        if (closeSkinButton != null) closeSkinButton.onClick.AddListener(() => TogglePanel(skinsPanel, false));
        ShowMessage("按 Enter 或点击开始，进入房区生存。", 999f);
    }

    private void Update()
    {
        HandlePanelHotkeys();
        CompleteReloads();
        if (inLobby)
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (keyboard?.enterKey.wasPressedThisFrame == true ||
                keyboard?.spaceKey.wasPressedThisFrame == true ||
                mouse?.leftButton.wasPressedThisFrame == true)
            {
                StartRound();
            }
            UpdateHud();
            return;
        }

        CleanupEnemyList();
        MaybeSpawnEnemy();
        FindNearestLoot();

        if (messageText != null && Time.time > messageUntil && !IsPlayerDown)
        {
            messageText.text = "";
        }

        if (hitMarkerText != null && Time.time > hitMarkerUntil)
        {
            hitMarkerText.text = "";
        }
    }

    private void HandlePanelHotkeys()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.tabKey.wasPressedThisFrame)
        {
            TogglePanel(helpPanel);
        }

        if (keyboard.oKey.wasPressedThisFrame)
        {
            TogglePanel(settingsPanel);
        }

        if (keyboard.lKey.wasPressedThisFrame)
        {
            TogglePanel(skinsPanel);
        }
    }

    private static void TogglePanel(GameObject panel, bool? state = null)
    {
        if (panel == null)
        {
            return;
        }

        panel.SetActive(state ?? !panel.activeSelf);
    }

    public void RollSkin()
    {
        if (coins <= 0)
        {
            ShowMessage("没有抽奖币：每淘汰 10 个 NPC 获得 1 个。");
            return;
        }

        coins--;
        var nextSkin = SkinNames[Random.Range(0, SkinNames.Length)];
        if (nextSkin == currentSkinName)
        {
            skinShards++;
            ShowMessage("抽到重复皮肤：" + nextSkin + "，碎片 +1。");
            if (skinShards >= 5)
            {
                skinShards -= 5;
                coins++;
                ShowMessage("5 个碎片兑换 1 个抽奖币。");
            }
        }
        else
        {
            currentSkinName = nextSkin;
            ShowMessage("解锁并装备皮肤：" + currentSkinName);
        }

        UpdateHud();
    }

    public void RegisterEnemy(TacticalEnemy enemy)
    {
        if (enemy == null || enemies.Contains(enemy))
        {
            return;
        }

        enemies.Add(enemy);
        enemy.Initialize(this, player.transform);
    }

    public bool TryUseStamina(float amount)
    {
        if (stamina <= 2f)
        {
            return false;
        }

        stamina = Mathf.Max(0f, stamina - amount);
        UpdateHud();
        return true;
    }

    public void RecoverStamina(float amount)
    {
        stamina = Mathf.Min(100f, stamina + amount);
        UpdateHud();
    }

    public void DamagePlayer(float amount)
    {
        if (inLobby)
        {
            return;
        }

        if (Time.time - roundStartTime < 5f)
        {
            ShowMessage("开局保护中，先找掩体和物资。");
            return;
        }

        var reduction = 1f - Mathf.Clamp01(vestLevel * 0.14f + helmetLevel * 0.06f);
        hp = Mathf.Max(0f, hp - amount * reduction);
        if (hp <= 0f)
        {
            if (revives > 0)
            {
                revives--;
                hp = 60f;
                stamina = 70f;
                player.transform.position = new Vector3(0f, 1.04f, 30f);
                SnapCameraToPlayer();
                ShowMessage("复活晶石已消耗：你已复活。", 2.2f);
            }
            else
            {
                hp = 0f;
                if (deathPanel != null) deathPanel.SetActive(true);
                ShowMessage("你被 NPC 击倒了。按 Enter 重新开始。", 999f);
            }
        }
        else
        {
            PlayDamageSfx();
            ShowMessage("受到攻击，注意找掩体和医疗。");
        }

        UpdateHud();
    }

    public void FireCurrentWeapon()
    {
        if (inLobby)
        {
            return;
        }

        if (!weapons.TryGetValue(currentWeaponId, out var state) || !state.unlocked)
        {
            ShowMessage("当前武器不可用。");
            return;
        }

        if (state.reloading)
        {
            ShowMessage("正在换弹。");
            return;
        }

        if (Time.time - state.lastShotTime < state.spec.cooldown)
        {
            return;
        }

        if (state.magazine <= 0)
        {
            ShowMessage("弹匣为空，按 R 换弹。");
            return;
        }

        state.lastShotTime = Time.time;
        state.magazine--;
        NotifyFirstPersonWeaponShot(state.spec);
        PlayGunSfx(state.spec.id);

        var anyHit = false;
        var ray = BuildAimRay();
        var muzzle = GetVisualMuzzlePosition();
        SpawnMuzzleFlash(muzzle);
        var casingDirection = playerCamera != null && player != null && (player.CameraMode == TacticalCameraMode.FirstPerson || player.IsAds)
            ? playerCamera.transform.right + playerCamera.transform.up * 0.18f - playerCamera.transform.forward * 0.18f
            : player.transform.right + Vector3.up * 0.35f;
        SpawnCasing(muzzle - casingDirection.normalized * 0.18f, casingDirection);

        for (var i = 0; i < state.spec.pellets; i++)
        {
            var pelletRay = new Ray(ray.origin, ApplySpread(ray.direction, state.spec));
            if (TryCombatRaycast(pelletRay, state.spec, out var hit))
            {
                anyHit = true;
                var enemy = hit.collider.GetComponentInParent<TacticalEnemy>();
                var damage = DamageAtDistance(state.spec, hit.distance);
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    ShowMessage("命中 NPC。");
                    PlayHitSfx();
                    ShowHitMarker();
                    SpawnTracer(muzzle, hit.point, CurrentSkinTracer);
                    SpawnImpact(hit.point, new Color(1f, 0.42f, 0.48f));
                }
                else
                {
                    ShowMessage("击中掩体。");
                    SpawnTracer(muzzle, hit.point, CurrentSkinTracer);
                    SpawnImpact(hit.point, new Color(0.78f, 0.80f, 0.84f));
                }
            }
        }

        if (!anyHit)
        {
            ShowMessage("射击。");
        }

        UpdateHud();
    }

    private Vector3 GetVisualMuzzlePosition()
    {
        if (playerCamera != null && player != null && (player.CameraMode == TacticalCameraMode.FirstPerson || player.IsAds))
        {
            var cameraTransform = playerCamera.transform;
            return cameraTransform.position
                + cameraTransform.right * 0.48f
                - cameraTransform.up * 0.36f
                + cameraTransform.forward * 0.84f;
        }

        return player.transform.position + player.transform.forward * 0.82f + Vector3.up * 1.24f;
    }

    public void Reload()
    {
        var state = EnsureCurrentWeaponState();
        if (state == null)
        {
            return;
        }

        if (state.magazine >= state.spec.magazineSize)
        {
            ShowMessage("弹匣已满。");
            return;
        }

        if (state.reserve <= 0)
        {
            ShowMessage("没有备用弹药。");
            return;
        }

        state.reloading = true;
        state.reloadEndTime = Time.time + state.spec.reloadTime;
        NotifyFirstPersonWeaponReload(state.spec.reloadTime);
        PlayReloadSfx();
        ShowMessage("换弹中...");
        UpdateHud();
    }

    public void SelectWeapon(string weaponId)
    {
        if (!weapons.TryGetValue(weaponId, out var state) || !state.unlocked)
        {
            ShowMessage("还没有拾取这把武器。");
            return;
        }

        currentWeaponId = weaponId;
        NotifyFirstPersonWeaponSelected();
        ShowMessage("切换武器：" + state.spec.displayName);
        UpdateHud();
    }

    private TacticalFirstPersonWeaponVisual GetFirstPersonWeaponVisual()
    {
        if (firstPersonWeaponVisual == null)
        {
            firstPersonWeaponVisual = FindAnyObjectByType<TacticalFirstPersonWeaponVisual>();
        }

        return firstPersonWeaponVisual;
    }

    private void NotifyFirstPersonWeaponShot(TacticalWeaponSpec spec)
    {
        var visual = GetFirstPersonWeaponVisual();
        if (visual != null)
        {
            visual.NotifyShot(spec);
        }
    }

    private void NotifyFirstPersonWeaponReload(float reloadTime)
    {
        var visual = GetFirstPersonWeaponVisual();
        if (visual != null)
        {
            visual.NotifyReload(reloadTime);
        }
    }

    private void NotifyFirstPersonWeaponSelected()
    {
        var visual = GetFirstPersonWeaponVisual();
        if (visual != null)
        {
            visual.NotifyWeaponSelected();
        }
    }

    public void TryPickupNearest()
    {
        RefreshSceneRegistries();
        FindNearestLoot();

        if (nearestLoot == null && nearestLadder == null)
        {
            ShowMessage("靠近物资或爬梯后按 F。");
            return;
        }

        if (nearestLoot == null && nearestLadder != null)
        {
            currentFloor = nearestLadder.Use(player.transform);
            SnapCameraToPlayer();
            PlayPickupSfx();
            ShowMessage(currentFloor <= 1 ? "已下到地面。" : "已到达 " + currentFloor + " 层。");
            UpdateHud();
            return;
        }

        ApplyLoot(nearestLoot);
        PlayPickupSfx();
        loots.Remove(nearestLoot);
        Destroy(nearestLoot.gameObject);
        nearestLoot = null;
        UpdateHud();
    }

    public void UseHeal(TacticalLootKind kind)
    {
        if (kind == TacticalLootKind.Bandage)
        {
            if (bandages <= 0)
            {
                ShowMessage("没有绷带。");
                return;
            }

            bandages--;
            hp = Mathf.Min(75f, hp + 8f);
            ShowMessage("使用绷带。");
        }
        else if (kind == TacticalLootKind.FirstAid)
        {
            if (firstAid <= 0)
            {
                ShowMessage("没有急救包。");
                return;
            }

            firstAid--;
            hp = Mathf.Max(hp, 75f);
            ShowMessage("使用急救包。");
        }
        else if (kind == TacticalLootKind.Medkit)
        {
            if (medkits <= 0)
            {
                ShowMessage("没有全能医疗箱。");
                return;
            }

            medkits--;
            hp = 100f;
            ShowMessage("使用全能医疗箱。");
        }
        else if (kind == TacticalLootKind.Revive)
        {
            ShowMessage("复活晶石会在倒地时自动消耗。");
        }

        UpdateHud();
    }

    public void NotifyEnemyEliminated(TacticalEnemy enemy)
    {
        enemies.Remove(enemy);
        kills++;
        nextEnemyRespawn = Mathf.Min(nextEnemyRespawn, Time.time + 1.25f);
        if (kills >= nextCoinAt)
        {
            coins++;
            nextCoinAt += 10;
            ShowMessage("淘汰 NPC +1，获得抽奖币。");
        }
        else
        {
            ShowMessage("淘汰 NPC +1。");
        }
        UpdateHud();
    }

    public void StartRound()
    {
        inLobby = false;
        hp = 100f;
        stamina = 100f;
        kills = 0;
        vestLevel = 0;
        helmetLevel = 0;
        currentFloor = 1;
        bandages = 2;
        firstAid = 0;
        medkits = 0;
        revives = 0;
        nearestLoot = null;
        nearestLadder = null;
        hitMarkerUntil = 0f;
        messageUntil = 0f;
        currentWeaponId = "pistol";
        weapons.Clear();
        RegisterWeapons();
        RefreshSceneRegistries();
        if (player != null)
        {
            player.transform.position = new Vector3(0f, 1.04f, 30f);
            player.ResetView(180f, 24f);
            player.SetCameraMode(TacticalCameraMode.FirstPerson);
            SnapCameraToPlayer();
        }
        roundStartTime = Time.time;
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (deathPanel != null) deathPanel.SetActive(false);
        if (helpPanel != null) helpPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (skinsPanel != null) skinsPanel.SetActive(false);
        if (promptText != null) promptText.text = "";
        if (hitMarkerText != null) hitMarkerText.text = "";
        ShowMessage("进入房区生存：第一人称已开启，左键射击，R 换弹，V 切第三人称。", 4.5f);
        nextEnemyRespawn = Time.time + enemyRespawnInterval / Mathf.Max(0.3f, SpawnRateMultiplier);
        UpdateHud();
    }

    public void SpawnTracer(Vector3 start, Vector3 end, Color color)
    {
        var obj = new GameObject("Tracer");
        var line = obj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = 0.005f;
        line.endWidth = 0.0015f;
        var tracerColor = new Color(color.r, color.g, color.b, 0.42f);
        line.material = new Material(Shader.Find("Sprites/Default")) { color = tracerColor };
        Destroy(obj, 0.04f);
    }

    public void PlayEnemyShotSfx()
    {
        PlaySfx(enemyShotSfx, 0.52f);
    }

    public void TryPlayFootstepSfx(float speed01)
    {
        if (speed01 <= 0.04f || IsInLobby || IsPlayerDown)
        {
            return;
        }

        PlaySfx(footstepSfx, Mathf.Lerp(0.18f, 0.34f, Mathf.Clamp01(speed01)));
    }

    private void RegisterWeapons()
    {
        AddWeapon(new TacticalWeaponSpec("pistol", "P-9 手枪", 1, 15, 45, 0.24f, 0.95f, 33f, 65f, false, true, new Color(0.2f, 0.25f, 0.3f), 8f, 28f, 1, 0.030f, 0.009f, 0.020f));
        AddWeapon(new TacticalWeaponSpec("shotgun", "BREACH-12", 2, 5, 20, 0.74f, 1.35f, 17f, 42f, false, false, new Color(0.45f, 0.25f, 0.12f), 3f, 14f, 9, 0.105f, 0.055f, 0.044f));
        AddWeapon(new TacticalWeaponSpec("rifle", "TAC-AR", 3, 30, 90, 0.095f, 1.55f, 24f, 155f, true, false, new Color(0.15f, 0.18f, 0.22f), 12f, 82f, 1, 0.020f, 0.0055f, 0.014f));
        AddWeapon(new TacticalWeaponSpec("dmr", "XMR-7", 4, 12, 48, 0.34f, 1.75f, 50f, 235f, false, false, new Color(0.12f, 0.16f, 0.20f), 24f, 130f, 1, 0.026f, 0.003f, 0.031f));
    }

    private TacticalWeaponState EnsureCurrentWeaponState()
    {
        if (weapons.Count == 0)
        {
            RegisterWeapons();
        }

        if (weapons.TryGetValue(currentWeaponId, out var state))
        {
            return state;
        }

        currentWeaponId = "pistol";
        if (!weapons.TryGetValue(currentWeaponId, out state))
        {
            RegisterWeapons();
            weapons.TryGetValue(currentWeaponId, out state);
        }

        return state;
    }

    private void SnapCameraToPlayer()
    {
        var follow = playerCamera == null ? null : playerCamera.GetComponent<TacticalCameraFollow>();
        if (follow != null)
        {
            follow.SnapToPlayer();
        }
    }

    private void AddWeapon(TacticalWeaponSpec spec)
    {
        weapons[spec.id] = new TacticalWeaponState(spec);
    }

    private Ray BuildAimRay()
    {
        if (playerCamera != null)
        {
            return new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        }

        return new Ray(player.transform.position + Vector3.up * 1.35f, player.transform.forward);
    }

    private bool TryCombatRaycast(Ray ray, TacticalWeaponSpec spec, out RaycastHit hit)
    {
        hit = default;
        var radius = player != null && player.IsAds ? 0.10f : 0.22f;
        var hits = Physics.SphereCastAll(ray, radius, spec.range, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var candidate in hits)
        {
            if (player != null && candidate.collider.transform.IsChildOf(player.transform))
            {
                continue;
            }

            hit = candidate;
            return true;
        }

        return false;
    }

    private Vector3 ApplySpread(Vector3 direction, TacticalWeaponSpec spec)
    {
        var spread = player != null && player.IsAds ? spec.adsSpread : spec.hipSpread;
        return Quaternion.Euler(Random.Range(-spread, spread) * Mathf.Rad2Deg, Random.Range(-spread, spread) * Mathf.Rad2Deg, 0f) * direction;
    }

    private static float DamageAtDistance(TacticalWeaponSpec spec, float distance)
    {
        if (distance <= spec.effectiveRange)
        {
            return spec.damage;
        }

        var t = Mathf.Clamp01((distance - spec.effectiveRange) / Mathf.Max(1f, spec.range - spec.effectiveRange));
        return Mathf.Lerp(spec.damage, spec.minDamage, spec.id == "shotgun" ? Mathf.Pow(t, 0.55f) : Mathf.Pow(t, 0.9f));
    }

    private void SpawnMuzzleFlash(Vector3 position)
    {
        var flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "Muzzle Flash";
        flash.transform.position = position;
        flash.transform.localScale = Vector3.one * 0.08f;
        flash.GetComponent<Renderer>().material.color = new Color(1f, 0.66f, 0.20f, 0.82f);
        Destroy(flash, 0.04f);
    }

    private void SpawnImpact(Vector3 position, Color color)
    {
        var impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        impact.name = "Impact Spark";
        impact.transform.position = position;
        impact.transform.localScale = Vector3.one * 0.14f;
        impact.GetComponent<Renderer>().material.color = color;
        Destroy(impact, 0.18f);
    }

    private void SpawnCasing(Vector3 position, Vector3 direction)
    {
        var casing = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        casing.name = "Ejected Casing";
        casing.transform.position = position;
        casing.transform.localScale = new Vector3(0.028f, 0.070f, 0.028f);
        casing.GetComponent<Renderer>().material.color = new Color(0.70f, 0.55f, 0.25f);
        var body = casing.AddComponent<Rigidbody>();
        body.mass = 0.025f;
        body.linearVelocity = direction.normalized * 1.4f;
        body.angularVelocity = Random.insideUnitSphere * 14f;
        Destroy(casing, 0.85f);
    }

    private void EnsureSfx()
    {
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = 0.75f;
        }

        pistolSfx ??= CreateNoiseClip("P-9 procedural shot", 0.12f, 0.18f, 700f, true);
        shotgunSfx ??= CreateNoiseClip("BREACH-12 procedural shot", 0.23f, 0.34f, 420f, true);
        rifleSfx ??= CreateNoiseClip("TAC-AR procedural shot", 0.13f, 0.24f, 460f, true);
        dmrSfx ??= CreateNoiseClip("XMR-7 procedural shot", 0.16f, 0.30f, 390f, true);
        enemyShotSfx ??= CreateNoiseClip("NPC procedural shot", 0.10f, 0.10f, 520f, true);
        hitSfx ??= CreateNoiseClip("Hit procedural tick", 0.05f, 0.10f, 1200f, false);
        pickupSfx ??= CreateNoiseClip("Pickup procedural tick", 0.06f, 0.08f, 1500f, false);
        reloadSfx ??= CreateNoiseClip("Reload procedural rattle", 0.18f, 0.13f, 900f, true);
        footstepSfx ??= CreateNoiseClip("Footstep procedural thump", 0.055f, 0.09f, 180f, true);
        damageSfx ??= CreateNoiseClip("Damage procedural thump", 0.14f, 0.16f, 260f, true);
    }

    private void PlayGunSfx(string weaponId)
    {
        var clip = weaponId switch
        {
            "shotgun" => shotgunSfx,
            "rifle" => rifleSfx,
            "dmr" => dmrSfx,
            _ => pistolSfx
        };
        PlaySfx(clip, 1f);
    }

    private void PlayHitSfx()
    {
        PlaySfx(hitSfx, 0.9f);
    }

    private void PlayPickupSfx()
    {
        PlaySfx(pickupSfx, 0.9f);
    }

    private void PlayReloadSfx()
    {
        PlaySfx(reloadSfx, 0.9f);
    }

    private void PlayDamageSfx()
    {
        PlaySfx(damageSfx, 0.9f);
    }

    private void PlaySfx(AudioClip clip, float volumeScale)
    {
        EnsureSfx();
        sfxEventCount++;
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    private int CountSfxClips()
    {
        var count = 0;
        if (pistolSfx != null) count++;
        if (shotgunSfx != null) count++;
        if (rifleSfx != null) count++;
        if (dmrSfx != null) count++;
        if (enemyShotSfx != null) count++;
        if (hitSfx != null) count++;
        if (pickupSfx != null) count++;
        if (reloadSfx != null) count++;
        if (footstepSfx != null) count++;
        if (damageSfx != null) count++;
        return count;
    }

    private static AudioClip CreateNoiseClip(string clipName, float duration, float volume, float frequency, bool lowTone)
    {
        const int sampleRate = 24000;
        var samples = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
        var data = new float[samples];

        for (var i = 0; i < samples; i++)
        {
            var t = i / (float)sampleRate;
            var envelope = Mathf.Pow(1f - i / (float)samples, lowTone ? 1.25f : 2.15f);
            var hash = Mathf.Sin(i * 12.9898f + frequency * 78.233f) * 43758.5453f;
            var noise = (hash - Mathf.Floor(hash)) * 2f - 1f;
            var tone = Mathf.Sin(2f * Mathf.PI * frequency * t);
            data[i] = (lowTone ? tone * 0.55f + noise * 0.45f : noise) * envelope * volume;
        }

        var clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private void ShowHitMarker()
    {
        if (hitMarkerText == null)
        {
            return;
        }

        hitMarkerText.text = "X";
        hitMarkerUntil = Time.time + 0.16f;
    }

    private void CompleteReloads()
    {
        foreach (var pair in weapons)
        {
            var state = pair.Value;
            if (!state.reloading || Time.time < state.reloadEndTime)
            {
                continue;
            }

            var need = state.spec.magazineSize - state.magazine;
            var take = Mathf.Min(need, state.reserve);
            state.magazine += take;
            state.reserve -= take;
            state.reloading = false;
            if (pair.Key == currentWeaponId)
            {
                ShowMessage("换弹完成。");
            }
        }
    }

    private void CleanupEnemyList()
    {
        for (var i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] == null)
            {
                enemies.RemoveAt(i);
            }
        }
    }

    private void RefreshSceneRegistries()
    {
        CleanupEnemyList();

        for (var i = loots.Count - 1; i >= 0; i--)
        {
            if (loots[i] == null)
            {
                loots.RemoveAt(i);
            }
        }

        for (var i = ladders.Count - 1; i >= 0; i--)
        {
            if (ladders[i] == null)
            {
                ladders.RemoveAt(i);
            }
        }

        if (player != null)
        {
            foreach (var enemy in FindObjectsByType<TacticalEnemy>(FindObjectsInactive.Exclude))
            {
                RegisterEnemy(enemy);
            }
        }

        foreach (var loot in FindObjectsByType<TacticalLoot>(FindObjectsInactive.Exclude))
        {
            if (!loots.Contains(loot))
            {
                loots.Add(loot);
            }
        }

        foreach (var ladder in FindObjectsByType<TacticalLadder>(FindObjectsInactive.Exclude))
        {
            if (!ladders.Contains(ladder))
            {
                ladders.Add(ladder);
            }
        }
    }

    private void MaybeSpawnEnemy()
    {
        if (player == null || enemies.Count >= maxActiveEnemies || Time.time < nextEnemyRespawn)
        {
            return;
        }

        var spawn = enemySpawnPoints[runtimeEnemyCounter % enemySpawnPoints.Length];
        runtimeEnemyCounter++;
        if (Vector3.Distance(player.transform.position, spawn) < 18f)
        {
            spawn = enemySpawnPoints[runtimeEnemyCounter % enemySpawnPoints.Length];
            runtimeEnemyCounter++;
        }

        var weaponId = runtimeEnemyCounter % 3 == 0 ? "rifle" : "pistol";
        var enemy = enemyReinforcementTemplate == null
            ? GameObject.CreatePrimitive(PrimitiveType.Capsule)
            : Instantiate(enemyReinforcementTemplate);
        enemy.name = "Runtime NPC Reinforcement " + runtimeEnemyCounter;
        enemy.transform.position = spawn;
        enemy.transform.localScale = Vector3.one;

        if (enemyReinforcementTemplate != null)
        {
            enemy.SetActive(true);
            foreach (var visual in enemy.GetComponentsInChildren<TacticalThirdPersonWeaponVisual>(true))
            {
                if (!visual.FollowCurrentWeapon)
                {
                    visual.ConfigureFixedWeapon(weaponId);
                    visual.ForceRefresh();
                    break;
                }
            }
        }
        else
        {
            var renderer = enemy.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.78f, 0.22f, 0.26f);
                renderer.sharedMaterial = mat;
            }

            var capsule = enemy.GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                Destroy(capsule);
            }

            var controller = enemy.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.center = new Vector3(0f, 1f, 0f);
            enemy.AddComponent<TacticalEnemy>();
        }

        RegisterEnemy(enemy.GetComponent<TacticalEnemy>());
        if (enemyReinforcementTemplate == null)
        {
            var mount = new GameObject("HTML GLB Mount - character_enemy_final");
            mount.transform.SetParent(enemy.transform, false);
            var loader = mount.AddComponent<HtmlGlbAssetMount>();
            loader.Configure("models/character_enemy_final.glb", new Vector3(0f, -0.015f, -0.02f), new Vector3(0f, 180f, 0f), Vector3.one * 0.82f, true);
            CreateRuntimeNpcWeapon(enemy, weaponId);
        }

        nextEnemyRespawn = Time.time + enemyRespawnInterval / Mathf.Max(0.3f, SpawnRateMultiplier);
        ShowMessage("新的 NPC 巡逻队进入房区。");
        UpdateHud();
    }

    private static void CreateRuntimeNpcWeapon(GameObject enemy, string weaponId)
    {
        var weaponRoot = new GameObject("NPC Weapon Visual");
        weaponRoot.transform.SetParent(enemy.transform, false);
        weaponRoot.transform.localPosition = new Vector3(0.18f, 1.14f, 0.62f);
        weaponRoot.transform.localEulerAngles = new Vector3(-7f, 0f, 0f);
        var visual = weaponRoot.AddComponent<TacticalThirdPersonWeaponVisual>();
        visual.ConfigureFixedWeapon(weaponId);

        var weapon = new GameObject("Character Weapon - " + weaponId);
        weapon.transform.SetParent(weaponRoot.transform, false);
        var loader = weapon.AddComponent<HtmlGlbAssetMount>();
        loader.Configure(RuntimeWeaponGlbPath(weaponId), Vector3.zero, new Vector3(0f, 180f, 0f), Vector3.one * (weaponId == "pistol" ? 0.11f : 0.12f), false);
    }

    private static string RuntimeWeaponGlbPath(string weaponId)
    {
        return weaponId switch
        {
            "shotgun" => "models/shotgun_m5_candidate.glb",
            "rifle" => "models/groza_procedural_candidate.glb",
            "dmr" => "models/dmr_m5_candidate.glb",
            _ => "models/pistol_m5_candidate.glb"
        };
    }

    private void FindNearestLoot()
    {
        nearestLoot = null;
        nearestLadder = null;
        var bestDistance = 2.4f;
        var bestLadderDistance = 2.8f;
        var playerPosition = player.transform.position;

        foreach (var loot in loots)
        {
            if (loot == null)
            {
                continue;
            }

            var distance = Vector3.Distance(playerPosition, loot.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearestLoot = loot;
            }
        }

        foreach (var ladder in ladders)
        {
            if (ladder == null)
            {
                continue;
            }

            var flatPlayer = new Vector3(playerPosition.x, 0f, playerPosition.z);
            var flatLadder = new Vector3(ladder.transform.position.x, 0f, ladder.transform.position.z);
            var distance = Vector3.Distance(flatPlayer, flatLadder);
            if (distance < bestLadderDistance)
            {
                bestLadderDistance = distance;
                nearestLadder = ladder;
            }
        }

        if (promptText != null)
        {
            if (nearestLoot != null)
            {
                promptText.text = "按 F 拾取：" + nearestLoot.DisplayName;
            }
            else if (nearestLadder != null)
            {
                promptText.text = nearestLadder.Prompt;
            }
            else
            {
                promptText.text = "";
            }
        }
    }

    private void ApplyLoot(TacticalLoot loot)
    {
        switch (loot.Kind)
        {
            case TacticalLootKind.Ammo:
                foreach (var pair in weapons)
                {
                    if (pair.Value.unlocked)
                    {
                        pair.Value.reserve += ScaledLootAmount(pair.Key == "dmr" ? 12 : pair.Key == "shotgun" ? 8 : 24);
                    }
                }
                ShowMessage("拾取通用弹药。");
                break;
            case TacticalLootKind.Bandage:
                bandages += ScaledLootAmount(2);
                ShowMessage("拾取绷带 x2。");
                break;
            case TacticalLootKind.FirstAid:
                firstAid += ScaledLootAmount(1);
                ShowMessage("拾取急救包。");
                break;
            case TacticalLootKind.Medkit:
                medkits += ScaledLootAmount(1);
                ShowMessage("拾取全能医疗箱。");
                break;
            case TacticalLootKind.Revive:
                revives += ScaledLootAmount(1);
                ShowMessage("拾取复活晶石。");
                break;
            case TacticalLootKind.Vest:
                vestLevel = Mathf.Max(vestLevel, loot.Level);
                ShowMessage("装备防弹衣 " + vestLevel + " 级。");
                break;
            case TacticalLootKind.Helmet:
                helmetLevel = Mathf.Max(helmetLevel, loot.Level);
                ShowMessage("装备头盔 " + helmetLevel + " 级。");
                break;
            case TacticalLootKind.Weapon:
                UnlockWeapon(loot.WeaponId);
                break;
        }
    }

    private void UnlockWeapon(string weaponId)
    {
        if (!weapons.TryGetValue(weaponId, out var state))
        {
            ShowMessage("未知武器。");
            return;
        }

        if (!state.unlocked)
        {
            state.unlocked = true;
            state.magazine = state.spec.magazineSize;
            state.reserve = state.spec.reserveStart;
        }
        else
        {
            state.reserve += Mathf.Max(12, state.spec.magazineSize);
        }

        currentWeaponId = weaponId;
        ShowMessage("获得武器：" + state.spec.displayName);
    }

    private int ScaledLootAmount(int baseAmount)
    {
        return Mathf.Max(1, Mathf.RoundToInt(baseAmount * LootRichnessMultiplier));
    }

    private static Color SkinPrimaryColor(string skinName)
    {
        return skinName switch
        {
            "沙漠迷彩" => HexColor(0x8b7355),
            "深海蓝焰" => HexColor(0x1d4ed8),
            "霓虹电音" => HexColor(0xff4fd8),
            "赤焰龙纹" => HexColor(0xdc2626),
            "极光幻影" => HexColor(0x8b5cf6),
            _ => HexColor(0x303946)
        };
    }

    private static Color SkinAccentColor(string skinName)
    {
        return skinName switch
        {
            "沙漠迷彩" => HexColor(0x2d251a),
            "深海蓝焰" => HexColor(0x0f172a),
            "霓虹电音" => HexColor(0x22d3ee),
            "赤焰龙纹" => HexColor(0xfacc15),
            "极光幻影" => HexColor(0x86efac),
            _ => HexColor(0x111820)
        };
    }

    private static Color SkinTracerColor(string skinName)
    {
        return skinName switch
        {
            "深海蓝焰" => HexColor(0x7dd3fc),
            "霓虹电音" => HexColor(0xc084fc),
            "赤焰龙纹" => HexColor(0xfb7185),
            "极光幻影" => HexColor(0x86efac),
            _ => HexColor(0xfacc15)
        };
    }

    private static Color HexColor(int hex)
    {
        return new Color(((hex >> 16) & 0xff) / 255f, ((hex >> 8) & 0xff) / 255f, (hex & 0xff) / 255f, 1f);
    }

    private void UpdateHud()
    {
        var state = EnsureCurrentWeaponState();
        if (state == null)
        {
            return;
        }

        if (hpText != null) hpText.text = "生命值: " + Mathf.CeilToInt(hp);
        if (staminaText != null) staminaText.text = "体力: " + Mathf.CeilToInt(stamina);
        var cameraLabel = player == null ? "未知" : player.CameraMode == TacticalCameraMode.FirstPerson ? "第一人称" : "第三人称";
        if (weaponText != null) weaponText.text = "当前武器: " + state.spec.displayName + "  视角: " + cameraLabel;
        if (ammoText != null) ammoText.text = "弹药: " + state.magazine + " / " + state.reserve + (state.reloading ? " 换弹中" : "");
        if (armorText != null) armorText.text = "防弹衣: " + ArmorName(vestLevel) + "  头盔: " + ArmorName(helmetLevel);
        if (inventoryText != null) inventoryText.text = $"背包: 绷带 {bandages}  急救 {firstAid}  全能 {medkits}  复活 {revives}";
        if (npcText != null) npcText.text = $"NPC: {enemies.Count}  淘汰: {kills}  币: {coins}  楼层: {(currentFloor <= 1 ? "地面" : currentFloor + "层")}  操作: 左键射击 R换弹 V切视角 F拾取";
        if (coinText != null) coinText.text = "抽奖币: " + coins;
        if (skinText != null) skinText.text = "枪械皮肤: " + currentSkinName;
        if (skinCoinText != null) skinCoinText.text = "抽奖币: " + coins;
        if (shardText != null) shardText.text = "碎片: " + skinShards;
        if (npcStrengthValueText != null) npcStrengthValueText.text = EnemyStrengthMultiplier.ToString("0.00");
        if (spawnRateValueText != null) spawnRateValueText.text = SpawnRateMultiplier.ToString("0.00");
        if (lootRichnessValueText != null) lootRichnessValueText.text = LootRichnessMultiplier.ToString("0.00");
        if (crosshairText != null) crosshairText.text = player != null && player.IsAds ? "+" : ".";
    }

    private static string ArmorName(int level)
    {
        return level <= 0 ? "无" : level + "级";
    }

    private void ShowMessage(string message, float duration = 1.8f)
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = message;
        messageUntil = Time.time + duration;
    }
}
