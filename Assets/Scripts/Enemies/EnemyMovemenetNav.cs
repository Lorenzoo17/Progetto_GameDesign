using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovementNav : MonoBehaviour {

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float stopDistance = 1.2f;
    [SerializeField] private float minTargetDistance = 20f; // distanza minima dalla quale inizia a inseguire il player

    [Header("Random Follow Offset")]
    [SerializeField] private float randomOffsetDistance = 1f;
    [SerializeField] private float changeRandomOffsetTime = 3f;

    [Header("Knockback")]
    [SerializeField] private float knockbackResistance = 1f;

    [Header("Line of sight")]
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private float lineOfSightCheckInterval = 0.1f;
    [SerializeField] private bool debugLineOfSight = true;
    [SerializeField] private float lineOfSightRadius = 0.15f;
    private bool hasLineOfSight;
    private float nextLineOfSightCheckTime;
    private RaycastHit2D lastLineOfSightHit;

    private NavMeshAgent agent;
    private Animator anim;
    private EnemyAttackBase enemyAttack;
    private Enemy enemy;

    private Vector3 desiredPosition;
    private Vector3 randomFollowOffset;

    private float changeRandomOffsetCurrentTime;

    private bool isKnockedBack;
    private Coroutine knockbackCoroutine;

    private Transform firePoint;
    private bool isRangedEnemy;

    private HealthSystem hs;
    private bool hasBeenHit; // in modo che inizi a seguire il player a prescindere dalla distanza se
    // e' stato colpito da esso

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        enemyAttack = GetComponent<EnemyAttackBase>();
        enemy = GetComponent<Enemy>();

        hs = GetComponent<HealthSystem>();
        hasBeenHit = false;
        hs.OnDamageTaken += Hs_OnDamageTaken;

        SetupNavMeshAgent();

        if (TryGetComponent<ProjectileShooter>(out ProjectileShooter ps)) {
            firePoint = ps.firePoint;
            isRangedEnemy = true;
        }
        else {
            firePoint = transform;
            isRangedEnemy = false;
        }
    }

    private void Hs_OnDamageTaken(object sender, DamageEventArgs e) {
        hasBeenHit = true;
    }

    private void Update() {
        if (Player.Instance == null || agent == null || !agent.enabled)
            return;

        if (!agent.isOnNavMesh)
            return;

        if (enemy != null && enemy.IsStunned)
        {
            StopMovement();
            SetMoving(false);
            return;
        }

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

        if(distance <= minTargetDistance || hasBeenHit) { // se e' stato colpito o la distanza e' inferiore a minTargetDistance
            if (isRangedEnemy) {
                HandleRangedEnemy(distance, playerPosition);
            }
            else {
                HandleMeleeEnemy(distance);
            }
        }
        else {
            StopMovement();
        }
    }

    private void HandleRangedEnemy(float distance, Vector3 playerPosition) {
        if (enemyAttack == null) {
            agent.SetDestination(desiredPosition);
            return;
        }

        bool shouldCheckLineOfSight =
            distance <= enemyAttack.AttackDistance + 1f;

        if (shouldCheckLineOfSight && Time.time >= nextLineOfSightCheckTime) {
            hasLineOfSight = CanSeePlayer(playerPosition);
            nextLineOfSightCheckTime = Time.time + lineOfSightCheckInterval;
        }

        if (!shouldCheckLineOfSight) {
            hasLineOfSight = false;
        }

        bool canAttack =
            distance <= enemyAttack.AttackDistance &&
            hasLineOfSight;

        if (canAttack) {
            StopMovement();
            enemyAttack.TryAttack();
            return;
        }

        if (hasLineOfSight && distance <= stopDistance) {
            StopMovement();
            return;
        }

        agent.SetDestination(desiredPosition);
    }

    private void HandleMeleeEnemy(float distance) {
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

    // controlla se c'e' un ostacolo tra lui e il player, in modo da non fermarsi ad attaccare se
    // non vede il player
    private bool CanSeePlayer(Vector2 playerPosition) {
        if (Player.Instance == null)
            return false;

        Vector2 origin = firePoint != null ? firePoint.position : transform.position;

        Vector2 directionToPlayer = playerPosition - origin;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= 0.01f)
            return true;

        Vector2 direction = directionToPlayer.normalized;

        lastLineOfSightHit = Physics2D.CircleCast(
            origin,
            lineOfSightRadius,
            direction,
            distanceToPlayer,
            obstacleLayerMask
        );

        if (debugLineOfSight) {
            Debug.DrawLine(
                origin,
                playerPosition,
                lastLineOfSightHit.collider == null ? Color.green : Color.red
            );
        }

        return lastLineOfSightHit.collider == null;
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

    public void ForceStop()
    {
        StopMovement();
        SetMoving(false);
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