using UnityEngine;

public class ProjectileBoss1 : MonoBehaviour
{
    [SerializeField] private GameObject pozzaPrefab;
    [SerializeField] private GameObject impactPrefab;
    
    [Header("Parametri Proiettile")]
    [SerializeField] private float lifeTime = 0.8f; 
    [SerializeField] private float poolDuration = 3f;

    private BossCtrl bossInRoom;
    private Rigidbody2D rb;

    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start() 
    {
        bossInRoom = FindObjectOfType<BossCtrl>();
        // Distrugge e crea la pozza dopo 'lifeTime' secondi
        Invoke(nameof(HitGround), lifeTime);
    }

    private void Update() 
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.1f) 
        {
            transform.right = rb.linearVelocity;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        // Ignora sia il Boss che la stanza stessa (Tutto minuscolo per sicurezza)
        if (other.gameObject.name.ToLower().Contains("boss") || other.GetComponentInParent<BossCtrl>() != null) return;

        if(other.gameObject.CompareTag("Player")) {
            
            if (bossInRoom != null) {
                bossInRoom.ReportPlayerHit(other.transform.position);
            }
            
            CancelInvoke(); 
            HitGround(); // Crea la pozza anche se colpisce il player!
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Walls")) { 
            CancelInvoke();
            HitGround(); 
        }
    }

    private void HitGround() 
    {
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, 0f);
        Quaternion pozzaRotation = Quaternion.Euler(0, 0, -90f);
        
        if(impactPrefab != null) {
            GameObject impact = Instantiate(impactPrefab, spawnPos, pozzaRotation);
            Destroy(impact, 0.5f); 
        }
        
        if(pozzaPrefab != null) {
            GameObject pool = Instantiate(pozzaPrefab, spawnPos, pozzaRotation);
            PoolDuration poolScript = pool.GetComponent<PoolDuration>();
            if (poolScript != null) poolScript.StartLifeCycle(poolDuration);
        }
        
        Destroy(gameObject);
    }
}