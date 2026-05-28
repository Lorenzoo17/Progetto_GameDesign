using UnityEngine;

public class StartWeaponSpawner : MonoBehaviour {
    [SerializeField] private RoomBehaviour startRoom;
    [SerializeField] private LootDatabase lootDatabase;
    [SerializeField] private Transform startWeaponSpawnTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        MetaProgressionManager meta = MetaProgressionManager.Instance;
        if (lootDatabase == null || startWeaponSpawnTransform == null || meta == null || startRoom == null) return;

        int index = Random.Range(0, meta.GetDefaultWeapons().Count);
        string startWeaponId = meta.GetDefaultWeapons()[index];
        GameObject itemToSpawnPrefab = lootDatabase.GetItemByType(startWeaponId, SellingItemType.Weapon);
        GameObject startWeapon = Instantiate(itemToSpawnPrefab, startWeaponSpawnTransform.position, Quaternion.identity);

        startRoom.CloseDoors();

        if(startWeapon.TryGetComponent<Weapon>(out Weapon weapon)) {
            weapon.OnCollected += Weapon_OnCollected;
        }
    }

    private void Weapon_OnCollected(Weapon weapon) {
        startRoom.OpenDoors();

        weapon.OnCollected -= Weapon_OnCollected;
    }
}
