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

        if (_runner.NextAttackPattern == BossCtrl.AttackPattern.Cross)
        {
            if (healthRatio <= 0.40f) 
            {
                ShootCircle(8, 0f);
                yield return new WaitForSeconds(1.0f);
                ShootCircle(8, 22.5f);
                yield return new WaitForSeconds(0.5f);
            }
            else if (healthRatio <= 0.80f) 
            {
                ShootCircle(8, 0f);
                yield return new WaitForSeconds(0.5f);
            }
            else 
            {
                ShootCircle(4, 0f); 
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        else
        {
            
            int currentProj = Mathf.RoundToInt(Mathf.Lerp(maxProjectiles, minProjectiles, healthRatio));

            
            if (currentProj < minProjectiles) currentProj = minProjectiles;

            if (_runner.debug) Debug.Log($"[ATTACCO NORMALE] Vita: {healthRatio * 100}%, Sto per sparare {currentProj} proiettili!");

            for (int i = 0; i < currentProj; i++)
            {
                Vector2 shootDir = Random.insideUnitCircle.normalized;

                ShootProjectile(shootDir);
                yield return new WaitForSeconds(fireRate);
            }

        }

        attackCompleted = true;
        yield return new WaitForSeconds(0.5f);
        _runner.AnimActionComplete = true;
    }

    
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