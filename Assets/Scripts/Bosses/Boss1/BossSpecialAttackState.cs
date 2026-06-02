using UnityEngine;
using System.Collections;
using stateMachine;

[CreateAssetMenu(menuName = "States/Boss/Special Attack")]
public class BossSpecialAttackState : State<BossCtrl>
{
    public static int timesAttacked = 0;

    [Header("Impostazioni Attacco Tubi")]
    [SerializeField] private int projectilesPerPipe = 5;
    [SerializeField] private float fireRate = 0.3f;
    [SerializeField] private float delayBetweenPipes = 0.2f;

    [Header("Impostazioni Proiettile")]
    [SerializeField] private GameObject bossProjectilePrefab;
    [SerializeField] private float anticipationDelay = 0.4f;

    private PipeManager pipeManager;
    private bool attackCompleted = false;
    private bool triggerSent = false;

    public override void Enter()
    {
        attackCompleted = false;
        timesAttacked = 0;
        triggerSent = false;
        _runner.AnimActionComplete = false;
        pipeManager = _runner.transform.parent.GetComponentInChildren<PipeManager>();

        if (pipeManager == null)
        {
            attackCompleted = true;
            return;
        }
        _runner.StartCoroutine(FireSequenceRoutine());
    }

    private IEnumerator FireSequenceRoutine()
    {
        if (_runner.Anim != null) _runner.Anim.SetTrigger("jump_to_attack");
        else Debug.LogWarning("BossSpecialAttack: Boss does not have an Animator component.");

        yield return new WaitForSeconds(anticipationDelay);

        // 🔥 FIX: Trasformiamo da 0-100 a 0.0-1.0
        float healthRatio = _runner.Health.GetHealthPercentage() / 100f;

        int maxPipes = 2;
        if (healthRatio <= 0.25f) maxPipes = 8;
        else if (healthRatio <= 0.50f) maxPipes = 6;
        else if (healthRatio <= 0.75f) maxPipes = 4;

        timesAttacked = maxPipes;

        for (int i = 0; i < maxPipes; i++)
        {
            Transform selectedPipe = pipeManager.GetRandomPipe();

            if (selectedPipe != null)
            {
                Vector3 offset = _runner.FirePoint != null ? _runner.FirePoint.localPosition : Vector3.zero;
                _runner.transform.position = selectedPipe.position - offset;
                Physics2D.SyncTransforms();

                Vector2 shootDirection = selectedPipe.up;

                for (int p = 0; p < projectilesPerPipe; p++)
                {
                    ShootProjectile(shootDirection);
                    yield return new WaitForSeconds(fireRate);
                }
            }
            yield return new WaitForSeconds(delayBetweenPipes);
        }
        attackCompleted = true;
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

        if (_runner.AnimActionComplete) _runner.SetState(typeof(BossIdleState));
    }

    public override void Exit()
    {
        if (_runner != null && _runner.Visuals != null)
        {
            if (_runner.Visuals.TryGetComponent<SpriteRenderer>(out var sr)) sr.enabled = true;
        }
    }
    public override void CaptureInput() { }
    public override void FixedUpdate() { }
}