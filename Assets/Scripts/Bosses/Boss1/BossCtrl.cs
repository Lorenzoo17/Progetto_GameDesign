using UnityEngine;
using UnityEngine.AI;
using stateMachine;

public class BossCtrl : StateRunner<BossCtrl>
{
    [Header("Riferimenti Componenti")]
    public NavMeshAgent Agent { get; private set; }
    public Rigidbody2D Rb { get; private set; }
    public Animator Anim { get; private set; }
    public HealthSystem Health { get; private set; }
    public Transform Visuals { get; private set; }

    public NavMeshPlus.Components.NavMeshSurface LocalNavMesh { get; private set; }
    public ProjectileShooter Shooter { get; private set; }

    [Header("Dati Temporanei Stati")]
    [HideInInspector] public Vector2 LastKnockbackDirection;
    [HideInInspector] public float LastKnockbackForce;
    [HideInInspector] public float LastKnockbackDuration;

    // === NUOVO: SISTEMA DI MEMORIA E TIPO DI ATTACCO ===
    [HideInInspector] public Vector3? LastKnownPlayerPos = null;
    [HideInInspector] public int MemoryTurnsLeft = 0;

    public enum AttackPattern { RandomOrTarget, Cross }
    [HideInInspector] public AttackPattern NextAttackPattern = AttackPattern.RandomOrTarget;
    [HideInInspector] public bool AnimActionComplete = false;

    public Transform FirePoint { get; private set; }

    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackDuration = 0.2f;

    protected override void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponentInChildren<Animator>();
        Health = GetComponent<HealthSystem>();
        Visuals = transform.Find("Visual");
        LocalNavMesh = GetComponentInParent<NavMeshPlus.Components.NavMeshSurface>();
        Shooter = GetComponentInChildren<ProjectileShooter>();
        FirePoint = transform.Find("FirePoint");

        if (Agent != null)
        {
            Agent.updateRotation = false;
            Agent.updateUpAxis = false;
            // 🔥 SPEGNIAMO L'AGENTE ALL'AVVIO: Previene il warning al frame 0
            Agent.enabled = false;
        }
        base.Awake();
    }

    private void Start()
    {
        // Lascia lo Start vuoto! L'agente verrà acceso dalla stanza tramite ActivateBossAgent
    }

    public void ActivateBossAgent()
    {
        if (Agent == null) return;

        Vector3 bossPos = transform.position;
        bool sampleResult = NavMesh.SamplePosition(bossPos, out NavMeshHit hit, 5.0f, NavMesh.AllAreas);

        Debug.Log($"[BOSS NAV] Posizione boss: {bossPos}");
        Debug.Log($"[BOSS NAV] SamplePosition trovato: {sampleResult}");
        if (sampleResult)
        {
            Debug.Log($"[BOSS NAV] Hit position: {hit.position}  |  Distanza: {hit.distance}");
            Debug.Log($"[BOSS NAV] Hit mask: {hit.mask}");
        }

        if (sampleResult)
        {
            Vector3 safePos = new Vector3(hit.position.x, hit.position.y, bossPos.z);
            transform.position = safePos;
            Agent.enabled = true;
            Debug.Log($"[BOSS NAV] Agent.isOnNavMesh dopo enabled=true: {Agent.isOnNavMesh}");
        }
        else
        {
            Debug.LogWarning("[BOSS NAV] Impossibile trovare la NavMesh! Controlla il Bake.");
        }
    }

    protected override void Update()
    {
        base.Update();
        // Commentato il debug dell'animator per mantenere la console pulita (riattivalo se ti serve)
        /*
        if (Anim != null) {
            AnimatorClipInfo[] clipInfo = Anim.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0) Debug.Log($"[ANIM] Boss file: '{clipInfo[0].clip.name}'");
        }
        */
    }

    public void ApplyKnockback(Vector2 direction)
    {
        LastKnockbackDirection = direction;
        LastKnockbackForce = knockbackForce;
        LastKnockbackDuration = knockbackDuration;
        SetState(typeof(BossKnockbackState)); // Nota: Corretto con la 'c'
    }

    public void ReportPlayerHit(Vector3 playerPosition)
    {
        LastKnownPlayerPos = playerPosition;
        MemoryTurnsLeft = 2;
    }
}