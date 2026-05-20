using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Loot/Perk Loot Data")]
public class PerkLootData : ScriptableObject {
    public string id;
    public int price = 100;
    public GameObject prefab;
    public bool unlockedByDefault;
}
