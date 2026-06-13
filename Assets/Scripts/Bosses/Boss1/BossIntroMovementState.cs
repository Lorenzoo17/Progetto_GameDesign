using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using stateMachine;

[CreateAssetMenu(menuName = "States/Boss/Intro Movement")]
public class BossIntroMovementState : State<BossCtrl>
{
    [Header("Impostazioni Salto Boss")]
    [SerializeField] private float jumpDuration = 0.4f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float waitTimeBetweenJumps = 2f;
    [SerializeField] private float minJumpDistance = 3f;
    [SerializeField] private float maxJumpDistance = 7f;

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
    private int specialAttackCooldown = 4;
    private bool isReadyForSpecialAttack = false;
    private bool specialAttackEnabled = false;
    private bool hasJustLanded = false;
    private bool useSpecialPool = false;
    private bool leaveAcidPool = false;

    private BotolaManager botolaManager;

    [Header("Attacco Rush")]
    [SerializeField] private float rushDuration;
    public bool isRushing { get; private set; }


    public override void Init(BossCtrl runner)
    {
        base.Init(runner);
        currentProbability = baseProbability;
    }

    public override void Enter()
    {
        waitTimer = 0f;
        isJumping = false;
        hasJustLanded = false;
        botolaManager = BotolaManager.Instance;
    }

    public override void Update()
    {
        
        if (isJumping || isRushing) return;

        waitTimer += Time.deltaTime;
        if (waitTimer >= waitTimeBetweenJumps)
        {
            waitTimer = 0f;

            if (_runner.hasHitplayer)
            {
                if(_runner.debug)Debug.Log("Boss ha colpito il player, tenta di prevedere la posizione per il rush!");
                Player player = UnityEngine.Object.FindFirstObjectByType<Player>();

                if (player != null)
                {
                    Vector3 predictedPos = player.transform.position;
                    Vector3 targetPos = GetSafeNavMeshPoint(predictedPos);
                    _runner.StartCoroutine(RushRoutine(targetPos));
                }
                else
                {
                    
                    StartJump();
                }
            }
            else
            {
                StartJump();
            }
        }
    }

    public override void ChangeState()
    {
        if (isReadyForSpecialAttack)
        {
            isReadyForSpecialAttack = false;
            specialAttackEnabled = false;
            _runner.SetState(typeof(BossSpecialAttackState));
            return;
        }

        if (hasJustLanded)
        {
            hasJustLanded = false;
            _runner.SetState(typeof(BossRangedAttackState));
        }
    }

    private void StartJump()
    {
        if (_runner.Anim != null) _runner.Anim.SetTrigger("idle_to_jump");

        jumpCounter++;
        Vector3 targetPos;

        _runner.NextAttackPattern = BossCtrl.AttackPattern.RandomOrTarget;
        leaveAcidPool = false;
        useSpecialPool = false;
        specialAttackEnabled = false;

        
        if (jumpCounter >= 4)
        {
            _runner.NextAttackPattern = BossCtrl.AttackPattern.Cross;
            targetPos = _runner.roomCenter.transform.position;
            jumpCounter = 0;
            leaveAcidPool = false;
        }
        else
        {
            if (specialAttackCooldown > 0) specialAttackCooldown--;
            else
            {
                if (Random.value <= currentProbability)
                {
                    specialAttackEnabled = true;
                    specialAttackCooldown = 3;
                    currentProbability = baseProbability;
                }
                else currentProbability += probabilityIncrease;
            }

            
            if (specialAttackEnabled)
            {
                useSpecialPool = true;
                if (botolaManager != null && botolaManager.botole.Count > 0) targetPos = botolaManager.GetRandomBotola().position;
                else targetPos = _runner.roomCenter.transform.position;
            }
            
     
            
            
            else targetPos = FindValidJumpPoint(_runner.transform.position, maxJumpDistance);
        }

        _runner.StartCoroutine(JumpRoutine(targetPos));
    }

    private IEnumerator RushRoutine(Vector3 targetPos)
    {
        if(_runner.debug)Debug.Log("Inizio Rush verso: " + targetPos);
        isRushing = true;

        if (_runner.Agent != null && _runner.Agent.isOnNavMesh)
        {
            _runner.Agent.isStopped = true;
            _runner.Agent.updatePosition = false;
        }

        Vector3 startPos = _runner.transform.position;
        float timePassed = 0f;


        if (_runner.Anim != null) _runner.Anim.SetTrigger("idle_to_rush");


        while (timePassed < rushDuration)
        {
            timePassed += Time.deltaTime;
            float progress = timePassed / rushDuration;

        
            float easeProgress = 1f - Mathf.Pow(1f - progress, 3f);

            _runner.transform.position = Vector3.Lerp(startPos, targetPos, easeProgress);


            yield return null;
        }

        
        _runner.transform.position = targetPos;

       
        if (_runner.Agent != null)
        {

            if (!_runner.Agent.enabled) _runner.Agent.enabled = true;

            if (UnityEngine.AI.NavMesh.SamplePosition(_runner.transform.position, out UnityEngine.AI.NavMeshHit hit, 3.0f, _runner.Agent.areaMask))
            {
                _runner.transform.position = new Vector3(hit.position.x, hit.position.y, _runner.transform.position.z);

                _runner.Agent.nextPosition = hit.position;
            }

            _runner.Agent.updatePosition = true;
            if (_runner.Agent.isOnNavMesh) _runner.Agent.isStopped = false;
        }
        if (_runner.Anim != null) _runner.Anim.SetTrigger("rush_to_idle"); 
        //hasJustLanded = true; //se voglio passare alla fase d'attacco
        isRushing = false;
        _runner.hasHitplayer = false;
        _runner.coolDownRush = 3;
    }

    private Vector3 FindValidJumpPoint(Vector3 centerOrigin, float searchRadius)
    {
        for (int attempts = 0; attempts < 15; attempts++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float range = Random.Range(minJumpDistance, maxJumpDistance);
            Vector3 candidatePos = centerOrigin + (Vector3)(randomDir * range);

            Vector2 dirToHit = candidatePos - centerOrigin;
            RaycastHit2D wallCheck = Physics2D.Raycast(centerOrigin, dirToHit.normalized, dirToHit.magnitude, LayerMask.GetMask("Wall", "Walls"));

            if (wallCheck.collider == null)
            {
                // Modificato con la maschera specifica del boss
                if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, 2f, _runner.Agent.areaMask))
                {
                    return new Vector3(hit.position.x, hit.position.y, _runner.transform.position.z);
                }
            }
        }
        return _runner.roomCenter.transform.position;
    }

    private Vector3 GetSafeNavMeshPoint(Vector3 target)
    {
        // Modificato con la maschera specifica del boss
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 3f, _runner.Agent.areaMask))
        {
            return new Vector3(hit.position.x, hit.position.y, _runner.transform.position.z);
        }
        return target;
    }

    private IEnumerator JumpRoutine(Vector3 targetPos)
    {
        isJumping = true;

        if (_runner.Agent != null && _runner.Agent.isOnNavMesh)
        {
            _runner.Agent.isStopped = true;
            _runner.Agent.updatePosition = false;
        }

        // disabilito collider durante salto
        if (_runner.BossCollider != null) {
            _runner.BossCollider.enabled = false;
        }

        Vector3 startPos = _runner.transform.position;
        float timePassed = 0f;

        while (timePassed < jumpDuration)
        {
            timePassed += Time.deltaTime;
            float progress = timePassed / jumpDuration;
            _runner.transform.position = Vector3.Lerp(startPos, targetPos, progress);

            if (_runner.Visuals != null)
            {
                float heightOffset = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
                _runner.Visuals.localPosition = new Vector3(0, heightOffset, 0);
            }
            yield return null;
        }

        if (_runner.Visuals != null) _runner.Visuals.localPosition = Vector3.zero;

        

        if (specialAttackEnabled)
        {
            if (_runner.Visuals != null && _runner.Visuals.TryGetComponent<SpriteRenderer>(out var sr)) sr.enabled = false;
            Collider2D bossCollider = _runner.GetComponent<Collider2D>();
            if (bossCollider != null) bossCollider.enabled = false;

            if (_runner.Agent != null) _runner.Agent.enabled = false;

            isReadyForSpecialAttack = true;

            if (acidPoolPrefab != null)
            {
                GameObject pool = Instantiate(acidPoolPrefab, targetPos, Quaternion.identity);  
                pool.transform.localScale = new Vector3(15f, 8f, 1f);
                if (pool.TryGetComponent<PoolDuration>(out PoolDuration poolScript)) poolScript.StartLifeCycle(acidPoolDurationSpecial);
            }
        }
        else
        {
            if (_runner.Agent != null)
            {
                if (!_runner.Agent.enabled) _runner.Agent.enabled = true;

                if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5.0f, _runner.Agent.areaMask))
                {
                    _runner.transform.position = hit.position;

                    _runner.Agent.nextPosition = hit.position;
                }
                _runner.Agent.updatePosition = true;
                if (_runner.Agent.isOnNavMesh) _runner.Agent.isStopped = false;
            }
            hasJustLanded = true;
        }

        // riabilito collider alla fine del salto
        if (_runner.BossCollider != null) {
            _runner.BossCollider.enabled = true;
        }

        isJumping = false;
    }

    public override void Exit()
    {
        if (_runner.Agent != null && _runner.Agent.isOnNavMesh) _runner.Agent.isStopped = true;
        isJumping = false;
        hasJustLanded = false;
    }

    public override void CaptureInput() { }
    public override void FixedUpdate() { }
}