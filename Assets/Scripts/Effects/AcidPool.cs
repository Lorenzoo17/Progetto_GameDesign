using UnityEngine;

public class AcidPool : MonoBehaviour
{
    [Header("Impostazioni Danno")]
    [SerializeField] private int damagePerTick = 1;
    [SerializeField] private float damageTickRate = 0.5f; // Danno ogni mezzo secondo

    private float tickTimer = 0f;
    private BossCtrl bossInRoom;

    private void Awake() {
        // La pozza cerca il boss nella scena quando nasce
        bossInRoom = FindObjectOfType<BossCtrl>();
    }

    // Usiamo OnTriggerStay2D per fare danno continuo finché il player ci sta sopra
    private void OnTriggerStay2D(Collider2D other) 
    {
        if (other.CompareTag("Player")) 
        {
            tickTimer += Time.deltaTime;
            
            if (tickTimer >= damageTickRate) 
            {
                tickTimer = 0f;
                
                // TODO: Applica danno al Player
                // HealthSystem playerHealth = other.GetComponent<HealthSystem>();
                // if (playerHealth != null) playerHealth.TakeDamage(damagePerTick);
                
                // AVVISA IL BOSS! (Gli manda la posizione attuale del player)
                if (bossInRoom != null) {
                    bossInRoom.ReportPlayerHit(other.transform.position);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            tickTimer = 0f; // Resetta il timer se il player esce
        }
    }
}