using UnityEngine;
using System.Collections;
using stateMachine;

[CreateAssetMenu(menuName = "States/Boss/Idle Rest")]
public class BossIdleState : State<BossCtrl>
{
    [Header("Impostazioni Uscita Tubo")]
    [SerializeField] private float slideDistance = 2f;    // Quanto si allontana dal tubo
    [SerializeField] private float slideDuration = 0.5f;  // Quanto ci mette a scivolare fuori

    [Header("Impostazioni Riposo (Stanchezza)")]
    [SerializeField] private float baseRestTime = 1f;       // Tempo base di riposo
    [SerializeField] private float restTimePerAttack = 0.5f; // Secondi extra per ogni tubo usato. Es: 8 tubi * 0.5 = +4 secondi

    private bool isRestComplete;

    public override void Enter()
    {
        isRestComplete = false;
        
        // 1. RIACCENDIAMO IL BOSS FISICAMENTE
        // Il boss riappare magicamente sopra l'ultimo tubo usato
        if (_runner.Visuals != null) _runner.Visuals.gameObject.SetActive(true);
        
        Collider2D bossCollider = _runner.GetComponent<Collider2D>();
        if (bossCollider != null) bossCollider.enabled = true;

        // 2. Facciamo partire la sequenza di uscita e riposo
        _runner.StartCoroutine(ExitPipeAndRestRoutine());
    }

    private IEnumerator ExitPipeAndRestRoutine()
    {
        // === FASE A: SCIVOLAMENTO FUORI DAL TUBO ===
        Vector3 startPos = _runner.transform.position;
        Vector3 roomCenter = Vector3.zero;

        // Calcoliamo il centro della stanza per sapere verso dove deve "scivolare"
        if (_runner.LocalNavMesh != null && _runner.LocalNavMesh.navMeshData != null) {
            roomCenter = _runner.LocalNavMesh.navMeshData.sourceBounds.center;
        }
        
        // Calcola la direzione verso il centro della stanza
        Vector3 directionToCenter = (roomCenter - startPos).normalized;
        Vector3 targetPos = startPos + (directionToCenter * slideDistance);
        
        float elapsedTime = 0f;
        while (elapsedTime < slideDuration)
        {
            _runner.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / slideDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _runner.transform.position = targetPos; // Assicura la posizione finale esatta

        // === FASE B: CALCOLO DELLA STANCHEZZA E RIPOSO ===
        // Leggiamo quante volte ha attaccato dallo stato precedente usando la variabile statica
        int fatiche = BossSpecialAttackState.timesAttacked;
        float totalRestTime = baseRestTime + (fatiche * restTimePerAttack);
        
        Debug.Log($"[BOSS IDLE] Attacco finito! Il boss riposa per {totalRestTime} secondi (ha usato {fatiche} tubi).");

        // Se hai un'animazione di Idle o "Stanco", questo è il momento di farla partire
        //if (_runner.Anim != null) _runner.Anim.SetTrigger("Idle"); 
        if (_runner.Anim != null)
        {
            _runner.Anim.SetTrigger("idle_to_jet");
        }
        else
        {
            Debug.LogWarning("BossSpecialAttack: Boss does not have an Animator component.");
        }

        // Il boss sta fermo e vulnerabile a prendere botte per il tempo calcolato
        yield return new WaitForSeconds(totalRestTime);

        if (_runner.Anim != null)
        {
            _runner.Anim.SetTrigger("jet_to_idle");
        }
        else
        {
            Debug.LogWarning("BossSpecialAttack: Boss does not have an Animator component.");
        }

        // Fine del riposo, diamo il via libera per cambiare stato
        isRestComplete = true;
    }

    public override void Update() { 
        // Nessun timer nell'Update, gestisce tutto la Coroutine!
    }

    public override void ChangeState()
    {
        // Se si è riposato abbastanza, torna alla sua routine di salti
        if (isRestComplete)
        {
            _runner.SetState(typeof(BossIntroMovementState));
        }
    }

    public override void Exit()
    {
        // IMPORTANTISSIMO: Riaccendiamo il NavMeshAgent prima di rimetterlo a saltare!
        // Altrimenti lo stato di Movement andrebbe in errore cercando di usare un Agent spento.
        if (_runner.Agent != null) 
        {
            _runner.Agent.enabled = true;
            if (_runner.Agent.isOnNavMesh) _runner.Agent.isStopped = false;
        }
    }

    public override void CaptureInput() { }
    public override void FixedUpdate() { }
}