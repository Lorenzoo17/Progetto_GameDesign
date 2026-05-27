using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using stateMachine; 

[CreateAssetMenu(menuName = "States/Boss/Intro Movement")]
public class BossIntroMovementState : State<BossCtrl>
{
    [Header("Impostazioni Salto Boss")]
    [SerializeField] private float jumpDuration = 1f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float waitTimeBetweenJumps = 2f;
    [SerializeField] private float minJumpDistance = 2f; 
    [SerializeField] private float maxJumpDistance = 6f; 

    [Header("Pozza di Acido")]
    [SerializeField] private GameObject acidPoolPrefab;
    [SerializeField] private float acidPoolDuration = 5f;
    [SerializeField] private float acidPoolDurationSpecial = 5f;

    [Header("Probabilità Attacco Speciale")]
    [SerializeField] private float baseProbability = 0.12f; 
    [SerializeField] private float probabilityIncrease = 0.10f; 

    private float waitTimer = 0f;
    private bool isJumping = false;
    private int jumpCounter = 0;
    
    private float currentProbability;
    private int specialAttackCooldown = 4; // Impostato a 4 all'inizio per fargli fare il primo giro tranquillo
    private bool isReadyForSpecialAttack = false; 
    private bool specialAttackEnabled = false;
    
    private bool hasJustLanded = false; 
    
    private bool useSpecialPool = false;
    private bool leaveAcidPool = true; 

    private BotolaManager botolaManager;

    public override void Init(BossCtrl runner) {
        base.Init(runner);
        currentProbability = baseProbability;
    }

    public override void Enter() {
        waitTimer = 0f;
        isJumping = false;
        hasJustLanded = false;

        botolaManager = _runner.transform.parent.GetComponentInChildren<BotolaManager>();
    }

    public override void Update() {
        if (isJumping) return;

        waitTimer += Time.deltaTime;
        if (waitTimer >= waitTimeBetweenJumps) {
            waitTimer = 0f;
            StartJump();
        }
    }

    public override void ChangeState() {
        if(isReadyForSpecialAttack) {
            isReadyForSpecialAttack = false; 
            specialAttackEnabled = false;    
            _runner.SetState(typeof(BossSpecialAttackState));
            return;
        }

        if (hasJustLanded) {
            hasJustLanded = false; 
            _runner.SetState(typeof(BossRangedAttackState));
        }
    }

    public override void Exit() {
        if (_runner.Agent != null && _runner.Agent.isOnNavMesh) _runner.Agent.isStopped = true;
        isJumping = false;
        hasJustLanded = false;
    }

    private void StartJump() {
        jumpCounter++;
        Vector3 targetPos;

        // DI DEFAULT: Setup per salto normale
        _runner.NextAttackPattern = BossCtrl.AttackPattern.RandomOrTarget;
        leaveAcidPool = true;
        useSpecialPool = false;
        specialAttackEnabled = false;

        // --- PRIORITÀ 1: IL 4° SALTO AL CENTRO (Esclude il Dado) ---
        if (jumpCounter >= 4) {
            _runner.NextAttackPattern = BossCtrl.AttackPattern.Cross; 
            targetPos = GetRoomCenter();
            jumpCounter = 0; 
            leaveAcidPool = false; 
        } 
        // --- PRIORITÀ 2: TIRO DEL DADO (Solo nei salti 1, 2 e 3) ---
        else 
        {
            if (specialAttackCooldown > 0) {
                specialAttackCooldown--;
            } 
            else {
                float tiroDado = Random.value;
                if (tiroDado <= currentProbability) {
                    specialAttackEnabled = true;
                    specialAttackCooldown = 3; 
                    currentProbability = baseProbability; 
                } else {
                    currentProbability += probabilityIncrease; 
                }
            }

            // Esegue l'attacco speciale se ha vinto
            if (specialAttackEnabled) {
                useSpecialPool = true; 
                if(botolaManager != null && botolaManager.botole.Count > 0) {
                    targetPos = botolaManager.GetRandomBotola().position;
                } else {
                    targetPos = GetRoomCenter();
                }
            } 
            // Altrimenti salto normalissimo
            else {
                targetPos = FindValidJumpPoint(_runner.transform.position, maxJumpDistance);
            }
        }
        
        _runner.StartCoroutine(JumpRoutine(targetPos));
    }

    private Vector3 FindValidJumpPoint(Vector3 centerOrigin, float searchRadius) {
        int attempts = 0;
        while (attempts < 10) {
            attempts++;
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float range = Random.Range(minJumpDistance, maxJumpDistance); 
            Vector3 candidatePos = centerOrigin + (Vector3)(randomDir * range);
            candidatePos.z = 0f; 

            if (Mathf.Abs(candidatePos.x - _runner.transform.position.x) < 1.5f) continue; 

            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidatePos, out hit, 5f, NavMesh.AllAreas)) {
                Vector3 finalHitPos = new Vector3(hit.position.x, hit.position.y, _runner.transform.position.z);
                float dist = Vector2.Distance(_runner.transform.position, finalHitPos);
                Vector3 direction = (finalHitPos - _runner.transform.position).normalized;
                
                RaycastHit2D wallCheck = Physics2D.CircleCast(_runner.transform.position, 0.5f, direction, dist, LayerMask.GetMask("Walls"));

                if (wallCheck.collider == null) return finalHitPos; 
            }
        }
        return _runner.transform.position; 
    }

    private Vector3 GetRoomCenter() {
        if (_runner.LocalNavMesh != null && _runner.LocalNavMesh.navMeshData != null) {
            return _runner.LocalNavMesh.navMeshData.sourceBounds.center;
        }
        return Vector3.zero;
    }

    private IEnumerator JumpRoutine(Vector3 targetPos) {
        isJumping = true;
        if (_runner.Agent != null) _runner.Agent.enabled = false;

        Vector3 startPos = _runner.transform.position;
        float timePassed = 0f;

        while (timePassed < jumpDuration) {
            timePassed += Time.deltaTime;
            float progress = timePassed / jumpDuration; 
            _runner.transform.position = Vector3.Lerp(startPos, targetPos, progress);

            if (_runner.Visuals != null) {
                float heightOffset = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
                _runner.Visuals.localPosition = new Vector3(0, heightOffset, 0);
            }
            yield return null;
        }

        _runner.transform.position = targetPos;

        if(acidPoolPrefab != null && leaveAcidPool) {
            GameObject pool = Instantiate(acidPoolPrefab, targetPos, Quaternion.identity);
            
            float durataScelta = useSpecialPool ? acidPoolDurationSpecial : acidPoolDuration;
        
            if(useSpecialPool) {
                pool.transform.localScale = new Vector3(3f, 3f, 1f); 
            }

            PoolDuration poolScript = pool.GetComponent<PoolDuration>();
            if (poolScript != null) {
                poolScript.StartLifeCycle(durataScelta);
            }
        }

        if (specialAttackEnabled) {
            if (_runner.Visuals != null) {
                _runner.Visuals.localPosition = Vector3.zero;
                _runner.Visuals.gameObject.SetActive(false); 
            }
            if (_runner.Agent != null) _runner.Agent.enabled = false;

            Collider2D bossCollider = _runner.GetComponent<Collider2D>();
            if (bossCollider != null) {
                bossCollider.enabled = false;
            }

            isReadyForSpecialAttack = true; 
        } 
        else {
            if (_runner.Visuals != null) _runner.Visuals.localPosition = Vector3.zero;
            if (_runner.Agent != null) _runner.Agent.enabled = true;
            
            hasJustLanded = true; 
        }
        
        isJumping = false;
    }

    public override void CaptureInput() {}
    public override void FixedUpdate() {}
}