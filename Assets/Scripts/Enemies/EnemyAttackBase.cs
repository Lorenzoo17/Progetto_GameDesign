using UnityEngine;

public abstract class EnemyAttackBase : MonoBehaviour {
    [SerializeField] protected float attackDistance = 1f;
    [SerializeField] protected float attackCooldown = 1f;

    [Header("Attack Timing")]
    [SerializeField] protected float attackDuration = 0.4f;
    [SerializeField] protected float attackHitDelay = 0.15f;

    protected float attackCooldownTimer;
    protected float attackTimer;

    protected EnemyVisual visual;
    protected Rigidbody2D rb;

    private bool hasExecutedAttack;

    public float AttackDistance => attackDistance;
    public bool CanAttack => attackCooldownTimer <= 0f;
    public bool IsAttacking { get; protected set; }

    protected virtual void Awake() {
        visual = GetComponent<EnemyVisual>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update() {
        if (attackCooldownTimer > 0f) {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (!IsAttacking) return;

        attackTimer -= Time.deltaTime;

        if (!hasExecutedAttack && attackTimer <= attackDuration - attackHitDelay) {
            ExecuteAttack();
            hasExecutedAttack = true;
        }

        if (attackTimer <= 0f) {
            EndAttack();
        }
    }

    public void TryAttack() {
        if (!CanAttack || IsAttacking) return;

        attackCooldownTimer = attackCooldown;
        attackTimer = attackDuration;
        hasExecutedAttack = false;
        IsAttacking = true;

        if (rb != null) {
            rb.linearVelocity = Vector2.zero;
        }

        if (visual != null) {
            visual.PlayAttack();
        }
    }

    protected abstract void ExecuteAttack();

    public virtual void EndAttack() {
        IsAttacking = false;
    }
}
