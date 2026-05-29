using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TreasureRoomSpawner : MonoBehaviour {
    [SerializeField] private LootDatabase lootDatabase;
    [SerializeField] private Transform spawnPoint;

    private void Start() {
        this.GetComponent<RoomBehaviour>().OnRoomEnter += TreasureRoomSpawner_OnRoomEnter;
    }

    private void TreasureRoomSpawner_OnRoomEnter(object sender, System.EventArgs e) {
        SpawnItem(); // oggetto spanwa all'entrata del player nella stanza
    }

    private void SpawnItem() {
        MetaProgressionManager manager = MetaProgressionManager.Instance;
        Player player = Player.Instance;

        if (manager == null || player == null || lootDatabase == null || spawnPoint == null) {
            Debug.LogWarning("TreasureRoomSpawner: riferimenti mancanti");
            return;
        }

        Dictionary<SellingItemType, List<string>> availableItemsByType =
            GetAvailableItemsByType(manager, player);

        if (availableItemsByType.Count == 0) {
            Debug.LogWarning("Nessun oggetto disponibile per la treasure room: tutti gli oggetti sbloccati sono già equipaggiati");
            return;
        }

        List<SellingItemType> availableTypes = new List<SellingItemType>(availableItemsByType.Keys);

        SellingItemType itemToSpawnType =
            availableTypes[Random.Range(0, availableTypes.Count)];

        List<string> availableItems =
            availableItemsByType[itemToSpawnType];

        string itemToSpawnId =
            availableItems[Random.Range(0, availableItems.Count)];

        GameObject itemToSpawnPrefab =
            lootDatabase.GetItemByType(itemToSpawnId, itemToSpawnType);

        if (itemToSpawnPrefab == null) {
            Debug.LogWarning($"Prefab non trovato per item {itemToSpawnId} di tipo {itemToSpawnType}");
            return;
        }

        Instantiate(
            itemToSpawnPrefab,
            spawnPoint.position,
            Quaternion.identity
        );
    }

    private Dictionary<SellingItemType, List<string>> GetAvailableItemsByType(
        MetaProgressionManager manager,
        Player player
    ) {
        Dictionary<SellingItemType, List<string>> result = new();

        List<string> availableWeapons = new List<string>(manager.GetUnlockedWeapons());
        List<string> availablePerks = new List<string>(manager.GetUnlockedPerks());
        List<string> availableMutagens = new List<string>(manager.GetUnlockedMutagens());

        RemoveEquippedItems(
            availableWeapons,
            GetEquippedWeaponIds(player)
        );

        RemoveEquippedItems(
            availablePerks,
            GetEquippedPerkIds(player)
        );

        RemoveEquippedItems(
            availableMutagens,
            GetEquippedMutagenIds(player)
        );

        if (availableWeapons.Count > 0)
            result.Add(SellingItemType.Weapon, availableWeapons);

        if (availablePerks.Count > 0)
            result.Add(SellingItemType.Perk, availablePerks);

        if (availableMutagens.Count > 0)
            result.Add(SellingItemType.Mutagen, availableMutagens);

        return result;
    }

    private void RemoveEquippedItems(List<string> availableItems, HashSet<string> equippedItems) {
        if (availableItems == null || equippedItems == null || equippedItems.Count == 0)
            return;

        availableItems.RemoveAll(itemId => equippedItems.Contains(itemId));
    }

    private HashSet<string> GetEquippedWeaponIds(Player player) {
        HashSet<string> equippedWeaponIds = new();

        if (player == null || player.playerAttack == null)
            return equippedWeaponIds;

        Weapon currentWeapon = player.playerAttack.GetCurrentWeapon();

        if (
            currentWeapon != null &&
            currentWeapon.weaponLootData != null
        ) {
            equippedWeaponIds.Add(currentWeapon.weaponLootData.id);
        }

        return equippedWeaponIds;
    }

    private HashSet<string> GetEquippedPerkIds(Player player) {
        if (player == null || player.perkController == null)
            return new HashSet<string>();

        return player.perkController.GetEquippedPerkIds();
    }

    private HashSet<string> GetEquippedMutagenIds(Player player) {
        if (player == null || player.mutagenController == null)
            return new HashSet<string>();

        return player.mutagenController.GetEquippedMutagenIds();
    }
}
