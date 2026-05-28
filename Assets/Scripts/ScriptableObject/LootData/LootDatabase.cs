using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Loot/Loot Database")]
public class LootDatabase : ScriptableObject {
    // lista di SO di tutte le armi, perk e mutageni 
    public List<WeaponLootData> weapons;
    public List<PerkLootData> perks;
    public List<MutagenLootData> mutagens;

    public WeaponLootData GetWeaponById(string id) {
        return weapons.FirstOrDefault(w => w.id == id);
    }

    public PerkLootData GetPerkById(string id) {
        return perks.FirstOrDefault(p => p.id == id);
    }

    public MutagenLootData GetMutagenById(string id) {
        return mutagens.FirstOrDefault(m => m.id == id);
    }

    public GameObject GetItemByType(string id, SellingItemType type) {
        switch (type) {
            case SellingItemType.Weapon:
                return GetWeaponById(id).prefab;
            case SellingItemType.Perk:
                return GetPerkById(id).prefab;
            case SellingItemType.Mutagen:
                return GetMutagenById(id).prefab;
            default:
                return null;
        }
    }

    public int GetPriceByType(string id, SellingItemType type) {
        switch (type) {
            case SellingItemType.Weapon:
                return GetWeaponById(id).price;
            case SellingItemType.Perk:
                return GetPerkById(id).price;
            case SellingItemType.Mutagen:
                return GetMutagenById(id).price;
            default:
                return 0;
        }
    }
}
