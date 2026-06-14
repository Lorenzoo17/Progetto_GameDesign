using UnityEngine;
using System.Collections;

public class BossAcidProjectile : ProjectileBase
{
    [Header("Impostazioni Collisione")]
    [SerializeField] private LayerMask destroyOnContactLayers;

    [Header("Impostazioni Ombra (Shadow)")]
    [SerializeField] private Transform shadow;
    [SerializeField] private Vector2 initialShadowOffset = new Vector2(0, -2f);
    [SerializeField] private float shadowInterpolationValue = 3f;

    [Header("Impostazioni Pozza d'Acido")]
    [Tooltip("0 = 0% di probabilità, 0.5 = 50%, 1 = 100% di lasciare la pozza")]
    [SerializeField][Range(0f, 1f)] private float spawnPoolProbability = 1f;

    private Rigidbody2D rb;
    private GameObject pozzaPrefab;
    private GameObject impactPrefab;
    private float poolDuration;
    private BossCtrl bossInRoom;
    private bool isInitializedForBoss = false;

    private bool ignoreWallsTemporarily = false;

    // --- NUOVE VARIABILI LOGICA DISTANZA ---
    private Vector3 startPosition;
    private float targetDistance;
    private bool groundHitTriggered = false; // Sicurezza per evitare 2 pozze generate nello stesso frame

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void InitializeBossProjectile(
        GameObject owner, Vector2 direction, float speed, float damage,
        GameObject pozza, GameObject impact, float poolDur, float distance, BossCtrl boss)
    {
        // 9999f tiene alla larga il ProjectileBase.cs
        base.InitializeProjectile(owner, direction, damage, 9999f);

        pozzaPrefab = pozza;
        impactPrefab = impact;
        poolDuration = poolDur;
        bossInRoom = boss;
        targetDistance = distance;

        startPosition = transform.position; // Registra da dove parte
        isInitializedForBoss = true;

        if (rb != null)
        {
            rb.linearVelocity = this.direction * speed;
        }

        if (shadow != null)
        {
            shadow.localPosition = (Vector3)initialShadowOffset;
        }
    }

    public void EnablePipeMode()
    {
        StartCoroutine(IgnoreWallsRoutine());
    }

    private IEnumerator IgnoreWallsRoutine()
    {
        ignoreWallsTemporarily = true;
        yield return new WaitForSeconds(0.25f);
        ignoreWallsTemporarily = false;
    }

    private void Update()
    {
        if (!isInitializedForBoss || groundHitTriggered) return;

        if (shadow != null)
        {
            shadow.localPosition = Vector3.Lerp(shadow.localPosition, Vector3.zero, shadowInterpolationValue * Time.deltaTime);
        }

        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.right = rb.linearVelocity;
        }

        // --- IL NUOVO CONTROLLO DELLO SPAZIO ---
        // Se ha percorso tutti i metri che doveva percorrere, atterra!
        if (Vector3.Distance(startPosition, transform.position) >= targetDistance)
        {
            HitGround();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInitializedForBoss || groundHitTriggered) return;
        if (other.GetComponent<ICollectible>() != null) return;
        if (other.gameObject == owner || other.GetComponentInParent<BossCtrl>() != null || other.GetComponent<Enemy>() != null) return;

        if (((1 << other.gameObject.layer) & destroyOnContactLayers) != 0)
        {
            if (ignoreWallsTemporarily) return;

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
            HitGround();
        }
    }

    private void HitGround()
    {
        // Rete di sicurezza per non spawnare la pozza due volte per errore
        if (groundHitTriggered) return;
        groundHitTriggered = true;

        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, 0f);
        Quaternion pozzaRotation = Quaternion.Euler(0, 0, -90f);

        if (impactPrefab != null)
        {
            GameObject impact = Instantiate(impactPrefab, spawnPos, pozzaRotation);
            Destroy(impact, 0.5f);
        }

        if (pozzaPrefab != null && Random.value <= spawnPoolProbability)
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