using UnityEngine;

public class Heart : MonoBehaviour, ICollectible {
    [SerializeField] private int healUnits = 1;

    public void Collect(Player player) {
        if(player.playerHealth != null) {
            if(player.playerHealth.GetHealthPercentage() < 1f) { // se ha massima vita, non si cura e non si distrugge nemmeno
                // il cuore
                player.playerHealth.Heal(healUnits);

                // suono di raccolta/cura
                Destroy(gameObject);
            }
        }
    }
}
