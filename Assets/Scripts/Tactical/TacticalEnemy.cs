using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TacticalEnemy : MonoBehaviour
{
    [SerializeField] private float maxHealth = 90f;
    [SerializeField] private float moveSpeed = 2.3f;
    [SerializeField] private float sightRange = 32f;
    [SerializeField] private float attackRange = 2.1f;
    [SerializeField] private float shootRange = 21f;
    [SerializeField] private float attackDamage = 4.5f;
    [SerializeField] private float attackInterval = 1.2f;
    [SerializeField] private float shootInterval = 2.7f;

    private CharacterController controller;
    private TacticalGameManager game;
    private Transform player;
    private Vector3 home;
    private float health;
    private float nextAttackTime;
    private float nextShotTime;
    private float patrolPhase;

    public bool IsAlive => health > 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = maxHealth;
        home = transform.position;
        patrolPhase = Random.value * Mathf.PI * 2f;
    }

    public void Initialize(TacticalGameManager gameManager, Transform playerTransform)
    {
        game = gameManager;
        player = playerTransform;
        home = transform.position;
        health = maxHealth;
    }

    private void Update()
    {
        if (!IsAlive || player == null || game == null || game.IsPlayerDown || game.IsInLobby)
        {
            return;
        }

        var toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        var distance = toPlayer.magnitude;

        if (distance < sightRange)
        {
            var direction = toPlayer.normalized;
            if (distance > attackRange + 1.3f)
            {
                MoveTowards(direction, moveSpeed);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);
            }

            if (distance < shootRange && HasClearShotToPlayer() && Time.time >= nextShotTime)
            {
                nextShotTime = Time.time + shootInterval + Random.Range(-0.25f, 0.45f);
                game.DamagePlayer(attackDamage * game.EnemyStrengthMultiplier * (distance < 9f ? 1.25f : 1f));
                game.SpawnTracer(transform.position + Vector3.up * 1.35f, player.position + Vector3.up * 1.2f, new Color(1f, 0.36f, 0.42f));
                game.PlayEnemyShotSfx();
            }

            if (distance < attackRange && Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackInterval;
                game.DamagePlayer(attackDamage * game.EnemyStrengthMultiplier);
            }
        }
        else
        {
            var patrol = new Vector3(Mathf.Sin(Time.time + patrolPhase), 0f, Mathf.Cos(Time.time * 0.7f + patrolPhase));
            var returnHome = home - transform.position;
            returnHome.y = 0f;
            MoveTowards((patrol * 0.45f + returnHome.normalized * 0.55f).normalized, moveSpeed * 0.45f);
        }
    }

    private bool HasClearShotToPlayer()
    {
        if (player == null)
        {
            return false;
        }

        var start = transform.position + Vector3.up * 1.35f;
        var target = player.position + Vector3.up * 1.2f;
        var toTarget = target - start;
        var distance = toTarget.magnitude;
        if (distance < 0.1f)
        {
            return true;
        }

        var hits = Physics.RaycastAll(start, toTarget.normalized, distance, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            var hitPlayer = hit.collider.GetComponentInParent<TacticalPlayerController>();
            if (hitPlayer != null && hitPlayer.transform == player)
            {
                return true;
            }

            return false;
        }

        return true;
    }

    private void MoveTowards(Vector3 direction, float speed)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);
        controller.SimpleMove(direction * speed);
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive)
        {
            return;
        }

        health -= amount;
        var motionVisual = GetComponent<TacticalCharacterMotionVisual>();
        if (motionVisual != null)
        {
            motionVisual.PulseHit();
        }
        transform.localScale = Vector3.one * 1.08f;
        Invoke(nameof(ResetScale), 0.08f);

        if (health <= 0f)
        {
            game.NotifyEnemyEliminated(this);
            Destroy(gameObject);
        }
    }

    private void ResetScale()
    {
        if (this != null)
        {
            transform.localScale = Vector3.one;
        }
    }
}
