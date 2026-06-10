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
    [SerializeField] private float pipeProjectileSpeed = 15f;

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
        pipeManager = PipeManager.Instance;

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

        _runner.specialAttackActive = true;
        float healthRatio = _runner.Health.GetHealthPercentage() / 100f;

        int maxPipes = 4;
        if (healthRatio <= 0.30f) maxPipes = 12;
        else if (healthRatio <= 0.60f) maxPipes = 8;

        timesAttacked = maxPipes;
        Transform lastPipe = null;

        for (int i = 0; i < maxPipes; i++)
        {
            Transform selectedPipe = pipeManager.GetRandomPipe();

            if (selectedPipe != null)
            {
                lastPipe = selectedPipe;

                Transform pipeFirePoint = selectedPipe.Find("FirePoint");
                Vector3 shootPos = pipeFirePoint != null ? pipeFirePoint.position : selectedPipe.position;
                Vector2 shootDirection = pipeFirePoint != null ? (Vector2)pipeFirePoint.up : (Vector2)selectedPipe.up;

                Vector3 bossOffset = _runner.FirePoint != null ? _runner.FirePoint.localPosition : Vector3.zero;
                _runner.transform.position = shootPos - bossOffset;
                Physics2D.SyncTransforms();

                // Lancia la Coroutine indipendente che spara
                _runner.StartCoroutine(ShootFromPipeRoutine(shootPos, shootDirection));
            }

            // Aspetta il tempo di viaggio tra un tubo e l'altro
            yield return new WaitForSeconds(delayBetweenPipes);
        }

        // Aspetta che l'ULTIMO tubo abbia finito di sparare i suoi proiettili
        float waitRemaining = projectilesPerPipe * fireRate;
        yield return new WaitForSeconds(waitRemaining);

        // Aggancio sicuro alla NavMesh (Lasciando il boss ancora invisibile dietro le quinte)
        if (_runner.Agent != null)
        {
            _runner.Agent.enabled = true;

            if (UnityEngine.AI.NavMesh.SamplePosition(_runner.transform.position, out UnityEngine.AI.NavMeshHit hit, 5.0f, _runner.Agent.areaMask))
            {
                _runner.transform.position = hit.position;

                
                _runner.Agent.nextPosition = hit.position;
            }
            _runner.Agent.updatePosition = true;
            if (_runner.Agent.isOnNavMesh) _runner.Agent.isStopped = false;
        }

        attackCompleted = true;
        yield return new WaitForSeconds(0.05f); // Piccolo break di sicurezza restando invisibile
        _runner.AnimActionComplete = true;
        _runner.specialAttackActive = false;
    }

    private IEnumerator ShootFromPipeRoutine(Vector3 spawnPos, Vector2 direction)
    {
        for (int p = 0; p < projectilesPerPipe; p++)
        {
            ShootProjectile(direction, spawnPos);
            yield return new WaitForSeconds(fireRate);
        }
    }

    private void ShootProjectile(Vector2 direction, Vector3 spawnPos)
    {
        if (_runner.Shooter is ProjectileShooterBoss bossShooter)
            bossShooter.ShootPipeProjectile(_runner.gameObject, direction, pipeProjectileSpeed, spawnPos);
        else
            _runner.Shooter.ShootLinear(_runner.gameObject, direction);
    }

    public override void Update() { }

    public override void ChangeState()
    {
        if (!attackCompleted) return;

        if (_runner.AnimActionComplete) _runner.SetState(typeof(BossIdleState));
    }

    public override void Exit()
    {
        if (_runner != null && _runner.Visuals != null)
        {
            if (_runner.Visuals.TryGetComponent<SpriteRenderer>(out var sr)) sr.enabled = true;
        }
        Collider2D bossCollider = _runner.GetComponent<Collider2D>();
        if (bossCollider != null) bossCollider.enabled = true;
        if (_runner.Agent != null && !_runner.Agent.enabled) _runner.Agent.enabled = true;

        _runner.specialAttackActive = false;
    }

    public override void CaptureInput() { }
    public override void FixedUpdate() { }
}