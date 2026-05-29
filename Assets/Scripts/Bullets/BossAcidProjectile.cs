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

    // Variabili per gli effetti
    private GameObject pozzaPrefab;
    private GameObject impactPrefab;
    private float poolDuration;

    private BossCtrl bossInRoom;
    private bool isInitializedForBoss = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Metodo chiamato dallo ShooterBoss per passare tutti i parametri centralizzati
    public void InitializeBossProjectile(
        GameObject owner, Vector2 direction, float speed, float damage,
        GameObject pozza, GameObject impact, float poolDur, float lifeTime, BossCtrl boss)
    {
        // 1. Inizializza la base (ProjectileBase) per sapere chi l'ha sparato e il danno
        base.InitializeProjectile(owner, direction, damage);

        // 2. Salva i parametri degli effetti e riferimenti
        pozzaPrefab = pozza;
        impactPrefab = impact;
        poolDuration = poolDur;
        bossInRoom = boss;
        isInitializedForBoss = true;

        // 3. Applica la velocità fisica
        if (rb != null)
        {
            rb.linearVelocity = this.direction * speed;
        }

        // 4. Inizializza la posizione dell'ombra facendola partire "in alto"
        if (shadow != null)
        {
            shadow.localPosition = (Vector3)initialShadowOffset;
        }

        // 5. Autodistruzione: se non colpisce nulla, esplode in aria e fa la pozza dopo 'lifeTime' secondi
        Invoke(nameof(HitGround), lifeTime);
    }

    private void Update()
    {
        // Effetto Ombra che "cade" verso il suolo (Vector3.zero)
        if (shadow != null)
        {
            shadow.localPosition = Vector3.Lerp(shadow.localPosition, Vector3.zero, shadowInterpolationValue * Time.deltaTime);
        }

        // Allinea la rotazione del proiettile alla sua traiettoria
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

        // Se tocca un muro (Layer Wall), esplode subito, fa lo splash e la pozza
        if (((1 << other.gameObject.layer) & destroyOnContactLayers) != 0)
        {
            CancelInvoke(); // Ferma il timer di vita
            HitGround();
            return;
        }

        // Usa la logica nativa per capire se può fare danno
        bool hasHitSomething = TryDealDamage(other, direction);

        if (hasHitSomething)
        {
            // Se ha colpito il Player, avvisa il Boss per il sistema di memoria
            if (other.TryGetComponent<Player>(out Player playerScript) && bossInRoom != null)
            {
                bossInRoom.ReportPlayerHit(other.transform.position);
            }
            CancelInvoke(); // Ferma il timer di vita
            HitGround();
        }
    }

    // La funzione magica che crea l'effetto e l'acido
    private void HitGround()
    {
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, 0f);
        Quaternion pozzaRotation = Quaternion.Euler(0, 0, -90f);

        // 1. Crea l'effetto SPLASH e lo distrugge dopo 0.5 sec
        if (impactPrefab != null)
        {
            GameObject impact = Instantiate(impactPrefab, spawnPos, pozzaRotation);
            Destroy(impact, 0.5f);
        }

        // 2. Crea la POZZA D'ACIDO e fa partire il suo timer
        if (pozzaPrefab != null)
        {
            GameObject pool = Instantiate(pozzaPrefab, spawnPos, pozzaRotation);
            if (pool.TryGetComponent<PoolDuration>(out PoolDuration poolScript))
            {
                poolScript.StartLifeCycle(poolDuration);
            }
        }

        // 3. Distrugge il proiettile principale
        Destroy(gameObject);
    }
}