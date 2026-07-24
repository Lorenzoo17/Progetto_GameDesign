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

    [Header("Wander")]
    // per movimento casuale prima di vedere il player
    [SerializeField] private float wanderRadius = 6f; // raggio entro il quale il nemico cerca punto casuale
    [SerializeField] private float wanderRepathTime = 2f; // secondi prima di scegliere nuovo punto casuale
    [SerializeField] private float wanderPointReachedDistance = 0.1f; // offset di arrivo
    [SerializeField] private float navMeshSampleDistance = 2f;
    [SerializeField] private int maxWanderPointAttempts = 10;
    [SerializeField] private float maxDistanceFromSpawn = 10f;
    [SerializeField] private float minDistanceFromNavMeshEdge = 0.5f;
    private Vector3 spawnPosition; // per cercare punto attorno allo spawnpoint
    private Vector3 currentWanderDestination;
    private bool hasWanderDestination;
    private float nextWanderPointTime;
    private NavMeshPath wanderPath;

    //[Header("Wander attack")]
    // per attaccare casualmente sempre anche se non vede il player
    // da attivare principalmente per nemici a distanza che attaccano in direzione casuale
    //[SerializeField] private bool alwaysAttack;

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
    private bool hasSeenPlayer;

    private bool isMovementBlocked = false;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        enemyAttack = GetComponent<EnemyAttackBase>();
        enemy = GetComponent<Enemy>();

        spawnPosition = transform.position;
        wanderPath = new NavMeshPath();

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
        
        if (isMovementBlocked)
        {
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

        // Se il player entra nel range di "visione", il nemico lo ricorderà per sempre
        if (distance <= minTargetDistance) {
            hasSeenPlayer = true;
        }

        if (hasSeenPlayer || hasBeenHit) { // oppure anche se e' stato colpito
            hasWanderDestination = false;

            if (isRangedEnemy) {
                HandleRangedEnemy(distance, playerPosition);
            }
            else {
                HandleMeleeEnemy(distance);
            }
        }
        else {
            HandleWander();
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

        bool canAttack = distance <= enemyAttack.AttackDistance && hasLineOfSight;

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

    private void HandleWander() {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        bool reachedDestination =
            hasWanderDestination &&
            !agent.pathPending &&
            agent.hasPath &&
            agent.remainingDistance <= wanderPointReachedDistance;

        bool shouldPickNewPoint =
            !hasWanderDestination ||
            reachedDestination ||
            Time.time >= nextWanderPointTime;

        if (!shouldPickNewPoint)
            return;

        // provo a prendere punto casuale nel navmesh dove far muovere il nemico
        if (TryGetRandomNavMeshPoint(out Vector3 randomPoint)) {
            currentWanderDestination = randomPoint;
            hasWanderDestination = true;
            nextWanderPointTime = Time.time + wanderRepathTime;

            agent.SetDestination(currentWanderDestination);
        }
        else {
            StopMovement();
            hasWanderDestination = false;
        }
    }

    private bool TryGetRandomNavMeshPoint(out Vector3 result) {
        for (int i = 0; i < maxWanderPointAttempts; i++) {
            Vector2 random2D = Random.insideUnitCircle * wanderRadius;

            Vector3 randomPosition = spawnPosition + new Vector3(
                random2D.x,
                random2D.y,
                0f
            );

            if (NavMesh.SamplePosition(
                randomPosition,
                out NavMeshHit hit,
                navMeshSampleDistance,
                NavMesh.AllAreas
            )) {
                Vector3 point = hit.position;
                point.z = transform.position.z;

                if (Vector2.Distance(point, spawnPosition) > maxDistanceFromSpawn)
                    continue;

                if (NavMesh.FindClosestEdge(point, out NavMeshHit edgeHit, NavMesh.AllAreas)) {
                    if (edgeHit.distance < minDistanceFromNavMeshEdge)
                        continue;
                }

                if (agent.CalculatePath(point, wanderPath) &&
                    wanderPath.status == NavMeshPathStatus.PathComplete) {
                    result = point;
                    return true;
                }
            }
        }

        result = transform.position;
        return false;
    }

    private void StopMovement() {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    public void ForceStop()
    {
        isMovementBlocked = true;
        StopMovement();
        SetMoving(false);
    }
    public void ResumeMovement()
    {
        isMovementBlocked = false; 

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false; 
        }
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(currentWanderDestination, 0.15f);
    }
}