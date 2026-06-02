using UnityEngine;

public class BossAcidProjectile : ProjectileBase
{
    [Header("Impostazioni Collisione")]
    [SerializeField] private LayerMask destroyOnContactLayers;

    [Header("Impostazioni Ombra (Shadow)")]
    [SerializeField] private Transform shadow;
    [SerializeField] private Vector2 initialShadowOffset = new Vector2(0, -2f);
    [SerializeField] private float shadowInterpolationValue = 3f;

    private Rigidbody2D rb;

    
    private GameObject pozzaPrefab;
    private GameObject impactPrefab;
    private float poolDuration;

    private BossCtrl bossInRoom;
    private bool isInitializedForBoss = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    
    public void InitializeBossProjectile(
        GameObject owner, Vector2 direction, float speed, float damage,
        GameObject pozza, GameObject impact, float poolDur, float lifeTime, BossCtrl boss)
    {
        
        base.InitializeProjectile(owner, direction, damage);

        
        pozzaPrefab = pozza;
        impactPrefab = impact;
        poolDuration = poolDur;
        bossInRoom = boss;
        isInitializedForBoss = true;

        
        if (rb != null)
        {
            rb.linearVelocity = this.direction * speed;
        }

        
        if (shadow != null)
        {
            shadow.localPosition = (Vector3)initialShadowOffset;
        }

        
        Invoke(nameof(HitGround), lifeTime);
    }

    private void Update()
    {
        
        if (shadow != null)
        {
            shadow.localPosition = Vector3.Lerp(shadow.localPosition, Vector3.zero, shadowInterpolationValue * Time.deltaTime);
        }

        
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.right = rb.linearVelocity;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"[PROIETTILE DEBUG] Collisione rilevata con: {other.gameObject.name} sul layer: {LayerMask.LayerToName(other.gameObject.layer)}");
        if (!isInitializedForBoss) return;
        if (other.GetComponent<ICollectible>() != null) return;

        if (other.gameObject == owner || other.GetComponentInParent<BossCtrl>() != null || other.GetComponent<Enemy>() != null) return;

        
        if (((1 << other.gameObject.layer) & destroyOnContactLayers) != 0)
        {
            CancelInvoke(); 
            HitGround();
            return;
        }

        
        bool hasHitSomething = TryDealDamage(other, direction);

        if (hasHitSomething)
        {
            
            if (other.TryGetComponent<Player>(out Player playerScript) && bossInRoom != null)
            {
                bossInRoom.ReportPlayerHit();
            }
            CancelInvoke(); 
            HitGround();
        }
    }

    
    private void HitGround()
    {
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, 0f);
        Quaternion pozzaRotation = Quaternion.Euler(0, 0, -90f);

        
        if (impactPrefab != null)
        {
            GameObject impact = Instantiate(impactPrefab, spawnPos, pozzaRotation);
            Destroy(impact, 0.5f);
        }

        
        if (pozzaPrefab != null)
        {
            GameObject pool = Instantiate(pozzaPrefab, spawnPos, pozzaRotation);
            if (pool.TryGetComponent<PoolDuration>(out PoolDuration poolScript))
            {
                poolScript.StartLifeCycle(poolDuration);
            }
        }

        
        Destroy(gameObject);
    }
}