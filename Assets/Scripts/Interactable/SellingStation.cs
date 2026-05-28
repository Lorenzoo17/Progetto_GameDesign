using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ShopContext {
    Hub,
    Dungeon
}

public enum SellingItemType {
    Weapon,
    Perk,
    Mutagen,
    MutagenCoin,
    Heart
}

public enum SellingPurchaseMode {
    UnlockMetaProgression,
    ReleasePickup
}

[System.Serializable]
public class SellingItemData {
    public string id;
    public GameObject itemPrefab;
    public int price;
    public SellingItemType type;
    public SellingPurchaseMode purchaseMode;

    public SellingItemData(
        string id,
        GameObject itemPrefab,
        int price,
        SellingItemType type,
        SellingPurchaseMode purchaseMode
    ) {
        this.id = id;
        this.itemPrefab = itemPrefab;
        this.price = price;
        this.type = type;
        this.purchaseMode = purchaseMode;
    }
}

[System.Serializable]
public class DungeonShopExtraItem {
    public string id;
    public GameObject prefab;
    public int price;
    public SellingItemType type;
}


public class SellingStation : MonoBehaviour {
    [Header("Shop")]
    [SerializeField] private ShopContext shopContext;
    [SerializeField] private LootDatabase lootDatabase;
    [SerializeField] private SellingSlot[] slots;
    [SerializeField] private int maxItemsToSell = 3;

    [Header("Solo dungeon")]
    [SerializeField] private DungeonShopExtraItem[] dungeonExtraItems;

    private void Start() {
        if (MetaProgressionManager.Instance == null) {
            Debug.LogWarning("MetaProgressionManager non presente");
            return;
        }

        if (lootDatabase == null) {
            Debug.LogWarning("LootDatabase non presente");
            return;
        }

        SpawnItemsToSell(); // si spawnano nei sellingSlots gli item corretti
    }

    // se lo shop e' quello nell'hub, si ottiene una lista solo di perk, mutageni e weapon
    // mentre se lo shop e' quello del dungeon, si aggiungono anche altri come cuori o monete mutagene
    private List<SellingItemData> GetItemsToSell() {
        switch (shopContext) {
            case ShopContext.Hub:
                return GetHubItemsToSell();

            case ShopContext.Dungeon:
                return GetDungeonItemsToSell();

            default:
                return new List<SellingItemData>();
        }
    }

    // possibili oggetti da vendere nell'hub
    private List<SellingItemData> GetHubItemsToSell() {
        List<SellingItemData> result = new();

        MetaProgressionManager manager = MetaProgressionManager.Instance;

        foreach (WeaponLootData weapon in lootDatabase.weapons) {
            if (weapon == null || weapon.prefab == null)
                continue;

            if (!manager.IsWeaponUnlocked(weapon.id)) {
                result.Add(new SellingItemData(
                    weapon.id,
                    weapon.prefab,
                    weapon.price,
                    SellingItemType.Weapon,
                    SellingPurchaseMode.UnlockMetaProgression
                ));
            }
        }

        foreach (PerkLootData perk in lootDatabase.perks) {
            if (perk == null || perk.prefab == null)
                continue;

            if (!manager.IsPerkUnlocked(perk.id)) {
                result.Add(new SellingItemData(
                    perk.id,
                    perk.prefab,
                    perk.price,
                    SellingItemType.Perk,
                    SellingPurchaseMode.UnlockMetaProgression
                ));
            }
        }

        foreach (MutagenLootData mutagen in lootDatabase.mutagens) {
            if (mutagen == null || mutagen.prefab == null)
                continue;

            if (!manager.IsMutagenUnlocked(mutagen.id)) {
                result.Add(new SellingItemData(
                    mutagen.id,
                    mutagen.prefab,
                    mutagen.price,
                    SellingItemType.Mutagen,
                    SellingPurchaseMode.UnlockMetaProgression
                ));
            }
        }

        return result;
    }

    // possibili oggetti da vendere nel dungeon
    private List<SellingItemData> GetDungeonItemsToSell() {
        List<SellingItemData> result = new();

        MetaProgressionManager manager = MetaProgressionManager.Instance;
        Player player = Player.Instance;

        if (manager == null || player == null)
            return result;

        // si prendono tutte le armi, perk e mutageni sbloccati dal MetaProgressionManager
        List<string> availableWeapons = new(manager.GetUnlockedWeapons());
        List<string> availablePerks = new(manager.GetUnlockedPerks());
        List<string> availableMutagens = new(manager.GetUnlockedMutagens());

        // a questi, si tolgono le armi e mutageni attualmente equipaggiati
        RemoveEquippedItems(availableWeapons, GetEquippedWeaponIds(player));
        // I PERK NON SI TOLGONO, NEL DUNGEON E' POSSIBILE TROVARE 2 VOLTE LO STESSO PERK
        // RemoveEquippedItems(availablePerks, GetEquippedPerkIds(player));
        RemoveEquippedItems(availableMutagens, GetEquippedMutagenIds(player));

        AddDungeonLootItems(result, availableWeapons, SellingItemType.Weapon);
        AddDungeonLootItems(result, availablePerks, SellingItemType.Perk);
        AddDungeonLootItems(result, availableMutagens, SellingItemType.Mutagen);
        // Si aggiungono allo shop del dungeon gli oggetti specifici
        AddDungeonExtraItems(result);

        return result;
    }

    private void AddDungeonLootItems(
        List<SellingItemData> result,
        List<string> itemIds,
        SellingItemType type
    ) {
        foreach (string itemId in itemIds) {
            GameObject prefab = lootDatabase.GetItemByType(itemId, type);

            if (prefab == null)
                continue;

            int price = lootDatabase.GetPriceByType(itemId, type);

            result.Add(new SellingItemData(
                itemId,
                prefab,
                price,
                type,
                SellingPurchaseMode.ReleasePickup
            ));
        }
    }

    private void AddDungeonExtraItems(List<SellingItemData> result) {
        if (dungeonExtraItems == null)
            return;

        foreach (DungeonShopExtraItem extraItem in dungeonExtraItems) {
            if (extraItem == null || extraItem.prefab == null)
                continue;

            result.Add(new SellingItemData(
                extraItem.id,
                extraItem.prefab,
                extraItem.price,
                extraItem.type,
                SellingPurchaseMode.ReleasePickup
            ));
        }
    }

    private void SpawnItemsToSell() {
        if (slots == null || slots.Length == 0)
            return;

        List<SellingItemData> availableItems = GetItemsToSell();

        if (availableItems.Count == 0) {
            Debug.Log("Nessun oggetto disponibile da vendere.");
            return;
        }

        int itemsToSpawn = Mathf.Min(maxItemsToSell, slots.Length, availableItems.Count);

        for (int i = 0; i < itemsToSpawn; i++) {
            int randomIndex = Random.Range(0, availableItems.Count);
            SellingItemData selectedItem = availableItems[randomIndex];

            Vector3 spawnPosition = slots[i].transform.position;

            if (slots[i].itemPositioningTransform != null) {
                spawnPosition = slots[i].itemPositioningTransform.position;
            }

            GameObject itemInstance = Instantiate(
                selectedItem.itemPrefab,
                spawnPosition,
                Quaternion.identity
            );

            itemInstance.transform.SetParent(slots[i].transform);

            slots[i].SetUpSellingSlot(itemInstance, selectedItem);

            availableItems.RemoveAt(randomIndex);
        }
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

        if (currentWeapon != null && currentWeapon.weaponLootData != null) {
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
