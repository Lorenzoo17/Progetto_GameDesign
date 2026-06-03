using UnityEngine;
using System.Collections;
using stateMachine;

[CreateAssetMenu(menuName = "States/Boss/Idle Rest")]
public class BossIdleState : State<BossCtrl>
{
    [Header("Impostazioni Uscita Tubo")]
    [SerializeField] private float slideDistance = 2f;    
    [SerializeField] private float slideDuration = 0.5f; 

    [Header("Impostazioni Riposo (Stanchezza)")]
    [SerializeField] private float baseRestTime = 1f;       
    [SerializeField] private float restTimePerAttack = 0.5f; 

    private bool isRestComplete;

    public override void Enter()
    {
        isRestComplete = false;
        
        if (_runner.Visuals != null) _runner.Visuals.gameObject.SetActive(true);
        
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
        Vector3 roomCenter = Vector3.zero;

        if (_runner.LocalNavMesh != null && _runner.LocalNavMesh.navMeshData != null)
        {
            roomCenter = _runner.LocalNavMesh.navMeshData.sourceBounds.center;
        }

        Vector3 directionToCenter = (roomCenter - startPos).normalized;
        Vector3 targetPos = startPos + (directionToCenter * slideDistance);

        float elapsedTime = 0f;

        
        while (elapsedTime < slideDuration)
        {
            _runner.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / slideDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _runner.transform.position = targetPos;

        int fatiche = BossSpecialAttackState.timesAttacked;
        float totalRestTime = baseRestTime + (fatiche * restTimePerAttack);

        if (_runner.debug) Debug.Log($"[BOSS IDLE] Attacco finito! Il boss riposa per {totalRestTime} secondi (ha usato {fatiche} tubi).");

        

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

    public override void Update()
    {
        
    }
}