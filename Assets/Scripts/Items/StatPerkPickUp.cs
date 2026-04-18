using UnityEngine;

public class StatPerkPickUp : MonoBehaviour, ICollectible {

    [SerializeField] private StatPerkSO perkSO;

    public void Collect(Player player) {
        player.playerStats.AddPerk(perkSO); // Aggiungo alla lista dei perk del player quello raccolto

        // Visual effect

        Destroy(gameObject); // Distruggo il gameobeject relativo al perk
    }
}
