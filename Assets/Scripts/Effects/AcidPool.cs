using UnityEngine;

public class AcidPool : MonoBehaviour
{
    [Header("Impostazioni Danno")]
    [SerializeField] private int damagePerTick = 1;
    [SerializeField] private float damageTickRate = 0.5f; 

    private float tickTimer = 0f;
    private BossCtrl bossInRoom;

    private void Awake() {
        
        bossInRoom = FindObjectOfType<BossCtrl>();
    }

    
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
                
                
                if (bossInRoom != null) {
                    bossInRoom.ReportPlayerHit();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            tickTimer = 0f; 
        }
    }
}