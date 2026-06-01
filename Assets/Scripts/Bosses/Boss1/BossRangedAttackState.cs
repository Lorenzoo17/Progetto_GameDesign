using UnityEngine;
using System.Collections;
using stateMachine;

[CreateAssetMenu(menuName = "States/Boss/Ranged Attack")]
public class BossRangedAttackState : State<BossCtrl>
{
    [Header("Impostazioni Generali")]
    [SerializeField] private float fireRate = 0.5f;     

    [Header("Impostazioni Proiettile")]
    [SerializeField] private GameObject bossProjectilePrefab; // Trascina qui il prefab!
    [SerializeField] private float projectileSpeed = 8f;

    [Header("Quantità Proiettili (Attacco Mirato/Random)")] 
    [SerializeField] private int minProjectiles = 2; 
    [SerializeField] private int maxProjectiles = 6; 

    private bool attackCompleted = false;

    public override void Enter() 
    {
        attackCompleted = false;
        
        if (_runner.Agent != null && _runner.Agent.isOnNavMesh) {
            _runner.Agent.isStopped = true;
        }
        
        //if (_runner.Anim != null) _runner.Anim.SetTrigger("Attack");

        _runner.StartCoroutine(FireSequenceRoutine());
    }

    private IEnumerator FireSequenceRoutine() 
    {
        if (_runner.Anim != null)
        {
            _runner.Anim.SetTrigger("jump_to_attack");
        }
        else
        {
            Debug.LogWarning("BossRangedAttack: Boss does not have an Animator component.");
        }
        // === ATTACCO A CROCE (Al 4° Salto) ===
        if (_runner.NextAttackPattern == BossCtrl.AttackPattern.Cross) 
        {
            Vector2[] crossDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            
            foreach(Vector2 dir in crossDirections) {
                ShootProjectile(dir);
            }
            yield return new WaitForSeconds(0.5f); 
        } 
        // === ATTACCO MIRATO O RANDOM ===
        else 
        {
            float healthPercent = _runner.Health.GetHealthPercentage();
            int currentProjectiles = Mathf.RoundToInt(Mathf.Lerp(maxProjectiles, minProjectiles, healthPercent));
            Debug.Log($"Health: {healthPercent}%, Projectiles to Fire: {currentProjectiles}");

            for (int i = 0; i < currentProjectiles; i++) 
            {
                Vector2 shootDir = Random.insideUnitCircle.normalized;

                if (_runner.MemoryTurnsLeft > 0 && _runner.LastKnownPlayerPos != null) 
                {
                    
                    Player player = Object.FindFirstObjectByType<Player>();
                    if (player != null)
                    {
                        // Calcola la direzione verso la sua posizione attuale
                        Vector3 targetPos = player.transform.position;
                        if (player.TryGetComponent<Rigidbody2D>(out Rigidbody2D playerRb))
                        {
                            float distance = Vector2.Distance(_runner.transform.position, targetPos);
                            float timeToImpact = distance / 8f;
                            Vector2 playerVelocity = playerRb.linearVelocity;
                            targetPos += (Vector3)(playerVelocity * timeToImpact);
                        }

                        Vector2 errorMargin = Random.insideUnitCircle * 1.5f;
                        targetPos += (Vector3)errorMargin;
                        shootDir = (targetPos - _runner.transform.position).normalized;
                        Debug.Log($"Shooting towards last known player position: {targetPos}, Direction: {shootDir}");
                    }
                } 
                else 
                {

                    Debug.Log($"Shooting in random direction: {shootDir}");
                }

                ShootProjectile(shootDir);
                yield return new WaitForSeconds(fireRate); 
            }


            if (_runner.MemoryTurnsLeft > 0) {
                _runner.MemoryTurnsLeft--;
                if (_runner.MemoryTurnsLeft == 0) {
                    _runner.LastKnownPlayerPos = null; 
                }
            }
        }

        attackCompleted = true; 
    }

    // NUOVO METODO: Instanzia il proiettile e usa la fisica per muoverlo!
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
        if (attackCompleted) {
            if (_runner.Anim != null)
            {
                _runner.Anim.SetTrigger("attack_to_idle");
            }
            else
            {
                Debug.LogWarning("BossSpecialAttack: Boss does not have an Animator component.");
            }
            _runner.SetState(typeof(BossIntroMovementState));
        }
        
    }

    public override void Exit() {
        if (_runner.Agent != null && _runner.Agent.isOnNavMesh) _runner.Agent.isStopped = false;
    }
    public override void CaptureInput() { }
    public override void FixedUpdate() { }
}