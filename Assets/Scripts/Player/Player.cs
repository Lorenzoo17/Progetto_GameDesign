using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public PlayerMovement playerMovement;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerMovement = GetComponent<PlayerMovement>();
    }
}
