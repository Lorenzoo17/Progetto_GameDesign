using FirstGearGames.SmoothCameraShaker;
using UnityEngine;

public class Enemy : MonoBehaviour {
    [SerializeField] private float knockBackForce;
    [SerializeField] private float knockbackDuration = 0.2f;    

    [SerializeField] private float enemyTouchDamage = 1f;
    [SerializeField] private GameObject deadBodyPlaceholder;
    [SerializeField] private float itemDropChance = 0.4f;

    private HealthSystem enemyHealthSystem;
    public EnemyStatus enemyStatus;
    
    private EnemySpawner enemySpawner;
    
    private bool isDead = false;

    private EnemyVisual visual;

    private void Awake() {
        visual = GetComponent<EnemyVisual>();
        enemyHealthSystem = GetComponent<HealthSystem>();
        enemyStatus = GetComponent<EnemyStatus>();
        
        enemyHealthSystem.OnDamageTaken += EnemyHealthSystem_OnDamageTaken;
    }

    private void EnemyHealthSystem_OnDamageTaken(object sender, DamageEventArgs e) {
        if (isDead) return;

        if (TryGetComponent<BossCtrl>(out BossCtrl boss)) {
            boss.ApplyKnockback(e.AttackDirection);
        }
        else if (TryGetComponent<EnemyMovementNav>(out EnemyMovementNav nav)) {
            nav.ApplyKnockback(
                e.AttackDirection,
                knockBackForce + e.KnockBackStrenght,
                knockbackDuration
            );
        }

        visual?.PlayHitFeedback(e.AttackDirection);

        if (enemyHealthSystem.CurrentHealth <= 0) {
            Vector2 direction = e.AttackDirection;
            if (e.AttackDamageType != DamageType.Physical && Player.Instance != null) {
                direction = -(Player.Instance.transform.position - transform.position).normalized;
            }
            DeadManagement(direction);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.TryGetComponent<Player>(out Player player)) {
            Vector2 attackDirection = (other.transform.position - transform.position).normalized;
            player.gameObject.GetComponent<IDamageable>().TakeDamage(
                new DamageInfo(enemyTouchDamage, attackDirection, this.gameObject, EntityType.Enemy)
            );
        }
    }
    private void DeadManagement(Vector2 attackDirection) {
        if (isDead) return;
        isDead = true;

        if (deadBodyPlaceholder != null) {
            GameObject deadBody = Instantiate(deadBodyPlaceholder, transform.position, Quaternion.identity);

            if (deadBody.TryGetComponent<DeadBodyBehaviour>(out DeadBodyBehaviour db)) {
                db.SetUpDeadBody(attackDirection, visual.CurrentSprite, visual.SortingLayerName, visual.SortingOrder);
            }
            else {
                Destroy(deadBody);
            }
        }

        if (enemySpawner != null) {
            enemySpawner.OnEnemyDeath();
        }

        if (SpawnItems.Instance != null) {
            SpawnItems.Instance.SpawnItem(transform.position, gameObject);
        }
        else {
            Debug.Log("Spawn non presente");
        }

        if (SoundManager.Instance != null) {
            SoundManager.Instance.PlaySound3D(SoundID.EnemyDeath, transform.position);
        }

        Destroy(gameObject);
    }

    public void SetEnemySpawner(EnemySpawner es) {
        enemySpawner = es;
    }
}