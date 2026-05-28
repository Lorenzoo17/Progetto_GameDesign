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
        
        if (_runner.Anim != null) _runner.Anim.SetTrigger("Attack");

        _runner.StartCoroutine(FireSequenceRoutine());
    }

    private IEnumerator FireSequenceRoutine() 
    {
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

            for (int i = 0; i < currentProjectiles; i++) 
            {
                Vector2 shootDir;

                if (_runner.MemoryTurnsLeft > 0 && _runner.LastKnownPlayerPos != null) 
                {
                    Vector3 target = _runner.LastKnownPlayerPos.Value;
                    shootDir = ((Vector2)target - (Vector2)_runner.transform.position).normalized;
                } 
                else 
                {
                    shootDir = Random.insideUnitCircle.normalized;
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
        /*
        if (bossProjectilePrefab == null) {
            Debug.LogWarning("Manca il prefab del proiettile in BossRangedAttackState!");
            return;
        }

        Vector3 spawnPos = _runner.FirePoint != null ? _runner.FirePoint.position : _runner.transform.position;
        GameObject proj = Instantiate(bossProjectilePrefab, spawnPos, Quaternion.identity);
        
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null) {
            rb.linearVelocity = direction.normalized * projectileSpeed;
        }
        */

        _runner.Shooter.ShootLinear(_runner.gameObject, direction);
    }

    public override void Update() {}

    public override void ChangeState() {
        if (attackCompleted) _runner.SetState(typeof(BossIntroMovementState));
    }

    public override void Exit() {
        if (_runner.Agent != null && _runner.Agent.isOnNavMesh) _runner.Agent.isStopped = false;
    }
    public override void CaptureInput() { }
    public override void FixedUpdate() { }
}