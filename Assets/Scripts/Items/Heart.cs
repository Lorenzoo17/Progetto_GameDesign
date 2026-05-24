using UnityEngine;

public class Heart : MonoBehaviour, ICollectible {
    [SerializeField] private int healUnits = 1;

    public void Collect(Player player) {
        if(player.playerHealth != null) {
            player.playerHealth.Heal(healUnits);

            // suono di raccolta/cura
            Destroy(gameObject);
        }
    }
}
