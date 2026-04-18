using UnityEngine;

public class WeaponPickUp : MonoBehaviour, ICollectible {
    // gestirlo poi meglio con SO per drop ecc...
    [SerializeField] private GameObject weaponPrefab; // prefab dell'arma associata

    public void Collect(Player player) {
        player.playerAttack.SetCurrentWeapon(weaponPrefab);

        // Visual effect

        Destroy(gameObject); // distruggo dopo aver raccolto
    }
}
