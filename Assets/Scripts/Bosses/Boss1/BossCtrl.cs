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

    [SerializeField] public bool debug = false;
    public NavMeshPlus.Components.NavMeshSurface LocalNavMesh { get; private set; }
    public ProjectileShooter Shooter { get; private set; }

    [Header("Dati Temporanei Stati")]
    [HideInInspector] public Vector2 LastKnockbackDirection;
    [HideInInspector] public float LastKnockbackForce;
    [HideInInspector] public float LastKnockbackDuration;
    [Header("Riferimenti Stanza")]
    public Transform roomCenter;


    public enum AttackPattern { RandomOrTarget, Cross }
    [HideInInspector] public AttackPattern NextAttackPattern = AttackPattern.RandomOrTarget;
    [HideInInspector] public bool AnimActionComplete = false;

    public Transform FirePoint { get; private set; }
    public bool hasHitplayer = false;
    [Header("Cooldown Attacco Rush")]
    [SerializeField] public int coolDownRush = 3;

    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackDuration = 0.2f;
    public bool specialAttackActive = false;

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
           
        }
        base.Awake();
    }

    public void ApplyKnockback(Vector2 direction)
    {
        LastKnockbackDirection = direction;
        LastKnockbackForce = knockbackForce;
        LastKnockbackDuration = knockbackDuration;
        SetState(typeof(BossKnockbackState)); 
    }

    public void ReportPlayerHit()
    {
        if (debug)
        {
            Debug.Log("BossCtrl: Player hit reported.");
            Debug.Log("Current Cooldown: " + coolDownRush);
        }
            if (!specialAttackActive) { 
            if(coolDownRush==0)hasHitplayer = true;
            else coolDownRush--;
        }
    }
}