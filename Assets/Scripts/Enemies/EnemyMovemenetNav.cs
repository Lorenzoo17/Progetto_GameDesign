using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovementNav : MonoBehaviour {

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float stopDistance = 1.2f;

    [Header("Random Follow Offset")]
    [SerializeField] private float randomOffsetDistance = 1f;
    [SerializeField] private float changeRandomOffsetTime = 3f;

    [Header("Knockback")]
    [SerializeField] private float knockbackResistance = 1f;

    private NavMeshAgent agent;
    private Animator anim;
    private EnemyAttackBase enemyAttack;

    private Vector3 desiredPosition;
    private Vector3 randomFollowOffset;

    private float changeRandomOffsetCurrentTime;

    private bool isKnockedBack;
    private Coroutine knockbackCoroutine;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        enemyAttack = GetComponent<EnemyAttackBase>();

        SetupNavMeshAgent();
    }

    private void Update() {
        if (Player.Instance == null || agent == null || !agent.enabled)
            return;

        if (!agent.isOnNavMesh)
            return;

        if (isKnockedBack)
            return;

        if (enemyAttack != null && enemyAttack.IsAttacking) {
            StopMovement();
            SetMoving(false);
            return;
        }

        FollowTarget();

        SetMoving(agent.velocity.sqrMagnitude > 0.01f);
    }

    private void FollowTarget() {
        UpdateRandomOffset();

        Vector3 playerPosition = Player.Instance.transform.position;

        desiredPosition = playerPosition + randomFollowOffset;
        desiredPosition.z = transform.position.z;

        float distance = Vector2.Distance(
            transform.position,
            playerPosition
        );

        if (distance <= stopDistance) {
            StopMovement();

            if (enemyAttack != null && distance <= enemyAttack.AttackDistance) {
                enemyAttack.TryAttack();
            }

            return;
        }

        if (enemyAttack != null && distance <= enemyAttack.AttackDistance) {
            StopMovement();

            enemyAttack.TryAttack();
            return;
        }

        agent.SetDestination(desiredPosition);
    }

    private void UpdateRandomOffset() {
        if (changeRandomOffsetCurrentTime <= 0f) {
            Vector2 random2D = Random.insideUnitCircle * randomOffsetDistance;

            randomFollowOffset = new Vector3(
                random2D.x,
                random2D.y,
                0f
            );

            changeRandomOffsetCurrentTime = changeRandomOffsetTime;
        }
        else {
            changeRandomOffsetCurrentTime -= Time.deltaTime;
        }
    }

    private void StopMovement() {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration) {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (knockbackCoroutine != null) {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(
            KnockbackCoroutine(direction, force, duration)
        );
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction, float force, float duration) {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            yield break;

        isKnockedBack = true;

        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.isStopped = true;

        Vector3 knockbackDirection = new Vector3(
            direction.normalized.x,
            direction.normalized.y,
            0f
        );

        float timer = 0f;

        while (timer < duration) {
            timer += Time.deltaTime;

            float knockbackSpeed = force / Mathf.Max(duration, 0.01f);

            agent.Move(
                knockbackDirection *
                (knockbackSpeed / Mathf.Max(knockbackResistance, 0.01f)) *
                Time.deltaTime
            );

            yield return null;
        }

        agent.isStopped = false;
        isKnockedBack = false;
        knockbackCoroutine = null;
    }

    private void SetMoving(bool value) {
        if (anim != null) {
            anim.SetBool("Moving", value);
        }
    }

    private void SetupNavMeshAgent() {
        if (agent == null)
            return;

        agent.speed = speed;

        // Fondamentale per usare NavMeshAgent in 2D
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // Il nemico non deve fermarsi "da solo" prima della destinazione.
        // Lo gestiamo noi con stopDistance / AttackDistance.
        agent.stoppingDistance = 0f;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, desiredPosition);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}