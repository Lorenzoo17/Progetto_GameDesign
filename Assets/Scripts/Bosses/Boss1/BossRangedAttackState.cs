using UnityEngine;
using System.Collections;
using stateMachine;

[CreateAssetMenu(menuName = "States/Boss/Ranged Attack")]
public class BossRangedAttackState : State<BossCtrl>
{
    [Header("Impostazioni Generali")]
    [SerializeField] private float fireRate = 0.5f;

    [Header("Impostazioni Proiettile")]
    [SerializeField] private GameObject bossProjectilePrefab;
    [SerializeField] private float projectileSpeed = 8f;

    [Header("Quantità Proiettili (Attacco Normale)")]
    [SerializeField] private int minProjectiles = 2;
    [SerializeField] private int maxProjectiles = 6;

    private bool attackCompleted = false;
    private bool triggerSent = false;
    [SerializeField] private float anticipationDelay = 0.4f;

    public override void Enter()
    {
        attackCompleted = false;
        _runner.AnimActionComplete = false;
        triggerSent = false;

        if (_runner.Agent != null && _runner.Agent.isOnNavMesh)
        {
            _runner.Agent.isStopped = true;
        }

        _runner.StartCoroutine(FireSequenceRoutine());
    }

    private IEnumerator FireSequenceRoutine()
    {
        if (_runner.Anim != null) _runner.Anim.SetTrigger("jump_to_attack");
        else Debug.LogWarning("BossRangedAttack: Boss does not have an Animator component.");

        yield return new WaitForSeconds(anticipationDelay);

        float healthRatio = _runner.Health.GetHealthPercentage() / 100f;

        // =======================================================
        // === ATTACCO AL CENTRO (Il tuo Special Attack!) ===
        // =======================================================
        if (_runner.NextAttackPattern == BossCtrl.AttackPattern.Cross)
        {
            if (healthRatio <= 0.40f) // Sotto il 40%: DUE ondate da 8 proiettili
            {
                ShootCircle(8, 0f);
                yield return new WaitForSeconds(1.0f);
                ShootCircle(8, 22.5f);
                yield return new WaitForSeconds(0.5f);
            }
            else if (healthRatio <= 0.80f) // Sotto l'80% ma sopra il 40%: UNA ondata da 8
            {
                ShootCircle(8, 0f);
                yield return new WaitForSeconds(0.5f);
            }
            else // Sopra l'80% di vita: Croce normale (4 proiettili)
            {
                ShootCircle(4, 0f); // 4 proiettili formano una croce perfetta!
                yield return new WaitForSeconds(0.5f);
            }
        }
        // =======================================================
        // === ATTACCO STANDARD (Fine Salto Normale) ===
        // =======================================================
        else
        {
            // Calcola quanti proiettili sparare tra min e max.
            int currentProj = Mathf.RoundToInt(Mathf.Lerp(maxProjectiles, minProjectiles, healthRatio));

            // Sicurezza: Evita che un valore errato nell'Inspector blocchi tutto a 1
            if (currentProj < minProjectiles) currentProj = minProjectiles;

            Debug.Log($"[ATTACCO NORMALE] Vita: {healthRatio * 100}%, Sto per sparare {currentProj} proiettili!");

            for (int i = 0; i < currentProj; i++)
            {
                Vector2 shootDir = Random.insideUnitCircle.normalized;

                // Se ha il tracciatore, mira al player
                if (_runner.MemoryTurnsLeft > 0 && _runner.LastKnownPlayerPos != null)
                {
                    Player player = UnityEngine.Object.FindFirstObjectByType<Player>();
                    if (player != null)
                    {
                        Vector3 targetPos = player.transform.position;
                        if (player.TryGetComponent<Rigidbody2D>(out Rigidbody2D playerRb))
                        {
                            float distance = Vector2.Distance(_runner.transform.position, targetPos);
                            float timeToImpact = distance / projectileSpeed;
                            Vector2 playerVelocity = playerRb.linearVelocity;
                            targetPos += (Vector3)(playerVelocity * timeToImpact);
                        }

                        Vector2 errorMargin = Random.insideUnitCircle * 1.5f;
                        targetPos += (Vector3)errorMargin;
                        shootDir = (targetPos - _runner.transform.position).normalized;
                    }
                }

                ShootProjectile(shootDir);
                yield return new WaitForSeconds(fireRate);
            }

            if (_runner.MemoryTurnsLeft > 0)
            {
                _runner.MemoryTurnsLeft--;
                if (_runner.MemoryTurnsLeft == 0) _runner.LastKnownPlayerPos = null;
            }
        }

        attackCompleted = true;
    }

    // Funzione comodissima per creare cerchi di proiettili
    private void ShootCircle(int numberOfProjectiles, float offsetAngle = 0f)
    {
        float angleStep = 360f / numberOfProjectiles;
        for (int i = 0; i < numberOfProjectiles; i++)
        {
            float currentAngle = (i * angleStep) + offsetAngle;
            Vector2 dir = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));
            ShootProjectile(dir.normalized);
        }
    }

    private void ShootProjectile(Vector2 direction)
    {
        if (_runner.Shooter is ProjectileShooterBoss bossShooter)
            bossShooter.ShootBossProjectile(_runner.gameObject, direction);
        else
            _runner.Shooter.ShootLinear(_runner.gameObject, direction);
    }

    public override void Update() { }

    public override void ChangeState()
    {
        if (!attackCompleted) return;

        if (!triggerSent)
        {
            if (_runner.Anim != null) _runner.Anim.SetTrigger("attack_to_idle");
            triggerSent = true;
        }

        if (_runner.AnimActionComplete) _runner.SetState(typeof(BossIntroMovementState));
    }

    public override void Exit()
    {
        if (_runner.Agent != null && _runner.Agent.isOnNavMesh) _runner.Agent.isStopped = false;
    }
    public override void CaptureInput() { }
    public override void FixedUpdate() { }
}