using FirstGearGames.SmoothCameraShaker;
using UnityEngine;
using UnityEngine.AI;

public class BossFungus : MonoBehaviour {
    public enum BossFungusFSM {
        Idle,
        Fly,
        StartSmash,
        SmashFall,
        SmashEnd,
        Reposition,
        ChaseBurst
    }

    [Header("General data")]
    public BossFungusFSM CurrentState;
    [SerializeField] private float speed;
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackStartDistance;
    [SerializeField] private float moveDistance;
    [SerializeField] private float meleeAttackRange;
    [SerializeField] private int projectileNumberToShootMin = 5;
    [SerializeField] private int projectileNumberToShootMax = 15;

    [Header("Attack animation data")]
    [SerializeField] private float startSmashDuration = 0.5f;
    [SerializeField] private float smashFallSpeed = 8f;
    [SerializeField] private float smashFallDistance = 2f;
    [SerializeField] private float smashFallDuration = 0.25f;
    [SerializeField] private float smashEndDuration = 0.6f;

    [Header("Reposition data")]
    [SerializeField] private float repositionSpeed = 0f;
    [SerializeField] private float repositionStoppingDistance = 0.2f;
    [SerializeField] private float[] repositionDistances = { 3f, 5f, 7f };
    private Vector3 repositionTarget;

    [Header("Chase Burst data")]
    [SerializeField] private float chaseBurstSpeed = 8f;
    [SerializeField] private float chaseBurstDuration = 0.45f;
    [SerializeField] private float chaseBurstMinDistance = 6f;
    [SerializeField] private float chaseBurstStopDistanceFromPlayer = 2f;
    [SerializeField] private float chaseBurstCooldown = 3f;
    [SerializeField] private float chaseBurstChance = 0.5f;
    private float chaseBurstCooldownTimer;

    [Header("Fly shooting data")]
    [SerializeField] private float flyShootCooldown = 1.5f;
    [SerializeField] private float flyShootMinDistance = 3.5f;
    [SerializeField] private float flyShootMaxDistance = 10f;
    [SerializeField] private int flyShootChanceDenominator = 3;
    private float flyShootCooldownTimer;

    private float distanceFromPlayer;
    private float stateTimer;

    private Vector3 desiredPosition;
    private Vector3 smashStartPosition;
    private Vector3 smashTargetPosition;

    private NavMeshAgent agent;
    private Animator anim;
    private ProjectileShooter shooter;
    private HealthSystem hs;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        shooter = GetComponent<ProjectileShooter>();
        hs = GetComponent<HealthSystem>();

        if(hs != null) {
            hs.OnDamageTaken += Hs_OnDamageTaken;
        }

        SetupNavMeshAgent();
    }

    // quando muore si torna alla musica del dungeon
    private void Hs_OnDamageTaken(object sender, DamageEventArgs e) {
        if(hs.CurrentHealth <= 0) {
            if(MusicManager.Instance != null) {
                MusicManager.Instance.PlayMusic(MusicID.SewerDungeon, 0.5f, 0.15f);
            }
        }
    }

    private void Start() {
        if(repositionSpeed == 0f) {
            repositionSpeed = speed;
        }
        ChangeState(BossFungusFSM.Idle);

        if(MusicManager.Instance != null) { // allo start (quindi allo spawn) o eventualmente ad uno specifico
            // trigger enter
            MusicManager.Instance.PlayMusic(MusicID.BossFungus, .5f, .3f);
        }
    }

    private void Update() {
        if (Player.Instance == null) return;

        if (chaseBurstCooldownTimer > 0f) {
            chaseBurstCooldownTimer -= Time.deltaTime;
        }

        if (flyShootCooldownTimer > 0f) {
            flyShootCooldownTimer -= Time.deltaTime;
        }

        distanceFromPlayer = Vector2.Distance(
            Player.Instance.transform.position,
            transform.position
        );

        switch (CurrentState) {
            case BossFungusFSM.Idle:
                Idle();
                break;

            case BossFungusFSM.Fly:
                Fly();
                break;

            case BossFungusFSM.StartSmash:
                StartSmash();
                break;

            case BossFungusFSM.SmashFall:
                SmashFall();
                break;

            case BossFungusFSM.SmashEnd:
                SmashEnd();
                break;

            case BossFungusFSM.Reposition:
                Reposition();
                break;

            case BossFungusFSM.ChaseBurst:
                ChaseBurst();
                break;
        }
    }

    private void ChangeState(BossFungusFSM newState) {
        CurrentState = newState;
        stateTimer = 0f;

        switch (CurrentState) {
            case BossFungusFSM.Idle:
                StopMovement();
                if (anim != null) anim.Play("Idle");
                break;

            case BossFungusFSM.Fly:
                if (agent != null) {
                    agent.speed = speed;
                }
                if (anim != null) anim.Play("Fly");
                break;

            case BossFungusFSM.StartSmash:
                StopMovement();
                if (anim != null) anim.Play("SmashStart");
                break;

            case BossFungusFSM.SmashFall:
                StopMovement();

                smashStartPosition = transform.position;
                smashTargetPosition = smashStartPosition + Vector3.down * smashFallDistance;

                if (anim != null) anim.Play("SmashFall");
                break;

            case BossFungusFSM.SmashEnd:
                StopMovement();

                if (anim != null) anim.Play("SmashEnd");

                MeleeDamage();
                Shoot();

                // - spawn effetto impatto
                // - attivazione hitbox
                break;

            case BossFungusFSM.Reposition:
                if (anim != null) anim.Play("Fly");

                if (agent != null) {
                    agent.speed = repositionSpeed;
                }

                ChooseRandomRepositionTarget();
                break;

            case BossFungusFSM.ChaseBurst:
                stateTimer = 0f;

                if (anim != null) anim.Play("Fly");

                if (agent != null && agent.enabled && agent.isOnNavMesh) {
                    agent.speed = chaseBurstSpeed;
                }

                break;
        }
    }

    private void Idle() {
        if (distanceFromPlayer <= moveDistance) {
            ChangeState(BossFungusFSM.Fly);
        }
        else {
            StopMovement();
        }
    }

    private void Fly() {
        if (distanceFromPlayer > moveDistance) {
            ChangeState(BossFungusFSM.Idle); // torno in stato idle
            return;
        }

        TryShootWhileFlying();

        if (ShouldDoChaseBurst()) {
            ChangeState(BossFungusFSM.ChaseBurst);
            return;
        }


        if (distanceFromPlayer > attackStartDistance) { // mi sposto verso il giocatore
            desiredPosition = Player.Instance.transform.position;
            desiredPosition.z = transform.position.z;

            if (agent != null && agent.enabled && agent.isOnNavMesh) {
                agent.SetDestination(desiredPosition);
            }
        }
        else {
            ChangeState(BossFungusFSM.StartSmash); // inizio attacco
        }
    }

    private void StartSmash() {
        stateTimer += Time.deltaTime; // faccio partire timer (concorde con durata animazione)

        StopMovement();

        if (stateTimer >= startSmashDuration) {
            ChangeState(BossFungusFSM.SmashFall);
        }
    }

    private void SmashFall() {
        stateTimer += Time.deltaTime;

        float t = stateTimer / smashFallDuration;
        t = Mathf.Clamp01(t);

        transform.position = Vector3.Lerp(
            smashStartPosition,
            smashTargetPosition,
            t
        );

        if (t >= 1f) {
            ChangeState(BossFungusFSM.SmashEnd);
        }
    }

    private void SmashEnd() {
        stateTimer += Time.deltaTime;

        StopMovement();

        if (stateTimer >= smashEndDuration) {
            bool shouldReposition = Random.Range(0, 3) == 0;

            if (shouldReposition) {
                ChangeState(BossFungusFSM.Reposition);
                return;
            }

            if (distanceFromPlayer <= attackStartDistance)
                ChangeState(BossFungusFSM.StartSmash);
            else
                ChangeState(BossFungusFSM.Idle);
        }
    }

    private void SetupNavMeshAgent() {
        if (agent == null)
            return;

        agent.speed = speed;

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.stoppingDistance = 0f;
    }

    private void StopMovement() {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    private void Reposition() {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) {
            ChangeState(BossFungusFSM.Idle);
            return;
        }

        if (agent.pathPending) return;

        if (agent.remainingDistance <= repositionStoppingDistance) {
            if (distanceFromPlayer <= attackStartDistance)
                ChangeState(BossFungusFSM.StartSmash);
            else
                ChangeState(BossFungusFSM.Idle);
        }
    }

    private bool ShouldDoChaseBurst() {
        if (chaseBurstCooldownTimer > 0f) return false;

        if (distanceFromPlayer < chaseBurstMinDistance) return false;

        return Random.value <= chaseBurstChance;
    }

    private void ChaseBurst() {
        stateTimer += Time.deltaTime;

        if (Player.Instance == null) {
            ChangeState(BossFungusFSM.Idle);
            return;
        }

        Vector3 targetPosition = Player.Instance.transform.position;

        Vector2 directionToBoss =
            ((Vector2)transform.position - (Vector2)Player.Instance.transform.position).normalized;

        Vector3 stopPosition = targetPosition + (Vector3)(directionToBoss * chaseBurstStopDistanceFromPlayer);
        stopPosition.z = transform.position.z;

        if (agent != null && agent.enabled && agent.isOnNavMesh) {
            agent.speed = chaseBurstSpeed;
            agent.SetDestination(stopPosition);
        }

        bool burstFinished = stateTimer >= chaseBurstDuration;

        bool closeEnough = distanceFromPlayer <= attackStartDistance;

        if (burstFinished || closeEnough) {
            chaseBurstCooldownTimer = chaseBurstCooldown;

            if (distanceFromPlayer <= attackStartDistance) {
                ChangeState(BossFungusFSM.StartSmash);
            }
            else {
                ChangeState(BossFungusFSM.Fly);
            }
        }
    }

    private void ChooseRandomRepositionTarget() {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        float randomDistance = repositionDistances[
            Random.Range(0, repositionDistances.Length)
        ];

        Vector3 rawTarget = transform.position + (Vector3)(randomDirection * randomDistance);
        rawTarget.z = transform.position.z;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(rawTarget, out hit, 2f, NavMesh.AllAreas)) {
            repositionTarget = hit.position;
            repositionTarget.z = transform.position.z;

            agent.SetDestination(repositionTarget);
        }
        else {
            ChangeState(BossFungusFSM.Idle);
        }
    }

    private void MeleeDamage() {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, meleeAttackRange);

        foreach (Collider2D entity in hitColliders) {
            if (entity == null) continue;
            if (entity.GetComponent<Enemy>() != null) continue;

            if (entity.TryGetComponent<IDamageable>(out IDamageable damageable)) {
                Vector2 direction = ((Vector2)entity.transform.position - (Vector2)transform.position).normalized;

                damageable.TakeDamage(new DamageInfo(
                    attackDamage,
                    direction,
                    gameObject,
                    EntityType.Enemy
                ));
            }
        }

        if (EffectManager.Instance != null) {
            ShakeData cameraShakeAttackData = EffectManager.Instance.GetShakeDataByType(ShakeDataType.SmashAttack);

            if (cameraShakeAttackData != null) {
                CameraShakerHandler.Shake(cameraShakeAttackData);
            }
        }
    }

    private void TryShootWhileFlying() {
        if (shooter == null) return;
        if (Player.Instance == null) return;

        if (flyShootCooldownTimer > 0f) return;

        // if (distanceFromPlayer < flyShootMinDistance) return;
        // if (distanceFromPlayer > flyShootMaxDistance) return;

        bool shouldShoot = Random.Range(0, flyShootChanceDenominator) == 0;

        flyShootCooldownTimer = flyShootCooldown;

        if (!shouldShoot) return;

        // camera shake quando attacca ed e' in volo
        if (EffectManager.Instance != null) {
            ShakeData cameraShakeAttackData = EffectManager.Instance.GetShakeDataByType(ShakeDataType.RangedAttack);

            if (cameraShakeAttackData != null) {
                CameraShakerHandler.Shake(cameraShakeAttackData);
            }
        }

        Shoot();
    }

    private void Shoot() {
        if (shooter == null) return;

        if (SoundManager.Instance != null) {
            SoundManager.Instance.PlaySound2D(SoundID.EnemyShoot, .25f);
        }

        int projectileNumber = Random.Range(projectileNumberToShootMin, projectileNumberToShootMax);

        if (distanceFromPlayer >= chaseBurstMinDistance) {
            // Se il player è lontano, uso più spesso rosa stretta
            shooter.ShootFocusedSpread(
                gameObject,
                Mathf.Min(projectileNumber, 7),
                Player.Instance.transform,
                45f
            );

            return;
        }

        int shootingType = Random.Range(0, 2);

        switch (shootingType) {
            case 0:
                shooter.ShootMultipleProjectile(
                    gameObject,
                    projectileNumber,
                    Player.Instance.transform,
                    true
                );
                return;

            case 1:
                shooter.ShootMultipleProjectile(
                    gameObject,
                    projectileNumber,
                    Player.Instance.transform,
                    false
                );
                return;
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
    }

}
