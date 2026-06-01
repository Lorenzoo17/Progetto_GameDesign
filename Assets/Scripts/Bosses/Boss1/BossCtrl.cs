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
    [HideInInspector] public Vector3? LastKnownPlayerPos = null; // Dove crede che sia il player
    [HideInInspector] public int MemoryTurnsLeft = 0;            // Per quanti attacchi se lo ricorda
    
    public enum AttackPattern { RandomOrTarget, Cross }
    [HideInInspector] public AttackPattern NextAttackPattern = AttackPattern.RandomOrTarget;

    public Transform FirePoint { get; private set; }

    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackDuration = 0.2f;


    protected override void Awake() {
        Agent = GetComponent<NavMeshAgent>();
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponentInChildren<Animator>();
        Health = GetComponent<HealthSystem>();
        Visuals = transform.Find("Visual");
        LocalNavMesh = GetComponentInParent<NavMeshPlus.Components.NavMeshSurface>();
        Shooter = GetComponentInChildren<ProjectileShooter>();
        FirePoint = transform.Find("FirePoint");
        if (Agent != null) {
            Agent.updateRotation = false;
            Agent.updateUpAxis = false;
        }
        base.Awake(); 
    }
    protected override void Update()
    {
        base.Update();
        if (Anim != null)
        {
            // Questo prende le informazioni sulla clip (il file PNG/anim) attualmente in esecuzione
            AnimatorClipInfo[] clipInfo = Anim.GetCurrentAnimatorClipInfo(0);

            if (clipInfo.Length > 0)
            {
                Debug.Log($"[ANIMATOR COMPILATO] Il Boss sta riproducendo il file: '{clipInfo[0].clip.name}'");
            }
            else
            {
                Debug.LogWarning("[ANIMATOR COMPILATO] Il blocco è attivo, ma la casella 'Motion' è VUOTA (None)!");
            }
        }
    }
    public void ApplyKnockback(Vector2 direction) {
        LastKnockbackDirection = direction;
        LastKnockbackForce = knockbackForce;
        LastKnockbackDuration = knockbackDuration;
        SetState(typeof(BossKnockbackState));
    }

    // === METODO CHIAMATO DA PROIETTILI E POZZE ===
    public void ReportPlayerHit(Vector3 playerPosition) {
        LastKnownPlayerPos = playerPosition;
        MemoryTurnsLeft = 2; // Si ricorda la posizione per i prossimi 2 attacchi a distanza
        Debug.Log($"[BOSS MEMORIA] Ahia! Ho sentito il player a {playerPosition}. Me lo ricorderò!");
    }

}