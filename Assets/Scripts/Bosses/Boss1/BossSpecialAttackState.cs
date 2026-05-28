using UnityEngine;
using System.Collections;
using stateMachine;

[CreateAssetMenu(menuName = "States/Boss/Special Attack")]
public class BossSpecialAttackState : State<BossCtrl>
{
    public static int timesAttacked = 0; 

    [Header("Impostazioni Attacco Tubi")]
    [SerializeField] private int projectilesPerPipe = 3; 
    [SerializeField] private float fireRate = 0.5f;     
    [SerializeField] private float delayBetweenPipes = 0.5f; 

    [Header("Impostazioni Proiettile")]
    [SerializeField] private GameObject bossProjectilePrefab; // Trascina qui il prefab!
    [SerializeField] private float projectileSpeed = 8f;

    private PipeManager pipeManager;
    private bool attackCompleted = false;

    public override void Enter() 
    {
        attackCompleted = false;
        timesAttacked = 0;
        
        pipeManager = _runner.transform.parent.GetComponentInChildren<PipeManager>();
        if (pipeManager == null) {
            attackCompleted = true; 
            return;
        }

        _runner.StartCoroutine(FireSequenceRoutine());
    }

    private IEnumerator FireSequenceRoutine() 
    {
        float healthPercent = _runner.Health.GetHealthPercentage();
        
        int maxPipes = 2;
        if (healthPercent <= 0.25f) maxPipes = 8;
        else if (healthPercent <= 0.50f) maxPipes = 6;
        else if (healthPercent <= 0.75f) maxPipes = 4;

        timesAttacked = maxPipes; 

        for (int i = 0; i < maxPipes; i++)
        {
            Transform selectedPipe = pipeManager.GetRandomPipe();
            
            if (selectedPipe != null) 
            {
                Vector3 offset = _runner.FirePoint != null ? _runner.FirePoint.localPosition : Vector3.zero;
                _runner.transform.position = selectedPipe.position - offset;
                
                Physics2D.SyncTransforms(); // Forza l'aggiornamento della fisica

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
        // Usa il polimorfismo: se lo shooter è quello del Boss, usa le funzioni nuove!
        if (_runner.Shooter is ProjectileShooterBoss bossShooter)
        {
            bossShooter.ShootBossProjectile(_runner.gameObject, direction);
        }
        else
        {
            // Sicurezza: se per sbaglio c'è il vecchio shooter, usa quello lineare
            _runner.Shooter.ShootLinear(_runner.gameObject, direction);
        }
    }

    public override void Update() {}

    public override void ChangeState() {
        if (attackCompleted) _runner.SetState(typeof(BossIdleState));
    }

    public override void Exit() {}
    public override void CaptureInput() { }
    public override void FixedUpdate() { }
}