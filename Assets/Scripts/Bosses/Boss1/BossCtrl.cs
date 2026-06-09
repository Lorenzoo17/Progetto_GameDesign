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

    [Header("Impostazioni Melee Bounce")]
    [SerializeField] private float meleeDistanceThreshold = 3f; // Distanza massima per considerare il colpo "melee"
    [SerializeField] private float meleeComboMaxDelay = 1.5f;
    private int consecutiveMeleeHits = 0;
    private float lastMeleeHitTime = -999f;
    public bool isBounceActive = false;
    [SerializeField] private float minDelayBetweenHits = 0.2f;


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
        Debug.Log($"[DEBUG BOUNCE] ApplyKnockback chiamato! Il boss ha subito un danno.");

        LastKnockbackDirection = direction;
        LastKnockbackForce = knockbackForce;
        LastKnockbackDuration = knockbackDuration;

        if (CheckMeleeHitCombo())
        {
            Debug.Log("[DEBUG BOUNCE] !! COMBO 3/3 RIUSCITA !! Tento il passaggio a BossBounceState.");
            SetState(typeof(BossBounceState));
        }
        
    }

    private bool CheckMeleeHitCombo()
    {
        if (specialAttackActive || isBounceActive) return false;

        Player player = Object.FindFirstObjectByType<Player>();
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance <= meleeDistanceThreshold)
            {
                
                float timeSinceLastHit = Time.time - lastMeleeHitTime;
                if (timeSinceLastHit < minDelayBetweenHits)
                {
                    if (debug) Debug.Log($"[BOSS COMBO] Colpo ignorato! Troppo veloce ({timeSinceLastHit:F3}s < {minDelayBetweenHits}s). Probabile multi-hit dello stesso attacco.");

                   
                    return false;
                }



                if (timeSinceLastHit > meleeComboMaxDelay)
                {
                    
                    consecutiveMeleeHits = 1;
                    if (debug) Debug.Log($"[BOSS COMBO] Tempo scaduto ({timeSinceLastHit:F2}s > {meleeComboMaxDelay}s). Combo resettata! Questo è il colpo (1/3).");
                }
                else
                {
                    
                    consecutiveMeleeHits++;
                    if (debug) Debug.Log($"[BOSS COMBO] Colpo rapido! Combo: ({consecutiveMeleeHits}/3) - Delta tempo: {timeSinceLastHit:F2}s");
                }

                
                lastMeleeHitTime = Time.time;

                if (consecutiveMeleeHits >= 3)
                {
                    consecutiveMeleeHits = 0; 
                    return true; 
                }
            }
            else
            {
                consecutiveMeleeHits = 0;
                if (debug) Debug.Log("[BOSS COMBO] Colpo da lontano. Combo resettata a 0.");
            }
        }
        return false;
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