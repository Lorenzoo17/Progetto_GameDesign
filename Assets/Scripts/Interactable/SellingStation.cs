using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SellingItemType {
    Weapon,
    Perk,
    Mutagen
}

[System.Serializable]
public class SellingItemData {
    public string id;
    public GameObject itemObject;
    public int price;
    public SellingItemType type;

    public SellingItemData(string id, GameObject itemObject, int price, SellingItemType type) {
        this.id = id;
        this.itemObject = itemObject;
        this.price = price;
        this.type = type;
    }
}

public class SellingStation : MonoBehaviour {
    [SerializeField] private LootDatabase lootDatabase;
    // per ora array di gameobject (tutti i prefab di tutti gli oggetti possibili)
    // poi magari gestirlo meglio, tramite ScriptableObjects, appena facciamo il merge dei vari lavori
    // anche perche gli oggetti dovranno essere 1 arma, 1 perk e 1 mutagene
    // e ad ognuno ci deve essere un prezzo specifico associato
    [SerializeField] private SellingSlot[] slots;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        if (MetaProgressionManager.Instance == null) {
            Debug.LogWarning("MetaProgressionManager non presente");
            return;
        }

        if (lootDatabase == null) {
            Debug.LogWarning("LootDatabase non presente");
            return;
        }

        SpawnItemsToSell();
    }

    private List<SellingItemData> GetItemsToSell() {
        List<SellingItemData> itemsToSellData = new();

        // in vendita vanno solamente armi, perk e mutageni non gia' sbloccati
        foreach(WeaponLootData weapon in lootDatabase.weapons) {
            if(weapon == null || weapon.prefab == null)
                continue;
            
            if (!MetaProgressionManager.Instance.IsWeaponUnlocked(weapon.id)) {
                itemsToSellData.Add(new SellingItemData(weapon.id, weapon.prefab, weapon.price, SellingItemType.Weapon));
            }
        }

        foreach (PerkLootData perk in lootDatabase.perks) {
            if (perk == null || perk.prefab == null)
                continue;

            if (!MetaProgressionManager.Instance.IsPerkUnlocked(perk.id)) {
                itemsToSellData.Add(new SellingItemData(perk.id, perk.prefab, perk.price, SellingItemType.Perk));
            }
        }

        foreach (MutagenLootData mutagen in lootDatabase.mutagens) {
            if (mutagen == null || mutagen.prefab == null)
                continue;

            if (!MetaProgressionManager.Instance.IsMutagenUnlocked(mutagen.id)) {
                itemsToSellData.Add(new SellingItemData(mutagen.id, mutagen.prefab, mutagen.price, SellingItemType.Mutagen));
            }
        }

        return itemsToSellData;
    }

    private void SpawnItemsToSell() {
        if (slots == null || slots.Length == 0) return;

        List<SellingItemData> availableItems = GetItemsToSell();

        if (availableItems.Count == 0) {
            Debug.Log("Nessun oggetto disponibile da vendere.");
            return;
        }

        // minimo tra numero di slot e numero di oggetti disponibili
        int itemsToSpawn = Mathf.Min(3, slots.Length, availableItems.Count);

        for (int i = 0; i < itemsToSpawn; i++) {
            int randomIndex = Random.Range(0, availableItems.Count);

            SellingItemData selectedItem = availableItems[randomIndex];

            Vector3 spawnPosition = slots[i].transform.position;
            if (slots[i].itemPositioningTransform != null) {
                spawnPosition = slots[i].itemPositioningTransform.position;
            }
            GameObject item = Instantiate(
                selectedItem.itemObject,
                spawnPosition,
                Quaternion.identity
            );
            item.transform.SetParent(slots[i].transform);
            slots[i].SetUpSellingSlot(item, selectedItem.price, selectedItem.id, selectedItem.type); // per ora prezzo fisso, poi dipendera' da SO

            availableItems.RemoveAt(randomIndex);
        }
    }
}
