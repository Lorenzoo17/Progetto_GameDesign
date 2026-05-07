using FirstGearGames.SmoothCameraShaker;
using UnityEngine;
using UnityEngine.AI;

public class BossFungus : MonoBehaviour {
    public enum BossFungusFSM {
        Idle,
        Fly,
        StartSmash,
        SmashFall,
        SmashEnd
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

    private float distanceFromPlayer;
    private float stateTimer;

    private Vector3 desiredPosition;
    private Vector3 smashStartPosition;
    private Vector3 smashTargetPosition;

    private NavMeshAgent agent;
    private Animator anim;
    private ProjectileShooter shooter;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        shooter = GetComponent<ProjectileShooter>();

        SetupNavMeshAgent();
    }

    private void Start() {
        ChangeState(BossFungusFSM.Idle);
    }

    private void Update() {
        if (Player.Instance == null) return;

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
            if(distanceFromPlayer <= attackStartDistance)
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
            ShakeData cameraShakeAttackData = EffectManager.Instance.GetShakeDataByType(ShakeDataType.MeleeAttack);

            if (cameraShakeAttackData != null) {
                CameraShakerHandler.Shake(cameraShakeAttackData);
            }
        }
    }

    private void Shoot() {
        if (shooter == null) return;

        int projectileNumber = Random.Range(projectileNumberToShootMin, projectileNumberToShootMax);
        shooter.ShootMultipleProjectile(gameObject, projectileNumber);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
    }

}
