using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 2f;

    [Header("Random Follow Offset")]
    [SerializeField] private float randomOffsetDistance = 1f;
    [SerializeField] private float changeRandomOffsetTime = 3f;

    private Rigidbody2D rb;
    private Animator anim;
    private EnemyAttackBase enemyAttack;
    private Enemy enemy; // 🔥 FIX MANCANTE

    private Vector2 desiredPosition;
    private Vector2 randomFollowOffset;
    private float changeRandomOffsetCurrentTime;

    private bool isKnockedBack;
    private float knockbackTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyAttack = GetComponent<EnemyAttackBase>();
        enemy = GetComponent<Enemy>(); // 🔥 FIX IMPORTANTE
    }

    private void Update()
    {
        // 🔥 STUN CHECK CORRETTO
        if (enemy != null && enemy.IsStunned)
        {
            rb.linearVelocity = Vector2.zero;
            SetMoving(false);
            return;
        }

        if (rb == null || Player.Instance == null) return;

        if (isKnockedBack)
        {
            HandleKnockback();
            return;
        }

        if (enemyAttack != null && enemyAttack.IsAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            SetMoving(false);
            return;
        }

        FollowTarget();
        SetMoving(rb.linearVelocity != Vector2.zero);
    }

    public void ForceStop()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        SetMoving(false);

        isKnockedBack = false;
        knockbackTimer = 0f;
    }

    private void FollowTarget()
    {
        UpdateRandomOffset();

        desiredPosition = (Vector2)Player.Instance.transform.position + randomFollowOffset;
        Vector2 direction = (desiredPosition - (Vector2)transform.position).normalized;

        float distance = Vector2.Distance(Player.Instance.transform.position, transform.position);

        if (enemyAttack != null && distance <= enemyAttack.AttackDistance)
        {
            rb.linearVelocity = Vector2.zero;
            enemyAttack.TryAttack();
            return;
        }

        rb.linearVelocity = direction * speed;
    }

    private void UpdateRandomOffset()
    {
        if (changeRandomOffsetCurrentTime <= 0f)
        {
            randomFollowOffset = Random.insideUnitCircle * randomOffsetDistance;
            changeRandomOffsetCurrentTime = changeRandomOffsetTime;
        }
        else
        {
            changeRandomOffsetCurrentTime -= Time.deltaTime;
        }
    }

    private void HandleKnockback()
    {
        knockbackTimer -= Time.deltaTime;

        if (knockbackTimer <= 0f)
        {
            isKnockedBack = false;
        }
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        if (rb == null) return;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);

        isKnockedBack = true;
        knockbackTimer = duration;
    }

    private void SetMoving(bool value)
    {
        if (anim != null)
        {
            anim.SetBool("Moving", value);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, desiredPosition);
    }
}