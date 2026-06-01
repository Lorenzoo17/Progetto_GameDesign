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
    private bool leaveAcidPool = true;

    private BotolaManager botolaManager;

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
        botolaManager = _runner.transform.parent.GetComponentInChildren<BotolaManager>();
    }

    public override void Update()
    {
        if (isJumping) return;

        waitTimer += Time.deltaTime;
        if (waitTimer >= waitTimeBetweenJumps)
        {
            waitTimer = 0f;
            StartJump();
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
        if (_runner.Anim != null)
        {
            _runner.Anim.SetTrigger("idle_to_jump");
        }
        else
        {
               Debug.LogWarning("BossIntroMovementState: Boss does not have an Animator component.");
        }
        jumpCounter++;
        Vector3 targetPos;

        _runner.NextAttackPattern = BossCtrl.AttackPattern.RandomOrTarget;
        leaveAcidPool = true;
        useSpecialPool = false;
        specialAttackEnabled = false;

        // PRIORITÀ 1: Salto al centro (Reset)
        if (jumpCounter >= 4 && _runner.MemoryTurnsLeft == 0)
        {
            _runner.NextAttackPattern = BossCtrl.AttackPattern.Cross;
            targetPos = GetRoomCenter();
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
                else
                {
                    currentProbability += probabilityIncrease;
                }
            }

            // PRIORITÀ 2: Attacco Speciale
            if (specialAttackEnabled)
            {
                useSpecialPool = true;
                if (botolaManager != null && botolaManager.botole.Count > 0)
                {
                    targetPos = botolaManager.GetRandomBotola().position;
                }
                else
                {
                    targetPos = GetRoomCenter();
                }
            }
            // PRIORITÀ 3: HA IL TRACCIATORE (Melma addosso al player)
            else if (_runner.MemoryTurnsLeft > 0)
            {
                Player player = Object.FindFirstObjectByType<Player>();
                if (player != null)
                {
                    // Precisione al 95%: miriamo al player ma aggiungiamo un errore fino a 1.5 unità
                    Vector2 errorMargin = Random.insideUnitCircle * 1.5f;
                    Vector3 predictedPos = player.transform.position + (Vector3)errorMargin;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(predictedPos, out hit, 3f, NavMesh.AllAreas))
                    {
                        targetPos = new Vector3(hit.position.x, hit.position.y, _runner.transform.position.z);
                    }
                    else
                    {
                        targetPos = predictedPos;
                    }
                }
                else
                {
                    targetPos = FindValidJumpPoint(_runner.transform.position, maxJumpDistance);
                }
            }
            // PRIORITÀ 4: Salto casuale ampio (Non sa dove sei)
            else
            {
                targetPos = FindValidJumpPoint(_runner.transform.position, maxJumpDistance);
            }
        }

        _runner.StartCoroutine(JumpRoutine(targetPos));
    }

    private Vector3 FindValidJumpPoint(Vector3 centerOrigin, float searchRadius)
    {
        int attempts = 0;
        while (attempts < 15)
        { // Aumentati i tentativi
            attempts++;
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float range = Random.Range(minJumpDistance, maxJumpDistance);
            Vector3 candidatePos = centerOrigin + (Vector3)(randomDir * range);
            candidatePos.z = _runner.transform.position.z;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidatePos, out hit, 2f, NavMesh.AllAreas))
            {
                Vector3 finalHitPos = new Vector3(hit.position.x, hit.position.y, _runner.transform.position.z);

                // Controllo Muri semplificato e più robusto
                Vector2 dirToHit = finalHitPos - _runner.transform.position;
                RaycastHit2D wallCheck = Physics2D.Raycast(_runner.transform.position, dirToHit.normalized, dirToHit.magnitude, LayerMask.GetMask("Wall", "Walls"));
                Debug.Log($"BossJumpState: Tentativo {attempts} - Posizione candidata: {finalHitPos}, Distanza: {dirToHit.magnitude}, Hit muro: {(wallCheck.collider != null ? wallCheck.collider.name : "Nessuno")}");
                if (wallCheck.collider == null) return finalHitPos;
            }
        }
        Debug.LogWarning("BossJumpState: Non sono riuscito a trovare un punto di salto valido dopo 15 tentativi, salto al centro.");
        return GetRoomCenter();
    }

    private Vector3 GetRoomCenter()
    {
        if (_runner.LocalNavMesh != null && _runner.LocalNavMesh.navMeshData != null)
        {
            return _runner.LocalNavMesh.navMeshData.sourceBounds.center;
        }
        return Vector3.zero;
    }

    private IEnumerator JumpRoutine(Vector3 targetPos)
    {
        isJumping = true;
        if (_runner.Agent != null) _runner.Agent.enabled = false;

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

        _runner.transform.position = targetPos;

        if (acidPoolPrefab != null && leaveAcidPool)
        {
            GameObject pool = Instantiate(acidPoolPrefab, targetPos, Quaternion.identity);
            float durataScelta = useSpecialPool ? acidPoolDurationSpecial : acidPoolDuration;
            if (useSpecialPool) pool.transform.localScale = new Vector3(3f, 3f, 1f);
            if (pool.TryGetComponent<PoolDuration>(out PoolDuration poolScript))
            {
                poolScript.StartLifeCycle(durataScelta);
            }
        }

        if (specialAttackEnabled)
        {
            if (_runner.Visuals != null)
            {
                _runner.Visuals.localPosition = Vector3.zero;
                if (_runner.Visuals.TryGetComponent<SpriteRenderer>(out var sr)) sr.enabled = false;
            }
            Collider2D bossCollider = _runner.GetComponent<Collider2D>();
            if (bossCollider != null) bossCollider.enabled = false;
            isReadyForSpecialAttack = true;
        }
        else
        {
            if (_runner.Visuals != null) _runner.Visuals.localPosition = Vector3.zero;
            if (_runner.Agent != null) _runner.Agent.enabled = true;
            hasJustLanded = true;
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