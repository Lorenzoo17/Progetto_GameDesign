using UnityEngine;
using System.Collections;
using stateMachine;

[CreateAssetMenu(menuName = "States/Boss/Idle Rest")]
public class BossIdleState : State<BossCtrl>
{
    [Header("Impostazioni Uscita Tubo")]
    [SerializeField] private float slideDuration = 0.5f;

    [Header("Impostazioni Riposo (Stanchezza)")]
    [SerializeField] private float baseRestTime = 1f;
    [SerializeField] private float restTimePerAttack = 0.5f;

    private bool isRestComplete;

    public override void Enter()
    {
        isRestComplete = false;

        if (_runner.Visuals != null)
        {
            _runner.Visuals.gameObject.SetActive(true);

            // Riaccendiamo lo sprite qui per evitare glitch visivi
            if (_runner.Visuals.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr.enabled = true;
            }
        }

        Collider2D bossCollider = _runner.GetComponent<Collider2D>();
        if (bossCollider != null) bossCollider.enabled = true;

        _runner.StartCoroutine(ExitPipeAndRestRoutine());
    }

    private IEnumerator ExitPipeAndRestRoutine()
    {
        if (_runner.Anim != null)
        {
            _runner.Anim.SetTrigger("play_jet");
        }

        Vector3 startPos = _runner.transform.position;
        Vector3 targetPos = startPos; // Destinazione di fallback iniziale
        bool targetFound = false;

        // 🎯 1. Recuperiamo il PipeManager presente nella stanza
        PipeManager pipeManager = PipeManager.Instance;

        if (pipeManager != null)
        {
            float closestDistance = float.MaxValue;

            // 🎯 2. Cicliamo tra tutti i tubi figli del PipeManager per trovare l'ultimo usato
            foreach (Transform pipe in pipeManager.transform)
            {
                Transform bp = pipe.Find("BossPoint");
                if (bp != null)
                {
                    float dist = Vector3.Distance(startPos, bp.position);

                    // Il Boss si trova già davanti all'ultimo tubo, quindi sarà il più vicino in assoluto!
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        targetPos = bp.position; // Impostiamo la posizione del BossPoint come target
                        targetFound = true;
                    }
                }
            }
        }

        // 🔄 Fallback di sicurezza: se un tubo non dovesse avere l'oggetto BossPoint, usa il vecchio calcolo matematico
        if (!targetFound)
        {
            Vector3 roomCenter = Vector3.zero;
            if (_runner.LocalNavMesh != null && _runner.LocalNavMesh.navMeshData != null)
            {
                roomCenter = _runner.LocalNavMesh.navMeshData.sourceBounds.center;
            }
            Vector3 directionToCenter = (roomCenter - startPos).normalized;
            targetPos = startPos + (directionToCenter * 2f); // 2 unità di default verso il centro
        }

        // 🏃‍♂️ 3. Scivolamento cinematico verso il BossPoint del tubo
        float elapsedTime = 0f;
        while (elapsedTime < slideDuration)
        {
            _runner.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / slideDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _runner.transform.position = targetPos;

        // 🎯 4. Sincronizziamo l'agente NavMesh sulla posizione esatta del BossPoint ad atterraggio completato
        if (_runner.Agent != null && _runner.Agent.enabled)
        {
            _runner.Agent.nextPosition = targetPos;
        }

        // Calcolo della stanchezza basato sul numero di tubi visitati
        int fatiche = BossSpecialAttackState.timesAttacked;
        float totalRestTime = baseRestTime + (fatiche * restTimePerAttack);

        if (_runner.debug) Debug.Log($"[BOSS IDLE] Uscita completata! Riposo per {totalRestTime}s sul BossPoint: {targetPos}");

        yield return new WaitForSeconds(totalRestTime);

        isRestComplete = true;
    }

    public override void ChangeState()
    {
        if (isRestComplete)
        {
            _runner.SetState(typeof(BossIntroMovementState));
        }
    }

    public override void Exit()
    {
        if (_runner.Agent != null)
        {
            _runner.Agent.enabled = true;
            if (_runner.Agent.isOnNavMesh) _runner.Agent.isStopped = false;
        }
    }

    public override void CaptureInput() { }
    public override void FixedUpdate() { }
    public override void Update() { }
}