using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TreasureRoomSpawner : MonoBehaviour {
    [SerializeField] private LootDatabase lootDatabase;
    [SerializeField] private Transform spawnPoint;

    private void Start() {
        SpawnItem();
    }

    private void SpawnItem() {
        MetaProgressionManager manager = MetaProgressionManager.Instance;

        if (manager == null || lootDatabase == null || spawnPoint == null) {
            Debug.LogWarning("TreasureRoomSpawner: riferimenti mancanti");
            return;
        }

        List<SellingItemType> availableTypes = GetAvailableTypes(manager);

        if (availableTypes.Count == 0) {
            Debug.LogWarning("Nessun oggetto sbloccato disponibile per la treasure room");
            return;
        }

        SellingItemType itemToSpawnType =
            availableTypes[Random.Range(0, availableTypes.Count)];

        List<string> unlockedItems =
            GetAvailableItems(itemToSpawnType, manager);

        if (unlockedItems == null || unlockedItems.Count == 0) {
            Debug.LogWarning($"Nessun item disponibile per il tipo {itemToSpawnType}");
            return;
        }

        string itemToSpawnId =
            unlockedItems[Random.Range(0, unlockedItems.Count)];

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
    
    // aggiungere a questo anche il fatto di non poter spawnare armi, perk o mutageni equipaggiati
    // aggiungere quindi al prefab di armi, perk e mutageni lo scriptableObject che permette 
    // di risalire all'id, in modo da poter effettuare il controllo
    // o, ad esempio, per:
    // armi -> mettere weaponLootData in Weapon.cs
    // mutageni -> mettere mutagenLootData in MutagenSO
    // perk -> mettere perkLootData in PerkBase
    // in questo modo si risale agli script attaccati al player ed e' possibile effettuare il controllo
    private List<SellingItemType> GetAvailableTypes(MetaProgressionManager manager) {
        List<SellingItemType> availableTypes = new();

        if (manager.GetUnlockedWeapons().Count > 0)
            availableTypes.Add(SellingItemType.Weapon);

        if (manager.GetUnlockedPerks().Count > 0)
            availableTypes.Add(SellingItemType.Perk);

        if (manager.GetUnlockedMutagens().Count > 0)
            availableTypes.Add(SellingItemType.Mutagen);

        return availableTypes;
    }

    private List<string> GetAvailableItems(SellingItemType type, MetaProgressionManager manager) {
        switch (type) {
            case SellingItemType.Weapon:
                return manager.GetUnlockedWeapons();

            case SellingItemType.Perk:
                return manager.GetUnlockedPerks();

            case SellingItemType.Mutagen:
                return manager.GetUnlockedMutagens();

            default:
                return new List<string>();
        }
    }
}
