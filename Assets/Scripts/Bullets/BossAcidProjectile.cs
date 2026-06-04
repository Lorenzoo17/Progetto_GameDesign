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
        if (!isInitializedForBoss) return;
        if (other.GetComponent<ICollectible>() != null) return;
        if (other.gameObject == owner || other.GetComponentInParent<BossCtrl>() != null || other.GetComponent<Enemy>() != null) return;

        if (((1 << other.gameObject.layer) & destroyOnContactLayers) != 0)
        {
            if (ignoreWallsTemporarily) return;

            //Debug.Log($"[BOSS ACID PROJECTILE] Hit {other.gameObject.name} and will be destroyed due to layer mask!");
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
            //Debug.Log($"[BOSS ACID PROJECTILE] Hit {other.gameObject.name} and dealt damage!");
            CancelInvoke();
            HitGround();
        }
    }

    private void HitGround()
    {
        //Debug.Log("[BOSS ACID PROJECTILE] HitGround called!");
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