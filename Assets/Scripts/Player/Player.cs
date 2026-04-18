using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public PlayerMovement playerMovement;
    public PlayerAttack playerAttack;
    public PlayerStats playerStats;
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerMovement = GetComponent<PlayerMovement>();
        playerAttack = GetComponent<PlayerAttack>();
        playerStats = GetComponent<PlayerStats>();
    }

    // Interazione per ora gestita in questo script direttamente
    // Per ora gestito con ontriggerenter
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.GetComponent<ICollectible>() != null) {
            other.gameObject.GetComponent<ICollectible>().Collect(this);
        }
    }
}
