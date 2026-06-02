using FirstGearGames.SmoothCameraShaker;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float knockBackForce;
    [SerializeField] private float knockbackDuration = 0.2f;

    [SerializeField] private Color blinkAfterDamageTargetColor;
    [SerializeField] private float blinkAfterDamageTime;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float hitEffectSpawnPositionOffset = 0.5f;
    [SerializeField] private float hitEffectRotationOffset = -90f;
    [SerializeField] private bool invertFlipDirection;

    [SerializeField] private float enemyTouchDamage = 1f;
    [SerializeField] private GameObject deadBodyPlaceholder;

    private HealthSystem enemyHealthSystem;
    private EnemyMovement enemyMovement;

    private SpriteRenderer sr;
    private Color initialColor;

    private Animator anim;

    private EnemySpawner enemySpawner;

    // 🔥 STUN STATE
    private bool isStun = false;
    public bool IsStunned => isStun;

    private void Awake()
    {
        enemyHealthSystem = GetComponent<HealthSystem>();
        enemyMovement = GetComponent<EnemyMovement>();

        sr = GetComponent<SpriteRenderer>();
        initialColor = sr.color;

        anim = GetComponent<Animator>();

        enemyHealthSystem.OnDamageTaken += EnemyHealthSystem_OnDamageTaken;
    }

    private void Update()
    {
        if (sr != null && sr.color != initialColor)
        {
            sr.color = Color.Lerp(sr.color, initialColor, blinkAfterDamageTime * Time.deltaTime);
        }

        FlipBasedOnPlayer();
    }

    private void EnemyHealthSystem_OnDamageTaken(object sender, DamageEventArgs e)
    {
        if (enemyMovement != null)
        {
            enemyMovement.ApplyKnockback(
                e.AttackDirection,
                knockBackForce,
                knockbackDuration
            );
        }
        else if (TryGetComponent<EnemyMovementNav>(out EnemyMovementNav nav))
        {
            nav.ApplyKnockback(
                e.AttackDirection,
                knockBackForce,
                knockbackDuration
            );
        }

        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        if (sr != null)
        {
            sr.color = blinkAfterDamageTargetColor * 3f;
        }

        if (hitEffect != null)
        {
            Vector2 spawnPos = (Vector2)transform.position + e.AttackDirection.normalized * hitEffectSpawnPositionOffset;
            float angle = Mathf.Atan2(e.AttackDirection.y, e.AttackDirection.x) * Mathf.Rad2Deg;

            GameObject effect = Instantiate(hitEffect, spawnPos, Quaternion.identity);
            effect.transform.rotation = Quaternion.Euler(0f, 0f, angle + hitEffectRotationOffset);
        }

        if (enemyHealthSystem.CurrentHealth <= 0)
        {
            DeadManagement(e.AttackDirection);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out Player player))
        {
            Vector2 attackDirection = (other.transform.position - transform.position).normalized;
            player.gameObject.GetComponent<IDamageable>().TakeDamage(
                new DamageInfo(enemyTouchDamage, attackDirection, this.gameObject, EntityType.Enemy)
            );
        }
    }

    private void FlipBasedOnPlayer()
    {
        if (Player.Instance == null) return;

        Vector3 scale = transform.localScale;
        int flipDirection = invertFlipDirection ? -1 : 1;

        if (Player.Instance.transform.position.x > transform.position.x)
        {
            scale.x = -Mathf.Abs(scale.x) * flipDirection;
        }
        else
        {
            scale.x = Mathf.Abs(scale.x) * flipDirection;
        }

        transform.localScale = scale;
    }

    private void DeadManagement(Vector2 attackDirection)
    {
        if (deadBodyPlaceholder != null)
        {
            GameObject deadBody = Instantiate(deadBodyPlaceholder, transform.position, Quaternion.identity);

            if (deadBody.TryGetComponent<DeadBodyBehaviour>(out DeadBodyBehaviour db))
            {
                db.SetUpDeadBody(attackDirection, sr.sprite, sr.sortingLayerName, sr.sortingOrder);
            }
            else
            {
                Destroy(deadBody);
            }
        }

        if(enemySpawner != null) {
            enemySpawner.OnEnemyDeath(); // aggiorno contatore numero corrente di nemici per l'enemy spawner
        }

        // spawno un oggetto
        if(SpawnItems.Instance != null) {
            SpawnItems.Instance.SpawnItem(transform.position);
        }
        else {
            Debug.Log("Spawn non presente");
        }

        // suono morte
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound3D(SoundID.EnemyDeath, transform.position);
        }

            Destroy(gameObject);
    }

    public void SetEnemySpawner(EnemySpawner es) {
        enemySpawner = es;
    }

    // ======================================================
    // 🔥 STUN SYSTEM FIXATO
    // ======================================================

    public void SetStun(bool value)
    {
        isStun = value;

        if (isStun)
        {
            if (enemyMovement != null)
            {
                enemyMovement.ForceStop(); // 🔥 aggiunta necessaria
            }
        }
    }
}